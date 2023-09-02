using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace SuperNewRoles.Replay.ReplayActions;
public class ReplayActionVent : ReplayAction
{
    public byte sourcePlayer;
    public int id;
    public bool isEnter;
    public override void ReadReplayFile(BinaryReader reader)
    {
        ActionTime = reader.ReadSingle();
        //ここにパース処理書く
        sourcePlayer = reader.ReadByte();
        id = reader.ReadInt32();
        isEnter = reader.ReadBoolean();
    }
    public override void WriteReplayFile(BinaryWriter writer)
    {
        writer.Write(ActionTime);
        //ここにパース処理書く
        writer.Write(sourcePlayer);
        writer.Write(id);
        writer.Write(isEnter);
    }
    public override ReplayActionId GetActionId() => ReplayActionId.Vent;
    //アクション実行時の処理
    public override void OnAction()
    {
        //ここに処理書く
        PlayerControl source = ModHelpers.PlayerById(sourcePlayer);
        if (source == null)
        {
            Logger.Info("sourceがnullだったで");
            return;
        }
        if (isEnter)
        {
            ((MonoBehaviour)source.MyPhysics).StopAllCoroutines();
            ((MonoBehaviour)source.MyPhysics).StartCoroutine(source.MyPhysics.CoEnterVent(id));
        }
        else
        {
            ((MonoBehaviour)source.MyPhysics).StopAllCoroutines();
            ((MonoBehaviour)source.MyPhysics).StartCoroutine(source.MyPhysics.CoExitVent(id));
        }
    }
    public override void OnReplay()
    {
        PlayerControl source = ModHelpers.PlayerById(sourcePlayer);
        if (source == null)
        {
            Logger.Info("sourceがnullだったで");
            return;
        }
        if (!isEnter)
        {
            ((MonoBehaviour)source.MyPhysics).StopAllCoroutines();
            ((MonoBehaviour)source.MyPhysics).StartCoroutine(source.MyPhysics.CoEnterVent(id));
        }
        else
        {
            ((MonoBehaviour)source.MyPhysics).StopAllCoroutines();
            ((MonoBehaviour)source.MyPhysics).StartCoroutine(source.MyPhysics.CoExitVent(id));
        }
    }
    //試合内でアクションがあったら実行するやつ
    public static ReplayActionVent Create(byte sourcePlayer, int id, bool isEnter)
    {
        ReplayActionVent action = new();
        if (!CheckAndCreate(action)) return null;
        //ここで初期化(コレは仮処理だから消してね)
        action.sourcePlayer = sourcePlayer;
        action.id = id;
        action.isEnter = isEnter;
        return action;
    }
}