using HarmonyLib;
using Hazel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SuperNewRoles.Mode.HideAndSeek
{
    class Patch
    {
        public class RepairSystemPatch
        {
            public static void Postfix(PlayerControl __instance) {
                        foreach (PlayerTask task in PlayerControl.LocalPlayer.myTasks)
                        {
                        if (task.TaskType == TaskTypes.FixLights)
                        {
                            MapUtilities.CachedShipStatus.RepairSystem(SystemTypes.Electrical, PlayerControl.LocalPlayer, (byte)16);
                            MapUtilities.CachedShipStatus.RepairSystem(SystemTypes.Laboratory, PlayerControl.LocalPlayer, (byte)16);
                            MapUtilities.CachedShipStatus.RepairSystem(SystemTypes.Reactor, PlayerControl.LocalPlayer, (byte)16);
                            MapUtilities.CachedShipStatus.RepairSystem(SystemTypes.LifeSupp, PlayerControl.LocalPlayer, (byte)16);
                        }
                            else if (task.TaskType == TaskTypes.RestoreOxy)
                            {
                                MapUtilities.CachedShipStatus.RpcRepairSystem(SystemTypes.LifeSupp, 0 | 64);
                                MapUtilities.CachedShipStatus.RpcRepairSystem(SystemTypes.LifeSupp, 1 | 64);
                            }
                            else if (task.TaskType == TaskTypes.ResetReactor)
                            {
                                MapUtilities.CachedShipStatus.RpcRepairSystem(SystemTypes.Reactor, 16);
                            }
                            else if (task.TaskType == TaskTypes.ResetSeismic)
                            {
                                MapUtilities.CachedShipStatus.RpcRepairSystem(SystemTypes.Laboratory, 16);
                            }
                            else if (task.TaskType == TaskTypes.FixComms)
                            {
                                MapUtilities.CachedShipStatus.RpcRepairSystem(SystemTypes.Comms, 16 | 0);
                                MapUtilities.CachedShipStatus.RpcRepairSystem(SystemTypes.Comms, 16 | 1);
                            }
                            else if (task.TaskType == TaskTypes.StopCharles)
                            {
                                MapUtilities.CachedShipStatus.RpcRepairSystem(SystemTypes.Reactor, 0 | 16);
                                MapUtilities.CachedShipStatus.RpcRepairSystem(SystemTypes.Reactor, 1 | 16);
                            }
                        }
                        foreach (PlainDoor door in MapUtilities.CachedShipStatus.AllDoors)
                        {
                            door.SetDoorway(true);
                        }
            }
        }
        public class HASFixed
        {
            public static void Postfix(PlayerControl __instance)
            {
                if (AmongUsClient.Instance.AmHost)
                {
                    foreach (PlayerControl player in CachedPlayer.AllPlayers)
                    {
                        if (player.isImpostor())
                        {
                            player.RpcSetName(ModHelpers.cs(Roles.RoleClass.ImpostorRed, player.Data.GetPlayerName(PlayerOutfitType.Default)));
                        }
                        else
                        {
                            player.RpcSetName(ModHelpers.cs(new Color32(0, 255, 0, byte.MaxValue), player.Data.GetPlayerName(PlayerOutfitType.Default)));

                        }
                    }
                    RepairSystemPatch.Postfix(__instance);
                }
                FastDestroyableSingleton<HudManager>.Instance.ReportButton.Hide();
                FastDestroyableSingleton<HudManager>.Instance.AbilityButton.Hide();
                
            }
        }
    }
}
