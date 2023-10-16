using System;
using System.Collections;
using SuperNewRoles.Roles.RoleBases.Interfaces;

namespace SuperNewRoles.Roles.RoleBases;
public abstract class RoleBase : IDisposable
{
    public static int MaxInstanceId = int.MinValue;
    public PlayerControl Player { get; private set; }

    //比較用なので各クライアントごとに違ってもOK
    //逆に比較用以外に使うな
    private int InstanceId { get; }
    //後で処理書く
    public RoleBase()
    {
        InstanceId = MaxInstanceId;
        MaxInstanceId++;
    }
    //Disposeを実装
    public void Dispose()
    {

    }
    public void SetPlayer(PlayerControl player)
    {
        Player = player;
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
}