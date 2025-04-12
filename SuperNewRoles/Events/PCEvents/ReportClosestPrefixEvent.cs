using HarmonyLib;
using SuperNewRoles.Modules.Events.Bases;

namespace SuperNewRoles.Events.PCEvents;

public class ReportClosestPrefixEventData : IEventData
{
    public bool CanReportClosest;
    public ReportClosestPrefixEventData(bool canReportClosest)
    {
        CanReportClosest = canReportClosest;
    }
}
public class ReportClosestPrefixEvent : EventTargetBase<ReportClosestPrefixEvent, ReportClosestPrefixEventData>
{
    public static void Invoke(ref bool canReportClosest)
    {
        var data = new ReportClosestPrefixEventData(canReportClosest);
        Instance.Awake(data);
        canReportClosest = data.CanReportClosest;
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.ReportClosest))]
class ReportClosestPatch
{
    public static bool Prefix(PlayerControl __instance)
    {
        bool canReportClosest = true;
        ReportClosestPrefixEvent.Invoke(ref canReportClosest);
        return canReportClosest;
    }
}