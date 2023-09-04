using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SuperNewRoles.Replay.ReplayActions;
public class ReplayActionBalancer : ReplayAction
{
    public byte sourcePlayer;
    public byte player1;
    public byte player2;
    public override void ReadReplayFile(BinaryReader reader)
    {
        ActionTime = reader.ReadSingle();
        //ここにパース処理書く
        sourcePlayer = reader.ReadByte();
        player1 = reader.ReadByte();
        player2 = reader.ReadByte();
    }
    public override void WriteReplayFile(BinaryWriter writer)
    {
        writer.Write(ActionTime);
        //ここにパース処理書く
        writer.Write(sourcePlayer);
        writer.Write(player1);
        writer.Write(player2);
    }
    public override ReplayActionId GetActionId() => ReplayActionId.Balancer;
    //アクション実行時の処理
    public override void OnAction()
    {
        //ここに処理書く
        RPCProcedure.BalancerBalance(sourcePlayer, player1, player2);
    }
    //試合内でアクションがあったら実行するやつ
    public static ReplayActionBalancer Create(byte sourcePlayer, byte player1, byte player2)
    {
        ReplayActionBalancer action = new();
        if (!CheckAndCreate(action)) return null;
        //ここで初期化(コレは仮処理だから消してね)
        action.sourcePlayer = sourcePlayer;
        action.player1 = player1;
        action.player2 = player2;
        return action;
    }
}