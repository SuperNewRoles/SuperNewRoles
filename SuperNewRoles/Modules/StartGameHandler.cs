using System;
using System.Collections.Generic;
using System.Linq;
using SuperNewRoles.Roles;

namespace SuperNewRoles.Modules;

// ゲーム開始時に何らかの処理をしたい場合につけるやつ
public interface IStartGameHandler
{
    public void OnGameStart();
}
public static class StartGameHandlerManager
{
    public static IStartGameHandler[] startGameHandlers;
    public static void Load()
    {
        startGameHandlers = SuperNewRolesPlugin.Assembly.GetTypes()
            // まずIStartGameHandlerインターフェースを実装している型を取得
            .Where(type => typeof(IStartGameHandler).IsAssignableFrom(type))
            // 抽象クラスは除外
            .Where(type => !type.IsAbstract)
            // さらにBaseSingletonがついている型なので、BaseSingleton<T>のInstanceプロパティを取得する
            .Select(type =>
            {
                var baseSingletonType = typeof(BaseSingleton<>).MakeGenericType(type);
                var instanceProperty = baseSingletonType.GetProperty("Instance", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                if (instanceProperty == null)
                {
                    throw new InvalidOperationException($"Type {type.FullName} does not have a public static Instance property.");
                }
                return (IStartGameHandler)instanceProperty.GetValue(null);
            })
            .ToArray();
    }
    public static void OnStartGame()
    {
        foreach (IStartGameHandler startGameHandler in startGameHandlers)
        {
            try
            {
                startGameHandler.OnGameStart();
            }
            catch (Exception e)
            {
                // 他のハンドラーに影響が出ないように握りつぶす
                Logger.Error(e);
            }
        }

    }

}