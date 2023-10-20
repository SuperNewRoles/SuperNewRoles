using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using SuperNewRoles.Mode;
using SuperNewRoles.Roles.RoleBases.Interfaces;

namespace SuperNewRoles.Roles.RoleBases;
public static class CustomRoles
{
    public static void FixedUpdate()
    {
        RoleBase roleBase = PlayerControl.LocalPlayer.GetRoleBase();
        IFixedUpdaterMe ifum = roleBase as IFixedUpdaterMe;
        IReadOnlyList<IFixedUpdaterAll> IFixedUpdaterAlls = RoleBaseManager.GetInterfaces<IFixedUpdaterAll>();
        switch (ModeHandler.GetMode())
        {
            case ModeId.Default:
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
            .Do(x => {
                x.OnWrapUp();
                if (exiled != null)
                    x.OnWrapUp(exiled);
            });
    }

    [HarmonyPatch(typeof(GameData), nameof(GameData.HandleDisconnect), new Type[] { typeof(PlayerControl), typeof(DisconnectReasons) })]
    class HandleDisconnectPatch
    {
        public static void Postfix(GameData __instance, PlayerControl player, DisconnectReasons reason)
        {
            if (AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Started)
            {
                RoleBaseManager.PlayerRoles.Values.Do(
                    x => {
                        if (x is IHandleDisconnect handleDisconnect)
                            handleDisconnect.OnDisconnect();
                    }
               );
            }
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

    public static void OnDeath(DeathInfo info)
    {
        RoleBaseManager.
            GetInterfaces<IDeathHandler>().Do(x => x.OnDeath(info));
    }
}