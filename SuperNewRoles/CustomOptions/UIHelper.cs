using UnityEngine;
using UnityEngine.Events;
using TMPro;
using SuperNewRoles.Modules;
using System;

namespace SuperNewRoles.CustomOptions
{
    public static class UIHelper
    {
        public static class Constants
        {
            public const float DefaultScale = 1f;
            public const float DefaultZPosition = -0.21f;
            public static readonly Color32 HoverColor = new(45, 235, 198, 255);
        }

        public static GameObject InstantiateUIElement(string assetName, Transform parent, Vector3 localPosition, Vector3 localScale)
        {
            GameObject obj = GameObject.Instantiate(AssetManager.GetAsset<GameObject>(assetName), parent);
            obj.transform.localScale = localScale;
            obj.transform.localPosition = localPosition;
            return obj;
        }

        public static void ConfigurePassiveButton(PassiveButton button, UnityAction onClick, SpriteRenderer spriteRenderer = null, Color32? hoverColor = null, GameObject selectedObject = null)
        {
            button.OnClick = new();
            button.OnClick.AddListener(onClick);

            button.OnMouseOver = new UnityEvent();
            button.OnMouseOver.AddListener((UnityAction)(() =>
            {
                if (selectedObject != null)
                    selectedObject.SetActive(true);
                else if (spriteRenderer != null)
                    spriteRenderer.color = hoverColor ?? Constants.HoverColor;
            }));

            button.OnMouseOut = new UnityEvent();
            button.OnMouseOut.AddListener((UnityAction)(() =>
            {
                if (selectedObject != null)
                    selectedObject.SetActive(false);
                else if (spriteRenderer != null)
                    spriteRenderer.color = Color.white;
            }));
        }

        public static void SetText(GameObject obj, string text)
        {
            obj.transform.Find("Text")?.GetComponent<TMPro.TextMeshPro>()?.SetText(text);
        }

        public static string FormatOptionValue(object value, CustomOption option)
        {
            if (value is float floatValue)
            {
                var floatAttribute = option.Attribute as CustomOptionFloatAttribute;
                if (floatAttribute != null)
                {
                    float step = floatAttribute.Step;
                    if (step >= 1f) return string.Format("{0:F0}", floatValue);
                    else if (step >= 0.1f) return string.Format("{0:F1}", floatValue);
                    else return string.Format("{0:F2}", floatValue);
                }
                return floatValue.ToString();
            }
            else if (value is Enum enumValue)
            {
                var selectAttribute = option.Attribute as CustomOptionSelectAttribute;
                if (selectAttribute != null)
                {
                    return ModTranslation.GetString($"{selectAttribute.TranslationPrefix}{enumValue}");
                }
                return enumValue.ToString();
            }
            return value.ToString();
        }
    }
}