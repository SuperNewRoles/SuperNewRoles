using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BepInEx.IL2CPP.Utils;
using HarmonyLib;
using SuperNewRoles.Mode;
using SuperNewRoles.Mode.SuperHostRoles;
using SuperNewRoles.Roles;
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
                if (!__instance.myPlayer.IsBot())
                {
                    AddChatPatch.SendCommand(__instance.myPlayer, text, AddChatPatch.WelcomeToSuperNewRoles);
                }
            }, 1f, "Welcome Message");
            if (SuperNewRolesPlugin.IsBeta)
            {
                string betaText =
                    ModTranslation.GetString("betatext1") +
                    ModTranslation.GetString("betatext2") +
                    $"\nBranch: {ThisAssembly.Git.Branch}" +
                    $"\nCommitId: {ThisAssembly.Git.Commit}" +
                    " " + "\n.";
                new LateTask(() =>
                {
                    if (!__instance.myPlayer.IsBot())
                    {
                        AddChatPatch.SendCommand(__instance.myPlayer, $" {SuperNewRolesPlugin.ModName} v{SuperNewRolesPlugin.VersionString}\nCreate by ykundesu{betaText}");
                    }
                }, 2f, "Welcome Beta Message");
            }
        }
    }
}
[HarmonyPatch(typeof(ChatController), nameof(ChatController.AddChat))]
class AddChatPatch
{
    static readonly string SNRCommander = $"<size=200%>{SuperNewRolesPlugin.ColorModName}</size>";
    public static string WelcomeToSuperNewRoles = $"<size=150%>Welcome To {SuperNewRolesPlugin.ColorModName}</size>";

    public static bool Prefix(PlayerControl sourcePlayer, string chatText)
    {
        if (Mode.Werewolf.Main.IsChatBlock(sourcePlayer, chatText)) return false;
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
                betatext = ModTranslation.GetString("betatext1");
                betatext += ModTranslation.GetString("betatext2");
                betatext += $"\nBranch: {ThisAssembly.Git.Branch}";
                betatext += $"\nCommitId: {ThisAssembly.Git.Commit}";
            }
            PlayerControl sendPlayer;
            if (sourcePlayer.AmOwner) sendPlayer = null;
            else sendPlayer = sourcePlayer;
            SendCommand(sendPlayer, $" {SuperNewRolesPlugin.ModName} v{SuperNewRolesPlugin.VersionString}\nCreate by ykundesu{betatext}");
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
                ModTranslation.GetString("CommandsMessage9") + "\n" +
                ModTranslation.GetString("CommandsMessage10");
            SendCommand(sourcePlayer, text);
            return false;
        }
        else if (
            Commands[0].Equals("/Discord", StringComparison.OrdinalIgnoreCase) ||
            Commands[0].Equals("/dc", StringComparison.OrdinalIgnoreCase)
            )
        {
            SendCommand(sourcePlayer, ModTranslation.GetString("SNROfficialDiscordMessage") + "\n" + SuperNewRolesPlugin.DiscordServer);
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
                Logger.Info("Length==1", "/ar");
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
                Logger.Info("Length!=1", "/ar");
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
        else if (
            Commands[0].Equals("/Winners", StringComparison.OrdinalIgnoreCase) ||
            Commands[0].Equals("/w", StringComparison.OrdinalIgnoreCase)
            )
        {
            PlayerControl target = sourcePlayer.AmOwner ? null : sourcePlayer;
            if (OnGameEndPatch.PlayerData == null)
            {
                SendCommand(target, ModTranslation.GetString("WinnersNoneData"), SNRCommander);
                return false;
            }
            StringBuilder builder = new();
            foreach (var data in OnGameEndPatch.PlayerData)
            {
                if (data.IsWin) builder.Append("★");
                else builder.Append("　");
                builder.Append(data.name);
                builder.Append($"({data.CompleteTask}/{data.TotalTask})");
                builder.Append(" : ");
                builder.Append(ModTranslation.GetString($"FinalStatus{data.finalStatus}"));
                builder.Append(" : ");
                if (data.role == null)
                    builder.Append(ModTranslation.GetString("WinnerGetError"));
                else
                    builder.Append(ModTranslation.GetString(IntroData.GetIntroData((RoleId)data.role).NameKey + "Name"));
                builder.AppendLine();
            }
            SendCommand(target, builder.ToString(), $"<size=200%>{OnGameEndPatch.WinText}</size>");
            return false;
        }
        else
        {
            return true;
        }
    }
    static string GetChildText(List<CustomOption> options, string indent)
    {
        string text = "";
        foreach (CustomOption option in options)
        {
            text += indent + option.GetName() + ":" + option.GetString() + "\n";
            if (option.children.Count > 0)
            {
                text += GetChildText(option.children, indent + "  ");
            }
        }
        return text;
    }
    static string GetOptionText(CustomRoleOption RoleOption, IntroData intro)
    {
        Logger.Info("GetOptionText", "ChatHandler");
        string text = "";
        text += GetChildText(RoleOption.children, "  ").Replace("<color=#03ff0c>", "").Replace("<color=#f22f21>", "").Replace("</color>", "");
        return text;
    }
    static string GetTeamText(TeamRoleType type)
    {
        return type switch
        {
            TeamRoleType.Crewmate => ModTranslation.GetString("CrewmateName"),
            TeamRoleType.Impostor => ModTranslation.GetString("ImpostorName"),
            TeamRoleType.Neutral => ModTranslation.GetString("NeutralName").Replace("陣営", ""),
            _ => "",
        };
    }
    static string GetText(CustomRoleOption option)
    {
        Logger.Info("GetText", "Chathandler");
        string text = "\n";
        IntroData intro = option.Intro;
        text += GetTeamText(intro.Team) + ModTranslation.GetString("Team") + "\n";
        text += "「" + IntroData.GetTitle(intro.NameKey, intro.TitleNum) + "」\n";
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
            foreach (CustomOption opt in option.children)
            {
                if (opt.GetName() == CustomOptionHolder.SheriffPlayerCount.GetName())
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
        if (!(ModeHandler.IsMode(ModeId.Default, false) || ModeHandler.IsMode(ModeId.SuperHostRoles, false) || ModeHandler.IsMode(ModeId.Werewolf, false)))
        {
            SendCommand(target, ModTranslation.GetString("Notassign"));
            return;
        }
        List<CustomRoleOption> EnableOptions = new();
        foreach (CustomRoleOption option in CustomRoleOption.RoleOptions)
        {
            if (!option.IsRoleEnable) continue;
            if (ModeHandler.IsMode(ModeId.SuperHostRoles, false) && !option.isSHROn) continue;
            EnableOptions.Add(option);
        }
        float time = 0;
        foreach (CustomRoleOption option in EnableOptions)
        {
            string text = GetText(option);
            string rolename = "<size=115%>\n" + CustomOptionHolder.Cs(option.Intro.color, option.Intro.NameKey + "Name") + "</size>";
            SuperNewRolesPlugin.Logger.LogInfo(text);
            Send(target, rolename, text, time);
            time += SendTime;
        }
    }
    static void GetInRoleCommand(PlayerControl target = null)
    {
        if (!AmongUsClient.Instance.AmHost) return;
        if (!(ModeHandler.IsMode(ModeId.Default, false) || ModeHandler.IsMode(ModeId.SuperHostRoles, false) || ModeHandler.IsMode(ModeId.Werewolf, false)))
        {
            SendCommand(target, ModTranslation.GetString("Notassign"));
            return;
        }
        List<CustomRoleOption> EnableOptions = new();
        foreach (CustomRoleOption option in CustomRoleOption.RoleOptions)
        {
            if (!option.IsRoleEnable) continue;
            if (ModeHandler.IsMode(ModeId.SuperHostRoles, false) && !option.isSHROn) continue;
            EnableOptions.Add(option);
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
                }, time, "Set SNR Name");
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
        var crs = CustomRpcSender.Create("AllSend");
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