// FIXME:RoleBase以前の方式で記載中、RoleBaseに戻す場合は「SKWaveCannonをRoleBase以前の形式に変更」をリバートしてください

/*using System;

using System.Collections.Generic;

using System.Text;

using SuperNewRoles.Roles.RoleBases;

using UnityEngine;

namespace SuperNewRoles.Roles.Neutral;

public class SidekickWaveCannon : RoleBase<SidekickWaveCannon>

{
    public static Color color = Palette.CrewmateBlue;

    public SidekickWaveCannon()

    {

        RoleId = roleId = RoleId.SidekickWaveCannon;

        //以下いるもののみ変更

        HasTask = false;

        IsAssignRoleFirst = false;

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

    public override void SetupMyOptions() { }

    public static void Clear()

    {
        players = new();
    }

}*/