using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SuperNewRoles.Patches;
using static GameData;

namespace SuperNewRoles.Replay.ReplayActions;
public class ReplayActionCompleteTask : ReplayAction
{
    public byte sourcePlayer;
    public uint taskId;
    public override void ReadReplayFile(BinaryReader reader)
    {
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
    public static void ToDontComplete(PlayerControl player, uint idx)
    {
        PlayerTask playerTask = player.myTasks.Find((Il2CppSystem.Predicate<PlayerTask>)((PlayerTask p) => p.Id == idx));
        if (playerTask)
        {
            TaskInfo taskInfo = player.Data.FindTaskById(idx);
            if (taskInfo != null)
            {
                if (taskInfo.Complete)
                {
                    taskInfo.Complete = false;
                    GameData.Instance.CompletedTasks--;
                }
            }
            if (playerTask is NormalPlayerTask)
            {
                (playerTask as NormalPlayerTask).taskStep = 0;
            }
        }
    }
    public override void OnAction()
    {
        //ここに処理書く
        PlayerControl source = ModHelpers.PlayerById(sourcePlayer);
        if (source == null)
        {
            Logger.Info("sourceがnull");
            return;
        }
        source.CompleteTask(taskId);
    }
    public override void OnReplay()
    {
        PlayerControl source = ModHelpers.PlayerById(sourcePlayer);
        if (source == null)
        {
            Logger.Info("sourceがnull");
            return;
        }
        ToDontComplete(source, taskId);
    }
    //試合内でアクションがあったら実行するやつ
    public static ReplayActionCompleteTask Create(byte sourcePlayer, uint taskId)
    {
        ReplayActionCompleteTask action = new();
        if (!CheckAndCreate(action)) return null;
        //ここで初期化(コレは仮処理だから消してね)
        action.sourcePlayer = sourcePlayer;
        action.taskId = taskId;
        return action;
    }
}