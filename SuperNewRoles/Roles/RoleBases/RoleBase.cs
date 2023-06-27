using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AmongUs.GameOptions;
using HarmonyLib;
using MonoMod.Cil;
using UnityEngine;

namespace SuperNewRoles.Roles.RoleBases;
public abstract class Role
{
    public static List<Role> allRoles = new();
    public PlayerControl player;
    public RoleId roleId;
    public int ObjectId;
    public int AbilityLimit;
    public static int MaxObjectId = 0;

    public abstract void OnMeetingStart();
    public abstract void OnWrapUp();
    public virtual void FixedUpdate() { }
    public virtual void MeFixedUpdateAlive() { }
    public virtual void MeFixedUpdateDead() { }
    public abstract void OnKill(PlayerControl target);
    public abstract void OnDeath(PlayerControl killer = null);
    public abstract void HandleDisconnect(PlayerControl player, DisconnectReasons reason);
    public virtual void EndUseAbility() { }
    public virtual void ResetRole() { }
    public virtual void PostInit() { }
    //public virtual string modifyNameText(string nameText) { return nameText; }
    //public virtual string meetingInfoText() { return ""; }
    public virtual void UseAbility() { AbilityLimit--; if (AbilityLimit <= 0) EndUseAbility(); }
    public virtual bool CanUseAbility() { return AbilityLimit <= 0; }

    public static void ClearAll()
    {
        MaxObjectId = 0;
        allRoles = new List<Role>();
    }
}

public abstract class RoleBase<T> : Role where T : RoleBase<T>, new()
{
    public static List<T> players = new();
    public static RoleId RoleId;
    //設定を有効にするか
    public bool CanUseVentOptionOn = false;
    public bool CanUseVentOptionDefault;
    public bool CanUseSaboOptionOn = false;
    public bool CanUseSaboOptionDefault;
    public bool IsImpostorViewOptionOn = false;
    public bool IsImpostorViewOptionDefault;
    public bool CoolTimeOptionOn = false;
    public bool DurationTimeOptionOn = false;
    public float CoolTimeOptionMax = -1f;
    public float CoolTimeOptionMin = -1f;
    public float DurationTimeOptionMax = -1f;
    public float DurationTimeOptionMin = -1f;

    //設定を参照
    public bool CanUseVent => CanUseVentOpt is null ? false : CanUseVentOpt.GetBool();
    public bool CanUseSabo => CanUseSaboOpt is null ? false : CanUseSaboOpt.GetBool();
    public bool IsImpostorView => IsImpostorViewOpt is null ? false : IsImpostorViewOpt.GetBool();
    public float CoolTime => CoolTimeOpt is null ? -1f : CoolTimeOpt.GetFloat();
    public float DurationTime => DurationTimeOpt is null ? -1f : DurationTimeOpt.GetFloat();


    //役職について設定するところ
    public bool HasTask = true;
    public bool HasFakeTask = true;
    public bool IsKiller = false;
    //最初から割り当てられるか
    public bool IsAssignRoleFirst = true;
    public int OptionId;
    public bool IsSHRRole = false;
    public CustomOptionType OptionType = CustomOptionType.Crewmate;
    public bool IsChangeOutfitRole = false;


    public static CustomOption RoleOption;
    public static CustomOption PlayerCountOption;
    public static CustomOption CanUseVentOption;
    public static CustomOption CanUseSaboOption;
    public static CustomOption IsImpostorViewOption;
    public static CustomOption CoolTimeOption;
    public static CustomOption DurationTimeOption;
    public CustomOption CanUseVentOpt => CanUseVentOption;
    public CustomOption CanUseSaboOpt => CanUseSaboOption;
    public CustomOption IsImpostorViewOpt => IsImpostorViewOption;
    public CustomOption CoolTimeOpt => CoolTimeOption;
    public CustomOption DurationTimeOpt => DurationTimeOption;
    public RoleBase()
    {

    }

    public RoleBase(bool isFirst)
    {
        if (RoleOption is null) SetUpOption();
    }

    public void SetUpOption()
    {
        if (IsAssignRoleFirst)
        {
            var Players = OptionType is CustomOptionType.Impostor ? CustomOptionHolder.ImpostorPlayers : CustomOptionHolder.CrewPlayers;
            RoleOption = CustomOption.SetupCustomRoleOption(OptionId, IsSHRRole, RoleId); OptionId++;
            PlayerCountOption = CustomOption.Create(OptionId, IsSHRRole, OptionType, "SettingPlayerCountName", Players[0], Players[1], Players[2], Players[3], RoleOption); OptionId++;
        }
        if (CoolTimeOptionOn) CoolTimeOption = CustomOption.Create(OptionId, false, CustomOptionType.Impostor, "NiceScientistCooldownSetting", 30f, CoolTimeOptionMin, CoolTimeOptionMax, 2.5f, RoleOption, format: "unitSeconds"); OptionId++;
        if (DurationTimeOptionOn) DurationTimeOption = CustomOption.Create(OptionId, false, CustomOptionType.Impostor, "NiceScientistDurationSetting", 10f, DurationTimeOptionMin, DurationTimeOptionMax, 2.5f, RoleOption, format: "unitSeconds"); OptionId++;
        if (CanUseVentOptionOn) CanUseVentOption = CustomOption.Create(OptionId, IsSHRRole, OptionType, "JackalUseVentSetting", CanUseVentOptionDefault, RoleOption); OptionId++;
        if (CanUseSaboOptionOn) CanUseSaboOption = CustomOption.Create(OptionId, IsSHRRole, OptionType, "JackalUseSaboSetting", CanUseSaboOptionDefault, RoleOption); OptionId++;
        if (IsImpostorViewOptionOn) IsImpostorViewOption = CustomOption.Create(OptionId, IsSHRRole, OptionType, "MadmateImpostorLightSetting", IsImpostorViewOptionDefault, RoleOption); OptionId++;
        SetupMyOptions();
    }
    public abstract void SetupMyOptions();


    public void Init(PlayerControl player)
    {
        this.player = player;
        players.Add((T)this);
        allRoles.Add(this);
        PostInit();
        ObjectId = MaxObjectId;
        MaxObjectId++;
        _local = null;
    }



    public static T local
    {
        get
        {
            return players.FirstOrDefault(x => x.player == PlayerControl.LocalPlayer);
        }
    }
    public static PlayerControl _local;

    public static List<PlayerControl> allPlayers
    {
        get
        {
            return players.Select(x => x.player).ToList();
        }
    }

    public static List<PlayerControl> livingPlayers
    {
        get
        {
            return players.Select(x => x.player).Where(x => x.IsAlive()).ToList();
        }
    }

    public static List<PlayerControl> deadPlayers
    {
        get
        {
            return players.Select(x => x.player).Where(x => x.IsDead()).ToList();
        }
    }

    public static bool exists
    {
        get { return players.Count > 0; }
    }

    public static T GetRole(PlayerControl player = null)
    {
        player = player ?? PlayerControl.LocalPlayer;
        return players.FirstOrDefault(x => x.player == player);
    }

    public static bool IsRole(PlayerControl player)
    {
        return players.Any(x => x.player == player);
    }

    public static T SetRole(PlayerControl player)
    {
        if (!IsRole(player))
        {
            T role = new();
            role.Init(player);
            return role;
        }
        return null;
    }

    public static void EraseRole(PlayerControl player)
    {
        players.DoIf(x => x.player == player, x => x.ResetRole());
        players.RemoveAll(x => x.player == player && x.roleId == RoleId);
        allRoles.RemoveAll(x => x.player == player && x.roleId == RoleId);
        if (_local is not null && player.PlayerId == _local.PlayerId) _local = null;
    }

    public static void SwapRole(PlayerControl p1, PlayerControl p2)
    {
        var index = players.FindIndex(x => x.player == p1);
        if (index >= 0)
        {
            players[index].player = p2;
        }
        if (_local is not null && p1.PlayerId == _local.PlayerId) _local = null;
    }
}