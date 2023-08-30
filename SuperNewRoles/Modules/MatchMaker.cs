using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using AmongUs.GameOptions;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using SuperNewRoles.Mode;
using UnityEngine;
using static SuperNewRoles.Modules.CustomOption;
using static SuperNewRoles.Modules.CustomOptionHolder;

namespace SuperNewRoles.Modules;

public static class MatchMaker
{
    public static string BaseURL = "https://supermatchmaker.vercel.app/";
    public static Dictionary<string, string> CreateBaseData()
    {
        var data = new Dictionary<string, string>
        {
            ["friendcode"] = PlayerControl.LocalPlayer.Data.FriendCode,
            ["roomid"] = InnerNet.GameCode.IntToGameName(AmongUsClient.Instance.GameId)
        };
        return data;
    }
    public static void EndInviting()
    {
        var data = CreateBaseData();
        data["type"] = "endinvite";
        AmongUsClient.Instance.StartCoroutine(Analytics.Post(BaseURL + "api/update_state", data.GetString()).WrapToIl2Cpp());
    }
    public static void KeepAlive()
    {
        var data = CreateBaseData();
        data["type"] = "keepalive";
        AmongUsClient.Instance.StartCoroutine(Analytics.Post(BaseURL + "api/update_state", data.GetString()).WrapToIl2Cpp());
    }
    public static void UpdatePlayerCount(bool Is = false)
    {
        var data = CreateBaseData();
        data["type"] = "updateplayer";
        data["MaxPlayer"] = GameOptionsManager.Instance.CurrentGameOptions.MaxPlayers.ToString();
        data["NowPlayer"] = ((Is ? 1 : 0) + GameData.Instance.PlayerCount).ToString();
        AmongUsClient.Instance.StartCoroutine(Analytics.Post(BaseURL + "api/update_state", data.GetString()).WrapToIl2Cpp());
    }
    public static void UpdateOption()
    {
        var data = CreateBaseData();
        data["type"] = "updateoption";
        string ActiveRole = "";
        List<string> ActivateRoles = new();
        foreach (CustomRoleOption opt in CustomRoleOption.RoleOptions)
        {
            if (opt.GetSelection() == 0) continue;
            if (opt.IsHidden()) continue;
            CustomOption countopt = options.FirstOrDefault(x => x.id == (opt.id + 1));
            for (int i = 0; i < (countopt.GetSelection() + 1); i++)
            {
                ActivateRoles.Add(opt.RoleId.ToString());
            }
        }
        string ActiveOptions = "";
        List<string> ActivateOptions = new();
        foreach (CustomOption option in options)
        {
            bool enabled = true;
            if (option.type == CustomOptionType.MatchTag) continue;
            if (AmongUsClient.Instance?.AmHost == false && hideSettings.GetBool())
            {
                enabled = false;
            }

            if (option.IsHidden())
            {
                enabled = false;
            }
            CustomOption parent = option.parent;

            while (parent != null && enabled)
            {
                enabled = parent.Enabled;
                parent = parent.parent;
            }
            if (enabled)
            {
                ActivateOptions.Add(option.id + ":" + option.GetSelection());
            }
        }
        data["roles"] = string.Join(',', ActivateRoles);
        data["options"] = string.Join(',', ActivateOptions);
        data["mode"] = GameOptionsManager.Instance.currentGameMode == GameModes.HideNSeek ? "HNS" : ModeHandler.GetMode(false).ToString();
        AmongUsClient.Instance.StartCoroutine(Analytics.Post(BaseURL + "api/update_state", data.GetString()).WrapToIl2Cpp());
    }
    internal static void UpdateTags()
    {
        var data = CreateBaseData();
        data["type"] = "updatetags";
        data["updatetags"] = GetTagData();
        AmongUsClient.Instance.StartCoroutine(Analytics.Post(BaseURL + "api/update_state", data.GetString()).WrapToIl2Cpp());
    }
    private static string GetTagData()
    {
        List<string> ActiveTags = new();
        foreach (CustomOption option in options)
        {
            if (option.GetSelection() == 0) continue;
            if (option.IsHidden()) continue;
            if (option.type != CustomOptionType.MatchTag) continue;

            bool enabled = true;

            if (AmongUsClient.Instance?.AmHost == false && hideSettings.GetBool())
                enabled = false;

            if (enabled)
            {
                // 先にカラータグを外す
                string tagName = option.name;
                string pattern = @"<color=#\w+>|</color>";

                Regex colorRegex = new(pattern);
                tagName = colorRegex.Replace(tagName, "");

                string tagKey = ModTranslation.GetTranslateKey(tagName);

                ActiveTags.Add($"{tagKey}");
                Logger.Info($"タグ情報 : {tagName}({option.id}) を送信します。");
            }
        }
        string tagData = string.Join(',', ActiveTags);
        return tagData;
    }
    public static void CreateRoom()
    {
        var data = CreateBaseData();
        string ActiveRole = "";
        List<string> ActivateRoles = new();
        foreach (CustomRoleOption opt in CustomRoleOption.RoleOptions)
        {
            if (opt.GetSelection() == 0) continue;
            if (opt.IsHidden()) continue;
            if (opt.type == CustomOptionType.MatchTag) continue;
            ActivateRoles.Add(opt.RoleId.ToString());
        }
        string ActiveOptions = "";
        List<string> ActivateOptions = new();
        foreach (CustomOption option in CustomOption.options)
        {
            bool enabled = true;
            if (AmongUsClient.Instance?.AmHost == false && CustomOptionHolder.hideSettings.GetBool())
            {
                enabled = false;
            }

            if (option.IsHidden())
            {
                enabled = false;
            }
            CustomOption parent = option.parent;

            while (parent != null && enabled)
            {
                enabled = parent.Enabled;
                parent = parent.parent;
            }
            if (enabled)
            {
                ActivateOptions.Add(option.id + ":" + option.GetSelection());
            }
        }
        data["Roles"] = string.Join(',', ActivateRoles);
        data["Options"] = string.Join(',', ActivateOptions);
        data["Mode"] = GameOptionsManager.Instance.currentGameMode == GameModes.HideNSeek ? "HNS" : ModeHandler.GetMode(false).ToString();
        data["NowPlayer"] = GameData.Instance.PlayerCount.ToString();
        data["MaxPlayer"] = GameOptionsManager.Instance.CurrentGameOptions.MaxPlayers.ToString();
        data["Version"] = SuperNewRolesPlugin.VersionString;
        data["updatetags"] = GetTagData();
        string server = "NoneServer";
        StringNames n = FastDestroyableSingleton<ServerManager>.Instance.CurrentRegion.TranslateName;
        switch (n)
        {
            case StringNames.ServerAS:
                server = "0";
                break;
            case StringNames.ServerNA:
                server = "1";
                break;
            case StringNames.ServerEU:
                server = "2";
                break;
        }
        data["Server"] = server;
        AmongUsClient.Instance.StartCoroutine(Analytics.Post(BaseURL + "api/create_room", data.GetString()).WrapToIl2Cpp());
    }
}

public static class MatchTagOption
{
    // 村レベル : 600000 ~
    public static CustomOption BeginnerTag; // 初心者
    public static CustomOption IntermediateTag; // 中級
    public static CustomOption AdvancedTag; // 上級者
    public static CustomOption CompetenceIsNotRequiredTag; // 実力不問

    // プレイスタンス : 600100 ~
    public static CustomOption SeriousTag; // ガチ勢
    public static CustomOption EnjoyTag;  // エンジョイ勢

    // プレイスタイル : 600200 ~
    public static CustomOption SeriousnessTag; // 真剣プレイ
    public static CustomOption WelcomeBeginnerTag; // 初心者歓迎

    // 状態 600300
    public static CustomOption NowRecordingTag; // 撮影中
    public static CustomOption NowBeingDeliveredTag; // 配信中
    public static CustomOption OKForRecordingTag; // 撮影OK
    public static CustomOption OKForBeingDeliveredTag; // 配信OK
    public static CustomOption NoRecordingTag; // 撮影NG
    public static CustomOption NoBeingDeliveredTag; // 配信NG


    // 会議方法 : 600400 ~
    public static CustomOption FreeChatTag; // フリーチャット
    public static CustomOption QuickChatTag; // クイックチャット
    public static CustomOption VoiceChatTag; // VC
    public static CustomOption BetterCrewLinkTag; // 近アモ
    public static CustomOption CanListenOnlyTag; // 聞き専可
    public static CustomOption FullVCOnlyTag; // 聞き専不可

    // レギュレーション : 600500 ~
    public static CustomOption SheriffAndMadRegulationTag; // シェリマ
    public static CustomOption NeutralKillerRegulationTag; // 第三キル人外入り
    public static CustomOption VillageForOutsidersRegulationTag; // 人外村
    public static CustomOption ManyRolesRegulationTag; // 多役
    public static CustomOption WerewolfMoonlightRegulationTag; // 月下
    public static CustomOption DarkPotRegulationTag; // 闇鍋
    public static CustomOption OthersRegulationTag; // その他レギュ
    public static CustomOption TryingOutRolesRegulationTag; // 役職お試し中(・ω・　)
    public static CustomOption AmusementRegulationTag; // お遊びレギュ
    public static CustomOption RegulationAdjustedTag; // レギュ調整中

    // 使用機能 : 600600 ~
    public static CustomOption FeatureAdminLimitTag;
    public static CustomOption FeatureCanNotUseAdminTag;

    // デバック : 600700 ~
    public static CustomOption DebugNewRolesTag; // 新役職
    public static CustomOption DebugNewFeaturesTag; // 新機能
    public static CustomOption DebugAddFeaturesTag; // 機能追加
    public static CustomOption DebugChangeTag; // 仕様変更
    public static CustomOption DebugBugFixTag; // バグ修正
    public static CustomOption DebugOthersTag; // その他(デバック)

    public static void LoadOption()
    {
        // 村レベル : 600000 ~
        Color villageLvColor = new(255f / 255f, 255f / 255f, 255f / 255f, 1);
        BeginnerTag = CreateMatchMakeTag(600000, true, Cs(new Color(134f / 255f, 214f / 255f, 31f / 255f, 1), "BeginnerTag"), false, null, isHeader: true); // 初心者
        IntermediateTag = CreateMatchMakeTag(600001, true, Cs(new Color(12f / 255f, 184f / 255f, 232f / 255f, 1), "IntermediateTag"), false, null); // 中級者
        AdvancedTag = CreateMatchMakeTag(600002, true, Cs(new Color(167f / 255f, 139f / 255f, 92f / 255f, 1), "AdvancedTag"), false, null); // 上級者
        CompetenceIsNotRequiredTag = CreateMatchMakeTag(600003, true, Cs(new Color(105f / 255f, 179f / 255f, 119f / 255f, 1), "CompetenceIsNotRequiredTag"), false, null); // 実力不問

        // プレイスタンス : 600100 ~
        Color playingStanceColor = new(255f / 255f, 255f / 255f, 255f / 255f, 1);
        SeriousTag = CreateMatchMakeTag(600100, true, Cs(playingStanceColor, "SeriousTag"), false, null, isHeader: true); // ガチ勢
        EnjoyTag = CreateMatchMakeTag(600101, true, Cs(playingStanceColor, "EnjoyTag"), false, null);  // エンジョイ勢

        // プレイスタイル : 600200 ~
        Color playingStyleColor = new(255f / 255f, 255f / 255f, 255f / 255f, 1);
        SeriousnessTag = CreateMatchMakeTag(600200, true, Cs(playingStyleColor, "SeriousnessTag"), false, null, isHeader: true); // 真剣プレイ
        WelcomeBeginnerTag = CreateMatchMakeTag(600201, true, Cs(playingStyleColor, "WelcomeBeginnerTag"), false, null); // 初心者歓迎

        // 状態 600300
        Color conditionColor = new(255f / 255f, 255f / 255f, 255f / 255f, 1);
        NowRecordingTag = CreateMatchMakeTag(600300, true, Cs(conditionColor, "NowRecordingTag"), false, null, isHeader: true); // 撮影中
        NowBeingDeliveredTag = CreateMatchMakeTag(600301, true, Cs(conditionColor, "NowBeingDeliveredTag"), false, null); // 配信中
        OKForRecordingTag = CreateMatchMakeTag(600302, true, Cs(conditionColor, "OKForRecordingTag"), false, null); // 撮影OK
        OKForBeingDeliveredTag = CreateMatchMakeTag(600303, true, Cs(conditionColor, "OKForBeingDeliveredTag"), false, null); // 配信OK
        NoRecordingTag = CreateMatchMakeTag(600304, true, Cs(conditionColor, "NoRecordingTag"), false, null); // 撮影NG
        NoBeingDeliveredTag = CreateMatchMakeTag(600305, true, Cs(conditionColor, "NoBeingDeliveredTag"), false, null); // 配信NG

        // 会議方法 : 600400 ~
        Color meetingSystemColor = new(255f / 255f, 255f / 255f, 255f / 255f, 1);
        FreeChatTag = CreateMatchMakeTag(600400, true, Cs(meetingSystemColor, "FreeChatTag"), false, null, isHeader: true); // フリーチャット
        QuickChatTag = CreateMatchMakeTag(600401, true, Cs(meetingSystemColor, "QuickChatTag"), false, null); // クイックチャット
        VoiceChatTag = CreateMatchMakeTag(600402, true, Cs(meetingSystemColor, "VoiceChatTag"), false, null); // VC
        BetterCrewLinkTag = CreateMatchMakeTag(600403, true, Cs(meetingSystemColor, "BetterCrewLinkTag"), false, null); // 近アモ
        CanListenOnlyTag = CreateMatchMakeTag(600404, true, Cs(meetingSystemColor, "CanListenOnlyTag"), false, null); // 聞き専可
        FullVCOnlyTag = CreateMatchMakeTag(600405, true, Cs(meetingSystemColor, "FullVCOnlyTag"), false, null); // 聞き専不可

        // レギュレーション : 600500 ~
        Color RegulationColor = new(255f / 255f, 255f / 255f, 255f / 255f, 1);
        SheriffAndMadRegulationTag = CreateMatchMakeTag(600501, true, Cs(RegulationColor, "SheriffAndMadRegulationTag"), false, null, isHeader: true); // シェリマ
        NeutralKillerRegulationTag = CreateMatchMakeTag(600502, true, Cs(RegulationColor, "NeutralKillerRegulationTag"), false, null); // 第三キル人外入り
        ManyRolesRegulationTag = CreateMatchMakeTag(600503, true, Cs(RegulationColor, "ManyRolesRegulationTag"), false, null); // 多役
        WerewolfMoonlightRegulationTag = CreateMatchMakeTag(600504, true, Cs(RegulationColor, "WerewolfMoonlightRegulationTag"), false, null); // 月下
        VillageForOutsidersRegulationTag = CreateMatchMakeTag(600505, true, Cs(RegulationColor, "VillageForOutsidersRegulationTag"), false, null); // 人外村
        DarkPotRegulationTag = CreateMatchMakeTag(600506, true, Cs(RegulationColor, "DarkPotRegulationTag"), false, null); // 闇鍋
        OthersRegulationTag = CreateMatchMakeTag(600507, true, Cs(RegulationColor, "OthersRegulationTag"), false, null); // その他
        TryingOutRolesRegulationTag = CreateMatchMakeTag(600508, true, Cs(RegulationColor, "TryingOutRolesRegulationTag"), false, null); // 役職お試し中(・ω・　)
        AmusementRegulationTag = CreateMatchMakeTag(600509, true, Cs(RegulationColor, "AmusementRegulationTag"), false, null); // お遊びレギュ
        RegulationAdjustedTag = CreateMatchMakeTag(600510, true, Cs(RegulationColor, "RegulationAdjustedTag"), false, null); // レギュ調整中

        // 使用機能 : 600600 ~
        Color useFeatureColor = new(255f / 255f, 255f / 255f, 255f / 255f, 1);
        FeatureAdminLimitTag = CreateMatchMakeTag(600600, false, Cs(useFeatureColor, "FeatureAdminLimitTag"), false, null, isHeader: true); // アドミン使用制限
        FeatureCanNotUseAdminTag = CreateMatchMakeTag(600601, true, Cs(useFeatureColor, "FeatureCanNotUseAdminTag"), false, null, isHeader: true); // アドミン禁止

        // デバッグ : 600600 ~
        Color debugColor = (Color)Roles.RoleClass.Debugger.color;
        bool notDebugMode = !ConfigRoles.DebugMode.Value;
        DebugNewRolesTag = CreateMatchMakeTag(600700, true, Cs(debugColor, "DebugNewRolesTag"), false, null, isHeader: true, isHidden: notDebugMode);
        DebugNewFeaturesTag = CreateMatchMakeTag(600701, true, Cs(debugColor, "DebugNewFeaturesTag"), false, null, isHidden: notDebugMode);
        DebugAddFeaturesTag = CreateMatchMakeTag(600702, true, Cs(debugColor, "DebugAddFeaturesTag"), false, null, isHidden: notDebugMode);
        DebugChangeTag = CreateMatchMakeTag(600703, true, Cs(debugColor, "DebugChangeTag"), false, null, isHidden: notDebugMode);
        DebugBugFixTag = CreateMatchMakeTag(600704, true, Cs(debugColor, "DebugBugFixTag"), false, null, isHidden: notDebugMode);
        DebugOthersTag = CreateMatchMakeTag(600705, true, Cs(debugColor, "DebugOthersTag"), false, null, isHidden: notDebugMode);

        if (notDebugMode)
        {
            DebugNewRolesTag.selection = 0;
            DebugNewFeaturesTag.selection = 0;
            DebugAddFeaturesTag.selection = 0;
            DebugChangeTag.selection = 0;
            DebugBugFixTag.selection = 0;
            DebugOthersTag.selection = 0;
        }

        // SHRでは表示しない設定を内部的にもオフにする
        try
        {
            if (ModeHandler.IsMode(ModeId.SuperHostRoles))
            {
                FeatureCanNotUseAdminTag.selection = 0;
            }
        }
        catch { }
    }
}