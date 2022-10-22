using System;
using AmongUs.Data.Legacy;
using HarmonyLib;
using Hazel;
using InnerNet;
using static System.Int32;

namespace SuperNewRoles.Patches
{
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
                            if (!TryParse(text[4..], out LobbyLimit))
                            {
                                __instance.AddChat(CachedPlayer.LocalPlayer.PlayerControl, "使い方\n/mp {最大人数}");
                            }
                            else
                            {
                                if (!ModHelpers.IsCustomServer())
                                {
                                    LobbyLimit = Math.Clamp(LobbyLimit, 4, 15);
                                }
                                if (LobbyLimit != PlayerControl.GameOptions.MaxPlayers)
                                {
                                    PlayerControl.GameOptions.MaxPlayers = LobbyLimit;
                                    FastDestroyableSingleton<GameStartManager>.Instance.LastPlayerCount = LobbyLimit;
                                    CachedPlayer.LocalPlayer.PlayerControl.RpcSyncSettings(PlayerControl.GameOptions);
                                    __instance.AddChat(CachedPlayer.LocalPlayer.PlayerControl, $"ロビーの最大人数を{LobbyLimit}人に変更しました！");
                                }
                                else
                                {
                                    __instance.AddChat(CachedPlayer.LocalPlayer.PlayerControl, $"プレイヤー最小人数は {LobbyLimit}です。");
                                }
                            }
                        }
                    }
                    else if (text.ToLower().StartsWith("/kc "))
                    { // Unfortunately server holds this - need to do more trickery
                        if (AmongUsClient.Instance.AmHost && AmongUsClient.Instance.CanBan())
                        {
                            handled = true;
                            if (!float.TryParse(text[4..], out var cooltime)) __instance.AddChat(CachedPlayer.LocalPlayer.PlayerControl, "使い方\n/kc {キルクールタイム}");
                            var settime = cooltime;
                            if (settime == 0) settime = 0.00001f;
                            PlayerControl.GameOptions.KillCooldown = settime;
                            CachedPlayer.LocalPlayer.PlayerControl.RpcSyncSettings(PlayerControl.GameOptions);
                            __instance.AddChat(CachedPlayer.LocalPlayer.PlayerControl, $"キルクールタイムを{cooltime}秒に変更しました！");
                        }
                    }
                    else if (text.ToLower().StartsWith("/rename "))
                    {
                        if (AmongUsClient.Instance.AmHost)
                        {
                            handled = true;
                            CachedPlayer.LocalPlayer.PlayerControl.RpcSetName(text.ToLower().Replace("/rename ", ""));
                        }
                        else //ゲスト時には使用不可能にする
                        {
                            handled = true;
                            __instance.AddChat(CachedPlayer.LocalPlayer.PlayerControl, ModTranslation.GetString("CannotUseRenameMessage"));
                            SuperNewRolesPlugin.Logger.LogWarning($"ホストでない時に{text}を使用しました。ホストでない時は/renameは使用できません。");
                        }
                    }

                    if (AmongUsClient.Instance.GameMode == GameModes.FreePlay)
                    {
                        if (text.ToLower().Equals("/murder"))
                        {
                            CachedPlayer.LocalPlayer.PlayerControl.Exiled();
                            FastDestroyableSingleton<HudManager>.Instance.KillOverlay.ShowKillAnimation(CachedPlayer.LocalPlayer.Data, CachedPlayer.LocalPlayer.Data);
                            handled = true;
                        }
                        else if (text.ToLower().StartsWith("/color "))
                        {
                            handled = true;
                            if (!TryParse(text[7..], out int col))
                            {
                                __instance.AddChat(CachedPlayer.LocalPlayer.PlayerControl, "Unable to parse color id\nUsage: /color {id}");
                            }
                            col = Math.Clamp(col, 0, Palette.PlayerColors.Length - 1);
                            CachedPlayer.LocalPlayer.PlayerControl.SetColor(col);
                            __instance.AddChat(CachedPlayer.LocalPlayer.PlayerControl, "Changed color succesfully");
                        }
                        else if (text.ToLower().StartsWith("/name "))
                        {
                            handled = true;
                            string col = text[6..];
                            CachedPlayer.LocalPlayer.PlayerControl.SetName(col);
                            __instance.AddChat(CachedPlayer.LocalPlayer.PlayerControl, "Changed name succesfully");
                        }
                    }
                    if (handled)
                    {
                        __instance.TextArea.Clear();
                        FastDestroyableSingleton<HudManager>.Instance.Chat.TimeSinceLastMessage = 0f;
                        __instance.quickChatMenu.ResetGlyphs();
                    }
                }
                return !handled;
            }
            [HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.HostGame))]
            public static class InnerNetClientHostPatch
            {
                public static void Prefix([HarmonyArgument(0)] GameOptionsData settings)
                {
                    LobbyLimit = settings.MaxPlayers;
                    settings.MaxPlayers = 15; // Force 15 Player Lobby on Server
                    /*LegacySaveManager.chatModeType = QuickChatModes.FreeChatOrQuickChat;*/
                }
                public static void Postfix([HarmonyArgument(0)] GameOptionsData settings)
                {
                    settings.MaxPlayers = LobbyLimit;
                }
            }
            [HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.JoinGame))]
            public static class InnerNetClientJoinPatch
            {
                public static void Prefix()
                {
                    /*LegacySaveManager.ChatModeType = QuickChatModes.FreeChatOrQuickChat;*/
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
}