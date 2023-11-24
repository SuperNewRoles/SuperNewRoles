using System;
using AmongUs.GameOptions;
using HarmonyLib;
using Hazel;
using SuperNewRoles.MapCustoms;
using SuperNewRoles.Mode;
using SuperNewRoles.Mode.SuperHostRoles;
using SuperNewRoles.Replay.ReplayActions;
using SuperNewRoles.Roles;
using SuperNewRoles.Roles.Crewmate;
using UnityEngine;

namespace SuperNewRoles.Patches;

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

[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.UpdateSystem), new Type[] { typeof(SystemTypes), typeof(PlayerControl), typeof(MessageReader) })]
class UpdateSystemMessagReaderPatch
{
    public static bool Prefix(ShipStatus __instance,
        [HarmonyArgument(0)] SystemTypes systemType,
        [HarmonyArgument(1)] PlayerControl player,
        [HarmonyArgument(2)] MessageReader msgReader)
    {
        int position = msgReader.Position;
        amount = msgReader.ReadByte();
        msgReader.Position = position;
        return UpdateSystemPatch.Prefix(__instance, systemType, player, amount);
    }
    static byte amount;
    public static void Postfix(ShipStatus __instance,
        [HarmonyArgument(0)] SystemTypes systemType,
        [HarmonyArgument(1)] PlayerControl player,
        [HarmonyArgument(2)] MessageReader msgReader)
    {
        UpdateSystemPatch.Postfix(__instance, systemType, player, amount);
    }
}

[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.UpdateSystem), new Type[] { typeof(SystemTypes), typeof(PlayerControl), typeof(byte) })]
class UpdateSystemPatch
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
            if (systemType is SystemTypes.Comms or SystemTypes.Sabotage or SystemTypes.Electrical)
            {
                if (PlayerControl.LocalPlayer.IsRole(RoleId.Painter) && RoleClass.Painter.CurrentTarget != null && RoleClass.Painter.CurrentTarget.PlayerId == player.PlayerId) Roles.Crewmate.Painter.Handle(Roles.Crewmate.Painter.ActionType.SabotageRepair);
            }
        }
        if ((ModeHandler.IsMode(ModeId.BattleRoyal) || ModeHandler.IsMode(ModeId.Zombie) || ModeHandler.IsMode(ModeId.HideAndSeek) || ModeHandler.IsMode(ModeId.CopsRobbers, ModeId.PantsRoyal)) && (systemType == SystemTypes.Sabotage || systemType == SystemTypes.Doors)) return false;

        if (systemType == SystemTypes.Electrical && 0 <= amount && amount <= 4) // 停電を直そうとした
        {
            if ((player.IsMadRoles() && !CustomOptionHolder.MadRolesCanFixElectrical.GetBool()) ||
                player.IsRole(RoleId.Vampire, RoleId.Dependents))
            {
                return false;
            }
        }
        if (systemType == SystemTypes.Comms && amount is 0 or 16 or 17) // コミュサボを直そうとした
        {
            if (player.IsMadRoles() && !CustomOptionHolder.MadRolesCanFixComms.GetBool())
            {
                return false;
            }
        }
        if (ModeHandler.IsMode(ModeId.SuperHostRoles))
        {
            bool returndata = MorePatch.UpdateSystem(__instance, systemType, player, amount);
            return returndata;
        }
        return true;
    }
    public static void Postfix(
        ShipStatus __instance,
        [HarmonyArgument(0)] SystemTypes systemType,
        [HarmonyArgument(1)] PlayerControl player,
        [HarmonyArgument(2)] byte amount)
    {
        ReplayActionUpdateSystem.Create(systemType, player.PlayerId, amount);
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
                ChangeName.SetRoleNames();
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
        float num = 1f;
        if (__instance.Systems.ContainsKey(SystemTypes.Electrical))
        {
            SwitchSystem switchSystem = __instance.Systems[SystemTypes.Electrical].TryCast<SwitchSystem>();
            if (switchSystem != null)
                num = switchSystem.Value / 255f;
        }


        __result = player == null || player.IsDead
            ? __instance.MaxLightRadius
            : player.Object.IsRole(RoleId.CountChanger) && CountChanger.GetRoleType(player.Object) == TeamRoleType.Crewmate
            ? GetNeutralLightRadius(__instance, false)
            : Squid.Abilitys.IsObstruction
            ? Mathf.Lerp(__instance.MaxLightRadius * Squid.SquidDownVision.GetFloat(), __instance.MaxLightRadius * Squid.SquidDownVision.GetFloat(), num)
            : player.Object.IsImpostor() || RoleHelpers.IsImpostorLight(player.Object)
            ? GetNeutralLightRadius(__instance, true)
            : player.Object.IsRole(RoleId.Lighter) && RoleClass.Lighter.IsLightOn
            ? Mathf.Lerp(__instance.MaxLightRadius * RoleClass.Lighter.UpVision, __instance.MaxLightRadius * RoleClass.Lighter.UpVision, num)
            : GetNeutralLightRadius(__instance, false);
        return false;
    }
    public static float GetNeutralLightRadius(ShipStatus shipStatus, bool isImpostor)
    {
        if (Clergyman.IsLightOutVision()) return shipStatus.MaxLightRadius * RoleClass.Clergyman.DownImpoVision;
        if (isImpostor) return shipStatus.MaxLightRadius * GameManager.Instance.LogicOptions.currentGameOptions.GetFloat(FloatOptionNames.ImpostorLightMod);

        float lerpValue = 1;
        if (shipStatus.Systems.TryGetValue(SystemTypes.Electrical, out ISystemType elec)){
            lerpValue = elec.TryCast<SwitchSystem>().Value / 255f;
        }
        var LocalPlayer = PlayerControl.LocalPlayer;
        if (LocalPlayer.IsRole(RoleId.Dependents) ||
            (LocalPlayer.IsRole(RoleId.Nocturnality) && !ModeHandler.IsMode(ModeId.SuperHostRoles)))
            lerpValue = 1 - lerpValue;
        return Mathf.Lerp(shipStatus.MinLightRadius, shipStatus.MaxLightRadius, lerpValue) * GameManager.Instance.LogicOptions.currentGameOptions.GetFloat(FloatOptionNames.CrewLightMod);
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
        if (AmongUsClient.Instance.NetworkMode != NetworkModes.FreePlay && MapOption.MapOption.IsenableMirrorMap)
        {
            if (GameManager.Instance.LogicOptions.currentGameOptions.MapId == 0)
            {
                skeld = GameObject.Find("SkeldShip(Clone)");
                skeld.transform.localScale = new Vector3(-1.2f, 1.2f, 1.2f);
                MapUtilities.CachedShipStatus.InitialSpawnCenter = new Vector2(0.8f, 0.6f);
                MapUtilities.CachedShipStatus.MeetingSpawnCenter = new Vector2(0.8f, 0.6f);
            }
            else if (GameManager.Instance.LogicOptions.currentGameOptions.MapId == 1)
            {
                mira = GameObject.Find("MiraShip(Clone)");
                mira.transform.localScale = new Vector3(-1f, 1f, 1f);
                MapUtilities.CachedShipStatus.InitialSpawnCenter = new Vector2(4.4f, 2.2f);
                MapUtilities.CachedShipStatus.MeetingSpawnCenter = new Vector2(-25.3921f, 2.5626f);
                MapUtilities.CachedShipStatus.MeetingSpawnCenter2 = new Vector2(-25.3921f, 2.5626f);
            }
            else if (GameManager.Instance.LogicOptions.currentGameOptions.MapId == 2)
            {
                polus = GameObject.Find("PolusShip(Clone)");
                polus.transform.localScale = new Vector3(-1f, 1f, 1f);
                MapUtilities.CachedShipStatus.InitialSpawnCenter = new Vector2(-16.7f, -2.1f);
                MapUtilities.CachedShipStatus.MeetingSpawnCenter = new Vector2(-19.5f, -17f);
                MapUtilities.CachedShipStatus.MeetingSpawnCenter2 = new Vector2(-19.5f, -17f);
            }
            //airshipは選択スポーンシステムの対応ができてないため非表示
        }
    }
}