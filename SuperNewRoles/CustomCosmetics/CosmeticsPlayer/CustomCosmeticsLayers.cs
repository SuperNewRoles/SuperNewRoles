using System.Collections.Generic;
using Il2CppInterop.Runtime;
using PowerTools;
using SuperNewRoles.Modules;
using UnityEngine;

namespace SuperNewRoles.CustomCosmetics.CosmeticsPlayer;

public static class CustomCosmeticsLayers
{
    public static Dictionary<int, CustomCosmeticsLayer> layers = new();
    public static Dictionary<int, CustomVisorLayer> visorLayer1s = new();
    public static Dictionary<int, CustomVisorLayer> visorLayer2s = new();
    public static bool Exists(CosmeticsLayer cosmeticsLayer)
    {
        return layers.ContainsKey(cosmeticsLayer.GetInstanceID());
    }
    public static CustomCosmeticsLayer ExistsOrInitialize(CosmeticsLayer cosmeticsLayer)
    {
        if (!Exists(cosmeticsLayer))
            Initialize(cosmeticsLayer);
        return layers[cosmeticsLayer.GetInstanceID()];
    }
    public static void Initialize(CosmeticsLayer cosmeticsLayer)
    {
        layers[cosmeticsLayer.GetInstanceID()] = new CustomCosmeticsLayer(cosmeticsLayer);
    }
    public static (CustomVisorLayer layer1, CustomVisorLayer layer2) GetVisorLayers(VisorLayer visorLayer)
    {
        return (visorLayer1s.TryGetValue(visorLayer.GetInstanceID(), out var layer1) ? layer1 : null, visorLayer2s.TryGetValue(visorLayer.GetInstanceID(), out var layer2) ? layer2 : null);
    }
}
public class CustomCosmeticsLayer
{
    public CosmeticsLayer cosmeticsLayer;
    public CustomHatLayer hat1;
    public CustomHatLayer hat2;
    public CustomVisorLayer visor1;
    public CustomVisorLayer visor2;

    public CustomCosmeticsLayer(CosmeticsLayer cosmeticsLayer)
    {
        this.cosmeticsLayer = cosmeticsLayer;
        visor1 = CreateVisorLayer(cosmeticsLayer, "visor1", -0.8f);
        hat1 = CreateHatLayer(cosmeticsLayer, "hat1", new Vector3(0f, 0f, -0.2f), new Vector3(0f, 0f, 0.7f));
        hat2 = CreateHatLayer(cosmeticsLayer, "hat2", new Vector3(0f, 0f, -0.1f), new Vector3(0f, 0f, 0.6f));
        visor2 = CreateVisorLayer(cosmeticsLayer, "visor2", -0.51f);
        CustomCosmeticsLayers.visorLayer1s[cosmeticsLayer.visor.GetInstanceID()] = visor1;
        CustomCosmeticsLayers.visorLayer2s[cosmeticsLayer.visor.GetInstanceID()] = visor2;
    }
    private CustomVisorLayer CreateVisorLayer(CosmeticsLayer cosmeticsLayer, string visorName, float z)
    {
        CustomVisorLayer visorLayer = new GameObject(visorName, Il2CppType.Of<CustomVisorLayer>()).GetComponent<CustomVisorLayer>();
        visorLayer.CosmeticLayer = cosmeticsLayer;
        visorLayer.transform.parent = cosmeticsLayer.transform;
        visorLayer.transform.localScale = Vector3.one;
        visorLayer.transform.localRotation = Quaternion.identity;
        visorLayer.gameObject.layer = cosmeticsLayer.gameObject.layer;
        visorLayer.Image = visorLayer.gameObject.AddComponent<SpriteRenderer>();

        // 位置を設定する前に親子関係を確立
        visorLayer.transform.localPosition = Vector3.zero;
        // 明示的にワールド座標をリセット
        visorLayer.transform.position = cosmeticsLayer.transform.position;
        // その後、ローカル座標を設定
        visorLayer.transform.localPosition = new Vector3(-0.04f, 0.575f, z);

        Logger.Info("visorLayer.transform.localPosition: " + visorLayer.transform.localPosition);
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
        }

        // if (cosmeticsLayer.visor)
        return visorLayer;
    }
    private CustomHatLayer CreateHatLayer(CosmeticsLayer baseLayer, string hatName, Vector3 frontOffset, Vector3 backOffset)
    {
        // 新しいCustomHatLayerの生成と共通設定の適用
        CustomHatLayer hatLayer = new GameObject(hatName, Il2CppType.Of<CustomHatLayer>()).GetComponent<CustomHatLayer>();
        hatLayer.transform.parent = baseLayer.hat.transform.parent;
        hatLayer.transform.localPosition = new Vector3(-0.04f, 0.575f, -0.5999f);
        hatLayer.transform.localScale = Vector3.one;
        hatLayer.Parent = baseLayer.hat.Parent;
        hatLayer.gameObject.layer = baseLayer.gameObject.layer;

        if (baseLayer.hat.SpriteSyncNode == null)
            baseLayer.hat.SpriteSyncNode = baseLayer.hat.GetComponent<SpriteAnimNodeSync>();
        if (baseLayer.hat.SpriteSyncNode != null)
        {
            SpriteAnimNodeSync nodeSync = hatLayer.gameObject.AddComponent<SpriteAnimNodeSync>();
            nodeSync.Parent = baseLayer.currentBodySprite.BodySprite.GetComponent<SpriteAnimNodes>();
            nodeSync.ParentRenderer = baseLayer.currentBodySprite.BodySprite;
            nodeSync.Renderer = baseLayer.currentBodySprite.BodySprite;
            nodeSync.NodeId = 1;
            cosmeticsLayer.transform.parent.GetComponentInChildren<PlayerAnimations>().group.NodeSyncs.Add(nodeSync);
        }
        else
            Logger.Info("NULLLLLLLLLLLLLLLLLLLLLL");

        // フロントレイヤーの作成と設定
        hatLayer.FrontLayer = new GameObject("front", Il2CppType.Of<SpriteRenderer>()).GetComponent<SpriteRenderer>();
        hatLayer.FrontLayer.transform.parent = hatLayer.transform;
        hatLayer.FrontLayer.transform.localPosition = frontOffset;
        hatLayer.FrontLayer.transform.localScale = Vector3.one;
        hatLayer.FrontLayer.gameObject.layer = baseLayer.gameObject.layer;
        hatLayer.FrontLayer.material = baseLayer.hat.FrontLayer.material;
        // バックレイヤーの作成と設定
        hatLayer.BackLayer = new GameObject("back", Il2CppType.Of<SpriteRenderer>()).GetComponent<SpriteRenderer>();
        hatLayer.BackLayer.transform.parent = hatLayer.transform;
        hatLayer.BackLayer.transform.localPosition = backOffset;
        hatLayer.BackLayer.transform.localScale = Vector3.one;
        hatLayer.BackLayer.gameObject.layer = baseLayer.gameObject.layer;
        hatLayer.BackLayer.material = baseLayer.hat.BackLayer.material;
        return hatLayer;
    }
}
