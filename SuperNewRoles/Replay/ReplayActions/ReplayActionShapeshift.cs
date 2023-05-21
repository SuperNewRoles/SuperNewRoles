using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SuperNewRoles.Replay.ReplayActions;
public class ReplayActionShapeshift : ReplayAction
{
    public byte sourcePlayer;
    public byte targetPlayer;
    public bool animate;
    public override void ReadReplayFile(BinaryReader reader) {
        ActionTime = reader.ReadSingle();
        //ここにパース処理書く
        sourcePlayer = reader.ReadByte();
        targetPlayer = reader.ReadByte();
        animate = reader.ReadBoolean();
    }
    public override void WriteReplayFile(BinaryWriter writer)
    {
        writer.Write(ActionTime);
        //ここにパース処理書く
        writer.Write(sourcePlayer);
        writer.Write(targetPlayer);
        writer.Write(animate);
    }
    public override ReplayActionId GetActionId() => ReplayActionId.Shapeshift;
    //アクション実行時の処理
    public override void OnAction() {
        //ここに処理書く
        PlayerControl source = ModHelpers.PlayerById(sourcePlayer);
        PlayerControl target = ModHelpers.PlayerById(targetPlayer);
        if (source == null || target == null)
        {
            Logger.Info("対象がnullでした。");
            return;
        }
        source.Shapeshift(target, animate);
    }
    //試合内でアクションがあったら実行するやつ
    public static ReplayActionShapeshift Create(byte sourcePlayer, byte targetPlayer, bool animate)
    {
        if (ReplayManager.IsReplayMode) return null;
        ReplayActionShapeshift action = new();
        Recorder.ReplayActions.Add(action);
        //ここで秒数指定
        action.ActionTime = Recorder.ReplayActionTime;
        Recorder.ReplayActionTime = 0f;
        //ここで初期化(コレは仮処理だから消してね)
        action.sourcePlayer = sourcePlayer;
        action.targetPlayer = targetPlayer;
        action.animate = animate;
        return action;
    }
}