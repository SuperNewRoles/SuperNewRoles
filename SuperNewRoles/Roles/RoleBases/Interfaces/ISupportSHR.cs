using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AmongUs.GameOptions;

namespace SuperNewRoles.Roles.RoleBases.Interfaces;
public interface ISupportSHR : ITaskHolder
{
    /// <summary>
    /// 判定上の役職
    /// </summary>
    public RoleTypes RealRole { get; }
    public bool IsRealRoleNotModOnly => false;
    /// <summary>
    /// Desyncの場合はDesync役職を設定する
    /// </summary>
    public RoleTypes DesyncRole => RealRole;
    /// <summary>
    /// Desync役職か判定
    /// </summary>
    public sealed bool IsDesync => RealRole != DesyncRole;
    /// <summary>
    /// インポスター視界かを設定
    /// nullの場合はクルーか第三でDesyncインポならクルーに設定
    /// </summary>
    public bool? IsImpostorLight => null;
    public bool IsZeroCoolEngineer => false;
    public bool HasKillButtonClient => this is IKiller killer && killer.CanUseKill;
    public void BuildName(StringBuilder Suffix, StringBuilder RoleNameText, PlayerData<string> ChangePlayers)
    {

    }
    /// <summary>
    /// 全てのプレイヤーに見える名前
    /// </summary>
    public bool BuildAllName(out string text)
    {
        text = "";
        return false;
    }
    public void BuildSetting(IGameOptions gameOptions)
    {

    }
}