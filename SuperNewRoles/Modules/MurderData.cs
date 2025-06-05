using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace SuperNewRoles.Modules;

public class MurderData
{
    public ExPlayerControl Killer { get; }
    public ExPlayerControl Target { get; }
    public Vector3 DeathPosition { get; }
    public MurderData(ExPlayerControl killer, ExPlayerControl target)
    {
        Killer = killer;
        Target = target;
        DeathPosition = target.transform.position;
    }
}

public static class MurderDataManager
{
    public static List<MurderData> MurderDataList { get; } = new();
    public static Dictionary<ExPlayerControl, List<MurderData>> KillerMurderDataDict { get; } = new();
    public static Dictionary<ExPlayerControl, MurderData> TargetMurderDataDict { get; } = new();

    public static void AddMurderData(ExPlayerControl killer, ExPlayerControl target)
    {
        var murderData = new MurderData(killer, target);
        MurderDataList.Add(murderData);
        KillerMurderDataDict.TryAdd(killer, new());
        KillerMurderDataDict[killer].Add(murderData);
        TargetMurderDataDict[target] = murderData;
    }

    public static bool TryGetMurderData(ExPlayerControl target, out MurderData murderData)
            => TargetMurderDataDict.TryGetValue(target, out murderData);

    public static void RevivedMurderData(ExPlayerControl revivedPlayer)
    {
        MurderDataList.RemoveAll(x => x.Target == revivedPlayer);
        foreach (var killer in KillerMurderDataDict.Values)
        {
            killer.RemoveAll(x => x.Target == revivedPlayer);
        }
        TargetMurderDataDict.Remove(revivedPlayer);
    }

    public static void ClearAndReloads()
    {
        MurderDataList.Clear();
        KillerMurderDataDict.Clear();
        TargetMurderDataDict.Clear();
    }
    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.CoStartGame))]
    public static class AmongUsClientCoStartGamePatch
    {
        public static void Postfix()
        {
            ClearAndReloads();
        }
    }
}