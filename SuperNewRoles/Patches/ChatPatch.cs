using System.Linq;
using HarmonyLib;
using SuperNewRoles.Replay;

namespace SuperNewRoles.Patches;

class Chat
{
    [HarmonyPatch(typeof(ChatController), nameof(ChatController.AddChatNote))]
    class ChatControllerAddChatNote {
        public static void Postfix(ChatController __instance, GameData.PlayerInfo srcPlayer, ChatNoteTypes noteType) {
            if (noteType == ChatNoteTypes.DidVote) Recorder.OnVoteChat(srcPlayer);
        }
    }
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