//元:https://github.com/yukieiji/ExtremeRoles/blob/master/ExtremeRoles/Patches/AirShipStatusPatch.cs
using System.Linq;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using SuperNewRoles.Helpers;
using SuperNewRoles.Mode;
using SuperNewRoles.Mode.SuperHostRoles;

namespace SuperNewRoles.Roles;

public static class Assassin
{
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
        if (!ModeHandler.IsMode(ModeId.SuperHostRoles)) return;
        if (RoleClass.Assassin.TriggerPlayer != null && sourcePlayer.PlayerId == RoleClass.Assassin.TriggerPlayer.PlayerId)
        {
            var player = CachedPlayer.AllPlayers.FirstOrDefault((_) => chatText.Equals(_.PlayerControl.name));
            if (player == null || player.PlayerControl.IsBot()) return;

            Il2CppStructArray<MeetingHud.VoterState> array =
                new(
                    MeetingHud.Instance.playerStates.Length);

            for (int i = 0; i < MeetingHud.Instance.playerStates.Length; i++)
            {
                PlayerVoteArea playerVoteArea = MeetingHud.Instance.playerStates[i];
                playerVoteArea.VotedFor = playerVoteArea.TargetPlayerId == RoleClass.Assassin.TriggerPlayer.PlayerId ? player.Data.PlayerId : (byte)254;
                MeetingHud.Instance.SetDirtyBit(1U);

                array[i] = new MeetingHud.VoterState
                {
                    VoterId = playerVoteArea.TargetPlayerId,
                    VotedForId = playerVoteArea.VotedFor
                };
            }
            NetworkedPlayerInfo target = player.Data;
            NetworkedPlayerInfo exileplayer = null;
            if (target != null)
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
                            p.RpcSetName(target.Object.GetDefaultName() + (target.Object.IsRole(RoleId.Marlin) ? ModTranslation.GetString("AssassinSuccess") : ModTranslation.GetString("AssassinFail")) + "<size=0%>");
                            p.RpcSetHat(outfit.HatId);
                            p.RpcSetVisor(outfit.VisorId);
                            p.RpcSetSkin(outfit.SkinId);
                            break;
                        }
                    }
                }
                RoleClass.Assassin.MeetingEndPlayers.Add(RoleClass.Assassin.TriggerPlayer.PlayerId);
                if (target.Object.IsRole(RoleId.Marlin))
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
                        exile.RpcSetName(exile.GetDefaultName());
                        exile.RpcSetColor(1);
                        exile.RpcSetHat("hat_NoHat");
                        exile.RpcSetPet("peet_EmptyPet");
                        exile.RpcSetVisor("visor_EmptyVisor");
                        exile.RpcSetNamePlate("nameplate_NoPlate");
                        exile.RpcSetSkin("skin_None");
                    }
                }, 5f, "Assassin Set Skins");
            }
            new LateTask(() => MeetingHud.Instance.RpcVotingComplete(array, exileplayer, true), 0.2f, "Assassin Vote Comp");
        }
    }
    public static void WrapUp()
    {
        if (RoleClass.Assassin.DeadPlayer != null)
        {
            if (ModeHandler.IsMode(ModeId.SuperHostRoles))
            {
                if (AmongUsClient.Instance.AmHost)
                {
                    RoleClass.Assassin.DeadPlayer.RpcInnerExiled();
                }
            }
            else
            {
                RoleClass.Assassin.DeadPlayer.Exiled();
            }
            RoleClass.Assassin.DeadPlayer = null;
        }
        if (RoleClass.Assassin.IsImpostorWin)
        {
            MapUtilities.CachedShipStatus.enabled = false;
            GameManager.Instance.RpcEndGame(GameOverReason.ImpostorsByVote, false);
        }
        var exile = Main.RealExiled;
        if (ModeHandler.IsMode(ModeId.SuperHostRoles) && exile != null && exile.IsRole(RoleId.Assassin))
        {
            if (AmongUsClient.Instance.AmHost)
            {
                new LateTask(() =>
                {
                    MeetingRoomManager.Instance.AssignSelf(exile, null);
                    FastDestroyableSingleton<HudManager>.Instance.OpenMeetingRoom(exile);
                    exile.RpcStartMeeting(null);
                }, 10.5f, "Assassin Meet");
                new LateTask(() =>
                {
                    exile.RpcSetName($"<size=200%>{CustomOptionHolder.Cs(RoleClass.Marlin.color, IntroData.MarlinIntro.NameKey + "Name")}<color=white>は誰だ？</size>");
                }, 12f, "Assassin Name");
                new LateTask(() =>
                {
                    exile.RpcSendChat($"\n{ModTranslation.GetString("MarlinWhois")}");
                }, 12.5f, "Assassin Chat");
                new LateTask(() =>
                {
                    exile.RpcSetName(exile.GetDefaultName());
                }, 13f, "Assassin Default");
            }
            RoleClass.Assassin.TriggerPlayer = exile;
        }
    }
}