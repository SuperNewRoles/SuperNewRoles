using HarmonyLib;
using SuperNewRoles.CustomCosmetics;
using SuperNewRoles.Modules;
using UnityEngine;

namespace SuperNewRoles.Patches;

[HarmonyPatch(typeof(PlayerVoteArea), nameof(PlayerVoteArea.SetCosmetics))]
public static class DarkLightPlayerVoteAreaPatch
{
    public static readonly Color DarkColor = new(0.4f, 0.4f, 0.4f, 1f);
    public static readonly Color LightColor = new(0.95f, 0.95f, 0.95f, 1f);

    public static void Postfix(PlayerVoteArea __instance, NetworkedPlayerInfo playerInfo)
    {
        ApplyToVoteArea(__instance, playerInfo);
    }

    public static void ApplyCurrentSettingToMeeting()
    {
        if (MeetingHud.Instance == null || MeetingHud.Instance.playerStates == null)
            return;

        foreach (PlayerVoteArea voteArea in MeetingHud.Instance.playerStates)
        {
            if (voteArea == null)
                continue;

            NetworkedPlayerInfo playerInfo = GameData.Instance?.GetPlayerById(voteArea.TargetPlayerId);
            ApplyToVoteArea(voteArea, playerInfo);
        }
    }

    private static void ApplyToVoteArea(PlayerVoteArea voteArea, NetworkedPlayerInfo playerInfo)
    {
        if (voteArea == null)
            return;

        SpriteRenderer darkLight = voteArea.gameObject.transform.Find("DarkLight")?.GetComponent<SpriteRenderer>();
        if (!ShouldShowDarkLight() || playerInfo == null)
        {
            if (darkLight != null)
                darkLight.gameObject.SetActive(false);
            return;
        }

        if (darkLight != null)
        {
            darkLight.color = CustomColors.IsLighter(playerInfo) ? LightColor : DarkColor;
            darkLight.gameObject.SetActive(true);
            return;
        }

        darkLight = new GameObject("DarkLight").AddComponent<SpriteRenderer>();
        darkLight.transform.SetParent(voteArea.gameObject.transform);
        darkLight.transform.localPosition = new Vector3(1.26f, -0.2f, -1f);
        darkLight.transform.localScale = Vector3.one * 0.35f;
        darkLight.gameObject.layer = 5;
        darkLight.color = CustomColors.IsLighter(playerInfo) ? LightColor : DarkColor;
        darkLight.sprite = AssetManager.GetAsset<Sprite>("BrightORDarkIcon.png");
        darkLight.gameObject.SetActive(true);
    }

    private static bool ShouldShowDarkLight()
    {
        return ConfigRoles.IsLightAndDarker == null || ConfigRoles.IsLightAndDarker.Value;
    }
}
