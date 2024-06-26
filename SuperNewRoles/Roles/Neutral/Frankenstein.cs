using System.Collections.Generic;
using AmongUs.GameOptions;
using Hazel;
using SuperNewRoles.Buttons;
using SuperNewRoles.Helpers;
using SuperNewRoles.MapCustoms;
using SuperNewRoles.Patches;
using UnityEngine;
using IEnumerator = System.Collections.IEnumerator;

namespace SuperNewRoles.Roles.Neutral;

public class Frankenstein
{
    private const int OptionId = 303700;
    public static CustomRoleOption FrankensteinOption;
    public static CustomOption FrankensteinPlayerCount;
    public static CustomOption FrankensteinCreateCoolTime;
    public static CustomOption FrankensteinWinKillCount;
    public static CustomOption FrankensteinMonsterImpostorLight;
    public static CustomOption FrankensteinMonsterCanVent;
    public static void SetupCustomOptions()
    {
        FrankensteinOption = CustomOption.SetupCustomRoleOption(OptionId, false, RoleId.Frankenstein);
        FrankensteinPlayerCount = CustomOption.Create(OptionId + 1, false, CustomOptionType.Neutral, "SettingPlayerCountName", CustomOptionHolder.CrewPlayers[0], CustomOptionHolder.CrewPlayers[1], CustomOptionHolder.CrewPlayers[2], CustomOptionHolder.CrewPlayers[3], FrankensteinOption);
        FrankensteinCreateCoolTime = CustomOption.Create(OptionId + 2, false, CustomOptionType.Neutral, "FrankensteinCreateCoolTimeSetting", 30f, 2.5f, 60f, 2.5f, FrankensteinOption);
        FrankensteinWinKillCount = CustomOption.Create(OptionId + 3, false, CustomOptionType.Neutral, "FrankensteinWinKillCountSetting", 3f, 1f, 10f, 1f, FrankensteinOption);
        FrankensteinMonsterImpostorLight = CustomOption.Create(OptionId + 4, false, CustomOptionType.Neutral, "FrankensteinMonsterImpostorLightSetting", true, FrankensteinOption);
        FrankensteinMonsterCanVent = CustomOption.Create(OptionId + 5, false, CustomOptionType.Neutral, "FrankensteinMonsterCanVentOetting", true, FrankensteinOption);
    }

    public static List<PlayerControl> FrankensteinPlayer;
    public static Color32 color = new(122, 169, 146, byte.MaxValue);
    public static PlayerData<DeadBody> MonsterPlayer;
    public static Vector2 OriginalPosition;
    public static PlayerData<int> KillCount;
    public static void ClearAndReload()
    {
        FrankensteinPlayer = new();
        MonsterPlayer = new();
        OriginalPosition = new();
        KillCount = new(defaultvalue: FrankensteinWinKillCount.GetInt());
    }

    public static CustomButton FrankensteinCreateMonsterButton;
    public static CustomButton FrankensteinKillButton;
    public static void SetupCustomButtons(HudManager __instance)
    {
        FrankensteinCreateMonsterButton = new(
            () =>
            {
                NetworkedPlayerInfo player = null;
                Vector2 pos = PlayerControl.LocalPlayer.GetTruePosition();
                foreach (Collider2D collider2D in Physics2D.OverlapCircleAll(pos, PlayerControl.LocalPlayer.MaxReportDistance, Constants.PlayersOnlyMask))
                {
                    if (collider2D.tag != "DeadBody") continue;

                    DeadBody component = collider2D.GetComponent<DeadBody>();
                    if (!component || component.Reported) continue;

                    Vector2 dead = component.TruePosition;
                    if (Vector2.Distance(pos, dead) <= PlayerControl.LocalPlayer.MaxReportDistance && !PhysicsHelpers.AnythingBetween(pos, dead, Constants.ShipAndObjectsMask, false))
                    {
                        player = GameData.Instance.GetPlayerById(component.ParentId);
                        break;
                    }
                }
                if (player == null) return;
                MoveDeadBody(player.PlayerId, new(9999f, 9999f));
                OriginalPosition = PlayerControl.LocalPlayer.transform.position;
                SetMonsterPlayer(target: player.PlayerId);
            },
            (bool isAlive, RoleId role) => { return isAlive && role == RoleId.Frankenstein && !IsMonster(PlayerControl.LocalPlayer); },
            () => { return __instance.ReportButton.graphic.color == Palette.EnabledColor && PlayerControl.LocalPlayer.CanMove; },
            () =>
            {
                FrankensteinCreateMonsterButton.MaxTimer = FrankensteinCreateCoolTime.GetFloat();
                FrankensteinCreateMonsterButton.Timer = FrankensteinCreateMonsterButton.MaxTimer;
                OriginalPosition = PlayerControl.LocalPlayer.transform.position;
                SetMonsterPlayer();
            },
            ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.FrankensteinCreateMonsterButton.png", 115f),
            new Vector3(-2f, 1, 0),
            __instance,
            __instance.AbilityButton,
            KeyCode.F,
            49,
            () => { return false; }
        )
        {
            buttonText = ModTranslation.GetString("FrankensteinCreateMonsterButtonName"),
            showButtonText = true
        };

        FrankensteinKillButton = new(
            () =>
            {
                PlayerControl target = HudManagerStartPatch.SetTarget();
                ModHelpers.CheckMurderAttemptAndKill(PlayerControl.LocalPlayer, target);
                MoveDeadBody(MonsterPlayer.Local.ParentId, PlayerControl.LocalPlayer.GetTruePosition());
                SetMonsterPlayer(kill: true);
                FrankensteinCreateMonsterButton.MaxTimer = FrankensteinCreateCoolTime.GetFloat();
                FrankensteinCreateMonsterButton.Timer = FrankensteinCreateMonsterButton.MaxTimer;
            },
            (bool isAlive, RoleId role) => { return isAlive && role == RoleId.Frankenstein && IsMonster(PlayerControl.LocalPlayer); },
            () =>
            {
                if (!PlayerControl.LocalPlayer.CanMove) return false;
                PlayerControl target = HudManagerStartPatch.SetTarget();
                if (!target) return false;
                PlayerControlFixedUpdatePatch.SetPlayerOutline(target, color);
                return true;
            },
            () =>
            {
                FrankensteinKillButton.MaxTimer = 0f;
                FrankensteinKillButton.Timer = FrankensteinKillButton.MaxTimer;
            },
            __instance.KillButton.graphic.sprite,
            new Vector3(0, 1, 0),
            __instance,
            __instance.KillButton,
            KeyCode.Q,
            49,
            () => { return false; }
        )
        {
            buttonText = FastDestroyableSingleton<HudManager>.Instance.KillButton.buttonLabelText.text,
            showButtonText = true
        };
    }

    public static void FixedUpdate()
    {
        PlayerControl player = PlayerControl.LocalPlayer;
        if (player.IsRole(RoleId.Frankenstein) && player.IsAlive())
        {
            if (!IsMonster(player)) Vulture.FixedUpdate.Postfix();
            else
            {
                foreach (var arrow in RoleClass.Vulture.DeadPlayerArrows)
                {
                    if (arrow.Value?.arrow != null)
                        Object.Destroy(arrow.Value.arrow);
                    RoleClass.Vulture.DeadPlayerArrows.Remove(arrow.Key);
                }
            }
        }
    }

    public static bool IsMonster(PlayerControl player)
    {
        if (!player) return false;
        byte id = player.PlayerId;
        if (!MonsterPlayer.TryGetValue(id, out DeadBody body)) return false;
        return body;
    }
    /// <summary>
    /// 他の人がモンスターをキルした時に処理する
    /// </summary>
    /// <param name="target"></param>
    public static void OnMurderMonster(PlayerControl target)
    {
        MoveDeadBody(MonsterPlayer[target.PlayerId]?.ParentId ?? 255, target.GetTruePosition());
        SetMonsterPlayer(target.PlayerId);
        FrankensteinCreateMonsterButton.MaxTimer = FrankensteinCreateCoolTime.GetFloat();
        FrankensteinCreateMonsterButton.Timer = FrankensteinCreateMonsterButton.MaxTimer;
    }
    public static void SetMonsterPlayer(byte? player = null, byte target = byte.MaxValue, bool kill = false)
    {
        MessageWriter writer = RPCHelper.StartRPC(CustomRPC.SetFrankensteinMonster);
        writer.Write(player.GetValueOrDefault(PlayerControl.LocalPlayer.PlayerId));
        writer.Write(target);
        writer.Write(kill);
        writer.EndRPC();
        RPCProcedure.SetFrankensteinMonster(player.GetValueOrDefault(PlayerControl.LocalPlayer.PlayerId), target, kill);
    }

    /// <summary>
    /// 死体の位置を移動させる
    /// </summary>
    /// <param name="id">対象の死体のPlayerId</param>
    /// <param name="pos">移動先</param>
    public static void MoveDeadBody(byte id, Vector2 pos)
    {
        MessageWriter writer = RPCHelper.StartRPC(CustomRPC.MoveDeadBody);
        writer.Write(id);
        writer.Write(pos.x);
        writer.Write(pos.y);
        writer.EndRPC();
        RPCProcedure.MoveDeadBody(id, pos.x, pos.y);
    }

    public static IEnumerator KillPerform(PlayerControl source, PlayerControl target)
    {
        if (source.AmOwner) SoundManager.Instance.PlaySound(source.KillSfx, false, 0.8f, null);
        FollowerCamera cam = Camera.main.GetComponent<FollowerCamera>();
        KillAnimation.SetMovement(source, false);
        if (source.AmOwner)
        {
            cam.Locked = true;
            ConsoleJoystick.SetMode_Task();
            PlayerControl.LocalPlayer.MyPhysics.inputHandler.enabled = true;
        }
        yield return source.MyPhysics.Animations.CoPlayCustomAnimation(source.KillAnimations.Random().BlurAnim);
        source.NetTransform.SnapTo(target.transform.position);
        source.MyPhysics.Animations.PlayIdleAnimation();
        KillAnimation.SetMovement(source, true);
        if (source.AmOwner) cam.Locked = false;
        yield break;
    }
}