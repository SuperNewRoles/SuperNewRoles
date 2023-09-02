using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SuperNewRoles.Replay.ReplayActions;
public class ReplayActionSetMechanicStatus : ReplayAction
{
    public byte sourcePlayer;
    public byte targetVent;
    public bool Is;
    public byte[] buff;
    public override void ReadReplayFile(BinaryReader reader)
    {
        ActionTime = reader.ReadSingle();
        //ここにパース処理書く
        sourcePlayer = reader.ReadByte();
        targetVent = reader.ReadByte();
        Is = reader.ReadBoolean();
        buff = reader.ReadBytes(reader.ReadInt32());
    }
    public override void WriteReplayFile(BinaryWriter writer)
    {
        writer.Write(ActionTime);
        //ここにパース処理書く
        writer.Write(sourcePlayer);
        writer.Write(targetVent);
        writer.Write(Is);
        writer.Write(buff.Length);
        writer.Write(buff);
    }
    public override ReplayActionId GetActionId() => ReplayActionId.SetMechanicStatus;
    //アクション実行時の処理
    public override void OnAction()
    {
        //ここに処理書く
        RPCProcedure.SetVentStatusMechanic(sourcePlayer, targetVent, Is, buff);
    }
    public override void OnReplay()
    {
        //ここに処理書く
        RPCProcedure.SetVentStatusMechanic(sourcePlayer, targetVent, !Is, buff);
    }
    //試合内でアクションがあったら実行するやつ
    public static ReplayActionSetMechanicStatus Create(byte sourcePlayer, byte targetPlayer, bool Is, byte[] buff)
    {
        ReplayActionSetMechanicStatus action = new();
        if (!CheckAndCreate(action)) return null;
        //ここで初期化(コレは仮処理だから消してね)
        action.sourcePlayer = sourcePlayer;
        action.targetVent = targetPlayer;
        action.Is = Is;
        action.buff = buff;
        return action;
    }
}