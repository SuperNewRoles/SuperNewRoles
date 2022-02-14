using System.Text.RegularExpressions;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.IL2CPP;
using System;
using HarmonyLib;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnhollowerBaseLib;
using Hazel;
using System.Linq;

namespace SuperNewRoles.CustomCosmetics
{
    public class CustomPlate
    {
        [HarmonyPatch(typeof(HatManager), nameof(HatManager.GetUnlockedNamePlates))]
        class GetUnlockedNamePlatesPatch
        {
            public static bool isAdded = false;
            public static void Postfix(HatManager __instance, ref Il2CppReferenceArray<NamePlateData> __result)
            {
                if (isAdded) return;
                isAdded = true;
                SuperNewRolesPlugin.Logger.LogInfo("プレート読み込み処理開始");
                var AllPlates = __result.ToList();

                var plateDir = new DirectoryInfo("SuperNewRoles\\CustomPlates");
                if (!plateDir.Exists) plateDir.Create();
                var pngFiles = plateDir.GetFiles("*.png");
                var CustomPlates = new List<NamePlateData>();
                foreach (var file in pngFiles)
                {
                    try
                    {
                        var plate = ScriptableObject.CreateInstance<NamePlateData>();
                        string plateName = file.Name.Substring(0, file.Name.Length - 4);
                        plate.name = plateName;
                        plate.ProductId = "SNR_" + plateName;
                        plate.BundleId = plateName;
                        plate.Order = 99;
                        plate.ChipOffset = new Vector2(0f, 0.2f);
                        plate.Free = true;
                        plate.Image = LoadTex.loadSprite("SuperNewRoles\\CustomPlates\\" + file.Name);
                        CustomPlates.Add(plate);
                        AllPlates.Add(plate);
                        __instance.AllNamePlates.Add(plate);
                        SuperNewRolesPlugin.Logger.LogInfo("プレート読み込み完了:" + file.Name);
                    }
                    catch
                    {
                        SuperNewRolesPlugin.Logger.LogError("エラー:CustomNamePlateの読み込みに失敗しました:" + file.FullName);
                    }
                }
                SuperNewRolesPlugin.Logger.LogInfo("プレート読み込み処理終了");

                __result = AllPlates.ToArray();
            }
        }
    }
    
    [HarmonyPatch(typeof(CosmeticData), nameof(CosmeticData.GetItemName))]
    class CosmeticItemNamePatch
    {
        public static bool Prefix(CosmeticData __instance, ref string __result)
        {
                if (__instance.ProductId.StartsWith("SNR"))
                {
                    __result = __instance.ProductId = __instance.BundleId;
                    return false;
                }
                return true;
            
        }
    }
}