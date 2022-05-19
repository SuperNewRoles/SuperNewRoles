using SuperNewRoles.Helpers;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SuperNewRoles.Mode.LevelUp
{
    class main
    {
        public static void ClearAndReloads()
        {
            UpdateTime = 12f;
            Count = 0;
        }
        public static float UpdateTime = 2f;
        public static float Count = 0;
        public static void FixedUpdate()
        {
            UpdateTime -= Time.fixedDeltaTime;
            if (UpdateTime <= 0)
            {
                Count++;
                UpdateTime = 2f;
                foreach(PlayerControl player in PlayerControl.AllPlayerControls)
                {
                    if (!player.Data.Disconnected && player.IsPlayer())
                    {
                        for (int i = 0; i < 100; i++)
                        {
                            player.RpcMurderPlayer(BotManager.AllBots[0]);
                        }
                    }
                }
                if (Count > 5)
                {
                    MeetingRoomManager.Instance.AssignSelf(PlayerControl.LocalPlayer, null);
                    DestroyableSingleton<HudManager>.Instance.OpenMeetingRoom(PlayerControl.LocalPlayer);
                    PlayerControl.LocalPlayer.RpcStartMeeting(null);
                    
                    new LateTask(() =>
                    {
                        MeetingHud.Instance.RpcClose();
                    }, 2f);
                    Count = 0;
                }
            }
        }
    }
}
