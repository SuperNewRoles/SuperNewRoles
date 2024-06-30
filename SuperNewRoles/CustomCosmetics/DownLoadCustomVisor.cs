using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using UnityEngine;
using static SuperNewRoles.CustomCosmetics.CustomCosmeticsData.CustomVisors;

namespace SuperNewRoles.CustomCosmetics;

public static class DownLoadClassVisor
{
    public static bool IsEndDownload = false;
    public static bool running = false;

    /// <summary>バイザーをダウンロードするRepositoryURL</summary>
    /// <value>key : URL, Item1 : クローゼット名, Item2 : SNRのテンプレートで記載されているか</value>
    public static Dictionary<string, (string, bool)> VisorRepository()
    {
        Dictionary<string, (string, bool)> visorRepos = new()
        {
            { DownLoadCustomCosmetics.SNCmainURL, ("SuperNewCosmetics", true) },

            { "https://raw.githubusercontent.com/hinakkyu/TheOtherHats/master", ("mememurahat", false)},
            { "https://raw.githubusercontent.com/Ujet222/TOPVisors/main", ("YJ", false) },
        };

        if (!DownLoadCustomCosmetics.IsTestLoad) return visorRepos;
        else
        {
            visorRepos.Add(DownLoadCustomCosmetics.TestRepoURL, ("TestRepository", true));
            if (DownLoadCustomCosmetics.IsBlocLoadSNCmain) visorRepos.Remove(DownLoadCustomCosmetics.SNCmainURL);
            return visorRepos;
        }
    }

    /// <summary>ダウンロード処理が済んでいない リポジトリ</summary>
    public static List<string> ReposDLProcessing = new(); // Hat Reposと同じ
    /// <summary>ダウンロード済みリポジトリ</summary>
    public static List<string> CachedRepos = new();
    public static List<CustomVisorOnline> VisorDetails = new();
    private static Task hatFetchTask = null;

    public static void Load()
    {
        if (running) return;
        running = true;
        SuperNewRolesPlugin.Logger.LogInfo("[CustomVisor:Download] バイザーダウンロード開始");
        hatFetchTask = LaunchVisorFetcherAsync();
    }

    private static async Task LaunchVisorFetcherAsync()
    {
        Logger.Info("[CustomVisor:Download] ISDOWNLOAD:"+DownLoadCustomCosmetics.IsLoad);
        if (!DownLoadCustomCosmetics.IsLoad) return;

        Directory.CreateDirectory(Path.GetDirectoryName(Application.dataPath) + @"\SuperNewRoles\");
        Directory.CreateDirectory(Path.GetDirectoryName(Application.dataPath) + @"\SuperNewRoles\CustomVisorsChache\");

        var visorRepos = VisorRepository();

        List<string> repos = new(visorRepos.Keys);
        foreach (string repo in repos) { ReposDLProcessing.Add(repo); }

        string filePath = Path.GetDirectoryName(Application.dataPath) + @"\SuperNewRoles\CustomVisorsChache\";

        foreach (string repo in repos)
        {
            var repoData = visorRepos.FirstOrDefault(data => data.Key == repo).Value;

            if (File.Exists($"{filePath}\\{repoData.Item1}.json"))
            {
                IsEndDownload = true;
                StreamReader sr = new($"{filePath}\\{repoData.Item1}.json");

                string json = sr.ReadToEnd();
                sr.Close();

                JToken jobj = JObject.Parse(json)["Visors"];
                if (jobj == null || !jobj.HasValues) jobj = JObject.Parse(json)["visors"];
                if (jobj != null && jobj.HasValues)
                {
                    List<CustomVisorOnline> visorData = new();

                    for (JToken current = jobj.First; current != null; current = current.Next)
                    {
                        if (current.HasValues)
                        {
                            CustomVisorOnline info = new()
                            {
                                name = current["name"]?.ToString(),
                                author = current["author"]?.ToString(),
                                resource = SanitizeResourcePath(current["resource"]?.ToString())
                            };
                            if (info.resource == null || info.name == null) continue;

                            bool isSNR = repoData.Item2;
                            if (isSNR) { isSNR = bool.TryParse(current["IsSNR"]?.ToString(), out bool SNRTmpStatus) ? SNRTmpStatus : false; }
                            info.IsSNR = isSNR;

                            info.reshasha = info.resource + info.name + info.author;
                            info.flipresource = SanitizeResourcePath(current["flipresource"]?.ToString());
                            info.reshashf = current["reshashf"]?.ToString();
                            info.adaptive = current["adaptive"] != null;
                            info.behindHats = bool.TryParse(current["behindHats"]?.ToString(), out bool behindHats) ? behindHats : false;

                            info.package = current["package"]?.ToString();
                            if (current["package"] == null) info.package = "NameNone";
                            if (info.package != null && !PackageNames.Contains(info.package))
                            {
                                PackageNames.Add(info.package);
                            }

                            if (info.package == "Developer Visors")
                                info.package = "developerVisorts";

                            if (info.package == "Community Visors")
                                info.package = "communityVisors";

                            visorData.Add(info);
                        }
                    }
                    if (!PackageNames.Contains("InnerSloth")) PackageNames.Add("InnerSloth");

                    VisorDetails.AddRange(visorData);
                    CachedRepos.Add(repo);
                    ReposDLProcessing.Remove(repo);
                }
            }
        }
        IsEndDownload = true;
        foreach (var repo in visorRepos)
        {
            SuperNewRolesPlugin.Logger.LogInfo("[CustomVisors] バイザースタート:" + repo.Key);

            try
            {
                HttpStatusCode status = await FetchVisors(repo.Key);
                if (status != HttpStatusCode.OK)
                    System.Console.WriteLine($"Custom visors could not be loaded from repo: {repo.Key}\n");
                else
                    SuperNewRolesPlugin.Logger.LogInfo("バイザー終了:" + repo.Key);
            }
            catch (System.Exception e)
            {
                System.Console.WriteLine($"Unable to fetch visors from repo: {repo.Key}\n" + e.Message);
            }
        }
        running = false;
    }

    private static string SanitizeResourcePath(string res)
    {
        if (res == null || !res.EndsWith(".png"))
            return null;

        res = res.Replace("\\", "")
                .Replace("/", "")
                .Replace("*", "")
                .Replace("..", "");
        return res;
    }
    private static bool DoesResourceRequireDownload(string respath, string reshash, MD5 md5)
    {
        if (reshash == null || !File.Exists(respath))
            return true;

        using var stream = File.OpenRead(respath);
        var hash = System.BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "").ToLowerInvariant();
        return !reshash.Equals(hash);
    }
    public static async Task<HttpStatusCode> FetchVisors(string repo)
    {
        SuperNewRolesPlugin.Logger.LogInfo("[CustomVisor:Download] バイザーダウンロード開始:" + repo);

        HttpClient http = new();
        http.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue { NoCache = true };
        var response = await http.GetAsync(new System.Uri($"{repo}/CustomVisors.json"), HttpCompletionOption.ResponseContentRead);
        if (response.StatusCode == HttpStatusCode.NotFound) response = await http.GetAsync(new System.Uri($"{repo}/CustomHats.json"), HttpCompletionOption.ResponseContentRead);

        try
        {
            if (response.StatusCode != HttpStatusCode.OK) return response.StatusCode;
            if (response.Content == null)
            {
                System.Console.WriteLine("Server returned no data: " + response.StatusCode.ToString());
                return HttpStatusCode.ExpectationFailed;
            }

            string json = await response.Content.ReadAsStringAsync();
            var responsestream = await response.Content.ReadAsStreamAsync();
            string filePath = Path.GetDirectoryName(Application.dataPath) + @"\SuperNewRoles\CustomVisorsChache\";
            var repoData = VisorRepository().FirstOrDefault(data => data.Key == repo).Value;
            responsestream.CopyTo(File.Create($"{filePath}\\{repoData.Item1}.json"));

            JToken jobj = JObject.Parse(json)["Visors"];
            if (jobj == null || !jobj.HasValues) jobj = JObject.Parse(json)["visors"]; //  ``"Visors"``が無い場合, ``"visors"``を再検索して取得
            if (!jobj.HasValues) return HttpStatusCode.ExpectationFailed;

            List<CustomVisorOnline> visorData = new();

            for (JToken current = jobj.First; current != null; current = current.Next)
            {
                if (current != null && current.HasValues)
                {
                    CustomVisorOnline info = new()
                    {
                        name = current["name"]?.ToString(),
                        author = current["author"]?.ToString(),
                        resource = SanitizeResourcePath(current["resource"]?.ToString())
                    };

                    if (info.resource == null || info.name == null) continue;

                    info.reshasha = info.resource + info.name + info.author;

                    info.flipresource = SanitizeResourcePath(current["flipresource"]?.ToString());
                    info.reshashf = current["reshashf"]?.ToString();

                    info.adaptive = current["adaptive"] != null;

                    bool isSNR = repoData.Item2;
                    if (isSNR) { isSNR = bool.TryParse(current["IsSNR"]?.ToString(), out bool SNRTmpStatus) ? SNRTmpStatus : false; }
                    info.IsSNR = isSNR;

                    info.package = current["package"]?.ToString();
                    if (current["package"] == null) info.package = "NameNone";
                    if (info.package != null && !PackageNames.Contains(info.package))
                    {
                        PackageNames.Add(info.package);
                    }

                    visorData.Add(info);
                }
            }
            PackageNames.Add("InnerSloth");

            List<string> markedfordownload = new();

            MD5 md5 = MD5.Create();
            foreach (CustomVisorOnline data in visorData)
            {
                if (DoesResourceRequireDownload(filePath + data.resource, data.reshasha, md5))
                    markedfordownload.Add(data.resource);
                if (data.flipresource != null && DoesResourceRequireDownload(filePath + data.flipresource, data.reshashf, md5))
                    markedfordownload.Add(data.flipresource);
            }

            foreach (var file in markedfordownload)
            {
                var visorFileResponse = await http.GetAsync($"{repo}/Visors/{file}", HttpCompletionOption.ResponseContentRead);
                if (visorFileResponse.StatusCode == HttpStatusCode.NotFound) visorFileResponse = await http.GetAsync($"{repo}/visors/{file}", HttpCompletionOption.ResponseContentRead);

                if (visorFileResponse.StatusCode != HttpStatusCode.OK) continue;

                using var responseStream = await visorFileResponse.Content.ReadAsStreamAsync();
                using var fileStream = File.Create($"{filePath}\\{file}");
                responseStream.CopyTo(fileStream);
            }

            if (!CachedRepos.Contains(repo))
            {
                VisorDetails.AddRange(visorData);
                ReposDLProcessing.Remove(repo);
                if (ReposDLProcessing.Count < 1)
                {
                    IsEndDownload = true;
                }
            }
        }
        catch (System.Exception ex)
        {
            SuperNewRolesPlugin.Instance.Log.LogError("VisorsError: " + ex.ToString());
        }
        return HttpStatusCode.OK;
    }
}