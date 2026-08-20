using System.Collections.Generic;
using System.Text;
using InnerNet;
using SuperNewRoles.Modules;
using SuperNewRoles.Safety.Api;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using BepInEx.Unity.IL2CPP.Utils.Collections;

namespace SuperNewRoles.Safety;

public static class BlockListPopup
{
    public static void Open(MonoBehaviour runner)
    {
        runner.StartCoroutine(PlayerSafetyApiClient.ListBlocks(rows => Show(rows)).WrapToIl2Cpp());
    }

    private static void Show(List<BlockRow> rows)
    {
        GameObject popup = AssetManager.Instantiate("AnalyticsBG", Camera.main.transform);
        popup.SetActive(true);
        popup.transform.localScale = Vector3.one * 0.58f;
        popup.transform.Find("Title").GetComponent<TextMeshPro>().text = ModTranslation.GetString("SafetyBlockListTitle");
        var sb = new StringBuilder();
        if (rows == null || rows.Count == 0)
            sb.Append(ModTranslation.GetString("SafetyBlockListEmpty"));
        else
        {
            foreach (var row in rows)
                sb.AppendLine($"{row.Name}  {row.CreatedAt}  {row.Note}\n  /unblock {row.Id}\n  /blocknote {row.Id} {ModTranslation.GetString("SafetyBlockNoteHint")}");
        }
        popup.transform.Find("Text").GetComponent<TextMeshPro>().text = sb.ToString();
        var buttons = popup.GetComponentsInChildren<PassiveButton>(true);
        if (buttons.Length > 0)
        {
            buttons[0].OnClick = new();
            buttons[0].OnClick.AddListener((UnityAction)(() => Object.Destroy(popup)));
        }
    }
}

public static class PublicGamesPopup
{
    public static void Open(MonoBehaviour runner)
    {
        runner.StartCoroutine(PlayerSafetyApiClient.ListGames(rows => Show(rows)).WrapToIl2Cpp());
    }

    private static void Show(List<PublicGameRow> rows)
    {
        GameObject popup = AssetManager.Instantiate("AnalyticsBG", Camera.main.transform);
        popup.SetActive(true);
        popup.transform.localScale = Vector3.one * 0.58f;
        popup.transform.Find("Title").GetComponent<TextMeshPro>().text = ModTranslation.GetString("SafetyPublicGamesTitle");
        var sb = new StringBuilder();
        if (rows == null || rows.Count == 0)
            sb.Append(ModTranslation.GetString("SafetyPublicGamesEmpty"));
        else
        {
            foreach (var row in rows)
            {
                string warn = row.HasBlockedPlayer ? ModTranslation.GetString("SafetyHasBlockedPlayer") : string.Empty;
                sb.AppendLine($"{row.Code} {row.HostName} {row.PlayerCount}/{row.MaxPlayers} {warn}");
            }
        }
        popup.transform.Find("Text").GetComponent<TextMeshPro>().text = sb.ToString();
        var buttons = popup.GetComponentsInChildren<PassiveButton>(true);
        if (buttons.Length > 0)
        {
            buttons[0].OnClick = new();
            buttons[0].OnClick.AddListener((UnityAction)(() => Object.Destroy(popup)));
        }
    }
}

public static class PlayerDetailPopup
{
    public static void Open(ClientData client, MonoBehaviour runner)
    {
        if (client == null || runner == null) return;
        GameObject popup = AssetManager.Instantiate("AnalyticsBG", Camera.main.transform);
        popup.SetActive(true);
        popup.transform.localScale = Vector3.one * 0.58f;
        popup.transform.Find("Title").GetComponent<TextMeshPro>().text = client.PlayerName;
        popup.transform.Find("Text").GetComponent<TextMeshPro>().text = ModTranslation.GetString("SafetyPlayerDetailBody");
        var buttons = popup.GetComponentsInChildren<PassiveButton>(true);
        if (buttons.Length > 0)
        {
            buttons[0].OnClick = new();
            buttons[0].OnClick.AddListener((UnityAction)(() =>
            {
                Object.Destroy(popup);
                PlayerSafetyActions.BlockClient(client, string.Empty, runner);
            }));
        }
        if (buttons.Length > 1)
        {
            buttons[1].OnClick = new();
            buttons[1].OnClick.AddListener((UnityAction)(() =>
            {
                Object.Destroy(popup);
                PlayerSafetyActions.ReportByName(client.PlayerName, "other", string.Empty, runner);
            }));
        }
    }
}
