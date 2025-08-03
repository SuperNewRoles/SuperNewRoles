using UnityEngine;
using UnityEngine.Events;
using TMPro;
using SuperNewRoles.Modules;

namespace SuperNewRoles.CustomOptions
{
    public abstract class UIComponentBase
    {
        protected static class UIConstants
        {
            public const float DefaultScale = 1f;
            public const float DefaultZPosition = -0.21f;
            public static readonly Color32 HoverColor = new(45, 235, 198, 255);
        }

        protected static GameObject InstantiateUIElement(string assetName, Transform parent, Vector3 localPosition, Vector3 localScale)
        {
            GameObject obj = GameObject.Instantiate(AssetManager.GetAsset<GameObject>(assetName), parent);
            obj.transform.localScale = localScale;
            obj.transform.localPosition = localPosition;
            return obj;
        }

        protected static void ConfigurePassiveButton(PassiveButton button, UnityAction onClick, SpriteRenderer spriteRenderer = null)
        {
            button.OnClick = new();
            button.OnClick.AddListener(onClick);

            button.OnMouseOver = new UnityEvent();
            button.OnMouseOver.AddListener((UnityAction)(() =>
            {
                if (spriteRenderer != null)
                    spriteRenderer.color = UIConstants.HoverColor;
            }));

            button.OnMouseOut = new UnityEvent();
            button.OnMouseOut.AddListener((UnityAction)(() =>
            {
                if (spriteRenderer != null)
                    spriteRenderer.color = Color.white;
            }));
        }

        protected static void SetText(GameObject obj, string text)
        {
            obj.transform.Find("Text")?.GetComponent<TMPro.TextMeshPro>()?.SetText(text);
        }

        protected static string FormatOptionValue(object value, CustomOption option)
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