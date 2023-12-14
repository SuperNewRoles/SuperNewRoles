
using System.Collections.Generic;
using AmongUs.GameOptions;
using Hazel;
using SuperNewRoles.Roles.Role;
using SuperNewRoles.Roles.RoleBases;
using SuperNewRoles.Roles.RoleBases.Interfaces;
using UnityEngine;

namespace SuperNewRoles.Roles.Crewmate.Santa;

public class Santa : RoleBase, ICrewmate, ICustomButton, IRpcHandler
{
    public static new RoleInfo Roleinfo = new(
        typeof(Santa),
        (p) => new Santa(p),
        RoleId.Santa,
        "Santa",
        new(255, 178, 178, byte.MaxValue),
        new(RoleId.Santa, TeamTag.Crewmate,
            RoleTag.Takada),
        TeamRoleType.Crewmate,
        TeamType.Crewmate
        );
    public static new OptionInfo Optioninfo =
        new(RoleId.Santa, 406500, false,
            CoolTimeOption: (30f, 2.5f, 60f, 2.5f, false),
            optionCreator: CreateOption);
    public static new IntroInfo Introinfo =
        new(RoleId.Santa, introSound: RoleTypes.Crewmate);

    private static CustomOption CanUseAbilityCount;

    public CustomButtonInfo[] CustomButtonInfos { get; }

    public static RoleId[] PresetRolesParam { get; } = new RoleId[]
    {
        RoleId.SpeedBooster,
        RoleId.Clergyman,
        RoleId.NiceGuesser,
        RoleId.Lighter,
        RoleId.Sheriff,
        RoleId.Balancer,
        RoleId.Celebrity,
        RoleId.HomeSecurityGuard,
        RoleId.SuicidalIdeation
    };
    private static CustomOption[] PresetRoleOptions { get; set; }
    
    private static void CreateOption()
    {
        CanUseAbilityCount = CustomOption.Create(Optioninfo.OptionId++, false, Optioninfo.RoleOption.type,
            "SantaCanUseAbilityCount", 1, 1, 15, 1,
            Optioninfo.RoleOption);
        PresetRoleOptions = new CustomOption[PresetRolesParam.Length];
        for (int i = 0; i < PresetRolesParam.Length; i++)
        {
            PresetRoleOptions[i] = CustomOption.Create(Optioninfo.OptionId++, false, Optioninfo.RoleOption.type,
                               string.Format(
                                   ModTranslation.GetString("SantaPresentRoleOptionFormat"),
                                   CustomRoles.GetRoleNameOnColor(PresetRolesParam[i])
                               ),
                               CustomOptionHolder.ratesper5, Optioninfo.RoleOption);
        }
    }

    public void RpcReader(MessageReader reader)
    {
        byte targetId = reader.ReadByte();
        if (targetId == 255)
        {
            Player.MurderPlayer(Player, MurderResultFlags.Succeeded);
            RPCProcedure.SetFinalStatus(Player.PlayerId, FinalStatus.SantaSelf);
            return;
        }
        RoleId role = (RoleId)reader.ReadInt16();
        RPCProcedure.SetRole(targetId, (byte)role);
        if (Player.PlayerId != PlayerControl.LocalPlayer.PlayerId)
            SantaButtonInfo.AbilityCount--;
    }

    private List<RoleId> RoleAssignTickets { get; }

    private CustomButtonInfo SantaButtonInfo { get; }

    private void SantaOnClick()
    {
        PlayerControl target = SantaButtonInfo.CurrentTarget;
        if (target == null)
            return;
        MessageWriter writer = RpcWriter;
        if (target.IsCrew())
        {
            writer.Write(target.PlayerId);
            RoleId role = ModHelpers.GetRandom(RoleAssignTickets);
            writer.Write((short)role);
        }
        else
        {
            //255だと失敗(自爆)
            writer.Write(255);
        }
        SendRpc(writer);
    }
    public Santa(PlayerControl p) : base(p, Roleinfo, Optioninfo, Introinfo)
    {
        SantaButtonInfo = new(CanUseAbilityCount.GetInt(), this, () => SantaOnClick(),
            (isAlive) => isAlive, CustomButtonCouldType.CanMove | CustomButtonCouldType.SetTarget, null,
            ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.SantaButton.png", 115f),
            () => Optioninfo.CoolTime, new(-2,1,0),
            "SantaButtonName", KeyCode.F);
        CustomButtonInfos = new CustomButtonInfo[1] { SantaButtonInfo };
        RoleAssignTickets = new();
        for(int i = 0; i < PresetRoleOptions.Length; i++)
        {
            RoleId roleId = PresetRolesParam[i];
            int ticketcount = PresetRoleOptions[i].GetSelection();
            SetTicket(roleId, ticketcount);
        }
        //もし全て0%ならすべて同じ確率で設定する
        if (RoleAssignTickets.Count <= 0)
        {
            for (int i = 0; i < PresetRoleOptions.Length; i++)
            {
                RoleId roleId = PresetRolesParam[i];
                SetTicket(roleId, 1);
            }
        }
    }
    private void SetTicket(RoleId roleId, int count)
    {
        for (int i = 0; i < count; i++)
            RoleAssignTickets.Add(roleId);
    }

}