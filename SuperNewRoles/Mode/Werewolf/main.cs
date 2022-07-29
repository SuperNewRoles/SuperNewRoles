using System.Collections.Generic;
using SuperNewRoles.Helpers;
using SuperNewRoles.Intro;
using SuperNewRoles.Mode.SuperHostRoles;
using SuperNewRoles.Roles;

namespace SuperNewRoles.Mode.Werewolf
{
    class Main
    {
        public static bool IsDiscussion;
        public static bool IsFirst;
        public static int AbilityTime;
        public static int DiscussionTime;
        public static Dictionary<int, int> SoothRoles;
        public static List<int> HunterKillPlayers;
        public static List<int> WolfKillPlayers;
        public static List<PlayerControl> HunterPlayers = new();
        public static PlayerControl HunterExilePlayer;
        public static int Time;
        public static bool IsAbility { get { return !IsDiscussion; } set { IsDiscussion = !value; } }
        public static void ClearAndReload()
        {
            PlayerControl.GameOptions.KillCooldown = -1;
            PlayerControl.GameOptions.DiscussionTime = 0;
            CachedPlayer.LocalPlayer.PlayerControl.RpcSyncSettings(PlayerControl.GameOptions);
            foreach (PlayerControl p in CachedPlayer.AllPlayers)
            {
                p.GetDefaultName();
            }
            IsDiscussion = true;
            IsFirst = true;
            DiscussionTime = 30;
            AbilityTime = 10;
            SoothRoles = new();
            HunterKillPlayers = new();
            WolfKillPlayers = new();
            HunterPlayers = new();
            HunterExilePlayer = null;
        }
        public static void IntroHandler()
        {
            if (!AmongUsClient.Instance.AmHost) return;
            PlayerControl.LocalPlayer.RpcSendChat(ModTranslation.GetString("WereWolfMeetingNormal"));
            new LateTask(() =>
            {
                IsDiscussion = true;
                PlayerControl.GameOptions.VotingTime = DiscussionTime;
                CachedPlayer.LocalPlayer.PlayerControl.RpcSyncSettings(PlayerControl.GameOptions);
                MeetingRoomManager.Instance.AssignSelf(PlayerControl.LocalPlayer, CachedPlayer.LocalPlayer.Data);
                FastDestroyableSingleton<HudManager>.Instance.OpenMeetingRoom(PlayerControl.LocalPlayer);
                PlayerControl.LocalPlayer.RpcStartMeeting(CachedPlayer.LocalPlayer.Data);
            }, 20, "DiscussionStartMeeting");
        }
        public static void Wrapup(GameData.PlayerInfo exiled)
        {
            IsDiscussion = !IsDiscussion;
            if (!AmongUsClient.Instance.AmHost) return;
            if (IsAbility)
            {
                PlayerControl.GameOptions.VotingTime = AbilityTime;
                PlayerControl.LocalPlayer.RpcSendChat(ModTranslation.GetString("WereWolfMeetingAbility"));
                HunterExilePlayer = null;
                SoothRoles = new();
                HunterKillPlayers = new();
                WolfKillPlayers = new();
            }
            else
            {
                PlayerControl.GameOptions.VotingTime = DiscussionTime;
                PlayerControl.LocalPlayer.RpcSendChat(ModTranslation.GetString("WereWolfMeetingNormal"));
                if (HunterExilePlayer != null)
                {
                    HunterExilePlayer.RpcMurderPlayer(HunterExilePlayer);
                }
                foreach (int playerid in WolfKillPlayers)
                {
                    PlayerControl player = ModHelpers.PlayerById((byte)playerid);
                    if (player != null) player.RpcMurderPlayer(player);
                }
            }
            if (IsDiscussion)
            {
                Time = 3;
                foreach (var players in SoothRoles)
                {
                    PlayerControl source = ModHelpers.PlayerById((byte)players.Key);
                    PlayerControl target = ModHelpers.PlayerById((byte)players.Value);
                    if (source == null || target == null || source.Data.Disconnected) break;
                    string Chat = "";
                    var RoleDate = IntroDate.GetIntroDate(target.GetRole(), target);
                    var RoleName = ModTranslation.GetString("Werewolf" + RoleDate.NameKey + "Name");
                    Chat += string.Format(ModTranslation.GetString("WereWolfMediumAbilityText"), target.GetDefaultName(), RoleName);
                    new LateTask(() =>
                    {
                        source.RPCSendChatPrivate(Chat);
                    }, Time, "AbilityChatSend");
                    Time += 3;
                }
                if (exiled != null)
                {
                    foreach (PlayerControl player in RoleClass.SpiritMedium.SpiritMediumPlayer)
                    {
                        string Chat = "";
                        PlayerControl target = exiled.Object;
                        var RoleDate = IntroDate.GetIntroDate(target.GetRole(), target);
                        var RoleName = ModTranslation.GetString("Werewolf" + RoleDate.NameKey + "Name");
                        Chat += string.Format(ModTranslation.GetString("WereWolfMediumAbilityText"), target.GetDefaultName(), RoleName);
                        new LateTask(() =>
                        {
                            player.RPCSendChatPrivate(Chat);
                        }, Time, "AbilityChatSend");
                        Time += 3;
                    }
                }
                foreach (var players in HunterKillPlayers)
                {
                    var player = ModHelpers.PlayerById((byte)players);
                    player.RpcMurderPlayer(player);
                }
                if (Time <= 9)
                {
                    Time += 7;
                }
                new LateTask(() =>
                {
                    GameData.PlayerInfo target;
                    try
                    {
                        target = ModHelpers.PlayerById((byte)WolfKillPlayers[0]).Data;
                    }
                    catch
                    {
                        target = CachedPlayer.LocalPlayer.Data;
                    }
                    MeetingRoomManager.Instance.AssignSelf(PlayerControl.LocalPlayer, target);
                    FastDestroyableSingleton<HudManager>.Instance.OpenMeetingRoom(PlayerControl.LocalPlayer);
                    PlayerControl.LocalPlayer.RpcStartMeeting(target);
                    PlayerControl.LocalPlayer.RpcSetName(ModTranslation.GetString("WereWolfMeetingNormal"));
                    SetDefaultName();
                }, Time, "KillStartMeeting");

                static void SetDefaultName()
                {
                    new LateTask(() =>
                    {
                        PlayerControl.LocalPlayer.RpcSetName(PlayerControl.LocalPlayer.GetDefaultName());
                    }, 5, "NameChangeMeeting");
                }
                SoothRoles = new();
                HunterKillPlayers = new();
            }
            else if (IsAbility)
            {
                new LateTask(() =>
                {
                    MeetingRoomManager.Instance.AssignSelf(PlayerControl.LocalPlayer, null);
                    FastDestroyableSingleton<HudManager>.Instance.OpenMeetingRoom(PlayerControl.LocalPlayer);
                    PlayerControl.LocalPlayer.RpcStartMeeting(null);
                    PlayerControl.LocalPlayer.RpcSetName(ModTranslation.GetString("WereWolfMeetingAbility"));
                }, 11, "AbilityStartMeeting");
                new LateTask(() =>
                {
                    PlayerControl.LocalPlayer.RpcSetName(PlayerControl.LocalPlayer.GetDefaultName());
                }, 5, "NameChangeMeeting");
            }
            CachedPlayer.LocalPlayer.PlayerControl.RpcSyncSettings(PlayerControl.GameOptions);
        }
    }
}