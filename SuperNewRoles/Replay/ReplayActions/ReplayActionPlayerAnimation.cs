using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SuperNewRoles.Replay.ReplayActions;
public class ReplayActionPlayerAnimation : ReplayAction
{
    public byte sourcePlayer;
    public byte type;
    public override void ReadReplayFile(BinaryReader reader) {
        ActionTime = reader.ReadSingle();
        //ここにパース処理書く
        sourcePlayer = reader.ReadByte();
        type = reader.ReadByte();
    }
    public override void WriteReplayFile(BinaryWriter writer)
    {
        writer.Write(ActionTime);
        //ここにパース処理書く
        writer.Write(sourcePlayer);
        writer.Write(type);
    }
    public override ReplayActionId GetActionId() => ReplayActionId.PlayerAnimation;
    //アクション実行時の処理
    public override void OnAction() {
        //ここに処理書く
        RPCProcedure.PlayPlayerAnimation(sourcePlayer, type);
    }
    //試合内でアクションがあったら実行するやつ
    public static ReplayActionPlayerAnimation Create(byte sourcePlayer, byte type)
    {
        ReplayActionPlayerAnimation action = new();
        if (!CheckAndCreate(action)) return null;
        //ここで初期化(コレは仮処理だから消してね)
        action.sourcePlayer = sourcePlayer;
        action.type = type;
        return action;
    }
}