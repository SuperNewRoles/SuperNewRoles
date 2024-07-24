using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using SuperNewRoles.Mode;
using SuperNewRoles.Mode.SuperHostRoles;
using SuperNewRoles.Roles.Role;
using SuperNewRoles.Roles.RoleBases.Interfaces;
using UnityEngine;

namespace SuperNewRoles.Roles.RoleBases;
public static class CustomRoles
{
    public static void FixedUpdate()
    {
        RoleBase roleBase = PlayerControl.LocalPlayer.GetRoleBase();
        IFixedUpdaterMe ifum = roleBase as IFixedUpdaterMe;
        IReadOnlySet<IFixedUpdaterAll> IFixedUpdaterAlls = RoleBaseManager.GetFixedUpdaterAlls();
        switch (ModeHandler.GetMode())
        {
            case ModeId.Default:
                if (IFixedUpdaterAlls != null)
                    foreach (IFixedUpdaterAll all in IFixedUpdaterAlls)
                        all.FixedUpdateAllDefault();

                if (ifum != null)
                {
                    ifum.FixedUpdateMeDefault();
                    if (PlayerControl.LocalPlayer.IsAlive())
                        ifum.FixedUpdateMeDefaultAlive();
                    else
                        ifum.FixedUpdateMeDefaultDead();
                }
                break;
            case ModeId.SuperHostRoles:
                if (IFixedUpdaterAlls != null)
                    foreach (IFixedUpdaterAll all in IFixedUpdaterAlls)
                        all.FixedUpdateAllSHR();
                if (ifum != null)
                {
                    ifum.FixedUpdateMeSHR();
                    if (PlayerControl.LocalPlayer.IsAlive())
                        ifum.FixedUpdateMeSHRAlive();
                    else
                        ifum.FixedUpdateMeSHRDead();
                }
                break;
        }
        if (PlayerControl.LocalPlayer.IsDead() && roleBase is IHaveHauntAbility haveNotHauntAbility)
        {
            Buttons.HauntButtonControl.HauntButtonSwitch(haveNotHauntAbility);
        }
    }
    public static void OnIntroStart()
    {
        RoleBaseManager.GetInterfaces<IIntroHandler>()
            .Do(x => x.OnIntroStart());
        if (PlayerControl.LocalPlayer.GetRoleBase() is IIntroHandler introHandler)
        {
            introHandler.OnIntroStartMe();
        }
    }
    public static void OnIntroDestroy()
    {
        RoleBaseManager.GetInterfaces<IIntroHandler>()
            .Do(x => x.OnIntroDestory());
        if (PlayerControl.LocalPlayer.GetRoleBase() is IIntroHandler introHandler)
        {
            introHandler.OnIntroDestoryMe();
        }
    }
    public static void NameHandler(bool CanSeeAllRole = false)
    {
        RoleBaseManager.GetInterfaces<INameHandler>().Do(x => x.OnHandleAllPlayer());
        if (CanSeeAllRole)
        {
            RoleBaseManager.GetInterfaces<INameHandler>()
                .Do(x => x.OnHandleDeadPlayer());
        }
        else
        {
            if (PlayerControl.LocalPlayer.GetRoleBase() is INameHandler nameHandler)
            {
                nameHandler.OnHandleName();
            }
        }
    }

    public static void OnMeetingStart()
    {
        RoleBaseManager.GetInterfaces<IMeetingHandler>()
            .Do(x => x.StartMeeting());
    }
    public static void OnMeetingClose()
    {
        RoleBaseManager.GetInterfaces<IMeetingHandler>()
            .Do(x => x.CloseMeeting());
    }

    public static void OnWrapUp(PlayerControl exiled)
    {
        RoleBaseManager.GetInterfaces<IWrapUpHandler>()
            .Do(x =>
            {
                x.OnWrapUp();
                if (exiled != null)
                    x.OnWrapUp(exiled);
            });
    }

    public static bool OnPetPet(PlayerControl petter)
    {
        return !(petter.GetRoleBase() is IPetHandler petHandler) || petHandler.OnCheckPet(ModeHandler.IsMode(ModeId.Default));
    }

    [HarmonyPatch(typeof(GameData), nameof(GameData.HandleDisconnect), new Type[] { typeof(PlayerControl), typeof(DisconnectReasons) })]
    class HandleDisconnectPatch
    {
        public static void Postfix(GameData __instance, PlayerControl player, DisconnectReasons reason)
        {
            if (AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Started)
            {
                RoleBaseManager.PlayerRoles.Values.Do(
                    x =>
                    {
                        if (x is IHandleDisconnect handleDisconnect)
                            handleDisconnect.OnDisconnect();
                    }
               );
                if (player.TryGetRoleBase(out RoleBase roleBase))
                    RoleBaseManager.ClearRole(player, roleBase);
                AntiBlackOut.OnDisconnect(player.Data);
            }
        }
    }

    [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.FixedUpdate))]
    public static class PlayerPhysicsSpeedPatch
    {
        public static void Postfix(PlayerPhysics __instance)
        {
            RoleBase roleBase = __instance.myPlayer.GetRoleBase();
            if (roleBase is IPlayerPhysics physics) physics.FixedUpdate(__instance);
        }
    }

    public static void OnExild(DeadPlayer deadPlayer)
    {
        DeathInfo info = new(deadPlayer);
        RoleBaseManager.GetInterfaces<IDeathHandler>()
            .Do(x => x.OnExiled(info));
        OnDeath(info);
    }
    public static void OnKill(DeadPlayer deadPlayer)
    {
        DeathInfo info = new(deadPlayer);
        RoleBaseManager.GetInterfaces<IDeathHandler>().Do(x => x.OnMurderPlayer(info));
        OnDeath(info);
    }
    public static bool OnCheckVanish(PlayerControl player)
    {
        return player.GetRoleBase() is ICheckPhantom checkPhantom
            ? checkPhantom.CheckVanish()
            : true;
    }
    public static bool OnCheckAppear(PlayerControl player, bool shouldAnimate)
    {
        return player.GetRoleBase() is ICheckPhantom checkPhantom
            ? checkPhantom.CheckAppear(shouldAnimate)
            : true;
    }
    public static bool OnCheckMurderPlayer(PlayerControl source, PlayerControl target)
    {
        bool resultsource = true;
        bool resulttarget = true;
        bool result = RoleBaseManager.GetInterfaces<ICheckMurderHandler>().All(x => x.OnCheckMurderPlayer(source, target));
        if (source.GetRoleBase() is ICheckMurderHandler checkMurderHandlerSource)
            resultsource = checkMurderHandlerSource.OnCheckMurderPlayerAmKiller(target);
        if (target.GetRoleBase() is ICheckMurderHandler checkMurderHandlerTarget)
            resulttarget = checkMurderHandlerTarget.OnCheckMurderPlayerAmTarget(source);
        return result && resultsource && resulttarget;
    }

    public static void OnDeath(DeathInfo info)
    {
        RoleBaseManager.
            GetInterfaces<IDeathHandler>().Do(x => x.OnDeath(info));
        if (info.DeathPlayer.AmOwner)
            RoleBaseManager.GetInterfaces<IDeathHandler>().Do(x => x.OnAmDeath(info));
    }

    public static Color GetRoleColor(PlayerControl player, bool IsImpostorReturn = false)
    {
        return GetRoleColor(player.GetRole(), player, IsImpostorReturn);
    }
    public static Color GetRoleColor(RoleId role, PlayerControl player = null, bool IsImpostorReturn = false)
    {
        RoleInfo roleInfo = RoleInfoManager.GetRoleInfo(role);
        if (roleInfo != null)
            return roleInfo.RoleColor;
        return IntroData.GetIntrodata(role, player, IsImpostorReturn)?.color ?? new();
    }
    public static string GetRoleName(PlayerControl player, bool IsImpostorReturn = false)
    {
        return GetRoleName(player.GetRole(), player, IsImpostorReturn);
    }
    public static string GetRoleName(RoleId role, PlayerControl player = null, bool IsImpostorReturn = false)
    {
        RoleInfo roleInfo = RoleInfoManager.GetRoleInfo(role);
        if (roleInfo != null)
            return ModTranslation.GetString($"{roleInfo.NameKey}Name");
        return ModTranslation.GetString($"{IntroData.GetIntrodata(role, player, IsImpostorReturn)?.NameKey}Name");
    }
    public static string GetRoleNameKey(PlayerControl player, bool IsImpostorReturn = false)
    {
        return GetRoleNameKey(player.GetRole(), player, IsImpostorReturn);
    }
    public static string GetRoleNameKey(RoleId role, PlayerControl player = null, bool IsImpostorReturn = false)
    {
        RoleInfo roleInfo = RoleInfoManager.GetRoleInfo(role);
        if (roleInfo != null)
            return roleInfo.NameKey;
        return IntroData.GetIntrodata(role, player, IsImpostorReturn)?.NameKey;
    }
    public static string GetRoleNameOnColor(PlayerControl player, bool IsImpostorReturn = false)
    {
        return GetRoleNameOnColor(player.GetRole(), player, IsImpostorReturn);
    }
    public static string GetRoleNameOnColor(RoleId role, PlayerControl player = null, bool IsImpostorReturn = false)
    {
        RoleInfo roleInfo = RoleInfoManager.GetRoleInfo(role);
        if (roleInfo != null)
            return CustomOptionHolder.Cs(roleInfo.RoleColor, $"{roleInfo.NameKey}Name");
        IntroData intro = IntroData.GetIntrodata(role, player, IsImpostorReturn);
        if (intro == null)
            return null;
        return CustomOptionHolder.Cs(intro.color, $"{intro?.NameKey}Name");
    }
    public static TeamRoleType GetRoleTeam(PlayerControl player, bool IsImpostorReturn = false)
    {
        return GetRoleTeam(player.GetRole(), player, IsImpostorReturn);
    }
    public static TeamRoleType GetRoleTeam(RoleId role, PlayerControl player = null, bool IsImpostorReturn = false)
    {
        RoleInfo roleInfo = RoleInfoManager.GetRoleInfo(role);
        if (roleInfo != null)
            return roleInfo.Team;
        return IntroData.GetIntrodata(role, player, IsImpostorReturn)?.Team ?? TeamRoleType.Error;
    }
    public static QuoteMod GetQuoteMod(PlayerControl player)
    {
        return GetQuoteMod(player.GetRole(), player);
    }
    public static QuoteMod GetQuoteMod(RoleId role, PlayerControl player = null) // FIXME : Lovers(Woodi_dev様)は現在アイデア元を表示できていません。
    {
        RoleInfo roleInfo = RoleInfoManager.GetRoleInfo(role);
        if (roleInfo != null) return roleInfo.QuoteMod;
        else if (role == RoleId.DefaultRole) return QuoteMod.AmongUs;

        var intro = IntroData.GetIntrodata(role, player);
        if (intro != IntroData.CrewmateIntro) return intro?.QuoteMod ?? QuoteMod.SuperNewRoles;
        else return QuoteMod.SuperNewRoles; // 存在しないのにRoleIdがある役はSNR役扱いにする
    }
    public static bool IsGhostRole(RoleId role)
    {
        RoleInfo roleInfo = RoleInfoManager.GetRoleInfo(role);
        if (roleInfo != null)
            return roleInfo.IsGhostRole;
        return IntroData.GetIntrodata(role)?.IsGhostRole ?? false;
    }
    public static string GetRoleIntro(PlayerControl player)
    {
        return GetRoleIntro(player.GetRole(), player);
    }
    public static string GetRoleIntro(RoleId role, PlayerControl player = null)
    {
        IntroInfo introInfo = IntroInfo.GetIntroInfo(role);
        if (introInfo != null)
            return introInfo.IntroDesc;
        return IntroData.GetIntrodata(role, player)?.TitleDesc;
    }
    public static string GetRoleDescription(RoleId role)
    {
        RoleInfo roleInfo = RoleInfoManager.GetRoleInfo(role);
        if (roleInfo != null)
            return ModTranslation.GetString($"{roleInfo.NameKey}Description");
        return IntroData.GetIntrodata(role)?.Description;
    }
    public static TeamType GetRoleTeamType(PlayerControl player, bool IsImpostorReturn = false)
    {
        return GetRoleTeamType(player.GetRole(), player, IsImpostorReturn);
    }
    public static TeamType GetRoleTeamType(RoleId role, PlayerControl player = null, bool IsImpostorReturn = false)
    {
        RoleInfo roleInfo = RoleInfoManager.GetRoleInfo(role);
        if (roleInfo != null)
            return roleInfo.TeamType;
        return IntroData.GetIntrodata(role, player, IsImpostorReturn)?.TeamType ?? TeamType.Error;
    }
    public static PlayerControl[] GetRolePlayers<T>() where T : RoleBase
    {
        IReadOnlyList<T> Roles = RoleBaseManager.GetRoleBases<T>();
        PlayerControl[] Players = new PlayerControl[Roles.Count];
        for (int i = 0; i < Roles.Count; i++)
        {
            Players[i] = Roles[i].Player;
        }
        return Players;
    }
}