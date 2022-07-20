using HarmonyLib;
using static SuperNewRoles.CustomCosmetics.CustomCosmeticsMenus.Patch.ObjectData;

namespace SuperNewRoles.CustomCosmetics.CustomCosmeticsMenus.Patch
{
    class SelectPatch
    {
        [HarmonyPatch(typeof(SaveManager), nameof(SaveManager.BodyColor), MethodType.Setter)]
        public static class SelectColor
        {
            public static void Postfix(ref byte value)
            {
                if (GetData().BodyColor.Value != value)
                {
                    GetData().BodyColor.Value = value;
                }
            }
        }
        [HarmonyPatch(typeof(SaveManager), nameof(SaveManager.LastVisor), MethodType.Setter)]
        public static class SelectVisor
        {
            public static void Postfix(ref string value)
            {
                if (GetData().Visor.Value != value)
                {
                    GetData().Visor.Value = value;
                }
            }
        }
        [HarmonyPatch(typeof(SaveManager), nameof(SaveManager.LastHat), MethodType.Setter)]
        public static class SelectHat
        {
            public static void Postfix(ref string value)
            {
                if (GetData().Hat.Value != value)
                {
                    GetData().Hat.Value = value;
                }
            }
        }
        [HarmonyPatch(typeof(SaveManager), nameof(SaveManager.LastSkin), MethodType.Setter)]
        public static class SelectSkin
        {
            public static void Postfix(ref string value)
            {
                if (GetData().Skin.Value != value)
                {
                    GetData().Skin.Value = value;
                }
            }
        }
        [HarmonyPatch(typeof(SaveManager), nameof(SaveManager.LastNamePlate), MethodType.Setter)]
        public static class SelectNamePlate
        {
            public static void Postfix(ref string value)
            {
                if (GetData().NamePlate.Value != value)
                {
                    GetData().NamePlate.Value = value;
                }
            }
        }
        [HarmonyPatch(typeof(SaveManager), nameof(SaveManager.LastPet), MethodType.Setter)]
        public static class SelectPet
        {
            public static void Postfix(ref string value)
            {
                if (GetData().Pet.Value != value)
                {
                    GetData().Pet.Value = value;
                }
            }
        }
        public static ClosetPresetData GetData(int index = -1)
        {
            if (index == -1) index = SelectedPreset.Value;
            ClosetPresetData data = !ClosetPresetDatas.ContainsKey(index)
                ? (new()
                {
                    BodyColor = SuperNewRolesPlugin.Instance.Config.Bind("ClosetPreset_" + index.ToString(), "BodyColor", (byte)0),
                    Hat = SuperNewRolesPlugin.Instance.Config.Bind("ClosetPreset_" + index.ToString(), "Hat", ""),
                    Visor = SuperNewRolesPlugin.Instance.Config.Bind("ClosetPreset_" + index.ToString(), "Visor", ""),
                    Skin = SuperNewRolesPlugin.Instance.Config.Bind("ClosetPreset_" + index.ToString(), "Skin", ""),
                    NamePlate = SuperNewRolesPlugin.Instance.Config.Bind("ClosetPreset_" + index.ToString(), "NamePlate", ""),
                    Pet = SuperNewRolesPlugin.Instance.Config.Bind("ClosetPreset_" + index.ToString(), "Pet", "")
                })
                : ClosetPresetDatas[index];
            return data;
        }
    }
}