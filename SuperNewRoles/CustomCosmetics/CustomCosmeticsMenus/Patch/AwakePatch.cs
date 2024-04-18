using HarmonyLib;
using TMPro;
using UnityEngine;

namespace SuperNewRoles.CustomCosmetics.CustomCosmeticsMenus.Patch;

[HarmonyPatch(typeof(PlayerCustomizationMenu), nameof(PlayerCustomizationMenu.Start))]
class AwakePatch
{
    public static void Prefix()
    {
        TabSelectPatch.IsFirst = true;
    }
    public static void Postfix(PlayerCustomizationMenu __instance)
    {
        CustomHats.HatsTabOnEnablePatch.Chips = new();
        VisorTabPatch.VisorsTabOnEnablePatch.Chips = new();
        ObjectData.Presets = new Transform[] { };
        ObjectData.hats = new HatParent[] { };
        ObjectData.Selected = "";
        ObjectData.HatTabButtons = new Transform[] { };
        TabSelectPatch.IsFirst = false;
        var ClosetTabButton = __instance.Tabs[0].Button.transform.parent;
        var PresetTabButton = __instance.Tabs[1].Button.transform.parent;
        var ClosetTab = __instance.Tabs[0].Tab.transform;
        var PresetTab = __instance.Tabs[1].Tab.transform;
        SuperNewRolesPlugin.Logger.LogInfo(ClosetTabButton.name + ":" + PresetTabButton.name);
        int i = 0;
        foreach (TabButton button in __instance.Tabs)
        {
            SuperNewRolesPlugin.Logger.LogInfo(button.Button.transform.parent.parent.name);
            if (i > 1)
            {
                button.Button.transform.parent.parent.gameObject.SetActive(false);
            }
            i++;
        }

        var ColorButton = GameObject.Instantiate(GameObject.FindObjectOfType<PlayerTab>().ColorChips[0], ClosetTab);
        var ColorButton_Passive = ColorButton.Button;
        ColorButton_Passive.OnClick = new();
        ColorButton_Passive.OnClick.AddListener((UnityEngine.Events.UnityAction)(() => ObjectData.ColorShow()));
        ObjectData.ColorButton = ColorButton;
        ColorButton.name = "ColorButton";

        ColorButton.GetComponent<SpriteRenderer>().color = new Color32(0xA8, 0xDF, 0xFF, byte.MaxValue);
        ColorButton.SelectionHighlight.color = Color.white;
        var HatButton = GameObject.Instantiate(ColorButton, ClosetTab);
        var HatButton_Passive = HatButton.Button;
        HatButton_Passive.OnClick = new();
        HatButton_Passive.OnClick.AddListener((UnityEngine.Events.UnityAction)(() => ObjectData.HatShow()));
        ObjectData.HatButton = HatButton;
        HatButton.name = "HatButton";
        HatButton.transform.localScale *= 2.75f;
        HatButton.SelectionHighlight.color = Color.white;

        var SkinButton = GameObject.Instantiate(HatButton, ClosetTab);
        var SkinButton_Passive = SkinButton.Button;
        SkinButton_Passive.OnClick = new();
        SkinButton_Passive.OnClick.AddListener((UnityEngine.Events.UnityAction)(() => ObjectData.CosmicShow("SkinGroup")));
        ObjectData.SkinButton = SkinButton;
        SkinButton.name = "SkinButton";

        var PetButton = GameObject.Instantiate(HatButton, ClosetTab);
        var PetButton_Passive = PetButton.Button;
        PetButton_Passive.OnClick = new();
        PetButton_Passive.OnClick.AddListener((UnityEngine.Events.UnityAction)(() => ObjectData.CosmicShow("PetsGroup")));
        ObjectData.PetButton = PetButton;
        PetButton.name = "PetButton";

        var VisorButton = GameObject.Instantiate(HatButton, ClosetTab);
        var VisorButton_Passive = VisorButton.Button;
        VisorButton_Passive.OnClick = new();
        VisorButton_Passive.OnClick.AddListener((UnityEngine.Events.UnityAction)(() => ObjectData.CosmicShow("VisorGroup")));
        ObjectData.VisorButton = VisorButton;
        VisorButton.name = "VisorButton";

        var NamePlateButton = GameObject.Instantiate(HatButton, ClosetTab);
        var NamePlateButton_Passive = NamePlateButton.Button;
        NamePlateButton_Passive.OnClick = new();
        NamePlateButton_Passive.OnClick.AddListener((UnityEngine.Events.UnityAction)(() => ObjectData.CosmicShow("NameplateGroup")));
        ObjectData.NamePlateButton = NamePlateButton;
        NamePlateButton.name = "NamePlateButton";

        var CubeButton = GameObject.Instantiate(HatButton, ClosetTab);
        var CubeButton_Passive = CubeButton.Button;
        CubeButton_Passive.OnClick = new();
        CubeButton_Passive.OnClick.AddListener((UnityEngine.Events.UnityAction)(() => ObjectData.CubeShow()));
        ObjectData.CubeButton = CubeButton;
        CubeButton.name = "CubeButton";

        UpdatePatch.area = __instance.transform.FindChild("Background/RightPanel/PlayerVoteArea").GetComponent<PlayerVoteArea>();

        ObjectData.ColorText = __instance.transform.FindChild("ColorGroup/Text").GetComponent<TextMeshPro>();
        ObjectData.HatText = GameObject.Instantiate(ObjectData.ColorText, ClosetTab);
        GameObject.Destroy(ObjectData.HatText.GetComponent<TextTranslatorTMP>());
        ObjectData.VisorText = GameObject.Instantiate(ObjectData.HatText, ClosetTab);
        ObjectData.SkinText = GameObject.Instantiate(ObjectData.HatText, ClosetTab);
        ObjectData.NamePlateText = GameObject.Instantiate(ObjectData.HatText, ClosetTab);
        ObjectData.PetText = GameObject.Instantiate(ObjectData.HatText, ClosetTab);
        ObjectData.CubeText = GameObject.Instantiate(ObjectData.HatText, ClosetTab);

        ObjectData.ColorText.name = "ColorText";
        ObjectData.HatText.name = "HatText";
        ObjectData.VisorText.name = "VisorText";
        ObjectData.SkinText.name = "SkinText";
        ObjectData.NamePlateText.name = "NamePlateText";
        ObjectData.PetText.name = "PetText";
        ObjectData.CubeText.name = "CubeText";

        ObjectData.HatText.text = FastDestroyableSingleton<TranslationController>.Instance.GetString(StringNames.HatLabel);
        ObjectData.VisorText.text = FastDestroyableSingleton<TranslationController>.Instance.GetString(StringNames.Visor);
        ObjectData.SkinText.text = FastDestroyableSingleton<TranslationController>.Instance.GetString(StringNames.SkinLabel);
        ObjectData.ColorText.text = FastDestroyableSingleton<TranslationController>.Instance.GetString(StringNames.Colors);
        ObjectData.NamePlateText.text = FastDestroyableSingleton<TranslationController>.Instance.GetString(StringNames.NamePlate);
        ObjectData.PetText.text = FastDestroyableSingleton<TranslationController>.Instance.GetString(StringNames.PetLabel);
        ObjectData.CubeText.text = FastDestroyableSingleton<TranslationController>.Instance.GetString(StringNames.Cosmicubes);

        ObjectData.ColorButton_SpriteRend = ObjectData.ColorButton.GetComponent<SpriteRenderer>();

        Transform ClosetIcon = ClosetTabButton.FindChild("Icon");
        Transform PresetIcon = PresetTabButton.FindChild("Icon");

        ClosetIcon.GetComponent<SpriteRenderer>().sprite = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.Cosmetics.ClosetButton.png", 115f);
        PresetIcon.GetComponent<SpriteRenderer>().sprite = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.Cosmetics.PresetButton.png", 115f);

        ClosetIcon.localScale *= 2;
        PresetIcon.localScale *= 2;
        __instance.transform.FindChild("SkinGroup").gameObject.SetActive(true);
        __instance.transform.FindChild("SkinGroup").gameObject.SetActive(false);

        ObjectData.HatButton_Hat = GameObject.Instantiate(__instance.PreviewArea.cosmetics.hat, ClosetTab);
        ObjectData.HatButton_Hat.transform.localPosition = new Vector3(4.85f, 0.6f, -1);

        ObjectData.SkinButton_Skin = GameObject.Instantiate(__instance.PreviewArea.cosmetics.skin, ClosetTab);
        ObjectData.SkinButton_Skin.transform.localPosition = new Vector3(0.875f, 0, -1);

        ObjectData.VisorButton_Visor = GameObject.Instantiate(__instance.PreviewArea.cosmetics.visor, ClosetTab);
        ObjectData.VisorButton_Visor.transform.localPosition = new Vector3(0.78f, 1.55f, -10);

        ObjectData.CosmicubeMenuHolderTint = __instance.transform.FindChild("CosmicubeMenuHolder/Tint").GetComponent<SpriteRenderer>();

        ObjectData.ClosetShow();
    }
}
[HarmonyPatch(typeof(PlayerCustomizationMenu), nameof(PlayerCustomizationMenu.OpenTab))]
class TabSelectPatch
{
    public static bool IsFirst = false;
    public static bool Prefix(PlayerCustomizationMenu __instance, InventoryTab tab)
    {
        if (IsFirst)
        {
            ObjectData.IsShow = false;
            return true;
        }
        SuperNewRolesPlugin.Logger.LogInfo(tab.name);
        if (tab.name == "ColorGroup")
        {
            ObjectData.ClosetShow();
        }
        else if (tab.name == "HatsGroup")
        {
            ObjectData.PresetShow();
        }
        return false;
    }
}