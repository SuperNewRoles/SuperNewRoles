using System;
using AmongUs.GameOptions;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Neutral;
using UnityEngine;

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
            () => JackData.SidekickType,
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
                    if (JackData.SidekickType is RoleId.Sidekick or RoleId.SidekickWaveCannon or RoleId.Bullet)
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
            },
            isSubButton: JackData.HasOtherButton
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

    private SidekickedPromoteData getPromoteData(RoleId sidekickType)
    {
        switch (sidekickType)
        {
            case RoleId.Sidekick:
                return new(RoleId.Jackal, RoleTypes.Crewmate);
            case RoleId.JackalFriends:
                return null;
            case RoleId.SidekickWaveCannon:
                return new(RoleId.WaveCannonJackal, RoleTypes.Crewmate);
            case RoleId.Bullet:
                return new(WaveCannonJackal.WaveCannonJackalCreateBulletToJackal ? RoleId.WaveCannonJackal : RoleId.Bullet, RoleTypes.Crewmate);
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
    public RoleId SidekickType { get; }
    /// <summary>Kill能力とSK能力以外のパッシブアビリティを有するジャッカル役職か</summary>
    /// <value>true => 有する, false => 有さない</value>
    public bool HasOtherButton { get; private set; }

    /// <summary>ジャッカルの基本能力(Kill, Vent, SK, ImpostorVision)</summary>
    /// <param name="canKill"></param>
    /// <param name="killCooldown"></param>
    /// <param name="canUseVent"></param>
    /// <param name="canCreateSidekick"></param>
    /// <param name="sidekickCooldown"></param>
    /// <param name="isImpostorVision"></param>
    /// <param name="isInfiniteJackal">無限ジャッカル</param>
    /// <param name="sidekickType">SK先の役職</param>
    /// <param name="hasOtherButton">基本能力以外のパッシブアビリティを有するジャッカル役職か</param>
    public JackalData(
        bool canKill,
        float killCooldown,
        bool canUseVent,
        bool canCreateSidekick,
        float sidekickCooldown,
        bool isImpostorVision = true,
        bool isInfiniteJackal = true,
        RoleId sidekickType = RoleId.Sidekick,
        bool hasOtherButton = false)
    {
        CanKill = canKill;
        KillCooldown = killCooldown;
        CanUseVent = canUseVent;
        CanCreateSidekick = canCreateSidekick;
        SidekickCooldown = sidekickCooldown;
        IsImpostorVision = isImpostorVision;
        IsInfiniteJackal = isInfiniteJackal;
        SidekickType = sidekickType;
        HasOtherButton = hasOtherButton;
    }
}