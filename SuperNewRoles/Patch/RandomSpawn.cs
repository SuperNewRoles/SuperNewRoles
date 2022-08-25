using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using PowerTools;
using SuperNewRoles.MapCustoms;
using SuperNewRoles.MapOptions;
using TMPro;
using UnhollowerBaseLib;
using UnityEngine;

namespace SuperNewRoles.Patch
{
    public static class RandomSpawn
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(SpawnInMinigame), nameof(SpawnInMinigame.Begin))]
        public static bool Prefix(SpawnInMinigame __instance, PlayerTask task)
        {
            if (!MapOption.RandomSpawn) return true;
            SpawnInMinigame.SpawnLocation[] array = __instance.Locations;
            array.Shuffle(0);
            array = (from s in array.Take(__instance.LocationButtons.Length)
                     orderby s.Location.x, s.Location.y descending
                     select s).ToArray<SpawnInMinigame.SpawnLocation>();
            PlayerControl.LocalPlayer.NetTransform.RpcSnapTo(new Vector2(-25f, 40f));

            for (int i = 0; i < __instance.LocationButtons.Length; i++)
            {
                PassiveButton passiveButton = __instance.LocationButtons[i];
                SpawnInMinigame.SpawnLocation pt = array[i];
                passiveButton.OnClick.AddListener((UnityEngine.Events.UnityAction)(() => __instance.SpawnAt(pt)));
                passiveButton.GetComponent<SpriteAnim>().Stop();
                passiveButton.GetComponent<SpriteRenderer>().sprite = pt.Image;
                // passiveButton.GetComponentInChildren<TextMeshPro>().text = DestroyableSingleton<TranslationController>.Instance.GetString(pt.Name, Array.Empty<object>());
                passiveButton.GetComponentInChildren<TextMeshPro>().text = DestroyableSingleton<TranslationController>.Instance.GetString(pt.Name, new Il2CppReferenceArray<Il2CppSystem.Object>(0));
                ButtonAnimRolloverHandler component = passiveButton.GetComponent<ButtonAnimRolloverHandler>();
                component.StaticOutImage = pt.Image;
                component.RolloverAnim = pt.Rollover;
                component.HoverSound = pt.RolloverSfx ? pt.RolloverSfx : __instance.DefaultRolloverSound;
            }


            PlayerControl.LocalPlayer.gameObject.SetActive(false);
            PlayerControl.LocalPlayer.NetTransform.RpcSnapTo(new Vector2(-25f, 40f));
                __instance.LocationButtons.Random().ReceiveClickUp();
            ControllerManager.Instance.OpenOverlayMenu(__instance.name, null, __instance.DefaultButtonSelected, __instance.ControllerSelectable, false);
            PlayerControl.HideCursorTemporarily();
            ConsoleJoystick.SetMode_Menu();
            return false;
        }
    }
}
