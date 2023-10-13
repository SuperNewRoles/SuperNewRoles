using System.Linq;
using System.Collections.Generic;
using AmongUs.GameOptions;
using HarmonyLib;
using Hazel;
using SuperNewRoles.Buttons;
using SuperNewRoles.Helpers;
using SuperNewRoles.Mode;
using SuperNewRoles.Roles.Crewmate;
using UnityEngine;

namespace SuperNewRoles.Roles.Impostor;

[HarmonyPatch]
public static class Penguin
{
    public static CustomRoleOption PenguinOption;
    public static CustomOption PenguinPlayerCount;
    public static CustomOption PenguinCoolTime;
    public static CustomOption PenguinDurationTime;
    public static CustomOption PenguinCanDefaultKill;
    public static CustomOption PenguinMeetingKill;
    public static void SetupCustomOptions()
    {
        PenguinOption = CustomOption.SetupCustomRoleOption(200600, true, RoleId.Penguin);
        PenguinPlayerCount = CustomOption.Create(200601, true, CustomOptionType.Impostor, "SettingPlayerCountName", CustomOptionHolder.ImpostorPlayers[0], CustomOptionHolder.ImpostorPlayers[1], CustomOptionHolder.ImpostorPlayers[2], CustomOptionHolder.ImpostorPlayers[3], PenguinOption);
        PenguinCoolTime = CustomOption.Create(200602, false, CustomOptionType.Impostor, "NiceScientistCooldownSetting", 30f, 2.5f, 60f, 2.5f, PenguinOption, format: "unitSeconds");
        PenguinDurationTime = CustomOption.Create(200603, true, CustomOptionType.Impostor, "NiceScientistDurationSetting", 10f, 2.5f, 30f, 2.5f, PenguinOption, format: "unitSeconds");
        PenguinCanDefaultKill = CustomOption.Create(200604, false, CustomOptionType.Impostor, "PenguinCanDefaultKill", false, PenguinOption);
        PenguinMeetingKill = CustomOption.Create(200605, true, CustomOptionType.Impostor, "PenguinMeetingKill", true, PenguinOption);
    }

    public static List<PlayerControl> PenguinPlayer;
    public static Color32 color = RoleClass.ImpostorRed;
    public static Dictionary<PlayerControl, PlayerControl> PenguinData;
    public static Dictionary<byte, float> PenguinTimer;
    public static PlayerControl currentTarget => PenguinData.ContainsKey(CachedPlayer.LocalPlayer) ? PenguinData[CachedPlayer.LocalPlayer] : null;
    private static Sprite _buttonSprite;
    public static Sprite GetButtonSprite() => _buttonSprite;
    public static void ClearAndReload()
    {
        PenguinPlayer = new();
        PenguinData = new();
        PenguinTimer = new();
        bool Is = ModHelpers.IsSucsessChance(4);
        _buttonSprite = ModHelpers.LoadSpriteFromResources($"SuperNewRoles.Resources.PenguinButton_{(Is ? 1 : 2)}.png", Is ? 87.5f : 110f);
    }
    public static CustomButton PenguinButton;
    public static void SetupCustomButtons(HudManager __instance)
    {
        PenguinButton = new(
            () =>
            {
                PlayerControl target = HudManagerStartPatch.SetTarget(null, true);
                MessageWriter writer = RPCHelper.StartRPC(CustomRPC.PenguinHikizuri);
                writer.Write(CachedPlayer.LocalPlayer.PlayerId);
                writer.Write(target.PlayerId);
                writer.EndRPC();
                PenguinHikizuri(CachedPlayer.LocalPlayer.PlayerId, target.PlayerId);
            },
            (bool isAlive, RoleId role) => { return isAlive && role == RoleId.Penguin; },
            () =>
            {
                if (PenguinButton.isEffectActive) CustomButton.FillUp(PenguinButton);
                return PlayerControl.LocalPlayer.CanMove && HudManagerStartPatch.SetTarget(null, true);
            },
            () =>
            {
                PenguinButton.MaxTimer = ModeHandler.IsMode(ModeId.Default) ? Penguin.PenguinCoolTime.GetFloat() : RoleClass.IsFirstMeetingEnd ? GameOptionsManager.Instance.CurrentGameOptions.GetFloat(FloatOptionNames.KillCooldown) : 10;
                PenguinButton.Timer = PenguinButton.MaxTimer;
                PenguinButton.effectCancellable = false;
                PenguinButton.EffectDuration = Penguin.PenguinDurationTime.GetFloat();
                PenguinButton.HasEffect = true;
                PenguinButton.Sprite = Penguin.GetButtonSprite();
            },
            Penguin.GetButtonSprite(),
            new Vector3(-2f, 1, 0),
            __instance,
            __instance.AbilityButton,
            KeyCode.F,
            49,
            () => { return false; },
            true,
            5f,
            () =>
            {
                if (ModeHandler.IsMode(ModeId.Default))
                    PlayerControl.LocalPlayer.UncheckedMurderPlayer(Penguin.currentTarget);
            }
        )
        {
            buttonText = ModTranslation.GetString("PenguinButtonName"),
            showButtonText = true
        };
    }

    [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.FixedUpdate))]
    public static class PlayerPhysicsSpeedPatch
    {
        public static void Postfix(PlayerPhysics __instance)
        {
            if (AmongUsClient.Instance.GameState != AmongUsClient.GameStates.Started) return;
            if (ModeHandler.IsMode(ModeId.Default))
            {
                if (PenguinData.Any(x => x.Value != null && x.Value.PlayerId == __instance.myPlayer.PlayerId) ||
                    Rocket.RoleData.RocketData.Any(x => x.Value.Any(y => y.PlayerId == __instance.myPlayer.PlayerId)))
                {
                    __instance.body.velocity = new(0f, 0f);
                }
            }
        }
    }
    public static void FixedUpdate()
    {
        if (PenguinData.Count <= 0) return;
        foreach (var data in PenguinData.ToArray())
        {
            if (ModeHandler.IsMode(ModeId.SuperHostRoles) && data.Key != null)
            {
                if (!PenguinTimer.ContainsKey(data.Key.PlayerId))
                    PenguinTimer.Add(data.Key.PlayerId, PenguinDurationTime.GetFloat());
                PenguinTimer[data.Key.PlayerId] -= Time.fixedDeltaTime;
                if (PenguinTimer[data.Key.PlayerId] <= 0 && data.Value != null && data.Value.IsAlive())
                    data.Key.RpcMurderPlayer(data.Value);
            }
            if (data.Key == null || data.Value == null
                || !data.Key.IsRole(RoleId.Penguin)
                || data.Key.IsDead()
                || data.Value.IsDead()
                || (ModeHandler.IsMode(ModeId.SuperHostRoles) && PenguinTimer[data.Key.PlayerId] <= 0))
            {

                if (data.Key != null && data.Key.PlayerId == CachedPlayer.LocalPlayer.PlayerId)
                {
                    PenguinButton.isEffectActive = false;
                    PenguinButton.MaxTimer = ModeHandler.IsMode(ModeId.Default) ? PenguinCoolTime.GetFloat() : GameOptionsManager.Instance.CurrentGameOptions.GetFloat(FloatOptionNames.KillCooldown);
                    PenguinButton.Timer = PenguinButton.MaxTimer;
                    PenguinButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
                }
                PenguinData.Remove(data.Key);
                continue;
            }
            if (ModeHandler.IsMode(ModeId.Default) || !AmongUsClient.Instance.AmHost)
            {
                if (data.Value.IsRole(RoleId.WiseMan) && WiseMan.WiseManData.ContainsKey(data.Value.PlayerId) && WiseMan.WiseManData[data.Value.PlayerId] is not null)
                    data.Key.transform.position = data.Value.transform.position;
                else
                    data.Value.transform.position = data.Key.transform.position;
            }
            else
            {
                data.Value.RpcSnapTo(data.Key.transform.position);
            }
        }
    }
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.ReportDeadBody))]
    public static void Prefix(PlayerControl __instance)
    {
        if (!AmongUsClient.Instance.AmHost) return;
        if (PenguinMeetingKill.GetBool())
        {
            foreach (var data in PenguinData.ToArray())
            {
                if (ModeHandler.IsMode(ModeId.Default))
                    ModHelpers.CheckMurderAttemptAndKill(data.Key, data.Value);
                else
                    data.Key.RpcMurderPlayer(data.Value);
            }
        }
        else
        {
            RPCHelper.StartRPC(CustomRPC.PenguinMeetingEnd).EndRPC();
            PenguinMeetingEnd();
        }
    }

    public static void PenguinHikizuri(byte sourceId, byte targetId)
    {
        PlayerControl source = ModHelpers.PlayerById(sourceId);
        PlayerControl target = ModHelpers.PlayerById(targetId);
        if (source == null || target == null) return;
        Penguin.PenguinData.Add(source, target);
    }
    public static void PenguinMeetingEnd()
    {
        Penguin.PenguinData.Clear();
        if (PlayerControl.LocalPlayer.GetRole() == RoleId.Penguin)
            Penguin.PenguinButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
    }
}