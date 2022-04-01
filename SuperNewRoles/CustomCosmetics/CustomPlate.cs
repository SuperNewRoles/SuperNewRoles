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
        public static bool isAdded = false;
        [HarmonyPatch(typeof(HatManager), nameof(HatManager.GetNamePlateById))]
        class UnlockedNamePlatesPatch
        {
            public static void Postfix(HatManager __instance)
            {
                if (isAdded) return;
                isAdded = true;
                SuperNewRolesPlugin.Logger.LogInfo("プレート読み込み処理開始");
                var AllPlates = __instance.allNamePlates;

                var plateDir = new DirectoryInfo("SuperNewRoles\\CustomPlatesChache");
                if (!plateDir.Exists) plateDir.Create();
                var pngFiles = plateDir.GetFiles("*.png");
                var CustomPlates = new List<NamePlateData>();
                foreach (var file in pngFiles)
                {
                    try
                    {
                        SuperNewRolesPlugin.Logger.LogInfo("1");
                        var plate = ScriptableObject.CreateInstance<NamePlateData>();
                        SuperNewRolesPlugin.Logger.LogInfo("2");
                        var FileName = file.Name.Substring(0, file.Name.Length - 4);
                        SuperNewRolesPlugin.Logger.LogInfo("3");
                        var Data = DownLoadClass.platedetails.FirstOrDefault(data => data.resource.Replace(".png", "") == FileName);
                        SuperNewRolesPlugin.Logger.LogInfo("4");
                        plate.name = Data.name + "\nby " + Data.author;
                        SuperNewRolesPlugin.Logger.LogInfo("5");
                        plate.ProductId = "CustomNamePlates_" + Data.resource.Replace(".png", "");
                        SuperNewRolesPlugin.Logger.LogInfo("6");
                        plate.BundleId = "CustomNamePlates_" + Data.resource.Replace(".png", "");
                        SuperNewRolesPlugin.Logger.LogInfo("7");
                        plate.displayOrder = 99;
                        SuperNewRolesPlugin.Logger.LogInfo("8");
                        plate.ChipOffset = new Vector2(0f, 0.2f);
                        SuperNewRolesPlugin.Logger.LogInfo("9");
                        plate.Free = true;
                        SuperNewRolesPlugin.Logger.LogInfo("10");
                        var a = plate.viewData;
                        SuperNewRolesPlugin.Logger.LogInfo("12");
                        var b = plate.viewData.viewData;
                        SuperNewRolesPlugin.Logger.LogInfo("13");
                        plate.viewData.viewData = new NamePlateViewData();
                        SuperNewRolesPlugin.Logger.LogInfo("17");
                        var c = plate.viewData.viewData.Image;
                        SuperNewRolesPlugin.Logger.LogInfo("11");
                        var d = LoadTex.loadSprite("SuperNewRoles\\CustomPlatesChache\\" + Data.resource);
                        SuperNewRolesPlugin.Logger.LogInfo("14");
                        c = d;
                        SuperNewRolesPlugin.Logger.LogInfo("15");
                        plate.viewData.viewData.Image = c;
                        SuperNewRolesPlugin.Logger.LogInfo("16");
                        //CustomPlates.Add(plate);
                        //AllPlates.Add(plate);
                        __instance.allNamePlates.Add(plate);
                        SuperNewRolesPlugin.Logger.LogInfo("12");
                        SuperNewRolesPlugin.Logger.LogInfo("プレート読み込み完了:" + file.Name);
                    }
                    catch(Exception e)
                    {
                        SuperNewRolesPlugin.Logger.LogError("エラー:CustomNamePlateの読み込みに失敗しました:" + file.FullName);
                        SuperNewRolesPlugin.Logger.LogError(file.FullName+"のエラー内容:"+e);
                    }
                }
                SuperNewRolesPlugin.Logger.LogInfo("プレート読み込み処理終了");

                //__instance.allNamePlates = AllPlates;
            }
        }
    }
}