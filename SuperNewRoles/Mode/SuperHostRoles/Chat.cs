
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using SuperNewRoles.EndGame;
using UnityEngine;

namespace SuperNewRoles.Mode.SuperHostRoles
{
    class Chat
    {
        public static bool IsOldSHR = false;
        public static CustomGameOverReason? WinCond = null;
        public static List<PlayerControl> Winner = null;
        [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Start))]
        public class GameStartManagerStartPatch
        {
            public static void Postfix()
            {
                if (!AmongUsClient.Instance.AmHost) return;
                if (IsOldSHR && WinCond != null && AmongUsClient.Instance.AmHost)
                {
                    var Template = "最終結果\n勝利陣営:{0}";
                    if (WinCond == CustomGameOverReason.GodWin)
                    {
                        var players = "";
                        try
                        {
                            foreach (PlayerControl p in Winner)
                            {
                                if (players == "")
                                {
                                    players += p.nameText().text;
                                }
                                else
                                {
                                    players += "," + p.nameText().text;
                                }
                            }
                        }
                        catch { }
                        //new LateTask(() => {
                        PlayerControl.LocalPlayer.RpcSendChat(string.Format(Template + "\n勝者:{1}", "神(God)", players));
                        //}, 3f, "SendResult");
                    }
                    else if (WinCond == CustomGameOverReason.CrewmateWin)
                    {
                        //new LateTask(() => {
                        PlayerControl.LocalPlayer.RpcSendChat(string.Format(Template, "クルーメイト(Crewmate)"));
                        //}, 3f, "SendResult");
                    }
                    else if (WinCond == CustomGameOverReason.ImpostorWin)
                    {
                        //new LateTask(() => {
                        PlayerControl.LocalPlayer.RpcSendChat(string.Format(Template, "インポスター(Impostor)"));
                        //}, 3f, "SendResult");
                    }
                    else if (WinCond == CustomGameOverReason.JesterWin && Winner != null)
                    {
                        //new LateTask(() => {
                        PlayerControl.LocalPlayer.RpcSendChat(string.Format(Template + "\n勝者:{1}", "てるてる(Jester)", Winner[0].nameText().text));
                        // }, 3f, "SendResult");
                    }
                    else if (WinCond == CustomGameOverReason.WorkpersonWin && Winner != null)
                    {
                        // new LateTask(() => {
                        PlayerControl.LocalPlayer.RpcSendChat(string.Format(Template + "\n勝者:{1}", "仕事人(Workperson)", Winner[0].nameText().text));
                        //}, 3f, "SendResult");
                    }
                }
                IsOldSHR = false;
                WinCond = null;
                Winner = null;
            }
        }
    }
}
