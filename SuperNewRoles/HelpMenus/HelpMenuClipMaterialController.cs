using System;
using System.Collections.Generic;
using HarmonyLib;
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
    private static GameObject _cachedRoot;
    private static SpriteMask[] _cachedMasks = System.Array.Empty<SpriteMask>();
    private static Scroller[] _cachedScrollers = System.Array.Empty<Scroller>();
    private static readonly Dictionary<int, ScrollerRendererCache> ScrollerCaches = new();
    private static int _cachedHierarchyCount = -1;
    private static bool _dirty = true;
    private static GameObject _activeHelpMenu;

    private sealed class ScrollerRendererCache
    {
        public SpriteRenderer[] SpriteRenderers = System.Array.Empty<SpriteRenderer>();
        public TextMeshPro[] Texts = System.Array.Empty<TextMeshPro>();
        public TMP_SubMesh[] SubMeshes = System.Array.Empty<TMP_SubMesh>();
        public SpriteMask Mask;
        public Vector4 LastClipRect;
        public bool HasLastClipRect;
        public int ChildCount = -1;
    }

    public static void MarkDirty()
    {
        _dirty = true;
        _cachedRoot = null;
        ScrollerCaches.Clear();
    }

    public static void Attach(GameObject helpMenuObject)
    {
        _activeHelpMenu = helpMenuObject;
        Refresh(helpMenuObject, retryShaderLoad: true);
    }

    public static void Detach(GameObject helpMenuObject)
    {
        if (_activeHelpMenu == helpMenuObject)
            _activeHelpMenu = null;
    }

    public static void Refresh(GameObject helpMenuObject, bool retryShaderLoad = false)
    {
        if (helpMenuObject == null)
            return;

        int hierarchyCount = helpMenuObject.transform.childCount;
        bool rebuild = _dirty || _cachedRoot != helpMenuObject || hierarchyCount != _cachedHierarchyCount;
        if (rebuild)
        {
            _cachedMasks = helpMenuObject.GetComponentsInChildren<SpriteMask>(true);
            _cachedScrollers = helpMenuObject.GetComponentsInChildren<Scroller>(false);
            _cachedRoot = helpMenuObject;
            _cachedHierarchyCount = hierarchyCount;
            ScrollerCaches.Clear();
            _dirty = false;
        }

        // SpriteMask は画面上のステンシルとしてプレイヤーの帽子・バイザーまで切る。
        // 階層走査は dirty 時だけ行い、毎フレームはキャッシュしたマスクを無効化する。
        DisableAllSpriteMasks(_cachedMasks);

        if (!EnsureShadersLoaded(retryShaderLoad))
            return;

        Scroller[] scrollers = _cachedScrollers;
        SpriteMask[] masks = _cachedMasks;
        foreach (Scroller scroller in scrollers)
        {
            if (scroller == null || !scroller.gameObject.activeInHierarchy)
                continue;

            ApplyToScroller(scroller.transform, masks, helpMenuObject.transform);
        }
    }

    internal static void HandlePreCull(Camera camera)
    {
        if (_activeHelpMenu == null || !ShouldUpdateClipForCamera(camera))
            return;

        Refresh(_activeHelpMenu);
    }

    internal static bool ShouldUpdateClipForCamera(Camera camera)
    {
        if (camera is null)
            return false;

        try
        {
            HudManager hud = HudManager.Instance;
            if (hud != null && hud.UICamera != null)
                return camera == hud.UICamera;
        }
        catch
        {
        }

        return camera == Camera.main;
    }

    internal static void DisableAllSpriteMasks(SpriteMask[] masks)
    {
        if (masks == null)
            return;

        foreach (SpriteMask mask in masks)
        {
            if (mask != null)
                mask.enabled = false;
        }
    }

    public static void Release()
    {
        _activeHelpMenu = null;
        foreach (MaterialBinding binding in MaterialBindings.Values)
        {
            if (binding?.Material != null)
                UnityEngine.Object.Destroy(binding.Material);
        }

        MaterialBindings.Clear();
        MarkDirty();
    }

    private static bool EnsureShadersLoaded(bool retryFailedLoad)
    {
        if (_spriteShader != null && _tmpShader != null && _tmpMobileShader != null)
            return true;
        if (_shaderLoadFailed && !retryFailedLoad)
            return false;

        _spriteShader = AssetManager.GetAsset<Shader>(SpriteShaderAssetName);
        _tmpShader = AssetManager.GetAsset<Shader>(TmpShaderAssetName);
        _tmpMobileShader = AssetManager.GetAsset<Shader>(TmpMobileShaderAssetName);
        if (_spriteShader != null && _tmpShader != null && _tmpMobileShader != null)
        {
            _shaderLoadFailed = false;
            return true;
        }

        _shaderLoadFailed = true;
        Logger.Error(
            $"HelpMenu clip shaders could not be loaded. sprite={_spriteShader != null}, tmp={_tmpShader != null}, tmpMobile={_tmpMobileShader != null}",
            "HelpMenuClipMaterialController");
        return false;
    }

    private static void ApplyToScroller(Transform scrollerTransform, SpriteMask[] masks, Transform root)
    {
        int id = scrollerTransform.GetInstanceID();
        int childCount = scrollerTransform.childCount;
        if (!ScrollerCaches.TryGetValue(id, out ScrollerRendererCache cache) || cache.ChildCount != childCount)
        {
            cache = new ScrollerRendererCache
            {
                SpriteRenderers = scrollerTransform.GetComponentsInChildren<SpriteRenderer>(true),
                Texts = scrollerTransform.GetComponentsInChildren<TextMeshPro>(true),
                SubMeshes = scrollerTransform.GetComponentsInChildren<TMP_SubMesh>(true),
                ChildCount = childCount,
            };
            ScrollerCaches[id] = cache;
        }

        if (cache.Mask == null)
            cache.Mask = FindBestMask(scrollerTransform, masks, root);
        if (cache.Mask == null || cache.Mask.sprite == null)
            return;

        Vector4 clipRect = CalculateWorldClipRect(cache.Mask);
        if (cache.HasLastClipRect && cache.LastClipRect == clipRect)
            return;

        cache.LastClipRect = clipRect;
        cache.HasLastClipRect = true;

        foreach (SpriteRenderer renderer in cache.SpriteRenderers)
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

        foreach (TextMeshPro text in cache.Texts)
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

        foreach (TMP_SubMesh subMesh in cache.SubMeshes)
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
            UnityEngine.Object.Destroy(binding.Material);

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

        var bounds = CalculateClipRectFromPoints(corners, static corner => (corner.x, corner.y));
        return new Vector4(bounds.Left, bounds.Bottom, bounds.Right, bounds.Top);
    }

    internal static (float Left, float Bottom, float Right, float Top) CalculateClipRectFromPoints<T>(
        IReadOnlyList<T> points,
        Func<T, (float x, float y)> selector)
    {
        if (points == null || points.Count == 0 || selector == null)
            return (0f, 0f, 0f, 0f);

        float left = float.PositiveInfinity;
        float bottom = float.PositiveInfinity;
        float right = float.NegativeInfinity;
        float top = float.NegativeInfinity;
        for (int i = 0; i < points.Count; i++)
        {
            (float x, float y) = selector(points[i]);
            if (x < left) left = x;
            if (y < bottom) bottom = y;
            if (x > right) right = x;
            if (y > top) top = y;
        }

        return (left, bottom, right, top);
    }
}

[HarmonyPatch(typeof(Camera), nameof(Camera.Render))]
internal static class HelpMenuClipCameraRenderPatch
{
    public static void Prefix(Camera __instance)
    {
        HelpMenuClipMaterialController.HandlePreCull(__instance);
    }
}
