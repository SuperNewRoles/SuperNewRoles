using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using AmongUs.Data;
using AmongUs.GameOptions;
using HarmonyLib;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Events;
using SuperNewRoles.Modules;
using SuperNewRoles.Modules.Events.Bases;
using SuperNewRoles.Roles.Ability;
using SuperNewRoles.Roles.Ability.CustomButton;
using SuperNewRoles.Patches;
using TMPro;
using UnityEngine;

namespace SuperNewRoles.Roles.Crewmate;

class PoliceSurgeon : RoleBase<PoliceSurgeon>
{
    public override RoleId Role { get; } = RoleId.PoliceSurgeon;
    public override Color32 RoleColor { get; } = new(137, 195, 235, 255);

    public override List<Func<AbilityBase>> Abilities { get; } = [
        () => new PoliceSurgeonMeetingAbility(),
        () => new PoliceSurgeonPortableVitalsAbility(),
    ];

    public override QuoteMod QuoteMod { get; } = QuoteMod.SuperNewRoles;
    public override short IntroNum { get; } = 1;

    public override AssignedTeamType AssignedTeam { get; } = AssignedTeamType.Crewmate;
    public override WinnerTeamType WinnerTeam { get; } = WinnerTeamType.Crewmate;
    public override TeamTag TeamTag { get; } = TeamTag.Crewmate;
    public override RoleTag[] RoleTags { get; } = [RoleTag.Information];
    public override RoleOptionMenuType OptionTeam { get; } = RoleOptionMenuType.Crewmate;

    [CustomOptionBool(nameof(PoliceSurgeonHaveVitalsInTaskPhase), false, translationName: "PoliceSurgeonHaveVitalsInTaskPhase")]
    public static bool PoliceSurgeonHaveVitalsInTaskPhase;

    [CustomOptionFloat(nameof(PoliceSurgeonVitalsDisplayCooldown), 5f, 60f, 5f, 15f, translationName: "VitalsDisplayCooldown", parentFieldName: nameof(PoliceSurgeonHaveVitalsInTaskPhase), suffix: "Seconds")]
    public static float PoliceSurgeonVitalsDisplayCooldown;

    [CustomOptionFloat(nameof(PoliceSurgeonBatteryDuration), 5f, 30f, 5f, 5f, translationName: "BatteryDuration", parentFieldName: nameof(PoliceSurgeonHaveVitalsInTaskPhase), suffix: "Seconds")]
    public static float PoliceSurgeonBatteryDuration;

    [CustomOptionBool(nameof(PoliceSurgeonCanResend), false, translationName: "PoliceSurgeonCanResend")]
    public static bool PoliceSurgeonCanResend;

    [CustomOptionBool(nameof(PoliceSurgeonIndicateTimeOfDeathInSubsequentTurn), true, translationName: "PoliceSurgeonIndicateTimeOfDeathInSubsequentTurn")]
    public static bool PoliceSurgeonIndicateTimeOfDeathInSubsequentTurn;

    [CustomOptionBool(nameof(PoliceSurgeonHowManyTurnAgoTheDied), false, translationName: "PoliceSurgeonHowManyTurnAgoTheDied", parentFieldName: nameof(PoliceSurgeonIndicateTimeOfDeathInSubsequentTurn))]
    public static bool PoliceSurgeonHowManyTurnAgoTheDied;

    [CustomOptionBool(nameof(PoliceSurgeonIsUseTaiwanCalendar), true, translationName: "PoliceSurgeonIsUseTaiwanCalendar")]
    public static bool PoliceSurgeonIsUseTaiwanCalendar;

    [CustomOptionBool(nameof(PoliceSurgeonIncludeErrorInDeathTime), true, translationName: "PoliceSurgeon_IncludeErrorInDeathTime")]
    public static bool PoliceSurgeonIncludeErrorInDeathTime;

    [CustomOptionInt(nameof(PoliceSurgeonMarginOfErrorToIncludeInTimeOfDeath), 0, 15, 1, 5, translationName: "PoliceSurgeonMarginOfErrorToIncludeInTimeOfDeath", parentFieldName: nameof(PoliceSurgeonIncludeErrorInDeathTime), suffix: "Seconds")]
    public static int PoliceSurgeonMarginOfErrorToIncludeInTimeOfDeath;
}

internal enum PoliceSurgeonDeadTiming
{
    TaskPhaseKilled,
    TaskPhaseExited,
    MeetingPhase,
    Exited,
}

internal sealed class PoliceSurgeonPersonalInformation
{
    public byte VictimId { get; }
    public int DeadTurn { get; }
    public PoliceSurgeonDeadTiming DeadReason { get; }
    public int DeathSecondsAgo { get; }

    public PoliceSurgeonPersonalInformation(byte victimId, int deadTurn, PoliceSurgeonDeadTiming deadReason, int deathSecondsAgo)
    {
        VictimId = victimId;
        DeadTurn = deadTurn;
        DeadReason = deadReason;
        DeathSecondsAgo = deathSecondsAgo;
    }
}

internal static class PoliceSurgeonSharedState
{
    private static readonly Dictionary<byte, PoliceSurgeonPersonalInformation> PersonalInfoByVictim = new();
    private static EventListener<MeetingStartEventData> _meetingStartListener;
    private static EventListener<MeetingCloseEventData> _meetingCloseListener;

    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.CoStartGame))]
    private static class CoStartGamePatch
    {
        public static void Postfix()
        {
            ClearAll();
            PoliceSurgeonOverlay.Destroy();

            _meetingStartListener = MeetingStartEvent.Instance.AddListener(OnMeetingStart);
            _meetingCloseListener = MeetingCloseEvent.Instance.AddListener(OnMeetingClose);
        }
    }

    public static void ClearAll()
    {
        PersonalInfoByVictim.Clear();
    }

    public static bool TryGetInfo(byte victimId, out PoliceSurgeonPersonalInformation info)
        => PersonalInfoByVictim.TryGetValue(victimId, out info);

    public static IReadOnlyCollection<PoliceSurgeonPersonalInformation> GetAllInfos()
        => PersonalInfoByVictim.Values;

    private static void OnMeetingStart(MeetingStartEventData data)
    {
        if (data == null) return;
        RecordTaskPhaseDeaths(data.meetingCount);
    }

    private static void OnMeetingClose(MeetingCloseEventData data)
    {
        if (data == null) return;
        RecordMeetingPhaseDeaths(data.meetingCount);
    }

    private static void RecordTaskPhaseDeaths(int meetingCount)
    {
        DateTime reportTimeUtc = DateTime.UtcNow;

        foreach (var exPlayer in ExPlayerControl.ExPlayerControls)
        {
            if (exPlayer == null || exPlayer.Data == null) continue;
            if (exPlayer.IsAlive()) continue;
            if (PersonalInfoByVictim.ContainsKey(exPlayer.PlayerId)) continue;

            int secondsAgo = 0;
            PoliceSurgeonDeadTiming reason = PoliceSurgeonDeadTiming.TaskPhaseExited;

            if (MurderDataManager.TryGetMurderData(exPlayer, out var murderData) && murderData != null)
            {
                reason = PoliceSurgeonDeadTiming.TaskPhaseKilled;
                secondsAgo = (int)(reportTimeUtc - murderData.DeathTimeUtc).TotalSeconds;
                if (secondsAgo < 0) secondsAgo = 0;

                if (PoliceSurgeon.PoliceSurgeonIncludeErrorInDeathTime && PoliceSurgeon.PoliceSurgeonMarginOfErrorToIncludeInTimeOfDeath > 0)
                {
                    int margin = Math.Abs(PoliceSurgeon.PoliceSurgeonMarginOfErrorToIncludeInTimeOfDeath);
                    int seed = unchecked((exPlayer.PlayerId << 16) ^ (meetingCount << 8) ^ secondsAgo);
                    var random = new System.Random(seed);
                    int error = random.Next(0, margin * 2 + 1) - margin;
                    secondsAgo += error;
                    if (secondsAgo < 0) secondsAgo = 0;
                }
            }

            PersonalInfoByVictim[exPlayer.PlayerId] = new PoliceSurgeonPersonalInformation(exPlayer.PlayerId, meetingCount, reason, secondsAgo);
        }
    }

    private static void RecordMeetingPhaseDeaths(int meetingCount)
    {
        foreach (var exPlayer in ExPlayerControl.ExPlayerControls)
        {
            if (exPlayer == null || exPlayer.Data == null) continue;
            if (exPlayer.IsAlive()) continue;
            if (PersonalInfoByVictim.ContainsKey(exPlayer.PlayerId)) continue;

            var reason = exPlayer.FinalStatus == FinalStatus.Exiled
                ? PoliceSurgeonDeadTiming.Exited
                : PoliceSurgeonDeadTiming.MeetingPhase;

            PersonalInfoByVictim[exPlayer.PlayerId] = new PoliceSurgeonPersonalInformation(exPlayer.PlayerId, meetingCount, reason, 0);
        }
    }
}

internal static class PoliceSurgeonCertificateBuilder
{
    public static string BuildFullCertificate(string doctorName, int currentMeetingCount)
    {
        string officialDate = GetOfficialDateNotation();
        var builder = new StringBuilder();

        const string fullDelimiterLine = "|--------------------------------------------------------------|";
        builder.AppendLine("<align=left>" + fullDelimiterLine);
        builder.AppendLine(ModTranslation.GetString("PostMortemCertificate_main1"));
        builder.AppendLine(fullDelimiterLine);

        bool wrote = false;
        var infos = new List<PoliceSurgeonPersonalInformation>(PoliceSurgeonSharedState.GetAllInfos());
        infos.Sort(static (a, b) =>
        {
            int cmp = a.DeadTurn.CompareTo(b.DeadTurn);
            return cmp != 0 ? cmp : a.VictimId.CompareTo(b.VictimId);
        });

        foreach (var info in infos)
        {
            if (!PoliceSurgeon.PoliceSurgeonIndicateTimeOfDeathInSubsequentTurn && info.DeadTurn != currentMeetingCount)
                continue;

            wrote = true;
            builder.Append(CreateContents(info, officialDate, currentMeetingCount));
            builder.AppendLine(fullDelimiterLine);
        }

        if (wrote)
        {
            builder.AppendLine(ModTranslation.GetString("PostMortemCertificate_main3"));
            builder.AppendLine($"{ModTranslation.GetString("PostMortemCertificate_main4")}{officialDate}");
            builder.AppendLine(((MapNames)GameOptionsManager.Instance.CurrentGameOptions.MapId).ToString());
            builder.AppendLine("");
            builder.AppendLine($"{ModTranslation.GetString("PostMortemCertificate_main5")} {doctorName}");
            builder.AppendLine(fullDelimiterLine);
            builder.AppendLine("");
        }
        else
        {
            builder.Clear();
            int rand = new System.Random((int)DateTime.UtcNow.Ticks).Next(1, 16);
            string transRandomText = ModTranslation.GetString($"PostMortemCertificate_NoDeaths_RandomMessage_{rand}");
            builder.AppendLine($"<align=left>{string.Format(ModTranslation.GetString("PostMortemCertificate_NoDeaths"), officialDate, doctorName, transRandomText)}");
        }

        builder.Insert(0, "<size=100%>");
        builder.AppendLine("</align></size>");
        builder.Replace("<color=#89c3eb>", "<color=#5654a2>");
        return builder.ToString();
    }

    public static string BuildShortCertificate(byte victimId, string doctorName, int currentMeetingCount)
    {
        if (!PoliceSurgeonSharedState.TryGetInfo(victimId, out var info))
        {
            return $"No death information recorded. (victimId={victimId})";
        }

        string officialDate = GetOfficialDateNotation();
        var builder = new StringBuilder();

        const string shortDelimiterLine = "|----------------------------------------------------------------|";
        builder.AppendLine("<align=left>" + shortDelimiterLine);
        builder.AppendLine(ModTranslation.GetString("PostMortemCertificate_main1"));
        builder.AppendLine(shortDelimiterLine);
        builder.Append(CreateContents(info, officialDate, currentMeetingCount));
        builder.AppendLine(shortDelimiterLine);

        builder.Insert(0, "<size=200%><color=#7d7d7d>");
        builder.AppendLine("</align></color></size>");

        return builder.ToString();
    }

    private static string CreateContents(PoliceSurgeonPersonalInformation victimInfo, string officialDate, int currentMeetingCount)
    {
        var builder = new StringBuilder();

        bool isWritingTurn = PoliceSurgeon.PoliceSurgeonIndicateTimeOfDeathInSubsequentTurn && PoliceSurgeon.PoliceSurgeonHowManyTurnAgoTheDied;
        bool inError = PoliceSurgeon.PoliceSurgeonIncludeErrorInDeathTime;

        string victimName = ExPlayerControl.ById(victimInfo.VictimId)?.Data?.PlayerName
            ?? GameData.Instance?.GetPlayerById(victimInfo.VictimId)?.PlayerName
            ?? $"Player{victimInfo.VictimId}";

        builder.AppendLine($"{ModTranslation.GetString("PostMortemCertificate_main2")}<color=#89c3eb>{victimName}</color>");

        switch (victimInfo.DeadReason)
        {
            case PoliceSurgeonDeadTiming.TaskPhaseKilled:
                {
                    string stateLabel = inError
                        ? ModTranslation.GetString("PostMortemCertificate_CauseOfDeath1")
                        : ModTranslation.GetString("PostMortemCertificate_CauseOfDeath2");

                    if (isWritingTurn)
                        builder.AppendLine(string.Format(
                            ModTranslation.GetString("PostMortemCertificate_AlreadyKnown_WriteTurn"),
                            officialDate,
                            currentMeetingCount - victimInfo.DeadTurn,
                            victimInfo.DeathSecondsAgo,
                            stateLabel
                        ));
                    else
                        builder.AppendLine(string.Format(
                            ModTranslation.GetString("PostMortemCertificate_AlreadyKnown"),
                            officialDate,
                            victimInfo.DeathSecondsAgo,
                            stateLabel
                        ));

                    builder.AppendLine("");
                    builder.AppendLine(ModTranslation.GetString("PostMortemCertificate_DeadReason1"));
                    builder.AppendLine("");
                    break;
                }

            case PoliceSurgeonDeadTiming.TaskPhaseExited:
            case PoliceSurgeonDeadTiming.MeetingPhase:
                {
                    string approx = ModTranslation.GetString("PostMortemCertificate_CauseOfDeath3");
                    if (isWritingTurn)
                        builder.AppendLine(string.Format(
                            ModTranslation.GetString("PostMortemCertificate_Unknown_WriteTurn"),
                            officialDate,
                            currentMeetingCount - victimInfo.DeadTurn,
                            approx
                        ));
                    else
                        builder.AppendLine(string.Format(
                            ModTranslation.GetString("PostMortemCertificate_Unknown"),
                            officialDate,
                            approx
                        ));

                    builder.AppendLine("");
                    builder.AppendLine(ModTranslation.GetString("PostMortemCertificate_DeadReason1"));
                    builder.AppendLine("");
                    break;
                }

            case PoliceSurgeonDeadTiming.Exited:
                {
                    string approx = ModTranslation.GetString("PostMortemCertificate_CauseOfDeath3");
                    if (isWritingTurn)
                        builder.AppendLine(string.Format(
                            ModTranslation.GetString("PostMortemCertificate_Unknown_WriteTurn"),
                            officialDate,
                            currentMeetingCount - victimInfo.DeadTurn,
                            approx
                        ));
                    else
                        builder.AppendLine(string.Format(
                            ModTranslation.GetString("PostMortemCertificate_Unknown"),
                            officialDate,
                            approx
                        ));

                    builder.AppendLine("");
                    builder.AppendLine(ModTranslation.GetString("PostMortemCertificate_DeadReason2"));
                    builder.AppendLine("");
                    break;
                }
        }

        return builder.ToString();
    }

    private static string GetOfficialDateNotation()
    {
        SupportedLangs langId;
        try
        {
            langId = DestroyableSingleton<TranslationController>.InstanceExists
                ? DestroyableSingleton<TranslationController>.Instance.currentLanguage.languageID
                : DataManager.Settings.Language.CurrentLanguage;
        }
        catch
        {
            langId = SupportedLangs.English;
        }

        CultureInfo ci = new("en-US");

        try
        {
            switch (langId)
            {
                case SupportedLangs.Japanese:
                    {
                        var jc = new JapaneseCalendar();
                        ci = new CultureInfo("Ja-JP", true);
                        ci.DateTimeFormat.Calendar = jc;
                        return DateTime.Now.ToString("ggy年 M月 d日", ci);
                    }
                case SupportedLangs.SChinese:
                    return GetKanjiCalendar();
                case SupportedLangs.TChinese:
                    {
                        if (PoliceSurgeon.PoliceSurgeonIsUseTaiwanCalendar)
                        {
                            var tc = new TaiwanCalendar();
                            ci = new CultureInfo("zh-TW", true);
                            ci.DateTimeFormat.Calendar = tc;
                            return DateTime.Now.ToString("ggy年 M月 d日", ci);
                        }
                        return GetKanjiCalendar();
                    }
                default:
                    return DateTime.Now.ToString("MMMM d, yyyy", ci);
            }
        }
        catch
        {
            return DateTime.Now.ToString("yyyy/MM/dd", ci);
        }
    }

    private static string GetKanjiCalendar()
    {
        string dateNum = DateTime.Now.ToString("yyyy年MM月dd日");

        string[] kanjiArr = { "〇", "一", "二", "三", "四", "五", "六", "七", "八", "九" };
        string[] kanjiArr10 = { "", "十", "二十", "三十" };

        var returnBuilder = new StringBuilder();
        int i = 1;
        foreach (char c in dateNum)
        {
            string str = c.ToString();

            if (i is 5 or 8 or 11) returnBuilder.Append(str);
            else if (i is 6 or 9) returnBuilder.Append(kanjiArr10[int.Parse(str)]);
            else returnBuilder.Append(kanjiArr[int.Parse(str)]);

            i++;
        }
        return returnBuilder.ToString();
    }
}

internal static class PoliceSurgeonOverlay
{
    private static bool _overlayShown;
    private static SpriteRenderer _infoUnderlay;
    private static TextMeshPro _infoOverlay;
    private static GameObject _root;

    public static bool IsShown => _overlayShown;

    public static void Toggle(byte targetId, string text)
    {
        if (_overlayShown)
            Hide();
        else
            Show(targetId, text);
    }

    public static void Show(byte targetId, string text)
    {
        if (!InitializeOverlays()) return;
        if (MeetingHud.Instance == null && HudManager.Instance == null) return;

        _overlayShown = true;

        Transform parent = MeetingHud.Instance != null ? MeetingHud.Instance.transform : HudManager.Instance.transform;
        _root.transform.SetParent(parent, false);
        _root.transform.localPosition = Vector3.zero;
        _root.transform.localScale = Vector3.one;

        _infoUnderlay.sprite = HudManager.Instance.FullScreen.sprite;
        _infoUnderlay.color = new Color(192f / 255f, 198f / 255f, 201f / 255f, 1f);
        _infoUnderlay.transform.localScale = new Vector3(7.5f, 5f, 1f);
        _infoUnderlay.enabled = true;

        _infoOverlay.text = text;
        _infoOverlay.enabled = true;

        var underlayTransparent = new Color(0.1f, 0.1f, 0.1f, 0.0f);
        var underlayOpaque = _infoUnderlay.color;
        HudManager.Instance.StartCoroutine(Effects.Lerp(0.2f, new Action<float>(t =>
        {
            if (_infoUnderlay != null)
                _infoUnderlay.color = Color.Lerp(underlayTransparent, underlayOpaque, t);
            if (_infoOverlay != null)
                _infoOverlay.color = Color.Lerp(Palette.ClearWhite, Palette.White, t);
        })));
    }

    public static void Hide()
    {
        if (!_overlayShown) return;

        _overlayShown = false;
        var underlayTransparent = new Color(0.1f, 0.1f, 0.1f, 0.0f);
        var underlayOpaque = new Color(192f / 255f, 198f / 255f, 201f / 255f, 1f);

        if (HudManager.Instance != null)
        {
            HudManager.Instance.StartCoroutine(Effects.Lerp(0.2f, new Action<float>(t =>
            {
                if (_infoUnderlay != null)
                {
                    _infoUnderlay.color = Color.Lerp(underlayOpaque, underlayTransparent, t);
                    if (t >= 1f) _infoUnderlay.enabled = false;
                }

                if (_infoOverlay != null)
                {
                    _infoOverlay.color = Color.Lerp(Palette.White, Palette.ClearWhite, t);
                    if (t >= 1f) _infoOverlay.enabled = false;
                }
            })));
        }
        else
        {
            if (_infoUnderlay != null) _infoUnderlay.enabled = false;
            if (_infoOverlay != null) _infoOverlay.enabled = false;
        }
    }

    public static void Destroy()
    {
        _overlayShown = false;
        if (_root != null)
            UnityEngine.Object.Destroy(_root);
        _root = null;
        _infoUnderlay = null;
        _infoOverlay = null;
    }

    private static bool InitializeOverlays()
    {
        if (_root != null && _infoUnderlay != null && _infoOverlay != null)
            return true;
        if (HudManager.Instance == null || HudManager.Instance.FullScreen == null || HudManager.Instance.TaskPanel == null)
            return false;

        _root = new GameObject("PoliceSurgeonOverlay");
        _root.layer = HudManager.Instance.gameObject.layer;
        _root.transform.localPosition = Vector3.zero;
        _root.transform.localScale = Vector3.one;

        _infoUnderlay = UnityEngine.Object.Instantiate(HudManager.Instance.FullScreen, _root.transform);
        _infoUnderlay.gameObject.name = "Underlay";
        _infoUnderlay.sortingOrder = 100;
        _infoUnderlay.enabled = false;

        _infoOverlay = UnityEngine.Object.Instantiate(HudManager.Instance.TaskPanel.taskText, _root.transform);
        _infoOverlay.gameObject.name = "OverlayText";
        _infoOverlay.transform.localPosition = new Vector3(0f, 0f, -1f);
        _infoOverlay.transform.localScale *= 1.3f;
        _infoOverlay.alignment = TextAlignmentOptions.TopLeft;
        _infoOverlay.enableWordWrapping = true;
        _infoOverlay.color = Palette.ClearWhite;
        _infoOverlay.text = "";
        _infoOverlay.enabled = false;
        var overlayRenderer = _infoOverlay.GetComponent<MeshRenderer>();
        if (overlayRenderer != null)
            overlayRenderer.sortingOrder = 101;

        return true;
    }
}

public sealed class PoliceSurgeonMeetingAbility : AbilityBase
{
    private EventListener<MeetingStartEventData> _meetingStartListener;
    private EventListener<MeetingCloseEventData> _meetingCloseListener;
    private EventListener _hudUpdateListener;

    private readonly Dictionary<byte, GameObject> _meetingButtons = new();

    public override void AttachToAlls()
    {
        base.AttachToAlls();
    }

    public override void AttachToLocalPlayer()
    {
        base.AttachToLocalPlayer();
        _meetingStartListener = MeetingStartEvent.Instance.AddListener(OnMeetingStart);
        _meetingCloseListener = MeetingCloseEvent.Instance.AddListener(OnMeetingClose);
        _hudUpdateListener = HudUpdateEvent.Instance.AddListener(OnHudUpdate);
    }

    public override void DetachToLocalPlayer()
    {
        base.DetachToLocalPlayer();
        _meetingStartListener?.RemoveListener();
        _meetingCloseListener?.RemoveListener();
        _hudUpdateListener?.RemoveListener();
        DestroyMeetingButtons();
        PoliceSurgeonOverlay.Destroy();
    }

    private void OnMeetingStart(MeetingStartEventData data)
    {
        DestroyMeetingButtons();
        PoliceSurgeonOverlay.Hide();

        if (MeetingHud.Instance == null) return;

        byte localId = PlayerControl.LocalPlayer.PlayerId;
        bool canResend = PoliceSurgeon.PoliceSurgeonCanResend;

        foreach (var playerVoteArea in MeetingHud.Instance.playerStates)
        {
            if (playerVoteArea == null) continue;
            ExPlayerControl target = (ExPlayerControl)playerVoteArea;
            if (target == null) continue;

            if (canResend)
            {
                if (target.IsAlive() && target.PlayerId != localId) continue;
            }
            else
            {
                if (target.IsAlive() || target.PlayerId == localId) continue;
            }

            if (!PoliceSurgeon.PoliceSurgeonIndicateTimeOfDeathInSubsequentTurn && target.PlayerId != localId)
            {
                if (PoliceSurgeonSharedState.TryGetInfo(target.PlayerId, out var info) && info.DeadTurn != data.meetingCount)
                    continue;
            }

            GameObject template = playerVoteArea.Buttons.transform.Find("CancelButton").gameObject;
            GameObject targetBox = UnityEngine.Object.Instantiate(template, playerVoteArea.transform);
            targetBox.name = "PoliceSurgeonButton";
            targetBox.transform.localPosition = new Vector3(0.95f, 0.03f, -1.3f);

            var renderer = targetBox.GetComponent<SpriteRenderer>();
            var sprite = AssetManager.GetAsset<Sprite>("PoliceSurgeonButton.png");
            if (sprite != null) renderer.sprite = sprite;
            renderer.sortingOrder = 0;

            PassiveButton button = targetBox.GetComponent<PassiveButton>();
            button.OnClick.RemoveAllListeners();
            byte targetId = target.PlayerId;
            button.OnClick.AddListener((Action)(() => OnClick(targetId)));

            _meetingButtons[targetId] = targetBox;
        }

        string doctorName = PlayerControl.LocalPlayer?.Data?.PlayerName ?? PlayerControl.LocalPlayer?.name ?? "Police Surgeon";
        new LateTask(() =>
        {
            if (HudManager.Instance?.Chat == null) return;
            string full = PoliceSurgeonCertificateBuilder.BuildFullCertificate(doctorName, data.meetingCount);
            HudManager.Instance.Chat.AddChatWarning(full);
            if (PoliceSurgeon.PoliceSurgeonCanResend)
            {
                HudManager.Instance.Chat.AddChatWarning(ModTranslation.GetString("PoliceSurgeonResendSNR"));
            }
        }, 3f, "PoliceSurgeonSendCertificate");
    }

    private void OnMeetingClose(MeetingCloseEventData _)
    {
        DestroyMeetingButtons();
        PoliceSurgeonOverlay.Hide();
    }

    private void OnHudUpdate()
    {
        if (!PoliceSurgeonOverlay.IsShown) return;
        if (HudManager.Instance?.Chat != null && HudManager.Instance.Chat.IsOpenOrOpening)
        {
            PoliceSurgeonOverlay.Hide();
            return;
        }
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(KeyCode.H))
        {
            PoliceSurgeonOverlay.Hide();
        }
    }

    private void OnClick(byte targetPlayerId)
    {
        if (PlayerControl.LocalPlayer == null) return;
        if (HudManager.Instance?.Chat == null) return;

        byte localId = PlayerControl.LocalPlayer.PlayerId;
        string doctorName = PlayerControl.LocalPlayer?.Data?.PlayerName ?? PlayerControl.LocalPlayer?.name ?? "Police Surgeon";
        int meetingCount = MeetingStartEvent.MeetingCount;

        if (targetPlayerId == localId)
        {
            string full = PoliceSurgeonCertificateBuilder.BuildFullCertificate(doctorName, meetingCount);
            HudManager.Instance.Chat.AddChatWarning(full);
            return;
        }

        if (HudManager.Instance.Chat.IsOpenOrOpening && PoliceSurgeonOverlay.IsShown)
        {
            PoliceSurgeonOverlay.Hide();
            return;
        }

        string shortText = PoliceSurgeonCertificateBuilder.BuildShortCertificate(targetPlayerId, doctorName, meetingCount);
        PoliceSurgeonOverlay.Toggle(targetPlayerId, shortText);
    }

    private void DestroyMeetingButtons()
    {
        foreach (var obj in _meetingButtons.Values)
        {
            if (obj != null)
                UnityEngine.Object.Destroy(obj);
        }
        _meetingButtons.Clear();
    }
}

public sealed class PoliceSurgeonPortableVitalsAbility : CustomButtonBase
{
    private TextMeshPro _batteryText;
    private float _batteryRemaining;
    private bool _batteryDepleted;
    private bool _isPortableVitalsOpen;
    private bool _skipCooldownOnUse;

    public override Sprite Sprite => FastDestroyableSingleton<HudManager>.Instance.UseButton.fastUseSettings[ImageNames.VitalsButton].Image;
    public override string buttonText => FastDestroyableSingleton<TranslationController>.Instance.GetString(StringNames.VitalsLabel);
    protected override KeyType keytype => KeyType.Ability2;
    public override float DefaultTimer => Mathf.Max(0.01f, PoliceSurgeon.PoliceSurgeonVitalsDisplayCooldown);
    public override bool IsFirstCooldownTenSeconds => false;
    private float BatteryDuration => Mathf.Max(0f, PoliceSurgeon.PoliceSurgeonBatteryDuration);

    public override void AttachToLocalPlayer()
    {
        base.AttachToLocalPlayer();
        ResetBattery();
    }

    public override void DetachToLocalPlayer()
    {
        base.DetachToLocalPlayer();
        ClearBatteryText();
    }

    public override bool CheckHasButton()
    {
        return base.CheckHasButton() && PoliceSurgeon.PoliceSurgeonHaveVitalsInTaskPhase;
    }

    public override bool CheckIsAvailable()
    {
        if (!PoliceSurgeon.PoliceSurgeonHaveVitalsInTaskPhase) return false;
        if (!PlayerControl.LocalPlayer.CanMove) return false;
        if (MeetingHud.Instance != null) return false;
        if (_batteryDepleted) return false;
        return true;
    }

    public override void OnClick()
    {
        // デバイス制限のカウント対象外にする（DevicesPatch側でClose時にfalseへ戻る）
        DevicesPatch.DontCountBecausePortableVitals = true;
        _isPortableVitalsOpen = true;
        _skipCooldownOnUse = true;

        var originalRole = PlayerControl.LocalPlayer.Data.Role.Role;
        FastDestroyableSingleton<RoleManager>.Instance.SetRole(PlayerControl.LocalPlayer, RoleTypes.Scientist);
        PlayerControl.LocalPlayer.Data.Role.TryCast<ScientistRole>()?.UseAbility();
        FastDestroyableSingleton<RoleManager>.Instance.SetRole(PlayerControl.LocalPlayer, originalRole);
    }

    public override void OnUpdate()
    {
        UpdatePortableVitalsState();
        base.OnUpdate();
    }

    public override void ResetTimer()
    {
        if (_skipCooldownOnUse)
        {
            _skipCooldownOnUse = false;
            Timer = 0f;
            if (actionButton != null)
                actionButton.cooldownTimerText.color = Palette.EnabledColor;
            return;
        }

        if (!_batteryDepleted)
        {
            Timer = 0f;
            if (actionButton != null)
                actionButton.cooldownTimerText.color = Palette.EnabledColor;
            return;
        }

        base.ResetTimer();
    }

    public override void OnMeetingEnds()
    {
        if (Minigame.Instance is VitalsMinigame)
            Minigame.Instance.Close();
        base.OnMeetingEnds();
    }

    private void UpdatePortableVitalsState()
    {
        if (_batteryDepleted && Timer <= 0f)
        {
            ResetBattery();
        }

        bool vitalsOpen = Minigame.Instance is VitalsMinigame;
        bool usingPortableVitals = _isPortableVitalsOpen && DevicesPatch.DontCountBecausePortableVitals;
        if (_isPortableVitalsOpen && !DevicesPatch.DontCountBecausePortableVitals)
        {
            _isPortableVitalsOpen = false;
        }

        if (usingPortableVitals && !vitalsOpen)
        {
            _isPortableVitalsOpen = false;
            ClearBatteryText();
            return;
        }

        if (!usingPortableVitals || !vitalsOpen)
        {
            ClearBatteryText();
            return;
        }

        EnsureBatteryText((VitalsMinigame)Minigame.Instance);
        if (_batteryDepleted || BatteryDuration <= 0f)
        {
            UpdateBatteryText();
            return;
        }

        _batteryRemaining -= Time.deltaTime;
        if (_batteryRemaining <= 0f)
        {
            _batteryRemaining = 0f;
            _batteryDepleted = true;
            _isPortableVitalsOpen = false;
            ClearBatteryText();
            if (Minigame.Instance is VitalsMinigame)
                Minigame.Instance.Close();
            StartCooldown();
            return;
        }

        UpdateBatteryText();
    }

    private void ResetBattery()
    {
        _batteryRemaining = BatteryDuration;
        _batteryDepleted = false;
    }

    private void StartCooldown()
    {
        Timer = DefaultTimer;
    }

    private void EnsureBatteryText(VitalsMinigame vitals)
    {
        if (_batteryText != null) return;
        _batteryText = UnityEngine.Object.Instantiate(FastDestroyableSingleton<HudManager>.Instance.TaskPanel.taskText, vitals.transform);
        _batteryText.alignment = TextAlignmentOptions.BottomRight;
        _batteryText.transform.position = Vector3.zero;
        _batteryText.transform.localPosition = new Vector3(1.7f, 3.95f);
        _batteryText.transform.localScale *= 1.8f;
        _batteryText.color = Palette.White;
        _batteryText.text = "";
    }

    private void UpdateBatteryText()
    {
        if (_batteryText == null) return;
        float seconds = Mathf.Max(0f, _batteryRemaining);
        _batteryText.text = TimeSpan.FromSeconds(seconds).ToString(@"mm\:ss\.ff");
    }

    private void ClearBatteryText()
    {
        if (_batteryText == null) return;
        UnityEngine.Object.Destroy(_batteryText.gameObject);
        _batteryText = null;
    }
}
