using System;
using System.Collections.Generic;
using System.Text;
using SuperNewRoles.Patch;
using UnityEngine;

namespace SuperNewRoles.Roles.Neutral
{
    public static class Pavlovsdogs
    {
        //ここにコードを書きこんでください
        public static PlayerControl SetTarget()
        {
            PlayerControl result = null;
            float num = GameOptionsData.KillDistances[Mathf.Clamp(PlayerControl.GameOptions.KillDistance, 0, 2)];
            if (!MapUtilities.CachedShipStatus) return result;
            PlayerControl targetingPlayer = PlayerControl.LocalPlayer;
            if (targetingPlayer.Data.IsDead || targetingPlayer.inVent) return result;

            Vector2 truePosition = targetingPlayer.GetTruePosition();
            Il2CppSystem.Collections.Generic.List<GameData.PlayerInfo> allPlayers = GameData.Instance.AllPlayers;
            for (int i = 0; i < allPlayers.Count; i++)
            {
                GameData.PlayerInfo playerInfo = allPlayers[i];
                if (!playerInfo.Disconnected && playerInfo.PlayerId != targetingPlayer.PlayerId && !playerInfo.IsDead && playerInfo.Object.IsPavlovsTeam())
                {
                    PlayerControl @object = playerInfo.Object;
                    if (@object && !@object.inVent)
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
            return result == null ? null : result.IsDead() ? null : result;
        }
        public static void SetNameUpdate()
        {
            if (PlayerControl.LocalPlayer.IsPavlovsTeam())
            {
                foreach (PlayerControl p in RoleClass.Pavlovsdogs.PavlovsdogsPlayer)
                {
                    SetNamesClass.SetPlayerRoleInfo(p);
                }
                foreach (PlayerControl p in RoleClass.Pavlovsowner.PavlovsownerPlayer)
                {
                    SetNamesClass.SetPlayerRoleInfo(p);
                }
            }
        }
    }
}