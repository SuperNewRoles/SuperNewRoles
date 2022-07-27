using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using Hazel;
using SuperNewRoles.CustomOption;
using SuperNewRoles.CustomRPC;
using SuperNewRoles.Mode;
using SuperNewRoles.Mode.SuperHostRoles;
using SuperNewRoles.Patch;
using SuperNewRoles.Roles;
using UnhollowerBaseLib;
using UnityEngine;

namespace SuperNewRoles.EndGame
{
    public enum CustomGameOverReason
    {
        CrewmateWin = 7,
        ImpostorWin = 8,
        GodWin = 9,
        HAISON = 10,
        JesterWin,
        JackalWin,
        QuarreledWin,
        EgoistWin,
        WorkpersonWin,
        LoversWin,
        MadJesterWin,
        FalseChargesWin,
        FoxWin,
        DemonWin,
        ArsonistWin,
        VultureWin,
        TunaWin,
        NeetWin,
        RevolutionistWin,
        SpelunkerWin,
        SuicidalIdeationWin,
        BugEnd
    }
    enum WinCondition
    {
        Default,
        HAISON,
        JesterWin,
        JackalWin,
        QuarreledWin,
        GodWin,
        EgoistWin,
        WorkpersonWin,
        LoversWin,
        MadJesterWin,
        FalseChargesWin,
        FoxWin,
        DemonWin,
        ArsonistWin,
        VultureWin,
        TunaWin,
        NeetWin,
        RevolutionistWin,
        SpelunkerWin,
        SuicidalIdeationWin,
        BugEnd
    }
    [HarmonyPatch(typeof(ShipStatus))]
    public class ShipStatusPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.IsGameOverDueToDeath))]
        public static void Postfix2(ShipStatus __instance, ref bool __result)
        {
            __result = false;
        }
    }
    static class AdditionalTempData
    {
        // Should be implemented using a proper GameOverReason in the future
        public static List<PlayerRoleInfo> playerRoles = new();
        public static GameOverReason gameOverReason;
        public static WinCondition winCondition = WinCondition.Default;
        public static List<WinCondition> additionalWinConditions = new();

        public static Dictionary<int, PlayerControl> plagueDoctorInfected = new();
        public static Dictionary<int, float> plagueDoctorProgress = new();

        public static void Clear()
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
            public int ColorId { get; set; }
            public FinalStatus Status { get; internal set; }
            public Intro.IntroDate IntroDate { get; set; }
            public Intro.IntroDate GhostIntroDate { get; set; }
        }
    }
    [HarmonyPatch(typeof(EndGameManager), nameof(EndGameManager.SetEverythingUp))]
    public class EndGameManagerSetUpPatch
    {
        public static bool IsHaison = false;
        public static TMPro.TMP_Text textRenderer;
        [HarmonyPatch(typeof(EndGameNavigation), nameof(EndGameNavigation.ShowProgression))]
        public class ShowProgressionPatch
        {
            public static void Prefix()
            {
                if (textRenderer != null)
                {
                    textRenderer.gameObject.SetActive(false);
                }
            }
        }
        public static void Postfix(EndGameManager __instance)
        {
            foreach (PoolablePlayer pb in __instance.transform.GetComponentsInChildren<PoolablePlayer>())
            {
                UnityEngine.Object.Destroy(pb.gameObject);
            }
            int num = Mathf.CeilToInt(7.5f);
            List<WinningPlayerData> list = TempData.winners.ToArray().ToList().OrderBy(delegate (WinningPlayerData b)
            {
                if (!b.IsYou)
                {
                    return 0;
                }
                return -1;
            }).ToList<WinningPlayerData>();
            for (int i = 0; i < list.Count; i++)
            {
                WinningPlayerData winningPlayerData2 = list[i];
                int num2 = (i % 2 == 0) ? -1 : 1;
                int num3 = (i + 1) / 2;
                float num4 = (float)num3 / (float)num;
                float num5 = Mathf.Lerp(1f, 0.75f, num4);
                float num6 = (float)((i == 0) ? -8 : -1);
                PoolablePlayer poolablePlayer = UnityEngine.Object.Instantiate<PoolablePlayer>(__instance.PlayerPrefab, __instance.transform);
                poolablePlayer.transform.localPosition = new Vector3(1f * (float)num2 * (float)num3 * num5, FloatRange.SpreadToEdges(-1.125f, 0f, num3, num), num6 + (float)num3 * 0.01f) * 0.9f;
                float num7 = Mathf.Lerp(1f, 0.65f, num4) * 0.9f;
                Vector3 vector = new(num7, num7, 1f);
                poolablePlayer.transform.localScale = vector;
                poolablePlayer.UpdateFromPlayerOutfit((GameData.PlayerOutfit)winningPlayerData2, PlayerMaterial.MaskType.ComplexUI, winningPlayerData2.IsDead, true);
                if (winningPlayerData2.IsDead)
                {
                    poolablePlayer.cosmetics.currentBodySprite.BodySprite.sprite = poolablePlayer.cosmetics.currentBodySprite.GhostSprite;
                    poolablePlayer.SetDeadFlipX(i % 2 == 0);
                }
                else
                {
                    poolablePlayer.SetFlipX(i % 2 == 0);
                }

                poolablePlayer.cosmetics.nameText.color = Color.white;
                poolablePlayer.cosmetics.nameText.transform.localScale = new Vector3(1f / vector.x, 1f / vector.y, 1f / vector.z);
                poolablePlayer.cosmetics.nameText.transform.localPosition = new Vector3(poolablePlayer.cosmetics.nameText.transform.localPosition.x, poolablePlayer.cosmetics.nameText.transform.localPosition.y, -15f);
                poolablePlayer.cosmetics.nameText.text = winningPlayerData2.PlayerName;

                foreach (var data in AdditionalTempData.playerRoles)
                {
                    Logger.Info(data.PlayerName + ":" + winningPlayerData2.PlayerName);
                    if (data.PlayerName != winningPlayerData2.PlayerName) continue;
                    poolablePlayer.cosmetics.nameText.text = $"{data.PlayerName}{data.NameSuffix}\n{string.Join("\n", ModHelpers.Cs(data.IntroDate.color, data.IntroDate.Name))}";
                }
            }

            GameObject bonusTextObject = UnityEngine.Object.Instantiate(__instance.WinText.gameObject);
            bonusTextObject.transform.position = new Vector3(__instance.WinText.transform.position.x, __instance.WinText.transform.position.y - 0.8f, __instance.WinText.transform.position.z);
            bonusTextObject.transform.localScale = new Vector3(0.7f, 0.7f, 1f);
            textRenderer = bonusTextObject.GetComponent<TMPro.TMP_Text>();
            textRenderer.text = "";
            var text = "";
            var RoleColor = Color.white;
            switch (AdditionalTempData.winCondition)
            {
                case WinCondition.LoversWin:
                    text = "LoversName";
                    RoleColor = RoleClass.Lovers.color;
                    break;
                case WinCondition.GodWin:
                    text = "GodName";
                    RoleColor = RoleClass.God.color;
                    break;
                case WinCondition.HAISON:
                    text = "HAISON";
                    __instance.WinText.text = ModTranslation.GetString("HaisonName");
                    Color32 HaisonColor = new(163, 163, 162, byte.MaxValue);
                    __instance.WinText.color = HaisonColor;
                    RoleColor = HaisonColor;
                    break;
                case WinCondition.JesterWin:
                    text = "JesterName";
                    RoleColor = RoleClass.Jester.color;
                    break;
                case WinCondition.JackalWin:
                    text = "JackalName";
                    RoleColor = RoleClass.Jackal.color;
                    break;
                case WinCondition.QuarreledWin:
                    text = "QuarreledName";
                    RoleColor = RoleClass.Quarreled.color;
                    break;
                case WinCondition.EgoistWin:
                    text = "EgoistName";
                    RoleColor = RoleClass.Egoist.color;
                    break;
                case WinCondition.WorkpersonWin:
                    text = "WorkpersonName";
                    RoleColor = RoleClass.Workperson.color;
                    break;
                case WinCondition.FalseChargesWin:
                    text = "FalseChargesName";
                    RoleColor = RoleClass.FalseCharges.color;
                    break;
                case WinCondition.FoxWin:
                    text = "FoxName";
                    RoleColor = RoleClass.Fox.color;
                    break;
                case WinCondition.DemonWin:
                    text = "DemonName";
                    RoleColor = RoleClass.Demon.color;
                    break;
                case WinCondition.ArsonistWin:
                    text = "ArsonistName";
                    RoleColor = RoleClass.Arsonist.color;
                    break;
                case WinCondition.VultureWin:
                    text = "VultureName";
                    RoleColor = RoleClass.Vulture.color;
                    break;
                case WinCondition.TunaWin:
                    text = "TunaName";
                    RoleColor = RoleClass.Tuna.color;
                    break;
                case WinCondition.NeetWin:
                    text = "NeetName";
                    RoleColor = RoleClass.Neet.color;
                    break;
                case WinCondition.RevolutionistWin:
                    text = "RevolutionistName";
                    RoleColor = RoleClass.Revolutionist.color;
                    break;
                case WinCondition.SpelunkerWin:
                    text = "SpelunkerName";
                    RoleColor = RoleClass.Spelunker.color;
                    break;
                case WinCondition.SuicidalIdeationWin:
                    text = RoleClass.SuicidalIdeation.SuicidalIdeationWinText ? "SuicidalIdeationWinText" : "SuicidalIdeationName";
                    RoleColor = RoleClass.SuicidalIdeation.color;
                    break;
                default:
                    switch (AdditionalTempData.gameOverReason)
                    {
                        case GameOverReason.HumansByTask:
                        case GameOverReason.HumansByVote:
                        case GameOverReason.HumansDisconnect:
                            text = "CrewMateName";
                            RoleColor = Palette.White;
                            break;
                        case GameOverReason.ImpostorByKill:
                        case GameOverReason.ImpostorBySabotage:
                        case GameOverReason.ImpostorByVote:
                        case GameOverReason.ImpostorDisconnect:
                        //MadJester勝利をインポスター勝利とみなした
                        case (GameOverReason)CustomGameOverReason.MadJesterWin:
                            text = "ImpostorName";
                            RoleColor = RoleClass.ImpostorRed;
                            break;
                    }
                    break;
            }
            textRenderer.color = AdditionalTempData.winCondition == WinCondition.HAISON ? Color.clear : RoleColor;
            __instance.BackgroundBar.material.SetColor("_Color", RoleColor);
            var haison = false;
            if (text == "HAISON")
            {
                haison = true;
                text = ModTranslation.GetString("HaisonName");
            }
            else
            {
                text = ModTranslation.GetString(text);
            }
            bool IsOpptexton = false;
            foreach (PlayerControl player in RoleClass.Opportunist.OpportunistPlayer)
            {
                if (player.IsAlive())
                {
                    if (!IsOpptexton && !haison)
                    {
                        IsOpptexton = true;
                        text = text + "&" + ModHelpers.Cs(RoleClass.Opportunist.color, ModTranslation.GetString("OpportunistName"));
                    }
                }
            }
            bool IsLovetexton = false;
            bool Temp1;
            if (!CustomOptions.LoversSingleTeam.GetBool())
            {
                foreach (List<PlayerControl> PlayerList in RoleClass.Lovers.LoversPlayer)
                {
                    Temp1 = false;
                    foreach (PlayerControl player in PlayerList)
                    {
                        if (player.IsAlive())
                        {
                            Temp1 = true;
                        }
                        if (Temp1)
                        {
                            if (!IsLovetexton && !haison)
                            {
                                IsLovetexton = true;
                                text = text + "&" + CustomOptions.Cs(RoleClass.Lovers.color, "LoversName");
                            }
                        }
                    }
                }
            }
            if (ModeHandler.IsMode(ModeId.Zombie))
            {
                if (AdditionalTempData.winCondition == WinCondition.Default)
                {
                    text = ModTranslation.GetString("ZombieZombieName");
                    textRenderer.color = Mode.Zombie.Main.Zombiecolor;
                }
                else if (AdditionalTempData.winCondition == WinCondition.WorkpersonWin)
                {
                    text = ModTranslation.GetString("ZombiePoliceName");
                    textRenderer.color = Mode.Zombie.Main.Policecolor;
                }
            }
            else if (ModeHandler.IsMode(ModeId.BattleRoyal))
            {
                foreach (PlayerControl p in CachedPlayer.AllPlayers)
                {
                    if (p.IsAlive())
                    {
                        text = p.NameText().text;
                        textRenderer.color = new Color32(116, 80, 48, byte.MaxValue);
                    }
                }
            }
            textRenderer.text = haison ? text : string.Format(text + " " + ModTranslation.GetString("WinName"));
            try
            {
                var position = Camera.main.ViewportToWorldPoint(new Vector3(0f, 1f, Camera.main.nearClipPlane));
                GameObject roleSummary = UnityEngine.Object.Instantiate(__instance.WinText.gameObject);
                roleSummary.transform.position = new Vector3(__instance.Navigation.ExitButton.transform.position.x + 0.1f, position.y - 0.1f, -14f);
                roleSummary.transform.localScale = new Vector3(1f, 1f, 1f);

                var roleSummaryText = new StringBuilder();
                roleSummaryText.AppendLine(ModTranslation.GetString("最終結果"));

                foreach (var datas in AdditionalTempData.playerRoles)
                {
                    var taskInfo = datas.TasksTotal > 0 ? $"<color=#FAD934FF>({datas.TasksCompleted}/{datas.TasksTotal})</color>" : "";
                    string roleText = CustomOptions.Cs(datas.IntroDate.color, datas.IntroDate.NameKey + "Name");
                    if (datas.GhostIntroDate.RoleId != RoleId.DefaultRole)
                    {
                        roleText += $" → {CustomOptions.Cs(datas.GhostIntroDate.color, datas.GhostIntroDate.NameKey + "Name")}";
                    }
                    string result = $"{ModHelpers.Cs(Palette.PlayerColors[datas.ColorId], datas.PlayerName)}{datas.NameSuffix}{taskInfo} - {GetStatusText(datas.Status)} - {roleText}";
                    if (ModeHandler.IsMode(ModeId.Zombie))
                    {
                        roleText = datas.ColorId == 1 ? CustomOptions.Cs(Mode.Zombie.Main.Policecolor, "ZombiePoliceName") : CustomOptions.Cs(Mode.Zombie.Main.Zombiecolor, "ZombieZombieName");
                        if (datas.ColorId == 2) taskInfo = "";
                        result = $"{ModHelpers.Cs(Palette.PlayerColors[datas.ColorId], datas.PlayerName)}{taskInfo} : {roleText}";
                    }
                    roleSummaryText.AppendLine(result);
                }

                TMPro.TMP_Text roleSummaryTextMesh = roleSummary.GetComponent<TMPro.TMP_Text>();
                roleSummaryTextMesh.alignment = TMPro.TextAlignmentOptions.TopLeft;
                roleSummaryTextMesh.color = Color.white;
                roleSummaryTextMesh.outlineWidth *= 1.2f;
                roleSummaryTextMesh.fontSizeMin = 1.25f;
                roleSummaryTextMesh.fontSizeMax = 1.25f;
                roleSummaryTextMesh.fontSize = 1.25f;

                var roleSummaryTextMeshRectTransform = roleSummaryTextMesh.GetComponent<RectTransform>();
                roleSummaryTextMeshRectTransform.anchoredPosition = new Vector2(position.x + 3.5f, position.y - 0.1f);
                roleSummaryTextMesh.text = roleSummaryText.ToString();
            }
            catch (Exception e)
            {
                SuperNewRolesPlugin.Logger.LogInfo("エラー:" + e);
            }
            AdditionalTempData.Clear();

            IsHaison = false;
        }
        static string GetStatusText(FinalStatus status) => ModTranslation.GetString("FinalStatus" + status.ToString()); //ローカル関数
    }

    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameEnd))]
    public static class OnGameEndPatch
    {
        public static PlayerControl WinnerPlayer;
        public static CustomGameOverReason? EndData = null;
        public static void Prefix([HarmonyArgument(0)] ref EndGameResult endGameResult)
        {
            AdditionalTempData.gameOverReason = endGameResult.GameOverReason;
            foreach (PlayerControl p in CachedPlayer.AllPlayers)
            {
                try
                {
                    p.resetChange();
                }
                catch { }
            }
            if ((int)endGameResult.GameOverReason >= 10) endGameResult.GameOverReason = GameOverReason.ImpostorByKill;
        }

        public static void Postfix()
    { 
            if (AmongUsClient.Instance.AmHost && ModeHandler.IsMode(ModeId.SuperHostRoles, ModeId.Zombie))
            {
                PlayerControl.GameOptions = SyncSetting.OptionData.DeepCopy();
                CachedPlayer.LocalPlayer.PlayerControl.RpcSyncSettings(PlayerControl.GameOptions);
            }
            var gameOverReason = AdditionalTempData.gameOverReason;
            AdditionalTempData.Clear();

            foreach (var p in GameData.Instance.AllPlayers)
            {
                if (p.Object.IsPlayer())
                {
                    //var p = pc.Data;
                    var roles = Intro.IntroDate.GetIntroDate(p.Object.GetRole(), p.Object);
                    var ghostRoles = Intro.IntroDate.GetIntroDate(p.Object.GetGhostRole(), p.Object);
                    var (tasksCompleted, tasksTotal) = TaskCount.TaskDate(p);
                    if (p.Object.IsImpostor())
                    {
                        tasksCompleted = 0;
                        tasksTotal = 0;
                    }
                    var finalStatus = FinalStatusPatch.FinalStatusData.FinalStatuses[p.PlayerId] =
                        p.Disconnected == true ? FinalStatus.Disconnected :
                        FinalStatusPatch.FinalStatusData.FinalStatuses.ContainsKey(p.PlayerId) ? FinalStatusPatch.FinalStatusData.FinalStatuses[p.PlayerId] :
                        p.IsDead == true ? FinalStatus.Exiled :
                        gameOverReason == GameOverReason.ImpostorBySabotage && !p.Role.IsImpostor ? FinalStatus.Sabotage :
                        FinalStatus.Alive;
                    string namesuffix = "";
                    if (p.Object.IsLovers())
                    {
                        namesuffix = ModHelpers.Cs(RoleClass.Lovers.color, " ♥");
                    }
                    AdditionalTempData.playerRoles.Add(new AdditionalTempData.PlayerRoleInfo()
                    {
                        PlayerName = p.DefaultOutfit.PlayerName,
                        NameSuffix = namesuffix,
                        PlayerId = p.PlayerId,
                        ColorId = p.DefaultOutfit.ColorId,
                        TasksTotal = tasksTotal,
                        TasksCompleted = gameOverReason == GameOverReason.HumansByTask ? tasksTotal : tasksCompleted,
                        Status = finalStatus,
                        IntroDate = roles,
                        GhostIntroDate = ghostRoles
                    });
                }
            }
            // Remove Jester, Arsonist, Vulture, Jackal, former Jackals and Sidekick from winners (if they win, they'll be readded)
            List<PlayerControl> notWinners = new();

            notWinners.AddRange(RoleClass.Jester.JesterPlayer);
            notWinners.AddRange(RoleClass.MadMate.MadMatePlayer);
            notWinners.AddRange(RoleClass.Jackal.JackalPlayer);
            notWinners.AddRange(RoleClass.Jackal.SidekickPlayer);
            notWinners.AddRange(RoleClass.JackalFriends.JackalFriendsPlayer);
            notWinners.AddRange(RoleClass.God.GodPlayer);
            notWinners.AddRange(RoleClass.Opportunist.OpportunistPlayer);
            notWinners.AddRange(RoleClass.Truelover.trueloverPlayer);
            notWinners.AddRange(RoleClass.Egoist.EgoistPlayer);
            notWinners.AddRange(RoleClass.Workperson.WorkpersonPlayer);
            notWinners.AddRange(RoleClass.Amnesiac.AmnesiacPlayer);
            notWinners.AddRange(RoleClass.SideKiller.MadKillerPlayer);
            notWinners.AddRange(RoleClass.MadMayor.MadMayorPlayer);
            notWinners.AddRange(RoleClass.MadStuntMan.MadStuntManPlayer);
            notWinners.AddRange(RoleClass.MadHawk.MadHawkPlayer);
            notWinners.AddRange(RoleClass.MadJester.MadJesterPlayer);
            notWinners.AddRange(RoleClass.MadSeer.MadSeerPlayer);
            notWinners.AddRange(RoleClass.FalseCharges.FalseChargesPlayer);
            notWinners.AddRange(RoleClass.Fox.FoxPlayer);
            notWinners.AddRange(BotManager.AllBots);
            notWinners.AddRange(RoleClass.MadMaker.MadMakerPlayer);
            notWinners.AddRange(RoleClass.Demon.DemonPlayer);
            notWinners.AddRange(RoleClass.SeerFriends.SeerFriendsPlayer);
            notWinners.AddRange(RoleClass.JackalSeer.JackalSeerPlayer);
            notWinners.AddRange(RoleClass.JackalSeer.SidekickSeerPlayer);
            notWinners.AddRange(RoleClass.Arsonist.ArsonistPlayer);
            notWinners.AddRange(RoleClass.Vulture.VulturePlayer);
            notWinners.AddRange(RoleClass.MadCleaner.MadCleanerPlayer);
            notWinners.AddRange(RoleClass.MayorFriends.MayorFriendsPlayer);
            notWinners.AddRange(RoleClass.Tuna.TunaPlayer);
            notWinners.AddRange(RoleClass.BlackCat.BlackCatPlayer);
            notWinners.AddRange(RoleClass.Neet.NeetPlayer);
            notWinners.AddRange(RoleClass.Revolutionist.RevolutionistPlayer);
            notWinners.AddRange(RoleClass.SuicidalIdeation.SuicidalIdeationPlayer);

            foreach (PlayerControl p in RoleClass.Survivor.SurvivorPlayer)
            {
                if (p.IsDead())
                {
                    notWinners.Add(p);
                }
            }

            List<WinningPlayerData> winnersToRemove = new();
            foreach (WinningPlayerData winner in TempData.winners)
            {
                if (notWinners.Any(x => x.Data.PlayerName == winner.PlayerName)) winnersToRemove.Add(winner);
            }
            foreach (var winner in winnersToRemove) TempData.winners.Remove(winner);
            // Neutral shifter can't win

            bool saboWin = gameOverReason == GameOverReason.ImpostorBySabotage;
            bool JesterWin = gameOverReason == (GameOverReason)CustomGameOverReason.JesterWin;
            bool MadJesterWin = gameOverReason == (GameOverReason)CustomGameOverReason.ImpostorWin;
            bool QuarreledWin = gameOverReason == (GameOverReason)CustomGameOverReason.QuarreledWin;
            bool JackalWin = gameOverReason == (GameOverReason)CustomGameOverReason.JackalWin;
            bool HAISON = EndGameManagerSetUpPatch.IsHaison;
            bool EgoistWin = gameOverReason == (GameOverReason)CustomGameOverReason.EgoistWin;
            bool WorkpersonWin = gameOverReason == (GameOverReason)CustomGameOverReason.WorkpersonWin;
            bool FalseChargesWin = gameOverReason == (GameOverReason)CustomGameOverReason.FalseChargesWin;
            bool FoxWin = gameOverReason == (GameOverReason)CustomGameOverReason.FoxWin;
            bool DemonWin = gameOverReason == (GameOverReason)CustomGameOverReason.DemonWin;
            bool ArsonistWin = gameOverReason == (GameOverReason)CustomGameOverReason.ArsonistWin;
            bool VultureWin = gameOverReason == (GameOverReason)CustomGameOverReason.VultureWin;
            bool NeetWin = gameOverReason == (GameOverReason)CustomGameOverReason.NeetWin;
            bool RevolutionistWin = gameOverReason == (GameOverReason)CustomGameOverReason.RevolutionistWin;
            bool SuicidalIdeationWin = gameOverReason == (GameOverReason)CustomGameOverReason.SuicidalIdeationWin;
            bool BUGEND = gameOverReason == (GameOverReason)CustomGameOverReason.BugEnd;
            if (ModeHandler.IsMode(ModeId.SuperHostRoles) && EndData != null)
            {
                JesterWin = EndData == CustomGameOverReason.JesterWin;
                MadJesterWin = EndData == CustomGameOverReason.MadJesterWin;
                EgoistWin = EndData == CustomGameOverReason.EgoistWin;
                WorkpersonWin = EndData == CustomGameOverReason.WorkpersonWin;
                FalseChargesWin = EndData == CustomGameOverReason.FalseChargesWin;
                QuarreledWin = EndData == CustomGameOverReason.QuarreledWin;
                FoxWin = EndData == CustomGameOverReason.FoxWin;
                JackalWin = EndData == CustomGameOverReason.JackalWin;
                DemonWin = EndData == CustomGameOverReason.DemonWin;
                ArsonistWin = EndData == CustomGameOverReason.ArsonistWin;
                VultureWin = EndData == CustomGameOverReason.VultureWin;
                NeetWin = EndData == CustomGameOverReason.NeetWin;
            }
            if (JesterWin)
            {
                TempData.winners = new Il2CppSystem.Collections.Generic.List<WinningPlayerData>();
                WinnerPlayer.Data.IsDead = false;
                WinningPlayerData wpd = new(WinnerPlayer.Data);
                TempData.winners.Add(wpd);
                AdditionalTempData.winCondition = WinCondition.JesterWin;
            }
            else if (MadJesterWin)
            {
                TempData.winners = new Il2CppSystem.Collections.Generic.List<WinningPlayerData>();
                WinnerPlayer.Data.IsDead = false;
                WinningPlayerData wpd = new(WinnerPlayer.Data);
                TempData.winners.Add(wpd);
                AdditionalTempData.winCondition = WinCondition.MadJesterWin;
            }
            else if (JackalWin)
            {
                TempData.winners = new Il2CppSystem.Collections.Generic.List<WinningPlayerData>();
                foreach (var cp in CachedPlayer.AllPlayers)
                {
                    if (cp.PlayerControl.IsJackalTeam())
                    {
                        WinningPlayerData wpd = new(cp.Data);
                        TempData.winners.Add(wpd);
                    }
                }
                AdditionalTempData.winCondition = WinCondition.JackalWin;
            }
            else if (EgoistWin)
            {
                TempData.winners = new Il2CppSystem.Collections.Generic.List<WinningPlayerData>();
                foreach (PlayerControl p in RoleClass.Egoist.EgoistPlayer)
                {
                    if (p.IsAlive())
                    {
                        WinningPlayerData wpd = new(p.Data);
                        TempData.winners.Add(wpd);
                    }
                }
                AdditionalTempData.winCondition = WinCondition.EgoistWin;
            }
            else if (WorkpersonWin)
            {
                TempData.winners = new Il2CppSystem.Collections.Generic.List<WinningPlayerData>();
                WinningPlayerData wpd = new(WinnerPlayer.Data);
                TempData.winners.Add(wpd);
                AdditionalTempData.winCondition = WinCondition.WorkpersonWin;
            }
            else if (FalseChargesWin)
            {
                TempData.winners = new Il2CppSystem.Collections.Generic.List<WinningPlayerData>();
                WinningPlayerData wpd = new(WinnerPlayer.Data);
                TempData.winners.Add(wpd);
                AdditionalTempData.winCondition = WinCondition.FalseChargesWin;
            }
            else if (DemonWin)
            {
                TempData.winners = new Il2CppSystem.Collections.Generic.List<WinningPlayerData>();
                foreach (PlayerControl player in RoleClass.Demon.DemonPlayer)
                {
                    if (Demon.IsWin(player))
                    {
                        WinningPlayerData wpd = new(player.Data);
                        TempData.winners.Add(wpd);
                    }
                }
                AdditionalTempData.winCondition = WinCondition.DemonWin;
            }
            else if (ArsonistWin)
            {
                TempData.winners = new Il2CppSystem.Collections.Generic.List<WinningPlayerData>();
                foreach (PlayerControl player in RoleClass.Arsonist.ArsonistPlayer)
                {
                    if (Arsonist.IsArsonistWinFlag())
                    {
                        SuperNewRolesPlugin.Logger.LogInfo("アーソニストがEndGame");
                        WinningPlayerData wpd = new(player.Data);
                        TempData.winners.Add(wpd);
                    }
                }
                AdditionalTempData.winCondition = WinCondition.ArsonistWin;
            }
            else if (VultureWin)
            {
                TempData.winners = new Il2CppSystem.Collections.Generic.List<WinningPlayerData>();
                WinningPlayerData wpd = new(WinnerPlayer.Data);
                TempData.winners.Add(wpd);
                AdditionalTempData.winCondition = WinCondition.VultureWin;
            }
            else if (RevolutionistWin)
            {
                TempData.winners = new();
                WinningPlayerData wpd = new(WinnerPlayer.Data);
                TempData.winners.Add(wpd);
                AdditionalTempData.winCondition = WinCondition.RevolutionistWin;
            }
            else if (SuicidalIdeationWin)
            {
                TempData.winners = new Il2CppSystem.Collections.Generic.List<WinningPlayerData>();
                WinningPlayerData wpd = new(WinnerPlayer.Data);
                TempData.winners.Add(wpd);
                AdditionalTempData.winCondition = WinCondition.SuicidalIdeationWin;
            }

            if (TempData.winners.GetFastEnumerator().ToArray().Any(x => x.IsImpostor))
            {
                foreach (var cp in CachedPlayer.AllPlayers)
                {
                    if (cp.PlayerControl.IsMadRoles())
                    {
                        WinningPlayerData wpd = new(cp.Data);
                        TempData.winners.Add(wpd);
                    }
                }
            }

            if (ModeHandler.IsMode(ModeId.BattleRoyal))
            {
                TempData.winners = new Il2CppSystem.Collections.Generic.List<WinningPlayerData>();
                if (Mode.BattleRoyal.Main.IsTeamBattle)
                {
                    foreach (PlayerControl p in Mode.BattleRoyal.Main.Winners)
                    {
                        WinningPlayerData wpd = new(p.Data);
                        TempData.winners.Add(wpd);
                    }
                }
                else
                {
                    foreach (PlayerControl p in CachedPlayer.AllPlayers)
                    {
                        if (p.IsAlive())
                        {
                            WinningPlayerData wpd = new(p.Data);
                            TempData.winners.Add(wpd);
                        }
                    }
                }
                AdditionalTempData.winCondition = WinCondition.Default;
            }
            foreach (PlayerControl p in RoleClass.God.GodPlayer)
            {
                if (p.IsAlive())
                {
                    var (complate, all) = TaskCount.TaskDateNoClearCheck(p.Data);
                    if (!RoleClass.God.IsTaskEndWin || complate >= all)
                    {
                        TempData.winners = new Il2CppSystem.Collections.Generic.List<WinningPlayerData>();
                        WinningPlayerData wpd = new(p.Data);
                        TempData.winners.Add(wpd);
                        AdditionalTempData.winCondition = WinCondition.GodWin;
                    }
                }
            }
            foreach (PlayerControl p in RoleClass.Fox.FoxPlayer)
            {
                if (p.IsAlive())
                {
                    TempData.winners = new Il2CppSystem.Collections.Generic.List<WinningPlayerData>();
                    WinningPlayerData wpd = new(p.Data);
                    TempData.winners.Add(wpd);
                    AdditionalTempData.winCondition = WinCondition.FoxWin;
                }
            }
            foreach (PlayerControl p in RoleClass.Tuna.TunaPlayer)
            {
                if (p.IsAlive() && !RoleClass.Tuna.IsTunaAddWin)
                {
                    TempData.winners = new Il2CppSystem.Collections.Generic.List<WinningPlayerData>();
                    WinningPlayerData wpd = new(p.Data);
                    TempData.winners.Add(wpd);
                    AdditionalTempData.winCondition = WinCondition.TunaWin;

                }
                if (p.IsAlive() && RoleClass.Tuna.IsTunaAddWin)
                {
                    TempData.winners.Add(new WinningPlayerData(p.Data));
                }
            }
            foreach (PlayerControl p in RoleClass.Neet.NeetPlayer)
            {
                if (p.IsAlive() && !RoleClass.Neet.IsAddWin)
                {
                    TempData.winners = new Il2CppSystem.Collections.Generic.List<WinningPlayerData>();
                    WinningPlayerData wpd = new(p.Data);
                    TempData.winners.Add(wpd);
                    AdditionalTempData.winCondition = WinCondition.NeetWin;

                }
                if (p.IsAlive() && RoleClass.Neet.IsAddWin)
                {
                    TempData.winners.Add(new WinningPlayerData(p.Data));
                }
            }
            foreach (PlayerControl p in RoleClass.Spelunker.SpelunkerPlayer)
            {
                bool isreset = false;
                if (p.IsAlive())
                {
                    if (!isreset)
                    {
                        TempData.winners = new Il2CppSystem.Collections.Generic.List<WinningPlayerData>();
                        WinningPlayerData wpd = new(p.Data);
                        TempData.winners.Add(wpd);
                        AdditionalTempData.winCondition = WinCondition.SpelunkerWin;
                    }
                    isreset = true;
                }
            }
            foreach (PlayerControl p in RoleClass.SuicidalIdeation.SuicidalIdeationPlayer)
            {
                var (playerCompleted, playerTotal) = TaskCount.TaskDate(p.Data);
                if (p.IsAlive() && playerTotal > playerCompleted) 
                {
                    TempData.winners.Add(new WinningPlayerData(p.Data));
                }
            }
            foreach (PlayerControl player in RoleClass.Opportunist.OpportunistPlayer)
            {
                if (player.IsAlive())
                {
                    TempData.winners.Add(new WinningPlayerData(player.Data));
                }
            }
            if (RoleClass.Revolutionist.IsAddWin)
            {
                foreach (PlayerControl player in RoleClass.Revolutionist.RevolutionistPlayer)
                {
                    if (!RoleClass.Revolutionist.IsAddWinAlive || player.IsAlive() && !TempData.winners.Contains(new(player.Data)))
                    {
                        TempData.winners.Add(new WinningPlayerData(player.Data));
                    }
                }
            }
            foreach (PlayerControl player in RoleClass.Neet.NeetPlayer)
            {
                if (player.IsAlive())
                {
                    TempData.winners.Add(new WinningPlayerData(player.Data));
                }
            }
            foreach (List<PlayerControl> players in RoleClass.Quarreled.QuarreledPlayer)
            {
                notWinners.AddRange(players);
            }
            foreach (List<PlayerControl> players in RoleClass.Lovers.LoversPlayer)
            {
                notWinners.AddRange(players);
            }

            notWinners = new();
            winnersToRemove = new();
            foreach (WinningPlayerData winner in TempData.winners)
            {
                if (notWinners.Any(x => x.Data.PlayerName == winner.PlayerName)) winnersToRemove.Add(winner);
            }
            foreach (var winner in winnersToRemove) TempData.winners.Remove(winner);

            if (QuarreledWin)
            {
                TempData.winners = new Il2CppSystem.Collections.Generic.List<WinningPlayerData>();
                List<PlayerControl> winplays = new()
                {
                    WinnerPlayer,
                    WinnerPlayer.GetOneSideQuarreled()
                };
                foreach (PlayerControl p in winplays)
                {
                    p.Data.IsDead = false;
                    WinningPlayerData wpd = new(p.Data);
                    TempData.winners.Add(wpd);
                }
                AdditionalTempData.winCondition = WinCondition.QuarreledWin;
            }
            bool IsSingleTeam = CustomOptions.LoversSingleTeam.GetBool();
            foreach (List<PlayerControl> plist in RoleClass.Lovers.LoversPlayer)
            {
                bool IsWinLovers = false;
                foreach (PlayerControl p in plist)
                {
                    if (p.IsAlive())
                    {
                        IsWinLovers = true;
                    }
                }
                if (IsWinLovers && IsSingleTeam)
                {
                    TempData.winners = new Il2CppSystem.Collections.Generic.List<WinningPlayerData>();
                    AdditionalTempData.winCondition = WinCondition.LoversWin;
                }
            }
            foreach (List<PlayerControl> plist in RoleClass.Lovers.LoversPlayer)
            {
                bool IsWinLovers = false;
                foreach (PlayerControl p in plist)
                {
                    if (p.IsAlive())
                    {
                        IsWinLovers = true;
                    }
                }
                if (IsWinLovers)
                {
                    foreach (PlayerControl p in plist)
                    {
                        WinningPlayerData wpd = new(p.Data);
                        TempData.winners.Add(wpd);
                        if (IsSingleTeam)
                        {
                        }
                    }
                }
            }
            if (ModeHandler.IsMode(ModeId.Zombie))
            {
                TempData.winners = new Il2CppSystem.Collections.Generic.List<WinningPlayerData>();
                if (gameOverReason == GameOverReason.ImpostorByKill)
                {
                    AdditionalTempData.winCondition = WinCondition.Default;
                    foreach (PlayerControl p in CachedPlayer.AllPlayers)
                    {
                        if (p.CurrentOutfit.ColorId == 2)
                        {
                            WinningPlayerData wpd = new(p.Data);
                            TempData.winners.Add(wpd);
                        }
                    }
                }
                else
                {
                    foreach (PlayerControl p in CachedPlayer.AllPlayers)
                    {
                        AdditionalTempData.winCondition = WinCondition.WorkpersonWin;
                        if (p.CurrentOutfit.ColorId == 1)
                        {
                            WinningPlayerData wpd = new(p.Data);
                            TempData.winners.Add(wpd);
                        }
                    }
                }
            }
            if (HAISON)
            {
                TempData.winners = new Il2CppSystem.Collections.Generic.List<WinningPlayerData>();
                foreach (PlayerControl p in CachedPlayer.AllPlayers)
                {
                    if (p.IsPlayer())
                    {
                        WinningPlayerData wpd = new(p.Data);
                        TempData.winners.Add(wpd);
                    }
                }
                AdditionalTempData.winCondition = WinCondition.HAISON;
            }
        }
    }
    [HarmonyPatch(typeof(TranslationController), nameof(TranslationController.GetString), new Type[] { typeof(StringNames), typeof(Il2CppReferenceArray<Il2CppSystem.Object>) })]
    class ExileControllerMessagePatch
    {
        static void Postfix(ref string __result, [HarmonyArgument(0)] StringNames id)
        {
            try
            {
                if (ExileController.Instance != null && ExileController.Instance.exiled != null && ModeHandler.IsMode(ModeId.Default))
                {
                    PlayerControl player = ModHelpers.playerById(ExileController.Instance.exiled.Object.PlayerId);
                    if (player == null) return;
                    FinalStatusPatch.FinalStatusData.FinalStatuses[player.PlayerId] = FinalStatus.Exiled;
                    // Exile role text
                    if (id is StringNames.ExileTextPN or StringNames.ExileTextSN or StringNames.ExileTextPP or StringNames.ExileTextSP)
                    {
                        __result = player.Data.PlayerName + " は " + ModTranslation.GetString(Intro.IntroDate.GetIntroDate(player.GetRole(), player).NameKey + "Name") + " だった！";
                    }
                }
            }
            catch
            {
                // pass - Hopefully prevent leaving while exiling to softlock game
            }
        }
    }
    public class WrapUpClass
    {
        public static void SetCoolTime()
        {
            PlayerControl.LocalPlayer.SetKillTimerUnchecked(RoleHelpers.GetEndMeetingKillCoolTime(PlayerControl.LocalPlayer), RoleHelpers.GetEndMeetingKillCoolTime(PlayerControl.LocalPlayer));
        }
        public static void WrapUpPostfix(GameData.PlayerInfo exiled)
        {
        }
    }
    [HarmonyPatch(typeof(ExileController), nameof(ExileController.ReEnableGameplay))]
    class ExileControllerReEnableGameplayPatch
    {
        public static void Postfix(ExileController __instance)
        {
            Buttons.CustomButton.MeetingEndedUpdate();
        }
    }

    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.CheckEndCriteria))]
    class CheckGameEndPatch
    {
        public static bool Prefix(ShipStatus __instance)
        {
            if (!GameData.Instance) return false;
            if (DestroyableSingleton<TutorialManager>.InstanceExists) return true;
            if (!RoleManagerSelectRolesPatch.IsSetRoleRPC) return false;
            if (DebugMode.IsDebugMode()) return false;
            if (RoleClass.Assassin.TriggerPlayer != null) return false;
            if (RoleClass.Revolutionist.MeetingTrigger != null) return false;
            var statistics = new PlayerStatistics(__instance);
            if (!ModeHandler.IsMode(ModeId.Default))
            {
                ModeHandler.EndGameChecks(__instance, statistics);
            }
            else
            {
                if (CheckAndEndGameForCrewmateWin(__instance, statistics)) return false;
                if (CheckAndEndGameForSabotageWin(__instance)) return false;
                if (CheckAndEndGameForJackalWin(__instance, statistics)) return false;
                if (CheckAndEndGameForEgoistWin(__instance, statistics)) return false;
                if (CheckAndEndGameForImpostorWin(__instance, statistics)) return false;
                if (CheckAndEndGameForWorkpersonWin(__instance)) return false;
                if (CheckAndEndGameForSuicidalIdeationWin(__instance)) return false;
                if (!PlusModeHandler.IsMode(PlusModeId.NotTaskWin) && CheckAndEndGameForTaskWin(__instance)) return false;
            }
            return false;
        }
        public static void CustomEndGame(GameOverReason reason, bool showAd)
        {
            ShipStatus.RpcEndGame(reason, showAd);
        }
        public static bool CheckAndEndGameForSabotageWin(ShipStatus __instance)
        {
            if (__instance.Systems == null) return false;
            ISystemType systemType = __instance.Systems.ContainsKey(SystemTypes.LifeSupp) ? __instance.Systems[SystemTypes.LifeSupp] : null;
            if (systemType != null)
            {
                LifeSuppSystemType lifeSuppSystemType = systemType.TryCast<LifeSuppSystemType>();
                if (lifeSuppSystemType != null && lifeSuppSystemType.Countdown < 0f)
                {
                    EndGameForSabotage(__instance);
                    lifeSuppSystemType.Countdown = 10000f;
                    return true;
                }
            }
            ISystemType systemType2 = __instance.Systems.ContainsKey(SystemTypes.Reactor) ? __instance.Systems[SystemTypes.Reactor] : null;
            if (systemType2 == null)
            {
                systemType2 = __instance.Systems.ContainsKey(SystemTypes.Laboratory) ? __instance.Systems[SystemTypes.Laboratory] : null;
            }
            if (systemType2 != null)
            {
                ICriticalSabotage criticalSystem = systemType2.TryCast<ICriticalSabotage>();
                if (criticalSystem != null && criticalSystem.Countdown < 0f)
                {
                    EndGameForSabotage(__instance);
                    criticalSystem.ClearSabotage();
                    return true;
                }
            }
            return false;
        }

        public static bool CheckAndEndGameForTaskWin(ShipStatus __instance)
        {
            if (GameData.Instance.TotalTasks <= GameData.Instance.CompletedTasks)
            {
                __instance.enabled = false;
                CustomEndGame(GameOverReason.HumansByTask, false);
                return true;
            }
            return false;
        }

        public static bool CheckAndEndGameForImpostorWin(ShipStatus __instance, PlayerStatistics statistics)
        {
            if (statistics.TeamImpostorsAlive >= statistics.TotalAlive - statistics.TeamImpostorsAlive && statistics.TeamJackalAlive == 0 && !EvilEraser.IsGodWinGuard() && !EvilEraser.IsFoxWinGuard() && !EvilEraser.IsNeetWinGuard())
            {
                __instance.enabled = false;
                var endReason = TempData.LastDeathReason switch
                {
                    DeathReason.Exile => GameOverReason.ImpostorByVote,
                    DeathReason.Kill => GameOverReason.ImpostorByKill,
                    _ => GameOverReason.ImpostorByVote,
                };
                if (Demon.IsDemonWinFlag())
                {
                    endReason = (GameOverReason)CustomGameOverReason.DemonWin;
                }

                CustomEndGame(endReason, false);
                return true;
            }
            return false;
        }

        public static bool CheckAndEndGameForArsonistWin(ShipStatus __instance, PlayerStatistics statistics)
        {
            if (Arsonist.IsArsonistWinFlag())
            {
                __instance.enabled = false;
                CustomEndGame((GameOverReason)CustomGameOverReason.ArsonistWin, false);
                return true;
            }
            return false;
        }
        public static bool CheckAndEndGameForEgoistWin(ShipStatus __instance, PlayerStatistics statistics)
        {
            if (statistics.EgoistAlive >= statistics.TotalAlive - statistics.EgoistAlive && statistics.EgoistAlive != 0 && statistics.TeamImpostorsAlive == 0 && statistics.TeamJackalAlive == 0)
            {
                __instance.enabled = false;
                CustomEndGame((GameOverReason)CustomGameOverReason.EgoistWin, false);
                return true;
            }
            return false;
        }
        public static bool CheckAndEndGameForJackalWin(ShipStatus __instance, PlayerStatistics statistics)
        {
            if (statistics.TeamJackalAlive >= statistics.TotalAlive - statistics.TeamJackalAlive && statistics.TeamImpostorsAlive == 0)
            {
                foreach (PlayerControl p in RoleClass.SideKiller.MadKillerPlayer)
                {
                    if (!p.IsImpostor() && !p.Data.Disconnected)
                    {
                        return false;
                    }
                }
                __instance.enabled = false;
                CustomEndGame((GameOverReason)CustomGameOverReason.JackalWin, false);
                return true;
            }
            return false;
        }

        public static bool CheckAndEndGameForCrewmateWin(ShipStatus __instance, PlayerStatistics statistics)
        {
            if (statistics.TeamImpostorsAlive == 0 && statistics.TeamJackalAlive == 0)
            {
                foreach (PlayerControl p in RoleClass.SideKiller.MadKillerPlayer)
                {
                    if (!p.IsImpostor() && !p.Data.Disconnected)
                    {
                        return false;
                    }
                }
                __instance.enabled = false;
                CustomEndGame(GameOverReason.HumansByVote, false);
                return true;
            }
            return false;
        }
        public static bool CheckAndEndGameForWorkpersonWin(ShipStatus __instance)
        {
            foreach (PlayerControl p in RoleClass.Workperson.WorkpersonPlayer)
            {
                if (!p.Data.Disconnected)
                {
                    if (p.IsAlive() || !RoleClass.Workperson.IsAliveWin)
                    {
                        var (playerCompleted, playerTotal) = TaskCount.TaskDate(p.Data);
                        if (playerCompleted >= playerTotal)
                        {
                            MessageWriter Writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.ShareWinner, SendOption.Reliable, -1);
                            Writer.Write(p.PlayerId);
                            AmongUsClient.Instance.FinishRpcImmediately(Writer);
                            RPCProcedure.ShareWinner(p.PlayerId);
                            __instance.enabled = false;
                            CustomEndGame((GameOverReason)CustomGameOverReason.WorkpersonWin, false);
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        public static bool CheckAndEndGameForSuicidalIdeationWin(ShipStatus __instance)
        {
            foreach (PlayerControl p in RoleClass.SuicidalIdeation.SuicidalIdeationPlayer)
            {
                if (!p.Data.Disconnected)
                {
                    if (p.IsAlive())
                    {
                        var (playerCompleted, playerTotal) = TaskCount.TaskDate(p.Data);
                        if (playerCompleted >= playerTotal)
                        {
                            MessageWriter Writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.ShareWinner, SendOption.Reliable, -1);
                            Writer.Write(p.PlayerId);
                            AmongUsClient.Instance.FinishRpcImmediately(Writer);
                            RPCProcedure.ShareWinner(p.PlayerId);
                            __instance.enabled = false;
                            CustomEndGame((GameOverReason)CustomGameOverReason.SuicidalIdeationWin, false);
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        public static void EndGameForSabotage(ShipStatus __instance)
        {
            __instance.enabled = false;
            CustomEndGame(GameOverReason.ImpostorBySabotage, false);
            return;
        }
        internal class PlayerStatistics
        {
            public int TeamImpostorsAlive { get; set; }
            public int CrewAlive { get; set; }
            public int TotalAlive { get; set; }
            public int TeamJackalAlive { get; set; }
            public int EgoistAlive { get; set; }
            public PlayerStatistics(ShipStatus __instance)
            {
                GetPlayerCounts();
            }
            private void GetPlayerCounts()
            {
                int numImpostorsAlive = 0;
                int numCrewAlive = 0;
                int numTotalAlive = 0;
                int numTotalJackalTeam = 0;
                int numTotalEgoist = 0;

                for (int i = 0; i < GameData.Instance.PlayerCount; i++)
                {
                    GameData.PlayerInfo playerInfo = GameData.Instance.AllPlayers[i];
                    if (!playerInfo.Disconnected && playerInfo.Object.IsPlayer())
                    {
                        if (playerInfo.Object.IsAlive())
                        {
                            numTotalAlive++;
                            if (playerInfo.Object.IsRole(RoleId.Jackal, RoleId.Sidekick, RoleId.TeleportingJackal, RoleId.JackalSeer, RoleId.SidekickSeer))
                            {
                                numTotalJackalTeam++;
                            }
                            else if (playerInfo.Object.IsImpostor())
                            {
                                numImpostorsAlive++;
                            }
                            else if (!playerInfo.Object.IsCrew())
                            {
                                numCrewAlive++;
                            }
                            else if (playerInfo.Object.IsNeutral())
                            {
                                if (playerInfo.Object.IsRole(RoleId.Egoist))
                                {
                                    numTotalEgoist++;
                                    numImpostorsAlive++;
                                }
                            }
                        }
                    }
                }
                TeamImpostorsAlive = numImpostorsAlive;
                TotalAlive = numTotalAlive;
                CrewAlive = numCrewAlive;
                TeamJackalAlive = numTotalJackalTeam;
                EgoistAlive = numTotalEgoist;
            }
        }
    }
}