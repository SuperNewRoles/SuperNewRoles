using System.Linq;
using HarmonyLib;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Modules;
using UnityEngine;
using UnityEngine.Events;

namespace SuperNewRoles.Patches;

[HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
public static class Debugger
{
    public static void Postfix()
    {
        // Shift Ctrl + D
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.D))
        {
            Logger.Info("Debugger Clicked");
            CustomOptionsMenu.ShowOptionsMenu();
            RoleOptionMenu.ShowRoleOptionMenu(RoleOptionMenuType.Crewmate);
            var select_check = AssetManager.GetAsset<GameObject>("Option_Check");
            GameObject obj = new("jtphsehejtrj");
            Scroller Scroller = new GameObject("SettingsScroller").AddComponent<Scroller>();
            Scroller.transform.SetParent(RoleOptionMenu.RoleOptionMenuObjectData.MenuObject.transform);
            Scroller.gameObject.layer = 5;

            Scroller.transform.localScale = Vector3.one;
            Scroller.transform.localPosition = new Vector3(18f, 0f, 0f);
            Scroller.allowX = false;
            Scroller.allowY = true;
            Scroller.active = true;
            Scroller.velocity = new Vector2(0, 0);
            Scroller.ScrollbarYBounds = new FloatRange(0, 0);
            Scroller.ContentXBounds = new FloatRange(0, 0);
            Scroller.enabled = true;

            Scroller.Inner = obj.transform;
            obj.transform.SetParent(Scroller.transform);
            obj.transform.localScale = Vector3.one;
            obj.transform.localPosition = Vector3.zero;
            var select_check_instance = UnityEngine.Object.Instantiate(select_check, obj.transform);
            select_check_instance.transform.localPosition = new Vector3(17.74f, 0f, 0f);
            select_check_instance.transform.localScale = Vector3.one * 2;
        }
    }
}

