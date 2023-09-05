using System.Collections.Generic;
using SuperNewRoles.Helpers;
using SuperNewRoles.Patches;
using SuperNewRoles.Roles;
using SuperNewRoles.Roles.Crewmate;
using SuperNewRoles.Roles.Impostor;
using SuperNewRoles.Roles.Neutral;
using SuperNewRoles.Roles.RoleBases;
using UnityEngine;
using static SuperNewRoles.Modules.CustomOption;

namespace SuperNewRoles.Roles;

public static class Levelinger
{
    public static void MurderPlayer(PlayerControl __instance, PlayerControl target)
    {
        if (__instance.PlayerId != CachedPlayer.LocalPlayer.PlayerId) return;
        if (__instance.IsRole(RoleId.Levelinger))
        {
            RoleClass.Levelinger.ThisXP += RoleClass.Levelinger.OneKillXP;
        }
        else if (target.IsRole(RoleId.Levelinger))
        {
            LevelingerRevive();
        }
    }
    public static void LevelingerRevive()
    {
        if (RoleClass.Levelinger.IsUseOKRevive)
        {
            if (RoleClass.Levelinger.ReviveUseXP <= RoleClass.Levelinger.ThisXP)
            {
                var Writer = RPCHelper.StartRPC(CustomRPC.ReviveRPC);
                Writer.Write(CachedPlayer.LocalPlayer.PlayerId);
                Writer.EndRPC();
                RPCProcedure.ReviveRPC(CachedPlayer.LocalPlayer.PlayerId);
                Writer = RPCHelper.StartRPC(CustomRPC.CleanBody);
                Writer.Write(CachedPlayer.LocalPlayer.PlayerId);
                Writer.EndRPC();
                RPCProcedure.CleanBody(CachedPlayer.LocalPlayer.PlayerId);
                CachedPlayer.LocalPlayer.Data.IsDead = false;
                DeadPlayer.deadPlayers?.RemoveAll(x => x.player?.PlayerId == CachedPlayer.LocalPlayer.PlayerId);
                RoleClass.Levelinger.ThisXP -= RoleClass.Levelinger.ReviveUseXP;
            }
        }
    }
    public static bool LevelingerCanUse(string data)
    {
        List<string> leveData = new() { "optionOff", "LevelingerSettingKeep", "PursuerName", "TeleporterName", "SidekickName", "SpeedBoosterName", "MovingName" };
        if (!leveData.Contains(data)) return false;
        if (CustomOptionHolder.LevelingerLevelOneGetPower.GetString() == ModTranslation.GetString(data) ||
           CustomOptionHolder.LevelingerLevelTwoGetPower.GetString() == ModTranslation.GetString(data) ||
           CustomOptionHolder.LevelingerLevelThreeGetPower.GetString() == ModTranslation.GetString(data) ||
           CustomOptionHolder.LevelingerLevelFourGetPower.GetString() == ModTranslation.GetString(data) ||
           CustomOptionHolder.LevelingerLevelFiveGetPower.GetString() == ModTranslation.GetString(data)) return true;
        return false;
    }
}