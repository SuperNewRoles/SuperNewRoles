using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AmongUs.GameOptions;
using SuperNewRoles.Helpers;
using UnityEngine;
using static Rewired.ComponentControls.Effects.RotateAroundAxis;

namespace SuperNewRoles.Mode.BattleRoyal.BattleRole;
public class KingPoster : BattleRoyalRole
{
    public static List<KingPoster> KingPosters;
    public static bool IsKingPoster(PlayerControl player)
    {
        return GetKingPoster(player) is not null;
    }
    public static KingPoster GetKingPoster(PlayerControl player)
    {
        return KingPosters.FirstOrDefault(x => x.CurrentPlayer == player);
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
    float Speed = 1;
    float Angle;
    public void OnKillClick(PlayerControl target)
    {
        //放置
        return;
        if (!IsAbilityTime) return;
        Vector3 velocity = Quaternion.FromToRotation(target.transform.position, CurrentPlayer.transform.position) * new Vector3(1, 0, 0);
        CurrentPlayer.RpcSnapTo(CurrentPlayer.transform.position + velocity);
        CurrentPlayer.RpcResetAbilityCooldown();
    }
    string Hat;
    string Visor;
    string Skin;
    public override void FixedUpdate()
    {
        if (IsAbilityUsingNow)
        {
            AbilityTime -= Time.fixedDeltaTime;
            if (IsAbilityTime)
            {
                foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                {
                    if (p.IsBot()) continue;
                    if (p.PlayerId == CurrentPlayer.PlayerId) continue;
                    if (p.IsDead()) continue;
                    //ショートの距離か判定
                    if (Vector2.Distance(p.transform.position, CurrentPlayer.transform.position) > NormalGameOptionsV09.KillDistances.FirstOrDefault()) continue;
                    if (BattleTeam.GetTeam(CurrentPlayer).IsTeam(p)) continue;
                    CurrentPlayer.RpcMurderPlayer(p, true);
                }
            }
            if (AbilityTime <= 0)
            {
                PlayerAbility currentability = PlayerAbility.GetPlayerAbility(CurrentPlayer);
                //最後の硬直が終わったとき
                if (IsAbilityEnded)
                {
                    currentability.CanUseKill = true;
                    currentability.CanMove = true;
                    IsAbilityUsingNow = false;
                    IsAbilityEnded = false;
                    CurrentPlayer.RpcResetAbilityCooldown();
                }
                //AbilityTimeが終わったとき
                else if (IsAbilityTime)
                {
                    currentability.CanMove = false;
                    currentability.CanUseKill = false;
                    currentability.CanKill = true;
                    IsAbilityEnded = true;
                    IsAbilityTime = false;
                    AbilityTime = RoleParameter.KingPosterPlayerStuckTimeEnd;
                    currentability.MyKillCoolTime = GameOptionsManager.Instance.CurrentGameOptions.GetFloat(FloatOptionNames.KillCooldown);
                    CurrentPlayer.RpcSetHatUnchecked("");
                    CurrentPlayer.RpcSetVisorUnchecked("");
                    CurrentPlayer.RpcSetSkinUnchecked("");
                    CurrentPlayer.RpcSetHatUnchecked(Hat, CurrentPlayer);
                    CurrentPlayer.RpcSetVisorUnchecked(Visor, CurrentPlayer);
                    CurrentPlayer.RpcSetSkinUnchecked(Skin, CurrentPlayer);
                }
                //最初の硬直が終わった時
                else
                {
                    currentability.CanUseKill = true;
                    currentability.CanMove = true;
                    currentability.CanKill = false;
                    IsAbilityTime = true;
                    AbilityTime = RoleParameter.KingPosterPlayerAbilityTime;
                    currentability.MyKillCoolTime = 0.5f;
                    Hat = CurrentPlayer.Data.DefaultOutfit.HatId;
                    Skin = CurrentPlayer.Data.DefaultOutfit.SkinId;
                    Visor = CurrentPlayer.Data.DefaultOutfit.VisorId;
                    bool IsSpecialHat = ModHelpers.IsSuccessChance(1);
                    CurrentPlayer.RpcSetHatUnchecked(IsSpecialHat ? ModHelpers.GetRandom(RoleParameter.KingPosterAbilityCosmeticSpecialHats) : RoleParameter.KingPosterAbilityCosmeticHat);
                    CurrentPlayer.RpcSetVisorUnchecked(RoleParameter.KingPosterAbilityCosmeticVisor);
                    CurrentPlayer.RpcSetSkinUnchecked(ModHelpers.GetRandom(RoleParameter.KingPosterAbilityCosmeticSkins));
                }
                SyncBattleOptions.CustomSyncOptions(CurrentPlayer);
            }
        }
    }
    public override void UseAbility(PlayerControl target)
    {
        if (IsAbilityUsingNow) return;
        PlayerAbility ability = PlayerAbility.GetPlayerAbility(CurrentPlayer);
        ability.CanUseKill = false;
        ability.CanMove = false;
        SyncBattleOptions.CustomSyncOptions(CurrentPlayer);
        IsAbilityUsingNow = true;
        IsAbilityTime = false;
        IsAbilityEnded = false;
        AbilityTime = RoleParameter.KingPosterPlayerStuckTimeStart;
        ChangeName.SetNotification(ModTranslation.GetString("KingPosterStartMessage"), RoleParameter.KingPosterShowNotificationDurationTime);
        ChangeName.UpdateName(true);
    }
    public static void Clear()
    {
        KingPosters = new();
    }
}