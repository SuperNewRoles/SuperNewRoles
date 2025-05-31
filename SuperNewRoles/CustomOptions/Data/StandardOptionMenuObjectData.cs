using System.Collections.Generic;
using UnityEngine;
using TMPro;
using SuperNewRoles.Roles;
using SuperNewRoles.Modules;

namespace SuperNewRoles.CustomOptions.Data
{
    public class StandardOptionMenuObjectData : OptionMenuBase
    {
        public static StandardOptionMenuObjectData Instance { get; private set; }

        /// <summary>
        /// メニューのGameObjectを取得
        /// </summary>
        public GameObject StandardOptionMenu { get; }

        /// <summary>
        /// 左エリアの内部コンテンツ
        /// </summary>
        public GameObject LeftAreaInner { get; }

        /// <summary>
        /// 右エリアの内部コンテンツ
        /// </summary>
        public GameObject RightAreaInner { get; }
        /// <summary>
        /// 右エリア
        /// </summary>
        public GameObject RightArea { get; }
        /// <summary>
        /// 右エリアのスクローラー
        /// </summary>
        public Scroller RightAreaScroller { get; }

        /// <summary>
        /// プリセットボタンのコンテナ
        /// </summary>
        public GameObject PresetButtonsContainer { get; set; }

        /// <summary>
        /// 現在表示中のオプションメニュー
        /// </summary>
        public GameObject CurrentOptionMenu { get; set; }

        /// <summary>
        /// 現在選択中のカテゴリー
        /// </summary>
        public CustomOptionCategory CurrentCategory { get; set; }

        /// <summary>
        /// 現在選択中のボタン
        /// </summary>
        public GameObject CurrentSelectedButton { get; set; }

        /// <summary>
        /// 標準オプションメニューのディクショナリ
        /// </summary>
        public Dictionary<string, GameObject> StandardOptionMenus { get; } = new();

        /// <summary>
        /// カテゴリー別のオプションUIデータ
        /// </summary>
        public Dictionary<string, List<OptionUIDataBase>> CategoryOptionUIData { get; } = new();

        public Dictionary<string, List<(CustomOption Option, GameObject GameObject)>> CategoryOptionObjects;
        public GameObject ModeMenu { get; set; }

        public StandardOptionMenuObjectData(GameObject standardOptionMenu) : base()
        {
            Instance = this;
            StandardOptionMenu = standardOptionMenu;
            LeftAreaInner = StandardOptionMenu.transform.Find("LeftArea/Scroller/Inner").gameObject;
            RightArea = StandardOptionMenu.transform.Find("RightArea").gameObject;
            RightAreaScroller = RightArea.transform.Find("Scroller").GetComponent<Scroller>();
            RightAreaInner = RightAreaScroller.transform.Find("Inner").gameObject;
            CategoryOptionObjects = new Dictionary<string, List<(CustomOption, GameObject)>>();
        }

        public void AddOptionUIData(string categoryName, CustomOption option, GameObject uiObject, bool isBooleanOption)
        {
            if (!CategoryOptionUIData.ContainsKey(categoryName))
            {
                CategoryOptionUIData[categoryName] = new List<OptionUIDataBase>();
            }

            if (isBooleanOption)
            {
                CategoryOptionUIData[categoryName].Add(new CheckOptionUIData
                {
                    Option = option,
                    UIObject = uiObject,
                    CheckMark = uiObject.transform.Find("CheckMark").gameObject
                });
            }
            else
            {
                CategoryOptionUIData[categoryName].Add(new SelectOptionUIData
                {
                    Option = option,
                    UIObject = uiObject,
                    SelectedText = uiObject.transform.Find("SelectedText").GetComponent<TMPro.TextMeshPro>()
                });
            }
        }

        public override void Hide()
        {
            if (StandardOptionMenu != null)
                StandardOptionMenu.SetActive(false);
        }

        public override void UpdateOptionDisplay()
        {
            if (CurrentCategory == null || CurrentOptionMenu == null) return;

            // カテゴリーのオプションを更新
            if (CategoryOptionUIData.TryGetValue(CurrentCategory.Name, out var optionUIDataList))
            {
                foreach (var optionUIData in optionUIDataList)
                {
                    var option = optionUIData.Option;
                    if (option.IsBooleanOption && optionUIData is CheckOptionUIData checkData)
                    {
                        checkData.CheckMark.SetActive((bool)option.Value);
                    }
                    else if (!option.IsBooleanOption && optionUIData is SelectOptionUIData selectData)
                    {
                        selectData.SelectedText.text = option.GetCurrentSelectionString();
                    }
                }
            }
        }

        public abstract class OptionUIDataBase
        {
            public CustomOption Option { get; set; }
            public GameObject UIObject { get; set; }
        }

        public class CheckOptionUIData : OptionUIDataBase
        {
            public GameObject CheckMark { get; set; }
        }

        public class SelectOptionUIData : OptionUIDataBase
        {
            public TMPro.TextMeshPro SelectedText { get; set; }
        }
    }
}