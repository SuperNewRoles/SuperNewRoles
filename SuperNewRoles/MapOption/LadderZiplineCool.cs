using HarmonyLib;

namespace SuperNewRoles.MapOption;

public class LadderZiplineCool
{
    [HarmonyPatch(typeof(Ladder))]
    public static class LadderPatch
    {
        [HarmonyPatch(nameof(Ladder.MaxCoolDown), MethodType.Getter), HarmonyPrefix]
        public static bool LadderMaxCoolDownGetterPrefix(ref float __result)
        {
            if (MapOption.IsLadderCoolChange)
            {
                __result = MapOption.LadderCoolTime;
                if (MapOption.IsLadderImpostorCoolChange && PlayerControl.LocalPlayer.IsImpostor())
                    __result = MapOption.LadderImpostorCoolTime;
                return false;
            }
            return true;
        }

        [HarmonyPatch(nameof(Ladder.Use)), HarmonyPostfix]
        public static void LadderUsePostfix(Ladder __instance)
        {
            if (!MapOption.IsLadderCoolChange) return;
            __instance.CoolDown = MapOption.LadderCoolTime;
            if (MapOption.IsLadderImpostorCoolChange && PlayerControl.LocalPlayer.IsImpostor())
                __instance.CoolDown = MapOption.LadderImpostorCoolTime;
        }

        [HarmonyPatch(nameof(Ladder.SetDestinationCooldown)), HarmonyPostfix]
        public static void LadderSetDestinationCooldownPostfix(Ladder __instance)
        {
            if (!MapOption.IsLadderCoolChange) return;
            __instance.Destination.CoolDown = MapOption.LadderCoolTime;
            if (MapOption.IsLadderImpostorCoolChange && PlayerControl.LocalPlayer.IsImpostor())
                __instance.Destination.CoolDown = MapOption.LadderImpostorCoolTime;
        }
    }

    [HarmonyPatch(typeof(ZiplineConsole))]
    public static class ZiplineConsolePatch
    {
        [HarmonyPatch(nameof(ZiplineConsole.MaxCoolDown), MethodType.Getter), HarmonyPrefix]
        public static bool ZiplineConsoleMaxCoolDownGetterPrefix(ref float __result)
        {
            if (MapOption.IsZiplineCoolChange)
            {
                __result = MapOption.ZiplineCoolTime;
                if (MapOption.IsZiplineImpostorCoolChange && PlayerControl.LocalPlayer.IsImpostor())
                    __result = MapOption.ZiplineImpostorCoolTime;
                return false;
            }
            return true;
        }

        [HarmonyPatch(nameof(ZiplineConsole.Use)), HarmonyPostfix]
        public static void ZiplineConsoleUsePostfix(ZiplineConsole __instance)
        {
            if (!MapOption.IsZiplineCoolChange) return;
            __instance.CoolDown = MapOption.ZiplineCoolTime;
            if (MapOption.IsZiplineImpostorCoolChange && PlayerControl.LocalPlayer.IsImpostor())
                __instance.CoolDown = MapOption.ZiplineImpostorCoolTime;
        }

        [HarmonyPatch(nameof(ZiplineConsole.SetDestinationCooldown)), HarmonyPostfix]
        public static void ZiplineConsoleSetDestinationCooldownPostfix(ZiplineConsole __instance)
        {
            if (!MapOption.IsZiplineCoolChange) return;
            __instance.destination.CoolDown = MapOption.ZiplineCoolTime;
            if (MapOption.IsZiplineImpostorCoolChange && PlayerControl.LocalPlayer.IsImpostor())
                __instance.destination.CoolDown = MapOption.ZiplineImpostorCoolTime;
        }
    }
}
