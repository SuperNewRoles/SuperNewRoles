using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

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
            [HarmonyPatch(typeof(ExileController), nameof(ExileController.WrapUp))]
            public class CountChange
            {
                
                public static void Postfix(ExileController __instance)
                {
                    try
                    {
                        WrapUpPatch();
                    }
                    catch (Exception e)
                    {
                        SuperNewRolesPlugin.Logger.LogInfo("CHECKERROR:" + e);
                    }
                }
            }
            [HarmonyPatch(typeof(AirshipExileController), nameof(AirshipExileController.WrapUpAndSpawn))]
            public class CountChangeairship
            {
                public static void Postfix(AirshipExileController __instance)
                {
                    try
                    {
                        WrapUpPatch();
                    }
                    catch (Exception e)
                    {
                        SuperNewRolesPlugin.Logger.LogInfo("CHECKERROR:" + e);
                    }
                }
            }
        }
        public static bool isChange(this PlayerControl p)
        {
            if (RoleClass.CountChanger.ChangeData.ContainsKey(p.PlayerId))
            {
                return true;
            } else if (RoleClass.CountChanger.ChangeData.ContainsValue(p.PlayerId))
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
                } else
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
                    if(ModHelpers.playerById((byte)RoleClass.CountChanger.ChangeData[p.PlayerId]).isRole(CustomRPC.RoleId.MadMate)) return true;
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
        public static int? GetKey(this Dictionary<int,int> dics,int id)
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
            }    }
}
