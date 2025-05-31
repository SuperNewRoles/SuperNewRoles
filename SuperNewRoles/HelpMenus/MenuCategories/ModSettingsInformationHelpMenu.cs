using System.Linq;
using System.Text;
using SuperNewRoles.Modules;
using SuperNewRoles.CustomOptions;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using Unity.Services.Core.Internal;

namespace SuperNewRoles.HelpMenus.MenuCategories;

public class ModSettingsInformationHelpMenu : HelpMenuCategoryBase
{
    public override string Name => "ModSettingsInformation";
    public override HelpMenuCategory Category => HelpMenuCategory.ModSettingsInformation;

    private GameObject Container;
    private GameObject MenuObject;
    public string lastHash;
    private DelayTask _updateShowTask;


    // 定数定義：ヘッダー高さ、オプション間のオフセット、初期オプション表示位置、子オプションのインデント幅
    public const float headerHeight = 0.6f;
    public const float optionYOffset = 0.25f;
    public const float textYOffset = optionYOffset * 0.8f;
    public const float headerOptionStartY = 1.75f;
    public const float indentWidth = 0.4f;

    public override void Show(GameObject Container)
    {
        this.Container = Container;

        // 既存のMenuObjectがあれば破棄
        if (MenuObject != null)
        {
            GameObject.Destroy(MenuObject);
            MenuObject = null;
        }

        // AssetManagerからメニューオブジェクトを取得
        MenuObject = GameObject.Instantiate(AssetManager.GetAsset<GameObject>("ModSettingsInformationHelpMenu"), Container.transform);
        MenuObject.transform.localPosition = Vector3.zero;
        MenuObject.transform.localScale = Vector3.one;
        MenuObject.transform.localRotation = Quaternion.identity;

        // 表示を更新
        UpdateShow();
    }

    public override void UpdateShow()
    {
        DelayTask.CancelIfExist(ref _updateShowTask);

        // MenuObjectがnullの場合は処理を中断
        if (MenuObject == null) return;

        var settingsScroller = MenuObject.transform.Find("Scroller");
        if (settingsScroller == null)
        {
            Logger.Error("Scrollerオブジェクトが見つかりませんでした。");
            return;
        }

        // 設定情報表示用のコンテナを取得
        var settingsInformation = settingsScroller.Find("SettingsInformation");
        if (settingsInformation == null)
        {
            Logger.Error("SettingsInformationオブジェクトが見つかりませんでした。");
            return;
        }

        // すべてのカテゴリを取得
        var categories = CustomOptionManager.GetOptionCategories().ToArray();
        SetupCategories(settingsInformation, categories, settingsScroller.GetComponent<Scroller>());
    }

    private void SetupCategories(Transform settingsInformation, CustomOptionCategory[] categories, Scroller scroller)
    {
        // OptionBaseのテンプレートを取得
        var optionBaseTemplate = settingsInformation.Find("OptionBase");
        if (optionBaseTemplate == null)
        {
            Logger.Error("OptionBaseテンプレートが見つかりませんでした。");
            return;
        }

        // 既存のOptionBase（テンプレート以外）の削除
        foreach (GameObject child in settingsInformation.gameObject.GetChildren())
        {
            if (child.name.StartsWith("OptionBase(Clone)"))
            {
                GameObject.Destroy(child);
            }
        }

        // 表示位置の初期設定
        float lastY = 0f;
        float contentYBoundsMax = 0f;

        // 各カテゴリごとにOptionBase（ヘッダー）を作成
        foreach (var category in categories)
        {
            // カテゴリ内にオプションが存在しなければスキップ
            if (category.Options.Length == 0) continue;
            if (category.IsModifier) continue;

            // ヘッダー用のOptionBaseを複製して配置
            var header = GameObject.Instantiate(optionBaseTemplate.gameObject, settingsInformation);
            header.name = $"OptionBase(Clone)_{category.Name}";
            header.transform.localPosition = new Vector3(0, lastY, 0);
            header.SetActive(true);
            lastY -= headerHeight;
            contentYBoundsMax += headerHeight;

            // カテゴリ名を設定
            var categoryNameTMP = header.transform.Find("CategoryName")?.GetComponent<TextMeshPro>();
            if (categoryNameTMP != null)
            {
                categoryNameTMP.text = ModTranslation.GetString(category.Name);
            }

            // Optionsテンプレートを取得
            var optionsTemplate = header.transform.Find("Options")?.gameObject;
            if (optionsTemplate == null)
            {
                Logger.Error("Optionsテンプレートが見つかりませんでした。");
                continue;
            }

            // Optionsコンテナはヘッダーと同じ
            var optionsContainer = optionsTemplate.transform.parent.gameObject;

            // カテゴリ内のオプション表示開始位置（ヘッダー内部での開始Y座標）
            float optionYPos = headerOptionStartY;

            // カテゴリ内の各オプションを表示
            foreach (var option in category.Options)
            {
                CreateOptionUI(optionsTemplate, optionsContainer.transform, option, 2.1f, ref optionYPos, ref lastY, ref contentYBoundsMax, indentWidth);
            }

            // Optionsテンプレートは元々のテンプレートなので非表示
            optionsTemplate.SetActive(false);
        }

        scroller.ContentYBounds.max = contentYBoundsMax / 0.9f + 0.2f;  // スクロール領域の調整値
    }

    private void CreateOptionUI(GameObject template, Transform container, CustomOption option, float currentX, ref float optionYPos, ref float lastYRef, ref float bounds, float indentWidth)
    {
        // オプションUIを複製して配置
        var item = GameObject.Instantiate(template, container);
        item.name = $"Options(Clone)_{option.Name}";
        item.transform.localPosition = new Vector3(currentX, optionYPos, 0);
        item.SetActive(true);
        var optionText = item.GetComponent<TextMeshPro>();
        if (optionText != null)
        {
            string optionValue = option.GetCurrentSelectionString();
            optionText.text = $"{ModTranslation.GetString(option.Name)}: {optionValue}";
        }

        optionYPos -= optionYOffset;
        lastYRef -= textYOffset;
        bounds += textYOffset * 0.45f;

        // 子オプションがあれば、表示条件を確認して再帰的に生成
        if (option.ChildrenOption.Count > 0)
        {
            foreach (var childOption in option.ChildrenOption)
            {
                if (!childOption.ShouldDisplay()) continue;
                // 子オプションUIを再帰的に作成（インデント付き）
                CreateOptionUI(template, container, childOption, currentX + indentWidth, ref optionYPos, ref lastYRef, ref bounds, indentWidth);
            }
        }
    }

    public override void Hide(GameObject Container)
    {
        if (MenuObject != null)
        {
            GameObject.Destroy(MenuObject);
            MenuObject = null;
        }
    }

    // カテゴリの設定ハッシュを生成するメソッド（変更検知用）
    private string GenerateSettingsHash()
    {
        StringBuilder sb = new StringBuilder();
        foreach (var category in CustomOptionManager.GetOptionCategories())
        {
            sb.Append(category.Name).Append(":");
            foreach (var option in category.Options)
            {
                sb.Append(option.Name).Append("=").Append(option.Selection).Append(";");
            }
        }
        return sb.ToString();
    }

    public override void OnUpdate()
    {
        string currentHash = GenerateSettingsHash();
        if (currentHash != lastHash)
        {
            lastHash = currentHash;
            UpdateShow();
        }
    }
}