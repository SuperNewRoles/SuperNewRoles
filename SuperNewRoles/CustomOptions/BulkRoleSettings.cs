using SuperNewRoles.Modules;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace SuperNewRoles.CustomOptions
{
    public static class BulkRoleSettings
    {
        public static void InitializeBulkRoleSettingsMenu()
        {
            // ロールの設定を初期化
        }
        public static void InitializeBulkRoleButton()
        {
            var obj = GameObject.Instantiate(AssetManager.GetAsset<GameObject>("BulkRoleButton"));
            obj.transform.SetParent(RoleOptionMenu.RoleOptionMenuObjectData.MenuObject.transform);
            obj.transform.localPosition = new Vector3(4.7f, 8.55f, -2f);
            obj.transform.localScale = Vector3.one * 2.15f;

            obj.transform.Find("Text").GetComponent<TextMeshPro>().text = ModTranslation.GetString("BulkRoleButton");
            var passiveButton = obj.AddComponent<PassiveButton>();
            passiveButton.Colliders = new Collider2D[1];
            passiveButton.Colliders[0] = obj.GetComponent<BoxCollider2D>();
            passiveButton.OnClick = new();
            GameObject SelectedObject = null;
            passiveButton.OnClick.AddListener((UnityAction)(() =>
            {
                RoleOptionMenu.RoleOptionMenuObjectData.MenuObject.SetActive(false);
                if (RoleOptionMenu.RoleOptionMenuObjectData.BulkRoleSettingsMenu == null)
                    InitializeBulkRoleSettingsMenu();
                else
                    RoleOptionMenu.RoleOptionMenuObjectData.BulkRoleSettingsMenu.SetActive(true);
            }));
            passiveButton.OnMouseOut = new();
            passiveButton.OnMouseOut.AddListener((UnityAction)(() =>
            {
                if (SelectedObject == null)
                    SelectedObject = obj.transform.FindChild("Selected").gameObject;
                SelectedObject.SetActive(false);
            }));
            passiveButton.OnMouseOver = new();
            passiveButton.OnMouseOver.AddListener((UnityAction)(() =>
            {
                if (SelectedObject == null)
                    SelectedObject = obj.transform.FindChild("Selected").gameObject;
                SelectedObject.SetActive(true);
            }));
        }
    }
}