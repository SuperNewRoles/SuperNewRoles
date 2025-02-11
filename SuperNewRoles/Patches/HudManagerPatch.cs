using HarmonyLib;
using UnityEngine;
using InnerNet;

namespace SuperNewRoles.Patches;

[HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
public static class HudManagerPatch
{
    public static void Postfix()
    {
        WallHackUpdate();
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