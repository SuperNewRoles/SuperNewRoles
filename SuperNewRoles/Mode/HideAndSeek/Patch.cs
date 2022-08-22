using SuperNewRoles.Roles;
using UnityEngine;
using static SuperNewRoles.MapUtilities;

namespace SuperNewRoles.Mode.HideAndSeek
{
    class Patch
    {
        public class RepairSystemPatch
        {
            public static void Postfix()
            {
                foreach (PlayerTask task in PlayerControl.LocalPlayer.myTasks)
                {
                    if (task.TaskType == TaskTypes.FixLights)
                    {
                        CachedShipStatus.RepairSystem(SystemTypes.Electrical, PlayerControl.LocalPlayer, 16);
                        CachedShipStatus.RepairSystem(SystemTypes.Laboratory, PlayerControl.LocalPlayer, 16);
                        CachedShipStatus.RepairSystem(SystemTypes.Reactor, PlayerControl.LocalPlayer, 16);
                        CachedShipStatus.RepairSystem(SystemTypes.LifeSupp, PlayerControl.LocalPlayer, 16);
                    }
                    else if (task.TaskType == TaskTypes.RestoreOxy)
                    {
                        CachedShipStatus.RpcRepairSystem(SystemTypes.LifeSupp, 0 | 64);
                        CachedShipStatus.RpcRepairSystem(SystemTypes.LifeSupp, 1 | 64);
                    }
                    else if (task.TaskType == TaskTypes.ResetReactor)
                    {
                        CachedShipStatus.RpcRepairSystem(SystemTypes.Reactor, 16);
                    }
                    else if (task.TaskType == TaskTypes.ResetSeismic)
                    {
                        CachedShipStatus.RpcRepairSystem(SystemTypes.Laboratory, 16);
                    }
                    else if (task.TaskType == TaskTypes.FixComms)
                    {
                        CachedShipStatus.RpcRepairSystem(SystemTypes.Comms, 16 | 0);
                        CachedShipStatus.RpcRepairSystem(SystemTypes.Comms, 16 | 1);
                    }
                    else if (task.TaskType == TaskTypes.StopCharles)
                    {
                        CachedShipStatus.RpcRepairSystem(SystemTypes.Reactor, 0 | 16);
                        CachedShipStatus.RpcRepairSystem(SystemTypes.Reactor, 1 | 16);
                    }
                }
                foreach (PlainDoor door in CachedShipStatus.AllDoors)
                {
                    door.SetDoorway(true);
                }
            }
        }
        public class HASFixed
        {
            public static void Postfix()
            {
                if (AmongUsClient.Instance.AmHost)
                {
                    foreach (PlayerControl player in CachedPlayer.AllPlayers)
                    {
                        if (player.IsImpostor())
                        {
                            player.RpcSetName(ModHelpers.Cs(RoleClass.ImpostorRed, player.Data.GetPlayerName(PlayerOutfitType.Default)));
                        }
                        else
                        {
                            player.RpcSetName(ModHelpers.Cs(new Color32(0, 255, 0, byte.MaxValue), player.Data.GetPlayerName(PlayerOutfitType.Default)));
                        }
                    }
                    RepairSystemPatch.Postfix();
                }
                FastDestroyableSingleton<HudManager>.Instance.ReportButton.Hide();
                FastDestroyableSingleton<HudManager>.Instance.AbilityButton.Hide();
            }
        }
    }
}