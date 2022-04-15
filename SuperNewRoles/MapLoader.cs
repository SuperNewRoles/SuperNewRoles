using BepInEx.IL2CPP.Utils;
using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace SuperNewRoles
{
	public static class MapLoader
	{
		private static readonly List<ShipStatus> Maps = new List<ShipStatus>();
		private static readonly List<GameObject> MapObjects = new List<GameObject>();

		public static ShipStatus Skeld => Maps[0];

		public static ShipStatus Airship => Maps[1];

		public static ShipStatus Polus => Maps[2];

		public static GameObject SkeldObject => MapObjects[0];

		public static GameObject AirshipObject => MapObjects[1];

		public static GameObject PolusObject => MapObjects[2];

		public static IEnumerator LoadMaps()
		{
			if (Maps.Count == 3)
			{
				yield break;
			}
			while ((Object)(object)AmongUsClient.Instance == null)
			{
				yield return null;
			}
			AssetReference val = AmongUsClient.Instance.ShipPrefabs.ToArray()[0];
			AssetReference airship = AmongUsClient.Instance.ShipPrefabs.ToArray()[4];
			AssetReference polus = AmongUsClient.Instance.ShipPrefabs.ToArray()[2];
			AsyncOperationHandle<GameObject> skeldAsset = val.LoadAsset<GameObject>();
			while (!skeldAsset.IsDone)
			{
				yield return null;
			}
			Maps.Add(skeldAsset.Result.GetComponent<ShipStatus>());
			MapObjects.Add(skeldAsset.Result);
			AsyncOperationHandle<GameObject> airshipAsset = airship.LoadAsset<GameObject>();
			while (!airshipAsset.IsDone)
			{
				yield return null;
			}
			Maps.Add(airshipAsset.Result.GetComponent<ShipStatus>());
			MapObjects.Add(airshipAsset.Result);
			AsyncOperationHandle<GameObject> polusAsset = polus.LoadAsset<GameObject>();
			while (!polusAsset.IsDone)
			{
				yield return null;
			}
			Maps.Add(polusAsset.Result.GetComponent<ShipStatus>());
			MapObjects.Add(polusAsset.Result);
		}
	}


	[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.Awake))]
	public static class AmongUsClient_Awake_Patch
	{
		[HarmonyPrefix]
		[HarmonyPriority(800)]
		public static void Prefix(AmongUsClient __instance)
		{
			Il2CppSystem.Collections.Generic.List<AssetReference> shipPrefabs = __instance.ShipPrefabs;
			if (shipPrefabs == null || shipPrefabs.Count < 6)
			{
				((MonoBehaviour)(object)__instance).StartCoroutine(MapLoader.LoadMaps());
			}
		}
	}
}