using System;
using System.Collections.Generic;
using AmongUs.Data;
using AmongUs.GameOptions;
using HarmonyLib;
using Hazel;
using InnerNet;
using SuperNewRoles.Helpers;
using UnityEngine;
using static System.Int32;

namespace SuperNewRoles.Patches;

[Harmony]
public static class DynamicLobbies
{
    static int LobbyLimit = 15;

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

    [HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.HostGame))]
    public static class InnerNetClientHostPatch
    {
        public static void Prefix([HarmonyArgument(0)] IGameOptions settings)
        {
            LobbyLimit = settings.MaxPlayers;
            GameOptionsManager.Instance.CurrentGameOptions.SetInt(Int32OptionNames.MaxPlayers, 15); // Force 15 Player Lobby on Server
            DataManager.Settings.Multiplayer.ChatMode = QuickChatModes.FreeChatOrQuickChat;
        }
        public static void Postfix([HarmonyArgument(0)] IGameOptions settings)
        {
            GameOptionsManager.Instance.CurrentGameOptions.SetInt(Int32OptionNames.MaxPlayers, LobbyLimit);
        }
    }

    [HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.JoinGame))]
    public static class InnerNetClientJoinPatch
    {
        public static void Prefix()
        {
            DataManager.Settings.Multiplayer.ChatMode = QuickChatModes.FreeChatOrQuickChat;
        }
    }

    public static string LobbyLimitChange(string command)
    {
        if (!TryParse(command[4..], out LobbyLimit))
        {
            return "使い方\n/mp {最大人数}";
        }
        else
        {
            if (!ModHelpers.IsCustomServer())
            {
                LobbyLimit = Math.Clamp(LobbyLimit, 4, 15);
            }
            if (LobbyLimit != GameManager.Instance.LogicOptions.currentGameOptions.MaxPlayers)
            {
                GameManager.Instance.LogicOptions.currentGameOptions.SetInt(Int32OptionNames.MaxPlayers, LobbyLimit);
                FastDestroyableSingleton<GameStartManager>.Instance.LastPlayerCount = LobbyLimit;
                RPCHelper.RpcSyncOption(GameManager.Instance.LogicOptions.currentGameOptions);
                return $"ロビーの最大人数を{LobbyLimit}人に変更しました！";
            }
            else
            {
                return $"プレイヤー最小人数は {LobbyLimit}です。";
            }
        }
    }

    [HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.SendInitialData))]
    public static class SendInitialDataPatch
    {
        public static bool Prefix(InnerNetClient __instance, int clientId)
        {
            SendData(__instance, clientId);
            return false;
        }

        public static void SendData(InnerNetClient __instance, int clientId, int start = 0)
        {
            int index = start;
            MessageWriter messageWriter = MessageWriter.Get(SendOption.Reliable);
            messageWriter.StartMessage(6);
            messageWriter.Write(__instance.GameId);
            messageWriter.WritePacked(clientId);
            Il2CppSystem.Collections.Generic.List<InnerNetObject> obj = __instance.allObjects;
            lock (obj)
            {
                HashSet<GameObject> hashSet = new();
                for (; index < __instance.allObjects.Count; index++)
                {
                    //本来はSerialize後のサイズ確認してダメそうならbreakするべきだが、そのためのコストがかなり大きいのである程度余裕を持ったサイズで適当に区切ることにする
                    //(Serializeすると500byte程度になるような巨大なObjectがない限りは大丈夫なはず)
                    if (messageWriter.Length > 1000) break;

                    InnerNetObject innerNetObject = __instance.allObjects[index];
                    if (innerNetObject && (innerNetObject.OwnerId != -4 || __instance.AmModdedHost) && hashSet.Add(innerNetObject.gameObject))
                    {
                        if (innerNetObject.Pointer == GameManager.Instance.Pointer)
                        {
                            __instance.SendGameManager(clientId, innerNetObject.CastFast<GameManager>());
                        }
                        else
                        {
                            __instance.WriteSpawnMessage(innerNetObject, innerNetObject.OwnerId, innerNetObject.SpawnFlags, messageWriter);
                        }
                    }
                }
            }
            messageWriter.EndMessage();
            __instance.SendOrDisconnect(messageWriter);
            messageWriter.Recycle();

            if (index < __instance.allObjects.Count) SendData(__instance, clientId, index);
        }
    }
}