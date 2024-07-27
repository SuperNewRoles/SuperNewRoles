using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using SuperNewRoles.Mode;
using SuperNewRoles.Mode.BattleRoyal.BattleRole;
using SuperNewRoles.Mode.SuperHostRoles;
using SuperNewRoles.Roles;
using SuperNewRoles.Roles.RoleBases;
using SuperNewRoles.SuperNewRolesWeb;
using UnityEngine;
using UnityEngine.Networking;
using static System.String;
using static SuperNewRoles.Patches.AddChatPatch;
using static UnityEngine.GraphicsBuffer;

namespace SuperNewRoles.Patches;

internal static class HostManagedChatCommandPatch
{
    internal enum CommandType
    {
        None,
        Welcome,
        CommandList,
        Version,
        ThisModisSNR,
        AllRoles,
        GetInRoles,
        RoleInfo,
        GetSetting,
        Winners,
        MatchMakingTags,
        BattleRoles,
        DiscordLink,
        TwitterLink,
        GenerateCode,
        WebInfo,
    }

    /// <summary>
    /// チャットで送信された文字列を小文字で判定し, 該当するコマンドを取得する
    /// </summary>
    /// <param name="command">チャットで送信された単語(' 'で区切った先頭のみ)</param>
    /// <returns>該当したコマンドの種類</returns>
    internal static CommandType CheckChatCommand(string command)
        => command.ToLower() switch
        {
            "/welcome" or "/wlc" => CommandType.Welcome,
            "/commands" or "/cmd" => CommandType.CommandList,
            "/version" or "/v" => CommandType.Version,
            "/now" or "/n" or "/help" or "/h" => CommandType.ThisModisSNR,
            "/allroles" or "/ar" => CommandType.AllRoles,
            "/getinroles" or "/gr" => CommandType.GetInRoles,
            "/roleinfo" or "/ri" => CommandType.RoleInfo,
            "/getsettings" or "/gs" => CommandType.GetSetting,
            "/winners" or "/w" => CommandType.Winners,
            "/matchtag" or "/tag" => CommandType.MatchMakingTags,
            "/battleroles" or "/btr" => CommandType.BattleRoles,
            "/discord" or "/dc" => CommandType.DiscordLink,
            "/Twitter" or "/tw" => CommandType.TwitterLink,
            "/generatecode" or "/gc" => CommandType.GenerateCode,
            "/getwebinfo" or "/gwi" => CommandType.WebInfo,
            _ => CommandType.None,
        };

    /// <summary>
    /// コマンドの処理を実行する
    /// </summary>
    /// <param name="commandUser">コマンド使用者</param>
    /// <param name="type">使用されたコマンドの種類</param>
    /// <param name="Commands">実行するコマンド</param>
    internal static void ChatCommandExecution(PlayerControl commandUser, CommandType type, string[] Commands)
    {
        switch (type)
        {
            case CommandType.Welcome:
                SendCommand(commandUser.AmOwner ? null : commandUser, GetChatCommands.GetWelcomeMessage(), GetChatCommands.WelcomeToSuperNewRoles);
                break;
            case CommandType.CommandList:
                SendCommand(commandUser, GetChatCommands.GetChatCommandList());
                break;
            case CommandType.Version:
                SendCommand(commandUser.AmOwner ? null : commandUser, GetChatCommands.GetVersionMessage());
                break;
            case CommandType.ThisModisSNR:
                SendCommand(commandUser, "ここはTOH部屋ではなくSNR部屋です。\n/nや/hは使えません。\n/arや/grを使用してください。\n詳細は、/cmdをご覧ください！");//, $" {SuperNewRolesPlugin.ModName} v{SuperNewRolesPlugin.VersionString}\nCreate by ykundesu{betatext}");
                SendCommand(null, "みなさん、ここはTOH部屋ではなく、SNR部屋です！\n/nや/hは使えないので、/arや/grを使用してください。\n詳細は、/cmdをご覧ください！");//, $" {SuperNewRolesPlugin.ModName} v{SuperNewRolesPlugin.VersionString}\nCreate by ykundesu{betatext}");
                break;
            case CommandType.AllRoles:
                GetChatCommands.ProcessAllRoles(commandUser, Commands);
                break;
            case CommandType.GetInRoles:
                GetChatCommands.ProcessGetInRoles(commandUser, Commands);
                break;
            case CommandType.RoleInfo:
                GetChatCommands.ProcessRoleInfo(commandUser, Commands);
                break;
            case CommandType.GetSetting:
                PlayerControl sendTarget = commandUser.AmOwner ? null : commandUser;
                if (CustomOptionHolder.hideSettings.GetBool()) // 設定が隠されてるなら表示しない
                {
                    SendCommand(sendTarget, ModTranslation.GetString("HideSettingsMessage"));
                }
                else
                {
                    SendCommand(sendTarget, GetChatCommands.GetSettingDescription());
                }
                break;
            case CommandType.Winners:
                if (OnGameEndPatch.PlayerData != null)
                {
                    SendCommand(commandUser.AmOwner ? null : commandUser, GetChatCommands.GetWinnerMessage(), $"<size=200%>{OnGameEndPatch.WinText}</size>");
                }
                else
                {
                    SendCommand(commandUser.AmOwner ? null : commandUser, ModTranslation.GetString("WinnersNoneData"), GetChatCommands.SNRCommander);
                }
                break;
            case CommandType.MatchMakingTags:
                SendCommand(commandUser.AmOwner ? null : commandUser, GetChatCommands.GetMatchMakingTag());
                break;
            case CommandType.BattleRoles:
                (string resultReplyText, string name) = GetChatCommands.GetBattleRoles(Commands);
                SendCommand(commandUser, resultReplyText, name);
                break;
            case CommandType.DiscordLink:
                SendCommand(commandUser, $"{ModTranslation.GetString("SNROfficialDiscordMessage")}\n{SuperNewRolesPlugin.DiscordServer}");
                break;
            case CommandType.TwitterLink:
                SendCommand(commandUser, $"{ModTranslation.GetString("SNROfficialTwitterMessage")}\n\n{ModTranslation.GetString("TwitterOfficialLink")}\n{ModTranslation.GetString("TwitterDevLink")}");
                break;
            case CommandType.GenerateCode:
                if (commandUser == null) return;
                GetChatCommands.CreateGenerateCode(commandUser);
                break;
            case CommandType.WebInfo:
                SendCommand(commandUser, GetChatCommands.GetWebInfo(), $"<size={(AprilFoolsManager.IsApril(2024) ? "120%" : "150%")}>About {SuperNewRolesPlugin.ColorModName}Web</size>");
                break;
        }
    }
}

/// <summary>
/// コマンドの処理を行うメソッドを置くクラス
/// </summary>
internal static class GetChatCommands
{
    internal static string SNRCommander => $"<size=200%>{SuperNewRolesPlugin.ColorModName}</size>";
    internal static string WelcomeToSuperNewRoles => $"<size={(AprilFoolsManager.IsApril(2024) ? "120%" : "150%")}>Welcome To {SuperNewRolesPlugin.ColorModName}</size>";

    internal static string GetWelcomeMessage()
    {
        string welcomeMessage;

        const string startText = $"<align={"left"}><size=80%>";
        const string endText = " " + "\n<size=0%>.</size>" + "</size></align>";

        string mainText;
        string webWarningText = null;

        mainText =
            $"{ModTranslation.GetString("WelcomeMessage1")}\n\n" +
            $"{ModTranslation.GetString("WelcomeMessage2")}\n" +
            $"<color=#FF4B00>{ModTranslation.GetString("WelcomeMessage3")}</color>\n" +
            $"{ModTranslation.GetString("WelcomeMessage4")}\n\n" +
            $"{ModTranslation.GetString("WelcomeMessage5")}\n" +
            $"{ModTranslation.GetString("WelcomeMessage6")}\n" +
            $"{ModTranslation.GetString("WelcomeMessage7")}\n" +
            $"{ModTranslation.GetString("WelcomeMessage8")}\n\n" +
            $"{ModTranslation.GetString("WelcomeMessage9")}\n" +
            $"{ModTranslation.GetString("WelcomeMessage10")}\n";

        if (WebAccountManager.IsLogined || CustomOptionHolder.SNRWebSendConditionHostDependency.GetBool())
        {
            string SNRWebWelcomMessage1 = WebAccountManager.IsLogined ? "SNRWebWelcomMessage1_HostSend" : "SNRWebWelcomMessage1_GestSend";

            webWarningText =
                "\n<color=#4d4398>|-----------------------------------------------------------------------------|</color>\n\n" +
                $"<color=#FF4B00>{ModTranslation.GetString(SNRWebWelcomMessage1)}</color>\n" +
                $"{ModTranslation.GetString("SNRWebWelcomMessage2")}\n" +
                $"{ModTranslation.GetString("SNRWebWelcomMessage3")}\n" +
                $"{ModTranslation.GetString("SNRWebWelcomMessage4")}\n";
        }

        welcomeMessage = startText + mainText + webWarningText + endText;

        return welcomeMessage;
    }

    internal static string GetChatCommandList()
    {
        const string startTag = $"<align={"left"}><size=60%>";

        string startText =
            $"{ModTranslation.GetString("CommandsMessage0")}\n\n";

        string modInfoText =
            $"<size=100%><b>{ModTranslation.GetString("CommandsTitelModInfo")}</b></size>\n" +
            $"{ModTranslation.GetString("CommandsMessageModInfo1")}\n\n" +
            $"{ModTranslation.GetString("CommandsMessageModInfo2")}\n\n" +
            $"{ModTranslation.GetString("CommandsMessageModInfo3")}\n\n" +
            $"{ModTranslation.GetString("CommandsMessageModInfo4")}\n\n";

        string externalInfoText =
            $"<size=100%><b>{ModTranslation.GetString("CommandsTitelExternalInfo")}</b></size>\n" +
            $"{ModTranslation.GetString("CommandsMessageExternalInfo1")}\n\n" +
            $"{ModTranslation.GetString("CommandsMessageExternalInfo2")}\n\n";

        string roleInfoText =
            $"<size=100%><b>{ModTranslation.GetString("CommandsTitelRoleInfo")}</b></size>\n" +
            $"{ModTranslation.GetString("CommandsMessageRoleInfo1")}\n\n" +
            $"{ModTranslation.GetString("CommandsMessageRoleInfo2")}\n\n" +
            $"{ModTranslation.GetString("CommandsMessageRoleInfo3")}\n\n" +
            $"{ModTranslation.GetString("CommandsMessageRoleInfo4")}\n\n" +
            $"{ModTranslation.GetString("CommandsMessageRoleInfo5")}\n\n" +
            $"{ModTranslation.GetString("CommandsMessageRoleInfo6")}\n\n" +
            $"{ModTranslation.GetString("CommandsMessageRoleInfo7")}\n\n";


        string OthersText =
            $"<size=100%><b>{ModTranslation.GetString("CommandsTitelOthers")}</b></size>\n" +
            $"{ModTranslation.GetString("CommandsMessageOthers1")}\n\n" +
            $"{ModTranslation.GetString("CommandsMessageOthers2")}\n\n" +
            $"{ModTranslation.GetString("CommandsMessageOthers3")}\n\n";

        const string endTag = "</size></align>";
        const string line = "<color={0}><size=80%>|-----------------------------------------------------------------------------|</size></color>\n";

        string commandList =
            startTag +
            line +
            startText +
            line +
            modInfoText +
            line +
            externalInfoText +
            line +
            roleInfoText +
            line +
            OthersText +
            line +
            endTag;

        const string commandColor = "#4d4398";
        const string commandSize = "80%";

        return Format(commandList, commandColor, commandSize);
    }

    internal static string GetVersionMessage()
    {
        string VersionText = $" {SuperNewRolesPlugin.ModName} v{SuperNewRolesPlugin.VersionString}\nCreate by TeamSuperNewRoles";

        if (SuperNewRolesPlugin.IsBeta)
        {
            string betaText = $"{ModTranslation.GetString("betatext1")}{ModTranslation.GetString("betatext2")}\nBranch: {{0}}\nCommitId: {ThisAssembly.Git.Commit}";
            string branchText;
            if (SuperNewRolesPlugin.IsSecretBranch)
            {
                for (int i = 0; i < ThisAssembly.Git.Branch.Length; i++)
                {
                    branchText += "*";
                }
            }
            else
            {
                branchText = ThisAssembly.Git.Branch;
            }

            VersionText += Format(betaText, branchText);
        }

        return VersionText;
    }

    internal static string GetWebInfo()
    {
        const string line = "<color=#4d4398>|----------------------------------------------------------------------------------------|</color>\n";
        const string startText = $"<align={"left"}><size=70%>";
        const string endText = " \n.</size></align>";
        const string titelText = "<size={0}><b>[{1}]</b>\n</size>";

        string webInfoText =
            startText +
            Format(titelText, "100%", ModTranslation.GetString("GetWebInfo_01")) +
            $"{ModTranslation.GetString("GetWebInfo_02")}\n\n" +
            line +
            Format(titelText, "100%", ModTranslation.GetString("GetWebInfo_03")) +
            Format(titelText, "90%", ModTranslation.GetString("GetWebInfo_04")) +
            $"{(WebAccountManager.IsLogined ? ModTranslation.GetString("GetWebInfo_05_Host") : ModTranslation.GetString("GetWebInfo_05_Guest"))}{ModTranslation.GetString("GetWebInfo_05_Main")}\n" +
            $"{ModTranslation.GetString("GetWebInfo_06")}\n\n" +
            Format(titelText, "90%", ModTranslation.GetString("GetWebInfo_07")) +
            $"{ModTranslation.GetString("GetWebInfo_08")}\n" +
            $"{ModTranslation.GetString("GetWebInfo_09")}\n\n" +
            $"<color=#FF4B00>{ModTranslation.GetString("GetWebInfo_10")}</color>\n\n" +
            line +
            Format(titelText, "100%", ModTranslation.GetString("GetWebInfo_11")) +
            Format(titelText, "90%", ModTranslation.GetString("GetWebInfo_12")) +
            $"{ModTranslation.GetString("GetWebInfo_13")}\n" +
            $"[ {WebConstants.WebUrl}docs/terms ]\n\n" +
            Format(titelText, "90%", ModTranslation.GetString("GetWebInfo_15")) +
            $"{ModTranslation.GetString("GetWebInfo_13")}\n" +
            $"[ {WebConstants.WebUrl}docs/privacy ]\n\n" +
            endText;

        return webInfoText;
    }

    internal static void ProcessAllRoles(PlayerControl commandUser, string[] Commands)
    {
        PlayerControl sendPlayer = commandUser.AmOwner ? null : commandUser;

        if (Commands.Length == 1)
        {
            Logger.Info("Length==1", "/ar");
            RoleinformationText.RoleCommand(sendPlayer);
        }
        else
        {
            Logger.Info("Length!=1", "/ar");
            if (Commands.Length >= 3 && (Commands[2].Equals("mp", StringComparison.OrdinalIgnoreCase) || Commands[2].Equals("myplayer", StringComparison.OrdinalIgnoreCase) || Commands[2].Equals("myp", StringComparison.OrdinalIgnoreCase)))
            {
                sendPlayer = commandUser;
            }
            if (!float.TryParse(Commands[1], out float sendtime))
            {
                Logger.Info("送信間隔の取得に失敗した為, コマンドが実行できませんでした。", "/ar");
                return;
            }
            RoleinformationText.RoleCommand(SendTime: sendtime, target: sendPlayer);
        }
    }
    internal static void ProcessGetInRoles(PlayerControl commandUser, string[] Commands)
    {
        PlayerControl sendPlayer = commandUser.AmOwner ? null : commandUser;

        if (Commands.Length == 1)
        {
            RoleinformationText.GetInRoleCommand(sendPlayer);
        }
        else
        {
            if (Commands.Length >= 2 && (Commands[1].Equals("mp", StringComparison.OrdinalIgnoreCase) || Commands[1].Equals("myplayer", StringComparison.OrdinalIgnoreCase) || Commands[1].Equals("myp", StringComparison.OrdinalIgnoreCase)))
            {
                sendPlayer = commandUser;
            }
            RoleinformationText.GetInRoleCommand(sendPlayer);
        }
    }
    internal static void ProcessRoleInfo(PlayerControl commandUser, string[] Commands)
    {
        if (Commands.Length == 1)
            SendCommand(commandUser, ModTranslation.GetString("RoleInfoDescription"));
        else if (Commands.Length == 2)
            RoleinformationText.RoleInfoSendCommand(commandUser, Commands[1]);
        else
        {
            string roleName = "";
            for (int i = 2; i <= Commands.Length; i++) { roleName += Commands[i - 1] + " "; }

            RoleinformationText.RoleInfoSendCommand(commandUser, roleName.TrimEnd());
        }
    }

    /// <summary>
    /// Generic設定とModifier設定をチャットに適応する形に文章を加工して取得
    /// </summary>
    /// <returns>Generic設定とModifier設定</returns>
    internal static string GetSettingDescription()
    {
        StringBuilder settingBuilder = new();
        const string line = "\n<color=#4d4398><size=80%>|-----------------------------------------------------------------------------|</size></color>\n";

        foreach (CustomOption option in CustomOption.options.AsSpan())
        {
            if (!(option.type == CustomOptionType.Generic || option.type == CustomOptionType.Modifier)) continue;
            if ((option == CustomOptionHolder.presetSelection) ||
                (option == CustomOptionHolder.crewmateRolesCountMax) ||
                (option == CustomOptionHolder.crewmateGhostRolesCountMax) ||
                (option == CustomOptionHolder.neutralRolesCountMax) ||
                (option == CustomOptionHolder.neutralGhostRolesCountMax) ||
                (option == CustomOptionHolder.impostorRolesCountMax) ||
                (option == CustomOptionHolder.impostorGhostRolesCountMax) ||
                (option == CustomOptionHolder.hideSettings) ||
                (option == CustomOptionHolder.specialOptions))
            {
                continue;
            }

            if (option.parent == null || option == ModeHandler.ModeSetting) // ModeSetting は Mode の子であり parent が nullでない。ModeSetting を 処理したい時は親が無効になっていて処理が行えない為, 単独で判定している。
            {
                if ((!option.Enabled) || (ModeHandler.IsMode(ModeId.SuperHostRoles, false) && !option.isSHROn)) continue;

                StringBuilder editing = new();
                string optionStr;

                if (!(option == ModeHandler.Mode && ModeHandler.IsMode(ModeId.Default, false)))
                {
                    optionStr = GameOptionsDataPatch.OptionToString(option);
                    editing.AppendLine($"<size=80%>{optionStr}</size>");

                    addChildren(option, ref editing, ModeHandler.GetMode(false), !GameOptionsMenuUpdatePatch.IsHidden(option, ModeHandler.GetMode(false)));
                }
                else // mode は 通常の方法で設定の文章を取得できない為, 個別で編集。 通常モード時出ない時は mode でなく ModeSetting で設定の文章を取得
                {
                    optionStr = $"{CustomOptionHolder.Cs(new Color(252f / 187f, 200f / 255f, 0, 1f), "ModeSetting")}:{ModTranslation.GetString("optionOff")}";
                    editing.AppendLine($"<size=80%>{optionStr}</size>");

                    // mode が off なら子設定が必要ない為, addChildren を呼ばない。
                }

                if (editing.ToString().Trim('\n', '\r') is not "\r" and not "")
                {
                    editing.Append(line);
                    settingBuilder.AppendLine(editing.ToString());
                }
            }
        }

        string startText = $"<size=125%>{ModTranslation.GetString("SettingSuperNewRolesVerGetSettingCommand")}</size>{line}";
        string setting = $"<align={"left"}>{startText}<size=70%>{settingBuilder}</size></align>";

        return setting;

        void addChildren(CustomOption option, ref StringBuilder entry, ModeId modeId, bool indent = true)
        {
            if (!option.Enabled || (modeId == ModeId.SuperHostRoles && !option.isSHROn)) return;

            foreach (var child in option.children.AsSpan())
            {
                if (modeId == ModeId.SuperHostRoles && !child.isSHROn) continue;

                if (!GameOptionsMenuUpdatePatch.IsHidden(option, modeId))
                {
                    if (child.isHeader == true) entry.Append("\n");
                    entry.AppendLine((indent ? "    " : "") + GameOptionsDataPatch.OptionToString(child));
                }
                addChildren(child, ref entry, modeId, indent);
            }
        }
    }

    internal static string GetWinnerMessage()
    {
        StringBuilder builder = new();
        foreach (var data in OnGameEndPatch.PlayerData.AsSpan())
        {
            if (data.IsWin) builder.Append("★");
            else builder.Append("　");
            builder.Append(data.name);
            builder.Append($"({data.CompleteTask}/{data.TotalTask})");
            builder.Append(" : ");
            builder.Append(ModTranslation.GetString($"FinalStatus{data.finalStatus}"));
            builder.Append(" : ");
            if (!data.role.HasValue)
                builder.Append(ModTranslation.GetString("WinnerGetError"));
            else
            {
                // このコメント消したら役職名に色がつく(見にくいから放置)
                /*
                Color RoleColor = CustomRoles.GetRoleColor(data.role.Value, IsImpostorReturn: data.isImpostor);
                if (data.role.Value == RoleId.DefaultRole && RoleColor.r == 1 && RoleColor.g == 1 && RoleColor.b == 1)
                    builder.Append(CustomOptionHolder.Cs(Palette.CrewmateBlue, IntroData.CrewmateIntro.NameKey+"Name"));
                else*/
                builder.Append(CustomRoles.GetRoleName/*OnColor*/
                    (data.role.Value, IsImpostorReturn: data.isImpostor));
            }
            builder.AppendLine();
        }
        return builder.ToString();
    }

    internal static string GetMatchMakingTag()
    {
        StringBuilder EnableTags = new();
        EnableTags.AppendLine(ModTranslation.GetString("EnableTagsMessage") + "\n");

        ModeId modeId = ModeHandler.GetMode(false);

        foreach (CustomOption option in CustomOption.options.AsSpan())
        {
            if (option.GetSelection() == 0) continue;
            if (option.type != CustomOptionType.MatchTag) continue;
            if (ModeHandler.IsMode(ModeId.SuperHostRoles, false) && !option.isSHROn) continue;
            if (option.IsHidden(modeId)) continue;

            string name = option.name;
            string pattern = @"<color=#\w+>|</color>";

            Regex colorRegex = new(pattern);
            name = colorRegex.Replace(name, "");

            EnableTags.AppendLine(name);
        }
        return $"{EnableTags}\n\n";
    }

    internal static (string, string) GetBattleRoles(string[] Commands)
    {
        if (Commands.Length > 1)
        {
            var data = Mode.BattleRoyal.SelectRoleSystem.RoleNames.FirstOrDefault(x => x.Key.Equals(Commands[1], StringComparison.OrdinalIgnoreCase));
            //nullチェック
            if (data.Equals(default(KeyValuePair<string, RoleId>)))
            {
                return (ModTranslation.GetString("BattleRoyalRoleNoneText"), Mode.BattleRoyal.SelectRoleSystem.BattleRoyalCommander);
            }
            else
            {
                string name = $"<size=200%>{ModHelpers.Cs(RoleClass.ImpostorRed, CustomRoles.GetRoleName(data.Value, IsImpostorReturn: true))}</size>";
                return (CustomRoles.GetRoleDescription(data.Value), name);
            }
        }
        else
        {
            string text = ModTranslation.GetString("BattleRoyalBattleRolesCommandText") + "\n\n";
            foreach (var role in Enum.GetValues(typeof(BattleRoles)))
            {
                text += $"{CustomRoles.GetRoleName((RoleId)(BattleRoles)role)}({(BattleRoles)role})\n";
            }
            return (text, Mode.BattleRoyal.SelectRoleSystem.BattleRoyalCommander);
        }
    }

    internal static void CreateGenerateCode(PlayerControl codeIssuanceUser)
    {
        if (codeIssuanceUser is null) return;

        void callback(long responseCode, DownloadHandler downloadHandler)
        {
            if (responseCode != 200)
            {
                if (downloadHandler.text.Length > 30)
                {
                    SendCommand(codeIssuanceUser, ModTranslation.GetString("SNRWebErrorReasonPrefix") + ModTranslation.GetString("SNRWebErrorReasonServer505"));
                }
                else
                {
                    SendCommand(codeIssuanceUser, ModTranslation.GetString("SNRWebErrorReasonPrefix") + ModTranslation.GetString(downloadHandler.text));
                }
            }
            else
                SendCommand(codeIssuanceUser, Format(ModTranslation.GetString("SNRWebSucGenerateCode"), downloadHandler.text));
        }

        WebApi.GenerateCode(codeIssuanceUser.Data.FriendCode, callback);
        SendCommand(codeIssuanceUser, ModTranslation.GetString("SNRWebCodeGeneratingNow"));
    }
}

/// <summary>
/// 役職情報のコマンドや文章作成を管理しているクラス
/// </summary>
internal static class RoleinformationText
{
    internal static void RoleCommand(PlayerControl target = null, float SendTime = 1.5f)
    {
        if (!AmongUsClient.Instance.AmHost) return;
        var isCommanderHost = target == PlayerControl.LocalPlayer;
        if (!(ModeHandler.IsMode(ModeId.Default, false) || ModeHandler.IsMode(ModeId.SuperHostRoles, false) || ModeHandler.IsMode(ModeId.Werewolf, false)))
        {
            SendCommand(target, ModTranslation.GetString("NotAssign"));
            return;
        }
        List<CustomRoleOption> EnableOptions = new();
        foreach (CustomRoleOption option in CustomRoleOption.RoleOptions.Values)
        {
            if (!option.IsRoleEnable) continue;
            if (ModeHandler.IsMode(ModeId.SuperHostRoles, false) && !option.isSHROn) continue;
            EnableOptions.Add(option);
        }
        float time = 0;
        foreach (CustomRoleOption option in EnableOptions.AsSpan())
        {
            (string rolename, string text) = RoleInfo.GetRoleInfo(option.RoleId, isGetAllRole: isCommanderHost);
            rolename = $"<align={"left"}><size=115%>\n" + CustomRoles.GetRoleNameOnColor(option.RoleId) + "</size></align>";
            text = $"\n<color=#00000000>{option.RoleId}</color>" + text;
            Send(target, rolename, text, time);
            time += SendTime;
        }
    }
    // /grのコマンド結果を返す。辞書を加工する。
    internal static string GetInRole()
    {
        string text = null;
        const string pos = "<pos=75%>";
        if (CustomOverlays.ActivateRolesDictionary.ContainsKey((byte)TeamRoleType.Impostor))
            text += CustomOverlays.ActivateRolesDictionary[(byte)TeamRoleType.Impostor].Replace(pos, "");
        if (CustomOverlays.ActivateRolesDictionary.ContainsKey((byte)TeamRoleType.Crewmate))
            text += CustomOverlays.ActivateRolesDictionary[(byte)TeamRoleType.Crewmate].Replace(pos, "");
        if (CustomOverlays.ActivateRolesDictionary.ContainsKey((byte)TeamRoleType.Neutral))
            text += CustomOverlays.ActivateRolesDictionary[(byte)TeamRoleType.Neutral].Replace(pos, "");
        return text;
    }
    internal static void GetInRoleCommand(PlayerControl target = null)
    {
        if (!AmongUsClient.Instance.AmHost) return;
        if (!(ModeHandler.IsMode(ModeId.Default, false) || ModeHandler.IsMode(ModeId.SuperHostRoles, false) || ModeHandler.IsMode(ModeId.Werewolf, false)))
        {
            SendCommand(target, ModTranslation.GetString("NotAssign"));
            return;
        }
        // ゲーム開始前は毎回現在の役職を取得する
        if (AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started)
            CustomOverlays.GetActivateRoles();
        SendCommand(target, GetInRole()); // 辞書の内容を加工した文字列を取得し、ターゲットに送信する
    }

    /// <summary>
    /// 役職の参考元Mod及び, 参考元Modのカラーコードも含むMod名を取得する。
    /// </summary>
    /// <param name="role">参考元を確認したい役職</param>
    /// <param name="quoteModText">役職参考元のMod名(Modカラーは, 参考元のModのゲーム上での色に準拠する)</param>
    /// <returns>参考元役職</returns>
    internal static Roles.Role.QuoteMod QuoteModName(RoleId role, out string quoteModText)
    {
        Roles.Role.QuoteMod quoteMod = CustomRoles.GetQuoteMod(role);
        quoteModText = ModTranslation.GetString($"QuoteMod{quoteMod}");
        return quoteMod;
    }

    /// <summary>
    /// コマンドで指定した役職の説明を取得する
    /// </summary>
    /// <param name="sourcePlayer">送信先(Hostだった場合全体送信)</param>
    /// <param name="command">指定された役職名</param>
    internal static void RoleInfoSendCommand(PlayerControl sourcePlayer, string command)
    {
        if (!AmongUsClient.Instance.AmHost) return;

        PlayerControl target = sourcePlayer.AmOwner ? null : sourcePlayer;
        /*
        (string[] roleNameKey, bool isSuccess) = ModTranslation.GetTranslateKey(command);

        string beforeIdChangeRoleName =
            isSuccess
                ? roleNameKey.FirstOrDefault(key => key.Contains("Name")).Replace("Name", "") ?? command // 翻訳キーの取得に成功した場合, 配列から"Name"を含む要素を取得し そのから要素"Name"を外して, RoleIdに一致する役職名を取得する.
                : command; // 翻訳辞書からの取得に失敗した場合, 入力された文字のまま (失敗処理は, RoleIdで入力された場合も含む)

        string roleName = "NONE", roleInfo = "";

        // 参考 => https://qiita.com/masaru/items/a44dc30bfc18aac95015#fnref1
        // 取得した役職名(string)からRoleIdを取得する。
        var roleIdChange = Enum.TryParse(beforeIdChangeRoleName, out RoleId roleId) && Enum.IsDefined(typeof(RoleId), roleId);
        if (roleIdChange)
        {
            (roleName, roleInfo) = RoleInfo.GetRoleInfo(roleId, AmongUsClient.Instance.AmHost);
            if (roleName == "NONE") roleInfo = Format(roleInfo, command); // RoleIdからの役職情報の取得に失敗していた場合, 入力した役職名を追加する。
            SendCommand(target, roleInfo, roleName);
            return;
        }*/
        string roleName = "NONE";
        RoleId? roleId = GetRoleIdByName(command);
        if (roleId.HasValue)
        {
            (roleName, string roleInfo) = RoleInfo.GetRoleInfo(roleId.Value, AmongUsClient.Instance.AmHost);
            if (roleName == "NONE") roleInfo = Format(roleInfo, command); // RoleIdからの役職情報の取得に失敗していた場合, 入力した役職名を追加する。
            SendCommand(target, roleInfo, roleName);
            return;
        }

        SendCommand(target, Format(ModTranslation.GetString("RoleInfoError"), command));
    }
    public static RoleId? GetRoleIdByName(string Name)
    {
        (string[] roleNameKey, bool isSuccess) = ModTranslation.GetTranslateKey(Name);

        string beforeIdChangeRoleName =
            isSuccess
                ? roleNameKey.FirstOrDefault(key => key.Contains("Name")).Replace("Name", "") ?? Name // 翻訳キーの取得に成功した場合, 配列から"Name"を含む要素を取得し そのから要素"Name"を外して, RoleIdに一致する役職名を取得する.
                : Name; // 翻訳辞書からの取得に失敗した場合, 入力された文字のまま (失敗処理は, RoleIdで入力された場合も含む)

        // 参考 => https://qiita.com/masaru/items/a44dc30bfc18aac95015#fnref1
        // 取得した役職名(string)からRoleIdを取得する。
        var roleIdChange = Enum.TryParse(beforeIdChangeRoleName, out RoleId roleId) && Enum.IsDefined(typeof(RoleId), roleId);
        return roleIdChange ? roleId : null;
    }
    internal static void YourRoleInfoSendCommand()
    {
        foreach (PlayerControl player in CachedPlayer.AllPlayers.AsSpan())
        {
            if (player == null || player.IsBot()) continue;
            RoleId roleId = player.GetRole();
            if (roleId == RoleId.Bestfalsecharge && player.IsAlive()) roleId = RoleId.DefaultRole;
            RoleId ghostRoleId = player.GetGhostRole();

            string roleName = "NONE", roleInfo = "";
            (roleName, roleInfo) = RoleInfo.GetRoleInfo(roleId, AmongUsClient.Instance.AmHost, player.IsImpostor());
            SendCommand(player, roleInfo, roleName);

            if (ghostRoleId != RoleId.DefaultRole)
            {
                string ghostRoleName = "NONE", ghostRoleInfo = "";
                (ghostRoleName, ghostRoleInfo) = RoleInfo.GetRoleInfo(ghostRoleId, AmongUsClient.Instance.AmHost, player.IsImpostor());
                SendCommand(player, ghostRoleInfo, ghostRoleName);
            }
        }
    }
    internal static class RoleInfo
    {
        private static Dictionary<RoleId, (string, string)> RoleInfoDic;
        private static Dictionary<TeamType, (string, string)> DefaultRoleInfoDic;
        internal static void ClearAndReload()
        {
            RoleInfoDic = new();
            DefaultRoleInfoDic = new();
            if (AmongUsClient.Instance.AmHost) SaveEnableRoleInfo();
        }

        /// <summary>
        /// 役職説明を記載する辞書に内容を保存する。
        /// </summary>
        private static void SaveEnableRoleInfo()
        {
            // 現在有効な役職を取得
            List<CustomRoleOption> EnableOptions = new();
            foreach (CustomRoleOption option in CustomRoleOption.RoleOptions.Values)
            {
                if (!option.IsRoleEnable) continue;
                if (ModeHandler.IsMode(ModeId.SuperHostRoles, false) && !option.isSHROn) continue;
                EnableOptions.Add(option);
            }

            // 現在有効な役職の説明を保存
            foreach (CustomRoleOption roleOption in EnableOptions.AsSpan())
            {
                RoleId roleId = roleOption.RoleId;
                string roleName, info;
                (roleName, info) = WriteRoleInfo(roleId);

                if (!RoleInfoDic.ContainsKey(roleId))
                    RoleInfoDic.Add(roleId, (roleName, info));
                else RoleInfoDic[roleId] = (roleName, info);
            }

            // 素インポスター及び素クルーメイト時の, 役職説明の保存
            for (int i = 0; i < 2; i++)
            {
                RoleId roleId = RoleId.DefaultRole;
                string roleName, roleInfo = null;
                TeamType teamType = (TeamType)i;

                (roleName, roleInfo) = WriteRoleInfo(roleId, (TeamType)i == TeamType.Impostor);

                if (!DefaultRoleInfoDic.ContainsKey(teamType))
                    DefaultRoleInfoDic.Add(teamType, (roleName, roleInfo));
                else DefaultRoleInfoDic[teamType] = (roleName, roleInfo);
            }
        }

        /// <summary>
        /// 役職の説明を取得する
        /// </summary>
        /// <param name="roleId">説明を取得したい役のid</param>
        /// <param name="isImpostor">true : インポスターである</param>
        /// <returns> string_1 : 役職名, 陣営 / string_2 : 役職説明</returns>
        private static (string, string) WriteRoleInfo(RoleId roleId, bool isImpostor = false)
        {
            string roleName = "NONE", roleInfo = "";

            // Mod役職の情報取得
            if (roleId != RoleId.DefaultRole)
            {
                IEnumerable<CustomRoleOption> roleOptions = CustomRoleOption.RoleOptions.Values.Where(option => option.RoleId == roleId).Select(option => { return option; });
                {
                    foreach (CustomRoleOption roleOption in roleOptions)
                    {
                        StringBuilder optionBuilder = new();

                        roleName = $"<align={"left"}><size=180%>{CustomRoles.GetRoleNameOnColor(roleOption.RoleId)}</size></align></size>";

                        optionBuilder.AppendLine($"<align={"left"}><size=80%>\n" + GetTeamText(CustomRoles.GetRoleTeamType(roleOption.RoleId)) + "\n</size>");
                        optionBuilder.AppendLine($"<size=100%>「{CustomOptionHolder.Cs(CustomRoles.GetRoleColor(roleOption.RoleId), CustomRoles.GetRoleIntro(roleOption.RoleId))}」</size>\n");
                        optionBuilder.AppendLine($"<size=80%>{CustomRoles.GetRoleDescription(roleOption.RoleId)}\n</size>");
                        optionBuilder.AppendLine($"<size=70%>{ModTranslation.GetString("MessageSettings")}:");
                        optionBuilder.AppendLine($"{GetOptionText(roleOption)}\n<color=#00000000>{CustomRoles.GetRoleNameKey(roleOption.RoleId)}</color></align></size>");

                        roleInfo = optionBuilder.ToString();
                        return (roleName, roleInfo);
                    }
                }
            }
            else // vanilla役職の説明を取得
            {
                Color color;
                TeamType teamType;
                StringBuilder vanillaRoleBuilder = new();
                string teamName;

                if (isImpostor)
                {
                    color = RoleClass.ImpostorRed;
                    teamName = "Impostor";
                    teamType = TeamType.Impostor;
                }
                else
                {
                    color = Palette.CrewmateBlue;
                    teamName = "Crewmate";
                    teamType = TeamType.Crewmate;
                }

                roleName = $"<align={"left"}><size=180%>{CustomOptionHolder.Cs(color, teamName + "Name")}</size> <size=50%>\n{GetTeamText(teamType)}</align></size>";

                vanillaRoleBuilder.AppendLine("\n");

                vanillaRoleBuilder.AppendLine($"<align={"left"}><size=100%>「{CustomOptionHolder.Cs(color, teamName + "Title1")}」</size>\n");
                vanillaRoleBuilder.AppendLine($"<size=80%>{ModTranslation.GetString(teamName + "Description")}</align></size>\n\n");

                roleInfo = vanillaRoleBuilder.ToString();
                return (roleName, roleInfo);
            }

            string errorMessage = Format(ModTranslation.GetString("RoleIdInfoError"), "{0}", roleId);
            Logger.Error($"RoleId : [ {roleId} ] の取得に失敗しました。", "WriteRoleInfo");
            return (roleName, errorMessage);
        }

        /// <summary>
        /// RoleIdから役職の説明を取得する。
        /// </summary>
        /// <param name="roleId">説明を取得したい役職のRoleId</param>
        /// <param name="isGetAllRole">辞書に保存されていない(配役されていない)役の情報も取得するか</param>
        /// <param name="isImpostor">true : 取得対象がインポスター役職</param>
        /// <returns>string.1 : 役職名 / string.2 : 役職説明</returns>
        internal static (string, string) GetRoleInfo(RoleId roleId, bool isGetAllRole = false, bool isImpostor = false)
        {
            string roleName = "NONE", roleInfo = "";

            if (roleId != RoleId.DefaultRole)
            {
                if (RoleInfoDic.ContainsKey(roleId))
                {
                    roleName = RoleInfoDic[roleId].Item1;
                    roleInfo = RoleInfoDic[roleId].Item2;
                }
                else if (isGetAllRole) (roleName, roleInfo) = WriteRoleInfo(roleId);
                else roleInfo = ModTranslation.GetString("GetRoleInfoErrorDisable");

                return (roleName, roleInfo);
            }
            else
            {
                TeamType type = isImpostor ? TeamType.Impostor : TeamType.Crewmate;
                if (DefaultRoleInfoDic.ContainsKey(type))
                {
                    roleName = DefaultRoleInfoDic[type].Item1;
                    roleInfo = DefaultRoleInfoDic[type].Item2;
                }
                else (roleName, roleInfo) = WriteRoleInfo(roleId, isImpostor);

                return (roleName, roleInfo);
            }
        }
    }
}