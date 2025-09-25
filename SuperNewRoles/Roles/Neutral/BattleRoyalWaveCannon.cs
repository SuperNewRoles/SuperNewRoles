using System;
using System.Collections.Generic;
using UnityEngine;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability;
using SuperNewRoles.WaveCannonObj;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Mode;

namespace SuperNewRoles.Roles.Neutral;

class BattleRoyalWaveCannon : RoleBase<BattleRoyalWaveCannon>, IRoleBase
{
    public override RoleId Role => RoleId.BattleRoyalWaveCannon;
    public override Color32 RoleColor => new Color32(128, 128, 128, 255); // グレー色
    public override string RoleName => "BattleRoyalWaveCannon";
    public override List<Func<AbilityBase>> Abilities => [
        () => new WaveCannonAbility(
            WCBattleRoyalMode.WaveCannonBattleRoyalCooldown,
            WCBattleRoyalMode.WaveCannonBattleRoyalChargeTime,
            WaveCannonType.Tank,
            false,
            friendlyFire: WCBattleRoyalMode.WaveCannonBattleRoyalFriendlyFire,
            KillSound: WCBattleRoyalMode.WaveCannonBattleRoyalKillSound,
            distributedKillSound: WCBattleRoyalMode.WaveCannonBattleRoyalKillSoundDistributed

        )
    ];
    public override QuoteMod QuoteMod => QuoteMod.SuperNewRoles;
    public override AssignedTeamType AssignedTeam => AssignedTeamType.Neutral;
    public override WinnerTeamType WinnerTeam => WinnerTeamType.Neutral;
    public override TeamTag TeamTag => TeamTag.Neutral;
    public override RoleTag[] RoleTags => [RoleTag.Information, RoleTag.SpecialWinner];
    public override short IntroNum => 1;
    public override RoleOptionMenuType OptionTeam => RoleOptionMenuType.Hidden; // バトルロワイヤルモードでのみ使用
}