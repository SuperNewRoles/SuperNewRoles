using AmongUs.GameOptions;
using HarmonyLib;
using SuperNewRoles.Helpers;
using SuperNewRoles.Mode;
using SuperNewRoles.Mode.SuperHostRoles;
using UnityEngine;

namespace SuperNewRoles.Patches;

public class Child
{
    public const float AllSkeld = 2;
    [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.OnDestroy))]
    public static class IntroCutsceneCoBeginPatch
    {
        public static void Postfix()
        {
            if (!IsRun()) return;
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            {
                if (!player) continue;
                Vector3 PlayerScale = player.transform.localScale;
                PlayerScale.x /= AllSkeld;
                PlayerScale.y /= AllSkeld;
                player.transform.localScale = PlayerScale;
            }
            IGameOptions options = OptionData.DeepCopy();
            options.SetFloat(FloatOptionNames.PlayerSpeedMod, options.GetFloat(FloatOptionNames.PlayerSpeedMod) / AllSkeld);
            options.SetFloat(FloatOptionNames.CrewLightMod, options.GetFloat(FloatOptionNames.CrewLightMod) / (AllSkeld * 2));
            options.SetFloat(FloatOptionNames.ImpostorLightMod, options.GetFloat(FloatOptionNames.ImpostorLightMod) / (AllSkeld * 2));
            GameManager.Instance.LogicOptions.SetGameOptions(options);
        }
    }
    public static IGameOptions OptionData;
    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.CoStartGame))]
    public class AmongUsClientCoStartGamePatch
    {
        public static void Postfix()
        {
            if (!IsRun()) return;
            OptionData = GameOptionsManager.Instance.CurrentGameOptions.DeepCopy();
        }
    }
    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameEnd))]
    public static class AmongUsClientOnGameEndPatch
    {
        public static void Postfix()
        {
            if (!IsRun()) return;
            GameManager.Instance.LogicOptions.SetGameOptions(SyncSetting.OptionData.DeepCopy());
            RPCHelper.RpcSyncOption(GameManager.Instance.LogicOptions.currentGameOptions);
        }
    }
    public static bool IsRun()
    {
        if (AmongUsClient.Instance.NetworkMode == NetworkModes.FreePlay) return false;
        if (!ModeHandler.IsMode(ModeId.Default)) return false;
        return CustomOptionHolder.enableChildMap.GetBool();
    }
}
