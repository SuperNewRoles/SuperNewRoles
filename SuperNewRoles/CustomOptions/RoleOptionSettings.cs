using SuperNewRoles.Modules;
using UnityEngine;

namespace SuperNewRoles.CustomOptions
{
    public class RoleOptionSettings
    {
        public static void GenerateScroll(Transform parent)
        {
            var select_check = AssetManager.GetAsset<GameObject>("Option_Check");
            GameObject obj = new("jtphsehejtrj");
            Scroller Scroller = new GameObject("SettingsScroller").AddComponent<Scroller>();
            Scroller.transform.SetParent(parent);
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
            select_check_instance.transform.localPosition = Vector3.zero; //new Vector3(17.74f, 0f, 0f);
            select_check_instance.transform.localScale = Vector3.one * 2;


        }
    }
}