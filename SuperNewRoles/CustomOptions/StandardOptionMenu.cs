using SuperNewRoles.Modules;
using UnityEngine;

namespace SuperNewRoles.CustomOptions;
public static class StandardOptionMenu
{
    public static void ShowStandardOptionMenu()
    {
        if (RoleOptionMenu.RoleOptionMenuObjectData?.StandardOptionMenu == null)
            Initialize();
        RoleOptionMenu.RoleOptionMenuObjectData.StandardOptionMenu.SetActive(true);
    }
    public static void Initialize()
    {
        var obj = GameObject.Instantiate(AssetManager.GetAsset<GameObject>("StandardOptionMenu"), RoleOptionMenu.GetGameSettingMenu().transform);
        obj.transform.localScale = Vector3.one;
        obj.transform.localPosition = Vector3.zero;
        RoleOptionMenu.RoleOptionMenuObjectData.StandardOptionMenu = obj;
    }
}