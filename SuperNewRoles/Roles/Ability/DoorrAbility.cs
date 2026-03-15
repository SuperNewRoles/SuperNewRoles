using UnityEngine;
using SuperNewRoles.Roles.Ability.CustomButton;
using SuperNewRoles.Modules;

namespace SuperNewRoles.Roles.Ability;

public class DoorrAbility : CustomButtonBase
{
    private const float AutoDoorUsableDistance = 1.5f;
    private static ShipStatus cachedShipStatus;
    // CheckIsAvailable が毎フレーム呼ばれるので、使用距離だけはマップ単位でキャッシュする。
    private static float[] cachedUsableDistanceSqr = [];

    public override float DefaultTimer => CoolTime;
    public override string buttonText => ModTranslation.GetString("DoorrButtonText");
    public override Sprite Sprite => AssetManager.GetAsset<Sprite>("DoorrDoorButton.png");
    protected override KeyType keytype => KeyType.Ability1;

    public float CoolTime { get; set; }

    public DoorrAbility(float coolTime)
    {
        CoolTime = coolTime;
    }

    public override bool CheckIsAvailable()
    {
        // ボタン表示は「移動可能で、今の位置から触れるドアがあるか」だけを見る。
        return PlayerControl.LocalPlayer && PlayerControl.LocalPlayer.CanMove && TryGetDoorIndex(out _);
    }

    public override void OnClick()
    {
        if (TryGetDoorIndex(out int doorIndex))
        {
            // ドア指定は Id ではなく AllDoors の index を使う。
            RpcSetDoorway(doorIndex, !ShipStatus.Instance.AllDoors[doorIndex].IsOpen);
            ResetTimer();
        }
    }

    [CustomRPC]
    public void RpcSetDoorway(int doorIndex, bool isOpen)
    {
        var allDoors = ShipStatus.Instance?.AllDoors;
        if (allDoors == null) return;
        if (doorIndex < 0 || doorIndex >= allDoors.Length) return;

        // 受信側でも同じ index から対象ドアを引く。
        var door = allDoors[doorIndex];
        if (door == null) return;
        SetDoorway(door, isOpen);
    }

    private static void SetDoorway(OpenableDoor door, bool isOpen)
    {
        var shipStatus = ShipStatus.Instance;
        if (door is AutoOpenDoor autoOpenDoor && shipStatus && shipStatus.Systems.TryGetValue(SystemTypes.Doors, out var doorSystem))
        {
            // AutoOpenDoor は System 経由で更新しないと dirty bit が立たず同期されない。
            var autoDoorsSystem = doorSystem.TryCast<AutoDoorsSystemType>();
            if (autoDoorsSystem != null)
            {
                if (shipStatus.Type == ShipStatus.MapType.Ship)
                {
                    SetRoomAutoDoors(autoDoorsSystem, shipStatus.AllDoors, autoOpenDoor, isOpen);
                }
                else
                {
                    autoDoorsSystem.SetDoor(autoOpenDoor, isOpen);
                }
                return;
            }
        }

        door.SetDoorway(isOpen);
    }

    private static void SetRoomAutoDoors(AutoDoorsSystemType autoDoorsSystem, OpenableDoor[] allDoors, AutoOpenDoor targetDoor, bool isOpen)
    {
        bool updated = false;
        for (int i = 0; i < allDoors.Length; i++)
        {
            if (allDoors[i] is not AutoOpenDoor roomDoor || roomDoor.Room != targetDoor.Room) continue;

            autoDoorsSystem.SetDoor(roomDoor, isOpen);
            updated = true;
        }

        if (!updated)
        {
            autoDoorsSystem.SetDoor(targetDoor, isOpen);
        }
    }

    private static bool TryGetDoorIndex(out int nearestDoorIndex)
    {
        var localPlayer = PlayerControl.LocalPlayer;
        var shipStatus = ShipStatus.Instance;
        var allDoors = shipStatus?.AllDoors;
        if (!localPlayer || allDoors == null)
        {
            nearestDoorIndex = -1;
            return false;
        }

        EnsureDoorCache(shipStatus);

        var playerPosition = localPlayer.GetTruePosition();
        float nearestDistanceSqr = float.MaxValue;
        nearestDoorIndex = -1;

        // 候補は ShipStatus.AllDoors をそのまま回して、一番近い有効ドアだけ拾う。
        for (int i = 0; i < allDoors.Length; i++)
        {
            var door = allDoors[i];
            float usableDistanceSqr = cachedUsableDistanceSqr[i];
            if (door == null || usableDistanceSqr <= 0f) continue;

            var doorPosition = (Vector2)door.transform.position;
            float distanceSqr = (playerPosition - doorPosition).sqrMagnitude;
            if (distanceSqr > usableDistanceSqr || distanceSqr >= nearestDistanceSqr) continue;

            nearestDoorIndex = i;
            nearestDistanceSqr = distanceSqr;
        }

        return nearestDoorIndex != -1;
    }

    private static void EnsureDoorCache(ShipStatus shipStatus)
    {
        // シーン内の ShipStatus が変わらない限り再計算しない。
        if (cachedShipStatus == shipStatus) return;

        cachedShipStatus = shipStatus;
        var allDoors = shipStatus.AllDoors;
        if (allDoors == null || allDoors.Length == 0)
        {
            cachedUsableDistanceSqr = [];
            return;
        }

        cachedUsableDistanceSqr = new float[allDoors.Length];
        for (int i = 0; i < allDoors.Length; i++)
        {
            // 毎フレーム GetComponent しないように、使用可能距離だけ前計算しておく。
            cachedUsableDistanceSqr[i] = GetUsableDistanceSqr(allDoors[i]);
        }
    }

    private static float GetUsableDistanceSqr(OpenableDoor door)
    {
        // この役職で触らせたくない特殊ドアはここで除外する。
        if (door is null or AutoCloseDoor or MushroomWallDoor) return -1f;
        // Skeld 系はコンソールなしで直接ドアを使う。
        if (door.TryCast<AutoOpenDoor>() != null) return AutoDoorUsableDistance * AutoDoorUsableDistance;

        // 通常マップは既存のコンソール距離に合わせる。
        var doorConsole = door.GetComponent<DoorConsole>();
        if (doorConsole != null) return doorConsole.UsableDistance * doorConsole.UsableDistance;

        var openDoorConsole = door.GetComponent<OpenDoorConsole>();
        return openDoorConsole != null ? openDoorConsole.UsableDistance * openDoorConsole.UsableDistance : -1f;
    }
}
