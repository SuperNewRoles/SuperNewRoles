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
        RpcUsePlatform();
        ResetTimer();
    }

    [CustomRPC]
    public void RpcUsePlatform()
    {
        AirshipStatus airshipStatus = GameObject.FindObjectOfType<AirshipStatus>();
        if (airshipStatus)
        {
            airshipStatus.GapPlatform.StopAllCoroutines();
            airshipStatus.GapPlatform.StartCoroutine(NotMoveUsePlatform(airshipStatus.GapPlatform).WrapToIl2Cpp());
        }
    }

    private static IEnumerator NotMoveUsePlatform(MovingPlatformBehaviour platform)
    {
        bool IsTargetOn = platform.Target != null;
        if (IsTargetOn)
        {
            // リプレイ機能は新版では削除されているため省略
        }
        if (platform.Target == null)
        {
            platform.Target = PlayerControl.LocalPlayer;
        }
        Vector3 val = platform.IsLeft ? platform.LeftUsePosition : platform.RightUsePosition;
        Vector3 val2 = (!platform.IsLeft) ? platform.LeftUsePosition : platform.RightUsePosition;
        Vector3 sourcePos = platform.IsLeft ? platform.LeftPosition : platform.RightPosition;
        Vector3 targetPos = (!platform.IsLeft) ? platform.LeftPosition : platform.RightPosition;
        Vector3 val3 = platform.transform.parent.TransformPoint(val);
        Vector3 worldUseTargetPos = platform.transform.parent.TransformPoint(val2);
        Vector3 worldSourcePos2 = platform.transform.parent.TransformPoint(sourcePos);
        Vector3 worldTargetPos2 = platform.transform.parent.TransformPoint(targetPos);

        if (Constants.ShouldPlaySfx())
        {
            SoundManager.Instance.StopNamedSound("PlatformMoving");
            SoundManager.Instance.PlayDynamicSound("PlatformMoving", platform.MovingSound, loop: true, (DynamicSound.GetDynamicsFunction)platform.SoundDynamics, SoundManager.instance.sfxMixer);
        }

        platform.IsLeft = !platform.IsLeft;

        if (IsTargetOn)
        {
            yield return Effects.All(
                Effects.Slide2D(platform.transform, platform.transform.localPosition, targetPos, platform.Target.MyPhysics.Speed),
                Effects.Slide2DWorld(platform.Target.transform, platform.transform.position + new Vector3(0, 0.3f), worldTargetPos2 + new Vector3(0, 0.3f), platform.Target.MyPhysics.Speed)
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

        if (IsTargetOn)
        {
            platform.Target.MyPhysics.enabled = true;
            yield return platform.Target.MyPhysics.WalkPlayerTo(worldUseTargetPos);
            platform.Target.SetPetPosition(platform.Target.transform.position);
            yield return Effects.Wait(0.1f);
            platform.Target.inMovingPlat = false;
            platform.Target.Collider.enabled = true;
            platform.Target.moveable = true;
            platform.Target.NetTransform.enabled = true;
        }

        platform.Target = null;
    }

    public override bool CheckIsAvailable()
    {
        return Timer <= 0 && PlayerControl.LocalPlayer.CanMove && ShipStatus.Instance.TryCast<AirshipStatus>() != null;
    }
}