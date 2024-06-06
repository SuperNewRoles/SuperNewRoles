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
public class WaveCannon : RoleBase, IImpostor, ICustomButton
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
        List<string> AnimTypeTexts = [];
        foreach (string TypeName in WCCreateAnimHandlers.Keys)
        {
            if (!Enum.TryParse(TypeName, out WCAnimType animType) || animType >= WCAnimType.None)
                continue;
            AnimTypeTexts.Add(ModTranslation.GetString("WaveCannonAnimType" + TypeName));
        }
        AnimationTypeOption = CustomOption.Create(200005, false, CustomOptionType.Impostor, "WaveCannonAnimationType", AnimTypeTexts.ToArray(), Optioninfo.RoleOption);
    }

    public WaveCannon(PlayerControl p) : base(p, Roleinfo, Optioninfo, Introinfo)
    {
        CustomButtonInfos = [
            new(null, this, ButtonOnClick, (isAlive) => isAlive, CustomButtonCouldType.CanMove, null,
            ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.WaveCannonButton.png", 115f),
            () => Optioninfo.CoolTime, new Vector3(-2f, 1, 0),
            ModTranslation.GetString("WaveCannonButtonName"), KeyCode.F,
            DurationTime:() => Optioninfo.DurationTime, OnEffectEnds:OnEffectEnds)
        ];
    }

    public void OnEffectEnds()
    {
        if (!WaveCannonObject.Objects.TryGetValue(PlayerControl.LocalPlayer.PlayerId, out WaveCannonObject obj))
        {
            Logger.Info("nullなのでreturnしました", "WaveCannonButton");
            return;
        }

        var pos = CachedPlayer.LocalPlayer.transform.position;
        MessageWriter writer = RPCHelper.StartRPC(CustomRPC.WaveCannon);
        writer.Write((byte)WaveCannonObject.RpcType.Shoot);
        writer.Write((byte)obj.Id);
        writer.Write(CachedPlayer.LocalPlayer.PlayerPhysics.FlipX);
        writer.Write(CachedPlayer.LocalPlayer.PlayerId);
        writer.Write(pos.x);
        writer.Write(pos.y);
        writer.Write((byte)AnimationTypeOption.GetSelection());
        writer.EndRPC();
        RPCProcedure.WaveCannon((byte)RpcType.Shoot, (byte)obj.Id, CachedPlayer.LocalPlayer.PlayerPhysics.FlipX, CachedPlayer.LocalPlayer.PlayerId, pos, (WCAnimType)AnimationTypeOption.GetSelection());
    }
    public CustomButtonInfo[] CustomButtonInfos { get; }
    public void ButtonOnClick()
    {
        var pos = CachedPlayer.LocalPlayer.transform.position;
        WCAnimType AnimType = (WCAnimType)AnimationTypeOption.GetSelection();
        MessageWriter writer = RPCHelper.StartRPC(CustomRPC.WaveCannon);
        writer.Write((byte)RpcType.Spawn);
        writer.Write((byte)0);
        writer.Write(CachedPlayer.LocalPlayer.PlayerPhysics.FlipX);
        writer.Write(CachedPlayer.LocalPlayer.PlayerId);
        writer.Write(pos.x);
        writer.Write(pos.y);
        writer.Write((byte)AnimType);
        writer.EndRPC();
        RPCProcedure.WaveCannon((byte)RpcType.Spawn, 0, CachedPlayer.LocalPlayer.PlayerPhysics.FlipX, CachedPlayer.LocalPlayer.PlayerId, pos, AnimType);

    }
}