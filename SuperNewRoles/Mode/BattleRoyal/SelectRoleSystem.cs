using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using SuperNewRoles.Helpers;
using SuperNewRoles.Mode.BattleRoyal.BattleRole;
using SuperNewRoles.Mode.SuperHostRoles;
using SuperNewRoles.Patches;
using UnhollowerBaseLib;
using static MeetingHud;

namespace SuperNewRoles.Mode.BattleRoyal
{
    [HarmonyPatch(typeof(GameData),nameof(GameData.Serialize))]
    class GameDataSerializePatch
    {
        public static bool Is;
        public static bool Prefix(GameData __instance, ref bool __result)
        {
            Logger.Info(FastDestroyableSingleton<HudManager>.Instance.IsIntroDisplayed.ToString(),"ISST");
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
                foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                {
                    if (player.IsBot()) continue;
                    player.Data.IsDead = !team.IsTeam(player);
                }
                foreach (PlayerControl player in team.TeamMember)
                {
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
            Logger.Info("a");
            MeetingRoomManager.Instance.AssignSelf(PlayerControl.LocalPlayer, null);
            //if (AmongUsClient.Instance.AmHost)
            {
                FastDestroyableSingleton<HudManager>.Instance.OpenMeetingRoom(PlayerControl.LocalPlayer);
                PlayerControl.LocalPlayer.RpcStartMeeting(null);
            }
            Logger.Info("b");
            foreach (PlayerControl p in PlayerControl.AllPlayerControls)
            {
                p.RpcSetName(p.GetDefaultName());
            }
            Logger.Info("d");
            AddChatPatch.SendCommand(null, ModTranslation.GetString("BattleRoyalStartMeetingText"), BattleRoyalCommander);

            BattleTeam team = BattleTeam.GetTeam(PlayerControl.LocalPlayer);

            foreach (PlayerVoteArea area in Instance.playerStates)
            {
                bool IsDead = !team.IsTeam(ModHelpers.PlayerById(area.TargetPlayerId));
                area.SetDead(area.DidReport, IsDead);
                area.AmDead = IsDead;
            }
        }
        public static Dictionary<string, RoleId> RoleNames {
            get
            {
                if (_roleNames is null)
                {
                    _roleNames = new();
                    foreach (var role in Enum.GetValues(typeof(BattleRoles)))
                    {
                        IntroData intro = IntroData.GetIntroData((RoleId)(int)(BattleRoles)role, IsImpostorReturn: true);
                        _roleNames.Add(((BattleRoles)role).ToString(), (RoleId)(int)(BattleRoles)role);
                        _roleNames.Add(ModTranslation.GetString(intro.NameKey + "Name"), (RoleId)(int)(BattleRoles)role);
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
                if (Main.IsRoleSetted) return;
                new LateTask(() =>
                {
                    TeamOnlyChat();
                    Logger.Info("c");
                    BattleTeam team = BattleTeam.GetTeam(PlayerControl.LocalPlayer);
                    Logger.Info("d");
                    foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                    {
                        Logger.Info("e");
                        if (player.IsBot()) continue;
                        if (player is null) continue;
                        Logger.Info("f");
                        player.Data.IsDead = !team.IsTeam(player);
                        Logger.Info("g");
                    }
                }, 1.5f);

            }
        }
        public static void OnWrapUp()
        {
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
            }
            RPCHelper.RpcSyncGameData();
            ChangeName.UpdateName();
        }
        private static Dictionary<string, RoleId> _roleNames;
        public static string BattleRoyalCommander => $"<size=200%><color=#745030>{ModTranslation.GetString("BattleRoyalModeName")}</color></size>";
        public static bool OnAddChat(PlayerControl source, string chat)
        {
            var Commands = chat.Split(" ");
            Logger.Info("a");
            if (Commands[0].Equals("/SetRole", StringComparison.OrdinalIgnoreCase))
            {
                if (Commands.Length <= 1) return false;
                Logger.Info("b");
                var data = RoleNames.FirstOrDefault(x => x.Key.Equals(Commands[1], StringComparison.OrdinalIgnoreCase));
                Logger.Info("c");
                //nullチェック
                if (data.Equals(default(KeyValuePair<string, RoleId>)))
                {
                    AddChatPatch.SendCommand(source, ModTranslation.GetString("BattleRoyalRoleNoneText"), BattleRoyalCommander);
                }
                else
                {
                    Logger.Info("e");
                    source.SetRoleRPC(data.Value);
                    Logger.Info("f");
                    string text = string.Format(ModTranslation.GetString("BattleRoyalSetRoleText"), source.GetDefaultName(), ModTranslation.GetString(IntroData.GetIntroData(data.Value, IsImpostorReturn: true).NameKey + "Name"));
                    Logger.Info("g");
                    foreach (PlayerControl teammember in BattleTeam.GetTeam(source).TeamMember)
                    {
                        Logger.Info("h");
                        AddChatPatch.SendCommand(teammember, text, BattleRoyalCommander);
                        Logger.Info("i");
                    }
                    Logger.Info("j");
                    Main.RoleSettedPlayers.Add(source);
                    Logger.Info("k");
                    bool IsEnd = true;
                    Logger.Info("l");
                    foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                    {
                        if (p.IsBot()) continue;
                        if (!Main.RoleSettedPlayers.IsCheckListPlayerControl(p))
                        {
                            IsEnd = false;
                            break;
                        }
                    }
                    Logger.Info("m");
                    if (IsEnd)
                    {
                        foreach(PlayerControl p in PlayerControl.AllPlayerControls)
                        {
                            p.Data.IsDead = false;
                        }
                        RPCHelper.RpcSyncGameData();
                        foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                        {
                            p.MyPhysics.RpcExitVent(0);
                        }
                        Instance.RpcClose();
                    }
                    Logger.Info("n");
                }
                Logger.Info("d");
                return false;
            }
            return true;
        }
    }
}
