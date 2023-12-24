using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace SuperNewRoles.Replay.ReplayActions;
public class ReplayActionWavecannon : ReplayAction
{
    public byte Type;
    public byte Id;
    public bool IsFlipX;
    public byte OwnerId;
    public Vector3 position;
    public override void ReadReplayFile(BinaryReader reader)
    {
        ActionTime = reader.ReadSingle();
        //ここにパース処理書く
        Type = reader.ReadByte();
        Id = reader.ReadByte();
        IsFlipX = reader.ReadBoolean();
        OwnerId = reader.ReadByte();
        position = new Vector3(reader.ReadSingle(), reader.ReadSingle());
    }
    public override void WriteReplayFile(BinaryWriter writer)
    {
        writer.Write(ActionTime);
        //ここにパース処理書く
        writer.Write(Type);
        writer.Write(Id);
        writer.Write(IsFlipX);
        writer.Write(OwnerId);
        writer.Write(position.x);
        writer.Write(position.y);
    }
    public override ReplayActionId GetActionId() => ReplayActionId.Wavecannon;
    //アクション実行時の処理
    public override void OnAction()
    {
        //ここに処理書く
        //WaveCannon(Type, Id, IsFlipX, OwnerId, buff);
    }
    //試合内でアクションがあったら実行するやつ
    public static ReplayActionWavecannon Create(byte Type, byte Id, bool IsFlipX, byte OwnerId, Vector3 position)
    {
        ReplayActionWavecannon action = new();
        if (!CheckAndCreate(action)) return null;
        //ここで初期化(コレは仮処理だから消してね)
        action.Type = Type;
        action.Id = Id;
        action.IsFlipX = IsFlipX;
        action.OwnerId = OwnerId;
        action.position = position;
        return action;
    }
}