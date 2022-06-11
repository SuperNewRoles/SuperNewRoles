using HarmonyLib;
using SuperNewRoles.CustomOption;
using SuperNewRoles.CustomRPC;
using SuperNewRoles.Helpers;
using SuperNewRoles.Intro;
using SuperNewRoles.Mode;
using SuperNewRoles.Mode.SuperHostRoles;
using SuperNewRoles.Patches;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnhollowerBaseLib;

namespace SuperNewRoles.Roles
{
    public static class Assassin
    {
        //元:https://github.com/yukieiji/ExtremeRoles/blob/master/ExtremeRoles/Patches/AirShipStatusPatch.cs
        [HarmonyPatch(typeof(AirshipStatus), nameof(AirshipStatus.PrespawnStep))]
        public static class AirshipStatusPrespawnStepPatch
        {
            public static bool Prefix()
            {
                return RoleClass.Assassin.TriggerPlayer == null;
            }
        }
        public static void AddChat(PlayerControl sourcePlayer, string chatText)
        {
            if (!ModeHandler.isMode(ModeId.SuperHostRoles)) return;
            if (RoleClass.Assassin.TriggerPlayer != null && sourcePlayer.PlayerId == RoleClass.Assassin.TriggerPlayer.PlayerId)
            {
                var player = CachedPlayer.AllPlayers.ToArray().ToList().FirstOrDefault((_) => chatText.Equals(_.PlayerControl.name));
                if (player == null || player.PlayerControl.IsBot()) return;

                Il2CppStructArray<MeetingHud.VoterState> array =
                    new Il2CppStructArray<MeetingHud.VoterState>(
                        MeetingHud.Instance.playerStates.Length);

                for (int i = 0; i < MeetingHud.Instance.playerStates.Length; i++)
                {
                    PlayerVoteArea playerVoteArea = MeetingHud.Instance.playerStates[i];
                    if (playerVoteArea.TargetPlayerId == RoleClass.Assassin.TriggerPlayer.PlayerId)
                    {
                        playerVoteArea.VotedFor = player.Data.PlayerId;
                    }
                    else
                    {
                        playerVoteArea.VotedFor = 254;
                    }
                    MeetingHud.Instance.SetDirtyBit(1U);

                    array[i] = new MeetingHud.VoterState
                    {
                        VoterId = playerVoteArea.TargetPlayerId,
                        VotedForId = playerVoteArea.VotedFor
                    };

                }
                GameData.PlayerInfo target = player.Data;
                GameData.PlayerInfo exileplayer = null;
                if (target != null)
                {
                    var outfit = target.DefaultOutfit;
                    exileplayer = target;
                    PlayerControl exile = null;
                    Mode.SuperHostRoles.main.RealExiled = target.Object;
                    if (ModeHandler.isMode(ModeId.SuperHostRoles))
                    {
                        foreach (PlayerControl p in BotManager.AllBots)
                        {
                            if (p.isDead())
                            {
                                exileplayer = p.Data;
                                exile = p;
                                p.RpcSetColor((byte)outfit.ColorId);
                                p.RpcSetName(target.Object.getDefaultName() + (target.Object.isRole(RoleId.Marine) ? ModTranslation.getString("AssassinSucsess") : ModTranslation.getString("AssassinFail")) + "<size=0%>");
                                p.RpcSetHat(outfit.HatId);
                                p.RpcSetVisor(outfit.VisorId);
                                p.RpcSetSkin(outfit.SkinId);
                                break;
                            }
                        }
                    }
                    RoleClass.Assassin.MeetingEndPlayers.Add(RoleClass.Assassin.TriggerPlayer.PlayerId);
                    if (target.Object.isRole(RoleId.Marine))
                    {
                        RoleClass.Assassin.IsImpostorWin = true;
                    }
                    else
                    {
                        RoleClass.Assassin.DeadPlayer = RoleClass.Assassin.TriggerPlayer;
                    }
                    new LateTask(() =>
                    {
                        if (exile != null)
                        {
                            exile.RpcSetName(exile.getDefaultName());
                            exile.RpcSetColor(1);
                            exile.RpcSetHat("hat_NoHat");
                            exile.RpcSetPet("peet_EmptyPet");
                            exile.RpcSetVisor("visor_EmptyVisor");
                            exile.RpcSetNamePlate("nameplate_NoPlate");
                            exile.RpcSetSkin("skin_None");
                        }
                    }, 5f);
                }
                new LateTask(() => MeetingHud.Instance.RpcVotingComplete(array, exileplayer, true), 0.2f);
            }
        }
        public static void WrapUp(GameData.PlayerInfo exiled)
        {
            if (RoleClass.Assassin.DeadPlayer != null)
            {
                if (ModeHandler.isMode(ModeId.SuperHostRoles))
                {
                    if (AmongUsClient.Instance.AmHost) {
                        RoleClass.Assassin.DeadPlayer.RpcInnerExiled();
                    }
                } else
                {
                    RoleClass.Assassin.DeadPlayer.Exiled();
                }
                RoleClass.Assassin.DeadPlayer = null;
            }
            if (RoleClass.Assassin.IsImpostorWin)
            {
                MapUtilities.CachedShipStatus.enabled = false;
                ShipStatus.RpcEndGame(GameOverReason.ImpostorByVote, false);
            }
            var exile = Mode.SuperHostRoles.main.RealExiled;
            if (ModeHandler.isMode(ModeId.SuperHostRoles) && exile != null && exile.isRole(CustomRPC.RoleId.Assassin))
            {
                if (AmongUsClient.Instance.AmHost)
                {
                    new LateTask(() =>
                    {
                        MeetingRoomManager.Instance.AssignSelf(exile, null);
                        FastDestroyableSingleton<HudManager>.Instance.OpenMeetingRoom(exile);
                        exile.RpcStartMeeting(null);
                    }, 10.5f);
                    new LateTask(() =>
                    {
                        exile.RpcSetName($"<size=200%>{CustomOptions.cs(RoleClass.Marine.color, IntroDate.MarineIntro.NameKey + "Name")}<color=white>は誰だ？</size>");
                    }, 12f);
                    new LateTask(() =>
                    {
                        exile.RpcSendChat($"\n{ModTranslation.getString("MarineWhois")}");
                    }, 12.5f);
                    new LateTask(() =>
                    {
                        exile.RpcSetName(exile.getDefaultName());
                    }, 13f);
                }
                RoleClass.Assassin.TriggerPlayer = exile;
            }
        }
    }
}
