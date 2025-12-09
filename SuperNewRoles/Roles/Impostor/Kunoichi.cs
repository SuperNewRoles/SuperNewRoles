using System;
using System.Collections.Generic;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability;
using UnityEngine;

namespace SuperNewRoles.Roles.Impostor;

class Kunoichi : RoleBase<Kunoichi>
{
    public override RoleId Role => RoleId.Kunoichi;
    public override Color32 RoleColor => Palette.ImpostorRed;
    public override List<Func<AbilityBase>> Abilities => [
        () => new KunoichiAbility(
            KunoichiCoolTime,
            KunoichiKillKunai,
            KunoichiHideKunai,
            KunoichiIsHide,
            KunoichiHideTime,
            KunoichiIsWaitAndPressTheButtonToHide)
    ];
    public override QuoteMod QuoteMod => QuoteMod.SuperNewRoles;
    public override AssignedTeamType AssignedTeam => AssignedTeamType.Impostor;
    public override WinnerTeamType WinnerTeam => WinnerTeamType.Impostor;
    public override TeamTag TeamTag => TeamTag.Impostor;
    public override RoleTag[] RoleTags => [RoleTag.SpecialKiller, RoleTag.Killer];
    public override short IntroNum => 1;
    public override RoleOptionMenuType OptionTeam => RoleOptionMenuType.Impostor;

    [CustomOptionFloat("KunoichiCoolTime", 0f, 15f, 0.5f, 2.5f, translationName: "KunoichiCoolTime")]
    public static float KunoichiCoolTime;

    [CustomOptionInt("KunoichiKillKunai", 1, 20, 1, 10)]
    public static int KunoichiKillKunai;

    [CustomOptionBool("KunoichiIsHide", true, translationName: "KunoichiIsHide")]
    public static bool KunoichiIsHide;

    [CustomOptionBool("KunoichiIsWaitAndPressTheButtonToHide", true, parentFieldName: nameof(KunoichiIsHide), translationName: "KunoichiIsWaitAndPressTheButtonToHide")]
    public static bool KunoichiIsWaitAndPressTheButtonToHide;

    [CustomOptionFloat("KunoichiHideTime", 0.5f, 10f, 0.5f, 3f, parentFieldName: nameof(KunoichiIsHide), translationName: "KunoichiHideTime")]
    public static float KunoichiHideTime;

    [CustomOptionBool("KunoichiHideKunai", false, parentFieldName: nameof(KunoichiIsHide), translationName: "KunoichiHideKunai")]
    public static bool KunoichiHideKunai;
}

