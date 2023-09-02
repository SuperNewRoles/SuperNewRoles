using System.Collections;
using SuperNewRoles.Replay.ReplayActions;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace SuperNewRoles.Roles.Impostor;

public static class Nun
{
    static bool Is = false;
    public static IEnumerator NotMoveUsePlatform(MovingPlatformBehaviour __instance)
    {
        bool IsTargetOn = __instance.Target != null;
        if (Is) IsTargetOn = false;
        if (IsTargetOn)
        {
            ReplayActionMovingPlatform.Create(__instance.Target.PlayerId);
        }
        if (__instance.Target == null)
        {
            __instance.Target = PlayerControl.LocalPlayer;
            Is = true;
        }
        Vector3 val = __instance.IsLeft ? __instance.LeftUsePosition : __instance.RightUsePosition;
        Vector3 val2 = (!__instance.IsLeft) ? __instance.LeftUsePosition : __instance.RightUsePosition;
        Vector3 sourcePos = __instance.IsLeft ? __instance.LeftPosition : __instance.RightPosition;
        Vector3 targetPos = (!__instance.IsLeft) ? __instance.LeftPosition : __instance.RightPosition;
        Vector3 val3 = __instance.transform.parent.TransformPoint(val);
        Vector3 worldUseTargetPos = __instance.transform.parent.TransformPoint(val2);
        Vector3 worldSourcePos2 = __instance.transform.parent.TransformPoint(sourcePos);
        Vector3 worldTargetPos2 = __instance.transform.parent.TransformPoint(targetPos);
        if (Constants.ShouldPlaySfx())
        {
            SoundManager.Instance.StopNamedSound("PlatformMoving");
            SoundManager.Instance.PlayDynamicSound("PlatformMoving", __instance.MovingSound, loop: true, (DynamicSound.GetDynamicsFunction)__instance.SoundDynamics, SoundManager.instance.SfxChannel);
        }
        __instance.IsLeft = !__instance.IsLeft;
        if (IsTargetOn)
        {
            yield return Effects.All(Effects.Slide2D(__instance.transform, __instance.transform.localPosition, targetPos, __instance.Target.MyPhysics.Speed), Effects.Slide2DWorld(__instance.Target.transform, __instance.transform.position + new Vector3(0, 0.3f), worldTargetPos2 + new Vector3(0, 0.3f), __instance.Target.MyPhysics.Speed));
        }
        else
        {
            yield return Effects.All(Effects.Slide2D(__instance.transform, __instance.transform.localPosition, targetPos, CachedPlayer.LocalPlayer.PlayerPhysics.Speed));
        }
        if (Constants.ShouldPlaySfx())
        {
            SoundManager.Instance.StopNamedSound("PlatformMoving");
        }
        if (IsTargetOn)
        {
            __instance.Target.MyPhysics.enabled = true;
            yield return __instance.Target.MyPhysics.WalkPlayerTo(worldUseTargetPos);
            __instance.Target.SetPetPosition(__instance.Target.transform.position);
            yield return Effects.Wait(0.1f);
            __instance.Target.inMovingPlat = false;
            __instance.Target.Collider.enabled = true;
            __instance.Target.moveable = true;
            __instance.Target.NetTransform.enabled = true;
        }
        __instance.Target = null;
        Is = false;
    }
}