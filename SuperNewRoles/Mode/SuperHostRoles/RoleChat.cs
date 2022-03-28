using InnerNet;
using System;
using System.Collections.Generic;
using System.Text;

namespace SuperNewRoles.Mode.SuperHostRoles
{
    class RoleChat
    {
        public static bool SendChat(ChatController __instance)
        {
            string text = __instance.TextArea.text;
            bool handled = false;
            if (AmongUsClient.Instance.GameState != InnerNetClient.GameStates.Started)
            {
                if (AmongUsClient.Instance.AmHost)
                {
                    if (text.ToLower().StartsWith("/help "))
                    {
                        handled = true;
                    }
                }
            }
            return handled;
        }
    }
}
