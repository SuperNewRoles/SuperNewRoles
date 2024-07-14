using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperNewRoles.Mode.BattleRoyal
{
    public class BattleTeam
    {
        public static List<BattleTeam> BattleTeams = new();
        public List<PlayerControl> TeamMember = new();
        public static void ClearAll()
        {
            BattleTeams = new();
        }
        public static BattleTeam GetTeam(PlayerControl Player)
        {
            if (Player is null) Player = PlayerControl.LocalPlayer;
            foreach (BattleTeam team in BattleTeams.AsSpan())
            {
                if (team.TeamMember.IsCheckListPlayerControl(Player))
                {
                    return team;
                }
            }
            Logger.Info($"{Player.NameText().text}のチームが参照できませんでした。");
            return null;
        }
        public bool IsTeam(PlayerControl Player = null)
        {
            Logger.Info($"{TeamMember.Count}", "TEAMCOUNT");
            return TeamMember.FirstOrDefault(x => x is not null && x == Player) is not null;
        }
        public BattleTeam()
        {
            BattleTeams.Add(this);
        }
    }
}