using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using HarmonyLib;
using Hazel;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using SuperNewRoles;
using SuperNewRoles.CustomCosmetics;
using SuperNewRoles.Helpers;
using SuperNewRoles.Mode;
using SuperNewRoles.Mode.SuperHostRoles;
using SuperNewRoles.Replay;
using SuperNewRoles.Replay.ReplayActions;
using SuperNewRoles.Roles;
using SuperNewRoles.Roles.Crewmate;
using SuperNewRoles.Roles.Impostor;
using SuperNewRoles.Roles.Impostor.MadRole;
using SuperNewRoles.Roles.Neutral;
using SuperNewRoles.Roles.RoleBases;
using SuperNewRoles.SuperNewRolesWeb;
using UnityEngine;
using static MeetingHud;

namespace SuperNewRoles.Patches;

[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Awake))] class AwakeMeetingPatch { public static void Postfix() => RoleClass.IsMeeting = true; }
[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.CastVote))]
class CastVotePatch
{
    /// <summary>
    /// 投票が有効であるか無効であるかを取得し, 無効ならば投票を反映せず, 投票者の投票権を復活させる。
    /// (ホストのみが行う処理)
    /// </summary>
    /// <param name="srcPlayerId">投票したプレイヤーのPlayerId</param>
    /// <param name="suspectPlayerId">投票先のPlayerId</param>
    /// <param name="__instance"></param>
    /// <returns>true : 投票を有効票として扱う / false : 投票を無効票として扱う </returns>
    public static bool Prefix(byte srcPlayerId, byte suspectPlayerId, MeetingHud __instance)
    {
        PlayerControl srcPlayer = ModHelpers.GetPlayerControl(srcPlayerId);
        PlayerControl suspectPlayer = ModHelpers.GetPlayerControl(suspectPlayerId);

        bool IsValidVote = true; // 投票が有効になるかを一時的に保存する

        RoleId srcPlayerRole = srcPlayer.GetRole();
        switch (srcPlayerRole)
        {
            case RoleId.PoliceSurgeon:
                IsValidVote = PostMortemCertificate_Display.MeetingHudCastVote_Prefix(srcPlayerId, suspectPlayerId);
                break;
            case RoleId.Crook:
                IsValidVote = Crook.Ability.InHostMode.MeetingHudCastVote_Prefix(srcPlayerId, suspectPlayerId);
                break;
        }

        if (IsValidVote) // 有効票であれば,
        {
            return true; // そのまま通す。
        }
        else // 無効票であれば,
        {
            __instance.RpcClearVote(srcPlayer.GetClientId()); // 投票を解除し,
            Logger.Info($"{srcPlayer.name}({srcPlayerRole}) の 投票を無効化しました。 (投票先 : {(suspectPlayer != null ? $"{suspectPlayer.name}({suspectPlayer.GetRole()})" : "投票者無し")}", "Vote Void"); // ログに記載し,
            return false; // 無効化する。
        }
    }
}
[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.RpcVotingComplete))]
class RpcVotingComplete
{
    public static void Postfix(MeetingHud __instance, [HarmonyArgument(0)] Il2CppStructArray<VoterState> states, [HarmonyArgument(1)] ref GameData.PlayerInfo exiled, [HarmonyArgument(2)] bool tie)
    {
        if (AmongUsClient.Instance.AmHost) ReplayActionVotingComplete.Create(states, exiled is null ? (byte)255 : exiled.PlayerId, tie);
    }
}
[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.VotingComplete))]
class VotingComplete
{
    public static void Prefix(MeetingHud __instance, [HarmonyArgument(0)] Il2CppStructArray<VoterState> states, [HarmonyArgument(1)] ref GameData.PlayerInfo exiled, [HarmonyArgument(2)] bool tie)
    {
        if (exiled != null && exiled.Object.IsBot() && RoleClass.Assassin.TriggerPlayer == null && Main.RealExiled == null)
        {
            exiled = null;
        }
        if (tie && Balancer.currentAbilityUser != null)
        {
            Balancer.IsDoubleExile = true;
        }
        if (!AmongUsClient.Instance.AmHost) ReplayActionVotingComplete.Create(states, exiled is null ? (byte)255 : exiled.PlayerId, tie);
    }
}
[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.VotingComplete))]
class VotingComplatePatch
{
    public static void Postfix(MeetingHud __instance, Il2CppStructArray<VoterState> states, GameData.PlayerInfo exiled, bool tie)
    {
        new GameHistoryManager.MeetingHistory(states, exiled);
    }
}
[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.CheckForEndVoting))]
class CheckForEndVotingPatch
{
    // Key:役職　Value:票数
    public static Dictionary<RoleId, int> VoteCountDictionary => new() {
            { RoleId.Mayor, RoleClass.Mayor.AddVote },
            { RoleId.MadMayor, RoleClass.MadMayor.AddVote },
            { RoleId.MayorFriends, RoleClass.MayorFriends.AddVote },
            { RoleId.Dictator, RoleClass.Dictator.VoteCount }
        };

    private static PlayerControl ChangeNameExiledPlayer = null;

    public static bool Prefix(MeetingHud __instance)
    {
        try
        {

            if (!AmongUsClient.Instance.AmHost) return true;
            if (ModeHandler.IsMode(ModeId.Detective) && Mode.Detective.Main.IsNotDetectiveVote)
            {
                foreach (var ps in __instance.playerStates)
                {
                    if (ps.TargetPlayerId == Mode.Detective.Main.DetectivePlayer.PlayerId && !ps.DidVote)
                    {
                        return false;
                    }
                    else if (ps.TargetPlayerId == Mode.Detective.Main.DetectivePlayer.PlayerId && ps.DidVote)
                    {
                        VoterState[] statesdetective;
                        GameData.PlayerInfo exiledPlayerdetective = CachedPlayer.LocalPlayer.Data;
                        bool tiedetective = false;

                        List<VoterState> statesListdetective = new();
                        if (ps.VotedFor != ps.TargetPlayerId)
                        {
                            statesListdetective.Add(new VoterState()
                            {
                                VoterId = ps.TargetPlayerId,
                                VotedForId = ps.VotedFor
                            });
                            statesdetective = statesListdetective.ToArray();

                            var VotingDatadetective = __instance.CustomCalculateVotes();

                            exiledPlayerdetective = GameData.Instance.AllPlayers.ToArray().FirstOrDefault(info => !tiedetective && info.PlayerId == ps.VotedFor);

                            __instance.RpcVotingComplete(statesdetective, exiledPlayerdetective, tiedetective); //RPC
                        }
                        else
                        {
                            statesListdetective.Add(new VoterState()
                            {
                                VoterId = ps.TargetPlayerId,
                                VotedForId = 253
                            });
                            statesdetective = statesListdetective.ToArray();

                            var VotingDatadetective = __instance.CustomCalculateVotes();
                            exiledPlayerdetective = GameData.Instance.AllPlayers.ToArray().FirstOrDefault(info => !tiedetective && info.PlayerId == 253);

                            __instance.RpcVotingComplete(statesdetective, exiledPlayerdetective, tiedetective); //RPC
                        }
                        return false;
                    }
                }
            }
            else if (ModeHandler.IsMode(ModeId.BattleRoyal))
            {
                int votingTime = GameOptionsManager.Instance.CurrentGameOptions.GetInt(Int32OptionNames.VotingTime);
                float num4 = __instance.discussionTimer - GameOptionsManager.Instance.CurrentGameOptions.GetInt(Int32OptionNames.DiscussionTime);
                if (votingTime > 0 && num4 >= (float)votingTime)
                {
                    __instance.discussionTimer = 0;
                    Mode.BattleRoyal.SelectRoleSystem.OnEndSetRole();
                }
                return false;
            }
            else if (RoleClass.Assassin.TriggerPlayer != null)
            {
                var (isVoteEnd, voteFor, voteArea) = AssassinVoteState(__instance);

                SuperNewRolesPlugin.Logger.LogInfo(isVoteEnd + "、" + voteFor);
                if (isVoteEnd)
                {
                    //GameData.PlayerInfo exiled = Helper.Player.GetPlayerControlById(voteFor).Data;
                    Il2CppStructArray<MeetingHud.VoterState> array =
                        new(
                            __instance.playerStates.Length);

                    for (int i = 0; i < __instance.playerStates.Length; i++)
                    {
                        PlayerVoteArea playerVoteArea = __instance.playerStates[i];
                        playerVoteArea.VotedFor = playerVoteArea.TargetPlayerId == RoleClass.Assassin.TriggerPlayer.PlayerId ? voteFor : (byte)254;
                        __instance.SetDirtyBit(1U);

                        array[i] = new VoterState
                        {
                            VoterId = playerVoteArea.TargetPlayerId,
                            VotedForId = playerVoteArea.VotedFor
                        };
                    }
                    GameData.PlayerInfo target = GameData.Instance.GetPlayerById(voteFor);
                    GameData.PlayerInfo exileplayer = null;
                    if (target != null && target.Object.PlayerId != RoleClass.Assassin.TriggerPlayer.PlayerId && !target.Object.IsBot())
                    {
                        var outfit = target.DefaultOutfit;
                        exileplayer = target;
                        PlayerControl exile = null;
                        Main.RealExiled = target.Object;
                        if (ModeHandler.IsMode(ModeId.SuperHostRoles))
                        {
                            foreach (PlayerControl p in BotManager.AllBots)
                            {
                                if (p.IsDead())
                                {
                                    exileplayer = p.Data;
                                    exile = p;
                                    p.RpcSetColor((byte)outfit.ColorId);
                                    p.RpcSetName(target.Object.GetDefaultName() +
                                        ModTranslation.GetString(target.Object.IsRole(RoleId.Marlin) ?
                                        "AssassinSucsess" :
                                        "AssassinFail")
                                        + "<size=0%>");
                                    p.RpcSetHat(outfit.HatId);
                                    p.RpcSetVisor(outfit.VisorId);
                                    p.RpcSetSkin(outfit.SkinId);
                                    break;
                                }
                            }
                        }
                        RoleClass.Assassin.MeetingEndPlayers.Add(RoleClass.Assassin.TriggerPlayer.PlayerId);
                        if (target.Object.IsRole(RoleId.Marlin))
                            RoleClass.Assassin.IsImpostorWin = true;
                        else
                            RoleClass.Assassin.DeadPlayer = RoleClass.Assassin.TriggerPlayer;
                        new LateTask(() =>
                        {
                            if (exile != null)
                            {
                                exile.RpcSetName(exile.GetDefaultName());
                                exile.RpcSetColor(1);
                                exile.RpcSetHat("hat_NoHat");
                                exile.RpcSetPet("peet_EmptyPet");
                                exile.RpcSetVisor("visor_EmptyVisor");
                                exile.RpcSetNamePlate("nameplate_NoPlate");
                                exile.RpcSetSkin("skin_None");
                            }
                        }, 5f, "Assassin Skin Set");
                    }
                    new LateTask(() => __instance.RpcVotingComplete(array, exileplayer, true), 0.2f, "Assassin Rpc Voting Comp");
                }
                return false;
            }
            else if (RoleClass.Revolutionist.MeetingTrigger != null)
            {
                var (isVoteEnd, voteFor, voteArea) = RevolutionistVoteState(__instance);

                SuperNewRolesPlugin.Logger.LogInfo(isVoteEnd + "、" + voteFor);
                if (isVoteEnd)
                {
                    //GameData.PlayerInfo exiled = Helper.Player.GetPlayerControlById(voteFor).Data;
                    Il2CppStructArray<MeetingHud.VoterState> array =
                        new(
                            __instance.playerStates.Length);

                    for (int i = 0; i < __instance.playerStates.Length; i++)
                    {
                        PlayerVoteArea playerVoteArea = __instance.playerStates[i];
                        playerVoteArea.VotedFor = playerVoteArea.TargetPlayerId == RoleClass.Revolutionist.MeetingTrigger.PlayerId ? voteFor : (byte)254;
                        __instance.SetDirtyBit(1U);

                        array[i] = new VoterState
                        {
                            VoterId = playerVoteArea.TargetPlayerId,
                            VotedForId = playerVoteArea.VotedFor
                        };
                    }
                    GameData.PlayerInfo target = GameData.Instance.GetPlayerById(voteFor);
                    GameData.PlayerInfo exileplayer = null;
                    if (target != null && target.Object.PlayerId != RoleClass.Revolutionist.MeetingTrigger.PlayerId && !target.Object.IsBot())
                    {
                        var outfit = target.DefaultOutfit;
                        exileplayer = target;
                        if (target.Object.IsRole(RoleId.Dictator))
                            RoleClass.Revolutionist.WinPlayer = RoleClass.Revolutionist.MeetingTrigger;
                    }
                    new LateTask(() => __instance.RpcVotingComplete(array, exileplayer, true), 0.2f, "Revolutionist Rpc Voting Comp");
                }
                return false;
            }
            else
            {
                foreach (var ps in __instance.playerStates)
                {
                    if (!(ps.AmDead || ps.DidVote) && ModHelpers.PlayerById(ps.TargetPlayerId) != null && !ModHelpers.PlayerById(ps.TargetPlayerId).IsBot())//死んでいないプレイヤーが投票していない
                        return false;
                }
            }
            VoterState[] states;
            GameData.PlayerInfo exiledPlayer = CachedPlayer.LocalPlayer.Data;
            bool tie = false;

            List<VoterState> statesList = new();
            for (var i = 0; i < __instance.playerStates.Length; i++)
            {
                PlayerVoteArea ps = __instance.playerStates[i];
                if (AmongUsClient.Instance.NetworkMode != NetworkModes.FreePlay || ps.TargetPlayerId == CachedPlayer.LocalPlayer.PlayerId)
                {
                    if (!ModeHandler.IsMode(ModeId.BattleRoyal))
                    {
                        if (ps == null) continue;
                        var voter = ModHelpers.PlayerById(ps.TargetPlayerId);
                        if (voter == null || voter.Data == null || voter.Data.Disconnected || voter.IsBot() || voter.IsDead() || ModHelpers.PlayerById(ps.TargetPlayerId).IsRole(RoleId.Neet)) continue;

                        //バランサー処理
                        if (Balancer.currentAbilityUser != null)
                        {
                            if (ps.VotedFor != Balancer.targetplayerright.PlayerId &&
                                ps.VotedFor != Balancer.targetplayerleft.PlayerId)
                            {
                                ps.VotedFor = ModHelpers.GetRandom(new byte[2] { Balancer.targetplayerright.PlayerId, Balancer.targetplayerleft.PlayerId });
                            }
                        }

                        //BOT・ニートならスキップ判定
                        if ((ps.VotedFor != 253 && ps.VotedFor != 254 && ModHelpers.PlayerById(ps.VotedFor).IsBot()) || ModHelpers.PlayerById(ps.TargetPlayerId).IsRole(RoleId.Neet))
                        {
                            ps.VotedFor = 253;
                        }

                        statesList.Add(new VoterState()
                        {
                            VoterId = ps.TargetPlayerId,
                            VotedForId = ps.VotedFor
                        });

                        if (VoteCountDictionary.ContainsKey(ModHelpers.PlayerById(ps.TargetPlayerId).GetRole()))
                        {
                            for (var i2 = 0; i2 < VoteCountDictionary[ModHelpers.PlayerById(ps.TargetPlayerId).GetRole()] - 1; i2++)
                            {
                                statesList.Add(new VoterState()
                                {
                                    VoterId = ps.TargetPlayerId,
                                    VotedForId = ps.VotedFor
                                });
                            }
                        }
                    }
                }
            }
            states = statesList.ToArray();

            var VotingData = __instance.CustomCalculateVotes();
            byte exileId = byte.MaxValue;
            int max = 0;
            foreach (var data in VotingData)
            {
                if (data.Value > max)
                {
                    exileId = data.Key;
                    max = data.Value;
                    tie = false;
                }
                else if (data.Value == max)
                {
                    exileId = byte.MaxValue;
                    tie = true;
                }
            }

            exiledPlayer = GameData.Instance.AllPlayers.ToArray().FirstOrDefault(info => !tie && info.PlayerId == exileId);

            if (ModeHandler.IsMode(ModeId.SuperHostRoles))
            {
                if (exiledPlayer != null && exiledPlayer.Object.IsRole(RoleId.Assassin))
                {
                    Main.RealExiled = exiledPlayer.Object;
                    PlayerControl exile = null;
                    PlayerControl defaultexile = exiledPlayer.Object;
                    var outfit = defaultexile.Data.DefaultOutfit;
                    foreach (PlayerControl p in BotManager.AllBots)
                    {
                        if (p.IsDead())
                        {
                            exiledPlayer = p.Data;
                            exile = p;
                            exile.RpcSetColor((byte)outfit.ColorId);
                            exile.RpcSetName(defaultexile.GetDefaultName());
                            exile.RpcSetHat(outfit.HatId);
                            exile.RpcSetVisor(outfit.VisorId);
                            exile.RpcSetSkin(outfit.SkinId);
                            break;
                        }
                    }
                    new LateTask(() =>
                    {
                        if (exile != null)
                        {
                            exile.RpcSetName(exile.GetDefaultName());
                            exile.RpcSetColor(1);
                            exile.RpcSetHat("hat_NoHat");
                            exile.RpcSetPet("peet_EmptyPet");
                            exile.RpcSetVisor("visor_EmptyVisor");
                            exile.RpcSetNamePlate("nameplate_NoPlate");
                            exile.RpcSetSkin("skin_None");
                        }
                    }, 5f, "Assissn Set Skin SHR");
                }

                bool isBakeryAlive = Bakery.BakeryAlive(); // パン屋 生存判定
                (bool, string) isCrookGetInsure = Crook.Ability.GetIsReceivedTheInsuranceAndAnnounce(); // 詐欺師 保険金受給判定
                bool isUseConfirmImpostorSecondText = false; // 2つ目の追放テキストとして記載する内容はあるか?
                StringBuilder changeStringBuilder = new(); // 変更する文字を, 一時的に保管する。

                if (isBakeryAlive) // パン屋が生存しているならば
                {
                    string confirmImpostorSecondText = $"{Bakery.GetExileText()}\n";
                    changeStringBuilder.AppendLine(Bakery.GetExileText());
                    isUseConfirmImpostorSecondText = true;
                }
                if (isCrookGetInsure.Item1) // 詐欺師が保険金を受け取ったのならば
                {
                    string confirmImpostorSecondText = isCrookGetInsure.Item2;
                    changeStringBuilder.AppendLine(confirmImpostorSecondText);
                    isUseConfirmImpostorSecondText = true;
                }

                if (isUseConfirmImpostorSecondText) // 2つ目の追放テキストが必要なら
                {
                    const string exileText = "<size=300%>{0}</size>\n";
                    string confirmImpostorSecondText = $"<size=300%>{changeStringBuilder}</size><size=0%>";

                    if (exiledPlayer == null)
                    {
                        string name = RoleSelectHandler.ConfirmImpostorSecondTextBot.GetDefaultName();

                        exiledPlayer = RoleSelectHandler.ConfirmImpostorSecondTextBot.Data;
                        ChangeNameExiledPlayer = RoleSelectHandler.ConfirmImpostorSecondTextBot;
                        foreach (PlayerControl p2 in CachedPlayer.AllPlayers)
                        {
                            if (!p2.IsBot() && !p2.Data.Disconnected && !p2.IsMod())
                            {
                                RoleSelectHandler.ConfirmImpostorSecondTextBot.RpcSetNamePrivate(string.Format(exileText, FastDestroyableSingleton<TranslationController>.Instance.GetString(StringNames.NoExileSkip)) + confirmImpostorSecondText, p2);
                            }
                        }
                    }
                    else if (!exiledPlayer.Object.IsBot())
                    {
                        ChangeNameExiledPlayer = exiledPlayer.Object;
                        bool isConfirmImpostor = GameOptionsManager.Instance.CurrentGameOptions.GetBool(BoolOptionNames.ConfirmImpostor);

                        foreach (PlayerControl p2 in CachedPlayer.AllPlayers)
                        {
                            if (!p2.IsBot() && !p2.Data.Disconnected && !p2.IsMod())
                            {
                                if (!isConfirmImpostor)
                                {
                                    string playerExiledText = string.Format(exileText, FastDestroyableSingleton<TranslationController>.Instance.GetString(StringNames.ExileTextNonConfirm));
                                    exiledPlayer.Object.RpcSetNamePrivate(string.Format(playerExiledText, exiledPlayer.Object.GetDefaultName()) + confirmImpostorSecondText, p2);
                                }
                                else
                                {
                                    string playerExiledText =
                                        p2.IsImpostor()
                                                ? string.Format(exileText, FastDestroyableSingleton<TranslationController>.Instance.GetString(StringNames.ExileTextPP))
                                                : string.Format(exileText, FastDestroyableSingleton<TranslationController>.Instance.GetString(StringNames.ExileTextPN));
                                    exiledPlayer.Object.RpcSetNamePrivate(string.Format(playerExiledText, exiledPlayer.Object.GetDefaultName()) + confirmImpostorSecondText, p2);
                                }
                            }
                        }
                    }
                }
            }

            if (exiledPlayer != null && exiledPlayer.Object.IsRole(RoleId.Dictator))
            {
                bool Flag = false;
                if (!RoleClass.Dictator.SubExileLimitData.Contains(exiledPlayer.Object.PlayerId))
                {
                    RoleClass.Dictator.SubExileLimitData[exiledPlayer.Object.PlayerId] = RoleClass.Dictator.SubExileLimit;
                }
                if (RoleClass.Dictator.SubExileLimitData[exiledPlayer.Object.PlayerId] > 0)
                {
                    RoleClass.Dictator.SubExileLimitData[exiledPlayer.Object.PlayerId]--;
                    Flag = true;
                }
                if (Flag)
                {
                    List<PlayerControl> DictatorSubExileTargetList = PlayerControl.AllPlayerControls.ToArray().ToList();
                    DictatorSubExileTargetList.RemoveAll(p =>
                    {
                        return p.IsDead() || p.PlayerId == exiledPlayer.PlayerId;
                    });
                    exiledPlayer = ModHelpers.GetRandom(DictatorSubExileTargetList)?.Data;
                }
            }
            else if (exiledPlayer != null && exiledPlayer.Object.IsRole(RoleId.Safecracker) && Safecracker.CheckTask(exiledPlayer.Object, Safecracker.CheckTasks.ExiledGuard) && (!Safecracker.ExiledGuardCount.ContainsKey(exiledPlayer.Object.PlayerId) || Safecracker.ExiledGuardCount[exiledPlayer.Object.PlayerId] >= 1))
            {
                Logger.Info($"金庫破りが追放ガードの条件を満たしましていました", "Safecracker Exiled Guard");
                Logger.Info($"金庫破りが追放ガードの回数(減らす前) : {(Safecracker.ExiledGuardCount.ContainsKey(exiledPlayer.Object.PlayerId) ? Safecracker.ExiledGuardCount[exiledPlayer.Object.PlayerId] : Safecracker.SafecrackerMaxExiledGuardCount.GetInt())}回", "Safecracker Exiled Guard");
                MessageWriter writer = RPCHelper.StartRPC(CustomRPC.SafecrackerGuardCount);
                writer.Write(exiledPlayer.PlayerId);
                writer.Write(false);
                writer.EndRPC();
                RPCProcedure.SafecrackerGuardCount(exiledPlayer.PlayerId, false);
                Logger.Info($"金庫破りが追放ガードの回数(減らした後) : {Safecracker.ExiledGuardCount[exiledPlayer.PlayerId]}回", "Safecracker Exiled Guard");
                exiledPlayer = null;
            }

            if (tie && Balancer.currentAbilityUser != null)
            {
                exiledPlayer = Balancer.targetplayerleft.Data;
            }

            __instance.RpcVotingComplete(states, exiledPlayer, tie); //RPC

            return false;
        }
        catch (Exception ex)
        {
            SuperNewRolesPlugin.Logger.LogInfo("エラー:" + ex);
            throw;
        }
    }

    /// <summary>
    /// 追放メッセージを非導入ゲストに表記する為に 名前を変えた, プレイヤー又はBotの名前を元に戻す
    /// </summary>
    internal static void ResetExiledPlayerName()
    {
        if (!AmongUsClient.Instance.AmHost) return;
        if (ChangeNameExiledPlayer == null) return;

        ChangeNameExiledPlayer.RpcSetName(ChangeNameExiledPlayer.GetDefaultName());
    }

    private static Tuple<bool, byte, PlayerVoteArea> AssassinVoteState(MeetingHud __instance)
    {
        bool isVoteEnd = false;
        byte voteFor = byte.MaxValue;
        PlayerVoteArea area = null;

        for (int i = 0; i < __instance.playerStates.Length; i++)
        {
            PlayerVoteArea playerVoteArea = __instance.playerStates[i];
            if (playerVoteArea.TargetPlayerId == RoleClass.Assassin.TriggerPlayer.PlayerId)
            {
                isVoteEnd = playerVoteArea.DidVote || playerVoteArea.AmDead;
                voteFor = playerVoteArea.VotedFor;
                area = playerVoteArea;
                break;
            }
        }
        return Tuple.Create(isVoteEnd, voteFor, area);
    }
    private static Tuple<bool, byte, PlayerVoteArea> RevolutionistVoteState(MeetingHud __instance)
    {
        bool isVoteEnd = false;
        byte voteFor = byte.MaxValue;
        PlayerVoteArea area = null;

        for (int i = 0; i < __instance.playerStates.Length; i++)
        {
            PlayerVoteArea playerVoteArea = __instance.playerStates[i];
            if (playerVoteArea.TargetPlayerId == RoleClass.Revolutionist.MeetingTrigger.PlayerId)
            {
                isVoteEnd = playerVoteArea.DidVote || playerVoteArea.AmDead;
                voteFor = playerVoteArea.VotedFor;
                area = playerVoteArea;
                break;
            }
        }
        return Tuple.Create(isVoteEnd, voteFor, area);
    }
}

static class ExtendedMeetingHud
{
    public static Dictionary<byte, int> CustomCalculateVotes(this MeetingHud __instance)
    {
        Dictionary<byte, int> dic = new();
        //| 投票された人 | 投票された回数 |
        for (int i = 0; i < __instance.playerStates.Length; i++)
        {
            PlayerVoteArea ps = __instance.playerStates[i];
            if (ps == null) continue;
            if (AmongUsClient.Instance.NetworkMode == NetworkModes.FreePlay && ps.TargetPlayerId != CachedPlayer.LocalPlayer.PlayerId) continue;
            if (ps != null && ModHelpers.PlayerById(ps.TargetPlayerId) != null && ps.VotedFor != 252 && ps.VotedFor != byte.MaxValue && ps.VotedFor != 254 && ModHelpers.PlayerById(ps.TargetPlayerId).IsAlive() && !ModHelpers.PlayerById(ps.TargetPlayerId).IsBot())
            {
                int VoteNum = 1;
                if (CheckForEndVotingPatch.VoteCountDictionary.ContainsKey(ModHelpers.PlayerById(ps.TargetPlayerId).GetRole())) VoteNum = CheckForEndVotingPatch.VoteCountDictionary[ModHelpers.PlayerById(ps.TargetPlayerId).GetRole()];
                dic[ps.VotedFor] = !dic.TryGetValue(ps.VotedFor, out int num) ? VoteNum : num + VoteNum;
            }
        }
        if (Moira.AbilityUsedThisMeeting && Moira.MoiraChangeVote.GetBool())
        {
            if (Moira.Player.IsAlive())
            {
                PlayerVoteArea swapped1 = null;
                PlayerVoteArea swapped2 = null;
                foreach (PlayerVoteArea playerVoteArea in __instance.playerStates)
                {
                    if (playerVoteArea.TargetPlayerId == Moira.SwapVoteData.Item1) swapped1 = playerVoteArea;
                    if (playerVoteArea.TargetPlayerId == Moira.SwapVoteData.Item2) swapped2 = playerVoteArea;
                }
                if (swapped1 != null && swapped2 != null)
                {
                    if (!dic.ContainsKey(swapped1.TargetPlayerId)) dic[swapped1.TargetPlayerId] = 0;
                    if (!dic.ContainsKey(swapped2.TargetPlayerId)) dic[swapped2.TargetPlayerId] = 0;
                    (dic[swapped1.TargetPlayerId], dic[swapped2.TargetPlayerId]) = (dic[swapped2.TargetPlayerId], dic[swapped1.TargetPlayerId]);
                }
            }
        }
        return dic;
    }
}

[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.SetForegroundForDead))]
class MeetingHudSetForegroundForDeadPatch
{
    public static bool Prefix()
    {
        return (RoleClass.Assassin.TriggerPlayer == null || !RoleClass.Assassin.TriggerPlayer.AmOwner) && (RoleClass.Revolutionist.MeetingTrigger == null || !RoleClass.Revolutionist.MeetingTrigger.AmOwner);
    }
}

[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.UpdateButtons))]
class MeetingHudUpdateButtonsPatch
{
    public static bool Prefix(MeetingHud __instance)
    {
        if (RoleClass.Assassin.TriggerPlayer == null && RoleClass.Revolutionist.MeetingTrigger) { return true; }

        if (AmongUsClient.Instance.AmHost)
        {
            for (int i = 0; i < __instance.playerStates.Length; i++)
            {
                PlayerVoteArea playerVoteArea = __instance.playerStates[i];
                GameData.PlayerInfo PlayerById = GameData.Instance.GetPlayerById(
                    playerVoteArea.TargetPlayerId);
                if (PlayerById == null)
                {
                    playerVoteArea.SetDisabled();
                }
            }
        }
        return false;
    }
    static void Postfix(MeetingHud __instance)
    {
        if (ModeHandler.IsMode(ModeId.SuperHostRoles))
        {
            Crook.Ability.InHostMode.TimeoutCountdownAnnounce();
        }

        var role = PlayerControl.LocalPlayer.GetRole();
        switch (role)
        {
            case RoleId.SoothSayer:
                SoothSayer_updatepatch.UpdateButtonsPostfix(__instance);
                break;
            case RoleId.MeetingSheriff:
                Meetingsheriff_updatepatch.UpdateButtonsPostfix(__instance);
                break;
            case RoleId.Knight:
                KnightProtected_Patch.UpdateButtonsPostfix(__instance);
                break;
            case RoleId.Balancer:
                Balancer.Balancer_updatepatch.UpdateButtonsPostfix(__instance);
                break;
            case RoleId.Crook:
                if (ModeHandler.IsMode(ModeId.Default, ModeId.Werewolf))
                {
                    Crook.Ability.InClientMode.UpdateButtonsPostfix(__instance);
                }
                break;
        }
    }
}
[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.PopulateButtons))]
class MeetingHudPopulateButtonsPatch
{
    public static void Postfix(MeetingHud __instance)
    {
        if (ReplayManager.IsReplayMode)
        {
            List<PlayerVoteArea> areas = new();
            foreach (PlayerVoteArea area in __instance.playerStates)
            {
                if (area.TargetPlayerId != PlayerControl.LocalPlayer.PlayerId)
                    areas.Add(area);
            }
            __instance.playerStates = areas.ToArray(); ;
        }
    }
}

[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.PopulateResults))]
public static class MeetingHudPopulateVotesPatch
{
    public static bool Prefix(MeetingHud __instance, Il2CppStructArray<VoterState> states)
    {
        Moira.SwapVoteArea(__instance);

        __instance.TitleText.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.MeetingVotingResults);
        int num = 0;
        for (int i = 0; i < __instance.playerStates.Length; i++)
        {
            PlayerVoteArea playerVoteArea = __instance.playerStates[i];
            playerVoteArea.ClearForResults();
            int num2 = 0;
            foreach (VoterState voterState in states)
            {
                GameData.PlayerInfo playerById = GameData.Instance.GetPlayerById(voterState.VoterId);
                if (playerById == null)
                {
                    __instance.logger.Error(string.Format("Couldn't find player info for voter: {0}", voterState.VoterId), null);
                }
                else if (i == 0 && voterState.SkippedVote)
                {
                    __instance.BloopAVoteIcon(playerById, num, __instance.SkippedVoting.transform);
                    num++;
                }
                else if (Moira.AbilityUsedThisMeeting && Moira.MoiraChangeVote.GetBool())
                {
                    if (voterState.VotedForId == Moira.SwapVoteData.Item1)
                    {
                        if (Moira.SwapVoteData.Item1 == playerVoteArea.TargetPlayerId)
                        {
                            __instance.BloopAVoteIcon(playerById, num2, playerVoteArea.transform);
                            num2++;
                        }
                    }
                    else if (voterState.VotedForId == Moira.SwapVoteData.Item2)
                    {
                        if (Moira.SwapVoteData.Item2 == playerVoteArea.TargetPlayerId)
                        {
                            __instance.BloopAVoteIcon(playerById, num2, playerVoteArea.transform);
                            num2++;
                        }
                    }
                }
                else if (voterState.VotedForId == playerVoteArea.TargetPlayerId)
                {
                    __instance.BloopAVoteIcon(playerById, num2, playerVoteArea.transform);
                    num2++;
                }
            }
        }
        return false;
    }
}

[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Start))]
class MeetingHudStartPatch
{
    public static void Postfix(MeetingHud __instance)
    {
        Logger.Info("会議開始時の処理 開始", "MeetingHudStartPatch");
        Recorder.StartMeeting();
        ReplayLoader.StartMeeting();
        CustomRoles.OnMeetingStart();
        if (ModeHandler.IsMode(ModeId.SuperHostRoles))
        {
            new LateTask(() =>
            {
                SyncSetting.CustomSyncSettings(out var options);
                SyncSetting.MeetingSyncSettings(options);
                if (!RoleClass.IsFirstMeetingEnd) AddChatPatch.YourRoleInfoSendCommand();
            }, 3f, "StartMeeting CustomSyncSetting");
        }
        if (ModeHandler.IsMode(ModeId.Default))
        {
            new LateTask(() =>
            {
                SyncSetting.MeetingSyncSettings();
            }, 3f, "StartMeeting MeetingSyncSettings SNR");
        }
        NiceMechanic.StartMeeting();
        Roles.Crewmate.Celebrity.AbilityOverflowingBrilliance.TimerStop();
        TheThreeLittlePigs.TheFirstLittlePig.TimerStop();
        MadRaccoon.Button.ResetShapeDuration(false);
        Crook.Ability.SaveReceiptOfInsuranceProceeds();
        NiceMechanic.StartMeeting();
        if (PlayerControl.LocalPlayer.IsRole(RoleId.WiseMan)) WiseMan.StartMeeting();
        Knight.ProtectedPlayer = null;
        Knight.GuardedPlayers = new();
        if (PlayerControl.LocalPlayer.IsRole(RoleId.Werewolf) && CachedPlayer.LocalPlayer.IsAlive() && !RoleClass.Werewolf.IsShooted)
        {
            CreateMeetingButton(__instance, "WerewolfKillButton", (int i, MeetingHud __instance) =>
            {
                if (RoleClass.Werewolf.IsShooted || CachedPlayer.LocalPlayer.IsDead() || !Mode.Werewolf.Main.IsUseButton())
                {
                    __instance.playerStates.ToList().ForEach(x => { if (x.transform.FindChild("WerewolfKillButton") != null) GameObject.Destroy(x.transform.FindChild("WerewolfKillButton").gameObject); });
                    return;
                }

                RoleClass.Werewolf.IsShooted = true;

                if (Knight.GuardedPlayers.Contains((byte)i))
                {
                    var Writer = RPCHelper.StartRPC(CustomRPC.KnightProtectClear);
                    Writer.Write((byte)i);
                    Writer.EndRPC();
                    RPCProcedure.KnightProtectClear((byte)i);
                    PlayerControl player = ModHelpers.PlayerById((byte)i);
                    var Guard = GameObject.Instantiate<RoleEffectAnimation>(FastDestroyableSingleton<RoleManager>.Instance.protectAnim, player.transform);
                    Guard.Play(player, null, player.cosmetics.FlipX, RoleEffectAnimation.SoundType.Global);
                    __instance.playerStates.ToList().ForEach(x => { if (x.transform.FindChild("WerewolfKillButton") != null) GameObject.Destroy(x.transform.FindChild("WerewolfKillButton").gameObject); });
                    return;
                }
                MessageWriter writer = RPCHelper.StartRPC(CustomRPC.MeetingKill);
                writer.Write(CachedPlayer.LocalPlayer.PlayerId);
                writer.Write((byte)i);
                writer.EndRPC();
                RPCProcedure.MeetingKill(CachedPlayer.LocalPlayer.PlayerId, (byte)i);
                __instance.playerStates.ToList().ForEach(x => { if (x.transform.FindChild("WerewolfKillButton") != null) GameObject.Destroy(x.transform.FindChild("WerewolfKillButton").gameObject); });
            }, RoleClass.Werewolf.GetButtonSprite(), (PlayerControl player) => player.IsAlive() && player.PlayerId != CachedPlayer.LocalPlayer.PlayerId);
        }
        if (PlayerControl.LocalPlayer.IsAlive())
        {
            switch (PlayerControl.LocalPlayer.GetRole())
            {
                case RoleId.Moira:
                    Moira.StartMeeting(__instance);
                    break;

                // 以下ネームプレート上の ボタン表示
                case RoleId.SoothSayer:
                case RoleId.SpiritMedium:
                    SoothSayer_Patch.MeetingHudStartPostfix(__instance);
                    break;
                case RoleId.EvilGuesser:
                case RoleId.NiceGuesser:
                    Roles.Attribute.Guesser.StartMeetingPatch.Postfix(__instance);
                    break;
                case RoleId.Knight:
                    KnightProtected_Patch.MeetingHudStartPostfix(__instance);
                    break;
                case RoleId.Balancer:
                    Balancer.Balancer_Patch.MeetingHudStartPostfix(__instance);
                    break;
                case RoleId.Crook:
                    Crook.Ability.InClientMode.MeetingHudStartPostfix(__instance);
                    break;
            }
        }
        Logger.Info("会議開始時の処理 終了", "MeetingHudStartPatch");
    }
    public static void CreateMeetingButton(MeetingHud __instance, string ButtonName, Action<int, MeetingHud> OnClick, Sprite sprite, Func<PlayerControl, bool> CheckCanButton)
    {
        for (int i = 0; i < __instance.playerStates.Length; i++)
        {
            PlayerVoteArea playerVoteArea = __instance.playerStates[i];
            var player = ModHelpers.PlayerById(__instance.playerStates[i].TargetPlayerId);
            if (CheckCanButton(player))
            {
                GameObject template = playerVoteArea.Buttons.transform.Find("CancelButton").gameObject;
                GameObject targetBox = GameObject.Instantiate(template, playerVoteArea.transform);
                targetBox.name = ButtonName;
                targetBox.transform.localPosition = new(1, 0.03f, -1);
                SpriteRenderer renderer = targetBox.GetComponent<SpriteRenderer>();
                renderer.sprite = sprite;
                PassiveButton button = targetBox.GetComponent<PassiveButton>();
                button.OnClick.RemoveAllListeners();
                int copiedIndex = player.PlayerId;
                button.OnClick.AddListener((UnityEngine.Events.UnityAction)(() => OnClick(copiedIndex, __instance)));
            }
        }
    }
}

public static class OpenVotes
{
    /// <summary>
    /// 公開投票にします。[Anonymous votes(匿名投票) / Open votes(公開投票)]
    /// </summary>
    /// <param name="player">設定送信先</param>
    /// <returns> Anonymous votes => [true : 匿名投票 / false : 公開投票]</returns>
    public static bool VoteSyncSetting(this PlayerControl player)
    {
        var role = player.GetRole();
        var optdata = SyncSetting.OptionData.DeepCopy();

        switch (role)
        {
            case RoleId.God:
                optdata.SetBool(BoolOptionNames.AnonymousVotes, !RoleClass.God.IsVoteView);
                break;
            case RoleId.Observer:
                optdata.SetBool(BoolOptionNames.AnonymousVotes, !RoleClass.Observer.IsVoteView);
                break;
            case RoleId.Marlin:
                optdata.SetBool(BoolOptionNames.AnonymousVotes, !RoleClass.Marlin.IsVoteView);
                break;
            case RoleId.Assassin:
                optdata.SetBool(BoolOptionNames.AnonymousVotes, !RoleClass.Assassin.IsVoteView);
                break;
        }
        if (player.IsDead()) optdata.SetBool(BoolOptionNames.AnonymousVotes, !Mode.PlusMode.PlusGameOptions.IsGhostSeeVote && optdata.GetBool(BoolOptionNames.AnonymousVotes));
        Logger.Info("開票しました。", "OpenVotes");
        return optdata.GetBool(BoolOptionNames.AnonymousVotes);
    }
}

[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Update))]
public class MeetingHudUpdatePatch
{
    public static List<string> ErrorNames;
    public static void Postfix()
    {
        if (Instance)
        {
            foreach (PlayerVoteArea player in Instance.playerStates)
            {
                PlayerControl target = null;
                foreach (PlayerControl x in PlayerControl.AllPlayerControls)
                {
                    string name = player.NameText.text.Replace(GetLightAndDarkerText(true), "").Replace(GetLightAndDarkerText(false), "");
                    if (name == x.Data.PlayerName) target = x;
                }
                if (target != null)
                {
                    if (ConfigRoles.IsLightAndDarker.Value)
                    {
                        if (player.NameText.text.Contains(GetLightAndDarkerText(true)) ||
                            player.NameText.text.Contains(GetLightAndDarkerText(false))) continue;
                        player.NameText.text += GetLightAndDarkerText(CustomColors.LighterColors.Contains(target.Data.DefaultOutfit.ColorId));
                    }
                    else player.NameText.text = player.NameText.text.Replace(GetLightAndDarkerText(true), "").Replace(GetLightAndDarkerText(false), "");
                }
                else
                {
                    if (ErrorNames.Contains(player.NameText.text)) continue;
                    Logger.Error($"プレイヤーコントロールを取得できませんでした。 プレイヤー名 : {player.NameText.text}", "LightAndDarkerText");
                    ErrorNames.Add(player.NameText.text);
                }
            }
        }
    }
    public static string GetLightAndDarkerText(bool isLight) => $" ({(isLight ? ModTranslation.GetString("LightColor") : ModTranslation.GetString("DarkerColor"))[0]})";
}