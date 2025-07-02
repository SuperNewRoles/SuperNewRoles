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
            string formattedValue = "";
            string suffix = "";

            if (value is float floatValue)
            {
                var floatAttribute = option.Attribute as CustomOptionFloatAttribute;
                if (floatAttribute != null)
                {
                    float step = floatAttribute.Step;
                    // 数値がほぼ整数であれば、小数点を表示せずにフォーマットする
                    if (Mathf.Approximately(floatValue, Mathf.Round(floatValue)))
                        formattedValue = string.Format("{0:F0}", floatValue);
                    // stepが0.1以上の場合、ほぼ1桁の精度なら小数点1桁でフォーマットする
                    else if (step >= 0.1f && Mathf.Approximately(floatValue * 10f, Mathf.Round(floatValue * 10f)))
                        formattedValue = string.Format("{0:F1}", floatValue);
                    else
                        formattedValue = floatValue.ToString();

                    // Suffixが設定されている場合は翻訳して追加
                    if (!string.IsNullOrEmpty(floatAttribute.Suffix))
                    {
                        suffix = ModTranslation.GetString(floatAttribute.Suffix);
                    }
                }
                else
                {
                    formattedValue = floatValue.ToString();
                }
            }
            else if (value is int intValue)
            {
                var intAttribute = option.Attribute as CustomOptionIntAttribute;
                formattedValue = intValue.ToString();
                if (intAttribute != null && !string.IsNullOrEmpty(intAttribute.Suffix))
                {
                    suffix = ModTranslation.GetString(intAttribute.Suffix);
                }
            }
            else if (value is byte byteValue)
            {
                var byteAttribute = option.Attribute as CustomOptionByteAttribute;
                formattedValue = byteValue.ToString();
                if (byteAttribute != null && !string.IsNullOrEmpty(byteAttribute.Suffix))
                {
                    suffix = ModTranslation.GetString(byteAttribute.Suffix);
                }
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
            else
            {
                formattedValue = value.ToString();
            }

            return formattedValue + suffix;
        }
    }
}