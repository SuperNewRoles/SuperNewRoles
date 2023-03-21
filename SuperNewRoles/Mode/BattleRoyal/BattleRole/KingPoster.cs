using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperNewRoles.Helpers;
using UnityEngine;

namespace SuperNewRoles.Mode.BattleRoyal.BattleRole;
public class KingPoster : BattleRoyalRole
{
    public static List<KingPoster> KingPosters;
    public static bool IsKingPoster(PlayerControl player)
    {
        return KingPosters.FirstOrDefault(x => x.CurrentPlayer == player) is not null;
    }
    public KingPoster(PlayerControl player)
    {
        CurrentPlayer = player;
        KingPosters.Add(this);
        IsAbilityUsingNow = false;
        IsAbilityTime = false;
        IsAbilityEnded = false;
        AbilityTime = 0;
    }
    public bool IsAbilityUsingNow;
    public bool IsAbilityTime;
    public bool IsAbilityEnded;
    public float AbilityTime;
    public override void FixedUpdate()
    {
        if (IsAbilityUsingNow)
        {
            AbilityTime -= Time.fixedDeltaTime;
            if (AbilityTime <= 0)
            {
                PlayerAbility currentability = PlayerAbility.GetPlayerAbility(CurrentPlayer);
                if (IsAbilityEnded)
                {
                    currentability.CanUseKill = false;
                    currentability.CanMove = false;
                    IsAbilityUsingNow = false;
                    IsAbilityEnded = false;
                }
                else if (IsAbilityTime)
                {
                    currentability.CanMove = false;
                    IsAbilityEnded = true;
                    AbilityTime = RoleParameter.KingPosterPlayerStuckTimeEnd;
                }
                else
                {
                    currentability.CanUseKill = true;
                    currentability.CanMove = true;
                    IsAbilityTime = true;
                    AbilityTime = RoleParameter.KingPosterPlayerAbilityTime;
                }
                SyncBattleOptions.CustomSyncOptions(CurrentPlayer);
            }
        }
    }
    public override void UseAbility(PlayerControl target)
    {
        PlayerAbility ability = PlayerAbility.GetPlayerAbility(CurrentPlayer);
        ability.CanUseKill = false;
        ability.CanMove = false;
        SyncBattleOptions.CustomSyncOptions(CurrentPlayer);
        IsAbilityUsingNow = true;
        IsAbilityTime = false;
        IsAbilityEnded = false;
        AbilityTime = RoleParameter.KingPosterPlayerStuckTimeStart;
        ChangeName.SetNotification(ModTranslation.GetString("KingPosterReviveStart"), RoleParameter.KingPosterShowNotificationDurationTime);
        ChangeName.UpdateName(true);
    }
    public static void Clear()
    {
        KingPosters = new();
    }
}