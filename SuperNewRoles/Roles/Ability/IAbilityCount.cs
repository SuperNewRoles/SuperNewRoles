using System;
using SuperNewRoles.Modules;

namespace SuperNewRoles.Roles.Ability;
public interface IAbilityCount
{
}

public static class AbilityCountExtensions
{
    public static void UseAbilityCount(this AbilityBase ability)
    {
        if (ability is IAbilityCount abilityCount)
        {
            if (!ability.HasCount) return;
            RpcIncreaseAbilityCount(ability.Player, ability.AbilityId);
        }
        else
        {
            throw new Exception("ability is not IAbilityCount");
        }
    }

    [CustomRPC]
    public static void RpcIncreaseAbilityCount(ExPlayerControl exPlayer, ulong abilityId)
    {
        AbilityBase ability = exPlayer.GetAbility(abilityId);
        if (ability is IAbilityCount abilityCount)
        {
            ability.Count--;
        }
        else
        {
            Logger.Error("ability is null or not IAbilityCount");
        }
    }
}