using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperNewRoles.Helpers;
using SuperNewRoles.Mode.SuperHostRoles;
using UnityEngine;

namespace SuperNewRoles.Mode.BattleRoyal.BattleRole;
public class Revenger : BattleRoyalRole
{
    public static List<Revenger> revengers;
    public static bool IsRevenger(PlayerControl player)
    {
        return revengers.FirstOrDefault(x => x.CurrentPlayer == player) is not null;
    }
    public Revenger(PlayerControl player)
    {
        CurrentPlayer = player;
        revengers.Add(this);
        IsAbilityUsingNow = false;
        AbilityTime = 0;
        DeathPosition = new();
        currentKiller = null;
        PlayerAbility.GetPlayerAbility(player).CanRevive = true;
    }
    public bool IsAbilityUsingNow;
    public float AbilityTime;
    public Vector2 DeathPosition;
    public PlayerControl currentKiller;
    public override void FixedUpdate()
    {
        if (IsAbilityUsingNow)
        {
            AbilityTime -= Time.fixedDeltaTime;
            if (AbilityTime <= 0)
            {
                CurrentPlayer.Data.IsDead = false;
                CurrentPlayer.RpcSnapTo(DeathPosition);
                CurrentPlayer.MyPhysics.RpcExitVentUnchecked(0);
                ChangeName.UpdateName();
                RPCHelper.RpcSyncGameData();
                IsAbilityUsingNow = false;
            }
        }
    }
    public void MeRevive()
    {
        IsAbilityUsingNow = true;
        AbilityTime = RoleParameter.RevengerReviveTime;
        Logger.Info($"{ModHelpers.GetInRoom(DeathPosition)} : {DeathPosition}");
        SystemTypes roomtype = ModHelpers.GetInRoom(DeathPosition);
        string room = roomtype is SystemTypes.Doors ? "None" : FastDestroyableSingleton<TranslationController>.Instance.GetString(roomtype);
        string msg = string.Format(ModTranslation.GetString("RevengerStartRevenge"), CurrentPlayer.GetDefaultName() , room);
        ChangeName.SetNotification(msg, RoleParameter.ReviverShowNotificationDurationTime);
        ChangeName.UpdateName(true);
    }
    public void OnKill(PlayerControl __instance, PlayerControl target)
    {
        if (target == CurrentPlayer)
        {
            DeathPosition = target.transform.position;
            currentKiller = __instance;
        }
        if (CurrentPlayer.IsDead() && currentKiller is not null && target == currentKiller && BattleTeam.GetTeam(CurrentPlayer).IsTeam(__instance))
        {
            MeRevive();
        }
    }
    public static void Clear()
    {
        revengers = new();
    }
}