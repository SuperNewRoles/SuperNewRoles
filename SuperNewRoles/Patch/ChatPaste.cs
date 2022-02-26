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
                if (HudManager.Instance.Chat.IsOpen)
                {
                    
                    SuperNewRolesPlugin.Logger.LogInfo(HudManager.Instance.Chat.TextArea.text);
                    if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.V))
                    {
                        HudManager.Instance.Chat.TextArea.text = HudManager.Instance.Chat.TextArea.text + GUIUtility.systemCopyBuffer;
                    }
                    if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.X))
                    {
                        GUIUtility.systemCopyBuffer = HudManager.Instance.Chat.TextArea.text;
                        HudManager.Instance.Chat.TextArea.text = "";
                    }
                    if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.C))
                    {
                        GUIUtility.systemCopyBuffer = HudManager.Instance.Chat.TextArea.text;
                    }
                }
            }
        }
    }
}
