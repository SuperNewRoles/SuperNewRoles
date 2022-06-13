using HarmonyLib;
using SuperNewRoles.Roles;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using SuperNewRoles.MapOptions;

namespace SuperNewRoles.Buttons
{
    public static class VentAndSabo
    {

        [HarmonyPatch(typeof(MapBehaviour))]
        class MapBehaviourPatch
        {

            [HarmonyPatch(typeof(MapBehaviour), nameof(MapBehaviour.FixedUpdate))]
            static bool Prefix(MapBehaviour __instance)
            {
                if (!MeetingHud.Instance) return true;  // Only run in meetings, and then set the Position of the HerePoint to the Position before the Meeting!
                if (!MapUtilities.CachedShipStatus)
                {
                    return false;
                }
                Vector3 vector = CachedPlayer.LocalPlayer.transform.position;
                vector /= MapUtilities.CachedShipStatus.MapScale;
                vector.x *= Mathf.Sign(MapUtilities.CachedShipStatus.transform.localScale.x);
                vector.z = -1f;
                __instance.HerePoint.transform.localPosition = vector;
                PlayerControl.LocalPlayer.SetPlayerMaterialColors(__instance.HerePoint);
                return false;
            }
            [HarmonyPrefix]
            [HarmonyPatch(typeof(MapBehaviour), nameof(MapBehaviour.ShowNormalMap))]
            static bool Prefix3(MapBehaviour __instance)
            {
                if (!MeetingHud.Instance)
                {
                    if (PlayerControl.LocalPlayer.IsUseSabo() && !ModHelpers.ShowButtons && !__instance.IsOpen)
                    {
                        __instance.Close();
                        FastDestroyableSingleton<HudManager>.Instance.ShowMap((Il2CppSystem.Action<MapBehaviour>)((m) => { m.ShowSabotageMap(); }));
                        return false;
                    }
                    return true;
                }  // Only run in meetings and when the map is closed
                if (__instance.IsOpen) return true;
                if (!Mode.ModeHandler.isMode(Mode.ModeId.Default)) return true;
                PlayerControl.LocalPlayer.SetPlayerMaterialColors(__instance.HerePoint);
                __instance.GenericShow();
                __instance.taskOverlay.Show();
                __instance.ColorControl.SetColor(new Color(0.05f, 0.2f, 1f, 1f));
                FastDestroyableSingleton<HudManager>.Instance.SetHudActive(false);
                return false;
            }
        }
        [HarmonyPatch(typeof(Vent), nameof(Vent.CanUse))]
        public static class VentCanUsePatch
        {
            [HarmonyPatch(typeof(Vent), nameof(Vent.EnterVent))]
            class EnterVentAnimPatch
            {
                public static bool Prefix(Vent __instance, [HarmonyArgument(0)] PlayerControl pc)
                {
                    if (MapOption.VentAnimation.getBool())
                    {
                        return pc.AmOwner;
                    }
                    return true;
                }
            }

            [HarmonyPatch(typeof(Vent), nameof(Vent.ExitVent))]
            class ExitVentAnimPatch
            {
                public static bool Prefix(Vent __instance, [HarmonyArgument(0)] PlayerControl pc)
                {
                    if (MapOption.VentAnimation.getBool())
                    {
                        return pc.AmOwner;
                    }
                    return true;
                }
            }
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
            public static void Postfix(PlayerControl __instance)
            {
                var ImpostorVentButton = DestroyableSingleton<HudManager>.Instance.ImpostorVentButton;
                var ImpostorSabotageButton = DestroyableSingleton<HudManager>.Instance.SabotageButton;

                if (PlayerControl.LocalPlayer.IsUseVent())
                {
                    if (!ImpostorVentButton.gameObject.active)
                    {
                        ImpostorVentButton.Show();
                    }
                    if (Input.GetKeyDown(KeyCode.V) || KeyboardJoystick.player.GetButtonDown(50))
                    {
                        ImpostorVentButton.DoClick();
                    }
                }
                else
                {
                    if (ImpostorVentButton.gameObject.active)
                    {
                        ImpostorVentButton.Hide();
                    }
                }

                if (PlayerControl.LocalPlayer.IsUseSabo())
                {
                    if (!ImpostorSabotageButton.gameObject.active)
                    {
                        ImpostorSabotageButton.Show();
                    }
                }
                else
                {
                    if (ImpostorSabotageButton.gameObject.active)
                    {
                        ImpostorSabotageButton.Hide();
                    }
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
                __instance.CanUse(CachedPlayer.LocalPlayer.Data, out canUse, out couldUse);
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
                if (CachedPlayer.LocalPlayer.Data.Role.TeamType == RoleTeamTypes.Impostor) return true;

                FastDestroyableSingleton<HudManager>.Instance.ShowMap((Il2CppSystem.Action<MapBehaviour>)((m) => { m.ShowSabotageMap(); }));
                return false;
            }
        }
    }
}
