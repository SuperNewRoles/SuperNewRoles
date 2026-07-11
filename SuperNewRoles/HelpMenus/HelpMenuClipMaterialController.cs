using System.Collections.Generic;
using SuperNewRoles.Modules;
using TMPro;
using UnityEngine;

namespace SuperNewRoles.HelpMenus;

/// <summary>
/// Replaces the SpriteMask / stencil based clipping used by HelpMenu scrollers with
/// materials that clip against a dedicated world-space rectangle.
/// </summary>
public static class HelpMenuClipMaterialController
{
    private const string SpriteShaderAssetName = "SNRWorldRectClipSprite";
    private const string TmpShaderAssetName = "SNRWorldRectClipTMP";
    private const string TmpMobileShaderAssetName = "SNRWorldRectClipTMP-Mobile";
    private const string ClipRectProperty = "_SNRClipRect";

    private sealed class MaterialBinding
    {
        public Material Material;
    }

    private static readonly Dictionary<int, MaterialBinding> MaterialBindings = new();
    private static Shader _spriteShader;
    private static Shader _tmpShader;
    private static Shader _tmpMobileShader;
    private static bool _shaderLoadFailed;

    public static void Refresh(GameObject helpMenuObject, bool force = false)
    {
        if (helpMenuObject == null)
            return;

        if (!EnsureShadersLoaded())
            return;

        // 毎フレーム最新のマスク・スクローラーを取得する。
        // カテゴリ切り替えで新しい SpriteMask が追加された際も確実に enabled = false を適用し、
        // 他のマスクとの干渉を防ぐ。
        SpriteMask[] masks = helpMenuObject.GetComponentsInChildren<SpriteMask>(false);
        Scroller[] scrollers = helpMenuObject.GetComponentsInChildren<Scroller>(false);
        foreach (Scroller scroller in scrollers)
        {
            if (scroller == null || !scroller.gameObject.activeInHierarchy)
                continue;

            SpriteMask mask = FindBestMask(scroller.transform, masks, helpMenuObject.transform);
            if (mask == null || mask.sprite == null)
                continue;

            Vector4 clipRect = CalculateWorldClipRect(mask);
            ApplyToScroller(scroller.transform, clipRect);
            mask.enabled = false;
        }
    }

    public static void Release()
    {
        foreach (MaterialBinding binding in MaterialBindings.Values)
        {
            if (binding?.Material != null)
                Object.Destroy(binding.Material);
        }

        MaterialBindings.Clear();
    }

    private static bool EnsureShadersLoaded()
    {
        if (_spriteShader != null && _tmpShader != null && _tmpMobileShader != null)
            return true;
        if (_shaderLoadFailed)
            return false;

        _spriteShader = AssetManager.GetAsset<Shader>(SpriteShaderAssetName);
        _tmpShader = AssetManager.GetAsset<Shader>(TmpShaderAssetName);
        _tmpMobileShader = AssetManager.GetAsset<Shader>(TmpMobileShaderAssetName);
        if (_spriteShader != null && _tmpShader != null && _tmpMobileShader != null)
            return true;

        _shaderLoadFailed = true;
        Logger.Error(
            $"HelpMenu clip shaders could not be loaded. sprite={_spriteShader != null}, tmp={_tmpShader != null}, tmpMobile={_tmpMobileShader != null}",
            "HelpMenuClipMaterialController");
        return false;
    }

    private static void ApplyToScroller(Transform scrollerTransform, Vector4 clipRect)
    {
        foreach (SpriteRenderer renderer in scrollerTransform.GetComponentsInChildren<SpriteRenderer>(true))
        {
            if (renderer == null)
                continue;

            Material material = GetOrCreateMaterial(
                renderer.GetInstanceID(),
                renderer.sharedMaterial,
                _spriteShader,
                clipRect,
                out bool replaced);
            if (material == null)
                continue;

            if (replaced)
                renderer.sharedMaterial = material;
            renderer.maskInteraction = SpriteMaskInteraction.None;
        }

        foreach (TextMeshPro text in scrollerTransform.GetComponentsInChildren<TextMeshPro>(true))
        {
            if (text == null || !IsSdfMaterial(text.fontSharedMaterial))
                continue;

            Material material = GetOrCreateMaterial(
                text.GetInstanceID(),
                text.fontSharedMaterial,
                GetTmpClipShader(text.fontSharedMaterial),
                clipRect,
                out bool replaced);
            if (material == null)
                continue;

            if (replaced)
            {
                text.fontSharedMaterial = material;
                text.UpdateMeshPadding();
            }
        }

        foreach (TMP_SubMesh subMesh in scrollerTransform.GetComponentsInChildren<TMP_SubMesh>(true))
        {
            if (subMesh == null || !IsSdfMaterial(subMesh.sharedMaterial))
                continue;

            Material material = GetOrCreateMaterial(
                subMesh.GetInstanceID(),
                subMesh.sharedMaterial,
                GetTmpClipShader(subMesh.sharedMaterial),
                clipRect,
                out bool replaced);
            if (material != null && replaced)
                subMesh.sharedMaterial = material;
        }
    }

    private static bool IsSdfMaterial(Material material)
        => material != null && material.HasProperty("_GradientScale");

    private static Shader GetTmpClipShader(Material sourceMaterial)
    {
        string sourceShaderName = sourceMaterial?.shader?.name;
        return sourceShaderName != null && sourceShaderName.Contains("Mobile")
            ? _tmpMobileShader
            : _tmpShader;
    }

    private static Material GetOrCreateMaterial(
        int rendererId,
        Material sourceMaterial,
        Shader targetShader,
        Vector4 clipRect,
        out bool replaced)
    {
        replaced = false;
        if (sourceMaterial == null || targetShader == null)
            return null;

        if (MaterialBindings.TryGetValue(rendererId, out MaterialBinding binding)
            && binding?.Material != null
            && sourceMaterial == binding.Material)
        {
            binding.Material.SetVector(ClipRectProperty, clipRect);
            return binding.Material;
        }

        if (binding?.Material != null)
            Object.Destroy(binding.Material);

        var material = new Material(sourceMaterial)
        {
            name = sourceMaterial.name + " (SNR Help Clip)",
            shader = targetShader,
            hideFlags = HideFlags.DontSave,
        };
        material.SetVector(ClipRectProperty, clipRect);

        MaterialBindings[rendererId] = new MaterialBinding { Material = material };
        replaced = true;
        return material;
    }

    private static SpriteMask FindBestMask(Transform scroller, SpriteMask[] masks, Transform root)
    {
        SpriteMask bestMask = null;
        int bestCommonDepth = -1;
        float bestDistance = float.PositiveInfinity;

        foreach (SpriteMask mask in masks)
        {
            if (mask == null || mask.sprite == null || !mask.gameObject.activeInHierarchy)
                continue;

            Transform commonAncestor = FindCommonAncestor(scroller, mask.transform, root);
            if (commonAncestor == null)
                continue;

            int commonDepth = GetDepthFromRoot(commonAncestor, root);
            float distance = (mask.transform.position - scroller.position).sqrMagnitude;
            if (commonDepth > bestCommonDepth || (commonDepth == bestCommonDepth && distance < bestDistance))
            {
                bestMask = mask;
                bestCommonDepth = commonDepth;
                bestDistance = distance;
            }
        }

        return bestMask;
    }

    private static Transform FindCommonAncestor(Transform first, Transform second, Transform root)
    {
        for (Transform current = first; current != null; current = current.parent)
        {
            if (second == current || second.IsChildOf(current))
                return current;
            if (current == root)
                break;
        }

        return null;
    }

    private static int GetDepthFromRoot(Transform transform, Transform root)
    {
        int depth = 0;
        for (Transform current = transform; current != null && current != root; current = current.parent)
            depth++;
        return depth;
    }

    private static Vector4 CalculateWorldClipRect(SpriteMask mask)
    {
        Bounds spriteBounds = mask.sprite.bounds;
        Vector3 min = spriteBounds.min;
        Vector3 max = spriteBounds.max;
        Vector3[] corners =
        {
            mask.transform.TransformPoint(new Vector3(min.x, min.y, 0f)),
            mask.transform.TransformPoint(new Vector3(min.x, max.y, 0f)),
            mask.transform.TransformPoint(new Vector3(max.x, min.y, 0f)),
            mask.transform.TransformPoint(new Vector3(max.x, max.y, 0f)),
        };

        float left = float.PositiveInfinity;
        float bottom = float.PositiveInfinity;
        float right = float.NegativeInfinity;
        float top = float.NegativeInfinity;
        foreach (Vector3 corner in corners)
        {
            left = Mathf.Min(left, corner.x);
            bottom = Mathf.Min(bottom, corner.y);
            right = Mathf.Max(right, corner.x);
            top = Mathf.Max(top, corner.y);
        }

        return new Vector4(left, bottom, right, top);
    }
}
