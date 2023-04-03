using System.Collections.Generic;
using System.IO;
using System.Reflection;
using AmongUs.Data;

namespace SuperNewRoles.Modules;

public static class ModTranslation
{
    // 一番左と一行全部
    private static Dictionary<string, string[]> dictionary = new();
    private static readonly List<string> outputtedStr = new();
    public static string GetString(string key)
    {
        // アモアス側の言語読み込みが完了しているか ? 今の言語 : 最後の言語
        SupportedLangs langId = TranslationController.InstanceExists ? TranslationController.Instance.currentLanguage.languageID : DataManager.Settings.Language.CurrentLanguage;

        if (!dictionary.ContainsKey(key)) return key; // keyが辞書にないならkeyのまま返す

        if (dictionary[key].Length < 4 || dictionary[key][3] == "")
        { //簡体中国語がない場合英語で返す
            if (!outputtedStr.Contains(key))
                Logger.Info($"SChinese not found:{key}", "ModTranslation");
            outputtedStr.Add(key);
            if (langId == SupportedLangs.SChinese) return dictionary[key][1];
        }

        if (dictionary[key].Length < 5 || dictionary[key][4] == "")
        { //繁体中国語がない場合英語で返す
            if (!outputtedStr.Contains(key))
                Logger.Info($"TChinese not found:{key}", "ModTranslation");
            outputtedStr.Add(key);
            if (langId == SupportedLangs.TChinese) return dictionary[key][1];
        }

        return langId switch
        {
            SupportedLangs.English => dictionary[key][1], // 英語
            SupportedLangs.Japanese => dictionary[key][2],// 日本語
            SupportedLangs.SChinese => dictionary[key][3],// 簡体中国語
            SupportedLangs.TChinese => dictionary[key][4],// 繁体中国語
            _ => dictionary[key][1] // それ以外は英語
        };
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