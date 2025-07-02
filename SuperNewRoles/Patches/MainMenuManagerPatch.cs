using System.Collections;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using HarmonyLib;
using SuperNewRoles.Modules;
using UnityEngine;

namespace SuperNewRoles.Patches;

public static class MainMenuManagerPatch
{
    private static SpriteRenderer BackgroundCover;

    // BackgroundCoverの有効状態を管理するヘルパーメソッド
    private static void SetBackgroundCover(MainMenuManager instance, bool enterCode, bool superDark)
    {
        if (BackgroundCover == null)
        {
            BackgroundCover = GameObject.Instantiate(AssetManager.GetAsset<GameObject>("BackgroundCover"), instance.transform).GetComponent<SpriteRenderer>();
            BackgroundCover.transform.localScale = new(0.715f, 0.4f, 0.8f);
        }
        if (enterCode)
        {
            BackgroundCover.transform.localPosition = new Vector3(7, -0.2f, -0.1f);
            BackgroundCover.color = new Color(1, 1, 1, superDark ? 1f : 0.6f);
        }
        else
        {
            BackgroundCover.transform.localPosition = new Vector3(7, -0.2f, 2);
            BackgroundCover.color = new Color(1, 1, 1, 0.35f);
        }
    }

    // EnterCodeMenuを初期化する処理をまとめたヘルパーメソッド
    private static void SetupEnterCodeMenu(MainMenuManager instance)
    {
        instance.ResetScreen();
        instance.screenTint.enabled = true;
        instance.enterCodeButtons.SetActive(true);
        for (int i = 0; i < instance.mainButtons.Count; i++)
        {
            instance.mainButtons[i].ControllerNav.selectOnRight = instance.entercodeField;
        }
        ControllerManager.Instance.SetCurrentSelected(instance.entercodeField);
    }

    // 指定のTransformのX座標のみを更新するヘルパーメソッド
    private static void SetContainerX(Transform container, float x)
    {
        Vector3 pos = container.localPosition;
        pos.x = x;
        container.localPosition = pos;
    }

    // 複数のスライドアニメーションを連続して実行するためのヘルパーメソッド
    private static IEnumerator SlideTransition(Transform container, Vector2 start, Vector2 overshoot, Vector2 end, float duration1, float duration2)
    {
        yield return Effects.Slide2D(container, start, overshoot, duration1);
        yield return Effects.Slide2D(container, overshoot, end, duration2);
    }

    [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
    public static class MainMenuManagerStartPatch
    {
        public static void Postfix(MainMenuManager __instance)
        {
            SetBackgroundCover(__instance, false, false);
            __instance.freePlayButton.gameObject.SetActive(false);
            float avg = (__instance.howToPlayButton.transform.localPosition.x + __instance.freePlayButton.transform.localPosition.x) / 2;
            __instance.howToPlayButton.transform.localPosition = new(avg, __instance.howToPlayButton.transform.localPosition.y, __instance.howToPlayButton.transform.localPosition.z);
        }
    }
    [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.ResetScreen))]
    public static class MainMenuManagerResetScreenPatch
    {
        public static void Postfix(MainMenuManager __instance)
        {
            SetBackgroundCover(__instance, false, false);
        }
    }
    [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.OpenGameModeMenu))]
    public static class MainMenuManagerOpenGameModeMenuPatch
    {
        public static void Postfix(MainMenuManager __instance)
        {
            SetBackgroundCover(__instance, true, false);
        }
    }
    [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.OpenOnlineMenu))]
    public static class MainMenuManagerOpenOnlineMenuPatch
    {
        public static void Postfix(MainMenuManager __instance)
        {
            SetBackgroundCover(__instance, true, true);
        }
    }
    [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.OpenAccountMenu))]
    public static class MainMenuManagerOpenAccountMenuPatch
    {
        public static void Postfix(MainMenuManager __instance)
        {
            SetBackgroundCover(__instance, true, false);
        }
    }

    [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.OpenEnterCodeMenu))]
    public static class MainMenuManagerOpenEnterCodeMenuPatch
    {
        public static bool Prefix(MainMenuManager __instance, bool animate)
        {
            if (__instance.animating) return false;
            SetBackgroundCover(__instance, true, true);
            SetupEnterCodeMenu(__instance);

            if (animate)
            {
                __instance.StartCoroutine(EnterCodeSlideCo(__instance).WrapToIl2Cpp());
                return false;
            }

            __instance.enterCodeHeader.SetActive(true);
            SetContainerX(__instance.enterCodeContainer, 0f);
            SetContainerX(__instance.onlineButtonsContainer, 0f);
            return false;
        }

        private static IEnumerator EnterCodeSlideCo(MainMenuManager instance)
        {
            SetBackgroundCover(instance, true, true);
            // オンラインボタンの表示と初期位置の設定
            instance.onlineButtons.SetActive(true);
            SetContainerX(instance.onlineButtonsContainer, 0f);

            // エンターコードコンテナを画面外右（X=10）に配置
            SetContainerX(instance.enterCodeContainer, 10f);

            instance.animating = true;
            instance.onlineHeader.SetActive(true);
            instance.enterCodeHeader.SetActive(false);

            // オンラインボタンコンテナを左方向（X=0から-8）へスライドアウト
            Vector2 onlineStart = new(0f, instance.onlineButtonsContainer.localPosition.y);
            Vector2 onlineEnd = new(-8f, instance.onlineButtonsContainer.localPosition.y);
            yield return Effects.Slide2D(instance.onlineButtonsContainer, onlineStart, onlineEnd, 0.1f);

            // エンターコードコンテナを右からスライドインさせる（オーバーシュートしてバウンス効果）
            Vector2 enterStart = new(10f, instance.enterCodeContainer.localPosition.y);
            Vector2 enterOvershoot = new(-1f, instance.enterCodeContainer.localPosition.y);
            Vector2 enterFinal = new(0f, instance.enterCodeContainer.localPosition.y);
            yield return SlideTransition(instance.enterCodeContainer, enterStart, enterOvershoot, enterFinal, 0.08f, 0.05f);

            // スライドアニメーション完了後の後処理
            instance.onlineButtons.SetActive(false);
            instance.enterCodeHeader.SetActive(true);
            instance.animating = false;
            SetBackgroundCover(instance, true, true);
        }
    }

    [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.ClickBackEnterCode))]
    public static class MainMenuManagerClickBackEnterCodePatch
    {
        public static bool Prefix(MainMenuManager __instance)
        {
            if (__instance.animating) return false;
            SetBackgroundCover(__instance, true, true);
            __instance.StartCoroutine(AnimateEnterCodeExitSlideCo(__instance).WrapToIl2Cpp());
            return false;
        }

        private static IEnumerator AnimateEnterCodeExitSlideCo(MainMenuManager instance)
        {
            // オンラインボタンを表示し、オンラインボタンコンテナを左側（X=-8）に配置
            instance.onlineButtons.SetActive(true);
            SetContainerX(instance.onlineButtonsContainer, -8f);

            // エンターコードコンテナを中央（X=0）に配置
            SetContainerX(instance.enterCodeContainer, 0f);

            instance.animating = true;
            instance.onlineHeader.SetActive(false);
            instance.enterCodeHeader.SetActive(true);

            // エンターコードコンテナを右方向へバウンス効果付きでスライドアウト（X=0から11を経由して10）
            Vector2 codeStart = new(0f, instance.enterCodeContainer.localPosition.y);
            Vector2 codeOvershoot = new(11f, instance.enterCodeContainer.localPosition.y);
            Vector2 codeEnd = new(10f, instance.enterCodeContainer.localPosition.y);
            yield return SlideTransition(instance.enterCodeContainer, codeStart, codeOvershoot, codeEnd, 0.08f, 0.05f);

            // オンラインボタンコンテナを左側（X=-8）から中央（X=0）へスライドイン
            Vector2 onlineStart = new(-8f, instance.onlineButtonsContainer.localPosition.y);
            Vector2 onlineEnd = new(0f, instance.onlineButtonsContainer.localPosition.y);
            yield return Effects.Slide2D(instance.onlineButtonsContainer, onlineStart, onlineEnd, 0.1f);

            // スライド完了後、ヘッダーを切り替えオンラインメニューを起動
            instance.enterCodeHeader.SetActive(false);
            instance.onlineHeader.SetActive(true);
            instance.animating = false;
            instance.OpenOnlineMenu();
        }
    }
}