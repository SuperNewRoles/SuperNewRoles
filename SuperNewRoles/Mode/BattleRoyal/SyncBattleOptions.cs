using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AmongUs.GameOptions;
using Hazel;
using SuperNewRoles.Helpers;
using SuperNewRoles.Mode.BattleRoyal.BattleRole;
using SuperNewRoles.Mode.SuperHostRoles;

namespace SuperNewRoles.Mode.BattleRoyal
{
    public static class SyncBattleOptions
    {
        public static void CustomSyncOptions()
        {
            var caller = new System.Diagnostics.StackFrame(1, false);
            var callerMethod = caller.GetMethod();
            string callerMethodName = callerMethod.Name;
            string callerClassName = callerMethod.DeclaringType.FullName;
            SuperNewRolesPlugin.Logger.LogInfo("[BR:SyncSettings] CustomSyncOptionsが" + callerClassName + "." + callerMethodName + "から呼び出されました。");
            foreach (PlayerControl p in CachedPlayer.AllPlayers)
            {
                if (!p.Data.Disconnected && !p.IsBot())
                {
                    CustomSyncOptions(p);
                }
            }
        }
        public static void CustomSyncOptions(this PlayerControl player)
        {
            if (!AmongUsClient.Instance.AmHost) return;
            if (!ModeHandler.IsMode(ModeId.BattleRoyal)) return;
            var role = player.GetRole();
            var optdata = SyncSetting.DefaultOption.DeepCopy();

            PlayerAbility ability = PlayerAbility.GetPlayerAbility(player);
            Logger.Info("SetAbilityCooltime:" + ability.KillCoolTime.ToString() + "," + player.Data.PlayerName);
            optdata.SetInt(Int32OptionNames.KillDistance, ability.KillDistance);
            optdata.SetFloat(FloatOptionNames.ImpostorLightMod, ability.Light);
            optdata.SetFloat(FloatOptionNames.KillCooldown, ability.KillCoolTime);
            if (!ability.CanMove) optdata.SetFloat(FloatOptionNames.PlayerSpeedMod, 0f);
            if (player.IsRole(RoleId.KingPoster) && KingPoster.KingPosters.FirstOrDefault(x => x.CurrentPlayer == player).IsAbilityUsingNow) optdata.SetInt(Int32OptionNames.KillDistance, NormalGameOptionsV09.KillDistances.Length - 1);

            if (player.PlayerId == PlayerControl.LocalPlayer.PlayerId)
            {
                GameOptionsManager.Instance.currentGameOptions = optdata;
                GameManager.Instance.LogicOptions.SetGameOptions(optdata);
            }
            else if (!player.IsBot() && player.GetClientId() != -1)
            {
                optdata.RpcSyncOption(player.GetClientId(), sendOption: SendOption.None);
            }
        }
    }
}