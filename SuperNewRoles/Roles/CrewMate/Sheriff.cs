using System;
using System.Collections.Generic;
using UnityEngine;
using AmongUs.GameOptions;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Roles.Ability;
using SuperNewRoles.Roles.Ability.CustomButton;
using SuperNewRoles.Modules;
using SuperNewRoles.Events.PCEvents;
using SuperNewRoles.Modules.Events.Bases;

namespace SuperNewRoles.Roles.Crewmate;

class Sheriff : RoleBase<Sheriff>
{
    public override RoleId Role { get; } = RoleId.Sheriff;
    public override Color32 RoleColor { get; } = new(255, 255, 0, byte.MaxValue); // 黄色
    public override List<Func<AbilityBase>> Abilities { get; } = [() => new SheriffAbility(SheriffKillCooldown, SheriffCanKillNeutral)];

    public override QuoteMod QuoteMod { get; } = QuoteMod.TheOtherRoles;
    public override RoleTypes IntroSoundType { get; } = RoleTypes.Crewmate;
    public override short IntroNum { get; } = 1;

    public override AssignedTeamType AssignedTeam { get; } = AssignedTeamType.Crewmate;
    public override WinnerTeamType WinnerTeam { get; } = WinnerTeamType.Crewmate;
    public override TeamTag TeamTag { get; } = TeamTag.Crewmate;
    public override RoleTag[] RoleTags { get; } = [];
    public override RoleOptionMenuType OptionTeam { get; } = RoleOptionMenuType.Crewmate;

    [CustomOptionFloat("SheriffKillCooldown", 0f, 60f, 2.5f, 25f)]
    public static float SheriffKillCooldown;

    [CustomOptionBool("SheriffCanKillNeutral", true)]
    public static bool SheriffCanKillNeutral;

    [CustomOptionBool("SheriffCanKillImpostor", true)]
    public static bool SheriffCanKillImpostor;
}

public class SheriffAbility : TargetCustomButtonBase
{
    public float KillCooldown { get; }
    public bool CanKillNeutral { get; }
    public override Color32 OutlineColor => Sheriff.Instance.RoleColor;

    private float cooldownTimer = 0f;
    public override float Timer { get => cooldownTimer; set => cooldownTimer = value; }
    public override float DefaultTimer => KillCooldown;
    public override Vector3 PositionOffset => new(0f, 1f, 0f);
    public override Vector3 LocalScale => Vector3.one;
    public override string buttonText => "Kill";
    public override Sprite Sprite => AssetManager.GetAsset<Sprite>("SheriffKillButton.png");
    public override Color? color => Color.yellow;
    protected override KeyCode? hotkey => KeyCode.Q;
    protected override int joystickkey => 0;
    public override bool OnlyCrewmates => false;
    public SheriffAbility(float killCooldown, bool canKillNeutral)
    {
        KillCooldown = killCooldown;
        CanKillNeutral = canKillNeutral;
    }
    public override bool CheckIsAvailable()
    {
        return TargetIsExist;
    }

    public override bool CheckHasButton() => !PlayerControl.LocalPlayer.Data.IsDead;

    public override void OnClick()
    {
        // インポスターまたは設定で許可された場合のニュートラルを殺害
        if (Target.IsImpostor() || (CanKillNeutral && Target.IsNeutral()))
        {
            // TODO: 後でRPCをCustomにする
            PlayerControl.LocalPlayer.RpcMurderPlayer(Target.Player, true);
        }
        else
        {
            // 無実の人を殺そうとした場合、自分が死亡
            PlayerControl.LocalPlayer.RpcMurderPlayer(PlayerControl.LocalPlayer, true);
        }
        Logger.Info("Clicked Sheriff Kill Button");
    }
}
