
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
        new(RoleId.Pusher, 205900, false,
            optionCreator: CreateOption);
    public static new IntroInfo Introinfo =
        new(RoleId.Pusher, introSound: RoleTypes.Impostor);

    public CustomButtonInfo PushButtonInfo { get; }

    public static (Vector2 Position, float Radius)[] PusherPushPositions = new (Vector2 Position, float Radius)[]
    {
        //昇降機
        (new(7.7f, 8.75f), 2.6f),
        //ロミジュリ右
        (new(28.15f, -1.5f), 1f),
        //ロミジュリ左
        (new(26.85f, 0.5f), 1f),
        //展望右
        (new(8f, -16.9254f), 3.85f),
        //展望左
        (new(-13.84f, -16.3494f), 1.15f)
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
        PlayerControl target = ModHelpers.PlayerById(TargetId);
        var anim = PlayerAnimation.GetPlayerAnimation(Player.PlayerId);
        anim.RpcAnimation(RpcAnimationType.PushHand);
        target.Exiled();
    }
    public void PushButtonOnClick()
    {
        PlayerControl target = PushButtonInfo.CurrentTarget;
        if (target == null)
            return;
        MessageWriter writer = RpcWriter;
        writer.Write(target.PlayerId);
        SendRpc(writer);
    }
    private float UpdateUntargetPlayersTimer;
    public void FixedUpdateMeDefaultAlive()
    {
        UpdateUntargetPlayersTimer -= Time.fixedDeltaTime;
        if (UpdateUntargetPlayersTimer <= 0)
        {
            UpdateUntargetPlayersTimer = 0.1f;
            _untargetPlayers = new();
            float num = GameOptionsData.KillDistances[Mathf.Clamp(GameManager.Instance.LogicOptions.currentGameOptions.GetInt(Int32OptionNames.KillDistance), 0, 2)] + 1f;
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
                bool positionsuccess = false;
                foreach (var positiondata in PusherPushPositions)
                {
                    if (Vector2.Distance(positiondata.Position, @object.transform.position) > positiondata.Radius)
                        continue;
                    positionsuccess = true;
                    break;
                }
                if (!positionsuccess)
                    _untargetPlayers.Add(@object);

            }
        }
    }
    public List<PlayerControl> UntargetPlayers => _untargetPlayers;
    private List<PlayerControl> _untargetPlayers;
    public Pusher(PlayerControl p) : base(p, Roleinfo, Optioninfo, Introinfo)
    {
        PushButtonInfo = new(null, this, PushButtonOnClick,
            (isAlive) => isAlive, CustomButtonCouldType.CanMove | CustomButtonCouldType.SetTarget,
            () => PushButtonInfo.customButton.Timer = 0f,
            ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.PusherPushButton.png",115f), () => IntervelOption.GetFloat(), new(-2f, 1, 0),
            "PusherButtonName", KeyCode.F, 49, SetTargetUntargetPlayer: () => UntargetPlayers, SetTargetCrewmateOnly: () => true);
        _untargetPlayers = new();
    }
}