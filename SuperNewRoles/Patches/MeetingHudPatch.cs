using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AmongUs.GameOptions;
using HarmonyLib;
using Hazel;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using SuperNewRoles;
using SuperNewRoles.CustomCosmetics;
using SuperNewRoles.Helpers;
using SuperNewRoles.Mode;
using SuperNewRoles.Mode.SuperHostRoles;
using SuperNewRoles.Mode.PlusMode;
using SuperNewRoles.Replay;
using SuperNewRoles.Replay.ReplayActions;
using SuperNewRoles.Roles;
using SuperNewRoles.Roles.Attribute;
using SuperNewRoles.Roles.Crewmate;
using SuperNewRoles.Roles.Impostor;
using SuperNewRoles.Roles.Impostor.MadRole;
using SuperNewRoles.Roles.Neutral;
using SuperNewRoles.Roles.RoleBases;
using SuperNewRoles.Roles.RoleBases.Interfaces;
using SuperNewRoles.SuperNewRolesWeb;
using UnityEngine;
using static MeetingHud;
using SuperNewRoles.MapOption;
using static Il2CppSystem.Xml.XmlWellFormedWriter.AttributeValueCache;
using SuperNewRoles.CustomObject;

namespace SuperNewRoles.Patches;

[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Awake))]
class AwakeMeetingPatch
{
    public static void Prefix(MeetingHud __instance) => BatteryIconDestroy(__instance);
    public static void Postfix() => RoleClass.IsMeeting = true;

    private static void BatteryIconDestroy(MeetingHud __instance)
    {
        UnityEngine.Object.Destroy(__instance.meetingContents.FindChild("PhoneUI").FindChild("UI_Icon_Battery").gameObject);
    }
}
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
            case RoleId.Balancer:
                IsValidVote = Balancer.InHostMode.MeetingHudCastVote_Prefix(srcPlayerId, suspectPlayerId);
                break;
        }

        if (srcPlayer.GetRoleBase() is IMeetingHandler handler)
            IsValidVote = handler.CastVote(suspectPlayerId);

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
    public static void Postfix(MeetingHud __instance, [HarmonyArgument(0)] Il2CppStructArray<VoterState> states, [HarmonyArgument(1)] ref NetworkedPlayerInfo exiled, [HarmonyArgument(2)] bool tie)
    {
        if (AmongUsClient.Instance.AmHost) ReplayActionVotingComplete.Create(states, exiled is null ? (byte)255 : exiled.PlayerId, tie);
    }
}
[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.VotingComplete))]
class VotingComplete
{
    public static void Prefix(MeetingHud __instance, [HarmonyArgument(0)] Il2CppStructArray<VoterState> states, [HarmonyArgument(1)] ref NetworkedPlayerInfo exiled, [HarmonyArgument(2)] bool tie)
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
    public static void Postfix()
    {
        CustomRoles.OnMeetingClose();
    }
}
[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.VotingComplete))]
class VotingComplatePatch
{
    public static void Postfix(MeetingHud __instance, Il2CppStructArray<VoterState> states, NetworkedPlayerInfo exiled, bool tie)
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
                        NetworkedPlayerInfo exiledPlayerdetective = CachedPlayer.LocalPlayer.Data;
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

                            exiledPlayerdetective = GameData.Instance.AllPlayers.FirstOrDefault(info => !tiedetective && info.PlayerId == ps.VotedFor);

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
                            exiledPlayerdetective = GameData.Instance.AllPlayers.FirstOrDefault(info => !tiedetective && info.PlayerId == 253);

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
                    //NetworkedPlayerInfo exiled = Helper.Player.GetPlayerControlById(voteFor).Data;
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
                    NetworkedPlayerInfo target = GameData.Instance.GetPlayerById(voteFor);
                    NetworkedPlayerInfo exileplayer = null;
                    if (target != null && target.Object.PlayerId != RoleClass.Assassin.TriggerPlayer.PlayerId && !target.Object.IsBot())
                    {
                        var outfit = target.DefaultOutfit;
                        exileplayer = target;
                        PlayerControl exile = null;
                        Main.RealExiled = target.Object;
                        if (ModeHandler.IsMode(ModeId.SuperHostRoles))
                        {
                            foreach (PlayerControl p in BotManager.AllBots.AsSpan())
                            {
                                if (p.IsDead())
                                {
                                    exileplayer = p.Data;
                                    exile = p;
                                    p.RpcSetColor((byte)outfit.ColorId);
                                    p.RpcSetName(target.Object.GetDefaultName() +
                                        ModTranslation.GetString(target.Object.IsRole(RoleId.Marlin) ?
                                        "AssassinSuccess" :
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
                    //NetworkedPlayerInfo exiled = Helper.Player.GetPlayerControlById(voteFor).Data;
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
                    NetworkedPlayerInfo target = GameData.Instance.GetPlayerById(voteFor);
                    NetworkedPlayerInfo exileplayer = null;
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
            NetworkedPlayerInfo exiledPlayer = CachedPlayer.LocalPlayer.Data;
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

            Moira moira = RoleBaseManager.GetRoleBases<Moira>().FirstOrDefault();
            if (Moira.ChangeVote.GetBool() && moira != null && moira.AbilityUsedThisMeeting)
            {
                for (int i = 0; i < statesList.Count; i++)
                {
                    VoterState state = statesList[i];
                    if (state.VotedForId == moira.SwapVoteData.Item1) state.VotedForId = moira.SwapVoteData.Item2;
                    else if (state.VotedForId == moira.SwapVoteData.Item2) state.VotedForId = moira.SwapVoteData.Item1;
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

            exiledPlayer = GameData.Instance.AllPlayers.FirstOrDefault(info => !tie && info.PlayerId == exileId);

            if (ModeHandler.IsMode(ModeId.SuperHostRoles))
            {
                if (exiledPlayer != null && exiledPlayer.Object.IsRole(RoleId.Assassin))
                {
                    Main.RealExiled = exiledPlayer.Object;
                    PlayerControl exile = null;
                    PlayerControl defaultexile = exiledPlayer.Object;
                    var outfit = defaultexile.Data.DefaultOutfit;
                    foreach (PlayerControl p in BotManager.AllBots.AsSpan())
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
                if (Mode.PlusMode.PlusGameOptions.EnableFirstEmergencyCooldown)
                {
                    EmergencyMinigamePatch.FirstEmergencyCooldown.OnCheckForEndVotingNotMod(exiledPlayer != null);
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
                        foreach (PlayerControl p2 in CachedPlayer.AllPlayers.AsSpan())
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

                        foreach (PlayerControl p2 in CachedPlayer.AllPlayers.AsSpan())
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
                    var DictatorSubExileTargetList = PlayerControl.AllPlayerControls;
                    DictatorSubExileTargetList.RemoveAll((Il2CppSystem.Predicate<PlayerControl>)(p =>
                    {
                        return p.IsDead() || p.PlayerId == exiledPlayer.PlayerId;
                    }));
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

            if (ModeHandler.IsMode(ModeId.SuperHostRoles))
            {
                var supportType = AntiBlackOut.GetSupportType(exiledPlayer);
                bool IsDesyncMode = false;
                (PlayerControl player, NetworkedPlayerInfo exiled, bool tie) DesyncDetail = default;
                Logger.Info("AntiBlackOut:          Selected is "+supportType.ToString());
                switch (supportType)
                {
                    case AntiBlackOut.SupportType.NoneExile:
                        exiledPlayer = null;
                        tie = false;
                        break;
                    case AntiBlackOut.SupportType.DeadExile:
                        NetworkedPlayerInfo NewExiled = null;
                        NetworkedPlayerInfo ExileCandidate = null;
                        foreach (var player in CachedPlayer.AllPlayers.AsSpan())
                        {
                            NetworkedPlayerInfo playerinfo = player.Data;
                            if (playerinfo.Disconnected)
                            {
                                NewExiled = playerinfo;
                                break;
                            }
                            if (!playerinfo.IsDead)
                                continue;
                            PlayerControl @object = playerinfo.Object;
                            if (@object == null)
                                continue;
                            if (@object.IsMod() ||
                                !AntiBlackOut.IsPlayerDesyncImpostorTeam(@object))
                            {
                                NewExiled = playerinfo;
                                break;
                            }
                            else
                                ExileCandidate = playerinfo;
                        }
                        if (NewExiled == null && ExileCandidate == null)
                            throw new Exception("None DeadPlayer");
                        if (NewExiled == null)
                        {
                            NewExiled = ExileCandidate;
                            if (NewExiled.Object)
                            {
                                IsDesyncMode = true;
                                DesyncDetail = (NewExiled.Object, null, true);
                                AntiBlackOut.SendAntiBlackOutInformation(NewExiled.Object, AntiBlackOut.ABOInformationType.OnlyDesyncImpostorDead, exiledPlayer?.PlayerName);
                            }
                        }
                        new LateTask(() =>
                        {
                            PlayerControl exiledObject = NewExiled.Object;

                            int exiledColorId = NewExiled.DefaultOutfit.ColorId;
                            string exiledName = NewExiled.DefaultOutfit.PlayerName;
                            string exiledHatId = NewExiled.DefaultOutfit.HatId;
                            string exiledVisorId = NewExiled.DefaultOutfit.VisorId;
                            string exiledSkinId = NewExiled.DefaultOutfit.SkinId;

                            Logger.Info("AAA");

                            new LateTask(() =>
                            {
                                if (exiledObject == null)
                                    return;
                                Logger.Info("CCC");
                                exiledObject.RpcSetName(exiledName);
                                exiledObject.RpcSetColor((byte)exiledColorId);
                                exiledObject.RpcSetHat(exiledHatId);
                                exiledObject.RpcSetVisor(exiledVisorId);
                                exiledObject.RpcSetSkin(exiledSkinId);
                                Logger.Info("戻しました");
                            }, 5f);

                            Logger.Info("BBB");
                            
                            CustomRpcSender customRpcSender = CustomRpcSender.Create(sendOption: SendOption.Reliable);
                            
                            customRpcSender.AutoStartRpc(exiledObject.NetId, (byte)RpcCalls.SetName)
                                .Write(exiledObject.Data.NetId)
                                .Write(AntiBlackOut.RealExiled.DefaultOutfit.PlayerName)
                                .EndRpc()
                                .AutoStartRpc(exiledObject.NetId, (byte)RpcCalls.SetColor)
                                .Write(AntiBlackOut.RealExiled.DefaultOutfit.ColorId)
                                .Write(exiledObject.GetNextRpcSequenceId(RpcCalls.SetColor))
                                .EndRpc()
                                .AutoStartRpc(exiledObject.NetId, (byte)RpcCalls.SetHatStr)
                                .Write(AntiBlackOut.RealExiled.DefaultOutfit.HatId)
                                .Write(exiledObject.GetNextRpcSequenceId(RpcCalls.SetHatStr))
                                .EndRpc()
                                .AutoStartRpc(exiledObject.NetId, (byte)RpcCalls.SetVisorStr)
                                .Write(AntiBlackOut.RealExiled.DefaultOutfit.VisorId)
                                .Write(exiledObject.GetNextRpcSequenceId(RpcCalls.SetVisorStr))
                                .EndRpc()
                                .AutoStartRpc(exiledObject.NetId, (byte)RpcCalls.SetSkinStr)
                                .Write(AntiBlackOut.RealExiled.DefaultOutfit.SkinId)
                                .Write(exiledObject.GetNextRpcSequenceId(RpcCalls.SetSkinStr))
                                .EndRpc()
                                .SendMessage();
                        }, AmongUsClient.Instance.NetworkMode == NetworkModes.OnlineGame ? 4.5f : 0.1f);
                        exiledPlayer = NewExiled;
                        break;
                    case AntiBlackOut.SupportType.DoubleVotedAfterExile:
                        exiledPlayer = null;
                        tie = true;
                        break;
                }
                if (IsDesyncMode)
                {
                    RPCHelper.RpcVotingCompleteDesync(states, DesyncDetail.exiled, DesyncDetail.tie, DesyncDetail.player);
                }
                new LateTask(() => __instance.RpcVotingComplete(states, exiledPlayer, tie), 0.1f); //RPC
                __instance.VotingComplete(states, exiledPlayer, tie);
                return false;
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

    private static (bool, byte, PlayerVoteArea) AssassinVoteState(MeetingHud __instance)
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
        return (isVoteEnd, voteFor, area);
    }
    private static (bool, byte, PlayerVoteArea) RevolutionistVoteState(MeetingHud __instance)
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
        return (isVoteEnd, voteFor, area);
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
        RoleBaseManager.GetInterfaces<IMeetingHandler>().Do(x => x.CalculateVotes(dic));
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
                NetworkedPlayerInfo PlayerById = GameData.Instance.GetPlayerById(
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
            __instance.playerStates = areas.ToArray();
        }
    }
}

[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Close))]
class MeetingHudClosePatch
{
    public static void Prefix(MeetingHud __instance)
    {
        if (!ModeHandler.IsMode(ModeId.SuperHostRoles) ||
            __instance.exiledPlayer == null)
            return;
        if (AntiBlackOut.RealExiled == null)
        {
            Logger.Warn("Warning: AntiBlackOut.RealExiled is null.", "RpcClose");
            return;
        }
        if (__instance.exiledPlayer.PlayerId == AntiBlackOut.RealExiled.PlayerId ||
            __instance.exiledPlayer.Object == null)
            return;
        __instance.exiledPlayer = AntiBlackOut.RealExiled;
    }
    public static void Postfix(MeetingHud __instance)
    {
        CustomRoles.OnMeetingClose();
        AntiBlackOut.OnMeetingHudClose(AntiBlackOut.RealExiled);
        Drone.CloseMeeting();
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
        DeviceClass.OnStartMeeting();
        if (ModeHandler.IsMode(ModeId.SuperHostRoles))
        {
            new LateTask(() =>
            {
                SyncSetting.CustomSyncSettings();

                if (!RoleClass.IsFirstMeetingEnd)
                {
                    if (SuperHostRolesOptions.SettingClass.IsSendYourRoleFirstTurn) { RoleinformationText.YourRoleInfoSendCommand(); }
                    EmergencyMinigamePatch.SHRMeetingStatusAnnounce.MakeSettingKnown();
                }
                else
                {
                    if (SuperHostRolesOptions.SettingClass.IsSendYourRoleAllTurn) { RoleinformationText.YourRoleInfoSendCommand(); }
                    EmergencyMinigamePatch.SHRMeetingStatusAnnounce.LimitAnnounce();
                }
            }, 3f, "StartMeeting CustomSyncSetting");
        }

        AnonymousVotes.SetLocalAnonymousVotes();

        if (ModeHandler.IsMode(ModeId.SuperHostRoles))
        {
            ReleaseGhostAbility.MeetingHudStartPostfix();
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
        Balancer.InHostMode.StartMeeting();
        if (PlayerControl.LocalPlayer.IsRole(RoleId.Werewolf) && CachedPlayer.LocalPlayer.IsAlive() && !RoleClass.Werewolf.IsShooted)
        {
            CreateMeetingButton(__instance, "WerewolfKillButton", (int i, MeetingHud __instance) =>
            {
                if (RoleClass.Werewolf.IsShooted || CachedPlayer.LocalPlayer.IsDead() || !Mode.Werewolf.Main.IsUseButton())
                {
                    foreach (PlayerVoteArea state in __instance.playerStates)
                    {
                        if (state.transform.FindChild("WerewolfKillButton") != null)
                            GameObject.Destroy(state.transform.FindChild("WerewolfKillButton").gameObject);
                    }
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
                    __instance.playerStates.ForEach(x => { if (x.transform.FindChild("WerewolfKillButton") != null) GameObject.Destroy(x.transform.FindChild("WerewolfKillButton").gameObject); });
                    return;
                }
                MessageWriter writer = RPCHelper.StartRPC(CustomRPC.MeetingKill);
                writer.Write(CachedPlayer.LocalPlayer.PlayerId);
                writer.Write((byte)i);
                writer.EndRPC();
                RPCProcedure.MeetingKill(CachedPlayer.LocalPlayer.PlayerId, (byte)i);
                __instance.playerStates.ForEach(x => { if (x.transform.FindChild("WerewolfKillButton") != null) GameObject.Destroy(x.transform.FindChild("WerewolfKillButton").gameObject); });
            }, RoleClass.Werewolf.GetButtonSprite(), (PlayerControl player) => player.IsAlive() && player.PlayerId != CachedPlayer.LocalPlayer.PlayerId);
        }
        if (PlayerControl.LocalPlayer.IsAlive())
        {
            switch (PlayerControl.LocalPlayer.GetRole())
            {
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
        else if (PlayerControl.LocalPlayer.IsDead())
        {
            switch (PlayerControl.LocalPlayer.GetGhostRole())
            {
                case RoleId.GhostMechanic:
                    GhostMechanic.MeetingHudStart();
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

/// <summary> 設定や, 役職による匿名投票の機能を制御する </summary>
public static class AnonymousVotes
{
    /// <summary> 匿名投票であるか取得する </summary>
    /// <param name="player">取得対象のプレイヤー</param>
    /// <returns> true : 匿名投票 / false : 公開投票</returns>
    public static bool GetAnonymousVotes(this PlayerControl player)
    {
        if (player == null || player.IsBot()) return GameOptionsManager.Instance.CurrentGameOptions.GetBool(BoolOptionNames.AnonymousVotes);

        var isClosed = GameOptionsManager.Instance.CurrentGameOptions.GetBool(BoolOptionNames.AnonymousVotes); // 初期値をバニラ設定に

        if (player.GetRoleBase() is IMeetingHandler meetingHandler) { isClosed = meetingHandler.EnableAnonymousVotes; }
        else
        {
            var role = player.GetRole();
            switch (role)
            {
                case RoleId.God:
                    isClosed = !RoleClass.God.IsVoteView;
                    break;
                case RoleId.Marlin:
                    isClosed = !RoleClass.Marlin.IsVoteView;
                    break;
                case RoleId.Assassin:
                    isClosed = !RoleClass.Assassin.IsVoteView;
                    break;
            }
        }

        if (player.IsDead()) isClosed = !Mode.PlusMode.PlusGameOptions.IsGhostSeeVote && isClosed; // "見られない状態" より "見られる状態" を優先する
        // 公開投票ならログを出力
        if (!isClosed) Logger.Info($"公開投票 : {player.name}, (role = {player.GetRole()}, IsDead() = {player.IsDead()})", "OpenVotes");

        return isClosed;
    }

    /// <summary>
    /// 導入者個人で, 匿名投票であるかを設定に従い反映する。
    /// </summary>
    public static void SetLocalAnonymousVotes()
    {
        if (!ModeHandler.IsMode(ModeId.Default, ModeId.Werewolf)) return; // SHRモードでは, Hostが送信する。

        var optData = SyncSetting.OptionDatas.Local.DeepCopy();
        optData.SetBool(BoolOptionNames.AnonymousVotes, GetAnonymousVotes(PlayerControl.LocalPlayer));
        GameManager.Instance.LogicOptions.SetGameOptions(optData);
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
                foreach (PlayerControl x in CachedPlayer.AllPlayers.AsSpan())
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