using System.Collections;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using HarmonyLib;
using SuperNewRoles.Modules;
using UnityEngine;

namespace SuperNewRoles.Patches;

public static class MainMenuManagerPatch
{
    private static GameObject BackgroundCover;
    [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.OpenEnterCodeMenu))]
    public static class MainMenuManagerOpenEnterCodeMenuPatch
    {
        public static bool Prefix(MainMenuManager __instance, bool animate)
        {
            if (BackgroundCover == null)
            {
                BackgroundCover = GameObject.Instantiate(AssetManager.GetAsset<GameObject>("BackgroundCover"), __instance.transform);
            }
            if (BackgroundCover != null)
                BackgroundCover.SetActive(true);
            if (!__instance.animating)
            {
                __instance.ResetScreen();
                __instance.screenTint.enabled = true;
                __instance.enterCodeButtons.SetActive(value: true);
                for (int i = 0; i < __instance.mainButtons.Count; i++)
                {
                    __instance.mainButtons[i].ControllerNav.selectOnRight = __instance.entercodeField;
                }
                ControllerManager.Instance.SetCurrentSelected(__instance.entercodeField);
                if (animate)
                {
                    __instance.StartCoroutine(EnterCodeSlideCo(__instance).WrapToIl2Cpp());
                    return false;
                }
                __instance.enterCodeHeader.SetActive(value: true);
                __instance.enterCodeContainer.localPosition = new(0f, __instance.enterCodeContainer.localPosition.y, __instance.enterCodeContainer.position.z);
                __instance.onlineButtonsContainer.localPosition = new(0f, __instance.onlineButtonsContainer.localPosition.y, __instance.onlineButtonsContainer.position.z);
            }
            return false;
        }
        private static IEnumerator EnterCodeSlideCo(MainMenuManager __instance)
        {
            // オンラインボタンの表示と初期位置の設定
            __instance.onlineButtons.SetActive(true);
            __instance.onlineButtonsContainer.localPosition = new(0f, __instance.onlineButtonsContainer.localPosition.y, __instance.onlineButtonsContainer.localPosition.z);
            // エンターコードのコンテナを画面外右に配置しておく
            __instance.enterCodeContainer.localPosition = new(10f, __instance.enterCodeContainer.localPosition.y, __instance.enterCodeContainer.localPosition.z);
            __instance.animating = true;
            __instance.onlineHeader.SetActive(true);
            __instance.enterCodeHeader.SetActive(false);

            // オンラインボタンコンテナを左方向へスムーズにスライドアウト（0.3秒）
            Vector2 onlineStart = new(0f, __instance.onlineButtonsContainer.localPosition.y);
            Vector2 onlineEnd = new(-8f, __instance.onlineButtonsContainer.localPosition.y);
            yield return Effects.Slide2D(source: onlineStart, dest: onlineEnd, target: __instance.onlineButtonsContainer, duration: 0.1f);

            // エンターコードコンテナを右からスライドインさせる
            // オーバーシュートさせてから最終位置に戻る演出（バウンス効果）
            Vector2 enterStart = new(10f, __instance.enterCodeContainer.localPosition.y);
            Vector2 enterOvershoot = new(-1f, __instance.enterCodeContainer.localPosition.y);
            Vector2 enterFinal = new(0f, __instance.enterCodeContainer.localPosition.y);
            yield return Effects.Slide2D(__instance.enterCodeContainer, enterStart, enterOvershoot, 0.08f);
            yield return Effects.Slide2D(__instance.enterCodeContainer, enterOvershoot, enterFinal, 0.05f);

            // スライドアニメーション完了後の後処理
            __instance.onlineButtons.SetActive(false);
            __instance.enterCodeHeader.SetActive(true);
            __instance.animating = false;
        }
    }
    [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.ClickBackEnterCode))]
    public static class MainMenuManagerClickBackEnterCodePatch
    {
        public static bool Prefix(MainMenuManager __instance)
        {
            if (!__instance.animating)
            {
                __instance.StartCoroutine(AnimateEnterCodeExitSlideCo(__instance).WrapToIl2Cpp());
            }
            return false;
        }

        private static IEnumerator AnimateEnterCodeExitSlideCo(MainMenuManager __instance)
        {
            // オンラインボタンを表示し、初期位置を左側（オフスクリーン）に設定
            __instance.onlineButtons.SetActive(true);
            __instance.onlineButtonsContainer.localPosition = new(-8f, __instance.onlineButtonsContainer.localPosition.y, __instance.onlineButtonsContainer.localPosition.z);
            // エンターコードコンテナは中央に配置（表示中）
            __instance.enterCodeContainer.localPosition = new(0f, __instance.enterCodeContainer.localPosition.y, __instance.enterCodeContainer.localPosition.z);
            __instance.animating = true;
            // 現在、エンターコードヘッダーが表示され、オンラインヘッダーは非表示
            __instance.onlineHeader.SetActive(false);
            __instance.enterCodeHeader.SetActive(true);

            // エンターコードコンテナを右方向へバウンス効果付きでスライドアウト
            // 逆アニメーション：EnterCodeSlideCoで、コンテナは右側（10）から出現して-1を通り0に落ち着くので、
            // ここでは0からまず11（オーバーシュート）へ、次に10（オフスクリーン）へ移動する
            Vector2 codeStart = new(0f, __instance.enterCodeContainer.localPosition.y);
            Vector2 codeOvershoot = new(11f, __instance.enterCodeContainer.localPosition.y);
            Vector2 codeEnd = new(10f, __instance.enterCodeContainer.localPosition.y);
            yield return Effects.Slide2D(__instance.enterCodeContainer, codeStart, codeOvershoot, 0.08f);
            yield return Effects.Slide2D(__instance.enterCodeContainer, codeOvershoot, codeEnd, 0.05f);

            // オンラインボタンコンテナを左側（-8）から中央（0）へスライドイン
            Vector2 onlineStart = new(-8f, __instance.onlineButtonsContainer.localPosition.y);
            Vector2 onlineEnd = new(0f, __instance.onlineButtonsContainer.localPosition.y);
            yield return Effects.Slide2D(__instance.onlineButtonsContainer, onlineStart, onlineEnd, 0.1f);

            // アニメーション完了後にヘッダーを切り替え、オンラインメニューを開く
            __instance.enterCodeHeader.SetActive(false);
            __instance.onlineHeader.SetActive(true);
            __instance.animating = false;
            __instance.OpenOnlineMenu();
            if (BackgroundCover != null)
                BackgroundCover.SetActive(false);
        }
    }
}