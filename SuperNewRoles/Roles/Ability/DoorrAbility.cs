using System.Linq;
using UnityEngine;
using SuperNewRoles.Roles.Ability.CustomButton;
using SuperNewRoles.Modules;

namespace SuperNewRoles.Roles.Ability;

public class DoorrAbility : CustomButtonBase
{
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
        return GetDoor() != null && PlayerControl.LocalPlayer.CanMove;
    }

    public override void OnClick()
    {
        var door = GetDoor();
        if (door != null)
        {
            RpcSetDoorway(door.Id, !door.IsOpen);
            ResetTimer();
        }
    }

    [CustomRPC]
    public void RpcSetDoorway(int doorId, bool isOpen)
    {
        var door = ShipStatus.Instance.AllDoors.FirstOrDefault(x => x.Id == doorId);
        if (door == null) return;
        door.SetDoorway(isOpen);
    }

    private static OpenableDoor GetDoor()
    {
        return GameObject.FindObjectsOfType<DoorConsole>().FirstOrDefault(x =>
        {
            if (x.MyDoor == null) return false;
            float num = Vector2.Distance(PlayerControl.LocalPlayer.GetTruePosition(), x.transform.position);
            return num <= x.UsableDistance;
        })?.MyDoor;
    }
}