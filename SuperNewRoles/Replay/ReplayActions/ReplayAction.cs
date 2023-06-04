using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Hazel;

namespace SuperNewRoles.Replay.ReplayActions;

public enum ReplayActionId {
    None,
    MurderPlayer,
    Exile,
    SetRole,
    SetCosmetics,
    Shapeshift,
    AddChat,
    Vent,
    ClimbLadder,
    CompleteTask,
    Wavecannon,
    SluggerExile,
    PlayerAnimation,
    ReportDeadBody,
    Balancer,
    Disconnect,
    MakeVent,
    SetMechanicStatus,
    RepairSystem,
    VotingComplete
}
public abstract class ReplayAction
{
    public float ActionTime = 0f;
    public abstract void ReadReplayFile(BinaryReader reader);
    public abstract void WriteReplayFile(BinaryWriter writer);
    public abstract void OnAction();
    public virtual ReplayActionId GetActionId() => ReplayActionId.None;
    public static bool CheckAndCreate(ReplayAction action)
    {
        if (!ReplayManager.IsReplayMode) return false;
        Recorder.ReplayActions.Add(action);
        //ここで秒数指定
        float actiontime = Recorder.ReplayActionTime;
        Recorder.ReplayActionTime = 0f;
        return true;
    }
    public static ReplayAction CreateReplayAction(ReplayActionId id)
    {
        switch (id) {
            case ReplayActionId.MurderPlayer:
                return new ReplayActionMurder();
            case ReplayActionId.Exile:
                return new ReplayActionExile();
        }
        Logger.Info("typeに合うReplayActionがありませんでした。:"+id);
        return null;
    }
}