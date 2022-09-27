using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HarmonyLib;
using UnityEngine;

namespace SuperNewRoles.CustomCosmetics
{
    public class CustomVisor
    {
        public static bool isAdded = false;
        [HarmonyPatch(typeof(HatManager), nameof(HatManager.GetVisorById))]
        class UnlockedVisorPatch
        {
            public static void Postfix(HatManager __instance)
            {
                if (isAdded || !DownLoadClassVisor.IsEndDownload) return;
                isAdded = true;
                SuperNewRolesPlugin.Logger.LogInfo("[CustomVisor] バイザー読み込み処理開始");
                var AllPlates = __instance.allNamePlates;

                var plateDir = new DirectoryInfo("SuperNewRoles\\CustomVisorsChache");
                if (!plateDir.Exists) plateDir.Create();
                var Files = plateDir.GetFiles("*.png").ToList();
                Files.AddRange(plateDir.GetFiles("*.jpg"));
                var CustomPlates = new List<VisorData>();
                foreach (var file in Files)
                {
                    try
                    {
                        var plate = ScriptableObject.CreateInstance<VisorData>();
                        var FileName = file.Name[0..^4];
                        var Data = DownLoadClassVisor.Visordetails.FirstOrDefault(data => data.resource.Replace(".png", "") == FileName);
                        plate.name = Data.name + "\nby " + Data.author;
                        plate.ProductId = "CustomVisors_" + Data.resource.Replace(".png", "").Replace(".jpg", "");
                        plate.BundleId = "CustomVisors_" + Data.resource.Replace(".png", "").Replace(".jpg", "");
                        plate.displayOrder = 99;
                        plate.ChipOffset = new Vector2(0f, 0.2f);
                        plate.Free = true;
                        plate.viewData.viewData = new VisorViewData
                        {
                            IdleFrame = Data.IsTOP
                            ? ModHelpers.CreateSprite("SuperNewRoles\\CustomVisorsChache\\" + file.Name, true)
                            : LoadTex.loadSprite("SuperNewRoles\\CustomVisorsChache\\" + file.Name)
                        };
                        __instance.allVisors.Add(plate);
                        //SuperNewRolesPlugin.Logger.LogInfo("[CustomVisor] バイザー読み込み完了:" + file.Name);
                    }
                    catch (Exception e)
                    {
                        SuperNewRolesPlugin.Logger.LogError("[CustomVisor:Error] エラー:CustomVisorの読み込みに失敗しました:" + file.FullName);
                        SuperNewRolesPlugin.Logger.LogError(file.FullName + "のエラー内容:" + e);
                    }
                }
                SuperNewRolesPlugin.Logger.LogInfo("[CustomVisor] バイザー読み込み処理終了");
            }
        }
    }
}