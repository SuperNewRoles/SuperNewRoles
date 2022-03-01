using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperNewRoles.Patch
{
    class Chat
    {
        [HarmonyPatch(typeof(ChatBubble), nameof(ChatBubble.SetName))]
        public static class SetBubbleName
        {
            public static void Postfix(ChatBubble __instance, [HarmonyArgument(0)] string playerName)
            {
                //チャット欄でImpostor陣営から見たSpyがばれないように
                PlayerControl sourcePlayer = PlayerControl.AllPlayerControls.ToArray().ToList().FirstOrDefault(x => x.Data.PlayerName.Equals(playerName));
                if (PlayerControl.LocalPlayer.Data.Role.IsImpostor && sourcePlayer.isRole(CustomRPC.RoleId.Egoist))
                {
                        __instance.NameText.color = Palette.ImpostorRed;
                }
            }
        }
    }
}
