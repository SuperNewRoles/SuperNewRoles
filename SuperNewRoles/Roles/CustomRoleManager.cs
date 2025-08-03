using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SuperNewRoles.Modules;

namespace SuperNewRoles.Roles;

public static class CustomRoleManager
{
    public static IRoleBase[] AllRoles { get; private set; }
    public static IModifierBase[] AllModifiers { get; private set; }
    public static IGhostRoleBase[] AllGhostRoles { get; private set; }
    public static Dictionary<int, IRoleBase> AllRolesByRoleId { get; private set; }
    public static Dictionary<int, IModifierBase> AllModifiersByModifierRoleId { get; private set; }
    public static Dictionary<int, IGhostRoleBase> AllGhostRolesByRoleId { get; private set; }
    public static void Load()
    {
        SuperNewRolesPlugin.Logger.LogInfo("[Splash] Loading Roles...");
        int loadedRoles = 0;
        int loadedModifiers = 0;
        int loadedGhostRoles = 0;
        AllRoles = SuperNewRolesPlugin.Assembly.GetTypes()
            // まずIRoleBaseインターフェースを実装している型を取得
            .Where(type => typeof(IRoleBase).IsAssignableFrom(type))
            // 次にRoleBase<T>を継承している型に絞る（直接の継承をチェック）
            .Where(type => type.BaseType != null &&
                           type.BaseType.IsGenericType &&
                           type.BaseType.GetGenericTypeDefinition() == typeof(RoleBase<>))
            // さらにBaseSingletonがついている型なので、BaseSingleton<T>のInstanceプロパティを取得する
            .Select(type =>
            {
                loadedRoles++;
                SuperNewRolesPlugin.Logger.LogInfo($"[Splash] Loading role {loadedRoles}: {type.Name}");
                var baseSingletonType = typeof(BaseSingleton<>).MakeGenericType(type);
                var instanceProperty = baseSingletonType.GetProperty("Instance", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                if (instanceProperty == null)
                {
                    throw new InvalidOperationException($"Type {type.FullName} does not have a public static Instance property.");
                }
                return (IRoleBase)instanceProperty.GetValue(null);
            })
            .ToArray();
        AllModifiers = SuperNewRolesPlugin.Assembly.GetTypes()
            .Where(type => typeof(IModifierBase).IsAssignableFrom(type))
            .Where(type => type.BaseType != null &&
                           type.BaseType.IsGenericType &&
                           type.BaseType.GetGenericTypeDefinition() == typeof(ModifierBase<>))
            .Select(type =>
            {
                loadedModifiers++;
                SuperNewRolesPlugin.Logger.LogInfo($"[Splash] Loading modifier {loadedModifiers}: {type.Name}");
                var baseSingletonType = typeof(BaseSingleton<>).MakeGenericType(type);
                var instanceProperty = baseSingletonType.GetProperty("Instance", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                if (instanceProperty == null)
                {
                    throw new InvalidOperationException($"Type {type.FullName} does not have a public static Instance property.");
                }
                return (IModifierBase)instanceProperty.GetValue(null);
            })
            .ToArray();
        AllGhostRoles = SuperNewRolesPlugin.Assembly.GetTypes()
            .Where(type => typeof(IGhostRoleBase).IsAssignableFrom(type))
            .Where(type => type.BaseType != null &&
                           type.BaseType.IsGenericType &&
                           type.BaseType.GetGenericTypeDefinition() == typeof(GhostRoleBase<>))
            .Select(type =>
            {
                loadedGhostRoles++;
                SuperNewRolesPlugin.Logger.LogInfo($"[Splash] Loading ghost role {loadedGhostRoles}: {type.Name}");
                var baseSingletonType = typeof(BaseSingleton<>).MakeGenericType(type);
                var instanceProperty = baseSingletonType.GetProperty("Instance", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                if (instanceProperty == null)
                {
                    throw new InvalidOperationException($"Type {type.FullName} does not have a public static Instance property.");
                }
                return (IGhostRoleBase)instanceProperty.GetValue(null);
            })
            .ToArray();

        AllRolesByRoleId = AllRoles.ToDictionary(role => (int)role.Role);
        AllModifiersByModifierRoleId = AllModifiers.ToDictionary(modifier => (int)modifier.ModifierRole);
        AllGhostRolesByRoleId = AllGhostRoles.ToDictionary(ghostRole => (int)ghostRole.Role);
        SuperNewRolesPlugin.Logger.LogInfo($"[Splash] Role loading complete ({loadedRoles} roles, {loadedModifiers} modifiers, {loadedGhostRoles} ghost roles)");
    }
    public static IRoleBase GetRoleById(RoleId roleId)
    {
        return AllRolesByRoleId.TryGetValue((int)roleId, out var role) ? role : null;
    }
    public static bool TryGetRoleById(RoleId roleId, out IRoleBase role)
    {
        return AllRolesByRoleId.TryGetValue((int)roleId, out role);
    }
    public static IModifierBase GetModifierById(ModifierRoleId modifierRoleId)
    {
        return AllModifiersByModifierRoleId.TryGetValue((int)modifierRoleId, out var modifier) ? modifier : null;
    }
    public static bool TryGetModifierById(ModifierRoleId modifierRoleId, out IModifierBase modifier)
    {
        return AllModifiersByModifierRoleId.TryGetValue((int)modifierRoleId, out modifier);
    }
    public static IGhostRoleBase GetGhostRoleById(GhostRoleId roleId)
    {
        return AllGhostRolesByRoleId.TryGetValue((int)roleId, out var ghostRole) ? ghostRole : null;
    }
    public static bool TryGetGhostRoleById(GhostRoleId roleId, out IGhostRoleBase ghostRole)
    {
        return AllGhostRolesByRoleId.TryGetValue((int)roleId, out ghostRole);
    }
}

