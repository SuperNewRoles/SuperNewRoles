using System;
using System.Linq;
using HarmonyLib;
using SuperNewRoles.Modules;

namespace SuperNewRoles.Roles.Ability;

class KnowVoteAbility : AbilityBase
{
    private readonly Func<bool> _isAnonymousVotes;
    public KnowVoteAbility(Func<bool> isAnonymousVotes)
    {
        _isAnonymousVotes = isAnonymousVotes;
    }
    public bool IsAnonymousVotes()
    {
        return _isAnonymousVotes?.Invoke() ?? true;
    }
}
[HarmonyPatch(typeof(LogicOptionsNormal), nameof(LogicOptionsNormal.GetAnonymousVotes))]
public static class LogicOptionsNormalGetAnonymousVotesPatch
{
    public static bool Prefix(ref bool __result)
    {
        if (ExPlayerControl.LocalPlayer.HasAbility(nameof(KnowVoteAbility)))
        {
            __result = ExPlayerControl.LocalPlayer.PlayerAbilities.Any(x => x is KnowVoteAbility knowVoteAbility && knowVoteAbility.IsAnonymousVotes());
            return false;
        }
        return true;
    }
}
