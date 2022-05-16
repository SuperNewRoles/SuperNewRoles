using System;
using System.Collections.Generic;
using System.Text;

namespace SuperNewRoles.Mode.CopsRobbers
{
    public class RoleSelectHandler
    {
        public static void Handler()
        {
            List<PlayerControl> SelectPlayers = new List<PlayerControl>();
            List<PlayerControl> impostors = new List<PlayerControl>();
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            {
                if (!player.Data.Disconnected && player.IsPlayer())
                {
                    SelectPlayers.Add(player);
                }
            }
            for (int i = 0; i < PlayerControl.GameOptions.NumImpostors; i++)
            {
                if (SelectPlayers.Count >= 1)
                {
                    var newimpostor = ModHelpers.GetRandom(SelectPlayers);
                    impostors.Add(newimpostor);
                    SelectPlayers.RemoveAll(a => a.PlayerId == newimpostor.PlayerId);
                }
            }
            foreach (PlayerControl player in impostors)
            {
                player.RpcSetRole(RoleTypes.Impostor);
            }
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            {
                if (!player.Data.Disconnected)
                {
                    if (!impostors.IsCheckListPlayerControl(player))
                    {
                        player.RpcSetRole(RoleTypes.Crewmate);
                    }
                   // player.RpcSetName("");
                }
            }
        }
    }
}
