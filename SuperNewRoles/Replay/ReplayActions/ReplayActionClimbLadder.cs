using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SuperNewRoles.Replay.ReplayActions;
public class ReplayActionClimbLadder : ReplayAction
{
    public byte sourcePlayer;
    public byte ladderid;
    public byte climbLadderSid;
    public override void ReadReplayFile(BinaryReader reader) {
        ActionTime = reader.ReadSingle();
        //ここにパース処理書く
        sourcePlayer = reader.ReadByte();
        ladderid = reader.ReadByte();
        climbLadderSid = reader.ReadByte();
    }
    public override void WriteReplayFile(BinaryWriter writer)
    {
        writer.Write(ActionTime);
        //ここにパース処理書く
        writer.Write(sourcePlayer);
        writer.Write(ladderid);
        writer.Write(climbLadderSid);
    }
    public override ReplayActionId GetActionId() => ReplayActionId.ClimbLadder;
    //アクション実行時の処理
    public override void OnAction() {
        //ここに処理書く
        PlayerControl source = ModHelpers.PlayerById(sourcePlayer);
        if (source == null)
        {
            Logger.Info("sourceがnullだ");
            return;
        }
        Ladder ladder = ModHelpers.LadderById(ladderid);
        if (ladder == null)
        {
            Logger.Info("ladderがnullだ");
        }
        source.MyPhysics.ClimbLadder(ladder, climbLadderSid);
    }
    //試合内でアクションがあったら実行するやつ
    public static ReplayActionClimbLadder Create(byte sourcePlayer,byte ladderid, byte cls)
    {
        if (ReplayManager.IsReplayMode) return null;
        ReplayActionClimbLadder action = new();
        Recorder.ReplayActions.Add(action);
        //ここで秒数指定
        action.ActionTime = Recorder.ReplayActionTime;
        Recorder.ReplayActionTime = 0f;
        //ここで初期化(コレは仮処理だから消してね)
        action.sourcePlayer = sourcePlayer;
        action.ladderid = ladderid;
        action.climbLadderSid = cls;
        return action;
    }
}