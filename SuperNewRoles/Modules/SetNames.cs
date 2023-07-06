using System;
using System.Collections.Generic;
using System.Linq;
using SuperNewRoles.Mode;
using SuperNewRoles.Patches;
using SuperNewRoles.Roles;
using SuperNewRoles.Roles.Neutral;
using TMPro;
using UnityEngine;

namespace SuperNewRoles.Modules;

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
            if ((PlayerControl.LocalPlayer.IsImpostor() && (player.IsImpostor() || player.IsRole(RoleId.Spy, RoleId.Egoist))) || (ModeHandler.IsMode(ModeId.HideAndSeek) && player.IsImpostor()))
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
        playerInfo.transform.localPosition = p.NameText().transform.localPosition + Vector3.up * 0.2f;

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
        playerInfoText = $"{CustomOptionHolder.Cs(roleColors, roleNames)}";
        if (GhostRoleNames != "")
        {
            playerInfoText = $"{CustomOptionHolder.Cs((Color)GhostRoleColor, GhostRoleNames)}({playerInfoText})";
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
                roleNames = "CrewmateName";
                roleColors = RoleClass.CrewmateWhite;
            }
        }
        else if (role == RoleId.Stefinder && RoleClass.Stefinder.IsKill)
        {
            var introData = IntroData.GetIntroData(role, p);
            roleNames = introData.Name;
            roleColors = RoleClass.ImpostorRed;
        }
        else if (p.IsPavlovsTeam())
        {
            var introData = IntroData.PavlovsdogsIntro;
            roleNames = introData.Name + (role == RoleId.Pavlovsdogs ? "(D)" : "(O)");
            roleColors = RoleClass.Pavlovsdogs.color;
        }
        else if (WaveCannonJackal.IwasSidekicked.Contains(p.PlayerId) &&
                !WaveCannonJackal.WaveCannonJackalNewJackalHaveWaveCannon.GetBool())
        {
            if (p.IsRole(RoleId.WaveCannonJackal))
            {
                var introData = IntroData.GetIntroData(RoleId.Jackal, p);
                roleNames = introData.Name;
                roleColors = introData.color;
            }
            else
            {
                var introData = IntroData.GetIntroData(RoleId.Sidekick, p);
                roleNames = introData.Name;
                roleColors = introData.color;
            }
        }
        else
        {
            var introData = IntroData.GetIntroData(role, p);
            roleNames = introData.Name;
            roleColors = introData.color;
        }
        var GhostRole = p.GetGhostRole();
        if (GhostRole != RoleId.DefaultRole)
        {
            var GhostIntro = IntroData.GetIntroData(GhostRole, p);
            GhostroleNames = GhostIntro.Name;
            GhostroleColors = GhostIntro.color;
        }
        SetPlayerRoleInfoView(p, roleColors, roleNames, GhostroleColors, GhostroleNames);
    }
    /// <summary>
    /// 死亡後役職が見えるかの基本的な条件を取得する　(全員が役職を見られるか/Impostorのみ役職が見られるか)
    /// </summary>
    /// <returns> true:見られる / false:見られない </returns>
    public static bool DefaultGhostSeeRoles(PlayerControl target = null)
    {
        if (target == null) target = PlayerControl.LocalPlayer;
        if (target.IsDead())
        {
            if (!Mode.PlusMode.PlusGameOptions.PlusGameOptionSetting.GetBool()) return true;
            else
            {
                if (!Mode.PlusMode.PlusGameOptions.CanNotGhostSeeRole.GetBool()) return true; // 「死亡時に他プレイヤーの役職を表示しない」設定が無効な時
                // この設定は、上記bool判定の子Optionである為、上記true時（親Option無効時）取得しない設定。
                else if (Mode.PlusMode.PlusGameOptions.OnlyImpostorGhostSeeRole.GetBool()) return target.IsImpostor();
            }
        }
        return false; // 上記[役職が確認できる]条件を満たさなかった場合falseを返す。
    }

    public static void SetPlayerNameColors(PlayerControl player)
    {
        var role = player.GetRole();
        if (role == RoleId.DefaultRole || (role == RoleId.Bestfalsecharge && player.IsAlive())) return;
        SetPlayerNameColor(player, IntroData.GetIntroData(role).color);
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
        if (DefaultGhostSeeRoles() && RoleClass.Quarreled.QuarreledPlayer != new List<List<PlayerControl>>())
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
    public static void JumboSet()
    {
        foreach (PlayerControl p in RoleClass.Jumbo.BigPlayer)
        {
            if (!RoleClass.Jumbo.JumboSize.ContainsKey(p.PlayerId)) continue;
            SetPlayerNameText(p, p.NameText().text + $"({(int)(RoleClass.Jumbo.JumboSize[p.PlayerId] * 15)})");
        }
    }

    public static void LoversSet()
    {
        string suffix = ModHelpers.Cs(RoleClass.Lovers.color, " ♥");
        if ((PlayerControl.LocalPlayer.IsLovers() || (PlayerControl.LocalPlayer.IsFakeLovers() && !PlayerControl.LocalPlayer.IsFakeLoversFake())) && PlayerControl.LocalPlayer.IsAlive())
        {
            PlayerControl side = PlayerControl.LocalPlayer.GetOneSideLovers();
            if (side == null) side = PlayerControl.LocalPlayer.GetOneSideFakeLovers();
            SetPlayerNameText(PlayerControl.LocalPlayer, PlayerControl.LocalPlayer.NameText().text + suffix);
            if (!side.Data.Disconnected)
                SetPlayerNameText(side, side.NameText().text + suffix);
        }
        else if (PlayerControl.LocalPlayer.IsRole(RoleId.Cupid) && RoleClass.Cupid.Created && RoleClass.Cupid.currentLovers != null)
        {
            PlayerControl side = RoleClass.Cupid.currentLovers.GetOneSideLovers();
            SetPlayerNameText(RoleClass.Cupid.currentLovers, $"{RoleClass.Cupid.currentLovers.NameText().text}{suffix}");
            if (!side.Data.Disconnected)
                SetPlayerNameText(side, $"{side.NameText().text}{suffix}");
        }
        else if ((DefaultGhostSeeRoles() || PlayerControl.LocalPlayer.IsRole(RoleId.God)) && RoleClass.Lovers.LoversPlayer != new List<List<PlayerControl>>())
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
        if (PlayerControl.LocalPlayer.IsRole(RoleId.Demon) || DefaultGhostSeeRoles() || PlayerControl.LocalPlayer.IsRole(RoleId.God))
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
        if (PlayerControl.LocalPlayer.IsRole(RoleId.Arsonist) || DefaultGhostSeeRoles() || PlayerControl.LocalPlayer.IsRole(RoleId.God))
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
        if (DefaultGhostSeeRoles() || PlayerControl.LocalPlayer.IsRole(RoleId.God))
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
        if ((SetNamesClass.DefaultGhostSeeRoles() && LocalRole != RoleId.NiceRedRidingHood) || Roles.Attribute.Debugger.canSeeRole)
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
                LocalRole == RoleId.Marlin ||
                (RoleClass.Demon.IsCheckImpostor && LocalRole == RoleId.Demon) ||
                (LocalRole == RoleId.Safecracker && Safecracker.CheckTask(__instance, Safecracker.CheckTasks.CheckImpostor)))
            {
                foreach (PlayerControl p in CachedPlayer.AllPlayers)
                {
                    if (p.IsImpostor() || p.IsRole(RoleId.Spy, RoleId.Egoist))
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
                    if ((p.IsJackalTeamJackal() || p.IsJackalTeamSidekick()) && p.PlayerId != CachedPlayer.LocalPlayer.PlayerId)
                    {
                        SetNamesClass.SetPlayerRoleNames(p);
                        SetNamesClass.SetPlayerNameColors(p);
                    }
                }
            }
            if (LocalRole == RoleId.Dependents)
            {
                foreach (PlayerControl p in RoleClass.Vampire.VampirePlayer)
                {
                    SetNamesClass.SetPlayerNameColors(p);
                }
            }
            else if (LocalRole == RoleId.Vampire)
            {
                foreach (PlayerControl p in RoleClass.Dependents.DependentsPlayer)
                {
                    SetNamesClass.SetPlayerNameColors(p);
                }
            }
            else if (LocalRole == RoleId.PartTimer)
            {
                if (RoleClass.PartTimer.IsLocalOn)
                {
                    if (CustomOptionHolder.PartTimerIsCheckTargetRole.GetBool())
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
            else if (LocalRole is RoleId.Fox or RoleId.FireFox)
            {
                List<PlayerControl> foxs = new(RoleClass.Fox.FoxPlayer);
                foxs.AddRange(FireFox.FireFoxPlayer);
                foreach (PlayerControl p in foxs)
                {
                    if (p.IsRole(PlayerControl.LocalPlayer.GetRole()) || FireFox.FireFoxIsCheckFox.GetBool())
                    {
                        SetNamesClass.SetPlayerRoleNames(p);
                        SetNamesClass.SetPlayerNameColors(p);
                    }
                }
            }
            else if (LocalRole is RoleId.TheFirstLittlePig or RoleId.TheSecondLittlePig or RoleId.TheThirdLittlePig)
            {
                foreach (var players in TheThreeLittlePigs.TheThreeLittlePigsPlayer)
                {
                    if (players.TrueForAll(x => x.PlayerId != PlayerControl.LocalPlayer.PlayerId)) continue;
                    foreach (PlayerControl p in players)
                    {
                        SetNamesClass.SetPlayerRoleNames(p);
                        SetNamesClass.SetPlayerNameColors(p);
                    }
                    break;
                }
            }
            else if (LocalRole is RoleId.OrientalShaman)
            {
                foreach (var date in OrientalShaman.OrientalShamanCausative)
                {
                    if (date.Key != PlayerControl.LocalPlayer.PlayerId) continue;
                    SetNamesClass.SetPlayerRoleNames(ModHelpers.PlayerById(date.Value));
                    SetNamesClass.SetPlayerNameColors(ModHelpers.PlayerById(date.Value));
                }
                foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                {
                    if (OrientalShaman.IsKiller(player))
                        SetNamesClass.SetPlayerNameColors(player);
                }
            }
            else if (LocalRole is RoleId.ShermansServant)
            {
                foreach (var date in OrientalShaman.OrientalShamanCausative)
                {
                    if (date.Value != PlayerControl.LocalPlayer.PlayerId) continue;
                    SetNamesClass.SetPlayerRoleNames(ModHelpers.PlayerById(date.Key));
                    SetNamesClass.SetPlayerNameColors(ModHelpers.PlayerById(date.Key));
                }
            }
            SetNamesClass.SetPlayerRoleNames(PlayerControl.LocalPlayer);
            SetNamesClass.SetPlayerNameColors(PlayerControl.LocalPlayer);
        }

        //名前の奴
        if (RoleClass.Camouflager.IsCamouflage)
        {
            if (RoleClass.Camouflager.ArsonistMark)
                SetNamesClass.ArsonistSet();
            if (RoleClass.Camouflager.DemonMark)
                SetNamesClass.DemonSet();
            if (RoleClass.Camouflager.LoversMark)
                SetNamesClass.LoversSet();
            if (RoleClass.Camouflager.QuarreledMark)
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
        SetNamesClass.JumboSet();

        if (RoleClass.PartTimer.Data.ContainsValue(CachedPlayer.LocalPlayer.PlayerId))
        {
            PlayerControl PartTimerTarget = ModHelpers.PlayerById((byte)RoleClass.PartTimer.Data.GetKey(CachedPlayer.LocalPlayer.PlayerId));
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