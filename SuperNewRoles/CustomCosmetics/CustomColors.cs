using System.Collections.Generic;
using System.Linq;
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
            Lightgreen
        }


        public static Dictionary<ColorType, Color32[]> LighterColorData = new(); // isLighterColorがtrue
        public static Dictionary<ColorType, Color32[]> NoLighterColorData = new(); // isLighterColorがfalse

        public static void Load()
        {
            List<StringNames> longlist = Enumerable.ToList(Palette.ColorNames);
            List<Color32> colorlist = Enumerable.ToList(Palette.PlayerColors);
            List<Color32> shadowlist = Enumerable.ToList(Palette.ShadowColors);


            LighterColorData.Add(ColorType.Salmon, new Color32[] { new(239, 191, 192, byte.MaxValue), new(182, 119, 114, byte.MaxValue) });
            NoLighterColorData.Add(ColorType.Bordeaux, new Color32[] { new(109, 7, 26, byte.MaxValue), new(54, 2, 11, byte.MaxValue) });
            NoLighterColorData.Add(ColorType.Olive, new Color32[] { new(154, 140, 61, byte.MaxValue), new(104, 95, 40, byte.MaxValue) });
            NoLighterColorData.Add(ColorType.Turqoise, new Color32[] { new(22, 132, 176, byte.MaxValue), new(15, 89, 117, byte.MaxValue) });
            LighterColorData.Add(ColorType.Mint, new Color32[] { new(111, 192, 156, byte.MaxValue), new(65, 148, 111, byte.MaxValue) });
            LighterColorData.Add(ColorType.Lavender, new Color32[] { new(173, 126, 201, byte.MaxValue), new(131, 58, 203, byte.MaxValue) });
            NoLighterColorData.Add(ColorType.Nougat, new Color32[] { new(160, 101, 56, byte.MaxValue), new(115, 15, 78, byte.MaxValue) });
            LighterColorData.Add(ColorType.Peach, new Color32[] { new(255, 164, 119, byte.MaxValue), new(238, 128, 100, byte.MaxValue) });
            NoLighterColorData.Add(ColorType.Wasabi, new Color32[] { new(112, 143, 46, byte.MaxValue), new(72, 92, 29, byte.MaxValue) });
            LighterColorData.Add(ColorType.HotPink, new Color32[] { new(255, 51, 102, byte.MaxValue), new(232, 0, 58, byte.MaxValue) });
            LighterColorData.Add(ColorType.Petrol, new Color32[] { new(0, 99, 105, byte.MaxValue), new(0, 61, 54, byte.MaxValue) });
            LighterColorData.Add(ColorType.Lemon, new Color32[] { new(0xDB, 0xFD, 0x2F, byte.MaxValue), new(0x74, 0xE5, 0x10, byte.MaxValue) });
            LighterColorData.Add(ColorType.SignalOrange, new Color32[] { new(0xF7, 0x44, 0x17, byte.MaxValue), new(0x9B, 0x2E, 0x0F, byte.MaxValue) });
            NoLighterColorData.Add(ColorType.Teal, new Color32[] { new(0x25, 0xB8, 0xBF, byte.MaxValue), new(0x12, 0x89, 0x86, byte.MaxValue) });
            NoLighterColorData.Add(ColorType.Blurple, new Color32[] { new(0x59, 0x3C, 0xD6, byte.MaxValue), new(0x29, 0x17, 0x96, byte.MaxValue) });
            LighterColorData.Add(ColorType.Sunrise, new Color32[] { new(0xFF, 0xCA, 0x19, byte.MaxValue), new(0xDB, 0x44, 0x42, byte.MaxValue) });
            LighterColorData.Add(ColorType.Ice, new Color32[] { new(0xA8, 0xDF, 0xFF, byte.MaxValue), new(0x59, 0x9F, 0xC8, byte.MaxValue) });
            NoLighterColorData.Add(ColorType.PitchBlack, new Color32[] { new(0, 0, 0, byte.MaxValue), new(0, 0, 0, byte.MaxValue) });
            NoLighterColorData.Add(ColorType.Darkmagenta, new Color32[] { new(139, 0, 139, byte.MaxValue), new(153, 50, 204, byte.MaxValue) });
            LighterColorData.Add(ColorType.Mintcream, new Color32[] { new(245, 255, 250, byte.MaxValue), new(224, 255, 255, byte.MaxValue) });
            NoLighterColorData.Add(ColorType.Leaf, new Color32[] { new(62, 90, 11, byte.MaxValue), new(34, 50, 6, byte.MaxValue) });
            LighterColorData.Add(ColorType.Emerald, new Color32[] { new(98, 214, 133, byte.MaxValue), new(82, 179, 111, byte.MaxValue) });
            LighterColorData.Add(ColorType.Brightyellow, new Color32[] { new(248, 181, 0, byte.MaxValue), new(255, 102, 0, byte.MaxValue) });
            LighterColorData.Add(ColorType.Darkaqua, new Color32[] { new(14, 104, 188, byte.MaxValue), new(11, 85, 153, byte.MaxValue) });
            NoLighterColorData.Add(ColorType.Matcha, new Color32[] { new(52, 99, 23, byte.MaxValue), new(34, 54, 19, byte.MaxValue) });
            LighterColorData.Add(ColorType.Pitchwhite, new Color32[] { new(255, 255, 255, byte.MaxValue), new(240, 240, 240, byte.MaxValue) });
            LighterColorData.Add(ColorType.Darksky, new Color32[] { new(64, 128, 192, byte.MaxValue), new(32, 96, 128, byte.MaxValue) });
            LighterColorData.Add(ColorType.Intenseblue, new Color32[] { new(83, 136, 255, byte.MaxValue), new(76, 122, 230, byte.MaxValue) });
            NoLighterColorData.Add(ColorType.Blueclosertoblack, new Color32[] { new(0, 0, 50, byte.MaxValue), new(0, 0, 25, byte.MaxValue) });
            LighterColorData.Add(ColorType.Sunkengreenishblue, new Color32[] { new(128, 156, 166, byte.MaxValue), new(115, 141, 153, byte.MaxValue) });
            NoLighterColorData.Add(ColorType.Azi, new Color32[] { new(100, 48, 0, byte.MaxValue), new(98, 5, 0, byte.MaxValue) });
            LighterColorData.Add(ColorType.Pitchred, new Color32[] { new(255, 0, 0, byte.MaxValue), new(220, 20, 60, byte.MaxValue) });
            LighterColorData.Add(ColorType.Pitchblue, new Color32[] { new(0, 0, 128, byte.MaxValue), new(0, 0, 112, byte.MaxValue) });
            LighterColorData.Add(ColorType.Pitchgreen, new Color32[] { new(0, 128, 0, byte.MaxValue), new(0, 120, 0, byte.MaxValue) });
            LighterColorData.Add(ColorType.Pitchyellow, new Color32[] { new(255, 255, 0, byte.MaxValue), new(255, 235, 0, byte.MaxValue) });
            LighterColorData.Add(ColorType.Backblue, new Color32[] { new(0, 128, 255, byte.MaxValue), new(0, 85, 255, byte.MaxValue) });
            LighterColorData.Add(ColorType.Mildpurple, new Color32[] { new(109, 83, 131, byte.MaxValue), new(109, 83, 131, byte.MaxValue) });
            NoLighterColorData.Add(ColorType.Ashishreddishpurplecolor, new Color32[] { new(139, 102, 118, byte.MaxValue), new(139, 102, 118, byte.MaxValue) });
            LighterColorData.Add(ColorType.Melon, new Color32[] { new(0, 225, 129, byte.MaxValue), new(24, 255, 81, byte.MaxValue) });
            NoLighterColorData.Add(ColorType.Crasyublue, new Color32[] { new(2, 38, 106, byte.MaxValue), new(64, 0, 111, byte.MaxValue) });
            LighterColorData.Add(ColorType.Lightgreen, new Color32[] { new(226, 255, 5, byte.MaxValue), new(192, 201, 10, byte.MaxValue) });


            List<CustomColor> colors = new();
            foreach (KeyValuePair<ColorType, Color32[]> dicItem in LighterColorData)
            {
                colors.Add(new CustomColor
                {
                    longname = $"color{dicItem.Key}",
                    color = dicItem.Value[0],
                    shadow = dicItem.Value[1],
                    isLighterColor = true
                });
            }
            foreach (KeyValuePair<ColorType, Color32[]> dicItem in NoLighterColorData)
            {
                colors.Add(new CustomColor
                {
                    longname = $"color{dicItem.Key}",
                    color = dicItem.Value[0],
                    shadow = dicItem.Value[1],
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
            [HarmonyPatch(typeof(SaveManager), nameof(SaveManager.LoadPlayerPrefs))]
            private static class LoadPlayerPrefsPatch
            { // Fix Potential issues with broken colors
                private static bool needsPatch = false;
                public static void Prefix([HarmonyArgument(0)] bool overrideLoad)
                {
                    if (!SaveManager.loaded || overrideLoad)
                        needsPatch = true;
                }
                public static void Postfix()
                {
                    if (!needsPatch) return;
                    SaveManager.colorConfig %= CustomColors.pickableColors;
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