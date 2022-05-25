﻿using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace SuperNewRoles.Mode.SuperHostRoles
{
    class Blackoutfix
    {
        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.CheckForEndVoting))]
        public class CheckForEndVotingPatch
        {
            public static void Prefix(MeetingHud __instance)
            {
                if (!AmongUsClient.Instance.AmHost) return;
                if (Mode.ModeHandler.isMode(Mode.ModeId.SuperHostRoles))
                {
                    EndMeetingPatch();
                }
            }
        }
        public static void EndMeetingPatch()
        {
            //BotManager.Spawn("暗転対策");
        }
    }
}
