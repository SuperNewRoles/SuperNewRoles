using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AmongUs.GameOptions;
using SuperNewRoles.Helpers;
using SuperNewRoles.Mode.SuperHostRoles;
using UnityEngine;

namespace SuperNewRoles.Mode.BattleRoyal.BattleRole;
public class Darknight : BattleRoyalRole
{
    public static List<Darknight> darknights;
    public static bool IsDarknight(PlayerControl player)
    {
        return darknights.FirstOrDefault(x => x.CurrentPlayer == player) is not null;
    }
    public static Darknight GetDarknightPlayer(PlayerControl player)
    {
        return darknights.FirstOrDefault(x => x.CurrentPlayer == player);
    }
    public Darknight(PlayerControl player)
    {
        CurrentPlayer = player;
        darknights.Add(this);
        PlayerAbility ability = PlayerAbility.GetPlayerAbility(player);
        ability.MyLight = 0;
        ability.MyKillCoolTime = 0;
        ability.MyKillDistance = 0;
    }
    public bool IsKillingNow = false;
    public void OnKill(PlayerControl target)
    {
        IsKillingNow = true;
        BattleTeam team = BattleTeam.GetTeam(CurrentPlayer);
        foreach (PlayerControl player in PlayerControl.AllPlayerControls)
        {
            if (player.IsBot()) continue;
            if (player.IsDead()) continue;
            if (team.IsTeam(player)) continue;
            if (Vector2.Distance(target.transform.position, player.transform.position) > GameOptionsData.KillDistances[0]) continue;
            CurrentPlayer.RpcMurderPlayer(player);
        }
        IsKillingNow = false;
    }
    public static void Clear()
    {
        darknights = new();
    }
}