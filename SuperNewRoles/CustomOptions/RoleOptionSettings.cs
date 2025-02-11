using System;
using System.Linq;
using SuperNewRoles.Modules;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace SuperNewRoles.CustomOptions
{
    public class RoleOptionSettings
    {
        private const int GenCount = 50;
        private const float DefaultRate = 4.5f;
        private const float DefaultLastY = 4f;
        private const float ElementXPosition = -0.22f;
        private const float ElementZPosition = -5f;
        private const float ElementScale = 2f;
        private const float ScrollerXPosition = 18f;

        private static readonly Color32 HoverColor = new(45, 235, 198, 255);

        private static GameObject CreateOptionElement(Transform parent, string optionName, ref float lastY, string prefabName)
        {
            var optionPrefab = AssetManager.GetAsset<GameObject>(prefabName);
            var optionInstance = UnityEngine.Object.Instantiate(optionPrefab, parent);
            optionInstance.transform.localPosition = new Vector3(ElementXPosition, lastY, ElementZPosition);
            lastY -= DefaultRate;
            optionInstance.transform.localScale = Vector3.one * ElementScale;
            optionInstance.transform.Find("Text").GetComponent<TextMeshPro>().text = optionName;
            return optionInstance;
        }

        private static void ConfigurePassiveButton(PassiveButton button, Action onClick, SpriteRenderer spriteRenderer = null)
        {
            button.OnClick = new();
            button.OnClick.AddListener(onClick);
            ConfigureButtonHoverEffects(button, spriteRenderer);
        }

        private static void ConfigureButtonHoverEffects(PassiveButton button, SpriteRenderer spriteRenderer)
        {
            button.OnMouseOver = new();
            button.OnMouseOver.AddListener((UnityAction)(() =>
            {
                if (spriteRenderer != null)
                    spriteRenderer.color = HoverColor;
            }));

            button.OnMouseOut = new();
            button.OnMouseOut.AddListener((UnityAction)(() =>
            {
                if (spriteRenderer != null)
                    spriteRenderer.color = Color.white;
            }));
        }

        private static GameObject CreateCheckBox(Transform parent, CustomOption option, ref float lastY)
        {
            GameObject optionInstance = CreateOptionElement(parent, ModTranslation.GetString(option.Name), ref lastY, "Option_Check");
            var passiveButton = optionInstance.AddComponent<PassiveButton>();
            var checkMark = optionInstance.transform.Find("CheckMark");

            SetupCheckBoxButton(passiveButton, checkMark, option);
            passiveButton.Colliders = new[] { optionInstance.GetComponent<BoxCollider2D>() };

            return optionInstance;
        }

        private static void SetupCheckBoxButton(PassiveButton passiveButton, Transform checkMark, CustomOption option)
        {
            checkMark.gameObject.SetActive((bool)option.Value);
            var spriteRenderer = passiveButton.GetComponent<SpriteRenderer>();

            ConfigurePassiveButton(passiveButton, () =>
            {
                bool newValue = !checkMark.gameObject.activeSelf;
                checkMark.gameObject.SetActive(newValue);
                option.UpdateSelection(newValue ? (byte)1 : (byte)0);
            }, spriteRenderer);
        }

        private static GameObject CreateSelect(Transform parent, CustomOption option, ref float lastY)
        {
            GameObject optionInstance = CreateOptionElement(parent, ModTranslation.GetString(option.Name), ref lastY, "Option_Select");
            var selectedText = optionInstance.transform.Find("SelectedText").GetComponent<TextMeshPro>();
            selectedText.text = FormatOptionValue(option.Selections[option.Selection], option);

            SetupSelectButtons(optionInstance, selectedText, option);

            return optionInstance;
        }
        private static GameObject CreateNumberOfCrewsSelect(Transform parent, RoleOptionManager.RoleOption roleOption, ref float lastY)
        {
            GameObject optionInstance = CreateOptionElement(parent, ModTranslation.GetString("NumberOfCrews"), ref lastY, "Option_Select");
            var selectedText = optionInstance.transform.Find("SelectedText").GetComponent<TextMeshPro>();
            selectedText.text = ModTranslation.GetString("NumberOfCrewsSelected", roleOption.NumberOfCrews);

            var minusButton = optionInstance.transform.Find("Button_Minus").gameObject;
            var minusPassiveButton = minusButton.AddComponent<PassiveButton>();
            var minusSpriteRenderer = minusPassiveButton.GetComponent<SpriteRenderer>();
            bool isExist = RoleOptionMenu.RoleOptionMenuObjectData.RoleDetailButtonDictionary.TryGetValue(roleOption.RoleId, out var roleDetailButton);
            ConfigurePassiveButton(minusPassiveButton, () =>
            {
                byte playerCount = (byte)PlayerControl.AllPlayerControls.Count;
                if (15 >= playerCount)
                    playerCount = 15;
                roleOption.NumberOfCrews--;
                if (roleOption.NumberOfCrews < 0)
                    roleOption.NumberOfCrews = playerCount;
                selectedText.text = ModTranslation.GetString("NumberOfCrewsSelected", roleOption.NumberOfCrews);
                if (isExist)
                    RoleOptionMenu.UpdateRoleDetailButtonColor(roleDetailButton.GetComponent<SpriteRenderer>(), roleOption);
            }, minusSpriteRenderer);

            var plusButton = optionInstance.transform.Find("Button_Plus").gameObject;
            var plusPassiveButton = plusButton.AddComponent<PassiveButton>();
            var plusSpriteRenderer = plusPassiveButton.GetComponent<SpriteRenderer>();

            ConfigurePassiveButton(plusPassiveButton, () =>
            {
                byte playerCount = (byte)PlayerControl.AllPlayerControls.Count;
                if (15 >= playerCount)
                    playerCount = 15;
                roleOption.NumberOfCrews++;
                if (roleOption.NumberOfCrews > playerCount)
                    roleOption.NumberOfCrews = 0;
                selectedText.text = ModTranslation.GetString("NumberOfCrewsSelected", roleOption.NumberOfCrews);
                if (isExist)
                    RoleOptionMenu.UpdateRoleDetailButtonColor(roleDetailButton.GetComponent<SpriteRenderer>(), roleOption);
            }, plusSpriteRenderer);
            return optionInstance;
        }

        private static void SetupSelectButtons(GameObject optionInstance, TextMeshPro selectedText, CustomOption option)
        {
            SetupMinusButton(optionInstance, selectedText, option);
            SetupPlusButton(optionInstance, selectedText, option);
        }

        private static void SetupMinusButton(GameObject optionInstance, TextMeshPro selectedText, CustomOption option)
        {
            var minusButton = optionInstance.transform.Find("Button_Minus").gameObject;
            var passiveButton = minusButton.AddComponent<PassiveButton>();
            var spriteRenderer = passiveButton.GetComponent<SpriteRenderer>();

            ConfigurePassiveButton(passiveButton, () =>
            {
                byte newSelection = option.Selection > 0 ? (byte)(option.Selection - 1) : (byte)(option.Selections.Length - 1);
                UpdateOptionSelection(option, newSelection, selectedText);
            }, spriteRenderer);
        }

        private static void SetupPlusButton(GameObject optionInstance, TextMeshPro selectedText, CustomOption option)
        {
            var plusButton = optionInstance.transform.Find("Button_Plus").gameObject;
            var passiveButton = plusButton.AddComponent<PassiveButton>();
            var spriteRenderer = passiveButton.GetComponent<SpriteRenderer>();

            ConfigurePassiveButton(passiveButton, () =>
            {
                byte newSelection = option.Selection < option.Selections.Length - 1 ? (byte)(option.Selection + 1) : (byte)0;
                UpdateOptionSelection(option, newSelection, selectedText);
            }, spriteRenderer);
        }

        private static void UpdateOptionSelection(CustomOption option, byte newSelection, TextMeshPro selectedText)
        {
            option.UpdateSelection(newSelection);
            selectedText.text = FormatOptionValue(option.Selections[option.Selection], option);
        }

        public static void GenerateScroll(Transform parent)
        {
            var scrollerObject = CreateScrollerObject(parent);
            var innerTransform = CreateInnerTransform(scrollerObject.transform);
            ConfigureScroller(scrollerObject.GetComponent<Scroller>(), innerTransform);
        }

        private static GameObject CreateScrollerObject(Transform parent)
        {
            var scrollerObject = new GameObject("SettingsScroller");
            var scroller = scrollerObject.AddComponent<Scroller>();
            scroller.transform.SetParent(parent);
            scroller.gameObject.layer = 5;
            scroller.transform.localScale = Vector3.one;
            scroller.transform.localPosition = new Vector3(ScrollerXPosition, 0f, 0f);

            return scrollerObject;
        }

        private static Transform CreateInnerTransform(Transform scrollerTransform)
        {
            var innerTransform = new GameObject("InnerContent").transform;
            innerTransform.SetParent(scrollerTransform);
            innerTransform.localScale = Vector3.one;
            innerTransform.localPosition = Vector3.zero;
            return innerTransform;
        }

        private static void ConfigureScroller(Scroller scroller, Transform innerTransform)
        {
            scroller.allowX = false;
            scroller.allowY = true;
            scroller.active = true;
            scroller.velocity = Vector2.zero;
            scroller.ContentXBounds = new FloatRange(0, 0);
            scroller.ContentYBounds = new FloatRange(0, 0);
            scroller.enabled = true;
            scroller.Inner = innerTransform;
            scroller.DragScrollSpeed = 3f;
            scroller.Colliders = new[] { RoleOptionMenu.RoleOptionMenuObjectData.MenuObject.transform.FindChild("Hitbox_Settings").GetComponent<BoxCollider2D>() };

            RoleOptionMenu.RoleOptionMenuObjectData.SettingsScroller = scroller;
            RoleOptionMenu.RoleOptionMenuObjectData.SettingsInner = innerTransform;
        }

        public static void ClickedRole(RoleOptionManager.RoleOption roleOption)
        {
            float lastY = DefaultLastY;
            // parentを破棄
            var parent = RoleOptionMenu.RoleOptionMenuObjectData.SettingsInner.Find("Parent");
            if (parent != null)
                GameObject.Destroy(parent.gameObject);
            int index = CreateRoleOptions(roleOption, ref lastY);
            UpdateScrollerBounds(index);
        }

        private static int CreateRoleOptions(RoleOptionManager.RoleOption roleOption, ref float lastY)
        {
            int index = 0;
            var parent = new GameObject("Parent");
            parent.transform.SetParent(RoleOptionMenu.RoleOptionMenuObjectData.SettingsInner);
            parent.transform.localScale = Vector3.one;
            parent.transform.localPosition = Vector3.zero;
            parent.layer = 5;
            CreateNumberOfCrewsSelect(parent.transform, roleOption, ref lastY);
            foreach (var option in roleOption.Options)
            {
                if (option.IsBooleanOption)
                    CreateCheckBox(parent.transform, option, ref lastY);
                else
                    CreateSelect(parent.transform, option, ref lastY);
                index++;
            }
            return index;
        }

        private static void UpdateScrollerBounds(int index)
        {
            RoleOptionMenu.RoleOptionMenuObjectData.SettingsScroller.ContentYBounds.max =
                index < 5 ? 0f : (index - 4) * DefaultRate;
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