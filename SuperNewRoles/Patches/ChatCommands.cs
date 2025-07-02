using System;
using AmongUs.GameOptions;
using HarmonyLib;
using InnerNet;

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
                    SuperNewRolesPlugin.Logger.LogWarning($"ホストでない時に{text}を使用しました。ホストでない時は/renameは使用できません。");
                }
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
}