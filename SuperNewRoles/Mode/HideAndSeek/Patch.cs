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
        [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.CoEnterVent))]
        class CoEnterVentPatch
        {
            public static void Prefix(PlayerPhysics __instance, [HarmonyArgument(0)] int id)
            {
                if (ModeHandler.isMode(ModeId.HideAndSeek) && !ZombieOptions.HASUseVent.getBool())
                {
                    __instance.ExitAllVents();
                    __instance.RpcExitVent(id);
                }
            }
        }
        public class RepairSystemPatch
        {
            public static void Postfix(PlayerControl __instance) {
                if (ModeHandler.isMode(ModeId.HideAndSeek) && !ZombieOptions.HASUseSabo.getBool())
                {
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
        }
        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.HandleRpc))]
        class VentRPCHandlerPatch
        {
            static void Postfix([HarmonyArgument(0)] byte callId, [HarmonyArgument(1)] MessageReader reader)
            {
                try
                {
                    byte packetId = callId;
                    switch (packetId)
                    {
                        case (byte)RpcCalls.EnterVent:
                            if (ModeHandler.isMode(ModeId.HideAndSeek))
                            {
                                var vent = ((IEnumerable<Vent>)ShipStatus.Instance.AllVents).First<Vent>((Func<Vent, bool>)(v => v.Id == reader.ReadPackedInt32()));
                                foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                                {
                                    p.MyPhysics.RpcExitVent(vent.Id);
                                }
                            }
                            break;
                    }
                }
                catch { 
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
