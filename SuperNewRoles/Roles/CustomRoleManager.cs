using System;
using System.Linq;
using System.Reflection;
using SuperNewRoles.Modules;

namespace SuperNewRoles.Roles;

public static class CustomRoleManager
{
    public static IRoleBase[] AllRoles { get; private set; }
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
                var baseSingletonType = typeof(BaseSingleton<>).MakeGenericType(type);
                var instanceProperty = baseSingletonType.GetProperty("Instance", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                if (instanceProperty == null)
                {
                    throw new InvalidOperationException($"Type {type.FullName} does not have a public static Instance property.");
                }
                return (IRoleBase)instanceProperty.GetValue(null);
            })
            .ToArray();
        // AllROlesの数と、その中のRoleIdをログにする
        Logger.Info($"AllRolesの数: {AllRoles.Length}");
        foreach (var role in AllRoles)
        {
            Logger.Info($"RoleId: {role.Role}");
        }
    }
}

