using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SuperNewRoles.Replay.ReplayActions;
public class ReplayActionUpdateSystem : ReplayAction
{
    public byte systemType;
    public byte sourcePlayer;
    public byte amount;
    public override void ReadReplayFile(BinaryReader reader)
    {
        ActionTime = reader.ReadSingle();
        //ここにパース処理書く
        systemType = reader.ReadByte();
        sourcePlayer = reader.ReadByte();
        amount = reader.ReadByte();
    }
    public override void WriteReplayFile(BinaryWriter writer)
    {
        writer.Write(ActionTime);
        //ここにパース処理書く
        writer.Write(systemType);
        writer.Write(sourcePlayer);
        writer.Write(amount);
    }
    public override ReplayActionId GetActionId() => ReplayActionId.UpdateSystem;
    //アクション実行時の処理
    public override void OnAction()
    {
        //ここに処理書く
        PlayerControl source = ModHelpers.PlayerById(sourcePlayer);
        if (source == null)
        {
            Logger.Info("エラー");
            return;
        }
        ShipStatus.Instance.UpdateSystem((SystemTypes)systemType, source, amount);
    }
    //試合内でアクションがあったら実行するやつ
    public static ReplayActionUpdateSystem Create(SystemTypes systemType, byte sourcePlayer, byte amount)
    {
        ReplayActionUpdateSystem action = new();
        if (!CheckAndCreate(action)) return null;
        //ここで初期化(コレは仮処理だから消してね)
        action.systemType = (byte)systemType;
        action.sourcePlayer = sourcePlayer;
        action.amount = amount;
        return action;
    }
}