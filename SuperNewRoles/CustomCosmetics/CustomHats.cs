using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Threading.Tasks;
using AmongUs.Data;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using HarmonyLib;
using Innersloth.Assets;
using Newtonsoft.Json.Linq;
using SuperNewRoles.CustomCosmetics.CustomCosmeticsData;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace SuperNewRoles.CustomCosmetics;

[HarmonyPatch]
public class CustomHats
{
    public static Material hatShader;

    public static Dictionary<string, HatExtension> CustomHatRegistry = new();
    public static HatExtension TestExt = new() { IsNull = true };
    public static bool IsEnd = false;

    public struct HatExtension
    {
        public bool IsNull;
        public string author;
        public string package;
        public string condition;
        public Sprite FlipImage;
        public Sprite BackFlipImage;
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
        if (!HatSprites.ContainsKey(path))
            HatSprites.Add(path, sprite);
        return sprite;
    }
    private static Sprite GetHatSprite(string path)
    {
        return HatSprites.ContainsKey(path) ? HatSprites[path] : null;
    }
    static readonly Dictionary<string, Sprite> HatSprites = new();
    private static CustomHatData CreateHatData(CustomHat ch, bool fromDisk = false, bool testOnly = false)
    {
        if (hatShader == null && DestroyableSingleton<HatManager>.InstanceExists)
            hatShader = new Material(Shader.Find("Unlit/PlayerShader"));

        CustomHatData.HatTempViewData hatViewData = new()
        {
            MainImage = GetHatSprite(ch.resource)
        };
        CustomHatData hat = new();
        if (ch.backresource != null)
        {
            hatViewData.BackImage = GetHatSprite(ch.backresource);
            ch.behind = true; // Required to view backresource
        }
        if (ch.climbresource != null)
            hatViewData.ClimbImage = GetHatSprite(ch.climbresource);
        hat.name = ch.name + "\nby " + ch.author;
        hat.displayOrder = 99;
        hat.ProductId = "MOD_" + ch.package + "_" + ch.name.Replace(' ', '_');
        hatViewData.name = hat.ProdId;
        hat.InFront = !ch.behind;
        hat.NoBounce = !ch.bounce;
        hat.ChipOffset = new Vector2(0f, 0.25f);
        hat.Free = true;
        hat.NotInStore = true;

        if (ch.adaptive)
            hatViewData.adaptive = true;

        HatExtension extend = new()
        {
            author = ch.author ?? "Unknown",
            package = ch.package ?? "YJ*白桜コレクション",
            condition = ch.condition ?? "none",
        };

        if (ch.flipresource != null)
            hatViewData.FlipImage = GetHatSprite(ch.flipresource);
        if (ch.backflipresource != null)
            hatViewData.BackFlipImage = GetHatSprite(ch.backflipresource);

        if (testOnly)
        {
            TestExt = extend;
            TestExt.condition = hat.name;
        }
        else
        {
            CustomHatRegistry.Add(hat.name, extend);
        }
        hat.htvd = hatViewData;
        return hat;
    }

    private static CustomHatLoader.CustomHatOnline GenereteHatData(CustomHatLoader.CustomHatOnline chd)
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
        return chd;
    }

    private static CustomHatData CreateHatData(CustomHatLoader.CustomHatOnline chd) => CreateHatData(chd, true);

    [HarmonyPatch(typeof(HatManager), nameof(HatManager.GetHatById))]
    public static class HatManagerPatch
    {
        private static bool LOADED;
        private static bool SPRITELOADED = false;
        private static bool RUNNING = false;
        public static bool IsLoadingnow = false;
        public static List<HatData> hatdata = new();

        static void Prefix(HatManager __instance)
        {
            if (!IsEnd) return;
            if (RUNNING) return;
            if (IsLoadingnow) return;
            if (SPRITELOADED)
            {
                RUNNING = true; // prevent simultanious execution
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
                        addHatData.Add(CreateHatData(ch));
                    }
                }
                while (CustomHatLoader.hatDetails.Count > 0)
                {
                    CustomHatData chdata = CreateHatData(CustomHatLoader.hatDetails[0]);
                    addHatData.Add(chdata);
                    CustomHatLoader.hatDetails.RemoveAt(0);
                }
                LOADED = true;
                var data = __instance.allHats.ToList();
                data.AddRange(addHatData);
                hatdata = data;
                __instance.allHats = new Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<HatData>(data.ToArray());
                IsLoadingnow = false;
            }
            else
            {
                SPRITELOADED = true;
                __instance.StartCoroutine(LoadHatSprite().WrapToIl2Cpp());
            }
        }

        public static readonly List<CustomHatData> addHatData = new();

        static IEnumerator LoadHatSprite()
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
                    CreateHatSprite(ch.resource);
                    yield return new WaitForSeconds(0.0005f);
                    if (ch.climbresource != null)
                    {
                        CreateHatSprite(ch.climbresource);
                        yield return new WaitForSeconds(0.0005f);
                    }
                    if (ch.flipresource != null)
                    {
                        CreateHatSprite(ch.flipresource);
                        yield return new WaitForSeconds(0.0005f);
                    }
                    if (ch.backflipresource != null)
                    {
                        CreateHatSprite(ch.backflipresource);
                        yield return new WaitForSeconds(0.0005f);
                    }
                }
            }
            foreach (var data in CustomHatLoader.hatDetails)
            {
                var hat = GenereteHatData(data);
                CreateHatSprite(hat.resource, true);
                yield return new WaitForSeconds(0.0005f);
                if (hat.backresource != null)
                {
                    CreateHatSprite(hat.backresource, true);
                    yield return new WaitForSeconds(0.0005f);
                }
                if (hat.climbresource != null)
                {
                    CreateHatSprite(hat.climbresource, true);
                    yield return new WaitForSeconds(0.0005f);
                }
                if (hat.flipresource != null)
                {
                    CreateHatSprite(hat.flipresource, true);
                    yield return new WaitForSeconds(0.0005f);
                }
                if (hat.backflipresource != null)
                {
                    CreateHatSprite(hat.backflipresource, true);
                    yield return new WaitForSeconds(0.0005f);
                }
            }
            IsLoadingnow = false;
        }
    }

    /*
    [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.HandleAnimation))]
    private static class PlayerPhysicsHandleAnimationPatch
    {
        private static void Postfix(PlayerPhysics __instance)
        {
            AnimationClip currentAnimation = __instance.Animations.Animator.GetCurrentAnimation();
            if (currentAnimation == __instance.Animations.group.ClimbUpAnim || currentAnimation == __instance.Animations.group.ClimbDownAnim) return;
            HatParent hp = __instance.myPlayer.HatRenderer();
            if (hp.Hat == null) return;
            HatExtension extend = hp.Hat.GetHatExtension();
            if (extend.IsNull) return;
            if (extend.FlipImage != null)
            {
                hp.FrontLayer.sprite = __instance.Rend().flipX ? extend.FlipImage : hp.Hat.CreateAddressableAsset().GetAsset().MainImage;
            }
            if (extend.BackFlipImage != null)
            {
                hp.BackLayer.sprite = __instance.Rend().flipX ? extend.BackFlipImage : hp.Hat.CreateAddressableAsset().GetAsset().BackImage;
            }
        }
    }*/

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
                    CreateHatSprite(hats[0].resource, true);
                    if (hats[0].backresource != null)
                        CreateHatSprite(hats[0].backresource, true);
                    if (hats[0].climbresource != null)
                        CreateHatSprite(hats[0].climbresource, true);
                    if (hats[0].flipresource != null)
                        CreateHatSprite(hats[0].flipresource, true);
                    if (hats[0].backflipresource != null)
                        CreateHatSprite(hats[0].backflipresource, true);
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

                int color = __instance.HasLocalPlayer() ? CachedPlayer.LocalPlayer.Data.DefaultOutfit.ColorId : DataManager.Player.Customization.Color;

                colorChip.transform.localPosition = new Vector3(xpos, ypos, inventoryZ);
                if (ActiveInputManager.currentControlType == ActiveInputManager.InputType.Keyboard)
                {
                    colorChip.Button.OnMouseOver.AddListener((UnityEngine.Events.UnityAction)(() => __instance.SelectHat(hat)));
                    colorChip.Button.OnMouseOut.AddListener((UnityEngine.Events.UnityAction)(() => __instance.SelectHat(FastDestroyableSingleton<HatManager>.Instance.GetHatById(DataManager.Player.Customization.Hat))));
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
            HatData[] unlockedHats = FastDestroyableSingleton<HatManager>.Instance.GetUnlockedHats();
            Dictionary<string, List<System.Tuple<HatData, HatExtension>>> packages = new();

            ModHelpers.DestroyList(hatsTabCustomTexts);
            ModHelpers.DestroyList(__instance.ColorChips);

            hatsTabCustomTexts.Clear();
            __instance.ColorChips.Clear();

            textTemplate = PlayerCustomizationMenu.Instance.itemName;

            foreach (HatData hatData in unlockedHats)
            {
                HatExtension ext = hatData.GetHatExtension();

                if (!ext.IsNull)
                {
                    if (!packages.ContainsKey(ext.package == null ? innerslothPackageName : ext.package))
                        packages[ext.package == null ? innerslothPackageName : ext.package] = new();
                    packages[ext.package == null ? innerslothPackageName : ext.package].Add(new System.Tuple<HatData, HatExtension>(hatData, ext));
                }
                else
                {
                    if (!packages.ContainsKey(innerslothPackageName))
                        packages[innerslothPackageName] = new List<System.Tuple<HatData, HatExtension>>();
                    packages[innerslothPackageName].Add(new System.Tuple<HatData, HatExtension>(hatData, new() { IsNull = true }));
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
            __instance.currentHat = FastDestroyableSingleton<HatManager>.Instance.GetHatById(DataManager.Player.Customization.Hat);
            return false;
        }
    }
    public static List<string> Keys = new();
    [HarmonyPatch(typeof(HatsTab), nameof(HatsTab.Update))]
    public class HatsTabUpdatePatch
    {
        public static bool Prefix(HatsTab __instance)
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
    }
}

public class CustomHatLoader
{
    public static bool running = false;

    //レポURL、レポ
    public static Dictionary<string, string> hatRepos = new()
        {

            { "https://raw.githubusercontent.com/SuperNewRoles/SuperNewCosmetics/master", "SuperNewCosmetics" },

            { "https://raw.githubusercontent.com/hinakkyu/TheOtherHats/master", "mememurahat" },
            { "https://raw.githubusercontent.com/Ujet222/TOPHats/main", "YJ" },
            { "https://raw.githubusercontent.com/catudon1276/Mememura-Hats/main", "MememuraByCatudon" },
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
        if (ConfigRoles.DebugMode.Value) return;
        if (ConfigRoles.IsModCosmeticsAreNotLoaded.Value) return;
        Directory.CreateDirectory(Path.GetDirectoryName(Application.dataPath) + @"\SuperNewRoles\");
        Directory.CreateDirectory(Path.GetDirectoryName(Application.dataPath) + @"\SuperNewRoles\CustomHatsChache\");
        hatDetails = new List<CustomHatOnline>();
        List<string> repos = new(hatRepos.Keys);
        SuperNewRolesPlugin.Logger.LogInfo("[CustomHats] フェチ");
        foreach (string repo in repos)
        {
            Repos.Add(repo);
        }
        string filePath = Path.GetDirectoryName(Application.dataPath) + @"\SuperNewRoles\CustomHatsChache\";
        foreach (string repo in repos)
        {
            if (File.Exists($"{filePath}\\{hatRepos.FirstOrDefault(data => data.Key == repo).Value}.json") && hatRepos.FirstOrDefault(data => data.Key == repo).Value is not ("TheOtherHats" or "TheOtherRolesGM"))
            {
                CustomHats.IsEnd = true;
                StreamReader sr = new($"{filePath}\\{hatRepos.FirstOrDefault(data => data.Key == repo).Value}.json");

                string text = sr.ReadToEnd();

                sr.Close();

                JToken jobj = JObject.Parse(text)["hats"];
                if (jobj != null && jobj.HasValues)
                {

                    List<CustomHatOnline> hatData = new();

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
                            if (current["package"] == null) info.package = "NameNone";
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

                            hatData.Add(info);
                        }
                    }
                    if (!CustomHats.Keys.Contains("InnerSloth"))
                        CustomHats.Keys.Add("InnerSloth");

                    hatDetails.AddRange(hatData);
                    CachedRepos.Add(repo);
                    Repos.Remove(repo);
                }
            }
        }
        CustomHats.IsEnd = true;
        foreach (var repo in hatRepos)
        {
            SuperNewRolesPlugin.Logger.LogInfo("[CustomHats] ハットスタート:" + repo.Key);
            if (ConfigRoles.IsModCosmeticsAreNotLoaded.Value)
            {
                SuperNewRolesPlugin.Logger.LogInfo("ダウンロードをスキップしました:" + repo.Key);
            }
            else
            {
                try
                {
                    HttpStatusCode status = await FetchHats(repo.Key);
                    if (status != HttpStatusCode.OK)
                        System.Console.WriteLine($"Custom hats could not be loaded from repo: {repo.Key}\n");
                    else
                        SuperNewRolesPlugin.Logger.LogInfo("ハット終了:" + repo.Key);
                }
                catch (System.Exception e)
                {
                    System.Console.WriteLine($"Unable to fetch hats from repo: {repo.Key}\n" + e.Message);
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

            List<CustomHatOnline> hatData = new();

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

                    hatData.Add(info);
                }
            }
            CustomHats.Keys.Add("InnerSloth");

            List<string> markedfordownload = new();

            MD5 md5 = MD5.Create();
            foreach (CustomHatOnline data in hatData)
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
                hatDetails.AddRange(hatData);
                Repos.Remove(repo);
                if (Repos.Count < 1)
                {
                    CustomHats.IsEnd = true;
                }
            }
        }
        catch (System.Exception ex)
        {
            SuperNewRolesPlugin.Instance.Log.LogError("HatsError: " + ex.ToString());
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
        if (!CustomHats.TestExt.IsNull && CustomHats.TestExt.condition.Equals(hat.name))
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