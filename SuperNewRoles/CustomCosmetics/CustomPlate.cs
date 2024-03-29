using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HarmonyLib;
using SuperNewRoles.CustomCosmetics.CustomCosmeticsData;
using UnityEngine;
using static SuperNewRoles.CustomCosmetics.CustomCosmeticsData.CustomPlateData;

namespace SuperNewRoles.CustomCosmetics;

public class CustomPlate
{
    public static bool isAdded = false;
    static readonly List<NamePlateData> namePlateData = new();
    public static readonly List<CustomPlateData> customPlateData = new();
    [HarmonyPatch(typeof(HatManager), nameof(HatManager.GetNamePlateById))]
    class UnlockedNamePlatesPatch
    {
        public static void Postfix(HatManager __instance)
        {
            if (!DownLoadCustomCosmetics.IsLoad) return;

            if (isAdded || !DownLoadClassPlate.IsEndDownload) return;
            isAdded = true;
            SuperNewRolesPlugin.Logger.LogInfo("[CustomPlate] プレート読み込み処理開始");
            var AllPlates = __instance.allNamePlates.ToList();

            var plateDir = new DirectoryInfo("SuperNewRoles\\CustomPlatesChache");
            if (!plateDir.Exists) plateDir.Create();
            var Files = plateDir.GetFiles("*.png").ToList();
            Files.AddRange(plateDir.GetFiles("*.jpg"));
            var CustomPlates = new List<NamePlateData>();
            foreach (var file in Files)
            {
                try
                {
                    var FileName = file.Name[0..^4];
                    var Data = DownLoadClassPlate.platedetails.FirstOrDefault(data => data.resource.Replace(".png", "") == FileName);
                    TempPlateViewData tpvd = new()
                    {
                        Image = LoadTex.loadSprite("SuperNewRoles\\CustomPlatesChache\\" + Data.resource)
                    };
                    var plate = new CustomPlateData
                    {
                        tpvd = tpvd,
                        name = Data.name + "\nby " + Data.author,
                        ProductId = "CustomNamePlates_" + Data.resource.Replace(".png", "").Replace(".jpg", ""),
                        BundleId = "CustomNamePlates_" + Data.resource.Replace(".png", "").Replace(".jpg", ""),
                        displayOrder = 99,
                        ChipOffset = new Vector2(0f, 0.2f),
                        Free = true,
                        SpritePreview = tpvd.Image
                    };
                    //CustomPlates.Add(plate);
                    //AllPlates.Add(plate);
                    namePlateData.Add(plate);
                    customPlateData.Add(plate);
                    //SuperNewRolesPlugin.Logger.LogInfo("[CustomPlate] プレート読み込み完了:" + file.Name);
                }
                catch (Exception e)
                {
                    SuperNewRolesPlugin.Logger.LogError("[CustomPlate:Error] エラー:CustomNamePlateの読み込みに失敗しました:" + file.FullName);
                    SuperNewRolesPlugin.Logger.LogError(file.FullName + "のエラー内容:" + e);
                }
            }
            SuperNewRolesPlugin.Logger.LogInfo("[CustomPlate] プレート読み込み処理終了");
            AllPlates.AddRange(namePlateData);
            __instance.allNamePlates = AllPlates.ToArray();
        }
    }
}