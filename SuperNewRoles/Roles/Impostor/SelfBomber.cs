using System;
using System.Collections.Generic;
using AmongUs.GameOptions;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability;
using UnityEngine;

namespace SuperNewRoles.Roles.Impostor;

class SelfBomber : RoleBase<SelfBomber>
{
    public override RoleId Role { get; } = RoleId.SelfBomber;
    public override Color32 RoleColor { get; } = Palette.ImpostorRed;
    public override List<Func<AbilityBase>> Abilities { get; } = [
        () => new AreaKillButtonAbility(
            canKill: () => true,
            killRadius: () => SelfBomberExplosionRadius,
            killCooldown: () => SelfBomberCooldown,
            onlyCrewmates: () => SelfBomberOnlyKillCrewmates,
            targetPlayersInVents: () => true,
            isTargetable: null,
            killedCallback: (killedPlayers) => {
                // 自分自身も爆発に巻き込まれて死亡
                var localPlayer = ExPlayerControl.LocalPlayer;
                localPlayer.RpcCustomDeath(localPlayer, CustomDeathType.SelfBomb);
            },
            customSprite: AssetManager.GetAsset<Sprite>("SelfBomberBomButton.png"),
            customButtonText: ModTranslation.GetString("SelfBomberKillButtonText"),
            customDeathType: CustomDeathType.BombBySelfBomb
        )
    ];

    public override QuoteMod QuoteMod { get; } = QuoteMod.SuperNewRoles;
    public override RoleTypes IntroSoundType { get; } = RoleTypes.Engineer;
    public override short IntroNum { get; } = 1;

    public override AssignedTeamType AssignedTeam { get; } = AssignedTeamType.Impostor;
    public override WinnerTeamType WinnerTeam { get; } = WinnerTeamType.Impostor;
    public override TeamTag TeamTag { get; } = TeamTag.Impostor;
    public override RoleTag[] RoleTags { get; } = [RoleTag.SpecialKiller];
    public override RoleOptionMenuType OptionTeam { get; } = RoleOptionMenuType.Impostor;

    [CustomOptionFloat("SelfBomberCooldown", 2.5f, 60f, 2.5f, 30f)]
    public static float SelfBomberCooldown;

    [CustomOptionFloat("SelfBomberExplosionRadius", 1f, 5f, 0.5f, 3f)]
    public static float SelfBomberExplosionRadius;

    [CustomOptionBool("SelfBomberOnlyKillCrewmates", false)]
    public static bool SelfBomberOnlyKillCrewmates;
}