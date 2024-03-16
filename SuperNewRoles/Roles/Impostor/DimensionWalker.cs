
using AmongUs.GameOptions;
using HarmonyLib;
using Hazel;
using Il2CppInterop.Generator.Extensions;
using SuperNewRoles.CustomObject;
using SuperNewRoles.Roles.Role;
using SuperNewRoles.Roles.RoleBases;
using SuperNewRoles.Roles.RoleBases.Interfaces;
using UnityEngine;

namespace SuperNewRoles.Roles.Impostor.DimensionWalker;

[HarmonyPatch]
public class DimensionWalker : RoleBase, IImpostor, ICustomButton, IRpcHandler
{
    public static new RoleInfo Roleinfo = new(
        typeof(DimensionWalker),
        (p) => new DimensionWalker(p),
        RoleId.DimensionWalker,
        "DimensionWalker",
        RoleClass.ImpostorRed,
        new(RoleId.DimensionWalker, TeamTag.Impostor),
        TeamRoleType.Impostor,
        TeamType.Impostor
        );
    public static new OptionInfo Optioninfo =
        new(RoleId.DimensionWalker, 250000, false,
            optionCreator: CreateOption);
    public static new IntroInfo Introinfo =
        new(RoleId.DimensionWalker, introSound: RoleTypes.Impostor);

    public static CustomOption PutWormHoleCooltime;
    public static CustomOption ActivateWormHoleTime;
    public static CustomOption DoPlayWormHoleAnimation;

    private static void CreateOption()
    {
        PutWormHoleCooltime = CustomOption.Create(Optioninfo.OptionId++, false, CustomOptionType.Impostor, "PutWormHoleCooltimeOption", 15f, 2.5f, 60f, 2.5f, Optioninfo.RoleOption);
        ActivateWormHoleTime = CustomOption.Create(Optioninfo.OptionId++, false, CustomOptionType.Impostor, "ActivateWormHoleTimeOption", 195f, 0f, 600f, 15f, Optioninfo.RoleOption);
        DoPlayWormHoleAnimation = CustomOption.Create(Optioninfo.OptionId++, false, CustomOptionType.Impostor, "DoPlayWormHoleAnimationOption", false, Optioninfo.RoleOption);
    }

    public CustomButtonInfo[] CustomButtonInfos { get; }
    private CustomButtonInfo PutWormHoleButtonInfo { get; }
    private CustomButtonInfo CollectWormHoleButtonInfo { get; }
    private Vent currentTargetWormHole;

    public DimensionWalker(PlayerControl p) : base(p, Roleinfo, Optioninfo, Introinfo)
    {
        PutWormHoleButtonInfo = new(3, this, () => OnButtonClick_Put(),
            (isAlive) => isAlive && !CollectWormHoleButtonInfo.CouldUse(),
            CustomButtonCouldType.CanMove,
            null,//FIXME:
            ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.aaa.png", 115f),
            () => PutWormHoleCooltime.GetFloat(), new(-2f, 1, 0),
            "PutWormHoleButtonName", KeyCode.F
        );
        CollectWormHoleButtonInfo = new(null, this, () => OnButtonClick_Collect(),
            (isAlive) => isAlive && CollectWormHoleButtonInfo.CouldUse(),
            CustomButtonCouldType.CanMove,
            null,//FIXME:
            ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.aaa.png", 115f),
            () => 0f, new(-2f, 1, 0),
            "CollectWormHoleButtonName", KeyCode.F,
            CouldUse: () => CanCollectWormHole()
        );
        CustomButtonInfos = new CustomButtonInfo[2] { PutWormHoleButtonInfo, CollectWormHoleButtonInfo };
    }

    // 能力関係
    private bool CanCollectWormHole()
    {
        var target = HudManager.Instance.ImpostorVentButton.currentTarget;
        if (target != null && target.gameObject.name == "WormHoleVent"
            && target.gameObject.transform.parent.gameObject.GetComponent<WormHole>().Owner.PlayerId == PlayerControl.LocalPlayer.PlayerId)
        {
            currentTargetWormHole = target;
            return true;
        }
        return false;
    }

    private enum AbilityTypes
    {
        PutWormHole,
        CollectWormHole,
    }
    private void OnButtonClick_Put()
    {
        MessageWriter writer = RpcWriter;
        writer.Write((byte)AbilityTypes.PutWormHole);
        writer.Write(PlayerControl.LocalPlayer.PlayerId);
        writer.Write(0);
        SendRpc(writer);
    }
    private void OnButtonClick_Collect()
    {
        MessageWriter writer = RpcWriter;
        writer.Write((byte)AbilityTypes.CollectWormHole);
        writer.Write(PlayerControl.LocalPlayer.PlayerId);
        writer.Write(currentTargetWormHole.Id);
        SendRpc(writer);
    }
    public void RpcReader(MessageReader reader)
    {
        byte abilityType = reader.ReadByte();
        PlayerControl player = reader.ReadByte().GetPlayerControl();
        int id = reader.ReadByte();

        switch ((AbilityTypes)abilityType)
        {
            case AbilityTypes.PutWormHole:
                PutWormHole(player);
                break;
            case AbilityTypes.CollectWormHole:
                CollectWormHole(id);
                break;
        }
    }
    public void PutWormHole(PlayerControl player)
    {
        new GameObject("WormHole").AddComponent<WormHole>().Init(player);
    }

    public void CollectWormHole(int wormholeId)
    {
        PutWormHoleButtonInfo.ResetCoolTime();
        PutWormHoleButtonInfo.AbilityCount++;

        UnityEngine.Object.Destroy(WormHole.GetWormHoleById(wormholeId).gameObject);
    }
}