using System;
using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using SuperNewRoles.CustomObject;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Events;
using SuperNewRoles.Modules;
using SuperNewRoles.Modules.Events.Bases;
using SuperNewRoles.Roles.Ability;
using SuperNewRoles.Roles.Ability.CustomButton;
using SuperNewRoles.Roles.Madmates;
using UnityEngine;

namespace SuperNewRoles.Roles.Impostor;

class Pusher : RoleBase<Pusher>
{
    public override RoleId Role { get; } = RoleId.Pusher;
    public override Color32 RoleColor { get; } = Palette.ImpostorRed;
    public override List<Func<AbilityBase>> Abilities { get; } = [
        () => new PusherAbility(PusherCooldown, PusherRevengeRole)
    ];

    public override QuoteMod QuoteMod { get; } = QuoteMod.SuperNewRoles;
    public override RoleTypes IntroSoundType { get; } = RoleTypes.Impostor;
    public override short IntroNum { get; } = 1;

    public override AssignedTeamType AssignedTeam { get; } = AssignedTeamType.Impostor;
    public override WinnerTeamType WinnerTeam { get; } = WinnerTeamType.Impostor;
    public override TeamTag TeamTag { get; } = TeamTag.Impostor;
    public override RoleTag[] RoleTags { get; } = [RoleTag.SpecialKiller];
    public override RoleOptionMenuType OptionTeam { get; } = RoleOptionMenuType.Impostor;
    public override MapNames[] AvailableMaps { get; } = [MapNames.Skeld, MapNames.Polus, MapNames.Airship, MapNames.Fungle];
    [CustomOptionFloat("PusherCooldown", 0f, 180f, 2.5f, 15f, translationName: "CoolTime")]
    public static float PusherCooldown;
    [CustomOptionBool("PusherRevengeRole", false)]
    public static bool PusherRevengeRole;
}
public class PusherAbility : TargetCustomButtonBase
{
    public enum PushTarget
    {
        Right,
        Left,
        Down,
        Up,
    }

    //上から順番に処理されます
    public static Dictionary<byte, (Vector2 Position, float Radius, Vector2 PushPosition, PushTarget PushTarget)[]> PusherPushPositions = new()
    {
        {(byte)MapNames.Airship, [
            //昇降機左
            (new(5.8f, 8.95f), 1f, new(5.8132f, 8.8f), PushTarget.Right),
            //昇降機右
            (new(10f, 9f), 1f, new(9.7064f, 8.8f), PushTarget.Left),
            //ロミジュリ左
            (new(26.85f, 0.5f), 1f, new(26.6653f, 0.45f), PushTarget.Right),
            //ロミジュリ右
            (new(28.15f, -1.5f), 1f, new(28.1792f, -1.65f), PushTarget.Left),
            //展望左
            (new(-13.84f, -16.3494f), 1.15f, new(-13.89f, -16.5f), PushTarget.Down),

            //展望右

            //展望右の左上
            (new(7.3f, -15.6f), 2.2f, new(7.05f, -14.672f), PushTarget.Down),
            //展望右の右下
            (new(10.3f, -16.7f), 1f, new(10.27f, -16.3995f), PushTarget.Down),
            //展望右の右上
            (new(10.2f, -15.1f), 1f, new(10.59f, -15.1f), PushTarget.Right)
        ]},
        {
            (byte)MapNames.Skeld, [
                // シールド
                (new(11.93f, -12.14f), 1.75f, new(10.3f, -12.24f), PushTarget.Right),
                (new(7.7f, -10.8f), 1f, new(8.7f, -11.21f), PushTarget.Left),
                (new(7.8f, -13.4f), 0.75f, new(8.3f, -13.05f), PushTarget.Left),
                // 保管庫
                (new(0.2f, -17.1f), 0.55f, new(0.2f, -17.1f), PushTarget.Down),
                // ウェポン
                (new(10.9f, 0.7f), 1.25f, new(9.98f, 0.95f), PushTarget.Right),
                (new(7.5f, 2.78f), 0.9f, new(8.2f, 2.4f), PushTarget.Left),
                (new(8.08f, -0.17f), 0.85f, new(8.2f, 0.78f), PushTarget.Down),
            ]
        },
        {
            (byte)MapNames.Polus, [
                // リアクター
                (new(24.2f, -3), 0.8f, new(24.2f, -3), PushTarget.Left),
                (new(4.79f, -4.07f), 0.8f, new(4.79f, -4.07f), PushTarget.Right),
                // ラボ
                (new(33.97f, -4.85f), 1.5f, new(34.5f, -5.18f), PushTarget.Up),
                // 溶岩
                (new(32.33f, -14.32f), 1.4f, new(31.5f, -13.9f), PushTarget.Right),
                (new(32.11f, -15.75f), 0.75f, new(32.68f, -15.728f), PushTarget.Right),
                (new(31.55f, -16.9f), 0.75f, new(31.17f, -16.95f), PushTarget.Down),
            ]
        },
        {
            (byte)MapNames.Fungle, [
                // キッチン橋
                (new(-23.81f, -7.9f), 1.2f, new(-23.26f, -7.12f), PushTarget.Left),
                // ジップライン崖
                (new(16.5f, 13.49f), 1.3f, new(16.8f, 13.6f), PushTarget.Left),
                // カメラ崖
                (new(6.8f, -0.14f), 0.75f, new(6.8f, -0.14f), PushTarget.Left),
                (new(5.85f, 1.19f), 0.75f, new(5.85f, 1.19f), PushTarget.Left),
            ]
        }
    };

    private float coolDown;
    private float effectTimer;
    private bool _revengeRole;
    private List<PlayerControl> _untargetPlayers = new();
    private float updateUntargetPlayersTimer;
    private EventListener fixedUpdateEvent;
    private EventListener<WrapUpEventData> wrapupEvent;

    public override Sprite Sprite => AssetManager.GetAsset<Sprite>("PusherPushButton.png");
    public override string buttonText => ModTranslation.GetString("PusherButtonName");
    protected override KeyType keytype => KeyType.Ability1;
    public override float DefaultTimer => coolDown;
    public override Color32 OutlineColor => Pusher.Instance.RoleColor;

    public override bool OnlyCrewmates => true;
    public override IEnumerable<PlayerControl> UntargetablePlayers => _untargetPlayers;

    public PusherAbility(float coolDown, bool revengeRole)
    {
        this.coolDown = coolDown;
        _untargetPlayers = new();
        _revengeRole = revengeRole;
    }

    public override void OnClick()
    {
        UpdateUntargetPlayers();
        if (Target == null) return;

        var targetPositionDetail = GetTargetPositionDetail(Target);
        Ladder targetLadder = null;
        if (targetPositionDetail == -1)
        {
            targetLadder = GetCanUseLadder(Target);
            if (targetLadder == null)
            {
                ResetTimer();
                return;
            }
        }

        AssetManager.PlaySoundFromBundle("pusher_se.wav", false);

        RpcPushPlayer(ExPlayerControl.LocalPlayer, Target, targetLadder != null, targetLadder != null ? (int)targetLadder.Id : targetPositionDetail);
        float cooltime = coolDown;
        ResetTimer();
    }

    [CustomRPC]
    public void RpcPushPlayer(ExPlayerControl source, ExPlayerControl target, bool isLadder, int targetPositionDetailIndex)
    {
        Ladder targetLadder = isLadder ? ShipStatus.Instance.Ladders.FirstOrDefault(x => x.Id == (byte)targetPositionDetailIndex) : null;
        if (isLadder ? targetLadder == null : PusherPushPositions[GameManager.Instance.LogicOptions.MapId].Length <= targetPositionDetailIndex)
            throw new Exception($"TargetPositionDetailIndex is out of range, IsLadder:{isLadder}, Id:{targetPositionDetailIndex}");

        Vector2 pushPosition = new();
        PushTarget pushTarget = PushTarget.Down;

        if (isLadder)
            pushPosition = (targetLadder.transform.position + new Vector3(0, 0.15f));
        else
        {
            var pushPositionDetail = PusherPushPositions[GameManager.Instance.LogicOptions.MapId][targetPositionDetailIndex];
            pushPosition = pushPositionDetail.PushPosition;
            pushTarget = pushPositionDetail.PushTarget;
        }

        if (source == null || target == null)
            return;

        PushAnimation(source);

        target.NetTransform.SnapTo(pushPosition);
        target.transform.position = pushPosition;
        source.NetTransform.SnapTo(pushPosition);
        source.transform.position = pushPosition;
        DeadBody deadBody = null;
        Vector3 deadBodyPosition = new();
        if (isLadder)
        {
            deadBody = UnityEngine.Object.Instantiate(GameManager.Instance.DeadBodyPrefab);
            deadBody.enabled = true;
            deadBody.ParentId = target.PlayerId;
            // SetPlayerMaterialColorsのループ処理
            for (int i = 0; i < deadBody.bodyRenderers.Count; i++)
            {
                SpriteRenderer b = deadBody.bodyRenderers[i];
                target.Player.SetPlayerMaterialColors(b);
            }
            target.Player.SetPlayerMaterialColors(deadBody.bloodSplatter);
            deadBody.transform.position = new(999, 999, 0);
            deadBodyPosition = targetLadder.Destination.transform.position + new Vector3(0.15f, 0.2f, 0);
            deadBodyPosition.z = target.transform.position.y / 1000f;
        }

        PushedPlayerDeadbody pushedPlayerDeadbody = new GameObject("PushedPlayerDeadBody").AddComponent<PushedPlayerDeadbody>();
        pushedPlayerDeadbody.Init(source, target, pushTarget, deadBody, deadBodyPosition);

        target.CustomDeath(CustomDeathType.Push, source: source);
        if (_revengeRole)
            target.GetAbilities<RevengeExileAbility>().ForEach(x => x.RandomExile());
    }

    private static void PushAnimation(PlayerControl player)
    {
        CustomPlayerAnimationSimple.Spawn(player, new CustomPlayerAnimationSimpleOption(
            Sprites: CustomPlayerAnimationSimple.GetSprites("pushanim_00{0}.png", 0, 14, 1),
            PlayerFlipX: true,
            IsLoop: false,
            frameRate: 20,
            localPosition: new Vector3(-0.6f, 0, -10),
            localScale: Vector3.one * 0.7f,
            Adaptive: true
        ));
    }
    private Ladder GetCanUseLadder(PlayerControl player)
    {
        if (player == null)
            return null;
        Ladder ladder = ShipStatus.Instance.Ladders.FirstOrDefault(x => x.IsTop && Vector2.Distance(x.transform.position, player.transform.position) <= x.UsableDistance);
        return ladder;
    }

    private int GetTargetPositionDetail(PlayerControl player)
    {
        if (player == null)
            return -1;
        int i = 0;
        foreach (var positiondata in PusherPushPositions[GameManager.Instance.LogicOptions.MapId])
        {
            if (Vector2.Distance(positiondata.Position, player.transform.position) > positiondata.Radius)
            {
                i++;
                continue;
            }
            return i;
        }
        return -1;
    }

    private void UpdateUntargetPlayers()
    {
        _untargetPlayers = new();
        float num = GameManager.Instance.LogicOptions.GetKillDistance() + 1f;
        Vector2 truePosition = Player.GetTruePosition();
        foreach (PlayerControl @object in PlayerControl.AllPlayerControls)
        {
            if (@object == null)
                continue;
            Vector2 vector = @object.GetTruePosition() - truePosition;
            float magnitude = vector.magnitude;
            if (magnitude > num ||
                PhysicsHelpers.AnyNonTriggersBetween(truePosition, vector.normalized, magnitude, Constants.ShipAndObjectsMask)
                )
                continue;
            if (GetTargetPositionDetail(@object) == -1 && GetCanUseLadder(@object) == null)
                _untargetPlayers.Add(@object);
        }
    }

    public override void Attach(PlayerControl player, ulong abilityId, AbilityParentBase parent)
    {
        base.Attach(player, abilityId, parent);
        SyncKillCoolTimeAbility.CreateAndAttach(this);
        fixedUpdateEvent = FixedUpdateEvent.Instance.AddListener(OnFixedUpdate);
        wrapupEvent = WrapUpEvent.Instance.AddListener(x => OnWrapup());
    }

    public override void Detach()
    {
        base.Detach();
        fixedUpdateEvent?.RemoveListener();
        wrapupEvent?.RemoveListener();
    }

    private void OnWrapup()
    {
        new LateTask(() => Timer = 0.001f, 0.1f);
    }

    private void OnFixedUpdate()
    {
        updateUntargetPlayersTimer -= Time.fixedDeltaTime;
        if (updateUntargetPlayersTimer <= 0)
        {
            updateUntargetPlayersTimer = 0.05f;
            UpdateUntargetPlayers();
        }
    }

    public override bool CheckIsAvailable()
    {
        return Player.IsAlive() && Target != null && Target.IsAlive();
    }
}