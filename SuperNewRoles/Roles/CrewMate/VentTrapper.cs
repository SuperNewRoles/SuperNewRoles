using System;
using System.Collections.Generic;
using UnityEngine;
using AmongUs.GameOptions;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability;
using SuperNewRoles.Roles.Ability.CustomButton;
using SuperNewRoles.Modules.Events.Bases;

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

public class VentTrapperAbility : CustomButtonBase
{
    private readonly VentTrapperData _data;
    private int _remainingUses;
    private Vent _targetVent;
    private bool _isPlanting;
    private float _plantTimer;

    public VentTrapperAbility(VentTrapperData data)
    {
        _data = data;
        _remainingUses = data.useLimit;
    }

    public override Sprite Sprite => AssetManager.GetAsset<Sprite>("VentTrapperButton.png");
    public override string buttonText => ModTranslation.GetString("VentTrapper.ButtonTitle");
    protected override KeyType keytype => KeyType.Ability1;
    public override float DefaultTimer => _data.coolTime;

    public override bool CheckHasButton() => Player.AmOwner && _remainingUses > 0;
    public override bool CheckIsAvailable()
    {
        if (_isPlanting) return false;
        if (!Player.Player.CanMove) return false;
        if (_data.canUseVent)
        {
            // Vent内でのみ設置
            return Player.Player.inVent && Vent.currentVent != null;
        }
        // Ventの近傍でのみ設置
        return TryGetNearbyVent(out _targetVent);
    }

    public override void OnClick()
    {
        if (_isPlanting) return;
        if (_data.canUseVent && Player.Player.inVent)
        {
            // Ventから退出
            Player.Player.MyPhysics?.RpcExitVent(Vent.currentVent.Id);
            return;
        }
        if (!TryGetNearbyVent(out _targetVent)) return;
        _isPlanting = true;
        Player.Player.moveable = false;
        _plantTimer = _data.setupTime;
    }

    public override void OnUpdate()
    {
        // UI update and cooldown handled by base
        base.OnUpdate();
        // handle planting timer
        if (!_isPlanting) return;
        if (MeetingHud.Instance != null)
        {
            _isPlanting = false;
            return;
        }
        _plantTimer -= Time.deltaTime;
        if (_plantTimer <= 0)
        {
            FinishPlanting();
        }
    }

    private void FinishPlanting()
    {
        _isPlanting = false;
        Player.Player.moveable = true;
        _remainingUses--;
        SetTrapRPC(this, _targetVent.Id, _data.stunTime);
        ResetTimer();
    }

    private bool TryGetNearbyVent(out Vent vent)
    {
        Vector3 center = Player.Player.Collider.bounds.center;
        foreach (Vent v in ShipStatus.Instance.AllVents)
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
    public static void SetTrapRPC(VentTrapperAbility ability, int ventId, float stunTime)
    {
        // TODO: トラップ登録と拘束処理を実装
        Logger.Info("SET TRAP!!!!!!!!");
    }
}