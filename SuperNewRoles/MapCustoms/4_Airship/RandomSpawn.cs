using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using SuperNewRoles.Helpers;
using SuperNewRoles.Mode;
using SuperNewRoles.Mode.SuperHostRoles;
using SuperNewRoles.Roles;
using UnityEngine;

namespace SuperNewRoles.MapCustoms;

[HarmonyPatch]
public static class AirShipRandomSpawn
{
    public static List<Vector2> Locations;
    public static bool IsLoading;
    public static int LastCount;
    public static void ClearAndReload()
    {
        IsLoading = false;
        LastCount = -1;
    }

    [HarmonyPatch(typeof(SpawnInMinigame), nameof(SpawnInMinigame.Begin)), HarmonyPostfix]
    static void BeginPrefix(SpawnInMinigame __instance)
    {
        if (!(MapCustomHandler.IsMapCustom(MapCustomHandler.MapCustomId.Airship, false) && MapCustom.AirshipRandomSpawn.GetBool()) ||
            ModeHandler.GetMode(false) is ModeId.CopsRobbers or ModeId.PantsRoyal)
            return;

        //__instance.StopAllCoroutines();

        if (ModeHandler.IsMode(ModeId.Default, false))
            __instance.LocationButtons.Random().OnClick.Invoke();
        else
        {
            Locations = __instance.Locations.ToList().ConvertAll(x => (Vector2)x.Location);
            IsLoading = true;
            LastCount = -1;
            if (AmongUsClient.Instance.AmHost)
            {
                foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                    if (RoleClass.IsFirstMeetingEnd && !p.IsBot())
                        p.RpcSnapTo(new(3, 6));
            }
            PlayerControl.LocalPlayer.RpcSnapTo(new(-30, 30));
        }
        __instance.Close();
    }

    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update)), HarmonyPostfix]
    static void UpdatePostfix()
    {
        if (!AmongUsClient.Instance.AmHost ||
            !IsLoading ||
            AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started ||
            !(MapCustomHandler.IsMapCustom(MapCustomHandler.MapCustomId.Airship, false) && MapCustom.AirshipRandomSpawn.GetBool()))
            return;

        List<PlayerControl> players = new();
        bool EndLoaded = true;
        int NotLoadedCount = 0;
        foreach (PlayerControl p in PlayerControl.AllPlayerControls)
        {
            if (p.IsDead())
                continue;
            if (ModHelpers.IsPositionDistance(p.transform.position, new(3, 6), 0.5f) ||
                ModHelpers.IsPositionDistance(p.transform.position, new(-25, 40), 0.5f) ||
                ModHelpers.IsPositionDistance(p.transform.position, new(-1.4f, 2.3f), 0.5f))
            {
                EndLoaded = false;
                NotLoadedCount++;
            }
            else
            {
                players.Add(p);
                p.RpcSnapTo(new(-30, 30));
            }
        }

        if (LastCount != players.Count)
        {
            LastCount = players.Count;
            string name = "\n\n\n\n\n\n\n\n<size=300%><color=white>" + ModeHandler.PlayingOnSuperNewRoles + "</size>\n\n\n\n\n\n\n\n\n\n\n\n\n\n<size=200%><color=white>" + string.Format(ModTranslation.GetString("CopsSpawnLoading"), NotLoadedCount);
            foreach (PlayerControl p in PlayerControl.AllPlayerControls)
            {
                if (!p.AmOwner) p.RpcSetNamePrivate(name);
                else p.SetName(name);
            }
        }
        if (EndLoaded)
        {
            IsLoading = false;
            LastCount = -1;
            foreach (PlayerControl p in PlayerControl.AllPlayerControls)
            {
                string name = p.GetDefaultName();
                p.RpcSetNamePrivate(name);
                if (!p.IsBot())
                {
                    p.RpcSnapTo(Locations.GetRandom());
                    p.ResetKillCool(RoleClass.IsFirstMeetingEnd ? float.NegativeInfinity : 10f);
                }
            }
            new LateTask(() => ChangeName.SetRoleNames(), 0.1f, "RandomSpawnSetRoleNames");
        }
    }
}