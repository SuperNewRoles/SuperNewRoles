using HarmonyLib;
using SuperNewRoles.Roles;
using SuperNewRoles.Roles.Impostor;
using SuperNewRoles.Roles.RoleBases;
using UnityEngine;

namespace SuperNewRoles.Patches;

[HarmonyPatch(typeof(MapBehaviour), "get_IsOpenStopped")]
class MapBehaviorGetIsOpenStoppedPatch
{
    static bool Prefix(ref bool __result)
    { // イビルハッカーがアドミン使用中に移動できる
        if (PlayerControl.LocalPlayer.IsRole(RoleId.EvilHacker) && EvilHacker.CanMoveWhenUsesAdmin.GetBool())
        {
            __result = false;
            return false;
        }
        return true;
    }
}

[HarmonyPatch(typeof(MapBehaviour))]
public static class MapBehaviourPatch
{
    private static readonly Sprite doorClosedSprite = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.DoorClosed.png", 115f);
    private static readonly SpriteRenderer doorClosedRendererPrefab = CreatePrefab();
    /// <summary>マップ上のドア閉まってるよマークの配列<br/>インデックスで<see cref="ShipStatus.AllDoors"/>と対応する</summary>
    public static SpriteRenderer[] DoorClosedMarks;

    private static SpriteRenderer CreatePrefab()
    {
        var prefabObject = new GameObject("SNR_DoorClosed");
        var renderer = prefabObject.AddComponent<SpriteRenderer>();
        renderer.sprite = doorClosedSprite;
        // シーン切替時に破棄されないようにする これで1回生成すればずっと使える
        Object.DontDestroyOnLoad(prefabObject);
        prefabObject.SetActive(false);
        prefabObject.layer = 5;  // UIレイヤ
        return renderer;
    }

    [HarmonyPatch(nameof(MapBehaviour.Awake)), HarmonyPostfix]
    public static void AwakePostfix(MapBehaviour __instance)
    {
        if (!CanSeeDoorsMark())
        {
            return;
        }
        var allDoors = ShipStatus.Instance.AllDoors;
        var mapScale = ShipStatus.Instance.MapScale;
        DoorClosedMarks = new SpriteRenderer[allDoors.Length];
        for (int i = 0; i < allDoors.Length; i++)
        {
            var door = allDoors[i];
            var mark = DoorClosedMarks[i] = Object.Instantiate(doorClosedRendererPrefab, __instance.taskOverlay.transform.parent);
            var localPosition = door.transform.position / mapScale;
            localPosition.z = -3f;
            mark.transform.localPosition = localPosition;
            mark.gameObject.SetActive(true);
            mark.enabled = false;
        }
    }

    [HarmonyPatch(nameof(MapBehaviour.Show)), HarmonyPrefix]
    public static void ShowPrefix([HarmonyArgument(0)] MapOptions opts, ref bool __state /* Postfixで現在位置マークを表示させるかどうか */)
    {
        __state = false;
        EvilHacker evilHacker = PlayerControl.LocalPlayer.GetRoleBase<EvilHacker>();        // 会議中にマップを開くとアドミンを見ることができる
        if (evilHacker != null && EvilHacker.CanUseAdminDuringMeeting.GetBool() && MeetingHud.Instance && opts.Mode == MapOptions.Modes.Normal)
        {
            evilHacker.IsMyAdmin = true;
            opts.Mode = MapOptions.Modes.CountOverlay;
            __state = true;
        }
    }
    [HarmonyPatch(nameof(MapBehaviour.Show)), HarmonyPostfix]
    public static void ShowPostfix(MapBehaviour __instance, [HarmonyArgument(0)] MapOptions opts, bool __state)
    {
        if (!__instance.IsOpen)
        {
            return;
        }
        if (__state)
        {
            __instance.HerePoint.enabled = true;
        }
        EvilHacker evilHacker = PlayerControl.LocalPlayer.GetRoleBase<EvilHacker>();
        // サボタージュマップにアドミンを表示する
        if (evilHacker != null && EvilHacker.SabotageMapShowsAdmin.GetBool() && !MeetingHud.Instance && opts.Mode == MapOptions.Modes.Sabotage)
        {
            evilHacker.IsMyAdmin = true;
            __instance.countOverlay.gameObject.SetActive(true);
            __instance.countOverlay.SetOptions(true, true);
            __instance.countOverlayAllowsMovement = true;
            __instance.taskOverlay.Hide();
            __instance.HerePoint.enabled = true;
            PlayerControl.LocalPlayer.SetPlayerMaterialColors(__instance.HerePoint);
            __instance.ColorControl.SetColor(Palette.ImpostorRed);
            // アドミンがサボタージュとドア閉めのボタンに隠れないようにする
            // ボタンより手前
            __instance.countOverlay.transform.SetLocalZ(-3f);
        }
    }

    [HarmonyPatch(nameof(MapBehaviour.FixedUpdate)), HarmonyPostfix]
    public static void FixedUpdatePostfix()
    {
        if (!CanSeeDoorsMark())
        {
            return;
        }
        var allDoors = ShipStatus.Instance.AllDoors;
        for (int i = 0; i < allDoors.Length; i++)
        {
            var door = allDoors[i];
            var mark = DoorClosedMarks[i];
            if (door == null || mark == null)
            {
                continue;
            }
            mark.enabled = !door.IsOpen;
        }
    }

    public static bool CanSeeDoorsMark() => CachedPlayer.LocalPlayer.PlayerControl.IsRole(RoleId.EvilHacker) && EvilHacker.MapShowsDoorState.GetBool();
}
