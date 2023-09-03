using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SuperNewRoles.Replay.ReplayActions;
public class ReplayActionAddChat : ReplayAction
{
    public byte sourcePlayer;
    public string chatText;
    public override void ReadReplayFile(BinaryReader reader)
    {
        ActionTime = reader.ReadSingle();
        //ここにパース処理書く
        sourcePlayer = reader.ReadByte();
        chatText = reader.ReadString();
        Logger.Info(chatText, "CHATTEXT");
    }
    public override void WriteReplayFile(BinaryWriter writer)
    {
        writer.Write(ActionTime);
        //ここにパース処理書く
        writer.Write(sourcePlayer);
        writer.Write(chatText);
    }
    public override ReplayActionId GetActionId() => ReplayActionId.AddChat;
    //アクション実行時の処理
    public override void OnAction()
    {
        //ここに処理書く
        PlayerControl source = ModHelpers.PlayerById(sourcePlayer);
        if (source == null)
        {
            Logger.Info("sourceがnullでした");
            return;
        }
        FastDestroyableSingleton<HudManager>.Instance.Chat.AddChat(source, chatText);
    }
    //試合内でアクションがあったら実行するやつ
    public static ReplayActionAddChat Create(byte sourcePlayer, string chatText)
    {
        ReplayActionAddChat action = new();
        if (!CheckAndCreate(action)) return null;
        //ここで初期化(コレは仮処理だから消してね)
        action.sourcePlayer = sourcePlayer;
        action.chatText = chatText;
        return action;
    }
}