using System;
using AmongUs.GameOptions;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability.CustomButton;
using UnityEngine;

namespace SuperNewRoles.Roles.Ability;


public class CustomSidekickButtonAbility : TargetCustomButtonBase
{
    private readonly Func<bool> _canCreateSidekick;
    private readonly Func<float?> _sidekickCooldown;
    private readonly Func<RoleId> _sidekickRole;
    private readonly Func<RoleTypes> _sidekickRoleVanilla;
    private readonly Action<ExPlayerControl> _onSidekickCreated;
    private readonly SidekickedPromoteData? _sidekickedPromoteData;
    private readonly Sprite _sidekickSprite;
    private readonly string _sidekickText;
    private readonly Func<ExPlayerControl, bool>? _isTargetable;
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
    public override Func<ExPlayerControl, bool>? IsTargetable => _isTargetable;
    public bool SidekickCreated { get; private set; }
    public CustomSidekickButtonAbility(
        Func<bool> canCreateSidekick,
        Func<float?> sidekickCooldown,
        Func<RoleId> sidekickRole,
        Func<RoleTypes> sidekickRoleVanilla,
        Sprite sidekickSprite,
        string sidekickText,
        Func<ExPlayerControl, bool>? isTargetable,
        SidekickedPromoteData? sidekickedPromoteData = null,
        Action<ExPlayerControl> onSidekickCreated = null)
    {
        _canCreateSidekick = canCreateSidekick;
        _sidekickCooldown = sidekickCooldown;
        _sidekickRole = sidekickRole;
        _sidekickRoleVanilla = sidekickRoleVanilla;
        _onSidekickCreated = onSidekickCreated;
        _sidekickSprite = sidekickSprite;
        _sidekickText = sidekickText;
        _sidekickedPromoteData = sidekickedPromoteData;
        _isTargetable = isTargetable;
    }

    public override void OnClick()
    {
        if (Target == null) return;
        if (!_canCreateSidekick() || SidekickCreated) return;

        RpcSidekicked(Player, Target, _sidekickRole(), _sidekickRoleVanilla());
        if (_sidekickedPromoteData != null)
        {
            new LateTask(() => RpcSetPromoteAbility(Player, Target, _sidekickedPromoteData.PromoteToRole, _sidekickedPromoteData.PromoteToRoleVanilla), 0.05f);
        }
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
    public static void RpcSidekicked(ExPlayerControl source, ExPlayerControl player, RoleId roleId, RoleTypes roleType)
    {
        player.SetRole(roleId);
        RoleManager.Instance.SetRole(player, roleType);
        NameText.UpdateAllNameInfo();
    }
    [CustomRPC]
    public static void RpcSetPromoteAbility(ExPlayerControl source, ExPlayerControl player, RoleId roleId, RoleTypes roleType)
    {
        PromoteOnParentDeathAbility promoteAbility = new(
            new AbilityParentRole(source, source.roleBase),
            roleId,
            roleType
        );
        player.AttachAbility(promoteAbility, new AbilityParentRole(player, player.roleBase));
    }
}
public class SidekickedPromoteData
{
    public RoleId PromoteToRole { get; }
    public RoleTypes PromoteToRoleVanilla { get; }
    public SidekickedPromoteData(RoleId promoteToRole, RoleTypes promoteToRoleVanilla)
    {
        PromoteToRole = promoteToRole;
        PromoteToRoleVanilla = promoteToRoleVanilla;
    }
}