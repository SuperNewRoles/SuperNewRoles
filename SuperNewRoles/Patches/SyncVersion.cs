using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using HarmonyLib;
using Hazel;
using InnerNet;
using SuperNewRoles.Modules;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace SuperNewRoles.Patches;

public enum SyncErrorType
{
    NotMismatch,
    VersionMismatch,
    RpcMapMismatch,
    HashMismatch,
    NotInstalled,
    AmongUsVersionMismatch,
}
public static class SyncVersion
{
    public const float TEXT_BASE_SCALE = 0.06f;
    public const float TEXT_BASE_SIZE = 1.8f;
    public const float ERROR_TEXT_Y_POSITION = 0.25f;
    private const float SYNC_RETRY_DELAY = 0.5f;
    private const float PLAYER_JOIN_SYNC_DELAY = 1f;
    private const float SYNC_SHOWERROR_DELAY = 1f;
    private const int MAX_RETRY_COUNT = 100;

    /// <summary>古いSNRは末尾 int なし。以降 int（GetBroadcastVersion）、任意で表示用文字列。</summary>
    private readonly record struct SyncVersionPayload(
        byte PlayerId,
        string Version,
        string Hash,
        string RpcMap,
        int BroadcastVersion,
        bool BroadcastPresent,
        string AmongUsDisplayVersion,
        bool DisplayVersionPresent);

    public static class VersionData
    {
        public static Dictionary<byte, SyncErrorType> IsError { get; } = new();
        public static Dictionary<byte, string> VersionMap { get; } = new();
        /// <summary>各プレイヤーが報告した RpcMap ハッシュ（再同期・ホスト比較用）</summary>
        public static Dictionary<byte, string> RpcMapByPlayer { get; } = new();
        /// <summary>各プレイヤーが報告した Among Us バニラ GetBroadcastVersion()</summary>
        public static Dictionary<byte, int> AmongUsBroadcastByPlayer { get; } = new();
        /// <summary>各プレイヤーが報告した Among Us 表示バージョン（VersionShower と同様 v + Application.version）</summary>
        public static Dictionary<byte, string> AmongUsDisplayVersionByPlayer { get; } = new();
        /// <summary>各プレイヤーが報告した DLL ハッシュ</summary>
        public static Dictionary<byte, string> HashByPlayer { get; } = new();
        public static string CurrentHash { get; private set; } = "";
        public static string CurrentRpcMap { get; private set; } = "";

        public static void Initialize(string hash, string rpcMap)
        {
            CurrentHash = hash;
            CurrentRpcMap = rpcMap;
        }

        public static void ClearTrackedState()
        {
            IsError.Clear();
            VersionMap.Clear();
            RpcMapByPlayer.Clear();
            AmongUsBroadcastByPlayer.Clear();
            AmongUsDisplayVersionByPlayer.Clear();
            HashByPlayer.Clear();
        }
    }

    private static int LocalBroadcastVersion => Constants.GetBroadcastVersion();

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
        var rpcMapBuilder = new StringBuilder();
        foreach (var method in CustomRPCManager.RpcMethods.OrderBy(m => m.Key))
        {
            AppendMethodInfo(rpcMapBuilder, method);
        }
        return ModHelpers.HashMD5(rpcMapBuilder.ToString());
    }
    private static void AppendMethodInfo(StringBuilder builder, KeyValuePair<int, MethodInfo> method)
    {
        builder.Append(method.Key)
               .Append(':')
               .Append(CustomRPCManager.GetStableMethodSignature(method.Value))
               .Append(';');
    }

    public static void ReceivedSyncVersion(MessageReader reader)
    {
        SyncVersionPayload payload = ReadSyncPayload(reader);
        if (!TryGetPlayerControl(payload.PlayerId, out PlayerControl player)) return;

        ApplyPayloadToVersionData(payload);

        string logDisplay = SanitizeAmongUsDisplayVersion(payload.AmongUsDisplayVersion);
        Logger.Info($"Received SyncVersion from {player.Data.PlayerName}: SNR={payload.Version} AU={(payload.BroadcastPresent ? payload.BroadcastVersion.ToString() : "legacy")} display={logDisplay}");

        if (player.PlayerId == PlayerControl.LocalPlayer.PlayerId)
        {
            VersionData.IsError[payload.PlayerId] = SyncErrorType.NotMismatch;
            VersionData.AmongUsBroadcastByPlayer[payload.PlayerId] = LocalBroadcastVersion;
            VersionData.AmongUsDisplayVersionByPlayer[payload.PlayerId] = GetAmongUsDisplayVersion();
            return;
        }

        SyncErrorType errorType = ValidateSyncPayload(payload);
        VersionData.IsError[payload.PlayerId] = errorType;
        LogValidationResult(player, errorType, payload);
    }

    private static SyncVersionPayload ReadSyncPayload(MessageReader reader)
    {
        byte playerId = reader.ReadByte();
        string version = reader.ReadString();
        string hash = reader.ReadString();
        string rpcMap = reader.ReadString();
        bool broadcastPresent = reader.Length - reader.Position >= 4;
        int broadcastVersion = broadcastPresent ? reader.ReadInt32() : 0;
        bool displayVersionPresent = reader.Position < reader.Length;
        string amongUsDisplayVersion = displayVersionPresent ? reader.ReadString() : string.Empty;
        return new SyncVersionPayload(playerId, version, hash, rpcMap, broadcastVersion, broadcastPresent, amongUsDisplayVersion, displayVersionPresent);
    }

    private static void ApplyPayloadToVersionData(SyncVersionPayload p)
    {
        VersionData.VersionMap[p.PlayerId] = p.Version;
        VersionData.RpcMapByPlayer[p.PlayerId] = p.RpcMap;
        VersionData.HashByPlayer[p.PlayerId] = p.Hash;
        if (p.BroadcastPresent)
            VersionData.AmongUsBroadcastByPlayer[p.PlayerId] = p.BroadcastVersion;
        else
            VersionData.AmongUsBroadcastByPlayer.Remove(p.PlayerId);

        if (p.DisplayVersionPresent && !string.IsNullOrEmpty(p.AmongUsDisplayVersion))
        {
            string sanitized = SanitizeAmongUsDisplayVersion(p.AmongUsDisplayVersion);
            if (!string.IsNullOrEmpty(sanitized))
                VersionData.AmongUsDisplayVersionByPlayer[p.PlayerId] = sanitized;
            else
                VersionData.AmongUsDisplayVersionByPlayer.Remove(p.PlayerId);
        }
        else
            VersionData.AmongUsDisplayVersionByPlayer.Remove(p.PlayerId);
    }

    internal static bool TryGetPlayerControl(byte playerId, out PlayerControl player)
    {
        player = PlayerControl.AllPlayerControls.ToArray().FirstOrDefault(p => p.PlayerId == playerId);
        return player != null;
    }

    private static SyncErrorType ValidateSyncPayload(SyncVersionPayload data)
    {
        if (data.Version != VersionInfo.VersionString)
            return SyncErrorType.VersionMismatch;
        if (data.Hash != VersionData.CurrentHash)
            return SyncErrorType.HashMismatch;
        if (data.RpcMap != VersionData.CurrentRpcMap)
            return SyncErrorType.RpcMapMismatch;
        if (data.BroadcastPresent
            && !Statics.AreAmongUsBroadcastVersionsCompatible(LocalBroadcastVersion, data.BroadcastVersion))
            return SyncErrorType.AmongUsVersionMismatch;
        return SyncErrorType.NotMismatch;
    }

    private static void LogValidationResult(PlayerControl player, SyncErrorType errorType, SyncVersionPayload data)
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
        else if (errorType == SyncErrorType.AmongUsVersionMismatch)
        {
            Logger.Info($"Error Among Us version: {player.Data.PlayerName} reported={data.AmongUsDisplayVersion} int={data.BroadcastVersion}");
        }
        else if (errorType == SyncErrorType.NotInstalled)
        {
            Logger.Info($"Not Received Version: {player.Data.PlayerName}");
        }
    }
    public static void SendSyncVersion()
    {
        if (PlayerControl.LocalPlayer == null) return;
        byte localId = PlayerControl.LocalPlayer.PlayerId;
        string auDisplay = GetAmongUsDisplayVersion();
        VersionData.VersionMap[localId] = VersionInfo.VersionString;
        VersionData.RpcMapByPlayer[localId] = VersionData.CurrentRpcMap;
        VersionData.HashByPlayer[localId] = VersionData.CurrentHash;
        VersionData.AmongUsBroadcastByPlayer[localId] = LocalBroadcastVersion;
        VersionData.AmongUsDisplayVersionByPlayer[localId] = auDisplay;
        VersionData.IsError[localId] = SyncErrorType.NotMismatch;

        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPCManager.SNRSyncVersionRpc, SendOption.Reliable, -1);
        writer.Write(localId);
        writer.Write(VersionInfo.VersionString);
        writer.Write(VersionData.CurrentHash);
        writer.Write(VersionData.CurrentRpcMap);
        writer.Write(LocalBroadcastVersion);
        writer.Write(auDisplay);
        AmongUsClient.Instance.FinishRpcImmediately(writer);
    }

    /// <summary>ゲームが表示に使う Among Us バージョン（ReferenceData の userFacingVersion）。表示は <see cref="SanitizeAmongUsDisplayVersion"/> 済み。</summary>
    public static string GetAmongUsDisplayVersion()
    {
        string raw;
        try
        {
            var rdm = FastDestroyableSingleton<ReferenceDataManager>.Instance;
            if (rdm?.Refdata == null)
                raw = RawFallbackAmongUsDisplayVersion();
            else
            {
                string u = rdm.Refdata.userFacingVersion;
                raw = string.IsNullOrEmpty(u) ? RawFallbackAmongUsDisplayVersion() : u;
            }
        }
        catch
        {
            raw = RawFallbackAmongUsDisplayVersion();
        }
        return SanitizeAmongUsDisplayVersion(raw);
    }

    private static string RawFallbackAmongUsDisplayVersion() => "v" + Application.version;

    /// <summary>
    /// 表示用 Among Us バージョン。小文字の v・数字・. のみ残し、それ以外は除去する。残らなければ空文字。
    /// 不正な文章が表示されることの対策として。
    /// </summary>
    public static string SanitizeAmongUsDisplayVersion(string raw)
    {
        if (string.IsNullOrEmpty(raw)) return string.Empty;
        StringBuilder sb = new(raw.Length);
        foreach (char c in raw)
        {
            if (c == 'v' || c == '.' || (c >= '0' && c <= '9'))
                sb.Append(c);
        }
        return sb.ToString();
    }

    /// <summary>
    /// クライアントがホスト報告の SNR / RpcMap / AU / ハッシュと一致しているか。
    /// </summary>
    public static bool LocalIsOutOfSyncWithHost()
    {
        if (AmongUsClient.Instance.AmHost) return false;
        if (GameData.Instance == null || PlayerControl.LocalPlayer == null) return false;
        NetworkedPlayerInfo host = GameData.Instance.GetHost();
        if (host == null || host.Disconnected) return false;
        byte hostId = host.PlayerId;
        if (!VersionData.VersionMap.TryGetValue(hostId, out string hostSnr) || string.IsNullOrEmpty(hostSnr))
            return false;
        if (VersionInfo.VersionString != hostSnr)
            return true;
        if (!VersionData.RpcMapByPlayer.TryGetValue(hostId, out string hostRpc) || string.IsNullOrEmpty(hostRpc))
            return false;
        if (VersionData.CurrentRpcMap != hostRpc)
            return true;
        if (!VersionData.HashByPlayer.TryGetValue(hostId, out string hostHash) || string.IsNullOrEmpty(hostHash))
            return false;
        if (VersionData.CurrentHash != hostHash)
            return true;
        if (!VersionData.AmongUsBroadcastByPlayer.TryGetValue(hostId, out int hostAu))
            return false;
        if (!Statics.AreAmongUsBroadcastVersionsCompatible(LocalBroadcastVersion, hostAu))
            return true;
        return false;
    }

    /// <summary>Among Us の GetBroadcastVersion を表示用 v年.月.日.改訂 に変換</summary>
    public static string FormatAmongUsBroadcastVersion(int broadcastVersion)
    {
        int y = broadcastVersion / 25000;
        broadcastVersion -= y * 25000;
        int m = broadcastVersion / 1800;
        broadcastVersion -= m * 1800;
        int d = broadcastVersion / 50;
        broadcastVersion -= d * 50;
        int rev = broadcastVersion;
        return $"v{y}.{m}.{d}.{rev}";
    }

    /// <summary>
    /// ホストがオンラインロビーでゲームを開始してよいか。
    /// ハッシュ不一致は開始を妨げない（たまにリリースミスるので例外）。
    /// </summary>
    public static bool CanHostStartGame()
    {
        if (GameData.Instance == null) return true;
        foreach (NetworkedPlayerInfo p in GameData.Instance.AllPlayers)
        {
            if (p == null || p.Disconnected) continue;
            byte id = p.PlayerId;
            if (!VersionData.VersionMap.TryGetValue(id, out string snr) || string.IsNullOrEmpty(snr))
                return false;
            if (!VersionData.IsError.TryGetValue(id, out SyncErrorType err))
                return false;
            if (err == SyncErrorType.NotMismatch || err == SyncErrorType.HashMismatch)
                continue;
            return false;
        }
        return true;
    }

    /// <summary>
    /// バニラ Update が人数変更時に SetButtonEnableState(人数のみ) する・他 Postfix の順で無効化が上書きされるため、
    /// BeginGame 本体を止めるのが確実（OldSNR 系と同様）。
    /// </summary>
    [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.BeginGame))]
    public static class GameStartManagerBeginGameSyncVersionPatch
    {
        public static bool Prefix(GameStartManager __instance)
        {
            if (!AmongUsClient.Instance.AmHost) return true;
            if (AmongUsClient.Instance.NetworkMode is NetworkModes.LocalGame or NetworkModes.FreePlay) return true;
            if (AmongUsClient.Instance.NetworkMode != NetworkModes.OnlineGame) return true;
            if (CanHostStartGame()) return true;
            if (GameData.Instance != null && GameData.Instance.PlayerCount >= __instance.MinPlayers)
                __instance.StartCoroutine(Effects.SwayX(__instance.PlayerCounter.transform));
            return false;
        }
    }

    /// <summary>
    /// HelpMenu 等の Postfix の後でも、バージョン不一致なら開始ボタンを必ず無効化する。
    /// </summary>
    [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Update))]
    [HarmonyAfter("SuperNewRoles.HelpMenus.HelpMenuObjectManager+GameStartManagerUpdatePatch")]
    public static class GameStartManagerUpdateSyncVersionEnforcePatch
    {
        public static void Postfix(GameStartManager __instance)
        {
            if (!AmongUsClient.Instance.AmHost) return;
            if (GameData.Instance == null) return;
            if (AmongUsClient.Instance.NetworkMode is NetworkModes.LocalGame or NetworkModes.FreePlay) return;
            if (AmongUsClient.Instance.NetworkMode != NetworkModes.OnlineGame) return;
            if (__instance.StartButton == null || !__instance.StartButton.gameObject.activeSelf) return;
            if (GameData.Instance.PlayerCount < __instance.MinPlayers) return;
            if (CanHostStartGame()) return;
            __instance.StartButton.SetButtonEnableState(false);
            if (__instance.StartButtonGlyph != null)
                __instance.StartButtonGlyph.SetColor(Palette.DisabledClear);
        }
    }

    [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Start))]
    public static class GameStartManagerStartPatch
    {
        public static void Postfix()
        {
            VersionData.ClearTrackedState();
            RetryManager.CreateRetryTask(
                () => PlayerControl.LocalPlayer != null,
                SendSyncVersion,
                MAX_RETRY_COUNT,
                SYNC_RETRY_DELAY
            );
            new LateTask(() => SendSyncVersion(), 5f);
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
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Start))]
    public static class SyncVersionHudManagerStartPatch
    {
        public static void Postfix(HudManager __instance)
        {
            SyncVersionErrorHandler.CreateResyncButton(__instance);
        }
    }

    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    public static class SyncVersionHudManagerUpdatePatch
    {
        public static void Postfix(HudManager __instance)
        {
            SyncVersionErrorHandler.UpdateErrorDisplay(__instance);
            SyncVersionErrorHandler.UpdateResyncButton(__instance);
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
    private const float ResyncCooldownSeconds = 0.25f;
    private static float _lastResyncButtonTime = float.NegativeInfinity;

    private static readonly StringBuilder ErrorMessageBuilder = new();
    private static TextMeshPro ErrorText;
    private static readonly List<string> ErrorCache = new();
    private static GameObject ResyncButtonRoot;

    public static void CreateResyncButton(HudManager hudManager)
    {
        if (ResyncButtonRoot != null) return;
        Sprite sprite = AssetManager.GetAsset<Sprite>("ReloadVersion");
        if (sprite == null)
        {
            Logger.Warning("SyncVersion: ReloadVersion asset not found; resync button skipped.");
            return;
        }

        ResyncButtonRoot = new GameObject();
        ResyncButtonRoot.transform.SetParent(hudManager.transform);
        ResyncButtonRoot.name = "SNRSyncVersionResyncButton";
        ResyncButtonRoot.transform.localScale = Vector3.one * 0.35f;

        ResyncButtonRoot.AddComponent<SpriteRenderer>().sprite = sprite;

        Transform badge = ResyncButtonRoot.transform.Find("badge");
        if (badge != null) badge.gameObject.SetActive(false);

        PassiveButton passiveButton = ResyncButtonRoot.AddComponent<PassiveButton>();
        CircleCollider2D collider = ResyncButtonRoot.AddComponent<CircleCollider2D>();
        collider.offset = sprite.bounds.center;
        collider.radius = Mathf.Max(sprite.bounds.extents.x, sprite.bounds.extents.y);
        passiveButton.Colliders = new Collider2D[] { collider };
        passiveButton.OnClick = new();
        passiveButton.OnMouseOut = new();
        passiveButton.OnMouseOver = new();
        passiveButton.OnClick.AddListener((UnityAction)OnResyncButtonClicked);

        AspectPosition aspectPosition = ResyncButtonRoot.AddComponent<AspectPosition>();
        aspectPosition.Alignment = AspectPosition.EdgeAlignments.Bottom;
        aspectPosition.DistanceFromEdge = new Vector3(0f, 0.2f, -25f);
        aspectPosition.OnEnable();

        if (hudManager.roomTracker != null && hudManager.roomTracker.text != null)
        {
            TextMeshPro label = GameObject.Instantiate(hudManager.roomTracker.text);
            label.name = "ResyncLabel";
            GameObject.Destroy(label.GetComponent<RoomTracker>());
            label.transform.SetParent(ResyncButtonRoot.transform, false);
            label.transform.localPosition = new Vector3(0f, -1.25f, -1f);
            label.transform.localScale = Vector3.one * 2.4f;
            label.text = ModTranslation.GetString("SyncVersion_ResyncButton");
            label.alignment = TextAlignmentOptions.Center;
            label.color = Color.white;
            label.fontSizeMin = 1.2f;
            label.enableWordWrapping = false;
        }

        ResyncButtonRoot.SetActive(false);
    }

    private static void OnResyncButtonClicked()
    {
        if (Time.time - _lastResyncButtonTime < ResyncCooldownSeconds)
            return;
        _lastResyncButtonTime = Time.time;
        SyncVersion.SendSyncVersion();
    }

    public static void UpdateResyncButton(HudManager hudManager)
    {
        if (ResyncButtonRoot == null) return;
        ResyncButtonRoot.SetActive(ShouldShowResyncButton());
    }

    private static bool ShouldShowResyncButton()
    {
        if (AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Joined)
            return false;
        if (PlayerControl.LocalPlayer == null)
            return false;
        if (AmongUsClient.Instance.AmHost)
            return HasAnyDisplayedSyncError();

        byte localId = PlayerControl.LocalPlayer.PlayerId;
        if (SyncVersion.LocalIsOutOfSyncWithHost())
            return true;
        return HasAnyDisplayedSyncError()
            && SyncVersion.VersionData.IsError.TryGetValue(localId, out SyncErrorType err)
            && err != SyncErrorType.NotMismatch;
    }

    private static bool HasAnyDisplayedSyncError()
    {
        foreach (var kvp in SyncVersion.VersionData.IsError)
        {
            if (CreateErrorMessage(kvp) != null)
                return true;
        }
        return false;
    }

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

    private static string GetAmongUsVersionMismatchDisplay(byte playerId)
    {
        if (SyncVersion.VersionData.AmongUsDisplayVersionByPlayer.TryGetValue(playerId, out string disp) && !string.IsNullOrEmpty(disp))
            return SyncVersion.SanitizeAmongUsDisplayVersion(disp);
        if (SyncVersion.VersionData.AmongUsBroadcastByPlayer.TryGetValue(playerId, out int au))
            return SyncVersion.SanitizeAmongUsDisplayVersion(SyncVersion.FormatAmongUsBroadcastVersion(au));
        return "";
    }

    private static string CreateErrorMessage(KeyValuePair<byte, SyncErrorType> kvp)
    {
        if (!SyncVersion.TryGetPlayerControl(kvp.Key, out PlayerControl player))
            return null;

        return kvp.Value switch
        {
            SyncErrorType.VersionMismatch => ModTranslation.GetString("SyncError_VersionMismatch")
                .Replace("{player}", player.Data.PlayerName)
                .Replace("{version}", SyncVersion.VersionData.VersionMap.TryGetValue(kvp.Key, out var version) ? version : ""),
            SyncErrorType.HashMismatch => ModTranslation.GetString("SyncError_HashMismatch")
                .Replace("{player}", player.Data.PlayerName),
            SyncErrorType.RpcMapMismatch => ModTranslation.GetString("SyncError_RpcMapMismatch")
                .Replace("{player}", player.Data.PlayerName),
            SyncErrorType.AmongUsVersionMismatch => ModTranslation.GetString("SyncError_AmongUsVersionMismatch")
                .Replace("{player}", player.Data.PlayerName)
                .Replace("{version}", GetAmongUsVersionMismatchDisplay(kvp.Key)),
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
