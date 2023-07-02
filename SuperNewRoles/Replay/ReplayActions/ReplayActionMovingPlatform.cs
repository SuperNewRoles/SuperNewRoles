using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SuperNewRoles.Replay.ReplayActions;
public class ReplayActionMovingPlatform : ReplayAction
{
    public byte sourcePlayer;
    public override void ReadReplayFile(BinaryReader reader) {
        ActionTime = reader.ReadSingle();
        //ここにパース処理書く
        sourcePlayer = reader.ReadByte();
    }
    public override void WriteReplayFile(BinaryWriter writer)
    {
        writer.Write(ActionTime);
        //ここにパース処理書く
        writer.Write(sourcePlayer);
    }
    public override ReplayActionId GetActionId() => ReplayActionId.MovingPlatform;
    //アクション実行時の処理
    public override void OnAction() {
        //ここに処理書く
        ShipStatus.Instance.TryCast<AirshipStatus>().GapPlatform.Use(ModHelpers.PlayerById(sourcePlayer));
    }
    //試合内でアクションがあったら実行するやつ
    public static ReplayActionMovingPlatform Create(byte sourcePlayer)
    {
        ReplayActionMovingPlatform action = new();
        if (!CheckAndCreate(action)) return null;
        //ここで初期化(コレは仮処理だから消してね)
        action.sourcePlayer = sourcePlayer;
        return action;
    }
}