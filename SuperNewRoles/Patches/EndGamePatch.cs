using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using Hazel;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppSystem.CodeDom;
using SuperNewRoles.Helpers;
using SuperNewRoles.Mode;
using SuperNewRoles.Mode.SuperHostRoles;
using SuperNewRoles.Replay;
using SuperNewRoles.Roles;
using SuperNewRoles.Roles.Crewmate;
using SuperNewRoles.Roles.Crewmate.BodyBuilder;
using SuperNewRoles.Roles.Neutral;
using SuperNewRoles.Roles.RoleBases;
using SuperNewRoles.Roles.RoleBases.Interfaces;
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
    OwlWin,
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
    OwlWin,
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
        public RoleId RoleId { get; set; }
        public RoleId GhostRoleId { get; set; }
        public string AttributeRoleName { get; set; }
        public bool isImpostor { get; set; }
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
    #region ProcessWinText
    static Color32 HaisonColor = new(163, 163, 162, byte.MaxValue);
    static Dictionary<WinCondition, (string, Color32)> WinConditionDictionary = new() {
            { WinCondition.HAISON, ("HAISON", HaisonColor) },
            { WinCondition.LoversWin, ("LoversName", RoleClass.Lovers.color) },
            { WinCondition.GodWin, ("GodName", RoleClass.God.color) },
            { WinCondition.JesterWin, ("JesterName", RoleClass.Jester.color) },
            { WinCondition.JackalWin, ("JackalName", RoleClass.Jackal.color) },
            { WinCondition.QuarreledWin, ("QuarreledName", RoleClass.Quarreled.color) },
            { WinCondition.EgoistWin, ("EgoistName", RoleClass.Egoist.color) },
            { WinCondition.WorkpersonWin, ("WorkpersonName", RoleClass.Workperson.color) },
            { WinCondition.FalseChargesWin, ("FalseChargesName", RoleClass.FalseCharges.color) },
            { WinCondition.FoxWin, ("FoxName", RoleClass.Fox.color) },
            { WinCondition.DemonWin, ("DemonName", RoleClass.Demon.color) },
            { WinCondition.ArsonistWin, ("ArsonistName", RoleClass.Arsonist.color) },
            { WinCondition.VultureWin, ("VultureName", RoleClass.Vulture.color) },
            { WinCondition.TunaWin, ("TunaName", RoleClass.Tuna.color) },
            { WinCondition.NeetWin, ("NeetName", RoleClass.Neet.color) },
            { WinCondition.RevolutionistWin, ("RevolutionistName", RoleClass.Revolutionist.color) },
            { WinCondition.SpelunkerWin, ("SpelunkerName", RoleClass.Spelunker.color) },
            { WinCondition.SuicidalIdeationWin, (CustomOptionHolder.SuicidalIdeationWinText.GetBool() ? "SuicidalIdeationWinText" : "SuicidalIdeationName", RoleClass.SuicidalIdeation.color) },
            { WinCondition.HitmanWin, ("HitmanName", RoleClass.Hitman.color) },
            { WinCondition.PhotographerWin, ("PhotographerName", RoleClass.Photographer.color) },
            { WinCondition.StefinderWin, ("StefinderName", RoleClass.Stefinder.color) },
            { WinCondition.PavlovsTeamWin, ("PavlovsTeamWinText", PavlovsDogs.PavlovsColor) },
            { WinCondition.LoversBreakerWin, ("LoversBreakerName", RoleClass.LoversBreaker.color) },
            { WinCondition.NoWinner, ("NoWinner", Color.white) },
            { WinCondition.SafecrackerWin, ("SafecrackerName", Safecracker.color) },
            { WinCondition.TheThreeLittlePigsWin, ("TheThreeLittlePigsName", TheThreeLittlePigs.color) },
            { WinCondition.OrientalShamanWin, ("OrientalShamanName", OrientalShaman.color) },
            { WinCondition.BlackHatHackerWin, ("BlackHatHackerName", BlackHatHacker.color) },
            { WinCondition.MoiraWin, ("MoiraName", Moira.Roleinfo.RoleColor) },
            { WinCondition.CrookWin, ("CrookName", Crook.RoleData.color) },
            { WinCondition.PantsRoyalWin, ("PantsRoyalYouareWinner" ,Mode.PantsRoyal.main.ModeColor) },
            { WinCondition.SaunerWin, ("SaunerRefreshing", Sauner.RoleData.color) },
            { WinCondition.PokerfaceWin, ("PokerfaceName", Pokerface.RoleData.color) },
            { WinCondition.FrankensteinWin, ("FrankensteinName",Frankenstein.color) },
            { WinCondition.OwlWin, ("OwlName", Owl.Roleinfo.RoleColor) },
        };
    public static (string text, Color color, bool Haison) ProcessWinText(GameOverReason gameOverReason, WinCondition winCondition)
    {
        string text = "";
        Color RoleColor = Color.white;
        if (WinConditionDictionary.ContainsKey(winCondition))
        {
            text = WinConditionDictionary[winCondition].Item1;
            RoleColor = WinConditionDictionary[winCondition].Item2;
        }
        else
        {
            switch (gameOverReason)
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

        if (winCondition == WinCondition.HAISON)
        {
            text = ModTranslation.GetString("HaisonName");
            RoleColor = HaisonColor;
        }
        else if (winCondition == WinCondition.NoWinner)
        {
            text = ModTranslation.GetString("NoWinner");
            RoleColor = Color.white;
        }

        bool haison = false;
        if (text == "HAISON")
        {
            haison = true;
            text = ModTranslation.GetString("HaisonName");
            RoleColor = HaisonColor;
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
            if (player.IsDead())
                continue;
            if (IsOpptexton || haison)
                continue;
            IsOpptexton = true;
            text = text + "&" + ModHelpers.Cs(RoleClass.Opportunist.color, ModTranslation.GetString("OpportunistName"));
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
                    if (!Temp1)
                        continue;
                    if (IsLovetexton || haison)
                        continue;
                    IsLovetexton = true;
                    text = text + "&" + CustomOptionHolder.Cs(RoleClass.Lovers.color, "LoversName");
                }
            }
        }

        if (haison) text = ModTranslation.GetString("HaisonName");
        else if (winCondition is WinCondition.PantsRoyalWin or WinCondition.SaunerWin) { }
        else if (text == ModTranslation.GetString("NoWinner")) text = ModTranslation.GetString("NoWinnerText");
        else if (text == ModTranslation.GetString("GodName")) text += " " + ModTranslation.GetString("GodWinText");
        else text = string.Format(text + " " + ModTranslation.GetString("WinName"));

        return (text, RoleColor, haison);
    }
    #endregion

    public static void Postfix(EndGameManager __instance)
    {
        AprilFoolsManager.SetRandomModMode();

        foreach (PoolablePlayer pb in __instance.transform.GetComponentsInChildren<PoolablePlayer>())
        {
            UnityEngine.Object.Destroy(pb.gameObject);
        }
        int num = Mathf.CeilToInt(7.5f);
        IEnumerable<CachedPlayerData> list = EndGameResult.CachedWinners.ToList().OrderBy(delegate (CachedPlayerData b)
        {
            return !b.IsYou ? 0 : -1;
        });
        int i = -1;
        foreach (CachedPlayerData CachedPlayerData2 in list)
        {
            i++;
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
            if (CachedPlayerData2.IsDead)
            {
                poolablePlayer.SetBodyAsGhost();
                poolablePlayer.SetDeadFlipX(i % 2 == 0);
            }
            else
            {
                poolablePlayer.SetFlipX(i % 2 == 0);
            }
            poolablePlayer.UpdateFromPlayerOutfit(CachedPlayerData2.Outfit, PlayerMaterial.MaskType.None, CachedPlayerData2.IsDead, true);
            poolablePlayer.cosmetics.nameText.color = Color.white;
            poolablePlayer.cosmetics.nameText.transform.localScale = new Vector3(1f / vector.x, 1f / vector.y, 1f / vector.z);
            poolablePlayer.cosmetics.nameText.transform.localPosition = new Vector3(poolablePlayer.cosmetics.nameText.transform.localPosition.x, poolablePlayer.cosmetics.nameText.transform.localPosition.y - 0.8f, -15f);
            poolablePlayer.cosmetics.nameText.text = CachedPlayerData2.PlayerName;

            foreach (var data in AdditionalTempData.playerRoles)
            {
                if (data.PlayerName != CachedPlayerData2.PlayerName) continue;
                poolablePlayer.cosmetics.nameText.text = $"{data.PlayerName}{data.NameSuffix}\n{string.Join("\n", CustomRoles.GetRoleNameOnColor(data.RoleId, IsImpostorReturn: CachedPlayerData2.IsImpostor))}";

                if (data.RoleId != RoleId.BodyBuilder || data.TasksCompleted != data.TasksTotal)
                    continue;

                var prefab = BodyBuilder.getPrefab((byte)BodyBuilder.PosingIdRange.Next());
                var pose = UnityEngine.Object.Instantiate(prefab, poolablePlayer.transform);
                pose.gameObject.transform.position = poolablePlayer.transform.position;
                pose.transform.localPosition = new(0f, 1f, 0f);
                pose.transform.localScale *= 1.5f;
                poolablePlayer.cosmetics.gameObject.SetActive(false);
                poolablePlayer.cosmetics.currentBodySprite.BodySprite.gameObject.SetActive(false);

                var spriteRenderer = pose.GetComponent<SpriteRenderer>();
                spriteRenderer.sharedMaterial = FastDestroyableSingleton<HatManager>.Instance.PlayerMaterial;
                spriteRenderer.maskInteraction = SpriteMaskInteraction.None;
                PlayerMaterial.SetMaskLayerBasedOnLocalPlayer(spriteRenderer, false);
                PlayerMaterial.SetColors(poolablePlayer.ColorId, spriteRenderer);
                spriteRenderer.color = new(1f, 1f, 1f, data.Status != FinalStatus.Alive ? 0.5f : 1f);
            }
        }

        GameObject bonusTextObject = UnityEngine.Object.Instantiate(__instance.WinText.gameObject);
        bonusTextObject.transform.position = new Vector3(__instance.WinText.transform.position.x, __instance.WinText.transform.position.y - 0.8f, __instance.WinText.transform.position.z);
        bonusTextObject.transform.localScale = new Vector3(0.7f, 0.7f, 1f);
        textRenderer = bonusTextObject.GetComponent<TMPro.TMP_Text>();
        textRenderer.text = "";
        (string text, Color RoleColor, bool haison) = ProcessWinText(AdditionalTempData.gameOverReason, AdditionalTempData.winCondition);

        textRenderer.color = AdditionalTempData.winCondition is WinCondition.HAISON ? Color.clear : RoleColor;

        __instance.BackgroundBar.material.SetColor("_Color", RoleColor);

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
        Logger.Info("WinCondition:" + AdditionalTempData.winCondition.ToString());
        if (AdditionalTempData.winCondition != WinCondition.HAISON)
            textRenderer.text = text;
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
                string roleText = CustomRoles.GetRoleNameOnColor(data.RoleId, IsImpostorReturn: data.isImpostor) + data.AttributeRoleName;
                if (data.GhostRoleId != RoleId.DefaultRole)
                {
                    roleText += $" → {CustomRoles.GetRoleNameOnColor(data.GhostRoleId)}";
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
        OnGameEndPatch.WinText = ModHelpers.Cs(RoleColor, text);
        IsHaison = false;
        GameHistoryManager.Send(textRenderer.text, RoleColor);

        LoggerPlus.EndGameAutoSave();
    }
}

public class CustomPlayerData
{
    public string name;
    public bool IsWin;
    public bool isImpostor;
    public FinalStatus finalStatus;
    public int CompleteTask;
    public int TotalTask;
    public RoleId? role;
    public CustomPlayerData(NetworkedPlayerInfo p, GameOverReason gameOverReason)
    {
        name = p.PlayerName;
        try
        {
            (CompleteTask, TotalTask) = TaskCount.TaskDate(p);
        }
        catch { }
        role = null;
        if (p.Object != null)
        {
            role = p.Object.GetRole();
            isImpostor = p.Role.IsImpostor;
        }
        finalStatus = FinalStatus.Alive;
        if (p.Disconnected)
            finalStatus = FinalStatus.Disconnected;
        else if (p.IsDead && FinalStatusPatch.FinalStatusData.FinalStatuses.ContainsKey(p.PlayerId))
            finalStatus = FinalStatusPatch.FinalStatusData.FinalStatuses[p.PlayerId];
        else if (p.IsDead)
            finalStatus = FinalStatus.Exiled;
        else if (gameOverReason == GameOverReason.ImpostorBySabotage && !p.Role.IsImpostor)
            finalStatus = FinalStatus.Sabotage;
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

        if (ModeHandler.IsMode(ModeId.SuperHostRoles) && EndData != null)
            endGameResult.GameOverReason = (GameOverReason)EndData;

        AdditionalTempData.gameOverReason = endGameResult.GameOverReason;
        foreach (PlayerControl p in CachedPlayer.AllPlayers)
        {
            try
            {
                p.resetChange();
            }
            catch { }
        }
        if (!ReplayManager.IsReplayMode && ConfigRoles.IsSendAnalytics.Value && !SuperNewRolesPlugin.IsBeta && !DebugModeManager.IsDebugMode)
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
    private static List<NetworkedPlayerInfo> ProcessGetWinnersToRemove()
    {
        // Remove Jester, Arsonist, Vulture, Jackal, former Jackals and Sidekick from winners (if they win, they'll be readded)

        List<PlayerControl> notWinners = new();

        notWinners.AddRanges([RoleClass.Jester.JesterPlayer,
            RoleClass.Madmate.MadmatePlayer,
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
            RoleClass.Revolutionist.RevolutionistPlayer,
            RoleClass.SuicidalIdeation.SuicidalIdeationPlayer,
            RoleClass.Spelunker.SpelunkerPlayer,
            RoleClass.Hitman.HitmanPlayer,
            RoleClass.PartTimer.PartTimerPlayer,
            RoleClass.Photographer.PhotographerPlayer,
            RoleClass.Stefinder.StefinderPlayer,
            RoleClass.LoversBreaker.LoversBreakerPlayer,
            Roles.Impostor.MadRole.Worshiper.RoleData.Player,
            Safecracker.SafecrackerPlayer,
            FireFox.FireFoxPlayer,
            OrientalShaman.OrientalShamanPlayer,
            OrientalShaman.ShermansServantPlayer,
            TheThreeLittlePigs.TheFirstLittlePig.Player,
            TheThreeLittlePigs.TheSecondLittlePig.Player,
            TheThreeLittlePigs.TheThirdLittlePig.Player,
            BlackHatHacker.BlackHatHackerPlayer,
            Roles.Impostor.MadRole.MadRaccoon.RoleData.Player,
            Sauner.RoleData.Player,
            Pokerface.RoleData.Player,
            Crook.RoleData.Player,
            Frankenstein.FrankensteinPlayer,
            RoleClass.Dependents.DependentsPlayer,
            ]);
        foreach (SatsumaAndImo satsuma in RoleBaseManager.GetRoleBases<SatsumaAndImo>())
            notWinners.Add(satsuma.Player);
        foreach (PlayerControl player in PlayerControl.AllPlayerControls)
        {
            if (!player.IsNeutral() || notWinners.Contains(player))
                continue;
            notWinners.Add(player);
        }

        foreach (PlayerControl p in RoleClass.Survivor.SurvivorPlayer)
        {
            if (p.IsAlive())
                continue;
            notWinners.Add(p);
        }

        List<NetworkedPlayerInfo> winnersToRemove = new();
        foreach (NetworkedPlayerInfo winner in GameData.Instance.AllPlayers)
        {
            if (notWinners.Any(x => x.Data.PlayerName == winner.PlayerName)) winnersToRemove.Add(winner);
        }
        return winnersToRemove;
    }
    private static Dictionary<string, WinCondition> WinnerisWinnerPlayers = new()
    {
        { CustomGameOverReason.JesterWin.ToString(), WinCondition.JesterWin },
        { CustomGameOverReason.MadJesterWin.ToString(), WinCondition.MadJesterWin },
        { CustomGameOverReason.WorkpersonWin.ToString(), WinCondition.WorkpersonWin },
        { CustomGameOverReason.FalseChargesWin.ToString(), WinCondition.FalseChargesWin },
        { CustomGameOverReason.VultureWin.ToString(), WinCondition.VultureWin },
        { CustomGameOverReason.RevolutionistWin.ToString(), WinCondition.RevolutionistWin },
        { CustomGameOverReason.SuicidalIdeationWin.ToString(), WinCondition.SuicidalIdeationWin },
        { CustomGameOverReason.PhotographerWin.ToString(), WinCondition.PhotographerWin },
        { CustomGameOverReason.SafecrackerWin.ToString(), WinCondition.SafecrackerWin },
        { CustomGameOverReason.BlackHatHackerWin.ToString(), WinCondition.BlackHatHackerWin },
        { CustomGameOverReason.SaunerWin.ToString(), WinCondition.SaunerWin },
    };

    #region ProcessEndgames
    /// <summary>
    /// ゲーム終了の処理を行う
    /// </summary>
    /// <returns>勝利情報</returns>
    public static (HashSet<NetworkedPlayerInfo> Winners, WinCondition winCondition, HashSet<NetworkedPlayerInfo> WillRevivePlayers) HandleEndGameProcess(GameOverReason gameOverReason)
    {
        HashSet<NetworkedPlayerInfo> winners = new();
        HashSet<NetworkedPlayerInfo> WillRevivePlayers = new();
        WinCondition winCondition = WinCondition.BugEnd;

        if (EndGameManagerSetUpPatch.IsHaison)
        {
            foreach (PlayerControl p in CachedPlayer.AllPlayers)
            {
                if (p.IsBot())
                    continue;
                winners.Add(p.Data);
            }
            return (winners, WinCondition.HAISON, WillRevivePlayers);
        }

        bool IsProcessReplaceWin = true;

        bool saboWin = gameOverReason == GameOverReason.ImpostorBySabotage;
        bool ImpostorWin = gameOverReason is GameOverReason.ImpostorByKill or GameOverReason.ImpostorBySabotage or GameOverReason.ImpostorByVote;
        bool TaskerWin = gameOverReason == (GameOverReason)CustomGameOverReason.TaskerWin;
        bool QuarreledWin = gameOverReason == (GameOverReason)CustomGameOverReason.QuarreledWin;
        bool JackalWin = gameOverReason == (GameOverReason)CustomGameOverReason.JackalWin;
        bool EgoistWin = gameOverReason == (GameOverReason)CustomGameOverReason.EgoistWin;
        bool FoxWin = gameOverReason == (GameOverReason)CustomGameOverReason.FoxWin;
        bool DemonWin = gameOverReason == (GameOverReason)CustomGameOverReason.DemonWin;
        bool ArsonistWin = gameOverReason == (GameOverReason)CustomGameOverReason.ArsonistWin;
        bool NeetWin = gameOverReason == (GameOverReason)CustomGameOverReason.NeetWin;
        bool HitmanWin = gameOverReason == (GameOverReason)CustomGameOverReason.HitmanWin;
        bool PavlovsTeamWin = gameOverReason == (GameOverReason)CustomGameOverReason.PavlovsTeamWin;
        bool LoversBreakerWin = gameOverReason == (GameOverReason)CustomGameOverReason.LoversBreakerWin;
        bool NoWinner = gameOverReason == (GameOverReason)CustomGameOverReason.NoWinner;
        bool CrewmateWin = gameOverReason is (GameOverReason)CustomGameOverReason.CrewmateWin or GameOverReason.HumansByVote or GameOverReason.HumansByTask or GameOverReason.ImpostorDisconnect;
        bool BUGEND = gameOverReason == (GameOverReason)CustomGameOverReason.BugEnd;
        bool CrookWin = gameOverReason == (GameOverReason)CustomGameOverReason.CrookWin;
        bool OwlWin = gameOverReason == (GameOverReason)CustomGameOverReason.OwlWin;
        if (ModeHandler.IsMode(ModeId.SuperHostRoles, ModeId.CopsRobbers) && EndData != null)
        {
            gameOverReason = (GameOverReason)EndData;
            JackalWin = EndData == CustomGameOverReason.JackalWin;
            EgoistWin = EndData == CustomGameOverReason.EgoistWin;
            DemonWin = EndData == CustomGameOverReason.DemonWin;
            ArsonistWin = EndData == CustomGameOverReason.ArsonistWin;
            QuarreledWin = EndData == CustomGameOverReason.QuarreledWin;

            /*
            JesterWin = EndData == CustomGameOverReason.JesterWin;
            MadJesterWin = EndData == CustomGameOverReason.MadJesterWin;
            EgoistWin = EndData == CustomGameOverReason.EgoistWin;
            WorkpersonWin = EndData == CustomGameOverReason.WorkpersonWin;
            FalseChargesWin = EndData == CustomGameOverReason.FalseChargesWin;
            FoxWin = EndData == CustomGameOverReason.FoxWin;
            DemonWin = EndData == CustomGameOverReason.DemonWin;
            ArsonistWin = EndData == CustomGameOverReason.ArsonistWin;
            VultureWin = EndData == CustomGameOverReason.VultureWin;
            NeetWin = EndData == CustomGameOverReason.NeetWin;
            CrookWin = EndData == CustomGameOverReason.CrookWin;*/
        }
        if (WinnerisWinnerPlayers.TryGetValue(((CustomGameOverReason)gameOverReason).ToString(), out WinCondition condition))
        {
            WillRevivePlayers.Add(WinnerPlayer.Data);
            winners = [WinnerPlayer.Data];
            winCondition = condition;
            IsProcessReplaceWin = false;
        }
        else if (JackalWin)
        {
            foreach (var cp in PlayerControl.AllPlayerControls)
            {
                if (!cp.IsJackalTeam())
                    continue;
                winners.Add(cp.Data);
            }
            winCondition = WinCondition.JackalWin;
        }
        else if (EgoistWin)
        {
            foreach (PlayerControl p in RoleClass.Egoist.EgoistPlayer)
            {
                if (p.IsDead())
                    continue;
                winners.Add(WinnerPlayer.Data);
            }
            winCondition = WinCondition.EgoistWin;
        }
        else if (DemonWin)
        {
            foreach (PlayerControl player in RoleClass.Demon.DemonPlayer)
            {
                if (!Demon.IsWin(player))
                    continue;
                winners.Add(WinnerPlayer.Data);
            }
            winCondition = WinCondition.DemonWin;
            IsProcessReplaceWin = false;
        }
        else if (ArsonistWin)
        {
            foreach (PlayerControl player in RoleClass.Arsonist.ArsonistPlayer)
            {
                if (!Arsonist.IsArsonistWinFlag())
                    continue;
                SuperNewRolesPlugin.Logger.LogInfo("アーソニストがEndGame");
                winners.Add(WinnerPlayer.Data);
            }
            winCondition = WinCondition.ArsonistWin;
            IsProcessReplaceWin = false;
        }
        else if (HitmanWin)
        {
            if (WinnerPlayer == null)
            {
                foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                    if (p.IsRole(RoleId.Hitman))
                        WinnerPlayer = p;
                if (WinnerPlayer == null)
                {
                    Logger.Error("エラー:殺し屋が生存していませんでした", "HitmanWin");
                    WinnerPlayer = PlayerControl.LocalPlayer;
                }
            }
            winners = [WinnerPlayer.Data];
            winCondition = WinCondition.HitmanWin;
            IsProcessReplaceWin = false;
        }
        else if (PavlovsTeamWin)
        {
            foreach (PlayerControl p in PlayerControl.AllPlayerControls)
            {
                if (p.IsPavlovsTeam())
                    winners.Add(p.Data);
            }
            winCondition = WinCondition.PavlovsTeamWin;
        }
        else if (QuarreledWin)
        {
            winners = new();
            List<PlayerControl> winplays = new()
                {
                    WinnerPlayer,
                    WinnerPlayer.GetOneSideQuarreled()
                };
            foreach (PlayerControl player in winplays)
            {
                WillRevivePlayers.Add(player.Data);
                winners.Add(player.Data);
            }
            winCondition = WinCondition.QuarreledWin;
        }
        else if (CrewmateWin)
        {
            var d = ProcessGetWinnersToRemove();
            foreach (NetworkedPlayerInfo player in GameData.Instance.AllPlayers)
            {
                if (player.Object != null && !player.Object.IsCrew())
                    continue;
                if (player.Role.IsImpostor)
                    continue;
                if (d.Any(x => x.PlayerId == player.PlayerId))
                    continue;
                winners.Add(player);
            }
            foreach (SatsumaAndImo satsumaAndImo in RoleBaseManager.GetRoleBases<SatsumaAndImo>())
            {
                if (satsumaAndImo.TeamState == SatsumaAndImo.SatsumaTeam.Crewmate)
                    winners.Add(satsumaAndImo.Player.Data);//さつまいもも勝ち
            }
        }
        else if (TaskerWin)
        {
            winCondition = WinCondition.TaskerWin;
        }
        else if (LoversBreakerWin)
        {
            if (WinnerPlayer != null)
            {
                winners = [WinnerPlayer.Data];
            }
            else
            {
                foreach (byte playerId in RoleClass.LoversBreaker.CanEndGamePlayers)
                {
                    winners.Add(GameData.Instance.GetPlayerById(playerId));
                }
            }
            winCondition = WinCondition.LoversBreakerWin;
            IsProcessReplaceWin = false;
        }
        else if (OwlWin)
        {
            Logger.Info($"Owl Win : WinnerPlayer : {WinnerPlayer.PlayerId}", "Owl");
            (winners = new()).Add(WinnerPlayer.Data);
            winCondition = WinCondition.OwlWin;
        }
        if (ImpostorWin)
        {
            foreach (NetworkedPlayerInfo player in GameData.Instance.AllPlayers)
            {
                if (player.Role.IsImpostor)
                    winners.Add(player);
            }
        }

        if (winners.Any(x => x.Role.IsImpostor))
        {
            foreach (PlayerControl cp in CachedPlayer.AllPlayers)
                if (cp.IsMadRoles() ||
                    cp.IsRole(RoleId.MadKiller, RoleId.Dependents)
                )
                    winners.Add(cp.Data);
        }


        //単独勝利系統
        //下に行くほど優先度が高い
        bool allowAdditionalWins = true;
        if (IsProcessReplaceWin) ProcessReplaceWin(ref winners, gameOverReason, ref winCondition, out allowAdditionalWins);

        //追加勝利系
        if (allowAdditionalWins) ProcessAdditionalWin(ref winners, gameOverReason, ref winCondition);

        if (ModeHandler.IsMode(ModeId.BattleRoyal))
        {
            winners = [];
            if (Mode.BattleRoyal.Main.IsTeamBattle)
            {
                foreach (PlayerControl p in Mode.BattleRoyal.Main.Winners)
                    winners.Add(p.Data);
            }
            else
            {
                foreach (PlayerControl p in CachedPlayer.AllPlayers)
                {
                    if (p.IsDead())
                        continue;
                    winners.Add(p.Data);
                }
            }
            winCondition = WinCondition.Default;
        }
        if (ModeHandler.IsMode(ModeId.Zombie))
        {
            winners = new();
            if (gameOverReason == GameOverReason.ImpostorByKill)
            {
                winCondition = WinCondition.Default;
                foreach (PlayerControl p in CachedPlayer.AllPlayers)
                {
                    if (p.CurrentOutfit.ColorId != 2)
                        continue;
                    winners.Add(p.Data);
                }
            }
            else
            {
                winCondition = WinCondition.WorkpersonWin;
            }
        }
        int i = 0;
        foreach (PlayerControl p in CachedPlayer.AllPlayers)
        {
            if (p.IsAlive())
                break;
            i++;
        }
        if (NoWinner || winners.Count <= 0)
        {
            winners = new();
            winCondition = WinCondition.NoWinner;
        }
        if (ModeHandler.IsMode(ModeId.PantsRoyal))
        {
            if (WinnerPlayer != null)
            {
                winners = [WinnerPlayer.Data];
                winCondition = WinCondition.PantsRoyalWin;
            }
            else
            {
                winners = new();
                foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                {
                    if (player.Data.Role.Role is AmongUs.GameOptions.RoleTypes.CrewmateGhost or
                        AmongUs.GameOptions.RoleTypes.Crewmate)
                    {
                        winners.Add(player.Data);
                        break;
                    }
                }
                if (winners.Count > 0)
                {
                    winCondition = WinCondition.PantsRoyalWin;
                }
                else
                {
                    winCondition = WinCondition.NoWinner;
                }
            }
        }
        return (winners, winCondition, WillRevivePlayers);
    }
    private static void ProcessReplaceWin(ref HashSet<NetworkedPlayerInfo> winners, GameOverReason gameOverReason, ref WinCondition winCondition, out bool allowAdditionalWins)
    {
        allowAdditionalWins = true;
        bool spereseted = false;
        IHijackingWinner.Rank last = IHijackingWinner.Rank.Rank1;
        foreach (IHijackingWinner hijacking in RoleBaseManager.GetInterfaces<IHijackingWinner>().OrderBy(x => x.Priority))
        {
            if (!hijacking.CanWin(gameOverReason, winCondition)) continue;
            if (!spereseted || last < hijacking.Priority)
            {
                winners = [];
                allowAdditionalWins = true;
            }
            spereseted = true;
            last = hijacking.Priority;
            winners.Add((hijacking as RoleBase).Player.Data);
            winCondition = hijacking.Condition;
            allowAdditionalWins &= hijacking.AllowAdditionalWins;
        }

        spereseted = false;
        foreach (PlayerControl player in RoleClass.Neet.NeetPlayer)
        {
            if (player.IsDead() || RoleClass.Neet.IsAddWin)
                continue;
            if (!spereseted)
                winners = [];
            spereseted = true;
            allowAdditionalWins = true;
            winners.Add(player.Data);
            winCondition = WinCondition.NeetWin;
        }
        spereseted = false;
        foreach (PlayerControl player in RoleClass.God.GodPlayer)
        {
            if (player.IsDead())
                continue;
            var (Complete, all) = TaskCount.TaskDateNoClearCheck(player.Data);
            if (!RoleClass.God.IsTaskEndWin || Complete >= all)
            {
                if (!spereseted)
                    winners = [];
                spereseted = true;
                allowAdditionalWins = true;
                winners.Add(player.Data);
                winCondition = WinCondition.GodWin;
            }
        }
        spereseted = false;
        foreach (PlayerControl player in OrientalShaman.OrientalShamanPlayer)
        {
            if (!OrientalShaman.OrientalShamanCrewTaskWinHijack.GetBool() &&
                AdditionalTempData.gameOverReason == GameOverReason.HumansByTask) break;
            if (player.IsDead())
                continue;
            if (OrientalShaman.OrientalShamanWinTask.GetBool())
            {
                var (completed, total) = TaskCount.TaskDate(player.Data);
                if (completed < total) continue;
            }
            if (!spereseted)
                winners = [];
            spereseted = true;
            allowAdditionalWins = true;
            winners.Add(player.Data);
            if (OrientalShaman.OrientalShamanCausative.ContainsKey(player.PlayerId))
            {
                PlayerControl causativePlayer = ModHelpers.PlayerById(OrientalShaman.OrientalShamanCausative[player.PlayerId]);
                if (causativePlayer) winners.Add(causativePlayer.Data);
            }
            winCondition = WinCondition.OrientalShamanWin;
        }
        spereseted = false;
        foreach (PlayerControl player in RoleClass.Tuna.TunaPlayer)
        {
            if (player.IsDead() || RoleClass.Tuna.IsTunaAddWin)
                continue;
            if (!spereseted)
                winners = [];
            spereseted = true;
            winners.Add(player.Data);
            winCondition = WinCondition.TunaWin;
        }
        spereseted = false;
        foreach (PlayerControl player in RoleClass.Stefinder.StefinderPlayer)
        {
            if (player.IsDead() || !CustomOptionHolder.StefinderSoloWin.GetBool())
                continue;
            if (RoleClass.Stefinder.IsKillPlayer.Contains(player.PlayerId))
            {
                if (gameOverReason is not (GameOverReason.ImpostorByKill or
                    GameOverReason.ImpostorBySabotage or
                    GameOverReason.ImpostorByVote or
                    GameOverReason.ImpostorDisconnect))
                    continue;
            }
            else if (gameOverReason is not (GameOverReason.HumansByTask or
                GameOverReason.HumansByVote or
                GameOverReason.HumansDisconnect))
                continue;
            if (!spereseted)
                winners = [];
            spereseted = true;
            allowAdditionalWins = true;
            winners.Add(player.Data);
            winCondition = WinCondition.StefinderWin;
        }
        spereseted = false;
        foreach (List<PlayerControl> plist in RoleClass.Lovers.LoversPlayer)
        {
            if (!RoleClass.Lovers.IsSingleTeam)
                break;
            bool IsWinLovers = false;
            foreach (PlayerControl player in plist)
            {
                if (player.IsDead())
                    continue;
                IsWinLovers = true;
            }
            if (!IsWinLovers)
                continue;
            if (!spereseted)
                winners = [];
            spereseted = true;
            allowAdditionalWins = true;
            foreach (PlayerControl player in plist)
            {
                winners.Add(player.Data);
                Cupid cupid = RoleBaseManager.GetRoleBases<Cupid>().FirstOrDefault(x => x.currentPair != null && x.currentPair.PlayerId == player.PlayerId);
                if (cupid != null)
                {
                    PlayerControl cPlayer = cupid.Player;
                    if (cPlayer != null && cPlayer.IsRole(RoleId.Cupid))
                        winners.Add(cupid.Player.Data);
                }
                winCondition = WinCondition.LoversWin;
            }
        }
        spereseted = false;
        //ポーカーフェイス勝利判定
        foreach (Pokerface.PokerfaceTeam team in Pokerface.RoleData.PokerfaceTeams)
        {
            if (!team.CanWin())
                continue;
            if (!spereseted)
                winners = [];
            spereseted = true;
            allowAdditionalWins = false;
            foreach (PlayerControl teammember in team.TeamPlayers)
                //ポーカーフェイスじゃない場合を考慮する
                if (teammember.IsRole(RoleId.Pokerface))
                    //生存者のみ勝利の設定が無効もしくは対象が生存している場合は追加する
                    if (!Pokerface.CustomOptionData.WinnerOnlyAlive.GetBool() ||
                        teammember.IsAlive())
                        winners.Add(teammember.Data);
            winCondition = WinCondition.PokerfaceWin;
        }
        spereseted = false;
        foreach (PlayerControl player in RoleClass.Spelunker.SpelunkerPlayer)
        {
            if (player.IsDead())
                continue;
            if (!spereseted)
                winners = [];
            spereseted = true;
            allowAdditionalWins = true;
            winners.Add(player.Data);
            winCondition = WinCondition.SpelunkerWin;
        }
        spereseted = false;
        foreach (List<PlayerControl> plist in TheThreeLittlePigs.TheThreeLittlePigsPlayer)
        {
            if (winCondition is WinCondition.LoversBreakerWin or WinCondition.SafecrackerWin or WinCondition.JesterWin or
                                WinCondition.VultureWin or WinCondition.WorkpersonWin or WinCondition.FalseChargesWin or
                                WinCondition.DemonWin or WinCondition.SuicidalIdeationWin or WinCondition.PhotographerWin or
                                WinCondition.RevolutionistWin or WinCondition.QuarreledWin or WinCondition.BlackHatHackerWin or
                                WinCondition.OwlWin) break;
            if (!TheThreeLittlePigs.IsTheThreeLittlePigs(plist) || plist.IsAllDead())
                continue;
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
            if (isAllAlive)
            {
                if (!spereseted)
                    winners = [];
                spereseted = true;
                allowAdditionalWins = false;
                foreach (PlayerControl player in plist)
                {
                    if (!TheThreeLittlePigs.IsTheThreeLittlePigs(player))
                        continue;
                    winners.Add(player.Data);
                    winCondition = WinCondition.TheThreeLittlePigsWin;
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
                    if (!spereseted)
                        winners = [];
                    spereseted = true;
                    allowAdditionalWins = false;
                    foreach (PlayerControl player in plist)
                    {
                        if (!TheThreeLittlePigs.IsTheThreeLittlePigs(player)) continue;
                        winners.Add(player.Data);
                        winCondition = WinCondition.TheThreeLittlePigsWin;
                    }
                }
            }
        }
        spereseted = false;
        foreach (KeyValuePair<byte, int> data in Frankenstein.KillCount)
        {
            //勝利に必要なキル数を満たしているか
            if (data.Value > 0)
                continue;
            //生存していなければ勝利できない
            NetworkedPlayerInfo FrankenPlayer = GameData.Instance.GetPlayerById(data.Key);
            if (FrankenPlayer.IsDead())
                continue;
            if (!spereseted)
                winners = [];
            spereseted = true;
            allowAdditionalWins = false;
            winners.Add(FrankenPlayer);
            winCondition = WinCondition.FrankensteinWin;
        }
        spereseted = false;
        // 詐欺師は, 勝利判定が実行される前に既に勝利条件を満たしている為, 狐の次の勝利順位 (勝利条件を満たす : MeetingHud.Start, 勝利判定 : SpawnInMinigame.Begin)
        if (Crook.RoleData.FirstWinFlag)
        {
            (bool crookFinalWinFlag, List<PlayerControl> crookWinners) = Crook.DecisionOfVictory.GetTheLastDecisionAndWinners();
            if (crookFinalWinFlag) // 最終的な勝利条件(受給回数, 生存, 最終の保管金の受領場所(追放処理)にたどり着いた) を 満たしている詐欺師がいたら
            {
                if (!spereseted)
                    winners = [];
                spereseted = true;
                foreach (var winner in crookWinners)
                {
                    Logger.Info($"{winner.name}は勝利リストに入った", "EndGame CrookWin");
                    winners.Add(winner.Data);
                }
                winCondition = WinCondition.CrookWin;
            }
        }
        spereseted = false;
        List<PlayerControl> foxPlayers = new(RoleClass.Fox.FoxPlayer);
        foxPlayers.AddRange(FireFox.FireFoxPlayer);
        foreach (PlayerControl player in foxPlayers)
        {
            if (player.IsDead())
                continue;
            if (!spereseted)
                winners = [];
            winners.Add(player.Data);
            winCondition = WinCondition.FoxWin;
            spereseted = true;
        }
    }
    private static void ProcessAdditionalWin(ref HashSet<NetworkedPlayerInfo> winners, GameOverReason gameOverReason, ref WinCondition winCondition)
    {
        foreach (PlayerControl player in PlayerControl.AllPlayerControls)
        {
            if (player.GetRoleBase() is IAdditionalWinner additionalWinner)
            {
                IAdditionalWinner.AdditionalWinData additionalWinData = additionalWinner.CanWin();
                if (additionalWinData.CanWin)
                {
                    winners.Add(player.Data);
                    winCondition = additionalWinData.winCondition;
                }
            }
        }
        foreach (PlayerControl p in RoleClass.Tuna.TunaPlayer)
        {
            if (p.IsDead() || !RoleClass.Tuna.IsTunaAddWin)
                    continue;
            winners.Add(p.Data);
        }
        foreach (PlayerControl p in RoleClass.Neet.NeetPlayer)
        {
            if (p.IsDead() || !RoleClass.Neet.IsAddWin)
                continue;
            winners.Add(p.Data);
        }
        foreach (PlayerControl p in RoleClass.SuicidalIdeation.SuicidalIdeationPlayer)
        {
            var (playerCompleted, playerTotal) = TaskCount.TaskDate(p.Data);
            if (p.IsAlive() && playerTotal > playerCompleted)
            {
                winners.Add(p.Data);
            }
        }
        foreach (PlayerControl player in RoleClass.Opportunist.OpportunistPlayer)
        {
            if (player.IsDead())
                continue;
            winners.Add(player.Data);
        }
        foreach (PlayerControl player in RoleClass.Revolutionist.RevolutionistPlayer)
        {
            if (RoleClass.Revolutionist.IsAddWin && (!RoleClass.Revolutionist.IsAddWinAlive || player.IsAlive()) && !winners.Contains(player.Data))
            {
                winners.Add(player.Data);
            }
        }
        foreach (PlayerControl player in RoleClass.Stefinder.StefinderPlayer)
        {
            if (player.IsDead() || CustomOptionHolder.StefinderSoloWin.GetBool())
                continue;
            if (!RoleClass.Stefinder.IsKillPlayer.Contains(player.PlayerId) &&
               (AdditionalTempData.gameOverReason == GameOverReason.HumansByTask ||
                AdditionalTempData.gameOverReason == GameOverReason.HumansByVote ||
                AdditionalTempData.gameOverReason == GameOverReason.HumansDisconnect))
            {
                winners.Add(player.Data);
            }
            if (RoleClass.Stefinder.IsKillPlayer.Contains(player.PlayerId) &&
               (AdditionalTempData.gameOverReason == GameOverReason.ImpostorByKill ||
                AdditionalTempData.gameOverReason == GameOverReason.ImpostorBySabotage ||
                AdditionalTempData.gameOverReason == GameOverReason.ImpostorByVote ||
                AdditionalTempData.gameOverReason == GameOverReason.ImpostorDisconnect))
            {
                winners.Add(player.Data);
            }
        }
        foreach (List<PlayerControl> plist in RoleClass.Lovers.LoversPlayer)
        {
            if (RoleClass.Lovers.IsSingleTeam)
                break;
            bool IsWinLovers = false;
            foreach (PlayerControl player in plist)
            {
                if (player.IsDead())
                    continue;
                IsWinLovers = true;
            }
            if (!IsWinLovers)
                continue;
            foreach (PlayerControl player in plist)
            {
                winners.Add(player.Data);
                Cupid cupid = RoleBaseManager.GetRoleBases<Cupid>().FirstOrDefault(x => x.currentPair != null && x.currentPair.PlayerId == player.PlayerId);
                if (cupid != null)
                {
                    PlayerControl cPlayer = cupid.Player;
                    if (cPlayer != null && cPlayer.IsRole(RoleId.Cupid))
                        winners.Add(cupid.Player.Data);
                }
            }
        }
        foreach (KeyValuePair<PlayerControl, byte> PartTimerData in (Dictionary<PlayerControl, byte>)RoleClass.PartTimer.Data) //フリーター
        {
            PlayerControl PartTimerValue = ModHelpers.PlayerById(PartTimerData.Value);
            if (winners.Any(x => x.PlayerName == PartTimerValue.Data.PlayerName))
            {
                winners.Add(PartTimerData.Key.Data);
            }
        }
    }
    #endregion

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
            if (p == null ||
                p.Object == null ||
                p.Object.IsBot())
                continue;
            //var p = pc.Data;
            RoleId playerrole = p.Object.GetRole();
            if (RoleClass.Stefinder.IsKillPlayer.Contains(p.PlayerId))
            {
                playerrole = RoleId.Stefinder1;
            }
            RoleId playerghostrole = p.Object.GetGhostRole();
            var (tasksCompleted, tasksTotal) = TaskCount.TaskDate(p);
            if (p.Object.IsImpostor())
            {
                tasksCompleted = 0;
                tasksTotal = 0;
            }
            var finalStatus = FinalStatus.Alive;

            if (p.Disconnected)
                finalStatus = FinalStatus.Disconnected;
            else if (p.IsDead && FinalStatusPatch.FinalStatusData.FinalStatuses.ContainsKey(p.PlayerId))
                finalStatus = FinalStatusPatch.FinalStatusData.FinalStatuses[p.PlayerId];
            else if (p.IsDead)
                finalStatus = FinalStatus.Exiled;
            else if (gameOverReason == GameOverReason.ImpostorBySabotage && !p.Role.IsImpostor)
                finalStatus = FinalStatus.Sabotage;
            FinalStatusPatch.FinalStatusData.FinalStatuses[p.PlayerId] = finalStatus;

            // サボタージュ死
            if (finalStatus == FinalStatus.Sabotage && !p.IsDead && !p.Role.IsImpostor)
                p.IsDead = true;

            string namesuffix = "";
            if (p.Object.IsLovers())
                namesuffix = ModHelpers.Cs(RoleClass.Lovers.color, " ♥");
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
                AttributeRoleName = attributeRoleName,
                RoleId = playerrole,
                GhostRoleId = playerghostrole,
                isImpostor = p.Role.IsImpostor
            });
        }

        if (ReplayManager.IsReplayMode)
        {
            Logger.Info("ComeEndReplay");
            var ReplayEndGameData = ReplayLoader.ReplayTurns[ReplayLoader.CurrentTurn].CurrentEndGameData;
            if (ReplayEndGameData == null) return;
            Logger.Info("EndNullReplay");
            Il2CppSystem.Collections.Generic.List<CachedPlayerData> WinningPlayers = new();
            foreach (byte winnerid in ReplayEndGameData.WinnerPlayers)
            {
                WinningPlayers.Add(new(GameData.Instance.GetPlayerById(winnerid)));
            }
            EndGameResult.CachedWinners = WinningPlayers;
            AdditionalTempData.winCondition = ReplayEndGameData.WinCond;
            return;
        }

        var (winners, winCondition, WillRevivePlayers) = HandleEndGameProcess(gameOverReason);

        // 勝者を処理
        EndGameResult.CachedWinners = new();
        foreach (var winner in winners)
            EndGameResult.CachedWinners.Add(new(winner));

        // 蘇生する
        foreach (NetworkedPlayerInfo player in WillRevivePlayers)
            player.IsDead = false;

        // WinConditionを設定
        AdditionalTempData.winCondition = winCondition;

        foreach (NetworkedPlayerInfo player in GameData.Instance.AllPlayers)
        {
            if (player.Object != null && player.Object.IsBot()) continue;
            CustomPlayerData data = new(player, gameOverReason)
            {
                IsWin = EndGameResult.CachedWinners.ToArray().Any(x => x.PlayerName == player.PlayerName)
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
            if (ExileController.Instance!= null && ExileController.Instance.initData?.networkedPlayer != null && ModeHandler.IsMode(ModeId.Default))
            {
                PlayerControl player = ExileController.Instance.initData?.networkedPlayer?.Object;
                if (player == null) return;
                FinalStatusPatch.FinalStatusData.FinalStatuses[player.PlayerId] = FinalStatus.Exiled;
                // Exile role text
                if (id is StringNames.ExileTextPN or StringNames.ExileTextSN or StringNames.ExileTextPP or StringNames.ExileTextSP)
                {
                    __result = string.Format(ModTranslation.GetString("ExiledText"), player.Data.PlayerName, CustomRoles.GetRoleName(player));
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
    private static float Timer;
    private const float EndGameTimerMax = 0.15f;

    public static bool Prefix()
    {
        Timer -= Time.fixedDeltaTime;
        if (Timer > 0 && Timer < 0.5f)
            return false;
        Timer = EndGameTimerMax;

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
            foreach (ISpecialWinner winner in RoleBaseManager.GetInterfaces<ISpecialWinner>())
                if (winner.CheckAndEndGame(__instance, statistics)) return false;
            if (CheckAndEndGameForLoversBreakerWin(__instance, statistics)) return false;
            if (CheckAndEndGameForPavlovsWin(__instance, statistics)) return false;
            if (CheckAndEndGameForHitmanWin(__instance, statistics)) return false;
            if (CheckAndEndGameForJackalWin(__instance, statistics)) return false;
            if (CheckAndEndGameForEgoistWin(__instance, statistics)) return false;
            if (CheckAndEndGameForTaskerWin(__instance, statistics)) return false;
            if (CheckAndEndGameForWorkpersonWin(__instance)) return false;
            if (CheckAndEndGameForFoxHouwaWin(__instance)) return false;
            if (CheckAndEndGameForSuicidalIdeationWin(__instance)) return false;
            if (CheckAndEndGameForSafecrackerWin(__instance)) return false;
        }
        return true;
    }
}

[HarmonyPatch(typeof(LogicGameFlowNormal), nameof(LogicGameFlowNormal.CheckEndCriteria))]
public static class CheckGameEndPatch
{
    private static float Timer;
    private const float EndGameTimerMax = 0.15f;

    public static bool Prefix()
    {
        Timer -= Time.fixedDeltaTime;
        if (Timer > 0 && Timer < 0.5f)
            return false;
        Timer = EndGameTimerMax;

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
            foreach (ISpecialWinner winner in RoleBaseManager.GetInterfaces<ISpecialWinner>())
                if (winner.CheckAndEndGame(__instance, statistics)) return false;
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
        ISystemType systemType2 = __instance.Systems.ContainsKey(SystemTypes.HeliSabotage) ? __instance.Systems[SystemTypes.HeliSabotage] : null;
        if (systemType2 == null)
        {
            systemType2 = __instance.Systems.ContainsKey(SystemTypes.Laboratory) ? __instance.Systems[SystemTypes.Laboratory] : null;
        }
        if (systemType2 == null)
        {
            systemType2 = __instance.Systems.ContainsKey(SystemTypes.Reactor) ? __instance.Systems[SystemTypes.Reactor] : null;
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
        if (statistics.TeamImpostorsAlive >= statistics.TotalAlive - statistics.TeamImpostorsAlive && statistics.TeamJackalAlive == 0 && statistics.HitmanAlive == 0 && statistics.OwlAlive == 0 && !statistics.IsGuardPavlovs && !EvilEraser.IsGodWinGuard() && !EvilEraser.IsFoxWinGuard() && !EvilEraser.IsNeetWinGuard())
        {
            __instance.enabled = false;
            var endReason = GameData.LastDeathReason switch
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
        if (statistics.EgoistAlive >= statistics.TotalAlive - statistics.EgoistAlive && statistics.EgoistAlive != 0 && statistics.TeamImpostorsAlive == 0 && statistics.TeamJackalAlive == 0 && statistics.HitmanAlive == 0 && statistics.OwlAlive == 0 && !statistics.IsGuardPavlovs)
        {
            __instance.enabled = false;
            CustomEndGame((GameOverReason)CustomGameOverReason.EgoistWin, false);
            return true;
        }
        return false;
    }
    public static bool CheckAndEndGameForJackalWin(ShipStatus __instance, PlayerStatistics statistics)
    {
        if (statistics.TeamJackalAlive >= statistics.TotalAlive - statistics.TeamJackalAlive && statistics.TeamImpostorsAlive == 0 && statistics.HitmanAlive == 0 && statistics.OwlAlive == 0 && !statistics.IsGuardPavlovs && !statistics.MadKillerEndGuardByPromote)
        {
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
        if (statistics.TeamImpostorsAlive == 0 && statistics.TeamJackalAlive == 0 && statistics.HitmanAlive == 0 && statistics.OwlAlive == 0 && !statistics.IsGuardPavlovs && !statistics.MadKillerEndGuardByPromote)
        {
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
    public static bool CheckAndEndGameForOwlWin(ShipStatus __instance, PlayerStatistics statistics)
    {
        if (statistics.TeamImpostorsAlive == 0 && statistics.TeamJackalAlive == 0 && statistics.HitmanAlive == 0 && statistics.OwlAlive == 1 && !statistics.IsGuardPavlovs && !statistics.MadKillerEndGuardByPromote)
        {
            foreach (Owl role in RoleBaseManager.GetRoleBases<Owl>())
            {
                PlayerControl player = role.Player;
                if (player == null) continue;
                if (player.IsDead()) continue;
                MessageWriter writer = RPCHelper.StartRPC(CustomRPC.ShareWinner);
                writer.Write(player.PlayerId);
                writer.EndRPC();
                RPCProcedure.ShareWinner(player.PlayerId);
                __instance.enabled = false;
                CustomEndGame((GameOverReason)CustomGameOverReason.OwlWin, false);
                return true;
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
        public int OwlAlive { get; set; }
        public int LoversAlive { get; set; }
        public bool IsGuardPavlovs { get; set; }
        public bool MadKillerEndGuardByPromote { get; set; }

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
            int numOwlAlive = 0;
            int numLoversAlive = 0;

            for (int i = 0; i < GameData.Instance.PlayerCount; i++)
            {
                NetworkedPlayerInfo playerInfo = GameData.Instance.AllPlayers[i];
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
                            else if (playerInfo.Object.IsRole(RoleId.Owl))
                            {
                                numOwlAlive++;
                            }
                        }
                        if (playerInfo.Object.IsLovers() || playerInfo.Object.IsRole(RoleId.truelover) || (playerInfo.Object.TryGetRoleBase<Cupid>(out Cupid cupid) && cupid.Created)) numLoversAlive++;
                    }
                }
            }

            if (ModeHandler.IsMode(ModeId.HideAndSeek))
                numTotalAlive += numImpostorsAlive;

            TeamImpostorsAlive = numImpostorsAlive;
            TotalAlive = numTotalAlive;
            CrewAlive = numCrewAlive;
            TeamJackalAlive = numTotalJackalTeam;
            EgoistAlive = numTotalEgoist;
            PavlovsDogAlive = numPavlovsDogAlive;
            PavlovsownerAlive = numPavlovsownerAlive;
            PavlovsTeamAlive = numPavlovsTeamAlive;
            HitmanAlive = numHitmanAlive;
            OwlAlive = numOwlAlive;
            LoversAlive = numLoversAlive;

            // まだパグロフの犬を作れるかを判定する
            if (!(IsGuardPavlovs = PavlovsDogAlive > 0))
            {
                foreach (RoleBase rolebase in RoleBaseManager.GetRoleBaseOrigins<PavlovsOwner>())
                {
                    if ((rolebase as PavlovsOwner).CreateCountLimit == 0)
                        continue;
                    if (rolebase.Player.IsDead())
                        continue;
                    IsGuardPavlovs = true;
                    break;
                }
            }
            // マッドキラーが昇格する前にゲームが終了してしまうのを防ぐ
            MadKillerEndGuardByPromote = false;
            foreach (PlayerControl p in RoleClass.SideKiller.MadKillerPlayer)
            {
                if (p.IsImpostor() || p.IsDead())
                    continue;
                MadKillerEndGuardByPromote = true;
            }
        }
    }
}