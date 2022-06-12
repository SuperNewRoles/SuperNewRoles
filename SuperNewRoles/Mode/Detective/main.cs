using Hazel;
using SuperNewRoles.Helpers;
using SuperNewRoles.Mode.SuperHostRoles;
using SuperNewRoles.Roles;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SuperNewRoles.Mode.Detective
{
    class main
    {
        public static bool IsNotDetectiveWin;
        public static bool IsNotDetectiveVote;
        public static bool IsDetectiveNotTask;
        public static bool IsNotDetectiveMeetingButton;
        public static PlayerControl DetectivePlayer;
        public static Color32 DetectiveColor = new Color32(255, 0, 255, byte.MaxValue);
        public static void ClearAndReload()
        {
            IsNotDetectiveWin = DetectiveOptions.IsWinNotCheckDetective.getBool();
            IsNotDetectiveVote = DetectiveOptions.IsNotDetectiveVote.getBool();
            IsDetectiveNotTask = DetectiveOptions.DetectiveIsNotTask.getBool();
            IsNotDetectiveMeetingButton = DetectiveOptions.IsNotDetectiveMeetingButton.getBool();
        }
        public static void RoleSelect() {
            DetectivePlayer = PlayerControl.LocalPlayer;
            List<PlayerControl> selectplayers = new List<PlayerControl>();
            foreach (PlayerControl p in CachedPlayer.AllPlayers)
            {
                if (p.isCrew())
                {
                    selectplayers.Add(p);
                }
            }
            var random = ModHelpers.GetRandom(selectplayers);
            MessageWriter writer = RPCHelper.StartRPC(CustomRPC.CustomRPC.SetDetective);
            writer.Write(random.PlayerId);
            writer.EndRPC();
            CustomRPC.RPCProcedure.SetDetective(random.PlayerId);
            DetectivePlayer.RpcSetName(ModHelpers.cs(DetectiveColor,DetectivePlayer.getDefaultName()));
            DetectivePlayer.SetName(ModHelpers.cs(DetectiveColor, DetectivePlayer.getDefaultName()));
        }
        public static void MurderPatch(PlayerControl target)
        {
            if (!AmongUsClient.Instance.AmHost) return;
            /*
            if (target.PlayerId != 0)
            {
                foreach (PlayerControl p in CachedPlayer.AllPlayers)
                {
                    if (!p.Data.Disconnected && p.isImpostor())
                    {
                        p.RpcSetNamePrivate(ModHelpers.cs(RoleClass.ImpostorRed, p.getDefaultName()), target);
                    }
                }
            } else
            {
                foreach (PlayerControl p in CachedPlayer.AllPlayers)
                {
                    if (!p.Data.Disconnected && p.isImpostor())
                    {
                        p.SetName(ModHelpers.cs(RoleClass.ImpostorRed, p.getDefaultName()));
                    }
                }
            }
            */
        }
    }
}
