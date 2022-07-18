using System.Collections.Generic;
using SuperNewRoles.CustomRPC;

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
                var player = ModHelpers.playerById((byte)RoleClass.CountChanger.ChangeData[p.PlayerId]);
                return Get(player);
            }
            else
            {
                return RoleClass.CountChanger.ChangeData.ContainsValue(p.PlayerId)
                    ? Get(ModHelpers.playerById((byte)RoleClass.CountChanger.ChangeData.GetKey(p.PlayerId)))
                    : Get(p);
            }
        }
        public static bool IsChangeMadmate(this PlayerControl p)
        {
            var getroledata = GetRoleType(p);
            if (getroledata == TeamRoleType.Crewmate)
            {
                if ((RoleClass.CountChanger.ChangeData.ContainsKey(p.PlayerId)
                    && ModHelpers.playerById((byte)RoleClass.CountChanger.ChangeData[p.PlayerId]).isRole(RoleId.MadMate))
                    || (RoleClass.CountChanger.ChangeData.ContainsValue(p.PlayerId)
                    && ModHelpers.playerById((byte)RoleClass.CountChanger.ChangeData.GetKey(p.PlayerId)).isRole(RoleId.MadMate)))
                    return true;
                else
                    return p.isRole(RoleId.MadMate);
            }
            return false;
        }
        public static bool IsChangeMadMayor(this PlayerControl p)
        {
            var getroledata = GetRoleType(p);
            if (getroledata == TeamRoleType.Crewmate)
            {
                if (RoleClass.CountChanger.ChangeData.ContainsKey(p.PlayerId)
                    && (ModHelpers.playerById((byte)RoleClass.CountChanger.ChangeData[p.PlayerId]).isRole(RoleId.MadMayor)
                    || (RoleClass.CountChanger.ChangeData.ContainsValue(p.PlayerId)
                    && ModHelpers.playerById((byte)RoleClass.CountChanger.ChangeData.GetKey(p.PlayerId)).isRole(RoleId.MadMayor))))
                    return true;
                else
                    return p.isRole(RoleId.MadMayor);
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
                    if (ModHelpers.playerById((byte)RoleClass.CountChanger.ChangeData[p.PlayerId]).isRole(RoleId.MadStuntMan)) return true;
                }
                else if (RoleClass.CountChanger.ChangeData.ContainsValue(p.PlayerId))
                {
                    if (ModHelpers.playerById((byte)RoleClass.CountChanger.ChangeData.GetKey(p.PlayerId)).isRole(RoleId.MadStuntMan)) return true;
                }
                else
                {
                    return p.isRole(RoleId.MadStuntMan);
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
                    if (ModHelpers.playerById((byte)RoleClass.CountChanger.ChangeData[p.PlayerId]).isRole(RoleId.MadJester)) return true;
                }
                else if (RoleClass.CountChanger.ChangeData.ContainsValue(p.PlayerId))
                {
                    if (ModHelpers.playerById((byte)RoleClass.CountChanger.ChangeData.GetKey(p.PlayerId)).isRole(RoleId.MadJester)) return true;
                }
                else
                {
                    return p.isRole(RoleId.MadJester);
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
                    if (ModHelpers.playerById((byte)RoleClass.CountChanger.ChangeData[p.PlayerId]).isRole(RoleId.MadHawk)) return true;
                }
                else if (RoleClass.CountChanger.ChangeData.ContainsValue(p.PlayerId))
                {
                    if (ModHelpers.playerById((byte)RoleClass.CountChanger.ChangeData.GetKey(p.PlayerId)).isRole(RoleId.MadHawk)) return true;
                }
                else
                {
                    return p.isRole(RoleId.MadHawk);
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
                    if (ModHelpers.playerById((byte)RoleClass.CountChanger.ChangeData[p.PlayerId]).isRole(RoleId.MadSeer)) return true;
                }
                else if (RoleClass.CountChanger.ChangeData.ContainsValue(p.PlayerId))
                {
                    if (ModHelpers.playerById((byte)RoleClass.CountChanger.ChangeData.GetKey(p.PlayerId)).isRole(RoleId.MadSeer)) return true;
                }
                else
                {
                    return p.isRole(RoleId.MadSeer);
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
                    if (ModHelpers.playerById((byte)RoleClass.CountChanger.ChangeData[p.PlayerId]).isRole(RoleId.MadMaker)) return true;
                }
                else if (RoleClass.CountChanger.ChangeData.ContainsValue(p.PlayerId))
                {
                    if (ModHelpers.playerById((byte)RoleClass.CountChanger.ChangeData.GetKey(p.PlayerId)).isRole(RoleId.MadMaker)) return true;
                }
                else
                {
                    return p.isRole(RoleId.MadMaker);
                }
            }
            return false;
        }
        public static bool IsChangeBlackCat(this PlayerControl p)
        {
            var getroledata = GetRoleType(p);
            if (getroledata == TeamRoleType.Crewmate)
            {
                if (RoleClass.CountChanger.ChangeData.ContainsKey(p.PlayerId))
                {
                    if (ModHelpers.playerById((byte)RoleClass.CountChanger.ChangeData[p.PlayerId]).isRole(RoleId.BlackCat)) return true;
                }
                else if (RoleClass.CountChanger.ChangeData.ContainsValue(p.PlayerId))
                {
                    if (ModHelpers.playerById((byte)RoleClass.CountChanger.ChangeData.GetKey(p.PlayerId)).isRole(RoleId.BlackCat)) return true;
                }
                else
                {
                    return p.isRole(RoleId.BlackCat);
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
                    if (ModHelpers.playerById((byte)RoleClass.CountChanger.ChangeData[p.PlayerId]).isRole(RoleId.Jackal)) return true;
                }
                else if (RoleClass.CountChanger.ChangeData.ContainsValue(p.PlayerId))
                {
                    if (ModHelpers.playerById((byte)RoleClass.CountChanger.ChangeData.GetKey(p.PlayerId)).isRole(RoleId.Jackal)) return true;
                }
                else
                {
                    return p.isRole(RoleId.Jackal);
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
                    if (ModHelpers.playerById((byte)RoleClass.CountChanger.ChangeData[p.PlayerId]).isRole(RoleId.Jackal)) return true;
                }
                else if (RoleClass.CountChanger.ChangeData.ContainsValue(p.PlayerId))
                {
                    if (ModHelpers.playerById((byte)RoleClass.CountChanger.ChangeData.GetKey(p.PlayerId)).isRole(RoleId.Jackal)) return true;
                }
                else
                {
                    return p.isRole(RoleId.Sidekick);
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
                    if (ModHelpers.playerById((byte)RoleClass.CountChanger.ChangeData[p.PlayerId]).isRole(RoleId.JackalFriends)) return true;
                }
                else if (RoleClass.CountChanger.ChangeData.ContainsValue(p.PlayerId))
                {
                    if (ModHelpers.playerById((byte)RoleClass.CountChanger.ChangeData.GetKey(p.PlayerId)).isRole(RoleId.JackalFriends)) return true;
                }
                else
                {
                    return p.isRole(RoleId.JackalFriends);
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
                    if (ModHelpers.playerById((byte)RoleClass.CountChanger.ChangeData[p.PlayerId]).isRole(RoleId.SeerFriends)) return true;
                }
                else if (RoleClass.CountChanger.ChangeData.ContainsValue(p.PlayerId))
                {
                    if (ModHelpers.playerById((byte)RoleClass.CountChanger.ChangeData.GetKey(p.PlayerId)).isRole(RoleId.SeerFriends)) return true;
                }
                else
                {
                    return p.isRole(RoleId.SeerFriends);
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
                    if (ModHelpers.playerById((byte)RoleClass.CountChanger.ChangeData[p.PlayerId]).isRole(RoleId.Jackal)) return true;
                }
                else if (RoleClass.CountChanger.ChangeData.ContainsValue(p.PlayerId))
                {
                    if (ModHelpers.playerById((byte)RoleClass.CountChanger.ChangeData.GetKey(p.PlayerId)).isRole(RoleId.Jackal)) return true;
                }
                else
                {
                    return p.isRole(RoleId.JackalSeer);
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
                    if (ModHelpers.playerById((byte)RoleClass.CountChanger.ChangeData[p.PlayerId]).isRole(RoleId.Jackal)) return true;
                }
                else if (RoleClass.CountChanger.ChangeData.ContainsValue(p.PlayerId))
                {
                    if (ModHelpers.playerById((byte)RoleClass.CountChanger.ChangeData.GetKey(p.PlayerId)).isRole(RoleId.Jackal)) return true;
                }
                else
                {
                    return p.isRole(RoleId.SidekickSeer);
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