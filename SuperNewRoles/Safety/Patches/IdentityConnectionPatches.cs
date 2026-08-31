using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AmongUs.GameOptions;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using HarmonyLib;
using Hazel;
using InnerNet;
using SuperNewRoles.Modules;
using SuperNewRoles.Safety;
using SuperNewRoles.Safety.Api;
using SuperNewRoles.Safety.Identity;
using UnityEngine;
using UnityEngine.Networking;

namespace SuperNewRoles.Safety.Patches;

internal static class SafetyParticipantIds
{
    private static readonly Dictionary<int, Dictionary<int, string>> ByGame = new();
    private static readonly Queue<int> GameOrder = new();

    public static void Apply(MessageReader reader)
    {
        int gameId = reader.ReadPackedInt32();
        int count = Math.Min(reader.ReadPackedInt32(), 100);
        if (!ByGame.TryGetValue(gameId, out Dictionary<int, string> players))
        {
            players = new Dictionary<int, string>();
            ByGame[gameId] = players;
            GameOrder.Enqueue(gameId);
            while (GameOrder.Count > 8)
            {
                int expired = GameOrder.Dequeue();
                ByGame.Remove(expired);
            }
        }

        for (int i = 0; i < count; i++)
        {
            int clientId = reader.ReadPackedInt32();
            string publicId = reader.ReadString();
            if (clientId >= 0 && !string.IsNullOrEmpty(publicId))
                players[clientId] = publicId;
        }
    }

    public static string Get(int gameId, int clientId)
    {
        return ByGame.TryGetValue(gameId, out Dictionary<int, string> players)
            && players.TryGetValue(clientId, out string publicId)
            ? publicId
            : null;
    }
}

internal static class OfficialPlayGate
{
    private static bool _passthrough;
    private static bool _deferring;

    public static bool IsResuming => _passthrough;

    public static bool AllowOrDefer(MonoBehaviour host, Action resume)
    {
        if (!OfficialSnrServer.IsIdentityEnabled() || _passthrough) return true;
        if (ConductJoinDecision.ShouldBlockRepeatJoinClick(_deferring, ConductPopup.IsBusy, WarningPopup.IsOpen))
            return false;

        MonoBehaviour runner = host != null ? host : SafetyRuntime.FindCoroutineRunner(AmongUsClient.Instance);
        if (runner == null)
            runner = AmongUsClient.Instance;
        if (runner == null) return true;

        _deferring = true;
        runner.StartCoroutine(CoDefer(runner, resume).WrapToIl2Cpp());
        return false;
    }

    private static IEnumerator CoDefer(MonoBehaviour runner, Action resume)
    {
        bool allow = false;
        yield return WaitUntilAllowed(runner, value => allow = value).WrapToIl2Cpp();
        _deferring = false;
        if (!ConductJoinDecision.ShouldResumeDeferredJoin(allow, ConductPopup.WasDeclined))
            yield break;
        _passthrough = true;
        try
        {
            resume();
        }
        finally
        {
            _passthrough = false;
        }
    }

    public static IEnumerator WaitUntilAllowed(MonoBehaviour runner, Action<bool> done)
    {
        if (!OfficialSnrServer.IsIdentityEnabled())
        {
            done(true);
            yield break;
        }

        if (!ConductGate.Fetched || ConductGate.Last == null)
            ConductPopup.ShowFetching(runner);
        ConductResponse fetched = null;
        yield return PlayerSafetyApiClient.GetConduct(result => fetched = result).WrapToIl2Cpp();
        ConductGate.Apply(fetched);

        if (ConductGate.Last == null && !ConductGate.IsBannedNow)
        {
            ConductPopup.CloseVisible();
            done(true);
            yield break;
        }

        if (ConductGate.IsBannedNow)
        {
            ConductPopup.CloseVisible();
            yield return BanPopup.WaitUntilDismissed(runner).WrapToIl2Cpp();
            done(false);
            yield break;
        }

        if (ConductGate.HasUnackedWarning)
        {
            ConductPopup.CloseVisible();
            if (ConductGate.Last?.Warning != null)
                WarningPopup.Queue(ConductGate.Last.Warning);
            yield return WarningPopup.WaitUntilDismissed(runner).WrapToIl2Cpp();
        }

        if (ConductGate.IsBannedNow)
        {
            ConductPopup.CloseVisible();
            yield return BanPopup.WaitUntilDismissed(runner).WrapToIl2Cpp();
            done(false);
            yield break;
        }

        if (ConductGate.CanPlayOfficial)
        {
            ConductPopup.CloseVisible();
            done(true);
            yield break;
        }

        ConductPopup.Queue(ConductGate.Last);
        ConductPopup.ShowNow(runner);
        while (!ConductGate.CanPlayOfficial && !ConductPopup.WasDeclined && !ConductGate.IsBannedNow)
        {
            if (ConductGate.HasUnackedWarning && ConductGate.Last?.Consented == true)
                break;
            if (!ConductPopup.IsOpen)
                ConductPopup.ShowNow(runner);
            ConductPopup.RebindToCamera();
            yield return null;
        }

        if (ConductPopup.WasDeclined)
        {
            ConductDeclineAbort.Run();
            done(false);
            yield break;
        }

        if (ConductGate.IsBannedNow)
        {
            ConductPopup.CloseVisible();
            yield return BanPopup.WaitUntilDismissed(runner).WrapToIl2Cpp();
            done(false);
            yield break;
        }

        if (ConductGate.HasUnackedWarning)
        {
            ConductPopup.CloseVisible();
            if (ConductGate.Last?.Warning != null)
                WarningPopup.Queue(ConductGate.Last.Warning);
            yield return WarningPopup.WaitUntilDismissed(runner).WrapToIl2Cpp();
        }

        if (ConductGate.IsBannedNow)
        {
            ConductPopup.CloseVisible();
            yield return BanPopup.WaitUntilDismissed(runner).WrapToIl2Cpp();
            done(false);
            yield break;
        }

        done(ConductGate.CanPlayOfficial);
    }

    public static IEnumerator WrapJoin(AmongUsClient client, Il2CppSystem.Collections.IEnumerator original)
    {
        bool allow = false;
        yield return WaitUntilAllowed(client, value => allow = value).WrapToIl2Cpp();
        if (!ConductJoinDecision.ShouldResumeDeferredJoin(allow, ConductPopup.WasDeclined))
            yield break;
        while (original != null && original.MoveNext())
            yield return original.Current;
    }
}
[HarmonyPatch]
public static class OnlineJoinConductGatePatch
{
    private static readonly string[] MethodNames =
    {
        "CoJoinOnlinePublicGame",
        "CoJoinOnlineGameFromListing",
        "CoJoinOnlineGameFromCode",
        "CoJoinOnlineGameDirect",
        "CoJoinGameViaFriendInvite",
    };

    public static IEnumerable<MethodBase> TargetMethods()
    {
        return typeof(AmongUsClient).GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Where(method => MethodNames.Contains(method.Name));
    }

    public static void Postfix(AmongUsClient __instance, ref Il2CppSystem.Collections.IEnumerator __result)
    {
        if (!OfficialSnrServer.IsIdentityEnabled() || __result == null) return;
        if (OfficialPlayGate.IsResuming) return;
        __result = OfficialPlayGate.WrapJoin(__instance, __result).WrapToIl2Cpp();
    }
}

[HarmonyPatch(typeof(CreateGameOptions), nameof(CreateGameOptions.Confirm))]
public static class CreateGameConfirmConductPatch
{
    public static bool Prefix(CreateGameOptions __instance)
    {
        return OfficialPlayGate.AllowOrDefer(__instance, __instance.Confirm);
    }
}

[HarmonyPatch(typeof(CreateGameOptions), nameof(CreateGameOptions.CoStartGame))]
public static class CreateGameStartConductPatch
{
    public static bool Prefix(CreateGameOptions __instance)
    {
        return OfficialPlayGate.AllowOrDefer(__instance, __instance.CoStartGame);
    }
}

[HarmonyPatch(typeof(CreateGameOptions), nameof(CreateGameOptions.ContinueStart))]
public static class CreateGameContinueConductPatch
{
    public static bool Prefix(CreateGameOptions __instance)
    {
        return OfficialPlayGate.AllowOrDefer(__instance, __instance.ContinueStart);
    }
}

[HarmonyPatch(typeof(EnterCodeManager), nameof(EnterCodeManager.ClickJoin))]
public static class EnterCodeJoinConductPatch
{
    public static bool Prefix(EnterCodeManager __instance)
    {
        return OfficialPlayGate.AllowOrDefer(__instance, __instance.ClickJoin);
    }
}

[HarmonyPatch(typeof(JoinGameButton), nameof(JoinGameButton.OnClick))]
public static class JoinGameButtonConductPatch
{
    public static bool Prefix(JoinGameButton __instance)
    {
        return OfficialPlayGate.AllowOrDefer(__instance, __instance.OnClick);
    }
}

[HarmonyPatch(typeof(JoinGameButton), nameof(JoinGameButton.ContinueOnClick))]
public static class JoinGameContinueConductPatch
{
    public static bool Prefix(JoinGameButton __instance)
    {
        return OfficialPlayGate.AllowOrDefer(__instance, __instance.ContinueOnClick);
    }
}

[HarmonyPatch(typeof(MatchMakerGameButton), nameof(MatchMakerGameButton.OnClick))]
public static class MatchMakerJoinConductPatch
{
    public static bool Prefix(MatchMakerGameButton __instance)
    {
        return OfficialPlayGate.AllowOrDefer(__instance, __instance.OnClick);
    }
}

[HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.BeginGame))]
public static class GameStartBeginOfficialPlayPatch
{
    public static bool Prefix(GameStartManager __instance)
    {
        if (AmongUsClient.Instance == null || !AmongUsClient.Instance.AmHost) return true;
        return OfficialPlayGate.AllowOrDefer(__instance, __instance.BeginGame);
    }
}

[HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.JoinGame))]
[HarmonyPriority(Priority.First)]
public static class JoinGameIdentityPatch
{
    public static bool Prefix(InnerNetClient __instance)
    {
        return IdentityPreJoinGate.AllowOrDefer(
            __instance,
            () => __instance?.JoinGame());
    }
}

[HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.HostGame))]
[HarmonyPriority(Priority.First)]
public static class HostGameIdentityPatch
{
    public static bool Prefix(InnerNetClient __instance, IGameOptions settings, GameFilterOptions filterOpts)
    {
        return IdentityPreJoinGate.AllowOrDefer(
            __instance,
            () => __instance?.HostGame(settings, filterOpts));
    }
}

internal static class IdentityPreJoinGate
{
    private const float HandshakeTimeoutSeconds = 5f;
    private static bool _accepted;
    private static bool _pending;
    private static bool _allowOneFailOpenReplay;
    private static int _generation;

    public static void Reset()
    {
        _generation++;
        _accepted = false;
        _pending = false;
        _allowOneFailOpenReplay = false;
        OnGameJoinedIdentityPatch.ResetChallenge();
    }

    public static void Accept()
    {
        _accepted = true;
    }

    public static bool AllowOrDefer(InnerNetClient client, Action replay)
    {
        if (!OfficialSnrServer.IsIdentityEnabled()) return true;
        if (!PlayerIdentityStore.HasKey())
        {
            Logger.Warning("Identity key missing; refusing join/host until conduct consent");
            return false;
        }
        if (_accepted) return true;
        if (_allowOneFailOpenReplay)
        {
            _allowOneFailOpenReplay = false;
            return true;
        }
        if (_pending) return false;

        MonoBehaviour runner = SafetyRuntime.FindCoroutineRunner(client);
        if (runner == null)
        {
            Logger.Warning("Identity pre-join handshake could not start; allowing join (fail-open)");
            return true;
        }

        _pending = true;
        int generation = _generation;
        OnGameJoinedIdentityPatch.SendIdentityHello();
        runner.StartCoroutine(CoWaitAndReplay(client, replay, generation).WrapToIl2Cpp());
        return false;
    }

    private static IEnumerator CoWaitAndReplay(InnerNetClient client, Action replay, int generation)
    {
        float deadline = Time.realtimeSinceStartup + HandshakeTimeoutSeconds;
        bool helloSent = false;
        while (generation == _generation && !_accepted && Time.realtimeSinceStartup < deadline)
        {
            if (client == null)
            {
                _pending = false;
                yield break;
            }
            if (client.connection != null && !helloSent)
            {
                OnGameJoinedIdentityPatch.SendIdentityHello();
                helloSent = true;
            }
            yield return null;
        }

        if (client == null)
        {
            _pending = false;
            yield break;
        }
        _pending = false;
        ConductJoinDecision.HandshakeJoinAction action = ConductJoinDecision.AfterHandshakeWait(
            generationChanged: generation != _generation,
            declined: ConductPopup.WasDeclined,
            accepted: _accepted,
            hasKey: PlayerIdentityStore.HasKey());
        if (action == ConductJoinDecision.HandshakeJoinAction.Abort)
            yield break;
        if (action == ConductJoinDecision.HandshakeJoinAction.Refuse)
        {
            Logger.Warning("Identity pre-join handshake has no key; refusing join");
            yield break;
        }
        if (action == ConductJoinDecision.HandshakeJoinAction.FailOpenReplay)
        {
            _allowOneFailOpenReplay = true;
            Logger.Warning("Identity pre-join handshake timed out; allowing join (fail-open)");
        }
        replay?.Invoke();
    }
}

[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameJoined))]
public static class OnGameJoinedIdentityPatch
{
    private static string _serverChallenge;

    public static void Postfix(AmongUsClient __instance)
    {
        if (!OfficialSnrServer.IsIdentityEnabled()) return;
        SendIdentityHello();
        SendIdentityProof();
        __instance.StartCoroutine(CoHandleBanNotice(__instance).WrapToIl2Cpp());
    }

    private static System.Collections.IEnumerator CoHandleBanNotice(AmongUsClient client)
    {
        SendIdentityHello();
        SendIdentityProof();
        ConductResponse conduct = null;
        yield return PlayerSafetyApiClient.GetConduct(result => conduct = result).WrapToIl2Cpp();
        ConductGate.Apply(conduct);
        if (conduct == null) yield break;
        if (conduct.Banned)
        {
            yield return BanPopup.WaitUntilDismissed(client).WrapToIl2Cpp();
            BanPopup.MarkSwallowDisconnect();
            client.ExitGame(DisconnectReasons.ExitGame);
            yield break;
        }
        if (conduct.HasUnackedWarning)
            WarningPopup.ShowNow(SafetyPopupUi.EnsureHost());
    }

    public static void SendIdentityHello()
    {
        if (!PlayerIdentityStore.HasKey()) return;
        if (AmongUsClient.Instance?.connection == null) return;
        MessageWriter writer = MessageWriter.Get(SendOption.Reliable);
        writer.StartMessage(IdentityRoot.Flag);
        writer.Write((byte)IdentityRootSubtype.Hello);
        writer.Write(SuperNewRoles.VersionInfo.VersionString);
        writer.EndMessage();
        AmongUsClient.Instance.connection.Send(writer);
    }

    public static void SendIdentityProof()
    {
        string challenge = _serverChallenge;
        _serverChallenge = null;
        if (string.IsNullOrEmpty(challenge)) return;
        if (AmongUsClient.Instance?.connection == null) return;
        byte[] body = System.Text.Encoding.UTF8.GetBytes(challenge);
        PlayerIdentityProof proof = PlayerIdentityStore.CreateProof("impostor.connect", body);
        if (proof == null) return;
        byte[] publicKey = System.Convert.FromBase64String(proof.PublicKeyBase64);
        byte[] signature = System.Convert.FromBase64String(proof.SignatureBase64);
        MessageWriter writer = MessageWriter.Get(SendOption.Reliable);
        writer.StartMessage(IdentityRoot.Flag);
        writer.Write((byte)IdentityRootSubtype.Prove);
        writer.WriteBytesAndSize(publicKey);
        writer.WritePacked((int)proof.TimestampUnix);
        writer.Write(proof.Nonce);
        writer.WriteBytesAndSize(signature);
        writer.Write(challenge);
        writer.Write(SuperNewRoles.VersionInfo.VersionString);
        writer.EndMessage();
        AmongUsClient.Instance.connection.Send(writer);
    }

    public static void ReceiveIdentityChallenge(string challenge)
    {
        if (string.IsNullOrEmpty(challenge)) return;
        _serverChallenge = challenge;
        SendIdentityProof();
    }

    public static void ReceiveIdentityAccepted()
    {
        IdentityPreJoinGate.Accept();
    }

    public static void ResetChallenge()
    {
        _serverChallenge = null;
    }

    public static void SendHostBlockKick(int clientId)
    {
        if (AmongUsClient.Instance?.connection == null) return;
        MessageWriter writer = MessageWriter.Get(SendOption.Reliable);
        writer.StartMessage(IdentityRoot.Flag);
        writer.Write((byte)IdentityRootSubtype.HostKick);
        writer.WritePacked(clientId);
        writer.EndMessage();
        AmongUsClient.Instance.connection.Send(writer);
    }
}

[HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.HandleMessage))]
public static class SafetyInboundMessagePatch
{
    public static bool Prefix(MessageReader reader)
    {
        if (reader == null || reader.Tag != IdentityRoot.Flag) return true;
        MessageReader clone = null;
        try
        {
            clone = MessageReader.Get(reader);
            switch ((IdentityRootSubtype)clone.ReadByte())
            {
                case IdentityRootSubtype.Notify:
                    PlayerSafetyActions.NotifyBlockedJoinRejected(clone.ReadString());
                    break;
                case IdentityRootSubtype.Challenge:
                    OnGameJoinedIdentityPatch.ReceiveIdentityChallenge(clone.ReadString());
                    break;
                case IdentityRootSubtype.ParticipantIds:
                    SafetyParticipantIds.Apply(clone);
                    break;
                case IdentityRootSubtype.IdentityAccepted:
                    OnGameJoinedIdentityPatch.ReceiveIdentityAccepted();
                    break;
                case IdentityRootSubtype.Warn:
                    string warningId = clone.ReadString();
                    string warningBody = clone.ReadString();
                    WarningPopup.ReceiveFromServer(new WarningInfo { Id = warningId, Body = warningBody });
                    break;
                case IdentityRootSubtype.ConductBanLeave:
                    int bannedClientId = clone.ReadPackedInt32();
                    string bannedName = clone.Position < clone.Length ? clone.ReadString() : string.Empty;
                    ConductBanLeaveNotice.Receive(bannedClientId, bannedName);
                    break;
                case IdentityRootSubtype.HostBlockLeave:
                    int blockedClientId = clone.ReadPackedInt32();
                    string blockedName = clone.Position < clone.Length ? clone.ReadString() : string.Empty;
                    HostBlockLeaveNotice.Receive(blockedClientId, blockedName);
                    break;
            }
        }
        catch (Exception error)
        {
            Logger.Warning($"Ignored malformed Identity root message: {error.Message}");
        }
        finally
        {
            clone?.Recycle();
        }
        return false;
    }
}

[HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.LateUpdate))]
public static class ConductPopupMainMenuPatch
{
    private static bool requestedConduct;

    public static void Postfix(MainMenuManager __instance)
    {
        if (!OfficialSnrServer.IsIdentityEnabled()) return;
        BanPopup.RebindToCamera();
        WarningPopup.RebindToCamera();
        ConductPopup.RebindToCamera();
        if (!requestedConduct)
        {
            requestedConduct = true;
            __instance.StartCoroutine(CoPrefetchConduct().WrapToIl2Cpp());
        }
    }

    private static System.Collections.IEnumerator CoPrefetchConduct()
    {
        ConductResponse conduct = null;
        yield return PlayerSafetyApiClient.GetConduct(result => conduct = result);
        ConductGate.Apply(conduct);
    }
}

[HarmonyPatch(typeof(UnityWebRequest), nameof(UnityWebRequest.SendWebRequest))]
public static class MatchmakingIdentityHeaderPatch
{
    public static void Prefix(UnityWebRequest __instance)
    {
        if (__instance == null || !OfficialSnrServer.IsIdentityEnabled()) return;
        if (!string.Equals(__instance.method, "GET", StringComparison.OrdinalIgnoreCase)) return;
        string url = __instance.url;
        if (string.IsNullOrEmpty(url) || url.IndexOf("/api/games", StringComparison.OrdinalIgnoreCase) < 0)
            return;

        PlayerIdentityProof proof = PlayerIdentityStore.CreateProof("games.list", System.Array.Empty<byte>());
        if (proof == null)
            return;
        __instance.SetRequestHeader("X-SNR-Public-Key", proof.PublicKeyBase64);
        __instance.SetRequestHeader("X-SNR-Timestamp", proof.TimestampUnix.ToString());
        __instance.SetRequestHeader("X-SNR-Nonce", proof.Nonce);
        __instance.SetRequestHeader("X-SNR-Signature", proof.SignatureBase64);
        __instance.SetRequestHeader("X-SNR-Action", proof.Action);
    }
}

[HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.Connect))]
public static class ConnectIdentityProofPatch
{
    public static void Postfix(InnerNetClient __instance)
    {
        if (!OfficialSnrServer.IsIdentityEnabled() || __instance == null) return;
        IdentityPreJoinGate.Reset();
        MonoBehaviour runner = SafetyRuntime.FindCoroutineRunner(__instance);
        if (runner == null) return;
        runner.StartCoroutine(CoSendWhenConnected(__instance).WrapToIl2Cpp());
    }

    private static System.Collections.IEnumerator CoSendWhenConnected(InnerNetClient client)
    {
        for (int i = 0; i < 80; i++)
        {
            if (client == null) yield break;
            if (client.connection != null)
            {
                OnGameJoinedIdentityPatch.SendIdentityHello();
                OnGameJoinedIdentityPatch.SendIdentityProof();
                yield break;
            }
            yield return null;
        }
    }
}

[HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.HandleDisconnect))]
public static class HostBlockedDisconnectPatch
{
    private static bool _showConductAfterDisconnect;

    public static void Prefix(ref DisconnectReasons reason, ref string stringReason)
    {
        if (BanNotice.TryParse(stringReason, out BanInfo ban))
            BanPopup.Queue(ban);
        if (BanPopup.SwallowDisconnect || BanPopup.IsOpen || BanNotice.TryParse(stringReason, out _))
        {
            BanPopup.MarkSwallowDisconnect();
            reason = DisconnectReasons.ExitGame;
            stringReason = string.Empty;
            return;
        }
        if (SafetyDisconnectCopy.IsNeedConduct(stringReason))
        {
            reason = DisconnectReasons.ExitGame;
            stringReason = string.Empty;
            if (!ConductJoinDecision.ShouldReopenConductAfterDisconnect(isNeedConduct: true, ConductPopup.WasDeclined))
                return;
            ConductGate.InvalidateFetched();
            ConductPopup.WasDeclined = false;
            _showConductAfterDisconnect = true;
            return;
        }
        if (string.IsNullOrEmpty(stringReason)) return;
        string key = SafetyDisconnectCopy.TranslationKey(stringReason);
        if (key != null)
            stringReason = ModTranslation.GetString(key);
    }

    public static void Postfix()
    {
        if (_showConductAfterDisconnect)
        {
            _showConductAfterDisconnect = false;
            BanPopup.HideVanillaDisconnect();
            MonoBehaviour host = SafetyPopupUi.EnsureHost();
            host.StartCoroutine(ConductPopup.CoRefreshAndPresent(host).WrapToIl2Cpp());
        }
        WarningPopup.RebindToCamera();
        if (!BanPopup.SwallowDisconnect && !BanPopup.IsOpen)
            return;
        BanPopup.HideVanillaDisconnect();
        BanPopup.RebindToCamera();
        if (!BanPopup.ShouldSuppressAutoShow)
            BanPopup.ShowNow(SafetyPopupUi.EnsureHost());
        BanPopup.ClearSwallowDisconnect();
    }
}
