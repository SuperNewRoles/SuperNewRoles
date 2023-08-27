using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AmongUs.GameOptions;
using SuperNewRoles.Helpers;
using UnityEngine;

namespace SuperNewRoles.Mode.BattleRoyal.BattleRole;
public class LongKiller : BattleRoyalRole
{
    public static List<LongKiller> longKillers;
    public static bool IsLongKiller(PlayerControl player)
    {
        return longKillers.FirstOrDefault(x => x.CurrentPlayer == player) is not null;
    }
    public LongKiller(PlayerControl player)
    {
        CurrentPlayer = player;
        longKillers.Add(this);
        PlayerAbility ability = PlayerAbility.GetPlayerAbility(player);
        ability.MyKillCoolTime *= 1.3f;
        ability.MyKillDistance = GameOptionsData.KillDistances.Length - 1;
    }
    public static void Clear()
    {
        longKillers = new();
    }
}