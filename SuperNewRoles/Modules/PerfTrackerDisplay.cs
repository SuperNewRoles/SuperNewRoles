// PerfTrackerで計測した情報をTextMeshProを使用して画面に表示します。
// F10キーでの表示/非表示切り替え、Shift+F10でのリセット機能も担当します。

using UnityEngine;
using TMPro;
using System.Text;
using System.Linq;
using HarmonyLib;

namespace SuperNewRoles.Modules;

// このクラス全体が表示と操作のロジックを管理します
public static class PerfTrackerDisplay
{
    private static TextMeshPro _perfText;
    private static bool _isVisible = false;  // デフォルト非表示
    private static readonly StringBuilder _stringBuilder = new(512);

    /// <summary>
    /// キー入力を処理し、表示を更新します。
    /// </summary>
    private static void UpdatePerfTracker()
    {
        // F10キーで表示/非表示を切り替え
        if (Input.GetKeyDown(KeyCode.F10))
        {
            // Shiftキーが押されていない場合のみ
            if (!Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.RightShift))
            {
                _isVisible = !_isVisible;
            }
        }

        // Shift + F10キーでデータをリセット
        if ((Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) && Input.GetKeyDown(KeyCode.F10))
        {
            PerfTracker.Reset();
        }

        // テキストオブジェクトが存在しない場合は何もしない
        if (_perfText == null) return;

        // 表示状態に応じてテキストの有効/無効を切り替え
        _perfText.gameObject.SetActive(_isVisible);

        if (!_isVisible) return;

        // --- 表示文字列の構築 ---
        _stringBuilder.Clear();
        _stringBuilder.AppendLine("<b>-- PerfTracker --</b>");
        _stringBuilder.AppendLine("<size=80%>F10: Toggle, Shift+F10: Reset</size>");
        _stringBuilder.AppendLine("--------------------------");

        var stats = PerfTracker.GetStats();

        if (!stats.Any())
        {
            _stringBuilder.AppendLine("No data collected yet.");
        }
        else
        {
            // 固定幅フォントのような表示にするため、書式を調整
            _stringBuilder.AppendLine(">Key                         Last(ms)  Avg(ms)".ToMonospace());
            foreach (var stat in stats)
            {
                var line = $"{stat.Key,-25}: {stat.LastMs,8:F2} {stat.AvgMs,8:F2}";
                _stringBuilder.AppendLine(line.ToMonospace());
            }
        }

        // 構築した文字列をテキストに設定
        _perfText.SetText(_stringBuilder.ToString());
    }

    /// <summary>
    /// HudManager.Startの後に呼び出され、PerfTracker用のUI要素を生成・設定します。
    /// </summary>
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Start))]
    public static class HudManager_Start_Patch
    {
        // ベータ版かどうかを判定。false の場合、以下の Prefix/Postfix は一切登録されない
        public static bool Prepare()
        {
            return Statics.IsBeta;
        }
        public static void Postfix(HudManager __instance)
        {
            // 既存UI要素(roomTracker.text)を複製
            var originalText = __instance.roomTracker.text;
            if (originalText == null)
            {
                Logger.Error("roomTracker.textがnullです");
                return;
            }

            var perfTextObject = GameObject.Instantiate(originalText.gameObject);

            // 不要なRoomTrackerだけを消す（他コンポーネントは消さない）
            var roomTracker = perfTextObject.GetComponent<RoomTracker>();
            if (roomTracker != null) Object.Destroy(roomTracker);

            // 分かりやすい名前に
            perfTextObject.name = "PerfTrackerText";
            // 親の設定
            var logoObject = GameObject.Find("Logo (1)") ?? GameObject.Find("Logo");
            if (logoObject != null)
                perfTextObject.transform.SetParent(logoObject.transform);
            else
                perfTextObject.transform.SetParent(GameObject.FindObjectOfType<Canvas>().transform);

            Logger.Info($"PerfTrackerText を生成しました: {logoObject.name}", "PerfTrackerDisplay");

            // 表示サイズ・位置
            perfTextObject.transform.localScale = Vector3.one * 2.5f;
            perfTextObject.transform.localPosition = new Vector3(-5f, -2f, 0f);

            // TextMeshProコンポーネント取得（型を明示）
            _perfText = perfTextObject.GetComponent<TextMeshPro>();
            if (_perfText == null)
            {
                Logger.Error("PerfTrackerText に TextMeshPro がアタッチされていません");
                return;
            }

            // アライメントや色など
            _perfText.alignment = TextAlignmentOptions.TopLeft;
            _perfText.transform.localScale = Vector3.one * 5f; // サイズ調
            _perfText.fontSize = 4.0f;     // 必要なら大きめに
            _perfText.color = Color.white;

            // AspectPosition設定（省略可）
            var aspectPosition = perfTextObject.AddComponent<AspectPosition>();
            aspectPosition.Alignment = AspectPosition.EdgeAlignments.LeftTop;
            aspectPosition.DistanceFromEdge = new Vector3(0.0f, 3.0f); // 上が見切れるので対策
            aspectPosition.OnEnable();

            // 初期表示
            _perfText.SetText("PerfTracker Initialized...");
        }
    }

    /// <summary>
    /// HudManager.Updateの後に呼び出され、毎フレーム表示を更新します。
    /// </summary>
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    public static class HudManager_Update_Patch
    {
        // ベータ版かどうかを判定。false の場合、以下の Prefix/Postfix は一切登録されない
        public static bool Prepare()
        {
            return Statics.IsBeta;
        }
        public static void Postfix()
        {
            // 毎フレーム、キー入力のチェックと表示更新を行う
            UpdatePerfTracker();
        }
    }
}
