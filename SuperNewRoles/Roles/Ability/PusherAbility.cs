using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability.CustomButton;
using SuperNewRoles.Events.PCEvents;
using SuperNewRoles.Modules.Events.Bases;
using SuperNewRoles.Events;
using AmongUs.GameOptions;
using SuperNewRoles.Roles.Impostor;
using SuperNewRoles.Ability;
using SuperNewRoles.SuperTrophies;
using Hazel;
using SuperNewRoles.CustomObject;
using BepInEx.Unity.IL2CPP.Utils.Collections;

namespace SuperNewRoles.Roles.Ability;

public class PusherAbility : TargetCustomButtonBase
{
    public enum PushTarget
    {
        Right,
        Left,
        Down
    }

    //上から順番に処理されます
    public static (Vector2 Position, float Radius, Vector2 PushPosition, PushTarget PushTarget)[] PusherPushPositions = new (Vector2 Position, float Radius, Vector2 PushPosition, PushTarget PushTarget)[]
    {
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
    };

    private float coolDown;
    private float effectTimer;
    private List<PlayerControl> _untargetPlayers = new();
    private float updateUntargetPlayersTimer;
    private EventListener fixedUpdateEvent;

    public override Sprite Sprite => AssetManager.GetAsset<Sprite>("PusherPushButton.png");
    public override string buttonText => ModTranslation.GetString("PusherButtonName");
    protected override KeyCode? hotkey => KeyCode.F;
    public override float DefaultTimer => coolDown;
    public override Color32 OutlineColor => Pusher.Instance.RoleColor;

    public override bool OnlyCrewmates => true;
    public override IEnumerable<PlayerControl> UntargetablePlayers => _untargetPlayers;

    public PusherAbility(float coolDown)
    {
        this.coolDown = coolDown;
        _untargetPlayers = new();
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
    public static void RpcPushPlayer(ExPlayerControl source, ExPlayerControl target, bool isLadder, int targetPositionDetailIndex)
    {
        Ladder targetLadder = isLadder ? ShipStatus.Instance.Ladders.FirstOrDefault(x => x.Id == (byte)targetPositionDetailIndex) : null;
        if (isLadder ? targetLadder == null : PusherPushPositions.Length <= targetPositionDetailIndex)
            throw new System.Exception($"TargetPositionDetailIndex is out of range, IsLadder:{isLadder}, Id:{targetPositionDetailIndex}");

        Vector2 pushPosition = new();
        PushTarget pushTarget = PushTarget.Down;

        if (isLadder)
            pushPosition = (targetLadder.transform.position + new Vector3(0, 0.15f));
        else
        {
            var pushPositionDetail = PusherPushPositions[targetPositionDetailIndex];
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
    }

    private static void PushAnimation(PlayerControl player)
    {
        CustomPlayerAnimationSimple.Spawn(player, new CustomPlayerAnimationSimpleOption(
            Sprites: CustomPlayerAnimationSimple.GetSprites("pushanim_00{0}.png", 0, 14, 1),
            PlayerFlipX: true,
            IsLoop: false,
            frameRate: 20,
            localPosition: new Vector3(0.6f, 0, -10),
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
        foreach (var positiondata in PusherPushPositions)
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
        fixedUpdateEvent = FixedUpdateEvent.Instance.AddListener(OnFixedUpdate);
    }

    public override void Detach()
    {
        base.Detach();
        if (fixedUpdateEvent != null)
            FixedUpdateEvent.Instance.RemoveListener(fixedUpdateEvent);
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