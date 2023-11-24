using System.Collections.Generic;
using AmongUs.GameOptions;
using Hazel;
using SuperNewRoles.CustomObject;
using SuperNewRoles.Replay.ReplayActions;
using SuperNewRoles.Roles.Role;
using SuperNewRoles.Roles.RoleBases;
using SuperNewRoles.Roles.RoleBases.Interfaces;
using UnityEngine;

namespace SuperNewRoles.Roles.Impostor;

public class Slugger : RoleBase, IWrapUpHandler, IImpostor, ICustomButton, IRpcHandler
{
    public static new RoleInfo Roleinfo = new(
        typeof(Slugger),
        (p) => new Slugger(p),
        RoleId.Slugger,
        "Slugger",
        RoleClass.ImpostorRed,
        new(RoleId.Slugger, TeamTag.Impostor,
            RoleTag.SpecialKiller, RoleTag.Killer, RoleTag.Hacchan),
        TeamRoleType.Impostor,
        TeamType.Impostor
        );
    public static new OptionInfo Optioninfo =
        new(RoleId.Slugger, 200100, false,
            CoolTimeOption: (20f, 2.5f, 60, 2.5f, false),
            optionCreator: CreateOption);
    public static new IntroInfo Introinfo =
        new(RoleId.Slugger, introSound: RoleTypes.Impostor);

    public static CustomOption ChargeTime;
    public static CustomOption IsMultiKill;
    public static CustomOption IsKillCoolSync;

    public CustomButtonInfo[] CustomButtonInfos { get; }

    public static void CreateOption()
    {
        ChargeTime = CustomOption.Create(Optioninfo.OptionId++, false, CustomOptionType.Impostor, "SluggerChargeTime", 3f, 0f, 30f, 0.5f, Optioninfo.RoleOption);
        IsMultiKill = CustomOption.Create(Optioninfo.OptionId++, false, CustomOptionType.Impostor, "SluggerIsMultiKill", false, Optioninfo.RoleOption);
        IsKillCoolSync = CustomOption.Create(Optioninfo.OptionId++, false, CustomOptionType.Impostor, "SluggerIsSyncKillCoolTime", false, Optioninfo.RoleOption);

    }
    public Slugger(PlayerControl p) : base(p, Roleinfo, Optioninfo, Introinfo)
    {
        CustomButtonInfos = new CustomButtonInfo[1]
        {
            new(null, this, ButtonOnClick, (isAlive) => isAlive, CustomButtonCouldType.CanMove, null,
            ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.SluggerButton.png", 115f),
            () => Optioninfo.CoolTime, new(-2f, 1, 0), ModTranslation.GetString("SluggerButtonName"),
            KeyCode.F, 49, DurationTime:() => ChargeTime.GetFloat(), CouldUse:CouldUseFunc,
            OnEffectEnds:OnEffectEnds)
        };
    }
    public bool CouldUseFunc()
    {
        if (CustomButtonInfos.FirstOrDefault().customButton.isEffectActive && !PlayerControl.LocalPlayer.CanMove)
        {
            var anim = PlayerAnimation.GetPlayerAnimation(CachedPlayer.LocalPlayer.PlayerId);
            CustomButtonInfos.FirstOrDefault().customButton.isEffectActive = false;
            anim.RpcAnimation(RpcAnimationType.Stop);
        }
        return true;
    }
    public void ButtonOnClick()
    {
        var anim = PlayerAnimation.GetPlayerAnimation(CachedPlayer.LocalPlayer.PlayerId);
        anim.RpcAnimation(RpcAnimationType.SluggerCharge);
    }
    public void OnEffectEnds()
    {
        List<PlayerControl> targets = new();
        //一気にキルできるか。後に設定で変更可に
        if (IsMultiKill.GetBool())
        {
            targets = SetTarget();
        }
        else
        {
            if (FastDestroyableSingleton<HudManager>.Instance.KillButton.currentTarget != null) targets.Add(FastDestroyableSingleton<HudManager>.Instance.KillButton.currentTarget);
        }
        RpcAnimationType animationType = RpcAnimationType.SluggerMurder;
        //空振り判定
        if (targets.Count <= 0)
        {
            animationType = RpcAnimationType.SluggerMurder;
        }
        var anim = PlayerAnimation.GetPlayerAnimation(CachedPlayer.LocalPlayer.PlayerId);
        anim.RpcAnimation(animationType);
        MessageWriter writer = RpcWriter;
        writer.Write(CachedPlayer.LocalPlayer.PlayerId);
        writer.Write((byte)targets.Count);
        foreach (PlayerControl Target in targets)
        {
            writer.Write(Target.PlayerId);
            Target.RpcSetFinalStatus(FinalStatus.SluggerHarisen);
        }
        SendRpc(writer);
        CustomButtonInfos.FirstOrDefault().ResetCoolTime();
        if (IsKillCoolSync.GetBool())
        {
            PlayerControl.LocalPlayer.SetKillTimerUnchecked(RoleHelpers.GetCoolTime(CachedPlayer.LocalPlayer, null));
        }
    }
    public void RpcReader(MessageReader reader)
    {
        PlayerControl source = reader.ReadByte().GetPlayerControl();
        if (source == null) return;
        byte count = reader.ReadByte();
        List<byte> Targets = new();
        for (int i = 0; i < count; i++)
        {
            byte playerId = reader.ReadByte();
            Targets.Add(playerId);
            PlayerControl Player = playerId.GetPlayerControl();
            if (Player == null) continue;
            Player.Exiled();
            new GameObject("SluggerDeadbody").AddComponent<SluggerDeadbody>().Init(source.PlayerId, Player.PlayerId);
        }
        ReplayActionSluggerExile.Create(source.PlayerId, Targets);
    }
    public List<PlayerControl> SetTarget()
    {
        List<PlayerControl> Targets = new();
        foreach (CachedPlayer player in CachedPlayer.AllPlayers)
        {
            if (player.IsDead()) continue;
            if (player.PlayerId == CachedPlayer.LocalPlayer.PlayerId) continue;
            if (Vector2.Distance(CachedPlayer.LocalPlayer.transform.position, player.transform.position) > 1.5f) continue;
            Targets.Add(player);
        }
        return Targets;
    }
}
/*
    public static class Slugger
    {
        public static List<PlayerControl> SluggerPlayer;
        public static Color32 color = ImpostorRed;
        public static Sprite GetButtonSprite() => ;
        public static void ClearAndReload()
        {
            SluggerPlayer = new();
        }
    }*/
/**/