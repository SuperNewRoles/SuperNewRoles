using AmongUs.GameOptions;
using SuperNewRoles.Roles.Role;
using SuperNewRoles.Roles.RoleBases;
using SuperNewRoles.Roles.RoleBases.Interfaces;

namespace SuperNewRoles.Roles.Neutral;

public class SidekickWaveCannon : RoleBase, ISidekick, INeutral, IVentAvailable, ISaboAvailable, IImpostorVision
{
    public static new RoleInfo Roleinfo = new(
        typeof(SidekickWaveCannon),
        (p) => new SidekickWaveCannon(p),
        RoleId.SidekickWaveCannon,
        "SidekickWaveCannon",
        RoleClass.JackalBlue,
        new(RoleId.SidekickWaveCannon, TeamTag.Jackal),
        TeamRoleType.Neutral,
        TeamType.Neutral
        );

    public bool CanUseSabo => WaveCannonJackal.Optioninfo.CanUseSabo;
    public bool CanUseVent => WaveCannonJackal.Optioninfo.CanUseVent;
    public bool IsImpostorVision => WaveCannonJackal.Optioninfo.IsImpostorVision;

    public RoleId TargetRole => RoleId.WaveCannonJackal;
    public WaveCannonJackal SidekickedParent;

    public static new IntroInfo Introinfo =
        new(RoleId.SidekickWaveCannon, introSound: RoleTypes.Shapeshifter);
    public SidekickWaveCannon(PlayerControl p) : base(p, Roleinfo, null, Introinfo)
    {
    }

    public void SetParent(PlayerControl player)
    {
        SidekickedParent = player?.GetRoleBase<WaveCannonJackal>();
    }
}