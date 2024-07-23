using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using AmongUs.Data;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using HarmonyLib;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using SuperNewRoles.CustomCosmetics.CustomCosmeticsData;
using UnityEngine;
using UnityEngine.AddressableAssets;
namespace SuperNewRoles.CustomCosmetics;

public class CustomVisor
{
    public static Material VisorShader;

    public static bool isAdded = false;
    static readonly List<VisorData> visorData = new();
    public static readonly Dictionary<string, CustomVisorData> customVisorData = new();
    [HarmonyPatch(typeof(HatManager), nameof(HatManager.GetVisorById))]
    class UnlockedVisorPatch
    {
        private static bool LOADED;
        private static bool SPRITELOADED = false;
        private static bool RUNNING = false;
        public static bool IsLoadingnow = false;
        public static List<VisorData> visordata = new();

        static void Prefix(HatManager __instance)
        {
            if (RUNNING) return;
            if (IsLoadingnow) return;
            if (SPRITELOADED)
            {
                RUNNING = true; // prevent simultanious execution
                IsLoadingnow = true;
                if (!LOADED)
                {
                    Assembly assembly = Assembly.GetExecutingAssembly();
                    string visorres = $"{assembly.GetName().Name}.Resources.CustomVisors";
                    var visors = from r in assembly.GetManifestResourceNames()
                                       where r.StartsWith(visorres) && r.EndsWith(".png")
                                       select r;

                    List<CustomVisors.CustomVisor> customvisors = CustomVisors.CreateCustomVisorDetails(visors);
                    foreach (CustomVisors.CustomVisor cv in customvisors.AsSpan())
                    {
                        CustomVisorData visorData = CreateVisorData(cv);
                        customVisorData.Add(visorData.ProductId, visorData);
                    }
                }
                while (DownLoadClassVisor.VisorDetails.Count > 0)
                {
                    CustomVisorData chdata = CreateVisorData(DownLoadClassVisor.VisorDetails[0]);
                    customVisorData.Add(chdata.ProductId, chdata);
                    DownLoadClassVisor.VisorDetails.RemoveAt(0);
                }
                LOADED = true;
                var data = __instance.allVisors.ToList();
                data.AddRange(customVisorData.Values);
                visordata = data;
                __instance.allVisors = new Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<VisorData>(data.ToArray());
                IsLoadingnow = false;
            }
            else
            {
                SPRITELOADED = true;
                __instance.StartCoroutine(LoadVisorSprite().WrapToIl2Cpp());
            }
        }

        static IEnumerator LoadVisorSprite()
        {
            IsLoadingnow = true;
            if (!LOADED)
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                string visorres = $"{assembly.GetName().Name}.Resources.CustomVisors";
                var visors = from r in assembly.GetManifestResourceNames()
                                   where r.StartsWith(visorres) && r.EndsWith(".png")
                                   select r;

                List<CustomVisors.CustomVisor> customvisors = CustomVisors.CreateCustomVisorDetails(visors);
                foreach (CustomVisors.CustomVisor cv in customvisors)
                {
                    CreateVisorSprite(cv.resource, cv.IsSNR);
                    yield return new WaitForSeconds(0.0005f);
                    if (cv.flipresource != null)
                    {
                        CreateVisorSprite(cv.flipresource, cv.IsSNR);
                        yield return new WaitForSeconds(0.0005f);
                    }
                }
            }
            foreach (var data in DownLoadClassVisor.VisorDetails)
            {
                var visor = GenereteVisorData(data);
                CreateVisorSprite(visor.resource, visor.IsSNR, true);
                yield return new WaitForSeconds(0.0005f);
                if (visor.flipresource != null)
                {
                    CreateVisorSprite(visor.flipresource, visor.IsSNR, true);
                    yield return new WaitForSeconds(0.0005f);
                }
            }
            IsLoadingnow = false;
        }
    }

    private static CustomVisors.CustomVisorOnline GenereteVisorData(CustomVisors.CustomVisorOnline cvo)
    {
        string filePath = Path.GetDirectoryName(Application.dataPath) + @"\SuperNewRoles\CustomVisorsChache\";
        cvo.resource = filePath + cvo.resource;
        if (cvo.flipresource != null)
            cvo.flipresource = filePath + cvo.flipresource;
        return cvo;
    }
    private static Sprite GetVisorSprite(string path) => VisorSprites.ContainsKey(path) ? VisorSprites[path] : null;

    /// <summary>
    /// バイザー画像を読み込む
    /// </summary>
    /// <param name="path">読み込む画像の(ユーザストレージ内の)絶対パス</param>
    /// <param name="isSNR">SNRの独自規格で読み込むか</param>
    /// <param name="fromDisk">キャッシュを使用せずに読み込むか</param>
    /// <returns>バイザー画像</returns>
    private static Sprite CreateVisorSprite(string path, bool isSNR, bool fromDisk = false)
    {
        Sprite sprite = isSNR ? SNRVisorLoadSprite(path) : ModHelpers.CreateSprite(path, fromDisk);
        if (!VisorSprites.ContainsKey(path)) VisorSprites.Add(path, sprite);
        return sprite;
    }

    /// <summary>
    /// バイザー画像を, SNR独自規格で読み込む (画像サイズを一定(115f)に変更し読み込む)
    /// </summary>
    /// <param name="path">読み込む画像の(ユーザストレージ内の)絶対パス</param>
    /// <returns>バイザー画像</returns>
    private static Sprite SNRVisorLoadSprite(string path)
    {
        //画像サイズは150*150
        if (ModHelpers.iCall_LoadImage == null)
            ModHelpers.iCall_LoadImage = IL2CPP.ResolveICall<ModHelpers.d_LoadImage>("UnityEngine.ImageConversion::LoadImage");
        if (ModHelpers.iCall_LoadImage == null) return null;

        try
        {
            byte[] bytes = File.ReadAllBytes(path);
            Texture2D texture = new(2, 2);
            var Array = (Il2CppStructArray<byte>)bytes;
            ModHelpers.iCall_LoadImage.Invoke(texture.Pointer, Array.Pointer, false);

            Rect rect = new(0f, 0f, texture.width, texture.height);
            Sprite sprite = Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f), 115f);

            if (sprite == null) return null;

            texture.hideFlags |= HideFlags.HideAndDontSave | HideFlags.DontUnloadUnusedAsset;
            sprite.hideFlags |= HideFlags.HideAndDontSave | HideFlags.DontUnloadUnusedAsset;
            return sprite;
        }
        catch { }
        return null;
    }

    static readonly Dictionary<string, Sprite> VisorSprites = new();

    private static CustomVisorData CreateVisorData(CustomVisors.CustomVisor cv, bool fromDisk = false, bool testOnly = false)
    {
        if (VisorShader == null && DestroyableSingleton<HatManager>.InstanceExists) VisorShader = new Material(Shader.Find("Unlit/PlayerShader"));

        CustomVisorData.VisorTempViewData visorViewData = new() { MainImage = GetVisorSprite(cv.resource) };
        var assetRef = new AssetReference(visorViewData.CreateVVD.Pointer);

        // PreviewViewData previewData = new() { PreviewSprite = visorViewData.MainImage };

        // var previewDataRef = new AssetReference(previewData.Pointer);

        CustomVisorData visor = new()
        {
            name = cv.name + "\nby " + cv.author,

            // 本体 : CosmticData
            displayOrder = 99,
            ProductId = "CustomVisors_" + cv.package + "_" + cv.name.Replace(' ', '_'),
            ChipOffset = new Vector2(0f, 0.25f),
            Free = true,
            // SpritePreview = visorViewData.MainImage,
            NotInStore = true,

            // 本体 : VisorData
            behindHats = cv.behindHats,
            ViewDataRef = assetRef,
            // PreviewData = previewDataRef,
        };
        visor.CreateAddressableAsset();

        visorViewData.VisorName = visor.ProdId;
        if (cv.adaptive) visorViewData.Adaptive = true;
        if (cv.flipresource != null) visorViewData.FlipImage = GetVisorSprite(cv.flipresource);

        CustomVisors.VisorExtension extend = new()
        {
            author = cv.author ?? "Unknown",
            package = cv.package
        };

        if (testOnly)
        {
            CustomVisors.TestExt = extend;
            CustomVisors.TestExt.condition = visor.name;
        }
        else
        {
            CustomVisors.CustomVisorRegistry[visor.name] = extend;
        }
        visor.vtvd = visorViewData;
        return visor;
    }
}

public class VisorTabPatch
{
    private static readonly List<TMPro.TMP_Text> visorsTabCustomTexts = new();
    private const string innerslothPackageName = "Innersloth Visors";
    private const float headerSize = 0.8f;
    private const float headerX = 0.8f;
    private static float inventoryTop = 1.5f;
    private static float inventoryBot = -2.5f;
    private const float inventoryZ = -2f;

    public static void CalcItemBounds(VisorsTab __instance)
    {
        inventoryTop = __instance.scroller.Inner.position.y - 0.5f;
        inventoryBot = __instance.scroller.Inner.position.y - 4.5f;
    }

    [HarmonyPatch(typeof(VisorsTab), nameof(VisorsTab.OnEnable))]
    public class VisorsTabOnEnablePatch
    {
        public static TMPro.TMP_Text textTemplate;
        public static List<ColorChip> Chips;

        public static float CreateVisorPackage(List<(VisorData, CustomVisors.VisorExtension)> visors, string packageName, float YStart, VisorsTab __instance)
        {
            float offset = YStart;
            if (textTemplate != null)
            {
                TMPro.TMP_Text title = UnityEngine.Object.Instantiate(textTemplate, __instance.scroller.Inner);
                title.transform.parent = __instance.scroller.Inner;
                title.transform.localPosition = new Vector3(headerX, YStart, inventoryZ);
                title.alignment = TMPro.TextAlignmentOptions.Center;
                title.fontSize *= 1.25f;
                title.fontWeight = TMPro.FontWeight.Thin;
                title.enableAutoSizing = false;
                title.autoSizeTextContainer = true;
                title.text = ModTranslation.GetString(packageName); // 渡されたパッケージ名に翻訳があれば取得

                offset -= headerSize * __instance.YOffset;
                visorsTabCustomTexts.Add(title);
            }

            var numVisors = visors.Count;

            int i2 = 0;
            for (int i = 0; i < visors.Count; i++)
            {
                VisorData visor = visors[i].Item1;
                CustomVisors.VisorExtension ext = visors[i].Item2;

                float xpos = __instance.XRange.Lerp((i2 % __instance.NumPerRow) / (__instance.NumPerRow - 1f));
                float ypos = offset - (i2 / __instance.NumPerRow) * __instance.YOffset;
                ColorChip colorChip = UnityEngine.Object.Instantiate<ColorChip>(__instance.ColorTabPrefab, __instance.scroller.Inner);

                int color = __instance.HasLocalPlayer() ? CachedPlayer.LocalPlayer.Data.DefaultOutfit.ColorId : DataManager.Player.Customization.Color;

                colorChip.transform.localPosition = new Vector3(xpos, ypos, inventoryZ);
                if (ActiveInputManager.currentControlType == ActiveInputManager.InputType.Keyboard)
                {
                    colorChip.Button.OnMouseOver.AddListener((UnityEngine.Events.UnityAction)(() => __instance.SelectVisor(visor)));
                    colorChip.Button.OnMouseOut.AddListener((UnityEngine.Events.UnityAction)(() => __instance.SelectVisor(FastDestroyableSingleton<HatManager>.Instance.GetVisorById(DataManager.Player.Customization.Visor))));
                    colorChip.Button.OnClick.AddListener((UnityEngine.Events.UnityAction)(() => __instance.ClickEquip()));
                }
                else
                {
                    colorChip.Button.OnClick.AddListener((UnityEngine.Events.UnityAction)(() => __instance.SelectVisor(visor)));
                }

                colorChip.ProductId = visor.ProductId;
                colorChip.Inner.transform.localPosition = visor.ChipOffset;
                colorChip.Tag = visor;
                colorChip.Button.ClickMask = __instance.scroller.Hitbox;
                __instance.UpdateMaterials(colorChip.Inner.FrontLayer, visor);
                __instance.visorId = DataManager.Player.Customization.Visor;
                __instance.ColorChips.Add(colorChip);
                //Modバイザーか判定
                if (CustomVisor.customVisorData.TryGetValue(visor.ProductId, out CustomVisorData cvd))
                {
                    colorChip.Inner.FrontLayer.sprite = cvd.vtvd.MainImage;
                    PlayerMaterial.SetColors(color, colorChip.Inner.FrontLayer);
                }
                else if (visor.ProductId.StartsWith("CustomVisors_"))
                {
                    colorChip.Inner.FrontLayer.sprite = null;
                    PlayerMaterial.SetColors(color, colorChip.Inner.FrontLayer);
                }
                else
                {
                    visor.SetPreview(colorChip.Inner.FrontLayer, color);
                }
                // visor.SetPreview(colorChip.Inner.FrontLayer, color);
                Chips.Add(colorChip);
                i2++;
            }
            return offset - ((numVisors - 1) / __instance.NumPerRow) * __instance.YOffset - headerSize;
        }
        public static bool Prefix(VisorsTab __instance)
        {
            CalcItemBounds(__instance);
            VisorData[] unlockedVisors = FastDestroyableSingleton<HatManager>.Instance.GetUnlockedVisors();
            Dictionary<string, List<(VisorData, CustomVisors.VisorExtension)>> packages = new();

            ModHelpers.DestroyList(visorsTabCustomTexts);
            ModHelpers.DestroyList(__instance.ColorChips);

            visorsTabCustomTexts.Clear();
            __instance.ColorChips.Clear();

            textTemplate = PlayerCustomizationMenu.Instance.itemName;

            foreach (VisorData visorData in unlockedVisors)
            {
                CustomVisors.VisorExtension ext = CustomVisors.GetVisorExtension(visorData);

                if (!ext.IsNull)
                {
                    if (!packages.ContainsKey(ext.package == null ? innerslothPackageName : ext.package))
                        packages[ext.package == null ? innerslothPackageName : ext.package] = new();
                    packages[ext.package == null ? innerslothPackageName : ext.package].Add((visorData, ext));
                }
                else
                {
                    if (!packages.ContainsKey(innerslothPackageName))
                        packages[innerslothPackageName] = new();
                    packages[innerslothPackageName].Add((visorData, new() { IsNull = true }));
                }
            }

            float YOffset = __instance.YStart;

            var orderedKeys = packages.Keys.OrderBy((string x) =>
            {
                return x == innerslothPackageName
                    ? 100003
                    : x == "developerVisors"
                    ? 20
                    : x.Contains("Visor_SNR") ? 0 : 500;
            });

            foreach (string key in orderedKeys)
            {
                List<(VisorData, CustomVisors.VisorExtension)> value = packages[key];
                YOffset = CreateVisorPackage(value, key, YOffset, __instance);
            }

            __instance.scroller.ContentYBounds.max = -(YOffset + 3.0f + headerSize);
            return false;
        }
    }
    [HarmonyPatch(typeof(VisorsTab), nameof(VisorsTab.Update))]
    public class VisorsTabUpdatePatch
    {
        public static bool Prefix()
        {
            foreach (TMPro.TMP_Text customText in visorsTabCustomTexts.AsSpan())
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