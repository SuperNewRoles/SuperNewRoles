using System.Collections.Generic;
using System.Text;
using AmongUs.GameOptions;
using Hazel;
using SuperNewRoles.Mode.SuperHostRoles;
using SuperNewRoles.Roles.Role;
using SuperNewRoles.Roles.RoleBases;
using SuperNewRoles.Roles.RoleBases.Interfaces;
using UnityEngine;

namespace SuperNewRoles.Roles.Neutral;

public class Cupid : RoleBase, INeutral, IFixedUpdaterAll, IFixedUpdaterMe, ISupportSHR, ICustomButton, IRpcHandler, INameHandler, ICheckMurderHandler
{
    public static new RoleInfo Roleinfo = new(
        typeof(Cupid),
        (p) => new Cupid(p),
        RoleId.Cupid,
        "Cupid",
        RoleClass.Lovers.color,
        new(RoleId.Cupid, TeamTag.Neutral
            ),
        TeamRoleType.Neutral,
        TeamType.Neutral
        );
    public static new OptionInfo Optioninfo =
        new(RoleId.Cupid, 301100, true,
            CoolTimeOption: (20f, 2.5f, 180f, 2.5f, true));
    public static new IntroInfo Introinfo =
        new(RoleId.Cupid, introSound: RoleTypes.Shapeshifter);

    public bool Created { get; private set; }

    public RoleTypes RealRole => RoleTypes.Crewmate;
    public RoleTypes DesyncRole => RoleTypes.Impostor;

    public CustomButtonInfo[] CustomButtonInfos { get; }
    private CustomButtonInfo CupidButtonInfo { get; }

    private List<PlayerControl> _cachedUntargetPlayers;
    //作ってる途中の片方
    public PlayerControl currentPair { get; private set; }

    public void FixedUpdateAllSHR()
    {
        if (!Created)
            return;
        if (currentPair != null &&
            currentPair.IsLovers())
            return;
        Created = false;
    }
    public void FixedUpdateMeDefaultAlive()
    {
        if (Created)
        {
            if (currentPair == null ||
                !currentPair.IsLovers())
            {
                Created = false;
            }
        }
        else
        {
            if (CupidButtonInfo.CurrentTarget != null)
                Patches.PlayerControlFixedUpdatePatch.SetPlayerOutline(CupidButtonInfo.CurrentTarget, Roleinfo.RoleColor);
        }
    }
    public bool OnCheckMurderPlayerAmKiller(PlayerControl target)
    {
        if (Created)
            return false;
        if (target.IsLovers() || target.IsRole(RoleId.LoversBreaker))
            return false;
        if (currentPair == target)
            return false;
        if (currentPair == null)
        {
            currentPair = target;
        }
        else
        {
            MessageWriter writer = RpcWriter;
            writer.Write(CachedPlayer.LocalPlayer.PlayerId);
            writer.Write(currentPair.PlayerId);
            writer.Write(target.PlayerId);
            SendRpc(writer);
            Created = true;
        }
        ChangeName.SetRoleName(Player);
        Player.RpcShowGuardEffect(target);
        return false;
    }
    public void BuildName(StringBuilder Suffix, StringBuilder RoleNameText, PlayerData<string> ChangePlayers)
    {
        if (currentPair != null)
            return;
        var suffix = ModHelpers.Cs(RoleClass.Lovers.color, " ♥");
        PlayerControl Side = currentPair.GetOneSideLovers();
        ChangePlayers[currentPair.PlayerId] = ChangeName.GetNowName(ChangePlayers, currentPair) + suffix;
        ChangePlayers[Side.PlayerId] = ChangeName.GetNowName(ChangePlayers, Side) + suffix;
        Suffix.Append(suffix);
    }
    private void ButtonOnClick()
    {
        PlayerControl target = CupidButtonInfo.CurrentTarget;
        if (target.IsLovers() || target.IsRole(RoleId.LoversBreaker)) return;
        if (currentPair == null)
        {
            currentPair = target;
            CupidButtonInfo.ResetCoolTime();
        }
        else
        {
            MessageWriter writer = RpcWriter;
            writer.Write(CachedPlayer.LocalPlayer.PlayerId);
            writer.Write(currentPair.PlayerId);
            writer.Write(target.PlayerId);
            SendRpc(writer);
            Created = true;
        }
        //アップデート
        CupidButtonInfo.SetTarget();
    }
    private bool OnCouldUse()
    {
        if (!Created && currentPair != null && currentPair.IsDead())
            currentPair = null;
        return true;
    }
    public void OnHandleName()
    {
        if (Created && currentPair != null)
        {
            string suffix = ModHelpers.Cs(RoleClass.Lovers.color, " ♥");
            PlayerControl side = currentPair.GetOneSideLovers();
            SetNamesClass.SetPlayerNameText(currentPair, $"{currentPair.NameText().text}{suffix}");
            if (!side.Data.Disconnected)
                SetNamesClass.SetPlayerNameText(side, $"{side.NameText().text}{suffix}");
        }
    }
    private List<PlayerControl> GetUntargetPlayers()
    {
        if (currentPair == null)
            return null;
        if (_cachedUntargetPlayers != null && _cachedUntargetPlayers.FirstOrDefault() != currentPair)
            return _cachedUntargetPlayers;
        return _cachedUntargetPlayers = new(1) { currentPair };
    }

    public void RpcReader(MessageReader reader)
    {
        (byte sourceid, byte player1, byte player2) = (reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
        currentPair = ModHelpers.PlayerById(player1);
        RPCProcedure.SetLovers(player1, player2);
    }

    public void FixedUpdateAllDefault()
    {
    }

    public Cupid(PlayerControl p) : base(p, Roleinfo, Optioninfo, Introinfo)
    {
        Created = false;
        CupidButtonInfo = new(null, this, () => ButtonOnClick(),
            (isAlive) => isAlive && !Created,
            CustomButtonCouldType.CanMove | CustomButtonCouldType.SetTarget,
            null, ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.cupidButton.png", 115f),
            () => Optioninfo.CoolTime, new(-2f, 1, 0),
            "CupidButtonName", KeyCode.F, 49,CouldUse: () => OnCouldUse(),
            SetTargetUntargetPlayer:() => GetUntargetPlayers()
            );
        _cachedUntargetPlayers = null;
        CustomButtonInfos = new CustomButtonInfo[1] { CupidButtonInfo };
    }
}