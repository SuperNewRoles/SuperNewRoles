using System.Collections.Generic;
using System.Linq;
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

    public SpriteAnimNodeSync nodeSync;

    private PlayerMaterial.Properties matProperties;

    public float ZIndexSpacing { get; set; } = 0.0001f;


    private float LocalZFrontLayer => ZIndexSpacing * -3f;

    private float LocalZBackLayer => ZIndexSpacing * -1.5f;

    public List<SpriteAnimNodeSync> vanillaNodeSyncs = new();

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

    public Dictionary<PlayerOutfitType, ICosmeticData> Visors = new();
    public PlayerOutfitType CurrentVisorType;

    public ICustomCosmeticVisor CustomCosmeticVisor => Visors.TryGetValue(CurrentVisorType, out var visor) ? visor as ICustomCosmeticVisor : null;
    public ICosmeticData Visor => CustomCosmeticVisor == null ? null : CustomCosmeticVisor as ICosmeticData;

    public ICosmeticData DefaultVisor => Visors.TryGetValue(PlayerOutfitType.Default, out var visor) ? visor as ICosmeticData : null;

    public void SetVisor(string visorId, int colorId)
    {
        if (DestroyableSingleton<HatManager>.InstanceExists)
        {
            ICosmeticData visor;
            if (visorId.StartsWith(CustomCosmeticsLoader.ModdedPrefix))
                visor = CustomCosmeticsLoader.GetModdedVisorData(visorId);
            else
                visor = new CosmeticDataWrapperVisor(FastDestroyableSingleton<HatManager>.Instance.GetVisorById(visorId));
            if (visor == null)
                visor = new CosmeticDataWrapperVisor(FastDestroyableSingleton<HatManager>.Instance.GetVisorById(HatData.EmptyId));
            SetVisor(visor, colorId);
        }
    }

    public void SetShapeshiftVisor(string visorId, int colorId)
    {
        if (DestroyableSingleton<HatManager>.InstanceExists)
        {
            ICosmeticData visor;
            if (visorId.StartsWith(CustomCosmeticsLoader.ModdedPrefix))
                visor = CustomCosmeticsLoader.GetModdedVisorData(visorId);
            else
                visor = new CosmeticDataWrapperVisor(FastDestroyableSingleton<HatManager>.Instance.GetVisorById(visorId));
            Visors[PlayerOutfitType.Shapeshifted] = visor;
            CurrentVisorType = PlayerOutfitType.Shapeshifted;
            SetMaterialColor(colorId);
            // UnloadAsset();
            Visor.LoadAsync(() =>
            {
                PopulateFromViewData();
            });
        }
    }

    public void FinishShapeshift(int colorId)
    {
        CurrentVisorType = PlayerOutfitType.Default;
        SetMaterialColor(colorId);
        // UnloadAsset();
        Visor.LoadAsync(() =>
        {
            PopulateFromViewData();
        });
    }

    public void SetVisor(ICosmeticData data, int color)
    {
        Logger.Info($"SetVisor: {data.ProdId}");
        if (data == null || data != Visor)
        {
            Image.sprite = null;
        }
        Visors[CurrentVisorType] = data;
        SetMaterialColor(color);
        // UnloadAsset();
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

    private int count = 0;

    public void Update()
    {
        count--;
        if (count <= 0 && nodeSync != null)
        {
            count = 30;
            var parentsync = vanillaNodeSyncs.FirstOrDefault(x => x.enabled);
            if (parentsync == null)
            {
                nodeSync.enabled = false;
            }
            else
            {
                nodeSync.Parent = parentsync.Parent;
                nodeSync.ParentRenderer = parentsync.ParentRenderer;
                nodeSync.Renderer = parentsync.Renderer;
                nodeSync.enabled = true;
            }
        }
    }

    private void UnloadAsset()
    {
        CustomCosmeticVisor?.SetDoUnload();
    }
}