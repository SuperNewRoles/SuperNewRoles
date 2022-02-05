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

namespace SuperNewRoles.EndGame
{
    enum WinCondition
    {
        Default,
        JesterWin,
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
            if (AdditionalTempData.winCondition == WinCondition.JesterWin) {
                //bonusText = "jesterWin";
                text = "JesterWinText";
                textRenderer.color = Roles.RoleClass.Jester.color;
                __instance.BackgroundBar.material.SetColor("_Color", Roles.RoleClass.Jester.color);
            }
            textRenderer.text = ModTranslation.getString(text);
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
            if (Roles.RoleClass.Jester.JesterPlayer != new List<PlayerControl>())
            {
                foreach (PlayerControl player in Roles.RoleClass.Jester.JesterPlayer)
                {
                    notWinners.Add(player);
                }
            }

            // Neutral shifter can't win


            List<WinningPlayerData> winnersToRemove = new List<WinningPlayerData>();
            foreach (WinningPlayerData winner in TempData.winners)
            {
                if (notWinners.Any(x => x.Data.PlayerName == winner.PlayerName)) winnersToRemove.Add(winner);
            }
            foreach (var winner in winnersToRemove) TempData.winners.Remove(winner);

            bool saboWin = gameOverReason == GameOverReason.ImpostorBySabotage;

            bool jesterWin = gameOverReason == (GameOverReason)CustomGameOverReason.JesterWin;

            // Jester win
            if (jesterWin)
            {
                TempData.winners = new Il2CppSystem.Collections.Generic.List<WinningPlayerData>();
                WinnerPlayer.Data.IsDead = true;
                WinningPlayerData wpd = new WinningPlayerData(WinnerPlayer.Data);
                TempData.winners.Add(wpd);
                AdditionalTempData.winCondition = WinCondition.JesterWin;
            }
        }
        [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.CheckEndCriteria))]
        class CheckEndCriteriaPatch
        {
            public static bool Prefix(ShipStatus __instance)
            {
                JesterWinCheck(__instance);
                return false;
            }
            static void JesterWinCheck(ShipStatus __instance)
            {
                if (Roles.RoleClass.Jester.IsJesterWin)
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
            WrapUpClass.WrapUpPostfix(__instance.exiled);
        }
    }
    [HarmonyPatch(typeof(AirshipExileController), nameof(AirshipExileController.WrapUpAndSpawn))]
    public class CheckAirShipEndGamePatch
    {
        public static void Postfix(AirshipExileController __instance)
        {
            WrapUpClass.WrapUpPostfix(__instance.exiled);
        }
    }
    public class WrapUpClass {
        public static void WrapUpPostfix(GameData.PlayerInfo exiled)
        {
            if (exiled.PlayerId == null) return;
            var Player = ModHelpers.playerById(exiled.PlayerId);
            if (Roles.RoleClass.Jester.JesterPlayer.IsCheckListPlayerControl(Player))
            {
                SuperNewRolesPlugin.Logger.LogInfo("---Jester Check!---");
                SuperNewRolesPlugin.Logger.LogInfo(!Roles.RoleClass.Jester.IsJesterTaskClearWin);
                SuperNewRolesPlugin.Logger.LogInfo((Roles.RoleClass.Jester.IsJesterTaskClearWin && Patch.TaskCount.TaskDateNoClearCheck(Player.Data).Item2 - Patch.TaskCount.TaskDateNoClearCheck(Player.Data).Item1 == 0));
                SuperNewRolesPlugin.Logger.LogInfo(Patch.TaskCount.TaskDateNoClearCheck(Player.Data).Item2);
                SuperNewRolesPlugin.Logger.LogInfo(Patch.TaskCount.TaskDateNoClearCheck(Player.Data).Item1);
                SuperNewRolesPlugin.Logger.LogInfo(Patch.TaskCount.TaskDateNoClearCheck(Player.Data).Item2 - Patch.TaskCount.TaskDateNoClearCheck(Player.Data).Item1 == 0);
                SuperNewRolesPlugin.Logger.LogInfo("-------------------");
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
}
