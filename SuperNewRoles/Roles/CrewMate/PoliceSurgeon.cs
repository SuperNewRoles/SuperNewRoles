using System;
using System.Collections.Generic;
using AmongUs.GameOptions;
using HarmonyLib;
using UnityEngine;
using SuperNewRoles.Mode;
using SuperNewRoles.Mode.SuperHostRoles;
using static SuperNewRoles.Modules.CustomOption;
using static SuperNewRoles.Roles.Crewmate.PoliceSurgeon;

namespace SuperNewRoles.Roles.Crewmate;
public static class PoliceSurgeon
{
    private static int optionId = 1266;
    public static CustomRoleOption PoliceSurgeonOption;
    public static CustomOption PoliceSurgeonPlayerCount;
    public static CustomOption PoliceSurgeonHaveVitalsInTaskPhase;
    public static CustomOption PoliceSurgeonVitalsDisplayCooldown;
    public static CustomOption PoliceSurgeonBatteryDuration;
    public static CustomOption PoliceSurgeonIndicateTimeOfDeathInSubsequentTurn;
    public static CustomOption PoliceSurgeonHowManyTurnAgoTheDied;
    public static CustomOption PoliceSurgeon_IncludeErrorInDeathTime;
    public static CustomOption PoliceSurgeonMarginOfErrorToIncludeInTimeOfDeath;

    public static void SetupCustomOptions()
    {
        PoliceSurgeonOption = SetupCustomRoleOption(optionId, true, RoleId.PoliceSurgeon); optionId++;
        PoliceSurgeonPlayerCount = Create(optionId, true, CustomOptionType.Crewmate, "SettingPlayerCountName", CustomOptionHolder.CrewPlayers[0], CustomOptionHolder.CrewPlayers[1], CustomOptionHolder.CrewPlayers[2], CustomOptionHolder.CrewPlayers[3], PoliceSurgeonOption); optionId++;
        PoliceSurgeonHaveVitalsInTaskPhase = Create(optionId, true, CustomOptionType.Crewmate, "PoliceSurgeonHaveVitalsInTaskPhase", false, PoliceSurgeonOption); optionId++;
        PoliceSurgeonVitalsDisplayCooldown = Create(optionId, true, CustomOptionType.Crewmate, "VitalsDisplayCooldown", 15f, 5f, 60f, 5f, PoliceSurgeonHaveVitalsInTaskPhase); optionId++;
        PoliceSurgeonBatteryDuration = Create(optionId, true, CustomOptionType.Crewmate, "BatteryDuration", 5f, 5f, 30f, 5f, PoliceSurgeonHaveVitalsInTaskPhase); optionId++;
        PoliceSurgeonIndicateTimeOfDeathInSubsequentTurn = Create(optionId, true, CustomOptionType.Crewmate, "PoliceSurgeonIndicateTimeOfDeathInSubsequentTurn", true, PoliceSurgeonOption); optionId++;
        PoliceSurgeonHowManyTurnAgoTheDied = Create(optionId, true, CustomOptionType.Crewmate, "PoliceSurgeonHowManyTurnAgoTheDied", false, PoliceSurgeonOption); optionId++;
        PoliceSurgeon_IncludeErrorInDeathTime = Create(optionId, true, CustomOptionType.Crewmate, "PoliceSurgeon_IncludeErrorInDeathTime", true, PoliceSurgeonOption); optionId++;
        PoliceSurgeonMarginOfErrorToIncludeInTimeOfDeath = Create(optionId, true, CustomOptionType.Crewmate, "PoliceSurgeonMarginOfErrorToIncludeInTimeOfDeath", 5f, 1f, 15f, 1f, PoliceSurgeon_IncludeErrorInDeathTime);
    }

    public static List<PlayerControl> PoliceSurgeonPlayer;
    public static Color32 color = new(137, 195, 235, byte.MaxValue);
    public static bool HaveVital;
    public static int MeetingTurn_Now; // ReportDeadBodyで代入している為 Host以外は正常に反映されていません (SNRはクライアント個人処理の為同時にRpcで送る必要がある)
    public static Dictionary<byte, (string, int)> PoliceSurgeon_ActualDeathTimes;
    public static void ClearAndReload()
    {
        PoliceSurgeonPlayer = new();
        HaveVital = PoliceSurgeonHaveVitalsInTaskPhase.GetBool();
        MeetingTurn_Now = 0;
        PoliceSurgeon_ActualDeathTimes = new();
    }

    /// <summary>
    /// Defaultモード時且つバイタルを有する設定の時に警察医を科学者置き換えにする
    /// </summary>
    public static void FixedUpdate()
    {
        if (!HaveVital) return;
        if (CachedPlayer.LocalPlayer.Data.Role == null || !CachedPlayer.LocalPlayer.IsRole(RoleTypes.Scientist))
        {
            VitalAbilityCoolSettings();
            new LateTask(() => { FastDestroyableSingleton<RoleManager>.Instance.SetRole(CachedPlayer.LocalPlayer, RoleTypes.Scientist); }, 0f, "ScientistSet");
        }
    }

    public static void VitalAbilityCoolSettings()
    {
        if (PlayerControl.LocalPlayer.GetRole() != RoleId.PoliceSurgeon) return;
        if (ModeHandler.IsMode(ModeId.SuperHostRoles, ModeId.CopsRobbers)) return;

        var optData = SyncSetting.OptionData.DeepCopy();

        optData.SetFloat(FloatOptionNames.ScientistCooldown, PoliceSurgeonVitalsDisplayCooldown.GetFloat());
        optData.SetFloat(FloatOptionNames.ScientistBatteryCharge, PoliceSurgeonBatteryDuration.GetFloat());

        GameManager.Instance.LogicOptions.SetGameOptions(optData);
    }
}

[Harmony]
internal static class PoliceSurgeon_AddActualDeathTime
{
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.ReportDeadBody)), HarmonyPostfix]
    internal static void ReportDeadBody_Postfix()
    {
        MeetingTurn_Now++;
        AddActualDeathTime((int)DeadTiming.TaskPhase);
    }

    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.CheckForEndVoting)), HarmonyPostfix]
    internal static void CheckForEndVoting_Postfix() => AddActualDeathTime((int)DeadTiming.MeetingPhase);

    [HarmonyPatch(typeof(ExileController), nameof(ExileController.WrapUp)), HarmonyPostfix]
    internal static void WrapUp_Postfix() => AddActualDeathTime((int)DeadTiming.Exited);

    [HarmonyPatch(typeof(AirshipExileController), nameof(AirshipExileController.WrapUpAndSpawn)), HarmonyPostfix]
    internal static void WrapUpAndSpawn_Postfix() => AddActualDeathTime((int)DeadTiming.Exited);

    /// <summary>
    /// 死亡推定時刻を保存する
    /// </summary>
    /// <param name="timingOfCall">死亡推定時刻を辞書に保存するメソッドが どこで呼び出されたか</param>
    private static void AddActualDeathTime(int timingOfCall)
    {
        if (!AmongUsClient.Instance.AmHost) return;
        if (PoliceSurgeonPlayer.Count <= 0) return;

        var reportTime = DateTime.Now;

        foreach (PlayerControl p in CachedPlayer.AllPlayers)
        {
            if (PoliceSurgeon_ActualDeathTimes.ContainsKey(p.PlayerId)) continue;
            if (p.IsAlive()) continue;

            string actualDeathTimeStr = null;

            switch (timingOfCall)
            {
                // MEMO:ターン表記オフなら MeetingTurn_NowとMeetingTurnが等しい時死亡時刻表記すればよい?

                case (int)DeadTiming.TaskPhase:
                    if (DyingMessenger.ActualDeathTime.ContainsKey(p.PlayerId)) actualDeathTimeStr = CalculateEstimatedTimeOfDeath(reportTime, p);
                    else actualDeathTimeStr = ModTranslation.GetString("PoliceSurgeonDeathTimeUnknown");
                    break;
                case (int)DeadTiming.MeetingPhase:
                    actualDeathTimeStr = ModTranslation.GetString("PoliceSurgeonDeathTimeMeetingPhase");
                    break;
                case (int)DeadTiming.Exited:
                    actualDeathTimeStr = ModTranslation.GetString("FinalStatusExiled");
                    break;
            }
            Logger.Info($"{p.name} : 死亡推定時刻_{actualDeathTimeStr}");
            PoliceSurgeon_ActualDeathTimes.Add(p.PlayerId, (actualDeathTimeStr, MeetingTurn_Now));
        }
    }

    /// <summary>
    /// 死亡推定時刻を辞書に保存するメソッドが どこで呼び出されたかを記す。
    /// </summary>
    private enum DeadTiming
    {
        TaskPhase,
        MeetingPhase,
        Exited,
    }

    /// <summary>
    /// 遺言伝達者の死亡時刻リストに乗っている死者の 死亡推定時刻の計算
    /// </summary>
    /// <param name="reportTime">死体が通報された時間</param>
    /// <param name="player">死者のPlayerControl</param>
    /// <returns>死亡推定時刻[s]をstring型で返却</returns>
    private static string CalculateEstimatedTimeOfDeath(DateTime reportTime, PlayerControl player)
    {
        DateTime actualDeathTime = DyingMessenger.ActualDeathTime[player.PlayerId].Item1;
        int seed = (int)actualDeathTime.Ticks;
        TimeSpan relativeDeathTime; // 相対死亡時刻 (ログ表記用)
        TimeSpan estimatedDeathTime; // 死亡推定時刻
        TimeSpan errorTimeSpan = new(0, 0, 0, 0); // 誤差秒数

        relativeDeathTime = estimatedDeathTime = reportTime - actualDeathTime; // 相対死亡時刻の計算 死亡推定時刻にも初期値(基準の値)として代入。

        if (PoliceSurgeon_IncludeErrorInDeathTime.GetBool())
        {
            errorTimeSpan = CalculateErrorTimeSpan(seed);
            estimatedDeathTime += errorTimeSpan;
            estimatedDeathTime = estimatedDeathTime >= TimeSpan.Zero ? estimatedDeathTime : -estimatedDeathTime;
        }

        Logger.Info($"{player.name} : 絶対死亡時刻_{actualDeathTime:hh:mm:ss} 通報時刻_{reportTime:ss}s 相対死亡時刻_{relativeDeathTime:ss}s 死亡推定時刻_{estimatedDeathTime:ss}s前 誤差設定_{PoliceSurgeon_IncludeErrorInDeathTime.GetBool()} 誤差範囲設定_{PoliceSurgeonMarginOfErrorToIncludeInTimeOfDeath.GetFloat()} 誤差秒数_{(errorTimeSpan >= TimeSpan.Zero ? "+" : "-")}{errorTimeSpan:ss}s");
        return $"{estimatedDeathTime:ss} s";
    }

    /// <summary>
    /// 死亡時刻に含める誤差を求める
    /// </summary>
    /// <param name="seed">乱数のシード値 (死亡時刻のミリ秒)</param>
    /// <param name="errorTimeSpan">含める誤差の絶対値</param>
    /// <param name="isPositive">true : 誤差が正の値 (誤差を含んだ値を求める時、真の値に加算する)</param>
    private static TimeSpan CalculateErrorTimeSpan(int seed)
    {
        int marginOfError = Mathf.Abs((int)PoliceSurgeonMarginOfErrorToIncludeInTimeOfDeath.GetFloat());
        System.Random random = new(seed);
        int error = random.Next(0, marginOfError * 2 + 1) - marginOfError;
        return new TimeSpan(0, 0, 0, error);
    }
}