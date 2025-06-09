using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Roles.Ability;
using SuperNewRoles.CustomObject;
using SuperNewRoles.Roles.Ability.CustomButton;
using SuperNewRoles.Modules;
using SuperNewRoles.Events;
using SuperNewRoles.Events.PCEvents;
using SuperNewRoles.Modules.Events.Bases;
using Hazel;
using HarmonyLib;
using Object = UnityEngine.Object;
using SuperNewRoles.CustomCosmetics;

namespace SuperNewRoles.Roles.Crewmate;

// むっちゃあつさん
class Ubiquitous : RoleBase<Ubiquitous>
{
    public override RoleId Role => RoleId.Ubiquitous;
    public override Color32 RoleColor => new(56, 155, 223, byte.MaxValue);
    public override List<System.Func<AbilityBase>> Abilities => new() { () => new UbiquitousAbility() };
    public override QuoteMod QuoteMod => QuoteMod.NebulaOnTheShip;

    public override short IntroNum => 1;

    public override AssignedTeamType AssignedTeam => AssignedTeamType.Crewmate;
    public override WinnerTeamType WinnerTeam => WinnerTeamType.Crewmate;
    public override TeamTag TeamTag => TeamTag.Crewmate;
    public override RoleTag[] RoleTags => new[] { RoleTag.Information, RoleTag.CustomObject };
    public override RoleOptionMenuType OptionTeam => RoleOptionMenuType.Crewmate;

    [CustomOptionFloat("Ubiquitous.CallCoolTime", 0f, 60f, 2.5f, 10f)]
    public static float CallCoolTime;
    [CustomOptionFloat("Ubiquitous.OperationCoolTime", 0f, 60f, 2.5f, 0f)]
    public static float OperationCoolTime;
    [CustomOptionBool("Ubiquitous.IsLimitOperableTime", false)]
    public static bool IsLimitOperableTime;
    [CustomOptionFloat("Ubiquitous.OperableTime", 2.5f, 60f, 2.5f, 10f, parentFieldName: nameof(IsLimitOperableTime))]
    public static float OperableTime;
    [CustomOptionFloat("Ubiquitous.DroneStayTurn", 1f, 20f, 1f, 5f)]
    public static float DroneStayTurn;
    [CustomOptionFloat("Ubiquitous.FlyingSpeed", 0.25f, 3f, 0.25f, 1.5f)]
    public static float FlyingSpeed;
    [CustomOptionFloat("Ubiquitous.DoorHackCoolTime", 2.5f, 60f, 2.5f, 30f)]
    public static float DoorHackCoolTime;
    [CustomOptionFloat("Ubiquitous.DoorHackScope", 0.25f, 5f, 0.25f, 0.5f)]
    public static float DoorHackScope;
    [CustomOptionFloat("Ubiquitous.DroneVisibilityRange", 0.25f, 5f, 0.25f, 1f)]
    public static float DroneVisibilityRange;
    [CustomOptionBool("Ubiquitous.MapShowPlayerColor", false)]
    public static bool MapShowPlayerColor;
}

class UbiquitousAbility : AbilityBase
{
    private CallAndHomeButton _callAndHomeButton;
    private OperationButton _operationButton;
    private DoorHackButton _doorHackButton;
    public Drone MyDrone;
    public bool UnderOperation => MyDrone?.UnderOperation ?? false;
    public SpriteRenderer[] MapHerePoints;

    private EventListener<MeetingStartEventData> _meetingStartListener;
    private EventListener<MeetingCloseEventData> _meetingCloseListener;

    private EventListener<MapBehaviourAwakePostfixEventData> _mapBehaviourAwakeListener;
    private EventListener<MapBehaviourFixedUpdatePostfixEventData> _mapBehaviourFixedUpdateListener;

    public UbiquitousAbility()
    {
    }

    public override void AttachToAlls()
    {
        base.AttachToAlls();

        Drone.DroneStayTurn = Ubiquitous.DroneStayTurn;
        Drone.FlyingSpeed = Ubiquitous.FlyingSpeed;
        Drone.DroneVisibilityRange = Ubiquitous.DroneVisibilityRange;

        _callAndHomeButton = new CallAndHomeButton(this);
        _operationButton = new OperationButton(this);
        _doorHackButton = new DoorHackButton(this);
        Player.AttachAbility(_callAndHomeButton, new AbilityParentAbility(this));
        Player.AttachAbility(_operationButton, new AbilityParentAbility(this));
        Player.AttachAbility(_doorHackButton, new AbilityParentAbility(this));
    }

    public override void AttachToLocalPlayer()
    {
        base.AttachToLocalPlayer();
        _meetingStartListener = MeetingStartEvent.Instance.AddListener(OnMeetingStart);
        _meetingCloseListener = MeetingCloseEvent.Instance.AddListener(OnMeetingClose);
        _mapBehaviourAwakeListener = MapBehaviourAwakePostfixEvent.Instance.AddListener(OnMapAwake);
        _mapBehaviourFixedUpdateListener = MapBehaviourFixedUpdatePostfixEvent.Instance.AddListener(OnMapFixedUpdate);
    }

    public override void DetachToLocalPlayer()
    {
        base.DetachToLocalPlayer();
        _meetingStartListener?.RemoveListener();
        _meetingCloseListener?.RemoveListener();
        _mapBehaviourAwakeListener?.RemoveListener();
        _mapBehaviourFixedUpdateListener?.RemoveListener();
    }

    private void OnMeetingStart(MeetingStartEventData data)
    {
        if (!Player.AmOwner || !MyDrone) return;
        UbiquitousRPC.RpcSyncDronePosition(MyDrone.transform.position.x, MyDrone.transform.position.y);
        MyDrone.IsActive = false;
        MyDrone = null;
        Camera.main.GetComponent<FollowerCamera>().SetTarget(ExPlayerControl.LocalPlayer.Player);
    }

    private void OnMeetingClose(MeetingCloseEventData data)
    {
        if (_operationButton != null)
        {
            _operationButton.OnEffectEnds?.Invoke();
        }
        if (_callAndHomeButton != null)
        {
            _callAndHomeButton._isCallMode = true;
        }
    }

    public void OnDeath()
    {
        if (!Player.AmOwner) return;
        _operationButton?.OnEffectEnds?.Invoke();
        MyDrone?.Destroy();
    }

    public void OnMapAwake(MapBehaviourAwakePostfixEventData data)
    {
        if (!Player.AmOwner) return;
        MapHerePoints = new SpriteRenderer[ExPlayerControl.ExPlayerControls.Count - 1];
        for (int i = 0; i < MapHerePoints.Length; i++)
        {
            MapHerePoints[i] = Object.Instantiate(data.__instance.HerePoint, data.__instance.HerePoint.transform.parent);
            MapHerePoints[i].gameObject.SetActive(false);
        }
    }

    public void OnMapFixedUpdate(MapBehaviourFixedUpdatePostfixEventData data)
    {
        if (!Player.AmOwner || MapHerePoints == null) return;
        List<ExPlayerControl> dronePlayer = Drone.GetPlayersVicinity(Player);
        for (int i = 0; i < MapHerePoints.Length; i++)
        {
            SpriteRenderer renderer = MapHerePoints[i];
            if (dronePlayer.Count > i)
            {
                ExPlayerControl player = dronePlayer[i];
                if (Ubiquitous.MapShowPlayerColor) player.Player.SetPlayerMaterialColors(renderer);
                else PlayerMaterial.SetColors(CustomColors.IsLighter(player) ? 7 : 6, renderer);
                Vector3 pos = player.GetTruePosition();
                pos /= ShipStatus.Instance.MapScale;
                pos.x *= Mathf.Sign(ShipStatus.Instance.transform.localScale.x);
                pos.z = -1f;
                renderer.transform.localPosition = pos;
                renderer.gameObject.SetActive(true);
            }
            else renderer.gameObject.SetActive(false);
        }
    }
}

class CallAndHomeButton : CustomButtonBase
{
    private readonly UbiquitousAbility _ability;
    public bool _isCallMode = true;

    public CallAndHomeButton(UbiquitousAbility ability)
    {
        _ability = ability;
    }

    public override float DefaultTimer => _ability.MyDrone ? Ubiquitous.CallCoolTime : 2.5f;
    public override string buttonText => ModTranslation.GetString(_isCallMode ? "UbiquitousCallButton" : "UbiquitousCallHomeButton");
    public override Sprite Sprite => AssetManager.GetAsset<Sprite>(_isCallMode ? "UbiquitousCallButton.png" : "UbiquitousCallHomeButton.png");
    protected override KeyType keytype => KeyType.Ability1;

    public override bool CheckIsAvailable() => !_ability.UnderOperation;

    public override void OnClick()
    {
        if (!_ability.MyDrone)
        {
            _ability.MyDrone = Drone.CreateActiveDrone($"{Player.PlayerId}", Player.transform.position, Player);
            _isCallMode = false;
        }
        else
        {
            _ability.MyDrone.Destroy();
            _ability.MyDrone = null;
            _isCallMode = true;
        }
    }
}

class OperationButton : CustomButtonBase, IButtonEffect
{
    private readonly UbiquitousAbility _ability;
    public OperationButton(UbiquitousAbility ability) { _ability = ability; }

    public override float DefaultTimer => Ubiquitous.OperationCoolTime;
    public override string buttonText => ModTranslation.GetString("UbiquitousOperationButton");
    public override Sprite Sprite => AssetManager.GetAsset<Sprite>("UbiquitousOperationButton.png");
    protected override KeyType keytype => KeyType.Ability2;

    public override bool CheckIsAvailable() => _ability.MyDrone != null;

    public override void OnClick()
    {
        if (_ability.MyDrone != null)
        {
            Camera.main.GetComponent<FollowerCamera>().SetTarget(_ability.MyDrone);
            ExPlayerControl.LocalPlayer.MyPhysics.body.velocity = Vector2.zero;
        }
    }

    public Action OnEffectEnds { get; set; } = () =>
    {
        if (ExPlayerControl.LocalPlayer.AmOwner)
        {
            Camera.main.GetComponent<FollowerCamera>().SetTarget(ExPlayerControl.LocalPlayer.Player);
        }
    };
    public float EffectDuration => Ubiquitous.OperableTime;
    public bool isEffectActive { get; set; }
    public float EffectTimer { get; set; }
    public bool IsEffectDurationInfinity => !Ubiquitous.IsLimitOperableTime;
    public bool effectCancellable => true;
    public bool doAdditionalEffect => Ubiquitous.IsLimitOperableTime;
}

class DoorHackButton : CustomButtonBase
{
    private readonly UbiquitousAbility _ability;

    public DoorHackButton(UbiquitousAbility ability) { _ability = ability; }

    public override float DefaultTimer => Ubiquitous.DoorHackCoolTime;
    public override string buttonText => ModTranslation.GetString("UbiquitousDoorHackButton");
    public override Sprite Sprite => AssetManager.GetAsset<Sprite>("UbiquitousDoorHack.png");
    protected override KeyType keytype => KeyType.Vent;

    public override bool CheckIsAvailable()
    {
        if (!_ability.MyDrone) return false;
        return ShipStatus.Instance.AllDoors.Any(x => Vector2.Distance(_ability.MyDrone.transform.position, x.transform.position) <= Ubiquitous.DoorHackScope * 3 && !x.IsOpen && !x.TryCast<AutoCloseDoor>());
    }

    public override void OnClick()
    {
        foreach (OpenableDoor door in ShipStatus.Instance.AllDoors)
        {
            if (door.IsOpen) continue;
            if (door.TryCast<AutoCloseDoor>()) continue;
            if (Vector2.Distance(_ability.MyDrone.transform.position, door.transform.position) > Ubiquitous.DoorHackScope * 3) continue;
            if (door.TryCast<AutoOpenDoor>())
            {
                foreach (var d in ShipStatus.Instance.AllDoors)
                {
                    if (door.Room == d.Room) d.SetDoorway(true);
                }
                continue;
            }
            door.SetDoorway(true);
        }
    }
}
public static class UbiquitousRPC
{
    [CustomRPC]
    public static void RpcSyncDronePosition(float x, float y)
    {
        Drone.CreateIdleDrone($"Idle {ExPlayerControl.LocalPlayer.PlayerId}", new(x, y), ExPlayerControl.LocalPlayer);
    }
}

[HarmonyPatch(typeof(PlayerControl))]
public static class UbiquitousPlayerControlPatch
{
    [HarmonyPatch(nameof(PlayerControl.CanMove), MethodType.Getter), HarmonyPostfix]
    public static void CanMoveGetterPostfix(PlayerControl __instance, ref bool __result)
    {
        if (AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started) return;
        if (HudManager.Instance.IsIntroDisplayed) return;
        if (!__result) return;
        var exPlayer = (ExPlayerControl)__instance;
        if (!exPlayer.TryGetAbility<UbiquitousAbility>(out var ubiquitousAbility)) return;
        __result = !ubiquitousAbility.UnderOperation;
    }
}