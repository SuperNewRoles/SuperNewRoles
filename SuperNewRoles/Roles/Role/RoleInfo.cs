
using System;
using System.Collections.Generic;
using SuperNewRoles.Roles.RoleBases;
using SuperNewRoles.Roles.RoleBases.Interfaces;
using UnityEngine;

namespace SuperNewRoles.Roles.Role;
public class RoleInfo
{
    public Type RoleObjectType { get; }
    public string RoleObjectTypeName { get; }
    public RoleId Role { get; }
    public string NameKey { get; }
    public Color32 RoleColor { get; }
    public TeamRoleType Team { get; }
    public TeamType TeamType { get; }
    public QuoteMod QuoteMod { get; }
    public bool IsGhostRole { get; }

    private RoleTags Tags;
    private Func<PlayerControl, RoleBase> _createInstance { get; }
    public RoleInfo(
        Type roleObjectType,
        Func<PlayerControl, RoleBase> createInstance,
        RoleId role,
        string namekey,
        Color32 roleColor,
        RoleTags tags,
        TeamRoleType team = TeamRoleType.Crewmate,
        TeamType teamType = TeamType.Crewmate,
        QuoteMod quoteMod = QuoteMod.SuperNewRoles
        )
    {
        this.RoleObjectType = roleObjectType;
        this.RoleObjectTypeName = roleObjectType.Name;
        this.Role = role;
        this._createInstance = createInstance;
        this.NameKey = namekey;
        this.Team = team;
        this.IsGhostRole = RoleObjectType.IsSubclassOf(typeof(IGhostRole));
        this.RoleColor = roleColor;
        this.TeamType = teamType;
        this.Tags = tags;
        this.QuoteMod = quoteMod;
        RoleInfoManager.RoleInfos.Add(role, this);
    }
    public RoleBase CreateInstance(PlayerControl player)
    {
        if (_createInstance != null)
            return _createInstance(player);
        //Instance作成
        //Functionが設定ていない場合はActivatorで作成
        return Activator.CreateInstance(RoleObjectType, player as object) as RoleBase;
    }
}
public enum RoleTag
{
    Information, //情報系
    PowerPlayResistance, //PP対抗
    SpecialKiller, //特殊キラー
    Killer, //キラー
    Bomb, //爆発系
    Hacchan, //ハッチャン役
    Takada, //高田村役
    PlayTimeAdjustment, //試合時間調整系
    Haloween, //ハロウィン役
    CanSidekick, //サイドキック可能役
    CustomObject, //カスタムオブジェクト使用役
    CanUseVent, //ベント使用可能役

}
public enum TeamTag
{
    Impostor,
    Crewmate,
    Neutral,
    Jackal,
    Sidekick,
    JackalFriends,
    Madmate,
    MadKiller,
}
public class RoleTags
{
    public readonly RoleTag[] Tags;
    public readonly TeamTag TeamTag;
    private RoleId Role;
    public RoleTags(RoleId role, TeamTag teamtag, params RoleTag[] tag)
    {
        this.Role = role;
        this.Tags = tag;
        this.TeamTag = teamtag;
    }
}

/// <summary>アイデア元及び移植元Mod (一部Mod名ではなく開発者様名)</summary>
public enum QuoteMod
{
    AmongUs,

    // 以下アルファベット順
    aulibhaltnet,
    ExtremeRoles,
    Jester, // 能力はTOR由来の為, てるてるはTOR元扱いとする。
    NebulaOnTheShip,
    SuperNewRoles,
    TheOtherRoles, // TORのクレジットで, Modにリンクが飛ばない役職はTOR元扱いにする。
    TheOtherRolesGM,
    TheOtherRolesGMH,
    tomarai,
    TownOfUs,
    TownOfUsR,
    Woodi_dev,
}