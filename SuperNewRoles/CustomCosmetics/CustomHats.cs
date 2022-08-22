using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Security.Cryptography;
using System.Threading.Tasks;
using BepInEx.IL2CPP.Utils;
using HarmonyLib;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace SuperNewRoles.CustomCosmetics
{
    [HarmonyPatch]
    public class CustomHats
    {
        public static Material hatShader;

        public static Dictionary<string, HatExtension> CustomHatRegistry = new();
        public static HatExtension TestExt = null;
        public static bool IsEnd = false;

        public class HatExtension
        {
            public string author { get; set; }
            public string package { get; set; }
            public string condition { get; set; }
            public Sprite FlipImage { get; set; }
            public Sprite BackFlipImage { get; set; }
        }

        public class CustomHat
        {
            public string author { get; set; }
            public string package { get; set; }
            public string condition { get; set; }
            public string name { get; set; }
            public string resource { get; set; }
            public string flipresource { get; set; }
            public string backflipresource { get; set; }
            public string backresource { get; set; }
            public string climbresource { get; set; }
            public bool bounce { get; set; }
            public bool adaptive { get; set; }
            public bool behind { get; set; }
        }

        private static List<CustomHat> CreateCustomHatDetails(string[] hats, bool fromDisk = false)
        {
            Dictionary<string, CustomHat> fronts = new();
            Dictionary<string, string> backs = new();
            Dictionary<string, string> flips = new();
            Dictionary<string, string> backflips = new();
            Dictionary<string, string> climbs = new();

            for (int i = 0; i < hats.Length; i++)
            {
                string s = fromDisk ? hats[i][(hats[i].LastIndexOf("\\") + 1)..].Split('.')[0] : hats[i].Split('.')[3];
                string[] p = s.Split('_');

                HashSet<string> options = new();
                for (int j = 1; j < p.Length; j++)
                    options.Add(p[j]);

                if (options.Contains("back") && options.Contains("flip"))
                    backflips.Add(p[0], hats[i]);
                else if (options.Contains("climb"))
                    climbs.Add(p[0], hats[i]);
                else if (options.Contains("back"))
                    backs.Add(p[0], hats[i]);
                else if (options.Contains("flip"))
                    flips.Add(p[0], hats[i]);
                else
                {
                    CustomHat custom = new()
                    {
                        resource = hats[i],
                        name = p[0].Replace('-', ' '),
                        bounce = options.Contains("bounce"),
                        adaptive = options.Contains("adaptive"),
                        behind = options.Contains("behind")
                    };

                    fronts.Add(p[0], custom);
                }
            }

            List<CustomHat> customhats = new();

            foreach (string k in fronts.Keys)
            {
                CustomHat hat = fronts[k];
                backs.TryGetValue(k, out string br);
                climbs.TryGetValue(k, out string cr);
                flips.TryGetValue(k, out string fr);
                backflips.TryGetValue(k, out string bfr);
                if (br != null)
                    hat.backresource = br;
                if (cr != null)
                    hat.climbresource = cr;
                if (fr != null)
                    hat.flipresource = fr;
                if (bfr != null)
                    hat.backflipresource = bfr;
                if (hat.backresource != null)
                    hat.behind = true;
                customhats.Add(hat);
            }
            return customhats;
        }

        private static Sprite CreateHatSprite(string path, bool fromDisk = false)
        {
            Texture2D texture = fromDisk ? ModHelpers.LoadTextureFromDisk(path) : ModHelpers.LoadTextureFromResources(path);
            if (texture == null)
                return null;
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.53f, 0.575f), texture.width * 0.375f);
            if (sprite == null)
                return null;
            texture.hideFlags |= HideFlags.HideAndDontSave | HideFlags.DontUnloadUnusedAsset;
            sprite.hideFlags |= HideFlags.HideAndDontSave | HideFlags.DontUnloadUnusedAsset;
            return sprite;
        }

        private static HatData CreateHatData(CustomHat ch, bool fromDisk = false, bool testOnly = false)
        {
            if (hatShader == null && DestroyableSingleton<HatManager>.InstanceExists)
                hatShader = new Material(Shader.Find("Unlit/PlayerShader"));

            HatData hat = new();
            hat.hatViewData.viewData = new HatViewData
            {
                MainImage = CreateHatSprite(ch.resource, fromDisk)
            };
            if (ch.backresource != null)
            {
                hat.hatViewData.viewData.BackImage = CreateHatSprite(ch.backresource, fromDisk);
                ch.behind = true; // Required to view backresource
            }
            if (ch.climbresource != null)
                hat.hatViewData.viewData.ClimbImage = CreateHatSprite(ch.climbresource, fromDisk);
            hat.name = ch.name + "\nby " + ch.author;
            hat.displayOrder = 99;
            hat.ProductId = "MOD_" + ch.package + "_" + ch.name.Replace(' ', '_');
            hat.InFront = !ch.behind;
            hat.NoBounce = !ch.bounce;
            hat.ChipOffset = new Vector2(0f, 0.2f);
            hat.Free = true;
            hat.NotInStore = true;

            if (ch.adaptive && hatShader != null)
                hat.hatViewData.viewData.AltShader = hatShader;

            HatExtension extend = new()
            {
                author = ch.author ?? "Unknown",
                package = ch.package ?? "YJ*白桜コレクション",
                condition = ch.condition ?? "none",
            };

            if (ch.flipresource != null)
                extend.FlipImage = CreateHatSprite(ch.flipresource, fromDisk);
            if (ch.backflipresource != null)
                extend.BackFlipImage = CreateHatSprite(ch.backflipresource, fromDisk);

            if (testOnly)
            {
                TestExt = extend;
                TestExt.condition = hat.name;
            }
            else
            {
                CustomHatRegistry.Add(hat.name, extend);
            }
            return hat;
        }

        private static HatData CreateHatData(CustomHatLoader.CustomHatOnline chd)
        {
            string filePath = Path.GetDirectoryName(Application.dataPath) + @"\SuperNewRoles\CustomHatsChache\";
            chd.resource = filePath + chd.resource;
            if (chd.backresource != null)
                chd.backresource = filePath + chd.backresource;
            if (chd.climbresource != null)
                chd.climbresource = filePath + chd.climbresource;
            if (chd.flipresource != null)
                chd.flipresource = filePath + chd.flipresource;
            if (chd.backflipresource != null)
                chd.backflipresource = filePath + chd.backflipresource;
            return CreateHatData(chd, true);
        }

        [HarmonyPatch(typeof(HatManager), nameof(HatManager.GetHatById))]
        public static class HatManagerPatch
        {
            private static bool LOADED;
            private static bool RUNNING = false;
            public static bool IsLoadingnow = false;

            static void Prefix(HatManager __instance)
            {
                if (!IsEnd) return;
                if (RUNNING) return;
                RUNNING = true; // prevent simultanious execution
                AmongUsClient.Instance.StartCoroutine(LoadHat(__instance));
            }

            static IEnumerator LoadHat(HatManager __instance)
            {
                IsLoadingnow = true;
                if (!LOADED)
                {
                    Assembly assembly = Assembly.GetExecutingAssembly();
                    string hatres = $"{assembly.GetName().Name}.Resources.CustomHats";
                    string[] hats = (from r in assembly.GetManifestResourceNames()
                                     where r.StartsWith(hatres) && r.EndsWith(".png")
                                     select r).ToArray<string>();

                    List<CustomHat> customhats = CreateCustomHatDetails(hats);
                    foreach (CustomHat ch in customhats)
                    {
                        __instance.allHats.Add(CreateHatData(ch));
                        yield return new WaitForSeconds(0.05f);
                    }
                }
                while (CustomHatLoader.hatDetails.Count > 0)
                {
                    __instance.allHats.Add(CreateHatData(CustomHatLoader.hatDetails[0]));
                    CustomHatLoader.hatDetails.RemoveAt(0);
                    yield return new WaitForSeconds(0.05f);
                }
                LOADED = true;
                IsLoadingnow = false;
            }
        }


        [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.HandleAnimation))]
        private static class PlayerPhysicsHandleAnimationPatch
        {
            private static void Postfix(PlayerPhysics __instance)
            {
                AnimationClip currentAnimation = __instance.Animator.GetCurrentAnimation();
                if (currentAnimation == __instance.CurrentAnimationGroup.ClimbAnim || currentAnimation == __instance.CurrentAnimationGroup.ClimbDownAnim) return;
                HatParent hp = __instance.myPlayer.HatRenderer();
                if (hp.Hat == null) return;
                HatExtension extend = hp.Hat.GetHatExtension();
                if (extend == null) return;
                if (extend.FlipImage != null)
                {
                    hp.FrontLayer.sprite = __instance.Rend().flipX ? extend.FlipImage : hp.Hat.hatViewData.viewData.MainImage;
                }
                if (extend.BackFlipImage != null)
                {
                    hp.BackLayer.sprite = __instance.Rend().flipX ? extend.BackFlipImage : hp.Hat.hatViewData.viewData.BackImage;
                }
            }
        }

        [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Begin))]
        private static class ShipStatusSetHat
        {
            static void Postfix(ShipStatus __instance)
            {
                if (DestroyableSingleton<TutorialManager>.InstanceExists)
                {
                    string filePath = Path.GetDirectoryName(Application.dataPath) + @"\SuperNewRoles\CustomHatsChache\Test";
                    DirectoryInfo d = new(filePath);
                    string[] filePaths = d.GetFiles("*.png").Select(x => x.FullName).ToArray(); // Getting Text files
                    List<CustomHat> hats = CreateCustomHatDetails(filePaths, true);
                    if (hats.Count > 0)
                    {
                        foreach (PlayerControl pc in CachedPlayer.AllPlayers)
                        {
                            var color = pc.CurrentOutfit.ColorId;
                            pc.SetHat("hat_dusk", color);
                            pc.HatRenderer().Hat = CreateHatData(hats[0], true, true);
                            pc.HatRenderer().SetHat(color);
                        }
                    }
                }
            }
        }

        private static readonly List<TMPro.TMP_Text> hatsTabCustomTexts = new();
        public static string innerslothPackageName = "innerslothHats";
        private static readonly float headerSize = 0.8f;
        private static readonly float headerX = 0.8f;
        private static float inventoryTop = 1.5f;
        private static float inventoryBot = -2.5f;
        private static readonly float inventoryZ = -2f;

        public static void CalcItemBounds(HatsTab __instance)
        {
            inventoryTop = __instance.scroller.Inner.position.y - 0.5f;
            inventoryBot = __instance.scroller.Inner.position.y - 4.5f;
        }

        [HarmonyPatch(typeof(HatsTab), nameof(HatsTab.OnEnable))]
        public class HatsTabOnEnablePatch
        {
            public static TMPro.TMP_Text textTemplate;

            public static float CreateHatPackage(List<System.Tuple<HatData, HatExtension>> hats, string packageName, float YStart, HatsTab __instance)
            {
                float offset = YStart;
                if (textTemplate != null)
                {
                    TMPro.TMP_Text title = UnityEngine.Object.Instantiate<TMPro.TMP_Text>(textTemplate, __instance.scroller.Inner);
                    title.transform.parent = __instance.scroller.Inner;
                    title.transform.localPosition = new Vector3(headerX, YStart, inventoryZ);
                    title.alignment = TMPro.TextAlignmentOptions.Center;
                    title.fontSize *= 1.25f;
                    title.fontWeight = TMPro.FontWeight.Thin;
                    title.enableAutoSizing = false;
                    title.autoSizeTextContainer = true;
                    title.text = ModTranslation.GetString(packageName);
                    switch (packageName)
                    {
                        case "shiuneCollection":
                            title.text = "しうねコレクション";
                            break;
                        case "gmEditionGeneral":
                            title.text = "TheOtherRoles-GMハット";
                            break;
                        case "communityHats":
                            title.text = "TheOtherRolesコミュニティーハット";
                            break;
                        case "developerHats":
                            title.text = "TheOtherRoles開発者ハット";
                            break;
                    }
                    offset -= headerSize * __instance.YOffset;
                    hatsTabCustomTexts.Add(title);
                }

                var numHats = hats.Count;

                int i2 = 0;
                for (int i = 0; i < hats.Count; i++)
                {
                    HatData hat = hats[i].Item1;
                    HatExtension ext = hats[i].Item2;

                    float xpos = __instance.XRange.Lerp((i2 % __instance.NumPerRow) / (__instance.NumPerRow - 1f));
                    float ypos = offset - (i2 / __instance.NumPerRow) * __instance.YOffset;
                    ColorChip colorChip = UnityEngine.Object.Instantiate<ColorChip>(__instance.ColorTabPrefab, __instance.scroller.Inner);

                    int color = __instance.HasLocalPlayer() ? CachedPlayer.LocalPlayer.Data.DefaultOutfit.ColorId : SaveManager.BodyColor;

                    colorChip.transform.localPosition = new Vector3(xpos, ypos, inventoryZ);
                    if (ActiveInputManager.currentControlType == ActiveInputManager.InputType.Keyboard)
                    {
                        colorChip.Button.OnMouseOver.AddListener((UnityEngine.Events.UnityAction)(() => __instance.SelectHat(hat)));
                        colorChip.Button.OnMouseOut.AddListener((UnityEngine.Events.UnityAction)(() => __instance.SelectHat(DestroyableSingleton<HatManager>.Instance.GetHatById(SaveManager.LastHat))));
                        colorChip.Button.OnClick.AddListener((UnityEngine.Events.UnityAction)(() => __instance.ClickEquip()));
                    }
                    else
                    {
                        colorChip.Button.OnClick.AddListener((UnityEngine.Events.UnityAction)(() => __instance.SelectHat(hat)));
                    }

                    colorChip.Inner.SetHat(hat, color);
                    colorChip.Inner.transform.localPosition = hat.ChipOffset;
                    colorChip.Tag = hat;
                    colorChip.Button.ClickMask = __instance.scroller.Hitbox;
                    __instance.ColorChips.Add(colorChip);
                    Chips.Add(colorChip);
                    i2++;
                }
                return offset - ((numHats - 1) / __instance.NumPerRow) * __instance.YOffset - headerSize;
            }
            public static List<ColorChip> Chips;
            public static bool Prefix(HatsTab __instance)
            {
                CalcItemBounds(__instance);

                HatData[] unlockedHats = DestroyableSingleton<HatManager>.Instance.GetUnlockedHats();
                Dictionary<string, List<System.Tuple<HatData, HatExtension>>> packages = new();

                ModHelpers.DestroyList(hatsTabCustomTexts);
                ModHelpers.DestroyList(__instance.ColorChips);

                hatsTabCustomTexts.Clear();
                __instance.ColorChips.Clear();

                textTemplate = PlayerCustomizationMenu.Instance.itemName;

                foreach (HatData hatData in unlockedHats)
                {
                    HatExtension ext = hatData.GetHatExtension();

                    if (ext != null)
                    {
                        if (!packages.ContainsKey(ext.package))
                            packages[ext.package] = new List<System.Tuple<HatData, HatExtension>>();
                        packages[ext.package].Add(new System.Tuple<HatData, HatExtension>(hatData, ext));
                    }
                    else
                    {
                        if (!packages.ContainsKey(innerslothPackageName))
                            packages[innerslothPackageName] = new List<System.Tuple<HatData, HatExtension>>();
                        packages[innerslothPackageName].Add(new System.Tuple<HatData, HatExtension>(hatData, null));
                    }
                }

                float YOffset = __instance.YStart;

                var orderedKeys = packages.Keys.OrderBy((string x) =>
                {
                    return x == innerslothPackageName
                        ? 100003
                        : x == "developerHats"
                        ? 20
                        : x.Contains("gmEdition") ? 40 : x.Contains("shiune") ? 30 : x.Contains("01haomingHat") ? 10 : x.Contains("Hat_SNR") ? 0 : 500;
                });

                foreach (string key in orderedKeys)
                {
                    List<System.Tuple<HatData, HatExtension>> value = packages[key];
                    YOffset = CreateHatPackage(value, key, YOffset, __instance);
                }

                __instance.scroller.ContentYBounds.max = -(YOffset + 3.0f + headerSize);
                return false;
            }
        }
        public static List<string> Keys = new();
        [HarmonyPatch(typeof(HatsTab), nameof(HatsTab.Update))]
        public class HatsTabUpdatePatch
        {
            public static bool Prefix()
            {
                foreach (TMPro.TMP_Text customText in hatsTabCustomTexts)
                {
                    if (customText != null && customText.transform != null && customText.gameObject != null)
                    {
                        bool active = customText.transform.position.y <= inventoryTop && customText.transform.position.y >= inventoryBot;
                        float epsilon = Mathf.Min(Mathf.Abs(customText.transform.position.y - inventoryTop), Mathf.Abs(customText.transform.position.y - inventoryBot));
                        if (active != customText.gameObject.active && epsilon > 0.1f) customText.gameObject.SetActive(active);
                    }
                }
                return true;
            }

            public static void Postfix(HatsTab __instance)
            {
            }
        }
    }

    public class CustomHatLoader
    {
        public static bool running = false;

        //レポURL、レポ
        public static Dictionary<string, string> hatRepos = new()
        {
            { "https://raw.githubusercontent.com/ykundesu/SuperNewNamePlates/master", "SuperNewNamePlates" },

            { "https://raw.githubusercontent.com/hinakkyu/TheOtherHats/master", "mememurahat" },
            { "https://raw.githubusercontent.com/Ujet222/TOPHats/main", "YJ" },

            { "https://raw.githubusercontent.com/haoming37/TheOtherHats-GM-Haoming/master", "TheOtherRolesGMHaoming"},
            { "https://raw.githubusercontent.com/yukinogatari/TheOtherHats-GM/master", "TheOtherRolesGM"},
            { "https://raw.githubusercontent.com/Eisbison/TheOtherHats/master", "TheOtherHats"},
        };

        public static List<string> CachedRepos = new();
        public static List<CustomHatOnline> hatDetails = new();
        private static Task hatFetchTask = null;
        public static void LaunchHatFetcher()
        {
            if (running)
                return;
            running = true;
            hatFetchTask = LaunchHatFetcherAsync();
        }

        private static async Task LaunchHatFetcherAsync()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(Application.dataPath) + @"\SuperNewRoles\");
            Directory.CreateDirectory(Path.GetDirectoryName(Application.dataPath) + @"\SuperNewRoles\CustomHatsChache\");
            hatDetails = new List<CustomHatOnline>();
            List<string> repos = new(hatRepos.Keys);
            SuperNewRolesPlugin.Logger.LogInfo("[CustomHats] フェチ");
            foreach (string repo in repos)
            {
                Repos.Add(repo);
            }
            if (!ConfigRoles.DownloadSuperNewNamePlates.Value) return;
            string filePath = Path.GetDirectoryName(Application.dataPath) + @"\SuperNewRoles\CustomHatsChache\";
            foreach (string repo in repos)
            {
                if (File.Exists($"{filePath}\\{hatRepos.FirstOrDefault(data => data.Key == repo).Value}.json"))
                {
                    StreamReader sr = new($"{filePath}\\{hatRepos.FirstOrDefault(data => data.Key == repo).Value}.json");

                    string text = sr.ReadToEnd();

                    sr.Close();

                    JToken jobj = JObject.Parse(text)["hats"];
                    if (jobj.HasValues)
                    {

                        List<CustomHatOnline> hatdatas = new();

                        for (JToken current = jobj.First; current != null; current = current.Next)
                        {
                            if (current.HasValues)
                            {
                                CustomHatOnline info = new()
                                {
                                    name = current["name"]?.ToString(),
                                    author = current["author"]?.ToString(),
                                    resource = SanitizeResourcePath(current["resource"]?.ToString())
                                };
                                if (info.resource == null || info.name == null) // required
                                    continue;
                                info.reshasha = info.resource + info.name + info.author;
                                info.backresource = SanitizeResourcePath(current["backresource"]?.ToString());
                                info.reshashb = current["reshashb"]?.ToString();
                                info.climbresource = SanitizeResourcePath(current["climbresource"]?.ToString());
                                info.reshashc = current["reshashc"]?.ToString();
                                info.flipresource = SanitizeResourcePath(current["flipresource"]?.ToString());
                                info.reshashf = current["reshashf"]?.ToString();
                                info.backflipresource = SanitizeResourcePath(current["backflipresource"]?.ToString());
                                info.reshashbf = current["reshashbf"]?.ToString();

                                info.package = current["package"]?.ToString();
                                SuperNewRolesPlugin.Logger.LogInfo(info.package);
                                if (info.package != null && !CustomHats.Keys.Contains(info.package))
                                {
                                    CustomHats.Keys.Add(info.package);
                                }
                                info.condition = current["condition"]?.ToString();
                                info.bounce = current["bounce"] != null;
                                info.adaptive = current["adaptive"] != null;
                                info.behind = current["behind"] != null;

                                if (info.package == "Developer Hats")
                                    info.package = "developerHats";

                                if (info.package == "Community Hats")
                                    info.package = "communityHats";

                                hatdatas.Add(info);
                            }
                        }
                        CustomHats.Keys.Add("InnerSloth");

                        hatDetails.AddRange(hatdatas);
                        CachedRepos.Add(repo);
                        Repos.Remove(repo);
                        if (Repos.Count < 1)
                        {
                            CustomHats.IsEnd = true;
                        }
                    }
                }
            }
            foreach (string repo in repos)
            {
                SuperNewRolesPlugin.Logger.LogInfo("[CustomHats] ハットスタート:" + repo);
                if (!ConfigRoles.DownloadSuperNewNamePlates.Value)
                {
                    SuperNewRolesPlugin.Logger.LogInfo("ダウンロードをスキップしました:"/*"Skipped download.:"*/ + repo);
                }
                else
                {
                    try
                    {
                        HttpStatusCode status = await FetchHats(repo);
                        if (status != HttpStatusCode.OK)
                            System.Console.WriteLine($"Custom hats could not be loaded from repo: {repo}\n");
                        else
                            SuperNewRolesPlugin.Logger.LogInfo("ハット終了:" + repo);
                    }
                    catch (System.Exception e)
                    {
                        System.Console.WriteLine($"Unable to fetch hats from repo: {repo}\n" + e.Message);
                    }
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
        public static List<string> Repos = new();

        public static async Task<HttpStatusCode> FetchHats(string repo)
        {
            HttpClient http = new();
            http.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue { NoCache = true };
            var response = await http.GetAsync(new System.Uri($"{repo}/CustomHats.json"), HttpCompletionOption.ResponseContentRead);
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
                string filePath = Path.GetDirectoryName(Application.dataPath) + @"\SuperNewRoles\CustomHatsChache\";
                responsestream.CopyTo(File.Create($"{filePath}\\{hatRepos.FirstOrDefault(data => data.Key == repo).Value}.json"));
                JToken jobj = JObject.Parse(json)["hats"];
                if (!jobj.HasValues) return HttpStatusCode.ExpectationFailed;

                List<CustomHatOnline> hatdatas = new();

                for (JToken current = jobj.First; current != null; current = current.Next)
                {
                    if (current.HasValues)
                    {
                        CustomHatOnline info = new()
                        {
                            name = current["name"]?.ToString(),
                            author = current["author"]?.ToString(),
                            resource = SanitizeResourcePath(current["resource"]?.ToString())
                        };
                        if (info.resource == null || info.name == null) // required
                            continue;
                        info.reshasha = info.resource + info.name + info.author;
                        info.backresource = SanitizeResourcePath(current["backresource"]?.ToString());
                        info.reshashb = current["reshashb"]?.ToString();
                        info.climbresource = SanitizeResourcePath(current["climbresource"]?.ToString());
                        info.reshashc = current["reshashc"]?.ToString();
                        info.flipresource = SanitizeResourcePath(current["flipresource"]?.ToString());
                        info.reshashf = current["reshashf"]?.ToString();
                        info.backflipresource = SanitizeResourcePath(current["backflipresource"]?.ToString());
                        info.reshashbf = current["reshashbf"]?.ToString();

                        info.package = current["package"]?.ToString();
                        SuperNewRolesPlugin.Logger.LogInfo(info.package);
                        if (info.package != null && !CustomHats.Keys.Contains(info.package))
                        {
                            CustomHats.Keys.Add(info.package);
                        }
                        info.condition = current["condition"]?.ToString();
                        info.bounce = current["bounce"] != null;
                        info.adaptive = current["adaptive"] != null;
                        info.behind = current["behind"] != null;

                        if (info.package == "Developer Hats")
                            info.package = "developerHats";

                        if (info.package == "Community Hats")
                            info.package = "communityHats";

                        hatdatas.Add(info);
                    }
                }
                CustomHats.Keys.Add("InnerSloth");

                List<string> markedfordownload = new();

                MD5 md5 = MD5.Create();
                foreach (CustomHatOnline data in hatdatas)
                {
                    if (DoesResourceRequireDownload(filePath + data.resource, data.reshasha, md5))
                        markedfordownload.Add(data.resource);
                    if (data.backresource != null && DoesResourceRequireDownload(filePath + data.backresource, data.reshashb, md5))
                        markedfordownload.Add(data.backresource);
                    if (data.climbresource != null && DoesResourceRequireDownload(filePath + data.climbresource, data.reshashc, md5))
                        markedfordownload.Add(data.climbresource);
                    if (data.flipresource != null && DoesResourceRequireDownload(filePath + data.flipresource, data.reshashf, md5))
                        markedfordownload.Add(data.flipresource);
                    if (data.backflipresource != null && DoesResourceRequireDownload(filePath + data.backflipresource, data.reshashbf, md5))
                        markedfordownload.Add(data.backflipresource);
                }

                foreach (var file in markedfordownload)
                {
                    var hatFileResponse = await http.GetAsync($"{repo}/hats/{file}", HttpCompletionOption.ResponseContentRead);
                    //SuperNewRolesPlugin.Logger.LogInfo(file);
                    if (hatFileResponse.StatusCode != HttpStatusCode.OK) continue;
                    using var responseStream = await hatFileResponse.Content.ReadAsStreamAsync();
                    using var fileStream = File.Create($"{filePath}\\{file}");
                    responseStream.CopyTo(fileStream);
                }
                if (!CachedRepos.Contains(repo))
                {
                    hatDetails.AddRange(hatdatas);
                    Repos.Remove(repo);
                    if (Repos.Count < 1)
                    {
                        CustomHats.IsEnd = true;
                    }
                }
            }
            catch (System.Exception ex)
            {
                SuperNewRolesPlugin.Instance.Log.LogError(ex.ToString());
                System.Console.WriteLine(ex);
            }
            return HttpStatusCode.OK;
        }

        private static bool DoesResourceRequireDownload(string respath, string reshash, MD5 md5)
        {
            if (reshash == null || !File.Exists(respath))
                return true;

            using var stream = File.OpenRead(respath);
            var hash = System.BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "").ToLowerInvariant();
            return !reshash.Equals(hash);
        }

        public class CustomHatOnline : CustomHats.CustomHat
        {
            public string reshasha { get; set; }
            public string reshashb { get; set; }
            public string reshashc { get; set; }
            public string reshashf { get; set; }
            public string reshashbf { get; set; }
        }
    }
    public static class CustomHatExtensions
    {
        public static CustomHats.HatExtension GetHatExtension(this HatData hat)
        {
            if (CustomHats.TestExt != null && CustomHats.TestExt.condition.Equals(hat.name))
            {
                return CustomHats.TestExt;
            }
            CustomHats.CustomHatRegistry.TryGetValue(hat.name, out CustomHats.HatExtension ret);
            return ret;
        }
    }

    [HarmonyPatch(typeof(PoolablePlayer), nameof(PoolablePlayer.UpdateFromPlayerOutfit))]
    public static class PoolablePlayerPatch
    {
        public static void Postfix(PoolablePlayer __instance)
        {
            if (__instance.VisorSlot()?.transform == null || __instance.HatSlot()?.transform == null) return;

            // fixes a bug in the original where the visor will show up beneath the hat,
            // instead of on top where it's supposed to be
            __instance.VisorSlot().transform.localPosition = new Vector3(
                __instance.VisorSlot().transform.localPosition.x,
                __instance.VisorSlot().transform.localPosition.y,
                __instance.HatSlot().transform.localPosition.z - 1
                );
        }
    }
}