using HarmonyLib;
using SuperNewRoles.CustomOption;
using SuperNewRoles.CustomRPC;

namespace SuperNewRoles.Roles
{
    class Workperson
    {
        [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.BeginCrewmate))]
        class BeginCrewmatePatch
        {
            public static void Postfix()
            {
                if (PlayerControl.LocalPlayer.IsRole(RoleId.Workperson))
                {
                    PlayerControl.LocalPlayer.GenerateAndAssignTasks(CustomOptions.WorkpersonCommonTask.GetFloat(), CustomOptions.WorkpersonShortTask.GetFloat(), CustomOptions.WorkpersonLongTask.GetFloat());
                }
            }
        }
    }
}