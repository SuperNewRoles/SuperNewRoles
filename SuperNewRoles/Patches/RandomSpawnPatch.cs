using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using PowerTools;
using SuperNewRoles.MapCustoms;
using TMPro;
using UnityEngine;

namespace SuperNewRoles.Patches;

[HarmonyPatch(typeof(SpawnInMinigame), nameof(SpawnInMinigame.Begin))]
public static class RandomSpawn
{
    public static bool IsFirstSpawn = false;
    static void resetButtons()
    {
        IsFirstSpawn = false;
        CachedPlayer.LocalPlayer.PlayerControl.SetKillTimerUnchecked(10f);

    }
    static void SpawnAt(SpawnInMinigame __instance, Vector3 spawnAt)
    {
        if (MapCustomClearAndReload.AirshipRandomSpawn)
        {
            if (IsFirstSpawn) resetButtons();
            if (__instance.amClosing != Minigame.CloseState.None)
            {
                return;
            }
            __instance.gotButton = true;
            CachedPlayer.LocalPlayer.PlayerControl.gameObject.SetActive(true);
            __instance.StopAllCoroutines();
            CachedPlayer.LocalPlayer.NetTransform.RpcSnapTo(spawnAt);
            FastDestroyableSingleton<HudManager>.Instance.PlayerCam.SnapToTarget();
            __instance.Close();
        }
    }
    public static bool Prefix(SpawnInMinigame __instance, PlayerTask task)
    {
        if (!MapCustomClearAndReload.AirshipRandomSpawn) return true;
        __instance.MyTask = task;
        __instance.MyNormTask = task as NormalPlayerTask;
        if (CachedPlayer.LocalPlayer.PlayerControl)
        {
            if (MapBehaviour.Instance)
            {
                MapBehaviour.Instance.Close();
            }
            CachedPlayer.LocalPlayer.PlayerControl.NetTransform.Halt();
        }
        __instance.StartCoroutine(__instance.CoAnimateOpen());


        List<SpawnInMinigame.SpawnLocation> list = __instance.Locations.ToList();

        SpawnInMinigame.SpawnLocation[] array = list.ToArray<SpawnInMinigame.SpawnLocation>();
        array.Shuffle(0);
        array = (from s in array.Take(__instance.LocationButtons.Length)
                 orderby s.Location.x, s.Location.y descending
                 select s).ToArray<SpawnInMinigame.SpawnLocation>();
        CachedPlayer.LocalPlayer.PlayerControl.NetTransform.RpcSnapTo(new Vector2(-25f, 40f));

        for (int i = 0; i < __instance.LocationButtons.Length; i++)
        {
            PassiveButton passiveButton = __instance.LocationButtons[i];
            SpawnInMinigame.SpawnLocation pt = array[i];
            passiveButton.OnClick.AddListener((UnityEngine.Events.UnityAction)(() => SpawnAt(__instance, pt.Location)));
            passiveButton.GetComponent<SpriteAnim>().Stop();
            passiveButton.GetComponent<SpriteRenderer>().sprite = pt.Image;
            // passiveButton.GetComponentInChildren<TextMeshPro>().text = FastDestroyableSingleton<TranslationController>.Instance.GetString(pt.Name, Array.Empty<object>());
            passiveButton.GetComponentInChildren<TextMeshPro>().text = FastDestroyableSingleton<TranslationController>.Instance.GetString(pt.Name, new Il2CppReferenceArray<Il2CppSystem.Object>(0));
            ButtonAnimRolloverHandler component = passiveButton.GetComponent<ButtonAnimRolloverHandler>();
            component.StaticOutImage = pt.Image;
            component.RolloverAnim = pt.Rollover;
            component.HoverSound = pt.RolloverSfx ? pt.RolloverSfx : __instance.DefaultRolloverSound;
        }


        CachedPlayer.LocalPlayer.NetTransform.RpcSnapTo(__instance.Locations.Random().Location);
        __instance.Close();
        return false;
    }
}