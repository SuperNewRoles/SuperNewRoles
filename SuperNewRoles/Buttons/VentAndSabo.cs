using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using HarmonyLib;
using SuperNewRoles.CustomObject;
using SuperNewRoles.MapOption;
using SuperNewRoles.Roles.Crewmate;
using SuperNewRoles.Roles.Neutral;
using SuperNewRoles.Roles.RoleBases;
using UnityEngine;

namespace SuperNewRoles.Buttons;

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
            //サウナー
            ImportantTextTask imptsk;
            if ((imptsk = task.TryCast<ImportantTextTask>()) != null)
            {
                if (imptsk.Text.StartsWith("<size=0%>Sauner</size>"))
                {
                    locations = Sauner.GetSaunaPos().ToIl2CppList();
                }
            }

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
                    FastDestroyableSingleton<HudManager>.Instance.ToggleMapVisible(new MapOptions()
                    {
                        Mode = MapOptions.Modes.Sabotage,
                        AllowMovementWhileMapOpen = true
                    });
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
        public static bool Prefix([HarmonyArgument(0)] PlayerControl pc) => MapOption.MapOption.CanPlayVentAnimation || pc.AmOwner;
    }
    [HarmonyPatch(typeof(Vent), nameof(Vent.ExitVent))]
    class ExitVentAnimPatch
    {
        public static bool Prefix([HarmonyArgument(0)] PlayerControl pc) => MapOption.MapOption.CanPlayVentAnimation || pc.AmOwner;
    }
    [HarmonyPatch(typeof(Vent), nameof(Vent.CanUse))]
    public static class VentCanUsePatch
    {
        public static bool Prefix(Vent __instance, ref float __result, [HarmonyArgument(0)] NetworkedPlayerInfo pc, [HarmonyArgument(1)] out bool canUse, [HarmonyArgument(2)] out bool couldUse)
        {
            float num = float.MaxValue;
            PlayerControl @object = pc.Object;

            if (@object == null)
            {
                __result = 0f;
                canUse = couldUse = true;
                return true;
            }
            if (@object.inVent && Vent.currentVent != null && __instance != null)
            {
                if (Vent.currentVent.Id == __instance.Id)
                {
                    __result = 0f;
                    canUse = couldUse = true;
                    return false;
                }
            }

            bool roleCouldUse = @object.IsUseVent() || @object.IsRole(RoleId.OrientalShaman);

            var usableDistance = __instance.UsableDistance;

            couldUse = (@object.inVent || roleCouldUse) && !pc.IsDead && (@object.CanMove || @object.inVent);
            canUse = couldUse;
            if (WormHole.IsWormHole(__instance) && !pc.Object.IsImpostor()) return true;
            if (pc.Object.IsRole(RoleTypes.Engineer)) return true;

            if (NiceMechanic.TargetVent.Values.FirstOrDefault(x => x is not null && x.Id == __instance.Id) is not null) canUse = false;
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
                if (PlayerControl.LocalPlayer.inVent)
                {
                    FastDestroyableSingleton<HudManager>.Instance.ImpostorVentButton.SetTarget(Vent.currentVent);
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
            if (!Mode.ModeHandler.IsMode(Mode.ModeId.SuperHostRoles) && PlayerControl.LocalPlayer.IsMadRoles() && !CustomOptionHolder.MadRolesCanVentMove.GetBool()) enabled = false;
            if (NiceMechanic.TargetVent.ContainsValue(__instance) && ModHelpers.PlayerById(NiceMechanic.TargetVent.FirstOrDefault(x => x.Value == __instance).Key).IsRole(RoleId.NiceMechanic)) enabled = false;
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

            FastDestroyableSingleton<HudManager>.Instance.ToggleMapVisible(new MapOptions()
            {
                Mode = MapOptions.Modes.Sabotage,
                AllowMovementWhileMapOpen = true
            });
            return false;
        }
    }
    [HarmonyPatch(typeof(Vent), nameof(Vent.SetOutline))]
    class VentSetOutlinePatch
    {
        static bool Prefix(Vent __instance, bool on, bool mainTarget)
        {
            // Vent outline set role color
            var color = CustomRoles.GetRoleColor(PlayerControl.LocalPlayer);
            Material material = __instance.myRend.material;
            material.SetColor("_AddColor", mainTarget ? color : Color.clear);
            material.SetColor("_OutlineColor", color);
            material.SetFloat("_Outline", on ? 1f : 0f);
            return false;
        }
    }
}