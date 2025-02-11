using SuperNewRoles.Modules;
using UnityEngine;
using UnityEngine.Events;

namespace SuperNewRoles.CustomOptions
{
    public class RoleOptionSettings
    {
        public static int GenCount = 50;
        public static float Rate = 4.5f;
        private static GameObject CreateOptionElement(Transform parent, ref float lastY, string prefabName)
        {
            var optionPrefab = AssetManager.GetAsset<GameObject>(prefabName);
            var optionInstance = UnityEngine.Object.Instantiate(optionPrefab, parent);
            optionInstance.transform.localPosition = new Vector3(-0.22f, lastY, 0);
            lastY -= 4.5f;
            optionInstance.transform.localScale = Vector3.one * 2;
            return optionInstance;
        }

        private static GameObject CreateCheckBox(Transform parent, ref float lastY)
        {
            GameObject optionInstance = CreateOptionElement(parent, ref lastY, "Option_Check");
            var passiveButton = optionInstance.AddComponent<PassiveButton>();
            SpriteRenderer spriteRenderer = null;
            Transform checkMark = optionInstance.transform.FindChild("CheckMark");
            passiveButton.Colliders = new Collider2D[1];
            passiveButton.Colliders[0] = optionInstance.GetComponent<BoxCollider2D>();
            passiveButton.OnClick = new();
            passiveButton.OnClick.AddListener((UnityAction)(() =>
            {
                if (checkMark.gameObject.activeSelf)
                    checkMark.gameObject.SetActive(false);
                else
                    checkMark.gameObject.SetActive(true);
            }));
            passiveButton.OnMouseOver = new();
            passiveButton.OnMouseOver.AddListener((UnityAction)(() =>
            {
                if (spriteRenderer == null)
                    spriteRenderer = optionInstance.GetComponent<SpriteRenderer>();
                spriteRenderer.color = new Color32(45, 235, 198, 255);
            }));
            passiveButton.OnMouseOut = new();
            passiveButton.OnMouseOut.AddListener((UnityAction)(() =>
            {
                if (spriteRenderer == null)
                    spriteRenderer = optionInstance.GetComponent<SpriteRenderer>();
                spriteRenderer.color = Color.white;
            }));

            return optionInstance;
        }

        private static GameObject CreateSelect(Transform parent, ref float lastY)
        {
            return CreateOptionElement(parent, ref lastY, "Option_Select");
        }

        public static void GenerateScroll(Transform parent)
        {
            // Scrollerオブジェクトの生成と初期設定
            GameObject scrollerObject = new("SettingsScroller");
            Scroller scroller = scrollerObject.AddComponent<Scroller>();
            scroller.transform.SetParent(parent);
            scroller.gameObject.layer = 5; // UIレイヤー
            scroller.transform.localScale = Vector3.one;
            scroller.transform.localPosition = new Vector3(18f, 0f, 0f); // 親からの相対位置
            scroller.allowX = false;
            scroller.allowY = true;
            scroller.active = true; // Scrollerを有効にする
            // 初期速度は0
            scroller.velocity = Vector2.zero;
            // スクロール範囲は後で設定される可能性があるので、ここでは初期値として0を設定
            scroller.ContentXBounds = new FloatRange(0, 0);
            scroller.enabled = true;

            // ScrollerのInner Transformを設定
            Transform innerTransform = new GameObject("InnerContent").transform;
            innerTransform.SetParent(scroller.transform);
            innerTransform.localScale = Vector3.one;
            innerTransform.localPosition = Vector3.zero;
            scroller.Inner = innerTransform;
            float lastY = 4f;
            int index = 0;
            for (int i = 0; i < GenCount; i++)
            {
                // チェックボックスの生成
                CreateCheckBox(innerTransform, ref lastY);
                index++;
            }
            for (int i = 0; i < GenCount; i++)
            {
                // セレクトオプションの生成
                CreateSelect(innerTransform, ref lastY);
                index++;
            }

            scroller.ContentYBounds = new(0, 0);
            scroller.ContentYBounds.max = index < 5 ? 0f : (index - 4) * Rate;

            scroller.DragScrollSpeed = 3f;
            scroller.Colliders = new[] { RoleOptionMenu.RoleOptionMenuObjectData.MenuObject.transform.FindChild("Hitbox_Settings").GetComponent<BoxCollider2D>() };

            // ScrollerとInnerをRoleOptionMenuに保存
            RoleOptionMenu.RoleOptionMenuObjectData.SettingsScroller = scroller;
            RoleOptionMenu.RoleOptionMenuObjectData.SettingsInner = innerTransform;
        }
    }
}