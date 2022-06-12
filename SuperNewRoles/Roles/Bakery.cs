using System;
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
using SuperNewRoles.Mode;

namespace SuperNewRoles.Roles
{
    [HarmonyPatch(typeof(ExileController), nameof(ExileController.Begin))]
    public class Bakery
    {
        private static TMPro.TextMeshPro breadText;
        private static TMPro.TextMeshPro text;
        public static bool Prefix(
            ExileController __instance,
            [HarmonyArgument(0)] GameData.PlayerInfo exiled,
            [HarmonyArgument(1)] bool tie)
        {
            if (RoleClass.Assassin.TriggerPlayer == null) { return true; }

            if (__instance.specialInputHandler != null)
            {
                __instance.specialInputHandler.disableVirtualCursor = true;
            }
            ExileController.Instance = __instance;
            ControllerManager.Instance.CloseAndResetAll();

            __instance.Text.gameObject.SetActive(false);
            __instance.Text.text = string.Empty;

            PlayerControl player = RoleClass.Assassin.TriggerPlayer;

            string printStr;

            var exile = ModeHandler.isMode(ModeId.SuperHostRoles) ? Mode.SuperHostRoles.main.RealExiled : exiled.Object;
            if (exile != null && exile.isRole(CustomRPC.RoleId.Marine))
            {
                printStr = player.Data.PlayerName + ModTranslation.getString("AssassinSucsess");
                RoleClass.Assassin.IsImpostorWin = true;
            }
            else
            {
                printStr = player.Data.PlayerName + ModTranslation.getString(
                    "AssassinFail");
                RoleClass.Assassin.DeadPlayer = RoleClass.Assassin.TriggerPlayer;
            }
            RoleClass.Assassin.TriggerPlayer = null;
            __instance.exiled = null;
            __instance.Player.gameObject.SetActive(false);
            __instance.completeString = printStr;
            __instance.ImpostorText.text = string.Empty;
            __instance.StartCoroutine(__instance.Animate());
            return false;
        }
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
        public static string GetExileText(){
            //翻訳
            var rand = new System.Random();
            if (rand.Next(1, 10) == 1)
            {
                return ModTranslation.getString("BakeryExileText2");
            }
            else
            {
                return ModTranslation.getString("BakeryExileText");
            }
        }

        static void Postfix(ExileController __instance)
        {
            breadText = UnityEngine.Object.Instantiate(                                             //文字定義
                    __instance.ImpostorText,
                    __instance.Text.transform);
            breadText.text = GetExileText();                                                        //文字の内容を変える
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
