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
        public static bool IsChangeMadmate(this PlayerControl p)
        {
            var getroledata = GetRoleType(p);
            return getroledata == TeamRoleType.Crewmate
                ? (RoleClass.CountChanger.ChangeData.ContainsKey(p.PlayerId)
                    && ModHelpers.PlayerById((byte)RoleClass.CountChanger.ChangeData[p.PlayerId]).IsRole(RoleId.MadMate))
                    || (RoleClass.CountChanger.ChangeData.ContainsValue(p.PlayerId)
                    && ModHelpers.PlayerById((byte)RoleClass.CountChanger.ChangeData.GetKey(p.PlayerId)).IsRole(RoleId.MadMate))
                    ? true
                    : p.IsRole(RoleId.MadMate)
                : false;
        }
        public static bool IsChangeMadMayor(this PlayerControl p)
        {
            var getroledata = GetRoleType(p);
            return getroledata == TeamRoleType.Crewmate
                ? RoleClass.CountChanger.ChangeData.ContainsKey(p.PlayerId)
                    && (ModHelpers.PlayerById((byte)RoleClass.CountChanger.ChangeData[p.PlayerId]).IsRole(RoleId.MadMayor)
                    || (RoleClass.CountChanger.ChangeData.ContainsValue(p.PlayerId)
                    && ModHelpers.PlayerById((byte)RoleClass.CountChanger.ChangeData.GetKey(p.PlayerId)).IsRole(RoleId.MadMayor)))
                    ? true
                    : p.IsRole(RoleId.MadMayor)
                : false;
        }
        public static bool IsChangeMadStuntMan(this PlayerControl p)
        {
            var getroledata = GetRoleType(p);
            if (getroledata == TeamRoleType.Crewmate)
            {
                if (RoleClass.CountChanger.ChangeData.ContainsKey(p.PlayerId))
                {
                    if (ModHelpers.PlayerById((byte)RoleClass.CountChanger.ChangeData[p.PlayerId]).IsRole(RoleId.MadStuntMan)) return true;
                }
                else if (RoleClass.CountChanger.ChangeData.ContainsValue(p.PlayerId))
                {
                    if (ModHelpers.PlayerById((byte)RoleClass.CountChanger.ChangeData.GetKey(p.PlayerId)).IsRole(RoleId.MadStuntMan)) return true;
                }
                else
                {
                    return p.IsRole(RoleId.MadStuntMan);
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
                    if (ModHelpers.PlayerById((byte)RoleClass.CountChanger.ChangeData[p.PlayerId]).IsRole(RoleId.MadJester)) return true;
                }
                else if (RoleClass.CountChanger.ChangeData.ContainsValue(p.PlayerId))
                {
                    if (ModHelpers.PlayerById((byte)RoleClass.CountChanger.ChangeData.GetKey(p.PlayerId)).IsRole(RoleId.MadJester)) return true;
                }
                else
                {
                    return p.IsRole(RoleId.MadJester);
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
                    if (ModHelpers.PlayerById((byte)RoleClass.CountChanger.ChangeData[p.PlayerId]).IsRole(RoleId.MadHawk)) return true;
                }
                else if (RoleClass.CountChanger.ChangeData.ContainsValue(p.PlayerId))
                {
                    if (ModHelpers.PlayerById((byte)RoleClass.CountChanger.ChangeData.GetKey(p.PlayerId)).IsRole(RoleId.MadHawk)) return true;
                }
                else
                {
                    return p.IsRole(RoleId.MadHawk);
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
                    if (ModHelpers.PlayerById((byte)RoleClass.CountChanger.ChangeData[p.PlayerId]).IsRole(RoleId.MadSeer)) return true;
                }
                else if (RoleClass.CountChanger.ChangeData.ContainsValue(p.PlayerId))
                {
                    if (ModHelpers.PlayerById((byte)RoleClass.CountChanger.ChangeData.GetKey(p.PlayerId)).IsRole(RoleId.MadSeer)) return true;
                }
                else
                {
                    return p.IsRole(RoleId.MadSeer);
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
                    if (ModHelpers.PlayerById((byte)RoleClass.CountChanger.ChangeData[p.PlayerId]).IsRole(RoleId.MadMaker)) return true;
                }
                else if (RoleClass.CountChanger.ChangeData.ContainsValue(p.PlayerId))
                {
                    if (ModHelpers.PlayerById((byte)RoleClass.CountChanger.ChangeData.GetKey(p.PlayerId)).IsRole(RoleId.MadMaker)) return true;
                }
                else
                {
                    return p.IsRole(RoleId.MadMaker);
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
                    if (ModHelpers.PlayerById((byte)RoleClass.CountChanger.ChangeData[p.PlayerId]).IsRole(RoleId.BlackCat)) return true;
                }
                else if (RoleClass.CountChanger.ChangeData.ContainsValue(p.PlayerId))
                {
                    if (ModHelpers.PlayerById((byte)RoleClass.CountChanger.ChangeData.GetKey(p.PlayerId)).IsRole(RoleId.BlackCat)) return true;
                }
                else
                {
                    return p.IsRole(RoleId.BlackCat);
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
                    if (ModHelpers.PlayerById((byte)RoleClass.CountChanger.ChangeData[p.PlayerId]).IsRole(RoleId.Jackal)) return true;
                }
                else if (RoleClass.CountChanger.ChangeData.ContainsValue(p.PlayerId))
                {
                    if (ModHelpers.PlayerById((byte)RoleClass.CountChanger.ChangeData.GetKey(p.PlayerId)).IsRole(RoleId.Jackal)) return true;
                }
                else
                {
                    return p.IsRole(RoleId.Jackal);
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
                    if (ModHelpers.PlayerById((byte)RoleClass.CountChanger.ChangeData[p.PlayerId]).IsRole(RoleId.Jackal)) return true;
                }
                else if (RoleClass.CountChanger.ChangeData.ContainsValue(p.PlayerId))
                {
                    if (ModHelpers.PlayerById((byte)RoleClass.CountChanger.ChangeData.GetKey(p.PlayerId)).IsRole(RoleId.Jackal)) return true;
                }
                else
                {
                    return p.IsRole(RoleId.Sidekick);
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
                    if (ModHelpers.PlayerById((byte)RoleClass.CountChanger.ChangeData[p.PlayerId]).IsRole(RoleId.JackalFriends)) return true;
                }
                else if (RoleClass.CountChanger.ChangeData.ContainsValue(p.PlayerId))
                {
                    if (ModHelpers.PlayerById((byte)RoleClass.CountChanger.ChangeData.GetKey(p.PlayerId)).IsRole(RoleId.JackalFriends)) return true;
                }
                else
                {
                    return p.IsRole(RoleId.JackalFriends);
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
                    if (ModHelpers.PlayerById((byte)RoleClass.CountChanger.ChangeData[p.PlayerId]).IsRole(RoleId.SeerFriends)) return true;
                }
                else if (RoleClass.CountChanger.ChangeData.ContainsValue(p.PlayerId))
                {
                    if (ModHelpers.PlayerById((byte)RoleClass.CountChanger.ChangeData.GetKey(p.PlayerId)).IsRole(RoleId.SeerFriends)) return true;
                }
                else
                {
                    return p.IsRole(RoleId.SeerFriends);
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
                    if (ModHelpers.PlayerById((byte)RoleClass.CountChanger.ChangeData[p.PlayerId]).IsRole(RoleId.Jackal)) return true;
                }
                else if (RoleClass.CountChanger.ChangeData.ContainsValue(p.PlayerId))
                {
                    if (ModHelpers.PlayerById((byte)RoleClass.CountChanger.ChangeData.GetKey(p.PlayerId)).IsRole(RoleId.Jackal)) return true;
                }
                else
                {
                    return p.IsRole(RoleId.JackalSeer);
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
                    if (ModHelpers.PlayerById((byte)RoleClass.CountChanger.ChangeData[p.PlayerId]).IsRole(RoleId.Jackal)) return true;
                }
                else if (RoleClass.CountChanger.ChangeData.ContainsValue(p.PlayerId))
                {
                    if (ModHelpers.PlayerById((byte)RoleClass.CountChanger.ChangeData.GetKey(p.PlayerId)).IsRole(RoleId.Jackal)) return true;
                }
                else
                {
                    return p.IsRole(RoleId.SidekickSeer);
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