using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using SuperNewRoles.Modules;
using UnityEngine;

namespace SuperNewRoles.Patches
{
    public static class MeetingPagingManager
    {
        // 定数定義
        public const int ITEMS_PER_PAGE = 15;
        private const int ITEMS_PER_ROW = 3;
        private const float BUTTON_SCALE = 0.075f;
        private const float RIGHT_BUTTON_X_POS = 4.8f;
        private const float LEFT_BUTTON_X_POS = -4.75f;
        private const float BUTTON_Y_POS = 0f;
        private const float BUTTON_Z_POS = -3f;

        // ページング状態管理
        public static bool IsPagingEnabled { get; private set; }
        public static int CurrentPage { get; private set; }
        public static List<PlayerVoteArea> AllPlayerVoteAreas { get; private set; }

        // UI要素
        public static GameObject RightButton { get; private set; }
        public static GameObject LeftButton { get; private set; }

        // 初期化メソッド
        public static void Initialize(bool enabled, List<PlayerVoteArea> playerVoteAreas)
        {
            IsPagingEnabled = enabled;
            AllPlayerVoteAreas = playerVoteAreas;
            CurrentPage = 1;
        }

        // キャッシュ
        private static Sprite _pagingButtonSprite;
        private static readonly Vector3 OffScreen = new Vector3(100, 100, 100);
        private static readonly Vector3[] PlayerVoteAreaPositions = GeneratePlayerVoteAreaPositions();

        private static int TotalPages => AllPlayerVoteAreas == null ? 1 :
            (AllPlayerVoteAreas.Count + ITEMS_PER_PAGE - 1) / ITEMS_PER_PAGE;

        private static Vector3[] GeneratePlayerVoteAreaPositions()
        {
            const float START_X = -3.1f;
            const float START_Y = 1.5f;
            const float START_Z = -0.9f;
            const float X_INCREMENT = 2.9f;
            const float Y_INCREMENT = -0.76f;
            const float Z_INCREMENT = -0.01f;

            var positions = new Vector3[ITEMS_PER_PAGE];
            for (int i = 0; i < ITEMS_PER_PAGE; i++)
            {
                int row = i / ITEMS_PER_ROW;
                int col = i % ITEMS_PER_ROW;
                positions[i] = new Vector3(
                    START_X + col * X_INCREMENT,
                    START_Y + row * Y_INCREMENT,
                    START_Z + row * Z_INCREMENT
                );
            }
            return positions;
        }

        public static Sprite GetPagingButtonSprite()
        {
            if (_pagingButtonSprite == null)
            {
                _pagingButtonSprite = AssetManager.GetAsset<Sprite>("Meeting_AreaTabChange.png");
            }
            return _pagingButtonSprite;
        }

        public static void NextPage()
        {
            if (AllPlayerVoteAreas == null || CurrentPage >= TotalPages) return;

            CurrentPage++;
            UpdatePlayerVoteAreas();
        }

        public static void PreviousPage()
        {
            if (CurrentPage <= 1) return;

            CurrentPage--;
            UpdatePlayerVoteAreas();
        }

        public static void UpdatePlayerVoteAreas()
        {
            if (AllPlayerVoteAreas == null) return;

            // ボタンの表示状態を更新
            if (RightButton != null)
                RightButton.SetActive(CurrentPage < TotalPages);

            if (LeftButton != null)
                LeftButton.SetActive(CurrentPage > 1);

            // 現在のページの範囲を計算
            int startIndex = (CurrentPage - 1) * ITEMS_PER_PAGE;
            int endIndex = Mathf.Min(startIndex + ITEMS_PER_PAGE, AllPlayerVoteAreas.Count);

            // 全エリアを一旦画面外に移動
            for (int i = 0; i < AllPlayerVoteAreas.Count; i++)
            {
                var area = AllPlayerVoteAreas[i];
                if (i >= startIndex && i < endIndex)
                {
                    // 現在のページ内のアイテムは適切な位置に配置
                    area.transform.localPosition = PlayerVoteAreaPositions[i - startIndex];
                }
                else
                {
                    // それ以外は画面外に
                    area.transform.localPosition = OffScreen;
                }
            }
        }

        private static GameObject CreatePagingButton(GameObject template, Transform parent, string name,
            Vector3 position, Vector3 scale, System.Action onClick)
        {
            var button = Object.Instantiate(template, parent);
            button.name = name;
            button.SetActive(true);
            button.transform.localPosition = position;
            button.transform.localScale = scale;

            // 不要な子要素を削除
            var textChild = button.transform.Find("Text_TMP");
            if (textChild != null)
                Object.Destroy(textChild.gameObject);

            // スプライトを設定
            var renderer = button.GetComponent<SpriteRenderer>();
            renderer.sprite = GetPagingButtonSprite();

            // コライダーを更新
            var boxCollider = button.GetComponent<BoxCollider2D>();
            if (boxCollider != null)
                Object.Destroy(boxCollider);

            // PassiveButtonを設定
            var passiveButton = button.GetComponent<PassiveButton>();
            passiveButton.Colliders = new Collider2D[] { button.AddComponent<PolygonCollider2D>() };
            passiveButton.OnClick.RemoveAllListeners();
            passiveButton.OnClick.AddListener(onClick);
            passiveButton.OnMouseOver.AddListener((System.Action)(() => renderer.color = Color.green));
            passiveButton.OnMouseOut.AddListener((System.Action)(() => renderer.color = Color.white));

            return button;
        }

        public static void CreatePagingButtons(MeetingHud meetingHud)
        {
            var template = meetingHud.SkipVoteButton.gameObject;

            // 右ボタンを作成
            RightButton = CreatePagingButton(
                template,
                meetingHud.transform,
                "RightButton",
                new Vector3(RIGHT_BUTTON_X_POS, BUTTON_Y_POS, BUTTON_Z_POS),
                new Vector3(BUTTON_SCALE, BUTTON_SCALE, BUTTON_SCALE),
                NextPage
            );

            // 左ボタンを作成
            LeftButton = CreatePagingButton(
                template,
                meetingHud.transform,
                "LeftButton",
                new Vector3(LEFT_BUTTON_X_POS, BUTTON_Y_POS, BUTTON_Z_POS),
                new Vector3(-BUTTON_SCALE, BUTTON_SCALE, BUTTON_SCALE),
                PreviousPage
            );
        }
    }

    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Start))]
    public class MeetingHudStartPagingPatch
    {
        public static void Postfix(MeetingHud __instance)
        {
            if (PlayerControl.AllPlayerControls.Count <= ITEMS_PER_PAGE)
            {
                MeetingPagingManager.Initialize(false, null);
                return;
            }

            // プレイヤーステートをソート（生存者を優先）
            var playerStates = __instance.playerStates.ToList();
            var sorted = playerStates
                .OrderBy(x =>
                {
                    var exPlayer = ExPlayerControl.ById(x.TargetPlayerId);
                    return exPlayer == null ? 1 : (exPlayer.IsDead() ? 1 : 0);
                })
                .ToList();

            MeetingPagingManager.Initialize(true, sorted);
            MeetingPagingManager.CreatePagingButtons(__instance);
            MeetingPagingManager.UpdatePlayerVoteAreas();
        }

        private const int ITEMS_PER_PAGE = MeetingPagingManager.ITEMS_PER_PAGE;
    }

    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Update))]
    public class MeetingHudUpdatePagingPatch
    {
        public static void Postfix(MeetingHud __instance)
        {
            if (!MeetingPagingManager.IsPagingEnabled) return;

            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                MeetingPagingManager.NextPage();
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                MeetingPagingManager.PreviousPage();
            }
        }
    }
}