using SuperNewRoles.Roles.Ability;
using UnityEngine;

namespace SuperNewRoles.WaveCannonObj;

public class WaveCannonObjectCannon : WaveCannonObjectBase
{
    public WaveCannonObjectCannon(WaveCannonAbility ability) : base(ability)
    {
        _gameObject = new GameObject("WaveCannonObjectCannon");
    }

    // WaveCannonObjectBase の抽象メンバーを実装
    public override Collider2D[] HitColliders => [ability.Player.Collider];
    public override float ShootTime => 6f;
    private GameObject _gameObject;
    public override GameObject WaveCannonObject => _gameObject;

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
    }
}