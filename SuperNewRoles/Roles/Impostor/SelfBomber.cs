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
using SuperNewRoles.Patches; // CheckGameEndEventData のため

namespace SuperNewRoles.Roles.Impostor;

class SelfBomber : RoleBase<SelfBomber>
{
    public override RoleId Role { get; } = RoleId.SelfBomber;
    public override Color32 RoleColor { get; } = Palette.ImpostorRed;
    public override List<Func<AbilityBase>> Abilities { get; } = [
        () => new AreaKillButtonAbility(
            canKill: () => true,
            killRadius: () => SelfBomberExplosionRadius,
            killCooldown: () => SelfBomberCooldown,
            onlyCrewmates: () => SelfBomberOnlyKillCrewmates,
            targetPlayersInVents: () => true,
            isTargetable: null,
            killedCallback: (killedPlayers) => {
            },
            customSprite: AssetManager.GetAsset<Sprite>("SelfBomberBomButton.png"),
            customButtonText: ModTranslation.GetString("SelfBomberKillButtonText"),
            customDeathType: CustomDeathType.BombBySelfBomb,
            callback: () =>
            {
                RpcSelfBomberCallback(ExPlayerControl.LocalPlayer);
                if (ExPlayerControl.ExPlayerControls.Count(x => x.IsAlive()) == 0)
                    EndGamer.RpcEndGameImpostorWin();
            },
            beforeKillCallback: (killedPlayers) =>
            {
                if (killedPlayers.Contains(ExPlayerControl.LocalPlayer))
                    CustomKillAnimationManager.SetCurrentCustomKillAnimation(new SelfBomberKillAnimation());
            }
        )
    ];
    [CustomRPC]
    public static void RpcSelfBomberCallback(ExPlayerControl player)
    {
        if (player.AmOwner)
            CustomKillAnimationManager.SetCurrentCustomKillAnimation(new SelfBomberKillAnimation());
        var obj = GameObject.Instantiate(AssetManager.GetAsset<GameObject>("SelfBomberEffect"), player.transform);
        obj.AddComponent<DestroyOnAnimationEndObject>();
        obj.transform.localPosition = new(0, 0, -0.5f);
        float size = 1.68f;
        obj.transform.localScale = player.MyPhysics.FlipX ? new Vector3(-size, size, size) : Vector3.one * size;
        player.CustomDeath(CustomDeathType.SelfBomb, source: player);
        if (player.AmOwner && Minigame.Instance != null)
        {
            new LateTask(() => Minigame.Instance.Close(), 0.1f);
        }
    }

    public override QuoteMod QuoteMod { get; } = QuoteMod.SuperNewRoles;
    public override RoleTypes IntroSoundType { get; } = RoleTypes.Engineer;
    public override short IntroNum { get; } = 1;

    public override AssignedTeamType AssignedTeam { get; } = AssignedTeamType.Impostor;
    public override WinnerTeamType WinnerTeam { get; } = WinnerTeamType.Impostor;
    public override TeamTag TeamTag { get; } = TeamTag.Impostor;
    public override RoleTag[] RoleTags { get; } = [RoleTag.SpecialKiller];
    public override RoleOptionMenuType OptionTeam { get; } = RoleOptionMenuType.Impostor;

    [CustomOptionFloat("SelfBomberCooldown", 2.5f, 60f, 2.5f, 30f)]
    public static float SelfBomberCooldown;

    [CustomOptionFloat("SelfBomberExplosionRadius", 1f, 5f, 0.5f, 3f)]
    public static float SelfBomberExplosionRadius;

    [CustomOptionBool("SelfBomberOnlyKillCrewmates", false)]
    public static bool SelfBomberOnlyKillCrewmates;
}

public class SelfBomberKillAnimation : ICustomKillAnimation
{
    private Animator _selfBomberEffect;
    private bool _animatorFinished = false;
    private float _timer = 0f;
    public void Initialize(OverlayKillAnimation __instance, KillOverlayInitData initData)
    {
        _selfBomberEffect = GameObject.Instantiate(AssetManager.GetAsset<GameObject>("SelfBomberEffect"), __instance.transform).GetComponent<Animator>();
        AspectPosition aspectPosition = _selfBomberEffect.gameObject.AddComponent<AspectPosition>();
        aspectPosition.Alignment = AspectPosition.EdgeAlignments.Center;
        aspectPosition.DistanceFromEdge = Vector3.zero;
        aspectPosition.OnEnable();
        // _selfBomberEffect.transform.localPosition = new(-1.5f, 0.3f, -0.5f);
        _selfBomberEffect.gameObject.layer = 5;
        _selfBomberEffect.transform.localScale = Vector3.one * 1.7f;
    }

    public bool FixedUpdate()
    {
        if (!_animatorFinished)
        {
            // アニメーターが終了したかチェック
            if (_selfBomberEffect != null && _selfBomberEffect.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1)
            {
                _animatorFinished = true;
                _timer = 0f; // タイマー開始
                GameObject.Destroy(_selfBomberEffect.gameObject);
            }
            else
            {
                // アニメーター再生中
                return true;
            }
        }

        // アニメーター終了後、1秒の遅延タイマーを進める
        _timer += Time.fixedDeltaTime;

        // 1秒経過するまでtrueを返す
        return _timer < 1f;
    }
}

// --- Trophies ---
/// <summary>
/// セルフボンバーが一度の爆発で3人以上キルするとトロフィーを獲得するクラス
/// </summary>
public class SelfBomberTripleKillTrophy : SuperTrophyRole<SelfBomberTripleKillTrophy>
{
    public override TrophiesEnum TrophyId => TrophiesEnum.SelfBomberTripleKill;
    public override TrophyRank TrophyRank => TrophyRank.Gold;
    public override RoleId[] TargetRoles => [RoleId.SelfBomber];

    private EventListener<AreaKillEventData> _onAreaKillEvent;

    public override void OnRegister()
    {
        _onAreaKillEvent = AreaKillEvent.Instance.AddListener(HandleAreaKillEvent);
    }

    private void HandleAreaKillEvent(AreaKillEventData data)
    {
        if (data.killer == PlayerControl.LocalPlayer &&
            (data.deathType == CustomDeathType.SelfBomb || data.deathType == CustomDeathType.BombBySelfBomb) &&
            data.killedPlayers.Count >= 3)
        {
            Complete();
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
/// セルフボンバーが最後のインポスターとして自爆し、勝利するとトロフィーを獲得するクラス
/// </summary>
public class SelfBomberSacrificeWinTrophy : SuperTrophyRole<SelfBomberSacrificeWinTrophy>
{
    public override TrophiesEnum TrophyId => TrophiesEnum.SelfBomberSacrificeWin;
    public override TrophyRank TrophyRank => TrophyRank.Gold;
    public override RoleId[] TargetRoles => [RoleId.SelfBomber];

    private EventListener<AreaKillEventData> _onAreaKillEvent;
    private EventListener<EndGameEventData> _onEndGameEvent;
    private bool _selfBombedThisRound = false;

    public override void OnRegister()
    {
        _onAreaKillEvent = AreaKillEvent.Instance.AddListener(HandleAreaKillEvent);
        _onEndGameEvent = EndGameEvent.Instance.AddListener(HandleEndGameEvent);
        _selfBombedThisRound = false;
    }

    private void HandleAreaKillEvent(AreaKillEventData data)
    {
        if (data.killer == PlayerControl.LocalPlayer &&
            (data.deathType == CustomDeathType.SelfBomb || data.deathType == CustomDeathType.BombBySelfBomb) &&
            !ExPlayerControl.ExPlayerControls.Any(x => x.IsAlive() && x.IsImpostor() && !x.AmOwner))
        {
            _selfBombedThisRound = true;
        }
    }

    private void HandleEndGameEvent(EndGameEventData data)
    {
        if (!_selfBombedThisRound) return;

        var localPlayer = ExPlayerControl.LocalPlayer;
        bool isImpostorWin = data.winners.Any(w => w.PlayerId == localPlayer.PlayerId);

        if (localPlayer?.PlayerId == PlayerControl.LocalPlayer.PlayerId &&
            _selfBombedThisRound &&
            isImpostorWin)
        {
            Complete();
        }
        _selfBombedThisRound = false;
    }

    public override void OnDetached()
    {
        if (_onAreaKillEvent != null)
        {
            AreaKillEvent.Instance.RemoveListener(_onAreaKillEvent);
            _onAreaKillEvent = null;
        }
        if (_onEndGameEvent != null)
        {
            EndGameEvent.Instance.RemoveListener(_onEndGameEvent);
            _onEndGameEvent = null;
        }
    }
}

/// <summary>
/// セルフボンバーがベント付近で爆発キルを行うとトロフィーを獲得するクラス
/// </summary>
public class SelfBomberVentKillTrophy : SuperTrophyRole<SelfBomberVentKillTrophy>
{
    public override TrophiesEnum TrophyId => TrophiesEnum.SelfBomberVentKill;
    public override TrophyRank TrophyRank => TrophyRank.Bronze;
    public override RoleId[] TargetRoles => [RoleId.SelfBomber];

    private EventListener<AreaKillEventData> _onAreaKillEvent;
    private const float VentKillRadius = 2.5f;

    public override void OnRegister()
    {
        _onAreaKillEvent = AreaKillEvent.Instance.AddListener(HandleAreaKillEvent);
    }

    private void HandleAreaKillEvent(AreaKillEventData data)
    {
        if (data.killer != PlayerControl.LocalPlayer ||
            !(data.deathType == CustomDeathType.SelfBomb || data.deathType == CustomDeathType.BombBySelfBomb))
            return;

        var vents = ShipStatus.Instance.AllVents;
        bool nearVentKill = false;

        foreach (var killedPlayer in data.killedPlayers)
        {
            // killedPlayerがnullでないか、死亡していないか再確認 (念のため)
            if (killedPlayer?.Player == null || killedPlayer.IsDead()) continue;

            foreach (var vent in vents)
            {
                if (Vector2.Distance(killedPlayer.GetTruePosition(), vent.transform.position) <= VentKillRadius)
                {
                    nearVentKill = true;
                    break;
                }
            }
            if (nearVentKill) break;
        }

        if (nearVentKill)
        {
            Complete();
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