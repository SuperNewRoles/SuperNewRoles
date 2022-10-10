using System;
using System.Collections.Generic;
using System.Linq;
using SuperNewRoles.Mode;
using SuperNewRoles.Patches;
using SuperNewRoles.Roles;
using TMPro;
using UnityEngine;

namespace SuperNewRoles.Modules
{
    public class SetNamesClass
    {
        public static Dictionary<int, string> AllNames = new();

        public static void SetPlayerNameColor(PlayerControl p, Color color)
        {
            if (p.IsBot()) return;
            p.NameText().color = color;
            if (MeetingHud.Instance)
            {
                foreach (PlayerVoteArea player in MeetingHud.Instance.playerStates)
                {
                    if (p.PlayerId == player.TargetPlayerId)
                    {
                        player.NameText.color = color;
                    }
                }
            }
        }
        public static void SetPlayerNameText(PlayerControl p, string text)
        {
            if (p.IsBot()) return;
            p.NameText().text = text;
            if (MeetingHud.Instance)
            {
                foreach (PlayerVoteArea player in MeetingHud.Instance.playerStates)
                {
                    if (player.TargetPlayerId == p.PlayerId)
                    {
                        player.NameText.text = text;
                        return;
                    }
                }
            }
        }
        public static void ResetNameTagsAndColors()
        {
            Dictionary<byte, PlayerControl> playersById = ModHelpers.AllPlayersById();

            foreach (var pro in PlayerInfos)
            {
                pro.Value.text = "";
            }
            foreach (var pro in MeetingPlayerInfos)
            {
                pro.Value.text = "";
            }
            foreach (PlayerControl player in CachedPlayer.AllPlayers)
            {
                bool hidename = ModHelpers.HidePlayerName(PlayerControl.LocalPlayer, player);
                player.NameText().text = hidename ? "" : player.CurrentOutfit.PlayerName;
                if ((PlayerControl.LocalPlayer.IsImpostor() && (player.IsImpostor() || player.IsRole(RoleId.Spy))) || (ModeHandler.IsMode(ModeId.HideAndSeek) && player.IsImpostor()))
                {
                    SetPlayerNameColor(player, RoleClass.ImpostorRed);
                }
                else
                {
                    SetPlayerNameColor(player, Color.white);
                }
            }
        }
        public static Dictionary<byte, TextMeshPro> PlayerInfos = new();
        public static Dictionary<byte, TextMeshPro> MeetingPlayerInfos = new();

        public static void SetPlayerRoleInfoView(PlayerControl p, Color roleColors, string roleNames, Color? GhostRoleColor = null, string GhostRoleNames = "")
        {
            if (p.IsBot()) return;
            bool commsActive = RoleHelpers.IsComms();
            TextMeshPro playerInfo = PlayerInfos.ContainsKey(p.PlayerId) ? PlayerInfos[p.PlayerId] : null;
            if (playerInfo == null)
            {
                playerInfo = UnityEngine.Object.Instantiate(p.NameText(), p.NameText().transform.parent);
                playerInfo.fontSize *= 0.75f;
                playerInfo.gameObject.name = "Info";
                PlayerInfos[p.PlayerId] = playerInfo;
            }

            // Set the position every time bc it sometimes ends up in the wrong place due to camoflauge
            playerInfo.transform.localPosition = p.NameText().transform.localPosition + Vector3.up * 0.5f;

            PlayerVoteArea playerVoteArea = MeetingHud.Instance?.playerStates?.FirstOrDefault(x => x.TargetPlayerId == p.PlayerId);
            TMPro.TextMeshPro meetingInfo = MeetingPlayerInfos.ContainsKey(p.PlayerId) ? MeetingPlayerInfos[p.PlayerId] : null;
            if (meetingInfo == null && playerVoteArea != null)
            {
                meetingInfo = UnityEngine.Object.Instantiate(playerVoteArea.NameText, playerVoteArea.NameText.transform.parent);
                meetingInfo.transform.localPosition += Vector3.down * 0.1f;
                meetingInfo.fontSize = 1.5f;
                meetingInfo.gameObject.name = "Info";
                MeetingPlayerInfos[p.PlayerId] = meetingInfo;
            }

            // Set player name higher to align in middle
            if (meetingInfo != null && playerVoteArea != null)
            {
                var playerName = playerVoteArea.NameText;
                playerName.transform.localPosition = new Vector3(0.3384f, 0.0311f + 0.0683f, -0.1f);
            }
            string TaskText = "";
            try
            {
                if (!p.IsClearTask())
                {
                    if (commsActive)
                    {
                        var all = TaskCount.TaskDateNoClearCheck(p.Data).Item2;
                        TaskText += ModHelpers.Cs(Color.yellow, "(?/" + all + ")");
                    }
                    else
                    {
                        var (Complete, all) = TaskCount.TaskDateNoClearCheck(p.Data);
                        TaskText += ModHelpers.Cs(Color.yellow, "(" + Complete + "/" + all + ")");
                    }
                }
            }
            catch { }
            string playerInfoText = "";
            string meetingInfoText = "";
            playerInfoText = $"{CustomOptions.Cs(roleColors, roleNames)}";
            if (GhostRoleNames != "")
            {
                playerInfoText = $"{CustomOptions.Cs((Color)GhostRoleColor, GhostRoleNames)}({playerInfoText})";
            }
            playerInfoText += TaskText;
            meetingInfoText = playerInfoText.Trim();
            playerInfo.text = playerInfoText;
            playerInfo.gameObject.SetActive(p.Visible);
            if (meetingInfo != null) meetingInfo.text = MeetingHud.Instance.state == MeetingHud.VoteStates.Results ? "" : meetingInfoText; p.NameText().color = roleColors;
        }
        public static void SetPlayerRoleInfo(PlayerControl p)
        {
            if (p.IsBot()) return;
            string roleNames;
            Color roleColors;
            string GhostroleNames = "";
            Color? GhostroleColors = null;
            var role = p.GetRole();
            if (role == RoleId.DefaultRole || (role == RoleId.Bestfalsecharge && p.IsAlive()))
            {
                if (p.IsImpostor())
                {
                    roleNames = "ImpostorName";
                    roleColors = RoleClass.ImpostorRed;
                }
                else
                {
                    roleNames = "CrewMateName";
                    roleColors = RoleClass.CrewmateWhite;
                }
            }
            else if (role == RoleId.Stefinder && RoleClass.Stefinder.IsKill)
            {
                var introdate = IntroDate.GetIntroDate(role);
                roleNames = introdate.Name;
                roleColors = RoleClass.ImpostorRed;
            }
            else if (p.IsPavlovsTeam())
            {
                var introdate = IntroDate.PavlovsdogsIntro;
                roleNames = introdate.Name + (role == RoleId.Pavlovsdogs ? "(D)" : "(O)");
                roleColors = RoleClass.Pavlovsdogs.color;
            }
            else
            {
                var introdate = IntroDate.GetIntroDate(role);
                roleNames = introdate.Name;
                roleColors = introdate.color;
            }
            var GhostRole = p.GetGhostRole();
            if (GhostRole != RoleId.DefaultRole)
            {
                var GhostIntro = IntroDate.GetIntroDate(GhostRole);
                GhostroleNames = GhostIntro.Name;
                GhostroleColors = GhostIntro.color;
            }
            SetPlayerRoleInfoView(p, roleColors, roleNames, GhostroleColors, GhostroleNames);
        }
        public static void SetPlayerNameColors(PlayerControl player)
        {
            var role = player.GetRole();
            if (role == RoleId.DefaultRole || (role == RoleId.Bestfalsecharge && player.IsAlive())) return;
            SetPlayerNameColor(player, IntroDate.GetIntroDate(role).color);
        }
        public static void SetPlayerRoleNames(PlayerControl player)
        {
            SetPlayerRoleInfo(player);
        }
        public static void QuarreledSet()
        {
            string suffix = ModHelpers.Cs(RoleClass.Quarreled.color, "○");
            if (PlayerControl.LocalPlayer.IsQuarreled() && PlayerControl.LocalPlayer.IsAlive())
            {
                PlayerControl side = PlayerControl.LocalPlayer.GetOneSideQuarreled();
                SetPlayerNameText(PlayerControl.LocalPlayer, PlayerControl.LocalPlayer.NameText().text + suffix);
                if (!side.Data.Disconnected)
                {
                    SetPlayerNameText(side, side.NameText().text + suffix);
                }
            }
            if (!PlayerControl.LocalPlayer.IsAlive() && RoleClass.Quarreled.QuarreledPlayer != new List<List<PlayerControl>>())
            {
                foreach (List<PlayerControl> ps in RoleClass.Quarreled.QuarreledPlayer)
                {
                    foreach (PlayerControl p in ps)
                    {
                        if (!p.Data.Disconnected)
                        {
                            SetPlayerNameText(p, p.NameText().text + suffix);
                        }
                    }
                }
            }
        }
        public static void LoversSet()
        {
            string suffix = ModHelpers.Cs(RoleClass.Lovers.color, " ♥");
            if (PlayerControl.LocalPlayer.IsLovers() && PlayerControl.LocalPlayer.IsAlive())
            {
                PlayerControl side = PlayerControl.LocalPlayer.GetOneSideLovers();
                SetPlayerNameText(PlayerControl.LocalPlayer, PlayerControl.LocalPlayer.NameText().text + suffix);
                if (!side.Data.Disconnected)
                    SetPlayerNameText(side, side.NameText().text + suffix);
            }
            if ((PlayerControl.LocalPlayer.IsDead() || PlayerControl.LocalPlayer.IsRole(RoleId.God)) && RoleClass.Lovers.LoversPlayer != new List<List<PlayerControl>>())
            {
                foreach (List<PlayerControl> ps in RoleClass.Lovers.LoversPlayer)
                {
                    foreach (PlayerControl p in ps)
                    {
                        if (!p.Data.Disconnected)
                            SetPlayerNameText(p, p.NameText().text + suffix);
                    }
                }
            }
        }
        public static void DemonSet()
        {
            if (PlayerControl.LocalPlayer.IsRole(RoleId.Demon) || PlayerControl.LocalPlayer.IsDead() || PlayerControl.LocalPlayer.IsRole(RoleId.God))
            {
                foreach (PlayerControl player in CachedPlayer.AllPlayers)
                {
                    if (Demon.IsViewIcon(player))
                    {
                        if (!player.NameText().text.Contains(ModHelpers.Cs(RoleClass.Demon.color, " ▲")))
                            SetPlayerNameText(player, player.NameText().text + ModHelpers.Cs(RoleClass.Demon.color, " ▲"));
                    }
                }
            }
        }
        public static void ArsonistSet()
        {
            if (PlayerControl.LocalPlayer.IsRole(RoleId.Arsonist) || PlayerControl.LocalPlayer.IsDead() || PlayerControl.LocalPlayer.IsRole(RoleId.God))
            {
                foreach (PlayerControl player in CachedPlayer.AllPlayers)
                {
                    if (Arsonist.IsViewIcon(player))
                    {
                        if (!player.NameText().text.Contains(ModHelpers.Cs(RoleClass.Arsonist.color, " §")))
                            SetPlayerNameText(player, player.NameText().text + ModHelpers.Cs(RoleClass.Arsonist.color, " §"));
                    }
                }
            }
        }
        public static void CelebritySet()
        {
            if (RoleClass.Celebrity.ChangeRoleView)
            {
                foreach (PlayerControl p in RoleClass.Celebrity.ViewPlayers)
                {
                    SetPlayerNameColor(p, RoleClass.Celebrity.color);
                }
            }
            else
            {
                foreach (PlayerControl p in RoleClass.Celebrity.CelebrityPlayer)
                {
                    SetPlayerNameColor(p, RoleClass.Celebrity.color);
                }
            }
        }
        public static void SatsumaimoSet()
        {
            if (PlayerControl.LocalPlayer.IsDead() || PlayerControl.LocalPlayer.IsRole(RoleId.God))
            {
                foreach (PlayerControl player in RoleClass.SatsumaAndImo.SatsumaAndImoPlayer)
                {
                    //クルーなら
                    if (!player.NameText().text.Contains(ModHelpers.Cs(RoleClass.Arsonist.color, " (C)")) && RoleClass.SatsumaAndImo.TeamNumber == 1)
                    {//名前に(C)をつける
                        SetNamesClass.SetPlayerNameText(player, player.NameText().text + ModHelpers.Cs(Palette.White, " (C)"));
                    }
                    if (!player.NameText().text.Contains(ModHelpers.Cs(RoleClass.ImpostorRed, " (M)")) && RoleClass.SatsumaAndImo.TeamNumber == 2)
                    {
                        SetNamesClass.SetPlayerNameText(player, player.NameText().text + ModHelpers.Cs(RoleClass.ImpostorRed, " (M)"));
                    }
                }
            }
            else if (PlayerControl.LocalPlayer.IsRole(RoleId.SatsumaAndImo))
            {
                PlayerControl player = PlayerControl.LocalPlayer;
                if (!player.NameText().text.Contains(ModHelpers.Cs(Palette.White, " (C)")) && RoleClass.SatsumaAndImo.TeamNumber == 1)
                {//名前に(C)をつける
                    SetNamesClass.SetPlayerNameText(player, player.NameText().text + ModHelpers.Cs(Palette.White, " (C)"));
                }
                else if (!player.NameText().text.Contains(ModHelpers.Cs(RoleClass.ImpostorRed, " (M)")) && RoleClass.SatsumaAndImo.TeamNumber == 2)
                {
                    SetNamesClass.SetPlayerNameText(player, player.NameText().text + ModHelpers.Cs(RoleClass.ImpostorRed, " (M)"));
                }
            }
        }
    }
    public class SetNameUpdate
    {
        public static void Postfix(PlayerControl __instance)
        {
            SetNamesClass.ResetNameTagsAndColors();
            RoleId LocalRole = PlayerControl.LocalPlayer.GetRole();
            if (PlayerControl.LocalPlayer.IsDead() && LocalRole != RoleId.NiceRedRidingHood)
            {
                foreach (PlayerControl player in CachedPlayer.AllPlayers)
                {
                    SetNamesClass.SetPlayerNameColors(player);
                    SetNamesClass.SetPlayerRoleNames(player);
                }
            }
            else if (LocalRole == RoleId.God)
            {
                foreach (PlayerControl player in CachedPlayer.AllPlayers)
                {
                    if (RoleClass.IsMeeting || player.IsAlive())
                    {
                        SetNamesClass.SetPlayerNameColors(player);
                        SetNamesClass.SetPlayerRoleNames(player);
                    }
                }
            }
            else
            {
                if (Madmate.CheckImpostor(PlayerControl.LocalPlayer) ||
                    LocalRole == RoleId.MadKiller ||
                    LocalRole == RoleId.Marine ||
                    (RoleClass.Demon.IsCheckImpostor && LocalRole == RoleId.Demon)
                    )
                {
                    foreach (PlayerControl p in CachedPlayer.AllPlayers)
                    {
                        if (p.IsImpostor() || p.IsRole(RoleId.Spy))
                        {
                            SetNamesClass.SetPlayerNameColor(p, RoleClass.ImpostorRed);
                        }
                    }
                }
                if (LocalRole == RoleId.Finder)
                {
                    if (RoleClass.Finder.IsCheck)
                    {
                        foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                        {
                            if (player.IsMadRoles())
                            {
                                SetNamesClass.SetPlayerNameColor(player, Color.red);
                            }
                        }
                    }
                }
                if (PlayerControl.LocalPlayer.IsImpostor())
                {
                    foreach (PlayerControl p in RoleClass.SideKiller.MadKillerPlayer)
                    {
                        SetNamesClass.SetPlayerNameColor(p, RoleClass.ImpostorRed);
                    }
                }
                if (PlayerControl.LocalPlayer.IsJackalTeamJackal() ||
                    PlayerControl.LocalPlayer.IsJackalTeamSidekick() ||
                    JackalFriends.CheckJackal(PlayerControl.LocalPlayer))
                {
                    foreach (PlayerControl p in CachedPlayer.AllPlayers)
                    {
                        RoleId role = p.GetRole();
                        if (p.IsJackalTeamJackal() || p.IsJackalTeamSidekick() || p.PlayerId != CachedPlayer.LocalPlayer.PlayerId)
                        {
                            SetNamesClass.SetPlayerRoleNames(p);
                            SetNamesClass.SetPlayerNameColors(p);
                        }
                    }
                }
                if (LocalRole == RoleId.PartTimer)
                {
                    if (RoleClass.PartTimer.IsLocalOn)
                    {
                        if (CustomOptions.PartTimerIsCheckTargetRole.GetBool())
                        {
                            SetNamesClass.SetPlayerRoleNames(RoleClass.PartTimer.CurrentTarget);
                            SetNamesClass.SetPlayerNameColors(RoleClass.PartTimer.CurrentTarget);
                        }
                        else
                        {
                            SetNamesClass.SetPlayerNameText(RoleClass.PartTimer.CurrentTarget, RoleClass.PartTimer.CurrentTarget.NameText().text + ModHelpers.Cs(RoleClass.PartTimer.color, "◀"));
                        }
                    }
                }
                SetNamesClass.SetPlayerRoleNames(PlayerControl.LocalPlayer);
                SetNamesClass.SetPlayerNameColors(PlayerControl.LocalPlayer);
            }
            
            //名前の奴
            if (RoleClass.Camouflager.IsCamouflage)
            {
                if(RoleClass.Camouflager.ArsonistMark)
                    SetNamesClass.ArsonistSet();
                if(RoleClass.Camouflager.DemonMark)
                    SetNamesClass.DemonSet();
                if(RoleClass.Camouflager.LoversMark)
                    SetNamesClass.LoversSet();
                if(RoleClass.Camouflager.QuarreledMark)
                    SetNamesClass.QuarreledSet();
            }
            else
            {
                Roles.Neutral.Pavlovsdogs.SetNameUpdate();
                SetNamesClass.ArsonistSet();
                SetNamesClass.DemonSet();
                SetNamesClass.CelebritySet();
                SetNamesClass.QuarreledSet();
                SetNamesClass.LoversSet();
            }
            SetNamesClass.SatsumaimoSet();

            if (RoleClass.PartTimer.Datas.ContainsValue(CachedPlayer.LocalPlayer.PlayerId))
            {
                PlayerControl PartTimerTarget = ModHelpers.PlayerById((byte)RoleClass.PartTimer.Datas.GetKey(CachedPlayer.LocalPlayer.PlayerId));
                SetNamesClass.SetPlayerRoleNames(PartTimerTarget);
                SetNamesClass.SetPlayerNameColors(PartTimerTarget);
            }
            if (RoleClass.Stefinder.IsKill)
            {
                SetNamesClass.SetPlayerNameColor(PlayerControl.LocalPlayer, Color.red);
            }
            if (ModeHandler.IsMode(ModeId.Default))
            {
                if (Sabotage.SabotageManager.thisSabotage == Sabotage.SabotageManager.CustomSabotage.CognitiveDeficit)
                {
                    foreach (PlayerControl p3 in CachedPlayer.AllPlayers)
                    {
                        if (p3.IsAlive() && !Sabotage.CognitiveDeficit.Main.OKPlayers.IsCheckListPlayerControl(p3))
                        {
                            if (PlayerControl.LocalPlayer.IsImpostor())
                            {
                                if (!(p3.IsImpostor() || p3.IsRole(RoleId.MadKiller)))
                                {
                                    SetNamesClass.SetPlayerNameColor(p3, new Color32(18, 112, 214, byte.MaxValue));
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}