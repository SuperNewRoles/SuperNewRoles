using System;
using SuperNewRoles.Modules;
using UnityEngine;
using UnityEngine.Rendering;

namespace SuperNewRoles.CustomObject;

public sealed class RocketLauncherHeldPlayer : MonoBehaviour
{
    private const string HeldPlayerSpriteName = "RocketLauncherLaunchPlayer.png";
    private const float ForwardOffset = 0.62f;
    private const float HeightOffset = 0.52f;
    private const float LocalDepth = 0.65f;
    private const float HeldScale = 1.44f;

    private ExPlayerControl _source;
    private ExPlayerControl _target;
    private SpriteRenderer _spriteRenderer;
    private Material _lastSharedMaterial;
    private SpriteMaskInteraction _lastMaskInteraction;
    private int _lastLayer = int.MinValue;
    private int _lastSortingLayerId = int.MinValue;
    private int _lastSortingOrder = int.MinValue;
    private int _lastTargetColorId = int.MinValue;
    private bool _lastMaskOwner;
    private bool _hasAppliedMaskLayer;

    public static RocketLauncherHeldPlayer Spawn(ExPlayerControl source, ExPlayerControl target)
    {
        GameObject gameObject = new("RocketLauncherHeldPlayer");
        var heldPlayer = gameObject.AddComponent<RocketLauncherHeldPlayer>();
        heldPlayer.Init(source, target);
        return heldPlayer;
    }

    private void Init(ExPlayerControl source, ExPlayerControl target)
    {
        _source = source;
        _target = target;
        if (_source?.Player == null)
            throw new Exception("RocketLauncherHeldPlayer source is null.");

        transform.SetParent(_source.Player.transform, worldPositionStays: false);

        var sprite = AssetManager.GetAsset<Sprite>(HeldPlayerSpriteName);
        if (sprite == null)
            throw new Exception($"Failed to load Asset: {HeldPlayerSpriteName}");

        _spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        _spriteRenderer.sprite = sprite;
        ApplySourceRendererSettings(force: true);

        UpdatePose();
    }

    private void FixedUpdate()
    {
        if (!CanShow())
        {
            if (_spriteRenderer != null)
                _spriteRenderer.enabled = false;
            return;
        }

        ApplySourceRendererSettings();

        UpdatePose();
    }

    private bool CanShow()
    {
        if (_source == null || _target == null)
            return false;
        if (!_source.IsAlive() || !_target.IsAlive())
            return false;
        if (_source.Player == null || _source.Player.inVent)
            return false;
        if (MeetingHud.Instance != null)
            return false;
        return true;
    }

    private void UpdatePose()
    {
        if (_source?.Player == null)
            return;

        bool playerFlipX = IsPlayerFlipX(_source);
        float direction = GetFacingDirection(_source);
        transform.localPosition = new Vector3(direction * ForwardOffset, HeightOffset, LocalDepth);
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one * HeldScale;
        if (_spriteRenderer != null)
            _spriteRenderer.flipX = !playerFlipX;
    }

    private void ApplySourceRendererSettings(bool force = false)
    {
        if (_spriteRenderer == null)
            return;

        bool settingsChanged = false;
        bool materialChanged = false;
        var sourceBodyRenderer = GetSourceBodyRenderer();
        if (sourceBodyRenderer != null)
        {
            settingsChanged |= ApplyLayer(sourceBodyRenderer.gameObject.layer);
            materialChanged = ApplySharedMaterial(sourceBodyRenderer.sharedMaterial);
            settingsChanged |= materialChanged;
            settingsChanged |= ApplyMaskInteraction(sourceBodyRenderer.maskInteraction);
            settingsChanged |= ApplySorting(sourceBodyRenderer.sortingLayerID, sourceBodyRenderer.sortingOrder - 1);
            SetEnabledIfChanged(sourceBodyRenderer.enabled && sourceBodyRenderer.gameObject.activeInHierarchy && sourceBodyRenderer.color.a > 0.001f);
        }
        else
        {
            materialChanged = ApplySharedMaterial(FastDestroyableSingleton<HatManager>.Instance.PlayerMaterial);
            settingsChanged |= materialChanged;
            settingsChanged |= ApplyMaskInteraction(SpriteMaskInteraction.None);
            SetEnabledIfChanged(true);
        }

        ApplyMaskLayerIfNeeded(force || settingsChanged);
        ApplyTargetColorIfNeeded(force: materialChanged);
    }

    private bool ApplyLayer(int layer)
    {
        if (_lastLayer == layer && gameObject.layer == layer)
            return false;

        gameObject.layer = layer;
        _lastLayer = layer;
        return true;
    }

    private bool ApplySharedMaterial(Material material)
    {
        if (_lastSharedMaterial == material)
            return false;

        _spriteRenderer.sharedMaterial = material;
        _lastSharedMaterial = material;
        return true;
    }

    private bool ApplyMaskInteraction(SpriteMaskInteraction maskInteraction)
    {
        if (_lastMaskInteraction == maskInteraction && _spriteRenderer.maskInteraction == maskInteraction)
            return false;

        _spriteRenderer.maskInteraction = maskInteraction;
        _lastMaskInteraction = maskInteraction;
        return true;
    }

    private bool ApplySorting(int sortingLayerId, int sortingOrder)
    {
        if (_lastSortingLayerId == sortingLayerId
            && _lastSortingOrder == sortingOrder
            && _spriteRenderer.sortingLayerID == sortingLayerId
            && _spriteRenderer.sortingOrder == sortingOrder)
            return false;

        _spriteRenderer.sortingLayerID = sortingLayerId;
        _spriteRenderer.sortingOrder = sortingOrder;
        _lastSortingLayerId = sortingLayerId;
        _lastSortingOrder = sortingOrder;
        return true;
    }

    private void SetEnabledIfChanged(bool enabled)
    {
        if (_spriteRenderer.enabled != enabled)
            _spriteRenderer.enabled = enabled;
    }

    private void ApplyMaskLayerIfNeeded(bool force)
    {
        bool isOwner = _source != null && _source.AmOwner;
        if (!force && _hasAppliedMaskLayer && _lastMaskOwner == isOwner)
            return;

        PlayerMaterial.SetMaskLayerBasedOnLocalPlayer(_spriteRenderer, isOwner);
        _lastMaskOwner = isOwner;
        _hasAppliedMaskLayer = true;
    }

    private SpriteRenderer GetSourceBodyRenderer()
    {
        return _source?.cosmetics?.currentBodySprite?.BodySprite;
    }

    internal static Vector2 GetHeldTargetPosition(ExPlayerControl source)
    {
        if (source?.Player == null)
            return Vector2.zero;

        var localOffset = new Vector3(GetFacingDirection(source) * ForwardOffset, HeightOffset, 0f);
        return source.Player.transform.TransformPoint(localOffset);
    }

    private static float GetFacingDirection(ExPlayerControl source)
    {
        return IsPlayerFlipX(source) ? -1f : 1f;
    }

    private static bool IsPlayerFlipX(ExPlayerControl source)
    {
        if (source?.cosmetics != null)
            return source.cosmetics.FlipX;
        return source?.Player?.MyPhysics != null && source.Player.MyPhysics.FlipX;
    }

    private void ApplyTargetColorIfNeeded(bool force = false)
    {
        if (_target?.Data == null || _spriteRenderer == null)
            return;

        int colorId = _target.Data.DefaultOutfit.ColorId;
        if (!force && _lastTargetColorId == colorId)
            return;

        PlayerMaterial.SetColors(colorId, _spriteRenderer);
        _lastTargetColorId = colorId;
    }

    private void OnDestroy()
    {
        if (_spriteRenderer != null)
            _spriteRenderer.sprite = null;
        _source = null;
        _target = null;
        _spriteRenderer = null;
        _lastSharedMaterial = null;
    }
}
