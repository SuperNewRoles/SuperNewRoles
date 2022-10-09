using HarmonyLib;
using UnityEngine;

namespace SuperNewRoles.Patches
{
    class WallHack
    {
        public static void WallHackUpdate()
        {
            if (PlayerControl.LocalPlayer?.Collider?.offset != null)
            {
                if (Input.GetKeyDown(KeyCode.LeftControl))
                {
                    if (AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started ||
                    AmongUsClient.Instance.GameMode == GameModes.FreePlay)
                    {
                        PlayerControl.LocalPlayer.Collider.offset = new Vector2(0f, 127f);
                    }
                }
                //壁抜け解除
                if (PlayerControl.LocalPlayer.Collider.offset.y == 127f)
                {
                    if (!Input.GetKey(KeyCode.LeftControl) || AmongUsClient.Instance.IsGameStarted)
                    {
                        PlayerControl.LocalPlayer.Collider.offset = new Vector2(0f, -0.3636f);
                    }
                }
            }
        }
    }
}