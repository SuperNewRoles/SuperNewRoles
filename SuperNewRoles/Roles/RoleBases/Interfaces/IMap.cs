using HarmonyLib;

namespace SuperNewRoles.Roles.RoleBases.Interfaces;

/// <summary>
/// マップに関する物を変更するインターフェイス
/// </summary>
public interface IMap
{
    /// <summary>
    /// <see cref="MapBehaviour.Awake"/>の<see cref="HarmonyPostfix"/>に変更を加える
    /// </summary>
    /// <param name="__instance"><see cref="MapBehaviour"/>のインスタンス</param>
    public virtual void AwakePostfix(MapBehaviour __instance) { }

    /// <summary>
    /// <see cref="MapBehaviour.Show"/>の<see cref="HarmonyPrefix"/>に変更を加える
    /// </summary>
    /// <param name="__instance"><see cref="MapBehaviour"/>のインスタンス</param>
    /// <param name="opts">表示するマップの設定</param>
    /// <param name="__state">現在の自分の位置を表示するか</param>
    public virtual void ShowPrefix(MapBehaviour __instance, MapOptions opts, ref bool __state) { }

    /// <summary>
    /// <see cref="MapBehaviour.Show"/>の<see cref="HarmonyPostfix"/>に変更を加える
    /// </summary>
    /// <param name="__instance"><see cref="MapBehaviour"/>のインスタンス</param>
    /// <param name="opts">表示するマップの設定</param>
    public virtual void ShowPostfix(MapBehaviour __instance, MapOptions opts) { }

    /// <summary>
    /// <see cref="MapBehaviour.FixedUpdate"/>の<see cref="HarmonyPostfix"/>に変更を加える
    /// </summary>
    /// <param name="__instance"><see cref="MapBehaviour"/>のインスタンス</param>
    public virtual void FixedUpdatePostfix(MapBehaviour __instance) { }

    /// <summary>
    /// <see cref="MapBehaviour.get_IsOpenStopped"/>の<see cref="HarmonyPostfix"/>に変更を加える
    /// </summary>
    /// <param name="__instance"><see cref="MapBehaviour"/>のインスタンス</param>
    public virtual void IsOpenStoppedPostfix(MapBehaviour __instance, ref bool __result) { }
}
