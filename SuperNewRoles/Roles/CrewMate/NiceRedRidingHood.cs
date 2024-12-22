
using AmongUs.GameOptions;
using MS.Internal.Xml.XPath;
using SuperNewRoles.Helpers;
using SuperNewRoles.Mode;
using SuperNewRoles.Roles.Role;
using SuperNewRoles.Roles.RoleBases;
using SuperNewRoles.Roles.RoleBases.Interfaces;
using System.Linq;
namespace SuperNewRoles.Roles.Crewmate.NiceRedRidingHood;

public class NiceRedRidingHood : RoleBase, ICrewmate, IWrapUpHandler, INameHandler, IHaveHauntAbility, ISupportSHR
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

    public RoleTypes RealRole => RoleTypes.Crewmate;

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

    public void OnWrapUp()
    {
        WrapUp();
    }
    public void OnWrapUp(PlayerControl exiled)
    {
        if (exiled == null) return;
        WrapUp(exiled);
    }

    public void WrapUp(PlayerControl exiled = null)
    {
        if (Player == null || Player.IsAlive() || !Player.IsRole(RoleId.NiceRedRidingHood)) return;

        bool canRevive;
        PlayerControl killer;
        if (exiled != null) canRevive = IsRevivableBasicConditions(out killer, false) && killer.PlayerId == exiled.PlayerId;  // 基本条件 + 追放者がキラーか
        else canRevive = IsRevivableBasicConditions(out killer) && NiceRedRidinIsKillerDeathRevive.GetBool() && killer.IsDead();  // 基本条件 + キラーが死亡しているか

        if (canRevive && !ReviveAbilityBlockEnabled(killer))
        {
            Logger.Info($"復活判定(キル者[{(exiled != null ? "追放" : "キル")}]) : 可", Roleinfo.NameKey);
            Revive();
        }
        else Logger.Info($"復活判定(キル者[{(exiled != null ? "追放" : "キル")}]) : 不可", Roleinfo.NameKey);
    }

    /// <summary>
    /// 赤ずきんの基本的な復活条件(使用回数が残っているか, 追放による死亡或いは自殺でないか)を満たしているかの判定
    /// 及び赤ずきんのキラーの取得を行う
    /// </summary>
    /// <param name="killer">赤ずきんをキルしたプレイヤー</param>
    /// <param name="log">ログを記載するか(追放無関係 WrapUpと, 追放有 WrapUpでの二重記載を防ぐ為に使用)</param>
    /// <returns>true: 復活 可 / false: 復活 不可</returns>
    private bool IsRevivableBasicConditions(out PlayerControl killer, bool log = true)
    {
        killer = null;

        if (log) Logger.Info($"復活判定(残り復活回数) : {RemainingCount}回", Roleinfo.NameKey);
        if (RemainingCount <= 0) return false;

        killer = Killer();
        if (log) Logger.Info($"復活判定(赤ずきん[追放, 自殺]) : {(killer == null || killer.PlayerId == Player.PlayerId ? "可" : "不可")}", Roleinfo.NameKey);
        if (killer == null || killer.PlayerId == Player.PlayerId) return false; // キラーが存在しない(追放) 或いは キラーが自分自身なら復活不可

        return true;

        PlayerControl Killer() // 赤ずきんをキルしたプレイヤーを取得
        {
            DeadPlayer deadPlayer = DeadPlayer.deadPlayers?.Where(x => x.player?.PlayerId == Player.PlayerId)?.FirstOrDefault();
            if (deadPlayer.killerIfExisting == null) return null;

            return PlayerControl.AllPlayerControls.FirstOrDefault((PlayerControl a) => a.PlayerId == deadPlayer.killerIfExistingId);
        }
    }

    /// <summary>赤ずきんの復活処理</summary>
    private void Revive()
    {
        Player.Revive();
        if (AmongUsClient.Instance.AmHost && ModeHandler.IsMode(ModeId.SuperHostRoles))
        {
            CustomRpcSender sender = CustomRpcSender.Create("Nice RedRidingHood", sendOption: Hazel.SendOption.Reliable);
            Player.Data.IsDead = true;
            RPCHelper.RpcSyncNetworkedPlayer(sender, Player.Data);
            sender.RpcSetRole(Player, RoleTypes.Crewmate, true);
        }

        FastDestroyableSingleton<RoleManager>.Instance.SetRole(Player, RoleTypes.Crewmate);
        DeadPlayer.deadPlayers?.RemoveAll(x => x.player?.PlayerId == Player.PlayerId);
        Patches.FinalStatusPatch.FinalStatusData.FinalStatuses[Player.PlayerId] = FinalStatus.Alive;

        RemainingCount--;
        Player.Data.IsDead = false;

        Logger.Info($"復活完了", Roleinfo.NameKey);
    }

    /// <summary>キラーが赤ずきんの復活を阻止するか</summary>
    /// <param name="killer">赤ずきんをキルしたプレイヤー</param>
    /// <returns>true: 阻止する / false: 阻止しない</returns>
    private static bool ReviveAbilityBlockEnabled(PlayerControl killer)
    {
        if (killer == null) return false; // キラーが存在しない場合は, キラーによる復活能力ブロックは存在しない

        if (EvilEraser.IsBlock(EvilEraser.BlockTypes.RedRidingHoodRevive, killer)) return true;

        return false;
    }

    public bool CanGhostSeeRole => GhostSeeRoleStatus();
    public bool CanUseHauntAbility => GhostSeeRoleStatus();
    private bool GhostSeeRoleStatus()
    {
        var killer = DeadPlayer.deadPlayers?.FirstOrDefault(x => x.player?.PlayerId == Player.PlayerId)?.killerIfExisting;
        return RemainingCount <= 0 || killer == null || killer.PlayerId == Player.PlayerId;
    }
}