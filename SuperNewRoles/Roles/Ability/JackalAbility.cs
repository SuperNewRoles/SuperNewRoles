using System;
using UnityEngine;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability.CustomButton;
using AmongUs.GameOptions;

namespace SuperNewRoles.Roles.Ability;

public class JackalAbility : AbilityBase
{
    public JackalData JackData { get; private set; }
    public CustomKillButtonAbility KillAbility { get; private set; }
    public CustomVentAbility VentAbility { get; private set; }
    public CustomSidekickButtonAbility SidekickAbility { get; private set; }
    public KnowOtherAbility KnowJackalAbility { get; private set; }

    public JackalAbility(JackalData jackData)
    {
        JackData = jackData;
    }

    public override void Attach(PlayerControl player, ulong abilityId, AbilityParentBase parent)
    {
        KillAbility = new CustomKillButtonAbility(
            () => JackData.CanKill,
            () => JackData.KillCooldown,
            onlyCrewmates: () => false,
            isTargetable: (player) => !player.IsJackal()
        );

        VentAbility = new CustomVentAbility(
            () => JackData.CanUseVent
        );

        SidekickAbility = new CustomSidekickButtonAbility(
            () => JackData.CanCreateSidekick,
            () => JackData.SidekickCooldown,
            () => RoleId.Sidekick,
            () => RoleTypes.Crewmate,
            AssetManager.GetAsset<Sprite>("JackalSidekickButton.png"),
            ModTranslation.GetString("SidekickButtonText")
        );

        KnowJackalAbility = new KnowOtherAbility(
            (player) => player.IsJackalTeam(),
            () => true
        );

        ExPlayerControl exPlayer = (ExPlayerControl)player;
        AbilityParentAbility parentAbility = new(this);
        exPlayer.AttachAbility(KillAbility, parentAbility);
        exPlayer.AttachAbility(VentAbility, parentAbility);
        exPlayer.AttachAbility(SidekickAbility, parentAbility);
        exPlayer.AttachAbility(KnowJackalAbility, parentAbility);

        base.Attach(player, abilityId, parent);
    }

    public override void AttachToLocalPlayer()
    {
    }
}

public class CustomSidekickButtonAbility : TargetCustomButtonBase
{
    private readonly Func<bool> _canCreateSidekick;
    private readonly Func<float?> _sidekickCooldown;
    private readonly Func<RoleId> _sidekickRole;
    private readonly Func<RoleTypes> _sidekickRoleVanilla;
    private readonly Action<ExPlayerControl> _onSidekickCreated;
    private readonly Sprite _sidekickSprite;
    private readonly string _sidekickText;
    public override Color32 OutlineColor => new Color32(0, 255, 255, 255);
    public override Vector3 LocalScale => Vector3.one;
    public override Sprite Sprite => _sidekickSprite;
    public override string buttonText => _sidekickText;
    public override Vector3 PositionOffset => new Vector3(0, 1.5f, 0);
    protected override KeyCode? hotkey => KeyCode.F;
    protected override int joystickkey => 1;
    public override float DefaultTimer => _sidekickCooldown?.Invoke() ?? 0;
    public override bool OnlyCrewmates => false;
    public override Color? color => Color.cyan;
    public override Func<ExPlayerControl, bool>? IsTargetable => (player) => !player.IsJackal();
    public bool SidekickCreated { get; private set; }
    public CustomSidekickButtonAbility(
        Func<bool> canCreateSidekick,
        Func<float?> sidekickCooldown,
        Func<RoleId> sidekickRole,
        Func<RoleTypes> sidekickRoleVanilla,
        Sprite sidekickSprite,
        string sidekickText,
        Action<ExPlayerControl> onSidekickCreated = null)
    {
        _canCreateSidekick = canCreateSidekick;
        _sidekickCooldown = sidekickCooldown;
        _sidekickRole = sidekickRole;
        _sidekickRoleVanilla = sidekickRoleVanilla;
        _onSidekickCreated = onSidekickCreated;
        _sidekickSprite = sidekickSprite;
        _sidekickText = sidekickText;
    }

    public override void OnClick()
    {
        if (Target == null) return;
        if (!_canCreateSidekick() || SidekickCreated) return;

        RpcSidekicked(Target, _sidekickRole(), _sidekickRoleVanilla());
        _onSidekickCreated?.Invoke(Target);
        SidekickCreated = true;
        ResetTimer();
    }

    public override bool CheckIsAvailable()
    {
        if (!TargetIsExist) return false;
        if (!_canCreateSidekick()) return false;
        if (!PlayerControl.LocalPlayer.CanMove || ExPlayerControl.LocalPlayer.IsDead()) return false;
        return true;
    }

    public override bool CheckHasButton()
    {
        return _canCreateSidekick() && !SidekickCreated;
    }
    [CustomRPC]
    public static void RpcSidekicked(ExPlayerControl player, RoleId roleId, RoleTypes roleType)
    {
        player.SetRole(roleId);
        RoleManager.Instance.SetRole(player, roleType);
        NameText.UpdateAllNameInfo();
    }
}

public class JackalData
{
    public bool CanKill { get; }
    public float KillCooldown { get; }
    public bool CanUseVent { get; }
    public bool CanCreateSidekick { get; }
    public float SidekickCooldown { get; }

    public JackalData(bool canKill, float killCooldown, bool canUseVent, bool canCreateSidekick, float sidekickCooldown)
    {
        CanKill = canKill;
        KillCooldown = killCooldown;
        CanUseVent = canUseVent;
        CanCreateSidekick = canCreateSidekick;
        SidekickCooldown = sidekickCooldown;
    }
}