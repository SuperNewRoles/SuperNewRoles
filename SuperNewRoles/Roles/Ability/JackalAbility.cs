using System;
using UnityEngine;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability.CustomButton;
using AmongUs.GameOptions;
using SuperNewRoles.Roles.Neutral;
using System.Linq;

namespace SuperNewRoles.Roles.Ability;

public class JackalAbility : AbilityBase
{
    public JackalData JackData { get; private set; }
    public CustomKillButtonAbility KillAbility { get; private set; }
    public CustomVentAbility VentAbility { get; private set; }
    public CustomSidekickButtonAbility SidekickAbility { get; private set; }
    public KnowOtherAbility KnowJackalAbility { get; private set; }
    public ImpostorVisionAbility ImpostorVisionAbility { get; private set; }

    public JackalAbility(JackalData jackData)
    {
        JackData = jackData;
    }

    public override void AttachToAlls()
    {
        KillAbility = new CustomKillButtonAbility(
            () => JackData.CanKill,
            () => JackData.KillCooldown,
            onlyCrewmates: () => false,
            isTargetable: (player) => !player.IsJackalTeam()
        );

        VentAbility = new CustomVentAbility(
            () => JackData.CanUseVent
        );

        SidekickAbility = new CustomSidekickButtonAbility(
            (bool sidekickCreated) => JackData.CanCreateSidekick && !sidekickCreated,
            () => JackData.SidekickCooldown,
            () => JackData.SidekickType == JackalSidekickType.Friends ? RoleId.JackalFriends : RoleId.Sidekick,
            () => RoleTypes.Crewmate,
            AssetManager.GetAsset<Sprite>("JackalSidekickButton.png"),
            ModTranslation.GetString("SidekickButtonText"),
            sidekickCount: () => 1,
            isTargetable: (player) => !player.IsJackalTeam(),
            sidekickedPromoteData: JackData.SidekickType == JackalSidekickType.Sidekick ? new(RoleId.Jackal, RoleTypes.Crewmate) : null,
            onSidekickCreated: (player) =>
            {
                new LateTask(() =>
                {
                    if (JackData.SidekickType == JackalSidekickType.Sidekick)
                    {
                        Logger.Info("Sidekick created: " + player.Data.PlayerName);
                        var jsidekick = player.PlayerAbilities.FirstOrDefault(x => x is JSidekickAbility);
                        Logger.Info("jsidekick: " + jsidekick);
                        if (jsidekick is JSidekickAbility jsidekickAbility)
                        {
                            Logger.Info("jsidekickAbility: " + jsidekickAbility);
                            JSidekickAbility.RpcSetCanInfinite(JackData.IsInfiniteJackal, jsidekickAbility);
                        }
                    }
                }, 0.5f);
            }
        );

        KnowJackalAbility = new KnowOtherAbility(
            (player) => player.IsJackalTeam(),
            () => true
        );

        ImpostorVisionAbility = new ImpostorVisionAbility(
            () => JackData.IsImpostorVision
        );

        AbilityParentAbility parentAbility = new(this);
        Player.AttachAbility(KillAbility, parentAbility);
        Player.AttachAbility(VentAbility, parentAbility);
        Player.AttachAbility(SidekickAbility, parentAbility);
        Player.AttachAbility(KnowJackalAbility, parentAbility);
        Player.AttachAbility(ImpostorVisionAbility, parentAbility);
    }

    public override void AttachToLocalPlayer()
    {
    }
}

public class JackalData
{
    public bool CanKill { get; }
    public float KillCooldown { get; }
    public bool CanUseVent { get; }
    public bool CanCreateSidekick { get; set; }
    public float SidekickCooldown { get; }
    public bool IsImpostorVision { get; }
    public bool IsInfiniteJackal { get; }
    public JackalSidekickType SidekickType { get; }

    public JackalData(
        bool canKill,
        float killCooldown,
        bool canUseVent,
        bool canCreateSidekick,
        float sidekickCooldown,
        bool isImpostorVision = true,
        bool isInfiniteJackal = true,
        JackalSidekickType sidekickType = JackalSidekickType.Sidekick)
    {
        CanKill = canKill;
        KillCooldown = killCooldown;
        CanUseVent = canUseVent;
        CanCreateSidekick = canCreateSidekick;
        SidekickCooldown = sidekickCooldown;
        IsImpostorVision = isImpostorVision;
        IsInfiniteJackal = isInfiniteJackal;
        SidekickType = sidekickType;
    }
}