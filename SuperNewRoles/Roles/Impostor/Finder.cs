using System;
using System.Collections.Generic;
using AmongUs.GameOptions;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Events.PCEvents;
using SuperNewRoles.Modules;
using SuperNewRoles.Modules.Events.Bases;
using SuperNewRoles.Roles.Ability;
using UnityEngine;

namespace SuperNewRoles.Roles.Impostor;

internal class Finder : RoleBase<Finder>
{
    public override RoleId Role => RoleId.Finder;
    public override Color32 RoleColor => Palette.ImpostorRed;
    public override List<Func<AbilityBase>> Abilities => new()
    {
        () => new ChangeKillTimerAbility(() => FinderKillCooldown),
        () => new FinderAbility(FinderRevealKillCount)
    };

    public override QuoteMod QuoteMod => QuoteMod.SuperNewRoles;
    public override RoleTypes IntroSoundType => RoleTypes.Impostor;
    public override short IntroNum => 1;

    public override AssignedTeamType AssignedTeam => AssignedTeamType.Impostor;
    public override WinnerTeamType WinnerTeam => WinnerTeamType.Impostor;
    public override TeamTag TeamTag => TeamTag.Impostor;
    public override RoleTag[] RoleTags => [RoleTag.Information];
    public override RoleOptionMenuType OptionTeam => RoleOptionMenuType.Impostor;

    [CustomOptionFloat("FinderKillCooldown", 2.5f, 60f, 2.5f, 25f, translationName: "KillCoolTime")]
    public static float FinderKillCooldown;

    [CustomOptionInt("FinderRevealKillCount", 1, 10, 1, 2)]
    public static int FinderRevealKillCount;
}

public class FinderAbility : AbilityBase
{
    private readonly int _neededKills;
    private int _killCount;
    private bool _revealed;
    private KnowOtherAbility _knowMadmateAbility;
    private EventListener<MurderEventData> _murderListener;

    public FinderAbility(int neededKills)
    {
        _neededKills = neededKills;
    }

    public override void AttachToAlls()
    {
        _knowMadmateAbility = new KnowOtherAbility(
            player => player.IsMadRoles(),
            () => _revealed
        );
        Player.AttachAbility(_knowMadmateAbility, new AbilityParentAbility(this));
    }

    public override void AttachToLocalPlayer()
    {
        base.AttachToLocalPlayer();
        _murderListener = MurderEvent.Instance.AddListener(OnMurder);
    }

    public override void DetachToLocalPlayer()
    {
        base.DetachToLocalPlayer();
        _murderListener?.RemoveListener();
    }

    private void OnMurder(MurderEventData data)
    {
        if (data.killer == Player && !_revealed)
        {
            _killCount++;
            if (_killCount >= _neededKills)
            {
                _revealed = true;
                NameText.UpdateAllNameInfo();
            }
        }
    }
}
