// PerfTrackerDisplay.cs
// PerfTrackerで計測した情報をTextMeshProを使用して画面に表示します。
// F10キーでの表示/非表示切り替え、Shift+F10でのリセット機能も担当します。
// このスクリプトは、シーン内のいずれかのGameObjectにアタッチしてください。

using UnityEngine;
using TMPro;
using System.Text;
using System.Linq;
using UnityEngine.UI;
using HarmonyLib;

namespace SuperNewRoles.Modules;

// このクラス全体が表示と操作のロジックを管理します
public static class PerfTrackerDisplay
{
    private static TextMeshPro _perfText;
    private static bool _isVisible = true;
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
            _stringBuilder.AppendLine("<font_mono>Key                  Last(ms)   Avg(ms)</font_mono>");
            foreach (var stat in stats)
            {
                _stringBuilder.AppendFormat("<font_mono>{0,-20}: {1,8:F2} {2,8:F2}</font_mono>\n", stat.Key, stat.LastMs, stat.AvgMs);
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
        public static void Postfix(HudManager __instance)
        {
            // 既存UI要素(roomTracker.text)を複製
            var originalText = __instance.roomTracker.text;
            var perfTextObject = GameObject.Instantiate(originalText.gameObject);

            // RoomTracker等不要なコンポーネントを削除
            Object.Destroy(perfTextObject.GetComponent<RoomTracker>());

            // 分かりやすい名前に
            perfTextObject.name = "PerfTrackerText";

            // 親をAmong Usロゴ等いつも存在するUIオブジェクトに付け換える
            // 例: _logoObjectが既設の場合。なければCanvas直下に配置推奨
            var logoObject = GameObject.Find("Logo (1)") ?? GameObject.Find("Logo"); // 例: ロゴオブジェクト名
            if (logoObject != null)
                perfTextObject.transform.SetParent(logoObject.transform);
            else
                perfTextObject.transform.SetParent(GameObject.FindObjectOfType<Canvas>().transform);

            // 表示サイズ・位置を調整
            perfTextObject.transform.localScale = Vector3.one * 2.5f;
            perfTextObject.transform.localPosition = new Vector3(-3.45f, -1.4f, 0); // 配置は調整してください

            // TextMeshProコンポーネント取得
            _perfText = perfTextObject.GetComponent<TextMeshPro>();
            if (_perfText == null)
            {
                Logger.Error("PerfTrackerText に TextMeshPro がアタッチされていません");
                return;
            }

            // テキスト表示プロパティを調整
            _perfText.alignment = TextAlignmentOptions.TopLeft;
            _perfText.fontSize = 4.0f;     // 見やすいサイズに
            _perfText.color = Color.white;

            // AspectPosition等で位置固定する場合はここで設定
            var aspectPosition = perfTextObject.AddComponent<AspectPosition>();
            aspectPosition.Alignment = AspectPosition.EdgeAlignments.LeftTop;
            aspectPosition.DistanceFromEdge = new Vector3(1.0f, 0.6f); // 画面端からの距離（調整可）
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
        public static void Postfix()
        {
            // 毎フレーム、キー入力のチェックと表示更新を行う
            UpdatePerfTracker();
        }
    }
}
