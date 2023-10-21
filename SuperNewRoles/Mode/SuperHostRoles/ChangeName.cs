using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SuperNewRoles.Helpers;
using SuperNewRoles.Patches;
using SuperNewRoles.Roles;
using SuperNewRoles.Roles.Neutral;
using UnityEngine;

namespace SuperNewRoles.Mode.SuperHostRoles;
public static class ChangeName
{
    private static string GetNowName(this PlayerData<string> pd, PlayerControl p)
    {
        if (!pd.TryGetValue(p, out string result))
            result = p.GetDefaultName();
        return result;
    }

    public static void SetRoleName(PlayerControl player, bool IsUnchecked = false)
    {
        //SHRではない場合は処理しない
        if (!ModeHandler.IsMode(ModeId.SuperHostRoles)) return;
        //デバッグが有効の場合は叩いた場所を表示する
        if (DebugModeManager.IsDebugMode)
        {
            var caller = new System.Diagnostics.StackFrame(1, false);
            var callerMethod = caller.GetMethod();
            string callerMethodName = callerMethod.Name;
            string callerClassName = callerMethod.DeclaringType.FullName;
            Logger.Info("[SHR:ChangeName]" + player.name + "への(IsCommsなしの)SetRoleNameが" + callerClassName + "." + callerMethodName + "から呼び出されました。");
        }
        //コミュ情報を取得して呼ぶ
        SetRoleName(player, RoleHelpers.IsComms(), IsUnchecked);
    }
    private static string GetPlayerName(this PlayerControl player)
    {
        if (ModeHandler.IsMode(ModeId.HideAndSeek))
        {
            if (player.IsImpostor())
                return ModHelpers.Cs(RoleClass.ImpostorRed, player.GetDefaultName());
        }
        return player.GetDefaultName();
    }
    public static void SetDefaultNames()
    {
        if (DebugModeManager.IsDebugMode)
        {
            var caller = new System.Diagnostics.StackFrame(1, false);
            var callerMethod = caller.GetMethod();
            string callerMethodName = callerMethod.Name;
            string callerClassName = callerMethod.DeclaringType.FullName;
            SuperNewRolesPlugin.Logger.LogInfo("SetDefaultNamesが" + callerClassName + "." + callerMethodName + "から呼び出されました。");
        }
        foreach (PlayerControl p in CachedPlayer.AllPlayers)
        {
            p.RpcSetName(p.GetDefaultName());
        }
    }
    public static void SetRoleNames(bool IsUnchecked = false)
    {
        //SHRではない場合は処理しない
        if (!ModeHandler.IsMode(ModeId.SuperHostRoles)) return;
        if (DebugModeManager.IsDebugMode)
        {
            var caller = new System.Diagnostics.StackFrame(1, false);
            var callerMethod = caller.GetMethod();
            string callerMethodName = callerMethod.Name;
            string callerClassName = callerMethod.DeclaringType.FullName;
            SuperNewRolesPlugin.Logger.LogInfo("[SHR:ChangeName] SetRoleNamesが" + callerClassName + "." + callerMethodName + "から呼び出されました。");
        }

        bool commsActive = RoleHelpers.IsComms();
        foreach (PlayerControl p in CachedPlayer.AllPlayers)
        {
            SetRoleName(p, commsActive, IsUnchecked);
        }
    }
    public static void SetRoleName(PlayerControl player, bool commsActive, bool IsUnchecked = false)
    {
        //SHRではない場合は処理しない
        if (!ModeHandler.IsMode(ModeId.SuperHostRoles)) return;
        //処理できない場合は処理しない
        if (player.IsBot() || !AmongUsClient.Instance.AmHost) return;

        //デバッグが有効の場合は叩いた場所を表示する
        if (DebugModeManager.IsDebugMode)
        {
            var caller = new System.Diagnostics.StackFrame(1, false);
            var callerMethod = caller.GetMethod();
            string callerMethodName = callerMethod.Name;
            string callerClassName = callerMethod.DeclaringType.FullName;
            SuperNewRolesPlugin.Logger.LogInfo("[SHR: ChangeName]" + player.name + "へのSetRoleNameが" + callerClassName + "." + callerMethodName + "から呼び出されました。");
        }

        //if (UpdateTime.ContainsKey(player.PlayerId) && UpdateTime[player.PlayerId] > 0) return;

        //UpdateTime[player.PlayerId] = UpdateDefaultTime;

        HashSet<PlayerControl> CanAllRolePlayers = new();
        HashSet<PlayerControl> AlivePlayers = new();
        foreach (PlayerControl p in CachedPlayer.AllPlayers)
        {
            //導入者ではないかつ
            if (!p.IsMod() &&
                //本人ではないかつ
                p.PlayerId != player.PlayerId &&
                //BOTではない
                !p.IsBot())
            {
                //神、もしくは死亡していてかつ役職が見れる場合
                if (SetNamesClass.DefaultGhostSeeRoles(p) || p.IsRole(RoleId.God))
                    CanAllRolePlayers.Add(p);
                else
                    AlivePlayers.Add(p);
            }
        }
        bool IsHideAndSeek = ModeHandler.IsMode(ModeId.HideAndSeek);
        //必要がないなら処理しない
        if (player.IsMod() && CanAllRolePlayers.Count < 1 && (!IsHideAndSeek || !player.IsImpostor())) return;

        RoleId PlayerRole = player.GetRole();

        var introData = IntroData.GetIntroData(PlayerRole, player);
        StringBuilder NewName = new();
        StringBuilder MySuffix = new();
        StringBuilder RoleNameText = new(ModHelpers.Cs(introData.color, introData.Name));
        PlayerData<string> ChangePlayers = new(needplayerlist: true);

        // カモフラ中は処理を破棄する
        if (!RoleClass.Camouflager.IsCamouflage)
        {
            // スターパッシブ能力 [ カリスマ ] の処理
            List<PlayerControl> CelebrityViewPlayers = RoleClass.Celebrity.ChangeRoleView ?
            RoleClass.Celebrity.ViewPlayers : RoleClass.Celebrity.CelebrityPlayer;

            foreach (PlayerControl viewPlayer in CelebrityViewPlayers)
            {
                if (viewPlayer == player) continue;
                ChangePlayers[viewPlayer.PlayerId] = ModHelpers.Cs(RoleClass.Celebrity.color, viewPlayer.GetDefaultName());
            }
        }
        if (Madmate.CheckImpostor(player) || PlayerRole == RoleId.Marlin)
        {
            // カモフラ中は処理を破棄する
            if (!RoleClass.Camouflager.IsCamouflage)
            {
                foreach (PlayerControl Impostor in CachedPlayer.AllPlayers)
                {
                    if (Impostor.IsImpostor() && !Impostor.IsBot())
                    {
                        ChangePlayers[Impostor] = ModHelpers.Cs(RoleClass.ImpostorRed, ChangePlayers.GetNowName(Impostor));
                    }
                }
            }
        }
        else if (JackalFriends.CheckJackal(player))
        {
            // カモフラ中は処理を破棄する
            if (!RoleClass.Camouflager.IsCamouflage)
            {
                foreach (PlayerControl Jackal in RoleClass.Jackal.JackalPlayer)
                {
                    if (!Jackal.Data.Disconnected)
                    {
                        ChangePlayers[Jackal.PlayerId] = ModHelpers.Cs(RoleClass.Jackal.color, ChangePlayers.GetNowName(Jackal));
                    }
                }
            }
        }
        else
        {
            switch (PlayerRole)
            {
                case RoleId.Demon:
                    if (RoleClass.Demon.IsCheckImpostor && !RoleClass.Camouflager.IsCamouflage)
                    {
                        foreach (PlayerControl Impostor in CachedPlayer.AllPlayers)
                        {
                            if (Impostor.IsImpostor() && !Impostor.IsBot())
                            {
                                ChangePlayers[Impostor.PlayerId] = ModHelpers.Cs(RoleClass.ImpostorRed, ChangePlayers.GetNowName(Impostor));
                            }
                        }
                    }
                    foreach (PlayerControl CursePlayer in Demon.GetIconPlayers(player))
                    {
                        if (CursePlayer.IsBot()) continue;
                        if (!RoleClass.Camouflager.IsCamouflage)
                        {
                            ChangePlayers[CursePlayer.PlayerId] = ChangePlayers.GetNowName(CursePlayer) + ModHelpers.Cs(RoleClass.Demon.color, " ▲");
                        }
                        else if (RoleClass.Camouflager.DemonMark)
                        {
                            if (!ChangePlayers.Contains(CursePlayer.PlayerId)) ChangePlayers[CursePlayer] = ModHelpers.Cs(RoleClass.Demon.color, " ▲");
                            else ChangePlayers[CursePlayer.PlayerId] = ChangePlayers[CursePlayer.PlayerId] + ModHelpers.Cs(RoleClass.Demon.color, " ▲");
                        }
                    }
                    break;
                case RoleId.Arsonist:
                    foreach (PlayerControl DousePlayer in Arsonist.GetIconPlayers(player))
                    {
                        if (DousePlayer.IsBot()) continue;
                        if (!RoleClass.Camouflager.IsCamouflage)
                        {
                            ChangePlayers[DousePlayer.PlayerId] = ChangePlayers.GetNowName(DousePlayer) + ModHelpers.Cs(RoleClass.Arsonist.color, " §");
                        }
                        else if (RoleClass.Camouflager.ArsonistMark)
                        {
                            if (!ChangePlayers.Contains(DousePlayer.PlayerId)) ChangePlayers[DousePlayer] = ModHelpers.Cs(RoleClass.Arsonist.color, " §");
                            else ChangePlayers[DousePlayer.PlayerId] = ChangePlayers[DousePlayer.PlayerId] + ModHelpers.Cs(RoleClass.Arsonist.color, " §");
                        }
                    }
                    break;
                case RoleId.SatsumaAndImo:
                    MySuffix.Append(
                        RoleClass.SatsumaAndImo.TeamNumber == 1 ?
                            ModHelpers.Cs(Palette.White, " (C)") :
                            ModHelpers.Cs(RoleClass.ImpostorRed, " (M)")
                        );
                    break;
                case RoleId.Finder:
                    //マッドを表示させられる場合
                    if (RoleClass.Finder.KillCounts[player.PlayerId] >= RoleClass.Finder.CheckMadmateKillCount)
                    {
                        foreach (PlayerControl Player in CachedPlayer.AllPlayers)
                        {
                            if (!Player.IsBot() && Player.IsMadRoles())
                            {
                                ChangePlayers[Player.PlayerId] = ModHelpers.Cs(RoleClass.ImpostorRed, ChangePlayers.GetNowName(Player));
                            }
                        }
                    }
                    break;
                case RoleId.Pokerface:
                    Pokerface.PokerfaceTeam pokerfaceTeam = Pokerface.GetPokerfaceTeam(player);
                    if (pokerfaceTeam != null)
                    {
                        foreach (PlayerControl pokerplayer in pokerfaceTeam.TeamPlayers)
                        {
                            if (pokerplayer.PlayerId == player.PlayerId)
                                continue;
                            ChangePlayers[pokerplayer.PlayerId] = ModHelpers.Cs(Pokerface.RoleData.color, ChangePlayers.GetNowName(pokerplayer));
                        }
                    }
                    break;
            }
        }
        //プレイヤーがラバーズかつ
        if (player.IsLovers() &&
            //カモフラ中じゃない、もしくはラバーズマークがカモフラ中でも見える場合
            !RoleClass.Camouflager.IsCamouflage || RoleClass.Camouflager.LoversMark)
        {
            var suffix = ModHelpers.Cs(RoleClass.Lovers.color, " ♥");
            PlayerControl Side = player.GetOneSideLovers();
            string name = Side.GetDefaultName();
            ChangePlayers[Side.PlayerId] = ChangePlayers.GetNowName(Side) + suffix;
            MySuffix.Append(suffix);
        }
        //プレイヤーがクラードかつ
        if (player.IsQuarreled() &&
            //カモフラ中じゃない、もしくはクラードマークがカモフラ中でも見える場合
            !RoleClass.Camouflager.IsCamouflage || RoleClass.Camouflager.QuarreledMark)
        {
            var suffix = ModHelpers.Cs(RoleClass.Quarreled.color, "○");
            PlayerControl Side = player.GetOneSideQuarreled();
            string name = Side.GetDefaultName();
            ChangePlayers[Side.PlayerId] = ChangePlayers.GetNowName(Side) + suffix;
            MySuffix.Append(suffix);
        }
        //ここで役職名の部分を変える
        switch (PlayerRole)
        {
            case RoleId.Sheriff:
                if (RoleClass.Sheriff.KillCount.TryGetValue(player.PlayerId, out int svalue))
                {
                    RoleNameText.Append(ModHelpers.Cs(introData.color, svalue.ToString()));
                }
                break;
            case RoleId.RemoteSheriff:
                if (RoleClass.RemoteSheriff.KillCount.TryGetValue(player.PlayerId, out int rsvalue))
                {
                    RoleNameText.Append(ModHelpers.Cs(introData.color, rsvalue.ToString()));
                }
                break;
            case RoleId.Mafia:
                if (Mafia.IsKillFlag())
                {
                    RoleNameText.Append(" (OK)");
                }
                break;
        }
        string TaskText = string.Empty;
        if (player.IsUseTaskTrigger())
        {
            try
            {
                var (Complete, all) = TaskCount.TaskDateNoClearCheck(player.Data);
                if (commsActive)
                    TaskText = ModHelpers.Cs(Color.yellow, $"(?/{all})");
                else
                    TaskText = ModHelpers.Cs(Color.yellow, $"({Complete}/{all})");
            }
            catch
            {
            }
        }

        bool IsDemonVIew = false;
        bool IsArsonistVIew = false;
        bool IsHauntedWolfVIew = false;
        bool IsGhostMechanicVIew = false; // 幽霊役職が1つしかない為, 単独処理
        StringBuilder attributeRoleName = new();

        if ((SetNamesClass.DefaultGhostSeeRoles(player) || player.IsRole(RoleId.God)) && !IsUnchecked)
        {
            if (Demon.IsViewIcon(player))
            {
                MySuffix.Append(ModHelpers.Cs(RoleClass.Demon.color, " ▲"));
                IsDemonVIew = true;
            }
            if (Arsonist.IsViewIcon(player))
            {
                MySuffix.Append(ModHelpers.Cs(RoleClass.Arsonist.color, " §"));
                IsArsonistVIew = true;
            }
            if (player.IsHauntedWolf())
            {
                attributeRoleName.Append($" + {ModHelpers.Cs(SuperNewRoles.Roles.Attribute.HauntedWolf.RoleData.color, ModTranslation.GetString("HauntedWolfName"))}");
                IsHauntedWolfVIew = true;
            }
            if (player.IsGhostRole(RoleId.GhostMechanic))
            {
                attributeRoleName.Append($" + {ModHelpers.Cs(RoleClass.GhostMechanic.color, ModTranslation.GetString("GhostMechanicName"))}");
                IsGhostMechanicVIew = true;
            }
            NewName.Append($"(<size=75%>{ModHelpers.Cs(introData.color, introData.Name)}{attributeRoleName}{TaskText}</size>)");
            if (!RoleClass.Camouflager.IsCamouflage)
            {
                NewName.Append(ModHelpers.Cs(introData.color, $"{player.GetDefaultName()}{MySuffix}"));
            }
            else
            {
                NewName.Append(ModHelpers.Cs(introData.color, MySuffix.ToString()));
            }
        }
        else if (player.IsAlive() || IsUnchecked)
        {
            if (SetNamesClass.DefaultGhostSeeRoles(player) || player.IsRole(RoleId.God))
            {
                if (Demon.IsViewIcon(player))
                {
                    MySuffix.Append(ModHelpers.Cs(RoleClass.Demon.color, " ▲"));
                    IsDemonVIew = true;
                }
                if (Arsonist.IsViewIcon(player))
                {
                    MySuffix.Append(ModHelpers.Cs(RoleClass.Arsonist.color, " §"));
                    IsArsonistVIew = true;
                }
            }
            NewName.Append($"<size=75%>{RoleNameText}{attributeRoleName}{TaskText}</size>\n");
            if (!RoleClass.Camouflager.IsCamouflage)
                NewName.Append(ModHelpers.Cs(introData.color, player.GetDefaultName() + MySuffix));
            else
                NewName.Append(ModHelpers.Cs(introData.color, MySuffix.ToString()));
        }
        if (!player.IsMod())
        {
            player.RpcSetNamePrivate(NewName.ToString());
            if (player.IsAlive())
            {
                foreach (var ChangePlayerData in (Dictionary<PlayerControl, string>)ChangePlayers)
                {
                    ChangePlayerData.Key.RpcSetNamePrivate(ChangePlayerData.Value, player);
                }
            }
        }
        if (player.IsImpostor() && IsHideAndSeek)
        {
            foreach (PlayerControl AlivePlayer in AlivePlayers)
            {
                if (AlivePlayer.IsMod()) continue;
                player.RpcSetNamePrivate(ModHelpers.Cs(RoleClass.ImpostorRed, player.GetDefaultName()), AlivePlayer);
            }
        }
        StringBuilder DieSuffix = new();
        // FIXME : SHRにおいて重複役の名前変更の共通処理が完成していない。
        if (!IsDemonVIew && Demon.IsViewIcon(player))
            DieSuffix.Append(ModHelpers.Cs(RoleClass.Demon.color, " ▲"));
        if (!IsArsonistVIew && Arsonist.IsViewIcon(player))
            DieSuffix.Append(ModHelpers.Cs(RoleClass.Arsonist.color, " §"));
        if (!IsHauntedWolfVIew && player.IsHauntedWolf())
            DieSuffix.Append($" + {ModHelpers.Cs(SuperNewRoles.Roles.Attribute.HauntedWolf.RoleData.color, ModTranslation.GetString("HauntedWolfName"))}");
        if (!IsGhostMechanicVIew && player.IsGhostRole(RoleId.GhostMechanic))
            DieSuffix.Append($" + {ModHelpers.Cs(RoleClass.GhostMechanic.color, ModTranslation.GetString("GhostMechanicName"))}");
        NewName.Append(DieSuffix);
        string NewNameString = NewName.ToString();
        foreach (PlayerControl DiePlayer in CanAllRolePlayers)
        {
            if (player.PlayerId != DiePlayer.PlayerId &&
                !DiePlayer.Data.Disconnected)
                player.RpcSetNamePrivate(NewNameString, DiePlayer);
        }
    }
}