using System;
using System.Collections.Generic;
using AmongUs.GameOptions;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability;
using UnityEngine;

namespace SuperNewRoles.Roles.Crewmate
{
    internal class SilverBullet : RoleBase<SilverBullet>
    {
        public override RoleId Role => RoleId.SilverBullet;
        public override Color32 RoleColor => new(204, 204, 204, byte.MaxValue);

        public override List<Func<AbilityBase>> Abilities { get; } = new()
        {
            () => new SilverBulletAnalyzeAbility(SilverBulletAnalysisCount, SilverBulletCoolTime),
            () => new SilverBulletRepairAbility(SilverBulletCanUseRepair, SilverBulletCanUseRepair ? SilverBulletRepairCount : 0, SilverBulletCoolTime),
            () => new CustomVentAbility(() => SilverBulletCanUseVent)
        };

        public override QuoteMod QuoteMod => QuoteMod.SuperNewRoles;

        public override RoleTypes IntroSoundType => RoleTypes.Engineer;
        public override short IntroNum => 1;

        public override TeamTag TeamTag => TeamTag.Crewmate;
        public override RoleTag[] RoleTags => new[] { RoleTag.Information };
        public override AssignedTeamType AssignedTeam => AssignedTeamType.Crewmate;
        public override WinnerTeamType WinnerTeam => WinnerTeamType.Crewmate;
        public override RoleOptionMenuType OptionTeam => RoleOptionMenuType.Crewmate;

        [CustomOptionInt("SilverBulletAnalyzeCanUseCountOption", 1, 15, 1, 1)]
        public static int SilverBulletAnalysisCount;

        [CustomOptionBool("SilverBulletAnalyzeCanCheckSentimentOption", false)]
        public static bool SilverBulletAnalysisLight;

        [CustomOptionBool("SilverBulletCanUseRepairOption", false)]
        public static bool SilverBulletCanUseRepair;

        [CustomOptionInt("SilverBulletCanRepairUseCountOption", 1, 15, 1, 1, parentFieldName: nameof(SilverBulletCanUseRepair))]
        public static int SilverBulletRepairCount;

        [CustomOptionFloat("SilverBulletCoolTime", 30f, 60f, 2.5f, 30f, translationName: "CoolTime")]
        public static float SilverBulletCoolTime;

        [CustomOptionBool("SilverBulletCanUseVent", true, translationName: "CanUseVent")]
        public static bool SilverBulletCanUseVent;
    }
}