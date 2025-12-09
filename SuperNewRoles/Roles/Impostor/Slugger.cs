using System;
using System.Collections.Generic;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability;
using SuperNewRoles.Roles.Ability.CustomButton;
using UnityEngine;

namespace SuperNewRoles.Roles.Impostor;

class Slugger : RoleBase<Slugger>
{
    public override RoleId Role => RoleId.Slugger;
    public override Color32 RoleColor => Palette.ImpostorRed;
    public override List<Func<AbilityBase>> Abilities => [
        () => new SluggerAbility(
            SluggerCoolTime,
            SluggerChargeTime,
            SluggerIsMultiKill,
            SluggerIsSyncKillCoolTime)
    ];
    public override QuoteMod QuoteMod => QuoteMod.SuperNewRoles;
    public override AssignedTeamType AssignedTeam => AssignedTeamType.Impostor;
    public override WinnerTeamType WinnerTeam => WinnerTeamType.Impostor;
    public override TeamTag TeamTag => TeamTag.Impostor;
    public override RoleTag[] RoleTags => [RoleTag.SpecialKiller, RoleTag.Killer];
    public override short IntroNum => 1;
    public override RoleOptionMenuType OptionTeam => RoleOptionMenuType.Impostor;

    [CustomOptionFloat("SluggerCoolTime", 2.5f, 60f, 2.5f, 20f, translationName: "CoolTime")]
    public static float SluggerCoolTime;

    [CustomOptionFloat("SluggerChargeTime", 0.5f, 10f, 0.5f, 3f)]
    public static float SluggerChargeTime;

    [CustomOptionBool("SluggerIsMultiKill", false, translationName: "SluggerIsMultiKill")]
    public static bool SluggerIsMultiKill;

    [CustomOptionBool("SluggerIsSyncKillCoolTime", false, translationName: "SluggerIsSyncKillCoolTime")]
    public static bool SluggerIsSyncKillCoolTime;
}