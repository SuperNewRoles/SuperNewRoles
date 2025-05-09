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
        () => new CupidAbility(CupidCoolTime)
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
}

public class CupidAbility : AbilityBase
{
    public float CoolTime { get; }
    private EventListener<NameTextUpdateEventData> _nameTextUpdateEvent;
    private byte lovers1;
    private byte lovers2;

    public CupidAbility(float coolTime)
    {
        CoolTime = coolTime;
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
                lovers1 = players[0].PlayerId;
                lovers2 = players[1].PlayerId;
            });
        Player.AttachAbility(createLoversAbility, new AbilityParentAbility(this));

        _nameTextUpdateEvent = NameTextUpdateEvent.Instance.AddListener(OnNameTextUpdate);
    }
    public override void DetachToAlls()
    {
        base.DetachToAlls();
        _nameTextUpdateEvent?.RemoveListener();
    }

    private void OnNameTextUpdate(NameTextUpdateEventData data)
    {
        if (!Player.AmOwner) return;
        if (Player.IsDead()) return;
        if (lovers1 != data.Player.PlayerId && lovers2 != data.Player.PlayerId) return;
        if (!data.Player.IsLovers()) return;
        if (data.Player.cosmetics.nameText.text.Contains("♥")) return;
        data.Player.cosmetics.nameText.text += ModHelpers.Cs(Lovers.Instance.RoleColor, "♥");
    }
}