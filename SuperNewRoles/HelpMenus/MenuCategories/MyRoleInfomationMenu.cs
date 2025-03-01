using SuperNewRoles.Modules;
using UnityEngine;

namespace SuperNewRoles.HelpMenus.MenuCategories;

public class MyRoleInfomationMenu : HelpMenuCategoryBase
{
    public override string Name => "MyRoleInfomation";
    private GameObject Container;
    private GameObject MenuObject;
    public override void Show(GameObject Container)
    {
        if (MenuObject != null)
            GameObject.Destroy(MenuObject);

        this.Container = Container;
        // MyRoleInfomationHelpMenu from AssetManager
        MenuObject = GameObject.Instantiate(AssetManager.GetAsset<GameObject>("MyRoleInfomationHelpMenu"), Container.transform);
        MenuObject.transform.localPosition = Vector3.zero;
        MenuObject.transform.localScale = Vector3.one;
        MenuObject.transform.localRotation = Quaternion.identity;
    }
    public override void Hide(GameObject Container)
    {
        // メニューを非表示にする処理を記述
        if (MenuObject != null)
            GameObject.Destroy(MenuObject);
    }
}
