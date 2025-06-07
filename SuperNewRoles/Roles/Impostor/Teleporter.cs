using System;
using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability;
using SuperNewRoles.Roles.Ability.CustomButton;
using UnityEngine;

namespace SuperNewRoles.Roles.Impostor;

internal class Teleporter : RoleBase<Teleporter>
{
    public override RoleId Role { get; } = RoleId.Teleporter;
    public override Color32 RoleColor { get; } = Palette.ImpostorRed;
    public override List<Func<AbilityBase>> Abilities { get; } = [
        () => new TeleporterAbility(
            new TeleporterAbilityData(TeleporterCooldown, TeleporterWaitingTime)
        )
    ];

    public override QuoteMod QuoteMod { get; } = QuoteMod.SuperNewRoles;
    public override RoleTypes IntroSoundType { get; } = RoleTypes.Impostor;
    public override short IntroNum { get; } = 1;

    public override AssignedTeamType AssignedTeam { get; } = AssignedTeamType.Impostor;
    public override WinnerTeamType WinnerTeam { get; } = WinnerTeamType.Impostor;
    public override TeamTag TeamTag { get; } = TeamTag.Impostor;
    public override RoleTag[] RoleTags { get; } = [RoleTag.Support, RoleTag.ImpostorTeam];
    public override RoleOptionMenuType OptionTeam { get; } = RoleOptionMenuType.Impostor;

    [CustomOptionFloat("TeleporterCooldown", 5f, 120f, 5f, 45f, translationName: "CoolTime")]
    public static float TeleporterCooldown;
    [CustomOptionFloat("TeleporterWaitingTime", 0f, 10f, 1f, 3f)]
    public static float TeleporterWaitingTime;

}

public record TeleporterAbilityData(float cooldown, float waitingTime);
internal class TeleporterAbility : CustomButtonBase, IButtonEffect
{
    public override float DefaultTimer => Data.cooldown;

    public override string buttonText => ModTranslation.GetString("TeleporterAbilityButtonText");

    public override Sprite Sprite => AssetManager.GetAsset<Sprite>("TeleporterButton.png");

    protected override KeyType keytype => KeyType.Ability1;

    public TeleporterAbilityData Data { get; }
    public bool isEffectActive { get; set; }

    public Action OnEffectEnds => Teleport;

    public float EffectDuration => Data.waitingTime;

    public float EffectTimer { get; set; }
    private CustomMessage CurrentMessage;

    public TeleporterAbility(TeleporterAbilityData data)
    {
        Data = data;
    }

    public override bool CheckIsAvailable()
    {
        return PlayerControl.LocalPlayer.CanMove;
    }

    public override void OnClick()
    {
        RpcShowTeleportWaitingEffect(ExPlayerControl.LocalPlayer, Data.waitingTime);
    }

    private void Teleport()
    {
        ExPlayerControl targetPlayer = ExPlayerControl.ExPlayerControls.Where(p => p.IsAlive() && p.moveable).ToList().GetRandom();
        RpcAllTeleportTo(targetPlayer.transform.position, targetPlayer);
    }

    [CustomRPC]
    public void RpcShowTeleportWaitingEffect(ExPlayerControl player, float waitingTime)
    {
        ShowTeleportWaitingEffect(player, waitingTime);
    }

    private void ShowTeleportWaitingEffect(ExPlayerControl player, float waitingTime)
    {
        CurrentMessage = new CustomMessage(ModTranslation.GetString("TeleporterWaitingText", waitingTime), waitingTime == 1 ? 2 : 1, true);
        if (waitingTime > 1 && player.IsAlive())
        {
            new LateTask(() =>
            {
                ShowTeleportWaitingEffect(player, waitingTime - 1f);
            }, 1f);
        }
    }

    [CustomRPC]
    public void RpcAllTeleportTo(Vector2 position, ExPlayerControl target)
    {
        foreach (var player in ExPlayerControl.ExPlayerControls)
            player.NetTransform.SnapTo(position);
        ExPlayerControl.LocalPlayer.RpcCustomSnapTo(position);
        if (CurrentMessage != null)
        {
            GameObject.Destroy(CurrentMessage.text.gameObject);
            CurrentMessage = null;
        }
        CurrentMessage = new CustomMessage(ModTranslation.GetString("TeleporterTeleportText", target.Data.PlayerName), 1, true);
    }
}