using System.Collections.Generic;
using HarmonyLib;
using SuperNewRoles.Roles.Crewmate;
using UnityEngine;

namespace SuperNewRoles.CustomObject;

public class Drone : MonoBehaviour
{
    public static List<Drone> AllDrone;
    public static PlayerData<List<Drone>> PlayerDrone;
    public static GameObject AllDroneObject;
    public static GameObject InstantiateDrone;
    public static Sprite[] Active = new Sprite[2]
    {
        ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.Ubiquitous.UbiquitousDroneActiveAnim1.png", 500f),
        ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.Ubiquitous.UbiquitousDroneActiveAnim2.png", 500f)
    };
    public static Sprite Idle => ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.Ubiquitous.UbiquitousDroneIdle.png", 500f);
    public static void ClearAndReload()
    {
        AllDrone = new();
        PlayerDrone = new(true);
    }

    public GameObject DroneObject;
    public GameObject RendererObject;
    public GameObject LightChild;
    public Rigidbody2D Body;
    public SpriteRenderer Renderer;
    public PlayerControl Owner;
    public bool IsActive;
    public int SpriteNumber;
    public float SpriteTimer;
    public float HoveringTimer;
    public int RemainingTurn;
    public bool UnderOperation => DestroyableSingleton<HudManager>.Instance.PlayerCam.Target == this;

    public void Start()
    {
        DroneObject = gameObject;
        RendererObject = transform.Find("Renderer").gameObject;
        LightChild = transform.Find("LightChild").gameObject;
        Body = DroneObject.GetComponent<Rigidbody2D>() ?? DroneObject.AddComponent<Rigidbody2D>();
        Renderer = RendererObject.GetComponent<SpriteRenderer>() ?? RendererObject.AddComponent<SpriteRenderer>();
        SpriteNumber = 0;
        SpriteTimer = 0f;
        HoveringTimer = 0f;
        RemainingTurn = Ubiquitous.DroneStayTurn.GetInt();
    }

    public void Update()
    {
        if (IsActive)
        {
            Renderer.transform.localPosition = new(0f, Mathf.Sin(HoveringTimer) / 20f);
            HoveringTimer += Time.fixedDeltaTime;
            if (HoveringTimer > Mathf.PI * 2) HoveringTimer -= Mathf.PI * 2;

            SpriteTimer += Time.fixedDeltaTime;
            if (SpriteTimer > 0.25f)
            {
                SpriteTimer -= 0.25f;
                SpriteNumber = (SpriteNumber + 1) % 2;
                Renderer.sprite = Active[SpriteNumber];
            }

            if (Body.velocity.x > 0.1f) Renderer.flipX = true;
            else if (Body.velocity.x < -0.1f) Renderer.flipX = false;

            Renderer.color = UnderOperation ? Color.white : new(1f, 1f, 1f, 0.5f);

            float size = UnderOperation ? ShipStatus.Instance.MaxLightRadius * Ubiquitous.DroneVisibilityRange.GetFloat() * 5.25f : 0f;
            LightChild.transform.localScale = new(size, size, 1f);
        }
        else
        {
            Renderer.sprite = Idle;
            Renderer.color = Color.white;
            Renderer.transform.localPosition = Vector3.zero;
            if (LightChild) Destroy(LightChild);
        }
    }

    public void FixedUpdate()
    {
        Vector3 position = transform.position;
        if (IsActive)
        {
            if (UnderOperation) Body.velocity = DestroyableSingleton<HudManager>.Instance.joystick.DeltaL * Ubiquitous.FlyingSpeed.GetFloat() * Owner.MyPhysics.Speed;
            else Body.velocity = Vector2.zero;
            position.z = -1f;
        }
        else position.z = position.y / 1000f;
        transform.position = position;
    }

    public void Destroy()
    {
        AllDrone.Remove(this);
        if (PlayerDrone.TryGetValue(Owner, out List<Drone> drones))
            drones.Remove(this);
        Destroy(gameObject);
    }

    public static Drone CreateActiveDrone(string id, Vector2 pos, PlayerControl owner)
    {
        GameObject drone_object = Instantiate(InstantiateDrone, AllDroneObject.transform);
        drone_object.name = $"Drone {id}";
        drone_object.transform.position = pos;
        drone_object.SetActive(true);
        Drone drone = drone_object.AddComponent<Drone>();
        drone.IsActive = true;
        drone.Owner = owner;
        AllDrone.Add(drone);
        if (!PlayerDrone.Contains(owner))
            PlayerDrone[owner] = new();
        PlayerDrone[owner].Add(drone);
        return drone;
    }

    public static Drone CreateIdleDrone(string id, Vector2 pos, PlayerControl owner)
    {
        Drone drone = CreateActiveDrone(id, pos, owner);
        drone.IsActive = false;
        return drone;
    }

    public static void CloseMeeting()
    {
        foreach (Drone drone in AllDrone.AsSpan())
        {
            if (drone.IsActive) continue;
            if (drone.RemainingTurn-- > 0) return;
            AllDrone.Remove(drone);
            drone.Destroy();
        }
    }

    public static List<PlayerControl> GetPlayersVicinity(PlayerControl owner)
    {
        List<PlayerControl> vicinity = new();
        if (!PlayerDrone.Contains(owner)) return vicinity;
        List<Drone> idle = PlayerDrone[owner].FindAll(x => !x.IsActive);
        foreach (PlayerControl player in CachedPlayer.AllPlayers.AsSpan())
        {
            if (player.PlayerId == owner.PlayerId) continue;
            if (idle.Any(x => Vector2.Distance(x.transform.position, player.GetTruePosition()) <= ShipStatus.Instance.MaxLightRadius * Ubiquitous.DroneVisibilityRange.GetFloat()))
                vicinity.Add(player);
        }
        return vicinity;
    }

    [HarmonyPatch(typeof(ShipStatus))]
    public static class ShipStatusPatch
    {
        [HarmonyPatch(nameof(ShipStatus.Start)), HarmonyPostfix]
        public static void StartPostfix()
        {
            AllDroneObject = new("AllDroneObject");
            AllDroneObject.transform.position = new(0f, 0f, 0f);

            InstantiateDrone = new("Instantiate Drone") { layer = LayerExpansion.GetPlayersLayer() };
            InstantiateDrone.transform.SetParent(AllDroneObject.transform);
            InstantiateDrone.SetActive(false);
            Rigidbody2D body = InstantiateDrone.AddComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            body.freezeRotation = true;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            body.sleepMode = RigidbodySleepMode2D.NeverSleep;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            CircleCollider2D collider = InstantiateDrone.AddComponent<CircleCollider2D>();
            collider.offset = new(0f, -0.2f);
            collider.radius = 0.2234f;
            collider.isTrigger = false;

            GameObject drone_renderer = new("Renderer") { layer = LayerExpansion.GetObjectsLayer() };
            drone_renderer.transform.SetParent(InstantiateDrone.transform);
            drone_renderer.transform.localPosition = new();
            SpriteRenderer renderer = drone_renderer.AddComponent<SpriteRenderer>();
            renderer.color = new Color(1f, 1f, 1f, 0.5f);
            renderer.sprite = Active[0];

            GameObject light_child = new("LightChild") { layer = LayerExpansion.GetShadowLayer() };
            light_child.transform.SetParent(InstantiateDrone.transform);
            light_child.transform.localPosition = new();
            light_child.transform.localScale = Vector3.zero;
            LightSource source = PlayerControl.LocalPlayer.LightPrefab;
            light_child.AddComponent<MeshFilter>().mesh = source.lightChildMesh;
            light_child.AddComponent<MeshRenderer>().material.shader = source.LightCutawayMaterial.shader;
        }
    }
}
