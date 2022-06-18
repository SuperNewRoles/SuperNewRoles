using HarmonyLib;
using SuperNewRoles.CustomOption;

namespace SuperNewRoles.Roles
{
    class Workperson
    {
        [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.BeginCrewmate))]
        class BeginCrewmatePatch
        {
            public static void Postfix(ShipStatus __instance)
            {
                if (PlayerControl.LocalPlayer.isRole(CustomRPC.RoleId.Workperson))
                {
                    PlayerControl.LocalPlayer.generateAndAssignTasks((int)CustomOptions.WorkpersonCommonTask.getFloat(), (int)CustomOptions.WorkpersonShortTask.getFloat(), (int)CustomOptions.WorkpersonLongTask.getFloat());
                }
            }
        }
    }
}
