using HarmonyLib;
using SuperNewRoles.Roles.Crewmate;

namespace SuperNewRoles.Patches;

[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.CheckForEndVoting))]
public static class CheckForEndVotingPatch
{
    public static bool Prefix(MeetingHud __instance)
    {
        if (BalancerAbility.BalancingAbility != null && BalancerAbility.BalancingAbility.Player != null && BalancerAbility.currentMeetingHud == __instance)
        {
            return false;
        }
        return true;
    }
}