using System.Collections.Generic;
using System.IO;
using System.Reflection;
using HarmonyLib;
using Newtonsoft.Json.Linq;
using SuperNewRoles.Patch;
using UnityEngine;
using System.Text.RegularExpressions;

namespace SuperNewRoles
{
    public class ModTranslation
    {
        public static int defaultLanguage = (int)SupportedLangs.English;
        public static Dictionary<string, Dictionary<int, string>> stringData = new();

        private const string blankText = "[BLANK]";

        public ModTranslation() { }
        public static dynamic LangDate;
        public static void Load()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            Stream stream = assembly.GetManifestResourceStream("SuperNewRoles.Resources.TranslateFile.json");
            var byteTexture = new byte[stream.Length];
            var read = stream.Read(byteTexture, 0, (int)stream.Length);
            string json = System.Text.Encoding.UTF8.GetString(byteTexture);
            JObject parsed = JObject.Parse(json);
            for (int i = 0; i < parsed.Count; i++)
            {
                JProperty token = parsed.ChildrenTokens[i].TryCast<JProperty>();
                if (token == null) continue;

                string stringName = token.Name;
                var val = token.Value.TryCast<JObject>();
                if (token.HasValues)
                {
                    var strings = new Dictionary<int, string>();
                    for (int j = 0; j < (int)SupportedLangs.Irish + 1; j++)
                    {
                        string key = j.ToString();
                        var text = val[key]?.TryCast<JValue>().Value.ToString();

                        if (text != null && text.Length > 0)
                        {
                            if (text == blankText) strings[j] = "";
                            else strings[j] = text;
                        }
                    }
                    stringData[stringName] = strings;
                }
            }
        }
        public static string GetString(string key, string def = null)
        {
            // Strip out color tags.
            string keyClean = Regex.Replace(key, "<.*?>", "");
            keyClean = Regex.Replace(keyClean, "^-\\s*", "");
            keyClean = keyClean.Trim();

            def = def ?? key;
            if (!stringData.ContainsKey(keyClean))
            {
                return def;
            }

            var data = stringData[keyClean];
            int lang = (int)SaveManager.LastLanguage;

            if (data.ContainsKey(lang))
            {
                return key.Replace(keyClean, data[lang]);
            }
            else if (data.ContainsKey(defaultLanguage))
            {
                return key.Replace(keyClean, data[defaultLanguage]);
            }
            return key;
        }
    }
    [HarmonyPatch(typeof(LanguageSetter), nameof(LanguageSetter.SetLanguage))]
    class SetLanguagePatch
    {
        static void Postfix()
        {
            VanillaOptionsPatch.updateTranslations();
            ClientModOptionsPatch.updateTranslations();
        }
    }
}