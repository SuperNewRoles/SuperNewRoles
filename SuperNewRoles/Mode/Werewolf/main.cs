using Hazel;
using SuperNewRoles.Helpers;
using SuperNewRoles.Intro;
using SuperNewRoles.Mode.SuperHostRoles;
using SuperNewRoles.Roles;
using System;
using System.Collections.Generic;
using System.Text;

namespace SuperNewRoles.Mode.Werewolf
{
    class main
    {
        public static bool IsDiscussion;
        public static bool IsFirst;
        public static int AbilityTime;
        public static int DiscussionTime;
        public static Dictionary<int,int> SoothRoles;
        public static List<int> HunterKillPlayers;
        public static List<int> WolfKillPlayers;
        public static List<PlayerControl> HunterPlayers = new List<PlayerControl>();
        public static PlayerControl HunterExilePlayer;
        public static int Time;
        public static bool IsAbility {get{return!IsDiscussion;}set{IsDiscussion=!value;}}
        public static void ClearAndReload()
        {
            PlayerControl.GameOptions.KillCooldown = -1;
            PlayerControl.GameOptions.DiscussionTime = 0;
            CachedPlayer.LocalPlayer.PlayerControl.RpcSyncSettings(PlayerControl.GameOptions);
            foreach (PlayerControl p in CachedPlayer.AllPlayers)
            {
                p.getDefaultName();
            }
            IsDiscussion = true;
            IsFirst = true;
            DiscussionTime = 30;
            AbilityTime = 10;
            SoothRoles = new Dictionary<int, int>();
            HunterKillPlayers = new List<int>();
            WolfKillPlayers = new List<int>();
            HunterPlayers = new List<PlayerControl>();
            HunterExilePlayer = null;
        }
        public static void IntroHandler()
        {
            if (!AmongUsClient.Instance.AmHost) return;
            PlayerControl.LocalPlayer.RpcSendChat("今回は会議タイムです。");
            new LateTask(() => {
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
                PlayerControl.LocalPlayer.RpcSendChat("今回は能力使用タイムです。");
                HunterExilePlayer = null;
                SoothRoles = new Dictionary<int, int>();
                HunterKillPlayers = new List<int>();
                WolfKillPlayers = new List<int>();
            }
            else
            {
                PlayerControl.GameOptions.VotingTime = DiscussionTime;
                PlayerControl.LocalPlayer.RpcSendChat("今回は会議タイムです。");
                if (HunterExilePlayer != null)
                {
                    HunterExilePlayer.RpcMurderPlayer(HunterExilePlayer);
                }
                foreach (int playerid in WolfKillPlayers) {
                    PlayerControl player = ModHelpers.playerById((byte)playerid);
                    if (player != null) player.RpcMurderPlayer(player);
                }
            }
            if (IsDiscussion) {
                Time = 3;
                foreach (var players in SoothRoles)
                {
                    PlayerControl source = ModHelpers.playerById((byte)players.Key);
                    PlayerControl target = ModHelpers.playerById((byte)players.Value);
                    if (source == null || target == null || source.Data.Disconnected) break;
                    string Chat = "";
                    var RoleDate = IntroDate.GetIntroDate(target.getRole(), target);
                    var RoleName = ModTranslation.getString("Werewolf"+RoleDate.NameKey+"Name");
                    Chat += string.Format("{0}の役職は{1}です！",target.getDefaultName(),RoleName);
                    new LateTask(() => {
                        source.RPCSendChatPrivate(Chat);
                    }, Time, "AbilityChatSend");
                    Time += 3;
                }
                if (exiled != null)
                {
                    foreach (PlayerControl player in RoleClass.SpiritMedium.SpiritMediumPlayer) {
                        string Chat = "";
                        PlayerControl target = exiled.Object;
                        var RoleDate = IntroDate.GetIntroDate(target.getRole(), target);
                        var RoleName = ModTranslation.getString("Werewolf" + RoleDate.NameKey + "Name");
                        Chat += string.Format("{0}の役職は{1}です！", target.getDefaultName(), RoleName);
                        new LateTask(() => {
                            player.RPCSendChatPrivate(Chat);
                        }, Time, "AbilityChatSend");
                        Time += 3;
                    }
                }
                foreach (var players in HunterKillPlayers)
                {
                    var player = ModHelpers.playerById((byte)players);
                    player.RpcMurderPlayer(player);
                }
                if (Time <= 9)
                {
                    Time += 7;
                }
                new LateTask(() => {
                    GameData.PlayerInfo target;
                    try
                    {
                        target = ModHelpers.playerById((byte)WolfKillPlayers[0]).Data;
                    }
                    catch
                    {
                        target = CachedPlayer.LocalPlayer.Data;
                    }
                    MeetingRoomManager.Instance.AssignSelf(PlayerControl.LocalPlayer, target);
                    FastDestroyableSingleton<HudManager>.Instance.OpenMeetingRoom(PlayerControl.LocalPlayer);
                    PlayerControl.LocalPlayer.RpcStartMeeting(target);
                    PlayerControl.LocalPlayer.RpcSetName("今回は会議タイムです。");
                    SetDefaultName();
                }, Time, "KillStartMeeting");
                void SetDefaultName()
                {
                    new LateTask(() =>
                    {
                        PlayerControl.LocalPlayer.RpcSetName(PlayerControl.LocalPlayer.getDefaultName());
                    }, 5, "NameChangeMeeting");
                }
                SoothRoles = new Dictionary<int, int>();
                HunterKillPlayers = new List<int>();
            } else if (IsAbility)
            {
                new LateTask(() => {
                    MeetingRoomManager.Instance.AssignSelf(PlayerControl.LocalPlayer,null);
                    FastDestroyableSingleton<HudManager>.Instance.OpenMeetingRoom(PlayerControl.LocalPlayer);
                    PlayerControl.LocalPlayer.RpcStartMeeting(null);
                    PlayerControl.LocalPlayer.RpcSetName("今回は能力使用タイムです。");
                }, 11, "AbilityStartMeeting");
                new LateTask(() =>
                {
                    PlayerControl.LocalPlayer.RpcSetName(PlayerControl.LocalPlayer.getDefaultName());
                }, 5, "NameChangeMeeting");
            }
            CachedPlayer.LocalPlayer.PlayerControl.RpcSyncSettings(PlayerControl.GameOptions);
        }
    }
}
