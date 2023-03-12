using System;
using System.Collections.Generic;
using System.Text;
using SuperNewRoles.Roles.RoleBases;
using UnityEngine;

namespace SuperNewRoles.Roles.Impostor;

public class EvilScientist : RoleBase<EvilScientist>
{
    public static Color color = Palette.CrewmateBlue;

    public EvilScientist()
    {
        RoleId = roleId = RoleId.EvilScientist;
        //以下いるもののみ変更
        HasTask = false;
        IsKiller = true;
        OptionId = 33;
        OptionType = CustomOptionType.Impostor;
        CoolTimeOptionOn = true;
        DurationTimeOptionOn = true;
    }

    public static Sprite GetButtonSprite() => ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.EvilScientistButton.png.png", 115f);

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
}