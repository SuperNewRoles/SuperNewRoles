using System;
using System.Collections.Generic;
using System.Linq;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Events;
using SuperNewRoles.Events.PCEvents;
using SuperNewRoles.Modules;
using SuperNewRoles.Modules.Events.Bases;
using SuperNewRoles.Roles.Ability;
using SuperNewRoles.Roles.Ability.CustomButton;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SuperNewRoles.Roles.Neutral;

internal sealed class Frankenstein : RoleBase<Frankenstein>
{
    public override RoleId Role { get; } = RoleId.Frankenstein;
    public override Sprite RoleIcon => AssetManager.GetAsset<Sprite>("FrankensteinRoleIcon.png");
    public override Color32 RoleColor { get; } = new(122, 169, 146, byte.MaxValue);
    public override List<Func<AbilityBase>> Abilities { get; } = [
        () => new FrankensteinAbility(new FrankensteinData(
            CreateCooldown: FrankensteinCreateCoolTime,
            WinKillCount: FrankensteinWinKillCount,
            MonsterHasImpostorVision: FrankensteinMonsterImpostorLight,
            MonsterCanUseVent: FrankensteinMonsterCanVent
        ))
    ];

    public override QuoteMod QuoteMod { get; } = QuoteMod.SuperNewRoles;
    public override short IntroNum { get; } = 1;

    public override AssignedTeamType AssignedTeam { get; } = AssignedTeamType.Neutral;
    public override WinnerTeamType WinnerTeam { get; } = WinnerTeamType.Neutral;
    public override TeamTag TeamTag { get; } = TeamTag.Neutral;
    public override RoleTag[] RoleTags { get; } = [];
    public override RoleOptionMenuType OptionTeam { get; } = RoleOptionMenuType.Neutral;
    public override RoleId[] RelatedRoleIds { get; } = [];

    [CustomOptionFloat("FrankensteinCreateCoolTimeSetting", 2.5f, 60f, 2.5f, 30f)]
    public static float FrankensteinCreateCoolTime;

    [CustomOptionInt("FrankensteinWinKillCountSetting", 1, 10, 1, 3)]
    public static int FrankensteinWinKillCount;

    [CustomOptionBool("FrankensteinMonsterImpostorLightSetting", true, translationName: "HasImpostorVision")]
    public static bool FrankensteinMonsterImpostorLight;

    [CustomOptionBool("FrankensteinMonsterCanVentSetting", true, translationName: "CanUseVent")]
    public static bool FrankensteinMonsterCanVent;
}

public sealed record FrankensteinData(
    float CreateCooldown,
    int WinKillCount,
    bool MonsterHasImpostorVision,
    bool MonsterCanUseVent
);

public sealed class FrankensteinAbility : AbilityBase
{
    private readonly FrankensteinData _data;

    private FrankensteinCreateMonsterButtonAbility _createButton;
    private CustomKillButtonAbility _killButton;
    private CustomVentAbility _ventAbility;
    private ImpostorVisionAbility _impostorVisionAbility;

    private EventListener<TryKillEventData> _tryKillListener;
    private EventListener<MeetingStartEventData> _meetingStartListener;
    private EventListener<DieEventData> _dieListener;

    private DeadBody _monsterBody;
    private byte _monsterBodyPlayerId = byte.MaxValue;
    private Vector2 _playerOriginalPosition;
    private Vector2 _bodyOriginalPosition;

    public int RemainingKillsToWin { get; private set; }
    public bool IsMonster => _monsterBodyPlayerId != byte.MaxValue;

    public FrankensteinAbility(FrankensteinData data)
    {
        _data = data;
        RemainingKillsToWin = Mathf.Max(0, _data.WinKillCount);
    }

    public override void AttachToAlls()
    {
        _createButton = new FrankensteinCreateMonsterButtonAbility(this, _data.CreateCooldown);
        _killButton = new CustomKillButtonAbility(
            canKill: () => IsMonster,
            killCooldown: () => 0f,
            onlyCrewmates: () => false,
            customKillHandler: target =>
            {
                if (!IsMonster) return true;
                if (target == null) return true;

                ExPlayerControl.LocalPlayer.RpcCustomDeath(target, CustomDeathType.Kill);

                Vector2 dropPos = Player.Player.GetTruePosition();
                RpcEndMonster(this, dropPos, decrementKill: true);
                return true;
            }
        );
        _ventAbility = new CustomVentAbility(() => IsMonster && _data.MonsterCanUseVent);
        _impostorVisionAbility = new ImpostorVisionAbility(() => IsMonster && _data.MonsterHasImpostorVision);

        var parent = new AbilityParentAbility(this);
        Player.AddAbility(_createButton, parent);
        Player.AddAbility(_killButton, parent);
        Player.AddAbility(_ventAbility, parent);
        Player.AddAbility(_impostorVisionAbility, parent);

        _tryKillListener = TryKillEvent.Instance.AddListener(OnTryKill);
        _meetingStartListener = MeetingStartEvent.Instance.AddListener(OnMeetingStart);
        _dieListener = DieEvent.Instance.AddListener(OnDie);
    }

    public override void DetachToAlls()
    {
        if (Player.AmOwner && IsMonster)
        {
            RpcEndMonster(this, _bodyOriginalPosition, decrementKill: false);
        }

        _tryKillListener?.RemoveListener();
        _meetingStartListener?.RemoveListener();
        _dieListener?.RemoveListener();

        base.DetachToAlls();
    }

    private void OnTryKill(TryKillEventData data)
    {
        if (!IsMonster) return;
        if (data.RefTarget != Player) return;

        data.RefSuccess = false;

        if (data.Killer.AmOwner)
            data.Killer.ResetKillCooldown();

        if (data.Killer.AmOwner || ExPlayerControl.LocalPlayer.IsDead())
            data.RefTarget.Player.ShowFailedMurder();

        if (Player.AmOwner)
        {
            Vector2 dropPos = Player.Player.GetTruePosition();
            RpcEndMonster(this, dropPos, decrementKill: false);
        }
    }

    private void OnMeetingStart(MeetingStartEventData data)
    {
        if (!Player.AmOwner) return;
        if (!IsMonster) return;

        RpcEndMonster(this, _bodyOriginalPosition, decrementKill: false);
    }

    private void OnDie(DieEventData data)
    {
        if (!Player.AmOwner) return;
        if (data.player?.PlayerId != Player.PlayerId) return;
        if (!IsMonster) return;

        RpcEndMonster(this, _bodyOriginalPosition, decrementKill: false);
    }

    internal DeadBody FindNearestAvailableDeadBody()
    {
        if (!Player.AmOwner) return null;

        Vector2 pos = PlayerControl.LocalPlayer.GetTruePosition();
        float maxDist = PlayerControl.LocalPlayer.MaxReportDistance;

        DeadBody best = null;
        float bestDist = float.MaxValue;

        foreach (Collider2D collider in Physics2D.OverlapCircleAll(pos, maxDist, Constants.PlayersOnlyMask))
        {
            if (!collider.CompareTag("DeadBody")) continue;
            DeadBody body = collider.GetComponent<DeadBody>();
            if (body == null) continue;
            if (!body.gameObject.activeInHierarchy) continue;
            if (body.Reported) continue;
            if (IsDeadBodyInUse(body)) continue;

            Vector2 bodyPos = body.TruePosition;
            float dist = Vector2.Distance(pos, bodyPos);
            if (dist > maxDist) continue;
            if (PhysicsHelpers.AnythingBetween(pos, bodyPos, Constants.ShipAndObjectsMask, false)) continue;

            if (dist < bestDist)
            {
                best = body;
                bestDist = dist;
            }
        }

        return best;
    }

    private static bool IsDeadBodyInUse(DeadBody body)
    {
        foreach (ExPlayerControl player in ExPlayerControl.ExPlayerControls)
        {
            if (player == null) continue;

            if (player.Role == RoleId.Matryoshka && player.TryGetAbility<MatryoshkaAbility>(out var matryoshka) && matryoshka.currentWearingBody == body)
                return true;

            if (player.Role == RoleId.Owl && player.TryGetAbility<OwlDeadBodyTransportAbility>(out var owl) && owl.DeadBodyInTransport == body)
                return true;

            if (player.Role == RoleId.Frankenstein && player.TryGetAbility<FrankensteinAbility>(out var franken) && franken._monsterBody == body)
                return true;
        }

        return false;
    }

    private static void SetDeadBodyActive(DeadBody body, bool active)
    {
        if (body == null) return;
        body.Reported = !active;
        if (body.myCollider != null)
            body.myCollider.enabled = active;
        if (body.bodyRenderers != null)
        {
            foreach (var renderer in body.bodyRenderers)
            {
                if (renderer != null)
                    renderer.enabled = active;
            }
        }
        body.gameObject.SetActive(active);
    }

    [CustomRPC]
    public static void RpcStartMonster(FrankensteinAbility ability, byte bodyPlayerId, Vector2 playerOriginalPosition, Vector2 bodyOriginalPosition)
    {
        if (ability == null) return;
        if (ability.IsMonster) return;

        DeadBody body = Object.FindObjectsOfType<DeadBody>().FirstOrDefault(x => x != null && x.ParentId == bodyPlayerId);
        if (body == null) return;
        if (body.Reported) return;
        if (IsDeadBodyInUse(body)) return;

        ability._playerOriginalPosition = playerOriginalPosition;
        ability._bodyOriginalPosition = bodyOriginalPosition;
        ability._monsterBody = body;
        ability._monsterBodyPlayerId = bodyPlayerId;

        NetworkedPlayerInfo info = GameData.Instance?.GetPlayerById(bodyPlayerId);
        if (info != null)
            ability.Player.Player.setOutfit(info.DefaultOutfit);

        SetDeadBodyActive(body, active: false);
    }

    [CustomRPC]
    public static void RpcEndMonster(FrankensteinAbility ability, Vector2 dropPosition, bool decrementKill)
    {
        if (ability == null) return;
        if (!ability.IsMonster) return;

        DeadBody body = ability._monsterBody;
        if (body == null)
        {
            body = Object.FindObjectsOfType<DeadBody>().FirstOrDefault(x => x != null && x.ParentId == ability._monsterBodyPlayerId);
        }

        if (body != null)
        {
            SetDeadBodyActive(body, active: true);
            body.transform.position = new Vector3(dropPosition.x, dropPosition.y, dropPosition.y / 1000f);
        }

        ability.Player.Player.setOutfit(ability.Player.Data.DefaultOutfit);

        if (decrementKill)
            ability.RemainingKillsToWin = Mathf.Max(0, ability.RemainingKillsToWin - 1);

        ability._monsterBody = null;
        ability._monsterBodyPlayerId = byte.MaxValue;

        if (ability.Player.AmOwner)
        {
            ability.Player.Player.NetTransform.RpcSnapTo(ability._playerOriginalPosition);
            ability.Player.Player.MyPhysics.body.velocity = Vector2.zero;
        }

        ability._createButton?.ResetTimer();
    }
}

public sealed class FrankensteinCreateMonsterButtonAbility : CustomButtonBase
{
    private readonly FrankensteinAbility _ability;
    private readonly float _cooldown;

    private DeadBody _targetBody;

    public override float DefaultTimer => _cooldown;
    public override string buttonText => ModTranslation.GetString("FrankensteinCreateMonsterButtonName");
    public override Sprite Sprite => AssetManager.GetAsset<Sprite>("FrankensteinCreateMonsterButton.png");
    protected override KeyType keytype => KeyType.Ability1;

    public FrankensteinCreateMonsterButtonAbility(FrankensteinAbility ability, float cooldown)
    {
        _ability = ability;
        _cooldown = cooldown;
    }

    public override bool CheckIsAvailable()
    {
        if (!PlayerControl.LocalPlayer.CanMove) return false;
        if (MeetingHud.Instance != null || ExileController.Instance != null) return false;
        if (_ability.IsMonster) return false;

        _targetBody = _ability.FindNearestAvailableDeadBody();
        return _targetBody != null;
    }

    public override bool CheckHasButton()
    {
        return base.CheckHasButton() && !_ability.IsMonster;
    }

    public override void OnClick()
    {
        if (_targetBody == null) return;

        Vector2 playerPos = PlayerControl.LocalPlayer.GetTruePosition();
        Vector2 bodyPos = _targetBody.TruePosition;

        FrankensteinAbility.RpcStartMonster(_ability, _targetBody.ParentId, playerPos, bodyPos);
    }
}

