using System;
using System.Collections.Generic;
using AmongUs.GameOptions;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability;
using UnityEngine;
using System.Linq;
using SuperNewRoles.Events;
using SuperNewRoles.Events.PCEvents;
using SuperNewRoles.Modules.Events.Bases;
using SuperNewRoles.SuperTrophies;
using SuperNewRoles.Patches;

namespace SuperNewRoles.Roles.Impostor;

class SerialKiller : RoleBase<SerialKiller>
{
    public override RoleId Role { get; } = RoleId.SerialKiller;
    public override Color32 RoleColor { get; } = Palette.ImpostorRed;
    public override List<Func<AbilityBase>> Abilities { get; } = [
        () => new ChangeKillTimerAbility(
            killTimerGetter: () => SerialKillerKillCooldown
        ),
        () => new SuicideTimerAbility(
            suicideTimeGetter: () => SerialKillerSuicideTime,
            resetOnMeetingGetter: () => SerialKillerResetOnMeeting
        ),
        () => new CustomSaboAbility(
            canSabotage: () => SerialKillerCanSabotage
        ),
        () => new CustomVentAbility(
            canUseVent: () => SerialKillerCanUseVent
        )
    ];

    public override QuoteMod QuoteMod { get; } = QuoteMod.TheOtherRolesGM;
    public override RoleTypes IntroSoundType { get; } = RoleTypes.Impostor;
    public override short IntroNum { get; } = 1;

    public override AssignedTeamType AssignedTeam { get; } = AssignedTeamType.Impostor;
    public override WinnerTeamType WinnerTeam { get; } = WinnerTeamType.Impostor;
    public override TeamTag TeamTag { get; } = TeamTag.Impostor;
    public override RoleTag[] RoleTags { get; } = [RoleTag.SpecialKiller];
    public override RoleOptionMenuType OptionTeam { get; } = RoleOptionMenuType.Impostor;

    [CustomOptionBool("SerialKillerResetOnMeeting", false)]
    public static bool SerialKillerResetOnMeeting;

    [CustomOptionFloat("SerialKillerKillCooldown", 2.5f, 120f, 2.5f, 20f)]
    public static float SerialKillerKillCooldown;

    [CustomOptionFloat("SerialKillerSuicideTime", 2.5f, 120f, 2.5f, 40f)]
    public static float SerialKillerSuicideTime;

    [CustomOptionBool("SerialKillerCanUseVent", true, translationName: "CanUseVent")]
    public static bool SerialKillerCanUseVent;

    [CustomOptionBool("SerialKillerCanSabotage", true)]
    public static bool SerialKillerCanSabotage;
}

/// <summary>
/// シリアルキラーが自殺タイマーが指定時間以下（例: 5秒以下）の状態で勝利するとトロフィーを獲得するクラス
/// </summary>
public class SerialKillerNearSuicideWinTrophy : SuperTrophyRole<SerialKillerNearSuicideWinTrophy>
{
    public override TrophiesEnum TrophyId => TrophiesEnum.SerialKillerNearSuicideWin;
    public override TrophyRank TrophyRank => TrophyRank.Bronze;
    public override RoleId[] TargetRoles => [RoleId.SerialKiller];

    private EventListener<EndGameEventData> _onEndGameEvent;
    private const float NearSuicideThreshold = 5.0f; // 5秒以下

    public override void OnRegister()
    {
        _onEndGameEvent = EndGameEvent.Instance.AddListener(HandleEndGameEvent);
    }

    private void HandleEndGameEvent(EndGameEventData data)
    {
        var localPlayer = ExPlayerControl.LocalPlayer;

        if (localPlayer.IsDead())
            return;

        bool isImpostorWin = data.winners.Any(w => w.PlayerId == localPlayer.PlayerId);

        if (localPlayer?.roleBase?.Role == RoleId.SerialKiller && isImpostorWin && !localPlayer.IsDead())
        {
            var suicideAbility = localPlayer.PlayerAbilities.FirstOrDefault(a => a is SuicideTimerAbility) as SuicideTimerAbility;

            if (suicideAbility != null && suicideAbility.CurrentTimer > 0 && suicideAbility.CurrentTimer <= NearSuicideThreshold)
            {
                Complete();
            }
        }
    }

    public override void OnDetached()
    {
        if (_onEndGameEvent != null)
        {
            EndGameEvent.Instance.RemoveListener(_onEndGameEvent);
            _onEndGameEvent = null;
        }
    }
}