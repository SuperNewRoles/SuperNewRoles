using System;
using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using Hazel;
using SuperNewRoles.Helpers;
using SuperNewRoles.Replay.ReplayActions;
using SuperNewRoles.Roles.Role;
using SuperNewRoles.Roles.RoleBases;
using SuperNewRoles.Roles.RoleBases.Interfaces;
using SuperNewRoles.WaveCannonObj;
using UnityEngine;
using static SuperNewRoles.WaveCannonObj.WaveCannonObject;

namespace SuperNewRoles.Roles.Impostor;
public class WaveCannon : RoleBase, IImpostor, ICustomButton, IRpcHandler
{
    public static new RoleInfo Roleinfo = new(
        typeof(WaveCannon),
        (p) => new WaveCannon(p),
        RoleId.WaveCannon,
        "WaveCannon",
        RoleClass.ImpostorRed,
        new(RoleId.WaveCannon, TeamTag.Impostor,
            RoleTag.SpecialKiller, RoleTag.Killer, RoleTag.Hacchan),
        TeamRoleType.Impostor,
        TeamType.Impostor
        );
    public static new OptionInfo Optioninfo =
        new(RoleId.WaveCannon, 200000, false,
            CoolTimeOption: (20f, 2.5f, 180, 2.5f, false),
            DurationTimeOption: (3f, 0.5f, 15f, 0.5f, false),
            optionCreator: CreateOption);
    public static new IntroInfo Introinfo =
        new(RoleId.WaveCannon, introSound: RoleTypes.Impostor);
    public static CustomOption IsSyncKillCoolTime;
    public static CustomOption AnimationTypeOption;
    public static void CreateOption()
    {
        IsSyncKillCoolTime = CustomOption.Create(200004, false, CustomOptionType.Impostor, "IsSyncKillCoolTime", false, Optioninfo.RoleOption);
        string[] AnimTypeTexts = new string[WCCreateAnimHandlers.Count];
        int index = 0;
        foreach (string TypeName in WCCreateAnimHandlers.Keys)
        {
            AnimTypeTexts[index] = ModTranslation.GetString("WaveCannonAnimType" + TypeName);
            index++;
        }
        AnimationTypeOption = CustomOption.Create(200005, false, CustomOptionType.Impostor, "WaveCannonAnimationType", AnimTypeTexts, Optioninfo.RoleOption);
    }

    public WaveCannon(PlayerControl p) : base(p, Roleinfo, Optioninfo, Introinfo)
    {
        CustomButtonInfos = new CustomButtonInfo[1]
        {
            new(null, this, ButtonOnClick, (isAlive) => isAlive, CustomButtonCouldType.CanMove, null,
            ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.WaveCannonButton.png", 115f),
            () => Optioninfo.CoolTime, new Vector3(-2f, 1, 0),
            ModTranslation.GetString("WaveCannonButtonName"), KeyCode.F,
            DurationTime:() => Optioninfo.DurationTime, OnEffectEnds:OnEffectEnds)
        };
    }

    public void OnEffectEnds()
    {
        if (!WaveCannonObject.Objects.TryGetValue(PlayerControl.LocalPlayer.PlayerId, out WaveCannonObject obj))
        {
            Logger.Info("nullなのでreturnしました", "WaveCannonButton");
            return;
        }

        var pos = CachedPlayer.LocalPlayer.transform.position;
        MessageWriter writer = RpcWriter;
        writer.Write((byte)WaveCannonObject.RpcType.Shoot);
        writer.Write((byte)obj.Id);
        writer.Write(CachedPlayer.LocalPlayer.PlayerPhysics.FlipX);
        writer.Write(CachedPlayer.LocalPlayer.PlayerId);
        writer.Write((byte)0);
        writer.Write(pos.x);
        writer.Write(pos.y);
        SendRpc(writer);
    }
    public CustomButtonInfo[] CustomButtonInfos { get; }
    public void RpcReader(MessageReader reader)
    {
        byte Type = reader.ReadByte();
        byte Id = reader.ReadByte();
        bool IsFlipX = reader.ReadBoolean();
        byte OwnerId = reader.ReadByte();
        WCAnimType AnimType = (WCAnimType)reader.ReadByte();
        Vector3 position = new(reader.ReadSingle(), reader.ReadSingle());
        ReplayActionWavecannon.Create(Type, Id, IsFlipX, OwnerId, position);
        Logger.Info($"{(WaveCannonObject.RpcType)Type} : {Id} : {IsFlipX} : {OwnerId} : {position} : {(ModHelpers.PlayerById(OwnerId) == null ? -1 : ModHelpers.PlayerById(OwnerId).Data.PlayerName)}", "RpcWaveCannon");
        switch ((WaveCannonObject.RpcType)Type)
        {
            case WaveCannonObject.RpcType.Spawn:
                new GameObject("WaveCannon Object").AddComponent<WaveCannonObject>().Init(position, IsFlipX, ModHelpers.PlayerById(OwnerId), AnimType);
                break;
            case WaveCannonObject.RpcType.Shoot:
                WaveCannonObject.Objects[OwnerId]?.Shoot();
                break;
        }
    }
    public void ButtonOnClick()
    {
        var pos = CachedPlayer.LocalPlayer.transform.position;
        MessageWriter writer = RpcWriter;

        WCAnimType AnimType = (WCAnimType)AnimationTypeOption.GetSelection();

        writer.Write((byte)WaveCannonObject.RpcType.Spawn);
        writer.Write((byte)0);
        writer.Write(CachedPlayer.LocalPlayer.PlayerPhysics.FlipX);
        writer.Write(CachedPlayer.LocalPlayer.PlayerId);
        writer.Write((byte)AnimType);
        writer.Write(pos.x);
        writer.Write(pos.y);
        SendRpc(writer);
    }
}