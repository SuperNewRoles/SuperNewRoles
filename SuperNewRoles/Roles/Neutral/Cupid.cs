using System;
using System.Collections.Generic;
using System.Linq;
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
    public override RoleId Role { get; } = RoleId.Cupid;
    public override Color32 RoleColor { get; } = Lovers.Instance.RoleColor;
    public override List<Func<AbilityBase>> Abilities { get; } = [
        () => new CupidAbility(
            CupidCoolTime,
            CupidEnabledTimeLimit,
            CupidTimeLimit,
            CupidCanSeeCreatedLoversRole,
            CupidLoversCanSeeCupidRole
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
    [CustomOptionBool("CupidLoversCanSeeCupidRole", false)]
    public static bool CupidLoversCanSeeCupidRole;
}

public class CupidAbility : AbilityBase
{
    public float CoolTime { get; }
    private EventListener<NameTextUpdateEventData> _nameTextUpdateEvent;
    private EventListener<NameTextUpdateVisiableEventData> _nameTextUpdateVisiableEvent;
    public byte Lovers1 { get; private set; }
    public byte Lovers2 { get; private set; }

    private CreateLoversAbility createLoversAbility;

    public bool EnabledTimeLimit { get; }
    public float TimeLimit { get; }
    public bool CanSeeCreatedLoversRole { get; }
    public bool LoversCanSeeCupidRole { get; }

    public CupidAbility(float coolTime, bool enabledTimeLimit, float timeLimit, bool canSeeCreatedLoversRole, bool loversCanSeeCupidRole)
    {
        CoolTime = coolTime;
        EnabledTimeLimit = enabledTimeLimit;
        TimeLimit = timeLimit;
        CanSeeCreatedLoversRole = canSeeCreatedLoversRole;
        LoversCanSeeCupidRole = loversCanSeeCupidRole;
    }
    public override void AttachToAlls()
    {
        base.AttachToAlls();

        createLoversAbility = new CreateLoversAbility(
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
        _nameTextUpdateVisiableEvent = NameTextUpdateVisiableEvent.Instance.AddListener(OnNameTextUpdateVisiable);
    }
    public override void DetachToAlls()
    {
        base.DetachToAlls();
        _nameTextUpdateVisiableEvent?.RemoveListener();
    }
    public override void AttachToLocalPlayer()
    {
        base.AttachToLocalPlayer();
        _nameTextUpdateEvent = NameTextUpdateEvent.Instance.AddListener(OnNameTextUpdate);
    }
    public override void DetachToLocalPlayer()
    {
        base.DetachToLocalPlayer();
        _nameTextUpdateEvent?.RemoveListener();
    }

    private void OnNameTextUpdate(NameTextUpdateEventData data)
    {
        if (!Player.AmOwner) return;
        if (Player.IsDead()) return;
        if (createLoversAbility.CreatedCouple != null)
        {
            if (!createLoversAbility.CreatedCouple.lovers.Any(x => x.Player.PlayerId == data.Player.PlayerId)) return;
            if (!data.Player.IsLovers()) return;
            if (data.Player.cosmetics.nameText.text.Contains("♥")) return;
            NameText.AddNameText(data.Player, ModHelpers.Cs(Lovers.Instance.RoleColor, "♥"));
        }
        // まだ作ってないけど1人に刺してたら中抜きハートを付ける
        else if (createLoversAbility.CurrentTarget != null)
        {
            if (createLoversAbility.CurrentTarget.PlayerId != data.Player.PlayerId) return;
            if (data.Player.cosmetics.nameText.text.Contains("♡")) return;
            data.Player.cosmetics.nameText.text += ModHelpers.Cs(Lovers.Instance.RoleColor, "♡");
        }
    }

    private void OnNameTextUpdateVisiable(NameTextUpdateVisiableEventData data)
    {
        if (!CanSeeCreatedLoversRole && !LoversCanSeeCupidRole) return;

        bool shouldBeVisible = false;
        // キューピッドが作ったラバーズの役職を見れる
        if (CanSeeCreatedLoversRole &&
            Player.AmOwner &&
            createLoversAbility?.CreatedCouple?.lovers != null &&
            createLoversAbility.CreatedCouple.lovers.Any(x => x.Player.PlayerId == data.Player.PlayerId))
        {
            shouldBeVisible = true;
        }

        if (LoversCanSeeCupidRole &&
            data.Player.PlayerId == Player.PlayerId &&
            createLoversAbility?.CreatedCouple?.lovers != null &&
            createLoversAbility.CreatedCouple.lovers.Any(x => x.Player.AmOwner))
        {
            shouldBeVisible = true;
        }
        if (shouldBeVisible)
        {
            NameText.UpdateVisiable(data.Player, true);
        }
    }
}