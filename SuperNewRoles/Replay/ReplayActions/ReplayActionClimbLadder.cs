using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using UnityEngine;

namespace SuperNewRoles.Replay.ReplayActions;
public class ReplayActionClimbLadder : ReplayAction
{
    public byte sourcePlayer;
    public byte ladderid;
    public byte climbLadderSid;
    public override void ReadReplayFile(BinaryReader reader)
    {
        ActionTime = reader.ReadSingle();
        //ここにパース処理書く
        sourcePlayer = reader.ReadByte();
        ladderid = reader.ReadByte();
        climbLadderSid = reader.ReadByte();
    }
    public override void WriteReplayFile(BinaryWriter writer)
    {
        writer.Write(ActionTime);
        //ここにパース処理書く
        writer.Write(sourcePlayer);
        writer.Write(ladderid);
        writer.Write(climbLadderSid);
    }
    public override ReplayActionId GetActionId() => ReplayActionId.ClimbLadder;
    //アクション実行時の処理
    public override void OnAction()
    {
        //ここに処理書く
        PlayerControl source = ModHelpers.PlayerById(sourcePlayer);
        if (source == null)
        {
            Logger.Info("sourceがnullだ");
            return;
        }
        Ladder ladder = ModHelpers.LadderById(ladderid);
        if (ladder == null)
        {
            Logger.Info("ladderがnullだ");
        }
        source.MyPhysics.lastClimbLadderSid = climbLadderSid;
        source.MyPhysics.ResetMoveState();
        ((MonoBehaviour)source.MyPhysics).StartCoroutine(CoClimbLadderCustom(source.MyPhysics, ladder, climbLadderSid).WrapToIl2Cpp());
    }
    public override void OnReplay()
    {
    }

    public static IEnumerator CoClimbLadderRewind(PlayerPhysics __instance, Ladder source, byte climbLadderSid)
    {
        Logger.Info("COMEEEEEEEEEEEED!!!!!!!!");
        if ((int)ReplayManager.CurrentReplay.CurrentLadderState[__instance.myPlayer.PlayerId] >= (int)LadderState.WaitEffect2nd)
        {
            ReplayManager.CurrentReplay.CurrentLadderState[__instance.myPlayer.PlayerId] = LadderState.WaitEffect2nd;
            yield return Effects.Wait(0.1f);
            ((Behaviour)__instance.myPlayer.Collider).enabled = true;
            __instance.myPlayer.moveable = true;
            ((Behaviour)__instance.myPlayer.NetTransform).enabled = true;
            __instance.myPlayer.ForceKillTimerContinue = false;
            __instance.myPlayer.onLadder = false;
        }
        if ((int)ReplayManager.CurrentReplay.CurrentLadderState[__instance.myPlayer.PlayerId] >= (int)LadderState.WalkTo2nd)
        {
            ReplayManager.CurrentReplay.CurrentLadderState[__instance.myPlayer.PlayerId] = LadderState.WalkTo2nd;
            yield return __instance.WalkPlayerTo(source.transform.position, 0.001f, (!source.IsTop) ? 1 : 2);
            __instance.myPlayer.SetPetPosition(((Component)__instance.myPlayer).transform.position);
            __instance.ResetAnimState();
        }

        if ((int)ReplayManager.CurrentReplay.CurrentLadderState[__instance.myPlayer.PlayerId] >= (int)LadderState.WaitEffect1st)
        {
            ReplayManager.CurrentReplay.CurrentLadderState[__instance.myPlayer.PlayerId] = LadderState.WaitEffect1st;
            yield return Effects.Wait(0.1f);
            __instance.StartClimb(source.IsTop);
            if (Constants.ShouldPlaySfx() && PlayerControl.LocalPlayer == __instance.myPlayer)
            {
                __instance.myPlayer.FootSteps.clip = source.UseSound;
                __instance.myPlayer.FootSteps.loop = true;
                __instance.myPlayer.FootSteps.Play();
            }
            __instance.ResetAnimState();
        }

        if ((int)ReplayManager.CurrentReplay.CurrentLadderState[__instance.myPlayer.PlayerId] >= (int)LadderState.WalkTo1st)
        {
            ReplayManager.CurrentReplay.CurrentLadderState[__instance.myPlayer.PlayerId] = LadderState.WalkTo1st;
            yield return __instance.WalkPlayerTo(source.transform.position, 0.001f);
            ((Behaviour)__instance.myPlayer.Collider).enabled = true;
            __instance.myPlayer.moveable = true;
            ((Behaviour)__instance.myPlayer.NetTransform).enabled = true;
            __instance.myPlayer.ForceKillTimerContinue = false;
            __instance.myPlayer.onLadder = false;
        }
        ReplayManager.CurrentReplay.CurrentLadderState[__instance.myPlayer.PlayerId] = LadderState.None;
        ReplayManager.CurrentReplay.CurrentLadder.Remove(__instance.myPlayer.PlayerId);
    }
    public static IEnumerator CoClimbLadderCustom(PlayerPhysics __instance, Ladder source, byte climbLadderSid)
    {
        if (!ReplayManager.CurrentReplay.CurrentLadderState.ContainsKey(__instance.myPlayer.PlayerId) || (int)ReplayManager.CurrentReplay.CurrentLadderState[__instance.myPlayer.PlayerId] <= (int)LadderState.Init)
        {
            ReplayManager.CurrentReplay.CurrentLadder.Add(__instance.myPlayer.PlayerId, source);
            ReplayManager.CurrentReplay.CurrentLadderState[__instance.myPlayer.PlayerId] = LadderState.Init;
            ((Behaviour)__instance.myPlayer.Collider).enabled = false;
            __instance.myPlayer.moveable = false;
            ((Behaviour)__instance.myPlayer.NetTransform).enabled = false;
            __instance.myPlayer.ForceKillTimerContinue = true;
            __instance.myPlayer.onLadder = true;
            if (__instance.myPlayer.AmOwner)
            {
                ((Behaviour)__instance.myPlayer.MyPhysics.inputHandler).enabled = true;
            }
        }
        if ((int)ReplayManager.CurrentReplay.CurrentLadderState[__instance.myPlayer.PlayerId] <= (int)LadderState.WalkTo1st)
        {
            ReplayManager.CurrentReplay.CurrentLadderState[__instance.myPlayer.PlayerId] = LadderState.WalkTo1st;
            yield return __instance.WalkPlayerTo(source.transform.position, 0.001f);
        }
        if ((int)ReplayManager.CurrentReplay.CurrentLadderState[__instance.myPlayer.PlayerId] <= (int)LadderState.WaitEffect1st)
        {
            ReplayManager.CurrentReplay.CurrentLadderState[__instance.myPlayer.PlayerId] = LadderState.WaitEffect1st;
            yield return Effects.Wait(0.1f);
            __instance.StartClimb(source.IsTop);
            if (Constants.ShouldPlaySfx() && PlayerControl.LocalPlayer == __instance.myPlayer)
            {
                __instance.myPlayer.FootSteps.clip = source.UseSound;
                __instance.myPlayer.FootSteps.loop = true;
                __instance.myPlayer.FootSteps.Play();
            }
        }
        if ((int)ReplayManager.CurrentReplay.CurrentLadderState[__instance.myPlayer.PlayerId] <= (int)LadderState.WalkTo2nd)
        {
            ReplayManager.CurrentReplay.CurrentLadderState[__instance.myPlayer.PlayerId] = LadderState.WalkTo2nd;
            yield return __instance.WalkPlayerTo(source.Destination.transform.position, 0.001f, (!source.IsTop) ? 1 : 2);
            __instance.myPlayer.SetPetPosition(((Component)__instance.myPlayer).transform.position);
            __instance.ResetAnimState();
        }
        if ((int)ReplayManager.CurrentReplay.CurrentLadderState[__instance.myPlayer.PlayerId] <= (int)LadderState.WaitEffect2nd)
        {
            ReplayManager.CurrentReplay.CurrentLadderState[__instance.myPlayer.PlayerId] = LadderState.WaitEffect2nd;
            yield return Effects.Wait(0.1f);
            ((Behaviour)__instance.myPlayer.Collider).enabled = true;
            __instance.myPlayer.moveable = true;
            ((Behaviour)__instance.myPlayer.NetTransform).enabled = true;
            __instance.myPlayer.ForceKillTimerContinue = false;
            __instance.myPlayer.onLadder = false;
        }
        ReplayManager.CurrentReplay.CurrentLadderState[__instance.myPlayer.PlayerId] = LadderState.None;
        ReplayManager.CurrentReplay.CurrentLadder.Remove(__instance.myPlayer.PlayerId);
    }
    //試合内でアクションがあったら実行するやつ
    public static ReplayActionClimbLadder Create(byte sourcePlayer, byte ladderid, byte cls)
    {
        ReplayActionClimbLadder action = new();
        if (!CheckAndCreate(action)) return null;
        //ここで初期化(コレは仮処理だから消してね)
        action.sourcePlayer = sourcePlayer;
        action.ladderid = ladderid;
        action.climbLadderSid = cls;
        return action;
    }
}