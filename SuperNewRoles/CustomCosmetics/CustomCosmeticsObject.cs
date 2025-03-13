using System;
using Newtonsoft.Json.Linq;
using SuperNewRoles.Modules;
using UnityEngine;

namespace SuperNewRoles.CustomCosmetics;

public class CustomCosmeticsPackage
{
    public string name { get; }
    public string name_en { get; }
    public int version { get; }
    public CustomCosmeticsHat[] hats { get; set; } = [];
    public CustomCosmeticsPackage(string name, string name_en, int version)
    {
        this.name = name;
        this.name_en = name_en;
        this.version = version;
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
    public string ProdId => "Modded_" + hat_id;
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
    public Sprite LoadFrontSprite()
    {
        return assetBundle.LoadAsset<Sprite>(path_base + "front.png");
    }
    public Sprite LoadFrontLeftSprite()
    {
        return assetBundle.LoadAsset<Sprite>(path_base + "front_left.png");
    }
    public Sprite LoadClimbSprite()
    {
        return assetBundle.LoadAsset<Sprite>(path_base + "climb.png");
    }
    public Sprite LoadBackSprite()
    {
        return assetBundle.LoadAsset<Sprite>(path_base + "back.png");
    }
    public Sprite LoadBackLeftSprite()
    {
        return assetBundle.LoadAsset<Sprite>(path_base + "back_left.png");
    }
    public Sprite LoadFlipSprite()
    {
        return assetBundle.LoadAsset<Sprite>(path_base + "flip.png");
    }
    public Sprite LoadFlipBackSprite()
    {
        return assetBundle.LoadAsset<Sprite>(path_base + "flip_back.png");
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
    private HatOptionType GetOption(JToken json, string keyPrefix, bool defaultOption = false)
    {
        // ヘルパーローカル関数：トークンからbool値を安全に取得する

        HatOptionType option;
        if (GetBool(json[$"{keyPrefix}_adaptive"]))
            option = HatOptionType.Adaptive;
        else
        {
            if (json[keyPrefix] == null && defaultOption)
                option = HatOptionType.NoAdaptive;
            else
                option = GetBool(json[keyPrefix]) ? HatOptionType.NoAdaptive : HatOptionType.None;
        }

        if (GetBool(json[$"{keyPrefix}_bounce"]))
            option |= HatOptionType.Bounce;
        if (GetBool(json[$"{keyPrefix}_behind"]))
            option |= HatOptionType.Behind;

        return option;
    }
    static bool GetBool(JToken token) => token != null && token.Type == JTokenType.Boolean && (bool)token;
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