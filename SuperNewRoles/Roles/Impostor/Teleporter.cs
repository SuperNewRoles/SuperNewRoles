using System;
using System.Collections.Generic;
using Hazel;
using SuperNewRoles.Buttons;
using SuperNewRoles.Roles.RoleBases;
using UnityEngine;

namespace SuperNewRoles.Roles;

public class Teleporter : RoleBase<Teleporter>
{
    public static Color color = RoleClass.ImpostorRed;

    public Teleporter()
    {
        RoleId = roleId = RoleId.Teleporter;
        //以下いるもののみ変更
        HasTask = false;
        HasFakeTask = false;
        IsKiller = true;
        OptionId = 66;
        OptionType = CustomOptionType.Impostor;
        CoolTimeOptionOn = true;
    }

    public static Sprite GetButtonSprite() => ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.SpeedUpButton.png", 115f);

    public override void OnMeetingStart() { }
    public override void OnWrapUp() { }
    public override void FixedUpdate() { }
    public override void MeFixedUpdateAlive() { }
    public override void MeFixedUpdateDead() { }
    public override void OnKill(PlayerControl target) { }
    public override void OnDeath(PlayerControl killer = null) { }
    public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }
    public override void EndUseAbility() { }
    public override void ResetRole() { }
    public override void PostInit() { }
    public override void UseAbility() { base.UseAbility(); AbilityLimit--; if (AbilityLimit <= 0) EndUseAbility(); }
    public override bool CanUseAbility() { return base.CanUseAbility() && AbilityLimit <= 0; }

    public static CustomButton TeleporterButton;

    //ボタンが必要な場合のみ(Buttonsの方に記述する必要あり)
    public static void MakeButtons(HudManager hm) {

        TeleporterButton = new(
            () =>
            {
                if (!PlayerControl.LocalPlayer.CanMove) return;
                Teleporter.TeleportStart();
                Teleporter.ResetCooldown();
            },
            (bool isAlive, RoleId role) => { return isAlive && (role == RoleId.Teleporter || role == RoleId.TeleportingJackal || role == RoleId.NiceTeleporter || (role == RoleId.Levelinger && RoleClass.Levelinger.IsPower(RoleClass.Levelinger.LevelPowerTypes.Teleporter))); },
            () =>
            {
                return PlayerControl.LocalPlayer.CanMove;
            },
            EndMeeting,
            GetButtonSprite(),
            new Vector3(-2f, 1, 0),
            hm,
            hm.AbilityButton,
            KeyCode.F,
            49,
            () => { return false; }
        )
        {
            buttonText = ModTranslation.GetString("TeleporterTeleportButton"),
            showButtonText = true
        };
    }
    public static void SetButtonCooldowns() { }

    public override void SetupMyOptions() { }

    public static void Clear()
    {
        players = new();
    }

    public static void ResetCooldown()
    {
        TeleporterButton.MaxTimer = CoolTime;
        TeleporterButton.Timer = TeleporterButton.MaxTimer;
    }
    public static void TeleportStart()
    {
        List<PlayerControl> aliveplayers = new();
        foreach (PlayerControl p in CachedPlayer.AllPlayers)
        {
            if (p.IsAlive() && p.CanMove)
            {
                aliveplayers.Add(p);
            }
        }
        var player = ModHelpers.GetRandom(aliveplayers);
        RPCProcedure.TeleporterTP(player.PlayerId);

        MessageWriter Writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.TeleporterTP, SendOption.Reliable, -1);
        Writer.Write(player.PlayerId);
        AmongUsClient.Instance.FinishRpcImmediately(Writer);
    }
    public static bool IsTeleporter(PlayerControl Player)
    {
        return Player.IsRole(RoleId.Teleporter);
    }
    public static float CoolTime {
        get
        {
            switch (PlayerControl.LocalPlayer.GetRole())
            {
                case RoleId.Levelinger:
                case RoleId.Teleporter:
                    return CoolTimeS;
                case RoleId.NiceTeleporter:
                    return RoleClass.NiceTeleporter.CoolTime;
                case RoleId.TeleportingJackal:
                    return RoleClass.TeleportingJackal.CoolTime;
                default:
                    return 0f;
            }
        }
    }
    public static void EndMeeting()
    {
        ResetCooldown();
    }
}