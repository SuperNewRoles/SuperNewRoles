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
using System.Threading.Tasks;

namespace SuperNewRoles.CustomCosmetics
{
    public class CustomPlate
    {
        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.SetHat))]
        class Sethat
        {
            public static void Postfix(PlayerControl __instance, [HarmonyArgument(0)] ref string colorid)
            {
                SuperNewRolesPlugin.Logger.LogInfo("[SetHat]" + __instance.nameText.text + ":" + colorid);
            }
        }
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

                    var plateDir = new DirectoryInfo("SuperNewRoles\\CustomPlatesChache");
                    if (!plateDir.Exists) plateDir.Create();
                    var pngFiles = plateDir.GetFiles("*.png");
                    var CustomPlates = new List<NamePlateData>();
                    foreach (var file in pngFiles)
                    {
                        try
                        {
                            var plate = ScriptableObject.CreateInstance<NamePlateData>();
                        var FileName = file.Name.Substring(0, file.Name.Length - 4);
                        var Data = DownLoadClass.platedetails.FirstOrDefault(data => data.resource.Replace(".png", "") == FileName);
                            plate.name = Data.name+"\nby "+Data.author;
                            plate.ProductId = "CustomNamePlates_" + Data.resource.Replace(".png", "");
                            plate.BundleId = "CustomNamePlates_" + Data.resource.Replace(".png", "");
                            plate.Order = 99;
                            plate.ChipOffset = new Vector2(0f, 0.2f);
                            plate.Free = true;
                            plate.Image = LoadTex.loadSprite("SuperNewRoles\\CustomPlatesChache\\" + Data.resource);
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
}