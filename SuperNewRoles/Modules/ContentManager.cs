using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace SuperNewRoles.Modules;
public static class ContentManager
{
    private readonly static Dictionary<string, DownloadedContent> Contents = new();
    private readonly static string BasePath = $@"{Path.GetDirectoryName(Application.dataPath)}\SuperNewRoles\DownloadContent\";
    private readonly static DirectoryInfo directory = new(BasePath);
    private const string ContentURL = "https://raw.githubusercontent.com/ykundesu/SupernewRolesData/main/Contents";
    public static void Load()
    {
        if (!directory.Exists)
            directory.Create();
    }
    private static Stream GetStream(DownloadedContent content)
    {
        if (content.Encrypted)
        {
            using (Aes aes = Aes.Create())
            {
                // 復号器を用意
                using (var decryptor = aes.CreateDecryptor(GK(content.WebPath), aes.IV))
                {
                    // 入力ファイルストリーム
                    using (FileStream in_fs = new FileStream(content.file.FullName, FileMode.Open, FileAccess.Read))
                    {
                        // 復号して一定サイズずつ読み出し、出力ファイルストリームに書き出す
                        using (CryptoStream cs = new CryptoStream(in_fs, decryptor, CryptoStreamMode.Read))
                        {
                            // 先頭16バイトは不要なのでまず復号して破棄
                            byte[] dummy = new byte[16];
                            cs.Read(dummy, 0, 16);
                            return cs;
                        }
                    }
                }
            }
        }
        return new FileStream(content.file.FullName, FileMode.Open, FileAccess.Read);
    }
    public static T GetContent<T>(string path, T defaultvalue = default)
    {
        if (!Contents.TryGetValue(path, out DownloadedContent content))
        {
            Logger.Info("一覧からの取得に失敗しました。");
            return defaultvalue;
        }
        if (content.Value == null || content.Value is not T)
        {
            string[] pathes = path.Split(".");
            switch (pathes[pathes.Length - 1])
            {
                case "ogg":
                    using (var vorbis = new NVorbis.VorbisReader(GetStream(content)))
                    {
                        Debug.Log($"Found ogg ch={vorbis.Channels} freq={vorbis.SampleRate} samp={vorbis.TotalSamples}");
                        float[] _audioBuffer = new float[vorbis.TotalSamples]; // Just dump everything
                        int read = vorbis.ReadSamples(_audioBuffer, 0, (int)vorbis.TotalSamples);
                        AudioClip audioClip = AudioClip.Create("NONAMECONTENTCLIP", (int)(vorbis.TotalSamples / vorbis.Channels), vorbis.Channels, vorbis.SampleRate, false);
                        audioClip.SetData(_audioBuffer, 0);
                        content.Value = audioClip;
                    }
                    break;
                default:
                    Logger.Info($"このタイプは対応していません。対応してください。パス：{path}、拡張子:{path[path.Length - 1]}");
                    return defaultvalue;
            }
        }
        if (content.Value == null || content.Value is not T)
        {
            Logger.Info("正常に取得できませんでした。");
            return defaultvalue;
        }
        return (T)content.Value;
    }
    public static IEnumerator Download()
    {
        var request = UnityWebRequest.Get($"{ContentURL}/DownloadData.json");
        yield return request.SendWebRequest();
        if (request.isNetworkError || request.isHttpError)
        {
            Logger.Info("Content一覧の取得に失敗しました。");
            yield break;
        }
        var json = JObject.Parse(request.downloadHandler.text);
        for (var ct = json["Contents"].First; ct != null; ct = ct.Next)
        {
            DownloadedContent dc = new()
            {
                WebPath = ct["path"]?.ToString(),
                hash = ct["hash"]?.ToString(),
                Encrypted = (bool)ct["encrypted"]
            };
            Contents.Add(dc.WebPath, dc);
        }
        foreach (DownloadedContent content in Contents.Values)
        {
            content.file = new(BasePath + content.WebPath);
            if (content.file.Exists)
            {
                FileStream stream = content.file.Open(FileMode.Open);
                //ファイルが違う、やファイルの改ざんをチェック
                content.Downloaded = content.hash == HashDepot.XXHash.Hash32(stream).ToString();
            }
        }
        foreach (DownloadedContent content in Contents.Values)
        {
            if (!content.Downloaded)
            {
                request = UnityWebRequest.Get($"{ContentURL}/DownloadData.json");
                yield return request.SendWebRequest();
                if (request.isNetworkError || request.isHttpError)
                {
                    Logger.Info("Content一覧の取得に失敗しました。");
                    yield break;
                }

                BinaryWriter writer = new(content.file.Open(FileMode.OpenOrCreate));
                writer.Write(request.downloadHandler.data);
                writer.Close();
                content.Downloaded = true;
            }
        }
    }
    public static byte[] GK(string k)
    {
        int l = 32;var r = new System.Random(k.GetHashCode());
        var s = new StringBuilder();
        for (int i = 0; i < l; i++)
        {
            char c = (char)r.Next(97, 122);
            s.Append(c);
        }
        return Encoding.ASCII.GetBytes(s.ToString());
    }
    //暗号化をする。コードでは使わない。
    public static void Encrypt(string WebPath,string Ex)
    {
        Logger.Info("a");
        using (Aes aes = Aes.Create())
        {
            // Encryptorを用意
            using (ICryptoTransform encryptor = aes.CreateEncryptor(GK(WebPath), aes.IV))
            {
                // 入力ファイルストリーム
                using (FileStream in_stream = new FileStream(BasePath+"base."+Ex, FileMode.Open, FileAccess.Read))
                {
                    // 暗号化したデータを書き出すための出力ファイルストリーム
                    string out_filepath = BasePath + "ato."+Ex;
                    using (FileStream out_fs = new FileStream(out_filepath, FileMode.Create, FileAccess.Write))
                    {
                        // 一定サイズずつ暗号化して出力ファイルストリームに書き出す
                        using (CryptoStream cs = new CryptoStream(out_fs, encryptor, CryptoStreamMode.Write))
                        {
                            // 先頭16バイトは適当な値(いまはゼロ)で埋める
                            byte[] dummy = new byte[16];
                            cs.Write(dummy, 0, 16);

                            // 一定量ずつ暗号化して書き込み
                            byte[] buffer = new byte[8192];
                            int len = 0;
                            while ((len = in_stream.Read(buffer, 0, 8192)) > 0)
                            {
                                cs.Write(buffer, 0, len);
                            }
                        }
                    }
                }
            }
        }
        Logger.Info("b");
        FileStream o_stream = new FileStream(BasePath + "ato." + Ex, FileMode.Open, FileAccess.Read);
        Logger.Info("HASH:" + HashDepot.XXHash.Hash32(o_stream).ToString());
    }
}
public class DownloadedContent
{
    public bool Downloaded = false;
    public string hash;
    public bool Encrypted;
    public string path;
    public string WebPath;
    public object Value;
    public FileInfo file;
}