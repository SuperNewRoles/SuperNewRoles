using System;
using System.Collections.Generic;
using System.Linq;
using SuperNewRoles.CustomObject;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Events;
using SuperNewRoles.Events.PCEvents;
using SuperNewRoles.Modules;
using SuperNewRoles.Modules.Events.Bases;
using SuperNewRoles.Roles.Ability;
using SuperNewRoles.Roles.Ability.CustomButton;
using TMPro;
using UnityEngine;

namespace SuperNewRoles.Roles.Crewmate;

/// <summary>
/// サイコメトリスト役職
/// 死体を調べることで死亡時刻、死因、足跡を確認できるクルーメイト役職
/// </summary>
class Psychometrist : RoleBase<Psychometrist>
{
    /// <summary>この役職のID</summary>
    public override RoleId Role { get; } = RoleId.Psychometrist;
    /// <summary>この役職の色 (紫色)</summary>
    public override Color32 RoleColor { get; } = new(238, 130, 238, 255);

    /// <summary>この役職が持つ能力のリスト</summary>
    public override List<Func<AbilityBase>> Abilities { get; } = [
        () => new PsychometristReadAbility(
            new PsychometristReadData(
                Cooldown: PsychometristCoolTime,
                ReadTime: PsychometristReadTime,
                IsCheckDeathTime: PsychometristIsCheckDeathTime,
                IsCheckDeathReason: PsychometristIsCheckDeathReason,
                IsCheckFootprints: PsychometristIsCheckFootprints,
                CanCheckFootprintsTime: PsychometristCanCheckFootprintsTime,
                IsReportCheckedDeadBody: PsychometristIsReportCheckedDeadBody,
                TimeDeviation: PsychometristDeathTimeDeviation
            )
        )
    ];

    public override QuoteMod QuoteMod { get; } = QuoteMod.SuperNewRoles;
    public override short IntroNum { get; } = 1;

    public override AssignedTeamType AssignedTeam { get; } = AssignedTeamType.Crewmate;
    public override WinnerTeamType WinnerTeam { get; } = WinnerTeamType.Crewmate;
    public override TeamTag TeamTag { get; } = TeamTag.Crewmate;
    public override RoleTag[] RoleTags { get; } = [RoleTag.Information];
    public override RoleOptionMenuType OptionTeam { get; } = RoleOptionMenuType.Crewmate;

    /// <summary>サイコメトリストの能力クールタイム</summary>
    [CustomOptionFloat(nameof(PsychometristCoolTime), 2.5f, 60f, 2.5f, 20f, translationName: "CoolTime", suffix: "Seconds")]
    public static float PsychometristCoolTime;

    /// <summary>死体を調べるのにかかる時間</summary>
    [CustomOptionFloat(nameof(PsychometristReadTime), 0f, 15f, 0.5f, 5f, translationName: "PsychometristReadTime", suffix: "Seconds")]
    public static float PsychometristReadTime;

    /// <summary>死亡時刻を確認するかどうか</summary>
    [CustomOptionBool(nameof(PsychometristIsCheckDeathTime), true, translationName: "PsychometristIsCheckDeathTime")]
    public static bool PsychometristIsCheckDeathTime;

    /// <summary>死亡時刻の誤差範囲 (±秒)</summary>
    [CustomOptionInt(nameof(PsychometristDeathTimeDeviation), 0, 30, 1, 3, translationName: "PsychometristDeathTimeDeviation", suffix: "Seconds")]
    public static int PsychometristDeathTimeDeviation;

    /// <summary>死因を確認するかどうか</summary>
    [CustomOptionBool(nameof(PsychometristIsCheckDeathReason), true, translationName: "PsychometristIsCheckDeathReason")]
    public static bool PsychometristIsCheckDeathReason;

    /// <summary>足跡を確認するかどうか</summary>
    [CustomOptionBool(nameof(PsychometristIsCheckFootprints), true, translationName: "PsychometristIsCheckFootprints")]
    public static bool PsychometristIsCheckFootprints;

    /// <summary>足跡を記録する時間</summary>
    [CustomOptionFloat(nameof(PsychometristCanCheckFootprintsTime), 0.5f, 60f, 0.5f, 7.5f, translationName: "PsychometristCanCheckFootprintsTime", suffix: "Seconds")]
    public static float PsychometristCanCheckFootprintsTime;

    /// <summary>調べた死体を報告可能にするかどうか</summary>
    [CustomOptionBool(nameof(PsychometristIsReportCheckedDeadBody), false, translationName: "PsychometristIsReportCheckedDeadBody")]
    public static bool PsychometristIsReportCheckedDeadBody;
}

/// <summary>
/// サイコメトリストの能力設定データを格納するレコード
/// </summary>
public record PsychometristReadData(
    /// <summary>能力のクールタイム</summary>
    float Cooldown,
    /// <summary>死体を調べるのにかかる時間</summary>
    float ReadTime,
    /// <summary>死亡時刻を確認するかどうか</summary>
    bool IsCheckDeathTime,
    /// <summary>死因を確認するかどうか</summary>
    bool IsCheckDeathReason,
    /// <summary>足跡を確認するかどうか</summary>
    bool IsCheckFootprints,
    /// <summary>足跡を記録する時間</summary>
    float CanCheckFootprintsTime,
    /// <summary>調べた死体を報告可能にするかどうか</summary>
    bool IsReportCheckedDeadBody,
    /// <summary>死亡時刻の誤差範囲 (±秒)</summary>
    int TimeDeviation
);

/// <summary>
/// サイコメトリストの死体読み取り能力
/// 死体に近づいてボタンを押すことで情報を取得する
/// </summary>
public sealed class PsychometristReadAbility : CustomButtonBase, IButtonEffect
{
    /// <summary>死体との最大距離</summary>
    private const float TargetDistance = 0.5f;

    /// <summary>能力設定データ</summary>
    public PsychometristReadData Data { get; }

    /// <summary>デフォルトのタイマー時間</summary>
    public override float DefaultTimer => Data.Cooldown;
    /// <summary>ボタンのテキスト</summary>
    public override string buttonText => ModTranslation.GetString("PsychometristButtonName");
    /// <summary>ボタンのスプライト</summary>
    public override Sprite Sprite => AssetManager.GetAsset<Sprite>("PsychometristButton.png") ?? HudManager.Instance.ReportButton.graphic.sprite;
    /// <summary>キー入力タイプ</summary>
    protected override KeyType keytype => KeyType.Ability1;

    /// <summary>効果がアクティブかどうか</summary>
    public bool isEffectActive { get; set; }
    /// <summary>効果終了時のコールバック</summary>
    public Action OnEffectEnds => Analyze;
    /// <summary>効果の持続時間</summary>
    public float EffectDuration => Data.ReadTime;
    /// <summary>効果の残り時間</summary>
    public float EffectTimer { get; set; }

    /// <summary>現在対象となっている死体</summary>
    private DeadBody _candidateTarget;
    /// <summary>現在読み取り中の死体</summary>
    private DeadBody _readingTarget;

    /// <summary>死亡情報のテキスト表示管理</summary>
    private readonly Dictionary<byte, (DeadBody body, TextMeshPro text, int deviation)> _deathInfoTexts = new();
    /// <summary>表示中の足跡管理</summary>
    private readonly Dictionary<(byte killerId, byte victimId), List<Footprint>> _shownFootprints = new();

    /// <summary>FixedUpdateイベントリスナー</summary>
    private EventListener _fixedUpdateListener;
    /// <summary>会議開始イベントリスナー</summary>
    private EventListener<MeetingStartEventData> _meetingStartListener;

    public PsychometristReadAbility(PsychometristReadData data)
    {
        Data = data;
    }

    /// <summary>
    /// ローカルプレイヤーに能力をアタッチする
    /// イベントリスナーを登録する
    /// </summary>
    public override void AttachToLocalPlayer()
    {
        base.AttachToLocalPlayer();
        _fixedUpdateListener = FixedUpdateEvent.Instance.AddListener(OnFixedUpdate);
        _meetingStartListener = MeetingStartEvent.Instance.AddListener(_ => ClearLocalVisuals());
    }

    /// <summary>
    /// ローカルプレイヤーから能力をデタッチする
    /// イベントリスナーを解除し、視覚効果をクリアする
    /// </summary>
    public override void DetachToLocalPlayer()
    {
        base.DetachToLocalPlayer();
        _fixedUpdateListener?.RemoveListener();
        _meetingStartListener?.RemoveListener();
        ClearLocalVisuals();
    }

    public override bool CheckIsAvailable()
    {
        return Player.Player.CanMove && _candidateTarget != null;
    }

    /// <summary>
    /// ボタンがクリックされた時の処理
    /// 現在対象となっている死体を読み取り対象に設定する
    /// </summary>
    public override void OnClick()
    {
        _readingTarget = _candidateTarget;
    }

    public override void OnUpdate()
    {
        _candidateTarget = FindCandidateDeadBody();

        if (isEffectActive && !IsReadingTargetValid())
        {
            CancelReading();
        }

        base.OnUpdate();
    }

    private void OnFixedUpdate()
    {
        if (_deathInfoTexts.Count == 0) return;
        if (MeetingHud.Instance != null)
        {
            ClearLocalVisuals();
            return;
        }

        foreach (var key in _deathInfoTexts.Keys.ToArray())
        {
            var (body, text, deviation) = _deathInfoTexts[key];
            if (body == null || text == null)
            {
                if (text != null) UnityEngine.Object.Destroy(text.gameObject);
                _deathInfoTexts.Remove(key);
                continue;
            }
            text.text = BuildDeathInfoText(body.ParentId, deviation);
        }
    }

    private DeadBody FindCandidateDeadBody()
    {
        if (Player.Player == null) return null;
        if (!Player.Player.CanMove) return null;
        if (HudManager.Instance?.ReportButton == null) return null;

        Vector2 myPos = Player.Player.GetTruePosition();
        DeadBody result = null;
        float bestDist = float.MaxValue;

        foreach (Collider2D col in Physics2D.OverlapCircleAll(myPos, Player.Player.MaxReportDistance, Constants.PlayersOnlyMask))
        {
            if (!col || !col.CompareTag("DeadBody")) continue;
            DeadBody body = col.GetComponent<DeadBody>();
            if (body == null || body.Reported) continue;

            Vector2 bodyPos = body.TruePosition;
            float dist = Vector2.Distance(bodyPos - new Vector2(0.15f, 0.2f), myPos);
            if (dist > TargetDistance || dist >= bestDist) continue;
            if (PhysicsHelpers.AnythingBetween(myPos, bodyPos, Constants.ShipAndObjectsMask, false)) continue;

            bestDist = dist;
            result = body;
        }

        return result;
    }

    private bool IsReadingTargetValid()
    {
        if (_readingTarget == null) return false;
        if (_readingTarget.Reported) return false;
        if (!Player.Player.CanMove) return false;

        Vector2 myPos = Player.Player.GetTruePosition();
        Vector2 bodyPos = _readingTarget.TruePosition;
        float dist = Vector2.Distance(bodyPos - new Vector2(0.15f, 0.2f), myPos);
        if (dist > TargetDistance) return false;
        if (HudManager.Instance?.ReportButton?.graphic?.color != Palette.EnabledColor) return false;
        return true;
    }

    private void CancelReading()
    {
        isEffectActive = false;
        EffectTimer = EffectDuration;
        Timer = 0f;
        _readingTarget = null;
        if (actionButton != null)
            actionButton.cooldownTimerText.color = Palette.EnabledColor;
    }

    /// <summary>
    /// 死体の分析を行う
    /// 死亡時刻、死因、足跡の情報を表示し、必要に応じて報告をブロックする
    /// </summary>
    private void Analyze()
    {
        DeadBody target = _readingTarget;
        _readingTarget = null;
        if (target == null) return;
        if (MeetingHud.Instance != null) return;
        if (target.Reported) return;

        // 死亡時刻の誤差を計算
        int deviation = 0;
        int devRange = Data.TimeDeviation;
        if (devRange > 0)
        {
            deviation = UnityEngine.Random.Range(-devRange, devRange);
        }

        // 死亡情報テキストの作成または更新
        if (!_deathInfoTexts.TryGetValue(target.ParentId, out var existing) || existing.text == null)
        {
            var template = HudManager.Instance?.KillButton?.cooldownTimerText;
            if (template == null) return;

            // テキストオブジェクトを作成
            TextMeshPro text = UnityEngine.Object.Instantiate(template, target.transform);
            text.transform.localPosition = new Vector3(-0.2f, 0.5f, 0f);
            text.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
            text.color = Color.white;
            text.enableWordWrapping = false;

            _deathInfoTexts[target.ParentId] = (target, text, deviation);
        }
        else
        {
            _deathInfoTexts[target.ParentId] = (target, existing.text, deviation);
        }

        // 報告ブロックの設定
        if (!Data.IsReportCheckedDeadBody)
        {
            PsychometristReportBlock.RpcSetReportBlocked(target.ParentId, true);
        }

        // 足跡の表示
        if (Data.IsCheckFootprints)
        {
            ShowFootprints(target.ParentId);
        }

        // 即時更新
        if (_deathInfoTexts.TryGetValue(target.ParentId, out var info) && info.text != null)
        {
            info.text.text = BuildDeathInfoText(target.ParentId, info.deviation);
        }
    }

    /// <summary>
    /// 死亡情報のテキストを構築する
    /// 死因と死亡時刻の情報を含むテキストを生成する
    /// </summary>
    /// <param name="victimId">被害者のプレイヤーID</param>
    /// <param name="deviation">死亡時刻の誤差(秒)</param>
    /// <returns>死亡情報のテキスト</returns>
    private string BuildDeathInfoText(byte victimId, int deviation)
    {
        var lines = new List<string>(2);

        ExPlayerControl victim = ExPlayerControl.ById(victimId);
        // 死因の表示
        if (victim != null && Data.IsCheckDeathReason)
        {
            string statusText = ModTranslation.GetString("FinalStatus." + victim.FinalStatus);
            lines.Add(ModTranslation.GetString("PsychometristDeathReasonText", statusText));
        }

        // 死亡時刻の表示
        if (victim != null && Data.IsCheckDeathTime && MurderDataManager.TryGetMurderData(victim, out var murderData))
        {
            int seconds = (int)(DateTime.UtcNow - murderData.DeathTimeUtc).TotalSeconds + deviation;
            if (seconds < 0) seconds = 0;
            lines.Add(ModTranslation.GetString("PsychometristSecondsAgoText", seconds));
        }

        return string.Join("\n", lines);
    }

    /// <summary>
    /// 殺人者の足跡を表示する
    /// 殺人発生時に記録された位置情報から足跡を生成する
    /// </summary>
    /// <param name="victimId">被害者のプレイヤーID</param>
    private void ShowFootprints(byte victimId)
    {
        ExPlayerControl victim = ExPlayerControl.ById(victimId);
        if (victim == null) return;
        if (!MurderDataManager.TryGetMurderData(victim, out var murderData)) return;
        if (murderData.Killer == null) return;

        var key = (murderData.Killer.PlayerId, victimId);
        var positions = PsychometristSharedState.GetFootprints(key.Item1, key.Item2);
        if (positions == null || positions.Count == 0) return;

        // 既存の足跡を削除
        if (_shownFootprints.TryGetValue(key, out var oldList))
        {
            foreach (var fp in oldList) fp?.Destroy();
            _shownFootprints.Remove(key);
        }

        // 新しい足跡を作成
        var created = new List<Footprint>(positions.Count);
        foreach (var pos in positions)
        {
            created.Add(new Footprint(durationSeconds: 0f, anonymousFootprints: true, position: pos));
        }
        _shownFootprints[key] = created;
    }

    /// <summary>
    /// ローカルの視覚効果をクリアする
    /// 死亡情報テキストと足跡をすべて削除する
    /// </summary>
    private void ClearLocalVisuals()
    {
        // 死亡情報テキストの削除
        foreach (var entry in _deathInfoTexts.Values)
        {
            if (entry.text != null)
                UnityEngine.Object.Destroy(entry.text.gameObject);
        }
        _deathInfoTexts.Clear();

        // 表示中の足跡の削除
        foreach (var list in _shownFootprints.Values)
        {
            foreach (var fp in list) fp?.Destroy();
        }
        _shownFootprints.Clear();

        // すべての足跡をクリア
        Footprint.ClearAll();
    }
}

/// <summary>
/// サイコメトリストの共有状態管理クラス
/// 殺人時の足跡記録を全プレイヤーで共有する
/// </summary>
internal static class PsychometristSharedState
{
    /// <summary>
    /// 足跡追跡データを格納するクラス
    /// </summary>
    private sealed class FootprintTrack
    {
        /// <summary>記録された位置のリスト</summary>
        public readonly List<Vector2> Positions = new();
        /// <summary>残り記録時間</summary>
        public float Remaining;
        /// <summary>次のサンプリングまでの時間</summary>
        public float SampleTimer;
        /// <summary>記録中かどうか</summary>
        public bool Recording;
    }

    /// <summary>殺人者と被害者のペアごとの足跡追跡データ</summary>
    private static readonly Dictionary<(byte killerId, byte victimId), FootprintTrack> Tracks = new();

    /// <summary>殺人イベントリスナー</summary>
    private static EventListener<MurderEventData> _murderListener;
    /// <summary>FixedUpdateイベントリスナー</summary>
    private static EventListener _fixedUpdateListener;

    public static void CoStartGame()
    {
        Tracks.Clear();
        PsychometristReportBlock.Clear();

        _murderListener = MurderEvent.Instance.AddListener(OnMurder);
        _fixedUpdateListener = FixedUpdateEvent.Instance.AddListener(OnFixedUpdate);
    }

    /// <summary>
    /// 殺人発生時の処理
    /// 殺人者の足跡記録を開始する
    /// </summary>
    /// <param name="data">殺人イベントデータ</param>
    private static void OnMurder(MurderEventData data)
    {
        if (data.killer == null || data.target == null) return;
        if (!Psychometrist.PsychometristIsCheckFootprints) return;

        var key = (killerId: data.killer.PlayerId, victimId: data.target.PlayerId);
        // 足跡追跡データの初期化
        Tracks[key] = new FootprintTrack
        {
            Remaining = Psychometrist.PsychometristCanCheckFootprintsTime,
            SampleTimer = 0.1f,
            Recording = true,
        };

        // 殺人時の最初の位置を記録
        Tracks[key].Positions.Add(data.killer.Player.GetTruePosition());
    }

    /// <summary>
    /// FixedUpdate時の処理
    /// 殺人者の位置を定期的にサンプリングして足跡を記録する
    /// </summary>
    private static void OnFixedUpdate()
    {
        if (Tracks.Count == 0) return;

        foreach (var key in Tracks.Keys.ToArray())
        {
            var track = Tracks[key];
            if (!track.Recording) continue;

            // 残り時間を減らす
            track.Remaining -= Time.fixedDeltaTime;
            if (track.Remaining <= 0f)
            {
                track.Recording = false;
                continue;
            }

            // 位置サンプリング
            track.SampleTimer -= Time.fixedDeltaTime;
            if (track.SampleTimer > 0f) continue;

            track.SampleTimer = 0.1f;
            ExPlayerControl killer = ExPlayerControl.ById(key.killerId);
            if (killer?.Player == null) continue;
            track.Positions.Add(killer.Player.GetTruePosition());
        }
    }

    public static IReadOnlyList<Vector2> GetFootprints(byte killerId, byte victimId)
    {
        return Tracks.TryGetValue((killerId, victimId), out var track) ? track.Positions : Array.Empty<Vector2>();
    }
}

/// <summary>
/// サイコメトリストの報告ブロック管理クラス
/// 調べた死体の報告を防ぐ
/// </summary>
internal static class PsychometristReportBlock
{
    /// <summary>報告ブロックされている死体プレイヤーのIDセット</summary>
    private static readonly HashSet<byte> Blocked = new();

    /// <summary>指定された死体が報告ブロックされているか確認する</summary>
    /// <param name="deadPlayerId">死体プレイヤーのID</param>
    /// <returns>ブロックされている場合はtrue</returns>
    public static bool IsBlocked(byte deadPlayerId) => Blocked.Contains(deadPlayerId);

    /// <summary>すべての報告ブロックをクリアする</summary>
    public static void Clear() => Blocked.Clear();

    /// <summary>
    /// 死体の報告ブロック状態を設定するRPC
    /// 調べた死体を報告できないようにする
    /// </summary>
    /// <param name="deadPlayerId">死体プレイヤーのID</param>
    /// <param name="blocked">ブロックする場合はtrue</param>
    [CustomRPC]
    public static void RpcSetReportBlocked(byte deadPlayerId, bool blocked)
    {
        if (blocked) Blocked.Add(deadPlayerId);
        else Blocked.Remove(deadPlayerId);

        // 対応するDeadBodyオブジェクトのReported状態を更新
        foreach (DeadBody body in UnityEngine.Object.FindObjectsOfType<DeadBody>())
        {
            if (body != null && body.ParentId == deadPlayerId)
            {
                body.Reported = blocked;
            }
        }
    }
}
