using HarmonyLib;
using SuperNewRoles.CustomRPC;
using SuperNewRoles.Roles;
using UnityEngine;

namespace SuperNewRoles.Patch
{
    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.CalculateLightRadius))]
    class LightPatch
    {
        public static float GetNeutralLightRadius(ShipStatus shipStatus, bool isImpostor)
        {
            if (SubmergedCompatibility.isSubmerged()) return SubmergedCompatibility.GetSubmergedNeutralLightRadius(isImpostor);
            if (Clergyman.IsLightOutVision() && isImpostor) return shipStatus.MaxLightRadius * RoleClass.Clergyman.DownImpoVision;
            if (isImpostor) return shipStatus.MaxLightRadius * PlayerControl.GameOptions.ImpostorLightMod;

            SwitchSystem switchSystem = shipStatus.Systems[SystemTypes.Electrical].TryCast<SwitchSystem>();
            float lerpValue = switchSystem.Value / 255f;

            var LocalPlayer = PlayerControl.LocalPlayer;
            if (LocalPlayer.IsRole(RoleId.Nocturnality))
            {
                lerpValue = 1 - lerpValue >= 0 ? 1f - lerpValue : 1f + (1f - lerpValue);
            }
            return Mathf.Lerp(shipStatus.MinLightRadius, shipStatus.MaxLightRadius, lerpValue) * PlayerControl.GameOptions.CrewLightMod;
        }
        public static bool Prefix(ref float __result, ShipStatus __instance, [HarmonyArgument(0)] GameData.PlayerInfo player)
        {
            ISystemType systemType = __instance.Systems.ContainsKey(SystemTypes.Electrical) ? __instance.Systems[SystemTypes.Electrical] : null;
            if (systemType == null) return true;
            SwitchSystem switchSystem = systemType.TryCast<SwitchSystem>();
            if (switchSystem == null) return true;

            float num = switchSystem.Value / 255f;

            __result = player == null || player.IsDead
                ? __instance.MaxLightRadius
                : player.Object.IsRole(RoleId.CountChanger) && CountChanger.GetRoleType(player.Object) == TeamRoleType.Crewmate
                ? GetNeutralLightRadius(__instance, false)
                : player.Object.IsImpostor() || RoleHelpers.IsImpostorLight(player.Object)
                ? GetNeutralLightRadius(__instance, true)
                : player.Object.IsRole(RoleId.Lighter) && RoleClass.Lighter.IsLightOn
                ? Mathf.Lerp(__instance.MaxLightRadius * RoleClass.Lighter.UpVision, __instance.MaxLightRadius * RoleClass.Lighter.UpVision, num)
                : GetNeutralLightRadius(__instance, false);
            return false;
        }
    }
}