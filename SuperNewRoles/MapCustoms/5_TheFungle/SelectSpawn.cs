using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using PowerTools;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using static Minigame;
using static Rewired.UI.ControlMapper.ControlMapper;

namespace SuperNewRoles.MapCustoms;
[HarmonyPatch]
public static class FungleSelectSpawn
{
    [HarmonyPatch(typeof(TranslationController), nameof(TranslationController.GetString), new[] {
                typeof(StringNames),
                typeof(Il2CppReferenceArray<Il2CppSystem.Object>)
            })]
    private class StringPatch
    {
        public static bool Prefix(ref string __result, [HarmonyArgument(0)] StringNames name)
        {
            if ((int)name == 50999)
            {
                __result = "キャンプファイアー";
                return false;
            }
            return true;
        }
    }
    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.PrespawnStep))]
    public class ShipStatisPrespawnStepPatch
    {
        public static bool Prefix(ShipStatus __instance, ref Il2CppSystem.Collections.IEnumerator __result)
        {
            if (!FungleHandler.IsFungleSpawnType(FungleHandler.FungleSpawnType.Select))
                return true;
            __result = SelectSpawn().WrapToIl2Cpp();
            return false;
        }
        static (StringNames, Vector3, string, Func<AudioClip>)[] Locations =
            {
                ((StringNames)50999, new Vector3(-9.81f, 0.6f),"Campfire", null),
                (StringNames.Dropship, new Vector3(-8f, 10.5f), "Dropship", null),
                (StringNames.Cafeteria, new Vector3(-16.16f, 7.25f), "Cafeteria", null),
                (StringNames.Kitchen, new Vector3(-15.5f, -7.5f), "Kitchen", null),
                (StringNames.Greenhouse, new Vector3(9.25f, -12f), "Hotroom", GetGreenHouseSound),
                (StringNames.UpperEngine, new Vector3(14.75f, 0f), "UpperEngine", null),
                (StringNames.Comms, new Vector3(21.65f, 13.75f), "Comms", null)
            };
        static Dictionary<SystemTypes, AudioClip> _cachedSounds = new();
        static AudioClip GetGreenHouseSound()
        {
            return null;
            if (_cachedSounds.TryGetValue(SystemTypes.Greenhouse, out AudioClip clip))
                return clip;
            if (!ShipStatus.Instance.FastRooms.TryGetValue(SystemTypes.Greenhouse, out PlainShipRoom room))
                return null;
            Transform trans = room.transform.FindChild("AMB_Muffled");
            if (trans == null)
                return null;
            clip = trans.GetComponent<AmbientSoundPlayer>().AmbientSound;
            clip.hideFlags |= HideFlags.DontUnloadUnusedAsset;
            return _cachedSounds[SystemTypes.Greenhouse] = clip;
        }
        static SpawnInMinigame.SpawnLocation Create(SpawnInMinigame miniGame, (StringNames, Vector3, string, Func<AudioClip>) obj)
        {
            SpawnInMinigame.SpawnLocation baseL = miniGame.Locations.FirstOrDefault();
            return new()
            {
                Name = obj.Item1,
                Location = obj.Item2,
                Image = ModHelpers.LoadSpriteFromResources($"SuperNewRoles.Resources.FungleSelectSpawn.{obj.Item3}.png", 90f),
                Rollover = null,
                RolloverSfx = obj.Item4?.Invoke()
            };
        }
        public static IEnumerator SelectSpawn()
        {
            SpawnInMinigame spawnInMinigame = GameObject.Instantiate<SpawnInMinigame>(Agartha.MapLoader.Airship.TryCast<AirshipStatus>().SpawnInGame);
            spawnInMinigame.transform.SetParent(Camera.main.transform, false);
            spawnInMinigame.transform.localPosition = new Vector3(0f, 0f, -600f);
            List<SpawnInMinigame.SpawnLocation> locations = new(Locations.Length);
            foreach (var loc in Locations)
            {
                locations.Add(Create(spawnInMinigame, loc));
            }
            spawnInMinigame.Locations = new(locations.ToArray());
            spawnInMinigame.Begin(null);
            foreach (PassiveButton button in spawnInMinigame.LocationButtons)
            {
                button.transform.localPosition = new(button.transform.localPosition.x,
                    0.5f, 0);
                button.GetComponentInChildren<TextMeshPro>().transform.localPosition = new(0f, -1.09f, 0f);
                BoxCollider2D collider = button.GetComponent<BoxCollider2D>();
                collider.size = new(1.7f, 1.5f);
                collider.offset = new(0f, 0.03f);
            }
            yield return spawnInMinigame.WaitForFinish();
        }
    }
}