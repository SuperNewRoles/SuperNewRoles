using System.Linq;
using AmongUs.GameOptions;
using Hazel;
using SuperNewRoles.Mode;
using SuperNewRoles.Mode.SuperHostRoles;
using SuperNewRoles.Roles.Role;
using SuperNewRoles.Roles.RoleBases;
using SuperNewRoles.Roles.RoleBases.Interfaces;

namespace SuperNewRoles.Roles.Crewmate;

public class Chief : RoleBase, ICrewmate, ICustomButton, IRpcHandler, ISupportSHR, ICheckMurderHandler
{
    public static new RoleInfo Roleinfo = new(
        typeof(Chief),
        (p) => new Chief(p),
        RoleId.Chief,
        "Chief",
        RoleClass.SheriffYellow,
        new(RoleId.Chief, TeamTag.Crewmate),
        TeamRoleType.Crewmate,
        TeamType.Crewmate
        );
    public static new OptionInfo Optioninfo =
        new(RoleId.Chief, 400301, true,
            optionCreator: CreateOption);
    public static new IntroInfo Introinfo =
        new(RoleId.Chief, introSound: RoleTypes.Tracker);

    public static CustomOption ChiefSheriffCoolTime;
    public static CustomOption ChiefSheriffKillLimit;
    public static CustomOption ChiefSheriffExecutionMode;
    public static CustomOption ChiefSheriffCanKillImpostor;
    public static CustomOption ChiefSheriffCanKillMadRole;
    public static CustomOption ChiefSheriffCanKillNeutral;
    public static CustomOption ChiefSheriffFriendsRoleKill;
    public static CustomOption ChiefSheriffCanKillLovers;
    public static CustomOption ChiefSheriffQuarreledKill;

    public CustomButtonInfo[] CustomButtonInfos { get; }

    public RoleTypes RealRole => RoleTypes.Crewmate;
    public RoleTypes DesyncRole => IsCreatedSheriff ? RealRole : RoleTypes.Impostor;

    private CustomButtonInfo SidekickButton;

    private bool IsCreatedSheriff;
    public byte CreatedSheriff = byte.MaxValue;

    private static void CreateOption()
    {
        ChiefSheriffCoolTime = CustomOption.Create(400303, Optioninfo.SupportSHR, CustomOptionType.Crewmate, "SheriffCooldownSetting", 30f, 2.5f, 60f, 2.5f, Optioninfo.RoleOption, format: "unitSeconds");
        ChiefSheriffKillLimit = CustomOption.Create(400304, Optioninfo.SupportSHR, CustomOptionType.Crewmate, "SheriffMaxKillCountSetting", 1f, 1f, 20f, 1, Optioninfo.RoleOption, format: "unitSeconds");
        ChiefSheriffExecutionMode = CustomOption.Create(400312, Optioninfo.SupportSHR, CustomOptionType.Crewmate, "SheriffExecutionMode", new string[] { "SheriffDefaultExecutionMode", "SheriffAlwaysSuicideMode", "SheriffAlwaysKillMode" }, Optioninfo.RoleOption);
        ChiefSheriffCanKillImpostor = CustomOption.Create(400306, Optioninfo.SupportSHR, CustomOptionType.Crewmate, "SheriffIsKillImpostorSetting", true, Optioninfo.RoleOption);
        ChiefSheriffCanKillMadRole = CustomOption.Create(400307, Optioninfo.SupportSHR, CustomOptionType.Crewmate, "SheriffIsKillMadRoleSetting", false, Optioninfo.RoleOption);
        ChiefSheriffCanKillNeutral = CustomOption.Create(400308, Optioninfo.SupportSHR, CustomOptionType.Crewmate, "SheriffIsKillNeutralSetting", false, Optioninfo.RoleOption);
        ChiefSheriffFriendsRoleKill = CustomOption.Create(400309, Optioninfo.SupportSHR, CustomOptionType.Crewmate, "SheriffIsKillFriendsRoleSetting", false, Optioninfo.RoleOption);
        ChiefSheriffCanKillLovers = CustomOption.Create(400310, Optioninfo.SupportSHR, CustomOptionType.Crewmate, "SheriffIsKillLoversSetting", false, Optioninfo.RoleOption);
        ChiefSheriffQuarreledKill = CustomOption.Create(400311, Optioninfo.SupportSHR, CustomOptionType.Crewmate, "SheriffIsKillQuarreledSetting", false, Optioninfo.RoleOption);
    }
    public static bool IsSheriffCreatedByChief(byte playerId)
    {
        return RoleBaseManager.GetRoleBaseOrigins<Chief>().Any(x => (x as Chief).CreatedSheriff == playerId);
    }

    public bool OnCheckMurderPlayerAmKiller(PlayerControl target)
    {
        if (target.IsImpostor())
        {
            Player.RpcMurderPlayer(Player, true);
            Player.RpcSetFinalStatus(FinalStatus.ChiefMisSet);
            return false;
        }
        if (!IsCreatedSheriff)
        {
            MessageWriter writer = RpcWriter;
            writer.Write(target.PlayerId);
            writer.Write(target.IsClearTask());
            SendRpc(writer);
        }
        return false;
    }

    public Chief(PlayerControl p) : base(p, Roleinfo, Optioninfo, Introinfo)
    {
        SidekickButton = new(null, this, SidekickOnClick,
            (isAlive) => isAlive && !IsCreatedSheriff,
            CustomButtonCouldType.CanMove | CustomButtonCouldType.SetTarget,
            null, ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.ChiefSidekickButton.png", 115f),
            () => 0f, new(), "ChiefSidekickButtonName",
            UnityEngine.KeyCode.F, 49);
        CustomButtonInfos = [SidekickButton];
    }
    private void SidekickOnClick()
    {
        var target = SidekickButton.CurrentTarget;
        if (!target || IsCreatedSheriff)
            return;
        if (target.IsImpostor())
        {
            PlayerControl.LocalPlayer.RpcMurderPlayer(PlayerControl.LocalPlayer, true);
            PlayerControl.LocalPlayer.RpcSetFinalStatus(FinalStatus.ChiefMisSet);
        }
        else
        {
            MessageWriter writer = RpcWriter;
            writer.Write(target.PlayerId);
            writer.Write(target.IsClearTask());
            SendRpc(writer);
        }
    }

    public void RpcReader(MessageReader reader)
    {
        byte targetid = reader.ReadByte();
        CreatedSheriff = targetid;
        IsCreatedSheriff = true;
        RPCProcedure.SetRole(targetid, (byte)RoleId.Sheriff);
        if (targetid == CachedPlayer.LocalPlayer.PlayerId)
        {
            Sheriff.ResetKillCooldown();
            RoleClass.Sheriff.KillMaxCount = ChiefSheriffKillLimit.GetFloat();
            RoleClass.Sheriff.CoolTime = ChiefSheriffCoolTime.GetFloat();
        }
        if (AmongUsClient.Instance.AmHost && ModeHandler.IsMode(ModeId.SuperHostRoles))
        {
            PlayerControl target = ModHelpers.PlayerById(targetid);
            target.RpcSetRole(target.IsMod() ? RoleTypes.Crewmate : RoleTypes.Tracker, true);
            new LateTask(() =>
            {
                CustomRpcSender sender = CustomRpcSender.Create("CreateSheriffByChief", SendOption.Reliable);
                if (!target.IsMod())
                {
                    Logger.Info("Target is not Modded.");
                    int clientId = target.GetClientId();
                    target.RpcSetRoleDesync(RoleTypes.Impostor, true);
                    foreach (PlayerControl player in CachedPlayer.AllPlayers.AsSpan())
                    {
                        if (player.PlayerId == target.PlayerId)
                            continue;
                        player.RpcSetRoleDesync(RoleTypes.Scientist, true, target);
                    }
                }
                ChangeName.SetRoleName(target);
                target.RpcShowGuardEffect(target);
            }, 0.25f);

            // もうキルボタンを持たないように
            Player.RpcSetRole(RoleTypes.Crewmate, true);
            // 暗転対策のために、インポスターをちゃんとインポスターに変更する
            foreach (PlayerControl seetarget in CachedPlayer.AllPlayers.AsSpan())
            {
                if (seetarget.IsImpostor())
                    seetarget.RpcSetRoleDesync(seetarget.Data.Role.Role, true, Player);
            }
        }
        RPCProcedure.UncheckedSetVanillaRole(targetid, (byte)RoleTypes.Crewmate);
    }
    public void BuildSetting(IGameOptions gameOptions)
    {
        gameOptions.SetFloat(FloatOptionNames.KillCooldown, 0.01f);
    }
}