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
public static class ClientOptionsPatches
{
    [HarmonyPatch(typeof(OptionsMenuBehaviour), nameof(OptionsMenuBehaviour.Start))]
    public static class OptionsMenuBehaviourStartPatch
    {
        public static void Postfix(OptionsMenuBehaviour __instance)
        {
            return;
            float leftX = __instance.CensorChatButton.transform.localPosition.x;
            float rightX = __instance.StreamerModeButton.transform.localPosition.x;

            __instance.CensorChatButton.gameObject.SetActive(false);
            __instance.EnableFriendInvitesButton.gameObject.SetActive(false);
            __instance.ColorBlindButton.gameObject.SetActive(false);
            __instance.StreamerModeButton.gameObject.SetActive(false);
            float y = (__instance.CensorChatButton.transform.localPosition.y - 0.17f);

            // vanilla
            ToggleButtonBehaviour toggleButtonBehaviour = GameObject.Instantiate(__instance.CensorChatButton.gameObject, __instance.CensorChatButton.transform.parent).GetComponent<ToggleButtonBehaviour>();
            PassiveButton passiveButton = toggleButtonBehaviour.gameObject.GetComponent<PassiveButton>();
            passiveButton.OnClick = new();
            passiveButton.OnClick.AddListener((UnityAction)(() =>
            {
                Logger.Info("Vanilla");
                ClientOptions.Open(__instance.transform);
            }));
            passiveButton.transform.localPosition = new Vector3(leftX, y, 0);
            passiveButton.gameObject.SetActive(true);
            passiveButton.GetComponentInChildren<TextMeshPro>().text = ModTranslation.GetString("ClientOptionsVanilla");
            GameObject.Destroy(passiveButton.GetComponent<AspectPosition>());

            // Mod
            toggleButtonBehaviour = GameObject.Instantiate(__instance.CensorChatButton.gameObject, __instance.CensorChatButton.transform.parent).GetComponent<ToggleButtonBehaviour>();
            passiveButton = toggleButtonBehaviour.gameObject.GetComponent<PassiveButton>();
            passiveButton.OnClick = new();
            passiveButton.OnClick.AddListener((UnityAction)(() =>
            {
                Logger.Info("Mod");
                ClientOptions.Open(__instance.transform);
            }));
            passiveButton.transform.localPosition = new Vector3(rightX, y, 0);
            passiveButton.gameObject.SetActive(true);
            passiveButton.GetComponentInChildren<TextMeshPro>().text = ModTranslation.GetString("ClientOptionsMod");
            GameObject.Destroy(passiveButton.GetComponent<AspectPosition>());
        }
    }
}