using System;
using HarmonyLib;
using UnityEngine;

namespace SuperNewRoles.Patches;

/// <summary>
/// ゲーム設定メニューのボタン位置を変更するパッチ
/// </summary>
[HarmonyPatch(typeof(GameSettingMenu), nameof(GameSettingMenu.Start))]
public static class GameSettingMenuButtonsPatch
{
    // ボタンの位置を入れ替えたかどうかのフラグ
    public static bool HasSwappedButtons = false;

    public static void Postfix(GameSettingMenu __instance)
    {
        // ゲームプリセットボタンとゲーム設定ボタンの位置を入れ替える
        // これにより、タブの順序が「通常」「ロング」「ショート」の順になる
        Vector3 presetsButtonPos = __instance.GamePresetsButton.transform.localPosition;
        Vector3 settingsButtonPos = __instance.GameSettingsButton.transform.localPosition;

        // 位置を入れ替える
        __instance.GamePresetsButton.transform.localPosition = settingsButtonPos;
        __instance.GameSettingsButton.transform.localPosition = presetsButtonPos;

        // ボタンのクリックイベントも入れ替える
        var presetsButtonOnClick = __instance.GamePresetsButton.OnClick;
        var settingsButtonOnClick = __instance.GameSettingsButton.OnClick;

        __instance.GamePresetsButton.OnClick = settingsButtonOnClick;
        __instance.GameSettingsButton.OnClick = presetsButtonOnClick;

        // ボタンのホバーイベントも入れ替える
        var presetsButtonOnMouseOver = __instance.GamePresetsButton.OnMouseOver;
        var settingsButtonOnMouseOver = __instance.GameSettingsButton.OnMouseOver;

        __instance.GamePresetsButton.OnMouseOver = settingsButtonOnMouseOver;
        __instance.GameSettingsButton.OnMouseOver = presetsButtonOnMouseOver;

        // フラグを設定
        HasSwappedButtons = true;

        __instance.ChangeTab(0, false);

        // ログ出力
        Logger.Info("ゲームプリセットボタンとゲーム設定ボタンの位置とイベントを入れ替えました");
    }
}

/// <summary>
/// ゲーム設定メニューのタブ切り替え処理を変更するパッチ
/// </summary>
[HarmonyPatch(typeof(GameSettingMenu), nameof(GameSettingMenu.ChangeTab))]
public static class GameSettingMenuChangeTabPatch
{
    public static bool Prefix(GameSettingMenu __instance, ref int tabNum, bool previewOnly)
    {
        // ボタンの位置を入れ替えた場合、タブ番号も入れ替える
        if (GameSettingMenuButtonsPatch.HasSwappedButtons)
        {
            // タブ番号0と1を入れ替える
            if (tabNum == 0)
                tabNum = 1;
            else if (tabNum == 1)
                tabNum = 0;
        }

        // 元のメソッドを実行する
        return true;
    }
}