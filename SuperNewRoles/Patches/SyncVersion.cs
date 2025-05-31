using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Hazel;
using SuperNewRoles.Modules;
using TMPro;
using UnityEngine;

namespace SuperNewRoles.Patches;

public enum SyncErrorType
{
    NotMismatch,
    VersionMismatch,
    RpcMapMismatch,
    HashMismatch,
    NotInstalled,
}
public static class SyncVersion
{
    public const float TEXT_BASE_SCALE = 0.06f;
    public const float TEXT_BASE_SIZE = 1.8f;
    public const float ERROR_TEXT_Y_POSITION = 0.25f;
    private const float SYNC_RETRY_DELAY = 0.5f;
    private const float PLAYER_JOIN_SYNC_DELAY = 1f;
    private const float SYNC_SHOWERROR_DELAY = 1f;
    private const int MAX_RETRY_COUNT = 5;


    public static class VersionData
    {
        public static Dictionary<byte, SyncErrorType> IsError { get; } = new();
        public static Dictionary<byte, string> VersionMap { get; } = new();
        public static string CurrentHash { get; private set; } = "";
        public static string CurrentRpcMap { get; private set; } = "";

        public static void Initialize(string hash, string rpcMap)
        {
            CurrentHash = hash;
            CurrentRpcMap = rpcMap;
        }
    }

    public static void Load()
    {
        string dllPath = SuperNewRolesPlugin.Assembly.Location;
        byte[] bytes = File.ReadAllBytes(dllPath);
        string hash = ModHelpers.HashMD5(bytes);
        string rpcMap = GenerateRpcMap();
        VersionData.Initialize(hash, rpcMap);
        Logger.Info($"CurrentRpcMap: {VersionData.CurrentRpcMap}");
    }
    private static string GenerateRpcMap()
    {
        var rpcMapBuilder = new System.Text.StringBuilder();
        foreach (var method in CustomRPCManager.RpcMethods)
        {
            AppendMethodInfo(rpcMapBuilder, method);
        }
        return ModHelpers.HashMD5(rpcMapBuilder.ToString());
    }
    private static void AppendMethodInfo(System.Text.StringBuilder builder, KeyValuePair<byte, MethodInfo> method)
    {
        var parameters = method.Value.GetParameters();
        builder.Append(method.Key)
               .Append(':')
               .Append(method.Value.Name)
               .Append('(')
               .Append(string.Join(",", parameters.Select(p => p.ParameterType.Name)))
               .Append(");");
    }
    public static void ReceivedSyncVersion(MessageReader reader)
    {
        var syncData = ReadSyncData(reader);
        if (!ValidatePlayer(syncData.PlayerId, out var player)) return;

        VersionData.VersionMap[syncData.PlayerId] = syncData.Version;
        Logger.Info($"Received SyncVersion from {player.Data.PlayerName}: {syncData.Version} {syncData.Hash}");

        var errorType = ValidateSyncData(syncData);
        VersionData.IsError[syncData.PlayerId] = errorType;

        LogValidationResult(player, errorType, syncData);
    }
    private static (byte PlayerId, string Version, string Hash, string RpcMap) ReadSyncData(MessageReader reader)
    {
        return (
            reader.ReadByte(),
            reader.ReadString(),
            reader.ReadString(),
            reader.ReadString()
        );
    }
    private static bool ValidatePlayer(byte playerId, out PlayerControl player)
    {
        player = PlayerControl.AllPlayerControls.ToArray().FirstOrDefault(p => p.PlayerId == playerId);
        return player != null;
    }
    private static SyncErrorType ValidateSyncData((byte PlayerId, string Version, string Hash, string RpcMap) data)
    {
        if (data.Version != VersionInfo.VersionString)
            return SyncErrorType.VersionMismatch;
        if (data.Hash != VersionData.CurrentHash)
            return SyncErrorType.HashMismatch;
        if (data.RpcMap != VersionData.CurrentRpcMap)
            return SyncErrorType.RpcMapMismatch;
        return SyncErrorType.NotMismatch;
    }
    private static void LogValidationResult(PlayerControl player, SyncErrorType errorType, (byte PlayerId, string Version, string Hash, string RpcMap) data)
    {
        if (errorType == SyncErrorType.VersionMismatch)
        {
            Logger.Info($"Error Version: {player.Data.PlayerName}");
            if (data.Hash == VersionData.CurrentHash)
                Logger.Info("Hmm... Version is different, but hash is same.");
        }
        else if (errorType == SyncErrorType.HashMismatch)
        {
            Logger.Info($"Error Hash: {player.Data.PlayerName}");
        }
        else if (errorType == SyncErrorType.RpcMapMismatch)
        {
            Logger.Info($"Error Rpc Map: {player.Data.PlayerName}");
        }
        else if (errorType == SyncErrorType.NotInstalled)
        {
            Logger.Info($"Not Received Version: {player.Data.PlayerName}");
        }
    }
    public static void SendSyncVersion()
    {
        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPCManager.SNRSyncVersionRpc, SendOption.Reliable, -1);
        writer.Write(PlayerControl.LocalPlayer.PlayerId);
        writer.Write(VersionInfo.VersionString);
        writer.Write(VersionData.CurrentHash);
        writer.Write(VersionData.CurrentRpcMap);
        AmongUsClient.Instance.FinishRpcImmediately(writer);
    }
    [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Start))]
    public static class GameStartManagerStartPatch
    {
        public static void Postfix()
        {
            RetryManager.CreateRetryTask(
                () => PlayerControl.LocalPlayer != null,
                SendSyncVersion,
                MAX_RETRY_COUNT,
                SYNC_RETRY_DELAY
            );
            RoleOptionManager.DelayedSyncTasks.Clear();

        }
    }
    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnPlayerJoined))]
    public static class AmongUsClientOnPlayerJoinedPatch
    {
        public static void Postfix(InnerNet.ClientData data)
        {
            if (PlayerControl.LocalPlayer != null)
            {
                new LateTask(() => SendSyncVersion(), PLAYER_JOIN_SYNC_DELAY);
            }

            HandleNewPlayer(data);
        }

        private static void HandleNewPlayer(InnerNet.ClientData data)
        {
            int clientId = data.Id;
            RetryManager.CreateRetryTask(
                () => data.Character != null,
                () => InitializePlayerVersion(data, clientId),
                MAX_RETRY_COUNT,
                SYNC_RETRY_DELAY
            );
        }

        private static void InitializePlayerVersion(InnerNet.ClientData data, int clientId)
        {
            if (clientId != data.Id) return;

            byte playerId = data.Character.PlayerId;
            VersionData.IsError[playerId] = SyncErrorType.NotMismatch;
            VersionData.VersionMap[playerId] = "";

            new LateTask(() => CheckPlayerVersion(data, clientId, playerId), SYNC_SHOWERROR_DELAY);
        }

        private static void CheckPlayerVersion(InnerNet.ClientData data, int clientId, byte playerId)
        {
            if (clientId != data.Id) return;
            if (!VersionData.VersionMap.ContainsKey(playerId) || string.IsNullOrEmpty(VersionData.VersionMap[playerId]))
            {
                VersionData.IsError[playerId] = SyncErrorType.NotInstalled;
                Logger.Info($"Not Received Version: {data.Character.Data.PlayerName}");
            }
        }
    }
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    public static class SyncVersionHudManagerUpdatePatch
    {
        public static void Postfix(HudManager __instance)
        {
            SyncVersionErrorHandler.UpdateErrorDisplay(__instance);
        }
    }
}

internal static class RetryManager
{
    public static void CreateRetryTask(Func<bool> condition, Action action, int maxRetries, float delay)
    {
        int retryCount = maxRetries;
        void TryAction()
        {
            new LateTask(() =>
            {
                if (!condition())
                {
                    if (retryCount > 0)
                    {
                        retryCount--;
                        TryAction();
                        return;
                    }
                }
                action();
            }, delay);
        }
        TryAction();
    }
}

internal static class SyncVersionErrorHandler
{
    private static readonly System.Text.StringBuilder ErrorMessageBuilder = new();
    private static TextMeshPro ErrorText;
    private static readonly List<string> ErrorCache = new();

    public static void UpdateErrorDisplay(HudManager hudManager)
    {
        if (!ShouldDisplayErrors(hudManager)) return;

        CollectErrors();
        if (ErrorCache.Count > 0)
        {
            DisplayErrors(hudManager);
        }
        else if (ErrorText != null)
        {
            ErrorText.gameObject.SetActive(false);
        }
    }

    private static bool ShouldDisplayErrors(HudManager hudManager)
    {
        if (AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Joined)
        {
            if (ErrorText != null) ErrorText.gameObject.SetActive(false);
            return false;
        }
        return true;
    }

    private static void CollectErrors()
    {
        ErrorCache.Clear();
        foreach (var error in SyncVersion.VersionData.IsError)
        {
            var errorMessage = CreateErrorMessage(error);
            if (!string.IsNullOrEmpty(errorMessage))
            {
                ErrorCache.Add(errorMessage);
            }
        }
    }

    private static string CreateErrorMessage(KeyValuePair<byte, SyncErrorType> kvp)
    {
        PlayerControl player = PlayerControl.AllPlayerControls.ToArray().FirstOrDefault(p => p.PlayerId == kvp.Key);
        if (player == null) return null;

        return kvp.Value switch
        {
            SyncErrorType.VersionMismatch => ModTranslation.GetString("SyncError_VersionMismatch")
                .Replace("{player}", player.Data.PlayerName)
                .Replace("{version}", SyncVersion.VersionData.VersionMap.TryGetValue(kvp.Key, out var version) ? version : ""),
            SyncErrorType.HashMismatch => ModTranslation.GetString("SyncError_HashMismatch")
                .Replace("{player}", player.Data.PlayerName),
            SyncErrorType.RpcMapMismatch => ModTranslation.GetString("SyncError_RpcMapMismatch")
                .Replace("{player}", player.Data.PlayerName),
            SyncErrorType.NotInstalled => ModTranslation.GetString("SyncError_NotInstalled")
                .Replace("{player}", player.Data.PlayerName),
            _ => null
        };
    }

    private static void DisplayErrors(HudManager hudManager)
    {
        InitializeErrorText(hudManager);

        ErrorMessageBuilder.Clear();
        ErrorMessageBuilder.AppendLine(ModTranslation.GetString("SyncError_Title"));
        foreach (var error in ErrorCache)
        {
            ErrorMessageBuilder.AppendLine(error);
        }

        ErrorText.text = ErrorMessageBuilder.ToString();
        ErrorText.gameObject.SetActive(true);
        ErrorText.transform.localScale = Vector3.one * (SyncVersion.TEXT_BASE_SCALE * (ErrorCache.Count + 1) + SyncVersion.TEXT_BASE_SIZE);
    }

    private static void InitializeErrorText(HudManager instance)
    {
        if (ErrorText != null) return;

        ErrorText = GameObject.Instantiate(instance.roomTracker.text);
        ErrorText.name = "SyncErrorText";
        GameObject.Destroy(ErrorText.gameObject.GetComponent<RoomTracker>());

        ErrorText.transform.SetParent(instance.transform);
        ErrorText.transform.localPosition = new Vector3(0f, SyncVersion.ERROR_TEXT_Y_POSITION, -19f);
        ErrorText.color = new Color(1f, 0.2f, 0f, 1f);
        ErrorText.fontSizeMin = 1.2f;
        ErrorText.enableWordWrapping = false;
    }
}

