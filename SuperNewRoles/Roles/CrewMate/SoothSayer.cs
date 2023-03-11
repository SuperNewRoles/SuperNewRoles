using System;
using System.Collections.Generic;
using System.Text;
using SuperNewRoles.Roles.RoleBases;
using UnityEngine;

namespace SuperNewRoles.Roles.CrewMate;

public class SoothSayer : RoleBase<SoothSayer>
{
    public static Color32 color = new(190, 86, 235, byte.MaxValue);

    public SoothSayer()
    {
        RoleId = roleId = RoleId.DefaultRole;
        //以下いるもののみ変更
        OptionId = 12;
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
    public override void PostInit() { DisplayedPlayer = new(); AbilityLimit = CustomOptionHolder.SoothSayerMaxCount.GetInt();
        CanFirstWhite = CustomOptionHolder.SoothSayerFirstWhiteOption.GetBool();
    }
    public override void UseAbility() { base.UseAbility(); AbilityLimit--; if (AbilityLimit <= 0) EndUseAbility(); }
    public override bool CanUseAbility() { return base.CanUseAbility() && AbilityLimit <= 0; }

    //ボタンが必要な場合のみ(Buttonsの方に記述する必要あり)
    public static void MakeButtons(HudManager hm) { }
    public static void SetButtonCooldowns() { }

    public List<byte> DisplayedPlayer;
    public bool CanFirstWhite;
    public static bool DisplayMode;
    public static Sprite GetButtonSprite() => ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.SoothSayerButton.png", 115f);

    public static CustomOption SoothSayerDisplayMode;
    public static CustomOption SoothSayerMaxCount;
    public static CustomOption SoothSayerFirstWhiteOption;

    public override void SetupMyOptions() {
        SoothSayerDisplayMode = CustomOption.Create(OptionId, false, CustomOptionType.Crewmate, "SoothSayerDisplaySetting", false, RoleOption); OptionId++;
        SoothSayerMaxCount = CustomOption.Create(OptionId, false, CustomOptionType.Crewmate, "SoothSayerMaxCountSetting", CustomOptionHolder.CrewPlayers[0] - 1, CustomOptionHolder.CrewPlayers[1], CustomOptionHolder.CrewPlayers[2], CustomOptionHolder.CrewPlayers[3], RoleOption); OptionId++;
        SoothSayerFirstWhiteOption = CustomOption.Create(1050, false, CustomOptionType.Crewmate, "SoothSayerFirstWhiteOption", false, RoleOption); OptionId++;
    }

    public static void Clear()
    {
        players = new();
        DisplayMode = CustomOptionHolder.SoothSayerDisplayMode.GetBool();
    }
}