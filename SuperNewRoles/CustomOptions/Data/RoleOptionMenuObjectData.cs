using System.Collections.Generic;
using UnityEngine;
using TMPro;
using SuperNewRoles.Roles;
using SuperNewRoles.Modules;

namespace SuperNewRoles.CustomOptions.Data
{
    public class RoleOptionMenuObjectData : OptionMenuBase
    {
        /// <summary>
        /// デフォルトのメニュースケール
        /// </summary>
        public const float DEFAULT_SCALE = 0.2f;

        /// <summary>
        /// タイトルテキストのスケール
        /// </summary>
        public const float TITLE_TEXT_SCALE = 0.7f;

        /// <summary>
        /// メニューのZ位置
        /// </summary>
        public const float Z_POSITION = -1f;

        public static RoleOptionMenuObjectData Instance { get; private set; }

        /// <summary>
        /// メニューのGameObjectを取得
        /// </summary>
        public GameObject MenuObject { get; }

        /// <summary>
        /// タイトルテキストのコンポーネントを取得
        /// </summary>
        public TextMeshPro TitleText { get; }

        /// <summary>
        /// スクローラーコンポーネント
        /// </summary>
        public Scroller Scroller { get; set; }

        /// <summary>
        /// スクロール内部のGameObject
        /// </summary>
        public GameObject InnerScroll { get; set; }

        public Transform CurrentScrollParent { get; set; }
        public Dictionary<RoleOptionMenuType, GameObject> RoleScrollDictionary { get; } = new();
        public Dictionary<RoleOptionMenuType, float> ScrollPositionDictionary { get; } = new();
        public RoleOptionMenuType CurrentRoleType { get; set; }

        public RoleId CurrentRoleId { get; set; }
        public TextMeshPro CurrentRoleNumbersOfCrewsText { get; set; }

        /// <summary>
        /// 現在選択中のロールの確率表示用TextMeshPro
        /// </summary>
        public TextMeshPro CurrentRolePercentageText { get; set; }

        /// <summary>
        /// メニューのBoxCollider2Dをキャッシュ
        /// </summary>
        public BoxCollider2D MenuObjectCollider { get; private set; }

        /// <summary>
        /// 設定メニューのスクローラーをキャッシュ
        /// </summary>
        public Scroller SettingsScroller { get; set; }
        public Transform SettingsInner { get; set; }
        public GameObject BulkRoleSettingsMenu { get; set; }

        /// <summary>
        /// 一括設定メニューのスクローラーとその内部コンテンツ
        /// </summary>
        public Scroller BulkSettingsScroller { get; set; }
        public Transform BulkSettingsInner { get; set; }
        public GameObject CurrentBulkSettingsParent { get; set; }

        /// <summary>
        /// 標準設定メニューのGameObject
        /// </summary>
        public GameObject StandardOptionMenu { get; set; }

        /// <summary>
        /// RoleIdとRoleDetailButtonの対応を保存するDictionary
        /// </summary>
        public Dictionary<RoleId, GameObject> RoleDetailButtonDictionary { get; } = new();

        /// <summary>
        /// 現在表示中の設定とそのテキストコンポーネントのリスト
        /// </summary>
        public List<(TextMeshPro Text, CustomOption Option)> CurrentOptionDisplays { get; } = new();

        /// <summary>
        /// コンストラクター：メニューオブジェクトからデータを初期化
        /// </summary>
        public RoleOptionMenuObjectData(GameObject menuObject) : base()
        {
            Instance = this;
            MenuObject = menuObject;
            TitleText = MenuObject.transform.Find("TitleText").GetComponent<TextMeshPro>();
            MenuObjectCollider = MenuObject.GetComponent<BoxCollider2D>();
            BulkSettingsInner = MenuObject.transform.Find("BulkSettings/Scroller/Inner");

            var (scroller, innerScroll) = CreateScrollbar(MenuObject.transform);
            Scroller = scroller;
            InnerScroll = innerScroll;
        }

        private (Scroller scroller, GameObject innerScroll) CreateScrollbar(Transform parent)
        {
            var scrollerObject = new GameObject("Scroller");
            scrollerObject.transform.SetParent(parent);
            scrollerObject.transform.localScale = Vector3.one;
            scrollerObject.transform.localPosition = Vector3.zero;

            var innerScrollObject = new GameObject("Inner");
            innerScrollObject.transform.SetParent(scrollerObject.transform);
            innerScrollObject.transform.localScale = Vector3.one;
            innerScrollObject.transform.localPosition = Vector3.zero;

            var scroller = scrollerObject.AddComponent<Scroller>();
            scroller.ScrollbarYBounds = new FloatRange(0, 1);
            scroller.ContentYBounds = new FloatRange(0, 0);
            scroller.enabled = true;

            return (scroller, innerScrollObject);
        }

        public override void Hide()
        {
            if (MenuObject != null)
                MenuObject.SetActive(false);
            if (BulkRoleSettingsMenu != null)
                BulkRoleSettingsMenu.SetActive(false);
            if (StandardOptionMenu != null)
                StandardOptionMenu.SetActive(false);
        }

        public override void UpdateOptionDisplay()
        {
            if (CurrentOptionDisplays == null) return;

            foreach (var (text, option) in CurrentOptionDisplays)
            {
                if (text != null && option != null)
                {
                    text.text = UIHelper.FormatOptionValue(option.Selections[option.Selection], option);
                }
            }
        }
    }
}