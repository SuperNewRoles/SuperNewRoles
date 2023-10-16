using System;
using SuperNewRoles.Roles.RoleBases.Interfaces;

namespace SuperNewRoles.Roles.RoleBases;
public abstract class RoleBase : IDisposable
{
    public PlayerControl Player { get; private set; }
    //Disposeを実装
    public void Dispose()
    {

    }
    public void SetPlayer(PlayerControl player)
    {
        Player = player;
    }
}