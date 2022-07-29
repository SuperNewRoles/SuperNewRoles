using System.Linq;
using HarmonyLib;
using SuperNewRoles.CustomRPC;

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
                PlayerControl sourcePlayer = CachedPlayer.AllPlayers.ToArray().ToList().FirstOrDefault(x => x.Data.PlayerName.Equals(playerName));
                if (sourcePlayer != null && CachedPlayer.LocalPlayer.PlayerControl.IsImpostor() && sourcePlayer.IsRole(RoleId.Egoist, RoleId.Spy))
                {
                    __instance.NameText.color = Palette.ImpostorRed;
                }
            }
        }
    }
}