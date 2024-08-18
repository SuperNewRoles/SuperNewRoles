using HarmonyLib;
using SuperNewRoles.Roles.RoleBases;
using SuperNewRoles.Roles.RoleBases.Interfaces;

namespace SuperNewRoles.Patches;

[HarmonyPatch(typeof(MapBehaviour))]
public static class MapBehaviourPatch
{

    [HarmonyPatch(nameof(MapBehaviour.Awake)), HarmonyPostfix]
    public static void AwakePostfix(MapBehaviour __instance)
    {
        if (PlayerControl.LocalPlayer.GetRoleBase() is IMapEvent map)
            map.MapAwakePostfix(__instance);
    }

    [HarmonyPatch(nameof(MapBehaviour.Show)), HarmonyPrefix]
    public static void ShowPrefix(MapBehaviour __instance, MapOptions opts, ref bool __state /* Postfixで現在位置マークを表示させるかどうか */)
    {
        __state = false;
        if (PlayerControl.LocalPlayer.GetRoleBase() is IMapEvent map)
            map.MapShowPrefix(__instance, opts, ref __state);
    }
    
    [HarmonyPatch(nameof(MapBehaviour.Show)), HarmonyPostfix]
    public static void ShowPostfix(MapBehaviour __instance, MapOptions opts, bool __state)
    {
        if (!__instance.IsOpen) return;
        if (__state) __instance.HerePoint.enabled = true;
        if (PlayerControl.LocalPlayer.GetRoleBase() is IMapEvent map)
            map.MapShowPostfix(__instance, opts);
    }

    [HarmonyPatch(nameof(MapBehaviour.FixedUpdate)), HarmonyPostfix]
    public static void FixedUpdatePostfix(MapBehaviour __instance)
    {
        if (PlayerControl.LocalPlayer.GetRoleBase() is IMapEvent map)
            map.MapFixedUpdatePostfix(__instance);
    }

    [HarmonyPatch(nameof(MapBehaviour.IsOpenStopped), MethodType.Getter), HarmonyPostfix]
    public static void IsOpenStoppedGetterPostfix(MapBehaviour __instance, ref bool __result)
    {
        if (PlayerControl.LocalPlayer.GetRoleBase() is IMapEvent map)
            map.IsMapOpenStoppedPostfix(__instance, ref __result);
    }
}