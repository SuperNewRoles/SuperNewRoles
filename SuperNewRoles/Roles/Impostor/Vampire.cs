using System;
using System.Linq;
using HarmonyLib;
using Hazel;
using SuperNewRoles.Buttons;
using SuperNewRoles.CustomObject;
using SuperNewRoles.Helpers;
using UnityEngine;

namespace SuperNewRoles.Roles
{
    class Vampire
    {
        public static void WrapUp()
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
        [HarmonyPatch(typeof(VitalsMinigame), nameof(VitalsMinigame.Update))]
        class VitalsMinigameUpdatePatch
        {
            static void Postfix(VitalsMinigame __instance)
            {
                for (int k = 0; k < __instance.vitals.Length; k++)
                {
                    VitalsPanel vitalsPanel = __instance.vitals[k];
                    GameData.PlayerInfo player = GameData.Instance.AllPlayers[k];
                    if (player.Object.IsRole(RoleId.Vampire, RoleId.Dependents))
                        if (vitalsPanel.IsDead)
                            vitalsPanel.SetAlive();
                }
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
            public static void AllClient() {
                Count--;
                if (Count > 0) return;
                Count = 5;
                foreach (var data in RoleClass.Vampire.Targets.ToArray()) {
                    if (data.Key == null || data.Value == null || !data.Key.IsRole(RoleId.Vampire) || data.Key.IsDead()|| data.Value.IsDead()) {
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
}