using System;
using System.Collections.Generic;
using SuperNewRoles.Helpers;
using SuperNewRoles.Mode.SuperHostRoles;
using SuperNewRoles.Patches;
using SuperNewRoles.Roles.Role;
using SuperNewRoles.Roles.RoleBases;
using UnityEngine;

namespace SuperNewRoles.Mode.CopsRobbers;

class RoleSystem
{
    public static void RoleSetName()
    {
        var caller = new System.Diagnostics.StackFrame(1, false);
        var callerMethod = caller.GetMethod();
        string callerMethodName = callerMethod.Name;
        string callerClassName = callerMethod.DeclaringType.FullName;
        SuperNewRolesPlugin.Logger.LogInfo("[CopsRobbers:FixedUpdate] SetRoleNamesが" + callerClassName + "." + callerMethodName + "から呼び出されました。");

        bool commsActive = RoleHelpers.IsComms();
        foreach (PlayerControl p in CachedPlayer.AllPlayers)
        {
            SetRoleName(p, commsActive);
        }
    }
    public static void SetRoleName(PlayerControl player, bool commsActive)
    {
        if (!ModeHandler.IsMode(ModeId.CopsRobbers)) return;
        if (!AmongUsClient.Instance.AmHost) return;

        var caller = new System.Diagnostics.StackFrame(1, false);
        var callerMethod = caller.GetMethod();
        string callerMethodName = callerMethod.Name;
        string callerClassName = callerMethod.DeclaringType.FullName;

        //必要がないなら処理しない
        if (player.IsMod()) return;

        string Name = player.GetDefaultName();
        string NewName = "";
        Dictionary<byte, string> ChangePlayers = new();

        /*
        if (player.IsLovers())
        {
            var suffix = ModHelpers.Cs(RoleClass.Lovers.color, " ♥");
            PlayerControl Side = player.GetOneSideLovers();
            string name = Side.GetDefaultName();
            if (!ChangePlayers.ContainsKey(Side.PlayerId)) ChangePlayers.Add(Side.PlayerId, Side.GetDefaultName() + suffix);
            else { ChangePlayers[Side.PlayerId] = ChangePlayers[Side.PlayerId] + suffix; }
            MySuffix += suffix;
        }
        if (player.IsQuarreled())
        {
            var suffix = ModHelpers.Cs(RoleClass.Quarreled.color, "○");
            PlayerControl Side = player.GetOneSideQuarreled();
            string name = Side.GetDefaultName();
            if (!ChangePlayers.ContainsKey(Side.PlayerId)) ChangePlayers.Add(Side.PlayerId, Side.GetDefaultName() + suffix);
            else { ChangePlayers[Side.PlayerId] = ChangePlayers[Side.PlayerId] + suffix; }
            MySuffix += suffix;
        }
        */

        string TaskText = "";
        if (!player.IsImpostor())
        {
            try
            {
                if (commsActive) TaskText = ModHelpers.Cs(Color.yellow, "(?/" + TaskCount.TaskDateNoClearCheck(player.Data).Item2 + ")");
                else
                {
                    var (Complete, all) = TaskCount.TaskDateNoClearCheck(player.Data);
                    TaskText = ModHelpers.Cs(Color.yellow, "(" + Complete + "/" + all + ")");
                }
            }
            catch { }
        }
        NewName = "<size=75%>" + CustomRoles.GetRoleNameOnColor(player) + TaskText + "</size>\n" + (CopsRobbersOptions.CRHideName.GetBool() && CopsRobbersOptions.CopsRobbersMode.GetBool() ? " " : ModHelpers.Cs(CustomRoles.GetRoleColor(player), Name));
        player.RpcSetNamePrivate(NewName);
    }
    public static void AssignRole()
    {
        foreach (IntroData intro in IntroData.Intros.Values)
        {
            if (!(intro.RoleId is
                RoleId.Workperson or RoleId.HomeSecurityGuard or RoleId.Tuna or RoleId.ToiletFan))
                continue;
            var option = IntroData.GetOption(intro.RoleId);
            if (option == null) continue;
            var selection = option.GetSelection();
            AllRoleSetClass.SetChance(selection, intro.RoleId, intro.Team);
        }
        foreach (RoleInfo info in RoleInfoManager.RoleInfos.Values)
        {
            if (!(info.Role is
                RoleId.Workperson or RoleId.HomeSecurityGuard or RoleId.Tuna or RoleId.ToiletFan))
                continue;
            var option = IntroData.GetOption(info.Role);
            if (option == null) continue;
            var selection = option.GetSelection();
            AllRoleSetClass.SetChance(selection, info.Role, info.Team);
        }
        AllRoleSetClass.CrewOrImpostorSet();
        AllRoleSetClass.AllRoleSet();
        SuperHostRoles.RoleSelectHandler.SetCustomRoles();
        SyncSetting.CustomSyncSettings();
        CacheManager.ResetCache();
    }
}