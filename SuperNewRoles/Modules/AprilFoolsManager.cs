using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SuperNewRoles.Modules;
public static class AprilFoolsManager
{
    public enum ModMode
    {
        SuperNagaiRoles,
        SuperNyankoRoles,
        SuperNewRollcakes,
        SuperNemuiRolezzz,
        SNRVerTOH,
    }

    private static DateTime startTimeUtc_2023 = new(2023, 3, 31, 15, 0, 0, 0, DateTimeKind.Utc);
    private static DateTime endTimeUtc_2023 = new(2023, 4, 1, 15, 0, 0, 0, DateTimeKind.Utc);
    private static DateTime startTimeUtc_2024 = new(2024, 3, 31, 22, 0, 0, 0, DateTimeKind.Utc);
    private static DateTime endTimeUtc_2024 = new(2024, 4, 8, 7, 0, 0, 0, DateTimeKind.Utc);

    public static bool isLastAprilFool = false;

    public static ModMode currentModMode { get; private set; } = ModMode.SuperNagaiRoles;

    private static Dictionary<string, string> JokeModNames = new()
    {
        { ModMode.SuperNagaiRoles.ToString(), "SuperNagaiRoles" },
        { ModMode.SuperNyankoRoles.ToString(), "SuperNyankoRoles" },
        { ModMode.SuperNewRollcakes.ToString(), "SuperNewRollcakes" },
        { ModMode.SuperNemuiRolezzz.ToString(), "SuperNemuiRolezzz..." },
        { ModMode.SNRVerTOH.ToString(), "SuperNewRoles" }
    };
    private static Dictionary<string, string> JokeModNameOnColors = new()
    {
        { ModMode.SuperNagaiRoles.ToString(), $"<color=#ffa500>Super</color><color=#ff0000>Nagai</color><color=#00ff00>Roles</color>" },
        { ModMode.SuperNyankoRoles.ToString(), $"<color=#ffa500>Super</color><color=#ff0000>Nyanko</color><color=#00ff00>Roles</color>" },
        { ModMode.SuperNewRollcakes.ToString(), $"<color=#ffa500>Super</color><color=#ff0000>New</color><color=#00ff00>Rollcakes</color>" },
        { ModMode.SuperNemuiRolezzz.ToString(), $"<color=#ffa500>Super</color><color=#ff0000>Nemui</color><color=#00ff00>Rolezzz...</color>" },
        { ModMode.SNRVerTOH.ToString(), $"<color=#00bfff>SuperNewRoles</color>" }
    };
    private static Dictionary<string, float> ModPixels = new()
    {
        { ModMode.SuperNagaiRoles.ToString(), 197f },
        { ModMode.SuperNyankoRoles.ToString(), 160f },
        { ModMode.SuperNewRollcakes.ToString(), 156f },
        { ModMode.SuperNemuiRolezzz.ToString(), 156f },
        { ModMode.SNRVerTOH.ToString(), 150f }
    };
    private static Dictionary<string, float> ModYPos = new()
    {
        { ModMode.SuperNagaiRoles.ToString(), 1.3f },
        { ModMode.SuperNewRollcakes.ToString(), 1.1f }
    };
    private static Dictionary<string, Dictionary<string, string>> ModModeReplaces = new()
    {
        { ModMode.SuperNewRollcakes.ToString(),
            new(){
                { "役職", "ロールケーキ(役職)" },
                { " Role ", " Rollcake(Role) " }
            }
        },
    };

    private static bool IsForceApril2024 = false;

    private static Dictionary<string, Sprite> ModBanners = new();

    public const string DefaultModNameOnColor = "<color=#ffa500>Super</color><color=#ff0000>New</color><color=#00ff00>Roles</color>";

    public static List<ModMode> _enums = null;

    public static void SetRandomModMode()
    {
        if (!IsApril(2024))
            return;
        if (_enums == null || _enums.Count <= 0)
            _enums = ((ModMode[])Enum.GetValues(typeof(ModMode))).ToList();
        currentModMode = _enums.GetRandom();
        _enums.Remove(currentModMode);
        UpdateAprilTranslation();
    }

    public static float getCurrentBannerYPos()
    {
        if (IsApril(2024) && ModYPos.TryGetValue(currentModMode.ToString(), out float yPos))
            return yPos;
        return 0.7f;
    }

    public static Sprite getCurrentBanner()
    {
        if (IsApril(2024))
        {
            if (ModBanners == null)
                ModBanners = new();
            if (ModBanners.TryGetValue(currentModMode.ToString(), out Sprite banner))
                return banner;
            return ModBanners[currentModMode.ToString()] = ModHelpers.LoadSpriteFromResources($"SuperNewRoles.Resources.AprilFools2024.{currentModMode.ToString()}.png", ModPixels[currentModMode.ToString()]);
        }
        return null;
    }

    public static string getCurrentModName()
    {
        if (IsApril(2024))
            return JokeModNames[currentModMode.ToString()];
        return "SuperNewRoles";
    }

    public static string getCurrentModNameOnColor()
    {
        if (IsApril(2024))
            return JokeModNameOnColors[currentModMode.ToString()];
        return $"<color=#ffa500>Super</color><color=#ff0000>New</color><color=#00ff00>Roles</color>";
    }

    public static bool IsApril(int year)
    {
        DateTime startTimeUtc;
        DateTime endTimeUtc;
        DateTime utcNow = DateTime.UtcNow;
        switch (year)
        {
            case 2023:
                startTimeUtc = startTimeUtc_2023;
                endTimeUtc = endTimeUtc_2023;
                break;
            case 2024:
                if (IsForceApril2024)
                    return true;
                startTimeUtc = startTimeUtc_2024;
                endTimeUtc = endTimeUtc_2024;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(year), year, $"I don't have year '{year}'");
        }
        return utcNow >= startTimeUtc && utcNow <= endTimeUtc;
    }

    private static void UpdateAprilTranslation()
    {
        Logger.Info("Start UpdateAprilTranslation");
        ModTranslation.AprilDictionary = new();
        if (!ModModeReplaces.TryGetValue(currentModMode.ToString(), out Dictionary<string, string> replaces))
            replaces = new();
        replaces["/SuperNewRoles"] = "SNR_APRIL_TEMP_URL";
        replaces["SuperNewRoles"] = getCurrentModName();
        replaces[DefaultModNameOnColor] = getCurrentModNameOnColor();
        replaces["SNR_APRIL_TEMP_URL"] = "/SuperNewRoles";
        int index = 0;
        string newValue = null;
        foreach (var trans in ModTranslation.dictionary)
        {
            var newValues = new string[trans.Value.Length];
            index = 0;
            foreach (string transvalue in trans.Value)
            {
                newValue = transvalue;
                foreach (var replacedetail in replaces)
                {
                    newValue = newValue.Replace(replacedetail.Key, replacedetail.Value);
                }
                newValues[index] = newValue;
                index++;
            }
            ModTranslation.AprilDictionary[trans.Key] = newValues;
        }
        Logger.Info("End UpdateAprilTranslation");
    }
    public static void OnLoad()
    {
        isLastAprilFool = IsApril(2024);
    }
}
