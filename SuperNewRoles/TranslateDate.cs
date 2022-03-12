using HarmonyLib;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;

namespace SuperNewRoles
{
    public class ModTranslation
    {
        public static int defaultLanguage = (int)SupportedLangs.English;
        public static Dictionary<string, Dictionary<int, string>> stringData = new Dictionary<string, Dictionary<int, string>>();

        private const string blankText = "[BLANK]";
        public ModTranslation()
        {

        }
        public static dynamic LangDate;
        public static void Load()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            Stream stream = assembly.GetManifestResourceStream("TheOtherRoles.Resources.stringData.json");
            var byteArray = new byte[stream.Length];
            var read = stream.Read(byteArray, 0, (int)stream.Length);
            string json = System.Text.Encoding.UTF8.GetString(byteArray);

            stringData = new Dictionary<string, Dictionary<int, string>>();
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

        public static uint GetLang()
        {
            return SaveManager.LastLanguage;
        }
        public static string getString(string key, string def = null)
        {
            try
            {
                return stringData[key][(int)GetLang()].Replace("\\n", "\n");
            } catch
            {
                try
                {
                    return stringData[key][defaultLanguage].Replace("\\n", "\n");
                }
                catch {
                    return key;
                }
            }
        }

        public static Sprite getImage(string key, float pixelsPerUnit)
        {
            key = key.Replace("/", ".");
            key = key.Replace("\\", ".");
            key = "SuperNewRoles.Resources." + key;

            return ModHelpers.loadSpriteFromResources(key, pixelsPerUnit);
        }
    }

    
}