using BepInEx.IL2CPP.Utils;
using HarmonyLib;
using SuperNewRoles.CustomOption;
using SuperNewRoles.Helpers;
using SuperNewRoles.Intro;
using SuperNewRoles.Mode.SuperHostRoles;
using SuperNewRoles.Roles;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SuperNewRoles.Patch
{
    [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.CoSpawnPlayer))]
    public class AmongUsClientOnPlayerJoinedPatch
    {
        public static void Postfix(PlayerPhysics __instance, LobbyBehaviour lobby)
        {
            if (AmongUsClient.Instance.AmHost && AmongUsClient.Instance.GameMode != GameModes.FreePlay)
            {
                string text = "SuperNewRolesへようこそ！" + "\n" +
                    "SuperNewRolesを使用することで、様々なモードや様々な役職をどの機種でも遊べます！" + "\n" +
                    "役職やモードの詳しい説明については公式wikiを御覧ください！" + "\n" +
                    "SuperNewRoles公式wiki:" + "\n" +
                    "　https://wikiwiki.jp/amas-snr" + "\n" +
                    "コマンドを実行して、情報を取得できます。" + "\n" +
                    "コマンド一覧を表示するにはこのコマンドを送信してください。" + "\n" +
                    "/commands" +
                    " " + "\n.";
                new LateTask(() => {
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
        static string SNR = "<color=#ffa500>Super</color><color=#ff0000>New</color><color=#00ff00>Roles</color>";
        static string SNRCommander = "<size=200%>"+SNR+"</size>";
        public static string WelcomeToSuperNewRoles = "<size=150%>Welcome To " + SNR + "</size>";

        public static void Postfix(PlayerControl sourcePlayer, string chatText)
        {
            if (!AmongUsClient.Instance.AmHost) return;

            if (AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Started)
            {
                Assassin.AddChat(sourcePlayer, chatText);
            }

            var Commands = chatText.Split(" ");
            if (Commands[0].Equals("/version", StringComparison.OrdinalIgnoreCase) ||
                Commands[0].Equals("/v", StringComparison.OrdinalIgnoreCase))
            {
                SendCommand(sourcePlayer, " SuperNewRoles v" + SuperNewRolesPlugin.VersionString + "\nCreate by ykundesu");
            }
            else if (
              Commands[0].Equals("/Commands", StringComparison.OrdinalIgnoreCase) ||
              Commands[0].Equals("/Cmd", StringComparison.OrdinalIgnoreCase)
              )
            {
                string text = "コマンド一覧:\n()の中にあるコマンドでも可能です。\n大文字と小文字は区別されません。\n\n";
                text += "/Commands (/cmd) : \nコマンド一覧を表示します。(このコマンドです)\n";
                text += "/version (/v) ： \nバージョンを表示します。\n";
                text += "/Discord (/dc) ： \n公式Discordへのリンクを表示します。\n";
                text += "/Twitter (/tw) ： \n公式Twitterと開発状況Twitterへのリンクを表示します。\n";
                text += "/GetInRoles (/gr) ： \n入っているすべての役職を表示します。(ホストの場合は全員に送信します)\n";
                text += "/GetInRoles myplayer (/gr mp もしくは /gr myp) ： \n入っているすべての役職を表示します。(ホストでも自分のみに送信します)\n";
                text += "/AllRoles (/ar) ： \n入っている全役職の説明と設定が確認できます。\n(ホストが実行すると全員に送信します)\n1.5秒間隔で送信されます。\n";
                text += "/AllRoles [送信間隔] (/ar [送信間隔]) ： \n/AllRolesに+で送信間隔を指定できます。";
                text += "/AllRoles [送信間隔] myplayer (/ar [送信間隔] (mp もしくは myp)) ： \n/AllRoles [送信間隔]に+で、ホストでも自分のみに送信されます。";
                SendCommand(sourcePlayer, text);
            }
            else if (
              Commands[0].Equals("/Discord", StringComparison.OrdinalIgnoreCase) || 
              Commands[0].Equals("/dc", StringComparison.OrdinalIgnoreCase)
              )
            {
                SendCommand(sourcePlayer, "SuperNewRoles公式Discordサーバーはこちらから:\n"+ MainMenuPatch.snrdiscordserver);
            }
            else if (
              Commands[0].Equals("/Twitter", StringComparison.OrdinalIgnoreCase) ||
              Commands[0].Equals("/tw", StringComparison.OrdinalIgnoreCase)
              )
            {
                SendCommand(sourcePlayer, "SuperNewRoles公式Twitterはこちらから\n\n公式アカウント:　https://twitter.com/SuperNewRoles \n開発情報アカウント:　https://twitter.com/SNRDevs");
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
                    return;
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
                    return;
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
                        return;
                    }
                    RoleCommand(SendTime: sendtime, target: target);
                }
            }
        }
        static string GetChildText(List<CustomOption.CustomOption> options,string indent)
        {
            string text = "";
            foreach (CustomOption.CustomOption option in options)
            {
                text += indent + option.getName() + ":" + option.getString() + "\n";
                if (option.children.Count > 0)
                {
                    text += GetChildText(option.children, indent + "　");
                }
            }
            return text;
        }
        static string GetOptionText(CustomRoleOption RoleOption, IntroDate intro)
        {
            string text = "";
            text += GetChildText(RoleOption.children, "　");
            return text;
        }
        static string GetTeamText(TeamRoleType type)
        {
            switch (type)
            {
                case TeamRoleType.Crewmate:
                    return ModTranslation.getString("CrewMateName");
                case TeamRoleType.Impostor:
                    return ModTranslation.getString("ImpostorName");
                case TeamRoleType.Neutral:
                    return ModTranslation.getString("NeutralName").Replace("陣営", "");
            }
            return "";
        }
        static string GetText(CustomRoleOption option)
        {
            string text = "";
            IntroDate intro = option.Intro;
            text += "【" + intro.Name + "】\n";
            text += GetTeamText(intro.Team) + "陣営\n";
            text += "「" + IntroDate.GetTitle(intro.NameKey, intro.TitleNum) + "」\n";
            text += intro.Description + "\n";
            text += "設定:\n";
            text += GetOptionText(option, intro);
            return text;
        }
        static string GetInRole(List<CustomRoleOption> optionsnotorder)
        {
            string text = "現在入っている役職\n";
            var options = optionsnotorder.OrderBy((CustomRoleOption x) => {
                switch (x.Intro.Team)
                {
                    case TeamRoleType.Impostor:
                        return 0;
                    case TeamRoleType.Neutral:
                        return 1000;
                    case TeamRoleType.Crewmate:
                        return 2000;
                }
                return 500;
            });
            TeamRoleType type = TeamRoleType.Error;
            foreach (CustomRoleOption option in options)
            {
                if (type != option.Intro.Team)
                {
                    type = option.Intro.Team;
                    text += "\n【" + GetTeamText(type) + "陣営】\n\n";
                }
                int PlayerCount = 0;
                foreach (CustomOption.CustomOption opt in option.children)
                {
                    if (opt.getName() == CustomOptions.SheriffPlayerCount.getName())
                    {
                        PlayerCount = (int)opt.getFloat();
                        break;
                    }
                }
                text += option.Intro.Name + " : " + PlayerCount + "人\n";
            }
            return text;
        }
        static void RoleCommand(PlayerControl target = null,float SendTime = 1.5f)
        {
            List<CustomRoleOption> EnableOptions = new List<CustomRoleOption>();
            foreach (CustomRoleOption option in CustomRoleOption.RoleOptions)
            {
                if (option.isRoleEnable && option.isSHROn)
                {
                    EnableOptions.Add(option);
                }
            }
            float time = 0;
            foreach (CustomRoleOption option in EnableOptions)
            {
                string text = GetText(option);
                SuperNewRolesPlugin.Logger.LogInfo(text);
                Send(target, text, time);
                time += SendTime;
            }
        }
        static void GetInRoleCommand(PlayerControl target = null)
        {
            List<CustomRoleOption> EnableOptions = new List<CustomRoleOption>();
            foreach (CustomRoleOption option in CustomRoleOption.RoleOptions)
            {
                if (option.isRoleEnable && option.isSHROn)
                {
                    EnableOptions.Add(option);
                }
            }
            SendCommand(target, GetInRole(EnableOptions));
        }
        static void Send(PlayerControl target, string text, float time = 0)
        {
            text = "\n" + text + "\n　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　";
            if (time <= 0)
            {
                if (target == null)
                {
                    string name = PlayerControl.LocalPlayer.getDefaultName();
                    AmongUsClient.Instance.StartCoroutine(AllSend(SNRCommander, text, name));
                    return;
                }
                if (target.PlayerId != 0)
                {
                    AmongUsClient.Instance.StartCoroutine(PrivateSend(target, SNRCommander, text, time));
                } else
                {
                    string name = PlayerControl.LocalPlayer.getDefaultName();
                    PlayerControl.LocalPlayer.SetName(SNRCommander);
                    FastDestroyableSingleton<HudManager>.Instance.Chat.AddChat(PlayerControl.LocalPlayer, text);
                    PlayerControl.LocalPlayer.SetName(name);
                }
                return;
            }
            else
            {
                string name = PlayerControl.LocalPlayer.getDefaultName();
                if (target == null)
                {
                    AmongUsClient.Instance.StartCoroutine(AllSend(SNRCommander, text, name, time));
                    return;
                }
                if (target.PlayerId != 0)
                {
                AmongUsClient.Instance.StartCoroutine(PrivateSend(target, SNRCommander, text, time));
                }
                else
                {
                    new LateTask(() => {
                        PlayerControl.LocalPlayer.SetName(SNRCommander);
                        FastDestroyableSingleton<HudManager>.Instance.Chat.AddChat(PlayerControl.LocalPlayer, text);
                        PlayerControl.LocalPlayer.SetName(name);
                    }, time);
                }
                return;
            }
        }
        public static void SendCommand(PlayerControl target, string command, string SendName = "NONE")
        {
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
            } else
            {
                AmongUsClient.Instance.StartCoroutine(PrivateSend(target, SendName, command));
            }
        }
        static IEnumerator AllSend(string SendName, string command,string name, float time = 0)
        {
            if (time > 0)
            {
                yield return new WaitForSeconds(time);
            }
            var crs = CustomRpcSender.Create();
            crs.StartRpc(CachedPlayer.LocalPlayer.NetId, RpcCalls.SetName)
                .Write(SendName)
                .EndRpc();
            crs.StartRpc(CachedPlayer.LocalPlayer.NetId, RpcCalls.SendChat)
                .Write(command)
                .EndRpc(); ;
            crs.StartRpc(CachedPlayer.LocalPlayer.NetId, RpcCalls.SetName)
                .Write(name)
                .EndRpc();
            crs.SendMessage();
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
            var crs = CustomRpcSender.Create(Hazel.SendOption.None);
            crs.StartRpc(target.NetId, RpcCalls.SetName, target.getClientId())
                .Write(SendName)
                .EndRpc();
            crs.StartRpc(target.NetId, RpcCalls.SendChat, target.getClientId())
                .Write(command)
                .EndRpc();
            crs.StartRpc(target.NetId, RpcCalls.SetName, target.getClientId())
                .Write(target.Data.PlayerName)
                .EndRpc();
            crs.SendMessage();
        }
    }/**
    [HarmonyPatch(typeof(ChatController),nameof(ChatController.AddChat))]
    class ChatHandler
    {
        public static bool Prefix(ChatController __instance, [HarmonyArgument(0)] ref PlayerControl sourcePlayer, [HarmonyArgument(1)] ref string chatText)
        {

            if (!(bool)(UnityEngine.Object)sourcePlayer || !(bool)(UnityEngine.Object)PlayerControl.LocalPlayer)
                return false;
            GameData.PlayerInfo data1 = CachedPlayer.LocalPlayer.Data;
            GameData.PlayerInfo data2 = sourcePlayer.Data;
            if (data2 == null || data1 == null || data2.IsDead && (!PlayerControl.LocalPlayer.isDead() || PlayerControl.LocalPlayer.isRole(CustomRPC.RoleId.NiceRedRidingHood)))
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
