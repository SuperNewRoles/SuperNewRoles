using System.Collections.Generic;
using Il2CppInterop.Runtime;
using SuperNewRoles.Modules;
using UnityEngine;

namespace SuperNewRoles.CustomCosmetics.CosmeticsPlayer;

public static class CustomCosmeticsLayers
{
    public static Dictionary<int, CustomCosmeticsLayer> layers = new();
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
}
public class CustomCosmeticsLayer
{
    public CosmeticsLayer cosmeticsLayer;
    public CustomHatLayer hat1;
    public CustomHatLayer hat2;

    public CustomCosmeticsLayer(CosmeticsLayer cosmeticsLayer)
    {
        this.cosmeticsLayer = cosmeticsLayer;
        hat1 = CreateHatLayer(cosmeticsLayer, "hat1", new Vector3(0f, 0f, -0.2f), new Vector3(0f, 0f, 0.2f));
        hat2 = CreateHatLayer(cosmeticsLayer, "hat2", new Vector3(0f, 0f, -0.1f), new Vector3(0f, 0f, 0.1f));
    }

    private CustomHatLayer CreateHatLayer(CosmeticsLayer baseLayer, string hatName, Vector3 frontOffset, Vector3 backOffset)
    {
        // 新しいCustomHatLayerの生成と共通設定の適用
        CustomHatLayer hatLayer = new GameObject(hatName, Il2CppType.Of<CustomHatLayer>()).GetComponent<CustomHatLayer>();
        hatLayer.transform.parent = baseLayer.hat.transform.parent;
        hatLayer.transform.localPosition = new Vector3(-0.04f, 0.575f, -0.2f);
        hatLayer.transform.localScale = Vector3.one;
        hatLayer.Parent = baseLayer.hat.Parent;

        // フロントレイヤーの作成と設定
        hatLayer.FrontLayer = new GameObject("front", Il2CppType.Of<SpriteRenderer>()).GetComponent<SpriteRenderer>();
        hatLayer.FrontLayer.transform.parent = hatLayer.transform;
        hatLayer.FrontLayer.transform.localPosition = frontOffset;
        hatLayer.FrontLayer.transform.localScale = Vector3.one;
        // バックレイヤーの作成と設定
        hatLayer.BackLayer = new GameObject("back", Il2CppType.Of<SpriteRenderer>()).GetComponent<SpriteRenderer>();
        hatLayer.BackLayer.transform.parent = hatLayer.transform;
        hatLayer.BackLayer.transform.localPosition = backOffset;
        hatLayer.BackLayer.transform.localScale = Vector3.one;

        return hatLayer;
    }
}
