using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

namespace SuperNewRoles.CustomCosmetics.CustomCosmeticsMenus.Patch
{
    public static class ObjectData
    {
        public static TextMeshPro HatText;
        public static TextMeshPro VisorText;
        public static TextMeshPro SkinText;
        public static TextMeshPro ColorText;
        public static TextMeshPro NamePlateText;
        public static TextMeshPro PetText;
        public static ColorChip ColorButton;
        public static ColorChip HatButton;
        public static ColorChip SkinButton;
        public static ColorChip PetButton;
        public static ColorChip VisorButton;
        public static ColorChip NamePlateButton;
        public static HatParent HatButton_Hat;
        public static SkinLayer SkinButton_Skin;
        public static PetBehaviour PetButton_Pet;
        public static VisorLayer VisorButton_Visor;
        public static NameplateChip NamePlateButton_NamePlate;
        public static SpriteRenderer ColorButton_SpriteRend;
        public static string Selected;
        public static Transform[] HatTabButtons;
        public static PassiveButton area_pas;
        public static bool IsShow;
        public static void HatShow()
        {
            ResetShow();
            IsShow = true;
            HideDefaultTabButton();
            PlayerCustomizationMenu.Instance.transform.FindChild("HatsGroup").gameObject.SetActive(true);
            ShowHatTabsButton();
        }
        public static void SkinShow()
        {
            ResetShow();
            IsShow = true;
            PlayerCustomizationMenu.Instance.transform.FindChild("SkinGroup").gameObject.SetActive(true);
        }
        public static void PetShow()
        {
            ResetShow();
            IsShow = true;
            PlayerCustomizationMenu.Instance.transform.FindChild("PetsGroup").gameObject.SetActive(true);
        }
        public static void VisorShow()
        {
            ResetShow();
            IsShow = true;
            PlayerCustomizationMenu.Instance.transform.FindChild("VisorGroup").gameObject.SetActive(true);
        }
        public static void NamePlateShow()
        {
            ResetShow();
            IsShow = true;
            PlayerCustomizationMenu.Instance.transform.FindChild("NameplateGroup").gameObject.SetActive(true);
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
        public static void HideHatTabsButton()
        {
            foreach (Transform obj in HatTabButtons)
            {
                obj.gameObject.SetActive(false);
            }
        }
        public static void ShowHatTabsButton()
        {
            SuperNewRolesPlugin.Logger.LogInfo(CustomHats.IsEnd);
            if (!CustomHats.IsEnd) {
                ShowDefaultTabButton();
                return;
            }
            SuperNewRolesPlugin.Logger.LogInfo(HatTabButtons.Length);
            if (HatTabButtons.Length > 0)
            {
                foreach (Transform obj in HatTabButtons)
                {
                    obj.gameObject.SetActive(true);
                }
                return;
            }
            Transform parent = PlayerCustomizationMenu.Instance.Tabs[0].Button.transform.parent.parent.parent;
            List<Transform> Tabs = new();
            SuperNewRolesPlugin.Logger.LogInfo(CustomHats.Keys.Count);
            int i = 1;
            foreach (string key in CustomHats.Keys)
            {
                var obj = GameObject.Instantiate(PlayerCustomizationMenu.Instance.Tabs[0].Button.transform.parent.parent, parent);
                obj.GetChild(0).gameObject.SetActive(true);
                PassiveButton button = obj.GetChild(0).FindChild("Tab Background").GetComponent<PassiveButton>();
                button.OnClick = new();
                button.OnClick.AddListener((UnityEngine.Events.UnityAction)(() => ClickHatTab(key)));
                obj.transform.localPosition = new Vector3(-3.75f + (i * 0.75f), 0, -5);
                obj.name = key;
                Tabs.Add(obj);
                i++;
                SuperNewRolesPlugin.Logger.LogInfo("追加:" + key);
            }
            HatTabButtons = Tabs.ToArray();
            ClickHatTab(CustomHats.Keys[0]);
        }
        public static void ClickHatTab(string package)
        {
            Selected = package;
            foreach (Transform obj in HatTabButtons)
            {
                if (obj.name == package)
                {
                    obj.GetChild(0).FindChild("Tab Background").GetComponent<SpriteRenderer>().enabled = true;
                }
                else
                {
                    obj.GetChild(0).FindChild("Tab Background").GetComponent<SpriteRenderer>().enabled = false;
                }
            }
            if (hats.Length <= 0)
            {
                hats = PlayerCustomizationMenu.Instance.transform.FindChild("HatsGroup").GetComponentsInChildren<HatParent>();
            }
            HatsTab hatstab = PlayerCustomizationMenu.Instance.transform.FindChild("HatsGroup").GetComponent<HatsTab>();
            foreach (var data in hatstab.ColorChips)
            {
                SuperNewRolesPlugin.Logger.LogInfo(data+"をDestroy");
                GameObject.Destroy(data);
            }
            hatstab.ColorChips = new();
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
            IsShow = false;
            ClosetHide();
            PresetHide();
            ShowDefaultTabButton();
            HideHatTabsButton();
            foreach (TabButton button in PlayerCustomizationMenu.Instance.Tabs)
            {
                GameObject btn = button.Tab.gameObject;
                if (btn.active)
                {
                    btn.SetActive(false);
                }
            }
            UpdatePatch.area.gameObject.SetActive(false);
        }
        public static void ClosetHide()
        {
            HatText.gameObject.SetActive(false);
            VisorText.gameObject.SetActive(false);
            SkinText.gameObject.SetActive(false);
            ColorText.gameObject.SetActive(false);
            NamePlateText.gameObject.SetActive(false);
            PetText.gameObject.SetActive(false);
            ColorButton.gameObject.SetActive(false);
            HatButton.gameObject.SetActive(false);
            SkinButton.gameObject.SetActive(false);
            PetButton.gameObject.SetActive(false);
            VisorButton.gameObject.SetActive(false);
            NamePlateButton.gameObject.SetActive(false);
            HatButton_Hat.gameObject.SetActive(false);
            SkinButton_Skin.gameObject.SetActive(false);
            //PetButton_Pet.gameObject.SetActive(false);
            VisorButton_Visor.Visible = false;
            //NamePlateButton_NamePlate.gameObject.SetActive(false);
        }
        public static void ClosetShow()
        {
            ResetShow();
            HatText.gameObject.SetActive(true);
            VisorText.gameObject.SetActive(true);
            SkinText.gameObject.SetActive(true);
            ColorText.gameObject.SetActive(true);
            NamePlateText.gameObject.SetActive(true);
            PetText.gameObject.SetActive(true);
            ColorButton.gameObject.SetActive(true);
            HatButton.gameObject.SetActive(true);
            SkinButton.gameObject.SetActive(true);
            PetButton.gameObject.SetActive(true);
            VisorButton.gameObject.SetActive(true);
            NamePlateButton.gameObject.SetActive(true);
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
        }
        public static void PresetShow()
        {
            ResetShow();
        }
    }
}
