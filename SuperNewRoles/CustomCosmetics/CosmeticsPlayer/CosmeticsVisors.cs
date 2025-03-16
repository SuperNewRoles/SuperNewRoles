using AmongUs.Data;
using HarmonyLib;
using Innersloth.Assets;
using PowerTools;
using SuperNewRoles.CustomCosmetics.UI;
using SuperNewRoles.Modules;
using UnityEngine;
using static CosmeticsLayer;

namespace SuperNewRoles.CustomCosmetics.CosmeticsPlayer;

public class CustomVisorLayer : MonoBehaviour
{
    public CosmeticsLayer CosmeticLayer;
    //

    private const float BackZLayer = -1.5f;

    private const float FrontZLayer = -3f;

    private const float ClimbZOffset = -0.01f;

    public SpriteRenderer Image;

    private PlayerMaterial.Properties matProperties;

    public float ZIndexSpacing { get; set; } = 0.0001f;


    private float LocalZFrontLayer => ZIndexSpacing * -3f;

    private float LocalZBackLayer => ZIndexSpacing * -1.5f;

    // SetLocalZ拡張メソッドの代わりに使用するメソッド
    public void SetLocalZ(float zPosition)
    {
        Vector3 pos = transform.localPosition;
        pos.z = zPosition;
        transform.localPosition = pos;
        Logger.Info($"CustomVisorLayer.SetLocalZ: {transform.localPosition}");
    }

    public bool Visible
    {
        set
        {
            Image.enabled = value;
        }
    }

    public float Alpha
    {
        set
        {
            Image.color = Image.color.SetAlpha(value);
        }
    }

    public ICustomCosmeticVisor CustomCosmeticVisor { get; set; }
    public ICosmeticData Visor => CustomCosmeticVisor as ICosmeticData;

    public void SetVisor(string visorId, int colorId)
    {
        if (DestroyableSingleton<HatManager>.InstanceExists)
        {
            ICosmeticData visor;
            if (visorId.StartsWith("Modded_"))
                visor = CustomCosmeticsLoader.GetModdedVisorData(visorId);
            else
                visor = new CosmeticDataWrapperVisor(FastDestroyableSingleton<HatManager>.Instance.GetVisorById(visorId));
            SetVisor(visor, colorId);
        }
    }

    public void SetVisor(ICosmeticData data, int color)
    {
        if (data == null || data != Visor)
        {
            Image.sprite = null;
        }
        CustomCosmeticVisor = data as ICustomCosmeticVisor;
        SetMaterialColor(color);
        UnloadAsset();
        Visor.LoadAsync(() =>
        {
            PopulateFromViewData();
        });
    }

    private void PopulateFromViewData()
    {
        UpdateMaterial();
        if (Visor != null && (bool)Visor.Asset && !this.IsDestroyedOrNull() && !base.gameObject.IsDestroyedOrNull())
        {
            SetFlipX(Image.flipX);
        }
    }

    public void SetFlipX(bool flipX)
    {
        Image.flipX = flipX;
        if (Visor != null)
        {
            if (flipX && (bool)CustomCosmeticVisor.IdleLeft)
            {
                Image.sprite = CustomCosmeticVisor.IdleLeft;
            }
            else
            {
                Image.sprite = CustomCosmeticVisor.Idle;
            }
        }
    }

    public void SetLocalPlayer(bool localPlayer)
    {
        matProperties.IsLocalPlayer = localPlayer;
        UpdateMaterial();
    }

    public void SetIdleAnim(int colorId)
    {
        if (CustomCosmeticVisor != null)
        {
            SetVisor(Visor, colorId);
        }
    }

    public void SetClimbAnim(PlayerBodyTypes bodyType)
    {
        /*if (!options.HideDuringClimb && bodyType != PlayerBodyTypes.Horse)
        {
            base.transform.localPosition = new Vector3(base.transform.localPosition.x, base.transform.localPosition.y, -0.01f);
            Image.sprite = CustomCosmeticVisor.Climb;
        }*/
        if (CustomCosmeticVisor != null)
        {
            Image.sprite = CustomCosmeticVisor.Climb;
        }
    }

    public void SetMaskType(PlayerMaterial.MaskType maskType)
    {
        matProperties.MaskType = maskType;
        UpdateMaterial();
    }

    public void SetMaterialColor(int color)
    {
        matProperties.ColorId = color;
        UpdateMaterial();
    }

    public void SetMaskLayer(int layer)
    {
        matProperties.MaskLayer = layer;
        UpdateMaterial();
    }

    private void UpdateMaterial()
    {
        PlayerMaterial.MaskType maskType = matProperties.MaskType;
        if (Visor != null && Visor.PreviewCrewmateColor)
        {
            if (maskType == PlayerMaterial.MaskType.ComplexUI || maskType == PlayerMaterial.MaskType.ScrollingUI)
            {
                Image.sharedMaterial = DestroyableSingleton<HatManager>.Instance.MaskedPlayerMaterial;
            }
            else
            {
                Image.sharedMaterial = DestroyableSingleton<HatManager>.Instance.PlayerMaterial;
            }
        }
        else if (maskType == PlayerMaterial.MaskType.ComplexUI || maskType == PlayerMaterial.MaskType.ScrollingUI)
        {
            Image.sharedMaterial = DestroyableSingleton<HatManager>.Instance.MaskedMaterial;
        }
        else
        {
            Image.sharedMaterial = DestroyableSingleton<HatManager>.Instance.DefaultShader;
        }
        switch (maskType)
        {
            case PlayerMaterial.MaskType.SimpleUI:
                Image.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
                break;
            case PlayerMaterial.MaskType.Exile:
                Image.maskInteraction = SpriteMaskInteraction.VisibleOutsideMask;
                break;
            default:
                Image.maskInteraction = SpriteMaskInteraction.None;
                break;
        }
        Image.material.SetInt(PlayerMaterial.MaskLayer, matProperties.MaskLayer);
        if (Visor != null && Visor.PreviewCrewmateColor)
        {
            PlayerMaterial.SetColors(matProperties.ColorId, Image);
        }
        if (matProperties.MaskLayer <= 0)
        {
            PlayerMaterial.SetMaskLayerBasedOnLocalPlayer(Image, matProperties.IsLocalPlayer);
        }
    }

    public void SetVisorColor(Color color)
    {
        Image.color = color;
    }

    public void OnDestroy()
    {
        UnloadAsset();
    }

    private void UnloadAsset()
    {
        CustomCosmeticVisor?.SetDoUnload();
    }
    //
    /*
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
            Logger.Info($"SetHat: {hatId}, {color}");
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
            if (CustomCosmeticHat.Options.climb != HatOptionType.None)
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
            if (CustomCosmeticHat.Options.front != HatOptionType.None)
            {
                FrontLayer.enabled = true;
                FrontLayer.sprite = CustomCosmeticHat.Front;
            }
            if (CustomCosmeticHat.Options.back != HatOptionType.None)
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

        public bool HideHat()
        {
            return false;
        }

        public void LateUpdate()
        {
            if (!Parent || !HasHat() || Hat.Asset == null)
            {
                return;
            }
            FlipX = Parent.flipX;
            if (FrontLayer.sprite != CustomCosmeticHat.Climb)
            {
                if (CustomCosmeticHat.Options.front != HatOptionType.None && CustomCosmeticHat.Options.front_left != HatOptionType.None)
                {
                    FrontLayer.sprite = ((Parent.flipX || shouldFaceLeft) ? CustomCosmeticHat.FrontLeft : CustomCosmeticHat.Front);
                }
                if (CustomCosmeticHat.Options.back != HatOptionType.None && CustomCosmeticHat.Options.back_left != HatOptionType.None)
                {
                    BackLayer.sprite = ((Parent.flipX || shouldFaceLeft) ? CustomCosmeticHat.BackLeft : CustomCosmeticHat.Back);
                }
            }
            else if (FrontLayer.sprite == CustomCosmeticHat.Climb || FrontLayer.sprite == CustomCosmeticHat.ClimbLeft)
            {
                SpriteAnimNodeSync spriteAnimNodeSync = spriteSyncNode ?? GetComponent<SpriteAnimNodeSync>();
                if ((bool)spriteAnimNodeSync)
                {
                    spriteAnimNodeSync.NodeId = 0;
                }
            }
        }*/
}