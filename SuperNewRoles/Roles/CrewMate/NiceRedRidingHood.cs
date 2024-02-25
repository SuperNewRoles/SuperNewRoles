
using AmongUs.GameOptions;
using SuperNewRoles.Helpers;
using SuperNewRoles.Roles.Role;
using SuperNewRoles.Roles.RoleBases;
using SuperNewRoles.Roles.RoleBases.Interfaces;
using System.Linq;
namespace SuperNewRoles.Roles.Crewmate.NiceRedRidingHood;

public class NiceRedRidingHood : RoleBase, ICrewmate, IWrapUpHandler, INameHandler, IHaveHauntAbility
{
    public static new RoleInfo Roleinfo = new(
        typeof(NiceRedRidingHood),
        (p) => new NiceRedRidingHood(p),
        RoleId.NiceRedRidingHood,
        "NiceRedRidingHood",
        new(250, 128, 114, byte.MaxValue),
        new(RoleId.NiceRedRidingHood, TeamTag.Crewmate),
        TeamRoleType.Crewmate,
        TeamType.Crewmate
        );
    public static new OptionInfo Optioninfo =
        new(RoleId.NiceRedRidingHood, 403400, false,
            optionCreator: CreateOption);
    public static new IntroInfo Introinfo =
        new(RoleId.NiceRedRidingHood, introSound: RoleTypes.Crewmate);

    public static CustomOption NiceRedRidingHoodCount;
    public static CustomOption NiceRedRidinIsKillerDeathRevive;
    private static void CreateOption()
    {
        NiceRedRidingHoodCount = CustomOption.Create(Optioninfo.OptionId++, Optioninfo.SupportSHR, Optioninfo.RoleOption.type, "NiceRedRidingHoodCount", 1f, 1f, 15f, 1f, Optioninfo.RoleOption);
        NiceRedRidinIsKillerDeathRevive = CustomOption.Create(Optioninfo.OptionId++, Optioninfo.SupportSHR, Optioninfo.RoleOption.type, "NiceRedRidinIsKillerDeathRevive", true, Optioninfo.RoleOption);
    }

    public int RemainingCount;
    public NiceRedRidingHood(PlayerControl p) : base(p, Roleinfo, Optioninfo, Introinfo)
    {
        RemainingCount = NiceRedRidingHoodCount.GetInt();
    }

    public void OnWrapUp(PlayerControl exiled)
    {
        if (exiled == null || Player == null || Player.IsAlive() || !Player.IsRole(RoleId.NiceRedRidingHood)) return;

        Logger.Info($"赤ずきん残り復活回数 : {RemainingCount}", Roleinfo.NameKey);
        if (RemainingCount <= 0) return;

        DeadPlayer deadPlayer = DeadPlayer.deadPlayers?.Where(x => x.player?.PlayerId == Player.PlayerId)?.FirstOrDefault();
        if (deadPlayer.killerIfExisting == null) return;

        var killer = PlayerControl.AllPlayerControls.FirstOrDefault((PlayerControl a) => a.PlayerId == deadPlayer.killerIfExistingId);
        if (killer == null) return;

        if ((killer.PlayerId == exiled.PlayerId) || (NiceRedRidinIsKillerDeathRevive.GetBool() && killer.IsDead()))
        {
            bool IsDisabledRevive = EvilEraser.IsBlock(EvilEraser.BlockTypes.RedRidingHoodRevive, killer);
            Logger.Info($"復活可否 : {!IsDisabledRevive}", Roleinfo.NameKey);

            if (IsDisabledRevive) return;

            Player.Revive();
            FastDestroyableSingleton<RoleManager>.Instance.SetRole(Player, RoleTypes.Crewmate);
            DeadPlayer.deadPlayers?.RemoveAll(x => x.player?.PlayerId == Player.PlayerId);
            Patches.FinalStatusPatch.FinalStatusData.FinalStatuses[Player.PlayerId] = FinalStatus.Alive;

            RemainingCount--;
            Player.Data.IsDead = false;

            Logger.Info($"復活完了 : {!IsDisabledRevive}", Roleinfo.NameKey);
        }
    }

    public bool CanGhostSeeRole => GhostSeeRoleStatus;
    public bool CanUseHauntAbility => GhostSeeRoleStatus;
    private bool GhostSeeRoleStatus =>
        RemainingCount <= 0 // 復活可能回数を使い切った場合
            ? true
            : DeadPlayer.deadPlayers?.FirstOrDefault(x => x.player?.PlayerId == Player.PlayerId)?.killerIfExisting == null // 追放, 又はキル者が登録されていない場合
                ? true
                : false;
}