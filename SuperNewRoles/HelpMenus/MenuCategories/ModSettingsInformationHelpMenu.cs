using System.Linq;
using System.Text;
using SuperNewRoles.Modules;
using SuperNewRoles.CustomOptions;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace SuperNewRoles.HelpMenus.MenuCategories;

public class ModSettingsInformationHelpMenu : HelpMenuCategoryBase
{
    public override string Name => "ModSettingsInformation";
    public override HelpMenuCategory Category => HelpMenuCategory.ModSettingsInformation;

    private GameObject Container;
    private GameObject MenuObject;
    public string lastHash;
    private DelayTask _updateShowTask;

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
        SetupCategories(settingsInformation, categories);
    }

    private void SetupCategories(Transform settingsInformation, CustomOptionCategory[] categories)
    {
        // OptionBaseのテンプレートを取得
        var optionBaseTemplate = settingsInformation.Find("OptionBase");
        if (optionBaseTemplate == null)
        {
            Logger.Error("OptionBaseテンプレートが見つかりませんでした。");
            return;
        }

        // 既存のOptionBaseを削除（テンプレート以外）
        foreach (GameObject child in settingsInformation.gameObject.GetChildren())
        {
            if (child.name.StartsWith("OptionBase(Clone)"))
            {
                GameObject.Destroy(child.gameObject);
            }
        }

        // 表示位置の初期設定
        float yPos = 0f;
        float categorySpacing = 0.6f;

        // カテゴリごとにOptionBaseを作成
        foreach (var category in categories)
        {
            // カテゴリ内にオプションがない場合はスキップ
            if (category.Options.Count == 0) continue;

            // OptionBaseを複製
            var optionBase = GameObject.Instantiate(optionBaseTemplate.gameObject, settingsInformation);
            optionBase.name = $"OptionBase(Clone)_{category.Name}";
            optionBase.transform.localPosition = new Vector3(0, yPos, 0);
            optionBase.SetActive(true);

            // カテゴリ名を設定
            var categoryNameTMP = optionBase.transform.Find("CategoryName")?.GetComponent<TextMeshPro>();
            if (categoryNameTMP != null)
            {
                categoryNameTMP.text = ModTranslation.GetString(category.Name);
            }

            // Optionsテンプレートを取得
            var optionsTemplate = optionBase.transform.Find("Options")?.gameObject;
            if (optionsTemplate == null)
            {
                Logger.Error("Optionsテンプレートが見つかりませんでした。");
                continue;
            }

            // Optionsコンテナを作成
            var optionsContainer = optionsTemplate.transform.parent.gameObject;

            // オプションの位置設定
            float optionYPos = 1.75f;
            float optionSpacing = 0.33f;

            // カテゴリ内の各オプションを表示
            foreach (var option in category.Options)
            {
                // Optionsを複製
                var optionItem = GameObject.Instantiate(optionsTemplate, optionsContainer.transform);
                optionItem.name = $"Options(Clone)_{option.Name}";
                optionItem.transform.localPosition = new Vector3(2.1f, optionYPos, 0);
                optionItem.SetActive(true);

                // オプション名と値を設定
                var optionText = optionItem.GetComponent<TextMeshPro>();
                if (optionText != null)
                {
                    string optionName = option.Name;
                    string optionValue = option.GetCurrentSelectionString();
                    optionText.text = $"{ModTranslation.GetString(optionName)}: {optionValue}";
                }

                // 子オプションがあれば再帰的に表示
                if (option.ChildrenOption.Count > 0)
                {
                    foreach (var childOption in option.ChildrenOption)
                    {
                        // 子オプションの表示条件をチェック
                        if (!childOption.ShouldDisplay()) continue;

                        optionYPos -= optionSpacing;

                        // 子オプションのUIを作成
                        var childOptionItem = GameObject.Instantiate(optionsTemplate, optionsContainer.transform);
                        childOptionItem.name = $"Options(Clone)_{childOption.Name}";
                        childOptionItem.transform.localPosition = new Vector3(2.5f, optionYPos, 0); // インデント
                        childOptionItem.SetActive(true);

                        // 子オプション名と値を設定
                        var childOptionText = childOptionItem.GetComponent<TextMeshPro>();
                        if (childOptionText != null)
                        {
                            string childOptionName = childOption.Name;
                            string childOptionValue = childOption.GetCurrentSelectionString();
                            childOptionText.text = $"{ModTranslation.GetString(childOptionName)}: {childOptionValue}";
                        }
                    }
                }

                optionYPos -= optionSpacing;
            }

            // オプションテンプレートを非表示
            optionsTemplate.SetActive(false);

            // カテゴリごとの間隔を設定
            yPos -= category.Options.Count * 0.38f + 0.3f;
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