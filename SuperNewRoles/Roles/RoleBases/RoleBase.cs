using System;
using System.Collections;
using System.Linq;
using Hazel;
using SuperNewRoles.Helpers;
using SuperNewRoles.Roles.Role;
using SuperNewRoles.Roles.RoleBases.Interfaces;
using UnityEngine;

namespace SuperNewRoles.Roles.RoleBases;
public abstract class RoleBase : IDisposable
{
    public static int MaxInstanceId = int.MinValue;
    public PlayerControl Player { get; private set; }
    public RoleId Role => Roleinfo.Role;
    //比較用なので各クライアントごとに違ってもOK
    //逆に比較用以外に使うな
    private int InstanceId { get; }
    public RoleInfo Roleinfo { get; }
    public OptionInfo Optioninfo { get; }
    public IntroInfo Introinfo { get; }
    //後で処理書く
    public RoleBase(PlayerControl player, RoleInfo roleInfo, OptionInfo optionInfo, IntroInfo introInfo)
    {
        InstanceId = MaxInstanceId;
        MaxInstanceId++;
        this.Player = player;
        this.Roleinfo = roleInfo;
        this.Optioninfo = optionInfo;
        this.Introinfo = introInfo;
    }
    //Disposeを実装
    public void Dispose()
    {
        Player = null;
    }
    public void SetPlayer(PlayerControl player)
    {
        RoleBaseManager.ChangeRole(player, this);
        Player = player;
    }
    public AudioClip GetIntroAudioClip()
    {
        if (this is ICustomIntroSound customIntroSound)
            return customIntroSound.IntroSound;
        return RoleManager.Instance.GetRole(
                Introinfo.IntroSound
            )?.IntroSound;
    }
    //比較を時前にして高速化
    public override bool Equals(object obj)
    {
        if (obj is not RoleBase)
            return false;
        return this.GetHashCode() == obj.GetHashCode();
    }
    public override int GetHashCode()
    {
        return InstanceId;
    }
    //IRpcHandler向け
    private int LastPosition;
    public MessageWriter RpcWriter
    {
        get
        {
            MessageWriter writer = RPCHelper.StartRPC(CustomRPC.RoleRpcHandler);
            // 自身のPlayerIdを送信
            writer.Write(Player?.PlayerId ?? (byte)255);
            LastPosition = writer.Length;
            return writer;
        }
    }
    public void SendRpc(MessageWriter writer)
    {
        if (this is not IRpcHandler rpcHandler)
            return;
        MessageReader reader = MessageReader.Get(writer.Buffer.ToArray()[(LastPosition - 1)..]);
        reader.ReadByte();
        rpcHandler.RpcReader(reader);
        writer.EndRPC();
    }
    public static bool operator !=(RoleBase rb, bool rhs)
    {
        bool isNull = rb == null || rb.Player == null;
        return isNull == rhs;
    }
    public static bool operator ==(RoleBase rb, bool rhs)
    {
        bool isNull = rb == null || rb.Player == null;
        return !isNull == rhs;
    }
}