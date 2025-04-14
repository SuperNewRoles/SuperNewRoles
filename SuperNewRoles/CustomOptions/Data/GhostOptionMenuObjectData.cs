using System.Collections.Generic;
using UnityEngine;
using TMPro;
using SuperNewRoles.Roles;
using SuperNewRoles.Modules;

namespace SuperNewRoles.CustomOptions.Data
{
    public class GhostOptionMenuObjectData : OptionMenuBase
    {
        public static GhostOptionMenuObjectData Instance { get; private set; }

        /// <summary>
        /// メニューのGameObject
        /// </summary>
        public GameObject MenuObject { get; }

        /// <summary>
        /// タイトルテキスト
        /// </summary>
        public TextMeshPro TitleText { get; }

        /// <summary>
        /// スクロールバー
        /// </summary>
        public Scroller Scroller { get; set; }

        /// <summary>
        /// スクロール内部のGameObject
        /// </summary>
        public GameObject InnerScroll { get; set; }

        /// <summary>
        /// ゴーストロールボタンの辞書
        /// </summary>
        public Dictionary<GhostRoleId, GameObject> GhostRoleButtonDictionary { get; } = new();

        /// <summary>
        /// 現在選択中のゴーストロールID
        /// </summary>
        public GhostRoleId CurrentGhostRoleId { get; set; } = GhostRoleId.None;

        /// <summary>
        /// 人数・確率表示用TextMeshPro
        /// </summary>
        public TextMeshPro CurrentRoleNumbersOfCrewsText { get; set; }
        public TextMeshPro CurrentRolePercentageText { get; set; }

        /// <summary>
        /// 設定エリアのTransformやScroller
        /// </summary>
        public Transform SettingsInner { get; set; }
        public Scroller SettingsScroller { get; set; }
        public GameObject CurrentSettingsParent { get; set; }

        /// <summary>
        /// 現在表示中の設定とそのテキストコンポーネントのリスト
        /// </summary>
        public List<(TextMeshPro Text, CustomOption Option)> CurrentOptionDisplays { get; } = new();

        public GhostOptionMenuObjectData(GameObject menuObject) : base()
        {
            Instance = this;
            MenuObject = menuObject;
            TitleText = menuObject.transform.Find("TitleText").GetComponent<TextMeshPro>();
        }

        public override void Hide()
        {
            if (MenuObject != null)
                MenuObject.SetActive(false);
        }

        public override void UpdateOptionDisplay()
        {
            // オプションの表示を更新（必要に応じて実装）
        }
    }
}