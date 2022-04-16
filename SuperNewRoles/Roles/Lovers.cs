using HarmonyLib;
using SuperNewRoles.Helpers;
using SuperNewRoles.Mode;
using SuperNewRoles.Mode.SuperHostRoles;
using System;
using System.Collections.Generic;
using System.Text;

namespace SuperNewRoles.Roles
{
    class Lovers
    {
        [HarmonyPatch(typeof(GameData), nameof(GameData.HandleDisconnect), new Type[] { typeof(PlayerControl), typeof(DisconnectReasons) })]
        class HandleDisconnectPatch
        {
            public static void Postfix(GameData __instance, PlayerControl player, DisconnectReasons reason)
            {
                if (AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Started)
                {
                    if (player.IsLovers())
                    {
                        RoleClass.Lovers.LoversPlayer.RemoveAll(x => x.TrueForAll(x2 => x2.PlayerId == player.PlayerId));
                        ChacheManager.ResetLoversChache();
                    }
                    if (ModeHandler.isMode(ModeId.SuperHostRoles))
                    {
                        Madmate.CheckedImpostor = new List<byte>();
                        foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                        {
                            foreach (PlayerControl p2 in PlayerControl.AllPlayerControls)
                            {
                                if (p.PlayerId != p2.PlayerId && !p2.Data.Disconnected)
                                {
                                    p2.RpcSetNamePrivate(p2.getDefaultName(), p);
                                }
                            }
                        }
                    } else if (ModeHandler.isMode(ModeId.Default))
                    {
                        if (player.isRole(CustomRPC.RoleId.SideKiller))
                        {
                            var sideplayer = RoleClass.SideKiller.getSidePlayer(PlayerControl.LocalPlayer);
                            if (sideplayer != null)
                            {
                                if (!RoleClass.SideKiller.IsUpMadKiller)
                                {
                                    sideplayer.RPCSetRoleUnchecked(RoleTypes.Impostor);
                                    RoleClass.SideKiller.IsUpMadKiller = true;
                                }
                            }
                        } else if (player.isRole(CustomRPC.RoleId.MadKiller))
                        {
                            var sideplayer = RoleClass.SideKiller.getSidePlayer(PlayerControl.LocalPlayer);
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
