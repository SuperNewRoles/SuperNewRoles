using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SuperNewRoles.Roles.Neutral
{
    public static class Pavlovsdogs
    {
        //ここにコードを書きこんでください
        public static void OwnerFixedUpdate()
        {
            if (RoleClass.Pavlovsowner.CurrentChildPlayer)
            {
                if (!RoleClass.Pavlovsowner.CurrentChildPlayer.IsRole(RoleId.Pavlovsdogs))
                    RoleClass.Pavlovsowner.CurrentChildPlayer = null;
                else
                    RoleClass.Pavlovsowner.DogArrow.Update(RoleClass.Pavlovsowner.CurrentChildPlayer.transform.position);
            }
            RoleClass.Pavlovsowner.DogArrow.arrow.gameObject.SetActive(RoleClass.Pavlovsowner.CurrentChildPlayer);
        }
        public static PlayerControl SetTarget(bool IsPavlovsTeamTarget = false)
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
                if (!playerInfo.Disconnected && playerInfo.PlayerId != targetingPlayer.PlayerId && !playerInfo.IsDead && ((IsPavlovsTeamTarget && playerInfo.Object.IsPavlovsTeam()) || (!IsPavlovsTeamTarget && !playerInfo.Object.IsPavlovsTeam())))
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