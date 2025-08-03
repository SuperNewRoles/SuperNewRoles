using System;
using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.CustomObject;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability;
using SuperNewRoles.Roles.Ability.CustomButton;
using UnityEngine;

namespace SuperNewRoles.Roles.Impostor;

internal class DimensionWalker : RoleBase<DimensionWalker>
{
    public override RoleId Role { get; } = RoleId.DimensionWalker;
    public override Color32 RoleColor { get; } = Palette.ImpostorRed;
    public override List<Func<AbilityBase>> Abilities { get; } = [
        () => new DimensionWalkerAbility(
            new DimensionWalkerAbilityData(
                DimensionWalkerCooldown,
                DimensionWalkerActivateTime,
                DimensionWalkerWormHoleCount,
                DimensionWalkerPlayAnimation
            )
        )
    ];

    public override QuoteMod QuoteMod { get; } = QuoteMod.SuperNewRoles;
    public override RoleTypes IntroSoundType { get; } = RoleTypes.Impostor;
    public override short IntroNum { get; } = 1;

    public override AssignedTeamType AssignedTeam { get; } = AssignedTeamType.Impostor;
    public override WinnerTeamType WinnerTeam { get; } = WinnerTeamType.Impostor;
    public override TeamTag TeamTag { get; } = TeamTag.Impostor;
    public override RoleTag[] RoleTags { get; } = [RoleTag.Support, RoleTag.ImpostorTeam];
    public override RoleOptionMenuType OptionTeam { get; } = RoleOptionMenuType.Impostor;

    [CustomOptionFloat("DimensionWalkerCooldown", 0f, 120f, 2.5f, 25f, translationName: "CoolTime")]
    public static float DimensionWalkerCooldown;
    [CustomOptionFloat("DimensionWalkerActivateTime", 0f, 600f, 15f, 195f)]
    public static float DimensionWalkerActivateTime;
    [CustomOptionInt("DimensionWalkerWormHoleCount", 1, 10, 1, 3, translationName: "UseLimit")]
    public static int DimensionWalkerWormHoleCount;
    [CustomOptionBool("DimensionWalkerPlayAnimation", false)]
    public static bool DimensionWalkerPlayAnimation;
}

public record DimensionWalkerAbilityData(float cooldown, float activateTime, int wormHoleCount, bool playAnimation);

internal class DimensionWalkerAbility : CustomButtonBase
{
    public override float DefaultTimer => Data.cooldown;

    public override string buttonText => ModTranslation.GetString(isCollectMode ? "DimensionWalkerCollectButtonText" : "DimensionWalkerPutButtonText");

    public override Sprite Sprite => isCollectMode ? AssetManager.GetAsset<Sprite>("DimensionWalkerCollectButton.png") : AssetManager.GetAsset<Sprite>("DimensionWalkerPutButton.png");

    protected override KeyType keytype => KeyType.Ability1;

    public DimensionWalkerAbilityData Data { get; }

    private Vent currentTargetWormHole;
    private bool isCollectMode = false;

    public override ShowTextType showTextType => ShowTextType.ShowWithCount;

    public DimensionWalkerAbility(DimensionWalkerAbilityData data)
    {
        Data = data;
        Count = data.wormHoleCount;
    }

    public override bool CheckIsAvailable()
    {
        if (isCollectMode)
        {
            return CanCollectWormHole();
        }
        return PlayerControl.LocalPlayer.CanMove && !IsNearDoor(PlayerControl.LocalPlayer) && HasCount;
    }

    /// <summary>
    /// プレイヤーがドアの近くにいるか判定する
    /// </summary>
    /// <returns>playerがドアの近くにいる</returns>
    public bool IsNearDoor(PlayerControl player)
    {
        var all = ShipStatus.Instance.AllDoors;

        foreach (OpenableDoor door in all)
        {
            var distance = Vector2.Distance(player.GetTruePosition(), door.gameObject.transform.position);
            if (distance <= 1f)
                return true;
        }

        return false;
    }

    public override void OnClick()
    {
        if (isCollectMode)
        {
            RpcCollectWormHole(currentTargetWormHole.Id);
        }
        else
        {
            RpcPutWormHole(ExPlayerControl.LocalPlayer);
        }
    }

    public override void OnUpdate()
    {
        base.OnUpdate();

        // ワームホールの回収モードかどうかを判定
        bool canCollect = CanCollectWormHole();
        if (canCollect != isCollectMode)
            isCollectMode = canCollect;
    }

    private bool CanCollectWormHole()
    {
        var target = HudManager.Instance.ImpostorVentButton.currentTarget;
        if (target != null && WormHole.IsWormHole(target))
        {
            var wormHole = WormHole.GetWormHoleById(target.Id);
            if (wormHole != null && wormHole.Owner.PlayerId == PlayerControl.LocalPlayer.PlayerId)
            {
                currentTargetWormHole = target;
                return true;
            }
        }
        return false;
    }

    [CustomRPC]
    public void RpcPutWormHole(ExPlayerControl player)
    {
        PutWormHole(player);
        Count--;
    }

    [CustomRPC]
    public void RpcCollectWormHole(int wormholeId)
    {
        CollectWormHole(wormholeId);
        Count++;
    }

    private void PutWormHole(ExPlayerControl player)
    {
        new GameObject("WormHole").AddComponent<WormHole>().Init(player.Data.Object, Data.activateTime, Data.playAnimation);
    }

    private void CollectWormHole(int wormholeId)
    {
        ResetTimer();

        var wormHole = WormHole.GetWormHoleById(wormholeId);
        if (wormHole != null)
        {
            wormHole.InActivate();
        }
    }
}