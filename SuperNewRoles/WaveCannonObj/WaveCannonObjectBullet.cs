using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability;
using UnityEngine;

namespace SuperNewRoles.WaveCannonObj;

public class WaveCannonObjectBullet : WaveCannonObjectTank
{
    public WaveCannonObjectBullet(WaveCannonAbility ability, bool isFlipX, Vector3 startPosition, bool isResetKillCooldown)
        : base(ability, isFlipX, startPosition, isResetKillCooldown, "WaveCannonBullet", "BulletChargeSound.mp3")
    {

    }
    public override bool EnabledWiseMan => false;
}