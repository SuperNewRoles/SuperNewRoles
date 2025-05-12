using System;
using System.Collections.Generic;
using System.Linq;
using SuperNewRoles.Events;
using SuperNewRoles.Events.PCEvents;
using SuperNewRoles.Modules;
using SuperNewRoles.Modules.Events.Bases;
using SuperNewRoles.Roles.Ability;
using UnityEngine;

namespace SuperNewRoles.Roles.Modifiers;


class Lovers : ModifierBase<Lovers>
{
    public override ModifierRoleId ModifierRole => ModifierRoleId.Lovers;

    public override Color32 RoleColor => new(255, 105, 180, byte.MaxValue);

    public override List<Func<AbilityBase>> Abilities => [() => new LoversAbility(LoversKnowPartnerRole, LoversKnowPartnerPosition), () => new CustomTaskAbility(() => (false, 0), null)];

    public override QuoteMod QuoteMod => QuoteMod.SuperNewRoles;

    public override List<AssignedTeamType> AssignedTeams => [];

    public override WinnerTeamType WinnerTeam => WinnerTeamType.Crewmate;

    public override RoleTag[] RoleTags => [];

    public override short IntroNum => 1;
    public override Func<ExPlayerControl, string> ModifierMark => (player) => "{0}"; // + ModHelpers.Cs(player.AmOwner && player.IsAlive() && player.Role != RoleId.God ? RoleColor : player.GetAbility<LoversAbility>().HeartColor, "♥");
    public override bool HiddenOption => true;

    [Modifier]
    [AssignFilter]
    public static CustomOptionCategory LoversCategory;

    [CustomOptionFloat("LoversMaxCoupleCount", 0f, 15f, 1f, 0f, parentFieldName: nameof(LoversCategory))]
    public static float LoversMaxCoupleCount;

    [CustomOptionFloat("LoversSpawnChance", 0f, 100f, 5f, 100f, parentFieldName: nameof(LoversCategory))]
    public static float LoversSpawnChance;

    //クラードと重複しない
    [CustomOptionBool("LoversAvoidQuarreled", true, parentFieldName: nameof(LoversCategory))]
    public static bool LoversAvoidQuarreled;

    // 追加勝利
    [CustomOptionBool("LoversAdditionalWinCondition", true, parentFieldName: nameof(LoversCategory))]
    public static bool LoversAdditionalWinCondition;
    // 元の陣営で勝利出来ない
    [CustomOptionBool("LoversOriginalTeamCannotWin", true, parentFieldName: nameof(LoversCategory))]
    public static bool LoversOriginalTeamCannotWin;
    // 相方の役職がわかる
    [CustomOptionBool("LoversKnowPartnerRole", true, parentFieldName: nameof(LoversCategory))]
    public static bool LoversKnowPartnerRole;
    // 相方の位置がわかる
    [CustomOptionBool("LoversKnowPartnerPosition", true, parentFieldName: nameof(LoversCategory))]
    public static bool LoversKnowPartnerPosition;

    // 抽選にインポスターを含めるかどうか
    [CustomOptionBool("LoversIncludeImpostorsInSelection", false, parentFieldName: nameof(LoversCategory))]
    public static bool LoversIncludeImpostorsInSelection;
    // 抽選に第3陣営を含めるかどうか

    [CustomOptionBool("LoversIncludeThirdTeamInSelection", false, parentFieldName: nameof(LoversCategory))]
    public static bool LoversIncludeThirdTeamInSelection;
}

public class LoversAbility : AbilityBase
{
    public Color32 HeartColor => couple.HeartColor;
    public LoversCouple couple { get; private set; }
    private EventListener<DieEventData> _dieListener;
    private EventListener<NameTextUpdateEventData> _nameTextUpdateListener;
    private EventListener _fixedUpdateListener;
    private PlayerArrowsAbility _playerArrowsAbility;
    private KnowOtherAbility _knowOtherAbility;
    private bool knowPartnerRole;
    private bool knowPartnerPosition;
    public LoversAbility(bool knowPartnerRole, bool knowPartnerPosition)
    {
        this.knowPartnerRole = knowPartnerRole;
        this.knowPartnerPosition = knowPartnerPosition;
    }
    public void SetCouple(LoversCouple couple)
    {
        this.couple = couple;
    }
    public override void AttachToAlls()
    {
        _dieListener = DieEvent.Instance.AddListener(OnDie);
        _nameTextUpdateListener = NameTextUpdateEvent.Instance.AddListener(OnNameTextUpdate);
        _playerArrowsAbility = new PlayerArrowsAbility(
            () => !knowPartnerPosition ? Array.Empty<ExPlayerControl>() : couple.lovers.Where(ability => !ability.Player.AmOwner && (ExPlayerControl.LocalPlayer.IsDead() || ability.Player.IsAlive())).Select(ability => ability.Player),
            (player) => Lovers.Instance.RoleColor
        );
        _knowOtherAbility = new KnowOtherAbility(
            (player) => knowPartnerRole && IsCoupleWith(player), () => true
        );
        Player.AttachAbility(_playerArrowsAbility, new AbilityParentAbility(this));
        Player.AttachAbility(_knowOtherAbility, new AbilityParentAbility(this));
        _fixedUpdateListener = FixedUpdateEvent.Instance.AddListener(OnFixedUpdate);
    }
    public override void DetachToAlls()
    {
        _dieListener?.RemoveListener();
        _nameTextUpdateListener?.RemoveListener();
        _fixedUpdateListener?.RemoveListener();
    }
    private void OnFixedUpdate()
    {
        if (!AmongUsClient.Instance.AmHost) return;
        if (Player.IsDead()) return;
        if (ShipStatus.Instance?.enabled != true) return;
        if (!couple.CheckWin(Player)) return;
        if (ExPlayerControl.ExPlayerControls.Count(player => player.IsAlive()) == couple.lovers.Count + 1)
        {
            EndGamer.RpcEndGameWithWinner(Patches.CustomGameOverReason.LoversWin, WinType.SingleNeutral, couple.lovers.Select(ability => ability.Player).ToArray(), Lovers.Instance.RoleColor, "Lovers", "WinText");
        }
    }
    private void OnNameTextUpdate(NameTextUpdateEventData data)
    {
        if (data.Player != Player) return;
        if (!data.Player.IsLovers()) return;
        if (ExPlayerControl.LocalPlayer.IsAlive() && ExPlayerControl.LocalPlayer.Role != RoleId.God && !IsCoupleWith(ExPlayerControl.LocalPlayer)) return;
        if (data.Player.cosmetics.nameText.text.Contains("♥")) return;
        data.Player.cosmetics.nameText.text += ModHelpers.Cs(ExPlayerControl.LocalPlayer.IsDead() || ExPlayerControl.LocalPlayer.Role == RoleId.God ? HeartColor : Lovers.Instance.RoleColor, "♥");
    }
    private void OnDie(DieEventData data)
    {
        Logger.Info($"OnDie: {data.player.name}");
        Logger.Info($"Dead: {ExPlayerControl.LocalPlayer.IsDead()}");
        if (!Player.AmOwner) return;
        if (ExPlayerControl.LocalPlayer.IsDead()) return;
        if (IsCoupleWith(data.player))
        {
            if (ExileController.Instance != null)
                ExPlayerControl.LocalPlayer.RpcCustomDeath(CustomDeathType.LoversSuicideMurderWithoutDeadbody);
            else
                ExPlayerControl.LocalPlayer.RpcCustomDeath(CustomDeathType.LoversSuicide);
        }
    }
    public bool IsCoupleWith(ExPlayerControl player)
    {
        return couple.lovers.Any(l => l.Player == player);
    }
}

public class LoversCouple
{
    public Color32 HeartColor { get; set; }
    public List<LoversAbility> lovers { get; set; } = [];
    public int loversIndex { get; set; }
    // カップルの中で一番PlayerIdが低い場合
    public bool CheckWin(ExPlayerControl player)
    {
        return lovers.Min(l => l.Player.PlayerId) == player.PlayerId;
    }
    public static readonly Color32[] LoversHearts = [
        new(255, 145, 200, byte.MaxValue),
        new(255, 155, 056, byte.MaxValue),
        new(169, 084, 255, byte.MaxValue),
        new(255, 138, 138, byte.MaxValue),
        new(255, 087, 255, byte.MaxValue),
        new(059, 195, 059, byte.MaxValue),
        new(094, 255, 255, byte.MaxValue),
        new(255, 223, 107, byte.MaxValue),
        new(054, 154, 255, byte.MaxValue),
        new(255, 196, 255, byte.MaxValue),
        new(255, 082, 168, byte.MaxValue),
        new(099, 099, 255, byte.MaxValue),
        new(181, 255, 107, byte.MaxValue),
        new(255, 178, 255, byte.MaxValue)
    ];
    private static Color32 OutrangeLoversHeartColor = new(255, 107, 107, byte.MaxValue);
    public LoversCouple(List<LoversAbility> lovers, int loversIndex)
    {
        this.lovers = lovers;
        this.loversIndex = loversIndex;
        if (loversIndex >= LoversHearts.Length)
        {
            Logger.Warning($"LoversCouple: loversIndexが配列の範囲を超えています: {loversIndex}");
            HeartColor = OutrangeLoversHeartColor;
        }
        else
            HeartColor = LoversHearts[loversIndex];
    }
}