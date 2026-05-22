using System;
using SuperNewRoles.Modules;
using UnityEngine;
using UnityEngine.Rendering;

namespace SuperNewRoles.CustomObject;

public sealed class RocketLauncherHeldPlayer : MonoBehaviour
{
    private const string HeldPlayerSpriteName = "RocketLauncherLaunchPlayer.png";
    private const float ForwardOffset = 0.62f;
    private const float HeightOffset = 0.42f;
    private const float LocalDepth = 0.65f;
    private const float HeldScale = 1.44f;

    private ExPlayerControl _source;
    private ExPlayerControl _target;
    private SpriteRenderer _spriteRenderer;

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
        ApplySourceRendererSettings();
        ApplyTargetColor();

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

        bool playerFlipX = IsPlayerFlipX();
        float direction = playerFlipX ? -1f : 1f;
        transform.localPosition = new Vector3(direction * ForwardOffset, HeightOffset, LocalDepth);
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one * HeldScale;
        if (_spriteRenderer != null)
            _spriteRenderer.flipX = !playerFlipX;
    }

    private void ApplySourceRendererSettings()
    {
        if (_spriteRenderer == null)
            return;

        var sourceBodyRenderer = GetSourceBodyRenderer();
        if (sourceBodyRenderer != null)
        {
            gameObject.layer = sourceBodyRenderer.gameObject.layer;
            _spriteRenderer.sharedMaterial = sourceBodyRenderer.sharedMaterial;
            _spriteRenderer.maskInteraction = sourceBodyRenderer.maskInteraction;
            _spriteRenderer.sortingLayerID = sourceBodyRenderer.sortingLayerID;
            _spriteRenderer.sortingOrder = sourceBodyRenderer.sortingOrder - 1;
            _spriteRenderer.enabled = sourceBodyRenderer.enabled && sourceBodyRenderer.gameObject.activeInHierarchy && sourceBodyRenderer.color.a > 0.001f;
        }
        else
        {
            _spriteRenderer.sharedMaterial = FastDestroyableSingleton<HatManager>.Instance.PlayerMaterial;
            _spriteRenderer.maskInteraction = SpriteMaskInteraction.None;
            _spriteRenderer.enabled = true;
        }

        PlayerMaterial.SetMaskLayerBasedOnLocalPlayer(_spriteRenderer, _source != null && _source.AmOwner);
        ApplyTargetColor();
    }

    private SpriteRenderer GetSourceBodyRenderer()
    {
        return _source?.cosmetics?.currentBodySprite?.BodySprite;
    }

    private bool IsPlayerFlipX()
    {
        if (_source?.cosmetics != null)
            return _source.cosmetics.FlipX;
        return _source?.Player?.MyPhysics != null && _source.Player.MyPhysics.FlipX;
    }

    private void ApplyTargetColor()
    {
        if (_target?.Data == null || _spriteRenderer == null)
            return;
        PlayerMaterial.SetColors(_target.Data.DefaultOutfit.ColorId, _spriteRenderer);
    }

    private void OnDestroy()
    {
        if (_spriteRenderer != null)
            _spriteRenderer.sprite = null;
        _source = null;
        _target = null;
        _spriteRenderer = null;
    }
}
