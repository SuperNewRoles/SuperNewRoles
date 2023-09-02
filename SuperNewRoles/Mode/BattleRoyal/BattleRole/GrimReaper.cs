using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperNewRoles.Helpers;
using UnityEngine;
using static MonoMod.RuntimeDetour.DynamicHookGen;

namespace SuperNewRoles.Mode.BattleRoyal.BattleRole;
public class GrimReaper : BattleRoyalRole
{
    public static List<GrimReaper> grimReapers;
    public static bool IsGrimReaper(PlayerControl player)
    {
        return grimReapers.FirstOrDefault(x => x.CurrentPlayer == player) is not null;
    }
    public GrimReaper(PlayerControl player)
    {
        CurrentPlayer = player;
        grimReapers.Add(this);
        IsAbilityUsingNow = false;
        IsGrimReaperTime = false;
        AbilityTime = 0;
    }
    public bool IsAbilityUsingNow;
    public bool IsGrimReaperTime;
    public bool IsGrimReaperEndStuckTime;
    public float AbilityTime;
    public override void FixedUpdate()
    {
        if (IsAbilityUsingNow)
        {
            if (CurrentPlayer.IsDead()) return;
            AbilityTime -= Time.fixedDeltaTime;
            if (AbilityTime <= 0)
            {
                PlayerAbility currentability = PlayerAbility.GetPlayerAbility(CurrentPlayer);
                if (IsGrimReaperEndStuckTime)
                {
                    IsAbilityUsingNow = false;
                    foreach (PlayerControl player in BattleTeam.GetTeam(CurrentPlayer).TeamMember)
                    {
                        PlayerAbility ability = PlayerAbility.GetPlayerAbility(player);
                        if (ability is null) continue;
                        ability.CanMove = true;
                        ability.CanUseKill = true;
                    }
                    CurrentPlayer.RpcResetAbilityCooldown();
                }
                else if (IsGrimReaperTime)
                {
                    BattleTeam team = BattleTeam.GetTeam(CurrentPlayer);
                    foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                    {
                        PlayerAbility ability = PlayerAbility.GetPlayerAbility(player);
                        if (ability is null) continue;
                        if (!team.IsTeam(player))
                        {
                            ability.CanMove = true;
                            ability.CanUseKill = true;
                            continue;
                        }
                        ability.CanMove = false;
                        ability.CanUseKill = false;
                        if (ability.AbilityKillCoolTime.ContainsKey("GrimReaperEnergy")) ability.AbilityKillCoolTime.Remove("GrimReaperEnergy");
                        if (ability.AbilityLight.ContainsKey("GrimReaperLight")) ability.AbilityLight.Remove("GrimReaperEnergy");
                    }
                    AbilityTime = RoleParameter.GrimReaperEndStuckTime;
                    IsGrimReaperEndStuckTime = true;
                }
                else
                {
                    BattleTeam team = BattleTeam.GetTeam(CurrentPlayer);
                    foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                    {
                        PlayerAbility ability = PlayerAbility.GetPlayerAbility(player);
                        if (ability is null) continue;
                        if (!team.IsTeam(player))
                        {
                            ability.CanMove = false;
                            ability.CanUseKill = false;
                            continue;
                        }
                        ability.AbilityKillCoolTime.TryAdd("GrimReaperEnergy", -999);
                        ability.AbilityLight.TryAdd("GrimReaperEnergy", 999);
                    }
                    currentability.CanUseKill = true;
                    currentability.CanMove = true;
                    IsGrimReaperTime = true;
                    AbilityTime = RoleParameter.GrimReaperAbilityTime;
                }
                SyncBattleOptions.CustomSyncOptions();
            }
        }
    }
    public static bool CanUseAbility()
    {
        foreach (GrimReaper gr in GrimReaper.grimReapers) if (gr.IsAbilityUsingNow) return false;
        return true;
    }
    public override void UseAbility(PlayerControl target)
    {
        if (IsAbilityUsingNow) return;
        if (!CanUseAbility()) return;
        PlayerAbility ability = PlayerAbility.GetPlayerAbility(CurrentPlayer);
        ability.CanUseKill = false;
        ability.CanMove = false;
        SyncBattleOptions.CustomSyncOptions(CurrentPlayer);
        IsAbilityUsingNow = true;
        IsGrimReaperTime = false;
        IsGrimReaperEndStuckTime = false;
        AbilityTime = RoleParameter.GrimReaperFirstStuckTime;
        SystemTypes roomtype = ModHelpers.GetInRoom(CurrentPlayer.transform.position);
        string room = roomtype is SystemTypes.Doors ? "None" : FastDestroyableSingleton<TranslationController>.Instance.GetString(roomtype);
        ChangeName.SetNotification(string.Format(ModTranslation.GetString("GrimReaperAbilityStart"), room), RoleParameter.GrimReaperShowNotificationDurationTime);
        ChangeName.UpdateName(true);
    }
    public static void Clear()
    {
        grimReapers = new();
    }
}