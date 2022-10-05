using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HarmonyLib;
using UnityEngine;

namespace SuperNewRoles.CustomCosmetics
{
    public class CustomPlate
    {
        public static bool isAdded = false;
        [HarmonyPatch(typeof(HatManager), nameof(HatManager.GetNamePlateById))]
        class UnlockedNamePlatesPatch
        {
            public static void Postfix(HatManager __instance)
            {
                if (isAdded || !DownLoadClass.IsEndDownload) return;
                isAdded = true;
                SuperNewRolesPlugin.Logger.LogInfo("[CustomPlate] プレート読み込み処理開始");
                var AllPlates = __instance.allNamePlates;

                var plateDir = new DirectoryInfo("SuperNewRoles\\CustomPlatesChache");
                if (!plateDir.Exists) plateDir.Create();
                var Files = plateDir.GetFiles("*.png").ToList();
                Files.AddRange(plateDir.GetFiles("*.jpg"));
                var CustomPlates = new List<NamePlateData>();
                foreach (var file in Files)
                {
                    try
                    {
                        var plate = ScriptableObject.CreateInstance<NamePlateData>();
                        var FileName = file.Name[0..^4];
                        var Data = DownLoadClass.platedetails.FirstOrDefault(data => data.resource.Replace(".png", "") == FileName);
                        plate.name = Data.name + "\nby " + Data.author;
                        plate.ProductId = "CustomNamePlates_" + Data.resource.Replace(".png", "").Replace(".jpg", "");
                        plate.BundleId = "CustomNamePlates_" + Data.resource.Replace(".png", "").Replace(".jpg", "");
                        plate.displayOrder = 99;
                        plate.ChipOffset = new Vector2(0f, 0.2f);
                        plate.Free = true;
                        plate.viewData.viewData = new()
                        {
                            Image = LoadTex.loadSprite("SuperNewRoles\\CustomPlatesChache\\" + Data.resource)
                        };
                        //CustomPlates.Add(plate);
                        //AllPlates.Add(plate);
                        __instance.allNamePlates.Add(plate);
                        //SuperNewRolesPlugin.Logger.LogInfo("[CustomPlate] プレート読み込み完了:" + file.Name);
                    }
                    catch (Exception e)
                    {
                        SuperNewRolesPlugin.Logger.LogError("[CustomPlate:Error] エラー:CustomNamePlateの読み込みに失敗しました:" + file.FullName);
                        SuperNewRolesPlugin.Logger.LogError(file.FullName + "のエラー内容:" + e);
                    }
                }
                SuperNewRolesPlugin.Logger.LogInfo("[CustomPlate] プレート読み込み処理終了");

                //__instance.allNamePlates = AllPlates;
            }
        }
    }
}