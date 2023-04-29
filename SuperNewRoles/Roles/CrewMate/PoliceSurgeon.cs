using System;
using System.Text;
using System.Globalization;
using System.Collections.Generic;
using AmongUs.GameOptions;
using AmongUs.Data;
using HarmonyLib;
using UnityEngine;
using SuperNewRoles.Mode;
using SuperNewRoles.Mode.SuperHostRoles;
using static SuperNewRoles.Modules.CustomOption;
using static SuperNewRoles.Roles.Crewmate.PoliceSurgeon;

namespace SuperNewRoles.Roles.Crewmate;
public static class PoliceSurgeon
{
    private static int optionId = 1266;
    public static CustomRoleOption PoliceSurgeonOption;
    public static CustomOption PoliceSurgeonPlayerCount;
    public static CustomOption PoliceSurgeonHaveVitalsInTaskPhase;
    public static CustomOption PoliceSurgeonVitalsDisplayCooldown;
    public static CustomOption PoliceSurgeonBatteryDuration;
    public static CustomOption PoliceSurgeonIndicateTimeOfDeathInSubsequentTurn;
    public static CustomOption PoliceSurgeonHowManyTurnAgoTheDied;
    public static CustomOption PoliceSurgeon_IncludeErrorInDeathTime;
    public static CustomOption PoliceSurgeonMarginOfErrorToIncludeInTimeOfDeath;
    public static CustomOption PoliceSurgeonIsUseTaiwanCalendar;

    public static void SetupCustomOptions()
    {
        PoliceSurgeonOption = SetupCustomRoleOption(optionId, true, RoleId.PoliceSurgeon); optionId++;
        PoliceSurgeonPlayerCount = Create(optionId, true, CustomOptionType.Crewmate, "SettingPlayerCountName", CustomOptionHolder.CrewPlayers[0], CustomOptionHolder.CrewPlayers[1], CustomOptionHolder.CrewPlayers[2], CustomOptionHolder.CrewPlayers[3], PoliceSurgeonOption); optionId++;
        PoliceSurgeonHaveVitalsInTaskPhase = Create(optionId, true, CustomOptionType.Crewmate, "PoliceSurgeonHaveVitalsInTaskPhase", false, PoliceSurgeonOption); optionId++;
        PoliceSurgeonVitalsDisplayCooldown = Create(optionId, true, CustomOptionType.Crewmate, "VitalsDisplayCooldown", 15f, 5f, 60f, 5f, PoliceSurgeonHaveVitalsInTaskPhase); optionId++;
        PoliceSurgeonBatteryDuration = Create(optionId, true, CustomOptionType.Crewmate, "BatteryDuration", 5f, 5f, 30f, 5f, PoliceSurgeonHaveVitalsInTaskPhase); optionId++;
        PoliceSurgeonIndicateTimeOfDeathInSubsequentTurn = Create(optionId, true, CustomOptionType.Crewmate, "PoliceSurgeonIndicateTimeOfDeathInSubsequentTurn", true, PoliceSurgeonOption); optionId++;
        PoliceSurgeonHowManyTurnAgoTheDied = Create(optionId, true, CustomOptionType.Crewmate, "PoliceSurgeonHowManyTurnAgoTheDied", false, PoliceSurgeonIndicateTimeOfDeathInSubsequentTurn); optionId++;
        if (DataManager.Settings.Language.CurrentLanguage == SupportedLangs.TChinese) PoliceSurgeonIsUseTaiwanCalendar = Create(optionId, true, CustomOptionType.Crewmate, "PoliceSurgeonIsUseTaiwanCalendar", true, PoliceSurgeonOption); optionId++;
        PoliceSurgeon_IncludeErrorInDeathTime = Create(optionId, true, CustomOptionType.Crewmate, "PoliceSurgeon_IncludeErrorInDeathTime", true, PoliceSurgeonOption); optionId++;
        PoliceSurgeonMarginOfErrorToIncludeInTimeOfDeath = Create(optionId, true, CustomOptionType.Crewmate, "PoliceSurgeonMarginOfErrorToIncludeInTimeOfDeath", 5f, 1f, 15f, 1f, PoliceSurgeon_IncludeErrorInDeathTime);
    }

    public static List<PlayerControl> PoliceSurgeonPlayer;
    public static Color32 color = new(137, 195, 235, byte.MaxValue);
    public static bool HaveVital;
    public static int MeetingTurn_Now; // ReportDeadBodyで代入している為 Host以外は正常に反映されていません (SNRはクライアント個人処理の為同時にRpcで送る必要がある)
    public static string OfficialDateNotation;
    /// <summary>
    /// 死体検案書を保管する辞書
    /// key = 発行ターン, Value = 死体検案書
    /// </summary>
    public static Dictionary<int, string> PostMortemCertificate;
    /// <summary>
    /// key = PlayerId, Value.Item1 = 死亡時刻, Value.Item3 = 死因, Value.Item3 = 死亡記録ターン,
    /// </summary>
    public static Dictionary<byte, (int, int, int)> PoliceSurgeon_ActualDeathTimes;
    public static void ClearAndReload()
    {
        PoliceSurgeonPlayer = new();
        HaveVital = PoliceSurgeonHaveVitalsInTaskPhase.GetBool();
        MeetingTurn_Now = 0;
        OfficialDateNotation = PoliceSurgeon_PostMortemCertificate.GetOfficialDateNotation();
        PoliceSurgeon_ActualDeathTimes = new();
        PostMortemCertificate = new();
    }

    /// <summary>
    /// Defaultモード時且つバイタルを有する設定の時に警察医を科学者置き換えにする
    /// </summary>
    public static void FixedUpdate()
    {
        if (!HaveVital) return;
        if (CachedPlayer.LocalPlayer.Data.Role == null || !CachedPlayer.LocalPlayer.IsRole(RoleTypes.Scientist))
        {
            VitalAbilityCoolSettings();
            new LateTask(() => { FastDestroyableSingleton<RoleManager>.Instance.SetRole(CachedPlayer.LocalPlayer, RoleTypes.Scientist); }, 0f, "ScientistSet");
        }
    }

    public static void VitalAbilityCoolSettings()
    {
        if (PlayerControl.LocalPlayer.GetRole() != RoleId.PoliceSurgeon) return;
        if (ModeHandler.IsMode(ModeId.SuperHostRoles, ModeId.CopsRobbers)) return;

        var optData = SyncSetting.OptionData.DeepCopy();

        optData.SetFloat(FloatOptionNames.ScientistCooldown, PoliceSurgeonVitalsDisplayCooldown.GetFloat());
        optData.SetFloat(FloatOptionNames.ScientistBatteryCharge, PoliceSurgeonBatteryDuration.GetFloat());

        GameManager.Instance.LogicOptions.SetGameOptions(optData);
    }
}

[Harmony]
internal static class PoliceSurgeon_AddActualDeathTime
{
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.ReportDeadBody)), HarmonyPostfix]
    internal static void ReportDeadBody_Postfix()
    {
        MeetingTurn_Now++;
        Logger.Info("ぷぇ");
        AddActualDeathTime((int)DeadTiming.TaskPhase_killed);
    }

    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.CheckForEndVoting)), HarmonyPostfix]
    internal static void CheckForEndVoting_Postfix() => AddActualDeathTime((int)DeadTiming.MeetingPhase);

    [HarmonyPatch(typeof(ExileController), nameof(ExileController.WrapUp)), HarmonyPostfix]
    internal static void WrapUp_Postfix(ExileController __instance)
    {
        if (__instance.exiled != null && __instance.exiled.Object == null) __instance.exiled = null;
        PlayerControl exiledObject = __instance.exiled != null ? __instance.exiled.Object : null;
        AddActualDeathTime((int)DeadTiming.Exited, exiledObject);
    }

    [HarmonyPatch(typeof(AirshipExileController), nameof(AirshipExileController.WrapUpAndSpawn)), HarmonyPostfix]
    internal static void WrapUpAndSpawn_Postfix(AirshipExileController __instance)
    {
        if (__instance.exiled != null && __instance.exiled.Object == null) __instance.exiled = null;
        PlayerControl exiledObject = __instance.exiled != null ? __instance.exiled.Object : null;
        AddActualDeathTime((int)DeadTiming.Exited, exiledObject);
    }
    /// <summary>
    /// 死亡推定時刻を保存する
    /// </summary>
    /// <param name="timingOfCall">死亡推定時刻を辞書に保存するメソッドが どこで呼び出されたか</param>
    /// <param name="playerWhoPlansToDie">死亡が予定されているプレイヤー(IsAliveがtrueになっているが後で死ぬプレイヤー)</param>
    private static void AddActualDeathTime(int timingOfCall, PlayerControl playerWhoPlansToDie = null)
    {
        if (!AmongUsClient.Instance.AmHost) return;
        if (PoliceSurgeonPlayer.Count <= 0) return;

        var reportTime = DateTime.Now;

        foreach (PlayerControl p in CachedPlayer.AllPlayers)
        {
            if (PoliceSurgeon_ActualDeathTimes.ContainsKey(p.PlayerId)) continue;
            if (p.IsAlive() && p != playerWhoPlansToDie) continue;

            int actualDeathTime = 0;
            int deadReason = timingOfCall;

            switch (timingOfCall)
            {
                case (int)DeadTiming.TaskPhase_killed:
                    // 遺言伝達者の辞書に死亡時刻が保存されているならば、死亡(推定)時刻を取得する。
                    // そうでなければ殺された方法(死亡理由)を、(タスクターン中の)追放に変更する。
                    if (DyingMessenger.ActualDeathTime.ContainsKey(p.PlayerId)) actualDeathTime = CalculateEstimatedTimeOfDeath(reportTime, p);
                    else deadReason = (int)DeadTiming.TaskPhase_Exited;
                    break;
                case (int)DeadTiming.TaskPhase_Exited:
                case (int)DeadTiming.MeetingPhase:
                case (int)DeadTiming.Exited:
                    break;
            }
            Logger.Info($"{p.name} : 死亡推定時刻_{actualDeathTime}s");
            PoliceSurgeon_ActualDeathTimes.Add(p.PlayerId, (actualDeathTime, deadReason, MeetingTurn_Now));
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
    /// <returns>死亡推定時刻[s]をstring型で返却</returns>
    private static int CalculateEstimatedTimeOfDeath(DateTime reportTime, PlayerControl player)
    {
        DateTime actualDeathTime = DyingMessenger.ActualDeathTime[player.PlayerId].Item1;
        int seed = (int)actualDeathTime.Ticks;
        TimeSpan relativeDeathTime; // 相対死亡時刻 (ログ表記用)
        TimeSpan estimatedDeathTime; // 死亡推定時刻
        TimeSpan errorTimeSpan = new(0, 0, 0, 0); // 誤差秒数

        relativeDeathTime = estimatedDeathTime = reportTime - actualDeathTime; // 相対死亡時刻の計算 死亡推定時刻にも初期値(基準の値)として代入。

        if (PoliceSurgeon_IncludeErrorInDeathTime.GetBool())
        {
            errorTimeSpan = CalculateErrorTimeSpan(seed);
            estimatedDeathTime += errorTimeSpan;
            estimatedDeathTime = estimatedDeathTime >= TimeSpan.Zero ? estimatedDeathTime : -estimatedDeathTime;
        }

        Logger.Info($"{player.name} : 絶対死亡時刻_{actualDeathTime:hh:mm:ss} 通報時刻_{reportTime:ss}s 相対死亡時刻_{relativeDeathTime:ss}s 死亡推定時刻_{estimatedDeathTime:ss}s前 誤差設定_{PoliceSurgeon_IncludeErrorInDeathTime.GetBool()} 誤差範囲設定_{PoliceSurgeonMarginOfErrorToIncludeInTimeOfDeath.GetFloat()} 誤差秒数_{(errorTimeSpan >= TimeSpan.Zero ? "+" : "-")}{errorTimeSpan:ss}s");
        return int.Parse($"{estimatedDeathTime:ss}");
    }

    /// <summary>
    /// 死亡時刻に含める誤差を求める
    /// </summary>
    /// <param name="seed">乱数のシード値 (死亡時刻のミリ秒)</param>
    /// <param name="errorTimeSpan">含める誤差の絶対値</param>
    /// <param name="isPositive">true : 誤差が正の値 (誤差を含んだ値を求める時、真の値に加算する)</param>
    private static TimeSpan CalculateErrorTimeSpan(int seed)
    {
        int marginOfError = Mathf.Abs((int)PoliceSurgeonMarginOfErrorToIncludeInTimeOfDeath.GetFloat());
        System.Random random = new(seed);
        int error = random.Next(0, marginOfError * 2 + 1) - marginOfError;
        return new TimeSpan(0, 0, 0, error);
    }
}

[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Start))]
/// <summary>
/// 死体検案書の作成及び送信関連のメソッドを集約したクラス
/// </summary>
internal static class PoliceSurgeon_PostMortemCertificate
{
    private const string delimiterLine = "|----------------------------------------------------|";

    /// <summary>
    /// 公的な年月日を文字列として取得する
    /// (日本語に設定している場合は和歴、それ以外は西暦且つアメリカ英語表記で取得)
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
                if (PoliceSurgeonIsUseTaiwanCalendar.GetBool())
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

    /// <summary>
    /// 今日の日付を漢数字表記にする
    /// </summary>
    /// <returns>string : 漢数字に変換された今日の日付</returns>
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
    ///会議開始時 警察医に死体検案書を送信する。
    /// </summary>
    private static void Postfix()
    {
        if (!AmongUsClient.Instance.AmHost) return;
        foreach (var pl in PoliceSurgeonPlayer)
            Patches.AddChatPatch.SendCommand(pl, "", GetPostMortemCertificate(pl));
    }

    /// <summary>
    /// 死体検案書を取得する。
    /// (そのターンの検案書が辞書に格納されていたらそれを取得し、されていなかったら作成して辞書に保存する。)
    /// </summary>
    /// <param name="policeSurgeon">送信先の警察医</param>
    /// <returns>string : 死体検案書 (名前は送信先の警察医名に置き換わっている)</returns>
    private static string GetPostMortemCertificate(PlayerControl policeSurgeon)
    {
        if (!PostMortemCertificate.ContainsKey(MeetingTurn_Now))
            PostMortemCertificate.Add(MeetingTurn_Now, CreatePostMortemCertificate());
        return string.Format(PostMortemCertificate[MeetingTurn_Now], policeSurgeon.name);
    }

    /// <summary>
    /// 死体検案書を作成する。
    /// 警察医名は{0}になっている為、呼び出し元で埋め込む必要がある。
    /// </summary>
    /// <returns>string : 死体検案書 (警察医名が{0}に置き換わっている)</returns>
    private static string CreatePostMortemCertificate()
    {
        bool isWrite = false; // 死体検案書に書く死者の情報があるか
        StringBuilder builder = new();

        builder.AppendLine(delimiterLine);
        builder.AppendLine(ModTranslation.GetString("PostMortemCertificate_main1"));
        builder.AppendLine(delimiterLine);

        foreach (KeyValuePair<byte, (int, int, int)> kvp in PoliceSurgeon_ActualDeathTimes)
        {
            // 以降に進むのは、[全てのターンの死亡情報を出す時]の全てのプレイヤーの情報と　[現在ターンの死亡情報しか出さない時]の現在ターンに死亡したプレイヤーの情報
            if (!PoliceSurgeonIndicateTimeOfDeathInSubsequentTurn.GetBool() && kvp.Value.Item3 != MeetingTurn_Now) continue;

            isWrite = true;

            // ターンを表記する設定が有効か? (全てのターンの死亡情報を出す設定 且つ 死亡ターンを表記する設定 の時に有効)
            var isWritingTurn = PoliceSurgeonIndicateTimeOfDeathInSubsequentTurn.GetBool() && PoliceSurgeonHowManyTurnAgoTheDied.GetBool();
            var inError = PoliceSurgeon_IncludeErrorInDeathTime.GetBool();

            var deadPl = ModHelpers.GetPlayerControl(kvp.Key);
            builder.AppendLine($"{ModTranslation.GetString("PostMortemCertificate_main2")}{deadPl.name}");

            var deadTime = kvp.Value.Item1;
            var deadReason = kvp.Value.Item2;

            switch (deadReason)
            {
                case (int)PoliceSurgeon_AddActualDeathTime.DeadTiming.TaskPhase_killed:
                    if (inError)
                    {
                        if (isWritingTurn)
                            // 死亡したとき {0:年月日} ({1:Value.Item3}ターン前) {2:kvp.Value.Item2}秒前 ({3:推定})
                            builder.AppendLine($"{string.Format(ModTranslation.GetString("PostMortemCertificate_AlreadyKnown_WriteTurn"), OfficialDateNotation, MeetingTurn_Now - kvp.Value.Item3, deadTime, ModTranslation.GetString("PostMortemCertificate_CauseOfDeath1"))}");
                        else// 死亡したとき {0:年月日} {1:kvp.Value.Item2}秒前 ({2:推定})
                            builder.AppendLine($"{string.Format(ModTranslation.GetString("PostMortemCertificate_AlreadyKnown"), OfficialDateNotation, deadTime, ModTranslation.GetString("PostMortemCertificate_CauseOfDeath1"))}");
                    }
                    else
                    {
                        if (isWritingTurn)
                            // 死亡したとき {0:年月日} ({1:Value.Item3}ターン前) {2:kvp.Value.Item2}秒前 ({3:確認})
                            builder.AppendLine($"{string.Format(ModTranslation.GetString("PostMortemCertificate_AlreadyKnown_WriteTurn"), OfficialDateNotation, MeetingTurn_Now - kvp.Value.Item3, deadTime, ModTranslation.GetString("PostMortemCertificate_CauseOfDeath2"))}");
                        else// 死亡したとき {0:年月日} {1:kvp.Value.Item2}秒前 ({2:確認})
                            builder.AppendLine($"{string.Format(ModTranslation.GetString("PostMortemCertificate_AlreadyKnown"), OfficialDateNotation, deadTime, ModTranslation.GetString("PostMortemCertificate_CauseOfDeath2"))}");
                    }
                    builder.AppendLine("");
                    builder.AppendLine($"{ModTranslation.GetString("PostMortemCertificate_DeadReason1")}"); // 死因 不詳の外因死
                    builder.AppendLine("");
                    break;

                case (int)PoliceSurgeon_AddActualDeathTime.DeadTiming.TaskPhase_Exited:
                case (int)PoliceSurgeon_AddActualDeathTime.DeadTiming.MeetingPhase:
                    if (isWritingTurn)
                        // 死亡したとき {0:年月日} ({1:Value.Item3}ターン前) ({2:頃})
                        builder.AppendLine($"{string.Format(ModTranslation.GetString("PostMortemCertificate_Unknown_WriteTurn"), OfficialDateNotation, MeetingTurn_Now - kvp.Value.Item3, ModTranslation.GetString("PostMortemCertificate_CauseOfDeath3"))}");
                    else// 死亡したとき {0:年月日} ({1:頃})
                        builder.AppendLine($"{string.Format(ModTranslation.GetString("PostMortemCertificate_Unknown"), OfficialDateNotation, ModTranslation.GetString("PostMortemCertificate_CauseOfDeath3"))}");
                    builder.AppendLine("");
                    builder.AppendLine($"{ModTranslation.GetString("PostMortemCertificate_DeadReason1")}"); // 死因 不詳の外因死
                    builder.AppendLine("");
                    break;

                case (int)PoliceSurgeon_AddActualDeathTime.DeadTiming.Exited:
                    if (isWritingTurn)
                        // 死亡したとき {0:年月日} ({1:Value.Item3}ターン前) ({2:頃})
                        builder.AppendLine($"{string.Format(ModTranslation.GetString("PostMortemCertificate_Unknown_WriteTurn"), OfficialDateNotation, MeetingTurn_Now - kvp.Value.Item3, ModTranslation.GetString("PostMortemCertificate_CauseOfDeath3"))}");
                    else// 死亡したとき {0:年月日} ({1:頃})
                        builder.AppendLine($"{string.Format(ModTranslation.GetString("PostMortemCertificate_Unknown"), OfficialDateNotation, ModTranslation.GetString("PostMortemCertificate_CauseOfDeath3"))}");
                    builder.AppendLine("");
                    builder.AppendLine($"{ModTranslation.GetString("PostMortemCertificate_DeadReason2")}"); // 死因 追放
                    builder.AppendLine("");
                    break;
            }
            builder.AppendLine(delimiterLine);
        }

        if (isWrite) // 死体検案書に記載する 死者の情報がある時
        {
            builder.AppendLine(ModTranslation.GetString("PostMortemCertificate_main3")); // 上記のとおり<s>診断</s>(検案)する
            builder.AppendLine($"{ModTranslation.GetString("PostMortemCertificate_main4")} {OfficialDateNotation}"); // 本診断書(検案書) 発行年月日
            builder.AppendLine($"{(MapNames)GameManager.Instance.LogicOptions.currentGameOptions.MapId}"); // マップ名
            builder.AppendLine("");
            builder.AppendLine($"{ModTranslation.GetString("PostMortemCertificate_main5")} {{0}}"); // (氏名) 医師
            builder.AppendLine("");
            builder.AppendLine(delimiterLine);
            builder.AppendLine("");
        }
        else // 死者の情報がない時はランダムに警察医の独白を出す
        {
            builder = new();

            System.Random random = new((int)DateTime.Now.Ticks);
            int rand = random.Next(1, 15 + 1);
            string transRandomText = ModTranslation.GetString($"PostMortemCertificate_NoDeaths_RandomMessage_{rand}");

            builder.AppendLine($"{string.Format(ModTranslation.GetString("PostMortemCertificate_NoDeaths"), OfficialDateNotation, "{0}", transRandomText)}");
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
}