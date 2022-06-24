using System;
using System.Collections.Generic;
using System.Linq;
using Hazel;
using SuperNewRoles.Buttons;
using SuperNewRoles.CustomRPC;
using UnityEngine;

namespace SuperNewRoles.Roles
{
    public static class DoubleKiller
    {
        public static void SetDoubleKillerButton()
        {
            if (PlayerControl.LocalPlayer.isRole(CustomRPC.RoleId.DoubleKiller))
            {
                HudManager.Instance.KillButton.gameObject.SetActive(false);
                //純正キルボタン消去
            }
        }
        public static void resetMainCoolDown()
        {
            HudManagerStartPatch.DoubleKillerMainKillButton.MaxTimer = RoleClass.DoubleKiller.MainKillCoolTime;
            HudManagerStartPatch.DoubleKillerMainKillButton.Timer = RoleClass.DoubleKiller.MainKillCoolTime;
        }
        public static void resetSubCoolDown()
        {
            HudManagerStartPatch.DoubleKillerSubKillButton.MaxTimer = RoleClass.DoubleKiller.SubKillCoolTime;
            HudManagerStartPatch.DoubleKillerSubKillButton.Timer = RoleClass.DoubleKiller.SubKillCoolTime;
        }
        public static void EndMeeting()
        {
            resetSubCoolDown();
            resetMainCoolDown();
            HudManagerStartPatch.DoubleKillerSubKillButton.MaxTimer = RoleClass.DoubleKiller.SubKillCoolTime;
        }
        public static void setPlayerOutline(PlayerControl target, Color color)
        {
            if (target == null || target.MyRend == null) return;

            target.MyRend().material.SetFloat("_Outline", 1f);
            target.MyRend().material.SetColor("_OutlineColor", color);
        }
        public class DoubleKillerFixedPatch
        {
            public static PlayerControl DoubleKillersetTarget(bool onlyCrewmates = false, bool targetPlayersInVents = false, List<PlayerControl> untargetablePlayers = null, PlayerControl targetingPlayer = null)
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
                    if (!playerInfo.Disconnected && playerInfo.PlayerId != targetingPlayer.PlayerId && playerInfo.Object.isAlive() && !playerInfo.Object.isDead() && !RoleClass.DoubleKiller.DoubleKillerPlayer.IsCheckListPlayerControl(playerInfo.Object))
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
            static void DoubleKillerPlayerOutLineTarget()
            {
                setPlayerOutline(DoubleKillersetTarget(), RoleClass.DoubleKiller.color);
            }
            public static void Postfix(PlayerControl __instance)
            {
                if (PlayerControl.LocalPlayer.isRole(RoleId.DoubleKiller))
                {
                    DoubleKillerPlayerOutLineTarget();
                }
            }
        }
    }
}
