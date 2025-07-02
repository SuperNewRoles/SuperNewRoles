using System;
using System.Collections.Generic;
using AmongUs.GameOptions;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability;
using UnityEngine;
using System.Linq;
using SuperNewRoles.Events;
using SuperNewRoles.Modules.Events.Bases;
using SuperNewRoles.SuperTrophies;
using SuperNewRoles.Patches; // CalledMeetingEventData のため

namespace SuperNewRoles.Roles.Impostor;

class Samurai : RoleBase<Samurai>
{
    public override RoleId Role { get; } = RoleId.Samurai;
    public override Color32 RoleColor { get; } = Palette.ImpostorRed;
    public override List<Func<AbilityBase>> Abilities { get; } = [
        () => new AreaKillButtonAbility(
            canKill: () => true,
            killRadius: () => SamuraiKillRadius,
            killCooldown: () => SamuraiKillCooldown,
            onlyCrewmates: () => SamuraiOnlyKillCrewmates,
            targetPlayersInVents: () => true,
            ignoreWalls: () => SamuraiIgnoreWalls,
            customSprite: AssetManager.GetAsset<Sprite>("SamuraiButton.png"),
            customButtonText: ModTranslation.GetString("SamuraiKillButtonText"),
            customDeathType: CustomDeathType.Samurai,
            callback: () =>
            {
                RpcSamuraiAnimation(ExPlayerControl.LocalPlayer);
            }
        ),
        () => new CustomSaboAbility(
            canSabotage: () => SamuraiCanSabotage
        ),
        () => new CustomVentAbility(
            canUseVent: () => SamuraiCanUseVent
        )
    ];
    [CustomRPC]
    public static void RpcSamuraiAnimation(ExPlayerControl player)
    {
        var obj = GameObject.Instantiate(AssetManager.GetAsset<GameObject>("SamuraiEffect"), player.transform);
        obj.AddComponent<DestroyOnAnimationEndObject>();
        obj.transform.localPosition = new(0, 0, -0.5f);
        obj.transform.localScale = Vector3.oneVector * 1f;
    }

    public override QuoteMod QuoteMod { get; } = QuoteMod.SuperNewRoles;
    public override RoleTypes IntroSoundType { get; } = RoleTypes.Shapeshifter;
    public override short IntroNum { get; } = 1;

    public override AssignedTeamType AssignedTeam { get; } = AssignedTeamType.Impostor;
    public override WinnerTeamType WinnerTeam { get; } = WinnerTeamType.Impostor;
    public override TeamTag TeamTag { get; } = TeamTag.Impostor;
    public override RoleTag[] RoleTags { get; } = [RoleTag.SpecialKiller];
    public override RoleOptionMenuType OptionTeam { get; } = RoleOptionMenuType.Impostor;

    [CustomOptionFloat("SamuraiKillCooldown", 2.5f, 60f, 2.5f, 30f)]
    public static float SamuraiKillCooldown;

    [CustomOptionFloat("SamuraiKillRadius", 1f, 5f, 0.5f, 2f)]
    public static float SamuraiKillRadius;

    [CustomOptionBool("SamuraiOnlyKillCrewmates", false)]
    public static bool SamuraiOnlyKillCrewmates;

    [CustomOptionBool("SamuraiIgnoreWalls", true)]
    public static bool SamuraiIgnoreWalls;

    [CustomOptionBool("SamuraiCanUseVent", true)]
    public static bool SamuraiCanUseVent;

    [CustomOptionBool("SamuraiCanSabotage", true)]
    public static bool SamuraiCanSabotage;
}

// --- Trophies ---

/// <summary>
/// 侍が一度のスキルで2人以上キルするとトロフィーを獲得するクラス
/// </summary>
public class SamuraiDoubleKillTrophy : SuperTrophyRole<SamuraiDoubleKillTrophy>
{
    public override TrophiesEnum TrophyId => TrophiesEnum.SamuraiDoubleKill;
    public override TrophyRank TrophyRank => TrophyRank.Silver;
    public override RoleId[] TargetRoles => [RoleId.Samurai];

    private EventListener<AreaKillEventData> _onAreaKillEvent;

    public override void OnRegister()
    {
        _onAreaKillEvent = AreaKillEvent.Instance.AddListener(HandleAreaKillEvent);
    }

    private void HandleAreaKillEvent(AreaKillEventData data)
    {
        if (data.killer != PlayerControl.LocalPlayer || data.killedPlayers.Count < 2)
            return;

        Complete();
    }

    public override void OnDetached()
    {
        if (_onAreaKillEvent != null)
        {
            AreaKillEvent.Instance.RemoveListener(_onAreaKillEvent);
            _onAreaKillEvent = null;
        }
    }
}


/// <summary>
/// 侍が壁越しにキルするとトロフィーを獲得するクラス
/// </summary>
public class SamuraiWallKillTrophy : SuperTrophyRole<SamuraiWallKillTrophy>
{
    public override TrophiesEnum TrophyId => TrophiesEnum.SamuraiWallKill;
    public override TrophyRank TrophyRank => TrophyRank.Silver;
    public override RoleId[] TargetRoles => [RoleId.Samurai];

    private EventListener<AreaKillEventData> _onAreaKillEvent;

    public override void OnRegister()
    {
        _onAreaKillEvent = AreaKillEvent.Instance.AddListener(HandleAreaKillEvent);
    }

    private void HandleAreaKillEvent(AreaKillEventData data)
    {
        if (data.killer != PlayerControl.LocalPlayer || !Samurai.SamuraiIgnoreWalls) // Static member access
            return;

        foreach (var target in data.killedPlayers)
        {
            RaycastHit2D hit = Physics2D.Linecast(data.killer.GetTruePosition(), target.GetTruePosition(), Constants.ShipOnlyMask);
            if (hit.collider != null && hit.transform != target.transform)
            {
                Complete();
                break;
            }
        }
    }

    public override void OnDetached()
    {
        if (_onAreaKillEvent != null)
        {
            AreaKillEvent.Instance.RemoveListener(_onAreaKillEvent);
            _onAreaKillEvent = null;
        }
    }
}


/// <summary>
/// 侍が会議招集前後（例えば5秒以内）にキルするとトロフィーを獲得するクラス
/// </summary>
public class SamuraiNearMeetingKillTrophy : SuperTrophyRole<SamuraiNearMeetingKillTrophy>
{
    public override TrophiesEnum TrophyId => TrophiesEnum.SamuraiNearMeetingKill;
    public override TrophyRank TrophyRank => TrophyRank.Bronze;
    public override RoleId[] TargetRoles => [RoleId.Samurai];

    private EventListener<AreaKillEventData> _onAreaKillEvent;
    private EventListener<CalledMeetingEventData> _onCalledMeetingEvent;
    private DateTime _lastKillTime = DateTime.MinValue;
    private const float NearMeetingTimeThreshold = 5f; // 5秒以内

    public override void OnRegister()
    {
        _onAreaKillEvent = AreaKillEvent.Instance.AddListener(HandleAreaKillEvent);
        _onCalledMeetingEvent = CalledMeetingEvent.Instance.AddListener(HandleCalledMeetingEvent);
    }

    private void HandleAreaKillEvent(AreaKillEventData data)
    {
        if (data.killer == PlayerControl.LocalPlayer)
        {
            _lastKillTime = DateTime.UtcNow;
        }
    }

    private void HandleCalledMeetingEvent(CalledMeetingEventData data)
    {
        if (_lastKillTime != DateTime.MinValue && (DateTime.UtcNow - _lastKillTime).TotalSeconds <= NearMeetingTimeThreshold)
        {
            Complete();
            _lastKillTime = DateTime.MinValue; // Reset after completion
        }
    }

    public override void OnDetached()
    {
        if (_onAreaKillEvent != null)
        {
            AreaKillEvent.Instance.RemoveListener(_onAreaKillEvent);
            _onAreaKillEvent = null;
        }
        if (_onCalledMeetingEvent != null)
        {
            CalledMeetingEvent.Instance.RemoveListener(_onCalledMeetingEvent);
            _onCalledMeetingEvent = null;
        }
    }
}