using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace SuperNewRoles.Replay.ReplayActions;
public class ReplayActionMurder : ReplayAction
{
    public byte sourcePlayer;
    public byte targetPlayer;
    public override void ReadReplayFile(BinaryReader reader)
    {
        ActionTime = reader.ReadSingle();
        //ここにパース処理書く
        sourcePlayer = reader.ReadByte();
        targetPlayer = reader.ReadByte();
    }
    public override void WriteReplayFile(BinaryWriter writer)
    {
        writer.Write(ActionTime);
        //ここにパース処理書く
        writer.Write(sourcePlayer);
        writer.Write(targetPlayer);
    }
    public override ReplayActionId GetActionId() => ReplayActionId.MurderPlayer;
    //アクション実行時の処理
    public override void OnAction()
    {
        Logger.Info("キルアクション");
        PlayerControl source = ModHelpers.PlayerById(sourcePlayer);
        PlayerControl target = ModHelpers.PlayerById(targetPlayer);
        if (source == null || target == null)
        {
            Logger.Info($"アクションを実行しようとしましたが、対象がいませんでした。source:{sourcePlayer},target:{targetPlayer}");
            return;
        }
        source.MurderPlayer(target, MurderResultFlags.Succeeded | MurderResultFlags.DecisionByHost);
    }
    public override void OnReplay()
    {
        PlayerControl source = ModHelpers.PlayerById(sourcePlayer);
        PlayerControl target = ModHelpers.PlayerById(targetPlayer);
        if (source == null || target == null)
        {
            Logger.Info($"アクションを実行しようとしましたが、対象がいませんでした。source:{sourcePlayer},target:{targetPlayer}");
            return;
        }
        target.Revive();
        foreach (DeadBody deadbody in GameObject.FindObjectsOfType<DeadBody>())
            if (deadbody.ParentId == target.PlayerId)
                GameObject.Destroy(deadbody);
    }

    //試合内でアクションがあったら実行するやつ
    public static ReplayActionMurder Create(byte sourcePlayer, byte targetPlayer)
    {
        ReplayActionMurder action = new();
        if (!CheckAndCreate(action)) return null;
        //初期化
        action.sourcePlayer = sourcePlayer;
        action.targetPlayer = targetPlayer;
        return action;
    }
}