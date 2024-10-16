using System;
using System.Collections.Generic;
using System.Linq;
using SuperNewRoles.Mode;
using SuperNewRoles.Mode.PlusMode;
using SuperNewRoles.Mode.SuperHostRoles;
using SuperNewRoles.Patches;
using SuperNewRoles.Roles;
using SuperNewRoles.Roles.Attribute;
using SuperNewRoles.Roles.Crewmate;
using SuperNewRoles.Roles.CrewMate;
using SuperNewRoles.Roles.Neutral;
using SuperNewRoles.Roles.RoleBases;
using SuperNewRoles.Roles.RoleBases.Interfaces;
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
        PlayerVoteArea area;
        if ((area = CacheManager.GetVoteAreaById(p.PlayerId)) != null)
            area.NameText.color = color;
    }
    public static void SetPlayerNameText(PlayerControl p, string text)
    {
        if (p.IsBot()) return;
        p.NameText().text = text;
        PlayerVoteArea area;
        if ((area = CacheManager.GetVoteAreaById(p.PlayerId)) != null)
            area.NameText.text = text;
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
            if (
                PlayerControl.LocalPlayer.IsImpostor() && (
                  ModeHandler.IsMode(ModeId.HideAndSeek) ||
                  player.IsImpostorAddedFake())
                )
                SetPlayerNameColor(player, RoleClass.ImpostorRed);
            else
                SetPlayerNameColor(player, Color.white);
        }
    }
    public static Dictionary<byte, TextMeshPro> PlayerInfos = new();
    public static Dictionary<byte, TextMeshPro> MeetingPlayerInfos = new();

    public static void SetPlayerRoleInfoView(PlayerControl p, Color roleColors, string roleNames, Dictionary<string, (Color, bool)> attributeRoles, Color? GhostRoleColor = null, string GhostRoleNames = "")
    {
        bool commsActive = RoleHelpers.IsComms();
        if (!PlayerInfos.TryGetValue(p.PlayerId, out TextMeshPro playerInfo) || playerInfo == null)
        {
            playerInfo = UnityEngine.Object.Instantiate(p.NameText(), p.NameText().transform.parent);
            playerInfo.fontSize *= 0.75f;
            playerInfo.gameObject.name = "Info";
            PlayerInfos[p.PlayerId] = playerInfo;
        }

        // Set the position every time bc it sometimes ends up in the wrong place due to camoflauge
        playerInfo.transform.localPosition = p.NameText().transform.localPosition + Vector3.up * 0.2f;

        PlayerVoteArea playerVoteArea = CacheManager.GetVoteAreaById(p.PlayerId);
        if ((!MeetingPlayerInfos.TryGetValue(p.PlayerId, out TextMeshPro meetingInfo) || meetingInfo == null) && playerVoteArea != null)
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
            if (p.IsUseTaskTrigger())
            {
                var (complete, all) = TaskCount.TaskDateNoClearCheck(p.Data);
                TaskText += ModHelpers.Cs(Color.yellow, "(" + (commsActive ? "?" : complete.ToString()) + "/" + all.ToString() + ")");
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

        if (attributeRoles.Count != 0)
        {
            foreach (var kvp in attributeRoles)
            {
                if (!kvp.Value.Item2) continue;
                playerInfoText += $" + {CustomOptionHolder.Cs(kvp.Value.Item1, kvp.Key)}";
            }
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
            roleNames = IntroData.StefinderIntro.Name;
            roleColors = RoleClass.ImpostorRed;
        }
        else if (p.IsPavlovsTeam())
        {
            roleNames = ModTranslation.GetString($"{PavlovsDogs.Roleinfo.NameKey}Name") + (role == RoleId.Pavlovsdogs ? "(D)" : "(O)");
            roleColors = PavlovsDogs.PavlovsColor;
        }
        else
        {
            roleNames = CustomRoles.GetRoleName(role, p);
            roleColors = CustomRoles.GetRoleColor(role, p);
        }

        var GhostRole = p.GetGhostRole();
        if (GhostRole != RoleId.DefaultRole)
        {
            GhostroleNames = CustomRoles.GetRoleName(GhostRole, p);
            GhostroleColors = CustomRoles.GetRoleColor(GhostRole, p);
        }

        Dictionary<string, (Color, bool)> attributeRoles = new(AttributeRoleNameSet(p));

        SetPlayerRoleInfoView(p, roleColors, roleNames, attributeRoles, GhostroleColors, GhostroleNames);
    }

    /// <summary>
    /// 重複役職の役職名を追加する。
    /// key = 役職名, value.Item1 = 役職カラー, value.Item2 = 役職名の表示条件を達しているか,
    /// </summary>
    /// <param name="player">役職名を表示したいプレイヤー</param>
    /// <param name="seePlayer">役職名を見るプレイヤー</param>
    internal static Dictionary<string, (Color, bool)> AttributeRoleNameSet(PlayerControl player, PlayerControl seePlayer = null)
    {
        Dictionary<string, (Color, bool)> attributeRoles = new();
        if (player.IsHauntedWolf())
        {
            if (seePlayer == null) seePlayer = PlayerControl.LocalPlayer;
            var isSeeing = seePlayer.IsDead() || seePlayer.IsRole(RoleId.God, RoleId.Marlin);
            attributeRoles.Add(ModTranslation.GetString("HauntedWolfName"), (HauntedWolf.RoleData.color, isSeeing));
        }
        return attributeRoles;
    }

    /// <summary>
    /// 死亡後, 全員の役職が見る事ができるか判定する。
    /// 見る事ができない状態は, 見る事ができる状態より優先して反映する。
    /// </summary>
    /// <param name="target">役職を見ようとしているプレイヤー (nullなら処理者本人)</param>
    /// <returns> true:見られる / false:見られない </returns>
    public static bool CanGhostSeeRoles(PlayerControl target = null)
    {
        if (target == null) target = PlayerControl.LocalPlayer;
        if (target.IsDead())
        {
            if (target.GetRoleBase() is INameHandler INameHandler) // 役職の個別判定
            {
                if (!INameHandler.CanGhostSeeRole) return false;  // 死後 役職をみえない役職の場合, 最優先で判定する。
            }

            // ゲーム設定による, 基本的な判定
            // (役職の個別判定で死後役職が見られる場合は, ゲーム設定による判定を優先する)
            if (!Mode.PlusMode.PlusGameOptions.PlusGameOptionSetting.GetBool()) return true; // 上位設定 "ゲームオプション" が有効か
            else
            {
                if (!Mode.PlusMode.PlusGameOptions.CanNotGhostSeeRole.GetBool()) return true;
                else if (Mode.PlusMode.PlusGameOptions.OnlyImpostorGhostSeeRole.GetBool()) return target.IsImpostor();
            }
        }
        return false;
    }

    public static void SetPlayerNameColors(PlayerControl player)
    {
        var role = player.GetRole();
        if (role == RoleId.DefaultRole || (role == RoleId.Bestfalsecharge && player.IsAlive())) return;
        SetPlayerNameColor(player, CustomRoles.GetRoleColor(player));
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
        if (CanGhostSeeRoles() && RoleClass.Quarreled.QuarreledPlayer != new List<List<PlayerControl>>())
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
        else if ((CanGhostSeeRoles() || PlayerControl.LocalPlayer.IsRole(RoleId.God)) && RoleClass.Lovers.LoversPlayer != new List<List<PlayerControl>>())
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
        if (PlayerControl.LocalPlayer.IsRole(RoleId.Demon, RoleId.God) || CanGhostSeeRoles())
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
        if (PlayerControl.LocalPlayer.IsRole(RoleId.Arsonist, RoleId.God) || CanGhostSeeRoles())
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
        foreach (PlayerControl p in
            RoleClass.Celebrity.ChangeRoleView ?
            RoleClass.Celebrity.ViewPlayers :
            RoleClass.Celebrity.CelebrityPlayer)
        {
            SetPlayerNameColor(p, RoleClass.Celebrity.color);
        }
    }
    public static void SatsumaimoSet()
    {
        if (CanGhostSeeRoles() || PlayerControl.LocalPlayer.IsRole(RoleId.God))
        {
            foreach (SatsumaAndImo player in RoleBaseManager.GetRoleBases<SatsumaAndImo>())
            {
                //クルーなら
                if (!player.Player.NameText().text.Contains(ModHelpers.Cs(RoleClass.Arsonist.color, " (C)")) && player.TeamState == SatsumaAndImo.SatsumaTeam.Crewmate)
                {//名前に(C)をつける
                    SetNamesClass.SetPlayerNameText(player.Player, player.Player.NameText().text + ModHelpers.Cs(Palette.White, " (C)"));
                }
                if (!player.Player.NameText().text.Contains(ModHelpers.Cs(RoleClass.ImpostorRed, " (M)")) && player.TeamState == SatsumaAndImo.SatsumaTeam.Madmate)
                {
                    SetNamesClass.SetPlayerNameText(player.Player, player.Player.NameText().text + ModHelpers.Cs(RoleClass.ImpostorRed, " (M)"));
                }
            }
        }
        else if (PlayerControl.LocalPlayer.IsRole(RoleId.SatsumaAndImo))
        {
            PlayerControl player = PlayerControl.LocalPlayer;
            SatsumaAndImo satsumaAndImo = RoleBaseManager.GetLocalRoleBase<SatsumaAndImo>();
            if (satsumaAndImo == null)
                return;
            if (!player.NameText().text.Contains(ModHelpers.Cs(Palette.White, " (C)")) && satsumaAndImo.TeamState == SatsumaAndImo.SatsumaTeam.Crewmate)
            {//名前に(C)をつける
                SetNamesClass.SetPlayerNameText(player, player.NameText().text + ModHelpers.Cs(Palette.White, " (C)"));
            }
            else if (!player.NameText().text.Contains(ModHelpers.Cs(RoleClass.ImpostorRed, " (M)")) && satsumaAndImo.TeamState == SatsumaAndImo.SatsumaTeam.Madmate)
            {
                SetNamesClass.SetPlayerNameText(player, player.NameText().text + ModHelpers.Cs(RoleClass.ImpostorRed, " (M)"));
            }
        }
    }
    public static void AttributeGuesserSet()
    {
        if (CanGhostSeeRoles() || PlayerControl.LocalPlayer.IsRole(RoleId.God))
        {
            foreach (PlayerControl player in AttributeGuesser.AttributeGuesserPlayer)
            {
                if (!player.NameText().text.Contains(ModHelpers.Cs(NiceGuesser.Roleinfo.RoleColor, " ⊕")))
                    SetNamesClass.SetPlayerNameText(player, player.NameText().text + ModHelpers.Cs(NiceGuesser.Roleinfo.RoleColor, " ⊕"));
            }
        }
        else if (PlayerControl.LocalPlayer.IsAttributeGuesser())
        {
            if (!PlayerControl.LocalPlayer.NameText().text.Contains(ModHelpers.Cs(NiceGuesser.Roleinfo.RoleColor, " ⊕")))
                SetNamesClass.SetPlayerNameText(PlayerControl.LocalPlayer, PlayerControl.LocalPlayer.NameText().text + ModHelpers.Cs(NiceGuesser.Roleinfo.RoleColor, " ⊕"));
        }
    }
}
public class SetNameUpdate
{
    public static void Postfix(PlayerControl __instance)
    {
        SetNamesClass.ResetNameTagsAndColors();
        RoleId LocalRole = PlayerControl.LocalPlayer.GetRole();
        bool CanSeeAllRole =
            SetNamesClass.CanGhostSeeRoles() ||
            Debugger.canSeeRole ||
            (PlayerControl.LocalPlayer.GetRoleBase() is INameHandler nameHandler &&
            nameHandler.AmAllRoleVisible);
        if (CanSeeAllRole)
        {
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            {
                SetNamesClass.SetPlayerNameColors(player);
                SetNamesClass.SetPlayerRoleNames(player);
            }
        }
        //TODO:神移行時にINameHandlerに移行する
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
                    if (p.IsImpostorAddedFake())
                    {
                        SetNamesClass.SetPlayerNameColor(p, RoleClass.ImpostorRed);
                    }
                }
            }
            int canSeeImpostorRoleTurnRemaining = PlusGameOptions.CanSeeImpostorRoleTurn.GetInt() - ReportDeadBodyPatch.MeetingCount.all;
            if (PlayerControl.LocalPlayer.IsImpostor() &&
                CustomOptionHolder.EgoistOption.GetSelection() is 0 && CustomOptionHolder.SpyOption.GetSelection() is 0 &&
                (canSeeImpostorRoleTurnRemaining < 0 ||
                (canSeeImpostorRoleTurnRemaining == 0 && !RoleClass.IsMeeting)))
                //会議開始時に1減らすので会議が終わってから見えるように
            {
                foreach (PlayerControl p in CachedPlayer.AllPlayers)
                {
                    if (p.IsImpostorAddedFake())
                    {
                        SetNamesClass.SetPlayerRoleNames(p);
                    }
                }
            }
            switch (LocalRole)
            {
                case RoleId.Finder:
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
                    break;
                case RoleId.Dependents:
                    foreach (PlayerControl p in RoleClass.Vampire.VampirePlayer)
                    {
                        SetNamesClass.SetPlayerNameColors(p);
                    }
                    break;
                case RoleId.Vampire:
                    foreach (PlayerControl p in RoleClass.Dependents.DependentsPlayer)
                    {
                        SetNamesClass.SetPlayerNameColors(p);
                    }
                    break;
                case RoleId.PartTimer:
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
                    break;
                case RoleId.Fox:
                case RoleId.FireFox:
                    List<PlayerControl> foxs = new(RoleClass.Fox.FoxPlayer);
                    foxs.AddRange(FireFox.FireFoxPlayer);
                    foreach (PlayerControl p in foxs)
                    {
                        if (FireFox.FireFoxIsCheckFox.GetBool() || p.IsRole(PlayerControl.LocalPlayer.GetRole()))
                        {
                            SetNamesClass.SetPlayerRoleNames(p);
                            SetNamesClass.SetPlayerNameColors(p);
                        }
                    }
                    break;
                case RoleId.TheFirstLittlePig:
                case RoleId.TheSecondLittlePig:
                case RoleId.TheThirdLittlePig:
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
                    break;
                case RoleId.OrientalShaman:
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
                    break;
                case RoleId.ShermansServant:
                    foreach (var date in OrientalShaman.OrientalShamanCausative)
                    {
                        if (date.Value != PlayerControl.LocalPlayer.PlayerId) continue;
                        SetNamesClass.SetPlayerRoleNames(ModHelpers.PlayerById(date.Key));
                        SetNamesClass.SetPlayerNameColors(ModHelpers.PlayerById(date.Key));
                    }
                    break;
                case RoleId.Pokerface:
                    Pokerface.PokerfaceTeam team = Pokerface.GetPokerfaceTeam(PlayerControl.LocalPlayer.PlayerId);
                    if (team != null)
                    {
                        foreach (PlayerControl member in team.TeamPlayers)
                        {
                            SetNamesClass.SetPlayerNameColor(member, Pokerface.RoleData.color);
                        }
                    }
                    break;
            }
            if (PlayerControl.LocalPlayer.IsImpostor())
            {
                foreach (PlayerControl p in RoleClass.SideKiller.MadKillerPlayer)
                {
                    SetNamesClass.SetPlayerNameColor(p, RoleClass.ImpostorRed);
                }
            }
            if ((PlayerControl.LocalPlayer.IsJackalTeam() && !PlayerControl.LocalPlayer.IsFriendRoles()) ||
                JackalFriends.CheckJackal(PlayerControl.LocalPlayer))
            {
                foreach (PlayerControl p in CachedPlayer.AllPlayers)
                {
                    if (p.PlayerId == PlayerControl.LocalPlayer.PlayerId)
                        continue;
                    if (!p.IsJackalTeam())
                        continue;
                    if (p.IsFriendRoles() && !p.IsRole(RoleId.Bullet))
                        continue;
                    SetNamesClass.SetPlayerRoleNames(p);
                    SetNamesClass.SetPlayerNameColors(p);
                }
            }
            SetNamesClass.SetPlayerRoleNames(PlayerControl.LocalPlayer);
            SetNamesClass.SetPlayerNameColors(PlayerControl.LocalPlayer);
        }
        CustomRoles.NameHandler(CanSeeAllRole);
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
            SetNamesClass.ArsonistSet();
            SetNamesClass.DemonSet();
            SetNamesClass.CelebritySet();
            SetNamesClass.QuarreledSet();
            SetNamesClass.LoversSet();
        }
        SetNamesClass.SatsumaimoSet();
        SetNamesClass.JumboSet();
        SetNamesClass.AttributeGuesserSet();

        if (RoleClass.PartTimer.Data.ContainsValue(CachedPlayer.LocalPlayer.PlayerId))
        {
            PlayerControl PartTimerTarget = RoleClass.PartTimer.Data.GetPCByValue(PlayerControl.LocalPlayer.PlayerId);
            SetNamesClass.SetPlayerRoleNames(PartTimerTarget);
            SetNamesClass.SetPlayerNameColors(PartTimerTarget);
        }

        if (RoleClass.Stefinder.IsKill)
        {
            SetNamesClass.SetPlayerNameColor(PlayerControl.LocalPlayer, Color.red);
        }
        // インポスターのプレイヤーから認識障害サボを直していない人を識別できるようにする
        if (ModeHandler.IsMode(ModeId.Default) &&
            Sabotage.SabotageManager.thisSabotage == Sabotage.SabotageManager.CustomSabotage.CognitiveDeficit)
        {
            foreach (PlayerControl p3 in CachedPlayer.AllPlayers)
            {
                if (p3.IsDead() || Sabotage.CognitiveDeficit.Main.OKPlayers.IsCheckListPlayerControl(p3))
                    continue;
                if (!PlayerControl.LocalPlayer.IsImpostor())
                    continue;
                if (!p3.IsImpostorAddedFake() && !p3.IsRole(RoleId.MadKiller))
                    continue;
                SetNamesClass.SetPlayerNameColor(p3, new Color32(18, 112, 214, byte.MaxValue));
            }
        }
    }
}