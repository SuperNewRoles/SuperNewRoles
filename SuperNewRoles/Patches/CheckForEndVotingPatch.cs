using HarmonyLib;
using SuperNewRoles.Roles.Crewmate;

namespace SuperNewRoles.Patches;

[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.CheckForEndVoting))]
public static class CheckForEndVotingPatch
{
    public static bool Prefix()
    {
        if (BalancerAbility.BalancingAbility != null)
        {
            return false;
        }
        return true;
    }
}