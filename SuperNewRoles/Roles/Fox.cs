using HarmonyLib;
using Hazel;
using SuperNewRoles.Buttons;
using SuperNewRoles.CustomRPC;
using SuperNewRoles.Patches;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using SuperNewRoles.CustomOption;

namespace SuperNewRoles.Roles
{
    class Fox
    {
        public static void setPlayerOutline(PlayerControl target, Color color)
        {
            if (target == null || target.MyRend == null) return;

            target.MyRend.material.SetFloat("_Outline", 1f);
            target.MyRend.material.SetColor("_OutlineColor", color);
        }
        public class FoxFixedPatch
        {/*
            public static PlayerControl FoxsetTarget(bool onlyCrewmates = false, bool targetPlayersInVents = false, List<PlayerControl> untargetablePlayers = null, PlayerControl targetingPlayer = null)
            {
                PlayerControl result = null;
                float num = GameOptionsData.KillDistances[Mathf.Clamp(PlayerControl.GameOptions.KillDistance, 0, 2)];
                if (!ShipStatus.Instance) return result;
                if (targetingPlayer == null) targetingPlayer = PlayerControl.LocalPlayer;
                if (targetingPlayer.Data.IsDead || targetingPlayer.inVent) return result;

                if (untargetablePlayers == null)
                {
                    untargetablePlayers = new List<PlayerControl>();
                }

                Vector2 truePosition = targetingPlayer.GetTruePosition();
                Il2CppSystem.Collections.Generic.List<GameData.PlayerInfo> allPlayers = GameData.Instance.AllPlayers;
                for (int i = 0; i < allPlayers.Count; i++)
                {
                    GameData.PlayerInfo playerInfo = allPlayers[i];
                    if (!playerInfo.Disconnected && playerInfo.PlayerId != targetingPlayer.PlayerId && playerInfo.Object.isAlive() && (!RoleClass.Fox.FoxPlayer.IsCheckListPlayerControl(playerInfo.Object) && !RoleClass.Fox.TraitorPlayer.IsCheckListPlayerControl(playerInfo.Object)))
                    {
                        PlayerControl @object = playerInfo.Object;
                        if (untargetablePlayers.Any(x => x == @object))
                        {
                            // if that player is not targetable: skip check
                            continue;
                        }

                        if (@object && (!@object.inVent || targetPlayersInVents))
                        {
                            Vector2 vector = @object.GetTruePosition() - truePosition;
                            float magnitude = vector.magnitude;
                            if (magnitude <= num && !PhysicsHelpers.AnyNonTriggersBetween(truePosition, vector.normalized, magnitude, Constants.ShipAndObjectsMask))
                            {
                                result = @object;
                                num = magnitude;
                            }
                        }
                    }
                }
                return result;
            }
            static void FoxPlayerOutLineTarget()
            {
                setPlayerOutline(FoxsetTarget(), RoleClass.Fox.color);
            }
            public static void Postfix(PlayerControl __instance)
            {
                if (AmongUsClient.Instance.AmHost)
                {
                    if (RoleClass.Fox.TraitorPlayer.Count != 0)
                    {
                        var upflag = true;
                        foreach (PlayerControl p in RoleClass.Fox.TraitorPlayer)
                        {
                            if (p.isAlive())
                            {
                                upflag = false;
                            }
                        }
                        if (upflag)
                        {
                            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.TraitorPromotes, Hazel.SendOption.Reliable, -1);
                            AmongUsClient.Instance.FinishRpcImmediately(writer);
                            RPCProcedure.TraitorPromotes();
                        }
                    }
                }
                if (PlayerControl.LocalPlayer.isRole(RoleId.Fox))
                {
                    FoxPlayerOutLineTarget();
                }
            }*/
        }

        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer))]
        class FoxMurderPatch
        {
            public static void Prefix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target)
            {
                if (AmongUsClient.Instance.AmHost && __instance.PlayerId != target.PlayerId)
                {
                    if (target.isRole(CustomRPC.RoleId.Fox))
                    {
                        if (EvilEraser.IsOKAndTryUse(EvilEraser.BlockTypes.FoxGuard, __instance))
                        {
                            if (!RoleClass.Fox.KillGuard.ContainsKey(target.PlayerId))
                            {
                                target.RpcProtectPlayer(target, 0);
                            }
                            else
                            {
                                if (!(RoleClass.Fox.KillGuard[target.PlayerId] <= 0))
                                {
                                    RoleClass.Fox.KillGuard[target.PlayerId]--;
                                    target.RpcProtectPlayer(target, 0);
                                }
                            }
                        }
                    }
                }
            }
            public static void Postfix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target)
            {
            }
        }

    }
}