using HarmonyLib;
using SuperNewRoles.CustomOption;
using SuperNewRoles.CustomRPC;
using SuperNewRoles.Mode;
using SuperNewRoles.Roles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SuperNewRoles.Patch
{
    public class SetNamesClass
    {
        public static Dictionary<int, string> AllNames = new Dictionary<int, string>();

        public static void SetPlayerNameColor(PlayerControl p, Color color)
        {
            if (p.IsBot()) return;
            p.nameText.color = color;
            try
            {
                foreach (PlayerVoteArea player in MeetingHud.Instance.playerStates)
                {
                    if (p.PlayerId == player.TargetPlayerId)
                    {
                        player.NameText.color = color;
                    }
                }
            }
            catch { }
        }
        public static void SetPlayerNameText(PlayerControl p, string text)
        {
            if (p.IsBot()) return;
            p.nameText.text = text;
            if (MeetingHud.Instance)
            {
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
        }
        public static void resetNameTagsAndColors()
        {
            Dictionary<byte, PlayerControl> playersById = ModHelpers.allPlayersById();

            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            {
                player.nameText.text = player.CurrentOutfit.PlayerName;
                if (PlayerControl.LocalPlayer.Data.Role.IsImpostor && (player.Data.Role.IsImpostor || player.isRole(CustomRPC.RoleId.Egoist)))
                {
                    player.nameText.color = Palette.ImpostorRed;
                }
                else
                {
                    player.nameText.color = Color.white;
                }
            }
            if (MeetingHud.Instance != null)
            {
                foreach (PlayerVoteArea player in MeetingHud.Instance.playerStates)
                {
                    PlayerControl playerControl = playersById.ContainsKey((byte)player.TargetPlayerId) ? playersById[(byte)player.TargetPlayerId] : null;
                    if (playerControl != null)
                    {
                        player.NameText.text = playerControl.Data.PlayerName;
                        if (PlayerControl.LocalPlayer.Data.Role.IsImpostor && (playerControl.Data.Role.IsImpostor || playerControl.isRole(CustomRPC.RoleId.Egoist)))
                        {
                            player.NameText.color = Palette.ImpostorRed;
                        }
                        else
                        {
                            player.NameText.color = Color.white;
                        }
                    }
                }
            }
            if (PlayerControl.LocalPlayer.isImpostor())
            {
                List<PlayerControl> impostors = PlayerControl.AllPlayerControls.ToArray().ToList();
                impostors.RemoveAll(x => !x.Data.Role.IsImpostor && !x.isRole(CustomRPC.RoleId.Egoist));
                foreach (PlayerControl player in impostors)
                    player.nameText.color = Palette.ImpostorRed;
                if (MeetingHud.Instance != null)
                    foreach (PlayerVoteArea player in MeetingHud.Instance.playerStates)
                    {
                        PlayerControl playerControl = ModHelpers.playerById((byte)player.TargetPlayerId);
                        if (playerControl != null && (playerControl.Data.Role.IsImpostor || playerControl.isRole(CustomRPC.RoleId.Egoist)))
                            player.NameText.color = Palette.ImpostorRed;
                    }
            }
            /*    if (PlayerControl.LocalPlayer.isFox())
                {
                    List<PlayerControl> foxes = PlayerControl.AllPlayerControls.ToArray().ToList();
                    foxes.RemoveAll(x => !x.isRole(CustomRPC.RoleId.Fox));
                    foreach (PlayerControl player in foxes)
                        player.nameText.color = Palette.Purple;
                    if (MeetingHud.Instance != null)
                        foreach (PlayerVoteArea player in MeetingHud.Instance.playerStates)
                        {
                            PlayerControl playerControl = ModHelpers.playerById((byte)player.TargetPlayerId);
                            if (playerControl != null && (playerControl.isRole(CustomRPC.RoleId.Fox)))
                                player.NameText.color = Palette.Purple;
                        }
                }*/
        }
        public static void SetPlayerRoleInfoView(PlayerControl p, Color roleColors, string roleNames)
        {
            if (p.IsBot()) return;
            bool commsActive = false;
            foreach (PlayerTask t in PlayerControl.LocalPlayer.myTasks)
            {
                if (t.TaskType == TaskTypes.FixComms)
                {
                    commsActive = true;
                    break;
                }
            }
            Transform playerInfoTransform = p.nameText.transform.parent.FindChild("Info");
            TMPro.TextMeshPro playerInfo = playerInfoTransform != null ? playerInfoTransform.GetComponent<TMPro.TextMeshPro>() : null;
            if (playerInfo == null)
            {
                playerInfo = UnityEngine.Object.Instantiate(p.nameText, p.nameText.transform.parent);
                playerInfo.fontSize *= 0.75f;
                playerInfo.gameObject.name = "Info";
            }

            // Set the position every time bc it sometimes ends up in the wrong place due to camoflauge
            playerInfo.transform.localPosition = p.nameText.transform.localPosition + Vector3.up * 0.5f;

            PlayerVoteArea playerVoteArea = MeetingHud.Instance?.playerStates?.FirstOrDefault(x => x.TargetPlayerId == p.PlayerId);
            Transform meetingInfoTransform = playerVoteArea != null ? playerVoteArea.NameText.transform.parent.FindChild("Info") : null;
            TMPro.TextMeshPro meetingInfo = meetingInfoTransform != null ? meetingInfoTransform.GetComponent<TMPro.TextMeshPro>() : null;
            if (meetingInfo == null && playerVoteArea != null)
            {
                meetingInfo = UnityEngine.Object.Instantiate(playerVoteArea.NameText, playerVoteArea.NameText.transform.parent);
                meetingInfo.transform.localPosition += Vector3.down * 0.1f;
                meetingInfo.fontSize = 1.5f;
                meetingInfo.gameObject.name = "Info";
            }

            // Set player name higher to align in middle
            if (meetingInfo != null && playerVoteArea != null)
            {
                var playerName = playerVoteArea.NameText;
                playerName.transform.localPosition = new Vector3(0.3384f, (0.0311f + 0.0683f), -0.1f);
            }
            string TaskText = "";
            if (!p.isImpostor())
            {
                try
                {
                    if (commsActive)
                    {
                        var all = TaskCount.TaskDateNoClearCheck(p.Data).Item2;
                        TaskText += ModHelpers.cs(Color.yellow, "(?/" + all + ")");
                    }
                    else
                    {
                        var (complate, all) = TaskCount.TaskDateNoClearCheck(p.Data);
                        TaskText += ModHelpers.cs(Color.yellow, "(" + complate + "/" + all + ")");
                    }
                }
                catch
                {

                }
            }
            string playerInfoText = "";
            string meetingInfoText = "";
            playerInfoText = $"{CustomOptions.cs(roleColors, roleNames)}{TaskText}";
            meetingInfoText = $"{CustomOptions.cs(roleColors, roleNames)}{TaskText}".Trim();
            playerInfo.text = playerInfoText;
            playerInfo.gameObject.SetActive(p.Visible);
            if (meetingInfo != null) meetingInfo.text = MeetingHud.Instance.state == MeetingHud.VoteStates.Results ? "" : meetingInfoText; p.nameText.color = roleColors;
        }
        public static void SetPlayerRoleInfo(PlayerControl p)
        {
            if (p.IsBot()) return;
            string roleNames;
            Color roleColors;
            var role = p.getRole();
            if (role == CustomRPC.RoleId.DefaultRole || (role == CustomRPC.RoleId.Bestfalsecharge && p.isAlive())) {
                if (p.Data.Role.IsImpostor)
                {
                    roleNames = "ImpostorName";
                    roleColors = Roles.RoleClass.ImpostorRed;
                }
                else
                {
                    roleNames = "CrewMateName";
                    roleColors = Roles.RoleClass.CrewmateWhite;
                }
            } else
            {
                var introdate = Intro.IntroDate.GetIntroDate(role);
                roleNames = introdate.NameKey + "Name";
                roleColors = introdate.color;
            }
            SetPlayerRoleInfoView(p, roleColors, roleNames);
        }
        public static void SetPlayerNameColors(PlayerControl player)
        {
            var role = player.getRole();
            if (role == CustomRPC.RoleId.DefaultRole || (role == CustomRPC.RoleId.Bestfalsecharge && player.isAlive())) return;
            SetPlayerNameColor(player, Intro.IntroDate.GetIntroDate(role).color);
        }
        public static void SetPlayerRoleNames(PlayerControl player)
        {
            SetPlayerRoleInfo(player);
        }
        public static void QuarreledSet()
        {
            string suffix = ModHelpers.cs(RoleClass.Quarreled.color, "○");
            if (PlayerControl.LocalPlayer.IsQuarreled() && PlayerControl.LocalPlayer.isAlive())
            {
                PlayerControl side = PlayerControl.LocalPlayer.GetOneSideQuarreled();
                SetPlayerNameText(PlayerControl.LocalPlayer, PlayerControl.LocalPlayer.nameText.text + suffix);
                if (!side.Data.Disconnected)
                {
                    SetPlayerNameText(side, side.nameText.text + suffix);
                }
            }
            if (!PlayerControl.LocalPlayer.isAlive() && RoleClass.Quarreled.QuarreledPlayer != new List<List<PlayerControl>>())
            {
                foreach (List<PlayerControl> ps in RoleClass.Quarreled.QuarreledPlayer) {
                    foreach (PlayerControl p in ps)
                    {
                        if (!p.Data.Disconnected)
                        {
                            SetPlayerNameText(p, p.nameText.text + suffix);
                        }
                    }
                }
            }
        }
        public static void LoversSet()
        {
            string suffix = ModHelpers.cs(RoleClass.Lovers.color, " ♥");
            if (PlayerControl.LocalPlayer.IsLovers() && PlayerControl.LocalPlayer.isAlive())
            {
                PlayerControl side = PlayerControl.LocalPlayer.GetOneSideLovers();
                SetPlayerNameText(PlayerControl.LocalPlayer, PlayerControl.LocalPlayer.nameText.text + suffix);
                if (!side.Data.Disconnected)
                {
                    SetPlayerNameText(side, side.nameText.text + suffix);
                }
            }
            if ((PlayerControl.LocalPlayer.isDead() || PlayerControl.LocalPlayer.isRole(RoleId.God))&& RoleClass.Lovers.LoversPlayer != new List<List<PlayerControl>>())
            {
                foreach (List<PlayerControl> ps in RoleClass.Lovers.LoversPlayer)
                {
                    foreach (PlayerControl p in ps)
                    {
                        if (!p.Data.Disconnected)
                        {
                            SetPlayerNameText(p, p.nameText.text + suffix);
                        }
                    }
                }
            }
        }
        public static void DemonSet()
        {
            if (PlayerControl.LocalPlayer.isRole(RoleId.Demon) || PlayerControl.LocalPlayer.isDead() || PlayerControl.LocalPlayer.isRole(RoleId.God))
            {
                foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                {
                    if (Demon.IsViewIcon(player))
                    {
                        if (!player.nameText.text.Contains(ModHelpers.cs(RoleClass.Demon.color, " ▲")))
                        {
                            SetNamesClass.SetPlayerNameText(player, player.nameText.text + ModHelpers.cs(RoleClass.Demon.color, " ▲"));
                        }
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
            } else
            {
                foreach (PlayerControl p in RoleClass.Celebrity.CelebrityPlayer)
                {
                    SetPlayerNameColor(p, RoleClass.Celebrity.color);
                }
            }
        }
    }
    public class SetNameUpdate
    {
        public static void Postfix(PlayerControl __instance)
        {
            SetNamesClass.resetNameTagsAndColors();
            if (PlayerControl.LocalPlayer.isDead() && !PlayerControl.LocalPlayer.isRole(CustomRPC.RoleId.NiceRedRidingHood))
            {
                foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                {
                    SetNamesClass.SetPlayerNameColors(player);
                    SetNamesClass.SetPlayerRoleNames(player);
                }
            }
            else if (PlayerControl.LocalPlayer.isRole(CustomRPC.RoleId.God))
            {
                foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                {
                    if (RoleClass.IsMeeting || player.isAlive())
                    {
                        SetNamesClass.SetPlayerNameColors(player);
                        SetNamesClass.SetPlayerRoleNames(player);
                    }
                }
            }
            else
            {
                if (Madmate.CheckImpostor(PlayerControl.LocalPlayer) || PlayerControl.LocalPlayer.isRole(RoleId.MadKiller) || (RoleClass.Demon.IsCheckImpostor && PlayerControl.LocalPlayer.isRole(RoleId.Demon)))
                {
                    foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                    {
                        if (p.isImpostor())
                        {
                            SetNamesClass.SetPlayerNameColor(p, RoleClass.ImpostorRed);
                        }
                    }
                }
                if (PlayerControl.LocalPlayer.isImpostor())
                {
                    foreach (PlayerControl p in RoleClass.SideKiller.MadKillerPlayer)
                    {
                        SetNamesClass.SetPlayerNameColor(p, RoleClass.ImpostorRed);
                    }
                }
                if (JackalFriends.CheckJackal(PlayerControl.LocalPlayer))
                {
                    foreach (PlayerControl p in RoleClass.Jackal.JackalPlayer)
                    {
                        SetNamesClass.SetPlayerNameColors(p);
                        SetNamesClass.SetPlayerRoleNames(p);
                    }
                    foreach (PlayerControl p in RoleClass.Jackal.SidekickPlayer)
                    {
                        SetNamesClass.SetPlayerNameColors(p);
                        SetNamesClass.SetPlayerRoleNames(p);
                    }
                    foreach (PlayerControl p in RoleClass.TeleportingJackal.TeleportingJackalPlayer)
                    {
                        SetNamesClass.SetPlayerNameColors(p);
                        SetNamesClass.SetPlayerRoleNames(p);
                    }
                    foreach (PlayerControl p in RoleClass.JackalSeer.JackalSeerPlayer)
                    {
                        SetNamesClass.SetPlayerNameColors(p);
                        SetNamesClass.SetPlayerRoleNames(p);
                    }
                }
                if (SeerFriends.CheckJackal(PlayerControl.LocalPlayer))
                {
                    foreach (PlayerControl p in RoleClass.Jackal.JackalPlayer)
                    {
                        SetNamesClass.SetPlayerNameColors(p);
                        SetNamesClass.SetPlayerRoleNames(p);
                    }
                    foreach (PlayerControl p in RoleClass.Jackal.SidekickPlayer)
                    {
                        SetNamesClass.SetPlayerNameColors(p);
                        SetNamesClass.SetPlayerRoleNames(p);
                    }
                    foreach (PlayerControl p in RoleClass.TeleportingJackal.TeleportingJackalPlayer)
                    {
                        SetNamesClass.SetPlayerNameColors(p);
                        SetNamesClass.SetPlayerRoleNames(p);
                    }
                    foreach (PlayerControl p in RoleClass.JackalSeer.JackalSeerPlayer)
                    {
                        SetNamesClass.SetPlayerNameColors(p);
                        SetNamesClass.SetPlayerRoleNames(p);
                    }
                }
                if (PlayerControl.LocalPlayer.isRole(CustomRPC.RoleId.Jackal) || PlayerControl.LocalPlayer.isRole(CustomRPC.RoleId.Sidekick))
                {
                    foreach (PlayerControl p in RoleClass.Jackal.JackalPlayer)
                    {
                        if (p != PlayerControl.LocalPlayer)
                        {
                            SetNamesClass.SetPlayerNameColors(p);
                            SetNamesClass.SetPlayerRoleNames(p);
                        }
                    }
                    foreach (PlayerControl p in RoleClass.Jackal.SidekickPlayer)
                    {
                        if (p != PlayerControl.LocalPlayer)
                        {
                            SetNamesClass.SetPlayerRoleNames(p);
                            SetNamesClass.SetPlayerNameColors(p);
                        }
                    }
                    foreach (PlayerControl p in RoleClass.Jackal.FakeSidekickPlayer)
                    {
                        SetNamesClass.SetPlayerNameColor(p, RoleClass.Jackal.color);
                        SetNamesClass.SetPlayerRoleInfoView(p, RoleClass.Jackal.color, Intro.IntroDate.SidekickIntro.NameKey + "Name");
                    }
                    foreach (PlayerControl p in RoleClass.JackalSeer.JackalSeerPlayer)
                    {
                        if (p != PlayerControl.LocalPlayer)
                        {
                            SetNamesClass.SetPlayerNameColors(p);
                            SetNamesClass.SetPlayerRoleNames(p);
                        }
                    }
                }
                SetNamesClass.SetPlayerRoleNames(PlayerControl.LocalPlayer);
                SetNamesClass.SetPlayerNameColors(PlayerControl.LocalPlayer);
            }
            SetNamesClass.DemonSet();
            SetNamesClass.CelebritySet();
            SetNamesClass.QuarreledSet();
            SetNamesClass.LoversSet();
            try
            {
                if (ModeHandler.isMode(ModeId.Default))
                {
                    if (Sabotage.SabotageManager.thisSabotage == Sabotage.SabotageManager.CustomSabotage.CognitiveDeficit)
                    {
                        foreach (PlayerControl p3 in PlayerControl.AllPlayerControls)
                        {
                            if (p3.isAlive() && !Sabotage.CognitiveDeficit.main.OKPlayers.IsCheckListPlayerControl(p3))
                            {
                                if (PlayerControl.LocalPlayer.isImpostor())
                                {
                                    if (!(p3.isImpostor() || p3.isRole(CustomRPC.RoleId.MadKiller)))
                                    {
                                        SetNamesClass.SetPlayerNameColor(p3, new Color32(18, 112, 214, byte.MaxValue));
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch { }
        }
    }
}
