using HarmonyLib;
using SuperNewRoles.MapCustoms;
using SuperNewRoles.Roles;
using SuperNewRoles.Mode;
using SuperNewRoles.Mode.SuperHostRoles;

namespace SuperNewRoles.Patch
{
    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Awake))]
    static class ShipStatus_AwakePatch
    {
        static void Postfix(ShipStatus __instance)
        {
            MapCustoms.Airship.SecretRoom.ShipStatusAwake(__instance);
            AddVitals.AddVital();
            RecordsAdminDestroy.AdminDestroy();
            ProctedMessager.Init();
        }
    }
    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Awake))]
    public static class ShipStatus_Awake_Patch
    {
        [HarmonyPostfix, HarmonyPriority(Priority.Last)]
        public static void Postfix(ShipStatus __instance)
        {
            MapUtilities.CachedShipStatus = __instance;
        }
    }
    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.RepairSystem))]
    class RepairSystemPatch
    {
        public static bool Prefix(ShipStatus __instance,
            [HarmonyArgument(0)] SystemTypes systemType,
            [HarmonyArgument(1)] PlayerControl player,
            [HarmonyArgument(2)] byte amount)
        {
            if (PlusModeHandler.IsMode(PlusModeId.NotSabotage))
            {
                return false;
            }
            if (ModeHandler.IsMode(ModeId.Default))
            {
                if (systemType == SystemTypes.Comms || systemType == SystemTypes.Sabotage || systemType == SystemTypes.Electrical)
                {
                    if (PlayerControl.LocalPlayer.IsRole(RoleId.Painter) && RoleClass.Painter.CurrentTarget != null && RoleClass.Painter.CurrentTarget.PlayerId == player.PlayerId) Roles.CrewMate.Painter.Handle(Roles.CrewMate.Painter.ActionType.SabotageRepair);
                }
            }
            if ((ModeHandler.IsMode(ModeId.BattleRoyal) || ModeHandler.IsMode(ModeId.Zombie) || ModeHandler.IsMode(ModeId.HideAndSeek) || ModeHandler.IsMode(ModeId.CopsRobbers)) && (systemType == SystemTypes.Sabotage || systemType == SystemTypes.Doors)) return false;

            if (systemType == SystemTypes.Electrical && 0 <= amount && amount <= 4) // 停電を直そうとした
            {
                if (player.IsMadRoles() && !CustomOptions.MadRolesCanFixElectrical.GetBool()){
                    return false;
                }
            }
            if (systemType == SystemTypes.Comms && amount is 0 or 16 or 17) // コミュサボを直そうとした
            {
                if (player.IsMadRoles() && !CustomOptions.MadRolesCanFixComms.GetBool()){
                    return false;
                }
            }
            if (ModeHandler.IsMode(ModeId.SuperHostRoles))
            {
                bool returndata = MorePatch.RepairSystem(__instance, systemType, player, amount);
                return returndata;
            }
            return true;
        }
        public static void Postfix(
            [HarmonyArgument(0)] SystemTypes systemType,
            [HarmonyArgument(1)] PlayerControl player,
            [HarmonyArgument(2)] byte amount)
        {
            if (!RoleHelpers.IsSabotage())
            {
                new LateTask(() =>
                {
                    foreach (PlayerControl p in RoleClass.Technician.TechnicianPlayer)
                    {
                        if (p.inVent && p.IsAlive() && Mode.BattleRoyal.Main.VentData.ContainsKey(p.PlayerId) && Mode.BattleRoyal.Main.VentData[p.PlayerId] != null)
                        {
                            p.MyPhysics.RpcBootFromVent((int)Mode.BattleRoyal.Main.VentData[p.PlayerId]);
                        }
                    }
                }, 0.1f, "TecExitVent");
            }
            SuperNewRolesPlugin.Logger.LogInfo(player.Data.PlayerName + " => " + systemType + " : " + amount);
            if (ModeHandler.IsMode(ModeId.SuperHostRoles))
            {
                SyncSetting.CustomSyncSettings();
                if (systemType == SystemTypes.Comms)
                {
                    Mode.SuperHostRoles.FixedUpdate.SetRoleNames();
                }
            }
        }
    }
    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.OnDestroy))]
    public static class ShipStatus_OnDestroy_Patch
    {
        [HarmonyPostfix, HarmonyPriority(Priority.Last)]
        public static void Postfix()
        {
            MapUtilities.CachedShipStatus = null;
            MapUtilities.MapDestroyed();
        }
    }
}