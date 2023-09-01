using System;
using System.Collections.Generic;
using System.Text;
using SuperNewRoles.Helpers;
using SuperNewRoles.Mode.BattleRoyal;
using SuperNewRoles.Patches;
using UnityEngine;

namespace SuperNewRoles.SuperNewRolesWeb
{
    public static class BattleRoyalWebManager
    {
        static Dictionary<byte, string> target;
        public static void OnStartMeeting() {
            foreach (var data in target) {
                PlayerControl player = ModHelpers.PlayerById(data.Key);
                if (player == null) continue;
                if (data.Value == "0") continue;
                AddChatPatch.SendCommand(player, string.Format(ModTranslation.GetString("SNRWebBRMultiRate"), data.Value),
                    ModHelpers.Cs(new Color32(116, 80, 48, byte.MaxValue), ModTranslation.GetString("BattleRoyalModeName")+"Web"));
            }
        }
        public static void StartGame() {
            if (!WebAccountManager.IsLogined) return;
            target = new();
            Dictionary<string, string> data = new();
            string fc = "";
            foreach (PlayerControl p in PlayerControl.AllPlayerControls) {
                if (p.IsBot()) continue;
                fc += p.Data.FriendCode + ",";
            }
            fc = fc[..(fc.Length - 1)];
            data.Add("friendcode", fc);
            WebApi.BRStartgame(data, (code, handler) => {
                if (code == 200) {
                    string result = handler.text;
                    Logger.Info(result);
                    foreach (string data in result.Split(",")) {
                        string[] sp = data.Split(":");
                        string targetfc = sp[0];
                        string targetmr = sp[1];
                        PlayerControl targetp = PlayerControl.AllPlayerControls.Find((Il2CppSystem.Predicate<PlayerControl>)(x => x.FriendCode == targetfc));
                        if (targetp != null) {
                            target.Add(targetp.PlayerId, targetmr);
                        }
                    }
                }
            });
        }
        public static void EndGame()
        {
            if (!WebAccountManager.IsLogined) return;
            Dictionary<string, string> data = new();
            string fc = "";
            foreach (PlayerControl p in PlayerControl.AllPlayerControls)
            {
                if (p.IsBot()) continue;
                fc += p.Data.FriendCode + ",";
                data.Add(p.Data.FriendCode+"_kc", Main.KillCount.ContainsKey(p.PlayerId) ? Mode.BattleRoyal.Main.KillCount[p.PlayerId].ToString() : "0");
                data.Add(p.Data.FriendCode + "_IsWin", TempData.winners.Find((Il2CppSystem.Predicate<WinningPlayerData>)(x => x.PlayerName == p.Data.DefaultOutfit.PlayerName)) is null ? "b" : "a");
            }
            fc = fc[..(fc.Length - 1)];
            data.Add("friendcode", fc);
            if (BROption.IsTeamBattle.GetBool()) {
                StringBuilder result = new();
                string[] teams = new string[BattleTeam.BattleTeams.Count];
                int teamindex = 0;
                foreach (BattleTeam team in BattleTeam.BattleTeams) {
                    string[] fcs = new string[team.TeamMember.Count];
                    int index = 0;
                    foreach (PlayerControl member in team.TeamMember)
                    {
                        fcs[index] = member.Data.FriendCode;
                        index++;
                    }
                    result.AppendJoin(",", fcs);
                    teams[teamindex] = result.ToString();
                    result.Clear();
                    teamindex++;
                }
                data.Add("teams", string.Join("/", teams));
            }
            WebApi.BREndgame(data, (code, handler) => {
            });
        }
    }
}
