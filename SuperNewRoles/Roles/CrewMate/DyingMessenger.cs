using System;
using System.Collections.Generic;
using SuperNewRoles.Roles.RoleBases;
using UnityEngine;

namespace SuperNewRoles.Roles.Crewmate;

public class DyingMessenger : RoleBase<DyingMessenger>
{
    public static Color32 color = new(191, 197, 202, byte.MaxValue);

    public DyingMessenger()
    {
        RoleId = roleId = RoleId.DyingMessenger;
        //以下いるもののみ変更
        OptionId = 1203;
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

    //ボタンが必要な場合のみ(Buttonsの方に記述する必要あり)
    public static void MakeButtons(HudManager hm) { }
    public static void SetButtonCooldowns() { }

    public static CustomOption DyingMessengerGetRoleTime;
    public static CustomOption DyingMessengerGetLightAndDarkerTime;

    public override void SetupMyOptions()
    {
        DyingMessengerGetRoleTime = CustomOption.Create(OptionId, false, CustomOptionType.Crewmate, "DyingMessengerGetRoleTimeSetting", 20f, 0f, 60f, 1f, RoleOption); OptionId++;
        DyingMessengerGetLightAndDarkerTime = CustomOption.Create(OptionId, false, CustomOptionType.Crewmate, "DyingMessengerGetLightAndDarkerTimeSetting", 2f, 0f, 60f, 1f, RoleOption); OptionId++;
    }

    public static Dictionary<byte, (DateTime, PlayerControl)> ActualDeathTime;
    public static void Clear()
    {
        players = new();
        ActualDeathTime = new();
    }
}