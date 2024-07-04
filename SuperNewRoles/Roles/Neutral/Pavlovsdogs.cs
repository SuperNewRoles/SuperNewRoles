using System.Collections.Generic;
using AmongUs.GameOptions;
using SuperNewRoles.Roles.Role;
using SuperNewRoles.Roles.RoleBases;
using SuperNewRoles.Roles.RoleBases.Interfaces;
using UnityEngine;
using static SuperNewRoles.Roles.RoleClass;

namespace SuperNewRoles.Roles.Neutral;

public class PavlovsDogs : RoleBase, INeutral, IVentAvailable, IImpostorVision, IKiller, ICustomButton, IFixedUpdaterMe, INameHandler
{
    public static Color32 PavlovsColor = new(244, 169, 106, byte.MaxValue);

    public static new RoleInfo Roleinfo = new(
        typeof(PavlovsDogs),
        (p) => new PavlovsDogs(p),
        RoleId.Pavlovsdogs,
        "Pavlovsdogs",
        PavlovsColor,
        new(RoleId.Pavlovsdogs, TeamTag.Neutral),
        TeamRoleType.Neutral,
        TeamType.Neutral
        );
    public static new IntroInfo Introinfo =
        new(RoleId.Pavlovsowner, introSound: RoleTypes.Phantom);

    public bool OwnerDead => CurrentOwner?.Player?.Data.IsDead ?? true;

    public bool CanUseVent => PavlovsOwner.CanVent.GetBool();
    public bool IsImpostorVision => PavlovsOwner.IsImpostorView.GetBool();

    public PavlovsOwner CurrentOwner { get; private set; }

    public CustomButtonInfo PavlovsKillButton;
    public CustomButtonInfo[] CustomButtonInfos { get; }

    public PavlovsDogs(PlayerControl p) : base(p, Roleinfo, null, Introinfo)
    {
        PavlovsKillButton = new(null,this,KillOnClick,(isAlive) => isAlive,
            CustomButtonCouldType.CanMove | CustomButtonCouldType.SetTarget,
            null, FastDestroyableSingleton<HudManager>.Instance.KillButton.graphic.sprite,
            () => OwnerDead ?
            PavlovsOwner.RunAwayKillCoolTime.GetFloat() :
            PavlovsOwner.KillCoolTime.GetFloat(),
            new(), FastDestroyableSingleton<HudManager>.Instance.KillButton.buttonLabelText.text,
            KeyCode.Q, 8, baseButton: FastDestroyableSingleton<HudManager>.Instance.KillButton,
            SetTargetFunc: () => SetTarget(), hasSecondButtonInfo: true);
    }

    public float RunAwayTime = PavlovsOwner.RunAwayDeathTime.GetFloat();

    public void UpdateOwner(PavlovsOwner owner)
        => CurrentOwner = owner;

    public void OnHandleName()
        => SeePavlovsTeam();

    public static void SeePavlovsTeam()
    {
        foreach (PlayerControl player in PlayerControl.AllPlayerControls)
        {
            if (!player.IsPavlovsTeam())
                continue;
            SetNamesClass.SetPlayerRoleInfo(player);
            SetNamesClass.SetPlayerNameColors(player);
        }
    }

    public void KillOnClick()
    {
        PlayerControl target = PavlovsKillButton.CurrentTarget;
        ModHelpers.CheckMurderAttemptAndKill(PlayerControl.LocalPlayer, target);
        if (target.IsRole(RoleId.Fox) && RoleClass.Fox.Killer.Contains(PlayerControl.LocalPlayer.PlayerId)) return;
        RunAwayTime = PavlovsOwner.RunAwayDeathTime.GetFloat();
    }

    public static PlayerControl SetTarget(bool IsPavlovsTeamTarget = false)
    {
        PlayerControl result = null;
        float num = GameOptionsData.KillDistances[Mathf.Clamp(GameManager.Instance.LogicOptions.currentGameOptions.GetInt(Int32OptionNames.KillDistance), 0, 2)];
        if (!MapUtilities.CachedShipStatus) return result;
        PlayerControl targetingPlayer = PlayerControl.LocalPlayer;
        if (targetingPlayer.Data.IsDead || targetingPlayer.inVent) return result;

        Vector2 truePosition = targetingPlayer.GetTruePosition();
        Il2CppSystem.Collections.Generic.List<NetworkedPlayerInfo> allPlayers = GameData.Instance.AllPlayers;
        for (int i = 0; i < allPlayers.Count; i++)
        {
            NetworkedPlayerInfo playerInfo = allPlayers[i];
            if (!playerInfo.Disconnected && playerInfo.PlayerId != targetingPlayer.PlayerId && !playerInfo.IsDead && ((IsPavlovsTeamTarget && playerInfo.Object.IsPavlovsTeam()) || (!IsPavlovsTeamTarget && !playerInfo.Object.IsPavlovsTeam())))
            {
                PlayerControl @object = playerInfo.Object;
                if (@object && !@object.inVent)
                {
                    Vector2 vector = @object.GetTruePosition() - truePosition;
                    float magnitude = vector.magnitude;
                    if (magnitude <= num && !PhysicsHelpers.AnyNonTriggersBetween(truePosition, vector.normalized, magnitude, Constants.ShipAndObjectsMask))
                    {
                        result = @object;
                        num = magnitude;
                    }
                }
            }
        }
        return result == null ? null : result.IsDead() ? null : result;
    }

    public void FixedUpdateMeDefaultAlive()
    {
        if (!OwnerDead || Player.IsDead() || RoleClass.IsMeeting)
        {
            PavlovsKillButton.SecondButtonInfoText.text = string.Empty;
            return;
        }
        RunAwayTime -= Time.fixedDeltaTime;
        PavlovsKillButton.SecondButtonInfoText.text =
            RunAwayTime > 0
            ? ModTranslation.GetString("SerialKillerSuicideText", (int)RunAwayTime + 1)
            : string.Empty;

        if (RunAwayTime <= 0)
            Player.RpcMurderPlayer(Player, true);
    }
}