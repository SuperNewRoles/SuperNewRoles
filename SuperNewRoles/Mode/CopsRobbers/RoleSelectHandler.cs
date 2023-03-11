using System.Collections.Generic;
using AmongUs.GameOptions;
using SuperNewRoles.Mode.SuperHostRoles;

namespace SuperNewRoles.Mode.CopsRobbers;

public class RoleSelectHandler
{
    public static void Handler()
    {
        List<PlayerControl> SelectPlayers = new();
        List<PlayerControl> impostors = new();
        foreach (PlayerControl player in CachedPlayer.AllPlayers)
        {
            if (!player.Data.Disconnected && !player.IsBot())
            {
                SelectPlayers.Add(player);
            }
        }
        for (int i = 0; i < GameManager.Instance.LogicOptions.currentGameOptions.NumImpostors; i++)
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
            foreach (PlayerControl player2 in CachedPlayer.AllPlayers)
            {
                if (!player2.Data.Disconnected)
                {
                    if (!player2.IsImpostor())
                    {
                        player2.RpcSetRoleDesync(RoleTypes.GuardianAngel, player);
                    }
                }
            }
        }
        foreach (PlayerControl player in CachedPlayer.AllPlayers)
        {
            if (!player.Data.Disconnected)
            {
                if (!player.IsImpostor())
                {
                    player.RpcSetRole(RoleTypes.Crewmate);
                }
                // player.RpcSetName("");
            }
        }
        RoleSystem.AssignRole();
        Main.ChangeCosmetics();
    }
}