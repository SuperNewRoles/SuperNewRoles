using System.Text;
using AmongUs.Data;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using HarmonyLib;
using Hazel;
using InnerNet;
using SuperNewRoles.Modules;
using SuperNewRoles.Safety.Api;
using SuperNewRoles.Safety.Identity;
using UnityEngine;

namespace SuperNewRoles.Safety.Patches;

[HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.JoinGame))]
public static class JoinGameIdentityPatch
{
    public static void Prefix()
    {
        if (OfficialSnrServer.IsCurrent())
            OnGameJoinedIdentityPatch.SendIdentityProof();
    }
}

[HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.HostGame))]
public static class HostGameIdentityPatch
{
    public static void Prefix()
    {
        if (OfficialSnrServer.IsCurrent())
            OnGameJoinedIdentityPatch.SendIdentityProof();
    }
}
[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameJoined))]
public static class OnGameJoinedIdentityPatch
{
    public static void Postfix(AmongUsClient __instance)
    {
        if (!OfficialSnrServer.IsCurrent()) return;
        __instance.StartCoroutine(CoProveAndConduct(__instance).WrapToIl2Cpp());
    }

    private static System.Collections.IEnumerator CoProveAndConduct(AmongUsClient client)
    {
        SendIdentityProof();
        ConductResponse conduct = null;
        yield return PlayerSafetyApiClient.GetConduct(result => conduct = result);
        if (conduct == null) yield break;
        if (conduct.Banned)
        {
            client.ExitGame(DisconnectReasons.Custom);
            yield break;
        }
        if (!conduct.Consented)
        {
            ConductPopup.Queue(conduct);
            client.ExitGame(DisconnectReasons.ExitGame);
        }
    }

    public static void SendIdentityProof()
    {
        if (AmongUsClient.Instance?.connection == null) return;
        PlayerIdentityProof proof = PlayerIdentityStore.CreateProof("impostor.connect", System.Array.Empty<byte>());
        byte[] publicKey = System.Convert.FromBase64String(proof.PublicKeyBase64);
        byte[] signature = System.Convert.FromBase64String(proof.SignatureBase64);
        MessageWriter writer = MessageWriter.Get(SendOption.Reliable);
        writer.StartMessage(0xD1);
        writer.Write((byte)1);
        writer.WriteBytesAndSize(publicKey);
        writer.WritePacked((int)proof.TimestampUnix);
        writer.Write(proof.Nonce);
        writer.WriteBytesAndSize(signature);
        writer.EndMessage();
        AmongUsClient.Instance.connection.Send(writer);
    }

    public static void SendHostBlockKick(int clientId)
    {
        if (AmongUsClient.Instance?.connection == null) return;
        MessageWriter writer = MessageWriter.Get(SendOption.Reliable);
        writer.StartMessage(0xD1);
        writer.Write((byte)2);
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
        if (reader == null || reader.Tag != 0xD1) return true;
        try
        {
            var clone = MessageReader.Get(reader);
            byte subtype = clone.ReadByte();
            if (subtype != 3) return false;
            string name = clone.ReadString();
            if (!ConfigRoles.NotifyHostWhenBlockedJoin.Value) return false;
            string text = string.Format(ModTranslation.GetString("SafetyHostBlockRejected"), name);
            if (FastDestroyableSingleton<HudManager>.Instance?.Chat != null)
                FastDestroyableSingleton<HudManager>.Instance.Chat.AddChatWarning(text);
            else
                Logger.Info(text);
        }
        catch
        {
            // 他の 0xD1 サブタイプは無視する
        }
        return false;
    }
}

[HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.LateUpdate))]
public static class ConductPopupMainMenuPatch
{
    private static GameObject current;
    private static bool fetching;
    private static bool fetched;
    public static void Postfix(MainMenuManager __instance)
    {
        if (OfficialSnrServer.IsCurrent() && !fetching && !fetched)
        {
            fetching = true;
            __instance.StartCoroutine(CoFetchConduct().WrapToIl2Cpp());
        }
        if (current == null) current = null;
        ConductPopup.TryHandle(__instance, ref current);
    }

    private static System.Collections.IEnumerator CoFetchConduct()
    {
        ConductResponse conduct = null;
        yield return PlayerSafetyApiClient.GetConduct(result => conduct = result);
        fetched = true;
        fetching = false;
        if (conduct != null && (!conduct.Consented || conduct.Banned))
            ConductPopup.Queue(conduct);
    }
}
