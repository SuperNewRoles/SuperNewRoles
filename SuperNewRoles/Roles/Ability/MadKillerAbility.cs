using System;
using SuperNewRoles.Events;
using SuperNewRoles.Events.PCEvents;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability.CustomButton;
using SuperNewRoles.Roles.Impostor;
using UnityEngine;
using AmongUs.GameOptions;
using SuperNewRoles.Patches;

namespace SuperNewRoles.Roles.Ability;

public record MadKillerData(
    bool hasImpostorVision,
    bool couldUseVent,
    float killCooldown,
    bool cannotBeSeenBeforePromotion,
    bool cannotSeeImpostorBeforePromotion
);

public class MadKillerAbility : AbilityBase
{
    private MadKillerData _data;
    public bool IsAwakened => _isAwakened;
    private bool _isAwakened;
    private CustomVentAbility _ventAbility;
    private ImpostorVisionAbility _visionAbility;
    private KnowImpostorAbility _knowImpostorAbility;
    public SideKillerAbility ownerAbility;
    public CustomKillButtonAbility _killButtonAbility;
    private bool _cannotBeSeenBeforePromotion;
    private float _currentKillCooldown;

    public MadKillerAbility(MadKillerData data)
    {
        _data = data;
        _isAwakened = false;
        _cannotBeSeenBeforePromotion = data.cannotBeSeenBeforePromotion;
        _currentKillCooldown = data.killCooldown;
    }
    private void OnNameTextUpdate(NameTextUpdateEventData data)
    {
        if (data.Player == Player && ExPlayerControl.LocalPlayer.IsImpostor())
        {
            if (_cannotBeSeenBeforePromotion && !_isAwakened) return;
            NameText.SetNameTextColor(data.Player, Palette.ImpostorRed);
        }
    }
    public override void AttachToAlls()
    {
        base.AttachToAlls();
        SubscribeWithAbility(NameTextUpdateEvent.Instance, OnNameTextUpdate);
        _ventAbility = new CustomVentAbility(() => _data.couldUseVent);
        _visionAbility = new ImpostorVisionAbility(() => _data.hasImpostorVision);
        _knowImpostorAbility = new KnowImpostorAbility(() => !_cannotBeSeenBeforePromotion || !_data.cannotSeeImpostorBeforePromotion);
        _killButtonAbility = new CustomKillButtonAbility(() => _isAwakened, () => _currentKillCooldown, () => true, isTargetable: (player) => SetTargetPatch.ValidMadkiller(player));

        AbilityParentAbility parentAbility = new(this);
        Player.AttachAbility(_ventAbility, parentAbility);
        Player.AttachAbility(_visionAbility, parentAbility);
        Player.AttachAbility(_knowImpostorAbility, parentAbility);
        Player.AttachAbility(_killButtonAbility, parentAbility);
    }
    public override void AttachToLocalPlayer()
    {
        base.AttachToLocalPlayer();
        SubscribeWithAbility(FixedUpdateEvent.Instance, OnFixedUpdate);
    }
    private void OnFixedUpdate()
    {
        if (_isAwakened || Player.IsDead()) return;
        // 忘却者引き継ぎ直後などオーナー情報がまだ存在しない場合は覚醒しない
        if (!TryGetOwnerPlayer(out var ownerPlayer)) return;

        // プレイヤーデータが消えた、死亡した、または役職がサイドキラーでなくなった場合
        if (ownerPlayer == null || ownerPlayer.IsDead() || ownerPlayer.Role != RoleId.SideKiller)
        {
            Awaken();
            return;
        }
    }


    public bool TryGetOwnerPlayer(out ExPlayerControl ownerPlayer)
    {
        if (ownerAbility != null)
        {
            ownerPlayer = ownerAbility.Player;
            return true;
        }

        // RpcSetMadKillerAbility が欠落しても、サイドキック作成時に全クライアントへ
        // 付与される昇格 Ability が保持している親情報から復元する。
        foreach (var promoteAbility in Player.GetAbilities<PromoteOnParentDeathAbility>())
        {
            if (promoteAbility.Parent is AbilityParentRole parentRole &&
                ReferenceEquals(parentRole.ParentRole, Player.roleBase))
            {
                ownerPlayer = promoteAbility.Owner?.Player;
                return true;
            }
        }

        ownerPlayer = null;
        return false;
    }

    private void Awaken()
    {
        if (_isAwakened || Player.IsDead()) return;
        // 送信前に立てて、同フレームの FixedUpdate 再入で二重送信しない。
        _isAwakened = true;
        RpcMadkillerAwaken(Player);
    }

    [CustomRPC]
    public static void RpcMadkillerAwaken(ExPlayerControl player)
    {
        if (player.IsDead()) return;
        var ability = player.GetAbility<MadKillerAbility>();
        RoleManager.Instance.SetRole(player, RoleTypes.Impostor);
        // 受信側でも覚醒状態を共有する。名前色や試合終了判定がローカルだけ進むのを防ぐ。
        if (ability != null)
            ability._isAwakened = true;
        NameText.UpdateAllNameInfo();
    }

    // 新しい設定を適用するメソッド
    public void UpdateSettings(bool canUseVent, bool hasImpostorVision, bool cannotBeSeenBeforePromotion, float killCooldown)
    {
        _data = _data with { couldUseVent = canUseVent, hasImpostorVision = hasImpostorVision };
        _cannotBeSeenBeforePromotion = cannotBeSeenBeforePromotion;
        _currentKillCooldown = killCooldown;
    }
}
