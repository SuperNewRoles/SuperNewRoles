using System.Collections.Generic;
using System.Linq;
using Hazel;
using SuperNewRoles.Buttons;
using SuperNewRoles.CustomRPC;
using UnityEngine;

namespace SuperNewRoles.Roles
{
    class JackalSeer
    {
        public static void ResetCoolDown()
        {
            HudManagerStartPatch.JackalKillButton.MaxTimer = RoleClass.JackalSeer.KillCoolDown;
            HudManagerStartPatch.JackalKillButton.Timer = RoleClass.JackalSeer.KillCoolDown;
            HudManagerStartPatch.JackalSeerSidekickButton.MaxTimer = RoleClass.JackalSeer.KillCoolDown;
            HudManagerStartPatch.JackalSeerSidekickButton.Timer = RoleClass.JackalSeer.KillCoolDown;
        }
        public static void EndMeeting()
        {
            ResetCoolDown();
        }
        public static void SetPlayerOutline(PlayerControl target, Color color)
        {
            if (target == null || target.MyRend == null) return;

            target.MyRend().material.SetFloat("_Outline", 1f);
            target.MyRend().material.SetColor("_OutlineColor", color);
        }
        public class JackalSeerFixedPatch
        {
            public static PlayerControl JackalSeerSetTarget(bool onlyCrewmates = false, bool targetPlayersInVents = false, List<PlayerControl> untargetablePlayers = null, PlayerControl targetingPlayer = null)
            {
                PlayerControl result = null;
                float num = GameOptionsData.KillDistances[Mathf.Clamp(PlayerControl.GameOptions.KillDistance, 0, 2)];
                if (!MapUtilities.CachedShipStatus) return result;
                if (targetingPlayer == null) targetingPlayer = PlayerControl.LocalPlayer;
                if (targetingPlayer.Data.IsDead || targetingPlayer.inVent) return result;

                if (untargetablePlayers == null)
                {
                    untargetablePlayers = new();
                }

                Vector2 truePosition = targetingPlayer.GetTruePosition();
                Il2CppSystem.Collections.Generic.List<GameData.PlayerInfo> allPlayers = GameData.Instance.AllPlayers;
                for (int i = 0; i < allPlayers.Count; i++)
                {
                    GameData.PlayerInfo playerInfo = allPlayers[i];
                    //下記JackalSeerがbuttonのターゲットにできない役職の設定
                    if (playerInfo.Object.IsAlive() && playerInfo.PlayerId != targetingPlayer.PlayerId && !playerInfo.Object.IsJackalTeamJackal() && !playerInfo.Object.IsJackalTeamSidekick())
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
            static void JackalSeerPlayerOutLineTarget()
            {
                SetPlayerOutline(JackalSeerSetTarget(), RoleClass.JackalSeer.color);
            }
            public static void Postfix(PlayerControl __instance, RoleId role)
            {
                if (AmongUsClient.Instance.AmHost)
                {
                    if (RoleClass.JackalSeer.SidekickSeerPlayer.Count > 0)
                    {
                        var upflag = true;
                        foreach (PlayerControl p in RoleClass.JackalSeer.JackalSeerPlayer)
                        {
                            if (p.IsAlive())
                            {
                                upflag = false;
                            }
                        }
                        if (upflag)
                        {
                            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.SidekickSeerPromotes, SendOption.Reliable, -1);
                            AmongUsClient.Instance.FinishRpcImmediately(writer);
                            RPCProcedure.SidekickSeerPromotes();
                        }
                    }
                }
                if (role == RoleId.JackalSeer)
                {
                    JackalSeerPlayerOutLineTarget();
                }
            }
        }
    }
}