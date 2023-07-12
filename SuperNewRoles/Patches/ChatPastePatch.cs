using UnityEngine;
using AmongUs.QuickChat;

namespace SuperNewRoles.Patches;

class ChatPaste
{
    [HarmonyLib.HarmonyPatch(typeof(KeyboardJoystick), nameof(KeyboardJoystick.Update))]
    class Pastepatch
    {
        static void Postfix()
        {
            if (FastDestroyableSingleton<HudManager>.Instance.Chat.IsOpenOrOpening)
            {
                if (ModHelpers.GetManyKeyDown(new[] { KeyCode.LeftControl, KeyCode.V }))
                {
                    FastDestroyableSingleton<FreeChatInputField>.Instance.textArea.SetText(FastDestroyableSingleton<FreeChatInputField>.Instance.textArea.text + GUIUtility.systemCopyBuffer);
                    FastDestroyableSingleton<HudManager>.Instance.Chat.quickChatMenu.ResetGlyphs();
                }
                if (ModHelpers.GetManyKeyDown(new[] { KeyCode.LeftControl, KeyCode.X }))
                {
                    GUIUtility.systemCopyBuffer = FastDestroyableSingleton<FreeChatInputField>.Instance.textArea.text;
                    FastDestroyableSingleton<FreeChatInputField>.Instance.textArea.Clear();
                    FastDestroyableSingleton<HudManager>.Instance.Chat.quickChatMenu.ResetGlyphs();
                    FastDestroyableSingleton<HudManager>.Instance.TextBoxTMP.sendButtonGlyph.enabled = false;
                }
                if (ModHelpers.GetManyKeyDown(new[] { KeyCode.LeftControl, KeyCode.C }))
                {
                    GUIUtility.systemCopyBuffer = FastDestroyableSingleton<FreeChatInputField>.Instance.textArea.text;
                    FastDestroyableSingleton<HudManager>.Instance.Chat.quickChatMenu.ResetGlyphs();
                }
            }
        }
    }
}