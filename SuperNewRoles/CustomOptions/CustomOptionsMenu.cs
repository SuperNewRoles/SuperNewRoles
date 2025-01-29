using System.Linq;
using SuperNewRoles.Modules;
using UnityEngine;
using UnityEngine.Events;

namespace SuperNewRoles.CustomOptions;

public static class CustomOptionsMenu
{
    private const string OptionsMenu_VanillaName = "Setting_Vanilla";
    public static void ShowOptionsMenu()
    {
        var obj = GameObject.Instantiate(AssetManager.GetAsset<GameObject>("OptionsMenuSelector"));
        obj.transform.SetParent(GameObject.FindObjectOfType<GameSettingMenu>().transform, false);
        // obj.transform.localPosition = new(-3.2f, 2.65f, -0.2f);
        obj.transform.localPosition = new(-3.046f, 2.65f, -1f);
        // -3.046 2.65 -1
        // -3.24 2.66 -1 0.29
        obj.transform.localScale = Vector3.one * 0.31f;
        // Setting_から始まる子オブジェクトをすべて代入
        var categories = obj.transform.GetComponentsInChildren<Transform>()
            .Where(t => t.name.StartsWith("Setting_"))
            .Select(t => t.gameObject)
            .ToArray();
        foreach (var category in categories)
        {
            SetupPassiveButton(category);
            static void SetupPassiveButton(GameObject category)
            {
                string categoryName = category.name;
                PassiveButton passiveButton = category.AddComponent<PassiveButton>();
                passiveButton.OnClick = new();
                passiveButton.OnMouseOut = new();
                passiveButton.OnMouseOver = new();
                passiveButton.OnClick.AddListener((UnityAction)(() =>
                {
                    Logger.Info($"Category Clicked: {category.name}");
                    if (categoryName == OptionsMenu_VanillaName)
                    {
                        SetVanillaTabActive(true);
                    }
                    else
                    {
                        SetVanillaTabActive(false);
                    }
                }));
                passiveButton.OnMouseOver.AddListener((UnityAction)(() =>
                {
                    SetCategoryHighlight(category, true);
                }));
                passiveButton.OnMouseOut.AddListener((UnityAction)(() =>
                {
                    SetCategoryHighlight(category, false);
                }));
            }
        }
    }
    private static void SetVanillaTabActive(bool active)
    {
        var gameSettingsMenu = GameObject.FindObjectOfType<GameSettingMenu>();
        gameSettingsMenu.transform.Find("MainArea").gameObject.SetActive(active);
        gameSettingsMenu.transform.Find("LeftPanel").gameObject.SetActive(active);
        gameSettingsMenu.transform.Find("What Is This?").gameObject.SetActive(active);
        gameSettingsMenu.transform.Find("GameSettingsLabel").gameObject.SetActive(active);
        gameSettingsMenu.transform.Find("PanelSprite/LeftSideTint").gameObject.SetActive(active);
    }
    private static void SetCategoryHighlight(GameObject category, bool active)
    {
        category.transform.Find("Highlight").gameObject.SetActive(active);
    }
}
