using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BepInEx.IL2CPP.Utils;
using HarmonyLib;
using SuperNewRoles.CustomOption;
using SuperNewRoles.Intro;
using SuperNewRoles.Mode.SuperHostRoles;
using SuperNewRoles.Roles;
using UnityEngine;
using static System.String;

namespace SuperNewRoles.Patch
{
    [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.CoSpawnPlayer))]
    public class AmongUsClientOnPlayerJoinedPatch
    {
        public static void Postfix(PlayerPhysics __instance)
        {
            if (AmongUsClient.Instance.AmHost && AmongUsClient.Instance.GameMode != GameModes.FreePlay)
            {
                string text =
                    ModTranslation.GetString("WelcomeMessage1") + "\n" +
                    ModTranslation.GetString("WelcomeMessage2") + "\n" +
                    ModTranslation.GetString("WelcomeMessage3") + "\n" +
                    ModTranslation.GetString("WelcomeMessage4") + "\n" +
                    ModTranslation.GetString("WelcomeMessage5") + "\n" +
                    ModTranslation.GetString("WelcomeMessage6") + "\n" +
                    ModTranslation.GetString("WelcomeMessage7") + "\n" +
                    ModTranslation.GetString("WelcomeMessage8") +
                    " " + "\n.";
                new LateTask(() =>
                {
                    if (__instance.myPlayer.IsPlayer())
                    {
                        AddChatPatch.SendCommand(__instance.myPlayer, text, AddChatPatch.WelcomeToSuperNewRoles);
                    }
                }
                , 1f);
                return;
            }
        }
    }
    [HarmonyPatch(typeof(ChatController), nameof(ChatController.AddChat))]
    class AddChatPatch
    {
        static readonly string SNR = "<color=#ffa500>Super</color><color=#ff0000>New</color><color=#00ff00>Roles</color>";
        static readonly string SNRCommander = "<size=200%>" + SNR + "</size>";
        public static string WelcomeToSuperNewRoles = "<size=150%>Welcome To " + SNR + "</size>";

        public static bool Prefix(PlayerControl sourcePlayer, string chatText)
        {
            if (AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Started)
            {
                if (AmongUsClient.Instance.AmHost)
                {
                    Assassin.AddChat(sourcePlayer, chatText);
                }
            }

            var Commands = chatText.Split(" ");
            if (Commands[0].Equals("/version", StringComparison.OrdinalIgnoreCase) ||
                Commands[0].Equals("/v", StringComparison.OrdinalIgnoreCase))
            {
                string betatext = "";
                if (SuperNewRolesPlugin.IsBeta)
                {
                    betatext = "\nベータ版です！バグには注意してください！";
                }
                SendCommand(sourcePlayer, " SuperNewRoles v" + SuperNewRolesPlugin.VersionString + "\nCreate by ykundesu" + betatext);
                return false;
            }
            else if (
                Commands[0].Equals("/Commands", StringComparison.OrdinalIgnoreCase) ||
                Commands[0].Equals("/Cmd", StringComparison.OrdinalIgnoreCase)
                )
            {
                string text =
                    ModTranslation.GetString("CommandsMessage0") + "\n\n" +
                    ModTranslation.GetString("CommandsMessage1") + "\n" +
                    ModTranslation.GetString("CommandsMessage2") + "\n" +
                    ModTranslation.GetString("CommandsMessage3") + "\n" +
                    ModTranslation.GetString("CommandsMessage4") + "\n" +
                    ModTranslation.GetString("CommandsMessage5") + "\n" +
                    ModTranslation.GetString("CommandsMessage6") + "\n" +
                    ModTranslation.GetString("CommandsMessage7") + "\n" +
                    ModTranslation.GetString("CommandsMessage8") + "\n" +
                    ModTranslation.GetString("CommandsMessage9");
                SendCommand(sourcePlayer, text);
                return false;
            }
            else if (
                Commands[0].Equals("/Discord", StringComparison.OrdinalIgnoreCase) ||
                Commands[0].Equals("/dc", StringComparison.OrdinalIgnoreCase)
                )
            {
                SendCommand(sourcePlayer, ModTranslation.GetString("SNROfficialDiscordMessage") + "\n" + MainMenuPatch.snrdiscordserver);
                return false;
            }
            else if (
                Commands[0].Equals("/Twitter", StringComparison.OrdinalIgnoreCase) ||
                Commands[0].Equals("/tw", StringComparison.OrdinalIgnoreCase)
                )
            {
                SendCommand(sourcePlayer, ModTranslation.GetString("SNROfficialTwitterMessage") + "\n\n" + ModTranslation.GetString("TwitterOfficialLink") + "\n" + ModTranslation.GetString("TwitterDevLink"));
                return false;
            }
            else if (
                Commands[0].Equals("/GetInRoles", StringComparison.OrdinalIgnoreCase) ||
                Commands[0].Equals("/gr", StringComparison.OrdinalIgnoreCase)
                )
            {
                if (Commands.Length == 1)
                {
                    if (sourcePlayer.AmOwner)
                    {
                        GetInRoleCommand(null);
                    }
                    else
                    {
                        GetInRoleCommand(sourcePlayer);
                    }
                }
                else
                {
                    PlayerControl target = sourcePlayer.AmOwner ? null : sourcePlayer;
                    if (Commands.Length >= 2 && (Commands[1].Equals("mp", StringComparison.OrdinalIgnoreCase) || Commands[1].Equals("myplayer", StringComparison.OrdinalIgnoreCase) || Commands[1].Equals("myp", StringComparison.OrdinalIgnoreCase)))
                    {
                        target = sourcePlayer;
                    }
                    GetInRoleCommand(target);
                }
                return false;
            }
            else if (
                Commands[0].Equals("/AllRoles", StringComparison.OrdinalIgnoreCase) ||
                Commands[0].Equals("/ar", StringComparison.OrdinalIgnoreCase)
                )
            {
                if (Commands.Length == 1)
                {
                    if (sourcePlayer.AmOwner)
                    {
                        RoleCommand(null);
                    }
                    else
                    {
                        RoleCommand(sourcePlayer);
                    }
                }
                else
                {
                    PlayerControl target = sourcePlayer.AmOwner ? null : sourcePlayer;
                    if (Commands.Length >= 3 && (Commands[2].Equals("mp", StringComparison.OrdinalIgnoreCase) || Commands[2].Equals("myplayer", StringComparison.OrdinalIgnoreCase) || Commands[2].Equals("myp", StringComparison.OrdinalIgnoreCase)))
                    {
                        target = sourcePlayer;
                    }
                    if (!float.TryParse(Commands[1], out float sendtime))
                    {
                        return false;
                    }
                    RoleCommand(SendTime: sendtime, target: target);
                }
                return false;
            }
            else
            {
                return true;
            }
        }
        static string GetChildText(List<CustomOption.CustomOption> options, string indent)
        {
            string text = "";
            foreach (CustomOption.CustomOption option in options)
            {
                text += indent + option.GetName() + ":" + option.GetString() + "\n";
                if (option.children.Count > 0)
                {
                    text += GetChildText(option.children, indent + "  ");
                }
            }
            return text;
        }
        static string GetOptionText(CustomRoleOption RoleOption, IntroDate intro)
        {
            string text = "";
            text += GetChildText(RoleOption.children, "  ");
            return text;
        }
        static string GetTeamText(TeamRoleType type)
        {
            return type switch
            {
                TeamRoleType.Crewmate => ModTranslation.GetString("CrewMateName"),
                TeamRoleType.Impostor => ModTranslation.GetString("ImpostorName"),
                TeamRoleType.Neutral => ModTranslation.GetString("NeutralName").Replace("陣営", ""),
                _ => "",
            };
        }
        static string GetText(CustomRoleOption option)
        {
            string text = "\n";
            IntroDate intro = option.Intro;
            text += GetTeamText(intro.Team) + ModTranslation.GetString("Team") + "\n";
            text += "「" + IntroDate.GetTitle(intro.NameKey, intro.TitleNum) + "」\n";
            text += intro.Description + "\n";
            text += ModTranslation.GetString("MessageSettings") + ":\n";
            text += GetOptionText(option, intro);
            return text;
        }
        static string GetInRole(List<CustomRoleOption> optionsnotorder)
        {
            string text = ModTranslation.GetString("NowRolesMessage") + "\n";
            var options = optionsnotorder.OrderBy((CustomRoleOption x) =>
            {
                return x.Intro.Team switch
                {
                    TeamRoleType.Impostor => 0,
                    TeamRoleType.Neutral => 1000,
                    TeamRoleType.Crewmate => 2000,
                    _ => 500,
                };
            });
            TeamRoleType type = TeamRoleType.Error;
            foreach (CustomRoleOption option in options)
            {
                if (type != option.Intro.Team)
                {
                    type = option.Intro.Team;
                    text += "\n" + Format(ModTranslation.GetString("TeamMessage"), GetTeamText(type)) + "\n\n";
                }
                int PlayerCount = 0;
                foreach (CustomOption.CustomOption opt in option.children)
                {
                    if (opt.GetName() == CustomOptions.SheriffPlayerCount.GetName())
                    {
                        PlayerCount = (int)opt.GetFloat();
                        break;
                    }
                }
                text += option.Intro.Name + " : " + PlayerCount + ModTranslation.GetString("PlayerCountMessage") + "\n";
            }
            return text;
        }
        static void RoleCommand(PlayerControl target = null, float SendTime = 1.5f)
        {
            if (!AmongUsClient.Instance.AmHost) return;
            List<CustomRoleOption> EnableOptions = new();
            foreach (CustomRoleOption option in CustomRoleOption.RoleOptions)
            {
                if (option.IsRoleEnable && option.isSHROn)
                {
                    EnableOptions.Add(option);
                }
            }
            float time = 0;
            foreach (CustomRoleOption option in EnableOptions)
            {
                string text = GetText(option);
                string rolename = "<size=115%>\n" + CustomOptions.Cs(option.Intro.color, option.Intro.NameKey + "Name") + "</size>";
                SuperNewRolesPlugin.Logger.LogInfo(text);
                Send(target, rolename, text, time);
                time += SendTime;
            }
        }
        static void GetInRoleCommand(PlayerControl target = null)
        {
            if (!AmongUsClient.Instance.AmHost) return;
            List<CustomRoleOption> EnableOptions = new();
            foreach (CustomRoleOption option in CustomRoleOption.RoleOptions)
            {
                if (option.IsRoleEnable && option.isSHROn)
                {
                    EnableOptions.Add(option);
                }
            }
            SendCommand(target, GetInRole(EnableOptions));
        }
        static void Send(PlayerControl target, string rolename, string text, float time = 0)
        {
            text = "\n" + text + "\n                                                                                                                                                                                                                                              ";
            if (time <= 0)
            {
                if (target == null)
                {
                    string name = PlayerControl.LocalPlayer.GetDefaultName();
                    AmongUsClient.Instance.StartCoroutine(AllSend(SNRCommander + rolename, text, name));
                    return;
                }
                if (target.PlayerId != 0)
                {
                    AmongUsClient.Instance.StartCoroutine(PrivateSend(target, SNRCommander + rolename, text, time));
                }
                else
                {
                    string name = PlayerControl.LocalPlayer.GetDefaultName();
                    PlayerControl.LocalPlayer.SetName(SNRCommander + "\n" + rolename);
                    FastDestroyableSingleton<HudManager>.Instance.Chat.AddChat(PlayerControl.LocalPlayer, text);
                    PlayerControl.LocalPlayer.SetName(name);
                }
                return;
            }
            else
            {
                string name = PlayerControl.LocalPlayer.GetDefaultName();
                if (target == null)
                {
                    AmongUsClient.Instance.StartCoroutine(AllSend(SNRCommander + rolename, text, name, time));
                    return;
                }
                if (target.PlayerId != 0)
                {
                    AmongUsClient.Instance.StartCoroutine(PrivateSend(target, SNRCommander + rolename, text, time));
                }
                else
                {
                    new LateTask(() =>
                    {
                        PlayerControl.LocalPlayer.SetName(SNRCommander + rolename);
                        FastDestroyableSingleton<HudManager>.Instance.Chat.AddChat(PlayerControl.LocalPlayer, text);
                        PlayerControl.LocalPlayer.SetName(name);
                    }, time);
                }
                return;
            }
        }
        public static void SendCommand(PlayerControl target, string command, string SendName = "NONE")
        {
            if (!AmongUsClient.Instance.AmHost) return;
            if (SendName == "NONE") SendName = SNRCommander;
            command = $"\n{command}\n";
            if (target != null && target.Data.Disconnected) return;
            if (target == null)
            {
                string name = CachedPlayer.LocalPlayer.Data.PlayerName;
                if (name == SNRCommander) return;
                AmongUsClient.Instance.StartCoroutine(AllSend(SendName, command, name));
                return;
            }
            else if (target.PlayerId == 0)
            {
                string name = target.Data.PlayerName;
                target.SetName(SendName);
                FastDestroyableSingleton<HudManager>.Instance.Chat.AddChat(target, command);
                target.SetName(name);
            }
            else
            {
                AmongUsClient.Instance.StartCoroutine(PrivateSend(target, SendName, command));
            }
        }
        static IEnumerator AllSend(string SendName, string command, string name, float time = 0)
        {
            if (time > 0)
            {
                yield return new WaitForSeconds(time);
            }
            var crs = CustomRpcSender.Create();
            crs.AutoStartRpc(PlayerControl.LocalPlayer.NetId, (byte)RpcCalls.SetName)
                .Write(SendName)
                .EndRpc()
                .AutoStartRpc(PlayerControl.LocalPlayer.NetId, (byte)RpcCalls.SendChat)
                .Write(command)
                .EndRpc()
                .AutoStartRpc(PlayerControl.LocalPlayer.NetId, (byte)RpcCalls.SetName)
                .Write(name)
                .EndRpc()
                .SendMessage();
            PlayerControl.LocalPlayer.SetName(SendName);
            FastDestroyableSingleton<HudManager>.Instance.Chat.AddChat(PlayerControl.LocalPlayer, command);
            PlayerControl.LocalPlayer.SetName(name);
        }
        static IEnumerator PrivateSend(PlayerControl target, string SendName, string command, float time = 0)
        {
            if (time > 0)
            {
                yield return new WaitForSeconds(time);
            }
            var crs = CustomRpcSender.Create();
            crs.AutoStartRpc(target.NetId, (byte)RpcCalls.SetName, target.GetClientId())
                .Write(SendName)
                .EndRpc()
                .AutoStartRpc(target.NetId, (byte)RpcCalls.SendChat, target.GetClientId())
                .Write(command)
                .EndRpc()
                .AutoStartRpc(target.NetId, (byte)RpcCalls.SetName, target.GetClientId())
                .Write(target.Data.PlayerName)
                .EndRpc()
                .SendMessage();
        }
    }/*
    [HarmonyPatch(typeof(ChatController),nameof(ChatController.AddChat))]
    class ChatHandler
    {
        public static bool Prefix(ChatController __instance, [HarmonyArgument(0)] ref PlayerControl sourcePlayer, [HarmonyArgument(1)] ref string chatText)
        {

            if (!(bool)(UnityEngine.Object)sourcePlayer || !(bool)(UnityEngine.Object)PlayerControl.LocalPlayer)
                return false;
            GameData.PlayerInfo data1 = CachedPlayer.LocalPlayer.Data;
            GameData.PlayerInfo data2 = sourcePlayer.Data;
            if (data2 == null || data1 == null || data2.IsDead && (!PlayerControl.LocalPlayer.IsDead() || PlayerControl.LocalPlayer.IsRole(RoleId.NiceRedRidingHood)))
                return false;
            if (__instance.chatBubPool.NotInUse == 0)
                __instance.chatBubPool.ReclaimOldest();
            ChatBubble bubble = FastDestroyableSingleton<HudManager>.Instance.Chat.chatBubPool.Get<ChatBubble>();
            try
            {
                bubble.transform.SetParent(__instance.scroller.Inner);
                bubble.transform.localScale = Vector3.one;
                int num = (UnityEngine.Object)sourcePlayer == (UnityEngine.Object)PlayerControl.LocalPlayer ? 1 : 0;
                if (num != 0)
                    bubble.SetRight();
                else
                    bubble.SetLeft();
                bool flag = (bool)(UnityEngine.Object)data1.Role && (bool)(UnityEngine.Object)data2.Role && data1.Role.NameColor == data2.Role.NameColor;
                bool didVote = (bool)(UnityEngine.Object)MeetingHud.Instance && MeetingHud.Instance.DidVote(sourcePlayer.PlayerId);
                https://media.discordapp.net/attachments/965644999578513450/967642315541856286/2022-04-24_3.png?width=875&height=492           bubble.SetCosmetics(data2);
                __instance.SetChatBubbleName(bubble, data2, data2.IsDead, didVote, flag ? data2.Role.NameColor : Color.white);
                if (SaveManager.CensorChat)
                    chatText = BlockedWords.CensorWords(chatText);
                bubble.SetText(chatText);
                bubble.AlignChildren();
                __instance.AlignAllBubbles();
                if (!FastDestroyableSingleton<HudManager>.Instance.Chat.IsOpen && FastDestroyableSingleton<HudManager>.Instance.Chat.notificationRoutine == null)
                    FastDestroyableSingleton<HudManager>.Instance.Chat.notificationRoutine = __instance.StartCoroutine(__instance.BounceDot());
                if (num != 0)
                    return false;
                SoundManager.Instance.PlaySound(__instance.MessageSound, false).pitch = (float)(0.5 + (double)sourcePlayer.PlayerId / 15.0);
            }
            catch (Exception ex)
            {
                SuperNewRolesPlugin.Logger.LogError((object)ex);
                FastDestroyableSingleton<HudManager>.Instance.Chat.chatBubPool.Reclaim((PoolableBehavior)bubble);
            }
            return false;
        }
    }
    **/
}