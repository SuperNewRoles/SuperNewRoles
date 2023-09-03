using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SuperNewRoles.CustomObject;
using UnityEngine;

namespace SuperNewRoles.Replay.ReplayActions;
public class ReplayActionSluggerExile : ReplayAction
{
    public byte sourcePlayer;
    public byte[] targets;
    public override void ReadReplayFile(BinaryReader reader)
    {
        ActionTime = reader.ReadSingle();
        //ここにパース処理書く
        sourcePlayer = reader.ReadByte();
        targets = reader.ReadBytes(reader.ReadInt32());
    }
    public override void WriteReplayFile(BinaryWriter writer)
    {
        writer.Write(ActionTime);
        //ここにパース処理書く
        writer.Write(sourcePlayer);
        writer.Write(targets.Length);
        writer.Write(targets);
    }
    public override ReplayActionId GetActionId() => ReplayActionId.SluggerExile;
    //アクション実行時の処理
    public override void OnAction()
    {
        //ここに処理書く
        RPCProcedure.SluggerExile(sourcePlayer, targets.ToList());
    }
    public override void OnReplay()
    {
        foreach (SluggerDeadbody db in SluggerDeadbody.DeadBodys.FindAll(x => x.PlayerId == sourcePlayer))
        {
            GameObject.Destroy(db.gameObject);
        }
    }
    //試合内でアクションがあったら実行するやつ
    public static ReplayActionSluggerExile Create(byte sourcePlayer, List<byte> targets)
    {
        ReplayActionSluggerExile action = new();
        if (!CheckAndCreate(action)) return null;
        //ここで初期化(コレは仮処理だから消してね)
        action.sourcePlayer = sourcePlayer;
        action.targets = targets.ToArray();
        return action;
    }
}