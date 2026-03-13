using System.Collections;
using SuperNewRoles.Roles.Ability.CustomButton;
using SuperNewRoles.Modules;
using UnityEngine;
using BepInEx.Unity.IL2CPP.Utils.Collections;

namespace SuperNewRoles.Roles.Ability;

public class NunAbility : CustomButtonBase
{
    private float coolDown;

    public override Sprite Sprite => AssetManager.GetAsset<Sprite>("NunButton.png");
    public override string buttonText => ModTranslation.GetString("NunButtonName");
    protected override KeyType keytype => KeyType.Ability1;
    public override float DefaultTimer => coolDown;

    public NunAbility(float coolDown)
    {
        this.coolDown = coolDown;
    }

    public override void OnClick()
    {
        AirshipStatus airshipStatus = ShipStatus.Instance.TryCast<AirshipStatus>();
        if (airshipStatus == null) return;

        MovingPlatformBehaviour platform = airshipStatus.GapPlatform;
        RpcUsePlatform(platform.IsLeft, platform.Target?.PlayerId ?? byte.MaxValue);
        ResetTimer();
    }

    [CustomRPC]
    public void RpcUsePlatform(bool startIsLeft, byte movingTargetId)
    {
        AirshipStatus airshipStatus = GameObject.FindObjectOfType<AirshipStatus>();
        if (airshipStatus)
        {
            airshipStatus.GapPlatform.StopAllCoroutines();
            airshipStatus.GapPlatform.StartCoroutine(NotMoveUsePlatform(airshipStatus.GapPlatform, startIsLeft, movingTargetId).WrapToIl2Cpp());
        }
    }

    private static IEnumerator NotMoveUsePlatform(MovingPlatformBehaviour platform, bool startIsLeft, byte movingTargetId)
    {
        platform.IsLeft = startIsLeft;

        PlayerControl movingTarget = movingTargetId == byte.MaxValue ? null : ExPlayerControl.ById(movingTargetId);
        bool isTargetOn = movingTarget != null;
        if (isTargetOn)
        {
            // リプレイ機能は新版では削除されているため省略
            platform.Target = movingTarget;
        }
        if (platform.Target == null)
        {
            platform.Target = PlayerControl.LocalPlayer;
        }
        Vector3 val2 = (!startIsLeft) ? platform.LeftUsePosition : platform.RightUsePosition;
        Vector3 targetPos = (!startIsLeft) ? platform.LeftPosition : platform.RightPosition;
        Vector3 worldUseTargetPos = platform.transform.parent.TransformPoint(val2);
        Vector3 worldTargetPos2 = platform.transform.parent.TransformPoint(targetPos);

        if (Constants.ShouldPlaySfx())
        {
            SoundManager.Instance.StopNamedSound("PlatformMoving");
            SoundManager.Instance.PlayDynamicSound("PlatformMoving", platform.MovingSound, loop: true, (DynamicSound.GetDynamicsFunction)platform.SoundDynamics, SoundManager.instance.sfxMixer);
        }

        platform.IsLeft = !startIsLeft;

        if (isTargetOn)
        {
            yield return Effects.All(
                Effects.Slide2D(platform.transform, platform.transform.localPosition, targetPos, movingTarget.MyPhysics.Speed),
                Effects.Slide2DWorld(movingTarget.transform, platform.transform.position + new Vector3(0, 0.3f), worldTargetPos2 + new Vector3(0, 0.3f), movingTarget.MyPhysics.Speed)
            );
        }
        else
        {
            yield return Effects.All(Effects.Slide2D(platform.transform, platform.transform.localPosition, targetPos, PlayerControl.LocalPlayer.MyPhysics.Speed));
        }

        if (Constants.ShouldPlaySfx())
        {
            SoundManager.Instance.StopNamedSound("PlatformMoving");
        }

        if (isTargetOn)
        {
            movingTarget.MyPhysics.enabled = true;
            yield return movingTarget.MyPhysics.WalkPlayerTo(worldUseTargetPos);
            movingTarget.MyPhysics.ResetMoveState();
            movingTarget.MyPhysics.ResetAnimState();
            movingTarget.MyPhysics.body.velocity = Vector2.zero;
            movingTarget.NetTransform.RpcSnapTo(movingTarget.transform.position);
            movingTarget.SetPetPosition(movingTarget.transform.position);
            yield return Effects.Wait(0.1f);
            movingTarget.inMovingPlat = false;
            movingTarget.onLadder = false;
            movingTarget.Collider.enabled = true;
            movingTarget.moveable = true;
            movingTarget.NetTransform.enabled = true;
        }

        platform.Target = null;
    }

    public override bool CheckIsAvailable()
    {
        return Timer <= 0 && PlayerControl.LocalPlayer.CanMove && ShipStatus.Instance.TryCast<AirshipStatus>() != null;
    }
}
