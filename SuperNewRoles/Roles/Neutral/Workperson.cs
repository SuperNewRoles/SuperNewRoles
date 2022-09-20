using HarmonyLib;



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
                    PlayerControl.LocalPlayer.GenerateAndAssignTasks(CustomOptions.WorkpersonCommonTask.GetInt(), CustomOptions.WorkpersonShortTask.GetInt(), CustomOptions.WorkpersonLongTask.GetInt());
                }
            }
        }
    }
}