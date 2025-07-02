using HarmonyLib;
using SuperNewRoles.Modules;
using SuperNewRoles.Modules.Events.Bases;

namespace SuperNewRoles.Events.PCEvents
{
    public class VentEnterEventData : IEventData
    {
        public ExPlayerControl player { get; }
        public Vent vent { get; }

        public VentEnterEventData(ExPlayerControl player, Vent vent)
        {
            this.player = player;
            this.vent = vent;
        }
    }

    public class VentEnterEvent : EventTargetBase<VentEnterEvent, VentEnterEventData>
    {
        public static void Invoke(ExPlayerControl player, Vent vent)
        {
            var data = new VentEnterEventData(player, vent);
            Instance.Awake(data);
        }
    }

    [HarmonyPatch(typeof(Vent), nameof(Vent.EnterVent))]
    public static class VentEnterPatch
    {
        public static void Postfix(Vent __instance, PlayerControl pc)
        {
            VentEnterEvent.Invoke(pc, __instance);
        }
    }
}