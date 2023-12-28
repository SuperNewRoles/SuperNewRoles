using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using SuperNewRoles.Helpers;
using SuperNewRoles.Mode.BattleRoyal.BattleRole;
using SuperNewRoles.Mode.SuperHostRoles;
using SuperNewRoles.Patches;
using SuperNewRoles.Roles.RoleBases;
using static MeetingHud;

namespace SuperNewRoles.Mode.BattleRoyal
{
    [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.OnDestroy))]
    class IntroCutsceneOnDestroyPatch
    {
        public static void Postfix(IntroCutscene __instance)
        {
            if (ModeHandler.IsMode(ModeId.BattleRoyal))
            {
                SelectRoleSystem.OnEndIntro(); Logger.Info("StartOnEndIntro");
            }
        }
    }
    [HarmonyPatch(typeof(GameData), nameof(GameData.Serialize))]
    class GameDataSerializePatch
    {
        public static bool Is;
        public static bool Prefix(GameData __instance, ref bool __result)
        {
            if (AmongUsClient.Instance is null || AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started || !ModeHandler.IsMode(ModeId.BattleRoyal) || Is || !Main.IsIntroEnded)
            {
                Is = false;
                __result = true;
                return true;
            }
            __instance.ClearDirtyBits();
            __result = false;
            return false;
        }
    }
    public static class SelectRoleSystem
    {
        public static void OnShowIntro()
        {
        }
        public static void TeamOnlyChat()
        {
            foreach (BattleTeam team in BattleTeam.BattleTeams)
            {
                Logger.Info("ーー終了ーー");
                foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                {
                    if (player.IsBot()) continue;
                    if (player is null) continue;
                    player.Data.IsDead = !team.IsTeam(player);
                    Logger.Info($"{player.GetDefaultName()} : {player.Data.IsDead} : {!team.IsTeam(player)}");
                }
                Logger.Info("ーー開始ーー");
                foreach (PlayerControl player in team.TeamMember)
                {
                    if (player is null) continue;
                    RPCHelper.RpcSyncGameData(player.GetClientId());
                }
            }
        }
        public static void OnEndIntro()
        {
            Main.IsIntroEnded = true;
            if (!AmongUsClient.Instance.AmHost) return;
            foreach (PlayerControl p in PlayerControl.AllPlayerControls)
            {
                p.Data.IsDead = false;
            }
            RPCHelper.RpcSyncGameData();
            MeetingRoomManager.Instance.AssignSelf(PlayerControl.LocalPlayer, null);
            //if (AmongUsClient.Instance.AmHost)
            {
                FastDestroyableSingleton<HudManager>.Instance.OpenMeetingRoom(PlayerControl.LocalPlayer);
                PlayerControl.LocalPlayer.RpcStartMeeting(null);
            }
            foreach (PlayerControl p in PlayerControl.AllPlayerControls)
            {
                p.RpcSetName(p.GetDefaultName());
            }
            AddChatPatch.SendCommand(null, ModTranslation.GetString("BattleRoyalStartMeetingText"), BattleRoyalCommander);

            BattleTeam team = BattleTeam.GetTeam(PlayerControl.LocalPlayer);

            foreach (PlayerVoteArea area in Instance.playerStates)
            {
                bool IsDead = !team.IsTeam(ModHelpers.PlayerById(area.TargetPlayerId));
                area.SetDead(area.DidReport, IsDead);
                area.AmDead = IsDead;
            }
        }
        public static Dictionary<string, RoleId> RoleNames
        {
            get
            {
                if (_roleNames is null)
                {
                    _roleNames = new();
                    foreach (var role in Enum.GetValues(typeof(BattleRoles)))
                    {
                        _roleNames.Add(((BattleRoles)role).ToString(), (RoleId)(int)(BattleRoles)role);
                        _roleNames.Add(CustomRoles.GetRoleName((RoleId)(int)(BattleRoles)role, IsImpostorReturn:true), (RoleId)(int)(BattleRoles)role);
                    }
                }
                return _roleNames;
            }
        }
        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.CoIntro))]
        public class MeetingHudCoIntro
        {
            public static void Postfix()
            {
                if (!AmongUsClient.Instance.AmHost) return;
                if (!ModeHandler.IsMode(ModeId.BattleRoyal)) return;
                if (Main.IsRoleSetted) return;
                new LateTask(() =>
                {
                    TeamOnlyChat();
                    BattleTeam team = BattleTeam.GetTeam(PlayerControl.LocalPlayer);
                    foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                    {
                        if (player.IsBot()) continue;
                        if (player is null) continue;
                        player.Data.IsDead = !team.IsTeam(player);
                    }
                    RPCHelper.RpcSyncMeetingHud();
                }, 1.5f);

            }
        }
        public static void OnWrapUp()
        {
            if (!AmongUsClient.Instance.AmHost) return;
            if (!ModeHandler.IsMode(ModeId.BattleRoyal)) return;
            if (Main.IsRoleSetted) return;
            Main.IsRoleSetted = true;
            SyncBattleOptions.CustomSyncOptions();
            foreach (BattleTeam team in BattleTeam.BattleTeams)
            {
                bool CanRevive = false;
                foreach (PlayerControl player in team.TeamMember)
                {
                    if (Reviver.IsReviver(player))
                        CanRevive = true;
                }
                if (CanRevive)
                {
                    foreach (PlayerControl player in team.TeamMember)
                    {
                        PlayerAbility.GetPlayerAbility(player).CanRevive = true;
                    }
                }
            }
            foreach (PlayerControl p in PlayerControl.AllPlayerControls)
            {
                p.Data.IsDead = false;
                if (!p.IsBot()) continue;
                p.RpcSnapTo(new(999, 999));
            }
            RPCHelper.RpcSyncGameData();
            SyncBattleOptions.CustomSyncOptions();
            ChangeName.UpdateName();
        }
        private static Dictionary<string, RoleId> _roleNames;
        public static string BattleRoyalCommander => $"<size=200%><color=#745030>{ModTranslation.GetString("BattleRoyalModeName")}</color></size>";
        public static bool OnAddChat(PlayerControl source, string chat)
        {
            var Commandsa = chat.Split(" ");
            var Commandsb = new List<string>();
            foreach (string com in Commandsa)
            {
                Commandsb.AddRange(com.Split("　"));
            }
            var Commands = Commandsb.ToArray();
            if (Commands[0].Equals("/SetRole", StringComparison.OrdinalIgnoreCase))
            {
                if (Commands.Length <= 1)
                {
                    AddChatPatch.SendCommand(source, ModTranslation.GetString("BattleRoyalRoleNoneText"), BattleRoyalCommander);
                    return false;
                }
                var data = RoleNames.FirstOrDefault(x => x.Key.Equals(Commands[1], StringComparison.OrdinalIgnoreCase));
                //nullチェック
                if (data.Equals(default(KeyValuePair<string, RoleId>)))
                {
                    AddChatPatch.SendCommand(source, ModTranslation.GetString("BattleRoyalRoleNoneText"), BattleRoyalCommander);
                }
                else
                {
                    source.SetRoleRPC(data.Value);
                    string text = string.Format(ModTranslation.GetString("BattleRoyalSetRoleText"), source.GetDefaultName(), CustomRoles.GetRoleName(data.Value, IsImpostorReturn: true));
                    foreach (PlayerControl teammember in BattleTeam.GetTeam(source).TeamMember)
                    {
                        if (teammember == null) continue;
                        AddChatPatch.SendCommand(teammember, text, BattleRoyalCommander);
                    }
                    Main.RoleSettedPlayers.Add(source);
                    bool IsEnd = true;
                    foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                    {
                        if (p.IsBot()) continue;
                        if (!Main.RoleSettedPlayers.IsCheckListPlayerControl(p))
                        {
                            IsEnd = false;
                            break;
                        }
                    }
                    if (IsEnd)
                    {
                        OnEndSetRole();
                    }
                }
                return false;
            }
            return true;
        }
        public static void OnEndSetRole()
        {
            foreach (PlayerControl p in PlayerControl.AllPlayerControls)
            {
                p.Data.IsDead = false;
            }
            RPCHelper.RpcSyncGameData();
            foreach (PlayerControl p in PlayerControl.AllPlayerControls)
            {
                p.MyPhysics.RpcExitVentUnchecked(0);
            }
            Instance.RpcClose();
        }
    }
}