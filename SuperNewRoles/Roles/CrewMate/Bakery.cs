using HarmonyLib;

using SuperNewRoles.Mode;

namespace SuperNewRoles.Roles
{
    [HarmonyPatch(typeof(ExileController), nameof(ExileController.Begin))]
    public class Bakery
    {
        private static TMPro.TextMeshPro breadText;
        public static bool Prefix(
            ExileController __instance,
            [HarmonyArgument(0)] GameData.PlayerInfo exiled,
            bool tie)
        {
            if (RoleClass.Assassin.TriggerPlayer == null) { if (!Agartha.MapData.IsMap(Agartha.CustomMapNames.Agartha)) return true; }
            if (Agartha.MapData.IsMap(Agartha.CustomMapNames.Agartha))
            {
                Agartha.ExileCutscenePatch.ExileControllerBeginePatch.Prefix(__instance, exiled, tie);
                if (RoleClass.Assassin.TriggerPlayer == null)
                {
                    return false;
                }
            };
            string printStr = "";

            if (RoleClass.Assassin.TriggerPlayer != null)
            {
                if (__instance.specialInputHandler != null) __instance.specialInputHandler.disableVirtualCursor = true;
                ExileController.Instance = __instance;
                ControllerManager.Instance.CloseAndResetAll();

                __instance.Text.gameObject.SetActive(false);
                __instance.Text.text = string.Empty;

                PlayerControl player = RoleClass.Assassin.TriggerPlayer;

                var exile = ModeHandler.IsMode(ModeId.SuperHostRoles) ? Mode.SuperHostRoles.Main.RealExiled : exiled.Object;
                if (exile != null && exile.IsRole(RoleId.Marine))
                {
                    printStr = player.Data.PlayerName + ModTranslation.GetString("AssassinSucsess");
                    RoleClass.Assassin.IsImpostorWin = true;
                }
                else
                {
                    printStr = player.Data.PlayerName + ModTranslation.GetString(
                        "AssassinFail");
                    RoleClass.Assassin.DeadPlayer = RoleClass.Assassin.TriggerPlayer;
                }
                RoleClass.Assassin.TriggerPlayer = null;
                __instance.exiled = null;
                __instance.Player.gameObject.SetActive(false);
                __instance.completeString = printStr;
                __instance.ImpostorText.text = string.Empty;
                __instance.StartCoroutine(__instance.Animate());
            }
            else if (RoleClass.Revolutionist.MeetingTrigger != null)
            {
                if (__instance.specialInputHandler != null) __instance.specialInputHandler.disableVirtualCursor = true;
                ExileController.Instance = __instance;
                ControllerManager.Instance.CloseAndResetAll();

                __instance.Text.gameObject.SetActive(false);
                __instance.Text.text = string.Empty;


                var exile = exiled.Object;
                if (exile != null && exile.IsRole(RoleId.Dictator))
                {
                    printStr = exiled.PlayerName + ModTranslation.GetString("RevolutionistSucsess");
                }
                else
                {
                    printStr = exiled.PlayerName + ModTranslation.GetString(
                        "RevolutionistFail");
                }
                __instance.exiled = null;
                __instance.Player.gameObject.SetActive(false);
                __instance.completeString = printStr;
                __instance.ImpostorText.text = string.Empty;
                __instance.StartCoroutine(__instance.Animate());
            }
            RoleClass.Assassin.TriggerPlayer = null;
            __instance.exiled = null;
            __instance.Player.gameObject.SetActive(false);
            __instance.completeString = printStr;
            __instance.ImpostorText.text = string.Empty;
            if (Agartha.MapData.IsMap(Agartha.CustomMapNames.Agartha))
            {
                return false;
            }
            __instance.StartCoroutine(__instance.Animate());
            return false;
        }
        //生存判定
        public static bool BakeryAlive()
        {
            foreach (PlayerControl p in RoleClass.Bakery.BakeryPlayer)
            {
                if (p.IsAlive())
                {
                    SuperNewRolesPlugin.Logger.LogInfo("パン屋が生きていると判定されました");
                    return true;
                }
            }
            SuperNewRolesPlugin.Logger.LogInfo("パン屋が生きていないと判定されました");
            return false;
        }
        public static string GetExileText()
        {
            //翻訳
            var rand = new System.Random();
            return rand.Next(1, 10) == 1 ? ModTranslation.GetString("BakeryExileText2") : ModTranslation.GetString("BakeryExileText");
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
                if (PlayerControl.GameOptions.ConfirmImpostor) breadText.transform.localPosition += new UnityEngine.Vector3(0f, -0.4f, 0f);    //位置がエ
                else breadText.transform.localPosition += new UnityEngine.Vector3(0f, -0.2f, 0f);
                breadText.gameObject.SetActive(true);                                               //文字の表示
            }
        }

        //会議終了
        [HarmonyPatch(typeof(ExileController), nameof(ExileController.ReEnableGameplay))]
        public class BakeryChatDisable
        {
            static void Postfix()
            {
                breadText.gameObject.SetActive(false);
            }
        }
    }
}