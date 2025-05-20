using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;

namespace SuperNewRoles.Modules.Events.Bases;

public static class EventListenerManager
{
    public static List<IEventTargetBase> listeners { get; } = new();
    public static void ResetAllListener()
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
        var types = SuperNewRolesPlugin.Assembly.GetTypes();

        // IEventTargetBaseを実装している型を抽出
        var eventTargetTypes = types.Where(t =>
            !t.IsInterface &&
            !t.IsAbstract &&
            typeof(IEventTargetBase).IsAssignableFrom(t)).ToList();

        SuperNewRolesPlugin.Logger.LogInfo($"[Splash] Found {eventTargetTypes.Count} event target types");
        int index = 0;
        foreach (var type in eventTargetTypes)
        {
            index++;
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
            SuperNewRolesPlugin.Logger.LogInfo($"[Splash] Loaded listeners ({index}/{eventTargetTypes.Count}): {type.Name}");
        }
    }
    [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Start))]
    public static class GameStartManagerStartPatch
    {
        public static void Postfix()
        {
            ResetAllListener();
        }
    }
}
