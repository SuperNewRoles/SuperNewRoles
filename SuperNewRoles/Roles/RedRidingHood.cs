using HarmonyLib;
using SuperNewRoles.Helpers;
using SuperNewRoles.Patch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SuperNewRoles.Roles
{
    public class RedRidingHood
    {
        [HarmonyPatch(typeof(ExileController), nameof(ExileController.WrapUp))]
        public class CheckEndGamePatch
        {
            public static void Postfix(ExileController __instance)
            {
                try
                {
                    WrapUp(__instance.exiled);
                }
                catch (Exception e)
                {
                    SuperNewRolesPlugin.Logger.LogInfo("CHECKERROR:" + e);
                }
            }
        }
        [HarmonyPatch(typeof(AirshipExileController), nameof(AirshipExileController.WrapUpAndSpawn))]
        public class CheckAirShipEndGamePatch
        {
            public static void Postfix(AirshipExileController __instance)
            {
                try
                {
                    WrapUp(__instance.exiled);
                }
                catch (Exception e)
                {
                    SuperNewRolesPlugin.Logger.LogInfo("CHECKERROR:" + e);
                }
            }
        }
        public static void WrapUp(GameData.PlayerInfo player)
        {
            if (PlayerControl.LocalPlayer.isDead() && PlayerControl.LocalPlayer.isRole(CustomRPC.RoleId.NiceRedRidingHood))
            {
                if (RoleClass.NiceRedRidingHood.Count >= 1)
                {
                    DeadPlayer deadPlayer = DeadPlayer.deadPlayers?.Where(x => x.player?.PlayerId == PlayerControl.LocalPlayer.PlayerId)?.FirstOrDefault();
                    if (deadPlayer.killerIfExisting != null && deadPlayer.killerIfExisting.isDead())
                    {
                        if (EvilEraser.IsOKAndTryUse(EvilEraser.BlockTypes.RedRidingHoodRevive,deadPlayer.killerIfExisting)) {
                            var Writer = RPCHelper.StartRPC(CustomRPC.CustomRPC.ReviveRPC);
                            Writer.Write(PlayerControl.LocalPlayer.PlayerId);
                            Writer.EndRPC();
                            CustomRPC.RPCProcedure.ReviveRPC(PlayerControl.LocalPlayer.PlayerId);
                            Writer = RPCHelper.StartRPC(CustomRPC.CustomRPC.CleanBody);
                            Writer.Write(PlayerControl.LocalPlayer.PlayerId);
                            Writer.EndRPC();
                            RoleClass.NiceRedRidingHood.deadbodypos = null;
                            CustomRPC.RPCProcedure.CleanBody(PlayerControl.LocalPlayer.PlayerId);
                            RoleClass.NiceRedRidingHood.Count--;
                            PlayerControl.LocalPlayer.Data.IsDead = false;

                            RoleClass.NiceRedRidingHood.deadbodypos = null;
                            DeadPlayer.deadPlayers?.RemoveAll(x => x.player?.PlayerId == PlayerControl.LocalPlayer.PlayerId);
                        }
                    }
                }
            }
        }
    }
}
