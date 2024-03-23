using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using HarmonyLib;
using Hazel;
using SuperNewRoles.Helpers;
using SuperNewRoles.Mode;
using SuperNewRoles.Roles;
using SuperNewRoles.Roles.RoleBases;
using SuperNewRoles.SuperNewRolesWeb;
using UnityEngine;
using static System.String;

namespace SuperNewRoles.Patches;

[HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.CoSpawnPlayer))]
public class AmongUsClientOnPlayerJoinedPatch
{
    public static void Postfix(PlayerPhysics __instance)
    {
        if (AmongUsClient.Instance.AmHost && AmongUsClient.Instance.NetworkMode != NetworkModes.FreePlay)
        {
            new LateTask(() =>
            {
                if (!__instance.myPlayer.IsBot())
                {
                    AddChatPatch.SendCommand(__instance.myPlayer, GetChatCommands.GetWelcomeMessage(), GetChatCommands.WelcomeToSuperNewRoles);
                }
            }, 1f, "Welcome Message");

            if (SuperNewRolesPlugin.IsBeta)
            {
                new LateTask(() =>
                {
                    if (!__instance.myPlayer.IsBot())
                    {
                        AddChatPatch.SendCommand(__instance.myPlayer, GetChatCommands.GetVersionMessage());
                    }
                }, 2f, "Welcome Beta Message");
            }
        }
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

        HostManagedChatCommandPatch.CommandType commandType = HostManagedChatCommandPatch.CheckChatCommand(Commands[0]);
        if (commandType != HostManagedChatCommandPatch.CommandType.None)
        {
            if (AmongUsClient.Instance.AmHost) HostManagedChatCommandPatch.ChatCommandExecution(sourcePlayer, commandType, Commands);
            return false;
        }

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
            TeamType.Crewmate => Format(ModTranslation.GetString("TeamMessage"), ModTranslation.GetString("CrewmateName")),
            TeamType.Impostor => Format(ModTranslation.GetString("TeamMessage"), ModTranslation.GetString("ImpostorName")),
            TeamType.Neutral => Format(ModTranslation.GetString("TeamMessage"), ModTranslation.GetString("NeutralName").Replace("陣営", "").Replace("阵营", "").Replace("陣營", "")),
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
            AmongUsClient.Instance.StartCoroutine(AllSend(SendName, command, name).WrapToIl2Cpp());
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
            AmongUsClient.Instance.StartCoroutine(PrivateSend(target, SendName, command).WrapToIl2Cpp());
        }
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
        string line = "|--------------------------------------------------------|";
        string name = $"<size=90%><color={color}><align={"left"}>{line}\n{infoName} {ModTranslation.GetString("InformationName")}\n{line}</align></color></size>";
        string contents = $"\n<align={"left"}>{infoContents}</align>\n　\n";

        if (AmongUsClient.Instance.AmHost)
        {
            SendCommand(target, contents, name);
        }
        else if (isSendFromGuest && target == PlayerControl.LocalPlayer)
        {
            string originalName = target.Data.PlayerName;
            contents = $"\n{contents}\n";

            target.SetName(name);
            FastDestroyableSingleton<HudManager>.Instance.Chat.AddChat(target, contents, false);
            target.SetName(originalName);
        }
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
            .Write(SendName)
            .EndRpc()
            .AutoStartRpc(sender.NetId, (byte)RpcCalls.SendChat)
            .Write(command)
            .EndRpc()
            .AutoStartRpc(sender.NetId, (byte)RpcCalls.SetName)
            .Write(name)
            .EndRpc()
            .SendMessage();
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
}