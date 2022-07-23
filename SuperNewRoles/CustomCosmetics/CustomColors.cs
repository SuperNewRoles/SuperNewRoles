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

        /* version 1
        private static readonly List<int> ORDER = new() { 7, 17, 5, 33, 4,
                                                                    30, 0, 19, 27, 3,
                                                                    13, 25, 18, 15, 23,
                                                                    8, 32, 1, 21, 31,
                                                                    10, 34, 12, 14, 28,
                                                                    22, 29, 11, 26, 2,
                                                                    20, 24, 9, 16, 6 }; */
        public static void Load()
        {
            List<StringNames> longlist = Enumerable.ToList<StringNames>(Palette.ColorNames);
            List<Color32> colorlist = Enumerable.ToList<Color32>(Palette.PlayerColors);
            List<Color32> shadowlist = Enumerable.ToList<Color32>(Palette.ShadowColors);

            List<CustomColor> colors = new()
            {

                /* Custom Colors */
                new CustomColor
                {
                    longname = "colorSalmon",
                    color = new Color32(239, 191, 192, byte.MaxValue), // color = new Color32(0xD8, 0x82, 0x83, byte.MaxValue),
                    shadow = new Color32(182, 119, 114, byte.MaxValue), // shadow = new Color32(0xA5, 0x63, 0x65, byte.MaxValue),
                    isLighterColor = true
                },

                new CustomColor
                {
                    longname = "colorBordeaux",
                    color = new Color32(109, 7, 26, byte.MaxValue),
                    shadow = new Color32(54, 2, 11, byte.MaxValue),
                    isLighterColor = false
                },

                new CustomColor
                {
                    longname = "colorOlive",
                    color = new Color32(154, 140, 61, byte.MaxValue),
                    shadow = new Color32(104, 95, 40, byte.MaxValue),
                    isLighterColor = false
                },

                new CustomColor
                {
                    longname = "colorTurqoise",
                    color = new Color32(22, 132, 176, byte.MaxValue),
                    shadow = new Color32(15, 89, 117, byte.MaxValue),
                    isLighterColor = false
                },

                new CustomColor
                {
                    longname = "colorMint",
                    color = new Color32(111, 192, 156, byte.MaxValue),
                    shadow = new Color32(65, 148, 111, byte.MaxValue),
                    isLighterColor = true
                },

                new CustomColor
                {
                    longname = "colorLavender",
                    color = new Color32(173, 126, 201, byte.MaxValue),
                    shadow = new Color32(131, 58, 203, byte.MaxValue),
                    isLighterColor = true
                },

                new CustomColor
                {
                    longname = "colorNougat",
                    color = new Color32(160, 101, 56, byte.MaxValue),
                    shadow = new Color32(115, 15, 78, byte.MaxValue),
                    isLighterColor = false
                },

                new CustomColor
                {
                    longname = "colorPeach",
                    color = new Color32(255, 164, 119, byte.MaxValue),
                    shadow = new Color32(238, 128, 100, byte.MaxValue),
                    isLighterColor = true
                },

                new CustomColor
                {
                    longname = "colorWasabi",
                    color = new Color32(112, 143, 46, byte.MaxValue),
                    shadow = new Color32(72, 92, 29, byte.MaxValue),
                    isLighterColor = false
                },

                new CustomColor
                {
                    longname = "colorHotPink",
                    color = new Color32(255, 51, 102, byte.MaxValue),
                    shadow = new Color32(232, 0, 58, byte.MaxValue),
                    isLighterColor = true
                },

                new CustomColor
                {
                    longname = "colorPetrol",
                    color = new Color32(0, 99, 105, byte.MaxValue),
                    shadow = new Color32(0, 61, 54, byte.MaxValue),
                    isLighterColor = false
                },

                new CustomColor
                {
                    longname = "colorLemon",
                    color = new Color32(0xDB, 0xFD, 0x2F, byte.MaxValue),
                    shadow = new Color32(0x74, 0xE5, 0x10, byte.MaxValue),
                    isLighterColor = true
                },

                new CustomColor
                {
                    longname = "colorSignalOrange",
                    color = new Color32(0xF7, 0x44, 0x17, byte.MaxValue),
                    shadow = new Color32(0x9B, 0x2E, 0x0F, byte.MaxValue),
                    isLighterColor = true
                },

                new CustomColor
                {
                    longname = "colorTeal",
                    color = new Color32(0x25, 0xB8, 0xBF, byte.MaxValue),
                    shadow = new Color32(0x12, 0x89, 0x86, byte.MaxValue),
                    isLighterColor = false
                },

                new CustomColor
                {
                    longname = "colorBlurple",
                    color = new Color32(0x59, 0x3C, 0xD6, byte.MaxValue),
                    shadow = new Color32(0x29, 0x17, 0x96, byte.MaxValue),
                    isLighterColor = false
                },

                new CustomColor
                {
                    longname = "colorSunrise",
                    color = new Color32(0xFF, 0xCA, 0x19, byte.MaxValue),
                    shadow = new Color32(0xDB, 0x44, 0x42, byte.MaxValue),
                    isLighterColor = true
                },

                new CustomColor
                {
                    longname = "colorIce",
                    color = new Color32(0xA8, 0xDF, 0xFF, byte.MaxValue),
                    shadow = new Color32(0x59, 0x9F, 0xC8, byte.MaxValue),
                    isLighterColor = true
                },

                new CustomColor
                {
                    longname = "colorPitchBlack",
                    color = new Color32(0, 0, 0, byte.MaxValue),
                    shadow = new Color32(0, 0, 0, byte.MaxValue),
                    isLighterColor = false
                },

                new CustomColor
                {
                    longname = "colorDarkmagenta",
                    color = new Color32(139, 0, 139, byte.MaxValue),
                    shadow = new Color32(153, 50, 204, byte.MaxValue),
                    isLighterColor = false
                },

                new CustomColor
                {
                    longname = "colorMintcream",
                    color = new Color32(245, 255, 250, byte.MaxValue),
                    shadow = new Color32(224, 255, 255, byte.MaxValue),
                    isLighterColor = true
                },

                new CustomColor
                {
                    longname = "colorLeaf",
                    color = new Color32(62, 90, 11, byte.MaxValue),
                    shadow = new Color32(34, 50, 6, byte.MaxValue),
                    isLighterColor = false
                },

                new CustomColor
                {
                    longname = "colorEmerald",
                    color = new Color32(98, 214, 133, byte.MaxValue),
                    shadow = new Color32(82, 179, 111, byte.MaxValue),
                    isLighterColor = true
                },

                new CustomColor
                {
                    longname = "colorBrightyellow",
                    color = new Color32(248, 181, 0, byte.MaxValue),
                    shadow = new Color32(255, 102, 0, byte.MaxValue),
                    isLighterColor = true
                },

                new CustomColor
                {
                    longname = "colorDarkaqua",
                    color = new Color32(14, 104, 188, byte.MaxValue),
                    shadow = new Color32(11, 85, 153, byte.MaxValue),
                    isLighterColor = true
                },

                new CustomColor
                {
                    longname = "colorMatcha",
                    color = new Color32(52, 99, 23, byte.MaxValue),
                    shadow = new Color32(34, 54, 19, byte.MaxValue),
                    isLighterColor = false
                },

                new CustomColor
                {
                    longname = "colorPitchwhite",
                    color = new Color32(255, 255, 255, byte.MaxValue),
                    shadow = new Color32(240, 240, 240, byte.MaxValue),
                    isLighterColor = true
                },

                new CustomColor
                {
                    longname = "colorDarksky",
                    color = new Color32(64, 128, 192, byte.MaxValue),
                    shadow = new Color32(32, 96, 128, byte.MaxValue),
                    isLighterColor = true
                },

                new CustomColor
                {
                    longname = "colorIntenseblue",
                    color = new Color32(83, 136, 255, byte.MaxValue),
                    shadow = new Color32(76, 122, 230, byte.MaxValue),
                    isLighterColor = true
                },

                new CustomColor
                {
                    longname = "colorBlueclosertoblack",
                    color = new Color32(0, 0, 50, byte.MaxValue),
                    shadow = new Color32(0, 0, 25, byte.MaxValue),
                    isLighterColor = false
                },

                new CustomColor
                {
                    longname = "colorSunkengreenishblue",
                    color = new Color32(128, 156, 166, byte.MaxValue),
                    shadow = new Color32(115, 141, 153, byte.MaxValue),
                    isLighterColor = true
                },

                new CustomColor
                {
                    longname = "colorAzi",
                    color = new Color32(100, 48, 0, byte.MaxValue),
                    shadow = new Color32(98, 5, 0, byte.MaxValue),
                    isLighterColor = false
                },

                new CustomColor
                {
                    longname = "colorPitchred",
                    color = new Color32(255, 0, 0, byte.MaxValue),
                    shadow = new Color32(220, 20, 60, byte.MaxValue),
                    isLighterColor = true
                },

                new CustomColor
                {
                    longname = "colorPitchblue",
                    color = new Color32(0, 0, 128, byte.MaxValue),
                    shadow = new Color32(0, 0, 112, byte.MaxValue),
                    isLighterColor = true
                },

                new CustomColor
                {
                    longname = "colorPitchgreen",
                    color = new Color32(0, 128, 0, byte.MaxValue),
                    shadow = new Color32(0, 120, 0, byte.MaxValue),
                    isLighterColor = true
                },

                new CustomColor
                {
                    longname = "colorPitchyellow",
                    color = new Color32(255, 255, 0, byte.MaxValue),
                    shadow = new Color32(255, 235, 0, byte.MaxValue),
                    isLighterColor = true
                },

                new CustomColor
                {
                    longname = "colorBackblue",
                    color = new Color32(0, 128, 255, byte.MaxValue),
                    shadow = new Color32(0, 85, 255, byte.MaxValue),
                    isLighterColor = true
                },

                new CustomColor
                {
                    longname = "colorMildpurple",
                    color = new Color32(109, 83, 131, byte.MaxValue),
                    shadow = new Color32(109, 83, 131, byte.MaxValue),
                    isLighterColor = true
                },

                new CustomColor
                {
                    longname = "colorAshishreddishpurplecolor",
                    color = new Color32(139, 102, 118, byte.MaxValue),
                    shadow = new Color32(139, 102, 118, byte.MaxValue),
                    isLighterColor = false
                },

                new CustomColor
                {
                    longname = "colorMelon",
                    color = new Color32(0, 225, 129, byte.MaxValue),
                    shadow = new Color32(24, 255, 81, byte.MaxValue),
                    isLighterColor = true
                },

                new CustomColor
                {
                    longname = "colorCrasyublue",
                    color = new Color32(2, 38, 106, byte.MaxValue),
                    shadow = new Color32(64, 0, 111, byte.MaxValue),
                    isLighterColor = false
                },

                new CustomColor
                {
                    longname = "colorLightgreen",
                    color = new Color32(226, 255, 5, byte.MaxValue),
                    shadow = new Color32(192, 201, 10, byte.MaxValue),
                    isLighterColor = true
                }
            };
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