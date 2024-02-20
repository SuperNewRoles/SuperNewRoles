
using System;
using System.Linq;
using AmongUs.GameOptions;
using HarmonyLib;
using Hazel;
using SuperNewRoles.Roles.Role;
using SuperNewRoles.Roles.RoleBases;
using SuperNewRoles.Roles.RoleBases.Interfaces;
using UnityEngine;

namespace SuperNewRoles.Roles.Impostor.Crab;

// 提案者：穴熊よしはる さん
[HarmonyPatch]
public class Crab : RoleBase, IImpostor, ICustomButton, IDeathHandler, IRpcHandler
{
    public static new RoleInfo Roleinfo = new(
        typeof(Crab),
        (p) => new Crab(p),
        RoleId.Crab,
        "Crab",
        RoleClass.ImpostorRed,
        new(RoleId.Crab, TeamTag.Impostor),
        TeamRoleType.Impostor,
        TeamType.Impostor
        );
    public static new OptionInfo Optioninfo =
        new(RoleId.Crab, 206100, false,
            CoolTimeOption: (17.5f, 2.5f, 90f, 2.5f, false),
            DurationTimeOption: (5f, 0.5f, 30f, 0.5f, false));
    public static new IntroInfo Introinfo =
        new(RoleId.Crab, introSound: RoleTypes.Impostor);

    public bool IsUsingAbility { get; private set; }

    public Crab(PlayerControl p) : base(p, Roleinfo, Optioninfo, Introinfo)
    {
        IsUsingAbility = false;
        CrabButtonInfo = new(null, this, () => CrabButtonOnClick(),
            (isAlive) => isAlive,
            CustomButtonCouldType.CanMove,
            () => ResetAbility(),
            ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.crabButton.png", 115f),
            () => Optioninfo.CoolTime, new(-2f, 1, 0),
            "CrabButtonName", KeyCode.F,
            DurationTime: () => Optioninfo.DurationTime,
            OnEffectEnds: () => ResetAbility());
        CustomButtonInfos = new CustomButtonInfo[1] { CrabButtonInfo };
    }

    // ボタン関係
    public CustomButtonInfo[] CustomButtonInfos { get; }
    private CustomButtonInfo CrabButtonInfo { get; }
    private void CrabButtonOnClick()
    {
        MessageWriter writer = RpcWriter;
        writer.Write(true);
        SendRpc(writer);
    }
    private void ResetAbility()
    {
        CrabButtonInfo.ResetCoolTime();

        MessageWriter writer = RpcWriter;
        writer.Write(false);
        SendRpc(writer);
    }

    // 能力関係
    public void RpcReader(MessageReader reader)
    {
        bool active = reader.ReadBoolean();
        IsUsingAbility = active;
    }
    [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.SetNormalizedVelocity)), HarmonyPrefix]
    static bool SetVelocity(PlayerPhysics __instance, [HarmonyArgument(0)] Vector2 direction)
    {
        bool anyoneUsingAbility = RoleBaseManager.GetRoleBases<Crab>().Any(x => x.IsUsingAbility);
        if (!anyoneUsingAbility) return true;

        direction.y *= 0;
        __instance.body.velocity = direction * __instance.TrueSpeed;
        return false;
    }
    // 死んだとき強制解除
    public void OnDeath(DeathInfo deathInfo)
    {
        if (deathInfo.DeathPlayer.PlayerId != Player.PlayerId) return;
        ResetAbility();
    }
}