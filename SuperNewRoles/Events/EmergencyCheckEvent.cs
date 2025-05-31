using System.Collections.Generic;
using HarmonyLib;
using SuperNewRoles.Modules.Events.Bases;

namespace SuperNewRoles.Events;

public class EmergencyCheckEventData : IEventData
{
    public bool RefEnabledEmergency;
    public List<string> RefEmergencyTexts = [];
    public List<string> RefNumberTexts = [];
    public EmergencyCheckEventData(bool enabledEmergency, List<string> emergencyTexts, List<string> numberTexts)
    {
        RefEnabledEmergency = enabledEmergency;
        RefEmergencyTexts = emergencyTexts;
        RefNumberTexts = numberTexts;
    }
}

public class EmergencyCheckEvent : EventTargetBase<EmergencyCheckEvent, EmergencyCheckEventData>
{
    public static void Invoke(ref bool enabledEmergency, ref List<string> emergencyTexts, ref List<string> numberTexts)
    {
        var data = new EmergencyCheckEventData(enabledEmergency, emergencyTexts, numberTexts);
        Instance.Awake(data);
        enabledEmergency = data.RefEnabledEmergency;
        emergencyTexts = data.RefEmergencyTexts;
        numberTexts = data.RefNumberTexts;
    }
}


[HarmonyPatch(typeof(EmergencyMinigame), nameof(EmergencyMinigame.Update))]
class EmergencyUpdatePatch
{
    public static void Postfix(EmergencyMinigame __instance)
    {
        bool enabledEmergency = true;
        List<string> emergencyTexts = [];
        List<string> numberTexts = [];
        EmergencyCheckEvent.Invoke(ref enabledEmergency, ref emergencyTexts, ref numberTexts);

        if (enabledEmergency) return; // バニラ判定を使用するなら 離脱

        __instance.StatusText.text = string.Join("\n", emergencyTexts);
        __instance.NumberText.text = string.Join("\n", numberTexts);

        bool buttonActive = enabledEmergency;

        __instance.state = buttonActive ? 1 : 2;
        __instance.ButtonActive = buttonActive;
        __instance.ClosedLid.gameObject.SetActive(!buttonActive);
        __instance.OpenLid.gameObject.SetActive(buttonActive);
    }
}