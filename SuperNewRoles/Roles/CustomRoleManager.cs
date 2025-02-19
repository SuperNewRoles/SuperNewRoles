using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SuperNewRoles.Modules;

namespace SuperNewRoles.Roles;

public static class CustomRoleManager
{
    public static IRoleBase[] AllRoles { get; private set; }
    public static Dictionary<int, IRoleBase> AllRolesByRoleId { get; private set; }
    public static void Load()
    {
        AllRoles = Assembly.GetExecutingAssembly().GetTypes()
            // まずIRoleBaseインターフェースを実装している型を取得
            .Where(type => typeof(IRoleBase).IsAssignableFrom(type))
            // 次にRoleBase<T>を継承している型に絞る（直接の継承をチェック）
            .Where(type => type.BaseType != null &&
                           type.BaseType.IsGenericType &&
                           type.BaseType.GetGenericTypeDefinition() == typeof(RoleBase<>))
            // さらにBaseSingletonがついている型なので、BaseSingleton<T>のInstanceプロパティを取得する
            .Select(type =>
            {
                Logger.Info($"Loading role: {type.FullName}");
                var baseSingletonType = typeof(BaseSingleton<>).MakeGenericType(type);
                var instanceProperty = baseSingletonType.GetProperty("Instance", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                if (instanceProperty == null)
                {
                    throw new InvalidOperationException($"Type {type.FullName} does not have a public static Instance property.");
                }
                return (IRoleBase)instanceProperty.GetValue(null);
            })
            .ToArray();
        AllRolesByRoleId = AllRoles.ToDictionary(role => (int)role.Role);
    }
    public static IRoleBase GetRoleById(RoleId roleId)
    {
        return AllRolesByRoleId.TryGetValue((int)roleId, out var role) ? role : null;
    }
    public static bool TryGetRoleById(RoleId roleId, out IRoleBase role)
    {
        return AllRolesByRoleId.TryGetValue((int)roleId, out role);
    }
}

