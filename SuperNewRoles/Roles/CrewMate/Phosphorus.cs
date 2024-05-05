using System.Collections.Generic;
using AmongUs.GameOptions;
using HarmonyLib;
using Hazel;
using SuperNewRoles.CustomObject;
using SuperNewRoles.Roles.Role;
using SuperNewRoles.Roles.RoleBases;
using SuperNewRoles.Roles.RoleBases.Interfaces;
using UnityEngine;

namespace SuperNewRoles.Roles.Crewmate.Phosphorus;

// アイデア元：NoS。ありがとうございます！
public class Phosphorus : RoleBase, ICrewmate, ICustomButton, IDeathHandler, IMeetingHandler, IHandleChangeRole, IRpcHandler
{
    public static new RoleInfo Roleinfo = new(
        typeof(Phosphorus),
        (p) => new Phosphorus(p),
        RoleId.Phosphorus,
        "Phosphorus",
        new(249, 188, 81, byte.MaxValue),
        new(RoleId.Phosphorus, TeamTag.Crewmate),
        TeamRoleType.Crewmate,
        TeamType.Crewmate,
        QuoteMod.NebulaOnTheShip
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
        LightRange = CustomOption.Create(Optioninfo.OptionId++, false, CustomOptionType.Crewmate, "PhosphorusLightRange", 0.5f, 0.1f, 5f, 0.05f, Optioninfo.RoleOption);
    }

    public Phosphorus(PlayerControl p) : base(p, Roleinfo, Optioninfo, Introinfo)
    {
        PutButtonInfo = new(Optioninfo.AbilityMaxCount, this, () => SendRpcPut(),
            (isAlive) => isAlive, CustomButtonCouldType.CanMove, null,
            ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.Phosphorus.PutButton.png", 115f),
            () => Optioninfo.CoolTime, Vector3.zero,
            "PhosphorusPutButtonName", KeyCode.F,
            HasAbilityCountText: true
        );
        LightingButtonInfo = new(null, this, () => SendRpcLighting(),
            (isAlive) => isAlive, CustomButtonCouldType.CanMove, null,
            ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.Phosphorus.LightingButton.png", 115f),
            () => LightingCooltime.GetFloat(), Vector3.zero,
            "PhosphorusLightingButtonName", KeyCode.Q,
            DurationTime: () => Optioninfo.DurationTime,
            CouldUse: () => PutButtonInfo.AbilityCount != Optioninfo.AbilityMaxCount,
            OnEffectEnds: () => SendRpcLighting(false)
        );

        CustomButtonInfos = new CustomButtonInfo[2] { PutButtonInfo, LightingButtonInfo };
    }

    private enum RpcTypes
    {
        Put,
        Activate,
        LightingOn,
        LightingOff,
    }

    private void SendRpcPut()
    {
        MessageWriter writer = RpcWriter;
        writer.Write((byte)RpcTypes.Put);
        SendRpc(writer);
    }
    private void SendRpcLighting(bool active = true)
    {
        RpcTypes type = active ? RpcTypes.LightingOn : RpcTypes.LightingOff;

        LightingButtonInfo.ResetCoolTime();

        MessageWriter writer = RpcWriter;
        writer.Write((byte)type);
        SendRpc(writer);
    }

    public void StartMeeting()
    {
        MessageWriter writer = RpcWriter;
        writer.Write((byte)RpcTypes.Activate);
        SendRpc(writer);
        MessageWriter writer2 = RpcWriter;
        writer2.Write((byte)RpcTypes.LightingOff);
        SendRpc(writer);
    }
    public void CloseMeeting() { }
    public void OnChangeRole()
    {
        MessageWriter writer = RpcWriter;
        writer.Write((byte)RpcTypes.LightingOff);
        SendRpc(writer);
    }
    public void OnAmDeath(DeathInfo deathInfo)
    {

    }

    public void RpcReader(MessageReader reader)
    {
        RpcTypes type = (RpcTypes)reader.ReadByte();
        List<Lantern> lanterns = Lantern.GetLanterns(Player);

        switch (type)
        {
            case RpcTypes.Put:
                new GameObject("Lantern").AddComponent<Lantern>().Init(Player);
                break;
            case RpcTypes.Activate:
                lanterns?.Do(x => x.Activate());
                break;
            case RpcTypes.LightingOn:
                lanterns?.Do(x => x.LightingOn());
                break;
            case RpcTypes.LightingOff:
                lanterns?.Do(x => x.LightingOff());
                break;
        }
    }
}