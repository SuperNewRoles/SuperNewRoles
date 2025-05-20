using System;
using System.Collections.Generic;
using AmongUs.GameOptions;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Events;
using SuperNewRoles.Modules;
using SuperNewRoles.Modules.Events.Bases;
using SuperNewRoles.Roles.Ability;
using SuperNewRoles.Roles.Modifiers;
using UnityEngine;

namespace SuperNewRoles.Roles.Neutral;

class Cupid : RoleBase<Cupid>
{
    public override RoleId Role { get; } = RoleId.Cupid; // RoleId.Cupid が enum に存在すると仮定
    public override Color32 RoleColor { get; } = Lovers.Instance.RoleColor;
    public override List<Func<AbilityBase>> Abilities { get; } = [
        () => new CupidAbility(
            CupidCoolTime,
            CupidEnabledTimeLimit,
            CupidTimeLimit,
            CupidCanSeeCreatedLoversRole
        )
    ];

    public override QuoteMod QuoteMod { get; } = QuoteMod.SuperNewRoles;
    public override RoleTypes IntroSoundType { get; } = RoleTypes.Crewmate;
    public override short IntroNum { get; } = 1;

    public override AssignedTeamType AssignedTeam { get; } = AssignedTeamType.Neutral;
    public override WinnerTeamType WinnerTeam { get; } = WinnerTeamType.Neutral;
    public override TeamTag TeamTag { get; } = TeamTag.Neutral;
    public override RoleTag[] RoleTags { get; } = [];
    public override RoleOptionMenuType OptionTeam { get; } = RoleOptionMenuType.Neutral;
    public override RoleId[] RelatedRoleIds { get; } = [];
    [CustomOptionFloat("CupidCoolTime", 0f, 180f, 2.5f, 0f, translationName: "CoolTime")]
    public static float CupidCoolTime;
    [CustomOptionBool("CupidEnabledTimeLimit", true)]
    public static bool CupidEnabledTimeLimit;
    [CustomOptionFloat("CupidTimeLimit", 30f, 600f, 15f, 120f, parentFieldName: nameof(CupidEnabledTimeLimit))]
    public static float CupidTimeLimit;
    [CustomOptionBool("CupidCanSeeCreatedLoversRole", false)]
    public static bool CupidCanSeeCreatedLoversRole;
}

public class CupidAbility : AbilityBase
{
    public float CoolTime { get; }
    private EventListener<NameTextUpdateEventData> _nameTextUpdateEvent;
    private EventListener<NameTextUpdateVisiableEventData> _nameTextUpdateVisiableEvent;
    public byte Lovers1 { get; private set; }
    public byte Lovers2 { get; private set; }

    public bool EnabledTimeLimit { get; }
    public float TimeLimit { get; }
    public bool CanSeeCreatedLoversRole { get; }

    public CupidAbility(float coolTime, bool enabledTimeLimit, float timeLimit, bool canSeeCreatedLoversRole)
    {
        CoolTime = coolTime;
        EnabledTimeLimit = enabledTimeLimit;
        TimeLimit = timeLimit;
        CanSeeCreatedLoversRole = canSeeCreatedLoversRole;
    }
    public override void AttachToAlls()
    {
        base.AttachToAlls();

        var createLoversAbility = new CreateLoversAbility(
            CoolTime,
            ModTranslation.GetString("CupidLoveArrow"),
            AssetManager.GetAsset<Sprite>("CupidButton.png"),
            false,
            (players) =>
            {
                Lovers1 = players[0].PlayerId;
                Lovers2 = players[1].PlayerId;
            },
            EnabledTimeLimit,
            TimeLimit
        );
        Player.AttachAbility(createLoversAbility, new AbilityParentAbility(this));
    }
    public override void AttachToLocalPlayer()
    {
        base.AttachToLocalPlayer();
        _nameTextUpdateEvent = NameTextUpdateEvent.Instance.AddListener(OnNameTextUpdate);
        _nameTextUpdateVisiableEvent = NameTextUpdateVisiableEvent.Instance.AddListener(OnNameTextUpdateVisiable);
    }
    public override void DetachToLocalPlayer()
    {
        base.DetachToLocalPlayer();
        _nameTextUpdateEvent?.RemoveListener();
        _nameTextUpdateVisiableEvent?.RemoveListener();
    }

    private void OnNameTextUpdate(NameTextUpdateEventData data)
    {
        if (!Player.AmOwner) return;
        if (Player.IsDead()) return;
        if (Lovers1 != data.Player.PlayerId && Lovers2 != data.Player.PlayerId) return;
        if (!data.Player.IsLovers()) return;
        if (data.Player.cosmetics.nameText.text.Contains("♥")) return;
        data.Player.cosmetics.nameText.text += ModHelpers.Cs(Lovers.Instance.RoleColor, "♥");
    }

    private void OnNameTextUpdateVisiable(NameTextUpdateVisiableEventData data)
    {
        if (!CanSeeCreatedLoversRole) return;
        if (data.Player.PlayerId != Lovers1 && data.Player.PlayerId != Lovers2) return;
        NameText.UpdateVisiable(data.Player, true);
    }
}