using System;
using System.Collections.Generic;
using UnityEngine;
using AmongUs.GameOptions;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability;
using SuperNewRoles.Roles.Ability.CustomButton;
using SuperNewRoles.Modules.Events.Bases;
using System.Linq;
using SuperNewRoles.Events;
using SuperNewRoles.Modules.Events;

namespace SuperNewRoles.Roles.CrewMate;

class VentTrapper : RoleBase<VentTrapper>
{
    public override RoleId Role { get; } = RoleId.VentTrapper;
    public override Color32 RoleColor { get; } = new Color32(149, 208, 228, byte.MaxValue);
    public override List<Func<AbilityBase>> Abilities { get; } = [() => new VentTrapperAbility(new VentTrapperData(
        coolTime: VentTrapperCoolTime,
        useLimit: VentTrapperUseLimit,
        setupTime: VentTrapperSetupTime,
        stunTime: VentTrapperStunTime,
        canUseVent: VentTrapperCanUseVent
    )), () => new CustomVentAbility(() => VentTrapperCanUseVent)];
    public override QuoteMod QuoteMod { get; } = QuoteMod.SuperNewRoles;
    public override RoleTypes IntroSoundType { get; } = RoleTypes.Crewmate;
    public override short IntroNum { get; } = 1;
    public override AssignedTeamType AssignedTeam { get; } = AssignedTeamType.Crewmate;
    public override WinnerTeamType WinnerTeam { get; } = WinnerTeamType.Crewmate;
    public override TeamTag TeamTag { get; } = TeamTag.Crewmate;
    public override RoleTag[] RoleTags { get; } = [];
    public override RoleOptionMenuType OptionTeam { get; } = RoleOptionMenuType.Crewmate;

    [CustomOptionFloat("VentTrapperCoolTime", 2.5f, 60f, 2.5f, 20f, translationName: "CoolTime")]
    public static float VentTrapperCoolTime = 20f;

    [CustomOptionInt("VentTrapperUseLimit", 1, 20, 1, 1, translationName: "UseLimit")]
    public static int VentTrapperUseLimit = 1;

    [CustomOptionFloat("VentTrapperSetupTime", 0f, 10f, 1f, 3f)]
    public static float VentTrapperSetupTime = 3f;

    [CustomOptionFloat("VentTrapperStunTime", 1f, 20f, 1f, 5f)]
    public static float VentTrapperStunTime = 5f;

    [CustomOptionBool("VentTrapperCanUseVent", false, translationName: "CanUseVent")]
    public static bool VentTrapperCanUseVent = false;
}

public record VentTrapperData(
    float coolTime,
    int useLimit,
    float setupTime,
    float stunTime,
    bool canUseVent
);

public class VentTrapperAbility : CustomButtonBase, IButtonEffect, IAbilityCount
{
    private List<Vent> _trappedVents = new();
    private List<int> _trappingVents = new();
    private readonly VentTrapperData _data;
    private Vent _targetVent;

    private EventListener<PlayerPhysicsRpcEnterVentPrefixEventData> _ventUsePrefixEvent;
    // IButtonEffect implementation for planting stun
    public bool isEffectActive { get; set; }
    public float EffectTimer { get; set; }
    public Action OnEffectEnds => FinishPlanting;
    public float EffectDuration => _data.setupTime;

    public VentTrapperAbility(VentTrapperData data)
    {
        _data = data;
        Count = data.useLimit;
    }

    public override Sprite Sprite => AssetManager.GetAsset<Sprite>("VentTrapperButton.png");
    public override string buttonText => ModTranslation.GetString("VentTrapperButtonText");
    protected override KeyType keytype => KeyType.Ability1;
    public override float DefaultTimer => _data.coolTime;
    public override ShowTextType showTextType => ShowTextType.ShowWithCount;
    public override bool CheckHasButton() => base.CheckHasButton() && Count > 0;
    private EventListener<PlayerPhysicsFixedUpdateEventData> _fixedUpdateEvent;
    private Vector3 _lastPosition;
    public override bool CheckIsAvailable()
    {
        if (isEffectActive) return false;
        if (!Player.Player.CanMove) return false;
        // Ventの近傍でのみ設置
        return TryGetNearbyVent(ShipStatus.Instance.AllVents.Where(x => !_trappedVents.Any(y => y.Id == x.Id)), out _targetVent);
    }

    public override void AttachToAlls()
    {
        base.AttachToAlls();
        _ventUsePrefixEvent = PlayerPhysicsRpcEnterVentPrefixEvent.Instance.AddListener(OnVentUsePrefix);
    }

    public override void DetachToAlls()
    {
        base.DetachToAlls();
        _ventUsePrefixEvent?.RemoveListener();
    }

    public override void AttachToLocalPlayer()
    {
        base.AttachToLocalPlayer();
        _fixedUpdateEvent = PlayerPhysicsFixedUpdateEvent.Instance.AddListener(OnFixedUpdate);
    }

    public override void DetachToLocalPlayer()
    {
        base.DetachToLocalPlayer();
        _fixedUpdateEvent?.RemoveListener();
    }

    private void OnFixedUpdate(PlayerPhysicsFixedUpdateEventData data)
    {
        if (!Player.AmOwner && !data.Instance.AmOwner) return;
        if (isEffectActive)
        {
            Player.Player.transform.position = _lastPosition;
            data.Instance.body.velocity = Vector2.zero;
        }
    }

    private void OnVentUsePrefix(PlayerPhysicsRpcEnterVentPrefixEventData data)
    {
        if (Player.AmOwner) return;
        if (_trappingVents.Contains(data.ventId))
        {
            data.result = false;
            PlayerControl.LocalPlayer.moveable = false;
            PlayerControl.LocalPlayer.MyPhysics.body.velocity = Vector2.zero;
            new LateTask(() =>
            {
                PlayerControl.LocalPlayer.moveable = true;
            }, _data.stunTime, "VentTrapperAbility");
            TrappedVentRPC(data.ventId);
            new CustomMessage(ModTranslation.GetString("VentTrapperStunMessage"), _data.stunTime);
            ShipStatus.Instance.AllVents.FirstOrDefault(x => x.Id == data.ventId)?.SetButtons(false);
        }
    }

    public override void OnClick()
    {
        if (_data.canUseVent && Player.Player.inVent)
        {
            // Ventから退出
            Player.Player.MyPhysics?.RpcExitVent(Vent.currentVent.Id);
            return;
        }
        if (!TryGetNearbyVent(ShipStatus.Instance.AllVents.Where(x => !_trappedVents.Any(y => y.Id == x.Id)), out _targetVent)) return;
        // Player.Player.moveable = false;
        Player.MyPhysics.body.velocity = Vector2.zero;
        _lastPosition = Player.Player.transform.position;
    }

    private void FinishPlanting()
    {
        Player.Player.moveable = true;
        this.UseAbilityCount();
        GameObject batu = new("Batu");
        batu.transform.position = _targetVent.transform.position - new Vector3(0, 0.05f, 0.01f);
        batu.transform.localScale = Vector3.one * 0.3f;
        batu.AddComponent<SpriteRenderer>().sprite = AssetManager.GetAsset<Sprite>("VentTrapped.png");
        SetTrapRPC(_targetVent.Id);
        ResetTimer();
    }

    private bool TryGetNearbyVent(IEnumerable<Vent> vents, out Vent vent)
    {
        Vector3 center = Player.Player.Collider.bounds.center;
        foreach (Vent v in vents)
        {
            if (Vector2.Distance(center, v.transform.position) <= v.UsableDistance &&
                !PhysicsHelpers.AnythingBetween(Player.Player.Collider, center, v.transform.position, Constants.ShipOnlyMask, useTriggers: false))
            {
                vent = v;
                return true;
            }
        }
        vent = null;
        return false;
    }

    [CustomRPC]
    public void SetTrapRPC(int ventId)
    {
        Vent vent = ShipStatus.Instance.AllVents.First(x => x.Id == ventId);
        if (vent == null)
        {
            Logger.Error($"Vent not found: {ventId}");
            return;
        }
        _trappedVents.Add(vent);
        _trappingVents.Add(ventId);
    }
    [CustomRPC]
    public void TrappedVentRPC(int ventId)
    {
        _trappingVents.Remove(ventId);
    }
}