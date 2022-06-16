using System.Collections.Generic;

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
                RoleClass.CountChanger.Setdata = new Dictionary<int, int>();
            }
        }
        public static bool isChange(this PlayerControl p)
        {
            if (RoleClass.CountChanger.ChangeData.ContainsKey(p.PlayerId))
            {
                return true;
            }
            else if (RoleClass.CountChanger.ChangeData.ContainsValue(p.PlayerId))
            {
                return true;
            }
            return false;
        }

        public static TeamRoleType GetRoleType(this PlayerControl p)
        {
            if (RoleClass.CountChanger.ChangeData.ContainsKey(p.PlayerId))
            {
                var player = ModHelpers.playerById((byte)RoleClass.CountChanger.ChangeData[p.PlayerId]);
                return Get(player);
            }
            else if (RoleClass.CountChanger.ChangeData.ContainsValue(p.PlayerId))
            {
                return Get(ModHelpers.playerById((byte)RoleClass.CountChanger.ChangeData.GetKey(p.PlayerId)));
            }
            else
            {
                return Get(p);
            }
        }
        public static bool IsChangeMadmate(this PlayerControl p)
        {
            var getroledata = GetRoleType(p);
            if (getroledata == TeamRoleType.Crewmate)
            {
                if (RoleClass.CountChanger.ChangeData.ContainsKey(p.PlayerId))
                {
                    if (ModHelpers.playerById((byte)RoleClass.CountChanger.ChangeData[p.PlayerId]).isRole(CustomRPC.RoleId.MadMate)) return true;
                }
                else if (RoleClass.CountChanger.ChangeData.ContainsValue(p.PlayerId))
                {
                    if (ModHelpers.playerById((byte)RoleClass.CountChanger.ChangeData.GetKey(p.PlayerId)).isRole(CustomRPC.RoleId.MadMate)) return true;
                }
                else
                {
                    return p.isRole(CustomRPC.RoleId.MadMate);
                }
            }
            return false;
        }
        public static bool IsChangeMadMayor(this PlayerControl p)
        {
            var getroledata = GetRoleType(p);
            if (getroledata == TeamRoleType.Crewmate)
            {
                if (RoleClass.CountChanger.ChangeData.ContainsKey(p.PlayerId))
                {
                    if (ModHelpers.playerById((byte)RoleClass.CountChanger.ChangeData[p.PlayerId]).isRole(CustomRPC.RoleId.MadMayor)) return true;
                }
                else if (RoleClass.CountChanger.ChangeData.ContainsValue(p.PlayerId))
                {
                    if (ModHelpers.playerById((byte)RoleClass.CountChanger.ChangeData.GetKey(p.PlayerId)).isRole(CustomRPC.RoleId.MadMayor)) return true;
                }
                else
                {
                    return p.isRole(CustomRPC.RoleId.MadMayor);
                }
            }
            return false;
        }
        public static bool IsChangeMadStuntMan(this PlayerControl p)
        {
            var getroledata = GetRoleType(p);
            if (getroledata == TeamRoleType.Crewmate)
            {
                if (RoleClass.CountChanger.ChangeData.ContainsKey(p.PlayerId))
                {
                    if (ModHelpers.playerById((byte)RoleClass.CountChanger.ChangeData[p.PlayerId]).isRole(CustomRPC.RoleId.MadStuntMan)) return true;
                }
                else if (RoleClass.CountChanger.ChangeData.ContainsValue(p.PlayerId))
                {
                    if (ModHelpers.playerById((byte)RoleClass.CountChanger.ChangeData.GetKey(p.PlayerId)).isRole(CustomRPC.RoleId.MadStuntMan)) return true;
                }
                else
                {
                    return p.isRole(CustomRPC.RoleId.MadStuntMan);
                }
            }
            return false;
        }
        public static bool IsChangeMadJester(this PlayerControl p)
        {
            var getroledata = GetRoleType(p);
            if (getroledata == TeamRoleType.Crewmate)
            {
                if (RoleClass.CountChanger.ChangeData.ContainsKey(p.PlayerId))
                {
                    if (ModHelpers.playerById((byte)RoleClass.CountChanger.ChangeData[p.PlayerId]).isRole(CustomRPC.RoleId.MadJester)) return true;
                }
                else if (RoleClass.CountChanger.ChangeData.ContainsValue(p.PlayerId))
                {
                    if (ModHelpers.playerById((byte)RoleClass.CountChanger.ChangeData.GetKey(p.PlayerId)).isRole(CustomRPC.RoleId.MadJester)) return true;
                }
                else
                {
                    return p.isRole(CustomRPC.RoleId.MadJester);
                }
            }
            return false;
        }
        public static bool IsChangeMadHawk(this PlayerControl p)
        {
            var getroledata = GetRoleType(p);
            if (getroledata == TeamRoleType.Crewmate)
            {
                if (RoleClass.CountChanger.ChangeData.ContainsKey(p.PlayerId))
                {
                    if (ModHelpers.playerById((byte)RoleClass.CountChanger.ChangeData[p.PlayerId]).isRole(CustomRPC.RoleId.MadHawk)) return true;
                }
                else if (RoleClass.CountChanger.ChangeData.ContainsValue(p.PlayerId))
                {
                    if (ModHelpers.playerById((byte)RoleClass.CountChanger.ChangeData.GetKey(p.PlayerId)).isRole(CustomRPC.RoleId.MadHawk)) return true;
                }
                else
                {
                    return p.isRole(CustomRPC.RoleId.MadHawk);
                }
            }
            return false;
        }
        public static bool IsChangeMadSeer(this PlayerControl p)
        {
            var getroledata = GetRoleType(p);
            if (getroledata == TeamRoleType.Crewmate)
            {
                if (RoleClass.CountChanger.ChangeData.ContainsKey(p.PlayerId))
                {
                    if (ModHelpers.playerById((byte)RoleClass.CountChanger.ChangeData[p.PlayerId]).isRole(CustomRPC.RoleId.MadSeer)) return true;
                }
                else if (RoleClass.CountChanger.ChangeData.ContainsValue(p.PlayerId))
                {
                    if (ModHelpers.playerById((byte)RoleClass.CountChanger.ChangeData.GetKey(p.PlayerId)).isRole(CustomRPC.RoleId.MadSeer)) return true;
                }
                else
                {
                    return p.isRole(CustomRPC.RoleId.MadSeer);
                }
            }
            return false;
        }
        public static bool IsChangeMadMaker(this PlayerControl p)
        {
            var getroledata = GetRoleType(p);
            if (getroledata == TeamRoleType.Crewmate)
            {
                if (RoleClass.CountChanger.ChangeData.ContainsKey(p.PlayerId))
                {
                    if (ModHelpers.playerById((byte)RoleClass.CountChanger.ChangeData[p.PlayerId]).isRole(CustomRPC.RoleId.MadMaker)) return true;
                }
                else if (RoleClass.CountChanger.ChangeData.ContainsValue(p.PlayerId))
                {
                    if (ModHelpers.playerById((byte)RoleClass.CountChanger.ChangeData.GetKey(p.PlayerId)).isRole(CustomRPC.RoleId.MadMaker)) return true;
                }
                else
                {
                    return p.isRole(CustomRPC.RoleId.MadMaker);
                }
            }
            return false;
        }
        public static bool IsChangeJackal(this PlayerControl p)
        {
            var getroledata = GetRoleType(p);
            if (getroledata == TeamRoleType.Crewmate)
            {
                if (RoleClass.CountChanger.ChangeData.ContainsKey(p.PlayerId))
                {
                    if (ModHelpers.playerById((byte)RoleClass.CountChanger.ChangeData[p.PlayerId]).isRole(CustomRPC.RoleId.Jackal)) return true;
                }
                else if (RoleClass.CountChanger.ChangeData.ContainsValue(p.PlayerId))
                {
                    if (ModHelpers.playerById((byte)RoleClass.CountChanger.ChangeData.GetKey(p.PlayerId)).isRole(CustomRPC.RoleId.Jackal)) return true;
                }
                else
                {
                    return p.isRole(CustomRPC.RoleId.Jackal);
                }
            }
            return false;
        }
        public static bool IsChangeSidekick(this PlayerControl p)
        {
            var getroledata = GetRoleType(p);
            if (getroledata == TeamRoleType.Crewmate)
            {
                if (RoleClass.CountChanger.ChangeData.ContainsKey(p.PlayerId))
                {
                    if (ModHelpers.playerById((byte)RoleClass.CountChanger.ChangeData[p.PlayerId]).isRole(CustomRPC.RoleId.Jackal)) return true;
                }
                else if (RoleClass.CountChanger.ChangeData.ContainsValue(p.PlayerId))
                {
                    if (ModHelpers.playerById((byte)RoleClass.CountChanger.ChangeData.GetKey(p.PlayerId)).isRole(CustomRPC.RoleId.Jackal)) return true;
                }
                else
                {
                    return p.isRole(CustomRPC.RoleId.Sidekick);
                }
            }
            return false;
        }
        public static bool IsChangeJackalFriends(this PlayerControl p)
        {
            var getroledata = GetRoleType(p);
            if (getroledata == TeamRoleType.Crewmate)
            {
                if (RoleClass.CountChanger.ChangeData.ContainsKey(p.PlayerId))
                {
                    if (ModHelpers.playerById((byte)RoleClass.CountChanger.ChangeData[p.PlayerId]).isRole(CustomRPC.RoleId.JackalFriends)) return true;
                }
                else if (RoleClass.CountChanger.ChangeData.ContainsValue(p.PlayerId))
                {
                    if (ModHelpers.playerById((byte)RoleClass.CountChanger.ChangeData.GetKey(p.PlayerId)).isRole(CustomRPC.RoleId.JackalFriends)) return true;
                }
                else
                {
                    return p.isRole(CustomRPC.RoleId.JackalFriends);
                }
            }
            return false;
        }
        public static bool IsChangeSeerFriends(this PlayerControl p)
        {
            var getroledata = GetRoleType(p);
            if (getroledata == TeamRoleType.Crewmate)
            {
                if (RoleClass.CountChanger.ChangeData.ContainsKey(p.PlayerId))
                {
                    if (ModHelpers.playerById((byte)RoleClass.CountChanger.ChangeData[p.PlayerId]).isRole(CustomRPC.RoleId.SeerFriends)) return true;
                }
                else if (RoleClass.CountChanger.ChangeData.ContainsValue(p.PlayerId))
                {
                    if (ModHelpers.playerById((byte)RoleClass.CountChanger.ChangeData.GetKey(p.PlayerId)).isRole(CustomRPC.RoleId.SeerFriends)) return true;
                }
                else
                {
                    return p.isRole(CustomRPC.RoleId.SeerFriends);
                }
            }
            return false;
        }
        public static bool IsChangeJackalSeer(this PlayerControl p)
        {
            var getroledata = GetRoleType(p);
            if (getroledata == TeamRoleType.Crewmate)
            {
                if (RoleClass.CountChanger.ChangeData.ContainsKey(p.PlayerId))
                {
                    if (ModHelpers.playerById((byte)RoleClass.CountChanger.ChangeData[p.PlayerId]).isRole(CustomRPC.RoleId.Jackal)) return true;
                }
                else if (RoleClass.CountChanger.ChangeData.ContainsValue(p.PlayerId))
                {
                    if (ModHelpers.playerById((byte)RoleClass.CountChanger.ChangeData.GetKey(p.PlayerId)).isRole(CustomRPC.RoleId.Jackal)) return true;
                }
                else
                {
                    return p.isRole(CustomRPC.RoleId.JackalSeer);
                }
            }
            return false;
        }
        public static bool IsChangeSidekickSeer(this PlayerControl p)
        {
            var getroledata = GetRoleType(p);
            if (getroledata == TeamRoleType.Crewmate)
            {
                if (RoleClass.CountChanger.ChangeData.ContainsKey(p.PlayerId))
                {
                    if (ModHelpers.playerById((byte)RoleClass.CountChanger.ChangeData[p.PlayerId]).isRole(CustomRPC.RoleId.Jackal)) return true;
                }
                else if (RoleClass.CountChanger.ChangeData.ContainsValue(p.PlayerId))
                {
                    if (ModHelpers.playerById((byte)RoleClass.CountChanger.ChangeData.GetKey(p.PlayerId)).isRole(CustomRPC.RoleId.Jackal)) return true;
                }
                else
                {
                    return p.isRole(CustomRPC.RoleId.SidekickSeer);
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
            if (player.isImpostor())
            {
                return TeamRoleType.Impostor;
            }
            else if (player.isCrew())
            {
                return TeamRoleType.Crewmate;
            }
            else if (player.isNeutral())
            {
                return TeamRoleType.Neutral;
            }
            return TeamRoleType.Error;
        }
    }
}