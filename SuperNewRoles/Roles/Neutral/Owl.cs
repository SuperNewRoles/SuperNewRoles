using System.Linq;
using AmongUs.GameOptions;
using Hazel;
using SuperNewRoles.Helpers;
using SuperNewRoles.Patches;
using SuperNewRoles.Roles.Role;
using SuperNewRoles.Roles.RoleBases;
using SuperNewRoles.Roles.RoleBases.Interfaces;
using UnityEngine;

namespace SuperNewRoles.Roles.Neutral;

// 提案者 : はるかさん, 提案代行者 : まっすーさん
public class Owl : RoleBase, INeutral, IKiller, IVentAvailable, ICustomButton, ISpecialWinner, ISpecialBlackout, IRpcHandler, IFixedUpdaterAll
{
    public static new RoleInfo Roleinfo = new(
        typeof(Owl),
        (p) => new Owl(p),
        RoleId.Owl,
        "Owl",
        new(169, 107, 46, byte.MaxValue),
        new(RoleId.Owl, TeamTag.Neutral, RoleTag.SpecialKiller, RoleTag.CanUseVent),
        TeamRoleType.Neutral,
        TeamType.Neutral
    );
    public static new OptionInfo Optioninfo = new(RoleId.Owl, 303800, false, KillCoolTimeOption: (30f, 2.5f, 60f, 2.5f, false), VentOption: (true, false), optionCreator: CreateOption);
    public static new IntroInfo Introinfo = new(RoleId.Owl, introSound: RoleTypes.Shapeshifter);

    public static CustomOption ImposterVisibilityDuringBlackout;
    public static CustomOption CanSpecialBlackoutDeadBodyCount;
    public static CustomOption SpecialBlackoutCool;
    public static CustomOption SpecialBlackoutTime;
    private static void CreateOption()
    {
        ImposterVisibilityDuringBlackout = CustomOption.Create(Optioninfo.OptionId++, false, CustomOptionType.Neutral, "OwlImposterVisibilityDuringBlackout", true, Optioninfo.RoleOption);
        CanSpecialBlackoutDeadBodyCount = CustomOption.Create(Optioninfo.OptionId++, false, CustomOptionType.Neutral, "OwlCanSpecialBlackoutDeaebodyCount", 1, 1, 5, 1, Optioninfo.RoleOption);
        SpecialBlackoutCool = CustomOption.Create(Optioninfo.OptionId++, false, CustomOptionType.Neutral, "SpecialBlackoutCool", 20f, 2.5f, 60f, 2.5f, Optioninfo.RoleOption);
        SpecialBlackoutTime = CustomOption.Create(Optioninfo.OptionId++, false, CustomOptionType.Neutral, "SpecialBlackoutTime", 10f, 2.5f, 60f, 2.5f, Optioninfo.RoleOption);
    }

    public int NestVentId;
    public DeadBody TransportBody;
    public int NestDeadBodyCount;
    public bool IsSpecialBlackout;
    public float BlackOutTimer;
    private readonly CustomButtonInfo NestBuildingButtom;
    private readonly CustomButtonInfo TransportButton;
    private readonly CustomButtonInfo SpecialBlackoutButton;
    private readonly CustomButtonInfo OwlKillButton;
    public CustomButtonInfo[] CustomButtonInfos { get; }
    public static float BlackoutValue
    {
        get
        {
            Owl owl = RoleBaseManager.GetRoleBases<Owl>().OrderByDescending(x => x.BlackOutTimer).First();
            if (owl == null) return 1f;
            if (SpecialBlackoutTime.GetFloat() - owl.BlackOutTimer < 0.5f) return Mathf.Clamp01((SpecialBlackoutTime.GetFloat() - owl.BlackOutTimer) * 2);
            else if (owl.BlackOutTimer < 0.5) return Mathf.Clamp01(owl.BlackOutTimer * 2);
            return 1f;
        }
    }
    public Owl(PlayerControl player) : base(player, Roleinfo, Optioninfo, Introinfo)
    {
        NestVentId = int.MinValue;
        TransportBody = null;
        NestDeadBodyCount = 0;
        IsSpecialBlackout = false;
        BlackOutTimer = 0f;
        NestBuildingButtom = new(1, this, NestBuildingButtomClick, (isAlive) => isAlive && NestBuildingButtom.AbilityCount > 0, CustomButtonCouldType.CanMove | CustomButtonCouldType.SetVent, null,
                                 ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.OwlNestBuildingButton.png", 110f),
                                 () => 0f, new(0f, 1f, 0f), "OwlNestBuildingButton", KeyCode.F, 49);
        TransportButton = new(null, this, TransportButtonClick, (isAlive) => isAlive && NestBuildingButtom.AbilityCount <= 0, CustomButtonCouldType.CanMove, TransportButtonMeetingEnd,
                              ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.OwlTransportButton.png", 110f),
                              () => 0f, new(-1f, 1f, 0f), "OwlTransportButton", KeyCode.F, 49, CouldUse: TransportButtonCouldUse);
        SpecialBlackoutButton = new(null, this, SpecialBlackoutButtonClick, (isAlive) => isAlive && NestBuildingButtom.AbilityCount <= 0, CustomButtonCouldType.CanMove, SpecialBlackoutButtonMeetingEnd,
                                    ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.OwlSpecialBlackoutButton.png", 110f),
                                    SpecialBlackoutCool.GetFloat, new(-2f, 1f, 0f), "OwlSpecialBlackoutButton", null, null, CouldUse: SpecialBlackoutButtonCouldUse,
                                    DurationTime: SpecialBlackoutTime.GetFloat, OnEffectEnds: SpecialBlackoutButtonEffectEnds);
        OwlKillButton = new(null, this, OwlKillButtonClick, (isAlive) => isAlive && NestBuildingButtom.AbilityCount <= 0, CustomButtonCouldType.CanMove | CustomButtonCouldType.SetTarget, null,
                            HudManager.Instance.KillButton.graphic.sprite,
                            () => Optioninfo.KillCoolTime, new(0f, 1f, 0f), "Kill", KeyCode.Q, baseButton: HudManager.Instance.KillButton, CouldUse: OwlKillButtonCouldUse);
        CustomButtonInfos = new CustomButtonInfo[4]
        {
            NestBuildingButtom,
            TransportButton,
            SpecialBlackoutButton,
            OwlKillButton,
        };
    }

    public bool CanUseKill => ModHelpers.IsBlackout();

    public bool CanUseVent => Optioninfo.CanUseVent;

    public void NestBuildingButtomClick()
    {
        Vent vent = NestBuildingButtom.SetTargetVent();
        if (vent == null)
        {
            NestBuildingButtom.AbilityCount = 1;
            return;
        }
        NestVentId = vent.Id;
        Logger.Info($"フクロウが巣を作成しました VentId : {NestVentId}", "Owl");
    }

    public void TransportButtonClick()
    {
        if (TransportBody)
        {
            TransportButtonReset();
            Vent vent = TransportButton.SetTargetVent();
            byte id = TransportBody.ParentId;
            MessageWriter writer1 = RpcWriter;
            writer1.Write(false);
            writer1.Write(byte.MaxValue);
            SendRpc(writer1);
            if (vent && vent.Id == NestVentId)
            {
                RPCProcedure.MoveDeadBody(id, 9999f, 9999f);
                MessageWriter writer2 = RPCHelper.StartRPC(CustomRPC.MoveDeadBody);
                writer2.Write(id);
                writer2.Write(9999f);
                writer2.Write(9999f);
                writer2.EndRPC();
                NestDeadBodyCount++;
            }
            return;
        }
        foreach (Collider2D collider in Physics2D.OverlapCircleAll(Player.GetTruePosition(), Player.MaxReportDistance, Constants.PlayersOnlyMask))
        {
            if (collider.tag != "DeadBody") continue;
            DeadBody component = collider.GetComponent<DeadBody>();
            if (component.Reported) continue;
            Vector2 player_pos = Player.GetTruePosition();
            Vector2 body_pos = component.TruePosition;
            if (PhysicsHelpers.AnythingBetween(player_pos, body_pos, Constants.ShipAndObjectsMask, false)) continue;
            MessageWriter writer = RpcWriter;
            writer.Write(false);
            writer.Write(component.ParentId);
            SendRpc(writer);
        }
    }

    public void TransportButtonMeetingEnd()
    {
        MessageWriter writer = RpcWriter;
        writer.Write(false);
        writer.Write(byte.MaxValue);
        SendRpc(writer);
        TransportButtonReset();
    }

    public bool TransportButtonCouldUse()
    {
        if (TransportBody)
        {
            if (!ModHelpers.IsBlackout())
            {
                MessageWriter writer = RpcWriter;
                writer.Write(false);
                writer.Write(byte.MaxValue);
                SendRpc(writer);
                TransportButtonReset();
                return false;
            }

            Vent vent = TransportButton.SetTargetVent(false);
            if (vent && vent.Id == NestVentId)
            {
                vent.SetOutline(true, true);
                TransportButton.customButton.Sprite = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.OwlHideDeadBodyButton.png", 110f);
                TransportButton.customButton.buttonText = ModTranslation.GetString("OwlHideDeadBodyButton");
            }
            else TransportButtonReset();
            return true;
        }
        if (!ModHelpers.IsBlackout()) return false;
        foreach (Collider2D collider in Physics2D.OverlapCircleAll(Player.GetTruePosition(), Player.MaxReportDistance, Constants.PlayersOnlyMask))
        {
            if (collider.tag != "DeadBody") continue;
            DeadBody component = collider.GetComponent<DeadBody>();
            if (component.Reported) continue;
            Vector2 player_pos = Player.GetTruePosition();
            Vector2 body_pos = component.TruePosition;
            if (PhysicsHelpers.AnythingBetween(player_pos, body_pos, Constants.ShipAndObjectsMask, false)) continue;
            return true;
        }
        return false;
    }

    public void TransportButtonReset()
    {
        TransportButton.customButton.Sprite = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.OwlTransportButton.png", 110f);
        TransportButton.customButton.buttonText = ModTranslation.GetString("OwlTransportButton");
    }

    public void SpecialBlackoutButtonClick()
    {
        MessageWriter writer = RpcWriter;
        writer.Write(true);
        writer.Write(true);
        writer.Write(SpecialBlackoutTime.GetFloat());
        SendRpc(writer);
    }

    public void SpecialBlackoutButtonMeetingEnd() => SpecialBlackoutButtonEffectEnds();

    public void SpecialBlackoutButtonEffectEnds()
    {
        MessageWriter writer = RpcWriter;
        writer.Write(true);
        writer.Write(false);
        writer.Write(0f);
        SendRpc(writer);
        SpecialBlackoutButton.ResetCoolTime();
    }

    public bool SpecialBlackoutButtonCouldUse()
    {
        if (ShipStatus.Instance)
        {
            if (!ShipStatus.Instance.Systems.TryGetValue(SystemTypes.Electrical, out ISystemType elecsystem)) return true;
            if (elecsystem.CastFast<SwitchSystem>() == null) return true;
        }
        return NestDeadBodyCount >= CanSpecialBlackoutDeadBodyCount.GetInt();
    }

    public void OwlKillButtonClick()
    {
        if (!ModHelpers.IsBlackout()) return;
        ModHelpers.CheckMurderAttemptAndKill(PlayerControl.LocalPlayer, OwlKillButton.SetTarget());
    }

    public bool OwlKillButtonCouldUse() => ModHelpers.IsBlackout();

    public bool CheckAndEndGame(ShipStatus __instance, CheckGameEndPatch.PlayerStatistics statistics)
    {
        if (1 >= statistics.TotalAlive - 1 && statistics.TeamImpostorsAlive == 0 && statistics.TeamJackalAlive == 0 && statistics.HitmanAlive == 0 && statistics.OwlAlive == 1 && !statistics.IsGuardPavlovs)
        {
            foreach (PlayerControl p in RoleClass.SideKiller.MadKillerPlayer.AsSpan())
                if (!p.IsImpostor() && !p.Data.Disconnected) return false;
            foreach (Owl role in RoleBaseManager.GetRoleBases<Owl>())
            {
                PlayerControl player = role.Player;
                if (player == null) continue;
                if (player.IsDead()) continue;
                MessageWriter writer = RPCHelper.StartRPC(CustomRPC.ShareWinner);
                writer.Write(player.PlayerId);
                writer.EndRPC();
                RPCProcedure.ShareWinner(player.PlayerId);
                __instance.enabled = false;
                CheckGameEndPatch.CustomEndGame((GameOverReason)CustomGameOverReason.OwlWin, false);
                return true;
            }
        }
        return false;
    }

    public bool IsBlackout() => IsSpecialBlackout;

    public void FixedUpdateAllDefault()
    {
        if (IsSpecialBlackout) BlackOutTimer -= Time.fixedDeltaTime;
        if (TransportBody)
        {
            TransportBody.transform.position = Player.transform.position;
            if (Player.IsDead()) TransportBody = null;
        }
    }

    public void RpcReader(MessageReader reader)
    {
        if (reader.ReadBoolean())
        {
            IsSpecialBlackout = reader.ReadBoolean();
            BlackOutTimer = reader.ReadSingle();
        }
        else
        {
            byte id = reader.ReadByte();
            if (id == byte.MaxValue)
            {
                TransportBody = null;
                return;
            }
            foreach (DeadBody dead in Object.FindObjectsOfType<DeadBody>())
            {
                if (dead.ParentId != id) continue;
                TransportBody = dead;
                break;
            }
            foreach (Owl owl in RoleBaseManager.GetRoleBases<Owl>())
            {
                if (owl.Player.PlayerId == Player.PlayerId) continue;
                if (owl.TransportBody.ParentId != id) continue;
                owl.TransportBody = null;
            }
        }
    }
}
