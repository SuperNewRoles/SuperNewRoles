using System.Collections.Generic;
using System.Text;
using AmongUs.GameOptions;
using Hazel;
using SuperNewRoles.Buttons;
using SuperNewRoles.Helpers;
using SuperNewRoles.Mode;
using SuperNewRoles.Mode.SuperHostRoles;
using SuperNewRoles.Roles.Role;
using SuperNewRoles.Roles.RoleBases;
using SuperNewRoles.Roles.RoleBases.Interfaces;
using UnityEngine;
using UnityEngine.UI;
using static SuperNewRoles.Helpers.RPCHelper;
using static SuperNewRoles.Patches.PlayerControlFixedUpdatePatch;

namespace SuperNewRoles.Roles.Neutral;

public class Jackal : RoleBase, INeutral, IJackal, IRpcHandler, IFixedUpdaterAll, ISupportSHR, IImpostorVision, IVentAvailable, ISaboAvailable, IHandleChangeRole, ICheckMurderHandler, ISHROneClickShape, ISHRAntiBlackout
{
    public static new RoleInfo Roleinfo = new(
               typeof(Jackal),
               (p) => new Jackal(p),
               RoleId.Jackal,
               "Jackal",
               RoleClass.JackalBlue,
               new(RoleId.Jackal, TeamTag.Jackal),
               TeamRoleType.Neutral,
               TeamType.Neutral
        );

    public static new OptionInfo Optioninfo =
        new(RoleId.Jackal, 300010, true,
            VentOption: (true, true),
            SaboOption: (false, true),
            ImpostorVisionOption: (true, true),
            optionCreator: CreateOption);

    public bool CanUseSabo => Optioninfo.CanUseSabo;
    public bool CanUseVent => Optioninfo.CanUseVent;
    public bool IsImpostorVision => Optioninfo.IsImpostorVision;
    public bool HasKillButtonClient => false;

    public static CustomOption JackalKillCooldown;
    public static CustomOption JackalCreateFriend;
    public static CustomOption JackalCreateSidekick;
    public static CustomOption JackalSKCooldown;
    public static CustomOption JackalNewJackalCreateSidekick;

    private static void CreateOption()
    {/*
        JackalKillCooldown = Create(300002, true, CustomOptionType.Neutral, "JackalCooldownSetting", 30f, 2.5f, 60f, 2.5f, JackalOption, format: "unitSeconds");
        JackalCreateFriend = Create(300006, true, CustomOptionType.Neutral, "JackalCreateFriendSetting", false, JackalOption);
        JackalCreateSidekick = Create(300007, false, CustomOptionType.Neutral, "JackalCreateSidekickSetting", false, JackalOption);
        JackalSKCooldown = Create(300008, false, CustomOptionType.Neutral, "PavlovsownerCreateDogCoolTime", 30f, 2.5f, 60f, 2.5f, JackalCreateSidekick, format: "unitSeconds");
        JackalNewJackalCreateSidekick = Create(300009, false, CustomOptionType.Neutral, "JackalNewJackalCreateSidekickSetting", false, JackalCreateSidekick);*/
        JackalKillCooldown = CustomOption.Create(Optioninfo.OptionId++, Optioninfo.SupportSHR, Optioninfo.RoleOption.type,
            "JackalCooldownSetting", 30f, 2.5f, 60f, 2.5f, Optioninfo.RoleOption, format: "unitSeconds");
        JackalCreateFriend = CustomOption.Create(Optioninfo.OptionId++, Optioninfo.SupportSHR, Optioninfo.RoleOption.type,
            "JackalCreateFriendSetting", false, Optioninfo.RoleOption);
        JackalCreateSidekick = CustomOption.Create(Optioninfo.OptionId++, Optioninfo.SupportSHR, Optioninfo.RoleOption.type,
            "JackalCreateSidekickSetting", false, Optioninfo.RoleOption);
        JackalSKCooldown = CustomOption.Create(Optioninfo.OptionId++, Optioninfo.SupportSHR, Optioninfo.RoleOption.type,
            "PavlovsownerCreateDogCoolTime", 30f, 2.5f, 60f, 2.5f, JackalCreateSidekick, format: "unitSeconds");
        JackalNewJackalCreateSidekick = CustomOption.Create(Optioninfo.OptionId++, Optioninfo.SupportSHR, Optioninfo.RoleOption.type,
            "JackalNewJackalCreateSidekickSetting", false, JackalCreateSidekick);
    }

    public CustomButtonInfo JackalKillButton;
    public CustomButtonInfo JackalSidekickButton;

    public static new IntroInfo Introinfo =
        new(RoleId.Jackal, introSound: RoleTypes.Shapeshifter);

    public bool CanSidekick { get; set; }
    public bool Promoted { get; private set; }
    public bool isShowSidekickButton => CanSidekick;

    public Jackal(PlayerControl p) : base(p, Roleinfo, Optioninfo, Introinfo)
    {
        CanSidekick = JackalCreateFriend.GetBool() || JackalCreateSidekick.GetBool();
        Promoted = false;
        SHR_IsSidekickMode = false;
    }

    public void BuildName(StringBuilder Suffix, StringBuilder RoleNameText, PlayerData<string> ChangePlayers)
    {
        if (CreatedSidekick is not null)
        {
            if (CreatedSidekick.Player != null)
                ChangePlayers[CreatedSidekick.Player] = ModHelpers.Cs(RoleClass.JackalBlue, ChangePlayers.GetNowName(CreatedSidekick.Player));
            else if (Player.IsAlive())
                ChangePlayers[CreatedSidekick.Player] = ModHelpers.Cs(RoleClass.CrewmateWhite, ChangePlayers.GetNowName(CreatedSidekick.Player));
        }
        if (Player.IsDead())
            return;
        if (!CanSidekick)
            return;
        string ModeText = SHR_IsSidekickMode ? "SK" : "Kill";
        RoleNameText.Append(ModHelpers.Cs(Roleinfo.RoleColor, $" (Mode:{ModeText})"));
    }

    public Sidekick CreatedSidekick { get; private set; }

    public bool SHR_IsSidekickMode = false;
    public bool isShowKillButton => true;
    public float SidekickCoolTime => JackalSKCooldown.GetFloat();
    public float JackalKillCoolTime => JackalKillCooldown.GetFloat();
    public PlayerControl OldSidekick;
    private bool SHRUpdatedToImpostor = false;

    public void OnClickSidekickButton(PlayerControl target)
    {
        if (JackalCreateFriend.GetBool())
        {
            CreateJackalFriends(target); //クルーにして フレンズにする
            CanSidekick = false;
        }
        else
        {
            bool isFakeSidekick = EvilEraser.IsBlockAndTryUse(EvilEraser.BlockTypes.JackalSidekick, target);
            MessageWriter writer = RpcWriter;
            writer.Write(true);
            writer.Write(target.PlayerId);
            writer.Write(isFakeSidekick);
            SendRpc(writer);
        }
    }
    public void SetAmSidekicked()
    {
        CanSidekick = CanSidekick && JackalNewJackalCreateSidekick.GetBool();
    }
    /// <summary>
    /// (役職をリセットし、)ジャッカルフレンズに割り当てます。
    /// </summary>
    /// <param name="target">役職がJackalFriendsに変更される対象</param>
    public static void CreateJackalFriends(PlayerControl target)
    {
        List<RoleTypes> CanNotHaveTaskForRoles = new() { RoleTypes.Impostor, RoleTypes.Shapeshifter, RoleTypes.ImpostorGhost };
        // マッドメイトになる前にタスクを持っていたかを取得
        var canNotHaveTask = CanNotHaveTaskForRoles.Contains(target.Data.Role.Role);
        canNotHaveTask = CanNotHaveTaskForRoles.Contains(RoleSelectHandler.GetDesyncRole(target.GetRole()).RoleType);// Desync役職ならタスクを持っていなかったと見なす ( 個別設定 )
        if (target.GetRoleBase() is ISupportSHR supportSHR) { canNotHaveTask = CanNotHaveTaskForRoles.Contains(supportSHR.DesyncRole); } // Desync役職ならタスクを持っていなかったと見なす ( RoleBace )

        target.ResetAndSetRole(RoleId.JackalFriends);
        if (target.IsRole(RoleId.JackalFriends)) JackalFriends.ChangeJackalFriendsPlayer[target.PlayerId] = !canNotHaveTask;
    }

    public void RpcReader(MessageReader reader)
    {
        bool isCreateSidekick = reader.ReadBoolean();
        if (!isCreateSidekick)
        {
            if (CreatedSidekick == null)
                return;
            PlayerControl sidekickPlayer = CreatedSidekick.Player;
            sidekickPlayer.ClearRole();
            sidekickPlayer.SetRole(CreatedSidekick.TargetRole);
            if (sidekickPlayer.GetRoleBase() is not IJackal changedRole)
                return;
            changedRole.SetAmSidekicked();
            PlayerControlHelper.RefreshRoleDescription(PlayerControl.LocalPlayer);
            CacheManager.ResetMyRoleCache();
            return;
        }
        byte playerId = reader.ReadByte();
        bool isFakeSidekick = reader.ReadBoolean();
        var player = ModHelpers.PlayerById(playerId);
        if (player == null) return;
        if (ModeHandler.IsMode(ModeId.SuperHostRoles) && AmongUsClient.Instance.AmHost)
        {
            if (JackalCreateFriend.GetBool())
            {
                if (!player.IsImpostor())
                    CreateJackalFriends(player);//クルーにして フレンズにする
            }
            else if (JackalCreateSidekick.GetBool())
            {
                bool isOldImpostor = player.IsImpostor();
                player.ClearRole();
                player.SetRoleRPC(RoleId.Sidekick);
                RoleTypes targetRole = RoleTypes.Crewmate;
                if (CanUseSabo)
                    targetRole = RoleTypes.Impostor;
                else if (CanUseVent)
                    targetRole = RoleTypes.Engineer;
                if (!player.IsMod())
                    player.RpcSetRoleDesync(targetRole, true);
                // キルできなくする
                if (!Player.IsMod())
                    player.RpcSetRoleDesync(RoleTypes.Impostor, true, Player);
                if (isOldImpostor)
                {
                    foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                    {
                        if (p == player)
                            continue;
                        if (!p.IsImpostor())
                            continue;
                        if (p.IsMod())
                            continue;
                        player.RpcSetRoleDesync(RoleTypes.Crewmate, true, p);
                    }
                }
            }
            else
                throw new System.NotImplementedException("Sidekick targetrole is not defined.");
            SHRUpdatedToImpostor = Player.IsMod();
        }
        if (isFakeSidekick)
            RoleClass.Jackal.FakeSidekickPlayer.Add(player);
        else
        {
            FastDestroyableSingleton<RoleManager>.Instance.SetRole(player, RoleTypes.Crewmate);
            player.ClearRole();
            player.SetRole(RoleId.Sidekick);
            if (player.TryGetRoleBase<Sidekick>(out Sidekick sidekick))
            {
                sidekick.SetParent(Player);
                CreatedSidekick = sidekick;
            }
            PlayerControlHelper.RefreshRoleDescription(PlayerControl.LocalPlayer);
            CacheManager.ResetMyRoleCache();
        }
        CanSidekick = false;
        if (ModeHandler.IsMode(ModeId.SuperHostRoles) && AmongUsClient.Instance.AmHost)
        {
            ChangeName.UpdateRoleName(Player, ChangeNameType.AllPlayers); //名前も変える
            ChangeName.UpdateRoleName(player, ChangeNameType.AllPlayers); //名前も変える
            SyncSetting.CustomSyncSettings(player);
        }
    }

    public void FixedUpdateAllDefault()
    {
        if (AmongUsClient.Instance.AmHost)
            PromoteCheck(false);
    }

    public void PromoteCheck(bool isRoleChanged)
    {
        // Logger.Info($"Checking Jackal Promote: {Promoted} : {CreatedSidekick == null} : {isRoleChanged} : {Player.IsAlive()} : {AmongUsClient.Instance.AmHost} : {ModeHandler.IsMode(ModeId.SuperHostRoles)}");
        if (Promoted)
            return;
        if (CreatedSidekick == null)
            return;
        if (!isRoleChanged && Player.IsAlive())
            return;
        Promoted = true;
        PlayerControl CreatedSidekickControl = CreatedSidekick.Player;
        MessageWriter writer = RpcWriter;
        writer.Write(false);
        SendRpc(writer);
        if (!AmongUsClient.Instance.AmHost ||
            !ModeHandler.IsMode(ModeId.SuperHostRoles))
            return;
        Logger.Info($"TryGetRoleBase: {CreatedSidekickControl.GetRoleBase().Roleinfo.Role}");
        if (!CreatedSidekickControl.TryGetRoleBase(out Jackal jackal))
            return;
        if (!CreatedSidekickControl.IsMod())
        {
            CreatedSidekickControl.RpcSetRoleDesync(
                jackal.DesyncRole, true
            );
        }
        foreach (PlayerControl player in PlayerControl.AllPlayerControls)
        {
            if (player.PlayerId == CreatedSidekickControl.PlayerId ||
                player.PlayerId == Player.PlayerId)
                continue;
            if (player.IsJackalTeamJackal() || player.IsJackalTeamSidekick())
                continue;
            if (!player.IsMod())
                CreatedSidekickControl.RpcSetRoleDesync(RoleTypes.Crewmate, true, player);
            if (!CreatedSidekickControl.IsMod())
                player.RpcSetRoleDesync(player.IsImpostor() ? RoleTypes.Crewmate : player.Data.Role.Role, true, CreatedSidekickControl);
        }
        if (jackal.DesyncRole == RoleTypes.Shapeshifter)
            OneClickShapeshift.OneClickShaped(CreatedSidekickControl);
        CreatedSidekickControl.RpcShowGuardEffect(CreatedSidekickControl);
        ChangeName.UpdateRoleName(CreatedSidekickControl, ChangeNameType.SelfOnly);
        SyncSetting.CustomSyncSettings(CreatedSidekickControl);
    }

    public RoleTypes RealRole => RoleTypes.Crewmate;
    public RoleTypes DesyncRole => CanSidekick ? RoleTypes.Shapeshifter : RoleTypes.Impostor;

    public bool? IsImpostorLight => Optioninfo.IsImpostorVision;

    public void BuildSetting(IGameOptions gameOptions)
    {
        gameOptions.SetFloat(FloatOptionNames.KillCooldown, SyncSetting.KillCoolSet(JackalKillCoolTime));
    }

    private void SHRUpdateToImpostor()
    {
        if (CreatedSidekick != null && SHRUpdatedToImpostor && !Player.IsMod())
        {
            Player.RpcSetRoleDesync(RoleTypes.Impostor, true);
            SHRUpdatedToImpostor = true;
        }
    }

    public bool OnCheckMurderPlayerAmKiller(PlayerControl target)
    {
        if (target.IsJackalTeam())
            return false;
        if (!CanSidekick || !SHR_IsSidekickMode)
        {
            Logger.Info($"通常キル CanSidekick:{CanSidekick} IsSidekickMode:{SHR_IsSidekickMode}", "JackalSHR");
            SHRUpdateToImpostor();
            return true;
        }
        SuperNewRolesPlugin.Logger.LogInfo("まだ作ってなくて、設定が有効の時なんでサイドキック作成");
        if (target == null) return false;
        Player.RpcForceGuard(target);
        CanSidekick = false;

        bool isFakeSidekick = EvilEraser.IsBlockAndTryUse(EvilEraser.BlockTypes.JackalSidekick, target);
        MessageWriter writer = RpcWriter;
        writer.Write(true);
        writer.Write(target.PlayerId);
        writer.Write(isFakeSidekick);
        SendRpc(writer);
        Logger.Info("ジャッカルフレンズを作成しました。", "JackalSHR");
        return false;
    }
    public void FixedUpdateAllSHR()
    {
        if (AmongUsClient.Instance.AmHost && AntiBlackOut.GamePlayers == null)
            PromoteCheck(false);
    }

    public void OnChangeRole()
    {
        if (AmongUsClient.Instance.AmHost)
            PromoteCheck(true);
    }

    public void OnOneClickShape()
    {
        if (!CanSidekick)
            return;
        SHR_IsSidekickMode = !SHR_IsSidekickMode;
        ChangeName.UpdateRoleName(Player, ChangeNameType.SelfOnly);
        return;
    }
    public void StartAntiBlackout()
    {
        if (CreatedSidekick?.Player != null && !Player.IsMod())
            CreatedSidekick.Player.RpcSetRoleDesync(CreatedSidekick.Player.IsDead() ? RoleTypes.CrewmateGhost : RoleTypes.Crewmate, Player);
    }

    public void EndAntiBlackout()
    {
        if (CreatedSidekick?.Player != null && !Player.IsMod() && CreatedSidekick.Player.IsAlive())
            CreatedSidekick.Player.RpcSetRoleDesync(RoleTypes.Impostor, Player);
        SHRUpdateToImpostor();
    }
}