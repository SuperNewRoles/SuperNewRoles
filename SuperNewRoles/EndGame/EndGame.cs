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
using SuperNewRoles.Patch;
using SuperNewRoles.CustomOption;
using SuperNewRoles.CustomRPC;
using SuperNewRoles.Mode.SuperHostRoles;
using InnerNet;
using SuperNewRoles.Helpers;
using SuperNewRoles.Sabotage;

namespace SuperNewRoles.EndGame
{
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
            List<WinningPlayerData> list = TempData.winners.GetFastEnumerator().ToArray().ToList().OrderBy(delegate (WinningPlayerData b)
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
                Vector3 vector = new Vector3(num7, num7, 1f);
                poolablePlayer.transform.localScale = vector;
                poolablePlayer.UpdateFromPlayerOutfit(winningPlayerData2, winningPlayerData2.IsDead);

                if (winningPlayerData2.IsDead)
                {
                    poolablePlayer.BodySprites[0].BodySprite.sprite = __instance.GhostSprite;
                    poolablePlayer.SetDeadFlipX(i % 2 == 0);
                }
                else
                {
                    poolablePlayer.SetFlipX(i % 2 == 0);
                }

                poolablePlayer.NameText.color = Color.white;
                poolablePlayer.NameText.lineSpacing *= 0.7f;
                poolablePlayer.NameText.transform.localScale = new Vector3(1f / vector.x, 1f / vector.y, 1f / vector.z);
                poolablePlayer.NameText.transform.localPosition = new Vector3(poolablePlayer.NameText.transform.localPosition.x, poolablePlayer.NameText.transform.localPosition.y, -15f);

                poolablePlayer.NameText.text = winningPlayerData2.PlayerName;

                foreach (var data in AdditionalTempData.playerRoles)
                {
                    if (data.PlayerName != winningPlayerData2.PlayerName) continue;
                    poolablePlayer.NameText.text = data.PlayerName + data.NameSuffix + $"\n<size=80%>{string.Join("\n", CustomOptions.cs(data.IntroDate.color, data.IntroDate.NameKey + "Name"))}</size>";
                }
            }
            GameObject bonusTextObject = UnityEngine.Object.Instantiate(__instance.WinText.gameObject);
            bonusTextObject.transform.position = new Vector3(__instance.WinText.transform.position.x, __instance.WinText.transform.position.y - 0.8f, __instance.WinText.transform.position.z);
            bonusTextObject.transform.localScale = new Vector3(0.7f, 0.7f, 1f);
            textRenderer = bonusTextObject.GetComponent<TMPro.TMP_Text>();
            textRenderer.text = "";
            var text = "";
            switch (AdditionalTempData.winCondition)
            {
                case WinCondition.LoversWin:
                    text = "LoversName";
                    textRenderer.color = RoleClass.Lovers.color;
                    __instance.BackgroundBar.material.SetColor("_Color", RoleClass.Lovers.color);
                    break;
                case WinCondition.GodWin:
                    text = "GodName";
                    textRenderer.color = RoleClass.God.color;
                    __instance.BackgroundBar.material.SetColor("_Color", Roles.RoleClass.God.color);
                    break;
                case WinCondition.HAISON:
                    text = "HAISON";
                    textRenderer.color = Color.white;
                    __instance.BackgroundBar.material.SetColor("_Color", Color.white);
                    break;
                case WinCondition.JesterWin:
                    text = "JesterName";
                    textRenderer.color = Roles.RoleClass.Jester.color;
                    __instance.BackgroundBar.material.SetColor("_Color", Roles.RoleClass.Jester.color);
                    break;
                case WinCondition.JackalWin:
                    text = "JackalName";
                    textRenderer.color = Roles.RoleClass.Jackal.color;
                    __instance.BackgroundBar.material.SetColor("_Color", Roles.RoleClass.Jackal.color);
                    break;
                case WinCondition.QuarreledWin:
                    text = "QuarreledName";
                    textRenderer.color = Roles.RoleClass.Quarreled.color;
                    __instance.BackgroundBar.material.SetColor("_Color", Roles.RoleClass.Quarreled.color);
                    break;
                case WinCondition.EgoistWin:
                    text = "EgoistName";
                    textRenderer.color = Roles.RoleClass.Egoist.color;
                    __instance.BackgroundBar.material.SetColor("_Color", Roles.RoleClass.Egoist.color);
                    break;
                case WinCondition.WorkpersonWin:
                    text = "WorkpersonName";
                    textRenderer.color = RoleClass.Workperson.color;
                    __instance.BackgroundBar.material.SetColor("_Color", RoleClass.Workperson.color);
                    break;
                case WinCondition.MadJesterWin:
                    text = "MadJesterName";
                    textRenderer.color = RoleClass.Workperson.color;
                    __instance.BackgroundBar.material.SetColor("_Color", RoleClass.MadJester.color);
                    break;
                case WinCondition.FalseChargesWin:
                    text = "FalseChargesName";
                    textRenderer.color = RoleClass.FalseCharges.color;
                    __instance.BackgroundBar.material.SetColor("_Color", RoleClass.FalseCharges.color);
                    break;
                case WinCondition.FoxWin:
                    text = "FoxName";
                    textRenderer.color = RoleClass.Fox.color;
                    __instance.BackgroundBar.material.SetColor("_Color", Roles.RoleClass.Fox.color);
                    break;
                case WinCondition.DemonWin:
                    text = "DemonName";
                    textRenderer.color = RoleClass.Demon.color;
                    __instance.BackgroundBar.material.SetColor("_Color", RoleClass.Demon.color);
                    break;
                case WinCondition.ArsonistWin:
                    text = "ArsonistName";
                    textRenderer.color = RoleClass.Arsonist.color;
                    __instance.BackgroundBar.material.SetColor("_Color", RoleClass.Arsonist.color);
                    break;
                case WinCondition.VultureWin:
                    text = "VultureName";
                    textRenderer.color = RoleClass.Vulture.color;
                    __instance.BackgroundBar.material.SetColor("_Color", RoleClass.Vulture.color);
                    break;
                default:
                    switch (AdditionalTempData.gameOverReason)
                    {
                        case GameOverReason.HumansByTask:
                        case GameOverReason.HumansByVote:
                        case GameOverReason.HumansDisconnect:
                            text = "CrewMateName";
                            textRenderer.color = Palette.White;
                            break;
                        case GameOverReason.ImpostorByKill:
                        case GameOverReason.ImpostorBySabotage:
                        case GameOverReason.ImpostorByVote:
                        case GameOverReason.ImpostorDisconnect:
                            text = "ImpostorName";
                            textRenderer.color = RoleClass.ImpostorRed;
                            break;
                    }
                    break;
            }
            var haison = false;
            if (text == "HAISON")
            {
                haison = true;
                text = ModTranslation.getString("HaisonName");
            }
            else
            {
                text = ModTranslation.getString(text);
            }
            bool IsOpptexton = false;
            foreach (PlayerControl player in RoleClass.Opportunist.OpportunistPlayer)
            {
                if (player.isAlive())
                {
                    if (!IsOpptexton && !haison)
                    {
                        IsOpptexton = true;
                        text = text + "&" + ModHelpers.cs(RoleClass.Opportunist.color, ModTranslation.getString("OpportunistName"));
                    }
                }
            }
            bool IsLovetexton = false;
            bool Temp1;
            if (!CustomOptions.LoversSingleTeam.getBool())
            {
                foreach (List<PlayerControl> PlayerList in RoleClass.Lovers.LoversPlayer)
                {
                    Temp1 = false;
                    foreach (PlayerControl player in PlayerList)
                    {
                        if (player.isAlive())
                        {
                            Temp1 = true;
                        }
                        if (Temp1)
                        {
                            if (!IsLovetexton && !haison)
                            {
                                IsLovetexton = true;
                                text = text + "&" + CustomOptions.cs(RoleClass.Lovers.color, "LoversName");
                            }
                        }
                    }
                }
            }
            if (ModeHandler.isMode(ModeId.Zombie))
            {
                if (AdditionalTempData.winCondition == WinCondition.Default)
                {
                    text = ModTranslation.getString("ZombieZombieName");
                    textRenderer.color = Mode.Zombie.main.Zombiecolor;
                }
                else if (AdditionalTempData.winCondition == WinCondition.WorkpersonWin)
                {
                    text = ModTranslation.getString("ZombiePoliceName");
                    textRenderer.color = Mode.Zombie.main.Policecolor;
                }
            }
            else if (ModeHandler.isMode(ModeId.BattleRoyal))
            {
                foreach (PlayerControl p in CachedPlayer.AllPlayers)
                {
                    if (p.isAlive())
                    {
                        text = p.nameText.text;
                        textRenderer.color = new Color32(116, 80, 48, byte.MaxValue);
                    }
                }
            }
            if (!haison)
            {
                textRenderer.text = string.Format(text + " " + ModTranslation.getString("WinName"));
            }
            else
            {
                textRenderer.text = text;
            }
            try
            {
                if (true)
                {
                    var position = Camera.main.ViewportToWorldPoint(new Vector3(0f, 1f, Camera.main.nearClipPlane));
                    GameObject roleSummary = UnityEngine.Object.Instantiate(__instance.WinText.gameObject);
                    roleSummary.transform.position = new Vector3(__instance.Navigation.ExitButton.transform.position.x + 0.1f, position.y - 0.1f, -14f);
                    roleSummary.transform.localScale = new Vector3(1f, 1f, 1f);

                    var roleSummaryText = new StringBuilder();
                    roleSummaryText.AppendLine(ModTranslation.getString("最終結果"));

                    foreach (var datas in AdditionalTempData.playerRoles)
                    {
                        var taskInfo = datas.TasksTotal > 0 ? $"<color=#FAD934FF>({datas.TasksCompleted}/{datas.TasksTotal})</color>" : "";
                        string aliveDead = "";
                        string Suffix = "";
                        string roleText = CustomOptions.cs(datas.IntroDate.color, datas.IntroDate.NameKey + "Name");
                        if (datas.GhostIntroDate.RoleId != RoleId.DefaultRole)
                        {
                            roleText += $" → {CustomOptions.cs(datas.GhostIntroDate.color, datas.GhostIntroDate.NameKey + "Name")}";
                        }
                        string result = $"{ModHelpers.cs(Palette.PlayerColors[datas.ColorId], datas.PlayerName)}{datas.NameSuffix}{taskInfo} - {GetStatusText(datas.Status)} - {roleText}";
                        if (ModeHandler.isMode(ModeId.Zombie))
                        {
                            roleText = datas.ColorId == 1 ? CustomOptions.cs(Mode.Zombie.main.Policecolor, "ZombiePoliceName") : CustomOptions.cs(Mode.Zombie.main.Zombiecolor, "ZombieZombieName");
                            if (datas.ColorId == 2) taskInfo = "";
                            result = $"{ModHelpers.cs(Palette.PlayerColors[datas.ColorId], datas.PlayerName)}{taskInfo} : {roleText}";
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
            }
            catch (Exception e)
            {
                SuperNewRolesPlugin.Logger.LogInfo("エラー:" + e);
            }
            AdditionalTempData.clear();

            static string GetStatusText(FinalStatus status)
            {
                if (status == FinalStatus.Alive)
                {
                    return ModTranslation.getString("FinalStatusAlive");
                }
                else if (status == FinalStatus.Kill)
                {
                    return ModTranslation.getString("FinalStatusKill");
                }
                else if (status == FinalStatus.NekomataExiled)
                {
                    return ModTranslation.getString("FinalStatusNekomataExiled");
                }
                else if (status == FinalStatus.SheriffKill)
                {
                    return ModTranslation.getString("FinalStatusSheriffKill");
                }
                else if (status == FinalStatus.SheriffMisFire)
                {
                    return ModTranslation.getString("FinalStatusSheriffMisFire");
                }
                else if (status == FinalStatus.MeetingSheriffKill)
                {
                    return ModTranslation.getString("FinalStatusMeetingSheriffMisFire");
                }
                else if (status == FinalStatus.MeetingSheriffMisFire)
                {
                    return ModTranslation.getString("FinalStatusMeetingSheriffMisFire");
                }
                else if (status == FinalStatus.SelfBomb)
                {
                    return ModTranslation.getString("FinalStatusSelfBomb");
                }
                else if (status == FinalStatus.BySelfBomb)
                {
                    return ModTranslation.getString("FinalStatusBySelfBomb");
                }
                else if (status == FinalStatus.Ignite)
                {
                    return ModTranslation.getString("FinalStatusIgnite");
                }
                else if (status == FinalStatus.Disconnected)
                {
                    return ModTranslation.getString("FinalStatusDisconnected");
                }
                else if (status == FinalStatus.Dead)
                {
                    return ModTranslation.getString("FinalStatusDead");
                }
                else if (status == FinalStatus.Sabotage)
                {
                    return ModTranslation.getString("FinalStatusSabotage");
                }
                return ModTranslation.getString("FinalStatusAlive");
            }
            IsHaison = false;
        }
    }

    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameEnd))]
    public class OnGameEndPatch
    {
        public static PlayerControl WinnerPlayer;
        public static CustomGameOverReason? EndData = null;
        public static void Prefix(AmongUsClient __instance, [HarmonyArgument(0)] ref EndGameResult endGameResult)
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

        public static void Postfix(AmongUsClient __instance, [HarmonyArgument(0)] ref EndGameResult endGameResult)
        {
            if (AmongUsClient.Instance.AmHost && (ModeHandler.isMode(ModeId.SuperHostRoles) || ModeHandler.isMode(ModeId.Zombie)))
            {
                PlayerControl.GameOptions = SyncSetting.OptionData.DeepCopy();
                CachedPlayer.LocalPlayer.PlayerControl.RpcSyncSettings(PlayerControl.GameOptions);
            }
            var gameOverReason = AdditionalTempData.gameOverReason;
            AdditionalTempData.clear();

            foreach (var p in GameData.Instance.AllPlayers.GetFastEnumerator())
            {
                if (p.Object.IsPlayer())
                {
                    //var p = pc.Data;
                    var roles = Intro.IntroDate.GetIntroDate(p.Object.getRole(), p.Object);
                    var ghostRoles = Intro.IntroDate.GetIntroDate(p.Object.getGhostRole(), p.Object);
                    var (tasksCompleted, tasksTotal) = TaskCount.TaskDate(p);
                    if (p.Object.isImpostor())
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
                        namesuffix = ModHelpers.cs(RoleClass.Lovers.color, " ♥");
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
            List<PlayerControl> notWinners = new List<PlayerControl>();

            notWinners.AddRange(RoleClass.Jester.JesterPlayer);
            notWinners.AddRange(RoleClass.MadMate.MadMatePlayer);
            notWinners.AddRange(RoleClass.Jackal.JackalPlayer);
            notWinners.AddRange(RoleClass.Jackal.SidekickPlayer);
            notWinners.AddRange(RoleClass.JackalFriends.JackalFriendsPlayer);
            notWinners.AddRange(RoleClass.God.GodPlayer);
            notWinners.AddRange(RoleClass.Opportunist.OpportunistPlayer);
            notWinners.AddRange(RoleClass.truelover.trueloverPlayer);
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

            foreach (PlayerControl p in RoleClass.Survivor.SurvivorPlayer)
            {
                if (p.isDead())
                {
                    notWinners.Add(p);
                }
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
            bool BUGEND = gameOverReason == (GameOverReason)CustomGameOverReason.BugEnd;
            if (ModeHandler.isMode(ModeId.SuperHostRoles) && EndData != null)
            {
                JesterWin = EndData == CustomGameOverReason.JesterWin;
                EgoistWin = EndData == CustomGameOverReason.EgoistWin;
                WorkpersonWin = EndData == CustomGameOverReason.WorkpersonWin;
                FalseChargesWin = EndData == CustomGameOverReason.FalseChargesWin;
                QuarreledWin = EndData == CustomGameOverReason.QuarreledWin;
                FoxWin = EndData == CustomGameOverReason.FoxWin;
                JackalWin = EndData == CustomGameOverReason.JackalWin;
                DemonWin = EndData == CustomGameOverReason.DemonWin;
                ArsonistWin = EndData == CustomGameOverReason.ArsonistWin;
                VultureWin = EndData == CustomGameOverReason.VultureWin;
            }


            if (JesterWin)
            {
                TempData.winners = new Il2CppSystem.Collections.Generic.List<WinningPlayerData>();
                WinnerPlayer.Data.IsDead = false;
                WinningPlayerData wpd = new WinningPlayerData(WinnerPlayer.Data);
                TempData.winners.Add(wpd);
                AdditionalTempData.winCondition = WinCondition.JesterWin;
            }
            else if (MadJesterWin)
            {
                TempData.winners = new Il2CppSystem.Collections.Generic.List<WinningPlayerData>();
                WinnerPlayer.Data.IsDead = false;
                WinningPlayerData wpd = new WinningPlayerData(WinnerPlayer.Data);
                TempData.winners.Add(wpd);
                AdditionalTempData.winCondition = WinCondition.MadJesterWin;
            }
            else if (JackalWin)
            {
                TempData.winners = new Il2CppSystem.Collections.Generic.List<WinningPlayerData>();
                foreach (PlayerControl p in RoleClass.Jackal.JackalPlayer)
                {
                    WinningPlayerData wpd = new WinningPlayerData(p.Data);
                    TempData.winners.Add(wpd);
                }
                foreach (PlayerControl p in RoleClass.Jackal.SidekickPlayer)
                {
                    WinningPlayerData wpd = new WinningPlayerData(p.Data);
                    TempData.winners.Add(wpd);
                }
                foreach (PlayerControl p in RoleClass.JackalFriends.JackalFriendsPlayer)
                {
                    WinningPlayerData wpd = new WinningPlayerData(p.Data);
                    TempData.winners.Add(wpd);
                }
                foreach (PlayerControl p in RoleClass.SeerFriends.SeerFriendsPlayer)
                {
                    WinningPlayerData wpd = new WinningPlayerData(p.Data);
                    TempData.winners.Add(wpd);
                }
                foreach (PlayerControl p in RoleClass.TeleportingJackal.TeleportingJackalPlayer)
                {
                    WinningPlayerData wpd = new WinningPlayerData(p.Data);
                    TempData.winners.Add(wpd);
                }
                foreach (PlayerControl p in RoleClass.JackalSeer.JackalSeerPlayer)
                {
                    WinningPlayerData wpd = new WinningPlayerData(p.Data);
                    TempData.winners.Add(wpd);
                }
                foreach (PlayerControl p in RoleClass.JackalSeer.SidekickSeerPlayer)
                {
                    WinningPlayerData wpd = new WinningPlayerData(p.Data);
                    TempData.winners.Add(wpd);
                }
                foreach (PlayerControl p in RoleClass.MayorFriends.MayorFriendsPlayer)
                {
                    WinningPlayerData wpd = new WinningPlayerData(p.Data);
                    TempData.winners.Add(wpd);
                }

                AdditionalTempData.winCondition = WinCondition.JackalWin;
            }
            else if (EgoistWin)
            {
                TempData.winners = new Il2CppSystem.Collections.Generic.List<WinningPlayerData>();
                foreach (PlayerControl p in RoleClass.Egoist.EgoistPlayer)
                {
                    if (p.isAlive())
                    {
                        WinningPlayerData wpd = new WinningPlayerData(p.Data);
                        TempData.winners.Add(wpd);
                    }
                }
                AdditionalTempData.winCondition = WinCondition.EgoistWin;
            }
            else if (WorkpersonWin)
            {
                TempData.winners = new Il2CppSystem.Collections.Generic.List<WinningPlayerData>();
                WinningPlayerData wpd = new WinningPlayerData(WinnerPlayer.Data);
                TempData.winners.Add(wpd);
                AdditionalTempData.winCondition = WinCondition.WorkpersonWin;
            }
            else if (FalseChargesWin)
            {
                TempData.winners = new Il2CppSystem.Collections.Generic.List<WinningPlayerData>();
                WinningPlayerData wpd = new WinningPlayerData(WinnerPlayer.Data);
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
                        WinningPlayerData wpd = new WinningPlayerData(player.Data);
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
                        WinningPlayerData wpd = new WinningPlayerData(player.Data);
                        TempData.winners.Add(wpd);
                    }
                }
                AdditionalTempData.winCondition = WinCondition.ArsonistWin;
            }
            else if (VultureWin)
            {
                TempData.winners = new Il2CppSystem.Collections.Generic.List<WinningPlayerData>();
                WinningPlayerData wpd = new WinningPlayerData(WinnerPlayer.Data);
                TempData.winners.Add(wpd);
                AdditionalTempData.winCondition = WinCondition.VultureWin;
            }

            if (TempData.winners.GetFastEnumerator().ToArray().Any(x => x.IsImpostor))
            {
                foreach (PlayerControl p in RoleClass.MadMate.MadMatePlayer)
                {
                    WinningPlayerData wpd = new WinningPlayerData(p.Data);
                    TempData.winners.Add(wpd);
                }
                foreach (PlayerControl p in RoleClass.SideKiller.MadKillerPlayer)
                {
                    WinningPlayerData wpd = new WinningPlayerData(p.Data);
                    TempData.winners.Add(wpd);
                }
                foreach (PlayerControl p in RoleClass.MadMayor.MadMayorPlayer)
                {
                    WinningPlayerData wpd = new WinningPlayerData(p.Data);
                    TempData.winners.Add(wpd);
                }
                foreach (PlayerControl p in RoleClass.MadStuntMan.MadStuntManPlayer)
                {
                    WinningPlayerData wpd = new WinningPlayerData(p.Data);
                    TempData.winners.Add(wpd);
                }
                foreach (PlayerControl p in RoleClass.MadJester.MadJesterPlayer)
                {
                    WinningPlayerData wpd = new WinningPlayerData(p.Data);
                    TempData.winners.Add(wpd);
                }
                foreach (PlayerControl p in RoleClass.MadSeer.MadSeerPlayer)
                {
                    WinningPlayerData wpd = new WinningPlayerData(p.Data);
                    TempData.winners.Add(wpd);
                }
                foreach (PlayerControl p in RoleClass.MadMaker.MadMakerPlayer)
                {
                    WinningPlayerData wpd = new WinningPlayerData(p.Data);
                    TempData.winners.Add(wpd);
                }
                foreach (PlayerControl p in RoleClass.MadHawk.MadHawkPlayer)
                {
                    WinningPlayerData wpd = new WinningPlayerData(p.Data);
                    TempData.winners.Add(wpd);
                }
                foreach (PlayerControl p in RoleClass.MadCleaner.MadCleanerPlayer)
                {
                    WinningPlayerData wpd = new WinningPlayerData(p.Data);
                    TempData.winners.Add(wpd);
                }
            }

            if (ModeHandler.isMode(ModeId.BattleRoyal))
            {
                TempData.winners = new Il2CppSystem.Collections.Generic.List<WinningPlayerData>();
                if (Mode.BattleRoyal.main.IsTeamBattle)
                {
                    foreach (PlayerControl p in Mode.BattleRoyal.main.Winners)
                    {
                        WinningPlayerData wpd = new WinningPlayerData(p.Data);
                        TempData.winners.Add(wpd);
                    }
                }
                else
                {
                    foreach (PlayerControl p in CachedPlayer.AllPlayers)
                    {
                        if (p.isAlive())
                        {
                            WinningPlayerData wpd = new WinningPlayerData(p.Data);
                            TempData.winners.Add(wpd);
                        }
                    }
                }
                AdditionalTempData.winCondition = WinCondition.Default;
            }
            var godalive = false;
            foreach (PlayerControl p in RoleClass.God.GodPlayer)
            {
                if (p.isAlive())
                {
                    var (complate, all) = TaskCount.TaskDateNoClearCheck(p.Data);
                    if (!RoleClass.God.IsTaskEndWin || complate >= all)
                    {
                        godalive = true;
                        TempData.winners = new Il2CppSystem.Collections.Generic.List<WinningPlayerData>();
                        WinningPlayerData wpd = new WinningPlayerData(p.Data);
                        TempData.winners.Add(wpd);
                        AdditionalTempData.winCondition = WinCondition.GodWin;
                    }
                }
            }
            var Foxalive = false;
            foreach (PlayerControl p in RoleClass.Fox.FoxPlayer)
            {
                if (p.isAlive())
                {
                    Foxalive = true;
                    TempData.winners = new Il2CppSystem.Collections.Generic.List<WinningPlayerData>();
                    WinningPlayerData wpd = new WinningPlayerData(p.Data);
                    TempData.winners.Add(wpd);
                    AdditionalTempData.winCondition = WinCondition.FoxWin;

                }
            }

            foreach (PlayerControl player in RoleClass.Opportunist.OpportunistPlayer)
            {
                if (player.isAlive())
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

            notWinners = new List<PlayerControl>();
            winnersToRemove = new List<WinningPlayerData>();
            foreach (WinningPlayerData winner in TempData.winners)
            {
                if (notWinners.Any(x => x.Data.PlayerName == winner.PlayerName)) winnersToRemove.Add(winner);
            }
            foreach (var winner in winnersToRemove) TempData.winners.Remove(winner);

            if (QuarreledWin)
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
            else if (BUGEND)
            {
                TempData.winners = new Il2CppSystem.Collections.Generic.List<WinningPlayerData>();
                foreach (PlayerControl p in CachedPlayer.AllPlayers)
                {
                    if (p.isImpostor() || p.isRole(CustomRPC.RoleId.Jackal) || RoleClass.Jackal.SidekickPlayer.IsCheckListPlayerControl(p) || p.isRole(CustomRPC.RoleId.JackalFriends) || p.isRole(CustomRPC.RoleId.SeerFriends) || p.isRole(CustomRPC.RoleId.MayorFriends))
                    {
                        WinningPlayerData wpd = new WinningPlayerData(p.Data);
                        TempData.winners.Add(wpd);
                    }
                }
                AdditionalTempData.winCondition = WinCondition.BugEnd;
            }
            bool IsSingleTeam = CustomOptions.LoversSingleTeam.getBool();
            foreach (List<PlayerControl> plist in RoleClass.Lovers.LoversPlayer)
            {
                bool IsWinLovers = false;
                foreach (PlayerControl p in plist)
                {
                    if (p.isAlive())
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
                    if (p.isAlive())
                    {
                        IsWinLovers = true;
                    }
                }
                if (IsWinLovers)
                {
                    foreach (PlayerControl p in plist)
                    {
                        WinningPlayerData wpd = new WinningPlayerData(p.Data);
                        TempData.winners.Add(wpd);
                        if (IsSingleTeam)
                        {
                        }
                    }
                }
            }
            if (ModeHandler.isMode(ModeId.Zombie))
            {
                TempData.winners = new Il2CppSystem.Collections.Generic.List<WinningPlayerData>();
                if (gameOverReason == GameOverReason.ImpostorByKill)
                {
                    AdditionalTempData.winCondition = WinCondition.Default;
                    foreach (PlayerControl p in CachedPlayer.AllPlayers)
                    {
                        if (p.CurrentOutfit.ColorId == 2)
                        {
                            WinningPlayerData wpd = new WinningPlayerData(p.Data);
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
                            WinningPlayerData wpd = new WinningPlayerData(p.Data);
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
                        WinningPlayerData wpd = new WinningPlayerData(p.Data);
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
                if (ExileController.Instance != null && ExileController.Instance.exiled != null && ModeHandler.isMode(ModeId.Default))
                {
                    PlayerControl player = ModHelpers.playerById(ExileController.Instance.exiled.Object.PlayerId);
                    if (player == null) return;
                    FinalStatusPatch.FinalStatusData.FinalStatuses[player.PlayerId] = FinalStatus.Exiled;
                    // Exile role text
                    if (id == StringNames.ExileTextPN || id == StringNames.ExileTextSN || id == StringNames.ExileTextPP || id == StringNames.ExileTextSP)
                    {
                        __result = player.Data.PlayerName + " は " + ModTranslation.getString(Intro.IntroDate.GetIntroDate(player.getRole(), player).NameKey + "Name") + " だった！";
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
            if (!RoleManagerSelectRolesPatch.IsSetRoleRpc) return false;
            if (Patch.DebugMode.IsDebugMode()) return false;
            if (RoleClass.Assassin.TriggerPlayer != null) return false;
            var statistics = new PlayerStatistics(__instance);
            if (!ModeHandler.isMode(ModeId.Default))
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
                if (!PlusModeHandler.isMode(PlusModeId.NotTaskWin) && CheckAndEndGameForTaskWin(__instance)) return false;
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
            if (statistics.TeamImpostorsAlive >= statistics.TotalAlive - statistics.TeamImpostorsAlive && statistics.TeamJackalAlive == 0 && !EvilEraser.IsGodWinGuard() && !EvilEraser.IsFoxWinGuard())
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
            GameOverReason endReason;
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
                    if (!p.isImpostor() && !p.Data.Disconnected)
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
                    if (!p.isImpostor() && !p.Data.Disconnected)
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
                    if (p.isAlive() || !RoleClass.Workperson.IsAliveWin)
                    {
                        var (playerCompleted, playerTotal) = TaskCount.TaskDate(p.Data);
                        if (playerCompleted >= playerTotal)
                        {
                            MessageWriter Writer = AmongUsClient.Instance.StartRpcImmediately(CachedPlayer.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.ShareWinner, Hazel.SendOption.Reliable, -1);
                            Writer.Write(p.PlayerId);
                            AmongUsClient.Instance.FinishRpcImmediately(Writer);
                            CustomRPC.RPCProcedure.ShareWinner(p.PlayerId);
                            __instance.enabled = false;
                            CustomEndGame((GameOverReason)CustomGameOverReason.WorkpersonWin, false);
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
                        if (playerInfo.Object.isAlive())
                        {
                            numTotalAlive++;
                            if (playerInfo.Object.isRole(RoleId.Jackal) || playerInfo.Object.isRole(CustomRPC.RoleId.Sidekick) || playerInfo.Object.isRole(CustomRPC.RoleId.TeleportingJackal) || playerInfo.Object.isRole(CustomRPC.RoleId.JackalSeer) || playerInfo.Object.isRole(CustomRPC.RoleId.SidekickSeer))
                            {
                                numTotalJackalTeam++;
                            }
                            else if (playerInfo.Object.isImpostor())
                            {
                                numImpostorsAlive++;
                            }
                            else if (!playerInfo.Object.isCrew())
                            {
                                numCrewAlive++;
                            }
                            else if (playerInfo.Object.isNeutral())
                            {
                                if (playerInfo.Object.isRole(CustomRPC.RoleId.Egoist))
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
