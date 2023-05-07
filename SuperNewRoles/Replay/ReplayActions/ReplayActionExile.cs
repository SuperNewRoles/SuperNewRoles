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
        if (exile == null) {
            Logger.Info($"アクションを実行しようとしましたが、対象がいませんでした。exile:{exilePlayer}");
            return;
        }
        exile.Exiled();
    }
    //試合内でアクションがあったら実行するやつ
    public static ReplayActionExile Create(byte exilePlayer)
    {
        if (Recorder.IsReplayMode) return null;
        ReplayActionExile action = new();
        Recorder.ReplayActions.Add(action);
        //ここで秒数指定
        action.ActionTime = Recorder.ReplayActionTime;
        Recorder.ReplayActionTime = 0f;
        //初期化
        action.exilePlayer = exilePlayer;
        return action;
    }
}