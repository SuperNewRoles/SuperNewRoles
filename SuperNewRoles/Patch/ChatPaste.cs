using UnityEngine;
namespace SuperNewRoles.Patch
{
    class ChatPaste
    {
        [HarmonyLib.HarmonyPatch(typeof(KeyboardJoystick),nameof(KeyboardJoystick.Update))]
        class pastepatch
        {
            static void Postfix()
            {
                if (FastDestroyableSingleton<HudManager>.Instance.Chat.IsOpen && SaveManager.chatModeType == 1)
                {
                    if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.V))
                    {
                        FastDestroyableSingleton<HudManager>.Instance.Chat.TextArea.SetText(FastDestroyableSingleton<HudManager>.Instance.Chat.TextArea.text + GUIUtility.systemCopyBuffer);
                        FastDestroyableSingleton<HudManager>.Instance.Chat.quickChatMenu.ResetGlyphs();
                    }
                    if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.X))
                    {
                        GUIUtility.systemCopyBuffer = FastDestroyableSingleton<HudManager>.Instance.Chat.TextArea.text;
                        FastDestroyableSingleton<HudManager>.Instance.Chat.TextArea.Clear();
                        FastDestroyableSingleton<HudManager>.Instance.Chat.quickChatMenu.ResetGlyphs();
                    }
                    if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.C))
                    {
                        GUIUtility.systemCopyBuffer = FastDestroyableSingleton<HudManager>.Instance.Chat.TextArea.text;
                        FastDestroyableSingleton<HudManager>.Instance.Chat.quickChatMenu.ResetGlyphs();
                    }
                }
            }
        }
    }
}
