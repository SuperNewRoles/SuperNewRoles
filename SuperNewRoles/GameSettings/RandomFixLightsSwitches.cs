using System;
using HarmonyLib;
using SuperNewRoles.CustomOptions.Categories;
using SuperNewRoles.Modules;

namespace SuperNewRoles.GameSettings;

[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.UpdateSystem), [typeof(SystemTypes), typeof(PlayerControl), typeof(byte)])]
public static class RandomFixLightsSwitchesPatch
{
    public static void Prefix(SystemTypes systemType, ref byte amount)
    {
        if (!RandomFixLightsSwitches.ShouldRandomize(systemType, amount)) return;

        byte before = amount;
        amount = RandomFixLightsSwitches.CreateRandomDamageAmount(
            RandomFixLightsSwitches.NumSwitches,
            maxExclusive => ModHelpers.GetRandomInt(maxExclusive - 1),
            () => ModHelpers.GetRandomInt(1) == 1);
        Logger.Info($"FixLights damage randomized: amount={before}->{amount}", RandomFixLightsSwitches.LogTag);
    }
}

internal static class RandomFixLightsSwitches
{
    internal const int NumSwitches = 5;
    internal const byte DamageSystem = 128;
    internal const byte SwitchesMask = 31;
    private const int MaxSwitchesInByte = 8;
    internal const string LogTag = nameof(RandomFixLightsSwitches);

    internal static bool ShouldRandomize(SystemTypes systemType, byte amount)
    {
        if (!GameSettingOptions.RandomizeFixLightsSwitches) return false;
        if (AmongUsClient.Instance == null || !AmongUsClient.Instance.AmHost) return false;
        if (systemType != SystemTypes.Electrical) return false;

        return (amount & DamageSystem) != 0;
    }

    internal static byte CreateRandomDamageAmount(
        int switchCount,
        Func<int, int> nextSwitchIndex,
        Func<bool> drawAdditionalSwitch) =>
        (byte)(DamageSystem | CreateRandomDifferenceMask(switchCount, nextSwitchIndex, drawAdditionalSwitch));

    internal static byte CreateRandomActualSwitches(
        byte expectedSwitches,
        int switchCount,
        Func<int, int> nextSwitchIndex,
        Func<bool> drawAdditionalSwitch) =>
        (byte)((expectedSwitches ^ CreateRandomDifferenceMask(switchCount, nextSwitchIndex, drawAdditionalSwitch)) & CreateValidSwitchesMask(switchCount));

    private static byte CreateRandomDifferenceMask(
        int switchCount,
        Func<int, int> nextSwitchIndex,
        Func<bool> drawAdditionalSwitch)
    {
        if (switchCount is < 1 or > MaxSwitchesInByte)
        {
            throw new ArgumentOutOfRangeException(nameof(switchCount));
        }

        int guaranteedSwitch = nextSwitchIndex(switchCount);
        if (guaranteedSwitch < 0 || guaranteedSwitch >= switchCount)
        {
            throw new ArgumentOutOfRangeException(nameof(nextSwitchIndex));
        }

        byte differenceMask = (byte)(1 << guaranteedSwitch);
        for (int i = 0; i < switchCount; i++)
        {
            if (i == guaranteedSwitch) continue;
            if (drawAdditionalSwitch())
            {
                differenceMask |= (byte)(1 << i);
            }
        }

        return differenceMask;
    }

    private static byte CreateValidSwitchesMask(int switchCount) => (byte)((1 << switchCount) - 1);
}
