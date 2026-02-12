using HarmonyLib;
using TMPro;
using UnityEngine;

namespace SuperNewRoles.Patches;

[HarmonyPatch
(typeof(GameContainer), nameof(GameContainer.SetupGameInfo))]
public static class GameContainerAwakePatch
{
    public static void Postfix(GameContainer __instance)
    {
        TextMeshPro RoomName = null;
        var roomnametext = __instance.transform.Find("Container/RoomNameText");
        if (roomnametext != null)
        {
            RoomName = roomnametext.GetComponent<TextMeshPro>();
        }
        else
        {
            RoomName = GameObject.Instantiate(__instance.capacity, __instance.transform.Find("Container"));
        }
        RoomName.text = __instance.gameListing.HostName;
        RoomName.name = "RoomNameText";
        RoomName.transform.localPosition = new Vector3(-0.92f, 0f, -4f);

        // parentのparent
        TextMeshPro RoomLanguage = null;
        var roomlanguagetext = __instance.transform.Find("Container/RoomLanguageText");
        if (roomlanguagetext != null)
        {
            RoomLanguage = roomlanguagetext.GetComponent<TextMeshPro>();
        }
        else
        {
            RoomLanguage = GameObject.Instantiate(__instance.capacity, __instance.transform.Find("Container"));
        }
        RoomLanguage.text = ModHelpers.GetCurrentLanguageName();
        RoomLanguage.name = "RoomLanguageText";
        RoomLanguage.transform.localPosition = new Vector3(2.86f, -0.14f, 0f);

        __instance.transform.Find("Container/Container").GetComponent<AspectPosition>().anchorPoint = new Vector2(0.775f, 0.521f);
    }
}