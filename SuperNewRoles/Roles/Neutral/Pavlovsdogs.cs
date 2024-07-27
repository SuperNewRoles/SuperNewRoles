using System.Collections.Generic;
using System.Text;
using AmongUs.GameOptions;
using Hazel;
using SuperNewRoles.Helpers;
using SuperNewRoles.Mode;
using SuperNewRoles.Mode.SuperHostRoles;
using SuperNewRoles.Roles.Role;
using SuperNewRoles.Roles.RoleBases;
using SuperNewRoles.Roles.RoleBases.Interfaces;
using UnityEngine;
using static SuperNewRoles.Roles.RoleClass;

namespace SuperNewRoles.Roles.Neutral;

public class PavlovsDogs : RoleBase, INeutral, IVentAvailable, IImpostorVision, IKiller, ICustomButton, IFixedUpdaterMe, INameHandler, ISupportSHR, IFixedUpdaterAll, ICheckMurderHandler, IDeathHandler, ISHRAntiBlackout
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

    public bool HasKillButtonClient => false;

    public static new IntroInfo Introinfo =
        new(RoleId.Pavlovsdogs, introSound: RoleTypes.Phantom);

    public bool OwnerDead => CurrentOwner?.Player?.Data.IsDead ?? true;

    public bool CanUseVent => PavlovsOwner.CanVent.GetBool();
    public bool IsImpostorVision => PavlovsOwner.IsImpostorView.GetBool();

    public PavlovsOwner CurrentOwner { get; private set; }

    public CustomButtonInfo PavlovsKillButton;
    public CustomButtonInfo[] CustomButtonInfos { get; }

    public RoleTypes RealRole => RoleTypes.Crewmate;
    public RoleTypes DesyncRole => OwnerDead ? RoleTypes.Shapeshifter : RoleTypes.Impostor;

    public bool? IsImpostorLight => IsImpostorVision;

    public PavlovsDogs(PlayerControl p) : base(p, Roleinfo, null, Introinfo)
    {
        UpdatedToOwnerDead = false;
        PavlovsKillButton = new(null,this,KillOnClick,(isAlive) => isAlive,
            CustomButtonCouldType.CanMove | CustomButtonCouldType.SetTarget,
            null, FastDestroyableSingleton<HudManager>.Instance.KillButton.graphic.sprite,
            () => OwnerDead ?
            PavlovsOwner.RunAwayKillCoolTime.GetFloat() :
            PavlovsOwner.KillCoolTime.GetFloat(),
            new(), FastDestroyableSingleton<HudManager>.Instance.KillButton.buttonLabelText.text,
            KeyCode.Q, 8, baseButton: FastDestroyableSingleton<HudManager>.Instance.KillButton,
            SetTargetFunc: () => SetTarget(), hasSecondButtonInfo: true);
        CustomButtonInfos = [PavlovsKillButton];
        SyncSetting.CustomSyncSettings(Player);
    }

    public float RunAwayTime = PavlovsOwner.RunAwayDeathTime.GetFloat();

    private bool UpdatedToOwnerDead;

    public void UpdateOwner(PavlovsOwner owner)
        => CurrentOwner = owner;

    public void OnHandleName()
        => SeePavlovsTeam();

    public static void SeePavlovsTeam()
    {
        foreach (PlayerControl player in CachedPlayer.AllPlayers.AsSpan())
        {
            if (!player.IsPavlovsTeam())
                continue;
            SetNamesClass.SetPlayerRoleInfo(player);
            SetNamesClass.SetPlayerNameColors(player);
        }
    }

    public bool OnCheckMurderPlayerAmKiller(PlayerControl target)
    {
        if (target.IsPavlovsTeam())
            return false;
        RunAwayTime = PavlovsOwner.RunAwayDeathTime.GetFloat();
        if (OwnerDead)
            Player.RpcResetAbilityCooldown();
        return true;
    }

    public void KillOnClick()
    {
        PlayerControl target = PavlovsKillButton.CurrentTarget;
        if (ModeHandler.IsMode(ModeId.Default))
            ModHelpers.CheckMurderAttemptAndKill(PlayerControl.LocalPlayer, target);
        else
            Player.RpcMurderPlayer(target, true);
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

    public void FixedUpdateMeSHRAlive()
    {
        if (!AmongUsClient.Instance.AmHost)
            FixedUpdateMeDefaultAlive();
    }
    public void FixedUpdateMeDefaultAlive()
    {
        if (!OwnerDead || Player.IsDead() || IsMeeting)
        {
            PavlovsKillButton.SecondButtonInfoText.text = string.Empty;
            return;
        }
        RunAwayTime -= Time.fixedDeltaTime;
        PavlovsKillButton.SecondButtonInfoText.text =
            RunAwayTime > 0
            ? ModTranslation.GetString("SerialKillerSuicideText", (int)RunAwayTime + 1)
            : string.Empty;

        if (RunAwayTime <= 0 && ModeHandler.IsMode(ModeId.Default))
            Player.RpcMurderPlayer(Player, true);
    }
    public void FixedUpdateAllSHR()
    {
        if (!AmongUsClient.Instance.AmHost)
            return;
        if (!UpdatedToOwnerDead && OwnerDead)
        {
            UpdatedToOwnerDead = true;
            CustomRpcSender sender = CustomRpcSender.Create("PavlovsDogsUpdateOwner", sendOption: SendOption.Reliable);
            SyncSetting.CustomSyncSettings(Player, sender);
            if (!Player.IsMod())
                sender.RpcSetRole(Player, RoleTypes.Shapeshifter, true);
            sender.SendMessage();
            Player.RpcResetAbilityCooldown();
        }
        else if (UpdatedToOwnerDead && !OwnerDead)
        {
            UpdatedToOwnerDead = false;
            if (!Player.IsMod())
                Player.RpcSetRole(RoleTypes.Impostor, true);
        }
        if (!OwnerDead || Player.IsDead() || IsMeeting)
            return;
        RunAwayTime -= Time.fixedDeltaTime;
        PavlovsKillButton.SecondButtonInfoText.text =
            RunAwayTime > 0
            ? ModTranslation.GetString("SerialKillerSuicideText", (int)RunAwayTime + 1)
            : string.Empty;

        if (RunAwayTime <= 0)
            Player.RpcMurderPlayer(Player, true);
    }
    public void BuildName(StringBuilder Suffix, StringBuilder RoleNameText, PlayerData<string> ChangePlayers)
    {
        if (Player == null)
            return;
        foreach (PlayerControl player in CachedPlayer.AllPlayers.AsSpan())
        {
            if (player.PlayerId == Player.PlayerId || !player.IsPavlovsTeam())
                continue;
            ChangePlayers[player.PlayerId] = ModHelpers.Cs(PavlovsColor, ChangeName.GetNowName(ChangePlayers, player));
        }
    }
    public void BuildSetting(IGameOptions gameOptions)
    {
        Logger.Info("PavlovsDogsRunAwayTime: "+ PavlovsOwner.RunAwayDeathTime.GetFloat());
        gameOptions.SetFloat(FloatOptionNames.ShapeshifterCooldown, PavlovsOwner.RunAwayDeathTime.GetFloat());
        gameOptions.SetFloat(FloatOptionNames.ShapeshifterDuration, PavlovsOwner.RunAwayDeathTime.GetFloat());
        gameOptions.SetFloat(FloatOptionNames.KillCooldown,
            OwnerDead
            ? PavlovsOwner.RunAwayKillCoolTime.GetFloat()
            : PavlovsOwner.KillCoolTime.GetFloat()
        );
    }

    public void FixedUpdateAllDefault()
    {
    }

    public void StartAntiBlackout()
    {
        if (CurrentOwner != null && !Player.IsMod())
            CurrentOwner.Player.RpcSetRoleDesync(CurrentOwner.Player.IsDead() ? RoleTypes.CrewmateGhost : RoleTypes.Crewmate, Player);
    }

    public void EndAntiBlackout()
    {
        if (CurrentOwner != null && !Player.IsMod() && CurrentOwner.Player.IsAlive())
            CurrentOwner.Player.RpcSetRoleDesync(RoleTypes.Impostor, Player);
    }
}