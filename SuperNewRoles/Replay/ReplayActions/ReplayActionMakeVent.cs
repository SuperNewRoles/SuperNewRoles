using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SuperNewRoles.Replay.ReplayActions;
public class ReplayActionMakeVent : ReplayAction
{
    public float x;
    public float y;
    public float z;
    public override void ReadReplayFile(BinaryReader reader) {
        ActionTime = reader.ReadSingle();
        //ここにパース処理書く
        x = reader.ReadSingle();
        y = reader.ReadSingle();
        z = reader.ReadSingle();
    }
    public override void WriteReplayFile(BinaryWriter writer)
    {
        writer.Write(ActionTime);
        //ここにパース処理書く
        writer.Write(x);
        writer.Write(y);
        writer.Write(z);
    }
    public override ReplayActionId GetActionId() => ReplayActionId.MakeVent;
    //アクション実行時の処理
    public override void OnAction() {
        //ここに処理書く
        RPCProcedure.MakeVent(x,y,z);
    }
    //試合内でアクションがあったら実行するやつ
    public static ReplayActionMakeVent Create(float x,float y,float z)
    {
        ReplayActionMakeVent action = new();
        if (!CheckAndCreate(action)) return null;
        //ここで初期化(コレは仮処理だから消してね)
        action.x = x;
        action.y = y;
        action.z = z;
        return action;
    }
}