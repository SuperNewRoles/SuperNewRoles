﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HarmonyLib;
using InnerNet;
using Hazel;
using UnhollowerBaseLib;
//TOHの開発者さんたち(主に空き瓶さん)ありがとうございます

namespace SuperNewRoles
{
    public class CustomRpcSender
    {
        public MessageWriter stream;
        public SendOption sendOption;
        public bool isUnsafe;

        private State currentState = State.BeforeInit;

        private CustomRpcSender() { }
        public CustomRpcSender(SendOption sendOption, bool isUnsafe)
        {
            stream = MessageWriter.Get(sendOption);

            this.sendOption = sendOption;
            this.isUnsafe = isUnsafe;

            currentState = State.Ready;
        }
        public static CustomRpcSender Create(SendOption sendOption = SendOption.None, bool isUnsafe = false)
        {
            return new CustomRpcSender(sendOption, isUnsafe);
        }

        public CustomRpcSender StartRpc(
          uint targetNetId,
          RpcCalls rpcCall,
          int targetClientId = -1)
         => StartRpc(targetNetId, (byte)rpcCall, targetClientId);
        public CustomRpcSender StartRpc(
          uint targetNetId,
          byte callId,
          int targetClientId = -1)
        {
            if (currentState != State.Ready && !isUnsafe)
            {
                SuperNewRolesPlugin.Logger.LogError("RPCを開始しようとしましたが、StateがReady(準備完了)ではありません");
                return this;
            }

            if (targetClientId < 0)
            {
                // 全員に対するRPC
                stream.StartMessage(5);
                stream.Write(AmongUsClient.Instance.GameId);
            }
            else
            {
                // 特定のクライアントに対するRPC (Desync)
                stream.StartMessage(6);
                stream.Write(AmongUsClient.Instance.GameId);
                stream.WritePacked(targetClientId);
            }
            stream.StartMessage(2);
            stream.WritePacked(targetNetId);
            stream.Write(callId);

            currentState = State.Writing;
            return this;
        }
        public void EndRpc()
        {
            if (currentState != State.Writing && !isUnsafe)
            {
                SuperNewRolesPlugin.Logger.LogError("RPCを終了しようとしましたが、StateがWriting(書き込み中)ではありません");
                return;
            }

            stream.EndMessage();
            stream.EndMessage();
            currentState = State.Ready;
        }
        public void SendMessage()
        {
            if (currentState != State.Ready && !isUnsafe)
            {
                SuperNewRolesPlugin.Logger.LogError("RPCを終了しようとしましたが、StateがReady(準備完了)ではありません");
                return;
            }

            AmongUsClient.Instance.SendOrDisconnect(stream);
            currentState = State.Finished;
            stream.Recycle();
        }

        // Write
        public CustomRpcSender Write(MessageWriter msg, bool includeHeader) => Write(w => w.Write(msg, includeHeader));
        public CustomRpcSender Write(float val) => Write(w => w.Write(val));
        public CustomRpcSender Write(string val) => Write(w => w.Write(val));
        public CustomRpcSender Write(ulong val) => Write(w => w.Write(val));
        public CustomRpcSender Write(int val) => Write(w => w.Write(val));
        public CustomRpcSender Write(uint val) => Write(w => w.Write(val));
        public CustomRpcSender Write(ushort val) => Write(w => w.Write(val));
        public CustomRpcSender Write(byte val) => Write(w => w.Write(val));
        public CustomRpcSender Write(sbyte val) => Write(w => w.Write(val));
        public CustomRpcSender Write(bool val) => Write(w => w.Write(val));
        public CustomRpcSender Write(Il2CppStructArray<byte> bytes) => Write(w => w.Write(bytes));
        public CustomRpcSender Write(Il2CppStructArray<byte> bytes, int offset, int length) => Write(w => w.Write(bytes, offset, length));
        public CustomRpcSender WriteBytesAndSize(Il2CppStructArray<byte> bytes) => Write(w => w.WriteBytesAndSize(bytes));
        public CustomRpcSender WritePacked(int val) => Write(w => w.WritePacked(val));
        public CustomRpcSender WritePacked(uint val) => Write(w => w.WritePacked(val));
        public CustomRpcSender WriteNetObject(InnerNetObject obj) => Write(w => w.WriteNetObject(obj));

        private CustomRpcSender Write(Action<MessageWriter> action)
        {
            if (currentState != State.Writing && !isUnsafe)
                SuperNewRolesPlugin.Logger.LogError("RPCを書き込もうとしましたが、StateがWrite(書き込み中)ではありません");
            else
                action(stream);

            return this;
        }

        public enum State
        {
            BeforeInit = 0, //初期化前 何もできない
            Ready, //送信準備完了 StartRpcとSendMessageを実行可能
            Writing, //RPC書き込み中 WriteとEndRpcを実行可能
            Finished, //送信後 何もできない
        }
    }
}
