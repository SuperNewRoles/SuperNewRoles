using HarmonyLib;
using UnityEngine;

namespace SuperNewRoles.CustomCosmetics.CustomCosmeticsMenus.Patch
{
    [HarmonyPatch(typeof(PlayerCustomizationMenu), nameof(PlayerCustomizationMenu.Update))]
    class UpdatePatch
    {
        public static PlayerVoteArea area;
        public static void Postfix(PlayerCustomizationMenu __instance)
        {
            var ClosetTab = __instance.Tabs[0].Tab.transform;
            var PresetTab = __instance.Tabs[1].Tab.transform;
            ObjectData.CosmicubeMenuHolderTint.enabled = false;
            if (!ObjectData.IsShow)
            {
                if (GameObject.FindObjectOfType<PlayerTab>()?.ColorChips != null)
                {
                    foreach (ColorChip chip in GameObject.FindObjectOfType<PlayerTab>().ColorChips)
                    {
                        chip.gameObject.SetActive(false);
                    }
                }
            }

            __instance.equipButton.SetActive(false);
            __instance.equippedText.SetActive(false);
            if (ObjectData.IsCloset || ObjectData.IsShow)
            {
                if (ObjectData.IsCube)
                {
                    __instance.cubesTab.gameObject.SetActive(true);
                    PlayerCustomizationMenu.Instance.transform.FindChild("Background/RightPanel/CubeView").transform.localPosition = new Vector3(2.75f, -0.27f, 0);
                }
                if (area.gameObject.active) area.PreviewNameplate(SaveManager.LastNamePlate);

                var panel = __instance.transform.FindChild("Background/RightPanel");

                panel.FindChild("Gradient").gameObject.SetActive(false);

                panel.localPosition = new Vector3(0, 0, -4.29f);
                area.transform.localPosition = new Vector3(3.5f, 1.75f, -70.71f);
                var colortab = __instance.transform.FindChild("Header/Tabs/ColorTab");
                var closettab = __instance.transform.FindChild("Header/Tabs/HatsTab");

                if (!ObjectData.IsShow)
                {
                    ObjectData.ColorText.transform.localPosition = new Vector3(7, -1.25f, -55);
                    __instance.PreviewArea.transform.localPosition = new Vector3(0, -1f, -3);
                    //__instance.PreviewArea.transform.localScale = new Vector3(1, 1, 1);
                    area.gameObject.SetActive(true);
                    __instance.itemName.gameObject.SetActive(false);
                }
                else
                {
                    ObjectData.ColorText.transform.localPosition = new Vector3(0.4223f, 2.2f, -55f);
                    __instance.PreviewArea.transform.localPosition = new Vector3(3.5f, -0.5f, -3);
                    //__instance.PreviewArea.transform.localScale = new Vector3(-1, 1, 1);
                    if (!__instance.cubesTab.gameObject.active)
                    {
                        __instance.itemName.gameObject.SetActive(true);
                    }
                    __instance.itemName.transform.localPosition = new Vector3(3.5f, -1.74f, -5);
                }
                ObjectData.PetText.transform.localPosition = new Vector3(8f, -1.25f, -55);
                ObjectData.HatText.transform.localPosition = new Vector3(6.7f, 0.25f, -55);
                ObjectData.VisorText.transform.localPosition = new Vector3(2.5f, 0.75f, -55);
                ObjectData.SkinText.transform.localPosition = new Vector3(2.6f, -1f, -55);
                ObjectData.NamePlateText.transform.localPosition = new Vector3(3.8f, 1.6f, -55);
                ObjectData.CubeText.transform.localPosition = new Vector3(0.625f, -1f, -55);

                ObjectData.ColorButton.transform.localPosition = new Vector3(4.85f, -0.6f, -1);
                ObjectData.HatButton.transform.localPosition = new Vector3(4.9f, 1, -1);
                ObjectData.SkinButton.transform.localPosition = new Vector3(0.9f, -0.25f, -1);
                ObjectData.PetButton.transform.localPosition = new Vector3(6.25f, -0.5f, -55);
                ObjectData.CubeButton.transform.localPosition = new Vector3(-1.1f, 0, -1);

                ObjectData.VisorButton.transform.localPosition = new Vector3(0.9f, 1.5f, -1);
                ObjectData.NamePlateButton.transform.localPosition = new Vector3(3, 2.25f, -1);
                ObjectData.NamePlateButton.transform.localScale = new Vector3(8.498f, 2.145f, 2.145f);

                colortab.localPosition = new Vector3(-0.5f, 0, -5);
                closettab.localPosition = new Vector3(0.5f, 0, -5);

                if (!ObjectData.IsShow)
                {
                    ObjectData.ColorButton_SpriteRend.color = Palette.PlayerColors[SaveManager.BodyColor];
                    ObjectData.ColorButton.transform.localScale = new Vector3(1.56f, 1.56f, 1.56f);

                    ObjectData.HatButton_Hat.SetHat(SaveManager.lastHat, SaveManager.BodyColor);
                    ObjectData.HatButton_Hat.transform.localScale = new Vector3(0.75f, 0.75f, 0.75f);

                    ObjectData.SkinButton_Skin.SetSkin(SaveManager.lastSkin, SaveManager.BodyColor, false);
                    ObjectData.SkinButton_Skin.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);

                    ObjectData.VisorButton_Visor.SetVisor(FastDestroyableSingleton<HatManager>.Instance.GetVisorById(SaveManager.LastVisor));
                    ObjectData.VisorButton_Visor.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                }
            }
            else
            {
                __instance.PreviewArea.transform.localPosition = new Vector3(4.25f, 0f, -3f);
                __instance.itemName.gameObject.SetActive(true);
                __instance.itemName.transform.localPosition = new Vector3(4.25f, -1.2f, -5);
                int i = 0;
                __instance.itemName.text = "プリセット" + (ObjectData.SelectedPreset.Value + 1);
                try
                {
                    foreach (PoolablePlayer player in ObjectData.PresetAreas)
                    {
                        var outfit = new GameData.PlayerOutfit();
                        var data = SelectPatch.GetData(i);
                        outfit.ColorId = data.BodyColor.Value;
                        outfit.HatId = data.Hat.Value;
                        outfit.VisorId = data.Visor.Value;
                        outfit.SkinId = data.Skin.Value;
                        outfit.NamePlateId = data.NamePlate.Value;
                        outfit.PetId = data.Pet.Value;
                        Logger.Info(outfit.PetId, "");
                        player.UpdateFromPlayerOutfit(outfit, PlayerMaterial.MaskType.ComplexUI, false, data.Pet.Value == "");
                        player.transform.FindChild("PetSlot").gameObject.SetActive(data.Pet.Value != "");
                        i++;
                    }
                }
                catch
                {
                    Logger.Error("エラー", "");
                }
            }
        }
    }
}