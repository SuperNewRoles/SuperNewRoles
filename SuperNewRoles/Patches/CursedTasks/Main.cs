using System.Collections.Generic;
using SuperNewRoles;
using SuperNewRoles.CustomOptions.Categories;
using SuperNewRoles.Mode;
using SuperNewRoles.Modules;
using SuperNewRoles.Patches;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SuperNewRoles.Patches.CursedTasks;

public static class Main
{
    public static bool IsCursed => GameSettingOptions.CursedTaskOption && ModeManager.IsMode(ModeId.Default);

    private static GameObject _cursedLogoObject;

    public static void ClearAndReload()
    {
        CursedDivertPowerTask.Data = new();
        CursedDivertPowerTask.SliderOrder = null;

        CursedDressUpTask.IsDisabledPlatform = false;

        CursedFixShowerTask.Data = new();
        CursedStartFansTask.Data = new();
        CursedSampleTask.Data = new();
        CursedShowerTask.Timer = new();
        CursedToiletTask.Count = new();

        // 以前の呪いロゴが残っていたら破棄
        if (_cursedLogoObject != null)
        {
            Object.Destroy(_cursedLogoObject);
            _cursedLogoObject = null;
        }
    }

    public static void IntroFinished()
    {
        if (!IsCursed) return;
        CursedMushroom.SpawnCustomMushroomFungle();
    }

    /// <summary>
    /// イントロ完了時点で呪いモードが有効なら、画面右上ロゴの横に呪いロゴを表示する
    /// </summary>
    public static void ShowCursedLogoIfNeeded()
    {
        if (!IsCursed) return;

        // 既に生成済みなら何もしない
        if (_cursedLogoObject != null) return;

        var aspect = VersionTextHandler.ShowInGameLogosPatch._aspectPosition;
        if (aspect == null) return;

        var logoObject = aspect.gameObject;
        if (logoObject == null) return;

        var sprite = AssetManager.GetAsset<Sprite>("CursedLogo", AssetManager.AssetBundleType.Sprite);
        if (sprite == null)
        {
            Logger.Warning("CursedLogo sprite not found.");
            return;
        }

        _cursedLogoObject = new GameObject("CursedLogo");
        _cursedLogoObject.layer = logoObject.layer;
        _cursedLogoObject.transform.SetParent(logoObject.transform);

        // 既存ロゴの右隣に少しだけずらして配置
        _cursedLogoObject.transform.localScale = Vector3.one;
        _cursedLogoObject.transform.localPosition = new Vector3(-4.75f, 1.3f, -0.1f);
        _cursedLogoObject.transform.rotation = Quaternion.Euler(0f, 0f, 20f);

        var renderer = _cursedLogoObject.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
    }

    public static int Num => ModHelpers.GetRandomInt(1) == 1 ? 1024 : 1183;
}
