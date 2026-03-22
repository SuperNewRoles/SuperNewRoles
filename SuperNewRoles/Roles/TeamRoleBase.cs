using System.Collections.Generic;

namespace SuperNewRoles.Roles;

/// <summary>
/// n人1組で選出される「チーム役職(メタ役職)」のためのインターフェース。
/// このRoleId自体は通常プレイヤーに直接アサインせず、AssignRoles側が TeamSize 人をまとめて選び、
/// TeamRoleBase が指定するメンバーRoleIdを付与するために利用する。
/// </summary>
public interface ITeamRoleBase : IRoleBase
{
    /// <summary>
    /// 1チームに必要な人数。
    /// </summary>
    int TeamSize { get; }

    /// <summary>
    /// チームに付与するRoleIdの並び。
    /// - 長さが TeamSize の場合: それぞれ異なるRoleIdを割り当てる
    /// - 長さが 1 の場合: 同一RoleIdを TeamSize 人に割り当てる
    /// </summary>
    IReadOnlyList<RoleId> MemberRoleIds { get; }

    /// <summary>
    /// 1チーム分の割り当て処理（RpcCustomSetRole等を呼んで実際にメンバー役職を付与する）。
    /// 引数 players の要素数は TeamSize と一致する想定。
    /// </summary>
    void AssignTeam(IReadOnlyList<PlayerControl> players);

    /// <summary>
    /// ゲーム開始時や会議終了時の状態クリア処理。
    /// </summary>
    void ClearAndReload();
}

/// <summary>
/// n人1組で選出される「チーム役職(メタ役職)」の共通基底クラス。
/// </summary>
internal abstract class TeamRoleBase<T> : RoleBase<T>, ITeamRoleBase
    where T : TeamRoleBase<T>, new()
{
    public abstract int TeamSize { get; }
    public abstract IReadOnlyList<RoleId> MemberRoleIds { get; }
    public abstract void AssignTeam(IReadOnlyList<PlayerControl> players);

    /// <summary>
    /// ゲーム開始時や会議終了時の状態クリア処理。
    /// </summary>
    public virtual void ClearAndReload() { }
}



