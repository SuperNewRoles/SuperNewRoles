using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using SuperNewRoles.Roles.Impostor;
using UnityEngine;

namespace SuperNewRoles.Replay.ReplayActions;
public class ReplayActionMovingPlatform : ReplayAction
{
    public byte sourcePlayer;

    public int CurrentTurn;
    public int endposindex;

    public static ReplayActionMovingPlatform currentAction;

    public override void ReadReplayFile(BinaryReader reader)
    {
        ActionTime = reader.ReadSingle();
        //ここにパース処理書く
        sourcePlayer = reader.ReadByte();
    }
    public override void WriteReplayFile(BinaryWriter writer)
    {
        writer.Write(ActionTime);
        //ここにパース処理書く
        writer.Write(sourcePlayer);
    }
    public override ReplayActionId GetActionId() => ReplayActionId.MovingPlatform;
    //アクション実行時の処理
    public override void OnAction()
    {
        //ここに処理書く
        MovingPlatformBehaviour mpb = GameObject.FindObjectOfType<MovingPlatformBehaviour>();
        currentAction = this;
        mpb.StartCoroutine(UseMovingPlatform(mpb, ModHelpers.PlayerById(sourcePlayer), this).WrapToIl2Cpp());
    }
    public override void OnReplay()
    {
        //MovingPlatformBehaviour mpb = GameObject.FindObjectOfType<MovingPlatformBehaviour>();
        //mpb.StartCoroutine(UseMovingPlatform(mpb, ModHelpers.PlayerById(sourcePlayer)).WrapToIl2Cpp());
    }
    //試合内でアクションがあったら実行するやつ
    public static ReplayActionMovingPlatform Create(byte sourcePlayer)
    {
        ReplayActionMovingPlatform action = new();
        if (!CheckAndCreate(action)) return null;
        //ここで初期化(コレは仮処理だから消してね)
        action.sourcePlayer = sourcePlayer;
        return action;
    }
    public static IEnumerator MovingPlatformFrameCounter(MovingPlatformBehaviour __instance)
    {
        while (true)
        {
            yield return null;
            ReplayManager.CurrentReplay.MovingPlatformFrameCount--;
            ReplayManager.CurrentReplay.MovingPlatformPosition = __instance.transform.localPosition;
            if (ReplayManager.CurrentReplay.MovingPlatformFrameCount <= 0)
            {
                yield break;
            }
        }
    }
    public static IEnumerator MovingPlatformFrameCounterRewind(MovingPlatformBehaviour __instance, PlayerPhysics target)
    {
        while (true)
        {
            yield return null;
            ReplayManager.CurrentReplay.MovingPlatformFrameCount++;
            ReplayManager.CurrentReplay.MovingPlatformPosition = __instance.transform.localPosition;
            if (ReplayManager.CurrentReplay.MovingPlatformFrameCount >= (int)(target.Speed * 60))
            {
                yield break;
            }
        }
    }

    public static IEnumerator UseMovingPlatform(MovingPlatformBehaviour __instance, PlayerControl target, ReplayActionMovingPlatform by = null)
    {
        if ((int)ReplayManager.CurrentReplay.CurrentMovingPlatformState <= (int)MovingPlatformState.Init)
        {
            ReplayManager.CurrentReplay.MovingPlatformFrameCount = (int)(target.MyPhysics.Speed * 60);
            ReplayManager.CurrentReplay.CurrentMovingPlatformState = MovingPlatformState.Init;
            target.MyPhysics.ResetMoveState();
        }
        __instance.Target = target;
        Logger.Info(ReplayManager.CurrentReplay.MovingPlatformFrameCount.ToString() + ":" + ReplayManager.CurrentReplay.CurrentMovingPlatformState.ToString(), "FRAMECOUNT");
        if (target.AmOwner)
        {
            PlayerControl.HideCursorTemporarily();
        }
            ((Behaviour)target.Collider).enabled = false;
        target.moveable = false;
        ((Behaviour)target.NetTransform).enabled = false;
        target.inMovingPlat = true;
        target.ForceKillTimerContinue = true;
        Vector3 val = (__instance.IsLeft ? __instance.LeftUsePosition : __instance.RightUsePosition);
        Vector3 val2 = ((!__instance.IsLeft) ? __instance.LeftUsePosition : __instance.RightUsePosition);
        Vector3 sourcePos = ReplayManager.CurrentReplay.MovingPlatformPosition.x == -999 ? (__instance.IsLeft ? __instance.LeftPosition : __instance.RightPosition) : ReplayManager.CurrentReplay.MovingPlatformPosition;
        Vector3 targetPos = ((!__instance.IsLeft) ? __instance.LeftPosition : __instance.RightPosition);
        Vector3 val3 = ((Component)__instance).transform.parent.TransformPoint(val);
        Vector3 worldUseTargetPos = ((Component)__instance).transform.parent.TransformPoint(val2);
        Vector3 worldSourcePos2 = ((Component)__instance).transform.parent.TransformPoint(sourcePos);
        Vector3 worldTargetPos2 = ((Component)__instance).transform.parent.TransformPoint(targetPos);
        if (ReplayManager.CurrentReplay.MovingPlatformPosition.x != -999)
        {
            worldSourcePos2.y = 8.9246f;
            worldTargetPos2.y = 8.9246f;
        }
        __instance.IsLeft = !__instance.IsLeft;
        if ((int)ReplayManager.CurrentReplay.CurrentMovingPlatformState <= (int)MovingPlatformState.WalkTo1st)
        {
            ReplayManager.CurrentReplay.CurrentMovingPlatformState = MovingPlatformState.WalkTo1st;
            yield return target.MyPhysics.WalkPlayerTo(val3);
        }
        if ((int)ReplayManager.CurrentReplay.CurrentMovingPlatformState <= (int)MovingPlatformState.WalkTo2nd)
        {
            ReplayManager.CurrentReplay.CurrentMovingPlatformState = MovingPlatformState.WalkTo2nd;
            yield return target.MyPhysics.WalkPlayerTo(worldSourcePos2);
        }
        if ((int)ReplayManager.CurrentReplay.CurrentMovingPlatformState <= (int)MovingPlatformState.WaitEffect1st)
        {
            ReplayManager.CurrentReplay.CurrentMovingPlatformState = MovingPlatformState.WaitEffect1st;
            yield return Effects.Wait(0.1f);
            ((Behaviour)target.MyPhysics).enabled = false;
            worldSourcePos2 -= (Vector3)target.Collider.offset;
            worldTargetPos2 -= (Vector3)target.Collider.offset;
            if (Constants.ShouldPlaySfx())
            {
                SoundManager.Instance.PlayDynamicSound("PlatformMoving", __instance.MovingSound, loop: true, (DynamicSound.GetDynamicsFunction)__instance.SoundDynamics, SoundManager.Instance.SfxChannel);
            }
        }
        if ((int)ReplayManager.CurrentReplay.CurrentMovingPlatformState <= (int)MovingPlatformState.Slide)
        {
            float Speed = (ReplayManager.CurrentReplay.MovingPlatformFrameCount / 60.0f);
            ReplayManager.CurrentReplay.CurrentMovingPlatformState = MovingPlatformState.Slide;
            yield return Effects.All(Effects.Slide2D(((Component)__instance).transform, sourcePos, targetPos, Speed), Effects.Slide2DWorld(((Component)target).transform, worldSourcePos2, worldTargetPos2, Speed), MovingPlatformFrameCounter(__instance).WrapToIl2Cpp());
            if (Constants.ShouldPlaySfx())
            {
                SoundManager.Instance.StopNamedSound("PlatformMoving");
            }
            if (target == null)
            {
                __instance.ResetPlatform();
                yield break;
            }
        ((Behaviour)target.MyPhysics).enabled = true;
        }

        if ((int)ReplayManager.CurrentReplay.CurrentMovingPlatformState <= (int)MovingPlatformState.WalkTo3rd)
        {
            ReplayManager.CurrentReplay.CurrentMovingPlatformState = MovingPlatformState.WalkTo3rd;
            yield return target.MyPhysics.WalkPlayerTo(worldUseTargetPos);
            target.SetPetPosition(((Component)target).transform.position);
        }
        if ((int)ReplayManager.CurrentReplay.CurrentMovingPlatformState <= (int)MovingPlatformState.WaitEffect2nd)
        {
            ReplayManager.CurrentReplay.CurrentMovingPlatformState = MovingPlatformState.WaitEffect2nd;
            yield return Effects.Wait(0.1f);
            target.inMovingPlat = false;
            ((Behaviour)target.Collider).enabled = true;
            target.moveable = true;
            ((Behaviour)target.NetTransform).enabled = true;
            target.ForceKillTimerContinue = false;
            __instance.Target = null;
        }
        ReplayManager.CurrentReplay.CurrentMovingPlatformState = MovingPlatformState.None;
        ReplayManager.CurrentReplay.MovingPlatformPosition = new(-999, -999, -999);
        if (by != null)
        {
            by.CurrentTurn = ReplayLoader.CurrentTurn;
            by.endposindex = ReplayLoader.posindex;
        }
        currentAction = null;
    }
}