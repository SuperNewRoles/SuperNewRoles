using HarmonyLib;
using SuperNewRoles.Roles;
using System;
using System.Collections.Generic;
using System.Text;

namespace SuperNewRoles.Mode.SuperHostRoles
{
    public static class FixedUpdate
    {
        public static Dictionary<int, string> DefaultName = new Dictionary<int, string>();
        private static int UpdateDate = 0;

        [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnPlayerJoined))]
        public class AmongUsClientOnPlayerJoinedPatch
        {
            public static void Postfix()
            {
                DefaultName = new Dictionary<int, string>();
            }
        }
        public static string getDefaultName(this PlayerControl player)
        {
            if (DefaultName.ContainsKey(player.PlayerId))
            {
                return DefaultName[player.PlayerId];
            }
            else
            {
                DefaultName[player.PlayerId] = player.CurrentOutfit.PlayerName;
                return DefaultName[player.PlayerId];
            }
        }
        public static string getsetname(PlayerControl p)
        {
            var introdate = SuperNewRoles.Intro.IntroDate.GetIntroDate(p.getRole(), p);
            if (RoleClass.IsMeeting)
            {
                return "<size=50%>(" + ModHelpers.cs(introdate.color, ModTranslation.getString(introdate.NameKey + "Name")) + ")</size>" + p.getDefaultName();
            }
            else
            {
                return "<size=75%>" + ModHelpers.cs(introdate.color, ModTranslation.getString(introdate.NameKey + "Name")) + "</size>\n" + p.getDefaultName();
            }
        }
        public static void RoleFixedUpdate()
        {


        }
        public static void Update()
        {
            if (AmongUsClient.Instance.GameState == AmongUsClient.GameStates.Started)
            {
                UpdateDate--;
                RoleFixedUpdate();
                if (AmongUsClient.Instance.AmHost)
                {
                    if (UpdateDate <= 0)
                    {
                        UpdateDate = 10;
                        foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                        {
                            if (p.isAlive() && !p.isRole(CustomRPC.RoleId.God))
                            {
                                if (p.PlayerId != 0)
                                {
                                    p.RpcSetNamePrivate(getsetname(p));
                                }
                                else
                                {
                                    Patch.SetNamesClass.SetPlayerRoleNames(PlayerControl.LocalPlayer);
                                }
                            }
                            else
                            {
                                if (p.PlayerId != 0)
                                {
                                    foreach (PlayerControl p2 in PlayerControl.AllPlayerControls)
                                    {
                                        p2.RpcSetNamePrivate(getsetname(p2), SeePlayer: p);
                                    }
                                }
                                else if (p.PlayerId == PlayerControl.LocalPlayer.PlayerId)
                                {
                                    foreach (PlayerControl p2 in PlayerControl.AllPlayerControls)
                                    {
                                        Patch.SetNamesClass.SetPlayerRoleInfo(p2);
                                    }
                                }
                            }
                        }
                        if (RoleClass.MadMate.IsImpostorCheck)
                        {
                            var impostorplayers = new List<PlayerControl>();
                            foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                            {
                                if (p.isImpostor())
                                {
                                    impostorplayers.Add(p);
                                }
                            }
                            foreach (PlayerControl p in RoleClass.MadMate.MadMatePlayer)
                            {
                                if (p.isAlive() && p.PlayerId != 0)
                                {
                                    foreach (PlayerControl p2 in impostorplayers)
                                    {
                                        p2.RpcSetNamePrivate(ModHelpers.cs(RoleClass.ImpostorRed,p2.getDefaultName()),p);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
