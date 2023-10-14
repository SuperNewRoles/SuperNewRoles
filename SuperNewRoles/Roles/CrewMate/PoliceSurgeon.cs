using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using AmongUs.Data;
using AmongUs.GameOptions;
using HarmonyLib;
using Hazel;
using SuperNewRoles.Patches;
using SuperNewRoles.Helpers;
using SuperNewRoles.Mode;
using SuperNewRoles.Mode.SuperHostRoles;
using UnityEngine;
using static SuperNewRoles.Modules.CustomOption;
using static SuperNewRoles.Roles.Crewmate.PoliceSurgeon;

namespace SuperNewRoles.Roles.Crewmate;
public static class PoliceSurgeon
{
    internal static class CustomOptionData
    {
        private static int optionId = 406100;
        public static CustomRoleOption Option;
        public static CustomOption PlayerCount;
        public static CustomOption HaveVitalsInTaskPhase;
        public static CustomOption VitalsDisplayCooldown;
        public static CustomOption BatteryDuration;
        public static CustomOption CanResend;
        public static CustomOption IndicateTimeOfDeathInSubsequentTurn;
        public static CustomOption HowManyTurnAgoTheDied;
        public static CustomOption IncludeErrorInDeathTime;
        public static CustomOption MarginOfErrorToIncludeInTimeOfDeath;
        public static CustomOption IsUseTaiwanCalendar;

        public static void SetupCustomOptions()
        {
            Option = SetupCustomRoleOption(optionId, true, RoleId.PoliceSurgeon); optionId++;
            PlayerCount = Create(optionId, true, CustomOptionType.Crewmate, "SettingPlayerCountName", CustomOptionHolder.CrewPlayers[0], CustomOptionHolder.CrewPlayers[1], CustomOptionHolder.CrewPlayers[2], CustomOptionHolder.CrewPlayers[3], Option); optionId++;
            HaveVitalsInTaskPhase = Create(optionId, true, CustomOptionType.Crewmate, "PoliceSurgeonHaveVitalsInTaskPhase", false, Option); optionId++;
            VitalsDisplayCooldown = Create(optionId, true, CustomOptionType.Crewmate, "VitalsDisplayCooldown", 15f, 5f, 60f, 5f, HaveVitalsInTaskPhase); optionId++;
            BatteryDuration = Create(optionId, true, CustomOptionType.Crewmate, "BatteryDuration", 5f, 5f, 30f, 5f, HaveVitalsInTaskPhase); optionId++;
            CanResend = Create(optionId, true, CustomOptionType.Crewmate, "PoliceSurgeonCanResend", false, Option); optionId++;
            IndicateTimeOfDeathInSubsequentTurn = Create(optionId, true, CustomOptionType.Crewmate, "PoliceSurgeonIndicateTimeOfDeathInSubsequentTurn", true, Option); optionId++;
            HowManyTurnAgoTheDied = Create(optionId, true, CustomOptionType.Crewmate, "PoliceSurgeonHowManyTurnAgoTheDied", false, IndicateTimeOfDeathInSubsequentTurn); optionId++;
            IsUseTaiwanCalendar = Create(optionId, true, CustomOptionType.Crewmate, "PoliceSurgeonIsUseTaiwanCalendar", true, Option, isHidden: DataManager.Settings.Language.CurrentLanguage != SupportedLangs.TChinese); optionId++;
            IncludeErrorInDeathTime = Create(optionId, true, CustomOptionType.Crewmate, "PoliceSurgeon_IncludeErrorInDeathTime", true, Option); optionId++;
            MarginOfErrorToIncludeInTimeOfDeath = Create(optionId, true, CustomOptionType.Crewmate, "PoliceSurgeonMarginOfErrorToIncludeInTimeOfDeath", 5f, 1f, 15f, 1f, IncludeErrorInDeathTime);
        }
    }

    internal static class RoleData
    {
        public static List<PlayerControl> Player;
        public static Color32 color = new(137, 195, 235, byte.MaxValue);
        public static bool HaveVital;
        public static string OfficialDateNotation;
        /// <summary>
        /// 死亡情報を管理する辞書, 死体検案書の発行時 情報はここから取得する。
        /// key = PlayerId, Value.Item1 = 死亡(推定)時刻, Value.Item2 = 死因, Value.Item3 = 死亡記録ターン,
        /// </summary>
        public static Dictionary<byte, (int, int, int)> ActualDeathTimeManager;

        /// <summary>
        /// 全体の死体検案書の情報を保管する辞書
        /// key = 発行ターン, Value.Item1 = 死体検案書
        /// </summary>
        public static Dictionary<int, string> PostMortemCertificateFullText;

        /// <summary>
        /// 個人単位の死体検案書の情報を保管する辞書
        /// key = PlayerId, Value.Item1 = 発行ターン, Value.Item2 = 死体検案書
        /// </summary>
        public static Dictionary<int, (int, string)> PostMortemCertificateShortText;

        public static Sprite GetButtonSprite() =>
            ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.PoliceSurgeonButton.png", 115f);

        public static void ClearAndReload()
        {
            Player = new();
            HaveVital = CustomOptionData.HaveVitalsInTaskPhase.GetBool();
            OfficialDateNotation = PostMortemCertificate_CreateAndGet.GetOfficialDateNotation();
            ActualDeathTimeManager = new();
            PostMortemCertificateFullText = new();
            PostMortemCertificateShortText = new();
        }
    }

    // 通常モードの時とSHRでHostの時, 且つバイタルを有する設定の時に警察医を科学者置き換えにする
    public static void FixedUpdate()
    {
        if (!RoleData.HaveVital) return;
        if (CachedPlayer.LocalPlayer.Data.Role == null || !CachedPlayer.LocalPlayer.IsRole(RoleTypes.Scientist))
        {
            VitalAbilityCoolSettings();
            new LateTask(() => { FastDestroyableSingleton<RoleManager>.Instance.SetRole(CachedPlayer.LocalPlayer, RoleTypes.Scientist); }, 0f, "ScientistSet");
        }
    }

    // バイタルの時間設定系の反映
    public static void VitalAbilityCoolSettings()
    {
        if (PlayerControl.LocalPlayer.GetRole() != RoleId.PoliceSurgeon) return;
        if (ModeHandler.IsMode(ModeId.SuperHostRoles, ModeId.CopsRobbers)) return;

        var optData = SyncSetting.DefaultOption.DeepCopy();

        optData.SetFloat(FloatOptionNames.ScientistCooldown, CustomOptionData.VitalsDisplayCooldown.GetFloat());
        optData.SetFloat(FloatOptionNames.ScientistBatteryCharge, CustomOptionData.BatteryDuration.GetFloat());

        GameManager.Instance.LogicOptions.SetGameOptions(optData);
    }
}

[Harmony]
/// <summary>
/// 死亡(推定)時刻の計算や保存に関係するメソッドを纏めたクラス
/// </summary>
internal static class PostMortemCertificate_AddActualDeathTime
{
#pragma warning disable 8321
    // 警察医が存在しない場合読む必要のないHarmonyPatchをまとめている
    internal static void Harmony()
    {
        if (RoleData.Player.Count <= 0) return; // 警察医が存在しない場合harmonyを読まないようにする。

        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.ReportDeadBody)), HarmonyPostfix]
        static void ReportDeadBody_Postfix() => AddActualDeathTime((int)DeadTiming.TaskPhase_killed); // タスクフェイズ中の死亡を処理する。

        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.CheckForEndVoting)), HarmonyPostfix]
        // 会議中の死亡を記録する。
        static void CheckForEndVoting_Postfix() => AddActualDeathTime((int)DeadTiming.MeetingPhase);

        [HarmonyPatch(typeof(ExileController), nameof(ExileController.WrapUp)), HarmonyPostfix]
        // 追放による死亡を記録する。(Airship以外)
        static void WrapUp_Postfix(ExileController __instance)
        {
            if (__instance.exiled != null && __instance.exiled.Object == null) __instance.exiled = null;
            PlayerControl exiledObject = __instance.exiled != null ? __instance.exiled.Object : null;
            AddActualDeathTime((int)DeadTiming.Exited, exiledObject);
        }

        [HarmonyPatch(typeof(AirshipExileController), nameof(AirshipExileController.WrapUpAndSpawn)), HarmonyPostfix]
        // 追放による死亡を記録する。(Airship)
        static void WrapUpAndSpawn_Postfix(AirshipExileController __instance)
        {
            if (__instance.exiled != null && __instance.exiled.Object == null) __instance.exiled = null;
            PlayerControl exiledObject = __instance.exiled != null ? __instance.exiled.Object : null;
            AddActualDeathTime((int)DeadTiming.Exited, exiledObject);
        }
#pragma warning restore 8321
    }

    /// <summary>
    /// ホストからrpcで送られた死体検案書用死亡情報を辞書[ActualDeathTimeManager]に格納する
    /// </summary>
    /// <param name="victimPlayerId">死者のPlayerId => Key</param>
    /// <param name="actualDeathTime">死亡(推定)時刻 => value.Item1</param>
    /// <param name="deadReason">死因 => value.Item2</param>
    internal static void RPCImportActualDeathTimeManager(byte victimPlayerId, byte actualDeathTime, byte deadReason)
    {
        if (!RoleData.ActualDeathTimeManager.ContainsKey(victimPlayerId))
            RoleData.ActualDeathTimeManager.Add(victimPlayerId, (actualDeathTime, deadReason, ReportDeadBodyPatch.MeetingTurn_Now));
    }
    /// <summary>
    /// 死亡(推定)時刻の計算 & 辞書[ActualDeathTimeManager]に全死亡情報を保存する。
    /// </summary>
    /// <param name="timingOfCall">死亡推定時刻を辞書に保存するメソッドが どこで呼び出されたか</param>
    /// <param name="playerWhoPlansToDie">死亡が予定されているプレイヤー(IsAliveがtrueになっているが後で死ぬプレイヤー)</param>
    private static void AddActualDeathTime(int timingOfCall, PlayerControl playerWhoPlansToDie = null)
    {
        if (!AmongUsClient.Instance.AmHost) return;
        if (RoleData.Player.Count <= 0) return;

        var reportTime = DateTime.Now;

        foreach (PlayerControl p in CachedPlayer.AllPlayers)
        {
            if (RoleData.ActualDeathTimeManager.ContainsKey(p.PlayerId)) continue;
            if (p.IsAlive() && p != playerWhoPlansToDie) continue;

            int actualDeathTime = 0;
            int deadReason = timingOfCall;

            switch (timingOfCall)
            {
                case (int)DeadTiming.TaskPhase_killed:
                    // 遺言伝達者の辞書に死亡時刻が保存されているならば、死亡(推定)時刻を取得する。
                    // そうでなければ殺された方法(死亡理由)を、(タスクターン中の)追放に変更する。
                    if (DeadPlayer.ActualDeathTime.ContainsKey(p.PlayerId)) actualDeathTime = CalculateEstimatedTimeOfDeath(reportTime, p);
                    else deadReason = (int)DeadTiming.TaskPhase_Exited;
                    break;
                case (int)DeadTiming.TaskPhase_Exited:
                case (int)DeadTiming.MeetingPhase:
                case (int)DeadTiming.Exited:
                    break;
            }
            Logger.Info($"{p.name} : 死亡推定時刻_{actualDeathTime}s");

            // 死亡情報を一元管理している辞書に保存する。(この辞書に保存するのはここでのみ)
            RoleData.ActualDeathTimeManager.Add(p.PlayerId, (actualDeathTime, deadReason, ReportDeadBodyPatch.MeetingTurn_Now));

            if (ModeHandler.IsMode(ModeId.SuperHostRoles)) continue; // SHRの場合RPCは送らない

            // ゲストのActualDeathTimeManagerに死亡情報を保存させるために送信
            MessageWriter writer = RPCHelper.StartRPC(CustomRPC.PoliceSurgeonSendActualDeathTimeManager);
            writer.Write(p.PlayerId);
            writer.Write((byte)actualDeathTime);
            writer.Write((byte)deadReason);
            writer.EndRPC();
        }
    }

    /// <summary>
    /// 死亡推定時刻を辞書に保存するメソッドが どこで呼び出されたかを記す。
    /// </summary>
    internal enum DeadTiming
    {
        TaskPhase_killed,
        TaskPhase_Exited,
        MeetingPhase,
        Exited,
    }

    /// <summary>
    /// 遺言伝達者の死亡時刻リストに乗っている死者の 死亡推定時刻の計算
    /// </summary>
    /// <param name="reportTime">死体が通報された時間</param>
    /// <param name="player">死者のPlayerControl</param>
    /// <returns>int : 死亡推定時刻[s]</returns>
    private static int CalculateEstimatedTimeOfDeath(DateTime reportTime, PlayerControl player)
    {
        DateTime actualDeathTime = DeadPlayer.ActualDeathTime[player.PlayerId].Item1;
        int seed = (int)actualDeathTime.Ticks;
        TimeSpan relativeDeathTime; // 相対死亡時刻 (ログ表記用)
        TimeSpan estimatedDeathTime; // 死亡推定時刻
        TimeSpan errorTimeSpan = new(0, 0, 0, 0); // 誤差秒数

        relativeDeathTime = estimatedDeathTime = reportTime - actualDeathTime; // 相対死亡時刻の計算 死亡推定時刻にも初期値(基準の値)として代入。

        if (CustomOptionData.IncludeErrorInDeathTime.GetBool())
        {
            errorTimeSpan = CalculateErrorTimeSpan(seed);
            estimatedDeathTime += errorTimeSpan;
            estimatedDeathTime = estimatedDeathTime >= TimeSpan.Zero ? estimatedDeathTime : -estimatedDeathTime;
        }

        Logger.Info($"{player.name} : 絶対死亡時刻_{actualDeathTime:hh:mm:ss} 通報時刻_{reportTime:ss}s 相対死亡時刻_{relativeDeathTime:ss}s 死亡推定時刻_{estimatedDeathTime:ss}s前 誤差設定_{CustomOptionData.IncludeErrorInDeathTime.GetBool()} 誤差範囲設定_{CustomOptionData.MarginOfErrorToIncludeInTimeOfDeath.GetFloat()} 誤差秒数_{(errorTimeSpan >= TimeSpan.Zero ? "+" : "-")}{errorTimeSpan:ss}s");
        return int.Parse($"{estimatedDeathTime:ss}");
    }

    /// <summary>
    /// 死亡時刻に含める誤差を求める
    /// </summary>
    /// <param name="seed">乱数のシード値 (死亡時刻のミリ秒)</param>
    /// <returns>TimeSpan : 誤差</returns>
    private static TimeSpan CalculateErrorTimeSpan(int seed)
    {
        int marginOfError = Mathf.Abs((int)CustomOptionData.MarginOfErrorToIncludeInTimeOfDeath.GetFloat());
        System.Random random = new(seed);
        int error = random.Next(0, marginOfError * 2 + 1) - marginOfError;
        return new TimeSpan(0, 0, 0, error);
    }
}

[HarmonyPatch]
/// <summary>
/// 死体検案書の表示関連のメソッドを集約したクラス
/// </summary>
internal static class PostMortemCertificate_Display
{
#pragma warning disable 8321
    // 警察医が存在しない場合読む必要のないHarmonyPatchをまとめている
    internal static void Harmony()
    {
        if (RoleData.Player.Count <= 0) return; // 警察医が存在しない場合harmonyを読まないようにする。

        // 会議開始時 警察医に死体検案書を送信する。
        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Start)), HarmonyPostfix]
        static void MeetingHudStart_Postfix(MeetingHud __instance)
        {
            Event(__instance);

            if (!AmongUsClient.Instance.AmHost) return;

            bool canResend = CustomOptionData.CanResend.GetBool();
            foreach (var pl in RoleData.Player)
            {
                Patches.AddChatPatch.ChatInformation(pl, ModTranslation.GetString("PoliceSurgeonName"), PostMortemCertificate_CreateAndGet.GetPostMortemCertificateFullText(pl), "#89c3eb");
                if (canResend) Patches.AddChatPatch.ChatInformation(pl, ModTranslation.GetString("PoliceSurgeonName"), AboutResendPostMortemCertificate(), "#89c3eb");
            }
        }

        // overlayを閉じる時。
        [HarmonyPatch(typeof(KeyboardJoystick), nameof(KeyboardJoystick.Update)), HarmonyPostfix]
        static void KeyboardJoystickUpdatePostfix(KeyboardJoystick __instance)
        {
            if (!PlayerControl.LocalPlayer.IsRole(RoleId.PoliceSurgeon)) return;
            // チャットを開いた時、Esc, Tab, hキーを押した時に 死体検案書が開かれていれば 死体検案書を閉じる。
            if (!OverlayInfo.overlayShown) return;
            if (FastDestroyableSingleton<HudManager>.Instance.Chat.IsOpenOrOpening
                || Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(KeyCode.H))
                OverlayInfo.HideInfoOverlay();
        }
    }
#pragma warning restore 8321

    /// <summary>
    /// 自投票リセット形式での死体検案書閲覧要求
    /// </summary>
    /// <param name="srcPlayerId">投票者のplayerId</param>
    /// <param name="suspectPlayerId">投票先のplayerId</param>
    /// <returns> true : 投票を反映する / false : 投票を反映しない </returns>
    internal static bool MeetingHudCastVote_Prefix(byte srcPlayerId, byte suspectPlayerId)
    {
        // SHRで, 設定が有効な時, 警察医が自投票していたら
        if (!(ModeHandler.IsMode(ModeId.SuperHostRoles) && AmongUsClient.Instance.AmHost)) return true;

        if (!CustomOptionData.CanResend.GetBool()) return true;
        if (srcPlayerId != suspectPlayerId) return true;

        PlayerControl srcPlayer = ModHelpers.GetPlayerControl(srcPlayerId);
        if (!srcPlayer.IsRole(RoleId.PoliceSurgeon)) return true;

        // 死体検案書全文を送信する。
        AddChatPatch.ChatInformation(srcPlayer, ModTranslation.GetString("PoliceSurgeonName"), PostMortemCertificate_CreateAndGet.GetPostMortemCertificateFullText(srcPlayer), "#89c3eb");

        return false;
    }

    // 死体検案書再確認方法に関するシステムメッセージ
    private static string AboutResendPostMortemCertificate()
    {
        string text;
        if (ModeHandler.IsMode(ModeId.SuperHostRoles))
            text = $"<align={"left"}>{ModTranslation.GetString("PoliceSurgeonResendSHR")}</align>";
        else
            text = $"<align={"left"}>{ModTranslation.GetString("PoliceSurgeonResendSNR")}</align>";
        return text;
    }
    // ネームプレート上に死体検案書を確認するためのボタンを作成する。
    private static void Event(MeetingHud __instance)
    {
        if (ModeHandler.IsMode(ModeId.SuperHostRoles)) return;
        if (!PlayerControl.LocalPlayer.IsRole(RoleId.PoliceSurgeon)) return;

        for (int i = 0; i < __instance.playerStates.Length; i++)
        {
            PlayerVoteArea playerVoteArea = __instance.playerStates[i];
            var player = ModHelpers.PlayerById(__instance.playerStates[i].TargetPlayerId);
            var playerRole = player.GetRole();

            // 再確認確認な設定で、ネームプレートの対象が生存していて、自分自身ではないなら
            if (CustomOptionData.CanResend.GetBool()) { if (player.IsAlive() && player.PlayerId != CachedPlayer.LocalPlayer.PlayerId) continue; }
            // 再確認不可能な設定で、ネームプレートの対象が生存している、又は本人ならば
            else if (player.IsAlive() || player.PlayerId == CachedPlayer.LocalPlayer.PlayerId) continue;
            // 死亡した者が妖狐でもヴァンパイアでも眷属でもないなら
            if (playerRole is RoleId.Fox or RoleId.Vampire or RoleId.Dependents) continue;
            // 死亡ターン以外に情報を表示しない設定で、ネームプレートの対象が現在ターンに死亡した者でなく、自分自身でもないなら
            if (!CustomOptionData.IndicateTimeOfDeathInSubsequentTurn.GetBool() && RoleData.ActualDeathTimeManager[player.PlayerId].Item3 != ReportDeadBodyPatch.MeetingTurn_Now && player.PlayerId != CachedPlayer.LocalPlayer.PlayerId) continue;

            GameObject template = playerVoteArea.Buttons.transform.Find("CancelButton").gameObject;
            GameObject targetBox = UnityEngine.Object.Instantiate(template, playerVoteArea.transform);
            targetBox.name = "PoliceSurgeonButton";
            targetBox.transform.localPosition = new Vector3(1f, 0.03f, -8.0f);
            SpriteRenderer renderer = targetBox.GetComponent<SpriteRenderer>();
            renderer.sprite = RoleData.GetButtonSprite();
            renderer.sortingOrder = 0;
            PassiveButton button = targetBox.GetComponent<PassiveButton>();
            button.OnClick.RemoveAllListeners();
            int TargetPlayerId = player.PlayerId;
            button.OnClick.AddListener((UnityEngine.Events.UnityAction)(() => OnClick(TargetPlayerId)));
            Logger.Info($"{player.name}に死体検案書を確認する為のボタンを作成します。", "PoliceSurgeonButton");
        }
    }

    // 確認ボタンを押したときの動作。
    private static void OnClick(int TargetPlayerId)
    {
        var target = ModHelpers.PlayerById((byte)TargetPlayerId);

        // 自分に表示されているボタンの場合死体検案書全文をチャットに表示する。
        if (target == PlayerControl.LocalPlayer)
        {
            FastDestroyableSingleton<HudManager>.Instance.Chat.AddChat(target, PostMortemCertificate_CreateAndGet.GetPostMortemCertificateFullText(target));
            return;
        }

        if (FastDestroyableSingleton<HudManager>.Instance.Chat.IsOpenOrOpening && OverlayInfo.overlayShown)
            OverlayInfo.HideInfoOverlay();
        else OverlayInfo.YoggleInfoOverlay(target);
    }

    // 死体検案書用のoverlayの作成に関わるクラス
    private class OverlayInfo
    {
        private static Sprite colorBG;
        private static SpriteRenderer infoUnderlay;
        private static SpriteRenderer meetingUnderlay;
        private static TMPro.TextMeshPro infoOverlay;
        public static bool overlayShown = false;

        // overlayの初期化
        public static bool InitializeOverlays()
        {
            HudManager hudManager = FastDestroyableSingleton<HudManager>.Instance;
            if (hudManager == null) return false;

            if (colorBG == null)
                colorBG = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.White.png", 100f);

            if (meetingUnderlay == null)
            {
                meetingUnderlay = UnityEngine.Object.Instantiate(hudManager.FullScreen, hudManager.transform);
                meetingUnderlay.transform.localPosition = new Vector3(0f, 0f, 20f);
                meetingUnderlay.gameObject.SetActive(true);
                meetingUnderlay.enabled = false;
            }

            if (infoUnderlay == null)
            {
                infoUnderlay = UnityEngine.Object.Instantiate(meetingUnderlay, hudManager.transform);
                infoUnderlay.transform.localPosition = new Vector3(0f, 0f, -9.367681f);
                infoUnderlay.gameObject.SetActive(true);
                infoUnderlay.enabled = false;
            }

            if (infoOverlay == null)
            {
                infoOverlay = UnityEngine.Object.Instantiate(hudManager.TaskPanel.taskText, hudManager.transform);
                infoOverlay.fontSize = infoOverlay.fontSizeMin = infoOverlay.fontSizeMax = 1.15f;
                infoOverlay.autoSizeTextContainer = false;
                infoOverlay.enableWordWrapping = false;
                infoOverlay.alignment = TMPro.TextAlignmentOptions.TopLeft;
                infoOverlay.transform.position = Vector3.zero;
                // 死体検案書をoverlayの真ん中に表示する
                infoOverlay.transform.localPosition = new Vector3(-1.5f, 1.15f, -10f);
                infoOverlay.transform.localScale = Vector3.one;
                infoOverlay.color = Palette.Black;
                infoOverlay.enabled = false;
            }
            return true;
        }

        // overlayの表示と文字表記
        public static void ShowInfoOverlay(PlayerControl target)
        {
            if (overlayShown) return;

            HudManager hudManager = FastDestroyableSingleton<HudManager>.Instance;
            if
            (
                (
                    MapUtilities.CachedShipStatus == null ||
                    PlayerControl.LocalPlayer == null ||
                    hudManager == null ||
                    FastDestroyableSingleton<HudManager>.Instance.IsIntroDisplayed
                )
                && MeetingHud.Instance != null
            )
                return;

            if (!InitializeOverlays()) return;

            if (MapBehaviour.Instance != null)
                MapBehaviour.Instance.Close();

            hudManager.SetHudActive(false);

            overlayShown = true;

            Transform parent = MeetingHud.Instance != null ? MeetingHud.Instance.transform : hudManager.transform;
            infoUnderlay.transform.parent = parent;
            infoOverlay.transform.parent = parent;

            infoUnderlay.sprite = colorBG;
            infoUnderlay.color = new Color(192f / 255f, 198f / 255f, 201f / 255f); // #c0c6c9 灰青
            infoUnderlay.transform.localScale = new Vector3(7.5f, 5f, 1f);
            infoUnderlay.enabled = true;

            StringBuilder text = new();
            // ボタン対象の死体検案書を取得する
            text.Append(PostMortemCertificate_CreateAndGet.GetPostMortemCertificateShortText(PlayerControl.LocalPlayer, target));
            infoOverlay.text = text.ToString();
            infoOverlay.enabled = true;

            var underlayTransparent = new Color(0.1f, 0.1f, 0.1f, 0.0f);
            var underlayOpaque = infoUnderlay.color;
            FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(0.2f, new Action<float>(t =>
            {
                infoUnderlay.color = Color.Lerp(underlayTransparent, underlayOpaque, t);
                infoOverlay.color = Color.Lerp(Palette.ClearWhite, Palette.White, t);
            })));
        }

        // overlayを消す
        public static void HideInfoOverlay()
        {
            if (!overlayShown) return;

            if (MeetingHud.Instance == null) FastDestroyableSingleton<HudManager>.Instance.SetHudActive(true);

            overlayShown = false;
            var underlayTransparent = new Color(0.1f, 0.1f, 0.1f, 0.0f);
            var underlayOpaque = new Color(192f / 255f, 198f / 255f, 201f / 255f);

            FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(0.2f, new Action<float>(t =>
            {
                if (infoUnderlay != null)
                {
                    infoUnderlay.color = Color.Lerp(underlayOpaque, underlayTransparent, t);
                    if (t >= 1.0f) infoUnderlay.enabled = false;
                }

                if (infoOverlay != null)
                {
                    infoOverlay.color = Color.Lerp(Palette.White, Palette.ClearWhite, t);
                    if (t >= 1.0f) infoOverlay.enabled = false;
                }
            })));
        }

        public static void YoggleInfoOverlay(PlayerControl target)
        {
            if (overlayShown) HideInfoOverlay();
            else ShowInfoOverlay(target);
        }
    }
}

/// <summary>
/// 死体検案書の取得及び作成関連のメソッドを集約したクラス
/// </summary>
internal static class PostMortemCertificate_CreateAndGet
{
    /// <summary>
    /// 全文の死体検案書を取得する。
    /// (そのターンの検案書が辞書に格納されていたらそれを取得し、されていなかったら作成して辞書に保存する。)
    /// </summary>
    /// <param name="policeSurgeon">送信先の警察医</param>
    /// <returns>string : 死体検案書 (名前は送信先の警察医名に置き換わっている)</returns>

    internal static string GetPostMortemCertificateFullText(PlayerControl policeSurgeon)
    {
        if (!RoleData.PostMortemCertificateFullText.ContainsKey(ReportDeadBodyPatch.MeetingTurn_Now))
            RoleData.PostMortemCertificateFullText.Add(ReportDeadBodyPatch.MeetingTurn_Now, CreateBody());
        return string.Format(RoleData.PostMortemCertificateFullText[ReportDeadBodyPatch.MeetingTurn_Now], policeSurgeon.name);
    }

    /// <summary>
    /// 対象プレイヤー個人の死体検案書を取得する。
    /// (対象の検案書が辞書に格納されていたら取得し、されていなかったら作成して辞書に保存する。)
    /// </summary>
    /// <param name="policeSurgeon">閲覧する警察医</param>
    /// <param name="victimPlayer">死体検案書を確認したい対象</param>
    /// <returns>string : 死体検案書 (名前は閲覧する警察医名に置き換わっている)</returns>
    internal static string GetPostMortemCertificateShortText(PlayerControl policeSurgeon, PlayerControl victimPlayer)
    {
        if (!RoleData.PostMortemCertificateShortText.ContainsKey(victimPlayer.PlayerId))
            RoleData.PostMortemCertificateShortText[victimPlayer.PlayerId] = (RoleData.ActualDeathTimeManager[victimPlayer.PlayerId].Item3, CreateBody(victimPlayer));
        return string.Format(RoleData.PostMortemCertificateShortText[victimPlayer.PlayerId].Item2, policeSurgeon.name);
    }

    /// <summary>
    /// 公的な年月日を文字列として取得する
    /// (日本語 => 和歴, 繁体字 => 西暦漢数字, 簡体字 => 民国歴 or 西暦漢数字, 其の他 =>西暦アメリカ英語)
    /// </summary>
    /// <returns>公的な年月日の文字列</returns>
    // 参考 => https://csharp.programmer-reference.com/datetime-wareki/
    // 参考 => https://www.ipentec.com/document/csharp-datetime-get-english-month-and-date-name-in-format-string
    internal static string GetOfficialDateNotation()
    {
        string dateOfDocumentIssuance;

        SupportedLangs langId = TranslationController.InstanceExists ? TranslationController.Instance.currentLanguage.languageID : DataManager.Settings.Language.CurrentLanguage;
        CultureInfo ci = new("en-US");

        switch (langId)
        {
            case SupportedLangs.Japanese:
                JapaneseCalendar jc = new();
                ci = new("Ja-JP", true);
                ci.DateTimeFormat.Calendar = jc;
                dateOfDocumentIssuance = DateTime.Now.ToString("ggy年 M月 d日", ci);
                break;
            case SupportedLangs.SChinese:
                dateOfDocumentIssuance = GetKanjiCalendar();
                break;
            case SupportedLangs.TChinese:
                if (CustomOptionData.IsUseTaiwanCalendar.GetBool())
                {
                    TaiwanCalendar tc = new();
                    ci = new("zh-TW", true);
                    ci.DateTimeFormat.Calendar = tc;
                    dateOfDocumentIssuance = DateTime.Now.ToString("ggy年 M月 d日", ci);
                }
                else dateOfDocumentIssuance = GetKanjiCalendar();
                break;
            default:
                dateOfDocumentIssuance = DateTime.Now.ToString("MMMM d, yyyy", ci);
                break;
        }
        Logger.Info("DateOfDocumentIssuance");
        return dateOfDocumentIssuance;
    }

    // 今日の日付を漢数字表記にする
    //  参考 => https://qiita.com/Akirakong/items/22477de0fff2e711e07f
    private static string GetKanjiCalendar()
    {
        string dateNum = DateTime.Now.ToString("yyyy年MM月dd日");

        // 年表記及び一の位(インデックスと要素(int.Parse(str))が一致している為、順序変更不可)
        string[] kanjiArr = { "〇", "一", "二", "三", "四", "五", "六", "七", "八", "九" };
        string[] kanjiArr10 = { "", "十", "二十", "三十" }; // 十の位

        StringBuilder returnBuilder = new();
        int i = 1; // foreachで現在処理している文字列の位(及び年月日)を示す為のインデックス

        foreach (char c in dateNum)
        {
            string str = c.ToString();

            if (i is 5 or 8 or 11) returnBuilder.Append(str); // 年月日を[int.Parse(str)](int変換)しないようにする
            else if (i is 6 or 9) returnBuilder.Append(kanjiArr10[int.Parse(str)]); // 月日の十の位を変換する
            else returnBuilder.Append(kanjiArr[int.Parse(str)]); // 上記に引っかからなかった部分を漢数字に変換する

            i++;
        }
        return returnBuilder.ToString();
    }

    /// <summary>
    /// 死体検案書の本体を作成する。
    /// 警察医名は{0}になっている為、呼び出し元で埋め込む必要がある。
    /// </summary>
    /// <param name="victimPlayer">個人単位の死体検案書を発行したい対象(nullの場合全体の死体検案書を発行する)</param>
    /// <returns></returns>
    private static string CreateBody(PlayerControl victimPlayer = null)
    {
        bool isWrite = false; // 死体検案書に書く死者の情報があるか
        StringBuilder builder = new();

        string fullDelimiterLine = "|--------------------------------------------------------------|";
        string shortDelimiterLine = "|----------------------------------------------------------------|";

        string delimiterLine = victimPlayer == null ? fullDelimiterLine : shortDelimiterLine;

        builder.AppendLine($"<align={"left"}>" + delimiterLine);
        builder.AppendLine(ModTranslation.GetString("PostMortemCertificate_main1"));
        builder.AppendLine(delimiterLine);
        if (victimPlayer == null) // 特定のプレイヤー指定ではない時、全員の死体検案書を取得する。
        {
            foreach (KeyValuePair<byte, (int, int, int)> kvp in RoleData.ActualDeathTimeManager)
            {
                // 以降に進むのは、[全てのターンの死亡情報を出す時]の全てのプレイヤーの情報と　[現在ターンの死亡情報しか出さない時]の現在ターンに死亡したプレイヤーの情報
                if (!CustomOptionData.IndicateTimeOfDeathInSubsequentTurn.GetBool() && kvp.Value.Item3 != ReportDeadBodyPatch.MeetingTurn_Now) continue;

                // 妖狐とヴァンパイアと眷属は検案書を作成しない
                var victimPlayerRole = ModHelpers.PlayerById(kvp.Key).GetRole();
                if (victimPlayerRole is RoleId.Fox or RoleId.Vampire or RoleId.Dependents) continue;

                isWrite = true;

                builder.Append(CreateContents(kvp.Key, kvp.Value.Item1, kvp.Value.Item2, kvp.Value.Item3));
                builder.AppendLine(delimiterLine);
            }
        }
        else
        {
            isWrite = true;

            var victimPlayerId = victimPlayer.PlayerId;
            var deadTime = RoleData.ActualDeathTimeManager[victimPlayerId].Item1;
            var deadReason = RoleData.ActualDeathTimeManager[victimPlayerId].Item2;
            var deadTurn = RoleData.ActualDeathTimeManager[victimPlayerId].Item3;

            builder.Append(CreateContents(victimPlayerId, deadTime, deadReason, deadTurn));
            builder.AppendLine(delimiterLine);
        }

        if (isWrite) // 死体検案書に記載する 死者の情報がある時
        {
            builder.AppendLine(ModTranslation.GetString("PostMortemCertificate_main3")); // 上記のとおり<s>診断</s>(検案)する
            builder.AppendLine($"{ModTranslation.GetString("PostMortemCertificate_main4")}{RoleData.OfficialDateNotation}"); // 本診断書(検案書) 発行年月日
            builder.AppendLine($"{(MapNames)GameManager.Instance.LogicOptions.currentGameOptions.MapId}"); // マップ名
            builder.AppendLine("");
            builder.AppendLine($"{ModTranslation.GetString("PostMortemCertificate_main5")} {{0}}"); // (氏名) 医師
            builder.AppendLine(delimiterLine);
            builder.AppendLine("");
        }
        else // 死者の情報がない時はランダムに警察医の独白を出す
        {
            builder = new();

            System.Random random = new((int)DateTime.Now.Ticks);
            int rand = random.Next(1, 15 + 1);
            string transRandomText = ModTranslation.GetString($"PostMortemCertificate_NoDeaths_RandomMessage_{rand}");

            builder.AppendLine($"<align={"left"}>{string.Format(ModTranslation.GetString("PostMortemCertificate_NoDeaths"), RoleData.OfficialDateNotation, "{0}", transRandomText)}");
        }

        // 死体検案書の文字サイズと色をchat式とCustomoverlay式に合わせて変更する
        var sizeColor = victimPlayer == null ? "<size=100%>" : "<size=200%><color=#7d7d7d>";
        builder.Insert(0, sizeColor);
        if (victimPlayer != null)
            builder.AppendLine("</align></color></size>");
        else
        {
            builder.AppendLine("</align></size>");
            builder.Replace("<color=#89c3eb>", "<color=#5654a2>");
        }
        return builder.ToString();

        /*構造メモ
        |----------------------------------------------------|
        <s>死亡診断書</s> (死体検案書)
        |----------------------------------------------------|

        氏名 ◯◯◯◯◯◯◯◯◯◯

        死亡したとき 令和  年  月  日 (  ターン前)   秒前 (推定) // <= 死亡推定時刻設定の時
        死亡したとき 令和  年  月  日 (  ターン前)   秒前 (確認) // <= 死亡時刻設定の時
        死亡したとき 令和  年  月  日 (  ターン前) (頃) // <= MurderPlayer以外の死亡の時

        死因 不詳の外因 // <= 死因が会議追放以外の時
        死因 追放 // <= 死因が会議追放の時

        |-----------------------------------------------------|

        繰り返し

        |-----------------------------------------------------|

        上記のとおり<s>診断</s>(検案)する
        <s>本診断書</s>(検案書) 発行年月日 令和  年  月  日
        Map名

        (氏名) 医師 ◯◯◯◯◯◯◯◯◯◯

        |-----------------------------------------------------|
        */
    }

    /// <summary>
    /// 死体検案書の中身を作成する
    /// </summary>
    /// <param name="victimPlayerId">作成対象</param>
    /// <param name="deadTime">対象の死亡(推定)時刻</param>
    /// <param name="deadReason">死亡理由</param>
    /// <param name="DeadTurn">死亡ターン</param>
    /// <returns>string = 作成対象の死体検案書の文章</returns>
    private static string CreateContents(byte victimPlayerId, int deadTime, int deadReason, int DeadTurn)
    {
        StringBuilder builder = new();

        // ターンを表記する設定が有効か? (全てのターンの死亡情報を出す設定 且つ 死亡ターンを表記する設定 の時に有効)
        var isWritingTurn = CustomOptionData.IndicateTimeOfDeathInSubsequentTurn.GetBool() && CustomOptionData.HowManyTurnAgoTheDied.GetBool();
        var inError = CustomOptionData.IncludeErrorInDeathTime.GetBool();

        var deadPlayer = ModHelpers.GetPlayerControl(victimPlayerId);
        builder.AppendLine($"{ModTranslation.GetString("PostMortemCertificate_main2")}<color=#89c3eb>{deadPlayer.name}</color>");

        switch (deadReason)
        {
            case (int)PostMortemCertificate_AddActualDeathTime.DeadTiming.TaskPhase_killed:
                if (inError)
                {
                    if (isWritingTurn)
                        // 死亡したとき {0:年月日} ({1:Value.Item3}ターン前) {2:kvp.Value.Item2}秒前 ({3:推定})
                        builder.AppendLine($"{string.Format(ModTranslation.GetString("PostMortemCertificate_AlreadyKnown_WriteTurn"), RoleData.OfficialDateNotation, ReportDeadBodyPatch.MeetingTurn_Now - DeadTurn, deadTime, ModTranslation.GetString("PostMortemCertificate_CauseOfDeath1"))}");
                    else// 死亡したとき {0:年月日} {1:kvp.Value.Item2}秒前 ({2:推定})
                        builder.AppendLine($"{string.Format(ModTranslation.GetString("PostMortemCertificate_AlreadyKnown"), RoleData.OfficialDateNotation, deadTime, ModTranslation.GetString("PostMortemCertificate_CauseOfDeath1"))}");
                }
                else
                {
                    if (isWritingTurn)
                        // 死亡したとき {0:年月日} ({1:Value.Item3}ターン前) {2:kvp.Value.Item2}秒前 ({3:確認})
                        builder.AppendLine($"{string.Format(ModTranslation.GetString("PostMortemCertificate_AlreadyKnown_WriteTurn"), RoleData.OfficialDateNotation, ReportDeadBodyPatch.MeetingTurn_Now - DeadTurn, deadTime, ModTranslation.GetString("PostMortemCertificate_CauseOfDeath2"))}");
                    else// 死亡したとき {0:年月日} {1:kvp.Value.Item2}秒前 ({2:確認})
                        builder.AppendLine($"{string.Format(ModTranslation.GetString("PostMortemCertificate_AlreadyKnown"), RoleData.OfficialDateNotation, deadTime, ModTranslation.GetString("PostMortemCertificate_CauseOfDeath2"))}");
                }
                builder.AppendLine("");
                builder.AppendLine($"{ModTranslation.GetString("PostMortemCertificate_DeadReason1")}"); // 死因 不詳の外因死
                builder.AppendLine("");
                break;

            case (int)PostMortemCertificate_AddActualDeathTime.DeadTiming.TaskPhase_Exited:
            case (int)PostMortemCertificate_AddActualDeathTime.DeadTiming.MeetingPhase:
                if (isWritingTurn)
                    // 死亡したとき {0:年月日} ({1:Value.Item3}ターン前) ({2:頃})
                    builder.AppendLine($"{string.Format(ModTranslation.GetString("PostMortemCertificate_Unknown_WriteTurn"), RoleData.OfficialDateNotation, ReportDeadBodyPatch.MeetingTurn_Now - DeadTurn, ModTranslation.GetString("PostMortemCertificate_CauseOfDeath3"))}");
                else// 死亡したとき {0:年月日} ({1:頃})
                    builder.AppendLine($"{string.Format(ModTranslation.GetString("PostMortemCertificate_Unknown"), RoleData.OfficialDateNotation, ModTranslation.GetString("PostMortemCertificate_CauseOfDeath3"))}");
                builder.AppendLine("");
                builder.AppendLine($"{ModTranslation.GetString("PostMortemCertificate_DeadReason1")}"); // 死因 不詳の外因死
                builder.AppendLine("");
                break;

            case (int)PostMortemCertificate_AddActualDeathTime.DeadTiming.Exited:
                if (isWritingTurn)
                    // 死亡したとき {0:年月日} ({1:Value.Item3}ターン前) ({2:頃})
                    builder.AppendLine($"{string.Format(ModTranslation.GetString("PostMortemCertificate_Unknown_WriteTurn"), RoleData.OfficialDateNotation, ReportDeadBodyPatch.MeetingTurn_Now - DeadTurn, ModTranslation.GetString("PostMortemCertificate_CauseOfDeath3"))}");
                else// 死亡したとき {0:年月日} ({1:頃})
                    builder.AppendLine($"{string.Format(ModTranslation.GetString("PostMortemCertificate_Unknown"), RoleData.OfficialDateNotation, ModTranslation.GetString("PostMortemCertificate_CauseOfDeath3"))}");
                builder.AppendLine("");
                builder.AppendLine($"{ModTranslation.GetString("PostMortemCertificate_DeadReason2")}"); // 死因 追放
                builder.AppendLine("");
                break;
        }
        return builder.ToString();
    }
}