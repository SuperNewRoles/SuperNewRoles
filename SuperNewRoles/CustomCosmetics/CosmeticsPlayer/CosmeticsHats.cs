using System;
using System.Collections.Generic;
using System.Linq;
using AmongUs.Data;
using HarmonyLib;
using PowerTools;
using SuperNewRoles.CustomCosmetics.UI;
using SuperNewRoles.Modules;
using UnityEngine;
using static CosmeticsLayer;

namespace SuperNewRoles.CustomCosmetics.CosmeticsPlayer;
public class CustomHatLayer : MonoBehaviour
{
    public int LayerNumber;
    public CosmeticsLayer CosmeticLayer;
    public SpriteRenderer BackLayer;

    public SpriteRenderer FrontLayer;

    public SpriteRenderer Parent;

    private PlayerMaterial.Properties matProperties;

    private bool shouldFaceLeft;

    private const float ClimbZOffset = -0.02f;
    public SpriteAnimNodeSync spriteSyncNode;

    public Dictionary<PlayerOutfitType, ICosmeticData> Hats = new();
    public PlayerOutfitType CurrentHatType;

    public ICustomCosmeticHat CustomCosmeticHat => Hats.TryGetValue(CurrentHatType, out var hat) ? hat as ICustomCosmeticHat : null;
    public ICosmeticData Hat => CustomCosmeticHat == null ? null : CustomCosmeticHat as ICosmeticData;

    public ICosmeticData DefaultHat => Hats.TryGetValue(PlayerOutfitType.Default, out var hat) ? hat as ICosmeticData : null;

    public bool Visible
    {
        set
        {
            FrontLayer.gameObject.SetActive(value);
            BackLayer.gameObject.SetActive(value);
        }
    }

    public Color SpriteColor
    {
        set
        {
            BackLayer.color = value;
            FrontLayer.color = value;
        }
    }

    public bool FlipX
    {
        set
        {
            BackLayer.flipX = value;
            FrontLayer.flipX = value;
        }
    }

    public bool HasHat()
    {
        if (Hat != null)
        {
            return Hat.ProdId != HatData.EmptyId;
        }
        return false;
    }

    public void SetShapeshiftHat(string hatId, int color)
    {
        if (DestroyableSingleton<HatManager>.InstanceExists)
        {
            ICosmeticData hat;
            if (hatId.StartsWith(CustomCosmeticsLoader.ModdedPrefix))
                hat = CustomCosmeticsLoader.GetModdedHatData(hatId);
            else
                hat = new CosmeticDataWrapperHat(FastDestroyableSingleton<HatManager>.Instance.GetHatById(hatId));
            Hats[PlayerOutfitType.Shapeshifted] = hat;
            CurrentHatType = PlayerOutfitType.Shapeshifted;
            SetHat(color);
        }
    }

    public void FinishShapeshift(int color)
    {
        CurrentHatType = PlayerOutfitType.Default;
        SetHat(color);
        PopulateFromViewData();
    }

    public void SetHat(string hatId, int color)
    {
        if (DestroyableSingleton<HatManager>.InstanceExists)
        {
            ICosmeticData hat;
            if (hatId.StartsWith(CustomCosmeticsLoader.ModdedPrefix))
                hat = CustomCosmeticsLoader.GetModdedHatData(hatId);
            else
                hat = new CosmeticDataWrapperHat(FastDestroyableSingleton<HatManager>.Instance.GetHatById(hatId));
            if (hat == null)
                hat = new CosmeticDataWrapperHat(FastDestroyableSingleton<HatManager>.Instance.GetHatById(HatData.EmptyId));
            SetHat(hat, color);
        }
    }

    public void SetHat(ICosmeticData hat, int color)
    {
        if (hat == null || hat != Hat)
        {
            BackLayer.sprite = null;
            FrontLayer.sprite = null;
        }
        Hats[CurrentHatType] = hat;
        Logger.Info($"SetHat: {hat.ProdId}");
        SetHat(color);
    }

    private void SetHat(int color)
    {
        if (Hat == null)
            return;

        var clayer = CustomCosmeticsLayers.ExistsOrInitialize(CosmeticLayer);

        // レイヤー番号に応じた設定を行う
        switch (LayerNumber)
        {
            case 1:
                clayer.HideBody.hat1 = CustomCosmeticHat?.Options?.HideBody ?? false;
                break;
            case 2:
                clayer.HideBody.hat2 = CustomCosmeticHat?.Options?.HideBody ?? false;
                break;
            default:
                throw new Exception("Invalid layer number");
        }

        // BodySpriteの表示状態を更新（どちらかが隠れる設定の場合は非表示）
        if (CosmeticLayer.currentBodySprite != null)
            CosmeticLayer.currentBodySprite.BodySprite.enabled = !(clayer.HideBody.hat1 || clayer.HideBody.hat2) && CosmeticLayer.Visible;

        SetMaterialColor(color);
        // UnloadAsset();
        Hat.LoadAsync(() => PopulateFromViewData());
    }

    public void SetIdleAnim(int colorId)
    {
        if (Hat != null)
        {
            SetHat(colorId);
            base.transform.SetLocalZ(0f);
        }
    }

    public void SetShouldFaceLeft(bool leftFacingVictim)
    {
        shouldFaceLeft = leftFacingVictim;
    }

    public void SetFloorAnim()
    {
        BackLayer.enabled = false;
        FrontLayer.enabled = true;
        FrontLayer.flipX = false;
        FrontLayer.sprite = Hat.Asset;
    }

    public void SetClimbAnim()
    {
        if (!CustomCosmeticHat.Options.climb.HasFlag(HatOptionType.None))
        {
            base.transform.localPosition = new Vector3(base.transform.localPosition.x, base.transform.localPosition.y, -0.02f);
            BackLayer.enabled = false;
            FrontLayer.enabled = true;
            FrontLayer.sprite = CustomCosmeticHat.Climb;
        }
    }

    public void SetLocalPlayer(bool localPlayer)
    {
        matProperties.IsLocalPlayer = localPlayer;
        UpdateMaterial();
    }

    public void SetMaterialColor(int color)
    {
        matProperties.ColorId = color;
        UpdateMaterial();
    }

    public void SetMaskType(PlayerMaterial.MaskType maskType)
    {
        matProperties.MaskType = maskType;
        UpdateMaterial();
    }

    public void SetMaskLayer(int layer)
    {
        matProperties.MaskLayer = layer;
        UpdateMaterial();
    }

    private void UnloadAsset()
    {
        CustomCosmeticHat?.SetDoUnload();
    }
    private void DontUnloadAsset()
    {
        CustomCosmeticHat?.SetDontUnload();
    }

    private void UpdateMaterial()
    {
        PlayerMaterial.MaskType maskType = matProperties.MaskType;
        if (Hat != null && Hat.PreviewCrewmateColor)
        {
            if (maskType == PlayerMaterial.MaskType.ComplexUI || maskType == PlayerMaterial.MaskType.ScrollingUI)
            {
                BackLayer.sharedMaterial = DestroyableSingleton<HatManager>.Instance.MaskedPlayerMaterial;
                FrontLayer.sharedMaterial = DestroyableSingleton<HatManager>.Instance.MaskedPlayerMaterial;
            }
            else
            {
                BackLayer.sharedMaterial = DestroyableSingleton<HatManager>.Instance.PlayerMaterial;
                FrontLayer.sharedMaterial = DestroyableSingleton<HatManager>.Instance.PlayerMaterial;
            }
        }
        else if (maskType == PlayerMaterial.MaskType.ComplexUI || maskType == PlayerMaterial.MaskType.ScrollingUI)
        {
            BackLayer.sharedMaterial = DestroyableSingleton<HatManager>.Instance.MaskedMaterial;
            FrontLayer.sharedMaterial = DestroyableSingleton<HatManager>.Instance.MaskedMaterial;
        }
        else
        {
            BackLayer.sharedMaterial = DestroyableSingleton<HatManager>.Instance.DefaultShader;
            FrontLayer.sharedMaterial = DestroyableSingleton<HatManager>.Instance.DefaultShader;
        }
        switch (maskType)
        {
            case PlayerMaterial.MaskType.SimpleUI:
                BackLayer.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
                FrontLayer.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
                break;
            case PlayerMaterial.MaskType.Exile:
                BackLayer.maskInteraction = SpriteMaskInteraction.VisibleOutsideMask;
                FrontLayer.maskInteraction = SpriteMaskInteraction.VisibleOutsideMask;
                break;
            default:
                BackLayer.maskInteraction = SpriteMaskInteraction.None;
                FrontLayer.maskInteraction = SpriteMaskInteraction.None;
                break;
        }
        BackLayer.material.SetInt(PlayerMaterial.MaskLayer, matProperties.MaskLayer);
        FrontLayer.material.SetInt(PlayerMaterial.MaskLayer, matProperties.MaskLayer);
        if (Hat != null && Hat.Asset != null && Hat.PreviewCrewmateColor)
        {
            PlayerMaterial.SetColors(matProperties.ColorId, BackLayer);
            PlayerMaterial.SetColors(matProperties.ColorId, FrontLayer);
        }
        if (matProperties.MaskLayer <= 0)
        {
            PlayerMaterial.SetMaskLayerBasedOnLocalPlayer(BackLayer, matProperties.IsLocalPlayer);
            PlayerMaterial.SetMaskLayerBasedOnLocalPlayer(FrontLayer, matProperties.IsLocalPlayer);
        }
    }

    private void PopulateFromViewData()
    {
        UpdateMaterial();
        if (Hat.Asset == null) return;

        SpriteAnimNodeSync spriteAnimNodeSync = spriteSyncNode ?? GetComponent<SpriteAnimNodeSync>();
        if ((bool)spriteAnimNodeSync)
        {
            spriteAnimNodeSync.NodeId = !CustomCosmeticHat.Options.front.HasFlag(HatOptionType.Bounce) ? 1 : 0;
        }
        if (!CustomCosmeticHat.Options.front.HasFlag(HatOptionType.None))
        {
            if (!CustomCosmeticHat.Options.behind || CustomCosmeticHat.Back != null)
            {
                FrontLayer.enabled = true;
                FrontLayer.sprite = CustomCosmeticHat.Front;
            }
            else
            {
                FrontLayer.enabled = false;
                BackLayer.enabled = true;
                BackLayer.sprite = CustomCosmeticHat.Front;
            }
        }
        if (!CustomCosmeticHat.Options.back.HasFlag(HatOptionType.None) && CustomCosmeticHat.Back != null)
        {
            BackLayer.enabled = true;
            BackLayer.sprite = CustomCosmeticHat.Back;
        }
        if (HideHat())
        {
            FrontLayer.enabled = false;
            BackLayer.enabled = false;
        }
    }

    public void UpdateBounceHatZipline()
    {
        SpriteAnimNodeSync spriteAnimNodeSync = spriteSyncNode ?? GetComponent<SpriteAnimNodeSync>();
        if ((bool)spriteAnimNodeSync)
        {
            spriteAnimNodeSync.NodeId = 1;
        }
    }

    public List<SpriteAnimNodeSync> vanillaNodeSyncs = new();

    public bool HideHat()
    {
        return false;
    }

    private int count = 0;

    public void LateUpdate()
    {
        if (Parent == null || !HasHat() || Hat.Asset == null)
        {
            return;
        }

        count--;
        if (count <= 0 && spriteSyncNode != null)
        {
            count = 30;
            var parentsync = vanillaNodeSyncs.FirstOrDefault(x => x.enabled);
            if (parentsync != null)
            {
                spriteSyncNode.Parent = parentsync.Parent;
                spriteSyncNode.ParentRenderer = parentsync.ParentRenderer;
                spriteSyncNode.Renderer = parentsync.Renderer;
                spriteSyncNode.enabled = true;
            }
            else
            {
                Logger.Error("parentsync is null");
                spriteSyncNode.enabled = false;
            }
        }

        Parent = CosmeticLayer.hat.Parent;

        var clayer = CustomCosmeticsLayers.ExistsOrInitialize(CosmeticLayer);
        if (clayer == null)
            Logger.Error("clayer is null");
        else
        {
            // BodySprite の表示状態を更新
            bool shouldHideBody = clayer.HideBody.hat1 || clayer.HideBody.hat2;
            if (CosmeticLayer.currentBodySprite != null)
                CosmeticLayer.currentBodySprite.BodySprite.enabled = !shouldHideBody && CosmeticLayer.Visible;
        }

        // 向き (左右反転) の更新
        FlipX = Parent.flipX;

        // Climb 状態でない場合、通常のハット表示を更新
        if (FrontLayer.sprite != CustomCosmeticHat.Climb)
        {
            if (!CustomCosmeticHat.Options.front.HasFlag(HatOptionType.None) &&
                !CustomCosmeticHat.Options.flip.HasFlag(HatOptionType.None))
            {
                FrontLayer.sprite = (Parent.flipX || shouldFaceLeft) ? CustomCosmeticHat.Flip : CustomCosmeticHat.Front;
            }
            if (!CustomCosmeticHat.Options.back.HasFlag(HatOptionType.None) &&
                !CustomCosmeticHat.Options.flip_back.HasFlag(HatOptionType.None))
            {
                BackLayer.sprite = (Parent.flipX || shouldFaceLeft) ? CustomCosmeticHat.FlipBack : CustomCosmeticHat.Back;
            }
        }
        // Climb 状態の場合は、spriteSyncNode の NodeId を更新
        else if (FrontLayer.sprite == CustomCosmeticHat.Climb || FrontLayer.sprite == CustomCosmeticHat.ClimbLeft)
        {
            var syncNode = spriteSyncNode ?? GetComponent<SpriteAnimNodeSync>();
            if (syncNode != null)
            {
                syncNode.NodeId = 0;
            }
        }
    }
}