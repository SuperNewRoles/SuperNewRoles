using System;
using AmongUs.GameOptions;
using Hazel;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability.CustomButton;
using UnityEngine;

namespace SuperNewRoles.Roles.Ability;

public class CustomSidekickButtonAbilityOptions
{
    public Func<bool, bool> CanCreateSidekick { get; }
    public Func<float?> SidekickCooldown { get; }
    public Func<RoleId> SidekickRole { get; }
    public Func<RoleTypes?> SidekickRoleVanilla { get; }
    public Sprite SidekickSprite { get; }
    public string SidekickText { get; }
    public Func<int> SidekickCount { get; }
    public Func<ExPlayerControl, bool>? IsTargetable { get; }
    public Func<ExPlayerControl, bool>? SidekickSuccess { get; }
    public SidekickedPromoteData? SidekickedPromoteData { get; }
    public Action<ExPlayerControl> OnSidekickCreated { get; }
    public Func<bool> ShowSidekickLimitText { get; }
    public bool IsSubButton { get; }

    public CustomSidekickButtonAbilityOptions(
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
        CanCreateSidekick = canCreateSidekick;
        SidekickCooldown = sidekickCooldown;
        SidekickRole = sidekickRole;
        SidekickRoleVanilla = sidekickRoleVanilla;
        SidekickSprite = sidekickSprite;
        SidekickText = sidekickText;
        SidekickCount = sidekickCount;
        IsTargetable = isTargetable;
        SidekickSuccess = sidekickSuccess;
        SidekickedPromoteData = sidekickedPromoteData;
        OnSidekickCreated = onSidekickCreated;
        ShowSidekickLimitText = showSidekickLimitText;
        IsSubButton = isSubButton;
    }
}

public class CustomSidekickButtonAbility : TargetCustomButtonBase
{
    private readonly CustomSidekickButtonAbilityOptions _options;

    public Action<float> OnCooldownStarted;
    private bool _sidekickCreated = false;

    public override Color32 OutlineColor => new(0, 255, 255, 255);
    public override Sprite Sprite => _options.SidekickSprite;
    public override string buttonText => _options.SidekickText;
    protected override KeyType keytype => _options.IsSubButton ? KeyType.Ability2 : KeyType.Ability1;
    public override float DefaultTimer => _options.SidekickCooldown?.Invoke() ?? 0;
    public override bool OnlyCrewmates => false;
    public override Func<ExPlayerControl, bool>? IsTargetable => _options.IsTargetable;
    public override ShowTextType showTextType => _options.ShowSidekickLimitText == null ? ShowTextType.Hidden : ShowTextType.ShowWithCount;
    public override string showText => ModTranslation.GetString("RemainingText");
    public CustomSidekickButtonAbility(CustomSidekickButtonAbilityOptions options)
    {
        _options = options;
        Count = _options.SidekickCount();
        _sidekickCreated = false;
    }

    public override void AttachToAlls()
    {
        base.AttachToAlls();
    }

    public override void OnClick()
    {
        if (Target == null) return;
        if (!_options.CanCreateSidekick(_sidekickCreated)) return;
        ExPlayerControl target = Target;
        if (_options.SidekickSuccess == null || _options.SidekickSuccess(target))
        {
            var vanillaRole = _options.SidekickRoleVanilla?.Invoke();
            RpcSidekicked(target, new SidekickData(
                _options.SidekickRole(),
                vanillaRole ?? RoleTypes.Crewmate,
                vanillaRole != null,
                _options.SidekickedPromoteData?.PromoteToRole,
                _options.SidekickedPromoteData?.PromoteToRoleVanilla
            ));
        }
        _options.OnSidekickCreated?.Invoke(target);
        _sidekickCreated = true;
        ResetTimer();
        OnCooldownStarted?.Invoke(DefaultTimer);
        Count--;
    }

    public override bool CheckIsAvailable()
    {
        if (!TargetIsExist) return false;
        if (!_options.CanCreateSidekick(_sidekickCreated)) return false;
        if (!PlayerControl.LocalPlayer.CanMove || ExPlayerControl.LocalPlayer.IsDead()) return false;
        return true;
    }

    public override bool CheckHasButton()
    {
        return ExPlayerControl.LocalPlayer.IsAlive() && _options.CanCreateSidekick(_sidekickCreated);
    }
    [CustomRPC]
    public void RpcSidekicked(ExPlayerControl player, SidekickData data)
    {
        Logger.Info($"RpcSidekicked: {player.PlayerId}, {data.RoleId}, {data.IsVanilla}, {data.IsPromote}, {data.PromoteToRole}, {data.PromoteToRoleVanilla}");

        player.SetRole(data.RoleId);
        if (data.IsPromote)
        {
            Logger.Info("SidekickData IsPromote");
            PromoteOnParentDeathAbility promoteAbility = new(
                new AbilityParentRole(Player, Player.roleBase),
                data.PromoteToRole,
                data.PromoteToRoleVanilla
            );
            Logger.Info("SidekickData AttachAbility to player: " + player.PlayerId);
            player.AttachAbility(promoteAbility, new AbilityParentRole(player, player.roleBase));
        }
        if (data.IsVanilla)
            RoleManager.Instance.SetRole(player, data.RoleType);
        NameText.UpdateAllNameInfo();
    }
}

public class SidekickData : ICustomRpcObject
{
    // sidekick Data
    public RoleId RoleId { get; private set; }
    public RoleTypes RoleType { get; private set; }
    public bool IsVanilla { get; private set; }
    // Promote Data
    public bool IsPromote { get; private set; }
    public RoleId PromoteToRole { get; private set; }
    public RoleTypes PromoteToRoleVanilla { get; private set; }
    public SidekickData() { }
    public SidekickData(RoleId roleId, RoleTypes roleType, bool isVanilla, RoleId? promoteToRole = null, RoleTypes? promoteToRoleVanilla = null)
    {
        RoleId = roleId;
        RoleType = roleType;
        IsVanilla = isVanilla;
        IsPromote = promoteToRole != null;
        PromoteToRole = promoteToRole ?? RoleId.None;
        PromoteToRoleVanilla = promoteToRoleVanilla ?? RoleTypes.Crewmate;
    }

    public void Serialize(MessageWriter writer)
    {
        writer.Write((ushort)RoleId);
        writer.Write((ushort)RoleType);
        writer.Write(IsVanilla);
        writer.Write(IsPromote);
        writer.Write((ushort)PromoteToRole);
        writer.Write((ushort)PromoteToRoleVanilla);
    }

    public void Deserialize(MessageReader reader)
    {
        RoleId = (RoleId)reader.ReadUInt16();
        RoleType = (RoleTypes)reader.ReadUInt16();
        IsVanilla = reader.ReadBoolean();
        IsPromote = reader.ReadBoolean();
        PromoteToRole = (RoleId)reader.ReadUInt16();
        PromoteToRoleVanilla = (RoleTypes)reader.ReadUInt16();
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