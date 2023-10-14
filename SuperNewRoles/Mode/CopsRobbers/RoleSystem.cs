using System;
using System.Collections.Generic;
using SuperNewRoles.Helpers;
using SuperNewRoles.Mode.SuperHostRoles;
using SuperNewRoles.Patches;
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

        var introData = IntroData.GetIntroData(player.GetRole(), player);
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
        NewName = "<size=75%>" + ModHelpers.Cs(introData.color, introData.Name) + TaskText + "</size>\n" + (CopsRobbersOptions.CRHideName.GetBool() && CopsRobbersOptions.CopsRobbersMode.GetBool() ? " " : ModHelpers.Cs(introData.color, Name));
        player.RpcSetNamePrivate(NewName);
    }
    public static void AssignRole()
    {
        AllRoleSetClass.Impoonepar = new();
        AllRoleSetClass.Imponotonepar = new();
        AllRoleSetClass.Neutonepar = new();
        AllRoleSetClass.Neutnotonepar = new();
        AllRoleSetClass.Crewonepar = new();
        AllRoleSetClass.Crewnotonepar = new();
        foreach (IntroData intro in IntroData.Intros.Values)
        {
            if (intro.RoleId is
                RoleId.Workperson or RoleId.HomeSecurityGuard or RoleId.Tuna or RoleId.ToiletFan)
            {
                var option = IntroData.GetOption(intro.RoleId);
                if (option == null) continue;
                var selection = option.GetSelection();
                if (selection != 0)
                {
                    if (selection == 10)
                    {
                        switch (intro.Team)
                        {
                            case TeamRoleType.Crewmate:
                                AllRoleSetClass.Crewonepar.Add(intro.RoleId);
                                break;
                            case TeamRoleType.Impostor:
                                AllRoleSetClass.Impoonepar.Add(intro.RoleId);
                                break;
                            case TeamRoleType.Neutral:
                                AllRoleSetClass.Neutonepar.Add(intro.RoleId);
                                break;
                        }
                    }
                    else
                    {
                        for (int i = 1; i <= selection; i++)
                        {
                            switch (intro.Team)
                            {
                                case TeamRoleType.Crewmate:
                                    AllRoleSetClass.Crewnotonepar.Add(intro.RoleId);
                                    break;
                                case TeamRoleType.Impostor:
                                    AllRoleSetClass.Imponotonepar.Add(intro.RoleId);
                                    break;
                                case TeamRoleType.Neutral:
                                    AllRoleSetClass.Neutnotonepar.Add(intro.RoleId);
                                    break;
                            }
                        }
                    }
                }
            }
        }
        AllRoleSetClass.CrewOrImpostorSet();
        AllRoleSetClass.AllRoleSet();
        SuperHostRoles.RoleSelectHandler.SetCustomRoles();
        SyncSetting.CustomSyncSettings();
        ChacheManager.ResetChache();
    }
}