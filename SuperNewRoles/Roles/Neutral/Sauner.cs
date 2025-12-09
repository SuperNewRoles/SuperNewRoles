using System;
using System.Collections.Generic;
using AmongUs.GameOptions;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Events;
using SuperNewRoles.Modules;
using SuperNewRoles.Modules.Events.Bases;
using SuperNewRoles.Patches;
using SuperNewRoles.Roles.Ability;
using UnityEngine;

namespace SuperNewRoles.Roles.Neutral;

class Sauner : RoleBase<Sauner>
{
    public enum SaunerState
    {
        Darkroom,
        Shower,
        ObservationDeck
    }

    public override RoleId Role { get; } = RoleId.Sauner;
    public override Color32 RoleColor { get; } = new(219, 152, 101, byte.MaxValue);
    public override List<Func<AbilityBase>> Abilities { get; } =
    [
        () => new SaunerAbility(new SaunerConfig(SaunerDarkroomTime, SaunerShowerTime, SaunerDeckTime)),
    ];

    public override QuoteMod QuoteMod { get; } = QuoteMod.SuperNewRoles;
    public override RoleTypes IntroSoundType { get; } = RoleTypes.Crewmate;
    public override short IntroNum { get; } = 1;

    public override AssignedTeamType AssignedTeam { get; } = AssignedTeamType.Neutral;
    public override WinnerTeamType WinnerTeam { get; } = WinnerTeamType.Neutral;
    public override TeamTag TeamTag { get; } = TeamTag.Neutral;
    public override RoleTag[] RoleTags { get; } = [RoleTag.SpecialWinner];
    public override RoleOptionMenuType OptionTeam { get; } = RoleOptionMenuType.Neutral;
    public override RoleId[] RelatedRoleIds { get; } = [];
    public override MapNames[] AvailableMaps { get; } = [MapNames.Airship];

    [CustomOptionFloat("SaunerDarkroomTime", 20f, 600f, 20f, 180f)]
    public static float SaunerDarkroomTime;

    [CustomOptionFloat("SaunerShowerTime", 20f, 600f, 20f, 20f)]
    public static float SaunerShowerTime;

    [CustomOptionFloat("SaunerDeckTime", 20f, 600f, 20f, 60f)]
    public static float SaunerDeckTime;
}

public record SaunerConfig(float DarkroomTime, float ShowerTime, float DeckTime);

public class SaunerAbility : AbilityBase
{
    private readonly SaunerConfig _config;
    private Sauner.SaunerState _state;
    private float _timer;
    private bool _flashActive;
    private bool _winHandled;
    private AudioSource _audio;
    private AudioSource _fadingOutAudio;
    private Coroutine _audioFadeCoroutine;
    private FlashHandler.FlashHandle _flashHandle;
    private readonly HashSet<Sauner.SaunerState> _missingAudioStates = new();
    private EventListener _fixedUpdateListener;
    private ImportantTextTask _task;

    public SaunerAbility(SaunerConfig config)
    {
        _config = config;
        _state = Sauner.SaunerState.Darkroom;
        _timer = config.DarkroomTime;
    }

    public override void AttachToLocalPlayer()
    {
        _fixedUpdateListener = FixedUpdateEvent.Instance.AddListener(OnFixedUpdate);
    }

    public override void DetachToLocalPlayer()
    {
        // 全クライアント共通のリスナー/演出を片付ける
        _fixedUpdateListener?.RemoveListener();
        StopEffects(false, true);
        DestroyTask();
    }

    private void OnFixedUpdate()
    {
        bool inMeeting = MeetingHud.Instance != null || ExileController.Instance != null;
        bool alive = !Player.Data.IsDead;

        if (!alive)
        {
            // 死亡中は演出を止めてテキストだけ更新
            StopEffects(true, true);
            UpdateTaskText(true);
            return;
        }

        bool inside = !inMeeting && CheckRoom(_state, Player.transform.position);

        if (Player.AmOwner)
        {
            // ローカル側で視覚・音を制御
            UpdateVisuals(inside, inMeeting);
            UpdateTaskText(inMeeting);
        }

        // タイマー進行と勝利判定はホストのみ
        TickTimer(inside, inMeeting);
    }

    private void TickTimer(bool insideRoom, bool inMeeting)
    {
        if (_winHandled) return;
        if (!insideRoom || inMeeting) return;

        _timer -= Time.fixedDeltaTime;
        if (_timer > 0f) return;

        switch (_state)
        {
            case Sauner.SaunerState.Darkroom:
                // 次のステージへ遷移
                FadeOutAudio();
                _state = Sauner.SaunerState.Shower;
                _timer = _config.ShowerTime;
                break;
            case Sauner.SaunerState.Shower:
                // 最終ステージへ遷移
                FadeOutAudio();
                _state = Sauner.SaunerState.ObservationDeck;
                _timer = _config.DeckTime;
                break;
            case Sauner.SaunerState.ObservationDeck:
                // 最終段階達成で単独勝利
                _winHandled = true;
                EndGamer.RpcEndGameWithWinner(CustomGameOverReason.SaunerWin, WinType.SingleNeutral, [Player], Sauner.Instance.RoleColor, "Sauner", "SaunerWinText");
                break;
        }
    }

    private void UpdateVisuals(bool insideRoom, bool inMeeting)
    {
        if (inMeeting || !insideRoom)
        {
            StopEffects(true, true);
            return;
        }

        var color = GetFlashColor(_state);
        StartFlashLoop(color);
        TryPlayAudioForState();
    }

    private void StartFlashLoop(Color color)
    {
        if (_flashActive) return;
        _flashActive = true;
        LoopFlash(color);
    }

    private void LoopFlash(Color color)
    {
        _flashHandle = FlashHandler.ShowFlashHandle(color, 6f, () =>
        {
            _flashHandle = null;
            if (_flashActive) LoopFlash(color);
        });
    }

    private void StopEffects(bool fadeAudio = false, bool quickFlashStop = false)
    {
        _flashActive = false;
        StopFlash(quickFlashStop);
        if (fadeAudio) FadeOutAudio();
        else StopAudioImmediate();
    }

    private void StopFlash(bool quick)
    {
        var handle = _flashHandle;
        _flashHandle = null;
        if (handle == null) return;

        handle.Stop(quick ? 0.5f : 0f);
    }

    private void TryPlayAudioForState()
    {
        if (_audio != null) return;
        if (_missingAudioStates.Contains(_state)) return;

        // 音源が無い場合でも落ちないよう、取得失敗はスキップ
        string clipName = _state switch
        {
            Sauner.SaunerState.Darkroom => "Sauner_SaunaBGM.mp3",
            Sauner.SaunerState.Shower => "Sauner_ShowerBGM.wav",
            Sauner.SaunerState.ObservationDeck => "Sauner_BardBGM.wav",
            _ => null
        };

        if (string.IsNullOrEmpty(clipName)) return;

        try
        {
            var clip = AssetManager.GetAsset<AudioClip>(clipName);
            if (clip != null)
            {
                _audio = SoundManager.Instance.PlaySound(clip, true, audioMixer: SoundManager.Instance.sfxMixer);
            }
            else
            {
                _missingAudioStates.Add(_state);
            }
        }
        catch (Exception ex)
        {
            Logger.Warning($"Sauner: failed to start audio {clipName}: {ex.Message}");
            _missingAudioStates.Add(_state);
        }
    }

    private void StopAudioImmediate()
    {
        var hud = FastDestroyableSingleton<HudManager>.Instance;
        if (_audioFadeCoroutine != null && hud != null)
        {
            hud.StopCoroutine(_audioFadeCoroutine);
            _audioFadeCoroutine = null;
        }

        if (_audio != null)
        {
            _audio.Stop();
            _audio = null;
        }

        if (_fadingOutAudio != null)
        {
            _fadingOutAudio.Stop();
            _fadingOutAudio = null;
        }
    }

    private void FadeOutAudio(float duration = 0.6f)
    {
        if (_audio == null) return;

        var hud = FastDestroyableSingleton<HudManager>.Instance;
        if (hud == null)
        {
            StopAudioImmediate();
            return;
        }

        if (_audioFadeCoroutine != null)
        {
            hud.StopCoroutine(_audioFadeCoroutine);
            _audioFadeCoroutine = null;
        }

        _fadingOutAudio = _audio;
        _audio = null;
        float startVolume = _fadingOutAudio.volume;

        _audioFadeCoroutine = hud.StartCoroutine(Effects.Lerp(duration, new Action<float>((p) =>
        {
            if (_fadingOutAudio == null)
            {
                _audioFadeCoroutine = null;
                return;
            }

            _fadingOutAudio.volume = Mathf.Lerp(startVolume, 0f, p);
            if (p >= 1f)
            {
                _fadingOutAudio.Stop();
                _fadingOutAudio.volume = startVolume;
                _fadingOutAudio = null;
                _audioFadeCoroutine = null;
            }
        })));
    }

    private void EnsureTask()
    {
        if (!Player.AmOwner) return;
        if (_task != null) return;

        var pc = Player?.Player;
        if (pc == null) return;

        var taskObj = new GameObject("SaunerTask").AddComponent<ImportantTextTask>();
        taskObj.transform.SetParent(pc.transform, false);
        pc.myTasks.Insert(0, taskObj);
        taskObj.HasLocation = true;
        _task = taskObj;
    }

    private void DestroyTask()
    {
        if (_task == null) return;
        var pc = Player?.Player;
        if (pc != null) pc.myTasks.Remove(_task);
        UnityEngine.Object.Destroy(_task.gameObject);
        _task = null;
    }

    private void UpdateTaskText(bool inMeeting)
    {
        if (!Player.AmOwner) return;
        EnsureTask();
        if (_task == null) return;
        var pc = Player?.Player;
        if (pc == null) return;

        // ステージ名と残り時間を組み立ててタスク欄に表示
        string stageName = ModTranslation.GetString($"Sauner{_state}");
        string action = _state switch
        {
            Sauner.SaunerState.Darkroom => ModTranslation.GetString("SaunerTextInSauna"),
            Sauner.SaunerState.Shower => ModTranslation.GetString("SaunerTextShower"),
            Sauner.SaunerState.ObservationDeck => ModTranslation.GetString("SaunerTextAirBath"),
            _ => string.Empty
        };
        float stageLimit = _state switch
        {
            Sauner.SaunerState.Darkroom => _config.DarkroomTime,
            Sauner.SaunerState.Shower => _config.ShowerTime,
            Sauner.SaunerState.ObservationDeck => _config.DeckTime,
            _ => 0f
        };
        string remaining = _timer < stageLimit
            ? ModTranslation.GetString("SaunerTextRemaing", Math.Max(0, (int)(_timer + 1)))
            : string.Empty;
        string meetingText = inMeeting ? $" ({ModTranslation.GetString("Meeting")})" : string.Empty;
        Color color = GetFlashColor(_state);
        color.a = 1f;
        // 0%でサウナーということを指定
        // FixMapIconsの方でアイコンの場所を変更してます
        _task.Text = "<size=0%>Sauner</size>" + ModHelpers.Cs(color, $"{stageName}: {action}{remaining}{meetingText}");

        // 共通タスク以外は非表示にしてタスク欄を圧迫しない
        int index = 0;
        foreach (PlayerTask t in pc.myTasks.ToArray())
        {
            NormalPlayerTask npt = t.TryCast<NormalPlayerTask>();
            if (npt != null)
            {
                if (npt.Length != NormalPlayerTask.TaskLength.Common)
                {
                    pc.myTasks.RemoveAt(index);
                    // RemoveAtで1個減っているので。(後でどうせ++される)
                    index--;
                }
                else
                    npt.HasLocation = false;
            }
            index++;
        }
    }

    private static Color GetFlashColor(Sauner.SaunerState state)
    {
        return state switch
        {
            Sauner.SaunerState.Darkroom => new Color32(255, 123, 99, 103),
            Sauner.SaunerState.Shower => new Color32(65, 161, 255, 103),
            Sauner.SaunerState.ObservationDeck => new Color32(38, 200, 94, 103),
            _ => Color.white
        };
    }

    public List<Vector2> GetSaunaPos()
    {
        switch (_state)
        {
            case Sauner.SaunerState.Darkroom:
                return new() { new(12.5f, 2.2f) };
            case Sauner.SaunerState.Shower:
                return new() { new(22.5f, 2.55f) };
            case Sauner.SaunerState.ObservationDeck:
                return new() { new(-13.7f, -15), new(8f, -14.6f) };
            default:
                Logger.Info("GetSaunerPosで予期しない位置が入力されました：" + _state.ToString());
                return new();
        }
    }

    private static bool CheckRoom(Sauner.SaunerState room, Vector2 nowpos)
    {
        // ステージごとに矩形内かどうかを判定
        return room switch
        {
            Sauner.SaunerState.Darkroom => CheckPos(new(11.125f, 3.2f), new(13.8f, 0.9f), nowpos),
            Sauner.SaunerState.Shower => CheckPos(new(20.3f, 3.5f), new(24.9f, 1.4f), nowpos),
            Sauner.SaunerState.ObservationDeck => CheckPos(new(-14.7f, -13.95f), new(11.1f, -17f), nowpos),
            _ => false
        };
    }

    private static bool CheckPos(Vector2 up, Vector2 down, Vector2 pos)
    {
        return up.x <= pos.x && pos.x <= down.x &&
               up.y >= pos.y && pos.y >= down.y;
    }
}

