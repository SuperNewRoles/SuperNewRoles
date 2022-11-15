using HarmonyLib;

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
                    Vector3 localPosition = locations[i] / MapUtilities.CachedShipStatus.MapScale;
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
            public static bool Prefix(MapBehaviour __instance, ref bool __state)
            {
                __state = false;
                if (!MeetingHud.Instance)
                {
                    if (PlayerControl.LocalPlayer.IsUseSabo() && !__instance.IsOpen)
                    {
                        __instance.Close();
                        FastDestroyableSingleton<HudManager>.Instance.ShowMap((Il2CppSystem.Action<MapBehaviour>)((m) => { m.ShowSabotageMap(); }));
                        return false;
                    }
                    if (PlayerControl.LocalPlayer.IsImpostor() && !PlayerControl.LocalPlayer.IsUseSabo() && !__instance.IsOpen)
                    {
                        PlayerControl.LocalPlayer.Data.Role.TeamType = RoleTeamTypes.Crewmate;
                        __state = true;
                        return true;
                    }
                }
                return true;
            }
            public static void Postfix(ref bool __state)
            {
                if (__state) PlayerControl.LocalPlayer.Data.Role.TeamType = RoleTeamTypes.Impostor;
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

                couldUse = (@object.inVent || roleCouldUse) && !pc.IsDead && (@object.CanMove || @object.inVent);
                canUse = couldUse;
                if (pc.Object.IsRole(RoleTypes.Engineer)) return true;
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
                if (PlayerControl.LocalPlayer.IsUseVent() && (MapBehaviour.Instance == null || !MapBehaviour.Instance.IsOpen))
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

                if (PlayerControl.LocalPlayer.IsUseSabo() && (MapBehaviour.Instance == null || !MapBehaviour.Instance.IsOpen))
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
        [HarmonyPatch(typeof(Vent), nameof(Vent.SetButtons))]
        public static class VentSetButtonsPatch
        {
            public static void Prefix(Vent __instance, ref bool enabled)
            {
                if (PlayerControl.LocalPlayer.IsMadRoles() && !CustomOptionHolder.MadRolesCanVentMove.GetBool()) enabled = false;
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
        [HarmonyPatch(typeof(Vent), nameof(Vent.SetOutline))]
        class VentSetOutlinePatch
        {
            static void Postfix(Vent __instance)
            {
                // Vent outline set role color
                var color = IntroData.GetIntroData(PlayerControl.LocalPlayer.GetRole(), PlayerControl.LocalPlayer).color;
                string[] outlines = new[] { "_OutlineColor", "_AddColor" };
                foreach (var name in outlines)
                    __instance.myRend.material.SetColor(name, color);
            }
        }
    }
}