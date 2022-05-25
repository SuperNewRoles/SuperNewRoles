﻿using HarmonyLib;
using Hazel;
using InnerNet;
using SuperNewRoles.Mode;
using System;
using System.Collections.Generic;
using System.Text;

namespace SuperNewRoles.Patch
{
    class ChatCommand
    {

    }
    [HarmonyPatch]
    public static class DynamicLobbies
    {
        public static int LobbyLimit = 15;
        [HarmonyPatch(typeof(ChatController), nameof(ChatController.SendChat))]
        private static class SendChatPatch
        {
            static bool Prefix(ChatController __instance)
            {
                string text = __instance.TextArea.text;
                bool handled = false;
                if (AmongUsClient.Instance.GameState != InnerNetClient.GameStates.Started)
                {
                    if (text.ToLower().StartsWith("/mp "))
                    { // Unfortunately server holds this - need to do more trickery
                        if (AmongUsClient.Instance.AmHost && AmongUsClient.Instance.CanBan())
                        {
                            handled = true;
                            if (!Int32.TryParse(text.Substring(4), out LobbyLimit))
                            {
                                __instance.AddChat(PlayerControl.LocalPlayer, "使い方\n/mp {最大人数}");
                            }
                            else
                            {
                                if (!ModHelpers.isCustomServer())
                                {
                                    LobbyLimit = Math.Clamp(LobbyLimit, 4, 15);
                                }
                                if (LobbyLimit != PlayerControl.GameOptions.MaxPlayers)
                                {
                                    PlayerControl.GameOptions.MaxPlayers = LobbyLimit;
                                    DestroyableSingleton<GameStartManager>.Instance.LastPlayerCount = LobbyLimit;
                                    PlayerControl.LocalPlayer.RpcSyncSettings(PlayerControl.GameOptions);
                                    __instance.AddChat(PlayerControl.LocalPlayer, $"ロビーの最大人数を{LobbyLimit}人に変更しました！");
                                }
                                else
                                {
                                    __instance.AddChat(PlayerControl.LocalPlayer, $"プレイヤー最小人数は {LobbyLimit}です。");
                                }
                            }
                        }
                    } else if (text.ToLower().StartsWith("/kc "))
                    { // Unfortunately server holds this - need to do more trickery
                        if (AmongUsClient.Instance.AmHost && AmongUsClient.Instance.CanBan())
                        {
                            handled = true;
                            if (!float.TryParse(text.Substring(4), out var cooltime))
                            {
                                __instance.AddChat(PlayerControl.LocalPlayer, "使い方\n/kc {キルクールタイム}");
                            }
                            var settime = cooltime;
                            if (settime == 0)
                            {
                                settime = 0.00001f;
                            }
                            
                            PlayerControl.GameOptions.KillCooldown = settime;
                            PlayerControl.LocalPlayer.RpcSyncSettings(PlayerControl.GameOptions);
                            __instance.AddChat(PlayerControl.LocalPlayer, $"キルクールタイムを{cooltime}秒に変更しました！");
                        }
                    }
                    else if (ModeHandler.isMode(ModeId.SuperHostRoles))
                    {
                        handled = Mode.SuperHostRoles.RoleChat.SendChat(__instance);
                    }
                }

                if (AmongUsClient.Instance.GameMode == GameModes.FreePlay)
                {
                    if (text.ToLower().Equals("/murder"))
                    {
                        PlayerControl.LocalPlayer.Exiled();
                        HudManager.Instance.KillOverlay.ShowKillAnimation(PlayerControl.LocalPlayer.Data, PlayerControl.LocalPlayer.Data);
                        handled = true;
                    }
                    else if (text.ToLower().StartsWith("/color "))
                    {
                        handled = true;
                        int col;
                        if (!Int32.TryParse(text.Substring(7), out col))
                        {
                            __instance.AddChat(PlayerControl.LocalPlayer, "Unable to parse color id\nUsage: /color {id}");
                        }
                        col = Math.Clamp(col, 0, Palette.PlayerColors.Length - 1);
                        PlayerControl.LocalPlayer.SetColor(col);
                        __instance.AddChat(PlayerControl.LocalPlayer, "Changed color succesfully"); ;
                    }
                    else if (text.ToLower().StartsWith("/name "))
                    {
                        handled = true;
                        string col = text.Substring(6);
                        PlayerControl.LocalPlayer.SetName(col);
                        __instance.AddChat(PlayerControl.LocalPlayer, "Changed name succesfully"); ;
                    }
                }
                if (handled)
                {
                    __instance.TextArea.Clear();
                    HudManager.Instance.Chat.TimeSinceLastMessage = 0f;
                    __instance.quickChatMenu.ResetGlyphs();
                }
                return !handled;
            }
        }
        [HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.HostGame))]
        public static class InnerNetClientHostPatch
        {
            public static void Prefix(InnerNet.InnerNetClient __instance, [HarmonyArgument(0)] GameOptionsData settings)
            {
                LobbyLimit = settings.MaxPlayers;
                settings.MaxPlayers = 15; // Force 15 Player Lobby on Server
                SaveManager.ChatModeType = InnerNet.QuickChatModes.FreeChatOrQuickChat;
            }
            public static void Postfix(InnerNet.InnerNetClient __instance, [HarmonyArgument(0)] GameOptionsData settings)
            {
                settings.MaxPlayers = LobbyLimit;
            }
        }
        [HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.JoinGame))]
        public static class InnerNetClientJoinPatch
        {
            public static void Prefix(InnerNet.InnerNetClient __instance)
            {
                SaveManager.ChatModeType = InnerNet.QuickChatModes.FreeChatOrQuickChat;
            }
        }
        [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnPlayerJoined))]
        public static class AmongUsClientOnPlayerJoined
        {
            public static bool Prefix(AmongUsClient __instance, [HarmonyArgument(0)] ClientData client)
            {
                if (LobbyLimit < __instance.allClients.Count)
                { // TODO: Fix this canceling start
                    DisconnectPlayer(__instance, client.Id);
                    return false;
                }
                return true;
            }

            private static void DisconnectPlayer(InnerNetClient _this, int clientId)
            {
                if (!_this.AmHost)
                {
                    return;
                }
                MessageWriter messageWriter = MessageWriter.Get(SendOption.Reliable);
                messageWriter.StartMessage(4);
                messageWriter.Write(_this.GameId);
                messageWriter.WritePacked(clientId);
                messageWriter.Write((byte)DisconnectReasons.GameFull);
                messageWriter.EndMessage();
                _this.SendOrDisconnect(messageWriter);
                messageWriter.Recycle();
            }
        }
    }
}
