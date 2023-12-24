using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperNewRoles.Roles.RoleBases;
public class DeathInfo
{
    /// <summary>
    /// キルしたプレイヤー
    /// </summary>
    public PlayerControl Killer => Deadplayer.killerIfExisting;
    /// <summary>
    /// 死亡したプレイヤー
    /// </summary>
    public PlayerControl DeathPlayer => Deadplayer.player;
    /// <summary>
    /// 死因
    /// </summary>
    public DeathReason deathReason => Deadplayer.deathReason;
    /// <summary>
    /// 自殺か
    /// </summary>
    public bool IsSuicide => Deadplayer.killerIfExisting != null && Deadplayer.player == Deadplayer.killerIfExisting;
    /// <summary>
    /// 守護されたか
    /// </summary>
    public readonly bool IsGuarded;
    private readonly DeadPlayer Deadplayer;
    public DeathInfo(DeadPlayer deadPlayer)
    {
        Deadplayer = deadPlayer;
        IsGuarded = deadPlayer.player.IsAlive();
    }
}