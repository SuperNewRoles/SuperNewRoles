using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using AmongUs.Data.Legacy;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using SuperNewRoles.Modules;
using UnityEngine;

namespace SuperNewRoles.CustomCosmetics;

public class CustomColors
{
    protected static Dictionary<int, string> ColorStrings = new();
    private static List<int> LighterColors = new() { 3, 4, 5, 7, 10, 11, 13, 14, 17 };
    public static readonly uint DefaultPickAbleColors = (uint)Palette.ColorNames.Length;
    public static uint PickAbleColors = DefaultPickAbleColors;

    /// <summary>プレイヤーカラーは明るい色か</summary>
    /// <param name="exp">色の判定を確認したいプレイヤー</param>
    /// <returns>true => 明るい色 / false => 暗い色, expがnullの場合 暗い色として扱う</returns>
    public static bool IsLighter(ExPlayerControl exp) => exp?.Data.DefaultOutfit?.ColorId != null && LighterColors.Contains(exp.Data.DefaultOutfit.ColorId);
    /// <summary>プレイヤーカラーは明るい色か</summary>
    /// <param name="pi">色の判定を確認したいプレイヤー情報</param>
    /// <returns>true => 明るい色 / false => 暗い色, piがnullの場合 暗い色として扱う</returns>
    public static bool IsLighter(NetworkedPlayerInfo pi) => pi?.DefaultOutfit?.ColorId != null && LighterColors.Contains(pi.DefaultOutfit.ColorId);

    /// <summary>有効なColorIdか</summary>
    /// <param name="colorId">判定したいColorId</param>
    /// <returns>true: 有効 / false: 不正</returns>
    public static bool IsValidColorId(int colorId) => colorId >= 0 && colorId < Palette.PlayerColors.Length;

    public enum ColorType
    {
        // |:===== 以下明るい色 =====:|
        Pitchwhite = 18,
        Posi,
        Pitchred,
        XmasRed,
        SignalOrange,
        Peach,
        LightOrange,
        HalloweenOrange,
        Brightyellow,
        Sunrise,
        Gold,
        Pitchyellow,
        Lightgreen,
        Lemon,
        Sprout,
        Pitchgreen,
        Emerald,
        Mintcream,
        Mint,
        Melon,
        LightCyan,
        Teal,
        Snow,
        SkyBlue,
        Ice,
        Darkaqua,
        Backblue,
        Darksky,
        Intenseblue,
        Pitchblue,
        Lavender,
        LightMagenta,
        HotPink,
        PeachFlower,
        Plum,
        Salmon,

        // |:===== 以下暗い色 =====:|
        PitchBlack,
        Nega,
        CyberRed,
        WineRed,
        Azuki,
        CrazyRed,
        Nougat,
        Azi,
        Olive,
        CyberYellow,
        Wasabi,
        Leaf,
        Matcha,
        XmasGreen,
        CyberGreen,
        TokiwaGreen,
        Petrol,
        Sunkengreenishblue,
        Turqoise,
        CyberBlue,
        Crasyublue,
        Blueclosertoblack,
        Blurple,
        Mildpurple,
        CyberPurple,
        Darkmagenta,
        Ashishreddishpurplecolor,
        Bordeaux,
    }

    private const byte bmv = 255; // byte.MaxValue

    // main, shadow, isLighter
    private static Dictionary<ColorType, (Color32, Color32, bool)> CustomColorData = new() { };
    /*
        private static Dictionary<ColorType, (Color32, Color32, bool)> CustomColorDataOld = new() {
                //明るい色(V値が70/100以上、並びはH値順、同じH値の場合はS値が高い方が先、S値も同じ場合はV値が高い方が先)
                { ColorType.Pitchwhite, (new(255, 255, 255, bmv), new(240, 240, 240, bmv), true) }, //H000
                { ColorType.Posi, (new(255, 255, 255, bmv), new(0, 0, 0, bmv), true) }, //H000
                { ColorType.Pitchred, (new(255, 0, 0, bmv), new(178, 0, 0, bmv), true) }, //H000,S100
                { ColorType.XmasRed, (new(219, 41, 41, bmv), new(255, 255, 255, bmv), true) }, //H000,S074
                { ColorType.SignalOrange, (new(0xF7, 0x44, 0x17, bmv), new(0x9B, 0x2E, 0x0F, bmv), true) }, //H012
                { ColorType.Peach, (new(255, 164, 119, bmv), new(238, 128, 100, bmv), true) }, //H020
                { ColorType.HalloweenOrange, (new(255, 156, 59, bmv), new(163, 71, 255, bmv), true) }, //H030,S072
                { ColorType.LightOrange, (new(255, 215, 176, bmv), new(240, 177, 124, bmv), true) }, //H030,S031
                { ColorType.Brightyellow, (new(248, 181, 0, bmv), new(255, 102, 0, bmv), true) }, //H044
                { ColorType.Sunrise, (new(0xFF, 0xCA, 0x19, bmv), new(0xDB, 0x44, 0x42, bmv), true) }, //H046
                { ColorType.Gold, (new(255, 216, 70, bmv), new(226, 168, 13, bmv), true) }, //H047
                { ColorType.Pitchyellow, (new(255, 255, 0, bmv), new(205, 185, 9, bmv), true) }, //H060
                { ColorType.Lightgreen, (new(226, 255, 5, bmv), new(192, 201, 10, bmv), true) }, //H067
                { ColorType.Lemon, (new(0xDB, 0xFD, 0x2F, bmv), new(0x74, 0xE5, 0x10, bmv), true) }, //H070
                { ColorType.Sprout, (new(187, 255, 120, bmv), new(127, 208, 48, bmv), true) }, //H090
                { ColorType.Pitchgreen, (new(0, 255, 0, bmv), new(0, 187, 0, bmv), true) }, //H120
                { ColorType.Emerald, (new(98, 214, 133, bmv), new(82, 179, 111, bmv), true) }, //H138
                { ColorType.Mintcream, (new(245, 255, 250, bmv), new(182, 241, 210, bmv), true) }, //H150
                { ColorType.Mint, (new(111, 192, 156, bmv), new(65, 148, 111, bmv), true) }, //H153
                { ColorType.Melon, (new(11, 254, 148, bmv), new(0, 217, 96, bmv), true) }, //H154
                { ColorType.LightCyan, (new(176, 255, 255, bmv), new(114, 229, 229, bmv), true) }, //H180
                { ColorType.Teal, (new(0x25, 0xB8, 0xBF, bmv), new(0x12, 0x89, 0x86, bmv), true) }, //H183
                { ColorType.Snow, (new(229, 249, 255, bmv), new(135, 226, 255, bmv), true) }, //H194
                { ColorType.SkyBlue,(new(89, 210, 255, bmv), new(37, 169, 232, bmv), true) }, //H196
                { ColorType.Ice, (new(0xA8, 0xDF, 0xFF, bmv), new(0x59, 0x9F, 0xC8, bmv), true) }, //H202
                { ColorType.Darkaqua, (new(14, 104, 188, bmv), new(11, 85, 153, bmv), true) }, //H209
                { ColorType.Backblue, (new(0, 128, 255, bmv), new(0, 85, 255, bmv), true) }, //H210, S100
                { ColorType.Darksky, (new(64, 128, 192, bmv), new(32, 96, 128, bmv), true) }, //H210, S054
                { ColorType.Intenseblue, (new(83, 136, 255, bmv), new(76, 122, 230, bmv), true) }, //H222
                { ColorType.Pitchblue, (new(0, 0, 255, bmv), new(0, 0, 172, bmv), true) }, //H240
                { ColorType.Lavender, (new(173, 126, 201, bmv), new(131, 58, 203, bmv), true) }, //H278
                { ColorType.LightMagenta, (new(255, 199, 255, bmv), new(243, 151, 243, bmv), true) }, //H300
                { ColorType.HotPink, (new(255, 51, 102, bmv), new(232, 0, 58, bmv), true) }, //H345, S080
                { ColorType.PeachFlower, (new(255, 163, 186, bmv), new(255, 129, 157, bmv), true) }, //H345, S036
                { ColorType.Plum, (new(255, 230, 236, bmv), new(255, 178, 195, bmv), true) }, //H346
                { ColorType.Salmon, (new(239, 191, 192, bmv), new(182, 119, 114, bmv), true) }, //H359

                //暗い色(V値が70/100未満、並びはH値順、同じH値の場合はS値が高い方が先、S値も同じ場合はV値が高い方が先)
                { ColorType.PitchBlack, (new(0, 0, 0, bmv), new(0, 0, 0, bmv), false) }, //H000
                { ColorType.Nega, (new(0, 0, 0, bmv), new(255, 255, 255, bmv), false) }, //H000
                { ColorType.CyberRed, (new(0, 0, 0, bmv), new(255, 0, 0, bmv), false) }, //H000,H000
                { ColorType.WineRed, (new(142, 7, 7, bmv), new(109, 0, 0, bmv), false) }, //H000,S061
                { ColorType.Azuki, (new(150, 81, 77, bmv), new(115, 40, 35, bmv), false) }, //H003
                { ColorType.CrazyRed, (new(166, 20, 0, bmv), new(13, 45, 188, bmv), false) }, //H007
                { ColorType.Nougat, (new(160, 101, 56, bmv), new(115, 15, 78, bmv), false) }, //H026
                { ColorType.Azi, (new(100, 48, 0, bmv), new(98, 5, 0, bmv), false) }, //H029
                { ColorType.Olive, (new(154, 140, 61, bmv), new(104, 95, 40, bmv), false) }, //H051
                { ColorType.CyberYellow, (new(0, 0, 0, bmv), new(255, 255, 0, bmv), false) }, //H000,H060
                { ColorType.Wasabi, (new(112, 143, 46, bmv), new(72, 92, 29, bmv), false) }, //H079
                { ColorType.Leaf, (new(62, 90, 11, bmv), new(34, 50, 6, bmv), false) }, //H081
                { ColorType.Matcha, (new(52, 99, 23, bmv), new(34, 54, 19, bmv), false) }, //H097
                { ColorType.XmasGreen, (new(36, 151, 26, bmv), new(183, 34, 34, bmv), false) }, //H120
                { ColorType.CyberGreen, (new(0, 0, 0, bmv), new(0, 255, 127, bmv), false) }, //H000,H150
                { ColorType.TokiwaGreen, (new(0, 123, 67, bmv), new(0, 84, 83, bmv), false) }, //H153
                { ColorType.Petrol, (new(0, 99, 105, bmv), new(0, 61, 54, bmv), false) }, //H183
                { ColorType.Sunkengreenishblue, (new(128, 156, 166, bmv), new(115, 141, 153, bmv), false) }, //H196
                { ColorType.Turqoise, (new(22, 132, 176, bmv), new(15, 89, 117, bmv), false) }, //H197
                { ColorType.CyberBlue, (new(0, 0, 0, bmv), new(0, 169, 255, bmv), false) }, //H000,H200
                { ColorType.Crasyublue, (new(2, 38, 106, bmv), new(64, 0, 111, bmv), false) }, //H219
                { ColorType.Blueclosertoblack, (new(0, 0, 50, bmv), new(0, 0, 25, bmv), false) }, //H240
                { ColorType.Blurple, (new(0x59, 0x3C, 0xD6, bmv), new(0x29, 0x17, 0x96, bmv), false) }, //H251
                { ColorType.Mildpurple, (new(109, 83, 131, bmv), new(82, 54, 105, bmv), false) }, //H272
                { ColorType.CyberPurple, (new(0, 0, 0, bmv), new(207, 15, 216, bmv), false) }, //H000,H297
                { ColorType.Darkmagenta, (new(139, 0, 139, bmv), new(153, 50, 204, bmv), false) }, //H300
                { ColorType.Ashishreddishpurplecolor, (new(139, 102, 118, bmv), new(114, 74, 91, bmv), false) }, //H334
                { ColorType.Bordeaux, (new(109, 7, 26, bmv), new(54, 2, 11, bmv), false) }, //H349
            };*/
    public static void Load()
    {
        List<StringNames> longList = Enumerable.ToList(Palette.ColorNames);
        List<Color32> colorList = Enumerable.ToList(Palette.PlayerColors);
        List<Color32> shadowList = Enumerable.ToList(Palette.ShadowColors);
        List<CustomColor> colors = new();
        var noLighterColorTemp = new List<KeyValuePair<ColorType, (Color32, Color32, bool)>>();
        CustomColorData = new();

        using var fileName = SuperNewRolesPlugin.Assembly.GetManifestResourceStream("SuperNewRoles.Resources.Color.csv");

        //csvを開く
        StreamReader sr = new(fileName);

        var i = 0;
        //1行ずつ処理
        while (!sr.EndOfStream)
        {
            try
            {
                // 行ごとの文字列
                string line = sr.ReadLine();
                // 行が空白 先頭が*なら次の行に
                if (line == "" || line[0] == '#') continue;
                if (line[0] == '*') continue;

                //カンマで配列の要素として分ける
                string[] values = line.Split(',');

                // 配列から辞書に格納する
                // Mがメイン、Sが影
                //カラーId,MR,MG,MB,MA,SR,SG,SB,SA,明るいならaでそれ以外ならb
                CustomColorData.Add((ColorType)(int.Parse(values[0].ToString()) - Palette.PlayerColors.Length),
                    (new(values[1].ParseToByte(), values[2].ParseToByte(), values[3].ParseToByte(), values[4].ParseToByte())
                    , new(values[5].ParseToByte(), values[6].ParseToByte(), values[7].ParseToByte(), values[8].ParseToByte()),
                    values[9] == "light"));

                i++;
            }
            catch (Exception e)
            {
                Logger.Info(e.ToString());
            }
        }
        //CustomColorData = CustomColorDataOld;
        //CustomColorData = CustomColorDataa;
        foreach (var dic in CustomColorData)
        {
            if (!dic.Value.Item3) // isLighterがfalseなら後ろに入れるため仮Listに追加して次ループ
            {
                noLighterColorTemp.Add(dic);
                continue;
            }
            colors.Add(new CustomColor
            {
                longName = $"color{dic.Key}",
                color = dic.Value.Item1,
                shadow = dic.Value.Item2,
                isLighterColor = true
            });
        }
        foreach (var dic in noLighterColorTemp)
        {
            colors.Add(new CustomColor
            {
                longName = $"color{dic.Key}",
                color = dic.Value.Item1,
                shadow = dic.Value.Item2,
                isLighterColor = false
            });
        }
        PickAbleColors += (uint)colors.Count; // Colors to show in Tab
        /** Hidden Colors **/

        /** Add Colors **/
        int id = 50000;
        foreach (CustomColor cc in colors)
        {
            longList.Add((StringNames)id);
            ColorStrings[id++] = cc.longName;
            colorList.Add(cc.color);
            shadowList.Add(cc.shadow);
            if (cc.isLighterColor)
                LighterColors.Add(colorList.Count - 1);
        }
        Palette.ColorNames = longList.ToArray();
        Palette.PlayerColors = colorList.ToArray();
        Palette.ShadowColors = shadowList.ToArray();

    }

    protected internal struct CustomColor
    {
        public string longName;
        public Color32 color;
        public Color32 shadow;
        public bool isLighterColor;
    }

    [HarmonyPatch(typeof(TranslationController), nameof(TranslationController.GetString), [typeof(StringNames), typeof(Il2CppReferenceArray<Il2CppSystem.Object>)])]
    private class ColorStringPatch
    {
        public static bool Prefix(ref string __result, [HarmonyArgument(0)] StringNames name)
        {
            if ((int)name >= 50000 && (int)name < 50999)
            {
                string text = ColorStrings[(int)name];
                if (text != null)
                {
                    __result = $"{ModTranslation.GetString(text)} (MOD)";
                    return false;
                }
            }
            return true;
        }
    }
}