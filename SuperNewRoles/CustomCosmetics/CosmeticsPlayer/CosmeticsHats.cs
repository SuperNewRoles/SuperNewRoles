using HarmonyLib;
using SuperNewRoles.CustomCosmetics.UI;
using UnityEngine;

namespace SuperNewRoles.CustomCosmetics.CosmeticsPlayer;

public static class CosmeticsHats
{
    public static void SetHat(string hatId)
    {
        PlayerControl.LocalPlayer.RpcSetHat(hatId);
    }
}
[HarmonyPatch(typeof(PoolablePlayer), nameof(PoolablePlayer.Awake))]
public static class PoolablePlayer_SetHat
{
    public static void Postfix(PoolablePlayer __instance, int id)
    {
        __instance.RpcSetHat(id);
    }
}
public class CustomHatLayer : MonoBehaviour
{
    public int layer;
    public SpriteRenderer BackLayer;

    public SpriteRenderer FrontLayer;

    public SpriteRenderer Parent;

    private PlayerMaterial.Properties matProperties;

    private HatOptions options;

    private bool shouldFaceLeft;

    private const float ClimbZOffset = -0.02f;

    public ICustomCosmeticHat CustomCosmeticHat { get; set; }
    public CosmeticDataWrapperHat Hat => CustomCosmeticHat as CosmeticDataWrapperHat;

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

    private void OnDestroy()
    {
        UnloadAsset();
    }

    public bool HasHat()
    {
        if (Hat != null)
        {
            return Hat.ProdId != HatData.EmptyId;
        }
        return false;
    }

    public void SetHat(string hatId, int color)
    {
        if (DestroyableSingleton<HatManager>.InstanceExists)
        {
            ICosmeticData hat;
            if (hatId.StartsWith("Modded_"))
                hat = CustomCosmeticsLoader.GetModdedHatData(hatId);
            else
                hat = new CosmeticDataWrapperHat(DestroyableSingleton<HatManager>.Instance.GetHatById(hatId));
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
        CustomCosmeticHat = hat as ICustomCosmeticHat;
        SetHat(color);
    }

    private void SetHat(int color)
    {
        if (Hat == null) return;
        SetMaterialColor(color);
        UnloadAsset();
        Hat.LoadAsync(() =>
        {
            PopulateFromViewData();
        });
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
        if (options.ShowForClimb)
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
        CustomCosmeticHat.SetDoUnload();
    }

    public void SetOptions(HatOptions b)
    {
        options = b;
        options.Initialized = true;
    }

    private void UpdateMaterial()
    {
        PlayerMaterial.MaskType maskType = matProperties.MaskType;
        if (Hat.Asset != null && Hat.PreviewCrewmateColor)
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
        if (Hat.Asset != null && Hat.PreviewCrewmateColor)
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
        /*
            SpriteAnimNodeSync spriteAnimNodeSync = SpriteSyncNode ?? GetComponent<SpriteAnimNodeSync>();
            if ((bool)spriteAnimNodeSync)
            {
                spriteAnimNodeSync.NodeId = (Hat.NoBounce ? 1 : 0);
            }*/
        if (Hat.Options.front != HatOptionType.None)
        {
            FrontLayer.enabled = true;
            FrontLayer.sprite = Hat.Asset;
        }
        if (Hat.Options.back != HatOptionType.None)
        {
            BackLayer.enabled = true;
            BackLayer.sprite = Hat.Asset;
        }
        if (options.Initialized && HideHat())
        {
            FrontLayer.enabled = false;
            BackLayer.enabled = false;
        }
    }

    public void UpdateBounceHatZipline()
    {
        /*SpriteAnimNodeSync spriteAnimNodeSync = SpriteSyncNode ?? GetComponent<SpriteAnimNodeSync>();
        if ((bool)spriteAnimNodeSync)
        {
            spriteAnimNodeSync.NodeId = 1;
        }*/
    }

    public bool HideHat()
    {
        if (!options.VisorBlockingHatsAllowed)
        {
            return CustomCosmeticHat.BlocksVisors;
        }
        return false;
    }

    private void LateUpdate()
    {
        if (!Parent || !HasHat() || Hat.Asset == null)
        {
            return;
        }
        if (FrontLayer.sprite != CustomCosmeticHat.Climb && FrontLayer.sprite != CustomCosmeticHat.FloorImage)
        {
            if ((Hat.InFront || (bool)hatViewData.BackImage) && (bool)hatViewData.LeftMainImage)
            {
                FrontLayer.sprite = ((Parent.flipX || shouldFaceLeft) ? hatViewData.LeftMainImage : hatViewData.MainImage);
            }
            if ((bool)hatViewData.BackImage && (bool)hatViewData.LeftBackImage)
            {
                BackLayer.sprite = ((Parent.flipX || shouldFaceLeft) ? hatViewData.LeftBackImage : hatViewData.BackImage);
            }
            else if (!hatViewData.BackImage && !Hat.InFront && (bool)hatViewData.LeftMainImage)
            {
                BackLayer.sprite = ((Parent.flipX || shouldFaceLeft) ? hatViewData.LeftMainImage : hatViewData.MainImage);
            }
        }
        /*
        else if (FrontLayer.sprite == hatViewData.ClimbImage || FrontLayer.sprite == hatViewData.LeftClimbImage)
        {
            SpriteAnimNodeSync spriteAnimNodeSync = SpriteSyncNode ?? GetComponent<SpriteAnimNodeSync>();
            if ((bool)spriteAnimNodeSync)
            {
                spriteAnimNodeSync.NodeId = 0;
            }
        }*/
    }
}