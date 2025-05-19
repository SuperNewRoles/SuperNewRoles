using System;
using System.Collections.Generic;
using UnityEngine;
using AmongUs.GameOptions;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability.CustomButton;
using SuperNewRoles.Roles.Ability;

namespace SuperNewRoles.Roles.Crewmate;

class ToiletFan : RoleBase<ToiletFan>
{
    public override RoleId Role { get; } = RoleId.ToiletFan;
    public override Color32 RoleColor { get; } = new(116, 80, 48, byte.MaxValue);
    public override List<Func<AbilityBase>> Abilities { get; } = [() => new ToiletFanAbility(ToiletFanCoolTime)];

    public override QuoteMod QuoteMod { get; } = QuoteMod.SuperNewRoles;
    public override RoleTypes IntroSoundType { get; } = RoleTypes.Crewmate;
    public override short IntroNum { get; } = 1;

    public override AssignedTeamType AssignedTeam { get; } = AssignedTeamType.Crewmate;
    public override WinnerTeamType WinnerTeam { get; } = WinnerTeamType.Crewmate;
    public override TeamTag TeamTag { get; } = TeamTag.Crewmate;
    public override RoleTag[] RoleTags { get; } = [];
    public override RoleOptionMenuType OptionTeam { get; } = RoleOptionMenuType.Crewmate;
    public override MapNames[] AvailableMaps { get; } = [MapNames.Airship];

    [CustomOptionFloat("ToiletFanCoolTime", 0f, 60f, 2.5f, 20f, translationName: "CoolTime")]
    public static float ToiletFanCoolTime;
}

public class ToiletFanAbility : CustomButtonBase
{
    private float cooltime;
    public override float DefaultTimer => cooltime;
    public override string buttonText => ModTranslation.GetString("ToiletFan.ButtonTitle");
    public override Sprite Sprite => AssetManager.GetAsset<Sprite>("ToiletFanButton.png");
    protected override KeyType keytype => KeyType.Ability1;

    public ToiletFanAbility(float cooltime)
    {
        this.cooltime = cooltime;
    }

    public override bool CheckIsAvailable()
    {
        if (Player.Data.IsDead || MeetingHud.Instance) return false;
        return Player.Player.CanMove;
    }

    public override void OnClick()
    {
        ModHelpers.RpcOpenToilet(Player);
    }
}