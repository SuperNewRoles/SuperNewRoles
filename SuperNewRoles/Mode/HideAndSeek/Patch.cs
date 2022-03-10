using HarmonyLib;
using Hazel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
                            ShipStatus.Instance.RepairSystem(SystemTypes.Electrical, PlayerControl.LocalPlayer, (byte)16);
                            ShipStatus.Instance.RepairSystem(SystemTypes.Laboratory, PlayerControl.LocalPlayer, (byte)16);
                            ShipStatus.Instance.RepairSystem(SystemTypes.Reactor, PlayerControl.LocalPlayer, (byte)16);
                            ShipStatus.Instance.RepairSystem(SystemTypes.LifeSupp, PlayerControl.LocalPlayer, (byte)16);
                        }
                            else if (task.TaskType == TaskTypes.RestoreOxy)
                            {
                                ShipStatus.Instance.RpcRepairSystem(SystemTypes.LifeSupp, 0 | 64);
                                ShipStatus.Instance.RpcRepairSystem(SystemTypes.LifeSupp, 1 | 64);
                            }
                            else if (task.TaskType == TaskTypes.ResetReactor)
                            {
                                ShipStatus.Instance.RpcRepairSystem(SystemTypes.Reactor, 16);
                            }
                            else if (task.TaskType == TaskTypes.ResetSeismic)
                            {
                                ShipStatus.Instance.RpcRepairSystem(SystemTypes.Laboratory, 16);
                            }
                            else if (task.TaskType == TaskTypes.FixComms)
                            {
                                ShipStatus.Instance.RpcRepairSystem(SystemTypes.Comms, 16 | 0);
                                ShipStatus.Instance.RpcRepairSystem(SystemTypes.Comms, 16 | 1);
                            }
                            else if (task.TaskType == TaskTypes.StopCharles)
                            {
                                ShipStatus.Instance.RpcRepairSystem(SystemTypes.Reactor, 0 | 16);
                                ShipStatus.Instance.RpcRepairSystem(SystemTypes.Reactor, 1 | 16);
                            }
                        }
                        foreach (PlainDoor door in ShipStatus.Instance.AllDoors)
                        {
                            door.SetDoorway(true);
                        }
            }
        }
        public class HASFixed
        {
            public static void Postfix(PlayerControl __instance)
            {
                RepairSystemPatch.Postfix(__instance);
                HudManager.Instance.ReportButton.Hide();
                HudManager.Instance.AbilityButton.Hide();
                
            }
        }
    }
}
