using System.Linq;
using UnityEngine;
using SuperNewRoles.CustomRPC;
using HarmonyLib;
using System.Collections.Generic;

namespace SuperNewRoles.Roles.Neutral
{
    public static class Spelunker
    {
        //ここにコードを書きこんでください
        public static bool CheckSetRole(PlayerControl player, RoleId role)
        {
            if (player.IsRole(RoleId.Spelunker)) {
                if (role != RoleId.Spelunker)
                {
                    player.RpcMurderPlayer(player);
                    return false;
                }
            }
            return true;
        }
        const float VentDistance = 0.35f;
        public static void FixedUpdate()
        {
            if (RoleClass.IsMeeting) return;
            //ベント判定
            if (RoleClass.Spelunker.VentDeathChance > 0)
            {
                Vent CurrentVent = null;
                if (!MapUtilities.CachedShipStatus.AllVents.ToList().TrueForAll(vent =>
                {
                    bool isok = Vector2.Distance(vent.transform.position + new Vector3(0, 0.15f, 0), CachedPlayer.LocalPlayer.transform.position) < VentDistance;
                    if (isok)
                    {
                        CurrentVent = vent;
                    }
                    return !isok;
                }))
                {
                    if (!RoleClass.Spelunker.IsVentChecked)
                    {
                        RoleClass.Spelunker.IsVentChecked = true;
                        if (ModHelpers.IsSucsessChance(RoleClass.Spelunker.VentDeathChance)) PlayerControl.LocalPlayer.RpcMurderPlayer(PlayerControl.LocalPlayer);
                    }
                }
                else
                {
                    RoleClass.Spelunker.IsVentChecked = false;
                }
            }
            //コミュと停電の不安死
            if (RoleClass.Spelunker.CommsOrLightdownDeathTime != -1)
            {
                if (RoleHelpers.IsComms() || RoleHelpers.IsLightdown()) {
                    RoleClass.Spelunker.CommsOrLightdownTime -= Time.fixedDeltaTime;
                    if (RoleClass.Spelunker.CommsOrLightdownTime <= 0)
                    {
                        PlayerControl.LocalPlayer.RpcMurderPlayer(PlayerControl.LocalPlayer);
                    }
                } else
                {
                    RoleClass.Spelunker.CommsOrLightdownTime = RoleClass.Spelunker.CommsOrLightdownDeathTime;
                }
            }
            if (DeathPosition != null && Vector2.Distance((Vector2)DeathPosition, CachedPlayer.LocalPlayer.transform.position) < 0.5f)
            {
                PlayerControl.LocalPlayer.RpcMurderPlayer(PlayerControl.LocalPlayer);
            }
        }
        public static void WrapUp()
        {
            DeathPosition = null;
        }
        public static Vector2?DeathPosition;
        [HarmonyPatch(typeof(MovingPlatformBehaviour),nameof(MovingPlatformBehaviour.UsePlatform))]
        class MovingPlatformUsePlatformPatch
        {
            public static void Postfix(MovingPlatformBehaviour __instance, PlayerControl target)
            {
                if (target.PlayerId == CachedPlayer.LocalPlayer.PlayerId &&
                    target.IsRole(RoleId.Spelunker))
                {
                    bool isok = ModHelpers.IsSucsessChance(RoleClass.Spelunker.LiftDeathChance);
                    Logger.Info($"{target.Data.PlayerName}のぬーん転落死の結果は{isok}でした ", "ぬーん転落死");
                    if (isok)
                    {
                        DeathPosition = __instance.transform.parent.TransformPoint((!__instance.IsLeft) ? __instance.LeftUsePosition : __instance.RightUsePosition);
                        Logger.Info(DeathPosition.ToString());
                    }
                }
            }
        }
        [HarmonyPatch(typeof(DoorConsole), nameof(DoorConsole.Use))]
        class DoorConsoleOpenPatch
        {
            public static void Postfix(DoorConsole __instance)
            {
                __instance.CanUse(PlayerControl.LocalPlayer.Data, out var canUse, out var _);
                if (canUse)
                {
                    if (PlayerControl.LocalPlayer.IsRole(RoleId.Spelunker) && ModHelpers.IsSucsessChance(RoleClass.Spelunker.DoorOpenChance))
                    {
                        PlayerControl.LocalPlayer.RpcMurderPlayer(PlayerControl.LocalPlayer);
                    }
                }
            }
        }
    }
}