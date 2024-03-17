/**

DimensionWalker(役職)で壁貫通対策で使う予定だったものの、そもそも壁貫通が発生しなかったので没

**/
/*using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace SuperNewRoles.CustomObject;

[HarmonyPatch]
class WallDetector : MonoBehaviour
{
    public static List<WallDetector> AllWallDetectors = new();
    public static WallDetector LocalWallDetector { get; private set; }
    public BoxCollider2D Collider { get; }
    public bool CollisionNow { get; private set; }

    public WallDetector()
    {
        Collider = this.gameObject.AddComponent<BoxCollider2D>();
        Collider.size = new(0.5f, 0.3f);
        Collider.isTrigger = true;
        CollisionNow = false;
    }

    void OnTriggerEnter2D(Collision collision)
    {
        Logger.Info("コライダーに触れた!");
        SoundManager.Instance.PlaySound(ShipStatus.Instance.VentEnterSound, false, 0.8f);
        // レイヤーがShip未満であればreturnする
        if (collision.gameObject.layer < 9)
            return;

        Logger.Info("Shipレイヤーのコライダーに触れた!");

        CollisionNow = true;
    }

    void OnTriggerExit2D(Collision collision)
    {
        Logger.Info("コライダーから離れた!");
        SoundManager.Instance.PlaySound(ShipStatus.Instance.VentExitSound, false, 0.8f);
        // レイヤーがShip未満であればreturnする
        if (collision.gameObject.layer < 9)
            return;

        Logger.Info("Shipレイヤーのコライダーから離れた!");

        CollisionNow = false;
    }

    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.StartGame)), HarmonyPostfix]
    static void SpawnDetector()
    {
        AllWallDetectors = new();

        new LateTask(() => {
            var detector = new GameObject("WallDetector").AddComponent<WallDetector>();
            detector.gameObject.transform.SetParent(PlayerControl.LocalPlayer.transform, false);
            detector.gameObject.transform.localPosition = new(0f, -0.27f);

            AllWallDetectors.Add(detector);
            LocalWallDetector = detector;
        }, 1f);
    }
}*/