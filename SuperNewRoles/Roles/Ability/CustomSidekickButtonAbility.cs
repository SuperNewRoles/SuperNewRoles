using System;
using AmongUs.GameOptions;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability.CustomButton;
using UnityEngine;

namespace SuperNewRoles.Roles.Ability;


public class CustomSidekickButtonAbility : TargetCustomButtonBase
{
    private readonly Func<bool, bool> _canCreateSidekick;
    private readonly Func<float?> _sidekickCooldown;
    private readonly Func<RoleId> _sidekickRole;
    private readonly Func<RoleTypes?> _sidekickRoleVanilla;
    private readonly Action<ExPlayerControl> _onSidekickCreated;
    private readonly Func<ExPlayerControl, bool>? _sidekickSuccess;
    private readonly SidekickedPromoteData? _sidekickedPromoteData;
    private readonly Sprite _sidekickSprite;
    private readonly string _sidekickText;
    private readonly Func<ExPlayerControl, bool>? _isTargetable;
    public Action<float> OnCooldownStarted;
    private readonly bool _isSubButton;
    public override Color32 OutlineColor => new(0, 255, 255, 255);
    public override Sprite Sprite => _sidekickSprite;
    public override string buttonText => _sidekickText;
    protected override KeyType keytype => _isSubButton ? KeyType.Ability2 : KeyType.Ability1;
    public override float DefaultTimer => _sidekickCooldown?.Invoke() ?? 0;
    public override bool OnlyCrewmates => false;
    public override Func<ExPlayerControl, bool>? IsTargetable => _isTargetable;
    private bool _sidekickCreated = false;
    private Func<int> _sidekickCount = null;
    private Func<bool> _showSidekickLimitText = null;
    public override ShowTextType showTextType => _showSidekickLimitText == null ? ShowTextType.Hidden : ShowTextType.ShowWithCount;
    public override string showText => ModTranslation.GetString("RemainingText");
    public CustomSidekickButtonAbility(
        Func<bool, bool> canCreateSidekick,
        Func<float?> sidekickCooldown,
        Func<RoleId> sidekickRole,
        Func<RoleTypes?> sidekickRoleVanilla,
        Sprite sidekickSprite,
        string sidekickText,
        Func<int> sidekickCount,
        Func<ExPlayerControl, bool>? isTargetable,
        Func<ExPlayerControl, bool>? sidekickSuccess = null,
        SidekickedPromoteData? sidekickedPromoteData = null,
        Action<ExPlayerControl> onSidekickCreated = null,
        Func<bool> showSidekickLimitText = null,
        bool isSubButton = false)
    {
        _canCreateSidekick = canCreateSidekick;
        _sidekickCooldown = sidekickCooldown;
        _sidekickRole = sidekickRole;
        _sidekickRoleVanilla = sidekickRoleVanilla;
        _onSidekickCreated = onSidekickCreated;
        _sidekickSprite = sidekickSprite;
        _sidekickText = sidekickText;
        _sidekickSuccess = sidekickSuccess;
        _sidekickedPromoteData = sidekickedPromoteData;
        _isTargetable = isTargetable;
        _sidekickCount = sidekickCount;
        _showSidekickLimitText = showSidekickLimitText;
        Count = sidekickCount();
        _isSubButton = isSubButton;
    }

    public override void OnClick()
    {
        if (Target == null) return;
        if (!_canCreateSidekick(_sidekickCreated)) return;
        if (_sidekickSuccess == null || _sidekickSuccess(Target))
        {
            var vanillaRole = _sidekickRoleVanilla?.Invoke();
            RpcSidekicked(Player, Target, _sidekickRole(), vanillaRole ?? RoleTypes.Crewmate, vanillaRole != null);
            var target = Target;
            if (_sidekickedPromoteData != null)
            {
                new LateTask(() => RpcSetPromoteAbility(Player, target, _sidekickedPromoteData.PromoteToRole, _sidekickedPromoteData.PromoteToRoleVanilla), 0.05f);
            }
        }
        _onSidekickCreated?.Invoke(Target);
        _sidekickCreated = true;
        ResetTimer();
        OnCooldownStarted?.Invoke(DefaultTimer);
        Count--;
    }

    public override bool CheckIsAvailable()
    {
        if (!TargetIsExist) return false;
        if (!_canCreateSidekick(_sidekickCreated)) return false;
        if (!PlayerControl.LocalPlayer.CanMove || ExPlayerControl.LocalPlayer.IsDead()) return false;
        return true;
    }

    public override bool CheckHasButton()
    {
        return ExPlayerControl.LocalPlayer.IsAlive() && _canCreateSidekick(_sidekickCreated);
    }
    [CustomRPC]
    public static void RpcSidekicked(ExPlayerControl source, ExPlayerControl player, RoleId roleId, RoleTypes roleType, bool isVanilla)
    {
        player.SetRole(roleId);
        if (isVanilla)
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