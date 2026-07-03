using System;
using System.Collections.Generic;
using HarmonyLib;
using SuperNewRoles.CustomOptions.Categories;
using SuperNewRoles.Modules;

namespace SuperNewRoles.GameSettings;

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.StartMeeting))]
public static class EmergencyMeetingLimitStartMeetingPatch
{
    public static void Postfix(NetworkedPlayerInfo target)
    {
        EmergencyMeetingLimit.RecordMeeting(target);
    }
}

[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.CoStartGame))]
public static class EmergencyMeetingLimitCoStartGamePatch
{
    public static void Postfix()
    {
        EmergencyMeetingLimit.Reset();
    }
}

internal static class EmergencyMeetingLimit
{
    internal static int EmergencyCount { get; private set; }
    internal static bool IsLimited => GameSettingOptions.IsLimitEmergencyMeeting;
    internal static int MaxCount => Math.Max(0, GameSettingOptions.EmergencyMeetingLimitCount);
    internal static int RemainingCount => Math.Max(0, MaxCount - EmergencyCount);
    internal static bool IsLimitReached => IsLimited && RemainingCount <= 0;

    internal static void Reset()
    {
        EmergencyCount = 0;
    }

    internal static void RecordMeeting(NetworkedPlayerInfo target)
    {
        if (target == null)
            EmergencyCount++;
    }

    internal static bool ShouldBlockMeeting(NetworkedPlayerInfo target) =>
        target == null && IsLimitReached;

    internal static void ApplyEmergencyCheck(
        ref bool useVanilla,
        ref bool enabledEmergency,
        List<string> emergencyTexts,
        List<string> numberTexts)
    {
        if (!enabledEmergency || !IsLimited)
            return;

        if (IsLimitReached)
        {
            useVanilla = false;
            enabledEmergency = false;
            emergencyTexts.Add(ModTranslation.GetString("MeetingStatusUpperLimitReached"));
            numberTexts.Clear();
            return;
        }

        if (!CanShowRemainingCount())
            return;

        useVanilla = false;
        enabledEmergency = PlayerControl.LocalPlayer != null && PlayerControl.LocalPlayer.RemainingEmergencies > 0;

        string playerName = PlayerControl.LocalPlayer.Data?.PlayerName ?? PlayerControl.LocalPlayer.name;
        string personalCount = $"<color=#fe1919>{PlayerControl.LocalPlayer.RemainingEmergencies}</color>";
        string allCount = $"<color=#fe1919>{RemainingCount}</color>";
        emergencyTexts.Add(ModTranslation.GetString("MeetingStatusPersonalEmergencyCount", playerName, personalCount));
        emergencyTexts.Add("");
        emergencyTexts.Add(ModTranslation.GetString("MeetingStatusAllEmergencyCount", allCount));
        numberTexts.Clear();
    }

    private static bool CanShowRemainingCount()
    {
        if (PlayerControl.LocalPlayer == null || ShipStatus.Instance == null)
            return false;
        if (ShipStatus.Instance.Timer < 15f || ShipStatus.Instance.EmergencyCooldown > 0f)
            return false;

        return !ModHelpers.IsSabotageAvailable();
    }
}
