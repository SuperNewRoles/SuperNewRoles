using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SuperNewRoles.Replay.ReplayActions;
public class ReplayActionVotingComplete : ReplayAction
{
    public MeetingHud.VoterState[] States;
    public byte exilePlayer;
    public bool tie;
    public override void ReadReplayFile(BinaryReader reader)
    {
        ActionTime = reader.ReadSingle();
        //ここにパース処理書く
        int count = reader.ReadInt32();
        List<MeetingHud.VoterState> states = new();
        for (int i = 0; i < count; i++)
        {
            MeetingHud.VoterState state = new()
            {
                VoterId = reader.ReadByte(),
                VotedForId = reader.ReadByte()
            };
            states.Add(state);
        }
        States = states.ToArray();
        exilePlayer = reader.ReadByte();
        tie = reader.ReadBoolean();
    }
    public override void WriteReplayFile(BinaryWriter writer)
    {
        writer.Write(ActionTime);
        //ここにパース処理書く
        writer.Write(States.Length);
        foreach (MeetingHud.VoterState state in States)
        {
            writer.Write(state.VoterId);
            writer.Write(state.VotedForId);
        }
        writer.Write(exilePlayer);
        writer.Write(tie);
    }
    public override ReplayActionId GetActionId() => ReplayActionId.VotingComplete;
    //アクション実行時の処理
    public override void OnAction()
    {
        //ここに処理書く
        NetworkedPlayerInfo exile = GameData.Instance.GetPlayerById(exilePlayer);
        MeetingHud.Instance.VotingComplete(States, exile, tie);
    }
    //試合内でアクションがあったら実行するやつ
    public static ReplayActionVotingComplete Create(MeetingHud.VoterState[] states, byte exilePlayer, bool tie)
    {
        ReplayActionVotingComplete action = new();
        if (!CheckAndCreate(action)) return null;
        //ここで初期化(コレは仮処理だから消してね)
        action.States = states;
        action.exilePlayer = exilePlayer;
        action.tie = tie;
        return action;
    }
}