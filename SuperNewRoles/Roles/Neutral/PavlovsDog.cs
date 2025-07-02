using System;
using System.Collections.Generic;
using AmongUs.GameOptions;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability;
using UnityEngine;

namespace SuperNewRoles.Roles.Neutral;

class PavlovsDog : RoleBase<PavlovsDog>
{
    public override RoleId Role => RoleId.PavlovsDog;
    public override Color32 RoleColor => new(244, 169, 106, byte.MaxValue);
    public override List<Func<AbilityBase>> Abilities => new()
    {
        () => new PavlovsDogAbility(new PavlovsDogData(
            canUseVent: PavlovsOwner.PavlovsDogCanUseVent,
            isImpostorVision: PavlovsOwner.PavlovsDogIsImpostorVision,
            killCooldown: PavlovsOwner.PavlovsDogKillCooldown,
            rampageKillCooldown: PavlovsOwner.PavlovsDogRampageKillCooldown,
            rampageSuicideTime: PavlovsOwner.PavlovsDogRampageSuicideTime,
            resetSuicideTimeOnMeeting: PavlovsOwner.PavlovsDogResetSuicideTimeOnMeeting
        ))
    };
    public override QuoteMod QuoteMod => QuoteMod.SuperNewRoles;
    public override RoleTypes IntroSoundType => RoleTypes.Shapeshifter;
    public override short IntroNum => 1;
    public override AssignedTeamType AssignedTeam => AssignedTeamType.Neutral;
    public override WinnerTeamType WinnerTeam => WinnerTeamType.Neutral;
    public override TeamTag TeamTag => TeamTag.Neutral;
    public override RoleTag[] RoleTags => new[] { RoleTag.SpecialKiller };
    public override RoleOptionMenuType OptionTeam => RoleOptionMenuType.Hidden;
    public override RoleId[] RelatedRoleIds => new[] { RoleId.PavlovsOwner };

}