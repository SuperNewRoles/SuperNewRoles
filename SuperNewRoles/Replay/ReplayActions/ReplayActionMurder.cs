using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SuperNewRoles.Replay.ReplayActions;
public class ReplayActionMurder : ReplayAction
{
    public byte sourcePlayer;
    public byte targetPlayer;
    public override void ReadReplayFile(BinaryReader reader) {
        //ここにパース処理書く
    }
    //試合内でアクションがあったら実行するやつ
    public static ReplayActionMurder Create(byte sourcePlayer, byte targetPlayer)
    {
        ReplayActionMurder action = new();
        Recorder.ReplayActions.Add(action);
        //ここで秒数指定
        action.ActionTime = Recorder.ReplayActionTime;
        Recorder.ReplayActionTime = 0f;
        //初期化
        action.sourcePlayer = sourcePlayer;
        action.targetPlayer = targetPlayer;
        return action;
    }
}