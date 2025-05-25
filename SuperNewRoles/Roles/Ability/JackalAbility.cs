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

        SidekickAbility = new CustomSidekickButtonAbility(new(
            (bool sidekickCreated) => JackData.CanCreateSidekick && !sidekickCreated,
            () => JackData.SidekickCooldown,
            () => getSidekickType(JackData.SidekickType),
            () => RoleTypes.Crewmate,
            AssetManager.GetAsset<Sprite>("JackalSidekickButton.png"),
            ModTranslation.GetString("SidekickButtonText"),
            sidekickCount: () => 1,
            isTargetable: (player) => !player.IsJackalTeam(),
            sidekickedPromoteData: getPromoteData(JackData.SidekickType),
            onSidekickCreated: (player) =>
            {
                Logger.Info($"OnSidekickCreated: {player.PlayerId}");
                new LateTask(() =>
                {
                    Logger.Info($"OnSidekickCreated2: {player.PlayerId}");
                    if (JackData.SidekickType is JackalSidekickType.Sidekick or JackalSidekickType.SidekickWaveCannon)
                    {
                        Logger.Info($"OnSidekickCreated3: {player.PlayerId}");
                        var jsidekick = player.GetAbility<JSidekickAbility>();
                        if (jsidekick != null)
                        {
                            Logger.Info($"OnSidekickCreated4: {player.PlayerId}");
                            jsidekick.RpcSetCanInfinite(JackData.IsInfiniteJackal);
                        }
                    }
                }, 0.5f, "JackalAbility.OnSidekickCreated");
            }
        ));

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

    private RoleId getSidekickType(JackalSidekickType sidekickType)
    {
        switch (sidekickType)
        {
            case JackalSidekickType.Sidekick:
                return RoleId.Sidekick;
            case JackalSidekickType.Friends:
                return RoleId.JackalFriends;
            case JackalSidekickType.SidekickWaveCannon:
                return RoleId.SidekickWaveCannon;
            default:
                throw new Exception("Invalid sidekick type in getSidekickType: " + sidekickType);
        }
    }

    private SidekickedPromoteData getPromoteData(JackalSidekickType sidekickType)
    {
        switch (sidekickType)
        {
            case JackalSidekickType.Sidekick:
                return new(RoleId.Jackal, RoleTypes.Crewmate);
            case JackalSidekickType.Friends:
                return null;
            case JackalSidekickType.SidekickWaveCannon:
                return new(RoleId.WaveCannonJackal, RoleTypes.Crewmate);
            default:
                throw new Exception("Invalid sidekick type in getPromoteData: " + sidekickType);
        }
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