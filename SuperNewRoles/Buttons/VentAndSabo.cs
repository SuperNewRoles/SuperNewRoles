using HarmonyLib;
using SuperNewRoles.CustomRPC;
using SuperNewRoles.MapOptions;
using UnityEngine;

namespace SuperNewRoles.Buttons
{
    public static class VentAndSabo
    {

        [HarmonyPatch(typeof(MapTaskOverlay), nameof(MapTaskOverlay.SetIconLocation))]
        public static class MapTaskOverlaySetIconLocationPatch
        {
            public static bool Prefix(
                MapTaskOverlay __instance,
                [HarmonyArgument(0)] PlayerTask task)
            {
                Il2CppSystem.Collections.Generic.List<Vector2> locations = task.Locations;
                for (int i = 0; i < locations.Count; i++)
                {
                    Vector3 localPosition = locations[i] / ShipStatus.Instance.MapScale;
                    localPosition.z = -1f;
                    PooledMapIcon pooledMapIcon = __instance.icons.Get<PooledMapIcon>();
                    pooledMapIcon.transform.localScale = new Vector3(
                        pooledMapIcon.NormalSize,
                        pooledMapIcon.NormalSize,
                        pooledMapIcon.NormalSize);
                    if (PlayerTask.TaskIsEmergency(task))
                    {
                        pooledMapIcon.rend.color = Color.red;
                        pooledMapIcon.alphaPulse.enabled = true;
                        pooledMapIcon.rend.material.SetFloat("_Outline", 1f);
                    }
                    else
                    {
                        pooledMapIcon.rend.color = Color.yellow;
                    }
                    pooledMapIcon.name = task.name;
                    pooledMapIcon.lastMapTaskStep = task.TaskStep;
                    pooledMapIcon.transform.localPosition = localPosition;
                    if (task.TaskStep > 0)
                    {
                        pooledMapIcon.alphaPulse.enabled = true;
                        pooledMapIcon.rend.material.SetFloat("_Outline", 1f);
                    }

                    string key = $"{task.name}{i}";
                    int index = 0;

                    while (__instance.data.ContainsKey(key))
                    {
                        key = $"{key}_{index}";
                        ++index;
                    }

                    __instance.data.Add(key, pooledMapIcon);
                }
                return false;
            }
        }
        [HarmonyPatch(typeof(MapBehaviour), nameof(MapBehaviour.ShowNormalMap))]
        class MapBehaviourPatch
        {
            public static bool Prefix(MapBehaviour __instance)
            {
                if (!MeetingHud.Instance)
                {
                    if (PlayerControl.LocalPlayer.IsUseSabo() && !__instance.IsOpen)
                    {
                        __instance.Close();
                        DestroyableSingleton<HudManager>.Instance.ShowMap((Il2CppSystem.Action<MapBehaviour>)((m) => { m.ShowSabotageMap(); }));
                        return false;
                    }
                }
                return true;
            }
        }
        [HarmonyPatch(typeof(Vent), nameof(Vent.EnterVent))]
        class EnterVentAnimPatch
        {
            public static bool Prefix([HarmonyArgument(0)] PlayerControl pc)
            {
                return !MapOption.VentAnimation.GetBool() || pc.AmOwner;
            }
        }
        [HarmonyPatch(typeof(Vent), nameof(Vent.ExitVent))]
        class ExitVentAnimPatch
        {
            public static bool Prefix([HarmonyArgument(0)] PlayerControl pc)
            {
                return !MapOption.VentAnimation.GetBool() || pc.AmOwner;
            }
        }
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

                    canUse &= num <= usableDistance && !PhysicsHelpers.AnythingBetween(truePosition, position, Constants.ShipOnlyMask, false);
                }
                __result = num;
                return false;
            }
        }
        public class VentButtonVisibilityPatch
        {
            public static void Postfix(PlayerControl __instance)
            {
                var ImpostorVentButton = FastDestroyableSingleton<HudManager>.Instance.ImpostorVentButton;
                var ImpostorSabotageButton = FastDestroyableSingleton<HudManager>.Instance.SabotageButton;

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
                __instance.CanUse(CachedPlayer.LocalPlayer.Data, out bool canUse, out bool couldUse);
                bool canMoveInVents = !PlayerControl.LocalPlayer.IsRole(RoleId.NiceNekomata);
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