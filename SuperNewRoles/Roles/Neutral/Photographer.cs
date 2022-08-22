using System;
using System.Collections.Generic;
using System.Text;
using Hazel;
using SuperNewRoles.CustomRPC;
using SuperNewRoles.EndGame;
using SuperNewRoles.Helpers;
using UnityEngine;

namespace SuperNewRoles.Roles.Neutral
{
    public static class Photographer
    {
        //ここにコードを書きこんでください
        public static void WrapUp()
        {
            RoleClass.Photographer.IsPhotographerShared = false;
        }
        public static List<byte> SetTarget()
        {
            List<byte> players = new();
            foreach (CachedPlayer p in CachedPlayer.AllPlayers)
            {
                if (p.Data.IsDead) continue;
                if (p.PlayerId == CachedPlayer.LocalPlayer.PlayerId) continue;
                if (RoleClass.Photographer.PhotedPlayerIds.Contains(p.PlayerId)) continue;

                float distMod = 1.025f;
                float distance = Vector3.Distance(CachedPlayer.LocalPlayer.transform.position, p.transform.position);
                bool anythingBetween = PhysicsHelpers.AnythingBetween(PlayerControl.LocalPlayer.GetTruePosition(), p.PlayerControl.GetTruePosition(), Constants.ShadowMask, false);

                if (!(distance > ShipStatus.Instance.CalculateLightRadius(CachedPlayer.LocalPlayer.Data) * distMod || anythingBetween)) players.Add(p.PlayerId);
            }
            return players;
        }
        public static void FixedUpdate()
        {
            SetOutlines();
            CheckWin();
        }
        public static void SetOutlines()
        {
            foreach (CachedPlayer p in CachedPlayer.AllPlayers)
            {
                if (p.Data.IsDead) continue;
                if (p.PlayerId == CachedPlayer.LocalPlayer.PlayerId) continue;
                if (RoleClass.Photographer.PhotedPlayerIds.Contains(p.PlayerId)) continue;

                float distMod = 1.025f;
                float distance = Vector3.Distance(CachedPlayer.LocalPlayer.transform.position, p.transform.position);
                bool anythingBetween = PhysicsHelpers.AnythingBetween(PlayerControl.LocalPlayer.GetTruePosition(), p.PlayerControl.GetTruePosition(), Constants.ShadowMask, false);

                if (!(distance > ShipStatus.Instance.CalculateLightRadius(CachedPlayer.LocalPlayer.Data) * distMod || anythingBetween))
                {
                    var rend = p.PlayerControl.MyRend();
                    if (p == null || p.PlayerControl == null || rend == null) continue;
                    rend.material.SetFloat("_Outline", 1f);
                    rend.material.SetColor("_OutlineColor", RoleClass.Photographer.color);
                }
            }
        }
        public static void CheckWin()
        {
            foreach(CachedPlayer player in CachedPlayer.AllPlayers)
            {
                if (player.Data.IsDead) continue;
                if (player.PlayerId == CachedPlayer.LocalPlayer.PlayerId) continue;
                if (RoleClass.Photographer.PhotedPlayerIds.Contains(player.PlayerId)) continue;
                return;
            }

            RPCProcedure.ShareWinner(CachedPlayer.LocalPlayer.PlayerId);
            MessageWriter Writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.ShareWinner, SendOption.Reliable, -1);
            Writer.Write(CachedPlayer.LocalPlayer.PlayerId);
            AmongUsClient.Instance.FinishRpcImmediately(Writer);

            //SuperNewRolesPlugin.Logger.LogInfo("CheckAndEndGame");
            var reason = (GameOverReason)CustomGameOverReason.PhotographerWin;
            if (AmongUsClient.Instance.AmHost)
            {
                CheckGameEndPatch.CustomEndGame(reason, false);
            }
            else
            {
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.CustomEndGame, SendOption.Reliable, -1);
                writer.Write((byte)reason);
                writer.Write(false);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
            }
        }
    }
}