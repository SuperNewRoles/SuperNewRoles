using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SuperNewRoles.Replay.ReplayActions;
public class ReplayActionMakeVent : ReplayAction
{
    public byte id;
    public float x;
    public float y;
    public float z;
    public bool chain;
    public override void ReadReplayFile(BinaryReader reader)
    {
        ActionTime = reader.ReadSingle();
        //ここにパース処理書く
        id = reader.ReadByte();
        x = reader.ReadSingle();
        y = reader.ReadSingle();
        z = reader.ReadSingle();
        chain = reader.ReadBoolean();
    }
    public override void WriteReplayFile(BinaryWriter writer)
    {
        writer.Write(ActionTime);
        //ここにパース処理書く
        writer.Write(id);
        writer.Write(x);
        writer.Write(y);
        writer.Write(z);
        writer.Write(chain);
    }
    public override ReplayActionId GetActionId() => ReplayActionId.MakeVent;
    //アクション実行時の処理
    public override void OnAction()
    {
        //ここに処理書く
        currentId = RPCProcedure.MakeVent(id, x, y, z, chain).Id;
    }
    public int currentId;
    public override void OnReplay()
    {
        Vent vent = ShipStatus.Instance.AllVents.FirstOrDefault(x => x.Id == currentId);
        if (vent != null)
            GameObject.Destroy(vent);
    }
    //試合内でアクションがあったら実行するやつ
    public static ReplayActionMakeVent Create(byte id, float x, float y, float z, bool chain)
    {
        ReplayActionMakeVent action = new();
        if (!CheckAndCreate(action)) return null;
        //ここで初期化(コレは仮処理だから消してね)
        action.id = id;
        action.x = x;
        action.y = y;
        action.z = z;
        action.chain = chain;
        return action;
    }
}