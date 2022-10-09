using HarmonyLib;
using SuperNewRoles.MapCustoms;
using SuperNewRoles.Mode;
using SuperNewRoles.Mode.SuperHostRoles;
using SuperNewRoles.Roles;
using UnityEngine;

namespace SuperNewRoles.Patches
{
    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Awake))]
    static class ShipStatus_AwakePatch
    {
        static void Postfix(ShipStatus __instance)
        {
            MapCustoms.Airship.SecretRoom.ShipStatusAwake(__instance);
            AddVitals.AddVital();
            RecordsAdminDestroy.AdminDestroy();
            ProctedMessager.Init();
        }
    }
    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Awake))]
    public static class ShipStatus_Awake_Patch
    {
        [HarmonyPostfix, HarmonyPriority(Priority.Last)]
        public static void Postfix(ShipStatus __instance)
        {
            MapUtilities.CachedShipStatus = __instance;
        }
    }
    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.RepairSystem))]
    class RepairSystemPatch
    {
        public static bool Prefix(ShipStatus __instance,
            [HarmonyArgument(0)] SystemTypes systemType,
            [HarmonyArgument(1)] PlayerControl player,
            [HarmonyArgument(2)] byte amount)
        {
            if (PlusModeHandler.IsMode(PlusModeId.NotSabotage))
            {
                return false;
            }
            if (ModeHandler.IsMode(ModeId.Default))
            {
                if (systemType == SystemTypes.Comms || systemType == SystemTypes.Sabotage || systemType == SystemTypes.Electrical)
                {
                    if (PlayerControl.LocalPlayer.IsRole(RoleId.Painter) && RoleClass.Painter.CurrentTarget != null && RoleClass.Painter.CurrentTarget.PlayerId == player.PlayerId) Roles.CrewMate.Painter.Handle(Roles.CrewMate.Painter.ActionType.SabotageRepair);
                }
            }
            if ((ModeHandler.IsMode(ModeId.BattleRoyal) || ModeHandler.IsMode(ModeId.Zombie) || ModeHandler.IsMode(ModeId.HideAndSeek) || ModeHandler.IsMode(ModeId.CopsRobbers)) && (systemType == SystemTypes.Sabotage || systemType == SystemTypes.Doors)) return false;

            if (systemType == SystemTypes.Electrical && 0 <= amount && amount <= 4) // 停電を直そうとした
            {
                if (player.IsMadRoles() && !CustomOptions.MadRolesCanFixElectrical.GetBool())
                {
                    return false;
                }
            }
            if (systemType == SystemTypes.Comms && amount is 0 or 16 or 17) // コミュサボを直そうとした
            {
                if (player.IsMadRoles() && !CustomOptions.MadRolesCanFixComms.GetBool())
                {
                    return false;
                }
            }
            if (ModeHandler.IsMode(ModeId.SuperHostRoles))
            {
                bool returndata = MorePatch.RepairSystem(__instance, systemType, player, amount);
                return returndata;
            }
            return true;
        }
        public static void Postfix(
            [HarmonyArgument(0)] SystemTypes systemType,
            [HarmonyArgument(1)] PlayerControl player,
            [HarmonyArgument(2)] byte amount)
        {
            if (!RoleHelpers.IsSabotage())
            {
                new LateTask(() =>
                {
                    foreach (PlayerControl p in RoleClass.Technician.TechnicianPlayer)
                    {
                        if (p.inVent && p.IsAlive() && Mode.BattleRoyal.Main.VentData.ContainsKey(p.PlayerId) && Mode.BattleRoyal.Main.VentData[p.PlayerId] != null)
                        {
                            p.MyPhysics.RpcBootFromVent((int)Mode.BattleRoyal.Main.VentData[p.PlayerId]);
                        }
                    }
                }, 0.1f, "TecExitVent");
            }
            SuperNewRolesPlugin.Logger.LogInfo(player.Data.PlayerName + " => " + systemType + " : " + amount);
            if (ModeHandler.IsMode(ModeId.SuperHostRoles))
            {
                SyncSetting.CustomSyncSettings();
                if (systemType == SystemTypes.Comms)
                {
                    Mode.SuperHostRoles.FixedUpdate.SetRoleNames();
                }
            }
        }
    }
    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.OnDestroy))]
    public static class ShipStatus_OnDestroy_Patch
    {
        [HarmonyPostfix, HarmonyPriority(Priority.Last)]
        public static void Postfix()
        {
            MapUtilities.CachedShipStatus = null;
            MapUtilities.MapDestroyed();
        }
    }
    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.CalculateLightRadius))]
    class LightPatch
    {
        public static bool Prefix(ShipStatus __instance, [HarmonyArgument(0)] GameData.PlayerInfo player, ref float __result)
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
        public static float GetNeutralLightRadius(ShipStatus shipStatus, bool isImpostor)
        {
            if (SubmergedCompatibility.isSubmerged()) return SubmergedCompatibility.GetSubmergedNeutralLightRadius(isImpostor);
            if (Clergyman.IsLightOutVision()) return shipStatus.MaxLightRadius * RoleClass.Clergyman.DownImpoVision;
            if (isImpostor) return shipStatus.MaxLightRadius * PlayerControl.GameOptions.ImpostorLightMod;

            SwitchSystem switchSystem = shipStatus.Systems[SystemTypes.Electrical].TryCast<SwitchSystem>();
            float lerpValue = switchSystem.Value / 255f;

            var LocalPlayer = PlayerControl.LocalPlayer;
            if (LocalPlayer.IsRole(RoleId.Nocturnality))
            {
                lerpValue = 1 - lerpValue;
            }
            return Mathf.Lerp(shipStatus.MinLightRadius, shipStatus.MaxLightRadius, lerpValue) * PlayerControl.GameOptions.CrewLightMod;
        }
    }
    [HarmonyPatch(typeof(ShipStatus), nameof(GameStartManager.Start))]
    public class Inversion
    { // マップ反転
        public static GameObject skeld;
        public static GameObject mira;
        public static GameObject polus;
        public static GameObject airship;
        public static void Prefix()
        {
            if (AmongUsClient.Instance.GameMode != GameModes.FreePlay && CustomOptions.enableMirroMap.GetBool())
            {
                if (PlayerControl.GameOptions.MapId == 0)
                {
                    skeld = GameObject.Find("SkeldShip(Clone)");
                    skeld.transform.localScale = new Vector3(-1.2f, 1.2f, 1.2f);
                    ShipStatus.Instance.InitialSpawnCenter = new Vector2(0.8f, 0.6f);
                    ShipStatus.Instance.MeetingSpawnCenter = new Vector2(0.8f, 0.6f);
                }
                else if (PlayerControl.GameOptions.MapId == 1)
                {
                    mira = GameObject.Find("MiraShip(Clone)");
                    mira.transform.localScale = new Vector3(-1f, 1f, 1f);
                    ShipStatus.Instance.InitialSpawnCenter = new Vector2(4.4f, 2.2f);
                    ShipStatus.Instance.MeetingSpawnCenter = new Vector2(-25.3921f, 2.5626f);
                    ShipStatus.Instance.MeetingSpawnCenter2 = new Vector2(-25.3921f, 2.5626f);
                }
                else if (PlayerControl.GameOptions.MapId == 2)
                {
                    polus = GameObject.Find("PolusShip(Clone)");
                    polus.transform.localScale = new Vector3(-1f, 1f, 1f);
                    ShipStatus.Instance.InitialSpawnCenter = new Vector2(-16.7f, -2.1f);
                    ShipStatus.Instance.MeetingSpawnCenter = new Vector2(-19.5f, -17f);
                    ShipStatus.Instance.MeetingSpawnCenter2 = new Vector2(-19.5f, -17f);
                }
                //airshipは選択スポーンシステムの対応ができてないため非表示
            }
        }
    }
}