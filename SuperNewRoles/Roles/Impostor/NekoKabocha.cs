using System.Collections.Generic;
using SuperNewRoles.Helpers;
using SuperNewRoles.Mode;
using UnityEngine;
using static SuperNewRoles.Modules.CustomOption;
using static SuperNewRoles.Modules.CustomOptionHolder;
using static SuperNewRoles.Roles.RoleClass;

namespace SuperNewRoles.Roles.Impostor;

public static class NekoKabocha
{
    private const int OptionId = 205400;
    public static CustomRoleOption NekoKabochaOption;
    public static CustomOption NekoKabochaPlayerCount;
    public static CustomOption KillCooldown;
    private static CustomOption CanRevengeCrewmate;
    private static CustomOption CanRevengeNeutral;
    private static CustomOption CanRevengeImpostor;
    private static CustomOption CanRevengeExiled;
    public static void SetupCustomOptions()
    {
        NekoKabochaOption = new(OptionId, true, CustomOptionType.Impostor, "NekoKabochaName", color, 1);
        NekoKabochaPlayerCount = Create(OptionId + 1, true, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], NekoKabochaOption);
        KillCooldown = Create(OptionId + 2, true, CustomOptionType.Impostor, "KillCooldown", 40f, 0f, 120f, 2.5f, NekoKabochaOption, format: "unitSeconds");
        CanRevengeCrewmate = Create(OptionId + 3, true, CustomOptionType.Impostor, "CanRevengeCrewmate", true, NekoKabochaOption);
        CanRevengeNeutral = Create(OptionId + 4, true, CustomOptionType.Impostor, "CanRevengeNeutral", true, NekoKabochaOption);
        CanRevengeImpostor = Create(OptionId + 5, true, CustomOptionType.Impostor, "CanRevengeImpostor", true, NekoKabochaOption);
        CanRevengeExiled = Create(OptionId + 6, true, CustomOptionType.Impostor, "CanRevengeExiled", true, NekoKabochaOption);
    }

    public static List<PlayerControl> NekoKabochaPlayer;
    public static Color32 color = ImpostorRed;
    public static bool CanRevengeCrew;
    public static bool CanRevengeNeut;
    public static bool CanRevengeImp;
    public static bool CanRevengeExile;
    public static void ClearAndReload()
    {
        NekoKabochaPlayer = new();
        CanRevengeCrew = CanRevengeCrewmate.GetBool();
        CanRevengeNeut = CanRevengeNeutral.GetBool();
        CanRevengeImp = CanRevengeImpostor.GetBool();
        CanRevengeExile = CanRevengeExiled.GetBool();
    }

    public static void OnWrapUp(PlayerControl exiled)
    {
        if (AmongUsClient.Instance.AmHost && CanRevengeExile && exiled.IsRole(RoleId.NekoKabocha))
        {
            List<PlayerControl> targets = new();
            foreach (PlayerControl p in CachedPlayer.AllPlayers)
            {
                if (p.IsDead()) continue;
                if (p.PlayerId == exiled.PlayerId) continue;
                if (p.IsBot()) continue;
                targets.Add(p);
            }
            if (targets.Count <= 0) return;
            if (ModeHandler.IsMode(ModeId.SuperHostRoles))
                targets.GetRandom().RpcInnerExiled();
            else
                targets.GetRandom().RpcExiledUnchecked();
        }
    }

    public static void OnKill(PlayerControl killer)
    {
        if ((killer.IsCrew() && CanRevengeCrew) ||
            (killer.IsNeutral() && CanRevengeNeut) ||
            (killer.IsImpostor() && CanRevengeImp))
        {
            killer.RpcMurderPlayer(killer, true);
            killer.RpcSetFinalStatus(FinalStatus.Revenge);
        }
    }
}