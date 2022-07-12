using System.Collections.Generic;
using UnityEngine;
namespace SuperNewRoles.Intro
{
    public class TeamDate
    {
        public string NameKey;
        public Color color;
        public Color BackGround;
        public List<CustomRPC.RoleId> RoleIds;

        TeamDate(string NameKey, Color color, Color BackGround, List<CustomRPC.RoleId> RoleId)
        {
            this.color = color;
            this.BackGround = BackGround;
            this.NameKey = NameKey;
            this.RoleIds = RoleId;
        }
        public static TeamDate VultureTeam = new("Test", Color.black, Color.yellow, new List<CustomRPC.RoleId> { CustomRPC.RoleId.Sheriff });
    }
}
