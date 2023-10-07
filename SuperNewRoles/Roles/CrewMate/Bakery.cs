using System.Text;
using AmongUs.GameOptions;
using HarmonyLib;
using SuperNewRoles.Mode;
using SuperNewRoles.Roles.Crewmate;
using UnityEngine;

namespace SuperNewRoles.Roles;

[HarmonyPatch(typeof(ExileController), nameof(ExileController.Begin))]
public class Bakery
{
    private static TMPro.TextMeshPro confirmImpostorSecondText;
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
    // 生存判定
    public static bool BakeryAlive()
    {
        if (RoleClass.Bakery.BakeryPlayer.Count <= 0) return false;

        foreach (PlayerControl p in RoleClass.Bakery.BakeryPlayer)
        {
            if (p.IsAlive())
            {
                Logger.Info("パン屋が生きていると判定されました");
                return true;
            }
        }
        Logger.Info("パン屋が生きていないと判定されました");
        return false;
    }
    public static string GetExileText()
    {
        // 翻訳
        var rand = new System.Random();
        return rand.Next(1, 10) == 1 ? ModTranslation.GetString("BakeryExileText2") : ModTranslation.GetString("BakeryExileText");
    }

    static void Postfix(ExileController __instance)
    {
        // 文字定義
        confirmImpostorSecondText = Object.Instantiate(
                __instance.ImpostorText,
                __instance.Text.transform);

        StringBuilder changeStringBuilder = new(); // 変更する文字を, 一時的に保管する。
        bool isUseConfirmImpostorSecondText = false; // 2つ目の追放テキストとして記載する内容はあるかを保存する

        bool isBakeryAlive = BakeryAlive(); // パン屋 生存判定
        (bool, string) isCrookGetInsure = Neutral.Crook.Ability.GetIsReceivedTheInsuranceAndAnnounce(); // 詐欺師 保険金受給判定

        // |:========== 2段目の追放確認テキスト 取得 ==========:|

        if (isBakeryAlive) // パン屋 生存していたら実行
        {
            Logger.Info("パン屋がパンを焼きました", "ConfirmImpostorSecondText"); // ログ
            isUseConfirmImpostorSecondText = true;
            changeStringBuilder.AppendLine(GetExileText());
        }

        if (isCrookGetInsure.Item1) // 詐欺師 保険金受給していたら実行
        {
            Logger.Info("詐欺師が保険金を受け取りました", "ConfirmImpostorSecondText"); // ログ
            isUseConfirmImpostorSecondText = true;
            changeStringBuilder.AppendLine(isCrookGetInsure.Item2);
        }

        // |:========== 2段目の追放確認テキスト 表示 ==========:|

        if (isUseConfirmImpostorSecondText)
        {
            // 文字位置変更
            if (GameManager.Instance.LogicOptions.currentGameOptions.GetBool(BoolOptionNames.ConfirmImpostor))
                confirmImpostorSecondText.transform.localPosition += new Vector3(0f, -0.4f, 0f);
            else confirmImpostorSecondText.transform.localPosition += new Vector3(0f, -0.2f, 0f);

            confirmImpostorSecondText.text = changeStringBuilder.ToString(); // 文字の内容を変える
            confirmImpostorSecondText.gameObject.SetActive(true); // 文字の表示
        }

        // |:================================================:|

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
            confirmImpostorSecondText.gameObject.SetActive(false);
        }
    }
}