using HarmonyLib;
using SuperNewRoles.Roles.RoleBases;
using UnityEngine;

namespace SuperNewRoles.Roles.Neutral;

public class Fox : RoleBase<Fox>
{
    public static Color32 color = RoleClass.FoxPurple;

    public Fox()
    {
        RoleId = roleId = RoleId.Fox;
        //以下いるもののみ変更
        OptionId = 310;
        OptionType = CustomOptionType.Neutral;
        HasTask = false;
        CanUseVentOptionOn = true;
        CanUseVentOptionDefault = false;
        IsImpostorViewOptionOn = true;
        IsImpostorViewOptionDefault = false;
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

    public static CustomOption FoxReport;

    public override void SetupMyOptions()
    {
        FoxReport = CustomOption.Create(OptionId, false, CustomOptionType.Neutral, "MinimalistReportSetting", true, RoleOption); OptionId++;
    }

    public static void Clear()
    {
        players = new();
    }


    public static class FoxMurderPatch
    {
        public static void Guard(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target)
        {
            if (EvilEraser.IsOKAndTryUse(EvilEraser.BlockTypes.FoxGuard, __instance))
                target.RpcProtectPlayer(target, 0);
        }
    }
}