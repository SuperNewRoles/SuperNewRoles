using System;
using System.Linq;
using HarmonyLib;
using SuperNewRoles.Modules;
using SuperNewRoles.CustomOptions.Categories;

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
        if (ExPlayerControl.LocalPlayer.IsDead()) // 幽霊の場合
        {
            switch (GameSettingOptions.GhostVoteDisplay)
            {
                case GhostVoteDisplayType.Show:
                    __result = false; // 投票を表示 (匿名投票を無効)
                    return false;
                case GhostVoteDisplayType.Hide:
                    __result = true; // 投票を非表示 (匿名投票を有効)
                    return false;
                case GhostVoteDisplayType.Vanilla:
                default:
                    return true; // バニラの動作に従う
            }
        }
        else if (ExPlayerControl.LocalPlayer.HasAbility<KnowVoteAbility>()) // 生存していてKnowVoteAbilityを持つ場合
        {
            __result = ExPlayerControl.LocalPlayer.PlayerAbilities.Any(x => x is KnowVoteAbility knowVoteAbility && knowVoteAbility.IsAnonymousVotes());
            return false;
        }
        return true; // 通常の処理
    }
}
