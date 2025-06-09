using System;
using System.Collections.Generic;
using System.Linq;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability.CustomButton;
using SuperNewRoles.Roles.Neutral;
using SuperNewRoles.Events;
using SuperNewRoles.Events.PCEvents;
using SuperNewRoles.Modules.Events.Bases;
using UnityEngine;
using Hazel;
using AmongUs.GameOptions;

namespace SuperNewRoles.Roles.Ability;

public class OrientalShamanAbility : AbilityBase
{
    public OrientalShamanData Data { get; set; }

    private CustomVentAbility _ventAbility;
    private CustomSidekickButtonAbility _servantAbility;
    private EventListener<ExileEventData> _exileListener;
    private KnowOtherAbility _knowOtherAbility;
    private CustomTaskAbility _taskAbility;
    private SabotageCanUseAbility _sabotageCanUseAbility;
    private PlayerArrowsAbility _playerArrowsAbility;

    public ShermansServantAbility _servant;

    public OrientalShamanAbility(OrientalShamanData data)
    {
        Data = data;
    }

    public override void AttachToAlls()
    {
        base.AttachToAlls();

        // ベント能力の初期化
        _ventAbility = new CustomVentAbility(
            canUseVent: () => Data.canUseVent,
            ventCooldown: () => Data.ventCooldown,
            ventDuration: () => Data.ventDuration
        );

        _knowOtherAbility = new KnowOtherAbility(
            canKnowOther: (player) => player.IsKiller(),
            () => false
        );

        // 式神作成能力の初期化
        var servantOptions = new CustomSidekickButtonAbilityOptions(
            canCreateSidekick: (created) => Data.canCreateServant && !created,
            sidekickCooldown: () => Data.servantCooldown,
            sidekickRole: () => RoleId.ShermansServant,
            sidekickRoleVanilla: () => RoleTypes.Crewmate,
            sidekickSprite: AssetManager.GetAsset<Sprite>("OrientalShamanButton.png"),
            sidekickText: ModTranslation.GetString("OrientalShamanServantButton"),
            sidekickCount: () => 1,
            isTargetable: (player) => player.IsAlive() && !player.IsImpostor() && player.Role != RoleId.OrientalShaman,
            sidekickSuccess: (target) =>
            {
                RpcServantCreate(target);
                return false;
            }
        );

        _taskAbility = new CustomTaskAbility(
            () => (Data.neededTaskComplete, false, null),
            Data.neededTaskComplete ? Data.task : null
        );

        _sabotageCanUseAbility = new SabotageCanUseAbility(
            () => SabotageType.All
        );

        _playerArrowsAbility = new PlayerArrowsAbility(() => [_servant?.Player], (player) => OrientalShaman.Instance.RoleColor);

        _servantAbility = new CustomSidekickButtonAbility(servantOptions);

        // インポスター視界の設定
        Player.AttachAbility(_knowOtherAbility, new AbilityParentAbility(this));
        Player.AttachAbility(new ImpostorVisionAbility(() => Data.isImpostorVision), new AbilityParentAbility(this));
        Player.AttachAbility(_servantAbility, new AbilityParentAbility(this));
        Player.AttachAbility(_ventAbility, new AbilityParentAbility(this));
        Player.AttachAbility(new HideVentAnimationAbility(() => true), new AbilityParentAbility(this));
        Player.AttachAbility(_taskAbility, new AbilityParentAbility(this));
        Player.AttachAbility(_sabotageCanUseAbility, new AbilityParentAbility(this));
        Player.AttachAbility(_playerArrowsAbility, new AbilityParentAbility(this));
    }

    public override void AttachToLocalPlayer()
    {
        base.AttachToLocalPlayer();
        _exileListener = ExileEvent.Instance.AddListener(OnExile);
    }

    public override void DetachToLocalPlayer()
    {
        _exileListener?.RemoveListener();
    }

    private void OnExile(ExileEventData data)
    {
        // 会議終了時の処理
    }

    [CustomRPC]
    private void RpcServantCreate(ExPlayerControl target)
    {
        RoleManager.Instance.SetRole(target, RoleTypes.Crewmate);
        target.SetRole(RoleId.ShermansServant);
        if (target.TryGetAbility<ShermansServantAbility>(out var servant))
        {
            servant.SetParent(this);
            _servant = servant;
        }
        else
        {
            Logger.Error($"OrientalShaman created servant: {target.Data.PlayerName} but servant ability not found");
        }
        // 名前の更新
        NameText.UpdateNameInfo(target);
        NameText.UpdateNameInfo(Player);
        // 式神作成時の処理
        Logger.Info($"OrientalShaman created servant: {target.Data.PlayerName}");
    }
}