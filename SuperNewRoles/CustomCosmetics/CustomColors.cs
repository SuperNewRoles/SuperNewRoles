using System.Collections.Generic;
using System.Linq;
using AmongUs.Data.Legacy;
using HarmonyLib;
using UnhollowerBaseLib;
using UnityEngine;

namespace SuperNewRoles.CustomCosmetics
{
    public class CustomColors
    {
        protected static Dictionary<int, string> ColorStrings = new();
        public static List<int> lighterColors = new() { 3, 4, 5, 7, 10, 11, 13, 14, 17 };
        public static uint pickableColors = (uint)Palette.ColorNames.Length;

        public enum ColorType
        {
            Salmon,
            Bordeaux,
            Olive,
            Turqoise,
            Mint,
            Lavender,
            Nougat,
            Peach,
            Wasabi,
            HotPink,
            Petrol,
            Lemon,
            SignalOrange,
            Teal,
            Blurple,
            Sunrise,
            Ice,
            PitchBlack,
            Darkmagenta,
            Mintcream,
            Leaf,
            Emerald,
            Brightyellow,
            Darkaqua,
            Matcha,
            Pitchwhite,
            Darksky,
            Intenseblue,
            Blueclosertoblack,
            Sunkengreenishblue,
            Azi,
            Pitchred,
            Pitchblue,
            Pitchgreen,
            Pitchyellow,
            Backblue,
            Mildpurple,
            Ashishreddishpurplecolor,
            Melon,
            Crasyublue,
            Lightgreen,
            Azuki,
            Snow
        }

        private const byte bmv = 255; // byte.MaxValue

        // main, shadow, isLighter
        private static readonly Dictionary<ColorType, (Color32, Color32, bool)> CustomColorData = new() {
            { ColorType.Salmon, (new(239, 191, 192, bmv), new(182, 119, 114, bmv), true) },
            { ColorType.Bordeaux, (new(109, 7, 26, bmv), new(54, 2, 11, bmv), false) },
            { ColorType.Olive, (new(154, 140, 61, bmv), new(104, 95, 40, bmv), false) },
            { ColorType.Turqoise, (new(22, 132, 176, bmv), new(15, 89, 117, bmv), false) },
            { ColorType.Mint, (new(111, 192, 156, bmv), new(65, 148, 111, bmv), true) },
            { ColorType.Lavender, (new(173, 126, 201, bmv), new(131, 58, 203, bmv), true) },
            { ColorType.Nougat, (new(160, 101, 56, bmv), new(115, 15, 78, bmv), false) },
            { ColorType.Peach, (new(255, 164, 119, bmv), new(238, 128, 100, bmv), true) },
            { ColorType.Wasabi, (new(112, 143, 46, bmv), new(72, 92, 29, bmv), false) },
            { ColorType.HotPink, (new(255, 51, 102, bmv), new(232, 0, 58, bmv), true) },
            { ColorType.Petrol, (new(0, 99, 105, bmv), new(0, 61, 54, bmv), false) },
            { ColorType.Lemon, (new(0xDB, 0xFD, 0x2F, bmv), new(0x74, 0xE5, 0x10, bmv), true) },
            { ColorType.SignalOrange, (new(0xF7, 0x44, 0x17, bmv), new(0x9B, 0x2E, 0x0F, bmv), true) },
            { ColorType.Teal, (new(0x25, 0xB8, 0xBF, bmv), new(0x12, 0x89, 0x86, bmv), false) },
            { ColorType.Blurple, (new(0x59, 0x3C, 0xD6, bmv), new(0x29, 0x17, 0x96, bmv), false) },
            { ColorType.Sunrise, (new(0xFF, 0xCA, 0x19, bmv), new(0xDB, 0x44, 0x42, bmv), true) },
            { ColorType.Ice, (new(0xA8, 0xDF, 0xFF, bmv), new(0x59, 0x9F, 0xC8, bmv), true) },
            { ColorType.PitchBlack, (new(0, 0, 0, bmv), new(0, 0, 0, bmv), false) },
            { ColorType.Darkmagenta, (new(139, 0, 139, bmv), new(153, 50, 204, bmv), false) },
            { ColorType.Mintcream, (new(245, 255, 250, bmv), new(224, 255, 255, bmv), true) },
            { ColorType.Leaf, (new(62, 90, 11, bmv), new(34, 50, 6, bmv), false) },
            { ColorType.Emerald, (new(98, 214, 133, bmv), new(82, 179, 111, bmv), true) },
            { ColorType.Brightyellow, (new(248, 181, 0, bmv), new(255, 102, 0, bmv), true) },
            { ColorType.Darkaqua, (new(14, 104, 188, bmv), new(11, 85, 153, bmv), true) },
            { ColorType.Matcha, (new(52, 99, 23, bmv), new(34, 54, 19, bmv), false) },
            { ColorType.Pitchwhite, (new(255, 255, 255, bmv), new(240, 240, 240, bmv), true) },
            { ColorType.Darksky, (new(64, 128, 192, bmv), new(32, 96, 128, bmv), true) },
            { ColorType.Intenseblue, (new(83, 136, 255, bmv), new(76, 122, 230, bmv), true) },
            { ColorType.Blueclosertoblack, (new(0, 0, 50, bmv), new(0, 0, 25, bmv), false) },
            { ColorType.Sunkengreenishblue, (new(128, 156, 166, bmv), new(115, 141, 153, bmv), true) },
            { ColorType.Azi, (new(100, 48, 0, bmv), new(98, 5, 0, bmv), false) },
            { ColorType.Pitchred, (new(255, 0, 0, bmv), new(220, 20, 60, bmv), false) },
            { ColorType.Pitchblue, (new(0, 0, 128, bmv), new(0, 0, 112, bmv), false) },
            { ColorType.Pitchgreen, (new(0, 128, 0, bmv), new(0, 120, 0, bmv), false) },
            { ColorType.Pitchyellow, (new(255, 255, 0, bmv), new(255, 235, 0, bmv), true) },
            { ColorType.Backblue, (new(0, 128, 255, bmv), new(0, 85, 255, bmv), true) },
            { ColorType.Mildpurple, (new(109, 83, 131, bmv), new(109, 83, 131, bmv), false) },
            { ColorType.Ashishreddishpurplecolor, (new(139, 102, 118, bmv), new(139, 102, 118, bmv), false) },
            { ColorType.Melon, (new(0, 225, 129, bmv), new(24, 255, 81, bmv), true) },
            { ColorType.Crasyublue, (new(2, 38, 106, bmv), new(64, 0, 111, bmv), false) },
            { ColorType.Lightgreen, (new(226, 255, 5, bmv), new(192, 201, 10, bmv), true) },
            { ColorType.Azuki, (new(150, 81, 77, bmv), new(115, 40, 35, bmv), false) },
            { ColorType.Snow, (new(229, 249, 255, bmv), new(135, 226, 255, bmv), true) },
        };
        public static void Load()
        {
            List<StringNames> longlist = Enumerable.ToList(Palette.ColorNames);
            List<Color32> colorlist = Enumerable.ToList(Palette.PlayerColors);
            List<Color32> shadowlist = Enumerable.ToList(Palette.ShadowColors);


            List<CustomColor> colors = new();
            var noLighterColorTemp = new List<KeyValuePair<ColorType, (Color32, Color32, bool)>>();
            foreach (var dic in CustomColorData)
            {
                if (!dic.Value.Item3) // isLighterがfalseなら後ろに入れるため仮Listに追加して次ループ
                {
                    noLighterColorTemp.Add(dic);
                    continue;
                }
                colors.Add(new CustomColor
                {
                    longname = $"color{dic.Key}",
                    color = dic.Value.Item1,
                    shadow = dic.Value.Item2,
                    isLighterColor = true
                });
            }
            foreach (var dic in noLighterColorTemp)
            {
                colors.Add(new CustomColor
                {
                    longname = $"color{dic.Key}",
                    color = dic.Value.Item1,
                    shadow = dic.Value.Item2,
                    isLighterColor = false
                });
            }
            pickableColors += (uint)colors.Count; // Colors to show in Tab
            /** Hidden Colors **/

            /** Add Colors **/
            int id = 50000;
            foreach (CustomColor cc in colors)
            {
                longlist.Add((StringNames)id);
                ColorStrings[id++] = cc.longname;
                colorlist.Add(cc.color);
                shadowlist.Add(cc.shadow);
                if (cc.isLighterColor)
                    lighterColors.Add(colorlist.Count - 1);
            }

            Palette.ColorNames = longlist.ToArray();
            Palette.PlayerColors = colorlist.ToArray();
            Palette.ShadowColors = shadowlist.ToArray();
        }


        protected internal struct CustomColor
        {
            public string longname;
            public Color32 color;
            public Color32 shadow;
            public bool isLighterColor;
        }

        [HarmonyPatch]
        public static class CustomColorPatches
        {
            [HarmonyPatch(typeof(TranslationController), nameof(TranslationController.GetString), new[] {
                typeof(StringNames),
                typeof(Il2CppReferenceArray<Il2CppSystem.Object>)
            })]
            private class ColorStringPatch
            {
                public static bool Prefix(ref string __result, [HarmonyArgument(0)] StringNames name)
                {
                    if ((int)name >= 50000)
                    {
                        string text = CustomColors.ColorStrings[(int)name];
                        if (text != null)
                        {
                            __result = ModTranslation.GetString(text) + " (MOD)";
                            return false;
                        }
                    }
                    return true;
                }
            }
            [HarmonyPatch(typeof(PlayerTab), nameof(PlayerTab.OnEnable))]
            private static class PlayerTabEnablePatch
            {
                public static void Postfix(PlayerTab __instance)
                { // Replace instead
                    Il2CppArrayBase<ColorChip> chips = __instance.ColorChips.ToArray();

                    int cols = 10; // TODO: Design an algorithm to dynamically position chips to optimally fill space
                    for (int i = 0; i < Palette.PlayerColors.Count; i++)
                    {
                        ColorChip chip = chips[i];
                        int row = i / cols, col = i % cols; // Dynamically do the positioningS
                        chip.transform.localPosition = new Vector3(-1.5f + (col * 0.35f), 1.6f - (row * 0.45f), chip.transform.localPosition.z);
                        chip.transform.localScale *= 0.6f;
                    }
                    for (int j = Palette.PlayerColors.Count; j < chips.Length; j++)
                    { // If number isn't in order, hide it
                        ColorChip chip = chips[j];
                        chip.transform.localScale *= 0f;
                        chip.enabled = false;
                        chip.Button.enabled = false;
                        chip.Button.OnClick.RemoveAllListeners();
                    }
                }
            }
            [HarmonyPatch(typeof(LegacySaveManager), nameof(LegacySaveManager.LoadPlayerPrefs))]
            private static class LoadPlayerPrefsPatch
            { // Fix Potential issues with broken colors
                private static bool needsPatch = false;
                public static void Prefix([HarmonyArgument(0)] bool overrideLoad)
                {
                    if (!LegacySaveManager.loaded || overrideLoad)
                        needsPatch = true;
                }
                public static void Postfix()
                {
                    if (!needsPatch) return;
                    LegacySaveManager.colorConfig %= CustomColors.pickableColors;
                    needsPatch = false;
                }
            }
            [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CheckColor))]
            private static class PlayerControlCheckColorPatch
            {
                private static bool isTaken(PlayerControl player, uint color)
                {
                    foreach (GameData.PlayerInfo p in GameData.Instance.AllPlayers)
                    {
                        //Logger.Info($"{!p.Disconnected} は {p.PlayerId != player.PlayerId} は {p.DefaultOutfit.ColorId == color}", "isTaken");
                        if (!p.Disconnected && p.PlayerId != player.PlayerId && p.DefaultOutfit.ColorId == color)
                            return true;
                    }
                    return false;
                }
                public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] byte bodyColor)
                { // Fix incorrect color assignment
                    uint color = (uint)bodyColor;
                    if (isTaken(__instance, color) || color >= Palette.PlayerColors.Length)
                    {
                        int num = 0;
                        while (num++ < 50 && (color >= CustomColors.pickableColors || isTaken(__instance, color)))
                        {
                            color = (color + 1) % CustomColors.pickableColors;
                        }
                    }
                    //Logger.Info(color.ToString() + "をセット:" + isTaken(__instance, color).ToString()+":"+ (color >= Palette.PlayerColors.Length));
                    __instance.RpcSetColor((byte)color);
                    return false;
                }
            }
        }
    }
}