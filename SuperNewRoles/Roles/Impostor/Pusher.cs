
using System.Collections.Generic;
using AmongUs.GameOptions;
using Hazel;
using SuperNewRoles.CustomObject;
using SuperNewRoles.Roles.Role;
using SuperNewRoles.Roles.RoleBases;
using SuperNewRoles.Roles.RoleBases.Interfaces;
using UnityEngine;

namespace SuperNewRoles.Roles.Impostor.Pusher;

public class Pusher : RoleBase, IImpostor, ICustomButton, IRpcHandler, IFixedUpdaterMe
{
    public enum PushTarget
    {
        Right,
        Left,
        Down
    }

    public static new RoleInfo Roleinfo = new(
        typeof(Pusher),
        (p) => new Pusher(p),
        RoleId.Pusher,
        "Pusher",
        RoleClass.ImpostorRed,
        new(RoleId.Pusher, TeamTag.Impostor,
            RoleTag.SpecialKiller),
        TeamRoleType.Impostor,
        TeamType.Impostor
        );
    public static new OptionInfo Optioninfo =
        new(RoleId.Pusher, 206200, false,
            optionCreator: CreateOption);
    public static new IntroInfo Introinfo =
        new(RoleId.Pusher, introSound: RoleTypes.Impostor);

    public CustomButtonInfo PushButtonInfo { get; }

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

    public CustomButtonInfo[] CustomButtonInfos { get; }

    public static CustomOption IntervelOption;

    private static void CreateOption()
    {
        IntervelOption = CustomOption.Create(Optioninfo.OptionId++, Optioninfo.SupportSHR,
            Optioninfo.RoleOption.type, "PusherIntervalOption", 15f, 0f, 60f, 2.5f,
            Optioninfo.RoleOption);
    }
    public void RpcReader(MessageReader reader)
    {
        byte TargetId = reader.ReadByte();
        bool IsLadder = reader.ReadBoolean();
        int TargetPositionDetailIndex = reader.ReadInt32();

        Ladder targetLadder = IsLadder ? ModHelpers.LadderById((byte)TargetPositionDetailIndex) : null;
        if (IsLadder ? targetLadder == null : PusherPushPositions.Length <= TargetPositionDetailIndex)
            throw new System.Exception($"TargetPositionDetailIndex is out of range, IsLadder:{IsLadder}, Id:{TargetPositionDetailIndex}");

        Vector2 PushPosition = new();
        PushTarget PushTarget = PushTarget.Down;

        if (IsLadder)
            PushPosition = (targetLadder.transform.position + new Vector3(0, 0.15f));
        else
        {
            var PushPositionDetail = PusherPushPositions[TargetPositionDetailIndex];
            PushPosition = PushPositionDetail.PushPosition;
            PushTarget = PushPositionDetail.PushTarget;
        }

        PlayerControl target = ModHelpers.PlayerById(TargetId);
        var anim = PlayerAnimation.GetPlayerAnimation(Player.PlayerId);
        anim.RpcAnimation(RpcAnimationType.PushHand);

        target.NetTransform.SnapTo(PushPosition);
        target.transform.position = PushPosition;
        Player.NetTransform.SnapTo(PushPosition);
        Player.transform.position = PushPosition;
        DeadBody deadBody = null;
        Vector3 deadBodyPosition = new();
        if (IsLadder)
        {
            deadBody = Object.Instantiate(GameManager.Instance.DeadBodyPrefab);
            deadBody.enabled = true;
            deadBody.ParentId = target.PlayerId;
            deadBody.bodyRenderers.ForEach(target.SetPlayerMaterialColors);
            target.SetPlayerMaterialColors(deadBody.bloodSplatter);
            deadBody.transform.position = new(999, 999, 0);
            deadBodyPosition = targetLadder.Destination.transform.position + new Vector3(0.15f, 0.2f, 0);
            deadBodyPosition.z = target.transform.position.y / 1000f;
            //Vector3 position = target.transform.position + PlayerControl.LocalPlayer.KillAnimations.FirstOrDefault().BodyOffset;
            //position.z = position.y / 1000f;
        }

        PushedPlayerDeadbody pushedPlayerDeadbody = new GameObject("PushedPlayerDeadBody").AddComponent<PushedPlayerDeadbody>();
        pushedPlayerDeadbody.Init(target, PushTarget, deadBody, deadBodyPosition);

        if (PlayerControl.LocalPlayer.PlayerId == Player.PlayerId || PlayerControl.LocalPlayer.PlayerId == TargetId)
            SoundManager.Instance.PlaySound(ModHelpers.loadAudioClipFromResources("SuperNewRoles.Resources.Pusher.pusher_se.raw"), false, 1.5f);

        target.Exiled();
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
    public void PushButtonOnClick()
    {
        UpdateUntargetPlayers();
        PushButtonInfo.SetTarget();
        PlayerControl target = PushButtonInfo.CurrentTarget;
        var targetPositionDetail = GetTargetPositionDetail(target);
        Ladder targetLadder = null;
        if (target == null || targetPositionDetail == -1)
        {
            targetLadder = GetCanUseLadder(target);
            if (targetLadder == null)
            {
                PushButtonInfo.customButton.Timer = 0f;
                return;
            }
        }
        MessageWriter writer = RpcWriter;
        writer.Write(target.PlayerId);
        writer.Write(targetLadder != null);
        if (targetLadder != null)
            writer.Write((int)targetLadder.Id);
        else
            writer.Write(targetPositionDetail);
        SendRpc(writer);
        float cooltime = RoleHelpers.GetCoolTime(PlayerControl.LocalPlayer, target);
        PlayerControl.LocalPlayer.SetKillTimerUnchecked(cooltime, cooltime);
    }
    private float UpdateUntargetPlayersTimer;
    private void UpdateUntargetPlayers()
    {
        _untargetPlayers = new();
        float num = GameOptionsData.KillDistances[Mathf.Clamp(GameManager.Instance.LogicOptions.currentGameOptions.GetInt(Int32OptionNames.KillDistance), 0, 2)] + 1f;
        Vector2 truePosition = Player.GetTruePosition();
        foreach (PlayerControl @object in CachedPlayer.AllPlayers.AsSpan())
        {
            if (@object == null)
                continue;
            Vector2 vector = @object.GetTruePosition() - truePosition;
            float magnitude = vector.magnitude;
            if (magnitude > num ||
                PhysicsHelpers.AnyNonTriggersBetween(truePosition, vector.normalized, magnitude, Constants.ShipAndObjectsMask)
                )
                continue;
            if (GetTargetPositionDetail(@object) == -1 && !GetCanUseLadder(@object))
                _untargetPlayers.Add(@object);
        }
    }
    public void FixedUpdateMeDefaultAlive()
    {
        UpdateUntargetPlayersTimer -= Time.fixedDeltaTime;
        if (UpdateUntargetPlayersTimer <= 0)
        {
            UpdateUntargetPlayersTimer = 0.05f;
            UpdateUntargetPlayers();
        }
    }
    public List<PlayerControl> UntargetPlayers => _untargetPlayers;
    private List<PlayerControl> _untargetPlayers;
    public Pusher(PlayerControl p) : base(p, Roleinfo, Optioninfo, Introinfo)
    {
        PushButtonInfo = new(null, this, PushButtonOnClick,
            (isAlive) => isAlive, CustomButtonCouldType.CanMove | CustomButtonCouldType.SetTarget,
            () => PushButtonInfo.customButton.Timer = 0f,
            ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.PusherPushButton.png", 115f), () => IntervelOption.GetFloat(), new(-2f, 1, 0),
            "PusherButtonName", KeyCode.F, 49, SetTargetUntargetPlayer: () => UntargetPlayers, SetTargetCrewmateOnly: () => true);
        _untargetPlayers = new();
    }
}