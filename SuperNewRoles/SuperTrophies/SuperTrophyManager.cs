using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SuperNewRoles.Roles;
using SuperNewRoles.Roles.Ability;

namespace SuperNewRoles.SuperTrophies;

public static class SuperTrophyManager
{
    public static List<ISuperTrophy> trophies { get; set; }
    public static void Load()
    {
        Logger.Info("SuperTrophyManager: Load");
        trophies = new();
        return;
        var allTypes = SuperNewRolesPlugin.Assembly.GetTypes().Where(x => IsSuperTrophyType(x)).ToList();
        SuperNewRolesPlugin.Logger.LogInfo($"[Splash] Found {allTypes.Count} super trophy types");
        int index = 0;
        foreach (var type in allTypes)
        {
            index++;
            SuperNewRolesPlugin.Logger.LogInfo($"[Splash] Loading super trophy ({index}/{allTypes.Count}): {type.Name}");
            Logger.Info($"SuperTrophyManager: Load {type.FullName}");
            var instance = (ISuperTrophy)type.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)?.GetValue(null);
            if (instance != null)
            {
                trophies.Add(instance);
                Logger.Info($"Loaded trophy: {type.FullName}");
            }
            else
            {
                Logger.Error($"Failed to get instance for trophy: {type.FullName}");
            }
        }
        SuperNewRolesPlugin.Logger.LogInfo($"[Splash] SuperTrophyManager: Loaded {trophies.Count} trophies");
        // トロフィーデータを読み込む
        SuperTrophySaver.Initialize();
    }
    private static bool IsSuperTrophyType(Type type)
    {
        if (type == null || type.IsAbstract || type.IsInterface)
            return false;

        if (typeof(ISuperTrophy).IsAssignableFrom(type))
        {
            var baseType = type.BaseType;
            while (baseType != null)
            {
                if (baseType.IsGenericType)
                {
                    var genericTypeDef = baseType.GetGenericTypeDefinition();
                    if (genericTypeDef == typeof(SuperTrophyBase<>) ||
                        genericTypeDef == typeof(SuperTrophyAbility<>) ||
                        genericTypeDef == typeof(SuperTrophyRole<>))
                    {
                        return true;
                    }
                }
                baseType = baseType.BaseType;
            }
        }
        return false;
    }
    private static List<ISuperTrophy> willComplete = new();
    public static void CompleteTrophy(ISuperTrophy trophy)
    {
        if (willComplete.Contains(trophy) || trophy.Completed) return;
        // トロフィーを獲得したことをログに記録
        Logger.Info($"Trophy '{trophy.TrophyId}' has been completed.");
        willComplete.Add(trophy);
    }
    public static void InCompleteTrophy(ISuperTrophy trophy)
    {
        if (!willComplete.Contains(trophy) || !trophy.Completed) return;
        willComplete.Remove(trophy);
    }
    public static void CoStartGame()
    {
        willComplete.Clear();
    }
    public static void OnEndGame()
    {
        foreach (var trophy in willComplete)
        {
            trophy.Completed = true;
            Logger.Info($"Trophy '{trophy.TrophyId}' has been completed and will be saved.");
        }

        // トロフィーデータを保存
        SuperTrophySaver.SaveData();
        GotTrophyUI.Initialize(willComplete);
    }
    public static void RegisterTrophy(AbilityBase ability)
    {
        return;
        foreach (var trophy in trophies)
        {
            if (trophy.Completed) continue;
            if (trophy is ISuperTrophyAbility superTrophyAbility)
            {
                if (superTrophyAbility.TargetAbilities.Contains(ability.GetType()))
                {
                    superTrophyAbility.OnRegister();
                }
            }
        }
    }
    public static void DetachTrophy(AbilityBase ability)
    {
        return;
        foreach (var trophy in trophies)
        {
            if (trophy.Completed) continue;
            if (trophy is ISuperTrophyAbility superTrophyAbility)
            {
                if (superTrophyAbility.TargetAbilities.Contains(ability.GetType()))
                {
                    superTrophyAbility.OnDetached();
                }
            }
        }
    }
    public static void DetachTrophy(List<AbilityBase> abilities)
    {
        return;
        foreach (var ability in abilities)
        {
            DetachTrophy(ability);
        }
    }
    public static void RegisterTrophy(RoleId roleId)
    {
        foreach (var trophy in trophies)
        {
            if (trophy.Completed) continue;
            if (trophy is ISuperTrophyRole superTrophyRole)
            {
                if (superTrophyRole.TargetRoles.Contains(roleId))
                {
                    superTrophyRole.OnRegister();
                }
            }
        }
    }
    public static void RegisterTrophy(ModifierRoleId modifierRoleId)
    {
        return;
        foreach (var trophy in trophies)
        {
            if (trophy.Completed) continue;
            if (trophy is ISuperTrophyModifier superTrophyModifier)
            {
                if (superTrophyModifier.TargetModifiers.Contains(modifierRoleId))
                {
                    superTrophyModifier.OnRegister();
                }
            }
        }
    }
    public static void RegisterTrophy(GhostRoleId ghostRoleId)
    {
        return;
        foreach (var trophy in trophies)
        {
            if (trophy.Completed) continue;
            if (trophy is ISuperTrophyGhostRole superTrophyGhostRole)
            {
                if (superTrophyGhostRole.TargetGhostRoles.Contains(ghostRoleId))
                {
                    superTrophyGhostRole.OnRegister();
                }
            }
        }
    }
    public static void DetachTrophy(RoleId roleId)
    {
        return;
        foreach (var trophy in trophies)
        {
            if (trophy.Completed) continue;
            if (trophy is ISuperTrophyRole superTrophyRole)
            {
                if (superTrophyRole.TargetRoles.Contains(roleId))
                {
                    superTrophyRole.OnDetached();
                }
            }
        }
    }
    public static void DetachTrophy(GhostRoleId ghostRoleId)
    {
        return;
        foreach (var trophy in trophies)
        {
            if (trophy.Completed) continue;
            if (trophy is ISuperTrophyGhostRole superTrophyGhostRole)
            {
                if (superTrophyGhostRole.TargetGhostRoles.Contains(ghostRoleId))
                {
                    superTrophyGhostRole.OnDetached();
                }
            }
        }
    }
}
