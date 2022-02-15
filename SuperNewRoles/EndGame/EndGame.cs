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
using SuperNewRoles.Mode;

namespace SuperNewRoles.EndGame
{
    enum WinCondition
    {
        Default,
        JesterWin,
        QuarreledWin,
        GodWin
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
            var text = "";
            if (AdditionalTempData.winCondition == WinCondition.GodWin) {
                text = "GodName";
                textRenderer.color = RoleClass.God.color;
                __instance.BackgroundBar.material.SetColor("_Color", Roles.RoleClass.God.color);
            }
            else if (AdditionalTempData.winCondition == WinCondition.JesterWin) {
                //bonusText = "jesterWin";
                text = "JesterName";
                textRenderer.color = Roles.RoleClass.Jester.color;
                __instance.BackgroundBar.material.SetColor("_Color", Roles.RoleClass.Jester.color);
            } else if (AdditionalTempData.winCondition == WinCondition.QuarreledWin)
            {
                text = "QuarreledName";
                textRenderer.color = Roles.RoleClass.Quarreled.color;
                __instance.BackgroundBar.material.SetColor("_Color", Roles.RoleClass.Quarreled.color);
            } else if (AdditionalTempData.gameOverReason == GameOverReason.HumansByTask || AdditionalTempData.gameOverReason == GameOverReason.HumansByVote)
            {
                text = "CrewMateName";
                textRenderer.color = Palette.White;
            } else if (AdditionalTempData.gameOverReason == GameOverReason.ImpostorByKill || AdditionalTempData.gameOverReason == GameOverReason.ImpostorBySabotage || AdditionalTempData.gameOverReason == GameOverReason.ImpostorByVote)
            {
                text = "ImpostorName";
                textRenderer.color = RoleClass.ImpostorRed;
            }

            if (ModeHandler.isMode(ModeId.BattleRoyal)) {
                SuperNewRolesPlugin.Logger.LogInfo("BATTLEROYAL!!!!");
                foreach (PlayerControl p in PlayerControl.AllPlayerControls) {
                    if (p.isAlive())
                    {
                        text = p.nameText.text;
                        textRenderer.color = new Color32(116, 80, 48, byte.MaxValue);
                    }
                }
            }

            text = ModTranslation.getString(text);
            bool IsOpptexton = false;
            foreach (PlayerControl player in RoleClass.Opportunist.OpportunistPlayer) {
                if (player.isAlive()) { 
                    if (!IsOpptexton)
                    {
                        text = text + "&"+ModTranslation.getString("OpportunistName");
                    }

                }
            }
            textRenderer.text = string.Format(text+" "+ModTranslation.getString("WinName"));
            AdditionalTempData.clear();
        }
    }
    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameEnd))]
    public class OnGameEndPatch
    {

        public static PlayerControl WinnerPlayer;
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
           
            notWinners.AddRange(RoleClass.Jester.JesterPlayer);
            notWinners.AddRange(RoleClass.MadMate.MadMatePlayer);
            notWinners.AddRange(RoleClass.God.GodPlayer);
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
                WinnerPlayer.Data.IsDead = false;
                WinningPlayerData wpd = new WinningPlayerData(WinnerPlayer.Data);
                TempData.winners.Add(wpd);
                AdditionalTempData.winCondition = WinCondition.JesterWin;
            } else if (QuarreledWin)
            {
                TempData.winners = new Il2CppSystem.Collections.Generic.List<WinningPlayerData>();
                var winplays = new List<PlayerControl>() { WinnerPlayer };
                winplays.Add(WinnerPlayer.GetOneSideQuarreled());
                foreach (PlayerControl p in winplays)
                {
                    p.Data.IsDead = false;
                    WinningPlayerData wpd = new WinningPlayerData(p.Data);
                    TempData.winners.Add(wpd);
                }
                AdditionalTempData.winCondition = WinCondition.QuarreledWin;
            }
            foreach (PlayerControl player in RoleClass.Opportunist.OpportunistPlayer)
            {
                if (player.isAlive())
                {
                    TempData.winners.Add(new WinningPlayerData(player.Data));
                }
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

            if (ModeHandler.isMode(ModeId.BattleRoyal)) {
                TempData.winners = new Il2CppSystem.Collections.Generic.List<WinningPlayerData>();
                foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                {
                    if (p.isAlive())
                    {
                        WinningPlayerData wpd = new WinningPlayerData(p.Data);
                        TempData.winners.Add(wpd);
                    }
                }
                AdditionalTempData.winCondition = WinCondition.Default;
            }
            var godalive = false;
            foreach (PlayerControl p in RoleClass.God.GodPlayer) { 
                if (p.isAlive())
                {
                    godalive = true;
                    TempData.winners = new Il2CppSystem.Collections.Generic.List<WinningPlayerData>();
                    WinningPlayerData wpd = new WinningPlayerData(p.Data);
                    TempData.winners.Add(wpd);
                    AdditionalTempData.winCondition = WinCondition.GodWin;
                }
            }

        }
        [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.CheckEndCriteria))]
        class CheckEndCriteriaPatch
        {
            public static bool Prefix(ShipStatus __instance)
            {
                __instance.enabled = true;
                try
                {
                    if (!GameData.Instance) return false;
                    if (DestroyableSingleton<TutorialManager>.InstanceExists) return true;
                    if (HudManager.Instance.isIntroDisplayed) return false;

                    var playerdates = new PlayerStatistics(__instance);
                    if (!ModeHandler.isMode(ModeId.Default))
                    {
                        ModeHandler.EndGameChecks(__instance,playerdates);
                        return false;
                    }
                    else {
                        QuarreledWinCheck(__instance);
                        JesterWinCheck(__instance);
                        ImpostorWinCheck(__instance, playerdates);
                        CrewmateWinCheck(__instance, playerdates);
                        return false;
                    }
                }
                catch {
                }
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

            private static bool CrewmateWinCheck(ShipStatus __instance, PlayerStatistics statistics)
            {
                if (statistics.TeamCrew > 0 && statistics.TeamImpostorsAlive == 0)
                {
                    __instance.enabled = false;
                    ShipStatus.RpcEndGame(GameOverReason.HumansByVote, false);
                    return true;
                }
                return false;
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
        public static void Prefix(ExileController __instance)
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
        public static void Prefix(AirshipExileController __instance)
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

                    MessageWriter Writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.ShareWinner, Hazel.SendOption.Reliable, -1);
                    Writer.Write(Player.PlayerId);
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
        public int TeamCrewAlive { get; set; }
        public int NeutralAlive { get; set; }
        public int TotalAlive { get; set; }

        public PlayerStatistics(ShipStatus __instance)
        {
            GetPlayerCounts();
        }

        

        private void GetPlayerCounts()
        {
            int numImpostorsAlive = 0;
            int numTotalAlive = 0;
            int numNeutralAlive = 0;
            int numCrew = 0;
            int numCrewAlive = 0;

            for (int i = 0; i < GameData.Instance.PlayerCount; i++)
            {
                GameData.PlayerInfo playerInfo = GameData.Instance.AllPlayers[i];
                if (!playerInfo.Disconnected)
                {
                    if (playerInfo.Object.isCrew()) { numCrew++; if (!playerInfo.IsDead) numCrewAlive++; }
                    numTotalAlive++;

                        

                    if (playerInfo.Role.IsImpostor)
                    {
                         numImpostorsAlive++;
                    }
                        

                    if (playerInfo.Object.isNeutral()) numNeutralAlive++;
                }
            }

            TeamCrew = numCrew;
            TeamCrewAlive = numCrewAlive;
            TeamImpostorsAlive = numImpostorsAlive;
            NeutralAlive = numNeutralAlive;
            TotalAlive = numTotalAlive;
        }
    }
}
