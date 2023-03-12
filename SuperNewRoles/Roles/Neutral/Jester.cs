using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using SuperNewRoles.Patches;
using SuperNewRoles.Roles.RoleBases;

namespace SuperNewRoles.Roles.Neutral;

public class Jester : RoleBase<Jester>
{
    public static Color32 color = new(255, 165, 0, byte.MaxValue);

    public Jester()
    {
        RoleId = roleId = RoleId.Jester;
        OptionId = 16;
        IsSHRRole = true;
        OptionType = CustomOptionType.Neutral;
        HasTask = false;
        CanUseVentOptionOn = true;
        CanUseVentOptionDefault = false;
        CanUseSaboOptionOn = true;
        CanUseSaboOptionDefault = false;
    }

    public override void OnMeetingStart() { }
    public override void OnWrapUp() { }
    public override void FixedUpdate() { }
    public override void MeFixedUpdateAlive() { }
    public override void MeFixedUpdateDead() { }
    public override void OnKill(PlayerControl target) { }
    public override void OnDeath(PlayerControl killer = null) { }
    public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }
    public override void EndUseAbility() { }
    public override void ResetRole() { }
    public override void PostInit() { }
    public override void UseAbility() { base.UseAbility(); AbilityLimit--; if (AbilityLimit <= 0) EndUseAbility(); }
    public override bool CanUseAbility() { return base.CanUseAbility() && AbilityLimit <= 0; }

    public static CustomOption JesterIsWinCleartask;
    public static CustomOption JesterCommonTask;
    public static CustomOption JesterShortTask;
    public static CustomOption JesterLongTask;

    public override void SetupMyOptions() {
        JesterIsWinCleartask = CustomOption.Create(OptionId, true, CustomOptionType.Neutral, "JesterIsWinClearTaskSetting", false, RoleOption); OptionId++;
        var jesteroption = SelectTask.TaskSetting(21, 22, 23, JesterIsWinCleartask, CustomOptionType.Neutral, true);
        JesterCommonTask = jesteroption.Item1;
        JesterShortTask = jesteroption.Item2;
        JesterLongTask = jesteroption.Item3;
    }

    public static void Clear()
    {
        players = new();
    }
}