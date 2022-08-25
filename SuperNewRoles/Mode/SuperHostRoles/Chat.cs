using System.Collections.Generic;
using HarmonyLib;
using SuperNewRoles.EndGame;

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
                                if (players == "") players += p.NameText().text;
                                else players += "," + p.NameText().text;
                            }
                        }
                        catch { }
                        PlayerControl.LocalPlayer.RpcSendChat(string.Format(Template + "\n勝者:{1}", "神(God)", players));
                    }
                    else if (WinCond == CustomGameOverReason.CrewmateWin) PlayerControl.LocalPlayer.RpcSendChat(string.Format(Template, "クルーメイト(Crewmate)"));
                    else if (WinCond == CustomGameOverReason.ImpostorWin) PlayerControl.LocalPlayer.RpcSendChat(string.Format(Template, "インポスター(Impostor)"));
                    else if (WinCond == CustomGameOverReason.JesterWin && Winner != null) PlayerControl.LocalPlayer.RpcSendChat(string.Format(Template + "\n勝者:{1}", "てるてる(Jester)", Winner[0].NameText().text));
                    else if (WinCond == CustomGameOverReason.WorkpersonWin && Winner != null) PlayerControl.LocalPlayer.RpcSendChat(string.Format(Template + "\n勝者:{1}", "仕事人(Workperson)", Winner[0].NameText().text));
                }
                IsOldSHR = false;
                WinCond = null;
                Winner = null;
            }
        }
    }
}