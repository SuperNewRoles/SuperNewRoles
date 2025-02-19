using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;

namespace SuperNewRoles.Modules.Events.Bases;

public static class EventListenerManager
{
    public static List<IEventTargetBase> listeners { get; } = new();
    public static void CoStartGame()
    {
        foreach (var listener in listeners)
        {
            listener.RemoveListenerAll();
        }
    }

    public static void Load()
    {
        listeners.Clear();

        // Assembly内の全ての型を取得
        var types = Assembly.GetExecutingAssembly().GetTypes();

        // IEventTargetBaseを実装している型を抽出
        var eventTargetTypes = types.Where(t =>
            !t.IsInterface &&
            !t.IsAbstract &&
            typeof(IEventTargetBase).IsAssignableFrom(t));

        foreach (var type in eventTargetTypes)
        {
            // BaseSingletonを継承しているか確認
            if (type.IsSubclassOf(typeof(BaseSingleton<>).MakeGenericType(type)))
            {
                // Instanceプロパティを取得
                var instanceProperty = AccessTools.Property(type, "Instance");
                if (instanceProperty != null)
                {
                    // Instanceを取得し、IEventTargetBaseとしてlistenersに追加
                    var instance = instanceProperty.GetValue(null) as IEventTargetBase;
                    if (instance != null)
                    {
                        listeners.Add(instance);
                    }
                }
            }
            else
            {
                // BaseSingletonを継承していない場合は、新しいインスタンスを作成
                var instance = System.Activator.CreateInstance(type) as IEventTargetBase;
                if (instance != null)
                {
                    listeners.Add(instance);
                }
            }
        }
        Logger.Info($"Loaded {listeners.Count} listeners");
        foreach (var listener in listeners)
        {
            Logger.Info($"Loaded {listener.GetType().Name}");
        }
    }
}
