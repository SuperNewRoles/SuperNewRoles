using HarmonyLib;
using SuperNewRoles.CustomOptions.Categories;

namespace SuperNewRoles.Patches;

public static class ReportDistancePatch
{
    public static void Init()
    {
        if (!GameSettingOptions.ChangeReportDistance) return;
        foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            player.MaxReportDistance = GameSettingOptions.ReportDistanceOption;
    }
}