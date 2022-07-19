using System.Collections;
using BepInEx.IL2CPP.Utils;
using HarmonyLib;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

namespace SuperNewRoles.MapCustoms
{
    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Awake))]
    public class AddVitals
    {
        public static void Postfix()
        {
            if (MapCustomHandler.IsMapCustom(MapCustomHandler.MapCustomId.Mira) && MapCustom.AddVitalsMira.GetBool())
            {
                Transform Vital = GameObject.Instantiate(PolusObject.transform.FindChild("Office").FindChild("panel_vitals"), GameObject.Find("MiraShip(Clone)").transform);
                Vital.transform.position = new Vector3(8.5969f, 14.6337f, 0.0142f);
            }
        }
        public static GameObject PolusObject => Polus.gameObject;
        public static ShipStatus Polus;
        public static IEnumerator LoadPolus()
        {
            while ((Object)(object)AmongUsClient.Instance == null)
            {
                yield return null;
            }
            AsyncOperationHandle<GameObject> polusAsset = AmongUsClient.Instance.ShipPrefabs.ToArray()[2].LoadAsset<GameObject>();
            while (!polusAsset.IsDone)
            {
                yield return null;
            }
            Polus = polusAsset.Result.GetComponent<ShipStatus>();
        }
    }
    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.Awake))]
    public static class AmongUsClient_Awake_Patch
    {
        private static bool Loaded;
        [HarmonyPrefix]
        [HarmonyPriority(900)]
        public static void Prefix(AmongUsClient __instance)
        {
            if (!Loaded)
            {
                ((MonoBehaviour)(object)__instance).StartCoroutine(AddVitals.LoadPolus());
            }
            Loaded = true;
        }
    }
}