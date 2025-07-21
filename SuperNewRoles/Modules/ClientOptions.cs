using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace SuperNewRoles.Modules;

public static class ClientOptions
{
    public static void Open(Transform parent)
    {
        GameObject settingUIBG = AssetManager.Instantiate("SettingUIBase", parent);
        settingUIBG.transform.localPosition = new Vector3(0, 0, -100f);
        settingUIBG.transform.localScale = Vector3.one * 0.55f;
        settingUIBG.transform.localRotation = Quaternion.identity;
        PassiveButton blockBG = settingUIBG.gameObject.AddComponent<PassiveButton>();
        blockBG.Colliders = new Collider2D[] { settingUIBG.GetComponent<Collider2D>() };
        blockBG.OnClick = new();
        blockBG.OnMouseOut = new();
        blockBG.OnMouseOver = new();
        PassiveButton background = settingUIBG.transform.Find("Background").gameObject.AddComponent<PassiveButton>();
        background.Colliders = new Collider2D[] { settingUIBG.transform.Find("Background").gameObject.GetComponent<Collider2D>() };
        background.OnClick = new();
        background.OnClick.AddListener((UnityAction)(() =>
        {
            GameObject.Destroy(settingUIBG);
        }));
        background.OnMouseOut = new();
        background.OnMouseOut = new();
    }
}
