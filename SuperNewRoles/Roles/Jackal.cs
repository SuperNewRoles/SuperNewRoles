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

namespace SuperNewRoles.Roles
{
    class Jackal
    {
        public static void resetCoolDown() {
            HudManagerStartPatch.JackalKillButton.MaxTimer = RoleClass.Jackal.KillCoolDown;
            HudManagerStartPatch.JackalKillButton.Timer = RoleClass.Jackal.KillCoolDown;
            HudManagerStartPatch.JackalSidekickButton.MaxTimer = RoleClass.Jackal.KillCoolDown;
            HudManagerStartPatch.JackalSidekickButton.Timer = RoleClass.Jackal.KillCoolDown;
        }
        public static void EndMeeting() {
            resetCoolDown();
        }
        public static void setPlayerOutline(PlayerControl target, Color color)
        {
            if (target == null || target.MyRend == null) return;

            target.MyRend.material.SetFloat("_Outline", 1f);
            target.MyRend.material.SetColor("_OutlineColor", color);
        }
        public class JackalFixedPatch
        {
            public static PlayerControl JackalsetTarget(bool onlyCrewmates = false, bool targetPlayersInVents = false, List<PlayerControl> untargetablePlayers = null, PlayerControl targetingPlayer = null) {
                PlayerControl result = null;
                float num = GameOptionsData.KillDistances[Mathf.Clamp(PlayerControl.GameOptions.KillDistance, 0, 2)];
                if (!MapUtilities.CachedShipStatus) return result;
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
                    if (!playerInfo.Disconnected && playerInfo.PlayerId != targetingPlayer.PlayerId && playerInfo.Object.isAlive() && (!RoleClass.Jackal.JackalPlayer.IsCheckListPlayerControl(playerInfo.Object) && !RoleClass.Jackal.SidekickPlayer.IsCheckListPlayerControl(playerInfo.Object)) && !RoleClass.TeleportingJackal.TeleportingJackalPlayer.IsCheckListPlayerControl(playerInfo.Object) && (!RoleClass.JackalSeer.JackalSeerPlayer.IsCheckListPlayerControl(playerInfo.Object) && !RoleClass.JackalSeer.SidekickSeerPlayer.IsCheckListPlayerControl(playerInfo.Object)))
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
            static void JackalPlayerOutLineTarget() {
                setPlayerOutline(JackalsetTarget(), RoleClass.Jackal.color);
            }
            public static void Postfix(PlayerControl __instance) {
                if (AmongUsClient.Instance.AmHost) {
                    if (RoleClass.Jackal.SidekickPlayer.Count != 0)
                    {
                        var upflag = true;
                        foreach (PlayerControl p in RoleClass.Jackal.JackalPlayer)
                        {
                            if (p.isAlive())
                            {
                                upflag = false;
                            }
                        }
                        if (upflag)
                        {
                            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(CachedPlayer.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.SidekickPromotes, Hazel.SendOption.Reliable, -1);
                            AmongUsClient.Instance.FinishRpcImmediately(writer);
                            RPCProcedure.SidekickPromotes();
                        }
                    }
                }
                if (PlayerControl.LocalPlayer.isRole(RoleId.Jackal)) {
                    JackalPlayerOutLineTarget();
                }
            }
        }
    }
}
