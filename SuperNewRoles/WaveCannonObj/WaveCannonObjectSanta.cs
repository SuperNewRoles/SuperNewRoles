using System.Collections.Generic;
using System.Linq;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability;
using UnityEngine;

namespace SuperNewRoles.WaveCannonObj;

public class WaveCannonObjectSanta : WaveCannonObjectBase
{
    private const float SantaSpawnTimeInterval = 0.3f;
    private const string SantaCannonObjectName = "WaveCannonCannon";
    private const string SantaCannonSpriteName = "WaveCannonCannon.png";
    private const string SantaTankObjectNameFallback = "WaveCannonTank";
    private const string SantaTankSpriteNameFallback = "WaveCannonTank.png";

    private readonly List<WCSantaHandler> _santas = new();
    private float _santaSpawnTimer = -1f;
    private readonly string _shootSound;

    public WaveCannonObjectSanta(
        WaveCannonAbility ability,
        bool isFlipX,
        Vector3 startPosition,
        bool isResetKillCooldown,
        string chargeSound = "WaveCannon.Charge.mp3",
        string shootSound = "WaveCannon.Shoot.mp3")
        : base(ability, isFlipX, startPosition, isResetKillCooldown)
    {
        _shootSound = shootSound;
        WCSantaHandler.IsFlipX = isFlipX;
        WCSantaHandler.WiseManVector = Vector3.zero;
        WCSantaHandler.Angle = 0f;
        WCSantaHandler.reflection = false;
        WCSantaHandler.Xdiff = 0f;

        _gameObject = new GameObject("WaveCannonObjectSanta");
        _gameObject.transform.localScale = new Vector3(isFlipX ? -1 : 1, 1, 1);
        _gameObject.transform.localPosition = startPosition - new Vector3(0, 0.15f, 3.5f);

        TankSpriteObject = new GameObject("TankSpriteObject");
        TankSpriteObject.transform.parent = _gameObject.transform;
        TankSpriteObject.transform.localPosition = Vector3.zero;
        TankSpriteObject.transform.localScale = Vector3.one;
        _tankspriteRenderer = TankSpriteObject.AddComponent<SpriteRenderer>();
        _tankspriteRenderer.sprite = LoadSpriteWithFallback(SantaCannonSpriteName, SantaTankSpriteNameFallback);
        if (_tankspriteRenderer.sprite == null)
            Logger.Error("Santa wave cannon sprite not found in asset bundle.", "WaveCannonObjectSanta");

        _tankspriteRenderer.sharedMaterial = FastDestroyableSingleton<HatManager>.Instance.PlayerMaterial;
        _tankspriteRenderer.maskInteraction = SpriteMaskInteraction.None;
        PlayerMaterial.SetMaskLayerBasedOnLocalPlayer(_tankspriteRenderer, false);
        PlayerMaterial.SetColors(ability.Player.Player.CurrentOutfit.ColorId, _tankspriteRenderer);

        _player = PoolablePrefabManager.GeneratePlayer(ability.Player);
        _player.transform.parent = _gameObject.transform;
        _player.transform.localPosition = new(0, 0.4f, 1f);
        _player.transform.localScale = Vector3.one * 0.4f;
        SetChildrenLayer(_player.gameObject, 0);

        _player.cosmetics.nameText.gameObject.SetActive(false);
        SoundManager.Instance.PlaySound(AssetManager.GetAsset<AudioClip>(chargeSound), false);

        var cannonObj = LoadPrefabWithFallback(SantaCannonObjectName, SantaTankObjectNameFallback);
        if (cannonObj == null)
            throw new System.Exception("Santa wave cannon prefab not found in asset bundle.");
        _tankObj = GameObject.Instantiate(cannonObj, _gameObject.transform);

        _shootRenderer = _tankObj.transform.Find("Shooting").GetComponentInChildren<SpriteRenderer>();
    }

    private static Sprite LoadSpriteWithFallback(string primary, string fallback)
    {
        var sprite = AssetManager.GetAsset<Sprite>(primary);
        return sprite ?? AssetManager.GetAsset<Sprite>(fallback);
    }

    private static GameObject LoadPrefabWithFallback(string primary, string fallback)
    {
        var prefab = AssetManager.GetAsset<GameObject>(primary);
        return prefab ?? AssetManager.GetAsset<GameObject>(fallback);
    }

    public override Collider2D[] HitColliders
        => _santas
            .Where(x => x != null && x.KillCollider != null)
            .Select(x => (Collider2D)x.KillCollider)
            .ToArray();
    public override float ShootTime => 2.88f;
    private GameObject _gameObject;
    private GameObject _tankObj;
    public override GameObject WaveCannonObject => _gameObject;
    public override bool HidePlayer => true;
    public override Vector3 startPositionOffset => new(0, 0f, 0);

    public override SpriteRenderer ShootRenderer => _shootRenderer;
    private SpriteRenderer _shootRenderer;

    private SpriteRenderer _tankspriteRenderer;
    private GameObject TankSpriteObject;
    private PoolablePlayer _player;

    public override void OnAnimationUpdateCharging()
    {
        // No-op for santa animation.
    }

    public override void OnAnimationUpdateShooting()
    {
        if (_santaSpawnTimer < 0f)
            return;

        _santas.RemoveAll(x => x == null);
        _santaSpawnTimer -= Time.deltaTime;
        if (_santaSpawnTimer <= 0f)
        {
            SpawnSanta();
        }
    }

    public override void OnAnimationShoot()
    {
        SoundManager.Instance.PlaySound(AssetManager.GetAsset<AudioClip>(_shootSound), false);
        _tankObj.transform.Find("Cannon_Charge").gameObject.SetActive(false);
        SpawnSanta();
    }

    public override void OnAnimationWiseMan(float distanceX, Vector3 position, float angle)
    {
        var shootingObj = _tankObj.transform.Find("Shooting");
        if (shootingObj == null) return;

        var wavecannonBeam = shootingObj.Find("wavecannon_beam");
        if (wavecannonBeam == null) return;

        var clonedBeam = GameObject.Instantiate(wavecannonBeam.gameObject, shootingObj);
        clonedBeam.transform.position = position;
        clonedBeam.transform.rotation = Quaternion.Euler(0, 0, angle);

        GameObject.Destroy(wavecannonBeam.GetComponent<PolygonCollider2D>());
        wavecannonBeam.gameObject.AddComponent<PolygonCollider2D>().isTrigger = true;

        var waveCannonBeamRenderer = wavecannonBeam.GetComponent<SpriteRenderer>();
        waveCannonBeamRenderer.size = new Vector2(distanceX, waveCannonBeamRenderer.size.y);
    }

    private void SpawnSanta()
    {
        var santaHandler = new GameObject("Santa").AddComponent<WCSantaHandler>();
        santaHandler.Init(ability);
        // 発射終了後にWaveCannonObjectがDestroyされてもサンタが破棄されないよう、親子付けしない
        santaHandler.transform.position = _gameObject.transform.TransformPoint(new Vector3(-2.4f + 3.3f, 0.275f, 0.1f));
        santaHandler.transform.localScale = new(WCSantaHandler.IsFlipX ? 0.1f : -0.1f, 0.1f, 0.1f);
        santaHandler.moveX = 2.4f;
        _santas.Add(santaHandler);
        _santaSpawnTimer = SantaSpawnTimeInterval;
    }

    private void SetChildrenLayer(GameObject gameObject, int layer)
    {
        gameObject.layer = layer;
        for (int i = 0; i < gameObject.transform.childCount; i++)
        {
            SetChildrenLayer(gameObject.transform.GetChild(i).gameObject, layer);
        }
    }
}
