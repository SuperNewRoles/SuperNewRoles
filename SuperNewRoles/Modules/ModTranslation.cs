using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace SuperNewRoles.Modules {
    public static class ModTranslation {
        // 一番左と一行全部
        private static Dictionary<string, string[]> dictionary = new();
        public static string GetString(string key) {
            // アモアス側の言語読み込みが完了しているか ? 今の言語 : 日本語
            SupportedLangs langId = TranslationController.InstanceExists ? TranslationController.Instance.currentLanguage.languageID : SupportedLangs.Japanese;

            if (!dictionary.ContainsKey(key)) return key; // keyが辞書にないならkeyのまま返す

            if (dictionary[key].Length < 4) { //中国語がない場合英語で返す
                if (langId == SupportedLangs.SChinese)return dictionary[key][1];
            }

            return langId switch {
                SupportedLangs.English => dictionary[key][1], // 英語
                SupportedLangs.Japanese => dictionary[key][2],// 日本語
                SupportedLangs.SChinese => dictionary[key][3],// 中国語
                _ => dictionary[key][1] // それ以外は英語
            };
        }

        public static void LoadCsv() {
            var fileName = Assembly.GetExecutingAssembly().GetManifestResourceStream("SuperNewRoles.Resources.Translate.csv");

            //csvを開く
            StreamReader sr = new(fileName);

            //1行ずつ処理
            while (!sr.EndOfStream) {
                // 行ごとの文字列
                string line = sr.ReadLine();

                // 行が空白 戦闘が*なら次の行に
                if (line == "" || line[0] == '#') continue;

                //カンマで配列の要素として分ける
                string[] values = line.Split(',');

                // 配列から辞書に格納する
                dictionary.Add(values[0],values);
            }
        }
    }
}