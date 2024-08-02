using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AmongUs.GameOptions;
using SuperNewRoles.Helpers;
using SuperNewRoles.Mode.SuperHostRoles;
using UnityEngine;

namespace SuperNewRoles.Mode.BattleRoyal.BattleRole;
public class Guardrawer : BattleRoyalRole
{
    public static List<Guardrawer> guardrawers;
    public static bool IsGuardrawer(PlayerControl player)
    {
        return guardrawers.FirstOrDefault(x => x.CurrentPlayer == player) is not null;
    }
    public Guardrawer(PlayerControl player)
    {
        CurrentPlayer = player;
        guardrawers.Add(this);
        IsAbilityUsingNow = false;
        AbilityTime = 0;
    }
    public bool IsAbilityUsingNow;
    public float AbilityTime;
    public override void FixedUpdate()
    {
        if (IsAbilityUsingNow)
        {
            AbilityTime -= Time.fixedDeltaTime;
            foreach (PlayerControl p in PlayerControl.AllPlayerControls)
            {
                if (p.IsBot()) continue;
                if (p.PlayerId == CurrentPlayer.PlayerId) continue;
                if (p.IsDead()) continue;
                if (Vector2.Distance(p.GetTruePosition(), CurrentPlayer.GetTruePosition()) > GameOptionsData.KillDistances[0]) continue;
                p.RpcSnapTo(CurrentPlayer.transform.position);
            }
            if (AbilityTime <= 0)
            {
                PlayerAbility currentability = PlayerAbility.GetPlayerAbility(CurrentPlayer);
                IsAbilityUsingNow = false;
                CurrentPlayer.RpcResetAbilityCooldown();
                currentability.CanUseKill = true;
            }
        }
    }
    public override void UseAbility(PlayerControl target)
    {
        if (IsAbilityUsingNow) return;
        PlayerAbility currentability = PlayerAbility.GetPlayerAbility(CurrentPlayer);
        currentability.CanUseKill = false;
        ChangeName.SetNotification(string.Format(ModTranslation.GetString("GuardrawerAbilityStart"), CurrentPlayer.GetDefaultName()), RoleParameter.GuardrawerShowNotificationDurationTime);
        ChangeName.UpdateName(true);
        AbilityTime = RoleParameter.GuardrawerAbilityTime;
        IsAbilityUsingNow = true;
    }
    public static void Clear()
    {
        guardrawers = new();
    }
}