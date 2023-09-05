using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using AmongUs.Data;

namespace SuperNewRoles.Modules;

public static class ModTranslation
{
    // 一番左と一行全部
    private static Dictionary<string, string[]> dictionary = new();
    private static readonly HashSet<string> outputtedStr = new();
    public static string GetString(string key)
    {
        // アモアス側の言語読み込みが完了しているか ? 今の言語 : 最後の言語
        SupportedLangs langId = DestroyableSingleton<TranslationController>.InstanceExists ? FastDestroyableSingleton<TranslationController>.Instance.currentLanguage.languageID : DataManager.Settings.Language.CurrentLanguage;

        if (!dictionary.TryGetValue(key, out string[] values)) return key; // keyが辞書にないならkeyのまま返す

        if (langId is SupportedLangs.SChinese or SupportedLangs.TChinese)
        {
            if (langId == SupportedLangs.SChinese && (values.Length < 4 || values[3] == ""))
            { //簡体中国語がない場合英語で返す
                if (!outputtedStr.Contains(key))
                    Logger.Info($"SChinese not found:{key}", "ModTranslation");
                outputtedStr.Add(key);
                return values[1];
            }

            if (langId == SupportedLangs.TChinese && (values.Length < 5 || values[4] == ""))
            { //繁体中国語がない場合英語で返す
                if (!outputtedStr.Contains(key))
                    Logger.Info($"TChinese not found:{key}", "ModTranslation");
                outputtedStr.Add(key);
                return values[1];
            }
        }

        return langId switch
        {
            SupportedLangs.English => values[1], // 英語
            SupportedLangs.Japanese => values[2],// 日本語
            SupportedLangs.SChinese => values[3],// 簡体中国語
            SupportedLangs.TChinese => values[4],// 繁体中国語
            _ => values[1] // それ以外は英語
        };
    }

    /// <summary>
    /// 翻訳語の文章から翻訳キーを取得する。
    /// CustomOptionで追加しているカラータグは先に外してください。
    /// </summary>
    /// <param name="value">keyを取得したい翻訳後の文</param>
    /// <returns>
    /// string : keyが存在 => key / keyが存在しない => 引数をそのまま返す
    /// bool : true => keyの取得に成功 / false => keyの取得に失敗
    /// </returns>
    internal static (string, bool) GetTranslateKey(string value)
    {
        SupportedLangs langId = TranslationController.InstanceExists ? TranslationController.Instance.currentLanguage.languageID : DataManager.Settings.Language.CurrentLanguage;

        int index = langId switch
        {
            SupportedLangs.English => 1,
            SupportedLangs.Japanese => 2,
            SupportedLangs.SChinese => 3,
            SupportedLangs.TChinese => 4,
            _ => 1,
        };

        string key = dictionary.FirstOrDefault(x => x.Value[index].Equals(value)).Key;
        if (key != null)
        {
            Logger.Info($"{key}", "ModTranslation");
            return (key, true);
        }
        else
        {
            Logger.Info($"key not found:{value}", "ModTranslation");
            return (value, false);
        }
    }

    public static void LoadCsv()
    {
        var fileName = Assembly.GetExecutingAssembly().GetManifestResourceStream("SuperNewRoles.Resources.Translate.csv");

        //csvを開く
        StreamReader sr = new(fileName);

        var i = 0;
        //1行ずつ処理
        while (!sr.EndOfStream)
        {
            try
            {
                // 行ごとの文字列
                string line = sr.ReadLine();

                // 行が空白 戦闘が*なら次の行に
                if (line == "" || line[0] == '#') continue;

                //カンマで配列の要素として分ける
                string[] values = line.Split(',');

                // 配列から辞書に格納する
                List<string> valuesList = new();
                foreach (string vl in values)
                {
                    valuesList.Add(vl.Replace("\\n", "\n").Replace("，", ","));
                }
                dictionary.Add(values[0], valuesList.ToArray());
                i++;
            }
            catch
            {
                Logger.Error($"Error: Loading Translate.csv Line:{i}", "ModTranslation");
            }
        }
    }
}