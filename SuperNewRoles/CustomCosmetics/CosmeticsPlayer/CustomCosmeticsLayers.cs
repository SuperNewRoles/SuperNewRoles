using System;
using System.Collections.Generic;
using System.Linq;
using Il2CppInterop.Runtime;
using PowerTools;
using SuperNewRoles.Modules;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

namespace SuperNewRoles.CustomCosmetics.CosmeticsPlayer;

public static class CustomCosmeticsLayers
{
    public static Dictionary<int, CustomCosmeticsLayer> layers = new();
    public static Dictionary<int, CustomVisorLayer> visorLayer1s = new();
    public static Dictionary<int, CustomVisorLayer> visorLayer2s = new();
    public static bool Exists(CosmeticsLayer cosmeticsLayer, out CustomCosmeticsLayer layer)
    {
        return layers.TryGetValue(cosmeticsLayer.GetInstanceID(), out layer) ? layers != null : false;
    }
    public static CustomCosmeticsLayer ExistsOrInitialize(CosmeticsLayer cosmeticsLayer)
    {
        if (!Exists(cosmeticsLayer, out var layer))
            layer = Initialize(cosmeticsLayer);
        return layer;
    }
    public static CustomCosmeticsLayer Initialize(CosmeticsLayer cosmeticsLayer)
    {
        if (cosmeticsLayer == null)
        {
            Logger.Error("Initialize failed cosmeticsLayer is null: " + cosmeticsLayer?.name ?? "NONAME");
            return null;
        }
        if (layers == null)
        {
            Logger.Error("Initialize failed layers is null: " + cosmeticsLayer?.name ?? "NONAME");
            return null;
        }
        if (cosmeticsLayer.hat == null || cosmeticsLayer.visor == null)
        {
            Logger.Error("Initialize failed hat or visor is null: " + cosmeticsLayer?.name ?? "NONAME");
            return null;
        }
        try
        {
            return layers[cosmeticsLayer.GetInstanceID()] = new CustomCosmeticsLayer(cosmeticsLayer);
        }
        catch (Exception e)
        {
            Logger.Error("Initialize failed: " + e.Message);
            return layers[cosmeticsLayer.GetInstanceID()] = null;
        }
    }
    public static (CustomVisorLayer layer1, CustomVisorLayer layer2) GetVisorLayers(VisorLayer visorLayer)
    {
        return (visorLayer1s.TryGetValue(visorLayer.GetInstanceID(), out var layer1) ? layer1 : null, visorLayer2s.TryGetValue(visorLayer.GetInstanceID(), out var layer2) ? layer2 : null);
    }
}
public class CustomCosmeticsLayer
{
    public CosmeticsLayer cosmeticsLayer;
    public GameObject ModdedCosmetics;
    public CustomHatLayer hat1;
    public CustomHatLayer hat2;
    public CustomVisorLayer visor1;
    public CustomVisorLayer visor2;
    public (bool hat1, bool hat2) HideBody = (false, false);

    public CustomCosmeticsLayer(CosmeticsLayer cosmeticsLayer)
    {
        cosmeticsLayer.transform.parent.gameObject.AddComponent<SortingGroup>();
        this.cosmeticsLayer = cosmeticsLayer;

        foreach (var bodySprite in cosmeticsLayer.bodySprites)
        {
            bodySprite.BodySprite.sortingOrder = 6;
        }
        foreach (var spriteRenderer in cosmeticsLayer.skin.GetComponentsInChildren<SpriteRenderer>())
        {
            spriteRenderer.sortingOrder = 7;
        }
        foreach (var textMeshPro in cosmeticsLayer.nameTextContainer.GetComponentsInChildren<TextMeshPro>())
        {
            textMeshPro.sortingOrder = 500;
        }

        ModdedCosmetics = new GameObject("ModdedCosmetics");
        ModdedCosmetics.transform.parent = cosmeticsLayer.transform;
        ModdedCosmetics.transform.localPosition = new(0, 0, -0.0001f);
        ModdedCosmetics.transform.localScale = Vector3.one;
        ModdedCosmetics.transform.localRotation = Quaternion.identity;
        ModdedCosmetics.layer = cosmeticsLayer.gameObject.layer;

        cosmeticsLayer.transform.localPosition = new(0, 0, -0.0001f);

        var hat = cosmeticsLayer.hat;
        visor1 = CreateVisorLayer(cosmeticsLayer, "visor1", -0.8f, 40);
        hat1 = CreateHatLayer(cosmeticsLayer, hat, "hat1", new Vector3(0f, 0f, 0), new Vector3(0f, 0f, 0.7f), 30, 4);
        hat1.LayerNumber = 1;
        visor2 = CreateVisorLayer(cosmeticsLayer, "visor2", -0.51f, 20);
        hat2 = CreateHatLayer(cosmeticsLayer, hat, "hat2", new Vector3(0f, 0f, 0), new Vector3(0f, 0f, 0.6f), 10, 5);
        hat2.LayerNumber = 2;

        CustomCosmeticsLayers.visorLayer1s[cosmeticsLayer.visor.GetInstanceID()] = visor1;
        CustomCosmeticsLayers.visorLayer2s[cosmeticsLayer.visor.GetInstanceID()] = visor2;
    }
    private CustomVisorLayer CreateVisorLayer(CosmeticsLayer cosmeticsLayer, string visorName, float z, int sortingOrder)
    {
        CustomVisorLayer visorLayer = new GameObject(visorName).AddComponent<CustomVisorLayer>();
        visorLayer.CosmeticLayer = cosmeticsLayer;
        visorLayer.transform.parent = ModdedCosmetics.transform;
        visorLayer.transform.localScale = Vector3.one;
        visorLayer.transform.localRotation = Quaternion.identity;
        visorLayer.gameObject.layer = cosmeticsLayer.gameObject.layer;
        visorLayer.Image = visorLayer.gameObject.AddComponent<SpriteRenderer>();
        visorLayer.Image.sortingOrder = sortingOrder;

        // 位置を設定する前に親子関係を確立
        visorLayer.transform.localPosition = Vector3.zero;
        // 明示的にワールド座標をリセット
        visorLayer.transform.position = cosmeticsLayer.transform.position;
        // その後、ローカル座標を設定
        visorLayer.transform.localPosition = new Vector3(cosmeticsLayer.visor.transform.localPosition.x, cosmeticsLayer.visor.transform.localPosition.y, z);

        visorLayer.SetLocalZ(z);

        var nodes = cosmeticsLayer.currentBodySprite.BodySprite.GetComponent<SpriteAnimNodes>();
        var anims = cosmeticsLayer.transform.parent.GetComponentInChildren<PlayerAnimations>();
        if (nodes != null && anims != null)
        {
            SpriteAnimNodeSync nodeSync = visorLayer.gameObject.AddComponent<SpriteAnimNodeSync>();
            nodeSync.Parent = nodes;
            nodeSync.ParentRenderer = cosmeticsLayer.currentBodySprite.BodySprite;
            nodeSync.Renderer = cosmeticsLayer.currentBodySprite.BodySprite;
            nodeSync.NodeId = 1;
            anims.group.NodeSyncs.Add(nodeSync);
            visorLayer.nodeSync = nodeSync;
        }
        visorLayer.vanillaNodeSyncs = cosmeticsLayer.visor.GetComponents<SpriteAnimNodeSync>().ToList();
        // if (cosmeticsLayer.visor)
        return visorLayer;
    }
    private CustomHatLayer CreateHatLayer(CosmeticsLayer baseLayer, HatParent hatParent, string hatName, Vector3 frontOffset, Vector3 backOffset, int sortingOrder, int sortingOrderBack)
    {
        // 新しいCustomHatLayerの生成と共通設定の適用
        CustomHatLayer hatLayer = new GameObject(hatName).AddComponent<CustomHatLayer>();
        hatLayer.transform.parent = ModdedCosmetics.transform;
        hatLayer.transform.localPosition = new Vector3(hatParent.transform.localPosition.x, hatParent.transform.localPosition.y, -0.5999f);
        hatLayer.transform.localScale = Vector3.one;
        hatLayer.Parent = hatParent.Parent;
        hatLayer.gameObject.layer = baseLayer.gameObject.layer;
        hatLayer.CosmeticLayer = baseLayer;

        if (hatParent.SpriteSyncNode == null)
            hatParent.SpriteSyncNode = hatParent.GetComponent<SpriteAnimNodeSync>();
        if (hatParent.SpriteSyncNode != null)
        {
            SpriteAnimNodeSync nodeSync = hatLayer.gameObject.AddComponent<SpriteAnimNodeSync>();
            nodeSync.Parent = baseLayer.currentBodySprite.BodySprite.GetComponent<SpriteAnimNodes>();
            nodeSync.ParentRenderer = baseLayer.currentBodySprite.BodySprite;
            nodeSync.Renderer = baseLayer.currentBodySprite.BodySprite;
            nodeSync.NodeId = 1;
            // ここで cosmeticsLayer ではなく baseLayer を使用するのが正しい
            baseLayer.transform.parent.GetComponentInChildren<PlayerAnimations>()?.group?.NodeSyncs?.Add(nodeSync);
            hatLayer.spriteSyncNode = nodeSync;
        }
        else
            Logger.Info("NULLLLLLLLLLLLLLLLLLLLLL");
        hatLayer.vanillaNodeSyncs = cosmeticsLayer.hat.GetComponents<SpriteAnimNodeSync>().ToList();

        // バックレイヤーの作成と設定
        hatLayer.BackLayer = new GameObject("back").AddComponent<SpriteRenderer>();
        hatLayer.BackLayer.transform.parent = hatLayer.transform;
        hatLayer.BackLayer.transform.localPosition = backOffset;
        hatLayer.BackLayer.transform.localScale = Vector3.one;
        hatLayer.BackLayer.gameObject.layer = baseLayer.gameObject.layer;
        hatLayer.BackLayer.material = hatParent.BackLayer.material;
        hatLayer.BackLayer.sortingOrder = sortingOrderBack;
        // フロントレイヤーの作成と設定
        hatLayer.FrontLayer = new GameObject("front").AddComponent<SpriteRenderer>();
        hatLayer.FrontLayer.transform.parent = hatLayer.transform;
        hatLayer.FrontLayer.transform.localPosition = frontOffset;
        hatLayer.FrontLayer.transform.localScale = Vector3.one;
        hatLayer.FrontLayer.gameObject.layer = baseLayer.gameObject.layer;
        hatLayer.FrontLayer.material = hatParent.FrontLayer.material;
        hatLayer.FrontLayer.sortingOrder = sortingOrder;

        return hatLayer;
    }
}
