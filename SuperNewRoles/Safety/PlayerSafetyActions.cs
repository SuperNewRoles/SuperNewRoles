using System.Linq;
using AmongUs.Data;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using InnerNet;
using SuperNewRoles.Modules;
using SuperNewRoles.Safety.Api;
using SuperNewRoles.Safety.Patches;
using UnityEngine;

namespace SuperNewRoles.Safety;

public static class PlayerSafetyActions
{
    public static ClientData FindByName(string name)
    {
        return AmongUsClient.Instance?.allClients?.ToArray()
            .FirstOrDefault(client => client != null && string.Equals(client.PlayerName, name, System.StringComparison.OrdinalIgnoreCase));
    }

    public static void BlockByName(string name, string note, MonoBehaviour runner)
    {
        var client = FindByName(name);
        if (client == null)
        {
            HudManager.Instance.Chat.AddChat(PlayerControl.LocalPlayer, ModTranslation.GetString("SafetyPlayerNotFound"));
            return;
        }
        BlockClient(client, note, runner);
    }

    public static void BlockClient(ClientData client, string note, MonoBehaviour runner)
    {
        string code = GameCode.IntToGameName(AmongUsClient.Instance.GameId);
        runner.StartCoroutine(PlayerSafetyApiClient.CreateBlock(code, client.Id, note, result =>
        {
            HudManager.Instance.Chat.AddChat(PlayerControl.LocalPlayer, string.Format(ModTranslation.GetString("SafetyBlocked"), client.PlayerName));
            if (result.KickTarget && AmongUsClient.Instance.AmHost)
                OnGameJoinedIdentityPatch.SendHostBlockKick(client.Id);
            if (result.LeaveSelf)
                AmongUsClient.Instance.ExitGame(DisconnectReasons.ExitGame);
        }).WrapToIl2Cpp());
    }

    public static void ReportByName(string name, string category, string comment, MonoBehaviour runner)
    {
        var client = FindByName(name);
        if (client == null)
        {
            HudManager.Instance.Chat.AddChat(PlayerControl.LocalPlayer, ModTranslation.GetString("SafetyPlayerNotFound"));
            return;
        }
        string code = GameCode.IntToGameName(AmongUsClient.Instance.GameId);
        runner.StartCoroutine(PlayerSafetyApiClient.CreateReport(code, client.Id, category, comment, string.Empty, ok =>
        {
            HudManager.Instance.Chat.AddChat(
                PlayerControl.LocalPlayer,
                ok ? ModTranslation.GetString("SafetyReported") : ModTranslation.GetString("SafetyReportFailed"));
        }).WrapToIl2Cpp());
    }

    public static void Unblock(string rowId, MonoBehaviour runner)
    {
        runner.StartCoroutine(PlayerSafetyApiClient.DeleteBlock(rowId, ok =>
        {
            HudManager.Instance.Chat.AddChat(
                PlayerControl.LocalPlayer,
                ok ? ModTranslation.GetString("SafetyUnblocked") : ModTranslation.GetString("SafetyReportFailed"));
        }).WrapToIl2Cpp());
    }

    public static void UpdateNote(string rowId, string note, MonoBehaviour runner)
    {
        runner.StartCoroutine(PlayerSafetyApiClient.UpdateBlockNote(rowId, note, ok =>
        {
            HudManager.Instance.Chat.AddChat(
                PlayerControl.LocalPlayer,
                ok ? ModTranslation.GetString("SafetyNoteSaved") : ModTranslation.GetString("SafetyReportFailed"));
        }).WrapToIl2Cpp());
    }
}

public static class SafetyChatCommands
{
    public static bool TryHandle(string text, MonoBehaviour runner)
    {
        if (runner == null || string.IsNullOrWhiteSpace(text)) return false;
        if (text.StartsWith("/block ", System.StringComparison.OrdinalIgnoreCase))
        {
            string rest = text[7..].Trim();
            int space = rest.IndexOf(' ');
            string name = space < 0 ? rest : rest[..space];
            string note = space < 0 ? string.Empty : rest[(space + 1)..];
            PlayerSafetyActions.BlockByName(name, note, runner);
            return true;
        }
        if (text.StartsWith("/report ", System.StringComparison.OrdinalIgnoreCase))
        {
            string rest = text[8..].Trim();
            string[] parts = rest.Split(' ', 3);
            string name = parts.Length > 0 ? parts[0] : string.Empty;
            string category = parts.Length > 1 ? parts[1] : "other";
            string comment = parts.Length > 2 ? parts[2] : string.Empty;
            PlayerSafetyActions.ReportByName(name, category, comment, runner);
            return true;
        }
        if (text.StartsWith("/player ", System.StringComparison.OrdinalIgnoreCase))
        {
            var client = PlayerSafetyActions.FindByName(text[8..].Trim());
            if (client == null)
                HudManager.Instance.Chat.AddChat(PlayerControl.LocalPlayer, ModTranslation.GetString("SafetyPlayerNotFound"));
            else
                PlayerDetailPopup.Open(client, runner);
            return true;
        }
        if (text.StartsWith("/unblock ", System.StringComparison.OrdinalIgnoreCase))
        {
            PlayerSafetyActions.Unblock(text[9..].Trim(), runner);
            return true;
        }
        if (text.StartsWith("/blocknote ", System.StringComparison.OrdinalIgnoreCase))
        {
            string rest = text[11..].Trim();
            int space = rest.IndexOf(' ');
            string id = space < 0 ? rest : rest[..space];
            string note = space < 0 ? string.Empty : rest[(space + 1)..];
            PlayerSafetyActions.UpdateNote(id, note, runner);
            return true;
        }
        return false;
    }
}
