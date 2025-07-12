using System.Collections.Generic;
using System.Globalization;
using HarmonyLib;
using Hazel;
using InnerNet;
using SuperNewRoles.CustomOptions.Categories;
using UnityEngine;

// https://github.com/tukasa0001/TownOfHost/blob/main/Patches/ClientPatch.cs

namespace SuperNewRoles;

[HarmonyPatch]
class InnerNetClientPatch
{
    [HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.HandleMessage)), HarmonyPrefix]
    public static bool HandleMessagePatch(InnerNetClient __instance, MessageReader reader, SendOption sendOption)
    {
        /*
        if (DebugModeManager.IsDebugMode)
        {
            Logger.Info($"HandleMessagePatch:Packet({reader.Length}) ,SendOption:{sendOption}", "InnerNetClient");
        }
        else */
        if (reader.Length > 1000)
        {
            Logger.Info($"HandleMessagePatch:Large Packet({reader.Length})", "InnerNetClient");
        }
        return true;
    }
    static Dictionary<int, int> messageCount = new(10);
    const int warningThreshold = 100;
    static int peak = warningThreshold;
    static float timer = 0f;
    [HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.FixedUpdate)), HarmonyPrefix]
    public static void FixedUpdatePatch(InnerNetClient __instance)
    {
        int last = (int)timer % 10;
        timer += Time.fixedDeltaTime;
        int current = (int)timer % 10;
        if (last != current)
        {
            messageCount[current] = 0;
        }
    }

    [HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.SendOrDisconnect)), HarmonyPrefix]
    public static bool SendOrDisconnectPatch(InnerNetClient __instance, MessageWriter msg)
    {
        //分割するサイズ。大きすぎるとリトライ時不利、小さすぎると受信パケット取りこぼしが発生しうる。
        //Vanila側で500byteで分割しているため競合を避け1000byteに設定
        var limitSize = 1000;
        /*
                if (DebugModeManager.IsDebugMode)
                {
                    Logger.Info($"SendOrDisconnectPatch:Packet({msg.Length}) ,SendOption:{msg.SendOption}", "InnerNetClient");
                }
                else */
        if (msg.Length > limitSize)
        {
            Logger.Info($"SendOrDisconnectPatch:Large Packet({msg.Length})", "InnerNetClient");
        }
        //メッセージピークのログ出力
        if (msg.SendOption == SendOption.Reliable)
        {
            int last = (int)timer % 10;
            messageCount[last]++;
            int totalMessages = 0;
            foreach (var count in messageCount.Values)
            {
                totalMessages += count;
            }
            if (totalMessages > warningThreshold)
            {
                if (peak > totalMessages)
                {
                    Logger.Warning($"SendOrDisconnectPatch:Packet Spam Detected ({peak})", "InnerNetClient");
                    peak = warningThreshold;
                }
                else
                {
                    peak = totalMessages;
                }
            }
        }
        if (!GeneralSettingOptions.FixSpawnPacketSize) return true;

        //ラージパケットを分割(9人以上部屋で落ちる現象の対策コード)

        //メッセージが大きすぎる場合は分割して送信を試みる
        if (msg.Length > limitSize)
        {
            var writer = MessageWriter.Get(msg.SendOption);
            var reader = MessageReader.Get(msg.ToByteArray(false));

            //Tagレベルの処理
            while (reader.Position < reader.Length)
            {
                //Logger.Info($"SendOrDisconnectPatch:reader {reader.Position} / {reader.Length}", "InnerNetClient");

                var partMsg = reader.ReadMessage();
                var tag = partMsg.Tag;

                //Logger.Info($"SendOrDisconnectPatch:partMsg Tag={tag} Length={partMsg.Length}", "InnerNetClient");

                //TagがGameData,GameDataToの場合のみ分割処理
                //それ以外では多分分割しなくても問題ない
                if (tag is 5 or 6 && partMsg.Length > limitSize)
                {
                    //分割を試みる
                    DivideLargeMessage(__instance, writer, partMsg);
                }
                else
                {
                    //そのまま追加
                    WriteMessage(writer, partMsg);
                }

                //送信サイズが制限を超えた場合は送信
                if (writer.Length > limitSize)
                {
                    Send(__instance, writer);
                    writer.Clear(writer.SendOption);
                }
            }

            //残りの送信
            if (writer.HasBytes(7))
            {
                Send(__instance, writer);
            }

            writer.Recycle();
            reader.Recycle();
            return false;
        }
        return true;
    }
    private static void DivideLargeMessage(InnerNetClient __instance, MessageWriter writer, MessageReader partMsg)
    {
        var tag = partMsg.Tag;
        var GameId = partMsg.ReadInt32();
        var ClientId = -1;

        //元と同じTagを開く
        writer.StartMessage(tag);
        writer.Write(GameId);
        if (tag == 6)
        {
            ClientId = partMsg.ReadPackedInt32();
            writer.WritePacked(ClientId);
        }

        //Flag単位の処理
        while (partMsg.Position < partMsg.Length)
        {
            var subMsg = partMsg.ReadMessage();
            var subLength = subMsg.Length;

            //加算すると制限を超える場合は先に送信
            if (writer.Length + subLength > 500)
            {
                writer.EndMessage();
                Send(__instance, writer);
                //再度Tagを開く
                writer.Clear(writer.SendOption);
                writer.StartMessage(tag);
                writer.Write(GameId);
                if (tag == 6)
                {
                    writer.WritePacked(ClientId);
                }
            }
            //メッセージの出力
            WriteMessage(writer, subMsg);
        }
        writer.EndMessage();
    }

    private static void WriteMessage(MessageWriter writer, MessageReader reader)
    {
        writer.Write((ushort)reader.Length);
        writer.Write(reader.Tag);
        writer.Write(reader.ReadBytes(reader.Length));
    }

    private static void Send(InnerNetClient __instance, MessageWriter writer)
    {
        Logger.Info($"SendOrDisconnectPatch: SendMessage Length={writer.Length}", "InnerNetClient");
        var err = __instance.connection.Send(writer);
        if (err != SendErrors.None)
        {
            Logger.Info($"SendOrDisconnectPatch: SendMessage Error={err}", "InnerNetClient");
        }
    }
}