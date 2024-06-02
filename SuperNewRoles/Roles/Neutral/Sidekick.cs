//◯作り方◯
// 1.ICrewmateかINeutralかIImpostorのどれかを継承する
// 2.必要なインターフェースを実装する
// 3.Roleinfo,Optioninfo,Introinfoを設定する
// 4.設定を作成する(CreateOptionが必要なければOptioninfoのoptionCreatorをnullにする)
// 5.インターフェースの内容を実装していく

using AmongUs.GameOptions;
using SuperNewRoles.Roles.Role;
using SuperNewRoles.Roles.RoleBases;
using SuperNewRoles.Roles.RoleBases.Interfaces;

namespace SuperNewRoles.Roles.Neutral;

public class Sidekick : RoleBase, ISidekick, INeutral, IImpostorVision, IVentAvailable, ISaboAvailable
{
    public static new RoleInfo Roleinfo = new(
        typeof(Sidekick),
        (p) => new Sidekick(p),
        RoleId.Sidekick,
        "Sidekick",
        RoleClass.JackalBlue,
        new(RoleId.Sidekick, TeamTag.Sidekick),
        TeamRoleType.Neutral,
        TeamType.Neutral
        );

    public bool CanUseSabo => Jackal.Optioninfo.CanUseSabo;
    public bool CanUseVent => Jackal.Optioninfo.CanUseVent;
    public bool IsImpostorVision => Jackal.Optioninfo.IsImpostorVision;

    public static new IntroInfo Introinfo =
        new(RoleId.Sidekick, introSound: RoleTypes.Crewmate);
    public Sidekick(PlayerControl p) : base(p, Roleinfo, null, Introinfo)
    {
    }

    public RoleId TargetRole => RoleId.Jackal;

    public void SetParent(PlayerControl player)
    {

    }
}