using System;
using AmongUs.GameOptions;
using SuperNewRoles.Buttons;
using SuperNewRoles.Roles.RoleBases;
using UnityEngine;

namespace SuperNewRoles.Roles;

public class Lighter : RoleBase<Lighter>
{
    public static Color32 color = new(255, 236, 71, byte.MaxValue);

    public static DateTime ButtonTimer;

    public Lighter()
    {
        RoleId = roleId = RoleId.Lighter;
        //以下いるもののみ変更
        IsAssignRoleFirst = true;
        OptionId = 24;
        IsSHRRole = false;
        OptionType = CustomOptionType.Crewmate;
        CoolTimeOptionOn = true;
        DurationTimeOptionOn = true;
    }

    public static void ResetCooldown()
    {
        HudManagerStartPatch.LighterLightOnButton.MaxTimer = CoolTimeS;
        ButtonTimer = DateTime.Now;
    }
    public static bool isLighter(PlayerControl Player)
    {
        return Player.IsRole(RoleId.Lighter);
    }
    public static void LightOnStart()
    {
        IsLightOn = true;
    }
    public static void LightOutEnd()
    {
        if (!IsLightOn) return;
        IsLightOn = false;
    }
    public static void EndMeeting()
    {
        HudManagerStartPatch.LighterLightOnButton.MaxTimer = CoolTimeS;
        ButtonTimer = DateTime.Now;
        IsLightOn = false;
    }
    public static float DefaultCrewVision;
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
    public override void PostInit() {  }
    public override void UseAbility() { base.UseAbility(); AbilityLimit--; if (AbilityLimit <= 0) EndUseAbility(); }
    public override bool CanUseAbility() { return base.CanUseAbility() && AbilityLimit <= 0; }

    //ボタンが必要な場合のみ(Buttonsの方に記述する必要あり)
    public static void MakeButtons(HudManager hm) { }
    public static void SetButtonCooldowns() { }

    public static CustomOption LighterUpVision;

    public static Sprite GetButtonSprite() => ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.LighterLightOnButton.png", 115f);

    public override void SetupMyOptions() {
        LighterUpVision = CustomOption.Create(OptionId, false, CustomOptionType.Crewmate, "LighterUpVisionSetting", 0.25f, 0f, 5f, 0.25f, RoleOption);
    }

    public static bool IsLightOn;

    public static void Clear()
    {
        players = new();
        DefaultCrewVision = GameOptionsManager.Instance.CurrentGameOptions.GetFloat(FloatOptionNames.CrewLightMod);
        IsLightOn = false;
    }
}