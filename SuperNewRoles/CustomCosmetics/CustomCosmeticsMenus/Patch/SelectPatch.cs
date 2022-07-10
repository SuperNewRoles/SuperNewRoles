using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using static SuperNewRoles.CustomCosmetics.CustomCosmeticsMenus.Patch.ObjectData;

namespace SuperNewRoles.CustomCosmetics.CustomCosmeticsMenus.Patch
{
    class SelectPatch
    {
        [HarmonyPatch(typeof(PlayerTab), nameof(PlayerTab.SelectColor))]
        class SelectColor
        {
            public static void Postfix(int colorId)
            {
                GetData().BodyColor.Value = (byte)colorId;
            }
        }
        [HarmonyPatch(typeof(HatsTab),nameof(HatsTab.SelectHat))]
        class SelectHat
        {
            public static void Postfix(HatData hat)
            {
                GetData().Hat.Value = hat.ProductId;
            }
        }
        [HarmonyPatch(typeof(VisorsTab), nameof(VisorsTab.SelectVisor))]
        class SelectVisor
        {
            public static void Postfix(VisorData visor)
            {
                GetData().Visor.Value = visor.ProductId;
            }
        }
        [HarmonyPatch(typeof(SkinsTab), nameof(SkinsTab.SelectSkin))]
        class SelectSkin
        {
            public static void Postfix(SkinData skin)
            {
                GetData().Skin.Value = skin.ProductId;
            }
        }
        [HarmonyPatch(typeof(NameplatesTab), nameof(NameplatesTab.SelectNameplate))]
        class SelectNamePlate
        {
            public static void Postfix(NamePlateData plate)
            {
                GetData().NamePlate.Value = plate.ProductId;
            }
        }
        [HarmonyPatch(typeof(PetsTab), nameof(PetsTab.SelectPet))]
        class SelectPet
        {
            public static void Postfix(PetData pet)
            {
                GetData().Pet.Value = pet.ProductId;
            }
        }
        public static ClosetPresetData GetData(int index = -1)
        {
            if (index == -1) index = SelectedPreset.Value;
            ClosetPresetData data = null;
            if (!ClosetPresetDatas.ContainsKey(index))
            {
                data = new();
                data.BodyColor = SuperNewRolesPlugin.Instance.Config.Bind("ClosetPreset_" + index.ToString(), "BodyColor", (byte)0);
                data.Hat = SuperNewRolesPlugin.Instance.Config.Bind("ClosetPreset_" + index.ToString(), "Hat", "");
                data.Visor = SuperNewRolesPlugin.Instance.Config.Bind("ClosetPreset_" + index.ToString(), "Visor", "");
                data.Skin = SuperNewRolesPlugin.Instance.Config.Bind("ClosetPreset_" + index.ToString(), "Skin", "");
                data.NamePlate = SuperNewRolesPlugin.Instance.Config.Bind("ClosetPreset_" + index.ToString(), "NamePlate", "");
                data.Pet = SuperNewRolesPlugin.Instance.Config.Bind("ClosetPreset_" + index.ToString(), "Pet", "");
            }
            else
            {
                data = ClosetPresetDatas[index];
            }
            return data;
        }
    }
}
