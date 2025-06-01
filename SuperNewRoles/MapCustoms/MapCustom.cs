using UnityEngine;
using SuperNewRoles.CustomOptions.Categories;

namespace SuperNewRoles.MapCustoms;

public static class MapCustomHandler
{
    public enum MapCustomId
    {
        Skeld,
        Mira,
        Polus,
        Dleks,
        Airship,
        TheFungle,
        Agartha
    }

    /// <summary>
    /// 指定されたマップでカスタム機能が有効かどうかを確認します
    /// </summary>
    /// <param name="mapCustomId">確認するマップID</param>
    /// <param name="checkOption">オプション設定を確認するかどうか</param>
    /// <returns>カスタム機能が有効な場合はtrue</returns>
    public static bool IsMapCustom(MapCustomId mapCustomId, bool checkOption = true)
    {
        // オプションチェックが有効で、どのカスタム機能も有効でない場合はfalse
        if (checkOption && !IsAnyCustomOptionEnabled())
            return false;

        // 現在のマップが指定されたマップIDと一致するか確認
        return IsCurrentMap(mapCustomId);
    }

    /// <summary>
    /// いずれかのマップカスタム機能が有効かどうかを確認します
    /// </summary>
    /// <returns>いずれかのカスタム機能が有効な場合はtrue</returns>
    private static bool IsAnyCustomOptionEnabled()
    {
        return MapEditSettingsOptions.AirshipRandomSpawn ||
               MapEditSettingsOptions.TheFungleSetting;
    }

    /// <summary>
    /// 現在のマップが指定されたマップIDと一致するか確認します
    /// </summary>
    /// <param name="mapCustomId">確認するマップID</param>
    /// <returns>現在のマップが指定されたマップIDと一致する場合はtrue</returns>
    private static bool IsCurrentMap(MapCustomId mapCustomId)
    {
        // ゲームが開始されていない場合はfalse
        if (AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started)
            return false;

        // 現在のマップIDを取得
        byte currentMapId = GameOptionsManager.Instance.CurrentGameOptions.MapId;

        return mapCustomId switch
        {
            MapCustomId.Skeld => currentMapId == 0,
            MapCustomId.Mira => currentMapId == 1,
            MapCustomId.Polus => currentMapId == 2,
            MapCustomId.Dleks => currentMapId == 3,
            MapCustomId.Airship => currentMapId == 4,
            MapCustomId.TheFungle => currentMapId == 5,
            MapCustomId.Agartha => currentMapId == 6,
            _ => false
        };
    }
}