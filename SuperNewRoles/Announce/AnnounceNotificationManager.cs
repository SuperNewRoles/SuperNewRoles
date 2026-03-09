using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AmongUs.Data;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using SuperNewRoles.Modules;
using UnityEngine;

namespace SuperNewRoles.Patches;

/// <summary>
/// アナウンスの新着チェックと未読管理を行います
/// </summary>
public static class AnnounceNotificationManager
{
    private static readonly string ReadIdsFilePath = Path.Combine(SuperNewRolesPlugin.BaseDirectory, "AnnounceCache", "read_ids.json");
    private static HashSet<string> _readIds = new();
    private static bool _hasCheckedOnStartup = false;
    private static bool _hasUnreadAnnouncements = false;
    private static bool _isInitialized = false;

    public static void Initialize()
    {
        if (_isInitialized)
            return;

        LoadReadIds();
        _isInitialized = true;
    }

    private static void LoadReadIds()
    {
        _readIds.Clear();

        try
        {
            if (File.Exists(ReadIdsFilePath))
            {
                string json = File.ReadAllText(ReadIdsFilePath);
                var parsed = JsonParser.Parse(json);

                if (parsed is Dictionary<string, object> dict && dict.TryGetValue("read_ids", out var idsObj))
                {
                    if (idsObj is List<object> idsList)
                    {
                        foreach (var id in idsList)
                        {
                            if (id != null && !string.IsNullOrWhiteSpace(id.ToString()))
                                _readIds.Add(id.ToString());
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            SuperNewRolesPlugin.Logger.LogWarning($"Failed to load read announcement IDs: {ex.Message}");
        }
    }

    private static void SaveReadIds()
    {
        try
        {
            var dir = Path.GetDirectoryName(ReadIdsFilePath);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            var data = new Dictionary<string, object>
            {
                ["read_ids"] = _readIds.ToList()
            };

            string json = JsonParser.Serialize(data);
            File.WriteAllText(ReadIdsFilePath, json);
        }
        catch (Exception ex)
        {
            SuperNewRolesPlugin.Logger.LogWarning($"Failed to save read announcement IDs: {ex.Message}");
        }
    }

    /// <summary>
    /// 指定したIDのアナウンスを既読にします
    /// </summary>
    public static void MarkAsRead(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return;

        if (_readIds.Add(id))
        {
            SaveReadIds();
            UpdateUnreadStatus();
        }
    }

    /// <summary>
    /// 複数のアナウンスを既読にします
    /// </summary>
    public static void MarkAsRead(IEnumerable<string> ids)
    {
        if (ids == null)
            return;

        bool changed = false;
        foreach (var id in ids)
        {
            if (!string.IsNullOrWhiteSpace(id) && _readIds.Add(id))
                changed = true;
        }

        if (changed)
        {
            SaveReadIds();
            UpdateUnreadStatus();
        }
    }

    /// <summary>
    /// 指定したIDのアナウンスが既読かどうかを返します
    /// </summary>
    public static bool IsRead(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return true;

        return _readIds.Contains(id);
    }

    /// <summary>
    /// 未読のアナウンスがあるかどうかを返します
    /// </summary>
    public static bool HasUnreadAnnouncements()
    {
        return _hasUnreadAnnouncements;
    }

    /// <summary>
    /// 起動時チェックが完了したかどうかを返します
    /// </summary>
    public static bool HasCheckedOnStartup()
    {
        return _hasCheckedOnStartup;
    }

    /// <summary>
    /// 起動時の新着チェックを実行します
    /// </summary>
    public static IEnumerator CheckForNewAnnouncementsOnStartup()
    {
        if (_hasCheckedOnStartup)
            yield break;

        _hasCheckedOnStartup = true;

        SuperNewRolesPlugin.Logger.LogInfo("Starting new announcements check...");

        string lang = GetApiLanguage();
        ApiResult<ArticlesResponse> listResult = null;
        yield return SuperNewAnnounceApi.ListArticles(lang, result => listResult = result, page: 1, pageSize: 10, fallback: true);

        if (listResult == null || !listResult.IsSuccess || listResult.Data == null || listResult.Data.Items.Count == 0)
        {
            SuperNewRolesPlugin.Logger.LogInfo("No announcements found or API error.");
            yield break;
        }

        SuperNewRolesPlugin.Logger.LogInfo($"Found {listResult.Data.Items.Count} announcements.");

        // リストをキャッシュに保存
        AnnounceCache.SaveArticlesList(lang, listResult.Data, listResult.ETag);

        // 未読があるかチェック
        bool hasUnread = false;
        List<string> newUnreadIds = new();

        foreach (var item in listResult.Data.Items)
        {
            if (!IsRead(item.Id))
            {
                hasUnread = true;
                newUnreadIds.Add(item.Id);
                SuperNewRolesPlugin.Logger.LogInfo($"Unread announcement: {item.Id} - {item.Title}");
            }
        }

        _hasUnreadAnnouncements = hasUnread;

        SuperNewRolesPlugin.Logger.LogInfo($"Unread count: {newUnreadIds.Count}");

        // 未読があればポップアップを表示
        if (hasUnread && newUnreadIds.Count > 0)
        {
            ShowNewAnnouncementsPopup(listResult.Data.Items.Where(x => newUnreadIds.Contains(x.Id)).ToList());
        }
    }

    /// <summary>
    /// 未読状態を更新します
    /// </summary>
    private static void UpdateUnreadStatus()
    {
        string lang = GetApiLanguage();
        var cachedList = AnnounceCache.GetArticlesList(lang);

        if (cachedList == null || cachedList.Response == null || cachedList.Response.Items.Count == 0)
        {
            _hasUnreadAnnouncements = false;
            return;
        }

        bool hasUnread = false;
        foreach (var item in cachedList.Response.Items)
        {
            if (!IsRead(item.Id))
            {
                hasUnread = true;
                break;
            }
        }

        _hasUnreadAnnouncements = hasUnread;
    }

    /// <summary>
    /// 新着アナウンスのポップアップを表示します
    /// </summary>
    private static void ShowNewAnnouncementsPopup(List<ArticleMinimal> newArticles)
    {
        if (newArticles == null || newArticles.Count == 0)
        {
            SuperNewRolesPlugin.Logger.LogInfo("ShowNewAnnouncementsPopup: No new articles to show.");
            return;
        }

        SuperNewRolesPlugin.Logger.LogInfo($"ShowNewAnnouncementsPopup: Attempting to show popup for {newArticles.Count} new articles.");
        var popup = GameObject.FindObjectOfType<MainMenuManager>();
        if (popup != null)
        {
            popup?.announcementPopUp?.Show();
            SuperNewRolesPlugin.Logger.LogInfo("ShowNewAnnouncementsPopup: Announcement popup opened.");
        }
    }

    private static string GetApiLanguage()
    {
        try
        {
            return DataManager.Settings.Language.CurrentLanguage switch
            {
                SupportedLangs.Japanese => "ja",
                SupportedLangs.SChinese => "zh-CN",
                SupportedLangs.TChinese => "zh-TW",
                _ => "en"
            };
        }
        catch
        {
            return "en";
        }
    }
}
