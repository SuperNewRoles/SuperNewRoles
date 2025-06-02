using System;
using System.Collections.Generic;
using UnityEngine;
using AmongUs.GameOptions;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Roles.Ability;
using SuperNewRoles.Roles.Ability.CustomButton;
using SuperNewRoles.Modules;
using SuperNewRoles.Events.PCEvents;
using SuperNewRoles.Modules.Events.Bases;
using SuperNewRoles.Ability;

namespace SuperNewRoles.Roles.Impostor;

class DoubleKiller : RoleBase<DoubleKiller>
{
    public override RoleId Role { get; } = RoleId.DoubleKiller;
    public override Color32 RoleColor { get; } = Palette.ImpostorRed;
    public override List<Func<AbilityBase>> Abilities { get; } = [
        () => new DoubleKillerAbility(new(DoubleKillerKillCountRemaining ? DoubleKillerMaxKillCount : null))
    ];

    public override QuoteMod QuoteMod { get; } = QuoteMod.SuperNewRoles;
    public override RoleTypes IntroSoundType { get; } = RoleTypes.Impostor;
    public override short IntroNum { get; } = 1;

    public override AssignedTeamType AssignedTeam { get; } = AssignedTeamType.Impostor;
    public override WinnerTeamType WinnerTeam { get; } = WinnerTeamType.Impostor;
    public override TeamTag TeamTag { get; } = TeamTag.Impostor;
    public override RoleTag[] RoleTags { get; } = [];
    public override RoleOptionMenuType OptionTeam { get; } = RoleOptionMenuType.Impostor;

    [CustomOptionBool("DoubleKillerKillCountRemaining", true)]
    public static bool DoubleKillerKillCountRemaining;

    [CustomOptionInt("DoubleKillerMaxKillCount", 1, 10, 1, 1)]
    public static int DoubleKillerMaxKillCount;
}

public record DoubleKillerAbilityData(int? DoubleKillerCount);

public class DoubleKillerAbility : AbilityBase, IAbilityCount
{
    public DoubleKillerAbilityData DoubleKillerAbilityData { get; set; }

    public DoubleKillerAbility(DoubleKillerAbilityData doubleKillerAbilityData)
    {
        DoubleKillerAbilityData = doubleKillerAbilityData;
    }
    public override void AttachToAlls()
    {
        base.AttachToAlls();
        Player.AttachAbility(new CustomKillButtonAbility(
            () => true, () => GameOptionsManager.Instance.CurrentGameOptions.GetFloat(FloatOptionNames.KillCooldown), () => true), new AbilityParentAbility(this));
        Player.AttachAbility(new CustomKillButtonAbility(
            () => DoubleKillerAbilityData.DoubleKillerCount.HasValue ? Count <= DoubleKillerAbilityData.DoubleKillerCount.Value : true, () => GameOptionsManager.Instance.CurrentGameOptions.GetFloat(FloatOptionNames.KillCooldown), onlyCrewmates: () => true,
            killedCallback: x => this.UseAbilityCount(),
            showTextType: () => DoubleKillerAbilityData.DoubleKillerCount.HasValue ? ShowTextType.Show : ShowTextType.Hidden,
            showText: () => DoubleKillerAbilityData.DoubleKillerCount.HasValue ? string.Format(ModTranslation.GetString("RemainingText"), (DoubleKillerAbilityData.DoubleKillerCount - Count).ToString()) : ""
        ), new AbilityParentAbility(this));
    }
}