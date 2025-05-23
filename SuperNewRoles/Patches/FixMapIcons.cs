using HarmonyLib;
using UnityEngine;

namespace SuperNewRoles.Patches;

[HarmonyPatch(typeof(MapTaskOverlay), nameof(MapTaskOverlay.SetIconLocation))]
public static class MapTaskOverlaySetIconLocationPatch
{
    public static bool Prefix(
        MapTaskOverlay __instance,
        [HarmonyArgument(0)] PlayerTask task)
    {
        Il2CppSystem.Collections.Generic.List<Vector2> locations = task.Locations;

        for (int i = 0; i < locations.Count; i++)
        {
            Vector3 localPosition = locations[i] / ShipStatus.Instance.MapScale;
            localPosition.z = -1f;
            PooledMapIcon pooledMapIcon = __instance.icons.Get<PooledMapIcon>();
            pooledMapIcon.transform.localScale = new Vector3(
                pooledMapIcon.NormalSize,
                pooledMapIcon.NormalSize,
                pooledMapIcon.NormalSize);
            if (PlayerTask.TaskIsEmergency(task))
            {
                pooledMapIcon.rend.color = Color.red;
                pooledMapIcon.alphaPulse.enabled = true;
                pooledMapIcon.rend.material.SetFloat("_Outline", 1f);
            }
            else
            {
                pooledMapIcon.rend.color = Color.yellow;
            }
            pooledMapIcon.name = task.name;
            pooledMapIcon.lastMapTaskStep = task.TaskStep;
            pooledMapIcon.transform.localPosition = localPosition;
            if (task.TaskStep > 0)
            {
                pooledMapIcon.alphaPulse.enabled = true;
                pooledMapIcon.rend.material.SetFloat("_Outline", 1f);
            }

            string key = $"{task.name}{i}";
            int index = 0;

            while (__instance.data.ContainsKey(key))
            {
                key = $"{key}_{index}";
                ++index;
            }

            __instance.data.Add(key, pooledMapIcon);
        }
        return false;
    }
}