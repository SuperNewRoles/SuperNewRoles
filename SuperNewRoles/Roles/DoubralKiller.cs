using Hazel;
using SuperNewRoles.CustomRPC;
using SuperNewRoles.Mode;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using HarmonyLib;
using SuperNewRoles.Buttons;
using System.Linq;

namespace SuperNewRoles.Roles
{
    public static class DoubralKiller
    {
        public static void SetDoubralKillerButton()
        {
            if (PlayerControl.LocalPlayer.isRole(CustomRPC.RoleId.DoubralKiller))
            {
                if (RoleClass.DoubralKiller.NoKill == true)
                {
                    HudManager.Instance.KillButton.gameObject.SetActiveRecursively(false);
                    HudManager.Instance.KillButton.gameObject.SetActive(false);
                    HudManager.Instance.KillButton.graphic.enabled = false;
                    HudManager.Instance.KillButton.enabled = false;
                    HudManager.Instance.KillButton.graphic.sprite = null;

                    //純正キルボタンばいばい
                }
            }
        }
        public class FixedUpdate2nd
        {
            public static void Postfix()
            {
                SetDoubralKillerButton();
            }
        }
        public static void resetNormalCoolDown()
        {
            HudManagerStartPatch.DoubralKillerNormalKillButton.MaxTimer = RoleClass.DoubralKiller.KillTime;
            HudManagerStartPatch.DoubralKillerNormalKillButton.Timer = RoleClass.DoubralKiller.KillTime;
            RoleClass.DoubralKiller.SuicideRTime = RoleClass.DoubralKiller.SuicideDefaultRTime;
        }
        public static void resetSecondCoolDown()
        {
            HudManagerStartPatch.DoubralKillerSecondKillButton.MaxTimer = RoleClass.DoubralKiller.SecondKillTime;
            HudManagerStartPatch.DoubralKillerSecondKillButton.Timer = RoleClass.DoubralKiller.SecondKillTime;
            RoleClass.DoubralKiller.SuicideLTime = RoleClass.DoubralKiller.SuicideDefaultLTime;
        }
        public static void EndMeeting()
        {
            resetSecondCoolDown();
            resetNormalCoolDown();
        }
        public static void FixedUpdate()
        {
            bool IsViewButtonLText = false;
            bool IsViewButtonRText = false;
            static void Postfix()
            {
                SuperNewRolesPlugin.Logger.LogInfo(RoleClass.DoubralKiller.SuicideLTime);
                SuperNewRolesPlugin.Logger.LogInfo(RoleClass.DoubralKiller.SuicideRTime);
            }
            if (!RoleClass.IsMeeting)
            {
                if (PlayerControl.LocalPlayer.isRole(RoleId.DoubralKiller))
                { 
                    if (RoleClass.DoubralKiller.IsSuicideViewL)
                    {
                        IsViewButtonLText = true;
                        if (RoleClass.DoubralKiller.SuicideLTime <= 0)
                        {
                            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.RPCMurderPlayer, SendOption.Reliable, -1);
                            writer.Write(PlayerControl.LocalPlayer.PlayerId);
                            writer.Write(PlayerControl.LocalPlayer.PlayerId);
                            writer.Write(byte.MaxValue);
                            AmongUsClient.Instance.FinishRpcImmediately(writer);
                            RPCProcedure.RPCMurderPlayer(PlayerControl.LocalPlayer.PlayerId, PlayerControl.LocalPlayer.PlayerId, byte.MaxValue);
                        }
                    }
                    if (RoleClass.DoubralKiller.IsSuicideViewR)
                    {
                        IsViewButtonRText = true;
                        RoleClass.DoubralKiller.SuicideRTime -= Time.fixedDeltaTime;
                        if (RoleClass.DoubralKiller.SuicideRTime <= 0)
                        {
                            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.RPCMurderPlayer, SendOption.Reliable, -1);
                            writer.Write(PlayerControl.LocalPlayer.PlayerId);
                            writer.Write(PlayerControl.LocalPlayer.PlayerId);
                            writer.Write(byte.MaxValue);
                            AmongUsClient.Instance.FinishRpcImmediately(writer);
                            RPCProcedure.RPCMurderPlayer(PlayerControl.LocalPlayer.PlayerId, PlayerControl.LocalPlayer.PlayerId, byte.MaxValue);
                        }
                    }
                }

             }
            
            if (IsViewButtonRText && RoleClass.DoubralKiller.IsSuicideViewR && PlayerControl.LocalPlayer.isAlive())
            {
                RoleClass.DoubralKiller.SuicideKillRText.text = string.Format(ModTranslation.getString("DoubralKillerSuicideRText"), ((int)RoleClass.DoubralKiller.SuicideRTime) + 1);
            }
            else
            {
                if (RoleClass.DoubralKiller.SuicideKillRText.text != "")
                {
                    RoleClass.DoubralKiller.SuicideKillRText.text = "";
                }
            }
            if (IsViewButtonLText && RoleClass.DoubralKiller.IsSuicideViewL && PlayerControl.LocalPlayer.isAlive())
            {
                RoleClass.DoubralKiller.SuicideKillLText.text = string.Format(ModTranslation.getString("DoubralKillerSuicideLText"), ((int)RoleClass.DoubralKiller.SuicideLTime) + 1);
            }
            else
            {
                if (RoleClass.DoubralKiller.SuicideKillLText.text != "")
                {
                    RoleClass.DoubralKiller.SuicideKillLText.text = "";
                }
            }
        }
        public static void MurderPlayer(PlayerControl __instance, PlayerControl target)
        {
            if (__instance.isRole(RoleId.DoubralKiller))
            {
                if (__instance.PlayerId == PlayerControl.LocalPlayer.PlayerId)
                {
                    RoleClass.DoubralKiller.SuicideDefaultLTime = RoleClass.DoubralKiller.SuicideDefaultLTime;
                    RoleClass.DoubralKiller.SuicideDefaultRTime = RoleClass.DoubralKiller.SuicideDefaultRTime;
                    RoleClass.DoubralKiller.IsSuicideViewL = true;
                    RoleClass.DoubralKiller.IsSuicideViewR = true;
                }
                RoleClass.DoubralKiller.IsSuicideViewsL[__instance.PlayerId] = true;
                RoleClass.DoubralKiller.IsSuicideViewsR[__instance.PlayerId] = true;
                if (ModeHandler.isMode(ModeId.SuperHostRoles))
                {
                    RoleClass.DoubralKiller.SuicideTimersL[__instance.PlayerId] = RoleClass.DoubralKiller.SuicideDefaultLTime;
                    RoleClass.DoubralKiller.SuicideTimersR[__instance.PlayerId] = RoleClass.DoubralKiller.SuicideDefaultRTime;
                }
                else if (ModeHandler.isMode(ModeId.Default))
                {
                    if (__instance.PlayerId == PlayerControl.LocalPlayer.PlayerId)
                    {
                        __instance.SetKillTimerUnchecked(RoleClass.DoubralKiller.KillTime);
                        RoleClass.DoubralKiller.SuicideLTime = RoleClass.DoubralKiller.SuicideDefaultLTime;
                        RoleClass.DoubralKiller.SuicideRTime = RoleClass.DoubralKiller.SuicideDefaultRTime;
                    }
                }
            }
        }
        public static void WrapUp()
        {
            if (RoleClass.DoubralKiller.IsMeetingReset)
            {
                RoleClass.DoubralKiller.SuicideLTime = RoleClass.DoubralKiller.SuicideDefaultLTime;
                RoleClass.DoubralKiller.SuicideRTime = RoleClass.DoubralKiller.SuicideDefaultRTime;
            }
        }
        public class DoubralKillerFixedPatch
        {
            public static PlayerControl DoubralKillersetTarget(bool onlyCrewmates = false, bool targetPlayersInVents = false, List<PlayerControl> untargetablePlayers = null, PlayerControl targetingPlayer = null)
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
                    if (!playerInfo.Disconnected && playerInfo.PlayerId != targetingPlayer.PlayerId && playerInfo.Object.isAlive())
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
        }
        public class Nokill
        {
            public static void Postfix()
            {
                SetDoubralKillerButton();
            }
        }
    }
}
