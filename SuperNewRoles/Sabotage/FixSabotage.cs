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
        /// <summary>
        /// リペア処理の受付, ここでモードの判別を行い, 適切な実行処理に飛ばす。
        /// </summary>
        /// <param name="taskType">リペアするタスク</param>
        public static void ReceiptOfSabotageFixing(TaskTypes taskType)
        {
            if (ModeHandler.IsMode(ModeId.Default, ModeId.Werewolf)) FixingSabotageClientMode(taskType);
            else FixingSabotageHostMode(taskType);
        }

        /// <summary>
        /// クライアントモードでのリペアの実行処理
        /// </summary>
        /// <param name="taskType">リペアするタスク</param>
        private static void FixingSabotageClientMode(TaskTypes taskType)
        {
            switch (taskType)
            {
                case TaskTypes.FixLights:
                    RPCHelper.StartRPC(CustomRPC.FixLights).EndRPC();
                    RPCProcedure.FixLights();
                    break;
                case TaskTypes.RestoreOxy: // 酸素 (Skeld, Mira)
                    MapUtilities.CachedShipStatus.RpcUpdateSystem(SystemTypes.LifeSupp, 0 | 64);
                    MapUtilities.CachedShipStatus.RpcUpdateSystem(SystemTypes.LifeSupp, 1 | 64);
                    break;
                case TaskTypes.ResetReactor: // リアクター (Skeld, Mira, Fungle)
                    MapUtilities.CachedShipStatus.RpcUpdateSystem(SystemTypes.Reactor, 16);
                    break;
                case TaskTypes.StopCharles: // 衝突回避 (Airship)
                    MapUtilities.CachedShipStatus.RpcUpdateSystem(SystemTypes.HeliSabotage, 0 | 16);
                    MapUtilities.CachedShipStatus.RpcUpdateSystem(SystemTypes.HeliSabotage, 1 | 16);
                    break;
                case TaskTypes.ResetSeismic: // 耐震 (Polus)
                    MapUtilities.CachedShipStatus.RpcUpdateSystem(SystemTypes.Laboratory, 16);
                    break;
                case TaskTypes.FixComms:
                    MapUtilities.CachedShipStatus.RpcUpdateSystem(SystemTypes.Comms, 16 | 0);
                    MapUtilities.CachedShipStatus.RpcUpdateSystem(SystemTypes.Comms, 16 | 1);
                    break;
                case TaskTypes.MushroomMixupSabotage:
                    MapUtilities.CachedShipStatus.RpcUpdateSystem(SystemTypes.MushroomMixupSabotage, 16 | 1);
                    break;
                default:
                    Logger.Info($"リペア処理が異常な呼び出しを受けました。", "Repair Process");
                    break;
            }
        }

        /// <summary>
        /// ホストモードでのリペアの実行処理
        /// </summary>
        /// <param name="taskType">リペアするタスク</param>
        private static void FixingSabotageHostMode(TaskTypes taskType)
        {
            SystemTypes sabotageId; // 初期化用にenumに割り当てのない数字を設定
            bool IsfixingSaboHere; // true : サボ修理が共通処理 / false :  サボ修理が特殊処理 (当メソッド外で処理) 又は 引数がサボでない
            bool IsSecondUnit = false;
            (int, int) amount = (0, 0);

            switch (taskType)
            {
                case TaskTypes.FixLights:
                    IsfixingSaboHere = false;
                    sabotageId = SystemTypes.Electrical;
                    FixLigftsSHR(); // 有効になっていないスイッチごとにRPCを飛ばす必要がある為, 特殊処理
                    break;
                case TaskTypes.RestoreOxy:
                    IsfixingSaboHere = true;
                    IsSecondUnit = true;
                    sabotageId = SystemTypes.LifeSupp;
                    amount = (0 | 64, 1 | 64);
                    break;
                case TaskTypes.ResetReactor:
                    IsfixingSaboHere = true;
                    sabotageId = SystemTypes.Reactor;
                    amount.Item1 = 16;
                    break;
                case TaskTypes.StopCharles:
                    IsfixingSaboHere = true;
                    IsSecondUnit = true;
                    sabotageId = SystemTypes.HeliSabotage;
                    amount = (0 | 16, 1 | 16);
                    break;
                case TaskTypes.ResetSeismic:
                    IsfixingSaboHere = true;
                    sabotageId = SystemTypes.Laboratory;
                    amount.Item1 = 16;
                    break;
                case TaskTypes.FixComms:
                    IsfixingSaboHere = true;
                    IsSecondUnit = true;
                    sabotageId = SystemTypes.Comms;
                    amount = (16 | 0, 16 | 1);
                    break;
                default:
                    IsfixingSaboHere = false;
                    Logger.Info($"リペア処理が異常な呼び出しを受けました。", "Repair Process");
                    sabotageId = (SystemTypes)255; // enumに割り当てされていない数字を設定
                    break;
            }

            if (!IsfixingSaboHere) return;

            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            {
                if (player == null || player.IsBot()) continue;
                ClientData cd = player.GetClient();

                MessageWriter SabotageFixWriter = AmongUsClient.Instance.StartRpcImmediately(MapUtilities.CachedShipStatus.NetId, (byte)RpcCalls.UpdateSystem, SendOption.Reliable, cd.Id);
                SabotageFixWriter.Write((byte)sabotageId);
                MessageExtensions.WriteNetObject(SabotageFixWriter, player);
                SabotageFixWriter.Write((byte)amount.Item1);
                AmongUsClient.Instance.FinishRpcImmediately(SabotageFixWriter);

                if (IsSecondUnit) // 2つ目のamountをが必要なものは送信する
                {
                    MessageWriter SabotageFixWriterSecond = AmongUsClient.Instance.StartRpcImmediately(MapUtilities.CachedShipStatus.NetId, (byte)RpcCalls.UpdateSystem, SendOption.Reliable, cd.Id);
                    SabotageFixWriterSecond.Write((byte)sabotageId);
                    MessageExtensions.WriteNetObject(SabotageFixWriterSecond, player);
                    SabotageFixWriterSecond.Write((byte)amount.Item2);
                    AmongUsClient.Instance.FinishRpcImmediately(SabotageFixWriterSecond);
                }
            }
        }

        private static void FixLigftsSHR()
        {
            if (!MapUtilities.Systems.ContainsKey(SystemTypes.Electrical))
                return;
            SwitchSystem switchSystem = MapUtilities.Systems[SystemTypes.Electrical].TryCast<SwitchSystem>();
            List<byte> amounts = new();
            for (int i = 0; i < SwitchSystem.NumSwitches; i++)
            {
                byte b = (byte)(1 << i);
                if ((switchSystem.ActualSwitches & b) != (switchSystem.ExpectedSwitches & b)) // オフになっているスイッチを
                {
                    amounts.Add(b); // Listに保存する
                }
            }
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            {
                if (player == null || player.IsBot()) continue;
                ClientData cd = player.GetClient();
                foreach (byte amount in amounts) // Listに保存されているスイッチを一気に押す
                {
                    MessageWriter SabotageFixWriter = AmongUsClient.Instance.StartRpcImmediately(MapUtilities.CachedShipStatus.NetId, (byte)RpcCalls.UpdateSystem, SendOption.Reliable, cd.Id);
                    SabotageFixWriter.Write((byte)SystemTypes.Electrical);
                    MessageExtensions.WriteNetObject(SabotageFixWriter, player);
                    SabotageFixWriter.Write(amount | 128);
                    AmongUsClient.Instance.FinishRpcImmediately(SabotageFixWriter);
                }
            }
        }
    }
}