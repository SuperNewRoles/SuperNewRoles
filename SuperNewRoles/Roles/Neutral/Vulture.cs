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
            requiredBodies: VultureRequiredBodies,
            canUseVent: VultureCanUseVent,
            showArrows: VultureShowArrows
        ))
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
    [CustomOptionFloat("VultureCooldown", 10f, 60f, 2.5f, 30f)]
    public static float VultureCooldown;

    [CustomOptionInt("VultureRequiredBodies", 1, 10, 1, 3)]
    public static int VultureRequiredBodies;

    [CustomOptionBool("VultureCanUseVent", false)]
    public static bool VultureCanUseVent;

    [CustomOptionBool("VultureShowArrows", true)]
    public static bool VultureShowArrows;
}

public class VultureData
{
    public float Cooldown { get; }
    public int RequiredBodies { get; }
    public bool CanUseVent { get; }
    public bool ShowArrows { get; }

    public VultureData(float cooldown, int requiredBodies, bool canUseVent, bool showArrows)
    {
        Cooldown = cooldown;
        RequiredBodies = requiredBodies;
        CanUseVent = canUseVent;
        ShowArrows = showArrows;
    }
}

public class EatDeadBodyAbility : CustomButtonBase
{
    private readonly VultureData _data;
    private int _eatenBodies = 0;
    private Dictionary<DeadBody, Arrow> _deadBodyArrows = new();
    private EventListener _fixedUpdateEvent;
    private EventListener<WrapUpEventData> _wrapUpEvent;

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
    protected override KeyCode? hotkey => KeyCode.F;

    public override bool CheckIsAvailable()
    {
        // プレイヤーが死んでいない、会議中でない、ベント内でない場合に使用可能
        if (Player.Data.IsDead || MeetingHud.Instance || Player.inVent)
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
            CustomRpcExts.RpcEndGameForHost((GameOverReason)CustomGameOverReason.VultureWin);
        }
    }

    public override void AttachToLocalPlayer()
    {
        base.AttachToLocalPlayer();

        // ベント使用可能設定
        if (_data.CanUseVent)
        {
            Player.Data.Role.CanVent = true;
        }

        // 矢印表示のイベントリスナーを設定
        if (_data.ShowArrows)
        {
            _fixedUpdateEvent = FixedUpdateEvent.Instance.AddListener(OnFixedUpdate);
            _wrapUpEvent = WrapUpEvent.Instance.AddListener(OnWrapUp);
        }
    }

    private void OnFixedUpdate()
    {
        if (!_data.ShowArrows || Player.Data.IsDead) return;

        // DeadBodyの検索を一度だけ行い、ParentIdでグループ化してキャッシュ
        DeadBody[] allDeadBodies = UnityEngine.Object.FindObjectsOfType<DeadBody>();
        Dictionary<int, DeadBody> deadBodiesByParentId = new();
        foreach (DeadBody dead in allDeadBodies)
        {
            if (!deadBodiesByParentId.ContainsKey(dead.ParentId))
                deadBodiesByParentId.Add(dead.ParentId, dead);
        }

        // 色の取得を一度だけ行う
        Color roleColor = Vulture.Instance.RoleColor;

        // 既存の矢印を更新または不要な矢印を削除
        foreach (var arrowEntry in _deadBodyArrows.ToList())
        {
            int parentId = arrowEntry.Key.ParentId;
            if (deadBodiesByParentId.ContainsKey(parentId))
            {
                if (arrowEntry.Value == null)
                    _deadBodyArrows[arrowEntry.Key] = new Arrow(roleColor);
                _deadBodyArrows[arrowEntry.Key].Update(arrowEntry.Key.transform.position, roleColor);
                _deadBodyArrows[arrowEntry.Key].arrow.SetActive(true);
            }
            else
            {
                if (arrowEntry.Value?.arrow != null)
                    UnityEngine.Object.Destroy(arrowEntry.Value.arrow);
                _deadBodyArrows.Remove(arrowEntry.Key);
            }
        }

        // 新しい死体に対して矢印を追加（既に同じParentIdの矢印が存在しなければ）
        foreach (var kv in deadBodiesByParentId)
        {
            if (_deadBodyArrows.Keys.Any(db => db.ParentId == kv.Key))
                continue;
            _deadBodyArrows.Add(kv.Value, new Arrow(roleColor));
            _deadBodyArrows[kv.Value].Update(kv.Value.transform.position, roleColor);
            _deadBodyArrows[kv.Value].arrow.SetActive(true);
        }
    }

    private void OnWrapUp(WrapUpEventData data)
    {
        // 会議終了時の処理
        ResetTimer();
    }

    public override void DetachToLocalPlayer()
    {
        base.DetachToLocalPlayer();

        // イベントリスナーを削除
        if (_fixedUpdateEvent != null)
            FixedUpdateEvent.Instance.RemoveListener(_fixedUpdateEvent);
        if (_wrapUpEvent != null)
            WrapUpEvent.Instance.RemoveListener(_wrapUpEvent);

        // 矢印を削除
        foreach (var arrow in _deadBodyArrows.Values)
        {
            if (arrow?.arrow != null)
                UnityEngine.Object.Destroy(arrow.arrow);
        }
        _deadBodyArrows.Clear();
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