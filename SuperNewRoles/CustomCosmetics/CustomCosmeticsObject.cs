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
    public int order { get; }
    public List<CustomCosmeticsHat> hats { get; set; } = [];
    public List<CustomCosmeticsVisor> visors { get; set; } = [];
    public List<CustomCosmeticsNamePlate> namePlates { get; set; } = [];
    public CustomCosmeticsPackage(string name, string name_en, int version, int order = 1)
    {
        this.name = name;
        this.name_en = name_en;
        this.version = version;
        this.order = order;
    }
    public override string ToString()
    {
        return $"{name} (ver.{version})";
    }
}

public static class CustomCosmeticsAssetBundleExtensions
{
    public static Sprite LoadCosmeticsAsset(this AssetBundle assetBundle, string path)
    {
        return assetBundle.LoadAsset<Sprite>(path);
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
    public string ProdId => CustomCosmeticsLoader.ModdedPrefix + package.name + "_" + hat_id;
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
                _loadFrontSprite = assetBundle.LoadCosmeticsAsset(path_base + "front.png");
        }
        return _loadFrontSprite;
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
                _loadClimbSprite = assetBundle.LoadCosmeticsAsset(path_base + "climb.png");
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
                _loadBackSprite = assetBundle.LoadCosmeticsAsset(path_base + "back.png");
        }
        return _loadBackSprite;
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
                _loadFlipSprite = assetBundle.LoadCosmeticsAsset(path_base + "flip.png");
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
                _loadFlipBackSprite = assetBundle.LoadCosmeticsAsset(path_base + "flip_back.png");
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
                _loadClimbLeftSprite = assetBundle.LoadCosmeticsAsset(path_base + "climb_left.png");
        }
        return _loadClimbLeftSprite;
    }
    public void UnloadSprites()
    {
        if (_loadFrontSprite != null)
        {
            if (assetBundle != null)
                Resources.UnloadAsset(_loadFrontSprite);
            else
            {
                GameObject.Destroy(_loadFrontSprite.texture);
                GameObject.Destroy(_loadFrontSprite);
            }
            _loadFrontSprite = null;
        }
        if (_loadFrontLeftSprite != null)
        {
            if (assetBundle != null)
                Resources.UnloadAsset(_loadFrontLeftSprite);
            else
            {
                GameObject.Destroy(_loadFrontLeftSprite.texture);
                GameObject.Destroy(_loadFrontLeftSprite);
            }
            _loadFrontLeftSprite = null;
        }
        if (_loadClimbSprite != null)
        {
            if (assetBundle != null)
                Resources.UnloadAsset(_loadClimbSprite);
            else
            {
                GameObject.Destroy(_loadClimbSprite.texture);
                GameObject.Destroy(_loadClimbSprite);
            }
            _loadClimbSprite = null;
        }
        if (_loadBackSprite != null)
        {
            if (assetBundle != null)
                Resources.UnloadAsset(_loadBackSprite);
            else
            {
                GameObject.Destroy(_loadBackSprite.texture);
                GameObject.Destroy(_loadBackSprite);
            }
            _loadBackSprite = null;
        }
        if (_loadBackLeftSprite != null)
        {
            if (assetBundle != null)
                Resources.UnloadAsset(_loadBackLeftSprite);
            else
            {
                GameObject.Destroy(_loadBackLeftSprite.texture);
                GameObject.Destroy(_loadBackLeftSprite);
            }
            _loadBackLeftSprite = null;
        }
        if (_loadFlipSprite != null)
        {
            if (assetBundle != null)
                Resources.UnloadAsset(_loadFlipSprite);
            else
            {
                GameObject.Destroy(_loadFlipSprite.texture);
                GameObject.Destroy(_loadFlipSprite);
            }
            _loadFlipSprite = null;
        }
        if (_loadFlipBackSprite != null)
        {
            if (assetBundle != null)
                Resources.UnloadAsset(_loadFlipBackSprite);
            else
            {
                GameObject.Destroy(_loadFlipBackSprite.texture);
                GameObject.Destroy(_loadFlipBackSprite);
            }
            _loadFlipBackSprite = null;
        }
        if (_loadClimbLeftSprite != null)
        {
            if (assetBundle != null)
                Resources.UnloadAsset(_loadClimbLeftSprite);
            else
            {
                GameObject.Destroy(_loadClimbLeftSprite.texture);
                GameObject.Destroy(_loadClimbLeftSprite);
            }
            _loadClimbLeftSprite = null;
        }
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
    public string ProdId => CustomCosmeticsLoader.ModdedPrefix + package.name + "_" + visor_id;
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
                _loadIdleSprite = CustomCosmeticsLoader.CreateVisorSprite(path_base + "idle.png", options.isSNR);
            else
                _loadIdleSprite = assetBundle.LoadCosmeticsAsset(path_base + "idle.png");
        }
        return _loadIdleSprite;
    }
    public Sprite LoadLeftIdleSprite()
    {
        if (!options.flip)
            return null;
        if (_loadLeftIdleSprite == null)
        {
            if (assetBundle == null)
                _loadLeftIdleSprite = CustomCosmeticsLoader.CreateVisorSprite(path_base + "flip.png", options.isSNR);
            else
                _loadLeftIdleSprite = assetBundle.LoadCosmeticsAsset(path_base + "flip.png");
        }
        return _loadLeftIdleSprite;
    }
    public Sprite LoadClimbSprite()
    {
        if (!options.climb)
            return null;
        if (_loadClimbSprite == null)
        {
            if (assetBundle == null)
                _loadClimbSprite = CustomCosmeticsLoader.CreateVisorSprite(path_base + "climb.png", options.isSNR);
            else
                _loadClimbSprite = assetBundle.LoadCosmeticsAsset(path_base + "climb.png");
        }
        return _loadClimbSprite;
    }
    public void UnloadSprites()
    {
        if (_loadIdleSprite != null)
        {
            if (assetBundle != null)
                Resources.UnloadAsset(_loadIdleSprite);
            else
            {
                GameObject.Destroy(_loadIdleSprite.texture);
                GameObject.Destroy(_loadIdleSprite);
            }
            _loadIdleSprite = null;
        }
        if (_loadLeftIdleSprite != null)
        {
            if (assetBundle != null)
                Resources.UnloadAsset(_loadLeftIdleSprite);
            else
            {
                GameObject.Destroy(_loadLeftIdleSprite.texture);
                GameObject.Destroy(_loadLeftIdleSprite);
            }
            _loadLeftIdleSprite = null;
        }
        if (_loadClimbSprite != null)
        {
            if (assetBundle != null)
                Resources.UnloadAsset(_loadClimbSprite);
            else
            {
                GameObject.Destroy(_loadClimbSprite.texture);
                GameObject.Destroy(_loadClimbSprite);
            }
            _loadClimbSprite = null;
        }
    }
}

public class CustomCosmeticsNamePlate
{
    public string name { get; }
    public string name_en { get; }
    public string plate_id { get; }
    public string path_base { get; }
    public string author { get; }
    public CustomCosmeticsPackage package { get; }
    private AssetBundle assetBundle { get; }
    public string ProdId => CustomCosmeticsLoader.ModdedPrefix + package.name + "_" + plate_id;
    public CustomCosmeticsNamePlate(string name, string name_en, string plate_id, string path_base, string author, CustomCosmeticsPackage package, AssetBundle assetBundle)
    {
        this.name = name;
        this.name_en = name_en ?? name;
        this.plate_id = plate_id;
        this.path_base = path_base;
        this.author = author;
        this.package = package;
        this.assetBundle = assetBundle;
    }

    // キャッシュ
    private Sprite _loadSprite;
    // キャッシュ

    public bool IsLoadedAllSprites()
    {
        return _loadSprite != null;
    }

    public Sprite LoadSprite()
    {
        if (_loadSprite == null)
        {
            if (assetBundle == null)
                _loadSprite = CustomCosmeticsLoader.LoadSpriteFromPath(path_base + "plate.png");
            else
                _loadSprite = assetBundle.LoadCosmeticsAsset(path_base + "plate.png");
        }
        return _loadSprite;
    }
    public void UnloadSprites()
    {
        if (_loadSprite != null)
        {
            if (assetBundle != null)
                Resources.UnloadAsset(_loadSprite);
            else
            {
                GameObject.Destroy(_loadSprite.texture);
                GameObject.Destroy(_loadSprite);
            }
            _loadSprite = null;
        }
    }
}

public class CustomCosmeticsVisorOptions
{
    public bool adaptive { get; }
    public bool flip { get; }
    public bool climb { get; set; }
    public bool isSNR { get; }
    public CustomCosmeticsVisorOptions(JToken optionsJson)
    {
        adaptive = CustomCosmeticsHatOptions.GetBool(optionsJson["adaptive"]);
        flip = CustomCosmeticsHatOptions.GetBool(optionsJson["flip"]);
        climb = CustomCosmeticsHatOptions.GetBool(optionsJson["climb"]);
        isSNR = CustomCosmeticsHatOptions.GetBool(optionsJson["IsSNR"]);
    }
    public CustomCosmeticsVisorOptions(bool adaptive, bool flip, bool isSNR)
    {
        this.adaptive = adaptive;
        this.flip = flip;
        this.isSNR = isSNR;
    }
}
public class CustomCosmeticsHatOptions
{
    public HatOptionType front { get; }
    public HatOptionType back { get; }
    public HatOptionType flip { get; }
    public HatOptionType flip_back { get; }
    public HatOptionType climb { get; set; }
    public bool blockVisors { get; }
    public bool HideBody { get; }
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
        back = GetOption(optionsJson, "back", adaptive: adaptive, bounce: bounce, behind: behind);
        flip = GetOption(optionsJson, "flip", adaptive: adaptive, bounce: bounce, behind: behind);
        flip_back = GetOption(optionsJson, "flip_back", adaptive: adaptive, bounce: bounce, behind: behind);
        climb = GetOption(optionsJson, "climb", adaptive: adaptive, bounce: bounce, behind: behind);
        HideBody = GetBool(optionsJson["hideBody"]);
        blockVisors = GetBool(optionsJson["block_visors"]);
    }
    public CustomCosmeticsHatOptions(HatOptionType front, HatOptionType back, HatOptionType flip, HatOptionType flip_back, HatOptionType climb, bool hideBody, bool blockVisors = false)
    {
        this.front = front;
        this.back = back;
        this.flip = flip;
        this.flip_back = flip_back;
        this.climb = climb;
        this.blockVisors = blockVisors;
        this.HideBody = hideBody;
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
    public static bool GetBool(JToken token, bool defaultValue = false)
        => token != null && token.Type == JTokenType.Boolean ? (bool)token : defaultValue;
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