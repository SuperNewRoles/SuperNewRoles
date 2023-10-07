using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using SuperNewRoles.Helpers;
using SuperNewRoles.Mode;
using SuperNewRoles.Mode.SuperHostRoles;
using UnityEngine;

namespace SuperNewRoles.MapCustoms;

public static class AirShipRandomSpawn
{
    public static List<Vector2> Locations;
    public static bool IsLoaded;
    public static int LastCount;
    public static void ClearAndReload()
    {
        Locations = new();
        IsLoaded = false;
        LastCount = -1;
    }

    [HarmonyPatch(typeof(SpawnInMinigame))]
    public static class SpawnInMinigamePatch
    {
        [HarmonyPatch(nameof(SpawnInMinigame.Begin)), HarmonyPrefix]
        public static bool BeginPrefix(SpawnInMinigame __instance)
        {
            if (!(MapCustomHandler.IsMapCustom(MapCustomHandler.MapCustomId.Airship, false) && MapCustom.AirshipRandomSpawn.GetBool())) return true;
            if (ModeHandler.IsMode(ModeId.Default, false)) PlayerControl.LocalPlayer.RpcSnapTo(__instance.Locations.Random().Location);
            else
            {
                if (ModeHandler.GetMode(false) is ModeId.CopsRobbers or ModeId.PantsRoyal) return true;
                Locations = __instance.Locations.ToList().ConvertAll(x => (Vector2)x.Location);
                IsLoaded = true;
                LastCount = -1;
                PlayerControl.LocalPlayer.RpcSnapTo(new(-30, 30));
            }
            __instance.Close();
            return false;

            /*
            __instance.Cast<Minigame>().Begin(task);
            SpawnInMinigame.SpawnLocation[] array = __instance.Locations.ToArray();
            array.Shuffle(0);
            array = (from location in __instance.Locations.Take(__instance.LocationButtons.Length)
                     orderby location.Location.x ascending, location.Location.y descending select location).ToArray();
            PlayerControl.LocalPlayer.NetTransform.RpcSnapTo(new Vector2(-25f, 40f));

            for (int i = 0; i < __instance.LocationButtons.Length; i++)
            {
                PassiveButton button = __instance.LocationButtons[i];
                SpawnInMinigame.SpawnLocation pt = array[i];
                button.OnClick.AddListener((Action)(() => __instance.SpawnAt(pt)));
                button.GetComponent<SpriteAnim>().Stop();
                button.GetComponent<SpriteRenderer>().sprite = pt.Image;
                button.GetComponentInChildren<TextMeshPro>().text = FastDestroyableSingleton<TranslationController>.Instance.GetString(pt.Name);
                ButtonAnimRolloverHandler component = button.GetComponent<ButtonAnimRolloverHandler>();
                component.StaticOutImage = pt.Image;
                component.RolloverAnim = pt.Rollover;
                component.HoverSound = pt.RolloverSfx ? pt.RolloverSfx : __instance.DefaultRolloverSound;
            }
            //*/
        }
    }

    [HarmonyPatch(typeof(HudManager))]
    public static class HudManagerPatch
    {
        [HarmonyPatch(nameof(HudManager.Update)), HarmonyPostfix]
        public static void UpdatePostfix()
        {
            if (!AmongUsClient.Instance.AmHost) return;
            if (!(MapCustomHandler.IsMapCustom(MapCustomHandler.MapCustomId.Airship, false) && MapCustom.AirshipRandomSpawn.GetBool())) return;
            if (!IsLoaded) return;

            List<PlayerControl> players = new();
            bool EndLoaded = true;
            int NotLoadedCount = 0;
            foreach (PlayerControl p in PlayerControl.AllPlayerControls)
            {
                if (ModHelpers.IsPositionDistance(p.transform.position, new(3, 6), 0.5f) ||
                    ModHelpers.IsPositionDistance(p.transform.position, new(-25, 40), 0.5f) ||
                    ModHelpers.IsPositionDistance(p.transform.position, new(-1.4f, 2.3f), 0.5f)
                    )
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
                IsLoaded = false;
                foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                {
                    string name = p.GetDefaultName();
                    if (!p.AmOwner) p.RpcSetNamePrivate(name);
                    else p.SetName(name);
                    p.RpcSnapTo(Locations.GetRandom());
                }
                FixedUpdate.SetRoleNames();
            }
        }
    }
}
