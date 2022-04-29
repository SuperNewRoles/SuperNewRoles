﻿using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using Hazel;
using SuperNewRoles.Patches;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using SuperNewRoles;
using SuperNewRoles.Roles;
using SuperNewRoles.Helpers;

namespace SuperNewRoles.Roles
{
    [HarmonyPatch(typeof(ExileController), nameof(ExileController.Begin))]
    public class Bakery
    {
        private static TMPro.TextMeshPro breadText;
        private static TMPro.TextMeshPro text;
        //生存判定
        public static bool BakeryAlive()
        {
            foreach (PlayerControl p in RoleClass.Bakery.BakeryPlayer)
            {
                if (p.isAlive())
                {
                    SuperNewRolesPlugin.Logger.LogInfo("パン屋が生きていると判定されました");
                    return true;
                }
            }
            SuperNewRolesPlugin.Logger.LogInfo("パン屋が生きていないと判定されました");
            return false;
        }

        static void Postfix(ExileController __instance)
        {
            breadText = UnityEngine.Object.Instantiate(                                             //文字定義
                    __instance.ImpostorText,
                    __instance.Text.transform);
            breadText.text = "パン屋によってパンが振舞われました";                                  //文字の内容を変える
            bool isBakeryAlive = BakeryAlive();                                                     //Boolの取得(生存判定)
            if (isBakeryAlive)                                                                      //if文(Bakeryが生きていたら実行)
            {
                SuperNewRolesPlugin.Logger.LogInfo("パン屋がパンを焼きました");                     //ログ
                if (PlayerControl.GameOptions.ConfirmImpostor)
                {
                    breadText.transform.localPosition += new UnityEngine.Vector3(0f, -0.4f, 0f);    //位置がエ
                }
                else
                {
                    breadText.transform.localPosition += new UnityEngine.Vector3(0f, -0.2f, 0f);
                }
                breadText.gameObject.SetActive(true);                                               //文字の表示
            }
        }

        //会議終了
        [HarmonyPatch(typeof(ExileController), nameof(ExileController.ReEnableGameplay))]
        public class BakeryChatDisable
        {
            static void Postfix(ExileController __instance)
            {
                breadText.gameObject.SetActive(false);
            }
        }
    }
}
