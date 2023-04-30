using AmongUs.GameOptions;
using HarmonyLib;
using SuperNewRoles.Mode;
using SuperNewRoles.Roles.Crewmate;
using UnityEngine;

namespace SuperNewRoles.Roles;

[HarmonyPatch(typeof(ExileController), nameof(ExileController.Begin))]
public class Bakery
{
    private static TMPro.TextMeshPro breadText;
    public static bool Prefix(
        ExileController __instance,
        [HarmonyArgument(0)] ref GameData.PlayerInfo exiled,
        bool tie)
    {
        if (RoleClass.Assassin.TriggerPlayer == null && RoleClass.Revolutionist.MeetingTrigger == null && (Balancer.currentAbilityUser == null || !Balancer.IsDoubleExile)) { if (!Agartha.MapData.IsMap(Agartha.CustomMapNames.Agartha)) return true; }
        if (Agartha.MapData.IsMap(Agartha.CustomMapNames.Agartha))
        {
            Agartha.ExileCutscenePatch.ExileControllerBeginePatch.Prefix(__instance, exiled, tie);
            if (RoleClass.Assassin.TriggerPlayer == null && RoleClass.Revolutionist.MeetingTrigger == null && (Balancer.currentAbilityUser == null || !Balancer.IsDoubleExile))
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
            if (exile != null && exile.IsRole(RoleId.Marlin))
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
            if (!Agartha.MapData.IsMap(Agartha.CustomMapNames.Agartha))
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
            if (!Agartha.MapData.IsMap(Agartha.CustomMapNames.Agartha))
                __instance.StartCoroutine(__instance.Animate());
        }
        else if (Balancer.currentAbilityUser != null)
        {
            if (!IsSec)
            {
                IsSec = true;
                __instance.exiled = null;
                ExileController controller = GameObject.Instantiate(__instance, __instance.transform.parent);
                controller.exiled = Balancer.targetplayerright.Data;
                controller.Begin(controller.exiled, false);
                IsSec = false;
                controller.completeString = string.Empty;

                controller.Text.gameObject.SetActive(false);
                controller.Player.UpdateFromEitherPlayerDataOrCache(controller.exiled, PlayerOutfitType.Default, PlayerMaterial.MaskType.Exile, includePet: false);
                controller.Player.ToggleName(active: false);
                SkinViewData skin = ShipStatus.Instance.CosmeticsCache.GetSkin(controller.exiled.Outfits[PlayerOutfitType.Default].SkinId);
                controller.Player.FixSkinSprite(skin.EjectFrame);
                AudioClip sound = null;
                if (controller.EjectSound != null)
                {
                    sound = new(controller.EjectSound.Pointer);
                }
                controller.EjectSound = null;
                void createlate(int index)
                {
                    new LateTask(() => { controller.StopAllCoroutines(); controller.StartCoroutine(controller.Animate()); }, 0.025f + index * 0.025f);
                }
                new LateTask(() => controller.StartCoroutine(controller.Animate()), 0f);
                for (int i = 0; i < 23; i++)
                {
                    createlate(i);
                }
                new LateTask(() => { controller.StopAllCoroutines(); controller.EjectSound = sound; controller.StartCoroutine(controller.Animate()); }, 0.6f);
                ExileController.Instance = __instance;
                __instance.exiled = Balancer.targetplayerleft.Data;
                exiled = __instance.exiled;
                return true;
            }
        }
        return false;
    }
    static bool IsSec;
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
            if (GameManager.Instance.LogicOptions.currentGameOptions.GetBool(BoolOptionNames.ConfirmImpostor)) breadText.transform.localPosition += new UnityEngine.Vector3(0f, -0.4f, 0f);    //位置がエ
            else breadText.transform.localPosition += new UnityEngine.Vector3(0f, -0.2f, 0f);
            breadText.gameObject.SetActive(true);                                               //文字の表示
        }
        if (Balancer.currentAbilityUser != null && Balancer.IsDoubleExile && __instance.exiled?.PlayerId == Balancer.targetplayerleft.PlayerId)
        {
            __instance.completeString = ModTranslation.GetString("BalancerDoubleExileText");
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