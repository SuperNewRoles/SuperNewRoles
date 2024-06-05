using System.Collections.Generic;
using System.Text;
using AmongUs.GameOptions;
using Hazel;
using SuperNewRoles.Buttons;
using SuperNewRoles.Mode.SuperHostRoles;
using SuperNewRoles.Roles.Role;
using SuperNewRoles.Roles.RoleBases;
using SuperNewRoles.Roles.RoleBases.Interfaces;
using UnityEngine;
using UnityEngine.UI;
using static SuperNewRoles.Helpers.RPCHelper;
using static SuperNewRoles.Patches.PlayerControlFixedUpdatePatch;

namespace SuperNewRoles.Roles.Neutral;

public class Jackal : RoleBase, INeutral, IJackal, IRpcHandler, IFixedUpdaterAll, ISupportSHR, IImpostorVision, IVentAvailable, ISaboAvailable, IHandleChangeRole
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
    }

    public Sidekick CreatedSidekick { get; private set; }

    public bool isShowKillButton => true;
    public float SidekickCoolTime => JackalSKCooldown.GetFloat();
    public float JackalKillCoolTime => JackalKillCooldown.GetFloat();

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
            ChacheManager.ResetMyRoleChache();
            return;
        }
        byte playerId = reader.ReadByte();
        bool isFakeSidekick = reader.ReadBoolean();
        var player = ModHelpers.PlayerById(playerId);
        if (player == null) return;
        if (isFakeSidekick)
        {
            RoleClass.Jackal.FakeSidekickPlayer.Add(player);
        }
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
            ChacheManager.ResetMyRoleChache();
        }
        CanSidekick = false;
    }

    public void FixedUpdateAllDefault()
    {
        if (AmongUsClient.Instance.AmHost)
            PromoteCheck(false);
    }
    public void PromoteCheck(bool isRoleChanged)
    {
        if (Promoted)
            return;
        if (CreatedSidekick == null)
            return;
        if (!isRoleChanged && Player.IsDead())
            return;
        Promoted = true;
        MessageWriter writer = RpcWriter;
        writer.Write(false);
        SendRpc(writer);
    }

    public RoleTypes RealRole => RoleTypes.Crewmate;
    public RoleTypes DesyncRole => RoleTypes.Impostor;

    public bool? IsImpostorLight => Optioninfo.IsImpostorVision;

    public void BuildSetting(IGameOptions gameOptions)
    {
        gameOptions.SetFloat(FloatOptionNames.KillCooldown, SyncSetting.KillCoolSet(JackalKillCoolTime));
    }

    public void OnChangeRole()
    {
        if (AmongUsClient.Instance.AmHost)
            PromoteCheck(true);
    }
}