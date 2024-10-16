using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Assets.CoreScripts;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using HarmonyLib;
using Hazel;
using Il2CppInterop.Generator.Extensions;
using SuperNewRoles.Helpers;
using SuperNewRoles.Mode;
using SuperNewRoles.Mode.SuperHostRoles;
using SuperNewRoles.Roles;
using SuperNewRoles.Roles.Attribute;
using SuperNewRoles.Roles.RoleBases;
using SuperNewRoles.Roles.RoleBases.Interfaces;
using SuperNewRoles.SuperNewRolesWeb;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace SuperNewRoles.Patches;

[HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.CoSpawnPlayer))]
public class AmongUsClientOnPlayerJoinedPatch
{
    public static void Postfix(PlayerPhysics __instance)
    {
        if (AmongUsClient.Instance.NetworkMode == NetworkModes.FreePlay || __instance.myPlayer.IsBot()) return;
            new LateTask(() =>
            {
                // 自分相手に送信するか。
                var isSelfSend = __instance.myPlayer.AmOwner && PlayerControlHelper.IsMod(AmongUsClient.Instance.HostId);
                // 他のプレイヤーに送信するか。
                var isOtherSend = AmongUsClient.Instance.AmHost && !__instance.myPlayer.IsMod();

                if (isSelfSend)
                    AddChatPatch.SelfSend(GetChatCommands.WelcomeToSuperNewRoles, GetChatCommands.GetWelcomeMessage());
                else if (isOtherSend)
                    AddChatPatch.SendCommand(__instance.myPlayer, GetChatCommands.GetWelcomeMessage(), GetChatCommands.WelcomeToSuperNewRoles);

                if (SuperNewRolesPlugin.IsBeta)
                {
                    new LateTask(() =>
                    {
                        if (isSelfSend)
                            AddChatPatch.SelfSend(GetChatCommands.SNRCommander, GetChatCommands.GetVersionMessage());
                        else if (isOtherSend)
                            AddChatPatch.SendCommand(__instance.myPlayer, GetChatCommands.GetVersionMessage());
                    }, 1f, "Welcome Beta Message");
                }
            }, 1f, "Welcome Message");

    }
}
[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcSendChat))]
public static class PlayerControlRpcSendChatPatch
{
    public static bool Prefix(PlayerControl __instance, string chatText, ref bool __result)
    {
        if (!AmongUsClient.Instance.AmHost)
            return true;
        chatText = Regex.Replace(chatText, "<.*?>", string.Empty);
        if (chatText.IsNullOrWhiteSpace())
            return true;
        var Commandsa = chatText.Split(" ");
        var Commandsb = new List<string>();
        foreach (string com in Commandsa) { Commandsb.AddRange(com.Split("　")); }
        var Commands = Commandsb.ToArray();
        HostManagedChatCommandPatch.CommandType commandType = HostManagedChatCommandPatch.CheckChatCommand(Commands.FirstOrDefault());
        if (commandType == HostManagedChatCommandPatch.CommandType.None)
            return true;
        // コマンドの送信をキャンセルしてコマンドを処理する
        if (AmongUsClient.Instance.AmClient && DestroyableSingleton<HudManager>.Instance)
            DestroyableSingleton<HudManager>.Instance.Chat.AddChat(__instance, chatText);
        __result = false;
        return false;
    }
}
[HarmonyPatch(typeof(ChatController), nameof(ChatController.AddChat))]
internal class AddChatPatch
{
    public static bool Prefix(PlayerControl sourcePlayer, string chatText)
    {
        if (Mode.Werewolf.Main.IsChatBlock(sourcePlayer, chatText)) return false;
        if (AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Started)
        {
            if (AmongUsClient.Instance.AmHost)
            {
                Assassin.AddChat(sourcePlayer, chatText);
                if (!Mode.BattleRoyal.SelectRoleSystem.OnAddChat(sourcePlayer, chatText)) return false;
            }
        }

        // ホスト統括制御のコマンド処理
        var Commandsa = chatText.Split(" ");
        var Commandsb = new List<string>();
        foreach (string com in Commandsa) { Commandsb.AddRange(com.Split("　")); }
        var Commands = Commandsb.ToArray();

        HostManagedChatCommandPatch.CommandType commandType = HostManagedChatCommandPatch.CheckChatCommand(Commands.FirstOrDefault());
        if (commandType != HostManagedChatCommandPatch.CommandType.None)
        {
            if (AmongUsClient.Instance.AmHost)
                HostManagedChatCommandPatch.ChatCommandExecution(sourcePlayer, commandType, Commands);
            return false;
        }

        if (ModeHandler.IsMode(ModeId.SuperHostRoles))
        {
            string lcmd = Commands[0].ToLower();
            if (sourcePlayer.GetRoleBase() is ISHRChatCommand shrcmd &&
                    ("/" + shrcmd.CommandName == lcmd ||
                    (
                       (shrcmd.Alias != null) &&
                       ("/" + shrcmd.Alias == lcmd)
                    ))
                )
            {
                bool isCancelChat;
                if (AmongUsClient.Instance.AmHost)
                {
                    if (Commands.Length > 1)
                        isCancelChat = shrcmd.OnChatCommand(Commands[1..]);
                    else
                        isCancelChat = shrcmd.OnChatCommand([]);
                }
                else
                {
                    if (Commands.Length > 1)
                        isCancelChat = shrcmd.OnChatCommandClient(Commands[1..]);
                    else
                        isCancelChat = shrcmd.OnChatCommandClient([]);
                }
                if (isCancelChat)
                    return false;
            }
            else
            {
                try
                {
                    if (lcmd == "/bt")
                    {
                        if (sourcePlayer.IsAttributeGuesser())
                        {
                            bool isCancelChat = true;
                            if (AmongUsClient.Instance.AmHost)
                            {
                                if (Commands.Length > 1)
                                    isCancelChat = AttributeGuesser.OnChatCommand(sourcePlayer, Commands[1..]);
                                else
                                    isCancelChat = AttributeGuesser.OnChatCommand(sourcePlayer, []);
                            }
                            if (isCancelChat) return false;
                        }
                    }
                }
                catch { }
                
                foreach (ISHRChatCommand shrChatCommand in RoleBaseManager.GetInterfaces<ISHRChatCommand>())
                {
                    if (
                         "/" + shrChatCommand.CommandName == lcmd ||
                           (
                              (shrChatCommand.Alias != null) &&
                              ("/" + shrChatCommand.Alias == lcmd)
                           )
                       )
                    {
                        SendCommand(sourcePlayer, ModTranslation.GetString("CommandNotFound"), GetChatCommands.SNRCommander);
                        return false;
                    }
                }
            }
        }
        bool isAdd = true;
        if (sourcePlayer.Data.PlayerName.Contains("<size") ||
            sourcePlayer.Data.PlayerName.Contains("<color"))
            isAdd = false;

        if(AmongUsClient.Instance.AmHost)
            HideChat.OnAddChat(sourcePlayer, chatText, isAdd);
        // ここまで到達したらチャットが表示できる
        return true;
    }

    static string GetChildText(List<CustomOption> options, string indent)
    {
        string text = "";
        foreach (CustomOption option in options)
        {
            if (!option.parent.Enabled && option.parent != null) continue;
            if (ModeHandler.IsMode(ModeId.SuperHostRoles, false) && !option.isSHROn) continue;

            string optionName = option.GetName();

            text += indent + option.GetName() + ":" + option.GetString() + "\n";
            var (isProcessingRequired, pattern) = GameOptionsDataPatch.ProcessingOptionCheck(option);

            if (isProcessingRequired)
                text += $"{GameOptionsDataPatch.ProcessingOptionString(option, indent, pattern)}\n";

            if (option.children.Count > 0)
            {
                if (!option.Enabled) continue;
                text += GetChildText(option.children, indent + "  ");
            }
        }
        return text;
    }
    internal static string GetOptionText(CustomRoleOption RoleOption)
    {
        Logger.Info("GetOptionText", "ChatHandler");
        string text = "";
        text += GetChildText(RoleOption.children, "  ");
        return text;
    }

    internal static string GetTeamText(TeamType type)
    {
        return type switch
        {
            TeamType.Crewmate => string.Format(ModTranslation.GetString("TeamMessage"), ModTranslation.GetString("CrewmateName")),
            TeamType.Impostor => string.Format(ModTranslation.GetString("TeamMessage"), ModTranslation.GetString("ImpostorName")),
            TeamType.Neutral => string.Format(ModTranslation.GetString("TeamMessage"), ModTranslation.GetString("NeutralName").Replace("陣営", "").Replace("阵营", "").Replace("陣營", "")),
            _ => "",
        };
    }
    static string GetText(CustomRoleOption option)
    {
        Logger.Info("GetText", "Chathandler");
        string text = "\n";
        text += GetTeamText(CustomRoles.GetRoleTeamType(option.RoleId)) + "\n";
        text += "「" + CustomRoles.GetRoleIntro(option.RoleId) + "」\n";
        text += CustomRoles.GetRoleDescription(option.RoleId) + "\n";
        text += ModTranslation.GetString("MessageSettings") + ":\n";
        text += GetOptionText(option);
        return text;
    }
    internal static void Send(PlayerControl target, string rolename, string text, float time = 0)
    {
        text = "\n" + text + "\n                                                                                                                                                                                                                                              ";
        if (time <= 0)
        {
            if (target == null)
            {
                string name = PlayerControl.LocalPlayer.GetDefaultName();
                AmongUsClient.Instance.StartCoroutine(AllSend(GetChatCommands.SNRCommander + rolename, text, name).WrapToIl2Cpp());
                return;
            }
            if (target.PlayerId != 0)
            {
                AmongUsClient.Instance.StartCoroutine(PrivateSend(target, GetChatCommands.SNRCommander + rolename, text, time).WrapToIl2Cpp());
            }
            else
            {
                string name = PlayerControl.LocalPlayer.GetDefaultName();
                PlayerControl.LocalPlayer.SetName(GetChatCommands.SNRCommander + rolename);
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
                AmongUsClient.Instance.StartCoroutine(AllSend(GetChatCommands.SNRCommander + rolename, text, name, time).WrapToIl2Cpp());
                return;
            }
            if (target.PlayerId != 0)
            {
                AmongUsClient.Instance.StartCoroutine(PrivateSend(target, GetChatCommands.SNRCommander + rolename, text, time).WrapToIl2Cpp());
            }
            else
            {
                new LateTask(() =>
                {
                    PlayerControl.LocalPlayer.SetName(GetChatCommands.SNRCommander + rolename);
                    FastDestroyableSingleton<HudManager>.Instance.Chat.AddChat(PlayerControl.LocalPlayer, text);
                    PlayerControl.LocalPlayer.SetName(name);
                }, time, "Set SNR Name");
            }
            return;
        }
    }
    public static void SendCommand(PlayerControl target, string command, string SendName = "NONE")
    {
        if (!AmongUsClient.Instance.AmHost) return;
        if (SendName == "NONE") SendName = GetChatCommands.SNRCommander;
        command = $"\n{command}\n";
        if (target != null && target.Data.Disconnected) return;
        if (target == null)
        {
            string name = CachedPlayer.LocalPlayer.Data.PlayerName;
            if (name == GetChatCommands.SNRCommander) return;
            if (AmongUsClient.Instance.AmHost && ModeHandler.IsMode(ModeId.SuperHostRoles) && HideChat.HideChatEnabled && AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Started)
            {
                foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                {
                    if (player.PlayerId != PlayerControl.LocalPlayer.PlayerId)
                    {
                        AmongUsClient.Instance.StartCoroutine(PrivateSend(player, SendName, command).WrapToIl2Cpp());
                        continue;
                    }
                    string tname = player.Data.PlayerName;
                    player.SetName(SendName);
                    FastDestroyableSingleton<HudManager>.Instance.Chat.AddChat(player, command);
                    player.SetName(tname);
                }
            } else
            {
                AmongUsClient.Instance.StartCoroutine(AllSend(SendName, command, name).WrapToIl2Cpp());
            }
            return;
        }
        else if (target.PlayerId == PlayerControl.LocalPlayer.PlayerId)
        {
            string name = target.Data.PlayerName;
            target.SetName(SendName);
            FastDestroyableSingleton<HudManager>.Instance.Chat.AddChat(target, command);
            target.SetName(name);
        }
        else
        {
            AmongUsClient.Instance.StartCoroutine(PrivateSend(target, SendName, command).WrapToIl2Cpp());
        }
    }

    public static void SendChat(PlayerControl target, string text, string title = "")
    {
        if (title != null && title != "") text = $"<size=100%><color=#ff8000>【{title}】</color></size>\n{text}";
        SendCommand(target, "", text);
    }

    /// <summary>
    /// システムメッセージ等を送信する。
    /// </summary>
    /// <param name="target">送信先</param>
    /// <param name="infoName">情報タイトル(名前で表記)</param>
    /// <param name="infoContents">情報本文(チャットで表記)</param>
    /// <param name="color">文字色, 16進数のcolorコードで指定([#FFFFFF]等)</param>
    /// <param name="isSendFromGuest">ゲストが自分自身に送信可能にする</param>
    public static void ChatInformation(PlayerControl target, string infoName, string infoContents, string color = "white", bool isSendFromGuest = false)
    {
        if (target == null) return;

        string line = "|--------------------------------------------------------|";
        string name = $"<size=90%><color={color}><align={"left"}>{line}\n{infoName} {ModTranslation.GetString("InformationName")}\n{line}</align></color></size>";
        string contents = $"\n<align={"left"}>{infoContents}</align>\n　\n";

        if (target == PlayerControl.LocalPlayer && AmongUsClient.Instance.AmHost || isSendFromGuest) SelfSend(name, contents);
        else if (AmongUsClient.Instance.AmHost) SendCommand(target, contents, name);
    }

    /// <summary>
    /// 自分自身にチャットを送信する (或いは, +25解除時の代替RPCの受信部)
    /// </summary>
    /// <param name="titelName">名前に表示する部分</param>
    /// <param name="text">チャットの内容</param>
    public static void SelfSend(string titelName, string text)
    {
        if (PlayerControl.LocalPlayer == null) return;

        var localPlayer = PlayerControl.LocalPlayer;
        var originalName = localPlayer.Data.PlayerName;
        const string blank = "<color=#00000000>.</color>\n";
        var contents = $"{blank}{blank}{text}{blank}";

        localPlayer.SetName(titelName);
        FastDestroyableSingleton<HudManager>.Instance?.Chat?.AddChat(localPlayer, contents, false);
        localPlayer.SetName(originalName);
    }
    static IEnumerator AllSend(string SendName, string command, string name, float time = 0)
    {
        if (time > 0)
        {
            yield return new WaitForSeconds(time);
        }
        var crs = CustomRpcSender.Create("AllSend");
        var sender = PlayerControl.LocalPlayer;
        if (sender.Data.IsDead)
        {
            sender = PlayerControl.AllPlayerControls.ToArray().Where(x => x != null && !x.Data.Disconnected && !x.Data.IsDead).OrderBy(x => x.PlayerId).FirstOrDefault();
            name = sender.GetDefaultName();
        }
        crs.AutoStartRpc(sender.NetId, (byte)RpcCalls.SetName)
            .Write(sender.Data.NetId)
            .Write(SendName)
            .EndRpc()
            .AutoStartRpc(sender.NetId, (byte)RpcCalls.SendChat)
            .Write(command)
            .EndRpc()
            .AutoStartRpc(sender.NetId, (byte)RpcCalls.SetName)
            .Write(sender.Data.NetId)
            .Write(name)
            .EndRpc();
        crs.SendMessage();
        sender.SetName(SendName);
        FastDestroyableSingleton<HudManager>.Instance.Chat.AddChat(sender, command);
        sender.SetName(name);
    }
    static IEnumerator PrivateSend(PlayerControl target, string SendName, string command, float time = 0)
    {
        if (time > 0)
        {
            yield return new WaitForSeconds(time);
        }
        var crs = CustomRpcSender.Create("PrivateSend");
        crs.AutoStartRpc(target.NetId, (byte)RpcCalls.SetName, target.GetClientId())
            .Write(target.Data.NetId)
            .Write(SendName)
            .EndRpc()
            .AutoStartRpc(target.NetId, (byte)RpcCalls.SendChat, target.GetClientId())
            .Write(command)
            .EndRpc()
            .AutoStartRpc(target.NetId, (byte)RpcCalls.SetName, target.GetClientId())
            .Write(target.Data.NetId)
            .Write(target.Data.PlayerName)
            .EndRpc()
            .SendMessage();
    }
}