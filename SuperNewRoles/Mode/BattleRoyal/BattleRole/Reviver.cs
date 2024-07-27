using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperNewRoles.Helpers;
using UnityEngine;

namespace SuperNewRoles.Mode.BattleRoyal.BattleRole;
public class Reviver : BattleRoyalRole
{
    public static List<Reviver> revivers;
    public static bool IsReviver(PlayerControl player)
    {
        return revivers.FirstOrDefault(x => x.CurrentPlayer == player) is not null;
    }
    public Reviver(PlayerControl player)
    {
        CurrentPlayer = player;
        revivers.Add(this);
        IsAbilityUsingNow = false;
        IsReviverTime = false;
        AbilityTime = 0;
    }
    public bool IsAbilityUsingNow;
    public bool IsReviverTime;
    public float AbilityTime;
    public PlayerControl currentTarget;
    public override void FixedUpdate()
    {
        if (IsAbilityUsingNow)
        {
            if (currentTarget is null)
            {
                IsAbilityUsingNow = false;
                currentTarget = null;
                PlayerAbility ability = PlayerAbility.GetPlayerAbility(CurrentPlayer);
                ability.CanMove = true;
                ability.CanUseKill = true;
                SyncBattleOptions.CustomSyncOptions(CurrentPlayer);
                return;
            }
            AbilityTime -= Time.fixedDeltaTime;
            if (AbilityTime <= 0)
            {
                PlayerAbility currentability = PlayerAbility.GetPlayerAbility(CurrentPlayer);
                PlayerAbility targetability = PlayerAbility.GetPlayerAbility(currentTarget);
                if (IsReviverTime)
                {
                    targetability.CanMove = true;
                    IsAbilityUsingNow = false;
                    CurrentPlayer.RpcResetAbilityCooldown();
                }
                else
                {
                    currentability.CanUseKill = true;
                    currentability.CanMove = true;
                    targetability.CanUseKill = true;
                    targetability.CanMove = false;
                    currentTarget.Data.IsDead = false;
                    currentTarget.RpcSnapTo(CurrentPlayer.transform.position);
                    RPCHelper.RpcSyncAllNetworkedPlayer();
                    SyncBattleOptions.CustomSyncOptions(CurrentPlayer);
                    IsReviverTime = true;
                    AbilityTime = RoleParameter.ReviverRevivePlayerStuckTime;
                    currentTarget.MyPhysics.RpcExitVentUnchecked(0);
                }
                SyncBattleOptions.CustomSyncOptions(currentTarget);
            }
        }
    }
    public override void UseAbility(PlayerControl target)
    {
        if (IsAbilityUsingNow) return;
        BattleTeam team = BattleTeam.GetTeam(CurrentPlayer);
        List<PlayerControl> targets = new();
        foreach (PlayerControl p in team.TeamMember.AsSpan())
        {
            if (p.IsDead()) targets.Add(p);
        }
        if (targets.Count <= 0) return;
        target = ModHelpers.GetRandom(targets);
        PlayerAbility ability = PlayerAbility.GetPlayerAbility(CurrentPlayer);
        ability.CanUseKill = false;
        ability.CanMove = false;
        SyncBattleOptions.CustomSyncOptions(CurrentPlayer);
        IsAbilityUsingNow = true;
        IsReviverTime = false;
        AbilityTime = RoleParameter.ReviverPlayerStuckTime;
        currentTarget = target;
        ChangeName.SetNotification(ModTranslation.GetString("ReviverReviveStart"), RoleParameter.ReviverShowNotificationDurationTime);
        ChangeName.UpdateName(true);
    }
    public static void Clear()
    {
        revivers = new();
    }
}