using UnityEngine;

namespace SuperNewRoles.Patches
{
    class ChatPaste
    {
        [HarmonyLib.HarmonyPatch(typeof(KeyboardJoystick), nameof(KeyboardJoystick.Update))]
        class Pastepatch
        {
            static void Postfix()
            {
                if (FastDestroyableSingleton<HudManager>.Instance.Chat.IsOpen && SaveManager.chatModeType == 1)
                {
                    if (ModHelpers.GetManyKeyDown(new[] { KeyCode.LeftControl, KeyCode.V }))
                    {
                        FastDestroyableSingleton<HudManager>.Instance.Chat.TextArea.SetText(FastDestroyableSingleton<HudManager>.Instance.Chat.TextArea.text + GUIUtility.systemCopyBuffer);
                        FastDestroyableSingleton<HudManager>.Instance.Chat.quickChatMenu.ResetGlyphs();
                    }
                    if (ModHelpers.GetManyKeyDown(new[] { KeyCode.LeftControl, KeyCode.X }))
                    {
                        GUIUtility.systemCopyBuffer = FastDestroyableSingleton<HudManager>.Instance.Chat.TextArea.text;
                        FastDestroyableSingleton<HudManager>.Instance.Chat.TextArea.Clear();
                        FastDestroyableSingleton<HudManager>.Instance.Chat.quickChatMenu.ResetGlyphs();
                    }
                    if (ModHelpers.GetManyKeyDown(new[] { KeyCode.LeftControl, KeyCode.C }))
                    {
                        GUIUtility.systemCopyBuffer = FastDestroyableSingleton<HudManager>.Instance.Chat.TextArea.text;
                        FastDestroyableSingleton<HudManager>.Instance.Chat.quickChatMenu.ResetGlyphs();
                    }
                }
            }
        }
    }
}