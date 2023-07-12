using UnityEngine;

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
                    FastDestroyableSingleton<HudManager>.Instance.Chat.freeChatField.textArea.SetText(FastDestroyableSingleton<HudManager>.Instance.Chat.freeChatField.textArea.text + GUIUtility.systemCopyBuffer);
                }
                if (ModHelpers.GetManyKeyDown(new[] { KeyCode.LeftControl, KeyCode.X }))
                {
                    GUIUtility.systemCopyBuffer = FastDestroyableSingleton<HudManager>.Instance.Chat.freeChatField.textArea.text;
                    FastDestroyableSingleton<HudManager>.Instance.Chat.freeChatField.textArea.Clear();
                }
                if (ModHelpers.GetManyKeyDown(new[] { KeyCode.LeftControl, KeyCode.C }))
                {
                    GUIUtility.systemCopyBuffer = FastDestroyableSingleton<HudManager>.Instance.Chat.freeChatField.textArea.text;
                }
            }
        }
    }
}