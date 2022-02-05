using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SuperNewRoles.Buttons
{
    public static class VentAndSabo
    {
        [HarmonyPatch(typeof(Vent), nameof(Vent.CanUse))]
        public static class VentCanUsePatch
        {
            
            public static bool Prefix(Vent __instance, ref float __result, [HarmonyArgument(0)] GameData.PlayerInfo pc, [HarmonyArgument(1)] out bool canUse, [HarmonyArgument(2)] out bool couldUse)
            {
                float num = float.MaxValue;
                PlayerControl @object = pc.Object;

                bool roleCouldUse = @object.IsUseVent();

                var usableDistance = __instance.UsableDistance;

                couldUse = (@object.inVent || roleCouldUse) && !pc.IsDead && (@object.CanMove || @object.inVent);
                canUse = couldUse;
                if (canUse)
                {
                    Vector2 truePosition = @object.GetTruePosition();
                    Vector3 position = __instance.transform.position;
                    num = Vector2.Distance(truePosition, position);

                    canUse &= (num <= usableDistance && !PhysicsHelpers.AnythingBetween(truePosition, position, Constants.ShipOnlyMask, false));
                }
                __result = num;
                return false;
            }
        }
        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
        class VentButtonVisibilityPatch
        {
            static void Postfix(PlayerControl __instance) { 
                    HudManager.Instance.ImpostorVentButton.Hide();
                    HudManager.Instance.SabotageButton.Hide();
                        if (PlayerControl.LocalPlayer.IsUseVent())
                            HudManager.Instance.ImpostorVentButton.Show();

                        if (PlayerControl.LocalPlayer.IsUseSabo())
                        {
                            HudManager.Instance.SabotageButton.Show();
                            HudManager.Instance.SabotageButton.gameObject.SetActive(true);
                        }
                }
         }
        [HarmonyPatch(typeof(SabotageButton), nameof(SabotageButton.DoClick))]
        public static class SabotageButtonDoClickPatch
        {
            public static bool Prefix(SabotageButton __instance)
            {
                // The sabotage button behaves just fine if it's a regular impostor
                if (PlayerControl.LocalPlayer.Data.Role.TeamType == RoleTeamTypes.Impostor) return true;

                DestroyableSingleton<HudManager>.Instance.ShowMap((Il2CppSystem.Action<MapBehaviour>)((m) => { m.ShowSabotageMap(); }));
                return false;
            }
        }
        [HarmonyPatch(typeof(MapBehaviour), nameof(MapBehaviour.ShowNormalMap))]
        public static class MapButtonDoClickPatch
        {
            public static void Postfix(MapBehaviour __instance)
            {
                SuperNewRolesPlugin.Logger.LogInfo("DoClicked!");
                SuperNewRolesPlugin.Logger.LogInfo(PlayerControl.LocalPlayer.IsUseSabo() && ModHelpers.ShowButtons);
                if (PlayerControl.LocalPlayer.IsUseSabo() && !ModHelpers.ShowButtons)
                {
                    __instance.Close();
                    DestroyableSingleton<HudManager>.Instance.ShowMap((Il2CppSystem.Action<MapBehaviour>)((m) => { m.ShowSabotageMap(); }));
                } else
                {
                    __instance.Close();
                }
            }
        }
    }
}
