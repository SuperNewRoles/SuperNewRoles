using HarmonyLib;
using SuperNewRoles.Modules;
using SuperNewRoles.Modules.Events.Bases;

namespace SuperNewRoles.Events;

public class FixedUpdateEvent : EventTargetBase<FixedUpdateEvent>
{
    public static void Invoke()
    {
        Instance.Awake();
    }
}

/// <summary>
/// FixedUpdateで、instanceありの場合。通常のものと比べて、プレイヤー数倍実行される。
/// ※ オブジェクト生成のオーバーヘッド削減のため、同一インスタンスを再利用する実装です。
/// </summary>
public class FixedUpdateWithInstanceEventData : IEventData
{
    public ExPlayerControl CurrentPlayer { get; private set; }

    public void UpdateData(ExPlayerControl player)
    {
        CurrentPlayer = player;
    }
}

public class FixedUpdateWithInstanceEvent : EventTargetBase<FixedUpdateWithInstanceEvent, FixedUpdateWithInstanceEventData>
{
    // インスタンス生成のオーバーヘッド削減のため、イベントデータのキャッシュを利用
    private static readonly FixedUpdateWithInstanceEventData s_cachedEventData = new();

    public static void Invoke(ExPlayerControl player)
    {
        s_cachedEventData.UpdateData(player);
        Instance.Awake(s_cachedEventData);
    }
}


[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
public static class FixedUpdatePatch
{
    public static void Postfix(PlayerControl __instance)
    {
        if (PlayerControl.LocalPlayer == __instance)
            FixedUpdateEvent.Invoke();
        FixedUpdateWithInstanceEvent.Invoke(__instance);
    }
}

