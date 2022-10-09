using System;
using Hazel;
using InnerNet;
using UnhollowerBaseLib;
//TOHの開発者さんたち(主に空き瓶さん)ありがとうございます

namespace SuperNewRoles.Modules
{
    public class CustomRpcSender
    {
        public MessageWriter stream;
        public readonly string name;
        public readonly SendOption sendOption;
        public bool isUnsafe;
        public delegate void onSendDelegateType();
        public onSendDelegateType onSendDelegate;

        private State currentState = State.Ready;

        //0~: tarGetClientId (GameDataTo)
        //-1: 全プレイヤー (GameData)
        //-2: 未設定
        private int currentRpcTarget;

        private CustomRpcSender() { }
        public CustomRpcSender(string name, SendOption sendOption, bool isUnsafe)
        {
            this.stream = MessageWriter.Get(sendOption);

            this.name = name;
            this.sendOption = sendOption;
            this.isUnsafe = isUnsafe;
            this.currentRpcTarget = -2;
            this.onSendDelegate = () => Logger.Info($"{this.name}'s onSendDelegate =>", "CustomRpcSender");

            this.currentState = State.Ready;
            Logger.Info($"\"{name}\" is ready", "CustomRpcSender");
        }
        public static CustomRpcSender Create(string name = "No Name Sender", SendOption sendOption = SendOption.None, bool isUnsafe = false)
        {
            return new CustomRpcSender(name, sendOption, isUnsafe);
        }

        #region Start/End Message
        public CustomRpcSender StartMessage(int tarGetClientId = -1)
        {
            if (this.currentState != State.Ready)
            {
                string errorMsg = $"Messageを開始しようとしましたが、StateがReadyではありません (in: \"{this.name}\") (State: \"{this.currentState}\")";
                if (this.isUnsafe)
                {
                    Logger.Warn(errorMsg, "CustomRpcSender.Warn");
                }
                else
                {
                    throw new InvalidOperationException(errorMsg);
                }
            }

            if (tarGetClientId < 0)
            {
                // 全員に対するRPC
                this.stream.StartMessage(5);
                this.stream.Write(AmongUsClient.Instance.GameId);
            }
            else
            {
                // 特定のクライアントに対するRPC (Desync)
                this.stream.StartMessage(6);
                this.stream.Write(AmongUsClient.Instance.GameId);
                this.stream.WritePacked(tarGetClientId);
            }

            this.currentRpcTarget = tarGetClientId;
            this.currentState = State.InRootMessage;
            return this;
        }
        public CustomRpcSender EndMessage()
        {
            if (this.currentState != State.InRootMessage)
            {
                string errorMsg = $"Messageを終了しようとしましたが、StateがInRootMessageではありません (in: \"{this.name}\")";
                if (this.isUnsafe)
                {
                    Logger.Warn(errorMsg, "CustomRpcSender.Warn");
                }
                else
                {
                    throw new InvalidOperationException(errorMsg);
                }
            }
            this.stream.EndMessage();

            this.currentRpcTarget = -2;
            this.currentState = State.Ready;
            return this;
        }
        #endregion
        #region Start/End Rpc
        public CustomRpcSender StartRpc(uint targetNetId, RpcCalls rpcCall)
            => this.StartRpc(targetNetId, (byte)rpcCall);
        public CustomRpcSender StartRpc(
          uint targetNetId,
          byte callId)
        {
            if (this.currentState != State.InRootMessage)
            {
                string errorMsg = $"RPCを開始しようとしましたが、StateがInRootMessageではありません (in: \"{this.name}\")";
                if (this.isUnsafe)
                {
                    Logger.Warn(errorMsg, "CustomRpcSender.Warn");
                }
                else
                {
                    throw new InvalidOperationException(errorMsg);
                }
            }

            this.stream.StartMessage(2);
            this.stream.WritePacked(targetNetId);
            this.stream.Write(callId);

            this.currentState = State.InRpc;
            return this;
        }
        public CustomRpcSender EndRpc()
        {
            if (this.currentState != State.InRpc)
            {
                string errorMsg = $"RPCを終了しようとしましたが、StateがInRpcではありません (in: \"{this.name}\")";
                if (this.isUnsafe)
                {
                    Logger.Warn(errorMsg, "CustomRpcSender.Warn");
                }
                else
                {
                    throw new InvalidOperationException(errorMsg);
                }
            }

            this.stream.EndMessage();
            this.currentState = State.InRootMessage;
            return this;
        }
        #endregion
        public CustomRpcSender AutoStartRpc(
          uint targetNetId,
          byte callId,
          int tarGetClientId = -1)
        {
            if (tarGetClientId == -2) tarGetClientId = -1;
            if (this.currentState is not State.Ready and not State.InRootMessage)
            {
                string errorMsg = $"RPCを自動で開始しようとしましたが、StateがReadyまたはInRootMessageではありません (in: \"{this.name}\")";
                if (this.isUnsafe)
                {
                    Logger.Warn(errorMsg, "CustomRpcSender.Warn");
                }
                else
                {
                    throw new InvalidOperationException(errorMsg);
                }
            }
            if (this.currentRpcTarget != tarGetClientId)
            {
                //StartMessage処理
                if (this.currentState == State.InRootMessage) this.EndMessage();
                this.StartMessage(tarGetClientId);
            }
            this.StartRpc(targetNetId, callId);

            return this;
        }
        public void SendMessage()
        {
            if (this.currentState == State.InRootMessage) this.EndMessage();
            if (this.currentState != State.Ready)
            {
                string errorMsg = $"RPCを送信しようとしましたが、StateがReadyではありません (in: \"{this.name}\")";
                if (this.isUnsafe)
                {
                    Logger.Warn(errorMsg, "CustomRpcSender.Warn");
                }
                else
                {
                    throw new InvalidOperationException(errorMsg);
                }
            }

            AmongUsClient.Instance.SendOrDisconnect(this.stream);
            this.onSendDelegate();
            this.currentState = State.Finished;
            Logger.Info($"\"{this.name}\" is finished", "CustomRpcSender");
            this.stream.Recycle();
        }

        // Write
        #region PublicWriteMethods
        public CustomRpcSender Write(float val) => this.Write(w => w.Write(val));
        public CustomRpcSender Write(string val) => this.Write(w => w.Write(val));
        public CustomRpcSender Write(ulong val) => this.Write(w => w.Write(val));
        public CustomRpcSender Write(int val) => this.Write(w => w.Write(val));
        public CustomRpcSender Write(uint val) => this.Write(w => w.Write(val));
        public CustomRpcSender Write(ushort val) => this.Write(w => w.Write(val));
        public CustomRpcSender Write(byte val) => this.Write(w => w.Write(val));
        public CustomRpcSender Write(sbyte val) => this.Write(w => w.Write(val));
        public CustomRpcSender Write(bool val) => this.Write(w => w.Write(val));
        public CustomRpcSender Write(Il2CppStructArray<byte> bytes) => this.Write(w => w.Write(bytes));
        public CustomRpcSender Write(Il2CppStructArray<byte> bytes, int offset, int length) => this.Write(w => w.Write(bytes, offset, length));
        public CustomRpcSender WriteBytesAndSize(Il2CppStructArray<byte> bytes) => this.Write(w => w.WriteBytesAndSize(bytes));
        public CustomRpcSender WritePacked(int val) => this.Write(w => w.WritePacked(val));
        public CustomRpcSender WritePacked(uint val) => this.Write(w => w.WritePacked(val));
        public CustomRpcSender WriteNetObject(InnerNetObject obj) => this.Write(w => w.WriteNetObject(obj));
        #endregion

        private CustomRpcSender Write(Action<MessageWriter> action)
        {
            if (this.currentState != State.InRpc)
            {
                string errorMsg = $"RPCを書き込もうとしましたが、StateがWrite(書き込み中)ではありません (in: \"{this.name}\")";
                if (this.isUnsafe)
                {
                    Logger.Warn(errorMsg, "CustomRpcSender.Warn");
                }
                else
                {
                    throw new InvalidOperationException(errorMsg);
                }
            }
            action(this.stream);

            return this;
        }

        public enum State
        {
            BeforeInit = 0, //初期化前 何もできない
            Ready, //送信準備完了 StartMessageとSendMessageを実行可能
            InRootMessage, //StartMessage～EndMessageの間の状態 StartRpcとEndMessageを実行可能
            InRpc, //StartRpc～EndRpcの間の状態 WriteとEndRpcを実行可能
            Finished, //送信後 何もできない
        }
    }

    public static class CustomRpcSenderExtensions
    {
        public static void RpcSetRole(this CustomRpcSender sender, PlayerControl player, RoleTypes role, int tarGetClientId = -1)
        {
            sender.AutoStartRpc(player.NetId, (byte)RpcCalls.SetRole, tarGetClientId)
              .Write((ushort)role)
              .EndRpc();
            if (tarGetClientId == -1)
            {
                player.SetRole(role);
            }
        }
        public static void RpcMurderPlayer(this CustomRpcSender sender, PlayerControl player, PlayerControl target, int tarGetClientId = -1)
        {
            sender.AutoStartRpc(player.NetId, (byte)RpcCalls.MurderPlayer, tarGetClientId)
              .WriteNetObject(target)
              .EndRpc();
        }
    }
}