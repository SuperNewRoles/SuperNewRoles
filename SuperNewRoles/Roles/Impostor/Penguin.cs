using System.Linq;
using HarmonyLib;
using Hazel;
using SuperNewRoles.Buttons;
using SuperNewRoles.Helpers;
using SuperNewRoles.Mode;
using SuperNewRoles.Mode.SuperHostRoles;
using UnityEngine;

namespace SuperNewRoles.Roles.Impostor
{
    public static class Penguin
    {
        [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.FixedUpdate))]
        public static class PlayerPhysicsSpeedPatch
        {
            public static void Postfix(PlayerPhysics __instance)
            {
                if (AmongUsClient.Instance.GameState != AmongUsClient.GameStates.Started) return;
                if (ModeHandler.IsMode(ModeId.Default))
                {
                    if (RoleClass.Penguin.PenguinData.Any(x => x.Value != null && x.Value.PlayerId == __instance.myPlayer.PlayerId))
                    {
                        __instance.body.velocity = new(0f, 0f);
                    }
                }
            }
        }
        public static void FixedUpdate()
        {
            if (RoleClass.Penguin.PenguinData.Count <= 0) return;
            foreach (var data in RoleClass.Penguin.PenguinData.ToArray())
            {
                if (data.Key == null || data.Value == null
                    || !data.Key.IsRole(RoleId.Penguin)
                    || data.Key.IsDead()
                    || data.Value.IsDead())
                {
                    if (data.Key != null && data.Key.PlayerId == CachedPlayer.LocalPlayer.PlayerId)
                    {
                        HudManagerStartPatch.PenguinButton.isEffectActive = false;
                        HudManagerStartPatch.PenguinButton.MaxTimer = CustomOptionHolder.PenguinCoolTime.GetFloat();
                        HudManagerStartPatch.PenguinButton.Timer = HudManagerStartPatch.PenguinButton.MaxTimer;
                        HudManagerStartPatch.PenguinButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
                    }
                    RoleClass.Penguin.PenguinData.Remove(data.Key);
                    continue;
                }
                data.Value.transform.position = data.Key.transform.position;
            }
        }
        public static void SHRFixedUpdate()
        {
            if (RoleClass.Penguin.PenguinData.Count >= 1) 
                foreach (var data in RoleClass.Penguin.PenguinData.ToArray())
                {
                    if (data.Key == null || data.Value == null
                        || !data.Key.IsRole(RoleId.Penguin)
                        || data.Key.IsDead()
                        || data.Value.IsDead())
                    {
                        RoleClass.Penguin.PenguinData.Remove(data.Key);
                        continue;
                    }
                    RPCHelper.RpcSnapTo(data.Value, data.Key.transform.position);
                    if (RoleClass.Penguin.PenguinTimer.ContainsKey(data.Key))
                    {
                        RoleClass.Penguin.PenguinTimer[data.Key] -= Time.fixedDeltaTime;
                        if(RoleClass.Penguin.PenguinTimer[data.Key] <= 0f)
                        {
                            RoleClass.Penguin.PenguinTimer[data.Key] = CustomOptionHolder.PenguinDurationTime.GetFloat();
                            data.Key.RpcMurderPlayerPlus(data.Value);
                            RoleClass.Penguin.PenguinData.Remove(data.Key);
                        }
                    }
                }
        }
        public static void RpcMurderPlayerPlus(this PlayerControl __instance, PlayerControl target)
        {
            if (__instance == null || __instance.Data == null) return;
            if (target == null || target.Data == null) return;
            bool isKill = true;
            if (target.IsRole(RoleId.StuntMan) && (!RoleClass.StuntMan.GuardCount.ContainsKey(target.PlayerId) || RoleClass.StuntMan.GuardCount[target.PlayerId] >= 1))
            {
                if (EvilEraser.IsOKAndTryUse(EvilEraser.BlockTypes.StuntmanGuard, __instance))
                {
                    KillGuard();
                    MessageWriter writer = RPCHelper.StartRPC(CustomRPC.UseStuntmanCount);
                    writer.Write(target.PlayerId);
                    writer.EndRPC();
                    RPCProcedure.UseStuntmanCount(target.PlayerId);
                    isKill = false;
                }
            }
            if (target.IsRole(RoleId.MadStuntMan) && (!RoleClass.MadStuntMan.GuardCount.ContainsKey(target.PlayerId) || RoleClass.MadStuntMan.GuardCount[target.PlayerId] >= 1))
            {
                if (EvilEraser.IsOKAndTryUse(EvilEraser.BlockTypes.MadStuntmanGuard, __instance))
                {
                    KillGuard();
                    MessageWriter writer = RPCHelper.StartRPC(CustomRPC.UseStuntmanCount);
                    writer.Write(target.PlayerId);
                    writer.EndRPC();
                    RPCProcedure.UseStuntmanCount(target.PlayerId);
                    isKill = false;
                }
            }
            if (target.IsRole(RoleId.Shielder) && RoleClass.Shielder.IsShield[target.PlayerId])
            {
                MessageWriter writer = RPCHelper.StartRPC(CustomRPC.ShielderProtect);
                writer.Write(target.PlayerId);
                writer.Write(target.PlayerId);
                writer.Write(0);
                writer.EndRPC();
                RPCProcedure.ShielderProtect(target.PlayerId, target.PlayerId, 0);
                isKill = false;
            }
            if (target.IsRole(RoleId.Fox) && (!RoleClass.Fox.KillGuard.ContainsKey(target.PlayerId) || RoleClass.Fox.KillGuard[target.PlayerId] >= 1))
            {
                if (EvilEraser.IsOKAndTryUse(EvilEraser.BlockTypes.FoxGuard, __instance))
                {
                    KillGuard();
                    MessageWriter writer = RPCHelper.StartRPC(CustomRPC.UseStuntmanCount);
                    writer.Write(target.PlayerId);
                    writer.EndRPC();
                    RPCProcedure.UseStuntmanCount(target.PlayerId);
                    isKill = false;
                }
            }
            if (isKill) __instance.RpcMurderPlayer(target);

            void KillGuard()
            {
                if (ModeHandler.IsMode(ModeId.Default))
                {
                    MessageWriter writer = RPCHelper.StartRPC(CustomRPC.UncheckedProtect);
                    writer.Write(target.PlayerId);
                    writer.Write(target.PlayerId);
                    writer.Write(0);
                    writer.EndRPC();
                    RPCProcedure.UncheckedProtect(target.PlayerId, target.PlayerId, 0);
                }
                else
                {
                    __instance.RpcShowGuardEffect(target);
                }
            }
        }
    }
}