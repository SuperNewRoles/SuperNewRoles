using System.Linq;
using System.Collections.Generic;
using AmongUs.GameOptions;
using HarmonyLib;
using SuperNewRoles.Buttons;
using SuperNewRoles.Helpers;
using SuperNewRoles.Mode;
using SuperNewRoles.Roles.Crewmate;
using UnityEngine;

namespace SuperNewRoles.Roles.Impostor;

[HarmonyPatch]
public static class Penguin
{
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
                    PenguinTimer.Add(data.Key.PlayerId, CustomOptionHolder.PenguinDurationTime.GetFloat());
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
                    HudManagerStartPatch.PenguinButton.isEffectActive = false;
                    HudManagerStartPatch.PenguinButton.MaxTimer = ModeHandler.IsMode(ModeId.Default) ? CustomOptionHolder.PenguinCoolTime.GetFloat() : GameOptionsManager.Instance.CurrentGameOptions.GetFloat(FloatOptionNames.KillCooldown);
                    HudManagerStartPatch.PenguinButton.Timer = HudManagerStartPatch.PenguinButton.MaxTimer;
                    HudManagerStartPatch.PenguinButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
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
        if (CustomOptionHolder.PenguinMeetingKill.GetBool())
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
            RPCProcedure.PenguinMeetingEnd();
        }
    }

}