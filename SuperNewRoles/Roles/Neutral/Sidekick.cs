using System.Text;
using AmongUs.GameOptions;
using SuperNewRoles.Mode.SuperHostRoles;
using SuperNewRoles.Roles.Role;
using SuperNewRoles.Roles.RoleBases;
using SuperNewRoles.Roles.RoleBases.Interfaces;

namespace SuperNewRoles.Roles.Neutral;

public class Sidekick : RoleBase, ISidekick, INeutral, IImpostorVision, IVentAvailable, ISaboAvailable, ISupportSHR, ISHRAntiBlackout
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
    public bool? IsImpostorLight => IsImpostorVision;

    public static new IntroInfo Introinfo =
        new(RoleId.Sidekick, introSound: RoleTypes.Crewmate);
    public Sidekick(PlayerControl p) : base(p, Roleinfo, null, Introinfo)
    {
    }

    public RoleId TargetRole => RoleId.Jackal;

    public RoleTypes RealRole => CanUseVent ? RoleTypes.Engineer : RoleTypes.Crewmate;

    private Jackal CurrentParent;

    public void SetParent(PlayerControl player)
        => CurrentParent = player.GetRoleBase<Jackal>();

    public void BuildSetting(IGameOptions gameOptions)
    {
        gameOptions.SetFloat(FloatOptionNames.EngineerCooldown, 0f);
        gameOptions.SetFloat(FloatOptionNames.EngineerInVentMaxTime, 0f);
    }

    public void BuildName(StringBuilder Suffix, StringBuilder RoleNameText, PlayerData<string> ChangePlayers)
    {
        if (CurrentParent is null)
            return;
        if (CurrentParent.Player != null)
            ChangePlayers[CurrentParent.Player] = ModHelpers.Cs(RoleClass.JackalBlue, ChangePlayers.GetNowName(CurrentParent.Player));
        else if (Player.IsAlive())
            ChangePlayers[CurrentParent.Player] = ModHelpers.Cs(RoleClass.CrewmateWhite, ChangePlayers.GetNowName(CurrentParent.Player));
    }

    public void StartAntiBlackout()
    {
        if (CurrentParent?.Player != null && !Player.IsMod())
            CurrentParent.Player.RpcSetRoleDesync(CurrentParent.Player.IsDead() ? RoleTypes.CrewmateGhost : RoleTypes.Crewmate, Player);
    }

    public void EndAntiBlackout()
    {
        if (CurrentParent?.Player != null && !Player.IsMod() && CurrentParent.Player.IsAlive())
            CurrentParent.Player.RpcSetRoleDesync(RoleTypes.Impostor, Player);
    }
}