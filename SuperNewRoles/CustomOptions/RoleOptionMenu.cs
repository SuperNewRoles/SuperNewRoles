using SuperNewRoles.Modules;
using TMPro;
using UnityEngine;

namespace SuperNewRoles.CustomOptions;

public enum RoleOptionMenuType
{
    Impostor,
    Crewmate,
    Neutral,
}
public class RoleOptionMenuObjectData
{
    public GameObject menuObject { get; }
    public TextMeshPro TitleText { get; }
    public Scrollbar Scrollbar { get; set; }
    public RoleOptionMenuObjectData(GameObject roleOptionMenuObject)
    {
        menuObject = roleOptionMenuObject;
        TitleText = roleOptionMenuObject.transform.Find("TitleText").GetComponent<TextMeshPro>();
    }
}
public static class RoleOptionMenu
{
    public static RoleOptionMenuObjectData RoleOptionMenuObjectData;
    public static void ShowRoleOptionMenu(RoleOptionMenuType type)
    {
        if (RoleOptionMenuObjectData == null || RoleOptionMenuObjectData.menuObject == null)
        {
            RoleOptionMenuObjectData = InitializeRoleOptionMenuObject(type);
        }
        // タイトルを設定
        RoleOptionMenuObjectData.TitleText.text = $"<b>{ModTranslation.GetString($"RoleOptionMenuType.{type}")}</b>";
        // メニューを表示
        RoleOptionMenuObjectData.menuObject.SetActive(true);
    }
    public static void HideRoleOptionMenu()
    {
        if (RoleOptionMenuObjectData == null || RoleOptionMenuObjectData.menuObject == null)
        {
            return;
        }
        RoleOptionMenuObjectData.menuObject.SetActive(false);
    }
    private static RoleOptionMenuObjectData InitializeRoleOptionMenuObject(RoleOptionMenuType type)
    {
        var obj = GameObject.Instantiate(AssetManager.GetAsset<GameObject>("RoleOptionMenu"));
        obj.transform.SetParent(GameObject.FindObjectOfType<GameSettingMenu>().transform);
        obj.transform.localScale = Vector3.one * 0.2f;
        obj.transform.localPosition = new(0, 0, -1f);
        var data = new RoleOptionMenuObjectData(obj);
        data.TitleText.transform.localScale = Vector3.one * 0.7f;
        data.Scrollbar = CopyScrollbar(data.menuObject.transform);
        return data;
    }
    private static Scrollbar CopyScrollbar(Transform parent)
    {
        Logger.Info("Retrieving GameSettingsTab from GameSettingMenu...");
        // GameSettingMenuからGameSettingsTabを取得
        var gameSettingMenu = GameObject.FindObjectOfType<GameSettingMenu>();
        var gameSettingsTab = gameSettingMenu.GameSettingsTab;
        if (gameSettingsTab == null)
        {
            Logger.Info("GameSettingsTab not found.");
            return null;
        }

        Logger.Info("Copying the entire GameSettingsTab...");
        // GameSettingsTab全体を一旦コピーする
        GameObject tabCopy = GameObject.Instantiate(gameSettingsTab.gameObject);
        tabCopy.transform.SetParent(parent, false);
        tabCopy.transform.localScale = Vector3.one * 5.3f;
        tabCopy.transform.localPosition = new Vector3(-30.7f, 12f, 0f);
        tabCopy.gameObject.SetActive(true);

        Logger.Info("Removing CloseButton from the copied tab...");
        // コピーしたタブからCloseButtonを破棄する
        var closeButton = tabCopy.transform.Find("CloseButton");
        if (closeButton != null)
        {
            GameObject.Destroy(closeButton.gameObject);
            Logger.Info("CloseButton has been destroyed.");
        }

        Logger.Info("Removing child elements from ScrollInner in the Scroller...");
        // Scroller内のScrollInnerの子要素を破棄する（forループで安全に処理）
        var scroller = tabCopy.transform.Find("Scroller");
        if (scroller != null)
        {
            var scrollInner = scroller.Find("SliderInner");
            if (scrollInner != null)
            {
                for (int i = scrollInner.childCount - 1; i >= 0; i--)
                {
                    GameObject.Destroy(scrollInner.GetChild(i).gameObject);
                }
                Logger.Info("All child elements in ScrollInner have been destroyed.");
            }
        }

        Logger.Info("Searching for Scrollbar component in the copied tab...");
        // コピーしたタブ内のScrollbarコンポーネントを探す
        Scrollbar foundScrollbar = tabCopy.GetComponentInChildren<Scrollbar>();
        Logger.Info("Scrollbar component has been retrieved.");
        return foundScrollbar;
    }
}
