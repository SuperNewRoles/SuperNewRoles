using System.Collections.Generic;
using System.Linq;
using SuperNewRoles.Buttons;
using SuperNewRoles.CustomRPC;
using UnityEngine;

namespace SuperNewRoles.Roles
{
    public static class DoubleKiller
    {
        public static void ResetMainCoolDown()
        {
            HudManagerStartPatch.DoubleKillerMainKillButton.MaxTimer = RoleClass.DoubleKiller.MainCoolTime;
            HudManagerStartPatch.DoubleKillerMainKillButton.Timer = RoleClass.DoubleKiller.MainCoolTime;
        }
        public static void ResetSubCoolDown()
        {
            HudManagerStartPatch.DoubleKillerSubKillButton.MaxTimer = RoleClass.DoubleKiller.SubCoolTime;
            HudManagerStartPatch.DoubleKillerSubKillButton.Timer = RoleClass.DoubleKiller.SubCoolTime;
        }
        public static void EndMeeting()
        {
            ResetSubCoolDown();
            ResetMainCoolDown();
            HudManagerStartPatch.DoubleKillerSubKillButton.MaxTimer = RoleClass.DoubleKiller.SubCoolTime;
        }
        public static void SetPlayerOutline(PlayerControl target, Color color)
        {
            if (target == null || target.MyRend == null) return;

            target.MyRend().material.SetFloat("_Outline", 1f);
            target.MyRend().material.SetColor("_OutlineColor", color);
        }
        public class DoubleKillerFixedPatch
        {
            public static PlayerControl DoubleKillerSetTarget(bool onlyCrewmates = false, bool targetPlayersInVents = false, List<PlayerControl> untargetablePlayers = null, PlayerControl targetingPlayer = null)
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
                    if (!playerInfo.Disconnected && playerInfo.PlayerId != targetingPlayer.PlayerId && playerInfo.Object.IsAlive() && !playerInfo.Object.IsDead() && !playerInfo.Object.IsRole(RoleId.DoubleKiller))
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
                SetPlayerOutline(DoubleKillerSetTarget(), RoleClass.DoubleKiller.color);
            }
            public static void Postfix(PlayerControl __instance)
            {
                if (PlayerControl.LocalPlayer.IsRole(RoleId.DoubleKiller))
                {
                    DoubleKillerPlayerOutLineTarget();
                }
            }
        }
    }
}