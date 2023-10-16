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

    public static void OnMeetingStart()
    {
        RoleBaseManager.GetInterfaces<IMeetingHandler>()
            .Do(x => x.StartMeeting());
    }

    public static void OnWrapUp()
    {
        RoleBaseManager.GetInterfaces<IWrapUpHandler>()
            .Do(x => x.OnWrapUp());
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

    public static void OnKill(this PlayerControl player, PlayerControl target)
    {
        RoleBaseManager.GetInterfaces<IMurderHandler>().Do(x => x.OnMurderPlayer(player, target));
    }

    public static void OnDeath(this PlayerControl player, PlayerControl killer)
    {
        RoleBaseManager.
            GetInterfaces<IDeathHandler>().Do(x => x.OnDeath(player));
    }
}