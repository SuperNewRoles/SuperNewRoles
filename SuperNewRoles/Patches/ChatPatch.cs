using System.Linq;
using HarmonyLib;
using SuperNewRoles.Replay;

namespace SuperNewRoles.Patches;

class Chat
{
    [HarmonyPatch(typeof(ChatController), nameof(ChatController.AddChatNote))]
    class ChatControllerAddChatNote
    {
        public static void Postfix(ChatController __instance, NetworkedPlayerInfo srcPlayer, ChatNoteTypes noteType)
        {
            if (noteType == ChatNoteTypes.DidVote) Recorder.OnVoteChat(srcPlayer);
        }
    }
    [HarmonyPatch(typeof(ChatBubble), nameof(ChatBubble.SetName))]
    public static class SetBubbleName
    {
        public static void Postfix(ChatBubble __instance, [HarmonyArgument(0)] string playerName)
        {
            //チャット欄でImpostor陣営から見たSpyがばれないように
            PlayerControl sourcePlayer = PlayerControl.AllPlayerControls.FirstOrDefault(x => x.Data.PlayerName == playerName);
            if (sourcePlayer != null && PlayerControl.LocalPlayer.IsImpostor() && sourcePlayer.IsRole(RoleId.Egoist, RoleId.Spy))
            {
                __instance.NameText.color = Palette.ImpostorRed;
            }
        }
    }
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    private static class EnableChat
    {
        // 参考 => https://github.com/yukinogatari/TheOtherRoles-GM/blob/gm-main/TheOtherRoles/Modules/ChatCommands.cs
        private static void Postfix(HudManager __instance)
        {
            if (__instance?.Chat?.isActiveAndEnabled == false && CanUseChat())
                __instance?.Chat?.SetVisible(true);
        }

        private static bool CanUseChat()
        {
            if (AmongUsClient.Instance.NetworkMode == NetworkModes.FreePlay) return true;
            if (ModHelpers.IsDebugMode() && CustomOptionHolder.CanUseChatWhenTaskPhase.GetBool()) return true;
            return false;
        }
    }
}