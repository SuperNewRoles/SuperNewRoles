using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SuperNewRoles.Replay.ReplayActions;
public class ReplayActionDisconnect : ReplayAction
{
    public byte sourcePlayer;
    public byte reason;
    public override void ReadReplayFile(BinaryReader reader)
    {
        ActionTime = reader.ReadSingle();
        //ここにパース処理書く
        Logger.Info("POS:" + reader.BaseStream.Position.ToString());
        sourcePlayer = reader.ReadByte();
        Logger.Info("POS2:" + reader.BaseStream.Position.ToString());
        reason = reader.ReadByte();
    }
    public override void WriteReplayFile(BinaryWriter writer)
    {
        writer.Write(ActionTime);
        //ここにパース処理書く
        writer.Write(sourcePlayer);
        writer.Write(reason);
    }
    public override ReplayActionId GetActionId() => ReplayActionId.Disconnect;
    //アクション実行時の処理
    public override void OnAction()
    {
        //ここに処理書く
        PlayerControl source = ModHelpers.PlayerById(sourcePlayer);
        if (source == null)
        {
            Logger.Info("えらー");
            return;
        }
        GameData.Instance.HandleDisconnect(source, (DisconnectReasons)reason);
    }
    //試合内でアクションがあったら実行するやつ
    public static ReplayActionDisconnect Create(byte sourcePlayer, DisconnectReasons reason)
    {
        ReplayActionDisconnect action = new();
        if (!CheckAndCreate(action)) return null;
        //ここで初期化(コレは仮処理だから消してね)
        action.sourcePlayer = sourcePlayer;
        action.reason = (byte)reason;
        return action;
    }
}