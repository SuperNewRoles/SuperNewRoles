using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability;
using UnityEngine;

namespace SuperNewRoles.WaveCannonObj;

public class WaveCannonObjectTank : WaveCannonObjectBase
{
    public WaveCannonObjectTank(WaveCannonAbility ability, bool isFlipX, Vector3 startPosition, bool isResetKillCooldown, string tankObjectName = "WaveCannonTank", string chargeSound = "WaveCannon.Charge.mp3") : base(ability, isFlipX, startPosition, isResetKillCooldown)
    {
        // 砲台のコンテナオブジェクトを生成
        _gameObject = new GameObject("WaveCannonObjectTank");
        _gameObject.transform.localScale = new Vector3(isFlipX ? -1 : 1, 1, 1);
        _gameObject.transform.localPosition = startPosition - new Vector3(0, 0.15f, 3.5f);

        // 砲台の見た目を生成
        TankSpriteObject = new GameObject("TankSpriteObject");
        TankSpriteObject.transform.parent = _gameObject.transform;
        TankSpriteObject.transform.localPosition = Vector3.zero;
        TankSpriteObject.transform.localScale = Vector3.one;
        _tankspriteRenderer = TankSpriteObject.AddComponent<SpriteRenderer>();
        _tankspriteRenderer.sprite = AssetManager.GetAsset<Sprite>("WaveCannonTank.png");

        // 色を設定
        _tankspriteRenderer.sharedMaterial = FastDestroyableSingleton<HatManager>.Instance.PlayerMaterial;
        _tankspriteRenderer.maskInteraction = SpriteMaskInteraction.None;
        PlayerMaterial.SetMaskLayerBasedOnLocalPlayer(_tankspriteRenderer, false);
        PlayerMaterial.SetColors(ability.Player.Player.CurrentOutfit.ColorId, _tankspriteRenderer);

        // 砲台に乗っているプレイヤーを生成
        _player = PoolablePrefabManager.GeneratePlayer(ability.Player);
        _player.transform.parent = _gameObject.transform;
        _player.transform.localPosition = new(0, 0.4f, 1f);
        _player.transform.localScale = Vector3.one * 0.4f;
        SetChildrenLayer(_player.gameObject, 0);

        _player.cosmetics.nameText.gameObject.SetActive(false);
        SoundManager.Instance.PlaySound(AssetManager.GetAsset<AudioClip>(chargeSound), false);

        var tankObj = AssetManager.GetAsset<GameObject>(tankObjectName);
        _tankObj = GameObject.Instantiate(tankObj, _gameObject.transform);
        _colliders = _tankObj.GetComponentsInChildren<Collider2D>(true);

        _shootRenderer = _tankObj.transform.Find("Shooting").GetComponentInChildren<SpriteRenderer>();
    }
    public override void Detach()
    {
        base.Detach();
    }
    private void SetChildrenLayer(GameObject gameObject, int layer)
    {
        gameObject.layer = layer;
        for (int i = 0; i < gameObject.transform.childCount; i++)
        {
            SetChildrenLayer(gameObject.transform.GetChild(i).gameObject, layer);
        }
    }
    // WaveCannonObjectBase の抽象メンバーを実装
    public override Collider2D[] HitColliders => _colliders;
    private Collider2D[] _colliders;
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
        // 実装を追加
    }

    public override void OnAnimationUpdateShooting()
    {
        // 実装を追加
    }

    public override void OnAnimationShoot()
    {
        // 実装を追加
        SoundManager.Instance.PlaySound(AssetManager.GetAsset<AudioClip>("WaveCannon.Shoot.mp3"), false);
        _tankObj.transform.Find("Cannon_Charge").gameObject.SetActive(false);
        _tankObj.transform.Find("Shooting").gameObject.SetActive(true);
    }

    public override void OnAnimationWiseMan(float distanceX, Vector3 position, float angle)
    {
        // Shooting/wavecannon_beamオブジェクトを取得
        var shootingObj = _tankObj.transform.Find("Shooting");
        if (shootingObj == null) return;

        var wavecannonBeam = shootingObj.Find("wavecannon_beam");
        if (wavecannonBeam == null) return;

        // wavecannon_beamを複製
        var clonedBeam = GameObject.Instantiate(wavecannonBeam.gameObject, shootingObj);

        // 複製したオブジェクトを指定された位置に移動
        clonedBeam.transform.position = position;
        // 指定された角度に回転
        clonedBeam.transform.rotation = Quaternion.Euler(0, 0, angle);

        // コライダーつけ直す
        GameObject.Destroy(wavecannonBeam.GetComponent<PolygonCollider2D>());
        wavecannonBeam.gameObject.AddComponent<PolygonCollider2D>().isTrigger = true;

        // コライダーを再計算
        _colliders = _tankObj.GetComponentsInChildren<Collider2D>(true);

        var waveCannonBeamRenderer = wavecannonBeam.GetComponent<SpriteRenderer>();
        waveCannonBeamRenderer.size = new Vector2(distanceX, waveCannonBeamRenderer.size.y);
    }
}