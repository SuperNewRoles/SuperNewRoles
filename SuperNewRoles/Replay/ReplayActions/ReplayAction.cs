using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Hazel;

namespace SuperNewRoles.Replay.ReplayActions;

public enum ReplayActionId
{
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
    UpdateSystem,
    VotingComplete,
    MovingPlatform
}
public abstract class ReplayAction
{
    public float ActionTime = 0f;
    public int ReplayId;
    public static ReplayAction GetLastAction(ReplayAction action, Func<ReplayAction, bool> check = null)
    {
        ReplayAction currentaction = null;
        foreach (ReplayAction act in ReplayLoader.ReplayTurns[ReplayLoader.CurrentTurn].Actions.AsSpan())
        {
            if (act.GetActionId() == action.GetActionId() && (currentaction == null || (act.ReplayId > currentaction.ReplayId && action.ReplayId > currentaction.ReplayId)) && (check == null || check(act)))
                currentaction = act;
        }
        return currentaction;
    }
    public abstract void ReadReplayFile(BinaryReader reader);
    public abstract void WriteReplayFile(BinaryWriter writer);
    public abstract void OnAction();
    public virtual void OnReplay() { Logger.Info("Commedd!!!"); }
    public abstract ReplayActionId GetActionId();
    public static bool CheckAndCreate(ReplayAction action)
    {
        if (ReplayManager.IsReplayMode || !ReplayManager.IsRecording) return false;
        Recorder.ReplayActions.Add(action);
        //ここで秒数指定
        action.ActionTime = Recorder.ReplayActionTime;
        Recorder.ReplayActionTime = 0f;
        return true;
    }
    public static int MaxId = 0;
    public void Init()
    {
        ReplayId = MaxId;
        MaxId++;
    }
    public static void CoIntroDestory()
    {
        MaxId = 0;
    }
    public static ReplayAction CreateReplayAction(ReplayActionId id)
    {
        switch (id)
        {
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
            case ReplayActionId.UpdateSystem:
                return new ReplayActionUpdateSystem();
            case ReplayActionId.VotingComplete:
                return new ReplayActionVotingComplete();
            case ReplayActionId.MovingPlatform:
                return new ReplayActionMovingPlatform();
        }
        Logger.Info("typeに合うReplayActionがありませんでした。:" + id);
        return null;
    }
}