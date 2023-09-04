using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SuperNewRoles.Replay.ReplayActions;
public class ReplayActionExile : ReplayAction
{
    public byte exilePlayer;
    public override void ReadReplayFile(BinaryReader reader)
    {
        ActionTime = reader.ReadSingle();
        //ここにパース処理書く
        exilePlayer = reader.ReadByte();
    }
    public override void WriteReplayFile(BinaryWriter writer)
    {
        writer.Write(ActionTime);
        //ここにパース処理書く
        writer.Write(exilePlayer);
    }
    public override ReplayActionId GetActionId() => ReplayActionId.Exile;
    //アクション実行時の処理
    public override void OnAction()
    {
        PlayerControl exile = ModHelpers.PlayerById(exilePlayer);
        if (exile == null)
        {
            Logger.Info($"アクションを実行しようとしましたが、対象がいませんでした。exile:{exilePlayer}");
            return;
        }
        exile.Exiled();
    }
    public override void OnReplay()
    {
        PlayerControl exile = ModHelpers.PlayerById(exilePlayer);
        if (exile == null)
        {
            Logger.Info($"アクションを実行しようとしましたが、対象がいませんでした。exile:{exilePlayer}");
            return;
        }
        exile.Revive();
    }
    //試合内でアクションがあったら実行するやつ
    public static ReplayActionExile Create(byte exilePlayer)
    {
        ReplayActionExile action = new();
        if (!CheckAndCreate(action)) return null;
        //初期化
        action.exilePlayer = exilePlayer;
        return action;
    }
}