using System;
using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using Hazel;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Events;
using SuperNewRoles.Modules;
using SuperNewRoles.Modules.Events.Bases;
using SuperNewRoles.Patches;
using SuperNewRoles.Roles.Ability;
using SuperNewRoles.Roles.Ability.CustomButton;
using UnityEngine;

namespace SuperNewRoles.Roles.Neutral;

class Vulture : RoleBase<Vulture>
{
    public override RoleId Role { get; } = RoleId.Vulture;
    public override Color32 RoleColor { get; } = new Color32(139, 69, 19, 255); // 茶色
    public override List<Func<AbilityBase>> Abilities { get; } = [
        () => new EatDeadBodyAbility(new VultureData(
            cooldown: VultureCooldown,
            requiredBodies: VultureRequiredBodies
        )),
        () => new DeadBodyArrowsAbility(() => VultureShowArrows),
        () => new CustomVentAbility(
            canUseVent: () => VultureCanUseVent
        )
    ];

    public override QuoteMod QuoteMod { get; } = QuoteMod.TheOtherRoles;
    public override RoleTypes IntroSoundType { get; } = RoleTypes.Impostor;
    public override short IntroNum { get; } = 1;

    public override AssignedTeamType AssignedTeam { get; } = AssignedTeamType.Neutral;
    public override WinnerTeamType WinnerTeam { get; } = WinnerTeamType.Neutral;
    public override TeamTag TeamTag { get; } = TeamTag.Neutral;
    public override RoleTag[] RoleTags { get; } = [];
    public override RoleOptionMenuType OptionTeam { get; } = RoleOptionMenuType.Neutral;
    public override RoleId[] RelatedRoleIds { get; } = [];

    // Vultureの設定
    [CustomOptionFloat("VultureCooldown", 2.5f, 60f, 2.5f, 30f)]
    public static float VultureCooldown;

    [CustomOptionInt("VultureRequiredBodies", 1, 10, 1, 3)]
    public static int VultureRequiredBodies;

    [CustomOptionBool("VultureCanUseVent", false, translationName: "CanUseVent")]
    public static bool VultureCanUseVent;

    [CustomOptionBool("VultureShowArrows", true)]
    public static bool VultureShowArrows;
}

public class VultureData
{
    public float Cooldown { get; }
    public int RequiredBodies { get; }

    public VultureData(float cooldown, int requiredBodies)
    {
        Cooldown = cooldown;
        RequiredBodies = requiredBodies;
    }
}

public class EatDeadBodyAbility : CustomButtonBase
{
    private readonly VultureData _data;
    private int _eatenBodies = 0;

    public EatDeadBodyAbility(VultureData data)
    {
        _data = data;
        _sprite = AssetManager.GetAsset<Sprite>("VultureEatButton.png");
        _buttonText = ModTranslation.GetString("VultureEatButton");
    }

    public override float DefaultTimer => _data.Cooldown;
    public override float Timer { get; set; }
    public override string buttonText => _buttonText;

    public override Sprite Sprite => _sprite;
    private Sprite _sprite;
    private string _buttonText;
    protected override KeyType keytype => KeyType.Ability1;

    public override bool CheckIsAvailable()
    {
        // プレイヤーが死んでいない、会議中でない、ベント内でない場合に使用可能
        if (Player.Data.IsDead || MeetingHud.Instance || Player.Player.inVent)
            return false;

        // 近くに死体があるか確認
        return HasNearbyDeadBody();
    }

    private bool HasNearbyDeadBody()
    {
        foreach (Collider2D collider in Physics2D.OverlapCircleAll(Player.GetTruePosition(), Player.MaxReportDistance, Constants.PlayersOnlyMask))
        {
            if (collider.tag != "DeadBody") continue;
            DeadBody component = collider.GetComponent<DeadBody>();
            if (component && !component.Reported)
            {
                Vector2 playerPos = Player.GetTruePosition();
                Vector2 bodyPos = component.TruePosition;
                if (!PhysicsHelpers.AnythingBetween(playerPos, bodyPos, Constants.ShipAndObjectsMask, false))
                {
                    return true;
                }
            }
        }
        return false;
    }

    public override void OnClick()
    {
        // 近くの死体を探して食べる
        foreach (Collider2D collider in Physics2D.OverlapCircleAll(Player.GetTruePosition(), Player.MaxReportDistance, Constants.PlayersOnlyMask))
        {
            if (collider.tag != "DeadBody") continue;
            DeadBody component = collider.GetComponent<DeadBody>();
            if (component && !component.Reported)
            {
                Vector2 playerPos = Player.GetTruePosition();
                Vector2 bodyPos = component.TruePosition;
                if (!PhysicsHelpers.AnythingBetween(playerPos, bodyPos, Constants.ShipAndObjectsMask, false))
                {
                    EatDeadBody(component);
                    break;
                }
            }
        }
    }

    private void EatDeadBody(DeadBody deadBody)
    {
        // 死体を消す
        RpcEatDeadBody(deadBody.ParentId, this);

        // 勝利条件を確認
        CheckWinCondition();
    }

    public bool canWin => _eatenBodies >= _data.RequiredBodies;

    private void CheckWinCondition()
    {
        if (canWin)
        {
            EndGamer.RpcEndGameWithWinner(CustomGameOverReason.VultureWin, WinType.SingleNeutral, [Player], Vulture.Instance.RoleColor, "Vulture", string.Empty);
        }
    }

    [CustomRPC]
    public static void RpcEatDeadBody(int parentId, EatDeadBodyAbility ability)
    {
        foreach (DeadBody deadBody in UnityEngine.Object.FindObjectsOfType<DeadBody>())
        {
            if (deadBody.ParentId == parentId)
            {
                GameObject.Destroy(deadBody.gameObject);
            }
        }
        ability._eatenBodies++;
    }
}