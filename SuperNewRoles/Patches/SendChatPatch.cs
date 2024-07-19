using System;
using AmongUs.Data;
using AmongUs.GameOptions;
using HarmonyLib;
using Hazel;
using InnerNet;
using SuperNewRoles.Helpers;
using static System.Int32;

namespace SuperNewRoles.Patches;

[HarmonyPatch(typeof(ChatController), nameof(ChatController.SendChat))]
public static class SendChatPatch
{
    static bool Prefix(ChatController __instance)
    {
        string text = __instance.freeChatField.textArea.text;
        bool handled = false;
        if (AmongUsClient.Instance.GameState != InnerNetClient.GameStates.Started)
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
                    RPCHelper.RpcSyncOption();
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
                    SuperNewRolesPlugin.Logger.LogWarning($"ホストでない時に{text}を使用しました。ホストでない時は/renameは使用できません。");
                }
            }
            if (AmongUsClient.Instance.NetworkMode == NetworkModes.FreePlay)
            {
                if (text.ToLower().Equals("/murder"))
                {
                    PlayerControl.LocalPlayer.Exiled();
                    FastDestroyableSingleton<HudManager>.Instance.KillOverlay.ShowKillAnimation(CachedPlayer.LocalPlayer.Data, CachedPlayer.LocalPlayer.Data);
                    handled = true;
                }
                else if (text.ToLower().StartsWith("/color "))
                {
                    handled = true;
                    if (!TryParse(text[7..], out int col))
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
        if (AmongUsClient.Instance.GameState == InnerNetClient.GameStates.Started)
        {
            if (text.ToLower().StartsWith("/yr") || text.ToLower().StartsWith("/yourrole"))
            {
                if (!AmongUsClient.Instance.AmHost) return true;
                handled = true;
                RoleinformationText.YourRoleInfoSendCommand();
            }
        }
        if (text.ToLower().StartsWith("/sl") || text.ToLower().StartsWith("/savelog"))
        {// ファイル名を付けてログを別の場所に保管するコマンド

            handled = true;
            string memo;
            string via = "ChatCommandVia";

            // textからコマンドを削除し、ファイル名につける文字列を抜き出す
            memo = text.ToLower()
                .Replace("/sl", "")
                .Replace("/savelog", "")
                .Replace(" ", "")
                .Replace("　", "");

            // memoが空なら ファイル名をChatCommandViaにする。
            if (memo == "") LoggerPlus.SaveLog(via, via);
            // memoの中身があるなら ファイル名を任意の文字列にする。
            else LoggerPlus.SaveLog(memo, via);
        }
        else if (text.ToLower().StartsWith("/lp"))
        {
            handled = true;

            string print = text.ToLower()
                .Replace("/lp ", "")
                .Replace("/lp", "");

            Logger.Info(print, "任意ログ印字");
            __instance.AddChat(PlayerControl.LocalPlayer, $"このチャットは貴方にのみ表示されています。\nLogに以下の内容を印字しました。\n「{print}」");
        }
        if (handled)
        {
            __instance.freeChatField.textArea.Clear();
            FastDestroyableSingleton<HudManager>.Instance.Chat.timeSinceLastMessage = 0f;
        }
        return !handled;
    }
}