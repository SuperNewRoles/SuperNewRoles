using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using SuperNewRoles.Modules;
using UnityEngine;

namespace SuperNewRoles.CustomCosmetics;

public class CustomCosmeticsPackage
{
    public string name { get; }
    public string name_en { get; }
    public int version { get; }
    public List<CustomCosmeticsHat> hats { get; set; } = [];
    public List<CustomCosmeticsVisor> visors { get; set; } = [];
    public CustomCosmeticsPackage(string name, string name_en, int version)
    {
        this.name = name;
        this.name_en = name_en;
        this.version = version;
    }
    public override string ToString()
    {
        return $"{name} (ver.{version})";
    }
}

public class CustomCosmeticsHat
{
    public string name { get; }
    public string name_en { get; }
    public string hat_id { get; }
    public string path_base { get; }
    public string author { get; }
    public CustomCosmeticsPackage package { get; }
    public CustomCosmeticsHatOptions options { get; }
    private AssetBundle assetBundle { get; }
    public string ProdId => "Modded_" + package.name + "_" + hat_id;
    public CustomCosmeticsHat(string name, string name_en, string hat_id, string path_base, string author, CustomCosmeticsPackage package, CustomCosmeticsHatOptions options, AssetBundle assetBundle)
    {
        this.name = name;
        this.name_en = name_en ?? name;
        this.hat_id = hat_id;
        this.path_base = path_base;
        this.author = author;
        this.package = package;
        this.options = options;
        this.assetBundle = assetBundle;
    }

    // キャッシュ
    private Sprite _loadFrontSprite;
    private Sprite _loadFrontLeftSprite;
    private Sprite _loadClimbSprite;
    private Sprite _loadBackSprite;
    private Sprite _loadBackLeftSprite;
    private Sprite _loadFlipSprite;
    private Sprite _loadFlipBackSprite;
    private Sprite _loadClimbLeftSprite;
    // キャッシュ

    public bool IsLoadedAllSprites()
    {
        return _loadFrontSprite != null && _loadFrontLeftSprite != null && _loadClimbSprite != null && _loadBackSprite != null && _loadBackLeftSprite != null && _loadFlipSprite != null && _loadFlipBackSprite != null && _loadClimbLeftSprite != null;
    }

    public Sprite LoadFrontSprite()
    {
        if (_loadFrontSprite == null)
        {
            if (assetBundle == null)
                _loadFrontSprite = CustomCosmeticsLoader.LoadSpriteFromPath(path_base + "front.png");
            else
                _loadFrontSprite = assetBundle.LoadAsset<Sprite>(path_base + "front.png")?.DontUnload();
        }
        return _loadFrontSprite;
    }
    public Sprite LoadFrontLeftSprite()
    {
        if (options.front_left.HasFlag(HatOptionType.None))
            return null;
        if (_loadFrontLeftSprite == null)
        {
            if (assetBundle == null)
                _loadFrontLeftSprite = CustomCosmeticsLoader.LoadSpriteFromPath(path_base + "front_left.png");
            else
                _loadFrontLeftSprite = assetBundle.LoadAsset<Sprite>(path_base + "front_left.png")?.DontUnload();
        }
        return _loadFrontLeftSprite;
    }
    public Sprite LoadClimbSprite()
    {
        if (options.climb.HasFlag(HatOptionType.None))
            return null;
        if (_loadClimbSprite == null)
        {
            if (assetBundle == null)
                _loadClimbSprite = CustomCosmeticsLoader.LoadSpriteFromPath(path_base + "climb.png");
            else
                _loadClimbSprite = assetBundle.LoadAsset<Sprite>(path_base + "climb.png")?.DontUnload();
        }
        return _loadClimbSprite;
    }
    public Sprite LoadBackSprite()
    {
        if (options.back.HasFlag(HatOptionType.None))
            return null;
        if (_loadBackSprite == null)
        {
            if (assetBundle == null)
                _loadBackSprite = CustomCosmeticsLoader.LoadSpriteFromPath(path_base + "back.png");
            else
                _loadBackSprite = assetBundle.LoadAsset<Sprite>(path_base + "back.png")?.DontUnload();
        }
        return _loadBackSprite;
    }
    public Sprite LoadBackLeftSprite()
    {
        if (options.back_left.HasFlag(HatOptionType.None))
            return null;
        if (_loadBackLeftSprite == null)
        {
            if (assetBundle == null)
                _loadBackLeftSprite = CustomCosmeticsLoader.LoadSpriteFromPath(path_base + "back_left.png");
            else
                _loadBackLeftSprite = assetBundle.LoadAsset<Sprite>(path_base + "back_left.png")?.DontUnload();
        }
        return _loadBackLeftSprite;
    }
    public Sprite LoadFlipSprite()
    {
        if (options.flip.HasFlag(HatOptionType.None))
            return null;
        if (_loadFlipSprite == null)
        {
            if (assetBundle == null)
                _loadFlipSprite = CustomCosmeticsLoader.LoadSpriteFromPath(path_base + "flip.png");
            else
                _loadFlipSprite = assetBundle.LoadAsset<Sprite>(path_base + "flip.png")?.DontUnload();
        }
        return _loadFlipSprite;
    }
    public Sprite LoadFlipBackSprite()
    {
        if (options.flip_back.HasFlag(HatOptionType.None))
            return null;
        if (_loadFlipBackSprite == null)
        {
            if (assetBundle == null)
                _loadFlipBackSprite = CustomCosmeticsLoader.LoadSpriteFromPath(path_base + "flip_back.png");
            else
                _loadFlipBackSprite = assetBundle.LoadAsset<Sprite>(path_base + "flip_back.png")?.DontUnload();
        }
        return _loadFlipBackSprite;
    }
    public Sprite LoadClimbLeftSprite()
    {
        if (options.climb.HasFlag(HatOptionType.None))
            return null;
        if (_loadClimbLeftSprite == null)
        {
            if (assetBundle == null)
                _loadClimbLeftSprite = CustomCosmeticsLoader.LoadSpriteFromPath(path_base + "climb_left.png");
            else
                _loadClimbLeftSprite = assetBundle.LoadAsset<Sprite>(path_base + "climb_left.png")?.DontUnload();
        }
        return _loadClimbLeftSprite;
    }
}

public class CustomCosmeticsVisor
{
    public string name { get; }
    public string name_en { get; }
    public string visor_id { get; }
    public string path_base { get; }
    public string author { get; }
    public CustomCosmeticsPackage package { get; }
    public CustomCosmeticsVisorOptions options { get; }
    private AssetBundle assetBundle { get; }
    public string ProdId => "Modded_" + package.name + "_" + visor_id;
    public CustomCosmeticsVisor(string name, string name_en, string visor_id, string path_base, string author, CustomCosmeticsPackage package, CustomCosmeticsVisorOptions options, AssetBundle assetBundle)
    {
        this.name = name;
        this.name_en = name_en ?? name;
        this.visor_id = visor_id;
        this.path_base = path_base;
        this.author = author;
        this.package = package;
        this.options = options;
        this.assetBundle = assetBundle;
    }

    // キャッシュ
    private Sprite _loadIdleSprite;
    private Sprite _loadLeftIdleSprite;
    private Sprite _loadClimbSprite;
    // キャッシュ

    public bool IsLoadedAllSprites()
    {
        return _loadIdleSprite != null && _loadLeftIdleSprite != null && _loadClimbSprite != null;
    }

    public Sprite LoadIdleSprite()
    {
        if (_loadIdleSprite == null)
        {
            if (assetBundle == null)
                _loadIdleSprite = CustomCosmeticsLoader.LoadSpriteFromPath(path_base + "idle.png");
            else
                _loadIdleSprite = assetBundle.LoadAsset<Sprite>(path_base + "idle.png")?.DontUnload();
        }
        return _loadIdleSprite;
    }
    public Sprite LoadLeftIdleSprite()
    {
        if (_loadLeftIdleSprite == null)
        {
            if (assetBundle == null)
                _loadLeftIdleSprite = CustomCosmeticsLoader.LoadSpriteFromPath(path_base + "idle_left.png");
            else
                _loadLeftIdleSprite = assetBundle.LoadAsset<Sprite>(path_base + "idle_left.png")?.DontUnload();
        }
        return _loadLeftIdleSprite;
    }
    public Sprite LoadClimbSprite()
    {
        if (_loadClimbSprite == null)
        {
            if (assetBundle == null)
                _loadClimbSprite = CustomCosmeticsLoader.LoadSpriteFromPath(path_base + "climb.png");
            else
                _loadClimbSprite = assetBundle.LoadAsset<Sprite>(path_base + "climb.png")?.DontUnload();
        }
        return _loadClimbSprite;
    }
}
public class CustomCosmeticsVisorOptions
{
    public bool adaptive { get; }
    public bool flip { get; }
    public CustomCosmeticsVisorOptions(JToken optionsJson)
    {
        adaptive = CustomCosmeticsHatOptions.GetBool(optionsJson["adaptive"]);
        flip = CustomCosmeticsHatOptions.GetBool(optionsJson["flip"]);
    }
    public CustomCosmeticsVisorOptions(bool adaptive, bool flip)
    {
        this.adaptive = adaptive;
        this.flip = flip;
    }
}
public class CustomCosmeticsHatOptions
{
    public HatOptionType front { get; }
    public HatOptionType front_left { get; }
    public HatOptionType back { get; }
    public HatOptionType back_left { get; }
    public HatOptionType flip { get; }
    public HatOptionType flip_back { get; }
    public HatOptionType climb { get; }
    public bool blockVisors { get; }
    /*
        public CustomCosmeticsHatOptions(JToken optionsJson)
        {
            front = GetOption(optionsJson, "front", true);
            front_left = GetOption(optionsJson, "front_left");
            back = GetOption(optionsJson, "back");
            back_left = GetOption(optionsJson, "back_left");
            flip = GetOption(optionsJson, "flip");
            flip_back = GetOption(optionsJson, "flip_back");
            climb = GetOption(optionsJson, "climb");
            blockVisors = GetBool(optionsJson["block_visors"]);
        }*/
    public CustomCosmeticsHatOptions(JToken optionsJson)
    {
        bool adaptive = GetBool(optionsJson["adaptive"]);
        bool bounce = GetBool(optionsJson["bounce"]);
        bool behind = GetBool(optionsJson["behind"]);
        front = GetOption(optionsJson, "front", adaptive: adaptive, bounce: bounce, behind: behind, defaultOption: true);
        front_left = GetOption(optionsJson, "front_left", adaptive: adaptive, bounce: bounce, behind: behind);
        back = GetOption(optionsJson, "back", adaptive: adaptive, bounce: bounce, behind: behind);
        back_left = GetOption(optionsJson, "back_left", adaptive: adaptive, bounce: bounce, behind: behind);
        flip = GetOption(optionsJson, "flip", adaptive: adaptive, bounce: bounce, behind: behind);
        flip_back = GetOption(optionsJson, "flip_back", adaptive: adaptive, bounce: bounce, behind: behind);
        climb = GetOption(optionsJson, "climb", adaptive: adaptive, bounce: bounce, behind: behind);
        blockVisors = GetBool(optionsJson["block_visors"]);
    }
    public CustomCosmeticsHatOptions(HatOptionType front, HatOptionType front_left, HatOptionType back, HatOptionType back_left, HatOptionType flip, HatOptionType flip_back, HatOptionType climb, bool blockVisors = false)
    {
        this.front = front;
        this.front_left = front_left;
        this.back = back;
        this.back_left = back_left;
        this.flip = flip;
        this.flip_back = flip_back;
        this.climb = climb;
        this.blockVisors = blockVisors;
    }

    /// <summary>
    /// 指定されたプレフィックスに対してオプション値を取得します。
    /// useBaseFlagがtrueの場合、adaptiveがfalseならベースフラグの値を反映し、falseの場合は常にNoAdaptiveとなります。
    /// </summary>
    private HatOptionType GetOption(JToken json, string keyPrefix, bool defaultOption = false, bool adaptive = false, bool bounce = false, bool behind = false)
    {
        // ヘルパーローカル関数：トークンからbool値を安全に取得する

        HatOptionType option;
        if (GetBool(json[$"{keyPrefix}_adaptive"]))
            option = HatOptionType.Adaptive;
        else
        {
            if (json[keyPrefix] == null && defaultOption)
                option = adaptive ? HatOptionType.Adaptive : HatOptionType.NoAdaptive;
            else
                option = GetBool(json[keyPrefix]) ? (adaptive ? HatOptionType.Adaptive : HatOptionType.NoAdaptive) : HatOptionType.None;
        }

        if (bounce || GetBool(json[$"{keyPrefix}_bounce"]))
            option |= HatOptionType.Bounce;
        if (behind || GetBool(json[$"{keyPrefix}_behind"]))
            option |= HatOptionType.Behind;

        return option;
    }
    public static bool GetBool(JToken token) => token != null && token.Type == JTokenType.Boolean && (bool)token;
}
[Flags]
public enum HatOptionType
{
    None = 1 << 0,
    NoAdaptive = 1 << 1,
    Adaptive = 1 << 2,
    Bounce = 1 << 3,
    Behind = 1 << 4,
}