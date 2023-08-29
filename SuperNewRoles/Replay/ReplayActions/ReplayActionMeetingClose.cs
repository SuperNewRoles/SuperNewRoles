using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SuperNewRoles.Replay.ReplayActions;
public class ReplayActionMeetingClose : ReplayAction
{
    public override void ReadReplayFile(BinaryReader reader) {
        ActionTime = reader.ReadSingle();
        //ここにパース処理書く
    }
    public override void WriteReplayFile(BinaryWriter writer)
    {
        writer.Write(ActionTime);
        //ここにパース処理書く
    }
    public override ReplayActionId GetActionId() => ReplayActionId.MeetingClose;
    //アクション実行時の処理
    public override void OnAction() {
        //ここに処理書く
        if (MeetingHud.Instance != null)
            MeetingHud.Instance.Close();
    }
    //試合内でアクションがあったら実行するやつ
    public static ReplayActionMeetingClose Create()
    {
        ReplayActionMeetingClose action = new();
        if (!CheckAndCreate(action)) return null;
        //ここで初期化(コレは仮処理だから消してね)
        return action;
    }
}