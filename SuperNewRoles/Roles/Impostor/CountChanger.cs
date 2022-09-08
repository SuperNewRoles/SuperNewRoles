using System.Collections.Generic;


//TODO:さつまいも、いつかリファクタします
namespace SuperNewRoles.Roles
{
    public static class CountChanger
    {
        public static class CountChangerPatch
        {
            public static void WrapUpPatch()
            {
                RoleClass.CountChanger.IsSet = false;
                RoleClass.CountChanger.ChangeData = RoleClass.CountChanger.Setdata;
                RoleClass.CountChanger.Setdata = new();
            }
        }
        public static bool IsChange(this PlayerControl p)
        {
            if (RoleClass.CountChanger.ChangeData.ContainsKey(p.PlayerId))
                return true;
            else if (RoleClass.CountChanger.ChangeData.ContainsValue(p.PlayerId))
                return true;
            return false;
        }

        public static TeamRoleType GetRoleType(this PlayerControl p)
        {
            if (RoleClass.CountChanger.ChangeData.ContainsKey(p.PlayerId))
            {
                var player = ModHelpers.PlayerById((byte)RoleClass.CountChanger.ChangeData[p.PlayerId]);
                return Get(player);
            }
            else
            {
                return RoleClass.CountChanger.ChangeData.ContainsValue(p.PlayerId)
                    ? Get(ModHelpers.PlayerById((byte)RoleClass.CountChanger.ChangeData.GetKey(p.PlayerId)))
                    : Get(p);
            }
        }
        public static bool IsChange(PlayerControl p, RoleId role)
        {
            if (GetRoleType(p) == TeamRoleType.Crewmate)
            {
                if (RoleClass.CountChanger.ChangeData.ContainsKey(p.PlayerId))
                {
                    if (ModHelpers.PlayerById((byte)RoleClass.CountChanger.ChangeData[p.PlayerId]).IsRole(role)) return true;
                }
                else if (RoleClass.CountChanger.ChangeData.ContainsValue(p.PlayerId))
                {
                    if (ModHelpers.PlayerById((byte)RoleClass.CountChanger.ChangeData.GetKey(p.PlayerId)).IsRole(role)) return true;
                }
                else
                {
                    return p.IsRole(role);
                }
            }
            return false;
        }
        public static int? GetKey(this Dictionary<int, int> dics, int id)
        {
            foreach (var data in dics)
            {
                if (data.Value == id)
                {
                    return data.Key;
                }
            }
            return null;
        }
        public static TeamRoleType Get(PlayerControl player)
        {
            if (player.IsImpostor())
            {
                return TeamRoleType.Impostor;
            }
            else if (player.IsCrew())
            {
                return TeamRoleType.Crewmate;
            }
            else if (player.IsNeutral())
            {
                return TeamRoleType.Neutral;
            }
            return TeamRoleType.Error;
        }
    }
}