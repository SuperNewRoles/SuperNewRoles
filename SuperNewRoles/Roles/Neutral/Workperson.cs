using HarmonyLib;

namespace SuperNewRoles.Roles;

class Workperson
{
    [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.BeginCrewmate))]
    class BeginCrewmatePatch
    {
        public static void Postfix()
        {
            if (PlayerControl.LocalPlayer.IsRole(RoleId.Workperson))
            {
                PlayerControl.LocalPlayer.GenerateAndAssignTasks(CustomOptionHolder.WorkpersonCommonTask.GetInt(), CustomOptionHolder.WorkpersonShortTask.GetInt(), CustomOptionHolder.WorkpersonLongTask.GetInt());
            }
        }
    }
}
