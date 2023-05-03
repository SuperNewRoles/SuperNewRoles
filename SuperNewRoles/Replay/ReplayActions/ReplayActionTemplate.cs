using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SuperNewRoles.Replay.ReplayActions;
public class ReplayActionTemplate : ReplayAction
{
    public byte sourcePlayer;
    public override void ReadReplayFile(BinaryReader reader) {
        ActionTime = reader.ReadSingle();
        //ここにパース処理書く
    }
    //試合内でアクションがあったら実行するやつ
    public static ReplayActionTemplate Create(byte sourcePlayer)
    {
        ReplayActionTemplate action = new();
        Recorder.ReplayActions.Add(action);
        //ここで秒数指定
        action.ActionTime = Recorder.ReplayActionTime;
        Recorder.ReplayActionTime = 0f;
        //ここで初期化(コレは仮処理だから消してね)
        action.sourcePlayer = sourcePlayer;
        return action;
    }
}