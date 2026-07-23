using System;
using System.Linq;
using System.Text;
using AmongUs.GameOptions;
using HarmonyLib;
using InnerNet;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles;
using UnityEngine;

namespace SuperNewRoles.Patches;


[HarmonyPatch(typeof(ChatController), nameof(ChatController.SendChat))]
public static class SendChatPatch
{
    static bool Prefix(ChatController __instance)
    {
        string text = __instance.freeChatField.textArea.text;
        bool handled = false;
        if (AmongUsClient.Instance.GameState == InnerNetClient.GameStates.Joined)
        {
            if (text.ToLower().StartsWith("/mp "))
            { // Unfortunately server holds this - need to do more trickery
                if (AmongUsClient.Instance.AmHost && AmongUsClient.Instance.CanBan())
                {
                    handled = true;
                    string changeLimitLog = DynamicLobbies.LobbyLimitChange(text);
                    __instance.AddChat(PlayerControl.LocalPlayer, changeLimitLog);
                }
            }
            else if (text.ToLower().StartsWith("/kc "))
            { // Unfortunately server holds this - need to do more trickery
                if (AmongUsClient.Instance.AmHost && AmongUsClient.Instance.CanBan())
                {
                    handled = true;
                    if (!float.TryParse(text[4..], out var cooltime)) __instance.AddChat(PlayerControl.LocalPlayer, "使い方\n/kc {キルクールタイム}");
                    var settime = cooltime;
                    if (settime == 0) settime = 0.00001f;
                    GameOptionsManager.Instance.CurrentGameOptions.SetFloat(FloatOptionNames.KillCooldown, settime);
                    DynamicLobbies.RpcSyncOption(GameOptionsManager.Instance.CurrentGameOptions);
                    __instance.AddChat(PlayerControl.LocalPlayer, $"キルクールタイムを{cooltime}秒に変更しました！");
                }
            }
            else if (text.ToLower().StartsWith("/rename "))
            {
                if (AmongUsClient.Instance.AmHost)
                {
                    handled = true;
                    PlayerControl.LocalPlayer.RpcSetName(text.ToLower().Replace("/rename ", ""));
                }
                else //ゲスト時には使用不可能にする
                {
                    handled = true;
                    __instance.AddChat(PlayerControl.LocalPlayer, ModTranslation.GetString("CannotUseRenameMessage"));
                    SuperNewRoles.Logger.Warning($"ホストでない時に{text}を使用しました。ホストでない時は/renameは使用できません。");
                }
            }
            else if (text.StartsWith("/winners", StringComparison.OrdinalIgnoreCase)
                  || string.Equals(text, "/w", StringComparison.OrdinalIgnoreCase)
                  || text.StartsWith("/w ", StringComparison.OrdinalIgnoreCase))
            {
                handled = true;
                // ホストの/winnersは全員に表示する
                if (AmongUsClient.Instance.AmHost)
                    RpcShowWinnerMessage();
                else
                    ShowWinnerMessage();
            }
            if (AmongUsClient.Instance.NetworkMode == NetworkModes.FreePlay)
            {
                if (text.ToLower().Equals("/murder"))
                {
                    PlayerControl.LocalPlayer.MurderPlayer(PlayerControl.LocalPlayer, MurderResultFlags.Succeeded);/*
                    PlayerControl.LocalPlayer.Exiled();
                    FastDestroyableSingleton<HudManager>.Instance.KillOverlay.ShowKillAnimation(PlayerControl.LocalPlayer.Data, CachedPlayer.LocalPlayer.Data);*/
                    handled = true;
                }
                else if (text.ToLower().StartsWith("/color "))
                {
                    handled = true;
                    if (!int.TryParse(text[7..], out int col))
                    {
                        __instance.AddChat(PlayerControl.LocalPlayer, "Unable to parse color id\nUsage: /color {id}");
                    }
                    col = Math.Clamp(col, 0, Palette.PlayerColors.Length - 1);
                    PlayerControl.LocalPlayer.SetColor(col);
                    __instance.AddChat(PlayerControl.LocalPlayer, "Changed color succesfully");
                }
                else if (text.ToLower().StartsWith("/name "))
                {
                    handled = true;
                    string col = text[6..];
                    PlayerControl.LocalPlayer.SetName(col);
                    __instance.AddChat(PlayerControl.LocalPlayer, "Changed name succesfully");
                }
            }
        }
        if (handled)
        {
            __instance.freeChatField.textArea.Clear();
            FastDestroyableSingleton<HudManager>.Instance.Chat.timeSinceLastMessage = 0f;
        }
        return !handled;
    }
    [CustomRPC]
    public static void RpcShowWinnerMessage()
    {
        ShowWinnerMessage();
    }
    private static void ShowWinnerMessage()
    {

        string originalName = PlayerControl.LocalPlayer.name;
        try
        {
            PlayerControl.LocalPlayer.SetName(GetWinnerMessage());
            HudManager.Instance.Chat.AddChat(PlayerControl.LocalPlayer, "\u200B");
        }
        finally
        {
            PlayerControl.LocalPlayer.SetName(originalName);
        }
    }
    private static string GetWinnerMessage()
    {
        StringBuilder builder = new();
        var cond = EndGameManagerSetUpPatch.endGameCondition;
        if (cond == null)
            return ModTranslation.GetString("Winners.NoData");
        var upperText = cond.UpperText;
        // additionalWinTexts の各要素は "役職名\x1FRRGGBB" 形式でエンコードされている場合がある
        // （EndGamer.EncodeWithColor 参照）。修正前は単純に連結して upperText 全体を
        // 一括で ModHelpers.Cs(cond.UpperTextColor, upperText) に通していたため、
        // ①色コード部分（\x1F以降）がデコードされずそのまま文字列として表示される
        // ②追加役職名や「&」記号までメインの役職色で塗られてしまい、
        //    各役職固有の色が反映されない
        // という2つの表示崩れが起きていた。ここでメイン役職名と追加役職名をそれぞれ
        // 個別に色付けしてから連結し、「&」区切りはデフォルト色（白）のまま残す。
        string coloredUpperText = ModHelpers.Cs(cond.UpperTextColor, upperText);
        if (cond.additionalWinTexts != null && cond.additionalWinTexts.Any())
        {
            foreach (var entry in cond.additionalWinTexts)
            {
                int sepIndex = entry.IndexOf(EndGamer.ColorEncodeSeparator);
                string entryText = sepIndex >= 0 ? entry[..sepIndex] : entry;
                string coloredEntry = entryText;
                if (sepIndex >= 0)
                {
                    string hex = entry[(sepIndex + 1)..];
                    if (ColorUtility.TryParseHtmlString("#" + hex, out Color parsedColor))
                        coloredEntry = ModHelpers.Cs(parsedColor, entryText);
                }
                coloredUpperText += " & " + coloredEntry;
            }
        }
        coloredUpperText += " " + ModHelpers.Cs(cond.UpperTextColor, cond.winText);
        if (cond.IsHaison)
            coloredUpperText = ModTranslation.GetString("Haison");
        builder.AppendLine($"<size=150%>{coloredUpperText}</size>");
        foreach (var data in AdditionalTempData.playerRoles)
        {
            builder.Append("<size=80%>");
            if (cond.winners != null && cond.winners.Contains((byte)data.PlayerId))
                builder.Append("★");
            else
                builder.Append("　");
            var taskInfo = data.TasksTotal > 0 ? $"<color=#FAD934FF>({data.TasksCompleted}/{data.TasksTotal})</color>" : "";
            string roleText = ModHelpers.Cs(data.roleBase.RoleColor, CustomRoleManager.GetRoleName(data.roleBase.Role));
            if (data.modifierMarks.Count > 0)
                roleText += " ";
            foreach (var modifier in data.modifierMarks)
                roleText = modifier.Replace("{0}", roleText);
            string playerName = ModHelpers.Cs(Palette.PlayerColors[data.ColorId], data.PlayerName);
            if (data.LoversHeartColor != null)
                playerName += ModHelpers.Cs(data.LoversHeartColor.Value, " ♥");
            string result = $"{playerName}{data.NameSuffix} {taskInfo} - {ModTranslation.GetString("FinalStatus." + data.Status)} - {roleText}";
            builder.AppendLine(result);
            builder.Append("</size>");
        }
        return builder.ToString();
    }
}
