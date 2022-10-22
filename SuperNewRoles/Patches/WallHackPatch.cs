using HarmonyLib;
using UnityEngine;

namespace SuperNewRoles.Patches
{
    class WallHack
    {
        public static void WallHackUpdate()
        {
            if (CachedPlayer.LocalPlayer.PlayerControl?.Collider?.offset != null)
            {
                if (Input.GetKeyDown(KeyCode.LeftControl))
                {
                    if (AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started ||
                    AmongUsClient.Instance.GameMode == GameModes.FreePlay)
                    {
                        CachedPlayer.LocalPlayer.PlayerControl.Collider.offset = new Vector2(0f, 127f);
                    }
                }
                //壁抜け解除
                if (CachedPlayer.LocalPlayer.PlayerControl.Collider.offset.y == 127f)
                {
                    if (!Input.GetKey(KeyCode.LeftControl) || AmongUsClient.Instance.IsGameStarted)
                    {
                        CachedPlayer.LocalPlayer.PlayerControl.Collider.offset = new Vector2(0f, -0.3636f);
                    }
                }
            }
        }
    }
}