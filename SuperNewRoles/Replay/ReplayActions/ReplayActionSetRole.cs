using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SuperNewRoles.Replay.ReplayActions;
public class ReplayActionSetRole : ReplayAction
{
    public byte sourcePlayer;
    public RoleId RoleId;
    public override void ReadReplayFile(BinaryReader reader)
    {
        ActionTime = reader.ReadSingle();
        //ここにパース処理書く
        sourcePlayer = reader.ReadByte();
        RoleId = (RoleId)reader.ReadByte();
    }
    public override void WriteReplayFile(BinaryWriter writer)
    {
        writer.Write(ActionTime);
        //ここにパース処理書く
        writer.Write(sourcePlayer);
        writer.Write((byte)RoleId);
    }
    public override ReplayActionId GetActionId() => ReplayActionId.SetRole;
    //アクション実行時の処理
    public override void OnAction()
    {
        //ここに処理書く
        PlayerControl target = ModHelpers.PlayerById(sourcePlayer);
        if (target == null)
        {
            Logger.Info($"アクションを実行しようとしましたが、対象がいませんでした。source:{sourcePlayer},target:{RoleId}");
            return;
        }
        target.SetRole(RoleId);
    }
    public override void OnReplay()
    {
        PlayerControl target = ModHelpers.PlayerById(sourcePlayer);
        if (target == null)
        {
            Logger.Info($"アクションを実行しようとしましたが、対象がいませんでした。source:{sourcePlayer},target:{RoleId}");
            return;
        }
        ReplayAction action = GetLastAction(this, (ReplayAction act) => (act as ReplayActionSetRole).sourcePlayer == sourcePlayer);
        if (action == null)
        {
            target.SetRole(RoleId.DefaultRole);
        }
        else
        {
            ReplayActionSetRole rasr = action as ReplayActionSetRole;
            target.SetRole(rasr.RoleId);
        }
    }
    //試合内でアクションがあったら実行するやつ
    public static ReplayActionSetRole Create(byte sourcePlayer, RoleId roleId)
    {
        ReplayActionSetRole action = new();
        if (!CheckAndCreate(action)) return null;
        //ここで初期化(コレは仮処理だから消してね)
        action.sourcePlayer = sourcePlayer;
        action.RoleId = roleId;
        return action;
    }
}