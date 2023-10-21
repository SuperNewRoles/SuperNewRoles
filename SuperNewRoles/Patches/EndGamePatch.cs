using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using Hazel;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using SuperNewRoles.Helpers;
using SuperNewRoles.Mode;
using SuperNewRoles.Mode.SuperHostRoles;
using SuperNewRoles.Replay;
using SuperNewRoles.Roles;
using SuperNewRoles.Roles.Neutral;
using SuperNewRoles.SuperNewRolesWeb;
using UnityEngine;
using static SuperNewRoles.Patches.CheckGameEndPatch;

namespace SuperNewRoles.Patches;

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
    HitmanWin,
    PhotographerWin,
    StefinderWin,
    PavlovsTeamWin,
    TaskerWin,
    LoversBreakerWin,
    NoWinner,
    BugEnd,
    SafecrackerWin,
    TheThreeLittlePigsWin,
    OrientalShamanWin,
    BlackHatHackerWin,
    MoiraWin,
    SaunerWin,
    CrookWin,
    FrankensteinWin,
}
public enum WinCondition
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
    HitmanWin,
    PhotographerWin,
    StefinderWin,
    PavlovsTeamWin,
    TaskerWin,
    LoversBreakerWin,
    NoWinner,
    BugEnd,
    SafecrackerWin,
    TheThreeLittlePigsWin,
    OrientalShamanWin,
    BlackHatHackerWin,
    MoiraWin,
    PantsRoyalWin,
    SaunerWin,
    PokerfaceWin,
    CrookWin,
    FrankensteinWin,
}
class FinalStatusPatch
{
    public static class FinalStatusData
    {
        public static List<Tuple<Vector3, bool>> localPlayerPositions = new();
        public static List<DeadPlayer> deadPlayers = new();
        public static Dictionary<int, FinalStatus> FinalStatuses = new();

        public static void ClearFinalStatusData()
        {
            localPlayerPositions = new List<Tuple<Vector3, bool>>();
            deadPlayers = new List<DeadPlayer>();
            FinalStatuses = new Dictionary<int, FinalStatus>();
        }
    }
    public static string GetStatusText(FinalStatus status) => ModTranslation.GetString("FinalStatus" + status.ToString()); //ローカル関数

}
[HarmonyPatch(typeof(ShipStatus))]
public static class ShipStatusPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(LogicGameFlowNormal), nameof(LogicGameFlowNormal.IsGameOverDueToDeath))]
    public static void Postfix2(ref bool __result)
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
        public List<IntroData> Roles { get; set; }
        public string RoleString { get; set; }
        public int TasksCompleted { get; set; }
        public int TasksTotal { get; set; }
        public int PlayerId { get; set; }
        public int ColorId { get; set; }
        public FinalStatus Status { get; internal set; }
        public IntroData IntroData { get; set; }
        public IntroData GhostIntroData { get; set; }
        public string AttributeRoleName { get; set; }
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
        List<WinningPlayerData> list = TempData.winners.ToList().OrderBy(delegate (WinningPlayerData b)
        {
            return !b.IsYou ? 0 : -1;
        }).ToList();
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
            if (winningPlayerData2.IsDead)
            {
                poolablePlayer.SetBodyAsGhost();
                poolablePlayer.SetDeadFlipX(i % 2 == 0);
            }
            else
            {
                poolablePlayer.SetFlipX(i % 2 == 0);
            }
            poolablePlayer.UpdateFromPlayerOutfit((GameData.PlayerOutfit)winningPlayerData2, PlayerMaterial.MaskType.None, winningPlayerData2.IsDead, true);
            poolablePlayer.cosmetics.nameText.color = Color.white;
            poolablePlayer.cosmetics.nameText.transform.localScale = new Vector3(1f / vector.x, 1f / vector.y, 1f / vector.z);
            poolablePlayer.cosmetics.nameText.transform.localPosition = new Vector3(poolablePlayer.cosmetics.nameText.transform.localPosition.x, poolablePlayer.cosmetics.nameText.transform.localPosition.y - 0.8f, -15f);
            poolablePlayer.cosmetics.nameText.text = winningPlayerData2.PlayerName;

            foreach (var data in AdditionalTempData.playerRoles)
            {
                Logger.Info(data.PlayerName + ":" + winningPlayerData2.PlayerName);
                if (data.PlayerName != winningPlayerData2.PlayerName) continue;
                poolablePlayer.cosmetics.nameText.text = $"{data.PlayerName}{data.NameSuffix}\n{string.Join("\n", ModHelpers.Cs(data.IntroData.color, data.IntroData.Name))}";
            }
        }

        GameObject bonusTextObject = UnityEngine.Object.Instantiate(__instance.WinText.gameObject);
        bonusTextObject.transform.position = new Vector3(__instance.WinText.transform.position.x, __instance.WinText.transform.position.y - 0.8f, __instance.WinText.transform.position.z);
        bonusTextObject.transform.localScale = new Vector3(0.7f, 0.7f, 1f);
        textRenderer = bonusTextObject.GetComponent<TMPro.TMP_Text>();
        textRenderer.text = "";
        var text = "";
        var RoleColor = Color.white;
        Color32 HaisonColor = new(163, 163, 162, byte.MaxValue);
        Dictionary<WinCondition, (string, Color32)> WinConditionDictionary = new() {
                {WinCondition.HAISON,("HAISON",HaisonColor)},
                {WinCondition.LoversWin,("LoversName",RoleClass.Lovers.color)},
                {WinCondition.GodWin,("GodName",RoleClass.God.color)},
                {WinCondition.JesterWin,("JesterName",RoleClass.Jester.color)},
                {WinCondition.JackalWin,("JackalName",RoleClass.Jackal.color)},
                {WinCondition.QuarreledWin,("QuarreledName",RoleClass.Quarreled.color)},
                {WinCondition.EgoistWin,("EgoistName",RoleClass.Egoist.color)},
                {WinCondition.WorkpersonWin,("WorkpersonName",RoleClass.Workperson.color)},
                {WinCondition.FalseChargesWin,("FalseChargesName",RoleClass.FalseCharges.color)},
                {WinCondition.FoxWin,("FoxName",RoleClass.Fox.color)},
                {WinCondition.DemonWin,("DemonName",RoleClass.Demon.color)},
                {WinCondition.ArsonistWin,("ArsonistName",RoleClass.Arsonist.color)},
                {WinCondition.VultureWin,("VultureName",RoleClass.Vulture.color)},
                {WinCondition.TunaWin,("TunaName",RoleClass.Tuna.color)},
                {WinCondition.NeetWin,("NeetName",RoleClass.Neet.color)},
                {WinCondition.RevolutionistWin,("RevolutionistName",RoleClass.Revolutionist.color)},
                {WinCondition.SpelunkerWin,("SpelunkerName",RoleClass.Spelunker.color)},
                {WinCondition.SuicidalIdeationWin,(CustomOptionHolder.SuicidalIdeationWinText.GetBool() ? "SuicidalIdeationWinText" : "SuicidalIdeationName",RoleClass.SuicidalIdeation.color)},
                {WinCondition.HitmanWin,("HitmanName",RoleClass.Hitman.color)},
                {WinCondition.PhotographerWin,("PhotographerName",RoleClass.Photographer.color)},
                {WinCondition.StefinderWin,("StefinderName",RoleClass.Stefinder.color)},
                {WinCondition.PavlovsTeamWin,("PavlovsTeamWinText",RoleClass.Pavlovsdogs.color)},
                {WinCondition.LoversBreakerWin,("LoversBreakerName",RoleClass.LoversBreaker.color)},
                {WinCondition.NoWinner,("NoWinner",Color.white)},
                {WinCondition.SafecrackerWin,("SafecrackerName",Safecracker.color)},
                {WinCondition.TheThreeLittlePigsWin,("TheThreeLittlePigsName",TheThreeLittlePigs.color)},
                {WinCondition.OrientalShamanWin,("OrientalShamanName", OrientalShaman.color)},
                {WinCondition.BlackHatHackerWin,("BlackHatHackerName",BlackHatHacker.color)},
                {WinCondition.MoiraWin,("MoiraName",Moira.color)},
                {WinCondition.CrookWin,("CrookName",Crook.RoleData.color)},
                {WinCondition.PantsRoyalWin,("PantsRoyalYouareWinner",Mode.PantsRoyal.main.ModeColor) },
                {WinCondition.SaunerWin, ("SaunerRefreshing",Sauner.RoleData.color) },
                {WinCondition.PokerfaceWin,("PokerfaceName",Pokerface.RoleData.color) },
                {WinCondition.FrankensteinWin, ("FrankensteinName",Frankenstein.color) },
            };
        Logger.Info(AdditionalTempData.winCondition.ToString(), "WINCOND");
        if (WinConditionDictionary.ContainsKey(AdditionalTempData.winCondition))
        {
            text = WinConditionDictionary[AdditionalTempData.winCondition].Item1;
            RoleColor = WinConditionDictionary[AdditionalTempData.winCondition].Item2;
        }
        else
        {
            switch (AdditionalTempData.gameOverReason)
            {
                case GameOverReason.HumansByTask:
                case GameOverReason.HumansByVote:
                case GameOverReason.HumansDisconnect:
                    text = "CrewmateName";
                    RoleColor = Palette.White;
                    break;
                case GameOverReason.ImpostorByKill:
                case GameOverReason.ImpostorBySabotage:
                case GameOverReason.ImpostorByVote:
                case GameOverReason.ImpostorDisconnect:
                //MadJester勝利をインポスター勝利とみなす
                case (GameOverReason)CustomGameOverReason.MadJesterWin:
                    text = "ImpostorName";
                    RoleColor = RoleClass.ImpostorRed;
                    break;
                case (GameOverReason)CustomGameOverReason.TaskerWin:
                    text = "TaskerWinText";
                    RoleColor = RoleClass.ImpostorRed;
                    break;
            }
        }
        if (AdditionalTempData.winCondition == WinCondition.HAISON)
        {
            __instance.WinText.text = ModTranslation.GetString("HaisonName");
            __instance.WinText.color = HaisonColor;
        }
        else if (AdditionalTempData.winCondition == WinCondition.NoWinner)
        {
            __instance.WinText.text = ModTranslation.GetString("NoWinner");
            __instance.WinText.color = Color.white;
            RoleColor = Color.white;
        }

        textRenderer.color = AdditionalTempData.winCondition is WinCondition.HAISON ? Color.clear : RoleColor;

        __instance.BackgroundBar.material.SetColor("_Color", RoleColor);
        var haison = false;
        if (text == "HAISON")
        {
            haison = true;
            text = ModTranslation.GetString("HaisonName");
        }
        else if (text is "NoWinner")
        {
            haison = true;
            text = ModTranslation.GetString("NoWinner");
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
        if (!CustomOptionHolder.LoversSingleTeam.GetBool())
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
                            text = text + "&" + CustomOptionHolder.Cs(RoleClass.Lovers.color, "LoversName");
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
        Logger.Info("WINCOND:" + AdditionalTempData.winCondition.ToString());
        if (haison || AdditionalTempData.winCondition is WinCondition.PantsRoyalWin or WinCondition.SaunerWin) textRenderer.text = text;
        else if (text == ModTranslation.GetString("NoWinner")) textRenderer.text = ModTranslation.GetString("NoWinnerText");
        else if (text == ModTranslation.GetString("GodName")) textRenderer.text = text + " " + ModTranslation.GetString("GodWinText");
        else textRenderer.text = string.Format(text + " " + ModTranslation.GetString("WinName"));
        try
        {
            var position = Camera.main.ViewportToWorldPoint(new Vector3(0f, 1f, Camera.main.nearClipPlane));
            GameObject roleSummary = UnityEngine.Object.Instantiate(__instance.WinText.gameObject);
            roleSummary.transform.position = new Vector3(__instance.Navigation.ExitButton.transform.position.x + 0.1f, position.y - 0.1f, -14f);
            roleSummary.transform.localScale = new Vector3(1f, 1f, 1f);

            var roleSummaryText = new StringBuilder();
            roleSummaryText.AppendLine(ModTranslation.GetString("FinalResults"));

            foreach (var data in AdditionalTempData.playerRoles)
            {
                var taskInfo = data.TasksTotal > 0 ? $"<color=#FAD934FF>({data.TasksCompleted}/{data.TasksTotal})</color>" : "";
                string roleText = CustomOptionHolder.Cs(data.IntroData.color, data.IntroData.NameKey + "Name") + data.AttributeRoleName;
                if (data.GhostIntroData.RoleId != RoleId.DefaultRole)
                {
                    roleText += $" → {CustomOptionHolder.Cs(data.GhostIntroData.color, data.GhostIntroData.NameKey + "Name")}";
                }
                //位置調整:ExR参考  by 漢方
                string result = $"{ModHelpers.Cs(Palette.PlayerColors[data.ColorId], data.PlayerName)}{data.NameSuffix}<pos=17%>{taskInfo} - <pos=27%>{FinalStatusPatch.GetStatusText(data.Status)} - {roleText}";
                if (ModeHandler.IsMode(ModeId.Zombie))
                {
                    roleText = data.ColorId == 1 ? CustomOptionHolder.Cs(Mode.Zombie.Main.Policecolor, "ZombiePoliceName") : CustomOptionHolder.Cs(Mode.Zombie.Main.Zombiecolor, "ZombieZombieName");
                    if (data.ColorId == 2) taskInfo = "";
                    result = $"{ModHelpers.Cs(Palette.PlayerColors[data.ColorId], data.PlayerName)}{taskInfo} : {roleText}";
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
        Recorder.OnEndGame(AdditionalTempData.gameOverReason);
        AdditionalTempData.Clear();
        OnGameEndPatch.WinText = ModHelpers.Cs(RoleColor, haison ? text : string.Format(text + " " + ModTranslation.GetString("WinName")));
        IsHaison = false;
        GameHistoryManager.Send(textRenderer.text, RoleColor);
    }
}

public class CustomPlayerData
{
    public WinningPlayerData currentData;
    public string name;
    public bool IsWin;
    public FinalStatus finalStatus;
    public int CompleteTask;
    public int TotalTask;
    public RoleId? role;
    public CustomPlayerData(GameData.PlayerInfo p, GameOverReason gameOverReason)
    {
        currentData = new(p);
        name = p.PlayerName;
        try
        {
            (CompleteTask, TotalTask) = TaskCount.TaskDate(p);
        }
        catch { }
        try
        {
            role = p.Object.GetRole();
        }
        catch
        {
            role = null;
        }
        var finalStatus = FinalStatusPatch.FinalStatusData.FinalStatuses[p.PlayerId] =
            p.Disconnected == true ? FinalStatus.Disconnected :
            FinalStatusPatch.FinalStatusData.FinalStatuses.ContainsKey(p.PlayerId) ? FinalStatusPatch.FinalStatusData.FinalStatuses[p.PlayerId] :
            p.IsDead == true ? FinalStatus.Exiled :
            gameOverReason == GameOverReason.ImpostorBySabotage && !p.Role.IsImpostor ? FinalStatus.Sabotage :
            FinalStatus.Alive;
        this.finalStatus = finalStatus;
    }
}

[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameEnd))]
public static class OnGameEndPatch
{
    public static PlayerControl WinnerPlayer;
    public static CustomGameOverReason? EndData = null;
    public static List<CustomPlayerData> PlayerData = null;
    public static string WinText;
    public static void Prefix([HarmonyArgument(0)] ref EndGameResult endGameResult)
    {
        Roles.Impostor.Camouflager.Camouflage();
        Roles.Impostor.Camouflager.ResetCamouflage();
        AdditionalTempData.gameOverReason = endGameResult.GameOverReason;
        foreach (PlayerControl p in CachedPlayer.AllPlayers)
        {
            try
            {
                p.resetChange();
            }
            catch { }
        }
        if (!ReplayManager.IsReplayMode && ConfigRoles.IsSendAnalytics.Value && !SuperNewRolesPlugin.IsBeta && !ConfigRoles.DebugMode.Value)
        {
            try
            {
                if (AmongUsClient.Instance.AmHost)
                    Analytics.PostSendData();
                Analytics.PostSendClientData();
            }
            catch (Exception e)
            {
                Logger.Info(e.ToString(), "解析エラー");
            }
        }
        if ((int)endGameResult.GameOverReason >= 10) endGameResult.GameOverReason = GameOverReason.ImpostorByKill;
    }

    public static void Postfix()
    {
        if (AmongUsClient.Instance.AmHost && ModeHandler.IsMode(ModeId.SuperHostRoles, ModeId.Zombie))
        {
            GameManager.Instance.LogicOptions.SetGameOptions(SyncSetting.DefaultOption.DeepCopy());
            RPCHelper.RpcSyncOption(GameManager.Instance.LogicOptions.currentGameOptions);
        }
        var gameOverReason = AdditionalTempData.gameOverReason;
        AdditionalTempData.Clear();
        foreach (var p in GameData.Instance.AllPlayers)
        {
            if (p != null && p.Object != null && !p.Object.IsBot())
            {
                //var p = pc.Data;
                var roles = IntroData.GetIntroData(p.Object.GetRole(), p.Object);
                if (RoleClass.Stefinder.IsKillPlayer.Contains(p.PlayerId))
                {
                    roles = IntroData.StefinderIntro1;
                }
                var ghostRoles = IntroData.GetIntroData(p.Object.GetGhostRole(), p.Object);
                var (tasksCompleted, tasksTotal) = TaskCount.TaskDate(p);
                if (p.Object.IsImpostor())
                {
                    tasksCompleted = 0;
                    tasksTotal = 0;
                }
                var finalStatus = FinalStatusPatch.FinalStatusData.FinalStatuses[p.PlayerId] =
                    p.Disconnected == true
                    ? FinalStatus.Disconnected
                    : FinalStatusPatch.FinalStatusData.FinalStatuses.ContainsKey(p.PlayerId)
                        ? FinalStatusPatch.FinalStatusData.FinalStatuses[p.PlayerId]
                        : p.IsDead == true
                            ? FinalStatus.Exiled
                            : gameOverReason == GameOverReason.ImpostorBySabotage && !p.Role.IsImpostor
                                ? FinalStatus.Sabotage
                                : FinalStatus.Alive;

                string namesuffix = "";
                if (p.Object.IsLovers())
                {
                    namesuffix = ModHelpers.Cs(RoleClass.Lovers.color, " ♥");
                }
                Dictionary<string, (Color, bool)> attributeRoles = new(SetNamesClass.AttributeRoleNameSet(p.Object));
                string attributeRoleName = "";
                if (attributeRoles.Count != 0)
                {
                    foreach (var kvp in attributeRoles)
                    {
                        attributeRoleName += $" + {CustomOptionHolder.Cs(kvp.Value.Item1, kvp.Key)}";
                    }
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
                    IntroData = roles,
                    GhostIntroData = ghostRoles,
                    AttributeRoleName = attributeRoleName
                });
            }
        }

        if (ReplayManager.IsReplayMode)
        {
            Logger.Info("ComeEndReplay");
            var ReplayEndGameData = ReplayLoader.ReplayTurns[ReplayLoader.CurrentTurn].CurrentEndGameData;
            if (ReplayEndGameData == null) return;
            Logger.Info("EndNullReplay");
            Il2CppSystem.Collections.Generic.List<WinningPlayerData> WinningPlayers = new();
            foreach (byte winnerid in ReplayEndGameData.WinnerPlayers)
            {
                WinningPlayers.Add(new(GameData.Instance.GetPlayerById(winnerid)));
            }
            TempData.winners = WinningPlayers;
            AdditionalTempData.winCondition = ReplayEndGameData.WinCond;
            return;
        }
        // Remove Jester, Arsonist, Vulture, Jackal, former Jackals and Sidekick from winners (if they win, they'll be readded)
        List<PlayerControl> notWinners = new();
        List<PlayerControl> peculiarNotWinners = new();

        /*
        TODO: 蔵徒:陣営Playerがうまく動かない為コメントアウトし、個別表記式に変更。いつか直す。

        // Neutral,MadRoles,FriendRolesから溢れたクルー勝利から除外する必要のある役職を個別追記する
        peculiarNotWinners.AddRanges(new[]
            {
                RoleClass.SatsumaAndImo.SatsumaAndImoPlayer, // クルー陣営の時はマッド役職でない為
                RoleClass.SideKiller.MadKillerPlayer, // マッドロールから外され[CrewmatePlayer]に含まれている為
                RoleClass.Dependents.DependentsPlayer, // マッドロールから外され[CrewmatePlayer]に含まれている為
                OrientalShaman.ShermansServantPlayer, // 第三陣営ではなく[CrewmatePlayer]に含まれている為
                //  RoleClass.Cupid.CupidPlayer,
                //  キューピットはNeutralPlayerだが元々記載の方法が特殊だった為コメントアウトで記載を残した。
            });

        notWinners.AddRanges(new[]
            {
                BotManager.AllBots,
                RoleHelpers.NeutralPlayer,
                RoleHelpers.MadRolesPlayer,
                RoleHelpers.FriendRolesPlayer,
                peculiarNotWinners, // 上記に含まれないクルー勝利除外役職
            });
        */

        notWinners.AddRanges(new[]{RoleClass.Jester.JesterPlayer,
            RoleClass.Madmate.MadmatePlayer,
            RoleClass.Jackal.JackalPlayer,
            RoleClass.Jackal.SidekickPlayer,
            RoleClass.JackalFriends.JackalFriendsPlayer,
            RoleClass.God.GodPlayer,
            RoleClass.Opportunist.OpportunistPlayer,
            RoleClass.Truelover.trueloverPlayer,
            RoleClass.Egoist.EgoistPlayer,
            RoleClass.Workperson.WorkpersonPlayer,
            RoleClass.Amnesiac.AmnesiacPlayer,
            RoleClass.SideKiller.MadKillerPlayer,
            RoleClass.MadMayor.MadMayorPlayer,
            RoleClass.MadStuntMan.MadStuntManPlayer,
            RoleClass.MadHawk.MadHawkPlayer,
            RoleClass.MadJester.MadJesterPlayer,
            RoleClass.MadSeer.MadSeerPlayer,
            RoleClass.FalseCharges.FalseChargesPlayer,
            RoleClass.Fox.FoxPlayer,
            BotManager.AllBots,
            RoleClass.MadMaker.MadMakerPlayer,
            RoleClass.Demon.DemonPlayer,
            RoleClass.SeerFriends.SeerFriendsPlayer,
            RoleClass.JackalSeer.JackalSeerPlayer,
            RoleClass.JackalSeer.SidekickSeerPlayer,
            RoleClass.Arsonist.ArsonistPlayer,
            RoleClass.Vulture.VulturePlayer,
            RoleClass.MadCleaner.MadCleanerPlayer,
            RoleClass.MayorFriends.MayorFriendsPlayer,
            RoleClass.Tuna.TunaPlayer,
            RoleClass.BlackCat.BlackCatPlayer,
            RoleClass.Neet.NeetPlayer,
            RoleClass.SatsumaAndImo.SatsumaAndImoPlayer,
            RoleClass.Revolutionist.RevolutionistPlayer,
            RoleClass.SuicidalIdeation.SuicidalIdeationPlayer,
            RoleClass.Spelunker.SpelunkerPlayer,
            RoleClass.Hitman.HitmanPlayer,
            RoleClass.PartTimer.PartTimerPlayer,
            RoleClass.Photographer.PhotographerPlayer,
            RoleClass.Stefinder.StefinderPlayer,
            RoleClass.Pavlovsdogs.PavlovsdogsPlayer,
            RoleClass.Pavlovsowner.PavlovsownerPlayer,
            RoleClass.LoversBreaker.LoversBreakerPlayer,
            Roles.Impostor.MadRole.Worshiper.RoleData.Player,
            Safecracker.SafecrackerPlayer,
            FireFox.FireFoxPlayer,
            OrientalShaman.OrientalShamanPlayer,
            OrientalShaman.ShermansServantPlayer,
            TheThreeLittlePigs.TheFirstLittlePig.Player,
            TheThreeLittlePigs.TheSecondLittlePig.Player,
            TheThreeLittlePigs.TheThirdLittlePig.Player,
            WaveCannonJackal.WaveCannonJackalPlayer,
            WaveCannonJackal.SidekickWaveCannonPlayer,
            BlackHatHacker.BlackHatHackerPlayer,
            Moira.MoiraPlayer,
            Roles.Impostor.MadRole.MadRaccoon.RoleData.Player,
            Sauner.RoleData.Player,
            Pokerface.RoleData.Player,
            Crook.RoleData.Player,
            Frankenstein.FrankensteinPlayer,
            });
        notWinners.AddRange(RoleClass.Cupid.CupidPlayer);
        notWinners.AddRange(RoleClass.Dependents.DependentsPlayer);

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
        bool TaskerWin = gameOverReason == (GameOverReason)CustomGameOverReason.TaskerWin;
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
        bool HitmanWin = gameOverReason == (GameOverReason)CustomGameOverReason.HitmanWin;
        bool PhotographerWin = gameOverReason == (GameOverReason)CustomGameOverReason.PhotographerWin;
        bool PavlovsTeamWin = gameOverReason == (GameOverReason)CustomGameOverReason.PavlovsTeamWin;
        bool LoversBreakerWin = gameOverReason == (GameOverReason)CustomGameOverReason.LoversBreakerWin;
        bool NoWinner = gameOverReason == (GameOverReason)CustomGameOverReason.NoWinner;
        bool CrewmateWin = gameOverReason is (GameOverReason)CustomGameOverReason.CrewmateWin or GameOverReason.HumansByVote or GameOverReason.HumansByTask or GameOverReason.ImpostorDisconnect;
        bool BUGEND = gameOverReason == (GameOverReason)CustomGameOverReason.BugEnd;
        bool SafecrackerWin = gameOverReason == (GameOverReason)CustomGameOverReason.SafecrackerWin;
        bool BlackHatHackerWin = gameOverReason == (GameOverReason)CustomGameOverReason.BlackHatHackerWin;
        bool SaunerWin = gameOverReason == (GameOverReason)CustomGameOverReason.SaunerWin;
        bool CrookWin = gameOverReason == (GameOverReason)CustomGameOverReason.CrookWin;
        if (ModeHandler.IsMode(ModeId.SuperHostRoles, ModeId.CopsRobbers) && EndData != null)
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
            CrookWin = EndData == CustomGameOverReason.CrookWin;
        }
        if (JesterWin)
        {
            WinnerPlayer.Data.IsDead = false;
            (TempData.winners = new()).Add(new(WinnerPlayer.Data));
            AdditionalTempData.winCondition = WinCondition.JesterWin;
        }
        else if (MadJesterWin)
        {
            WinnerPlayer.Data.IsDead = false;
            (TempData.winners = new()).Add(new(WinnerPlayer.Data));
            AdditionalTempData.winCondition = WinCondition.MadJesterWin;
        }
        else if (JackalWin)
        {
            TempData.winners = new();
            foreach (var cp in CachedPlayer.AllPlayers)
            {
                if (cp.PlayerControl.IsJackalTeam())
                {
                    TempData.winners.Add(new(cp.Data));
                }
            }
            AdditionalTempData.winCondition = WinCondition.JackalWin;
        }
        else if (EgoistWin)
        {
            TempData.winners = new();
            foreach (PlayerControl p in RoleClass.Egoist.EgoistPlayer)
            {
                if (p.IsAlive())
                {
                    TempData.winners.Add(new(WinnerPlayer.Data));
                }
            }
            AdditionalTempData.winCondition = WinCondition.EgoistWin;
        }
        else if (WorkpersonWin)
        {
            (TempData.winners = new()).Add(new(WinnerPlayer.Data));
            AdditionalTempData.winCondition = WinCondition.WorkpersonWin;
        }
        else if (FalseChargesWin)
        {
            (TempData.winners = new()).Add(new(WinnerPlayer.Data));
            AdditionalTempData.winCondition = WinCondition.FalseChargesWin;
        }
        else if (DemonWin)
        {
            TempData.winners = new();
            foreach (PlayerControl player in RoleClass.Demon.DemonPlayer)
            {
                if (Demon.IsWin(player))
                {
                    TempData.winners.Add(new(WinnerPlayer.Data));
                }
            }
            AdditionalTempData.winCondition = WinCondition.DemonWin;
        }
        else if (ArsonistWin)
        {
            TempData.winners = new();
            foreach (PlayerControl player in RoleClass.Arsonist.ArsonistPlayer)
            {
                if (Arsonist.IsArsonistWinFlag())
                {
                    SuperNewRolesPlugin.Logger.LogInfo("アーソニストがEndGame");
                    TempData.winners.Add(new(WinnerPlayer.Data));
                }
            }
            AdditionalTempData.winCondition = WinCondition.ArsonistWin;
        }
        else if (VultureWin)
        {
            (TempData.winners = new()).Add(new(WinnerPlayer.Data));
            AdditionalTempData.winCondition = WinCondition.VultureWin;
        }
        else if (RevolutionistWin)
        {
            (TempData.winners = new()).Add(new(WinnerPlayer.Data));
            AdditionalTempData.winCondition = WinCondition.RevolutionistWin;
        }
        else if (SuicidalIdeationWin)
        {
            (TempData.winners = new()).Add(new(WinnerPlayer.Data));
            AdditionalTempData.winCondition = WinCondition.SuicidalIdeationWin;
        }
        else if (HitmanWin)
        {
            if (WinnerPlayer == null)
            {
                foreach (PlayerControl p in PlayerControl.AllPlayerControls) if (p.IsRole(RoleId.Hitman)) WinnerPlayer = p;
                if (WinnerPlayer == null)
                {
                    Logger.Error("エラー:殺し屋が生存していませんでした", "HitmanWin");
                    WinnerPlayer = PlayerControl.LocalPlayer;
                }
            }
            (TempData.winners = new()).Add(new(WinnerPlayer.Data));
            AdditionalTempData.winCondition = WinCondition.HitmanWin;
        }
        else if (PhotographerWin)
        {
            (TempData.winners = new()).Add(new(WinnerPlayer.Data));
            AdditionalTempData.winCondition = WinCondition.PhotographerWin;
        }
        else if (PavlovsTeamWin)
        {
            TempData.winners = new();
            foreach (PlayerControl p in PlayerControl.AllPlayerControls)
            {
                if (p.IsPavlovsTeam()) TempData.winners.Add(new(p.Data));
            }
            AdditionalTempData.winCondition = WinCondition.PavlovsTeamWin;
        }
        else if (QuarreledWin)
        {
            TempData.winners = new();
            List<PlayerControl> winplays = new()
                {
                    WinnerPlayer,
                    WinnerPlayer.GetOneSideQuarreled()
                };
            foreach (PlayerControl player in winplays)
            {
                player.Data.IsDead = false;
                TempData.winners.Add(new(player.Data));
            }
            AdditionalTempData.winCondition = WinCondition.QuarreledWin;
        }
        else if (CrewmateWin)
        {
            if (RoleClass.SatsumaAndImo.TeamNumber == 1)//クルーなら
                foreach (PlayerControl smp in RoleClass.SatsumaAndImo.SatsumaAndImoPlayer)
                    TempData.winners.Add(new(smp.Data));//さつまいもも勝ち
        }
        else if (TaskerWin)
        {
            AdditionalTempData.winCondition = WinCondition.TaskerWin;
        }
        else if (LoversBreakerWin)
        {
            if (WinnerPlayer is not null)
            {
                (TempData.winners = new()).Add(new(WinnerPlayer.Data));
            }
            else
            {
                TempData.winners = new();
                foreach (byte playerId in RoleClass.LoversBreaker.CanEndGamePlayers)
                {
                    TempData.winners.Add(new(ModHelpers.PlayerById(playerId).Data));
                }
            }
            AdditionalTempData.winCondition = WinCondition.LoversBreakerWin;
        }
        else if (SafecrackerWin)
        {
            (TempData.winners = new()).Add(new(WinnerPlayer.Data));
            AdditionalTempData.winCondition = WinCondition.SafecrackerWin;
        }
        else if (BlackHatHackerWin)
        {
            (TempData.winners = new()).Add(new(WinnerPlayer.Data));
            AdditionalTempData.winCondition = WinCondition.BlackHatHackerWin;
        }
        else if (SaunerWin)
        {
            (TempData.winners = new()).Add(new(WinnerPlayer.Data));
            AdditionalTempData.winCondition = WinCondition.SaunerWin;
        }

        if (TempData.winners.ToArray().Any(x => x.IsImpostor))
        {
            foreach (var cp in CachedPlayer.AllPlayers)
                if (cp.PlayerControl.IsMadRoles() || cp.PlayerControl.IsRole(RoleId.MadKiller, RoleId.Dependents)) TempData.winners.Add(new(cp.Data));
        }


        //単独勝利系統
        //下に行くほど優先度が高い
        bool isDleted = false;
        bool changeTheWinCondition = Mode.PlusMode.PlusGameOptions.PlusGameOptionSetting.GetBool() && Mode.PlusMode.PlusGameOptions.IsChangeTheWinCondition.GetBool();
        bool isReset = false;

        foreach (PlayerControl player in RoleClass.Neet.NeetPlayer)
        {
            if (player.IsAlive() && !RoleClass.Neet.IsAddWin)
            {
                if (!((isDleted && changeTheWinCondition) || isReset))
                {
                    TempData.winners = new();
                    isDleted = true;
                    isReset = true;
                }
                TempData.winners.Add(new(player.Data));
                AdditionalTempData.winCondition = WinCondition.NeetWin;
            }
        }
        isReset = false;
        foreach (PlayerControl player in RoleClass.God.GodPlayer)
        {
            if (player.IsAlive())
            {
                if (!((isDleted && changeTheWinCondition) || isReset))
                {
                    TempData.winners = new();
                    isDleted = true;
                    isReset = true;
                }
                var (Complete, all) = TaskCount.TaskDateNoClearCheck(player.Data);
                if (!RoleClass.God.IsTaskEndWin || Complete >= all)
                {
                    TempData.winners.Add(new(player.Data));
                    AdditionalTempData.winCondition = WinCondition.GodWin;
                }
            }
        }
        isReset = false;
        foreach (PlayerControl player in OrientalShaman.OrientalShamanPlayer)
        {
            if (!OrientalShaman.OrientalShamanCrewTaskWinHijack.GetBool() &&
                AdditionalTempData.gameOverReason == GameOverReason.HumansByTask) break;
            if (OrientalShaman.OrientalShamanWinTask.GetBool())
            {
                var (completed, total) = TaskCount.TaskDate(player.Data);
                if (completed < total) continue;
            }
            if (player.IsAlive())
            {
                if (!((isDleted && changeTheWinCondition) || isReset))
                {
                    TempData.winners = new();
                    isDleted = true;
                    isReset = true;
                }
                TempData.winners.Add(new(player.Data));
                if (OrientalShaman.OrientalShamanCausative.ContainsKey(player.PlayerId))
                {
                    PlayerControl causativePlayer = ModHelpers.PlayerById(OrientalShaman.OrientalShamanCausative[player.PlayerId]);
                    if (causativePlayer) TempData.winners.Add(new(causativePlayer.Data));
                }
                AdditionalTempData.winCondition = WinCondition.OrientalShamanWin;
            }
        }
        isReset = false;
        foreach (PlayerControl player in RoleClass.Tuna.TunaPlayer)
        {
            if (player.IsAlive() && !RoleClass.Tuna.IsTunaAddWin)
            {
                if (!((isDleted && changeTheWinCondition) || isReset))
                {
                    TempData.winners = new();
                    isDleted = true;
                    isReset = true;
                }
                TempData.winners.Add(new(player.Data));
                AdditionalTempData.winCondition = WinCondition.TunaWin;

            }
        }
        isReset = false;
        foreach (PlayerControl player in RoleClass.Stefinder.StefinderPlayer)
        {
            if (player.IsAlive() && CustomOptionHolder.StefinderSoloWin.GetBool())
            {
                if (!RoleClass.Stefinder.IsKillPlayer.Contains(player.PlayerId) &&
                   (AdditionalTempData.gameOverReason == GameOverReason.HumansByTask ||
                    AdditionalTempData.gameOverReason == GameOverReason.HumansByVote ||
                    AdditionalTempData.gameOverReason == GameOverReason.HumansDisconnect))
                {
                    if (!((isDleted && changeTheWinCondition) || isReset))
                    {
                        TempData.winners = new();
                        isDleted = true;
                        isReset = true;
                    }
                    TempData.winners.Add(new(player.Data));
                    AdditionalTempData.winCondition = WinCondition.StefinderWin;
                }
                if (RoleClass.Stefinder.IsKillPlayer.Contains(player.PlayerId) &&
                   (AdditionalTempData.gameOverReason == GameOverReason.ImpostorByKill ||
                    AdditionalTempData.gameOverReason == GameOverReason.ImpostorBySabotage ||
                    AdditionalTempData.gameOverReason == GameOverReason.ImpostorByVote ||
                    AdditionalTempData.gameOverReason == GameOverReason.ImpostorDisconnect))
                {
                    if (!((isDleted && changeTheWinCondition) || isReset))
                    {
                        TempData.winners = new();
                        isDleted = true;
                        isReset = true;
                    }
                    TempData.winners.Add(new(player.Data));
                    AdditionalTempData.winCondition = WinCondition.StefinderWin;
                }
            }
        }
        isReset = false;
        foreach (List<PlayerControl> plist in RoleClass.Lovers.LoversPlayer)
        {
            if (RoleClass.Lovers.IsSingleTeam)
            {
                bool IsWinLovers = false;
                foreach (PlayerControl player in plist)
                {
                    if (player.IsAlive())
                    {
                        IsWinLovers = true;
                    }
                }
                if (IsWinLovers)
                {
                    foreach (PlayerControl player in plist)
                    {
                        if (!((isDleted && changeTheWinCondition) || isReset))
                        {
                            TempData.winners = new();
                            isDleted = true;
                            isReset = true;
                        }
                        TempData.winners.Add(new(player.Data));
                        if (RoleClass.Cupid.CupidLoverPair.ContainsValue(player.PlayerId))
                        {

                            PlayerControl cPlayer = ModHelpers.PlayerById((byte)RoleClass.Cupid.CupidLoverPair.GetKey(player.PlayerId));
                            if (cPlayer != null && cPlayer.IsRole(RoleId.Cupid))
                                TempData.winners.Add(new(ModHelpers.PlayerById((byte)RoleClass.Cupid.CupidLoverPair.GetKey(player.PlayerId)).Data));
                        }
                        AdditionalTempData.winCondition = WinCondition.LoversWin;
                    }
                }
            }
        }
        //ポーカーフェイス勝利判定
        isReset = false;
        foreach (Pokerface.PokerfaceTeam team in Pokerface.RoleData.PokerfaceTeams)
        {
            if (team.CanWin())
            {
                if (!((isDleted && changeTheWinCondition) || isReset))
                {
                    TempData.winners = new();
                    isDleted = true;
                    isReset = true;
                }
                foreach (PlayerControl teammember in team.TeamPlayers)
                    //ポーカーフェイスじゃない場合を考慮する
                    if (teammember.IsRole(RoleId.Pokerface))
                        //生存者のみ勝利の設定が無効もしくは対象が生存している場合は追加する
                        if (!Pokerface.CustomOptionData.WinnerOnlyAlive.GetBool() ||
                            teammember.IsAlive())
                            TempData.winners.Add(new(teammember.Data));
                AdditionalTempData.winCondition = WinCondition.PokerfaceWin;
            }
        }
        isReset = false;
        foreach (PlayerControl player in RoleClass.Spelunker.SpelunkerPlayer)
        {
            bool isreset = false;
            if (player.IsAlive())
            {
                if (!isreset)
                {
                    if (!((isDleted && changeTheWinCondition) || isReset))
                    {
                        TempData.winners = new();
                        isDleted = true;
                        isReset = true;
                    }
                    TempData.winners.Add(new(player.Data));
                    AdditionalTempData.winCondition = WinCondition.SpelunkerWin;
                }
                isreset = true;
            }
        }
        isReset = false;
        foreach (List<PlayerControl> plist in TheThreeLittlePigs.TheThreeLittlePigsPlayer)
        {
            if (AdditionalTempData.winCondition is WinCondition.LoversBreakerWin or WinCondition.SafecrackerWin or WinCondition.JesterWin or
                                                   WinCondition.VultureWin or WinCondition.WorkpersonWin or WinCondition.FalseChargesWin or
                                                   WinCondition.DemonWin or WinCondition.SuicidalIdeationWin or WinCondition.PhotographerWin or
                                                   WinCondition.RevolutionistWin or WinCondition.QuarreledWin or WinCondition.BlackHatHackerWin) break;
            if (!TheThreeLittlePigs.IsTheThreeLittlePigs(plist) || plist.IsAllDead()) continue;
            bool isAllAlive = true;
            if (plist.Count >= 3)
            {
                foreach (PlayerControl player in plist)
                {
                    if (player.IsDead() || !TheThreeLittlePigs.IsTheThreeLittlePigs(player))
                    {
                        isAllAlive = false;
                        break;
                    }
                }
            }
            else isAllAlive = false;
            if (isAllAlive)
            {
                if (!((isDleted && changeTheWinCondition) || isReset))
                {
                    TempData.winners = new();
                    isDleted = true;
                    isReset = true;
                }
                foreach (PlayerControl player in plist)
                {
                    if (!TheThreeLittlePigs.IsTheThreeLittlePigs(player)) continue;
                    TempData.winners.Add(new(player.Data));
                    AdditionalTempData.winCondition = WinCondition.TheThreeLittlePigsWin;
                }
            }
            else
            {
                bool isAllKillerDead = true;
                foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                {
                    if (player.IsDead()) continue;
                    if (player.IsImpostor() || player.IsKiller())
                    {
                        isAllKillerDead = false;
                        break;
                    }
                }
                if (isAllKillerDead)
                {
                    if (!((isDleted && changeTheWinCondition) || isReset))
                    {
                        TempData.winners = new();
                        isDleted = true;
                        isReset = true;
                    }
                    foreach (PlayerControl player in plist)
                    {
                        if (!TheThreeLittlePigs.IsTheThreeLittlePigs(player)) continue;
                        TempData.winners.Add(new(player.Data));
                        AdditionalTempData.winCondition = WinCondition.TheThreeLittlePigsWin;
                    }
                }
            }
        }
        foreach (KeyValuePair<byte, int> data in Frankenstein.KillCount)
        {
            //勝利に必要なキル数を満たしているか
            if (data.Value > 0)
                continue;
            //生存していなければ勝利できない
            GameData.PlayerInfo FrankenPlayer = GameData.Instance.GetPlayerById(data.Key);
            if (FrankenPlayer.IsDead())
                continue;
            if (!((isDleted && changeTheWinCondition) || isReset))
            {
                TempData.winners = new();
                isDleted = true;
                isReset = true;
            }
            TempData.winners.Add(new(FrankenPlayer));
            AdditionalTempData.winCondition = WinCondition.FrankensteinWin;
        }
        if (Moira.AbilityUsedUp && Moira.Player.IsAlive())
        {
            if (!((isDleted && changeTheWinCondition) || isReset))
            {
                TempData.winners = new();
                isDleted = true;
                isReset = true;
            }
            TempData.winners.Add(new(Moira.Player.Data));
            AdditionalTempData.winCondition = WinCondition.MoiraWin;
        }
        isReset = false;
        // 詐欺師は, 勝利判定が実行される前に既に勝利条件を満たしている為, 狐の次の勝利順位 (勝利条件を満たす : MeetingHud.Start, 勝利判定 : SpawnInMinigame.Begin)
        if (Crook.RoleData.FirstWinFlag)
        {
            (bool crookFinalWinFlag, List<PlayerControl> crookWinners) = Crook.DecisionOfVictory.GetTheLastDecisionAndWinners();
            if (crookFinalWinFlag) // 最終的な勝利条件(受給回数, 生存, 最終の保管金の受領場所(追放処理)にたどり着いた) を 満たしている詐欺師がいたら
            {
                if (!((isDleted && changeTheWinCondition) || isReset))
                {
                    TempData.winners = new();
                    isDleted = true;
                    isReset = true;
                }

                foreach (var winner in crookWinners)
                {
                    Logger.Info($"{winner.name}は勝利リストに入った", "EndGame CrookWin");
                    TempData.winners.Add(new(winner.Data));
                }
                AdditionalTempData.winCondition = WinCondition.CrookWin;
            }
        }
        List<PlayerControl> foxPlayers = new(RoleClass.Fox.FoxPlayer);
        foxPlayers.AddRange(FireFox.FireFoxPlayer);
        isReset = false;
        foreach (PlayerControl player in foxPlayers)
        {
            if (player.IsAlive())
            {
                if (!((isDleted && changeTheWinCondition) || isReset))
                {
                    TempData.winners = new();
                    isDleted = true;
                    isReset = true;
                }
                TempData.winners.Add(new(player.Data));
                AdditionalTempData.winCondition = WinCondition.FoxWin;
            }
        }

        //追加勝利系
        foreach (PlayerControl p in RoleClass.Tuna.TunaPlayer)
        {
            if (p.IsAlive() && RoleClass.Tuna.IsTunaAddWin)
            {
                TempData.winners.Add(new(p.Data));
            }
        }
        foreach (PlayerControl p in RoleClass.Neet.NeetPlayer)
        {
            if (p.IsAlive() && RoleClass.Neet.IsAddWin)
            {
                TempData.winners.Add(new(p.Data));
            }
        }
        foreach (PlayerControl p in RoleClass.SuicidalIdeation.SuicidalIdeationPlayer)
        {
            var (playerCompleted, playerTotal) = TaskCount.TaskDate(p.Data);
            if (p.IsAlive() && playerTotal > playerCompleted)
            {
                TempData.winners.Add(new(p.Data));
            }
        }
        foreach (PlayerControl player in RoleClass.Opportunist.OpportunistPlayer)
        {
            if (player.IsAlive())
            {
                TempData.winners.Add(new(player.Data));
            }
        }
        foreach (PlayerControl player in RoleClass.Revolutionist.RevolutionistPlayer)
        {
            if (RoleClass.Revolutionist.IsAddWin && (!RoleClass.Revolutionist.IsAddWinAlive || player.IsAlive()) && !TempData.winners.Contains(new(player.Data)))
            {
                TempData.winners.Add(new(player.Data));
            }
        }
        foreach (PlayerControl player in RoleClass.Neet.NeetPlayer)
        {
            if (player.IsAlive())
            {
                TempData.winners.Add(new(player.Data));
            }
        }
        foreach (PlayerControl player in RoleClass.Stefinder.StefinderPlayer)
        {
            if (player.IsAlive() && !CustomOptionHolder.StefinderSoloWin.GetBool())
            {
                if (!RoleClass.Stefinder.IsKillPlayer.Contains(player.PlayerId) &&
                   (AdditionalTempData.gameOverReason == GameOverReason.HumansByTask ||
                    AdditionalTempData.gameOverReason == GameOverReason.HumansByVote ||
                    AdditionalTempData.gameOverReason == GameOverReason.HumansDisconnect))
                {
                    TempData.winners.Add(new(player.Data));
                }
                if (RoleClass.Stefinder.IsKillPlayer.Contains(player.PlayerId) &&
                   (AdditionalTempData.gameOverReason == GameOverReason.ImpostorByKill ||
                    AdditionalTempData.gameOverReason == GameOverReason.ImpostorBySabotage ||
                    AdditionalTempData.gameOverReason == GameOverReason.ImpostorByVote ||
                    AdditionalTempData.gameOverReason == GameOverReason.ImpostorDisconnect))
                {
                    TempData.winners.Add(new(player.Data));
                }
            }
        }
        foreach (List<PlayerControl> plist in RoleClass.Lovers.LoversPlayer)
        {
            if (!RoleClass.Lovers.IsSingleTeam)
            {
                bool IsWinLovers = false;
                foreach (PlayerControl player in plist)
                {
                    if (player.IsAlive())
                    {
                        IsWinLovers = true;
                    }
                }
                if (IsWinLovers)
                {
                    foreach (PlayerControl player in plist)
                    {
                        TempData.winners.Add(new(player.Data));
                        if (RoleClass.Cupid.CupidLoverPair.ContainsValue(player.PlayerId))
                        {
                            PlayerControl cPlayer = ModHelpers.PlayerById((byte)RoleClass.Cupid.CupidLoverPair.GetKey(player.PlayerId));
                            if (cPlayer != null && cPlayer.IsRole(RoleId.Cupid))
                                TempData.winners.Add(new(ModHelpers.PlayerById((byte)RoleClass.Cupid.CupidLoverPair.GetKey(player.PlayerId)).Data));
                        }
                    }
                }
            }
        }
        foreach (KeyValuePair<PlayerControl, byte> PartTimerData in (Dictionary<PlayerControl, byte>)RoleClass.PartTimer.Data) //フリーター
        {
            PlayerControl PartTimerValue = ModHelpers.PlayerById(PartTimerData.Value);
            if (TempData.winners.ToArray().Any(x => x.PlayerName == PartTimerValue.Data.PlayerName))
            {
                WinningPlayerData wpd = new(PartTimerData.Key.Data);
                TempData.winners.Add(wpd);
            }
        }


        if (ModeHandler.IsMode(ModeId.BattleRoyal))
        {
            TempData.winners = new();
            if (Mode.BattleRoyal.Main.IsTeamBattle)
            {
                foreach (PlayerControl p in Mode.BattleRoyal.Main.Winners)
                    TempData.winners.Add(new(p.Data));
            }
            else
            {
                foreach (PlayerControl p in CachedPlayer.AllPlayers)
                {
                    if (p.IsAlive())
                        TempData.winners.Add(new(p.Data));
                }
            }
            AdditionalTempData.winCondition = WinCondition.Default;
        }
        if (ModeHandler.IsMode(ModeId.Zombie))
        {
            TempData.winners = new();
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
                AdditionalTempData.winCondition = WinCondition.WorkpersonWin;
            }
        }
        int i = 0;
        foreach (PlayerControl p in CachedPlayer.AllPlayers)
        {
            if (p.IsAlive()) break;
            i++;
        }
        if (NoWinner || i == CachedPlayer.AllPlayers.Count)
        {
            TempData.winners = new();
            AdditionalTempData.winCondition = WinCondition.NoWinner;
        }
        Logger.Info("WELCOME!!!");
        if (ModeHandler.IsMode(ModeId.PantsRoyal))
        {
            Logger.Info("Pants!!!!:" + (WinnerPlayer != null).ToString());
            if (WinnerPlayer != null)
            {
                TempData.winners = new();
                TempData.winners.Add(new WinningPlayerData(WinnerPlayer.Data));
                AdditionalTempData.winCondition = WinCondition.PantsRoyalWin;
            }
            else
            {
                TempData.winners = new();
                foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                {
                    Logger.Info(player.Data.Role.Role + ":" + player.PlayerId.ToString() + ":" + player.Data.PlayerName);
                    if (player.Data.Role.Role == AmongUs.GameOptions.RoleTypes.CrewmateGhost || player.Data.Role.Role == AmongUs.GameOptions.RoleTypes.Crewmate)
                    {
                        TempData.winners.Add(new WinningPlayerData(player.Data));
                        Logger.Info("PASS!!!!!:" + player.Data.PlayerName + ":" + player.PlayerId.ToString());
                        break;
                    }
                }
                if (TempData.winners.Count > 0)
                {
                    AdditionalTempData.winCondition = WinCondition.PantsRoyalWin;
                    Logger.Info("ToPantsRoyalWin");
                }
                else
                {
                    AdditionalTempData.winCondition = WinCondition.NoWinner;
                    Logger.Info("ToNoWinner");
                }
                Logger.Info(AdditionalTempData.winCondition.ToString() + ":WINCONDITION");
            }
        }
        if (HAISON)
        {
            TempData.winners = new();
            foreach (PlayerControl p in CachedPlayer.AllPlayers)
            {
                if (!p.IsBot())
                {
                    WinningPlayerData wpd = new(p.Data);
                    TempData.winners.Add(wpd);
                }
            }
            AdditionalTempData.winCondition = WinCondition.HAISON;
        }
        foreach (GameData.PlayerInfo player in GameData.Instance.AllPlayers)
        {
            if (player.Object != null && player.Object.IsBot()) continue;
            CustomPlayerData data = new(player, gameOverReason)
            {
                IsWin = TempData.winners.ToArray().Any(x => x.PlayerName == player.PlayerName)
            };
            PlayerData.Add(data);
        }
        GameHistoryManager.OnGameEndSet(FinalStatusPatch.FinalStatusData.FinalStatuses);
        BattleRoyalWebManager.EndGame();
    }
}
[HarmonyPatch(typeof(TranslationController), nameof(TranslationController.GetString), new Type[] { typeof(StringNames), typeof(Il2CppReferenceArray<Il2CppSystem.Object>) })]
class ExileControllerMessagePatch
{
    static void Postfix(ref string __result, [HarmonyArgument(0)] StringNames id)
    {
        if (id is StringNames.GameDiscussTime && ModeHandler.IsMode(ModeId.Werewolf, false)) __result = ModTranslation.GetString("WerewolfAbilityTimeSetting");
        try
        {
            if (ExileController.Instance != null && ExileController.Instance.exiled != null && ModeHandler.IsMode(ModeId.Default))
            {
                PlayerControl player = ModHelpers.PlayerById(ExileController.Instance.exiled.Object.PlayerId);
                if (player == null) return;
                FinalStatusPatch.FinalStatusData.FinalStatuses[player.PlayerId] = FinalStatus.Exiled;
                // Exile role text
                if (id is StringNames.ExileTextPN or StringNames.ExileTextSN or StringNames.ExileTextPP or StringNames.ExileTextSP)
                {
                    __result = player.Data.PlayerName + " は " + ModTranslation.GetString(IntroData.GetIntroData(player.GetRole(), player).NameKey + "Name") + " だった！";
                }
            }
        }
        catch
        {
            // pass - Hopefully prevent leaving while exiling to softlock game
        }
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

[HarmonyPatch(typeof(LogicGameFlowHnS), nameof(LogicGameFlowHnS.CheckEndCriteria))]
public static class CheckGameEndHnSPatch
{
    public static bool Prefix()
    {
        if (!GameData.Instance) return false;
        if (DestroyableSingleton<TutorialManager>.InstanceExists) return true;
        if (!RoleManagerSelectRolesPatch.IsSetRoleRPC) return false;
        if (ModHelpers.IsDebugMode()) return false;
        ShipStatus __instance = ShipStatus.Instance;
        PlayerStatistics statistics = new();
        if (!ModeHandler.IsMode(ModeId.Default))
        {
            ModeHandler.EndGameCheckHnSs(__instance, statistics);
        }
        else
        {
            if (CheckAndEndGameForPavlovsWin(__instance, statistics)) return false;
            if (CheckAndEndGameForHitmanWin(__instance, statistics)) return false;
            if (CheckAndEndGameForJackalWin(__instance, statistics)) return false;
            if (CheckAndEndGameForEgoistWin(__instance, statistics)) return false;
            if (CheckAndEndGameForTaskerWin(__instance, statistics)) return false;
            if (CheckAndEndGameForWorkpersonWin(__instance)) return false;
            if (CheckAndEndGameForFoxHouwaWin(__instance)) return false;
            if (CheckAndEndGameForSuicidalIdeationWin(__instance)) return false;
            if (CheckAndEndGameForHitmanWin(__instance, statistics)) return false;
            if (CheckAndEndGameForSafecrackerWin(__instance)) return false;
        }
        return true;
    }
}

[HarmonyPatch(typeof(LogicGameFlowNormal), nameof(LogicGameFlowNormal.CheckEndCriteria))]
public static class CheckGameEndPatch
{
    public static bool Prefix()
    {
        if (!GameData.Instance) return false;
        if (DestroyableSingleton<TutorialManager>.InstanceExists) return true;
        if (!RoleManagerSelectRolesPatch.IsSetRoleRPC) return false;
        if (ModHelpers.IsDebugMode()) return false;
        if (ReplayManager.IsReplayMode) return false;
        if (RoleClass.Assassin.TriggerPlayer != null) return false;
        if (RoleClass.Revolutionist.MeetingTrigger != null) return false;
        ShipStatus __instance = ShipStatus.Instance;
        PlayerStatistics statistics = new();
        if (!ModeHandler.IsMode(ModeId.Default))
        {
            ModeHandler.EndGameChecks(__instance, statistics);
        }
        else
        {
            if (CheckAndEndGameForLoversBreakerWin(__instance, statistics)) return false;
            if (CheckAndEndGameForCrewmateWin(__instance, statistics)) return false;
            if (CheckAndEndGameForSabotageWin(__instance)) return false;
            if (CheckAndEndGameForPavlovsWin(__instance, statistics)) return false;
            if (CheckAndEndGameForHitmanWin(__instance, statistics)) return false;
            if (CheckAndEndGameForJackalWin(__instance, statistics)) return false;
            if (CheckAndEndGameForEgoistWin(__instance, statistics)) return false;
            if (CheckAndEndGameForImpostorWin(__instance, statistics)) return false;
            if (CheckAndEndGameForTaskerWin(__instance, statistics)) return false;
            if (CheckAndEndGameForWorkpersonWin(__instance)) return false;
            if (CheckAndEndGameForFoxHouwaWin(__instance)) return false;
            if (CheckAndEndGameForSuicidalIdeationWin(__instance)) return false;
            if (CheckAndEndGameForHitmanWin(__instance, statistics)) return false;
            if (CheckAndEndGameForSafecrackerWin(__instance)) return false;
            if (!PlusModeHandler.IsMode(PlusModeId.NotTaskWin) && CheckAndEndGameForTaskWin(__instance)) return false;
        }
        return false;
    }
    public static void CustomEndGame(GameOverReason reason, bool showAd)
    {
        GameManager.Instance.RpcEndGame(reason, showAd);
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

    public static bool CheckAndEndGameForLoversBreakerWin(ShipStatus __instance, PlayerStatistics statistics)
    {
        if (!CustomOptionHolder.LoversBreakerIsDeathWin.GetBool())
        {
            foreach (byte playerId in RoleClass.LoversBreaker.CanEndGamePlayers.ToArray())
            {
                if (ModHelpers.PlayerById(playerId).IsDead())
                {
                    RoleClass.LoversBreaker.CanEndGamePlayers.Remove(playerId);
                }
            }
        }
        if (RoleClass.LoversBreaker.CanEndGamePlayers.Count > 0 && statistics.LoversAlive <= 0)
        {
            __instance.enabled = false;
            CustomEndGame((GameOverReason)CustomGameOverReason.LoversBreakerWin, false);
            return true;
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

    public static bool CheckAndEndGameForTaskerWin(ShipStatus __instance, PlayerStatistics statistics)
    {
        foreach (PlayerControl p in RoleClass.Tasker.TaskerPlayer)
        {
            if (p == null) continue;
            if (p.IsDead()) continue;
            if (p.AllTasksCompleted())
            {
                __instance.enabled = false;
                var endReason = (GameOverReason)CustomGameOverReason.TaskerWin;
                if (Demon.IsDemonWinFlag())
                {
                    endReason = (GameOverReason)CustomGameOverReason.DemonWin;
                }

                CustomEndGame(endReason, false);
                return true;
            }
        }
        return false;
    }
    public static bool CheckAndEndGameForHitmanWin(ShipStatus __instance, PlayerStatistics statistics)
    {
        if (statistics.TotalAlive <= 1 && statistics.HitmanAlive == 1)
        {
            __instance.enabled = false;
            CustomEndGame((GameOverReason)CustomGameOverReason.HitmanWin, false);
            return true;
        }
        return false;
    }

    public static bool CheckAndEndGameForImpostorWin(ShipStatus __instance, PlayerStatistics statistics)
    {
        if (statistics.TeamImpostorsAlive >= statistics.TotalAlive - statistics.TeamImpostorsAlive && statistics.TeamJackalAlive == 0 && statistics.HitmanAlive == 0 && !statistics.IsGuardPavlovs && !EvilEraser.IsGodWinGuard() && !EvilEraser.IsFoxWinGuard() && !EvilEraser.IsNeetWinGuard())
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
        if (statistics.EgoistAlive >= statistics.TotalAlive - statistics.EgoistAlive && statistics.EgoistAlive != 0 && statistics.TeamImpostorsAlive == 0 && statistics.TeamJackalAlive == 0 && statistics.HitmanAlive == 0 && !statistics.IsGuardPavlovs)
        {
            __instance.enabled = false;
            CustomEndGame((GameOverReason)CustomGameOverReason.EgoistWin, false);
            return true;
        }
        return false;
    }
    public static bool CheckAndEndGameForJackalWin(ShipStatus __instance, PlayerStatistics statistics)
    {
        if (statistics.TeamJackalAlive >= statistics.TotalAlive - statistics.TeamJackalAlive && statistics.TeamImpostorsAlive == 0 && statistics.HitmanAlive == 0 && !statistics.IsGuardPavlovs)
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

    public static bool CheckAndEndGameForPavlovsWin(ShipStatus __instance, PlayerStatistics statistics)
    {
        if (statistics.PavlovsTeamAlive >= statistics.TotalAlive - statistics.PavlovsTeamAlive && statistics.TeamImpostorsAlive == 0 && statistics.TeamJackalAlive == 0)
        {
            __instance.enabled = false;
            CustomEndGame((GameOverReason)CustomGameOverReason.PavlovsTeamWin, false);
            return true;
        }
        return false;
    }

    public static bool CheckAndEndGameForCrewmateWin(ShipStatus __instance, PlayerStatistics statistics)
    {
        if (statistics.TeamImpostorsAlive == 0 && statistics.TeamJackalAlive == 0 && statistics.HitmanAlive == 0 && !statistics.IsGuardPavlovs)
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
            if (p == null) continue;
            if (!p.Data.Disconnected)
            {
                if (p.IsAlive() || !RoleClass.Workperson.IsAliveWin)
                {
                    var (playerCompleted, playerTotal) = TaskCount.TaskDate(p.Data);
                    if (playerCompleted >= playerTotal)
                    {
                        MessageWriter Writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ShareWinner, SendOption.Reliable, -1);
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
    public static bool CheckAndEndGameForFoxHouwaWin(ShipStatus __instance)
    {
        int impostorNum = 0;
        int crewNum = 0;
        bool foxAlive = false;
        foreach (PlayerControl p in CachedPlayer.AllPlayers)
        {
            if (p.IsDead() || p.Data.Disconnected || p == null) continue;

            if (p.IsImpostor()) impostorNum++;
            else if (p.IsCrew()) crewNum++;
            else if (RoleClass.Fox.FoxPlayer.Contains(p) || FireFox.FireFoxPlayer.Contains(p)) foxAlive = true;
        }

        if (impostorNum == crewNum && foxAlive && CustomOptionHolder.FoxCanHouwaWin.GetBool())
        {
            List<PlayerControl> foxPlayers = new(RoleClass.Fox.FoxPlayer);
            foxPlayers.AddRange(FireFox.FireFoxPlayer);
            foreach (PlayerControl p in foxPlayers)
            {
                if (p.IsDead()) continue;
                MessageWriter Writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ShareWinner, SendOption.Reliable, -1);
                Writer.Write(p.PlayerId);
                AmongUsClient.Instance.FinishRpcImmediately(Writer);
                RPCProcedure.ShareWinner(p.PlayerId);

                __instance.enabled = false;
                CustomEndGame((GameOverReason)CustomGameOverReason.FoxWin, false);
            }
            return true;
        };
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
                        MessageWriter Writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ShareWinner, SendOption.Reliable, -1);
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
    public static bool CheckAndEndGameForSafecrackerWin(ShipStatus __instance)
    {
        foreach (PlayerControl p in Safecracker.SafecrackerPlayer)
        {
            if (p == null) continue;
            if (!p.Data.Disconnected)
            {
                var (playerCompleted, playerTotal) = TaskCount.TaskDate(p.Data);
                if (p.IsAlive() && playerCompleted >= playerTotal)
                {
                    MessageWriter Writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ShareWinner, SendOption.Reliable, -1);
                    Writer.Write(p.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(Writer);
                    RPCProcedure.ShareWinner(p.PlayerId);
                    __instance.enabled = false;
                    CustomEndGame((GameOverReason)CustomGameOverReason.SafecrackerWin, false);
                    return true;
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
    public class PlayerStatistics
    {
        public int TeamImpostorsAlive { get; set; }
        public int CrewAlive { get; set; }
        public int TotalAlive { get; set; }
        public int TeamJackalAlive { get; set; }
        public int EgoistAlive { get; set; }
        public int PavlovsDogAlive { get; set; }
        public int PavlovsownerAlive { get; set; }
        public int PavlovsTeamAlive { get; set; }
        public int HitmanAlive { get; set; }
        public int LoversAlive { get; set; }
        public bool IsGuardPavlovs { get; set; }
        public PlayerStatistics()
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
            int numPavlovsDogAlive = 0;
            int numPavlovsownerAlive = 0;
            int numPavlovsTeamAlive = 0;
            int numHitmanAlive = 0;
            int numLoversAlive = 0;

            for (int i = 0; i < GameData.Instance.PlayerCount; i++)
            {
                GameData.PlayerInfo playerInfo = GameData.Instance.AllPlayers[i];
                if (!playerInfo.Disconnected && !playerInfo.Object.IsBot())
                {
                    if (playerInfo.Object.IsAlive())
                    {
                        if (!playerInfo.Object.IsRole(RoleId.OrientalShaman)) numTotalAlive++;
                        if (playerInfo.Object.IsJackalTeamJackal() || playerInfo.Object.IsJackalTeamSidekick())
                        {
                            numTotalJackalTeam++;
                        }
                        else if (playerInfo.Object.IsRole(RoleId.Hitman))
                        {
                            numHitmanAlive++;
                        }
                        else if (playerInfo.Object.IsImpostor())
                        {
                            numImpostorsAlive++;
                        }
                        else if (playerInfo.Object.IsCrew())
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
                            else if (playerInfo.Object.IsRole(RoleId.Hitman))
                            {
                                numHitmanAlive++;
                            }
                            else if (playerInfo.Object.IsPavlovsTeam())
                            {
                                if (playerInfo.Object.IsRole(RoleId.Pavlovsdogs))
                                    numPavlovsDogAlive++;
                                if (playerInfo.Object.IsRole(RoleId.Pavlovsowner))
                                    numPavlovsownerAlive++;
                                numPavlovsTeamAlive++;
                            }
                        }
                        if (playerInfo.Object.IsLovers() || playerInfo.Object.IsRole(RoleId.truelover) || (playerInfo.Object.IsRole(RoleId.Cupid) && !RoleClass.Cupid.CupidLoverPair.ContainsKey(playerInfo.Object.PlayerId))) numLoversAlive++;
                    }
                }
            }
            if (ModeHandler.IsMode(ModeId.HideAndSeek)) numTotalAlive += numImpostorsAlive;
            TeamImpostorsAlive = numImpostorsAlive;
            TotalAlive = numTotalAlive;
            CrewAlive = numCrewAlive;
            TeamJackalAlive = numTotalJackalTeam;
            EgoistAlive = numTotalEgoist;
            PavlovsDogAlive = numPavlovsDogAlive;
            PavlovsownerAlive = numPavlovsownerAlive;
            PavlovsTeamAlive = numPavlovsTeamAlive;
            HitmanAlive = numHitmanAlive;
            LoversAlive = numLoversAlive;
            if (!(IsGuardPavlovs = PavlovsDogAlive > 0))
            {
                foreach (PlayerControl p in RoleClass.Pavlovsowner.PavlovsownerPlayer)
                {
                    if (p == null) continue;
                    if (p.IsDead()) continue;
                    if (!RoleClass.Pavlovsowner.CountData.ContainsKey(p.PlayerId)
                        || RoleClass.Pavlovsowner.CountData[p.PlayerId] > 0)
                    {
                        IsGuardPavlovs = true;
                        break;
                    }
                }
            }
        }
    }
}
