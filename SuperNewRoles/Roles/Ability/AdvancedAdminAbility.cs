using System;
using SuperNewRoles.Events;
using SuperNewRoles.Modules;
using SuperNewRoles.Modules.Events.Bases;
using SuperNewRoles.Patches;
using UnityEngine;

namespace SuperNewRoles.Roles.Ability;

public record AdvancedAdminData(
    Func<bool> AdvancedAdmin,
    Func<bool> CanUseAdminDuringComms = null,
    Func<bool> CanUseAdminDuringMeeting = null,
    Func<bool> SabotageMapShowsAdmin = null,
    Func<bool> DistinctionImpostor = null,
    Func<bool> DistinctionDead = null,
    Func<bool> ShowDoorClosedMarks = null
)
{
    public bool CanUseAdvancedAdmin => AdvancedAdmin?.Invoke() ?? true;
    public bool canUseAdminDuringComms => CanUseAdvancedAdmin && (CanUseAdminDuringComms?.Invoke() ?? false);
    public bool canUseAdminDuringMeeting => CanUseAdvancedAdmin && (CanUseAdminDuringMeeting?.Invoke() ?? false);
    public bool sabotageMapShowsAdmin => CanUseAdvancedAdmin && (SabotageMapShowsAdmin?.Invoke() ?? false);
    public bool distinctionImpostor => CanUseAdvancedAdmin && (DistinctionImpostor?.Invoke() ?? false);
    public bool distinctionDead => CanUseAdvancedAdmin && (DistinctionDead?.Invoke() ?? false);
    public bool showDoorClosedMarks => CanUseAdvancedAdmin && (ShowDoorClosedMarks?.Invoke() ?? false);
}
public class AdvancedAdminAbility : AbilityBase
{
    public readonly AdvancedAdminData Data;
    private EventListener<MapBehaviourShowPrefixEventData> MapBehaviourShowPrefixEventListener;
    private EventListener<MapBehaviourShowPostfixEventData> MapBehaviourShowPostfixEventListener;
    private EventListener<MapBehaviourAwakePostfixEventData> MapBehaviourAwakePostfixEventListener;
    private EventListener<MapBehaviourFixedUpdatePostfixEventData> MapBehaviourFixedUpdatePostfixEventListener;
    private EventListener<MapBehaviourClosePostfixEventData> MapBehaviourClosePostfixEventListener;

    public AdvancedAdminAbility(AdvancedAdminData data)
    {
        this.Data = data;
    }
    public override void AttachToLocalPlayer()
    {
        MapBehaviourShowPrefixEventListener = MapBehaviourShowPrefixEvent.Instance.AddListener(OnMapBehaviourShowPrefix);
        MapBehaviourShowPostfixEventListener = MapBehaviourShowPostfixEvent.Instance.AddListener(OnMapBehaviourShowPostfix);
        MapBehaviourAwakePostfixEventListener = MapBehaviourAwakePostfixEvent.Instance.AddListener(OnMapBehaviourAwakePostfix);
        MapBehaviourFixedUpdatePostfixEventListener = MapBehaviourFixedUpdatePostfixEvent.Instance.AddListener(OnMapBehaviourFixedUpdatePostfix);
        MapBehaviourClosePostfixEventListener = MapBehaviourClosePostfixEvent.Instance.AddListener(OnMapBehaviourClosePostfix);
    }
    public override void DetachToLocalPlayer()
    {
        base.DetachToLocalPlayer();
        if (MapBehaviourShowPrefixEventListener != null)
            MapBehaviourShowPrefixEvent.Instance.RemoveListener(MapBehaviourShowPrefixEventListener);
        if (MapBehaviourShowPostfixEventListener != null)
            MapBehaviourShowPostfixEvent.Instance.RemoveListener(MapBehaviourShowPostfixEventListener);
        if (MapBehaviourAwakePostfixEventListener != null)
            MapBehaviourAwakePostfixEvent.Instance.RemoveListener(MapBehaviourAwakePostfixEventListener);
        if (MapBehaviourFixedUpdatePostfixEventListener != null)
            MapBehaviourFixedUpdatePostfixEvent.Instance.RemoveListener(MapBehaviourFixedUpdatePostfixEventListener);
        if (MapBehaviourClosePostfixEventListener != null)
            MapBehaviourClosePostfixEvent.Instance.RemoveListener(MapBehaviourClosePostfixEventListener);
    }
    private void OnMapBehaviourShowPrefix(MapBehaviourShowPrefixEventData data)
    {
        if (!Data.CanUseAdvancedAdmin || !Data.canUseAdminDuringMeeting || !MeetingHud.Instance || data.opts.Mode != MapOptions.Modes.Normal) return;
        DevicesPatch.DontCountBecausePortableAdmin = true;
        ModHelpers.UpdateMeetingHudMaskAreas(false);
        data.opts.Mode = MapOptions.Modes.CountOverlay;
    }
    private void OnMapBehaviourShowPostfix(MapBehaviourShowPostfixEventData data)
    {
        if (!data.__instance.IsOpen) return;
        if (Data.canUseAdminDuringMeeting && MeetingHud.Instance)
            data.__instance.HerePoint.enabled = true;

        if (!Data.sabotageMapShowsAdmin || MeetingHud.Instance || data.opts.Mode != MapOptions.Modes.Sabotage) return;
        DevicesPatch.DontCountBecausePortableAdmin = true;
        data.__instance.countOverlay.gameObject.SetActive(true);
        data.__instance.countOverlay.SetOptions(true, true);
        data.__instance.countOverlayAllowsMovement = true;
        data.__instance.taskOverlay.Hide();
        data.__instance.HerePoint.enabled = true;
        PlayerControl.LocalPlayer.SetPlayerMaterialColors(data.__instance.HerePoint);
        data.__instance.ColorControl.SetColor(new(0f, 0.73f, 1f));
        // アドミンがサボタージュとドア閉めのボタンに隠れないようにする
        // ボタンより手前
        data.__instance.countOverlay.transform.SetLocalZ(-3f);
    }

    private static SpriteRenderer DoorClosedRendererPrefab;
    private static SpriteRenderer[] DoorClosedMarks;
    private static SpriteRenderer CreatePrefab()
    {
        var prefabObject = new GameObject("SNR_DoorClosed");
        var renderer = prefabObject.AddComponent<SpriteRenderer>();
        renderer.sprite = AssetManager.GetAsset<Sprite>("DoorClosed.png");
        // シーン切替時に破棄されないようにする これで1回生成すればずっと使える
        GameObject.DontDestroyOnLoad(prefabObject);
        prefabObject.SetActive(false);
        prefabObject.layer = 5;  // UIレイヤ
        return renderer;
    }
    private void OnMapBehaviourAwakePostfix(MapBehaviourAwakePostfixEventData data)
    {
        if (!Data.showDoorClosedMarks || !data.__instance.IsOpen) return;
        if (!DoorClosedRendererPrefab) DoorClosedRendererPrefab = CreatePrefab();
        var allDoors = ShipStatus.Instance.AllDoors;
        var mapScale = ShipStatus.Instance.MapScale;
        DoorClosedMarks = new SpriteRenderer[allDoors.Length];
        for (int i = 0; i < allDoors.Length; i++)
        {
            var door = allDoors[i];
            var mark = DoorClosedMarks[i] = GameObject.Instantiate(DoorClosedRendererPrefab, data.__instance.taskOverlay.transform.parent);
            var localPosition = door.transform.position / mapScale;
            localPosition.z = -3f;
            mark.transform.localPosition = localPosition;
            mark.gameObject.SetActive(true);
            mark.enabled = false;
        }
    }
    private void OnMapBehaviourClosePostfix(MapBehaviourClosePostfixEventData data)
    {
        if (!Data.CanUseAdvancedAdmin || !Data.canUseAdminDuringMeeting || !MeetingHud.Instance) return;
        ModHelpers.UpdateMeetingHudMaskAreas(true);
    }
    private void OnMapBehaviourFixedUpdatePostfix(MapBehaviourFixedUpdatePostfixEventData data)
    {
        if (!Data.showDoorClosedMarks || !data.__instance.IsOpen) return;
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
}