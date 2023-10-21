using System;
using System.Collections.Generic;
using UnityEngine;

namespace SuperNewRoles.Roles;

public class RoleInfo
{
    public static Dictionary<RoleId, RoleInfo> AllRoleInfo = new();

    public readonly Type ClassType;
    public readonly RoleId RoleId;
    public readonly int Id;
    public readonly Color32 RoleColor;
    public readonly OptionCreatorDelegate OptionCreator;
    public readonly ClearAndReloaderDelegate ClearAndReloader;
    public readonly bool IsEnable = false;

    public RoleInfo(
        Type classType,
        RoleId roleId,
        int id,
        Color32 roleColor,
        OptionCreatorDelegate optionCreator,
        ClearAndReloaderDelegate clearAndReloader
    )
    {
        ClassType = classType;
        RoleId = roleId;
        Id = id;
        RoleColor = roleColor;
        OptionCreator = optionCreator;
        ClearAndReloader = clearAndReloader;

        AllRoleInfo.TryAdd(roleId, this);
    }

    public delegate void OptionCreatorDelegate();
    public delegate void ClearAndReloaderDelegate();

}