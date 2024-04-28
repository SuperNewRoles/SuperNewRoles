using AmongUs.GameOptions;
using Hazel;
using SuperNewRoles.Roles.Role;
using SuperNewRoles.Roles.RoleBases;
using SuperNewRoles.Roles.RoleBases.Interfaces;
using UnityEngine;

namespace SuperNewRoles.Roles.Crewmate.Phosphorus;

public class Phosphorus : RoleBase, ICrewmate, ICustomButton, IMeetingHandler, IRpcHandler
{
    public static new RoleInfo Roleinfo = new(
        typeof(Phosphorus),
        (p) => new Phosphorus(p),
        RoleId.Phosphorus,
        "Phosphorus",
        new(249, 188, 81, byte.MaxValue),
        new(RoleId.Phosphorus, TeamTag.Crewmate),
        TeamRoleType.Crewmate,
        TeamType.Crewmate
        );
    public static new OptionInfo Optioninfo =
        new(RoleId.Phosphorus, 436500, false,
            CoolTimeOption: (30f, 2.5f, 60f, 2.5f, false),
            DurationTimeOption: (10f, 2.5f, 120f, 2.5f, false),
            AbilityCountOption: (1, 1, 10, 1, false),
            optionCreator: CreateOption);
    public static new IntroInfo Introinfo =
        new(RoleId.Phosphorus, introSound: RoleTypes.Crewmate);

    public static CustomOption LightingCooltime;
    public static CustomOption LightRange;

    public CustomButtonInfo[] CustomButtonInfos { get; }
    public CustomButtonInfo PutButtonInfo { get; private set; }
    public CustomButtonInfo LightingButtonInfo { get; private set; }

    private static void CreateOption()
    {
        LightingCooltime = CustomOption.Create(Optioninfo.OptionId++, false, CustomOptionType.Crewmate, "PhosphorusLightingCooltime", 30f, 2.5f, 60f, 2.5f, Optioninfo.RoleOption);
        LightRange = CustomOption.Create(Optioninfo.OptionId++, false, CustomOptionType.Crewmate, "PhosphorusLightRange", 0.5f, 0.25f, 5f, 0.25f, Optioninfo.RoleOption);
    }

    public Phosphorus(PlayerControl p) : base(p, Roleinfo, Optioninfo, Introinfo)
    {
        PutButtonInfo = new(Optioninfo.AbilityMaxCount, this, () => Put(),
            (isAlive) => isAlive, CustomButtonCouldType.CanMove, null,
            ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.Phosphorus.PutButton.png", 115f),
            () => Optioninfo.CoolTime, Vector3.zero,
            "PhosphorusPutButtonName", KeyCode.F,
            HasAbilityCountText: true
        );
        LightingButtonInfo = new(null, this, () => Lighting(),
            (isAlive) => isAlive, CustomButtonCouldType.CanMove, null,
            ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.Phosphorus.LightingButton.png", 115f),
            () => LightingCooltime.GetFloat(), Vector3.zero,
            "PhosphorusLightingButtonName", KeyCode.Q,
            DurationTime: () => Optioninfo.DurationTime,
            OnEffectEnds: () => Lighting(false)
        );

        CustomButtonInfos = new CustomButtonInfo[2] { PutButtonInfo, LightingButtonInfo };
    }

    private enum RpcTypes
    {
        Put,
        LightingOn,
        LightingOff,
    }

    private void Put()
    {
        MessageWriter writer = RpcWriter;
        writer.Write((byte)RpcTypes.Put);
        SendRpc(writer);
    }
    private void Lighting(bool active = true)
    {
        MessageWriter writer = RpcWriter;
        writer.Write((byte)RpcTypes.LightingOn);
        SendRpc(writer);
    }

    public void StartMeeting() { }
    public void CloseMeeting()
    {
        MessageWriter writer = RpcWriter;
        writer.Write((byte)RpcTypes.LightingOff);
        SendRpc(writer);
    }

    public void RpcReader(MessageReader reader)
    {
        throw new System.NotImplementedException();
    }
}