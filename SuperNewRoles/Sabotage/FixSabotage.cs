using System.Collections.Generic;
using HarmonyLib;
using SuperNewRoles.Mode;
using SuperNewRoles.Helpers;
using InnerNet;
using Hazel;

namespace SuperNewRoles.Sabotage;

public class FixSabotage
{
    /// <summary>
    /// (リアクター, 停電, 通信, 酸素)
    /// </summary>
    /// <remarks>true = 直せる, false = 直せない</remarks>
    private static Dictionary<RoleId, (bool, bool, bool, bool)> SetFixSabotageDictionary = new();
    public static void ClearAndReload()
    {
        SetFixSabotageDictionary = new()
        {
            { RoleId.Fox, (false, false, false, false) },
            { RoleId.FireFox, (false, false, false, false) },
            { RoleId.God, (false,false, false, false) },
            { RoleId.OrientalShaman, (false, false, false, false) },
            { RoleId.Vampire, (true, false, true, true) },
            { RoleId.Dependents, (true, false, true, true) },
            { RoleId.Madmate, (true, CustomOptionHolder.MadRolesCanFixElectrical.GetBool(), CustomOptionHolder.MadRolesCanFixComms.GetBool(), true) },
        };
    }
    [HarmonyPatch(typeof(Console), nameof(Console.Use))]
    public static class ConsolsUsePatch
    {
        public static bool Prefix(Console __instance)
        {
            if (ModeHandler.IsMode(ModeId.SuperHostRoles)) return true;
            if (!(SetFixSabotageDictionary.ContainsKey(PlayerControl.LocalPlayer.GetRole()) || PlayerControl.LocalPlayer.IsMadRoles())) return true;
            __instance.CanUse(PlayerControl.LocalPlayer.Data, out var canUse, out var _);
            if (canUse) return IsBlocked(__instance.FindTask(CachedPlayer.LocalPlayer).TaskType, GetRole(PlayerControl.LocalPlayer.GetRole()));
            return true;
        }
    }
    [HarmonyPatch(typeof(UseButton), nameof(UseButton.SetTarget))]
    public static class UseButtonSetTargetPatch
    {
        public static bool Prefix(UseButton __instance, [HarmonyArgument(0)] IUsable target)
        {
            if (ModeHandler.IsMode(ModeId.SuperHostRoles)) return true;
            if (IsBlocked(target))
            {
                __instance.currentTarget = null;
                __instance.graphic.color = Palette.DisabledClear;
                __instance.graphic.material.SetFloat("_Desat", 0f);
                return false;
            }
            __instance.enabled = true;
            __instance.currentTarget = target;
            return true;
        }
    }
    private static bool IsBlocked(TaskTypes type, RoleId role)
    {
        if (ModeHandler.IsMode(ModeId.SuperHostRoles)) return true;
        if (!IsSabotage(type)) return true;
        if (!SetFixSabotageDictionary.ContainsKey(role)) return true;
        (bool, bool, bool, bool) fixSabotage = SetFixSabotageDictionary[role];
        if (type is TaskTypes.StopCharles or TaskTypes.ResetSeismic or TaskTypes.ResetReactor && fixSabotage.Item1) return true;
        if (type is TaskTypes.FixLights && fixSabotage.Item2) return true;
        if (type is TaskTypes.FixComms && fixSabotage.Item3) return true;
        if (type is TaskTypes.RestoreOxy && fixSabotage.Item4) return true;
        return false;
    }
    private static bool IsBlocked(IUsable target)
    {
        if (ModeHandler.IsMode(ModeId.SuperHostRoles)) return false;
        if (target == null) return false;
        Console console = target.TryCast<Console>();
        if (console != null && !IsBlocked(console.FindTask(CachedPlayer.LocalPlayer).TaskType, GetRole(PlayerControl.LocalPlayer.GetRole())))
            return true;
        return false;
    }
    private static bool IsSabotage(TaskTypes type) =>
        type is TaskTypes.StopCharles or TaskTypes.ResetSeismic or TaskTypes.ResetReactor or TaskTypes.FixLights or TaskTypes.FixComms or TaskTypes.RestoreOxy;
    private static RoleId GetRole(RoleId role)
    {
        if (SetFixSabotageDictionary.ContainsKey(role)) return role;
        else if (PlayerControl.LocalPlayer.IsMadRoles()) return RoleId.Madmate;
        return role;
    }

    public static class RepairProcsee
    {
        public static void ReceiptOfSabotageFixing(TaskTypes taskType)
        {
            if (taskType is TaskTypes.FixLights or TaskTypes.RestoreOxy or TaskTypes.ResetReactor or TaskTypes.ResetSeismic or TaskTypes.FixComms or TaskTypes.StopCharles)
            {
                if (ModeHandler.IsMode(ModeId.Default, ModeId.Werewolf)) FixingSabotageSNR(taskType);
                else FixingSabotageSHR(taskType);
            }
        }

        private static void FixingSabotageSNR(TaskTypes taskType)
        {
            switch (taskType)
            {
                case TaskTypes.FixLights:
                    RPCHelper.StartRPC(CustomRPC.FixLights).EndRPC();
                    RPCProcedure.FixLights();
                    break;
                case TaskTypes.RestoreOxy:
                    MapUtilities.CachedShipStatus.RpcRepairSystem(SystemTypes.LifeSupp, 0 | 64);
                    MapUtilities.CachedShipStatus.RpcRepairSystem(SystemTypes.LifeSupp, 1 | 64);
                    break;
                case TaskTypes.ResetReactor:
                    MapUtilities.CachedShipStatus.RpcRepairSystem(SystemTypes.Reactor, 16);
                    break;
                case TaskTypes.ResetSeismic:
                    MapUtilities.CachedShipStatus.RpcRepairSystem(SystemTypes.Laboratory, 16);
                    break;
                case TaskTypes.FixComms:
                    MapUtilities.CachedShipStatus.RpcRepairSystem(SystemTypes.Comms, 16 | 0);
                    MapUtilities.CachedShipStatus.RpcRepairSystem(SystemTypes.Comms, 16 | 1);
                    break;
                case TaskTypes.StopCharles:
                    MapUtilities.CachedShipStatus.RpcRepairSystem(SystemTypes.Reactor, 0 | 16);
                    MapUtilities.CachedShipStatus.RpcRepairSystem(SystemTypes.Reactor, 1 | 16);
                    break;
            }
        }

        private static void FixingSabotageSHR(TaskTypes taskType)
        {
            //Logger.Info($"{taskType}");

            bool IsSecondUnit = false;
            SystemTypes sabotageId = (SystemTypes)255;
            (int, int) amount = (0, 0);

            switch (taskType)
            {
                case TaskTypes.FixLights:
                    FixLigftsSHR();
                    break;
                case TaskTypes.RestoreOxy:
                    IsSecondUnit = true;
                    sabotageId = SystemTypes.LifeSupp;
                    amount = (0 | 64, 1 | 64);
                    break;
                case TaskTypes.ResetReactor:
                    sabotageId = SystemTypes.Reactor;
                    amount.Item1 = 16;
                    break;
                case TaskTypes.ResetSeismic:
                    sabotageId = SystemTypes.Laboratory;
                    amount.Item1 = 16;
                    break;
                case TaskTypes.FixComms:
                    IsSecondUnit = true;
                    sabotageId = SystemTypes.Comms;
                    amount = (16 | 0, 16 | 1);
                    break;
                case TaskTypes.StopCharles:
                    IsSecondUnit = true;
                    sabotageId = SystemTypes.Reactor;
                    amount = (0 | 16, 1 | 16);
                    break;
            }

            if ((byte)sabotageId != 255)
            {
                foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                {
                    if (player == null || player.IsBot()) continue; // [ ]MEMO : bot除外処理はリリース版コードmergeすれば必要なくなる為, プルリク前に消す
                    ClientData cd = player.GetClient();

                    MessageWriter SabotageFixWriter = AmongUsClient.Instance.StartRpcImmediately(MapUtilities.CachedShipStatus.NetId, (byte)RpcCalls.RepairSystem, SendOption.Reliable, cd.Id);
                    SabotageFixWriter.Write((byte)sabotageId);
                    MessageExtensions.WriteNetObject(SabotageFixWriter, player);
                    SabotageFixWriter.Write((byte)amount.Item1);
                    AmongUsClient.Instance.FinishRpcImmediately(SabotageFixWriter);

                    if (IsSecondUnit)
                    {
                        MessageWriter SabotageFixWriterSecond = AmongUsClient.Instance.StartRpcImmediately(MapUtilities.CachedShipStatus.NetId, (byte)RpcCalls.RepairSystem, SendOption.Reliable, cd.Id);
                        SabotageFixWriterSecond.Write((byte)sabotageId);
                        MessageExtensions.WriteNetObject(SabotageFixWriterSecond, player);
                        SabotageFixWriterSecond.Write((byte)amount.Item2);
                        AmongUsClient.Instance.FinishRpcImmediately(SabotageFixWriterSecond);
                    }
                }
            }
        }

        private static void FixLigftsSHR() { }
    }
}