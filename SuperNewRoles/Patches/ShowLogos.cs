using HarmonyLib;
using SuperNewRoles.Modules;
using UnityEngine;

namespace SuperNewRoles.Patches;

// メインメニューのロゴ表示処理を担当するクラス
[HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
public static class MainMenuLogos
{
    // Transform拡張メソッド：位置とスケールを同時に設定する
    /// <summary>
    /// Transformの位置とスケールを一度に設定する拡張メソッド
    /// </summary>
    /// <param name="position">ローカル座標での位置</param>
    /// <param name="scale">適用するスケール値</param>
    public static void SetPositionAndScale(this Transform transform, Vector3 position, Vector3 scale)
    {
        transform.localPosition = position;
        transform.localScale = scale;
    }
    public static void Postfix()
    {
        ShowModStamp();
        CreateMainMenuLogo();
    }

    private static void ShowModStamp() =>
        FastDestroyableSingleton<ModManager>.Instance.ShowModStamp();

    private static void CreateMainMenuLogo()
    {
        // アセットバンドルからロゴ画像を読み込み
        var logo = AssetManager.GetAsset<Sprite>("banner", AssetManager.AssetBundleType.Sprite);
        if (logo == null)
        {
            Logger.Error("ロゴ画像の読み込みに失敗しました");
            return;
        }

        // メインメニュー用ロゴオブジェクトの作成
        var logoObject = new GameObject("MainMenuLogo");
        logoObject.transform.SetPositionAndScale(
            new(2.05f, 0.5f, 0),  // 右側に配置
            Vector3.one * 0.75f  // 適切なサイズに縮小
        );

        logoObject.AddComponent<SpriteRenderer>().sprite = logo;
    }
}

// バージョン情報表示のハンドリングクラス
public static class VersionTextHandler
{
    private const string ModColor = "#a6d289";
    private const float VersionTextScale = 1.5f;
    private const float CredentialsTextScale = 2.0f;

    [HarmonyPatch(typeof(VersionShower), nameof(VersionShower.Start))]
    private static class VersionShowerPatch
    {
        static void Postfix(VersionShower __instance)
        {
            if (GameObject.FindObjectOfType<MainMenuManager>() == null) return;
            CreateCredentialsText(__instance);
            CreateVersionText(__instance);
        }

        // クレジットテキスト生成処理
        /// <summary>
        /// 開発者クレジットテキストを作成し画面に表示する
        /// </summary>
        private static void CreateCredentialsText(VersionShower instance)
        {
            var credentials = Object.Instantiate(instance.text);
            credentials.transform.SetPositionAndScale(
                new Vector3(2, -0.3f, 0),
                Vector3.one * CredentialsTextScale
            );

            credentials.SetText(GetCredentialsText());
            credentials.alignment = TMPro.TextAlignmentOptions.Center;
            credentials.fontSize *= 0.9f;  // 文字サイズ微調整
        }

        // バージョン情報テキスト生成処理
        /// <summary>
        /// モッドのバージョン情報テキストを作成し表示する
        /// </summary>
        private static void CreateVersionText(VersionShower instance)
        {
            var version = Object.Instantiate(instance.text);
            version.transform.SetPositionAndScale(
                new Vector3(2, -0.65f, 0),  // クレジットテキストの直下
                Vector3.one * VersionTextScale
            );
            version.SetText($"{Statics.ModName} v{Statics.VersionString}");
        }

        /// <summary>
        /// Gitブランチ情報を含むクレジットテキストを生成
        /// </summary>
        /// <returns>フォーマット済みクレジット文字列</returns>
        private static string GetCredentialsText()
        {
            // ベータ版の場合のみブランチ情報を表示
            var branchInfo = Statics.IsBeta
                ? $"\r\n<color={ModColor}>{ThisAssembly.Git.Branch}({ThisAssembly.Git.Commit})</color>"
                : "";

            return branchInfo + ModTranslation.GetString("creditsMain");
        }
    }
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Start))]
    public static class ShowInGameLogosPatch
    {
        public static AspectPosition _aspectPosition { get; private set; }
        public static void Postfix(HudManager __instance)
        {
            GameObject _logoObject = new("Logo");
            _logoObject.layer = 5;
            _logoObject.transform.SetParent(__instance.transform);
            _logoObject.transform.localScale = Vector3.one * 0.2f;
            _logoObject.transform.localPosition = new(-3.8f, 2.5f, -1f);
            var _logoRenderer = _logoObject.AddComponent<SpriteRenderer>();
            _logoRenderer.sprite = AssetManager.GetAsset<Sprite>("banner", AssetManager.AssetBundleType.Sprite);
            // バージョンテキスト
            var versionText = GameObject.Instantiate(__instance.roomTracker.text);
            versionText.name = "VersionText";
            GameObject.Destroy(versionText.GetComponent<RoomTracker>());
            versionText.transform.SetParent(_logoObject.transform);
            versionText.transform.localScale = Vector3.one * 2.55f;
            versionText.transform.localPosition = new(-3.45f, -0.88f, 0f);
            versionText.SetText($"<color=#ffa500>v{VersionInfo.VersionString}</color>");
            _aspectPosition = _logoObject.AddComponent<AspectPosition>();
            _aspectPosition.Alignment = AspectPosition.EdgeAlignments.LeftTop;
            _aspectPosition.DistanceFromEdge = new(1, 0.27f);
            _aspectPosition.OnEnable();
        }
    }
    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Start))]
    public static class ShipStatusPatch
    {
        public static void Postfix()
        {
            ShowInGameLogosPatch._aspectPosition.Alignment = AspectPosition.EdgeAlignments.RightTop;
            ShowInGameLogosPatch._aspectPosition.DistanceFromEdge = new(4.13f, 0.5f);
            ShowInGameLogosPatch._aspectPosition.OnEnable();
        }
    }
}
