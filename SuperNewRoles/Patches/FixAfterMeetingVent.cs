using SuperNewRoles.Events;
using SuperNewRoles.Modules;

namespace SuperNewRoles.Patches;

public static class FixAfterMeetingVent
{
    public static void RegisterListener()
    {
        WrapUpEvent.Instance.AddListener(OnWrapUp);
    }

    private static void OnWrapUp(WrapUpEventData data)
    {
        if (ShipStatus.Instance == null) return;
        if (!ShipStatus.Instance.Systems.TryGetValue(SystemTypes.Ventilation, out var system)) return;
        if (!system.Il2CppIs(out VentilationSystem ventilation)) return;

        // 会議でベントから出されてもバニラの滞在記録が残るため、掃除時に前のベントへ戻されてしまう。
        // 旧版の FixAfterMeetingVent と同様、追放の有無やマップに関係なく記録を消して同期する。
        ventilation.PlayersInsideVents.Clear();
        ventilation.IsDirty = true;
    }
}
