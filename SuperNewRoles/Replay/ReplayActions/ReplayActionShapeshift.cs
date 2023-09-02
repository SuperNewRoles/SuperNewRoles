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
    public override void ReadReplayFile(BinaryReader reader)
    {
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
    public override void OnAction()
    {
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
    public override void OnReplay()
    {
        PlayerControl source = ModHelpers.PlayerById(sourcePlayer);
        PlayerControl target = ModHelpers.PlayerById(targetPlayer);
        if (source == null || target == null)
        {
            Logger.Info("対象がnullでした。");
            return;
        }
        ReplayAction action = GetLastAction(this, ((ReplayAction act) => (act as ReplayActionShapeshift).sourcePlayer == this.sourcePlayer));
        if (action == null)
        {
            source.Shapeshift(source, false);
        }
        else
        {
            ReplayActionShapeshift shape = action as ReplayActionShapeshift;
            PlayerControl targetlast = ModHelpers.PlayerById(shape.targetPlayer);
            if (targetlast == null)
            {
                Logger.Info("対象がnullでした。2");
                return;
            }
            source.Shapeshift(targetlast, !shape.animate);
        }
    }
    //試合内でアクションがあったら実行するやつ
    public static ReplayActionShapeshift Create(byte sourcePlayer, byte targetPlayer, bool animate)
    {
        ReplayActionShapeshift action = new();
        if (!CheckAndCreate(action)) return null;
        //ここで初期化(コレは仮処理だから消してね)
        action.sourcePlayer = sourcePlayer;
        action.targetPlayer = targetPlayer;
        action.animate = animate;
        return action;
    }
}