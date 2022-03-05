using BepInEx.IL2CPP.Utils;
using HarmonyLib;
using SuperNewRoles.EndGame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
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
                IEnumerator SendResult(string Chat)
                {
                    yield return new WaitForSeconds(3);
                    PlayerControl.LocalPlayer.RpcSendChat(Chat);
                }
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
                                    players += p.nameText.text;
                                }
                                else
                                {
                                    players += "," + p.nameText.text;
                                }
                            }
                        }
                        catch { }
                        AmongUsClient.Instance.StartCoroutine(SendResult(string.Format(Template + "\n勝者:{1}", "神(God)",players)));
                    }
                    else if (WinCond == CustomGameOverReason.CrewmateWin)
                    {
                        AmongUsClient.Instance.StartCoroutine(SendResult(string.Format(Template,"クルーメイト(Crewmate)")));
                    } else if(WinCond == CustomGameOverReason.ImpostorWin)
                    {
                        AmongUsClient.Instance.StartCoroutine(SendResult(string.Format(Template, "インポスター(Impostor)")));
                    } else if(WinCond == CustomGameOverReason.JesterWin && Winner != null)
                    {
                        AmongUsClient.Instance.StartCoroutine(SendResult(string.Format(Template+ "\n勝者:{1}", "てるてる(Jester)",Winner[0].nameText.text)));
                    }
                }
                IsOldSHR = false;
                WinCond = null;
                Winner = null;
            }
        }
    }
}
