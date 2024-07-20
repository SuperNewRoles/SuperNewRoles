using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using HarmonyLib;
using Hazel;
using SuperNewRoles.Buttons;
using SuperNewRoles.CustomObject;
using SuperNewRoles.Helpers;
using SuperNewRoles.Roles.Role;
using SuperNewRoles.Roles.RoleBases;
using SuperNewRoles.Roles.RoleBases.Interfaces;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SuperNewRoles.Roles.Crewmate;

// Nebula役職 あつさんありがとうございました！
public class Ubiquitous : RoleBase, ICrewmate, ICustomButton, IMeetingHandler, IMap, IDeathHandler, IHandleChangeRole, IRpcHandler
{
    public static new RoleInfo Roleinfo = new(
        typeof(Ubiquitous),
        p => new Ubiquitous(p),
        RoleId.Ubiquitous,
        "Ubiquitous",
        new(56, 155, 223, byte.MaxValue),
        new(RoleId.Ubiquitous, TeamTag.Crewmate, RoleTag.Information, RoleTag.CustomObject),
        quoteMod: QuoteMod.NebulaOnTheShip
    );
    public static new OptionInfo Optioninfo = new(RoleId.Ubiquitous, 453000, false, optionCreator: CreateOption);
    public static new IntroInfo Introinfo = new(RoleId.Ubiquitous, introSound: RoleTypes.Engineer);

    public static CustomOption CallCoolTime;
    public static CustomOption OperationCoolTime;
    public static CustomOption IsLimitOperableTime;
    public static CustomOption OperableTime;
    public static CustomOption DroneStayTurn;
    public static CustomOption FlyingSpeed;
    public static CustomOption DoorHackCoolTime;
    public static CustomOption DoorHackScope;
    public static CustomOption DroneVisibilityRange;
    private static void CreateOption()
    {
        CallCoolTime = CustomOption.Create(Optioninfo.OptionId++, false, CustomOptionType.Crewmate, "UbiquitousCallCoolTimeOption", 10f, 0f, 60f, 2.5f, Optioninfo.RoleOption);
        OperationCoolTime = CustomOption.Create(Optioninfo.OptionId++, false, CustomOptionType.Crewmate, "UbiquitousOperationCoolTimeOption", 0f, 0f, 60f, 2.5f, Optioninfo.RoleOption);
        IsLimitOperableTime = CustomOption.Create(Optioninfo.OptionId++, false, CustomOptionType.Crewmate, "UbiquitousIsLimitOperableTimeOption", false, Optioninfo.RoleOption);
        OperableTime = CustomOption.Create(Optioninfo.OptionId++, false, CustomOptionType.Crewmate, "UbiquitousOperableTimeOption", 10f, 2.5f, 60f, 2.5f, IsLimitOperableTime);
        DroneStayTurn = CustomOption.Create(Optioninfo.OptionId++, false, CustomOptionType.Crewmate, "UbiquitousDroneStayTurnOption", 5f, 1f, 20f, 1f, Optioninfo.RoleOption);
        FlyingSpeed = CustomOption.Create(Optioninfo.OptionId++, false, CustomOptionType.Crewmate, "UbiquitousFlyingSpeedOption", 1.5f, 0.25f, 3f, 0.25f, Optioninfo.RoleOption);
        DoorHackCoolTime = CustomOption.Create(Optioninfo.OptionId++, false, CustomOptionType.Crewmate, "UbiquitousDoorHackCoolTimeOption", 30f, 2.5f, 60f, 2.5f, Optioninfo.RoleOption);
        DoorHackScope = CustomOption.Create(Optioninfo.OptionId++, false, CustomOptionType.Crewmate, "UbiquitousDoorHackScopeOption", 0.5f, 0.25f, 5f, 0.25f, Optioninfo.RoleOption);
        DroneVisibilityRange = CustomOption.Create(Optioninfo.OptionId++, false, CustomOptionType.Crewmate, "UbiquitousDroneVisibilityRangeOption", 1f, 0.25f, 5f, 0.25f, Optioninfo.RoleOption);
    }

    public CustomButtonInfo[] CustomButtonInfos { get; }
    public CustomButtonInfo OperationButton;
    public CustomButtonInfo CallAndHomeButton;
    public CustomButtonInfo DoorHackButton;
    public Drone MyDrone;
    public bool UnderOperation => MyDrone?.UnderOperation ?? false;
    private Sprite CallButtonSprite => ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.Ubiquitous.UbiquitousCallButton.png", 115f);
    private Sprite CallHomeButtonSprite => ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.Ubiquitous.UbiquitousCallHomeButton.png", 115f);
    public SpriteRenderer[] MapHerePoints;
    public Ubiquitous(PlayerControl player) : base(player, Roleinfo, Optioninfo, Introinfo)
    {
        OperationButton = new(
            null, this, OperationButtonClick, (alive) => alive, CustomButtonCouldType.Always, null,
            ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.Ubiquitous.UbiquitousOperationButton.png", 115f),
            OperationCoolTime.GetFloat, new(-2, 0), "UbiquitousOperationButton", KeyCode.F, CouldUse: () => MyDrone,
            DurationTime: OperableTime.GetFloat, IsEffectDurationInfinity: !IsLimitOperableTime.GetBool(), OnEffectEnds: OperationButtonEffectEnds
        );
        OperationButton.GetOrCreateButton().effectCancellable = true;
        CallAndHomeButton = new(
            null, this, CallAndHomeButtonClick, (alive) => alive, CustomButtonCouldType.Always, CallAndHomeButtonMeetingEnd,
            CallButtonSprite, () => MyDrone ? CallCoolTime.GetFloat() : 2.5f, new(-1, 1), "UbiquitousCallButton", KeyCode.Q, CouldUse: () => !UnderOperation
        );
        DoorHackButton = new(
            null, this, DoorHackButtonClick, (alive) => alive, CustomButtonCouldType.Always, null,
            ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.Ubiquitous.UbiquitousDoorHack.png", 115f),
            DoorHackCoolTime.GetFloat, new(-2, 1), "UbiquitousDoorHackButton", KeyCode.V, CouldUse: DoorHackButtonCould
        );
        CustomButtonInfos = new CustomButtonInfo[3] {
            OperationButton,
            CallAndHomeButton,
            DoorHackButton,
        };
        MyDrone = null;
    }

    public void OperationButtonClick()
    {
        if (!MyDrone) return;
        AmongUsUtil.SetCamTarget(!UnderOperation ? MyDrone : null);
    }

    public void OperationButtonEffectEnds() => AmongUsUtil.SetCamTarget(null);

    public void CallAndHomeButtonClick()
    {
        if (!MyDrone)
        {
            MyDrone = Drone.CreateActiveDrone($"{Player.PlayerId}", Player.transform.position, Player);
            CallAndHomeButtonChangeMode(false);
        }
        else
        {
            MyDrone.Destroy();
            MyDrone = null;
            CallAndHomeButtonChangeMode(true);
        }
    }

    public void CallAndHomeButtonMeetingEnd() => OperationButtonEffectEnds();

    public void CallAndHomeButtonChangeMode(bool is_call)
    {
        CustomButton button = CallAndHomeButton.customButton;
        button.Sprite = is_call ? CallButtonSprite : CallHomeButtonSprite;
        button.buttonText = ModTranslation.GetString(is_call ? "UbiquitousCallButton" : "UbiquitousCallHomeButton");
    }

    public void DoorHackButtonClick()
    {
        OpenableDoor[] doors = ShipStatus.Instance.AllDoors.ToArray();
        foreach (OpenableDoor door in doors)
        {
            if (door.IsOpen) continue;
            if (door.TryCast<AutoCloseDoor>()) continue;
            if (Vector2.Distance(MyDrone.transform.position, door.transform.position) > DoorHackScope.GetFloat() * 3) continue;
            if (door.TryCast<AutoOpenDoor>())
            {
                doors.AllRun(x => { if (door.Room == x.Room) x.RpcSetDoorway(true); });
                continue;
            }
            door.RpcSetDoorway(true);
        }
    }

    public bool DoorHackButtonCould()
    {
        if (!MyDrone) return false;
        return ShipStatus.Instance.AllDoors.Any(x => Vector2.Distance(MyDrone.transform.position, x.transform.position) <= DoorHackScope.GetFloat() * 3 && !x.IsOpen && !x.TryCast<AutoCloseDoor>());
    }

    public void StartMeeting()
    {
        if (!Player.AmOwner || !MyDrone) return;
        MessageWriter writer = RpcWriter;
        writer.Write(MyDrone.transform.position.x);
        writer.Write(MyDrone.transform.position.y);
        writer.EndRPC();
        MyDrone.IsActive = false;
        MyDrone = null;
    }

    public void CloseMeeting() { }

    public void AwakePostfix(MapBehaviour __instance)
    {
        MapHerePoints = new SpriteRenderer[PlayerControl.AllPlayerControls.Count - 1];
        for (int i = 0; i < MapHerePoints.Length; i++)
        {
            MapHerePoints[i] = Object.Instantiate(__instance.HerePoint, __instance.HerePoint.transform.parent);
            MapHerePoints[i].gameObject.SetActive(false);
        }
    }

    public void FixedUpdatePostfix(MapBehaviour __instance)
    {
        List<PlayerControl> drone_player = Drone.GetPlayersVicinity(Player);
        for (int i = 0; i < MapHerePoints.Length; i++)
        {
            SpriteRenderer renderer = MapHerePoints[i];
            if (drone_player.Count > i)
            {
                PlayerControl player = drone_player[i];
                player.SetPlayerMaterialColors(renderer);
                Vector3 pos = player.transform.position;
                pos /= ShipStatus.Instance.MapScale;
                pos.x *= Mathf.Sign(ShipStatus.Instance.transform.localScale.x);
                pos.z = -1f;
                renderer.transform.localPosition = pos;
                renderer.gameObject.SetActive(true);
            }
            else renderer.gameObject.SetActive(false);
        }
    }

    public void OnAmDeath(DeathInfo deathInfo)
    {
        if (!Player.AmOwner) return;
        OperationButtonEffectEnds();
        MyDrone?.Destroy();
    }

    public void OnChangeRole()
    {
        if (!Player.AmOwner) return;
        OperationButtonEffectEnds();
        MyDrone?.Destroy();
    }

    public void RpcReader(MessageReader reader) => Drone.CreateIdleDrone($"Idle {Player.PlayerId}", new(reader.ReadSingle(), reader.ReadSingle()), Player);

    [HarmonyPatch(typeof(PlayerControl))]
    public static class PlayerControlPatch
    {
        [HarmonyPatch(nameof(PlayerControl.CanMove), MethodType.Getter), HarmonyPostfix]
        public static void CanMoveGetterPostfix(PlayerControl __instance, ref bool __result)
        {
            if (!__result) return;
            if (!__instance.TryGetRoleBase(out Ubiquitous ubiquitous)) return;
            __result = !ubiquitous.UnderOperation;
        }
    }
}
