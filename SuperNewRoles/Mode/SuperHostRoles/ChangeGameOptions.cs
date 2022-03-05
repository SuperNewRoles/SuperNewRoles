using HarmonyLib;
using Hazel;
using SuperNewRoles.Helpers;
using SuperNewRoles.Roles;
using System;
using System.Collections.Generic;
using System.Text;

namespace SuperNewRoles.Mode.SuperHostRoles
{
    
    class ChangeGameOptions
    {
        public static GameOptionsData DefaultGameOption;
        [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.StartGame))]
        public class StartGame
        {
            public static void Postfix()
            {
                DefaultGameOption = PlayerControl.GameOptions;
            }
        }
        public static class SelectRoleOptionChange {
            public static void RolesSelectOptionsChange()
            {
                foreach (PlayerControl p in PlayerControl.AllPlayerControls) {
                    if (p.isRole(CustomRPC.RoleId.Minimalist))
                    {
                        var NewOptions = DefaultGameOption;
                        NewOptions.KillCooldown = RoleClass.Minimalist.KillCoolTime;
                        RPCHelper.RPCGameOptionsPrivate(NewOptions,p);
                    }
                }
            }
        }
    }
}
