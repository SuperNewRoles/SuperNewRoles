using System.Collections.Generic;
using AmongUs.Data;
using BepInEx.Configuration;
using TMPro;
using UnityEngine;

namespace SuperNewRoles.CustomCosmetics.CustomCosmeticsMenus.Patch;

public static class ObjectData
{
    public static TextMeshPro HatText;
    public static TextMeshPro VisorText;
    public static TextMeshPro SkinText;
    public static TextMeshPro ColorText;
    public static TextMeshPro NamePlateText;
    public static TextMeshPro PetText;
    public static TextMeshPro CubeText;
    public static ColorChip ColorButton;
    public static ColorChip HatButton;
    public static ColorChip SkinButton;
    public static ColorChip PetButton;
    public static ColorChip VisorButton;
    public static ColorChip NamePlateButton;
    public static ColorChip CubeButton;
    public static HatParent HatButton_Hat;
    public static SkinLayer SkinButton_Skin;
    public static PetBehaviour PetButton_Pet;
    public static VisorLayer VisorButton_Visor;
    public static NameplateChip NamePlateButton_NamePlate;
    public static SpriteRenderer ColorButton_SpriteRend;
    public static string Selected;
    public static Transform[] HatTabButtons;
    public static PassiveButton area_pas;
    public static SpriteRenderer CosmicubeMenuHolderTint;
    public static bool IsCube;

    public static Transform[] Presets;
    public static PoolablePlayer[] PresetAreas;

    public static bool IsShow;
    public static bool IsCloset;
    public static void HatShow()
    {
        ResetShow();
        IsShow = true;
        HideDefaultTabButton();
        PlayerCustomizationMenu.Instance.transform.FindChild("HatsGroup").gameObject.SetActive(true);
        ShowDefaultTabButton();
    }
    public static void CubeShow()
    {
        ResetShow();
        IsShow = true;
        IsCube = true;
        PlayerCustomizationMenu.Instance.cubesTab.gameObject.SetActive(true);
        PlayerCustomizationMenu.Instance.transform.FindChild("Background/RightPanel/CubeView").gameObject.SetActive(true);
    }
    /// <summary>
    /// GameObjectのSetActiveをtrueにする
    /// </summary>
    /// <param name="obj">trueにしたいGameObject</param>
    public static void CosmicShow(string obj)
    {
        ResetShow();
        IsShow = true;
        PlayerCustomizationMenu.Instance.transform.FindChild(obj).gameObject.SetActive(true);
        if (obj is "NameplateGroup")
        {
            UpdatePatch.area.gameObject.SetActive(true);
            UpdatePatch.area.transform.localPosition = new(3.5f, 0, -70.71f);
        }
    }
    public static void ColorShow()
    {
        ResetShow();
        IsShow = true;
        PlayerCustomizationMenu.Instance.transform.FindChild("ColorGroup").gameObject.SetActive(true);
        foreach (ColorChip chip in GameObject.FindObjectOfType<PlayerTab>().ColorChips)
        {
            chip.gameObject.SetActive(true);
        }
        ColorText.gameObject.SetActive(true);
    }
    public static void ClickHatTab(string package)
    {
        Selected = package;
        foreach (Transform obj in HatTabButtons)
        {
            obj.GetChild(0).FindChild("Tab Background").GetComponent<SpriteRenderer>().enabled = obj.name == package;
        }
        if (hats.Length <= 0)
        {
            hats = PlayerCustomizationMenu.Instance.transform.FindChild("HatsGroup").GetComponentsInChildren<HatParent>();
        }
        HatsTab hatstab = PlayerCustomizationMenu.Instance.transform.FindChild("HatsGroup").GetComponent<HatsTab>();
        /*
        foreach (var data in CustomHats.HatsTabOnEnablePatch.Chips)
        {
            SuperNewRolesPlugin.Logger.LogInfo(data + "をDestroy");
            GameObject.Destroy(data);
        }
        */
        hatstab.ColorChips = new();
        // CustomHats.HatsTabOnEnablePatch.Chips = new();
        hatstab.OnEnable();
    }
    public static HatParent[] hats;
    public static void HideDefaultTabButton()
    {
        PlayerCustomizationMenu.Instance.Tabs[0].Button.transform.parent.gameObject.SetActive(false);
        PlayerCustomizationMenu.Instance.Tabs[1].Button.transform.parent.gameObject.SetActive(false);
    }
    public static void ShowDefaultTabButton()
    {
        PlayerCustomizationMenu.Instance.Tabs[0].Button.transform.parent.gameObject.SetActive(true);
        PlayerCustomizationMenu.Instance.Tabs[1].Button.transform.parent.gameObject.SetActive(true);
    }
    public static void ResetShow()
    {
        ShowDefaultTabButton();
        PlayerCustomizationMenu.Instance.transform.FindChild("Header/Tabs/HatsTab/Hat Button/Tab Background").GetComponent<SpriteRenderer>().enabled = false;
        PlayerCustomizationMenu.Instance.transform.FindChild("Header/Tabs/ColorTab/ColorButton/Tab Background").GetComponent<SpriteRenderer>().enabled = false;
        IsShow = false;
        IsCloset = false;
        IsCube = false;
        ClosetHide();
        PresetHide();
        ShowDefaultTabButton();
        foreach (TabButton button in PlayerCustomizationMenu.Instance.Tabs)
        {
            GameObject btn = button.Tab.gameObject;
            if (btn.active)
            {
                btn.SetActive(false);
            }
        }
        UpdatePatch.area.gameObject.SetActive(false);
        PlayerCustomizationMenu.Instance.transform.FindChild("Background/RightPanel/CubeView").gameObject.SetActive(false);
    }
    public static void ClosetHide()
    {
        HatText.gameObject.SetActive(false);
        VisorText.gameObject.SetActive(false);
        SkinText.gameObject.SetActive(false);
        ColorText.gameObject.SetActive(false);
        NamePlateText.gameObject.SetActive(false);
        PetText.gameObject.SetActive(false);
        CubeText.gameObject.SetActive(false);
        ColorButton.gameObject.SetActive(false);
        HatButton.gameObject.SetActive(false);
        SkinButton.gameObject.SetActive(false);
        PetButton.gameObject.SetActive(false);
        VisorButton.gameObject.SetActive(false);
        NamePlateButton.gameObject.SetActive(false);
        CubeButton.gameObject.SetActive(false);
        HatButton_Hat.gameObject.SetActive(false);
        SkinButton_Skin.gameObject.SetActive(false);
        //PetButton_Pet.gameObject.SetActive(false);
        VisorButton_Visor.Visible = false;
        PlayerCustomizationMenu.Instance.itemName.gameObject.SetActive(true);
        //NamePlateButton_NamePlate.gameObject.SetActive(false);
    }
    public static void ClosetShow()
    {
        ResetShow();
        PlayerCustomizationMenu.Instance.transform.FindChild("Header/Tabs/ColorTab/ColorButton/Tab Background").GetComponent<SpriteRenderer>().enabled = true;
        IsCloset = true;
        PlayerCustomizationMenu.Instance.itemName.gameObject.SetActive(false);
        HatText.gameObject.SetActive(true);
        VisorText.gameObject.SetActive(true);
        SkinText.gameObject.SetActive(true);
        ColorText.gameObject.SetActive(true);
        NamePlateText.gameObject.SetActive(true);
        PetText.gameObject.SetActive(true);
        CubeText.gameObject.SetActive(true);
        ColorButton.gameObject.SetActive(true);
        HatButton.gameObject.SetActive(true);
        SkinButton.gameObject.SetActive(true);
        PetButton.gameObject.SetActive(true);
        VisorButton.gameObject.SetActive(true);
        NamePlateButton.gameObject.SetActive(true);
        CubeButton.gameObject.SetActive(true);
        HatButton_Hat.gameObject.SetActive(true);
        SkinButton_Skin.gameObject.SetActive(true);
        //PetButton_Pet.gameObject.SetActive(true);
        VisorButton_Visor.Visible = true;
        //NamePlateButton_NamePlate.gameObject.SetActive(true);
        UpdatePatch.area.gameObject.SetActive(true);
        PlayerCustomizationMenu.Instance.transform.FindChild("ColorGroup").gameObject.SetActive(true);
    }
    public static void PresetHide()
    {
        foreach (Transform tfm in Presets)
        {
            tfm.gameObject.SetActive(false);
        }
    }
    public static void PresetShow()
    {
        ResetShow();
        PlayerCustomizationMenu.Instance.transform.FindChild("Header/Tabs/HatsTab/Hat Button/Tab Background").GetComponent<SpriteRenderer>().enabled = true;
        //PlayerCustomizationMenu.Instance.itemName.text = "プリセット" + (SelectedPreset.Value + 1);
        Logger.Info("PresetShow!", "");
        if (Presets.Length > 0)
        {
            Logger.Info("0以上", "");
            foreach (Transform trf in Presets)
            {
                if (trf != null)
                {
                    GameObject.Destroy(trf.gameObject);
                }
            }
            foreach (PoolablePlayer pl in PresetAreas)
            {
                if (pl != null)
                {
                    GameObject.Destroy(pl.gameObject);
                }
            }
        }
        List<Transform> presets = new();
        List<PoolablePlayer> presetplayers = new();
        for (float i = 0; i < 10; i++)
        {
            var obj = GameObject.Instantiate(ColorButton, PlayerCustomizationMenu.Instance.transform.FindChild("ColorGroup"));
            Set(obj.Button, (int)i);
            obj.Button.OnMouseOver = new();
            obj.Button.OnMouseOver.AddListener((UnityEngine.Events.UnityAction)(() => obj.GetComponent<SpriteRenderer>().color = Color.yellow));
            obj.Button.OnMouseOut = new();
            obj.Button.OnMouseOut.AddListener((UnityEngine.Events.UnityAction)(() => obj.GetComponent<SpriteRenderer>().color = Color.white));
            obj.GetComponent<SpriteRenderer>().color = Color.white;
            obj.transform.localScale = new Vector3(4, 6, 1);
            GameObject.Destroy(obj.GetComponent<BoxCollider2D>());
            obj.Button.Colliders = new List<Collider2D>() { obj.gameObject.AddComponent<PolygonCollider2D>() }.ToArray();
            obj.transform.localPosition = i > 4 ? new Vector3(-1.2f + ((i - 5) * 1.7f), -0.75f, 4) : new Vector3(-1.2f + (i * 1.7f), 1.6f, 4);
            var player = GameObject.Instantiate(PlayerCustomizationMenu.Instance.PreviewArea, obj.transform);
            player.transform.localScale = new(0.2f, 0.135f, 0.25f);
            player.transform.localPosition = new();
            obj.gameObject.SetActive(true);
            presets.Add(obj.transform);
            presetplayers.Add(player);
        }
        Presets = presets.ToArray();
        PresetAreas = presetplayers.ToArray();
        PlayerCustomizationMenu.Instance.transform.FindChild("ColorGroup").gameObject.SetActive(true);
    }
    static void Set(PassiveButton btn, int index)
    {
        btn.OnClick.AddListener((UnityEngine.Events.UnityAction)(() => SetPreset(index)));
    }
    public static Dictionary<int, ClosetPresetData> ClosetPresetDataDictionary = new();
    public static ConfigEntry<int> SelectedPreset;
    public struct ClosetPresetData
    {
        public ConfigEntry<byte> BodyColor;
        public ConfigEntry<string> Hat;
        public ConfigEntry<string> Visor;
        public ConfigEntry<string> Skin;
        public ConfigEntry<string> NamePlate;
        public ConfigEntry<string> Pet;
    }
    public static void SetPreset(int index)
    {
        SelectedPreset.Value = index;
        SuperNewRolesPlugin.Logger.LogInfo("セットプリセット:" + index);
        ClosetPresetData data = !ClosetPresetDataDictionary.ContainsKey(index)
            ? (new()
            {
                BodyColor = SuperNewRolesPlugin.Instance.Config.Bind("ClosetPreset_" + index.ToString(), "BodyColor", (byte)0),
                Hat = SuperNewRolesPlugin.Instance.Config.Bind("ClosetPreset_" + index.ToString(), "Hat", ""),
                Visor = SuperNewRolesPlugin.Instance.Config.Bind("ClosetPreset_" + index.ToString(), "Visor", ""),
                Skin = SuperNewRolesPlugin.Instance.Config.Bind("ClosetPreset_" + index.ToString(), "Skin", ""),
                NamePlate = SuperNewRolesPlugin.Instance.Config.Bind("ClosetPreset_" + index.ToString(), "NamePlate", ""),
                Pet = SuperNewRolesPlugin.Instance.Config.Bind("ClosetPreset_" + index.ToString(), "Pet", "")
            })
            : ClosetPresetDataDictionary[index];

        AmongUs.Data.DataManager.Player.Customization.Color = data.BodyColor.Value;
        AmongUs.Data.DataManager.Player.Customization.Hat = data.Hat.Value;
        AmongUs.Data.DataManager.Player.Customization.Visor = data.Visor.Value;
        AmongUs.Data.DataManager.Player.Customization.Skin = data.Skin.Value;
        AmongUs.Data.DataManager.Player.Customization.NamePlate = data.NamePlate.Value;
        AmongUs.Data.DataManager.Player.Customization.Pet = data.Pet.Value;

        if (AmongUsClient.Instance.GameState == AmongUsClient.GameStates.Joined)
        {
            PlayerControl.LocalPlayer.CmdCheckColor(DataManager.Player.Customization.Color);
            PlayerControl.LocalPlayer.RpcSetHat(DataManager.Player.Customization.Hat);
            PlayerControl.LocalPlayer.RpcSetVisor(DataManager.Player.Customization.Visor);
            PlayerControl.LocalPlayer.RpcSetSkin(DataManager.Player.Customization.Skin);
            PlayerControl.LocalPlayer.RpcSetNamePlate(DataManager.Player.Customization.NamePlate);
            PlayerControl.LocalPlayer.RpcSetPet(DataManager.Player.Customization.Pet);
        }
        PlayerCustomizationMenu.Instance.PreviewArea.UpdateFromDataManager(PlayerMaterial.MaskType.ComplexUI);
    }
}