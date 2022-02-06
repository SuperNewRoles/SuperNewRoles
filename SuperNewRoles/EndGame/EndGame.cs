using HarmonyLib;
using Hazel;
using System.Collections.Generic;
using System.Linq;
using UnhollowerBaseLib;
using System.Collections;
using System;
using System.Text;
using UnityEngine;
using System.Reflection;
using SuperNewRoles.Roles;

namespace SuperNewRoles.EndGame
{
    enum WinCondition
    {
        Default,
        JesterWin,
        QuarreledWin
    }
    static class AdditionalTempData
    {
        // Should be implemented using a proper GameOverReason in the future
        public static List<PlayerRoleInfo> playerRoles = new List<PlayerRoleInfo>();
        public static GameOverReason gameOverReason;
        public static WinCondition winCondition = WinCondition.Default;
        public static List<WinCondition> additionalWinConditions = new List<WinCondition>();

        public static Dictionary<int, PlayerControl> plagueDoctorInfected = new Dictionary<int, PlayerControl>();
        public static Dictionary<int, float> plagueDoctorProgress = new Dictionary<int, float>();

        public static void clear()
        {
            playerRoles.Clear();
            additionalWinConditions.Clear();
            winCondition = WinCondition.Default;
        }

        internal class PlayerRoleInfo
        {
            public string PlayerName { get; set; }
            public string NameSuffix { get; set; }
            public List<Intro.IntroDate> Roles { get; set; }
            public string RoleString { get; set; }
            public int TasksCompleted { get; set; }
            public int TasksTotal { get; set; }
            public int PlayerId { get; set; }
        }
    }
    [HarmonyPatch(typeof(EndGameManager), nameof(EndGameManager.SetEverythingUp))]
    public class EndGameManagerSetUpPatch
    {
        public static TMPro.TMP_Text textRenderer;
        public static void Postfix(EndGameManager __instance)
        {
            GameObject bonusTextObject = UnityEngine.Object.Instantiate(__instance.WinText.gameObject);
            bonusTextObject.transform.position = new Vector3(__instance.WinText.transform.position.x, __instance.WinText.transform.position.y - 0.8f, __instance.WinText.transform.position.z);
            bonusTextObject.transform.localScale = new Vector3(0.7f, 0.7f, 1f);
            textRenderer = bonusTextObject.GetComponent<TMPro.TMP_Text>();
            textRenderer.text = "";
            var text = "{0}";
            if (AdditionalTempData.winCondition == WinCondition.JesterWin) {
                //bonusText = "jesterWin";
                text = "JesterWinText";
                textRenderer.color = Roles.RoleClass.Jester.color;
                __instance.BackgroundBar.material.SetColor("_Color", Roles.RoleClass.Jester.color);
            } else if (AdditionalTempData.winCondition == WinCondition.QuarreledWin)
            {
                text = "QuarreledWinText";
                textRenderer.color = Roles.RoleClass.Quarreled.color;
                __instance.BackgroundBar.material.SetColor("_Color", Roles.RoleClass.Quarreled.color);
            }
            textRenderer.text = string.Format(ModTranslation.getString(text),"");
            AdditionalTempData.clear();
        }
    }
    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameEnd))]
    public class OnGameEndPatch
    {

        public static new List<PlayerControl> WinnerPlayer;
        public static void Prefix(AmongUsClient __instance, [HarmonyArgument(0)] ref EndGameResult endGameResult)
        {

            AdditionalTempData.gameOverReason = endGameResult.GameOverReason;
            if ((int)endGameResult.GameOverReason >= 10) endGameResult.GameOverReason = GameOverReason.ImpostorByKill;
        }

        public static void Postfix(AmongUsClient __instance, [HarmonyArgument(0)] ref EndGameResult endGameResult)
        {
            var gameOverReason = AdditionalTempData.gameOverReason;
            AdditionalTempData.clear();

            // Remove Jester, Arsonist, Vulture, Jackal, former Jackals and Sidekick from winners (if they win, they'll be readded)
            List<PlayerControl> notWinners = new List<PlayerControl>();
           
            notWinners.AddRange(Roles.RoleClass.Jester.JesterPlayer);
            notWinners.AddRange(Roles.RoleClass.MadMate.MadMatePlayer);
            foreach (List<PlayerControl> players in RoleClass.Quarreled.QuarreledPlayer)
            {
                notWinners.AddRange(players);
            }

            List<WinningPlayerData> winnersToRemove = new List<WinningPlayerData>();
            foreach (WinningPlayerData winner in TempData.winners)
            {
                if (notWinners.Any(x => x.Data.PlayerName == winner.PlayerName)) winnersToRemove.Add(winner);
            }
            foreach (var winner in winnersToRemove) TempData.winners.Remove(winner);
            // Neutral shifter can't win

            bool saboWin = gameOverReason == GameOverReason.ImpostorBySabotage;


            bool JesterWin = gameOverReason == (GameOverReason)CustomGameOverReason.JesterWin;
            bool QuarreledWin = gameOverReason == (GameOverReason)CustomGameOverReason.QuarreledWin;

            // Jester win
            if (JesterWin)
            {
                TempData.winners = new Il2CppSystem.Collections.Generic.List<WinningPlayerData>();
                WinnerPlayer[0].Data.IsDead = true;
                WinningPlayerData wpd = new WinningPlayerData(WinnerPlayer[0].Data);
                TempData.winners.Add(wpd);
                AdditionalTempData.winCondition = WinCondition.JesterWin;
            } else if (QuarreledWin)
            {
                TempData.winners = new Il2CppSystem.Collections.Generic.List<WinningPlayerData>();
                foreach (PlayerControl p in WinnerPlayer)
                {
                    WinningPlayerData wpd = new WinningPlayerData(p.Data);
                    TempData.winners.Add(wpd);
                }
                AdditionalTempData.winCondition = WinCondition.QuarreledWin;
            }
            if (TempData.winners.ToArray().Any(x => x.IsImpostor))
            {
                foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                {
                    if (Roles.RoleClass.MadMate.MadMatePlayer.IsCheckListPlayerControl(p))
                    { 
                        WinningPlayerData wpd = new WinningPlayerData(p.Data);
                        TempData.winners.Add(wpd);
                    }
                }
            }
        }
        [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.CheckEndCriteria))]
        class CheckEndCriteriaPatch
        {
            public static bool Prefix(ShipStatus __instance)
            {
                if (DestroyableSingleton<TutorialManager>.InstanceExists) return true;
                var playerdates = new PlayerStatistics(__instance);
                QuarreledWinCheck(__instance);
                JesterWinCheck(__instance);
                ImpostorWinCheck(__instance,playerdates);
                return false;
            }
            public static bool ImpostorWinCheck(ShipStatus __instance, PlayerStatistics statistics)
            {
                if (statistics.TeamImpostorsAlive >= statistics.TotalAlive - statistics.TeamImpostorsAlive)
                {
                    __instance.enabled = false;
                    GameOverReason endReason;
                    switch (TempData.LastDeathReason)
                    {
                        case DeathReason.Exile:
                            endReason = GameOverReason.ImpostorByVote;
                            break;
                        case DeathReason.Kill:
                            endReason = GameOverReason.ImpostorByKill;
                            break;
                        default:
                            endReason = GameOverReason.ImpostorByVote;
                            break;
                    }
                    ShipStatus.RpcEndGame(endReason, false);
                    return true;
                }
                return false;
            }
            static void JesterWinCheck(ShipStatus __instance)
            {
                if (Roles.RoleClass.Jester.IsJesterWin)
                {
                    
                    __instance.enabled = false;
                }
            }
            static void QuarreledWinCheck(ShipStatus __instance)
            {
                if (Roles.RoleClass.Quarreled.IsQuarreledWin)
                {

                    __instance.enabled = false;
                }
            }
        }
    }
    [HarmonyPatch(typeof(ExileController), nameof(ExileController.WrapUp))]
    public class CheckEndGamePatch
    {
        public static void Postfix(ExileController __instance)
        {
            try
            {
                WrapUpClass.WrapUpPostfix(__instance.exiled);
            }
            catch (Exception e)
            {
                SuperNewRolesPlugin.Logger.LogInfo("CHECKERROR:" + e);
            }
        }
    }
    [HarmonyPatch(typeof(AirshipExileController), nameof(AirshipExileController.WrapUpAndSpawn))]
    public class CheckAirShipEndGamePatch
    {
        public static void Postfix(AirshipExileController __instance)
        {
            try
            {
                WrapUpClass.WrapUpPostfix(__instance.exiled);
            }
            catch (Exception e)
            {
                SuperNewRolesPlugin.Logger.LogInfo("CHECKERROR:"+e);
            }
        }
    }
    public class WrapUpClass {
        public static void WrapUpPostfix(GameData.PlayerInfo exiled)
        {
            if (exiled.PlayerId == null) return;
            var Player = ModHelpers.playerById(exiled.PlayerId);
            if (RoleHelpers.IsQuarreled(Player))
            {
                var Side = RoleHelpers.GetOneSideQuarreled(Player);
                if (Side.isDead())
                {
                    CustomRPC.RPCProcedure.ShareWinner(Player.PlayerId);
                    CustomRPC.RPCProcedure.ShareWinner(Side.PlayerId);

                    MessageWriter Writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.ShareWinner, Hazel.SendOption.Reliable, -1);
                    Writer.Write(Player.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(Writer);
                    Writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.ShareWinner, Hazel.SendOption.Reliable, -1);
                    Writer.Write(Side.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(Writer);
                    Roles.RoleClass.Quarreled.IsQuarreledWin = true;
                    ShipStatus.RpcEndGame((GameOverReason)CustomGameOverReason.QuarreledWin, false);
                }
            }
            if (Roles.RoleClass.Jester.JesterPlayer.IsCheckListPlayerControl(Player))
            {
                
                if (!Roles.RoleClass.Jester.IsJesterTaskClearWin || (Roles.RoleClass.Jester.IsJesterTaskClearWin && Patch.TaskCount.TaskDateNoClearCheck(Player.Data).Item2 - Patch.TaskCount.TaskDateNoClearCheck(Player.Data).Item1 == 0))
                {
                    CustomRPC.RPCProcedure.ShareWinner(Player.PlayerId);

                    MessageWriter Writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.ShareWinner, Hazel.SendOption.Reliable, -1);
                    Writer.Write(Player.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(Writer);
                    Roles.RoleClass.Jester.IsJesterWin = true;
                    ShipStatus.RpcEndGame((GameOverReason)CustomGameOverReason.JesterWin, false);
                }
            }
        }
    }
    internal class PlayerStatistics
    {
        public int TeamImpostorsAlive { get; set; }
        public int TeamCrew { get; set; }
        public int NeutralAlive { get; set; }
        public int TotalAlive { get; set; }

        public PlayerStatistics(ShipStatus __instance)
        {
            GetPlayerCounts();
        }

        

        private void GetPlayerCounts()
        {
            int numJackalAlive = 0;
            int numImpostorsAlive = 0;
            int numTotalAlive = 0;
            int numNeutralAlive = 0;
            int numCrew = 0;

            int numLoversAlive = 0;
            int numCouplesAlive = 0;
            int impLovers = 0;

            for (int i = 0; i < GameData.Instance.PlayerCount; i++)
            {
                GameData.PlayerInfo playerInfo = GameData.Instance.AllPlayers[i];
                if (!playerInfo.Disconnected)
                {
                    if (playerInfo.Object.isCrew()) { numCrew++; }
                    numTotalAlive++;

                        

                    if (playerInfo.Role.IsImpostor)
                    {
                         numImpostorsAlive++;
                    }
                        

                    if (playerInfo.Object.isNeutral()) numNeutralAlive++;
                }
            }

            TeamCrew = numCrew;
            TeamImpostorsAlive = numImpostorsAlive;
            NeutralAlive = numNeutralAlive;
            TotalAlive = numTotalAlive;
        }
    }
}
