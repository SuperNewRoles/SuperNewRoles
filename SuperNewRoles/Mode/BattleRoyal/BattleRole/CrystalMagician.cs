using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibCpp2IL;
using SuperNewRoles.Helpers;
using UnityEngine;
using static SuperNewRoles.Roles.Crewmate.Squid;

namespace SuperNewRoles.Mode.BattleRoyal.BattleRole;
public class CrystalMagician : BattleRoyalRole
{
    public static List<CrystalMagician> crystalMagicians;
    public static bool IsCrystalMagician(PlayerControl player)
    {
        return crystalMagicians.FirstOrDefault(x => x.CurrentPlayer == player) is not null;
    }
    public static CrystalMagician GetCrystalMagician(PlayerControl player)
    {
        return crystalMagicians.FirstOrDefault(x => x.CurrentPlayer == player);
    }
    public CrystalMagician(PlayerControl player)
    {
        CurrentPlayer = player;
        crystalMagicians.Add(this);
        IsAbilityUsingNow = false;
        IsCrystalTime = false;
        AbilityTime = 0;
        Players = new();
    }
    public bool IsAbilityUsingNow;
    public bool IsCrystalTime;
    public float AbilityTime;
    public List<PlayerControl> Players;
    public static PlayerControl Bot;
    public override void FixedUpdate()
    {
        if (IsAbilityUsingNow)
        {
            AbilityTime -= Time.fixedDeltaTime;
            if (AbilityTime <= 0)
            {
                PlayerAbility currentability = PlayerAbility.GetPlayerAbility(CurrentPlayer);
                //クリスタルが終わったら
                if (IsCrystalTime)
                {
                    foreach (PlayerControl member in BattleTeam.GetTeam(CurrentPlayer).TeamMember)
                    {
                        Bot.RpcSnapTo(new(999, 999), member);
                    }
                    foreach (PlayerControl player in Players)
                    {
                        if (player is null) continue;
                        PlayerAbility ability = PlayerAbility.GetPlayerAbility(player);
                        if (ability.AbilityKillCoolTime.ContainsKey("CrystalMagicianCrystalEffect")) ability.AbilityKillCoolTime.Remove("CrystalMagicianCrystalEffect");
                        if (ability.AbilityKillDistance.ContainsKey("CrystalMagicianCrystalEffect")) ability.AbilityKillDistance.Remove("CrystalMagicianCrystalEffect");
                        if (ability.AbilityLight.ContainsKey("CrystalMagicianCrystalEffect")) ability.AbilityLight.Remove("CrystalMagicianCrystalEffect");
                        ability.AbilityLight.TryAdd("CrystalMagicianSuperPower", ability.Light * 4 / -5);
                        SyncBattleOptions.CustomSyncOptions(player);
                    }
                    IsCrystalTime = false;
                    AbilityTime = RoleParameter.CrystalMagicianSuperPowerTime;
                }
                //視界低下が終わったら
                else
                {
                    foreach (PlayerControl player in Players)
                    {
                        if (player is null) continue;
                        PlayerAbility ability = PlayerAbility.GetPlayerAbility(player);
                        if (ability.AbilityLight.ContainsKey("CrystalMagicianSuperPower")) ability.AbilityLight.Remove("CrystalMagicianSuperPower");
                        SyncBattleOptions.CustomSyncOptions(player);
                    }
                    IsAbilityUsingNow = false;
                    CurrentPlayer.RpcResetAbilityCooldown();
                }
            }
        }
    }
    public static void UseWater(PlayerControl target)
    {
        foreach (PlayerControl player in BattleTeam.GetTeam(target).TeamMember)
        {
            CrystalMagician cm = GetCrystalMagician(player);
            if (cm is not null && cm.IsAbilityUsingNow)
            {
                cm.OnGetHolyWater(target);
                break;
            }
        }
    }
    public void OnGetHolyWater(PlayerControl getter)
    {
        PlayerAbility ability = PlayerAbility.GetPlayerAbility(getter);
        ability.AbilityKillCoolTime.TryAdd("CrystalMagicianCrystalEffect", ability.MyKillCoolTime / -2);
        ability.AbilityKillDistance.TryAdd("CrystalMagicianCrystalEffect", 1);
        ability.AbilityLight.TryAdd("CrystalMagicianCrystalEffect", 5);
        SyncBattleOptions.CustomSyncOptions(getter);
        Players.Add(getter);
        if (Bot is null) return;
        Bot.RpcSnapTo(new(999, 999), getter);
    }
    public bool CanUseAbility()
    {
        BattleTeam team = BattleTeam.GetTeam(CurrentPlayer);
        foreach (CrystalMagician cm in crystalMagicians) if (team.IsTeam(cm.CurrentPlayer) && cm.IsAbilityUsingNow) return false;
        return true;
    }
    public override void UseAbility(PlayerControl target)
    {
        if (!CanUseAbility()) return;
        Players = new();
        IsAbilityUsingNow = true;
        IsCrystalTime = true;
        AbilityTime = RoleParameter.CrystalMagicianCrystalTime;
        foreach (PlayerControl player in BattleTeam.GetTeam(CurrentPlayer).TeamMember)
        {
            Bot.RpcSnapTo(CurrentPlayer.transform.position, player);
        }
        IsCrystalTime = true;
    }
    public static void Clear()
    {
        crystalMagicians = new();
    }
}