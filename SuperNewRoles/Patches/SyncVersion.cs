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
    public static void Load()
    {
        // dllのパスを取得して読み込み、ハッシュ化する
        string dllPath = Assembly.GetExecutingAssembly().Location;
        // Load file bytes
        byte[] bytes = File.ReadAllBytes(dllPath);
        CurrentHash = ModHelpers.HashMD5(bytes);
        // ランダムなテキストをCurrentRpcMapに設定
        CurrentRpcMap = Guid.NewGuid().ToString(); // ランダムなGUIDを生成して設定
        Logger.Info($"CurrentRpcMap: {CurrentRpcMap}");
        string rpcMapBase = "";
        // IdとMethod名とMethodの引数一覧のハッシュ
        foreach (var method in CustomRPCManager.RpcMethods)
        {
            string methodName = method.Value.Name;
            var parameters = method.Value.GetParameters();
            string parameterList = string.Join(",", parameters.Select(p => p.ParameterType.Name));
            rpcMapBase += $"{method.Key}:{methodName}({parameterList});";
        }
        // CurrentRpcMap = ModHelpers.HashMD5(rpcMapBase);
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
        public static TextMeshPro ErrorText;
        public static void Postfix(HudManager __instance)
        {
            if (!GameStartManager.InstanceExists)
            {
                ErrorText.gameObject.SetActive(false);
                return;
            }
            // IsErrorを元にエラー文を生成
            List<string> errors = new();
            foreach (var kvp in IsError)
            {
                PlayerControl player = ModHelpers.GetPlayerById(kvp.Key);
                if (player == null || kvp.Value == SyncErrorType.NotMismatch) continue;

                string errorMessage = kvp.Value switch
                {
                    SyncErrorType.VersionMismatch => ModTranslation.GetString("SyncError_VersionMismatch").Replace("{player}", player.Data.PlayerName).Replace("{version}", VersionMap[kvp.Key]),
                    SyncErrorType.HashMismatch => ModTranslation.GetString("SyncError_HashMismatch").Replace("{player}", player.Data.PlayerName),
                    SyncErrorType.RpcMapMismatch => ModTranslation.GetString("SyncError_RpcMapMismatch").Replace("{player}", player.Data.PlayerName),
                    _ => ""
                };
                if (!string.IsNullOrEmpty(errorMessage))
                    errors.Add(errorMessage);
            }
            if (errors.Count > 0)
            {
                if (ErrorText == null)
                {
                    ErrorText = GameObject.Instantiate(__instance.roomTracker.text);
                    ErrorText.name = "SyncErrorText";
                    GameObject.Destroy(ErrorText.gameObject.GetComponent<RoomTracker>());
                    ErrorText.transform.SetParent(__instance.transform);
                    // テキストの位置とスタイルを設定
                    ErrorText.transform.localPosition = new(0f, 0.25f, -19f);
                    // 明るめの赤
                    ErrorText.color = new Color(1f, 0.2f, 0f, 1f);
                    ErrorText.fontSizeMin = 1.2f;
                    ErrorText.enableWordWrapping = false;
                }
                errors.Insert(0, ModTranslation.GetString("SyncError_Title"));
                ErrorText.text = string.Join("\n", errors);
                ErrorText.gameObject.SetActive(true);
                ErrorText.transform.localScale = Vector3.one * (0.06f * errors.Count + 1.8f);
            }
            else if (ErrorText != null)
            {
                ErrorText.gameObject.SetActive(false);
            }
        }
    }
}

