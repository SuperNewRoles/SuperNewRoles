using System.Linq;
using SuperNewRoles.Modules;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace SuperNewRoles.CustomOptions
{
    public class RoleOptionSettings
    {
        public static int GenCount = 50;
        public static float Rate = 4.5f;
        private static GameObject CreateOptionElement(Transform parent, CustomOption option, ref float lastY, string prefabName)
        {
            var optionPrefab = AssetManager.GetAsset<GameObject>(prefabName);
            var optionInstance = UnityEngine.Object.Instantiate(optionPrefab, parent);
            optionInstance.transform.localPosition = new Vector3(-0.22f, lastY, -5f);
            lastY -= 4.5f;
            optionInstance.transform.localScale = Vector3.one * 2;
            optionInstance.transform.Find("Text").GetComponent<TextMeshPro>().text = ModTranslation.GetString(option.Name);
            return optionInstance;
        }

        private static GameObject CreateCheckBox(Transform parent, CustomOption option, ref float lastY)
        {
            GameObject optionInstance = CreateOptionElement(parent, option, ref lastY, "Option_Check");
            var passiveButton = optionInstance.AddComponent<PassiveButton>();
            SpriteRenderer spriteRenderer = null;
            Transform checkMark = optionInstance.transform.Find("CheckMark");
            checkMark.gameObject.SetActive((bool)option.Value);
            passiveButton.Colliders = new Collider2D[1];
            passiveButton.Colliders[0] = optionInstance.GetComponent<BoxCollider2D>();
            passiveButton.OnClick = new();
            passiveButton.OnClick.AddListener((UnityAction)(() =>
            {
                Logger.Info("クリックされた");
                if (checkMark.gameObject.activeSelf)
                    checkMark.gameObject.SetActive(false);
                else
                    checkMark.gameObject.SetActive(true);
                if ((bool)option.Value)
                    // false
                    option.UpdateSelection(0);
                else
                    // true
                    option.UpdateSelection(1);
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

        private static GameObject CreateSelect(Transform parent, CustomOption option, ref float lastY)
        {
            GameObject optionInstance = CreateOptionElement(parent, option, ref lastY, "Option_Select");
            var selectedText = optionInstance.transform.Find("SelectedText").gameObject.GetComponent<TextMeshPro>();
            selectedText.text = FormatOptionValue(option.Selections[option.Selection], option);
            var passiveButton_Minus = optionInstance.transform.Find("Button_Minus").gameObject.AddComponent<PassiveButton>();
            SpriteRenderer spriteRenderer_Minus = passiveButton_Minus.GetComponent<SpriteRenderer>();
            passiveButton_Minus.OnClick = new();
            passiveButton_Minus.OnClick.AddListener((UnityAction)(() =>
            {
                if (option.Selection > 0)
                    option.UpdateSelection((byte)(option.Selection - 1));
                else
                    option.UpdateSelection((byte)(option.Selections.Length - 1));
                selectedText.text = FormatOptionValue(option.Selections[option.Selection], option);
                Logger.Info("マイナスボタンがクリックされた");
            }));
            passiveButton_Minus.OnMouseOver = new();
            passiveButton_Minus.OnMouseOver.AddListener((UnityAction)(() =>
            {
                spriteRenderer_Minus.color = new Color32(45, 235, 198, 255);
            }));
            passiveButton_Minus.OnMouseOut = new();
            passiveButton_Minus.OnMouseOut.AddListener((UnityAction)(() =>
            {
                spriteRenderer_Minus.color = Color.white;
            }));

            var passiveButton_Plus = optionInstance.transform.Find("Button_Plus").gameObject.AddComponent<PassiveButton>();
            SpriteRenderer spriteRenderer_Plus = passiveButton_Plus.GetComponent<SpriteRenderer>();
            passiveButton_Plus.OnClick = new();
            passiveButton_Plus.OnClick.AddListener((UnityAction)(() =>
            {
                if (option.Selection < option.Selections.Length - 1)
                    option.UpdateSelection((byte)(option.Selection + 1));
                else
                    option.UpdateSelection(0);
                selectedText.text = FormatOptionValue(option.Selections[option.Selection], option);
                Logger.Info("プラスボタンがクリックされた");
            }));
            passiveButton_Plus.OnMouseOver = new();
            passiveButton_Plus.OnMouseOver.AddListener((UnityAction)(() =>
            {
                spriteRenderer_Plus.color = new Color32(45, 235, 198, 255);
            }));
            passiveButton_Plus.OnMouseOut = new();
            passiveButton_Plus.OnMouseOut.AddListener((UnityAction)(() =>
            {
                spriteRenderer_Plus.color = Color.white;
            }));

            return optionInstance;
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

            scroller.ContentYBounds = new(0, 0);

            scroller.DragScrollSpeed = 3f;
            scroller.Colliders = new[] { RoleOptionMenu.RoleOptionMenuObjectData.MenuObject.transform.FindChild("Hitbox_Settings").GetComponent<BoxCollider2D>() };

            // ScrollerとInnerをRoleOptionMenuに保存
            RoleOptionMenu.RoleOptionMenuObjectData.SettingsScroller = scroller;
            RoleOptionMenu.RoleOptionMenuObjectData.SettingsInner = innerTransform;
            ClickedRole(RoleOptionManager.RoleOptions[0]);
        }
        public static void ClickedRole(RoleOptionManager.RoleOption roleOption)
        {
            float lastY = 4f;
            int index = 0;
            foreach (var option in roleOption.Options)
            {
                if (option.IsBooleanOption)
                    CreateCheckBox(RoleOptionMenu.RoleOptionMenuObjectData.SettingsInner, option, ref lastY);
                else
                    CreateSelect(RoleOptionMenu.RoleOptionMenuObjectData.SettingsInner, option, ref lastY);
                index++;
            }/*
            for (int i = 0; i < GenCount; i++)
            {
                // セレクトオプションの生成
                CreateSelect(RoleOptionMenu.RoleOptionMenuObjectData.SettingsInner, ref lastY);
                index++;
            }*/
            RoleOptionMenu.RoleOptionMenuObjectData.SettingsScroller.ContentYBounds.max = index < 5 ? 0f : (index - 4) * Rate;
        }

        private static string FormatOptionValue(object value, CustomOption option)
        {
            if (value is float floatValue)
            {
                var attribute = option.Attribute as CustomOptionFloatAttribute;
                if (attribute != null)
                {
                    float step = attribute.Step;
                    if (step >= 1f) return string.Format("{0:F0}", floatValue);
                    else if (step >= 0.1f) return string.Format("{0:F1}", floatValue);
                    else return string.Format("{0:F2}", floatValue);
                }
                return floatValue.ToString();
            }
            return value.ToString();
        }
    }
}