using UnityEngine;
using System.Collections.Generic;
using SuperNewRoles.CustomRPC;
using SuperNewRoles.Helpers;
using SuperNewRoles.Roles;
using SuperNewRoles.Mode.SuperHostRoles;


namespace SuperNewRoles.Mode.CopsRobbers
{

    public static class CRFixedUpdate
    {
        public static void SetRoleName(PlayerControl player, bool commsActive, bool IsUnchecked = false)
        {
            if (!ModeHandler.IsMode(ModeId.CopsRobbers)) return;
            if (player.IsBot() || !AmongUsClient.Instance.AmHost) return;

            var caller = new System.Diagnostics.StackFrame(1, false);
            var callerMethod = caller.GetMethod();
            string callerMethodName = callerMethod.Name;
            string callerClassName = callerMethod.DeclaringType.FullName;
            SuperNewRolesPlugin.Logger.LogInfo("[ModeId.CopsRobbers : CRFixedUpdate]" + player.name + "へのSetRoleNameが" + callerClassName + "." + callerMethodName + "から呼び出されました。");

            //必要がないなら処理しない

            string Name = player.GetDefaultName();
            string NewName = "";
            Dictionary<byte, string> ChangePlayers = new();

            foreach (PlayerControl ALLPlayer in PlayerControl.AllPlayerControls)
            {
                if (ALLPlayer == player) continue;
                ChangePlayers.Add(ALLPlayer.PlayerId, ModHelpers.Cs(Color.clear, ALLPlayer.GetDefaultName()));
            }

            if (!player.IsMod())
            {
                player.RpcSetNamePrivate(NewName);
                if (player.IsAlive())
                {
                    foreach (var ChangePlayerData in ChangePlayers)
                    {
                        PlayerControl ChangePlayer = ModHelpers.PlayerById(ChangePlayerData.Key);
                        if (ChangePlayer != null)
                        {
                            ChangePlayer.RpcSetNamePrivate(ChangePlayerData.Value, player);
                        }
                    }
                }
            }
        }
    }
}