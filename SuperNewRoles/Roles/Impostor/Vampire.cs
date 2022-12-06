using System;
using System.Linq;
using HarmonyLib;
using Hazel;
using SuperNewRoles.Buttons;
using SuperNewRoles.Helpers;
using UnityEngine;

namespace SuperNewRoles.Roles;

class Vampire
{
    /// <summary>
    /// ヴァンパイアの血痕処理
    /// </summary>
    public static void SetActiveBloodStaiWrapUpPatch()
    {
        foreach (var data in RoleClass.Vampire.NoActiveTurnWait.ToArray())
        {
            RoleClass.Vampire.NoActiveTurnWait[data.Key]--;
            if (data.Value - 1 <= 0)
            {
                foreach (var bloodstain in data.Key) GameObject.Destroy(bloodstain.BloodStainObject);
                RoleClass.Vampire.NoActiveTurnWait.Remove(data.Key);
            }
        }
        foreach (var data in RoleClass.Vampire.WaitActiveBloodStains)
            data.BloodStainObject.SetActive(true);
        RoleClass.Vampire.NoActiveTurnWait.Add(RoleClass.Vampire.WaitActiveBloodStains, CustomOptionHolder.VampireViewBloodStainsTurn.GetInt());
        RoleClass.Vampire.WaitActiveBloodStains = new();
    }

    /// <summary>
    /// 眷属の心中処理
    /// </summary>
    public static void DependentsExileWrapUpPatch(PlayerControl exiled)
    {
        if (PlayerControl.LocalPlayer.IsRole(RoleId.Dependents) && PlayerControl.LocalPlayer.IsAlive())
        {
            bool Is = true;
            foreach (PlayerControl p in RoleClass.Vampire.VampirePlayer) if (p.IsAlive() && (exiled == null || exiled.PlayerId != p.PlayerId)) Is = false;
            if (Is)
                PlayerControl.LocalPlayer.RpcExiledUnchecked();
        }
    }
    [HarmonyPatch(typeof(VitalsPanel), nameof(VitalsPanel.SetDead))]
    class VitalsPanelSetDeadPatch
    {
        static bool Prefix(VitalsPanel __instance)
        {
            return __instance.PlayerInfo.Object is null || !__instance.PlayerInfo.Object.IsRole(RoleId.Vampire, RoleId.Dependents);
        }
    }
    [HarmonyPatch(typeof(VitalsPanel), nameof(VitalsPanel.SetDisconnected))]
    class VitalsPanelSetDisconnectPatch
    {
        static bool Prefix(VitalsPanel __instance)
        {
            return __instance.PlayerInfo.Object is null || !__instance.PlayerInfo.Object.IsRole(RoleId.Vampire, RoleId.Dependents);
        }
    }
    public static void OnMurderPlayer(PlayerControl source, PlayerControl target)
    {
        if (source.IsRole(RoleId.Vampire) && PlayerControl.LocalPlayer.IsRole(RoleId.Dependents))
            HudManagerStartPatch.DependentsKillButton.Timer = HudManagerStartPatch.DependentsKillButton.MaxTimer;
        else if (source.IsRole(RoleId.Dependents) && PlayerControl.LocalPlayer.IsRole(RoleId.Vampire))
            PlayerControl.LocalPlayer.killTimer = RoleHelpers.GetCoolTime(CachedPlayer.LocalPlayer);
    }
    public static class FixedUpdate
    {
        static int Count = 0;
        public static void DependentsOnly()
        {
            if (RoleClass.IsMeeting) return;
            foreach (PlayerControl p in RoleClass.Vampire.VampirePlayer) if (p.IsAlive()) return;
            PlayerControl.LocalPlayer.RpcMurderPlayer(PlayerControl.LocalPlayer);
        }
        public static void AllClient()
        {
            Count--;
            if (Count > 0) return;
            Count = 3;
            foreach (var data in RoleClass.Vampire.Targets.ToArray())
            {
                if (data.Key == null || data.Value == null || !data.Key.IsRole(RoleId.Vampire) || data.Key.IsDead() || data.Value.IsDead())
                {
                    RoleClass.Vampire.Targets.Remove(data.Key);
                    continue;
                }
                if (!RoleClass.Vampire.BloodStains.ContainsKey(data.Value.PlayerId))
                    RoleClass.Vampire.BloodStains.Add(data.Value.PlayerId, new());
                RoleClass.Vampire.BloodStains[data.Value.PlayerId].Add(new(data.Value));
            }
        }
        public static void VampireOnly()
        {
            if (RoleClass.Vampire.target == null) return;
            FastDestroyableSingleton<HudManager>.Instance.KillButton.SetTarget(null);
            PlayerControl.LocalPlayer.killTimer = RoleHelpers.GetCoolTime(CachedPlayer.LocalPlayer);
            var TimeSpanDate = new TimeSpan(0, 0, 0, (int)RoleClass.Vampire.KillDelay);
            RoleClass.Vampire.Timer = (float)((RoleClass.Vampire.KillTimer + TimeSpanDate - DateTime.Now).TotalSeconds);
            SuperNewRolesPlugin.Logger.LogInfo("ヴァンパイア:" + RoleClass.Vampire.Timer);
            if (RoleClass.Vampire.Timer <= 0f)
            {
                MessageWriter writer = RPCHelper.StartRPC(CustomRPC.RPCMurderPlayer);
                writer.Write(CachedPlayer.LocalPlayer.PlayerId);
                writer.Write(RoleClass.Vampire.target.PlayerId);
                writer.Write(0);
                writer.EndRPC();
                RPCProcedure.RPCMurderPlayer(CachedPlayer.LocalPlayer.PlayerId, RoleClass.Vampire.target.PlayerId, 0);
                writer = RPCHelper.StartRPC(CustomRPC.SetVampireStatus);
                writer.Write(CachedPlayer.LocalPlayer.PlayerId);
                writer.Write(RoleClass.Vampire.target.PlayerId);
                writer.Write(false);
                writer.Write(RoleClass.Vampire.target.IsDead());
                writer.EndRPC();
                RPCProcedure.SetVampireStatus(CachedPlayer.LocalPlayer.PlayerId, RoleClass.Vampire.target.PlayerId, false, RoleClass.Vampire.target.IsDead());
                RoleClass.Vampire.target = null;
            }
        }
    }
}
