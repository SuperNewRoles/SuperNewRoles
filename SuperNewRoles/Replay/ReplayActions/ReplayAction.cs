using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Hazel;

namespace SuperNewRoles.Replay.ReplayActions;

public enum ReplayActionId {
    MurderPlayer
}
public abstract class ReplayAction
{
    public float ActionTime = 0f;
    public abstract void ReadReplayFile(BinaryReader reader);
    public abstract void OnAction();
    public static ReplayAction CreateReplayAction(ReplayActionId id)
    {
        switch (id) {
            case ReplayActionId.MurderPlayer:
                return new ReplayActionMurder();
        }
        Logger.Info("typeに合うReplayActionがありませんでした。:"+id);
        return null;
    }
}