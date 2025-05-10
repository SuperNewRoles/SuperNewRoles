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

public class VentTrapperAbility : CustomButtonBase, IButtonEffect
{
    private List<Vent> _trappedVents = new();
    private List<Vent> _trappingVents = new();
    private readonly VentTrapperData _data;
    private int _remainingUses;
    private Vent _targetVent;
    // IButtonEffect implementation for planting stun
    public bool isEffectActive { get; set; }
    public float EffectTimer { get; set; }
    public Action OnEffectEnds => FinishPlanting;
    public float EffectDuration => _data.setupTime;

    public VentTrapperAbility(VentTrapperData data)
    {
        _data = data;
        _remainingUses = data.useLimit;
    }

    public override Sprite Sprite => AssetManager.GetAsset<Sprite>("VentTrapperButton.png");
    public override string buttonText => ModTranslation.GetString("VentTrapper.ButtonTitle");
    protected override KeyType keytype => KeyType.Ability1;
    public override float DefaultTimer => _data.coolTime;

    public override bool CheckHasButton() => base.CheckHasButton() && _remainingUses > 0;
    public override bool CheckIsAvailable()
    {
        if (isEffectActive) return false;
        if (!Player.Player.CanMove) return false;
        if (_data.canUseVent)
        {
            // Vent内でのみ設置
            return Player.Player.inVent && Vent.currentVent != null;
        }
        // Ventの近傍でのみ設置
        return TryGetNearbyVent(ShipStatus.Instance.AllVents.Where(x => !_trappedVents.Contains(x)), out _targetVent);
    }

    public override void OnClick()
    {
        if (_data.canUseVent && Player.Player.inVent)
        {
            // Ventから退出
            Player.Player.MyPhysics?.RpcExitVent(Vent.currentVent.Id);
            return;
        }
        if (!TryGetNearbyVent(ShipStatus.Instance.AllVents.Where(x => !_trappedVents.Contains(x)), out _targetVent)) return;
        Player.Player.moveable = false;
        Player.MyPhysics.body.velocity = Vector2.zero;
    }

    private void FinishPlanting()
    {
        Player.Player.moveable = true;
        _remainingUses--;
        GameObject batu = new("Batu");
        batu.transform.parent = _targetVent.transform;
        batu.transform.position = _targetVent.transform.position - new Vector3(0, 0.1f, 0.1f);
        batu.AddComponent<SpriteRenderer>().sprite = AssetManager.GetAsset<Sprite>("DoorClosed.png");
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
        // TODO: トラップ登録と拘束処理を実装
        Vent vent = ShipStatus.Instance.AllVents.First(x => x.Id == ventId);
        if (vent == null)
        {
            Logger.Error($"Vent not found: {ventId}");
            return;
        }
        _trappedVents.Add(vent);
        _trappingVents.Add(vent);
    }
}