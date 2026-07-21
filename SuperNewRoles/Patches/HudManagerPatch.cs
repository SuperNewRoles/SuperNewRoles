using HarmonyLib;
using UnityEngine;
using InnerNet;
using SuperNewRoles.Modules;
using SuperNewRoles.CustomOptions.Categories;
using TMPro;

namespace SuperNewRoles.Patches;

[HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
public static class HudManagerPatch
{
    private static TextMeshPro NoGameEndText;

    public static void Postfix(HudManager __instance)
    {
        WallHackUpdate();
        UpdateNoGameEndText(__instance);
    }

    public static void CreateNoGameEndText(HudManager hudManager)
    {
        if (hudManager?.roomTracker?.text == null)
            return;

        NoGameEndText = Object.Instantiate(hudManager.roomTracker.text, hudManager.transform);
        NoGameEndText.name = "NoGameEndStatusText";
        NoGameEndText.alignment = TextAlignmentOptions.TopRight;
        NoGameEndText.transform.localScale = Vector3.one * 1f;
        NoGameEndText.SetText($"<color=#ff4b4b>{ModTranslation.GetString("NoGameEndHudText")}</color>");

        GameObject.Destroy(NoGameEndText.GetComponent<RoomTracker>());

        AspectPosition aspectPosition = NoGameEndText.gameObject.AddComponent<AspectPosition>();
        aspectPosition.Alignment = AspectPosition.EdgeAlignments.RightTop;
        aspectPosition.DistanceFromEdge = new(2.96f, 1f, -20f);
        aspectPosition.OnEnable();
        NoGameEndText.gameObject.SetActive(false);
    }

    private static void UpdateNoGameEndText(HudManager hudManager)
    {
        if (NoGameEndText == null)
            CreateNoGameEndText(hudManager);
        if (NoGameEndText == null)
            return;

        bool visible = AmongUsClient.Instance != null
            && AmongUsClient.Instance.GameState == InnerNetClient.GameStates.Started
            && CustomOptionManager.DebugMode
            && CustomOptionManager.DebugModeNoGameEnd;
        NoGameEndText.gameObject.SetActive(visible);
    }
    public static void WallHackUpdate()
    {
        if (PlayerControl.LocalPlayer == null ||
            PlayerControl.LocalPlayer.Collider == null)
            return;
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            if (AmongUsClient.Instance.GameState != InnerNetClient.GameStates.Started ||
            AmongUsClient.Instance.NetworkMode == NetworkModes.FreePlay)
            {
                PlayerControl.LocalPlayer.Collider.offset = new Vector2(0f, 127f);
            }
        }
        //壁抜け解除
        else if (PlayerControl.LocalPlayer.Collider.offset.y == 127f)
        {
            if (!Input.GetKey(KeyCode.LeftControl) || AmongUsClient.Instance.IsGameStarted)
            {
                PlayerControl.LocalPlayer.Collider.offset = new Vector2(0f, -0.3636f);
            }
        }
    }
}

[HarmonyPatch(typeof(HudManager), nameof(HudManager.Start))]
public static class NoGameEndHudManagerStartPatch
{
    public static void Postfix(HudManager __instance)
        => HudManagerPatch.CreateNoGameEndText(__instance);
}
[HarmonyCoroutinePatch(typeof(HudManager), nameof(HudManager.CoShowIntro))]
public static class HudManagerShowIntroPatch
{
    public static void Postfix(ref bool __result)
    {
        if (!__result && ModHelpers.IsHnS())
        {
            Logger.Info("HudManagerShowIntroPatch");
            ExPlayerControl.LocalPlayer.SetKillTimerUnchecked(1f, 1f);
        }
    }
}
