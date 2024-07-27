using System.Linq;
using AmongUs.GameOptions;
using HarmonyLib;
using Hazel;
using SuperNewRoles.Helpers;
using SuperNewRoles.MapCustoms;
using SuperNewRoles.Mode.PlusMode;
using SuperNewRoles.Roles;

namespace SuperNewRoles.Mode.SuperHostRoles;

class ReportDeadBody
{
    public static bool ReportDeadBodyPatch(PlayerControl __instance, NetworkedPlayerInfo target)
    {
        if (!AmongUsClient.Instance.AmHost) return true;
        if (__instance.Data.IsDead) return false;
        if (RoleClass.Assassin.TriggerPlayer != null) return false;
        //会議ボタンでもレポートでも起こる処理

        if (target == null) //会議ボタンのみで起こる処理
        {

            if (MapCustomHandler.IsMapCustom(MapCustomHandler.MapCustomId.TheFungle, false) &&
                MapCustom.TheFungleMushroomMixupOption.GetBool() &&
                MapCustom.TheFungleMushroomMixupCantOpenMeeting.GetBool() &&
                __instance.IsMushroomMixupActive())
                return false;
            if (PlusGameOptions.EmergencyMeetingsCallstate.enabledSetting && PlusGameOptions.EmergencyMeetingsCallstate.maxCount != byte.MaxValue && // 会議回数制限が有効で
                PlusGameOptions.EmergencyMeetingsCallstate.maxCount <= Patches.ReportDeadBodyPatch.MeetingCount.emergency) // 全体回数を使い切っているなら
                return false;
            return true;
        };

        //死体レポートのみで起こる処理
        DeadPlayer deadPlayer;
        deadPlayer = DeadPlayer.deadPlayers?.Where(x => x.player?.PlayerId == CachedPlayer.LocalPlayer.PlayerId)?.FirstOrDefault();
        //if (RoleClass.Bait.ReportedPlayer.Contains(target.PlayerId)) return true;
        if (__instance.IsRole(RoleId.Minimalist))
        {
            return RoleClass.Minimalist.UseReport;
        }
        if (__instance.IsRole(RoleId.Fox))
        {
            return RoleClass.Fox.UseReport;
        }
        if (__instance.IsRole(RoleId.Amnesiac) &&
            target != null &&
            !target.Disconnected &&
            target.Object)
        {
            RoleTypes? DesyncRoleTypes = RoleSelectHandler.GetDesyncRole(target.Object);
            RoleTypes SyncRoleTypes = target.RoleWhenAlive == null ? target.Role.Role : target.RoleWhenAlive.Value;
            CustomRpcSender sender = CustomRpcSender.Create("ReportDeadBodyPatch", SendOption.Reliable);
            if (DesyncRoleTypes.HasValue)
            {
                __instance.RpcSetRoleImmediately(__instance.IsMod() ? RoleTypes.Crewmate : RoleTypes.Engineer, true);
                new LateTask(() =>
                {
                    if (!__instance.IsMod())
                    {
                        __instance.RpcSetRoleDesync(DesyncRoleTypes.Value, true);
                        foreach (PlayerControl player in CachedPlayer.AllPlayers.AsSpan())
                        {
                            if (player.PlayerId != __instance.PlayerId)
                                player.RpcSetRoleDesync(RoleTypes.Scientist, true, __instance);
                        }
                    }
                    __instance.SetRole(RoleTypes.Crewmate, true);
                }, 0.1f);
            }
            else if (SyncRoleTypes.IsImpostorRole())
            {
                __instance.RpcSetRoleImmediately(RoleTypes.Tracker, true);
                new LateTask(() =>
                {
                    foreach (PlayerControl player in CachedPlayer.AllPlayers.AsSpan())
                    {
                        if (player.PlayerId != PlayerControl.LocalPlayer.PlayerId &&
                            (player.IsImpostor() || player.PlayerId == __instance.PlayerId))
                            __instance.RpcSetRoleDesync(SyncRoleTypes, true, player);
                    }
                    __instance.SetRole(SyncRoleTypes, true);
                }, 0.1f);
            }
            else
                __instance.RpcSetRoleImmediately(SyncRoleTypes, true);
            __instance.SwapRoleRPC(target.Object);
            target.Object.SetRoleRPC(__instance.GetRole());
            ChangeName.SetRoleName(__instance, sender:sender);
            sender.SendMessage();
        }
        //if (target.Object.IsRole(RoleId.Bait) && (!deadPlayer.killerIfExisting.IsRole(RoleId.Minimalist) || RoleClass.Minimalist.UseReport)) if (!RoleClass.Bait.ReportedPlayer.Contains(target.PlayerId)) { return false; } else { return true; }

        return true;
    }
}