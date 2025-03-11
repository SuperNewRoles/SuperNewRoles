using System;
using Newtonsoft.Json.Linq;

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
    public CustomCosmeticsHat(string name, string name_en, string hat_id, string path_base, string author, CustomCosmeticsPackage package, CustomCosmeticsHatOptions options)
    {
        this.name = name;
        this.name_en = name_en ?? name;
        this.hat_id = hat_id;
        this.path_base = path_base;
        this.author = author;
        this.package = package;
        this.options = options;
    }
}

public class CustomCosmeticsHatOptions
{
    public HatOptionType front { get; }
    public HatOptionType back { get; }
    public HatOptionType flip { get; }
    public HatOptionType flip_back { get; }
    public HatOptionType climb { get; }

    public CustomCosmeticsHatOptions(JToken optionsJson)
    {
        front = GetOption(optionsJson, "front", useBaseFlag: false);
        back = GetOption(optionsJson, "back", useBaseFlag: true);
        flip = GetOption(optionsJson, "flip", useBaseFlag: true);
        flip_back = GetOption(optionsJson, "flip_back", useBaseFlag: true);
        climb = GetOption(optionsJson, "climb", useBaseFlag: true);
    }

    /// <summary>
    /// 指定されたプレフィックスに対してオプション値を取得します。
    /// useBaseFlagがtrueの場合、adaptiveがfalseならベースフラグの値を反映し、falseの場合は常にNoAdaptiveとなります。
    /// </summary>
    private HatOptionType GetOption(JToken json, string keyPrefix, bool useBaseFlag)
    {
        // ヘルパーローカル関数：トークンからbool値を安全に取得する
        bool GetBool(JToken token) => token != null && token.Type == JTokenType.Boolean && (bool)token;

        HatOptionType option;
        if (GetBool(json[$"{keyPrefix}_adaptive"]))
            option = HatOptionType.Adaptive;
        else if (useBaseFlag)
        {
            option = GetBool(json[keyPrefix]) ? HatOptionType.NoAdaptive : HatOptionType.None;
        }
        else
            option = HatOptionType.NoAdaptive;

        if (GetBool(json[$"{keyPrefix}_bounce"]))
            option |= HatOptionType.Bounce;
        if (GetBool(json[$"{keyPrefix}_behind"]))
            option |= HatOptionType.Behind;

        return option;
    }
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