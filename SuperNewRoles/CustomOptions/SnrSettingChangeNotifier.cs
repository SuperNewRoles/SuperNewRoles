using System;
using System.Linq;
using InnerNet;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles;
using TMPro;
using UnityEngine;

namespace SuperNewRoles.CustomOptions;

public static class SnrSettingChangeNotifier
{
    private const string Source = "SNR.SettingNotification";
    private const string FontOpen = "<font=\"Barlow-Black SDF\" material=\"Barlow-Black Outline\">";
    private const string FontClose = "</font>";
    private const string StandardCategoryKey = "SettingNotificationStandardCategory";
    private const string RoleAssignmentSettingKey = "SettingNotificationRoleAssignment";
    private const string UpdatedValueKey = "SettingNotificationUpdatedValue";
    private const string PathSeparator = " / ";
    private const int NotificationKeyBase = 100000;
    private static readonly Color32 ModifierContextColor = new(255, 112, 183, 255);

    public static void NotifyOptionChanged(CustomOption option, bool playSound = false)
    {
        if (option == null) return;

        var detail = GetOptionNotificationDetail(option);
        NotifySettingChanged(
            detail.Category,
            detail.Setting,
            option.GetCurrentSelectionString(),
            $"option:{option.Id}",
            playSound);
    }

    public static void NotifyRoleOptionChanged(RoleOptionManager.RoleOption roleOption, bool playSound = false)
    {
        if (roleOption == null) return;

        NotifySettingChanged(
            GetRoleCategory(roleOption.RoleId),
            ModTranslation.GetString(RoleAssignmentSettingKey),
            FormatRoleAssignment(roleOption.NumberOfCrews, roleOption.Percentage),
            $"role:{roleOption.RoleId}",
            playSound);
    }

    public static void NotifyGhostRoleOptionChanged(RoleOptionManager.GhostRoleOption roleOption, bool playSound = false)
    {
        if (roleOption == null) return;

        NotifySettingChanged(
            GetGhostRoleContext(roleOption.RoleId),
            ModTranslation.GetString(RoleAssignmentSettingKey),
            FormatRoleAssignment(roleOption.NumberOfCrews, roleOption.Percentage),
            $"ghost-role:{roleOption.RoleId}",
            playSound);
    }

    public static void NotifyModifierRoleOptionChanged(RoleOptionManager.ModifierRoleOption roleOption, bool playSound = false)
    {
        if (roleOption == null) return;

        NotifySettingChanged(
            GetModifierRoleContext(roleOption.ModifierRoleId),
            ModTranslation.GetString(RoleAssignmentSettingKey),
            FormatRoleAssignment(roleOption.NumberOfCrews, roleOption.Percentage),
            $"modifier:{roleOption.ModifierRoleId}",
            playSound);
    }

    public static void NotifyModifierRoleSettingChanged(RoleOptionManager.ModifierRoleOption roleOption, string settingName, string value, bool playSound = false)
    {
        if (roleOption == null) return;

        NotifySettingChanged(
            GetModifierRoleContext(roleOption.ModifierRoleId),
            settingName,
            value,
            $"modifier:{roleOption.ModifierRoleId}:{settingName}",
            playSound);
    }

    public static void NotifyExclusivitySettingsChanged(int index, bool playSound = false)
    {
        NotifyExclusivitySettingsChanged(
            index,
            ModTranslation.GetString("ExclusivityEditMenuTitle"),
            ModTranslation.GetString(UpdatedValueKey),
            playSound);
    }

    public static void NotifyExclusivitySettingsChanged(int index, string settingName, string value, bool playSound = false)
    {
        NotifySettingChanged(
            ModTranslation.GetString("ExclusivityOptionMenuTitle"),
            JoinPath(ModTranslation.GetString("ExclusivityOptionMenuGroupText", index + 1), settingName),
            value,
            $"exclusivity:{index}:{settingName}",
            playSound);
    }

    public static (string Label, string Value)? GetChangedModifierSetting(
        RoleOptionManager.ModifierRoleOption roleOption,
        byte numberOfCrews,
        int percentage,
        int maxImpostors,
        int impostorChance,
        int maxNeutrals,
        int neutralChance,
        int maxCrewmates,
        int crewmateChance)
    {
        if (roleOption == null) return null;

        if (roleOption.NumberOfCrews != numberOfCrews)
            return (ModTranslation.GetString("NumberOfCrews"), FormatCount(numberOfCrews));
        if (roleOption.Percentage != percentage)
            return (ModTranslation.GetString("AssignPer"), FormatPercent(percentage));
        if (roleOption.MaxImpostors != maxImpostors)
            return (ModTranslation.GetString("ModifierMaxImpostors"), FormatCount(maxImpostors));
        if (roleOption.ImpostorChance != impostorChance)
            return (ModTranslation.GetString("ModifierImpostorChance"), FormatPercent(impostorChance));
        if (roleOption.MaxNeutrals != maxNeutrals)
            return (ModTranslation.GetString("ModifierMaxNeutrals"), FormatCount(maxNeutrals));
        if (roleOption.NeutralChance != neutralChance)
            return (ModTranslation.GetString("ModifierNeutralChance"), FormatPercent(neutralChance));
        if (roleOption.MaxCrewmates != maxCrewmates)
            return (ModTranslation.GetString("ModifierMaxCrewmates"), FormatCount(maxCrewmates));
        if (roleOption.CrewmateChance != crewmateChance)
            return (ModTranslation.GetString("ModifierCrewmateChance"), FormatPercent(crewmateChance));

        return null;
    }

    public static bool ShouldPlayRemoteSound()
    {
        try
        {
            return AmongUsClient.Instance != null && AmongUsClient.Instance.AmConnected && !AmongUsClient.Instance.AmHost;
        }
        catch
        {
            return false;
        }
    }

    private static void NotifySettingChanged(string category, string setting, string value, string notificationKey, bool playSound)
    {
        if (!CanShowNotification()) return;

        var notifier = DestroyableSingleton<HudManager>.Instance.Notifier;
        string message = BuildMessage(category, setting, value);
        ShowMessage(notifier, notificationKey, message, playSound);
    }

    private static bool CanShowNotification()
    {
        try
        {
            return AmongUsClient.Instance != null
                && AmongUsClient.Instance.GameState == InnerNetClient.GameStates.Joined
                && DestroyableSingleton<HudManager>.InstanceExists
                && DestroyableSingleton<HudManager>.Instance.Notifier != null
                && DestroyableSingleton<TranslationController>.InstanceExists;
        }
        catch
        {
            return false;
        }
    }

    private static string BuildMessage(string category, string setting, string value)
    {
        string message = ModTranslation.GetString("SettingNotificationMessage", category, setting, value);
        return $"{FontOpen}{message}{FontClose}";
    }

    private static string FormatRoleAssignment(byte count, int percentage)
        => $"{FormatCount(count)} / {FormatPercent(percentage)}";

    private static string FormatCount(int count)
        => ModTranslation.GetString("NumberOfCrewsSelected", count);

    private static string FormatPercent(int percentage)
        => $"{percentage}%";

    private static void ShowMessage(NotificationPopper notifier, string notificationKey, string message, bool playSound)
    {
        string placeholder = $"{Source}:{notificationKey}:{Environment.TickCount}";
        notifier.AddSettingsChangeMessage(GetNotificationStringName(notificationKey), placeholder, playSound);
        ReplaceGeneratedMessage(placeholder, message);
    }

    private static StringNames GetNotificationStringName(string notificationKey)
    {
        unchecked
        {
            int hash = 216613626;
            string key = $"{Source}:{notificationKey}";
            foreach (char c in key)
            {
                hash ^= c;
                hash *= 16777619;
            }

            return (StringNames)(NotificationKeyBase + (hash & 0x0fffffff));
        }
    }

    private static void ReplaceGeneratedMessage(string placeholder, string message)
    {
        foreach (var text in UnityEngine.Object.FindObjectsOfType<TextMeshPro>(true))
        {
            if (text == null || string.IsNullOrEmpty(text.text)) continue;
            if (text.text.Contains(placeholder))
                text.text = message;
        }
    }

    private static (string Category, string Setting) GetOptionNotificationDetail(CustomOption option)
    {
        return (GetOptionCategoryPath(option), GetOptionSettingPath(option));
    }

    private static string GetOptionCategoryPath(CustomOption option)
    {
        if (option.ParentRole.HasValue)
            return GetRoleContext(option.ParentRole.Value);
        if (option.ParentGhostRole.HasValue)
            return GetGhostRoleContext(option.ParentGhostRole.Value);
        if (option.ParentModifierRole.HasValue)
            return GetModifierRoleContext(option.ParentModifierRole.Value);

        var category = FindOptionCategory(option);
        if (category != null && category.IsModifier)
            return Colorize(ModifierContextColor, ModTranslation.GetString(category.Name));

        return ModTranslation.GetString(StandardCategoryKey);
    }

    private static string GetOptionSettingPath(CustomOption option)
    {
        var names = new System.Collections.Generic.List<string>();
        for (CustomOption current = option; current != null; current = current.ParentOption)
        {
            names.Add(ModTranslation.GetString(current.Name));
        }

        names.Reverse();
        return JoinPath(names.ToArray());
    }

    private static CustomOptionCategory FindOptionCategory(CustomOption option)
    {
        var root = option;
        while (root.ParentOption != null)
        {
            root = root.ParentOption;
        }

        return CustomOptionManager.GetOptionCategories()
            .FirstOrDefault(category => category.Options.Contains(root) || category.Options.Contains(option));
    }

    private static string GetRoleCategory(RoleId roleId)
    {
        return GetRoleContext(roleId);
    }

    private static string GetRoleContext(RoleId roleId)
    {
        string name = ModTranslation.GetString(roleId.ToString());
        if (CustomRoleManager.TryGetRoleById(roleId, out var roleBase))
        {
            return Colorize(roleBase.RoleColor, name);
        }

        return name;
    }

    private static string GetGhostRoleContext(GhostRoleId roleId)
    {
        string name = ModTranslation.GetString(roleId.ToString());
        if (CustomRoleManager.TryGetGhostRoleById(roleId, out var roleBase))
        {
            return Colorize(roleBase.RoleColor, name);
        }

        return name;
    }

    private static string GetModifierRoleContext(ModifierRoleId roleId)
    {
        string name = ModTranslation.GetString(roleId.ToString());
        if (CustomRoleManager.TryGetModifierById(roleId, out var roleBase))
        {
            return Colorize(roleBase.RoleColor, name);
        }

        return Colorize(ModifierContextColor, name);
    }

    private static string Colorize(Color color, string text)
    {
        return ModHelpers.Cs(color, text);
    }

    private static string JoinPath(params string[] parts)
    {
        return string.Join(PathSeparator, parts.Where(part => !string.IsNullOrWhiteSpace(part)));
    }
}
