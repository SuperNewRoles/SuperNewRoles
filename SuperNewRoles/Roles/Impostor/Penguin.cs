using System.Linq;
using HarmonyLib;
using SuperNewRoles.Buttons;
using SuperNewRoles.Helpers;
using SuperNewRoles.Mode;
using SuperNewRoles.Roles.Crewmate;

namespace SuperNewRoles.Roles.Impostor;

[HarmonyPatch]
public static class Penguin
{
    [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.FixedUpdate))]
    public static class PlayerPhysicsSpeedPatch
    {
        public static void Postfix(PlayerPhysics __instance)
        {
            if (AmongUsClient.Instance.GameState != AmongUsClient.GameStates.Started) return;
            if (ModeHandler.IsMode(ModeId.Default))
            {
                if (RoleClass.Penguin.PenguinData.Any(x => x.Value != null && x.Value.PlayerId == __instance.myPlayer.PlayerId))
                {
                    __instance.body.velocity = new(0f, 0f);
                }
            }
        }
    }
    public static void FixedUpdate()
    {
        if (RoleClass.Penguin.PenguinData.Count <= 0) return;
        foreach (var data in RoleClass.Penguin.PenguinData.ToArray())
        {
            if (data.Key == null || data.Value == null
                || !data.Key.IsRole(RoleId.Penguin)
                || data.Key.IsDead()
                || data.Value.IsDead())
            {
                if (data.Key != null && data.Key.PlayerId == CachedPlayer.LocalPlayer.PlayerId)
                {
                    HudManagerStartPatch.PenguinButton.isEffectActive = false;
                    HudManagerStartPatch.PenguinButton.MaxTimer = CustomOptionHolder.PenguinCoolTime.GetFloat();
                    HudManagerStartPatch.PenguinButton.Timer = HudManagerStartPatch.PenguinButton.MaxTimer;
                    HudManagerStartPatch.PenguinButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
                }
                RoleClass.Penguin.PenguinData.Remove(data.Key);
                continue;
            }
            if (data.Value.IsRole(RoleId.WiseMan) && WiseMan.WiseManData.ContainsKey(data.Value.PlayerId) && WiseMan.WiseManData[data.Value.PlayerId] is not null)
                data.Key.transform.position = data.Value.transform.position;
            else
                data.Value.transform.position = data.Key.transform.position;
        }
    }
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.ReportDeadBody))]
    public static void Prefix(PlayerControl __instance)
    {
        if (!AmongUsClient.Instance.AmHost) return;
        if (CustomOptionHolder.PenguinMeetingKill.GetBool())
        {
            foreach (var data in RoleClass.Penguin.PenguinData.ToArray())
            {
                ModHelpers.CheckMurderAttemptAndKill(data.Key, data.Value);
            }
        }
        else
        {
            RPCHelper.StartRPC(CustomRPC.PenguinMeetingEnd).EndRPC();
            RPCProcedure.PenguinMeetingEnd();
        }
    }

}