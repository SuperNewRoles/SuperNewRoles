using HarmonyLib;
using SuperNewRoles.Roles;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SuperNewRoles.Patch
{
    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.CalculateLightRadius))]
    class LightPatch
    {
        public static bool Prefix(ref float __result, ShipStatus __instance, [HarmonyArgument(0)] GameData.PlayerInfo player)
        {
            ISystemType systemType = __instance.Systems.ContainsKey(SystemTypes.Electrical) ? __instance.Systems[SystemTypes.Electrical] : null;
            if (systemType == null) return true;
            SwitchSystem switchSystem = systemType.TryCast<SwitchSystem>();
            if (switchSystem == null) return true;

            float num = (float)switchSystem.Value / 255f;

            if (player == null || player.IsDead)
                __result = __instance.MaxLightRadius;
            else if (player.Role.IsImpostor)
                __result = __instance.MaxLightRadius * PlayerControl.GameOptions.ImpostorLightMod;
            else if (RoleClass.Lighter.LighterPlayer.IsCheckListPlayerControl(PlayerControl.LocalPlayer) && RoleClass.Lighter.IsLightOn)
                __result = Mathf.Lerp(__instance.MaxLightRadius * RoleClass.Lighter.UpVision, __instance.MaxLightRadius * RoleClass.Lighter.UpVision, num);
            else
                __result = Mathf.Lerp(__instance.MinLightRadius, __instance.MaxLightRadius, num) * PlayerControl.GameOptions.CrewLightMod;
            return false;
        }
    }
}
