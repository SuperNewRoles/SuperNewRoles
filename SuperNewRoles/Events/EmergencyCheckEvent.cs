using System.Collections.Generic;
using HarmonyLib;
using SuperNewRoles.GameSettings;
using SuperNewRoles.Modules.Events.Bases;

namespace SuperNewRoles.Events;

public class EmergencyCheckEventData : IEventData
{
    public bool RefEnabledEmergency;
    public List<string> RefEmergencyTexts;
    public List<string> RefNumberTexts;
    public EmergencyCheckEventData(bool enabledEmergency, List<string> emergencyTexts, List<string> numberTexts)
    {
        RefEnabledEmergency = enabledEmergency;
        RefEmergencyTexts = emergencyTexts;
        RefNumberTexts = numberTexts;
    }
}

public class EmergencyCheckEvent : EventTargetBase<EmergencyCheckEvent, EmergencyCheckEventData>
{
    private static readonly Stack<EmergencyCheckEventData> DataPool = new();

    private static EmergencyCheckEventData RentData(bool enabledEmergency, List<string> emergencyTexts, List<string> numberTexts)
    {
        if (!DataPool.TryPop(out var data))
            return new EmergencyCheckEventData(enabledEmergency, emergencyTexts, numberTexts);

        data.RefEnabledEmergency = enabledEmergency;
        data.RefEmergencyTexts = emergencyTexts;
        data.RefNumberTexts = numberTexts;
        return data;
    }

    private static void ReturnData(EmergencyCheckEventData data)
    {
        data.RefEmergencyTexts = null;
        data.RefNumberTexts = null;
        DataPool.Push(data);
    }

    public static void Invoke(ref bool enabledEmergency, ref List<string> emergencyTexts, ref List<string> numberTexts)
    {
        var evt = Instance;
        if (!evt.HasListeners)
            return;

        var data = RentData(enabledEmergency, emergencyTexts, numberTexts);
        try
        {
            evt.Awake(data);
            enabledEmergency = data.RefEnabledEmergency;
            emergencyTexts = data.RefEmergencyTexts;
            numberTexts = data.RefNumberTexts;
        }
        finally
        {
            ReturnData(data);
        }
    }
}


[HarmonyPatch(typeof(EmergencyMinigame), nameof(EmergencyMinigame.Update))]
class EmergencyUpdatePatch
{
    private static readonly Stack<List<string>> EmergencyTextPool = new();
    private static readonly Stack<List<string>> NumberTextPool = new();

    private static List<string> RentTextList(Stack<List<string>> pool)
    {
        if (!pool.TryPop(out var list))
            list = new();
        list.Clear();
        return list;
    }

    private static void ReturnTextList(Stack<List<string>> pool, List<string> list)
    {
        if (list == null)
            return;
        list.Clear();
        pool.Push(list);
    }

    public static void Postfix(EmergencyMinigame __instance)
    {
        bool enabledEmergency = true;
        List<string> emergencyTexts = RentTextList(EmergencyTextPool);
        List<string> numberTexts = RentTextList(NumberTextPool);
        try
        {
            EmergencyCheckEvent.Invoke(ref enabledEmergency, ref emergencyTexts, ref numberTexts);
            bool useVanilla = enabledEmergency;
            EmergencyMeetingLimit.ApplyEmergencyCheck(ref useVanilla, ref enabledEmergency, emergencyTexts, numberTexts);

            if (useVanilla) return; // バニラ判定を使用するなら 離脱

            __instance.StatusText.text = string.Join("\n", emergencyTexts);
            __instance.NumberText.text = string.Join("\n", numberTexts);

            bool buttonActive = enabledEmergency;

            __instance.state = buttonActive ? 1 : 2;
            __instance.ButtonActive = buttonActive;
            __instance.ClosedLid.gameObject.SetActive(!buttonActive);
            __instance.OpenLid.gameObject.SetActive(buttonActive);
        }
        finally
        {
            ReturnTextList(EmergencyTextPool, emergencyTexts);
            ReturnTextList(NumberTextPool, numberTexts);
        }
    }
}
