﻿using SuperNewRoles.CustomOption;
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
            MurderCount = (int)LevelUpMurder.getFloat();
        }
        public static float UpdateTime = 2f;
        public static float Count = 0;
        public static int MurderCount = 0;
        public static CustomOption.CustomOption LevelUpMurder;
        public static void Load()
        {
            LevelUpMurder = CustomOption.CustomOption.Create(426, false, CustomOptionType.Generic, "レベルアップモード:1秒あたりのキル回数", 25, 5,100,5, ModeHandler.ModeSetting);
        }
        public static void FixedUpdate()
        {
            if (!AmongUsClient.Instance.AmHost) return;
            UpdateTime -= Time.fixedDeltaTime;
            if (UpdateTime <= 0)
            {
                Count++;
                UpdateTime = 1f;
                foreach(PlayerControl player in PlayerControl.AllPlayerControls)
                {
                    if (!player.Data.Disconnected && player.IsPlayer())
                    {
                        for (int i = 0; i < MurderCount; i++)
                        {
                            player.RPCMurderPlayerPrivate(BotManager.AllBots[0]);
                        }
                    }
                }
                if (Count > 5)
                {
                    new LateTask(() =>
                    {
                        if (MeetingHud.Instance && AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Started)
                        {
                            MeetingHud.Instance.RpcClose();
                        }
                    }, 2f);
                    Count = 0;
                }
            }
        }
    }
}
