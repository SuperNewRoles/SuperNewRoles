using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using HarmonyLib;
using Hazel;
using UnityEngine;
using BepInEx.IL2CPP.Utils;
using System.Collections;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace SuperNewRoles.MapOptions
{
    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Awake))]
    public class AddVitals
    {
        public static void Postfix()
        {
            if (PlayerControl.GameOptions.MapId == 1 && MapOption.AddVitalsMira.getBool() && Mode.ModeHandler.isMode(Mode.ModeId.Default) && MapOption.MapOptionSetting.getBool() )
            {
                Transform Vital = GameObject.Instantiate(PolusObject.transform.FindChild("Office").FindChild("panel_vitals"), GameObject.Find("MiraShip(Clone)").transform);
                Vital.transform.position = new Vector3(8.5969f, 14.6337f, 0.0142f);
            }
        }
        public static GameObject PolusObject => Polus.gameObject;
        public static ShipStatus Polus;
        public static IEnumerator LoadPolus()
        {
            while ((UnityEngine.Object)(object)AmongUsClient.Instance == null)
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
        [HarmonyPrefix]
        [HarmonyPriority(900)]
        public static void Prefix(AmongUsClient __instance)
        {
            ((MonoBehaviour)(object)__instance).StartCoroutine(AddVitals.LoadPolus());
        }
    }
}