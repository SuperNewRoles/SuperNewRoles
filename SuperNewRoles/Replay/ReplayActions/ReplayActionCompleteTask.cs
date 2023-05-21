using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SuperNewRoles.Replay.ReplayActions;
public class ReplayActionCompleteTask : ReplayAction
{
    public byte sourcePlayer;
    public uint taskId;
    public override void ReadReplayFile(BinaryReader reader) {
        ActionTime = reader.ReadSingle();
        //ここにパース処理書く
        sourcePlayer = reader.ReadByte();
        taskId = reader.ReadUInt32();
    }
    public override void WriteReplayFile(BinaryWriter writer)
    {
        writer.Write(ActionTime);
        //ここにパース処理書く
        writer.Write(sourcePlayer);
        writer.Write(taskId);
    }
    public override ReplayActionId GetActionId() => ReplayActionId.CompleteTask;
    //アクション実行時の処理
    public override void OnAction() {
        //ここに処理書く
        PlayerControl source = ModHelpers.PlayerById(sourcePlayer);
        if (source == null)
        {
            Logger.Info("sourceがnull");
            return;
        }
        source.CompleteTask(taskId);
    }
    //試合内でアクションがあったら実行するやつ
    public static ReplayActionCompleteTask Create(byte sourcePlayer, uint taskId)
    {
        if (ReplayManager.IsReplayMode) return null;
        ReplayActionCompleteTask action = new();
        Recorder.ReplayActions.Add(action);
        //ここで秒数指定
        action.ActionTime = Recorder.ReplayActionTime;
        Recorder.ReplayActionTime = 0f;
        //ここで初期化(コレは仮処理だから消してね)
        action.sourcePlayer = sourcePlayer;
        action.taskId = taskId;
        return action;
    }
}