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
                if (CachedPlayer.LocalPlayer.PlayerControl.IsRole(RoleId.Workperson))
                {
                    CachedPlayer.LocalPlayer.PlayerControl.GenerateAndAssignTasks(CustomOptions.WorkpersonCommonTask.GetInt(), CustomOptions.WorkpersonShortTask.GetInt(), CustomOptions.WorkpersonLongTask.GetInt());
                }
            }
        }
    }
}