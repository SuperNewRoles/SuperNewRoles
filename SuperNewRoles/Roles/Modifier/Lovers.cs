using System;
using HarmonyLib;

using SuperNewRoles.Helpers;
using SuperNewRoles.Mode;

namespace SuperNewRoles.Roles
{
    class Lovers
    {
        [HarmonyPatch(typeof(GameData), nameof(GameData.HandleDisconnect), new Type[] { typeof(PlayerControl), typeof(DisconnectReasons) })]
        class HandleDisconnectPatch
        {
            public static void Postfix(PlayerControl player)
            {
                if (AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Started)
                {
                    if (player.IsLovers())
                    {
                        RoleClass.Lovers.LoversPlayer.RemoveAll(x => x.TrueForAll(x2 => x2.PlayerId == player.PlayerId));
                        ChacheManager.ResetLoversChache();
                    }
                    if (player.IsQuarreled() && player.IsAlive())
                    {
                        RoleClass.Quarreled.QuarreledPlayer.RemoveAll(x => x.TrueForAll(x2 => x2.PlayerId == player.PlayerId));
                        ChacheManager.ResetQuarreledChache();
                    }
                    if (ModeHandler.IsMode(ModeId.Default))
                    {
                        if (player.IsRole(RoleId.SideKiller))
                        {
                            var sideplayer = RoleClass.SideKiller.GetSidePlayer(PlayerControl.LocalPlayer);
                            if (sideplayer != null)
                            {
                                if (!RoleClass.SideKiller.IsUpMadKiller)
                                {
                                    sideplayer.RPCSetRoleUnchecked(RoleTypes.Impostor);
                                    RoleClass.SideKiller.IsUpMadKiller = true;
                                }
                            }
                        }
                        else if (player.IsRole(RoleId.MadKiller))
                        {
                            var sideplayer = RoleClass.SideKiller.GetSidePlayer(PlayerControl.LocalPlayer);
                            if (sideplayer != null)
                            {
                                player.RPCSetRoleUnchecked(RoleTypes.Impostor);
                            }
                        }
                    }
                }
            }
        }
    }
}