using System.Collections.Generic;
using UnityEngine;
using TMPro;
using SuperNewRoles.Roles;
using SuperNewRoles.Modules;
using System;
using System.Linq;

namespace SuperNewRoles.CustomOptions.Data;
public class ModifierOptionMenuObjectData : OptionMenuBase
{
    public static ModifierOptionMenuObjectData Instance { get; private set; }

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
    public ModifierCategoryDataBase CurrentCategory { get; set; }

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
    public Dictionary<string, List<GameObject>> CategoryModifierOptionGameObjects { get; private set; }
    public GameObject ModeMenu { get; set; }

    // AssignFilter Edit Menu related properties
    public GameObject AssignFilterEditMenu { get; set; }
    public Scroller AssignFilterEditRightAreaScroller { get; set; }
    public GameObject AssignFilterEditRightAreaInner { get; set; }
    public GameObject AssignFilterRoleDetailButtonContainer { get; set; }
    public ModifierCategoryDataBase CurrentEditingModifierForAssignFilter { get; set; }
    public string CurrentAssignFilterEditingRoleType { get; set; }

    public ModifierOptionMenuObjectData(GameObject standardOptionMenu) : base()
    {
        Instance = this;
        StandardOptionMenu = standardOptionMenu;
        LeftAreaInner = StandardOptionMenu.transform.Find("LeftArea/Scroller/Inner").gameObject;
        RightArea = StandardOptionMenu.transform.Find("RightArea").gameObject;
        RightAreaScroller = RightArea.transform.Find("Scroller").GetComponent<Scroller>();
        RightAreaInner = RightAreaScroller.transform.Find("Inner").gameObject;
        CategoryOptionObjects = new Dictionary<string, List<(CustomOption, GameObject)>>();
        CategoryModifierOptionGameObjects = new Dictionary<string, List<GameObject>>();
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
        if (AssignFilterEditMenu != null)
            AssignFilterEditMenu.SetActive(false);
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

        // Modifier固有オプションの表示を更新
        if (CurrentCategory is ModifierCategoryDataModifier modCategory && CategoryModifierOptionGameObjects.TryGetValue(CurrentCategory.Name, out var modifierGameObjects))
        {
            // ここで NumOfCrews と Percentage の GameObject を見つけてテキストを更新
            // 例えば、GameObject の名前に基づいて探すか、あるいは GameObject と対応する値の種類（NumOfCrews/Percentage）をペアで保存しておく必要がある
            // 今回は簡略化のため、リストの順序（0番目がNumOfCrews, 1番目がPercentage）に依存すると仮定する
            if (modifierGameObjects.Count >= 2)
            {
                // NumOfCrews の更新
                var numCrewsText = modifierGameObjects[0]?.transform.Find("SelectedText")?.GetComponent<TextMeshPro>();
                if (numCrewsText != null) numCrewsText.text = modCategory.ModifierOption.NumberOfCrews.ToString();

                // Percentage の更新
                var percentageText = modifierGameObjects[1]?.transform.Find("SelectedText")?.GetComponent<TextMeshPro>();
                if (percentageText != null) percentageText.text = $"{modCategory.ModifierOption.Percentage}%";
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
        public TextMeshPro SelectedText { get; set; }
    }

    public abstract class ModifierCategoryDataBase
    {
        public string Name { get; set; }
        public IEnumerable<CustomOption> Options { get; set; }
        public bool HiddenOption { get; set; } = false;
        public bool AssignFilter { get; set; } = false;
        public virtual Func<List<RoleId>> AssignFilterList { get; } = () => [];
        public virtual Action<List<RoleId>> OnUpdateAssignFilter { get; } = (_) => { };
    }
    public class ModifierCategoryDataCategory : ModifierCategoryDataBase
    {
        private List<RoleId> _assignFilterList;
        public override Func<List<RoleId>> AssignFilterList { get; }
        public override Action<List<RoleId>> OnUpdateAssignFilter { get; }
        public ModifierCategoryDataCategory(CustomOptionCategory category)
        {
            Name = category.Name;
            Options = category.Options;
            Logger.Info(category.GetType().Name);
            AssignFilter = category.HasModifierAssignFilter;
            _assignFilterList = category.ModifierAssignFilter;
            AssignFilterList = () => _assignFilterList;
            OnUpdateAssignFilter = (list) =>
            {
                _assignFilterList = list;
                category.ModifierAssignFilter = list;
            };
        }
    }
    public class ModifierCategoryDataModifier : ModifierCategoryDataBase
    {
        public RoleOptionManager.ModifierRoleOption ModifierOption { get; }
        private List<RoleId> _assignFilterList;
        public override Func<List<RoleId>> AssignFilterList { get; }
        public override Action<List<RoleId>> OnUpdateAssignFilter { get; }

        public ModifierCategoryDataModifier(RoleOptionManager.ModifierRoleOption modifier)
        {
            ModifierOption = modifier;
            Name = ModHelpers.CsWithTranslation(ModifierOption.RoleColor, modifier.ModifierRoleId.ToString());
            Options = modifier.Options;
            if (CustomRoleManager.TryGetModifierById(modifier.ModifierRoleId, out var modifierData))
            {
                AssignFilter = modifierData.AssignFilter;
                _assignFilterList = RoleOptionManager.ModifierRoleOptions.FirstOrDefault(x => x.ModifierRoleId == modifier.ModifierRoleId)?.AssignFilterList ?? new List<RoleId>();
            }
            else
            {
                AssignFilter = false;
                _assignFilterList = new List<RoleId>();
            }

            AssignFilterList = () => _assignFilterList;
            OnUpdateAssignFilter = (list) =>
            {
                _assignFilterList = list;
                // CustomRoleManager にも変更を反映する必要があるか確認
                if (CustomRoleManager.TryGetModifierById(modifier.ModifierRoleId, out var modDataToUpdate))
                {
                    RoleOptionManager.ModifierRoleOptions.FirstOrDefault(x => x.ModifierRoleId == modifier.ModifierRoleId).AssignFilterList = list;
                }
            };
        }
    }
}