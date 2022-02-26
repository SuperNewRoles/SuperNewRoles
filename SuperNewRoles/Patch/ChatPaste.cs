using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SuperNewRoles.Patch
{
    class ChatPaste
    {
        [HarmonyLib.HarmonyPatch(typeof(AmongUsClient),nameof(AmongUsClient.Update))]
        class pastepatch
        {
            static void Postfix()
            {
                if (HudManager.Instance.Chat.IsOpen)
                {
                    if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.V))
                    { 
                        HudManager.Instance.Chat.TextArea.text = HudManager.Instance.Chat.TextArea.text + ClipboardHelper.GetClipboardString();
                    }
                }
            }
        }
    }
}
