using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Hazel;
using SuperNewRoles.Helpers;
using SuperNewRoles.Modules;
using TMPro;
using UnityEngine;

namespace SuperNewRoles.Patches;

public static class SyncVersion
{
    public enum SyncErrorType
    {
        NotMismatch,
        VersionMismatch,
        RpcMapMismatch,
        HashMismatch
    }
    public static Dictionary<byte, SyncErrorType> IsError = new();
    public static Dictionary<byte, string> VersionMap = new();
    public static string CurrentHash = "";
    public static string CurrentRpcMap = "";
    private const float TextBaseScale = 0.06f;
    private const float TextBaseSize = 1.8f;
    private const float ErrorTextYPosition = 0.25f;
    public static void Load()
    {
        string dllPath = Assembly.GetExecutingAssembly().Location;
        byte[] bytes = File.ReadAllBytes(dllPath);
        CurrentHash = ModHelpers.HashMD5(bytes);

        CurrentRpcMap = GenerateRpcMapBase();
        Logger.Info($"CurrentRpcMap: {CurrentRpcMap}");
    }
    private static string GenerateRpcMapBase()
    {
        var rpcMapBuilder = new System.Text.StringBuilder();
        foreach (var method in CustomRPCManager.RpcMethods)
        {
            var parameters = method.Value.GetParameters();
            rpcMapBuilder.Append(method.Key)
                        .Append(':')
                        .Append(method.Value.Name)
                        .Append('(')
                        .Append(string.Join(",", parameters.Select(p => p.ParameterType.Name)))
                        .Append(");");
        }
        return ModHelpers.HashMD5(rpcMapBuilder.ToString());
    }
    public static void ReceivedSyncVersion(MessageReader reader)
    {
        byte playerId = reader.ReadByte();
        string version = reader.ReadString();
        string hash = reader.ReadString();
        PlayerControl player = ModHelpers.GetPlayerById(playerId);
        if (player == null) return;
        VersionMap[playerId] = version;
        Logger.Info($"Received SyncVersion from {player.Data.PlayerName}: {version} {hash}");
        if (version != VersionInfo.VersionString)
        {
            IsError[playerId] = SyncErrorType.VersionMismatch;
            Logger.Info($"Error Version: {player.Data.PlayerName}");
            if (hash == CurrentHash)
                Logger.Info("Hmm... Version is different, but hash is same.");
            return;
        }
        if (hash != CurrentHash)
        {
            IsError[playerId] = SyncErrorType.HashMismatch;
            Logger.Info($"Error Hash: {player.Data.PlayerName}");
            return;
        }
        string RpcMap = reader.ReadString();
        Logger.Info($"Received RpcMap from {player.Data.PlayerName}: {RpcMap}");
        if (RpcMap != CurrentRpcMap)
        {
            IsError[playerId] = SyncErrorType.RpcMapMismatch;
            Logger.Info($"Error Rpc Map: {player.Data.PlayerName}");
            return;
        }
        IsError[playerId] = SyncErrorType.NotMismatch;
    }
    public static void SendSyncVersion()
    {
        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPCManager.SNRSyncVersionRpc, SendOption.Reliable, -1);
        writer.Write(PlayerControl.LocalPlayer.PlayerId);
        writer.Write(VersionInfo.VersionString);
        writer.Write(CurrentHash);
        writer.Write(CurrentRpcMap);
        AmongUsClient.Instance.FinishRpcImmediately(writer);
    }
    [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Start))]
    public static class GameStartManagerStartPatch
    {
        public static void Postfix()
        {
            new LateTask(() => SendSyncVersion(), 1f);
        }
    }
    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnPlayerJoined))]
    public class AmongUsClientOnPlayerJoinedPatch
    {
        public static void Postfix(InnerNet.ClientData data)
        {
            if (PlayerControl.LocalPlayer != null)
                new LateTask(() => SendSyncVersion(), 1f);
            new LateTask(() =>
            {
                IsError[data.Character.PlayerId] = SyncErrorType.NotMismatch;
                VersionMap[data.Character.PlayerId] = "";
            }, 1f);
        }
    }
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    public static class SyncVersionHudManagerUpdatePatch
    {
        private static readonly System.Text.StringBuilder ErrorMessageBuilder = new();
        private static string CreateErrorMessage(KeyValuePair<byte, SyncErrorType> kvp)
        {
            PlayerControl player = ModHelpers.GetPlayerById(kvp.Key);
            if (player == null) return null;

            return kvp.Value switch
            {
                SyncErrorType.VersionMismatch => ModTranslation.GetString("SyncError_VersionMismatch")
                    .Replace("{player}", player.Data.PlayerName)
                    .Replace("{version}", VersionMap.TryGetValue(kvp.Key, out var version) ? version : ""),
                SyncErrorType.HashMismatch => ModTranslation.GetString("SyncError_HashMismatch")
                    .Replace("{player}", player.Data.PlayerName),
                SyncErrorType.RpcMapMismatch => ModTranslation.GetString("SyncError_RpcMapMismatch")
                    .Replace("{player}", player.Data.PlayerName),
                _ => null
            };
        }
        private static void InitializeErrorText(HudManager instance)
        {
            if (ErrorText != null) return;

            ErrorText = GameObject.Instantiate(instance.roomTracker.text);
            ErrorText.name = "SyncErrorText";
            GameObject.Destroy(ErrorText.gameObject.GetComponent<RoomTracker>());

            ErrorText.transform.SetParent(instance.transform);
            ErrorText.transform.localPosition = new Vector3(0f, ErrorTextYPosition, -19f);
            ErrorText.color = new Color(1f, 0.2f, 0f, 1f);
            ErrorText.fontSizeMin = 1.2f;
            ErrorText.enableWordWrapping = false;
        }
        public static TextMeshPro ErrorText;
        private static List<string> errorCache = new();
        public static void Postfix(HudManager __instance)
        {
            if (!GameStartManager.InstanceExists)
            {
                if (ErrorText != null) ErrorText.gameObject.SetActive(false);
                return;
            }

            errorCache.Clear();
            foreach (var error in IsError)
            {
                var errorMessage = CreateErrorMessage(error);
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    errorCache.Add(errorMessage);
                }
            }

            if (errorCache.Count > 0)
            {
                InitializeErrorText(__instance);

                ErrorMessageBuilder.Clear();
                ErrorMessageBuilder.AppendLine(ModTranslation.GetString("SyncError_Title"));
                foreach (var error in errorCache)
                {
                    ErrorMessageBuilder.AppendLine(error);
                }

                ErrorText.text = ErrorMessageBuilder.ToString();
                ErrorText.gameObject.SetActive(true);
                ErrorText.transform.localScale = Vector3.one * (TextBaseScale * (errorCache.Count + 1) + TextBaseSize);
            }
            else if (ErrorText != null)
            {
                ErrorText.gameObject.SetActive(false);
            }
        }
    }
}

