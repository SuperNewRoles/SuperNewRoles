using HarmonyLib;
using SuperNewRoles.Roles;
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


                if (SubmergedCompatibility.isSubmerged())
                {
                    // as submerged does, only change stuff for vents 9 and 14 of submerged. Code partially provided by AlexejheroYTB
                    if (SubmergedCompatibility.getInTransition())
                    {
                        __result = float.MaxValue;
                        return canUse = couldUse = false;
                    }
                    switch (__instance.Id)
                    {
                        case 9:  // Cannot enter vent 9 (Engine Room Exit Only Vent)!
                            if (PlayerControl.LocalPlayer.inVent) break;
                            __result = float.MaxValue;
                            return canUse = couldUse = false;
                        case 14: // Lower Central
                            __result = float.MaxValue;
                            couldUse = roleCouldUse && !pc.IsDead && (@object.CanMove || @object.inVent);
                            canUse = couldUse;
                            if (canUse)
                            {
                                Vector3 center = @object.Collider.bounds.center;
                                Vector3 position = __instance.transform.position;
                                __result = Vector2.Distance(center, position);
                                canUse &= __result <= __instance.UsableDistance;
                            }
                            return false;
                    }
                }

                couldUse = (@object.inVent || roleCouldUse) && !pc.IsDead && (@object.CanMove || @object.inVent);
                canUse = couldUse;
                if (pc.Role.Role == RoleTypes.Engineer) return true;
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
        public class VentButtonVisibilityPatch
        {
            public static void Postfix(PlayerControl __instance) {
                HudManager.Instance.ImpostorVentButton.Hide();
                HudManager.Instance.SabotageButton.Hide();

                if (PlayerControl.LocalPlayer.IsUseVent())
                {
                    HudManager.Instance.ImpostorVentButton.Show();
                    if (Input.GetKeyDown(KeyCode.V) || KeyboardJoystick.player.GetButtonDown(50))
                    {
                        HudManager.Instance.ImpostorVentButton.DoClick();
                    }
                }

                if (PlayerControl.LocalPlayer.IsUseSabo())
                {
                    HudManager.Instance.SabotageButton.Show();
                    HudManager.Instance.SabotageButton.gameObject.SetActive(true);
                }
           }
        }
        [HarmonyPatch(typeof(Vent), nameof(Vent.Use))]
        public static class VentUsePatch
        {
            public static bool Prefix(Vent __instance)
            {
                bool canUse;
                bool couldUse;
                __instance.CanUse(PlayerControl.LocalPlayer.Data, out canUse, out couldUse);
                bool canMoveInVents = !(RoleClass.MadMate.MadMatePlayer.IsCheckListPlayerControl(PlayerControl.LocalPlayer));
                if (!canUse) return false; // No need to execute the native method as using is disallowed anyways

                bool isEnter = !PlayerControl.LocalPlayer.inVent;

                if (isEnter)
                {
                    PlayerControl.LocalPlayer.MyPhysics.RpcEnterVent(__instance.Id);
                }
                else
                {
                    PlayerControl.LocalPlayer.MyPhysics.RpcExitVent(__instance.Id);
                }
                __instance.SetButtons(isEnter && canMoveInVents);
                return false;
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
                if (PlayerControl.LocalPlayer.IsUseSabo() && !ModHelpers.ShowButtons)
                {
                    __instance.Close();
                    DestroyableSingleton<HudManager>.Instance.ShowMap((Il2CppSystem.Action<MapBehaviour>)((m) => { m.ShowSabotageMap(); }));
                }
            }
        }
    }
}
