using System;
using System.Collections.Generic;
using AmongUs.Data;
using SuperNewRoles.Modules;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace SuperNewRoles.CustomCosmetics.UI;

public class CustomCosmeticsPetMenu : CustomCosmeticsMenuBase<CustomCosmeticsPetMenu>
{
    public override CustomCosmeticsMenuType MenuType => CustomCosmeticsMenuType.pet;

    private List<Transform> slots = new();
    private List<GameObject> activeSlots = new();
    private Dictionary<string, List<GameObject>> slotToGroupMap = new();
    private GameObject CurrentCostumeTab;
    private Scroller scroller;

    public override void Initialize()
    {
        var unlockedPets = FastDestroyableSingleton<HatManager>.Instance.GetUnlockedPets();

        string currentCosmeticId = PlayerControl.LocalPlayer != null ? PlayerControl.LocalPlayer.Data.DefaultOutfit.PetId : DataManager.Player.Customization.Pet;
        PetData currentPet = FastDestroyableSingleton<HatManager>.Instance.GetPetById(currentCosmeticId);
        PlayerCustomizationMenu.Instance.SetItemName(currentPet.GetItemName());

        slots = [];
        activeSlots = [];
        slotToGroupMap = null;
        if (CurrentCostumeTab != null)
        {
            GameObject.Destroy(CurrentCostumeTab);
        }
        CurrentCostumeTab = GameObject.Instantiate(AssetManager.GetAsset<GameObject>("CosmeticMenuList"), PlayerCustomizationMenu.Instance.transform);
        CurrentCostumeTab.transform.localPosition = new(0, -0.085f, -10);
        CurrentCostumeTab.transform.localScale = Vector3.one * 0.27f;
        CurrentCostumeTab.transform.Find("LeftArea/Scroller/Inner/CategoryText").GetComponent<TextMeshPro>().text = FastDestroyableSingleton<TranslationController>.Instance.GetString(StringNames.PetLabel);

        var slotBase = GameObject.Instantiate(AssetManager.GetAsset<GameObject>("CosmeticItemSlot"), CurrentCostumeTab.transform);
        var slotBasePassive = slotBase.AddComponent<PassiveButton>();
        slotBase.SetActive(false);

        CustomCosmeticsCostumeSlot costumeSlot = slotBase.AddComponent<CustomCosmeticsCostumeSlot>();
        PassiveButton selectedButton = null;

        int itemsPerRow = 7;
        int totalItems = unlockedPets.Count;
        scroller = CurrentCostumeTab.transform.Find("LeftArea/Scroller").GetComponent<Scroller>();
        Transform inner = scroller.transform.Find("Inner");

        int allI = itemsPerRow;
        const float offSetY = 2.7f;
        for (int i = 0; i < unlockedPets.Count; i++)
        {
            int index = i;
            string petId = unlockedPets[i].ProdId;

            // 各アイテムの行と列を計算
            int col = allI % itemsPerRow;
            int itemRow = allI / itemsPerRow;

            var slot = GameObject.Instantiate(costumeSlot, inner);
            slots.Add(slot.transform);
            slot.Awake();

            var cosmeticData = unlockedPets[i];

            if (cosmeticData.PreviewCrewmateColor)
            {
                slot.spriteRenderer.material = FastDestroyableSingleton<HatManager>.Instance.PlayerMaterial;
                PlayerMaterial.SetMaskLayerBasedOnLocalPlayer(slot.spriteRenderer, false);
                PlayerMaterial.SetColors(PlayerControl.LocalPlayer != null ? PlayerControl.LocalPlayer.Data.DefaultOutfit.ColorId : DataManager.Player.Customization.Color, slot.spriteRenderer);
            }

            slot.transform.localPosition = new(-15.78f + col * 2.63f, 2.63f - itemRow * 2.6f + offSetY, -10);
            slot.transform.localScale = Vector3.one * 0.8f;
            slot.button.Colliders = new Collider2D[] { slot.GetComponent<BoxCollider2D>() };
            slot.button.OnClick = new();
            slot.button.OnClick.AddListener((UnityAction)(() =>
            {
                Logger.Info("Slot clicked");
                if (selectedButton != null)
                {
                    selectedButton.SelectButton(false);
                    selectedButton.transform.Find("Selected").gameObject.SetActive(false);
                }
                slot.button.SelectButton(true);
                selectedButton = slot.button;
                selectedButton.transform.Find("Selected").gameObject.SetActive(true);
                DataManager.Player.Customization.Pet = petId;
                if (PlayerControl.LocalPlayer != null)
                {
                    PlayerControl.LocalPlayer.RpcSetPet(petId);
                }
            }));
            slot.button.OnMouseOver = new();
            slot.button.OnMouseOver.AddListener((UnityAction)(() =>
            {
                if (selectedButton != slot.button)
                    slot.transform.Find("Selected").gameObject.SetActive(true);
                PreviewCosmetic(cosmeticData);
            }));
            slot.button.OnMouseOut = new();
            slot.button.OnMouseOut.AddListener((UnityAction)(() =>
            {
                if (selectedButton != slot.button)
                    slot.transform.Find("Selected").gameObject.SetActive(false);
                PreviewCosmetic(FastDestroyableSingleton<HatManager>.Instance.GetPetById(PlayerControl.LocalPlayer != null ? PlayerControl.LocalPlayer.Data.DefaultOutfit.PetId : DataManager.Player.Customization.Pet));
            }));
            slot.gameObject.SetActive(true);

            ControllerManager.Instance.AddSelectableUiElement(slot.button);

            cosmeticData.SetPreview(slot.spriteRenderer, PlayerControl.LocalPlayer != null ? PlayerControl.LocalPlayer.Data.DefaultOutfit.ColorId : DataManager.Player.Customization.Color);
            if (cosmeticData.ProdId == currentCosmeticId)
            {
                slot.button.SelectButton(true);
                selectedButton = slot.button;
                selectedButton.transform.Find("Selected").gameObject.SetActive(true);
            }
            allI++;
        }

        if (selectedButton != null)
        {
            ControllerManager.Instance.SetCurrentSelected(selectedButton);
            selectedButton.ReceiveMouseOver();
        }

        float contentYBounds = 0;
        if (allI > 35)
        {
            int extraRows = Mathf.CeilToInt((allI - 35) / (float)itemsPerRow);
            contentYBounds = extraRows * 2.6f + 0.45f - offSetY;
        }
        scroller.ContentYBounds = new(0, contentYBounds);
    }

    private void PreviewCosmetic(PetData pet)
    {
        if (pet != null)
        {
            PlayerCustomizationMenu.Instance.SetItemName(pet.GetItemName());
            PlayerCustomizationMenu.Instance.PreviewArea.SetPetIdle(pet.ProdId, PlayerControl.LocalPlayer != null ? PlayerControl.LocalPlayer.Data.DefaultOutfit.ColorId : DataManager.Player.Customization.Color);
        }
    }


    public override void Update()
    {
    }

    public override void Hide()
    {
        GameObject.Destroy(CurrentCostumeTab);
    }
}
