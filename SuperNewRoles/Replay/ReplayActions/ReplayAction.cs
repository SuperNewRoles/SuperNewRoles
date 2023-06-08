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
    public abstract ReplayActionId GetActionId();
    public static bool CheckAndCreate(ReplayAction action)
    {
        if (ReplayManager.IsReplayMode) return false;
        Logger.Info(action.GetActionId().ToString(),"CheckAndCreate");
        Recorder.ReplayActions.Add(action);
        //ここで秒数指定
        action.ActionTime = Recorder.ReplayActionTime;
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
            case ReplayActionId.SetRole:
                return new ReplayActionSetRole();
            case ReplayActionId.SetCosmetics:
                return new ReplayActionSetCosmetics();
            case ReplayActionId.Shapeshift:
                return new ReplayActionShapeshift();
            case ReplayActionId.AddChat:
                return new ReplayActionAddChat();
            case ReplayActionId.Vent:
                return new ReplayActionVent();
            case ReplayActionId.ClimbLadder:
                return new ReplayActionClimbLadder();
            case ReplayActionId.CompleteTask:
                return new ReplayActionCompleteTask();
            case ReplayActionId.Wavecannon:
                return new ReplayActionWavecannon();
            case ReplayActionId.SluggerExile:
                return new ReplayActionSluggerExile();
            case ReplayActionId.PlayerAnimation:
                return new ReplayActionPlayerAnimation();
            case ReplayActionId.ReportDeadBody:
                return new ReplayActionReportDeadBody();
            case ReplayActionId.Balancer:
                return new ReplayActionBalancer();
            case ReplayActionId.Disconnect:
                return new ReplayActionDisconnect();
            case ReplayActionId.MakeVent:
                return new ReplayActionMakeVent();
            case ReplayActionId.SetMechanicStatus:
                return new ReplayActionSetMechanicStatus();
            case ReplayActionId.RepairSystem:
                return new ReplayActionRepairSystem();
            case ReplayActionId.VotingComplete:
                return new ReplayActionVotingComplete();
        }
        Logger.Info("typeに合うReplayActionがありませんでした。:"+id);
        return null;
    }
}