using System;
using System.Collections.Generic;
using SuperNewRoles.Events.PCEvents;
using SuperNewRoles.Modules;
using SuperNewRoles.Modules.Events.Bases;
using SuperNewRoles.Roles.Ability;
using UnityEngine;

namespace SuperNewRoles.Roles.Modifiers;


class Lovers : ModifierBase<Lovers>
{
    public override ModifierRoleId ModifierRole => ModifierRoleId.Lovers;

    public override Color32 RoleColor => new(255, 105, 180, byte.MaxValue);

    public override List<Func<AbilityBase>> Abilities => [() => new LoversAbility(), () => new CustomTaskAbility(() => (false, 0), null)];

    public override QuoteMod QuoteMod => QuoteMod.SuperNewRoles;

    public override List<AssignedTeamType> AssignedTeams => [];

    public override WinnerTeamType WinnerTeam => WinnerTeamType.Crewmate;

    public override TeamTag TeamTag => TeamTag.Crewmate;

    public override RoleTag[] RoleTags => [];

    public override short IntroNum => 1;
    public override Func<ExPlayerControl, string> ModifierMark => (player) => "{0}" + ModHelpers.Cs(player.AmOwner && player.IsAlive() ? RoleColor : player.GetAbility<LoversAbility>().HeartColor, "♡");
    public override bool HiddenOption => true;
    [Modifier]
    public static CustomOptionCategory LoversCategory;

    [CustomOptionFloat("LoversMaxCoupleCount", 0f, 0f, 15f, 1f, parentFieldName: nameof(LoversCategory))]
    public static float LoversMaxCoupleCount;

    [CustomOptionFloat("LoversSpawnChance", 0f, 0f, 100f, 10f, parentFieldName: nameof(LoversCategory))]
    public static float LoversSpawnChance;

    [CustomOptionBool("LoversDieWithPartner", true, parentFieldName: nameof(LoversCategory))]
    public static bool LoversDieWithPartner;

    [CustomOptionBool("LoversAvoidQuarreled", true, parentFieldName: nameof(LoversCategory))]
    public static bool LoversAvoidQuarreled;

    [CustomOptionBool("LoversAdditionalWinCondition", true, parentFieldName: nameof(LoversCategory))]
    public static bool LoversAdditionalWinCondition;

    [CustomOptionBool("LoversIncludeImpostorsInSelection", false, parentFieldName: nameof(LoversCategory))]
    public static bool LoversIncludeImpostorsInSelection;

    [CustomOptionBool("LoversIncludeThirdTeamInSelection", false, parentFieldName: nameof(LoversCategory))]
    public static bool LoversIncludeThirdTeamInSelection;
}

class LoversAbility : AbilityBase
{
    public Color32 HeartColor => couple.HeartColor;
    public LoversCouple couple { get; private set; }
    private EventListener<DieEventData> _dieListener;
    public LoversAbility()
    {
    }
    public void SetCouple(LoversCouple couple)
    {
        this.couple = couple;
    }
    public override void AttachToLocalPlayer()
    {
        _dieListener = DieEvent.Instance.AddListener(OnDie);
    }
    public override void DetachToLocalPlayer()
    {
        _dieListener?.RemoveListener();
    }
    private void OnDie(DieEventData data)
    {
        if (ExPlayerControl.LocalPlayer.IsDead()) return;
        if (IsCoupleWith(data.player))
        {
            ExPlayerControl.LocalPlayer.RpcCustomDeath(CustomDeathType.LoversSuicide);
        }
    }
    private bool IsCoupleWith(ExPlayerControl player)
    {
        return couple.players.Contains(player);
    }
}

public class LoversCouple
{
    public Color32 HeartColor { get; set; }
    public List<ExPlayerControl> players { get; set; } = [];
}