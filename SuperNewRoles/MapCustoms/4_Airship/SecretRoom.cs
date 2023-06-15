using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using HarmonyLib;
using Hazel;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using SuperNewRoles.Helpers;
using SuperNewRoles.Roles;
using TMPro;
using UnityEngine;
using static SuperNewRoles.MapCustoms.MapCustom;

namespace SuperNewRoles.MapCustoms.Airship;

public static class SecretRoom
{
    public static PlayerControl leftplayer;
    public static PlayerControl rightplayer;

    static PoolablePlayer left;
    static PoolablePlayer right;
    public static PlayerControl UsePlayer;
    public static bool IsWait = false;
    public static TextMeshPro LowerInfoText;
    static readonly List<Vector2> TeleportPositions = new()
        {
            new Vector2(-0.78f, -1f), new Vector2(-13, -1), new Vector2(-13, 1.5f), new Vector2(-21, -1.2f), new Vector2(-10, -7), new Vector2(-6.2f, -11),
            new Vector2(-13.4f, -12.2f), new Vector2(2.2f, -12), new Vector2(7.2f, -11.4f), new Vector2(16.3f, -8.6f), new Vector2(24.9f, -5.7f), new Vector2(33.6f, -0.6f), new Vector2(31.5f, 5.6f),
            new Vector2(20f, 10.1f), new Vector2(12.6f, 9.1f), new Vector2(19.5f, 0.1f), new Vector2(11.1f, 0), new Vector2(-0.7f, 8.6f), new Vector2(-9f, 12.7f), new Vector2(4.1f, 8.7f), new Vector2(11, 16)
        };

    public enum Status
    {
        UseConsole,
        CloseConsole,
        Join,
        Break,
        Wait,
        Teleport
    }

    public static void SetSecretRoomTeleportStatus(Status status, byte data1, byte data2)
    {
        switch (status)
        {
            case Status.UseConsole:
                PlayerControl useplayer = ModHelpers.PlayerById(data1);
                if (UsePlayer != null)
                {
                    if (data1 == CachedPlayer.LocalPlayer.PlayerId)
                    {
                        VitalsMinigame minigame = GameObject.FindObjectOfType<VitalsMinigame>();
                        if (minigame != null && minigame.name == "secretroom_teleport-console")
                        {
                            minigame.Close();
                        }
                    }
                    return;
                }
                if (useplayer == null) return;
                UsePlayer = useplayer;
                break;
            case Status.CloseConsole:
                UsePlayer = null;
                IsWait = false;
                if ((leftplayer != null && leftplayer.PlayerId == CachedPlayer.LocalPlayer.PlayerId) ||
                    (rightplayer != null && rightplayer.PlayerId == CachedPlayer.LocalPlayer.PlayerId))
                {
                    LowerInfoText.text = ModTranslation.GetString("ExitExperimentEsc"); // Escで実験から抜ける
                }
                break;
            case Status.Join:
                PlayerControl player = ModHelpers.PlayerById(data1);
                if (player == null) return;
                if (data2 == 0)
                {
                    leftplayer = player;
                }
                else
                {
                    rightplayer = player;
                }
                break;
            case Status.Break:
                if (data1 == 0)
                {
                    leftplayer = null;
                }
                else
                {
                    rightplayer = null;
                }
                break;
            case Status.Wait:
                if ((leftplayer != null && leftplayer.PlayerId == CachedPlayer.LocalPlayer.PlayerId) ||
                    (rightplayer != null && rightplayer.PlayerId == CachedPlayer.LocalPlayer.PlayerId))
                {
                    PlayerControl.LocalPlayer.moveable = false;
                    LowerInfoText.text = ModTranslation.GetString("Experimenting"); // 実験中...
                }
                IsWait = true;
                break;
            case Status.Teleport:
                if ((leftplayer != null && leftplayer.PlayerId == CachedPlayer.LocalPlayer.PlayerId) ||
                    (rightplayer != null && rightplayer.PlayerId == CachedPlayer.LocalPlayer.PlayerId))
                {
                    PlayerControl.LocalPlayer.moveable = true;
                    PlayerControl.LocalPlayer.transform.position = ModHelpers.GetRandom(TeleportPositions);
                }
                IsWait = false;
                rightplayer = null;
                leftplayer = null;
                UsePlayer = null;
                break;
        }
    }
    public static void Reset()
    {
        IsWait = false;
        leftplayer = null;
        rightplayer = null;
        UsePlayer = null;
        if (LowerInfoText != null) LowerInfoText.text = "";
    }
    public static void ShipStatusAwake(ShipStatus __instance)
    {
        if (GameManager.Instance.LogicOptions.currentGameOptions.MapId != (int)MapNames.Airship) return;
        if (__instance.Type == ShipStatus.MapType.Ship && MapCustomOption.GetBool() && AirshipSetting.GetBool() && SecretRoomOption.GetBool())
        {
            Transform room = __instance.transform.FindChild("HallwayPortrait");
            Transform Walls = room.FindChild("Walls");
            Transform Shadows = room.FindChild("Shadows");
            EdgeCollider2D collider = Walls.GetComponentsInChildren<EdgeCollider2D>()[1];
            EdgeCollider2D newcollider = Walls.gameObject.AddComponent<EdgeCollider2D>();
            EdgeCollider2D newcollider2 = Walls.gameObject.AddComponent<EdgeCollider2D>();
            EdgeCollider2D newdoorcollider = Walls.gameObject.AddComponent<EdgeCollider2D>();
            Vector2[] OldPoints = collider.points;
            List<Vector2> points1 = new();
            List<Vector2> points2 = new();
            List<Vector2> points3 = new();
            points1 = collider.points.ToArray()[..3].ToList();
            points1.Add(new Vector2(1.85f, -0.0783f));
            points1.Add(new Vector2(1.85f, 6f));
            points1.Add(new Vector2(-1.95f, 6f));
            points1.Add(new Vector2(-1.95f, 9.37f));
            points1.Add(new Vector2(7.05f, 9.37f));

            points2.Add(new Vector2(7.05f, 9.37f));
            points2.Add(new Vector2(7.05f, 6f));
            points2.Add(new Vector2(3.05f, 6f));
            points2.Add(new Vector2(3.05f, -0.0783f));
            points2.Add(new Vector2(5.3f, -0.0783f));
            points2.Add(new Vector2(5.3f, -0.2f));

            points3.Add(new Vector2(8f, 7.875f));
            points3.Add(new Vector2(4.25f, 7.875f));
            points3.Add(new Vector2(4.25f, 7f));
            points3.Add(new Vector2(8f, 7f));
            points3.Add(points3[0]);

            collider.points = points1.ToArray();
            newcollider.points = points2.ToArray();
            newcollider2.points = points3.ToArray();
            //newdoorcollider.points = new Vector2[] { new Vector2(1.85f, -0.0783f), new Vector2(3.05f, -0.0783f) };

            EdgeCollider2D shadow = Shadows.GetComponentsInChildren<EdgeCollider2D>()[0];
            EdgeCollider2D newshadow = GameObject.Instantiate(shadow, Shadows);
            List<Vector2> shadow_new = shadow.points.ToArray()[..14].ToList();
            shadow_new.Add(new Vector2(1.64f, 0.8122f));
            shadow_new.Add(new Vector2(1.64f, 6f));
            shadow_new.Add(new Vector2(-2f, 6f));
            shadow_new.Add(new Vector2(-2f, 10.25f));
            shadow_new.Add(new Vector2(7.05f, 10.25f));
            shadow_new.Add(new Vector2(7.05f, 6f));
            shadow_new.Add(new Vector2(3.15f, 6f));

            List<Vector2> newshadow_new = new()
                {
                    new Vector2(3.15f, 6f),
                    new Vector2(3.15f, 0.8332f)
                };
            newshadow_new.AddRange(shadow.points.ToArray()[16..].ToList());
            newshadow.points = newshadow_new.ToArray();
            shadow.points = shadow_new.ToArray();

            Transform entranse = GameObject.Instantiate(__instance.transform.FindChild("Cockpit/cockpit_chair"), room);
            entranse.GetComponent<SpriteRenderer>().sprite = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.SecretRoom_entrance.png", 115f);
            entranse.localPosition = new Vector3(2.45f, 1.23f, -0.0007f);
            entranse.localScale = new Vector3(1.5f, 1.5f, 1.5f);
            entranse.name = "secretroom_entranse";

            Transform Aisle = GameObject.Instantiate(entranse, room);
            Aisle.GetComponent<SpriteRenderer>().sprite = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.SecretRoom_Aisle.png", 115f);
            Aisle.localPosition = new Vector3(2.45f, 4.35f, -0.1f);
            Aisle.localScale = new Vector3(1.5f, 200f, 1.5f);
            Aisle.name = "secretroom_aisle";

            Transform Room = GameObject.Instantiate(entranse, room);
            Room.GetComponent<SpriteRenderer>().sprite = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.SecretRoom_Room.png", 115f);
            Room.localPosition = new Vector3(2.5326f, 7.9f, -0.09f);
            Room.localScale = new Vector3(1.44f, 1.44f, 1.44f);
            Room.name = "secretroom_room";

            Transform Grass = GameObject.Instantiate(entranse, room);
            Grass.GetComponent<SpriteRenderer>().sprite = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.SecretRoom_Grass.png", 115f);
            Grass.gameObject.AddComponent<EdgeCollider2D>().points = new Vector2[] {
                    new Vector2(-0.2f, -0.4f), new Vector2(0.175f, -0.4f), new Vector2(0.175f, -0.5f), new Vector2(-0.2f, -0.5f)
                };
            Grass.localPosition = new Vector3(1.57f, 6.78f, -8f);
            Grass.localScale = new Vector3(1.57f, 1.57f, 1.57f);
            Grass.name = "secretroom_grass";

            Transform Grass2 = GameObject.Instantiate(Grass, room);
            Grass2.localPosition = new Vector3(-1.67f, 6.9f, -8f);
            Grass2.localScale = new Vector3(1.57f, 1.57f, 1.57f);
            Grass2.name = "secretroom_grass2";

            Transform Grass3 = GameObject.Instantiate(Grass, room);
            Grass3.localPosition = new Vector3(3.3f, 6.9f, -8f);
            Grass3.localScale = new Vector3(1.57f, 1.57f, 1.57f);
            Grass3.name = "secretroom_grass3";

            Transform Grass4 = GameObject.Instantiate(Grass, room);
            Grass4.localPosition = new Vector3(6.78f, 6.9f, -8f);
            Grass4.localScale = new Vector3(1.57f, 1.57f, 1.57f);
            Grass4.name = "secretroom_grass4";

            Transform Dustbin = GameObject.Instantiate(Grass, room);
            Dustbin.GetComponent<SpriteRenderer>().sprite = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.SecretRoom_Dustbin.png", 115f);
            Dustbin.localPosition = new Vector3(-0.9f, 6.8f, -8f);
            Dustbin.localScale = new Vector3(1.4f, 1.4f, 1.4f);
            Dustbin.name = "secretroom_dustbin";

            Transform Teleport_on = GameObject.Instantiate(Grass, room);
            Teleport_on.GetComponent<SpriteRenderer>().sprite = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.SecretRoom_Teleport_on.png", 115f);
            Teleport_on.localPosition = new Vector3(5.7f, 8.92f, -0.1f);
            Teleport_on.localScale = new Vector3(1.5f, 1.5f, 1.5f);
            Teleport_on.name = "secretroom_teleport-on";
            GameObject.Destroy(Teleport_on.GetComponent<EdgeCollider2D>());

            Transform Teleport_on2 = GameObject.Instantiate(Teleport_on, room);
            Teleport_on2.localPosition = new Vector3(-0.6f, 8.9f, -0.1f);
            Teleport_on2.localScale = new Vector3(-1.5f, 1.5f, 1.5f);
            Teleport_on2.name = "secretroom_teleport-on2";
            ActivateConsole(Teleport_on2.gameObject);

            Transform Teleport_console = GameObject.Instantiate(Teleport_on, room);
            Teleport_console.GetComponent<SpriteRenderer>().sprite = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.SecretRoom_Teleport_Console.png", 115f);
            Teleport_console.localPosition = new Vector3(2.55f, 9.75f, -0.1f);
            Teleport_console.localScale = new Vector3(1.35f, 1.35f, 1.35f);
            Teleport_console.name = "secretroom_teleport-console";
            ActivateConsole(Teleport_console.gameObject, 1.2f);

            ActivateConsole(Teleport_on.gameObject);
        }
    }

    [HarmonyPatch(typeof(VitalsMinigame), nameof(VitalsMinigame.Update))]
    public static class VitalsMinigameUpdate
    {
        public static void Postfix(VitalsMinigame __instance)
        {
            if (__instance.name != "secretroom_teleport-console") return;
            if (UsePlayer == null)
            {
                __instance.Close();
                return;
            }
            if (PlayerControl.LocalPlayer.PlayerId != UsePlayer.PlayerId) return;
            new LateTask(() =>
            {
                if (GameObject.FindObjectOfType<VitalsMinigame>() == null && onTask)
                {
                    lastUpdate = DateTime.UtcNow;
                    MessageWriter writer = RPCHelper.StartRPC(CustomRPC.SetSecretRoomTeleportStatus);
                    writer.Write((byte)Status.CloseConsole);
                    RPCHelper.EndRPC(writer);
                    SetSecretRoomTeleportStatus(Status.CloseConsole, 0, 0);
                    onTask = false;
                }
            }, 0.03f, "SecretRoom-VitalsMinigameUpdate");
            if (left == null || right == null) return;
            if (leftplayer != null)
            {
                if (leftplayer.IsDead())
                {
                    leftplayer = null;
                    return;
                }
                if (!left.gameObject.active)
                {
                    left.gameObject.SetActive(true);
                }
                left.UpdateFromPlayerOutfit(leftplayer.CurrentOutfit, PlayerMaterial.MaskType.ComplexUI, false, true);
                left.NameText().text = leftplayer.CurrentOutfit.PlayerName;
            }
            else
            {
                left.gameObject.SetActive(false);
            }
            if (rightplayer != null)
            {
                if (rightplayer.IsDead())
                {
                    rightplayer = null;
                    return;
                }
                if (!right.gameObject.active)
                {
                    right.gameObject.SetActive(true);
                }
                right.UpdateFromPlayerOutfit(rightplayer.CurrentOutfit, PlayerMaterial.MaskType.ComplexUI, false, true);
                right.NameText().text = rightplayer.CurrentOutfit.PlayerName;
            }
            else
            {
                right.gameObject.SetActive(false);
            }
        }
    }
    static bool onTask;
    public static DateTime lastUpdate;
    [HarmonyPatch(typeof(VitalsMinigame), nameof(VitalsMinigame.Begin))]
    class VitalsMinigameStartPatch
    {
        static void Postfix()
        {
            onTask = true;
        }
    }
    [HarmonyPatch(typeof(Console), nameof(Console.Use))]
    public static class ConsoleUsePatch
    {
        public static bool Prefix(Console __instance)
        {
            if (CachedPlayer.LocalPlayer.IsDead()) return true;
            if (__instance.name is "secretroom_teleport-on" or "secretroom_teleport-on2")
            {
                if (LowerInfoText == null)
                {
                    LowerInfoText = UnityEngine.Object.Instantiate(PlayerControl.LocalPlayer.NameText());
                    LowerInfoText.transform.parent = FastDestroyableSingleton<HudManager>.Instance.transform;
                    LowerInfoText.transform.localPosition = new Vector3(0, -1.5f, 0);
                    LowerInfoText.transform.localScale = new Vector3(2, 2f, 2);
                    LowerInfoText.alignment = TextAlignmentOptions.Center;
                    LowerInfoText.overflowMode = TextOverflowModes.Overflow;
                    LowerInfoText.enableWordWrapping = false;
                    LowerInfoText.color = Color.white;
                    LowerInfoText.fontSizeMin = 2.0f;
                    LowerInfoText.fontSizeMax = 2.0f;
                }
                __instance.CanUse(PlayerControl.LocalPlayer.Data, out var canUse, out var _);
                if (canUse)
                {
                    LowerInfoText.text = ModTranslation.GetString("ExitExperimentEsc"); // Escで実験から抜ける
                    //LowerInfoText.
                    MessageWriter writer = RPCHelper.StartRPC(CustomRPC.SetSecretRoomTeleportStatus);
                    writer.Write((byte)Status.Join);
                    writer.Write(CachedPlayer.LocalPlayer.PlayerId);
                    int id = 1;
                    if (__instance.name.Contains("2"))
                    {
                        if (leftplayer != null) return false;
                        id = 0;
                    }
                    else
                    {
                        if (rightplayer != null) return false;
                    }
                    writer.Write(id);
                    RPCHelper.EndRPC(writer);
                    SetSecretRoomTeleportStatus(Status.Join, CachedPlayer.LocalPlayer.PlayerId, (byte)id);
                    PlayerControl.LocalPlayer.moveable = false;
                    //__instance.StartCoroutine(Move(__instance));
                    Coroutine move = __instance.StartCoroutine(Move2(__instance).WrapToIl2Cpp());
                    __instance.StartCoroutine(Escape(__instance, move).WrapToIl2Cpp());
                }
                return false;
            }
            else if (__instance.name == "secretroom_teleport-console")
            {
                __instance.CanUse(PlayerControl.LocalPlayer.Data, out var canUse, out var _);
                if (canUse)
                {
                    if (RoleHelpers.IsComms()) return false;
                    MessageWriter writer = RPCHelper.StartRPC(CustomRPC.SetSecretRoomTeleportStatus);
                    writer.Write((byte)Status.UseConsole);
                    writer.Write(CachedPlayer.LocalPlayer.PlayerId);
                    RPCHelper.EndRPC(writer);
                    SetSecretRoomTeleportStatus(Status.UseConsole, CachedPlayer.LocalPlayer.PlayerId, 0);
                    ViewMinigame();
                    var minigame = GameObject.FindObjectOfType<VitalsMinigame>();
                    minigame.name = "secretroom_teleport-console";
                    minigame.BatteryText.text = ModTranslation.GetString("StartExperiment"); // 実験を開始する
                    minigame.BatteryText.color = Color.white;
                    minigame.BatteryText.transform.localPosition = new Vector3(0f, -0.75f, -9f);
                    minigame.BatteryText.transform.localScale = new Vector3(1.75f, 1.75f, 1.75f);
                    minigame.BatteryText.transform.FindChild("Sprite").gameObject.SetActive(false);

                    PoolablePlayer leftpool = GameObject.Instantiate(minigame.vitals[0].PlayerIcon, minigame.transform);
                    leftpool.UpdateFromPlayerOutfit(PlayerControl.LocalPlayer.CurrentOutfit, PlayerMaterial.MaskType.ComplexUI, false, true);
                    leftpool.transform.localPosition = new Vector3(-2f, 0.5f, 0f);
                    leftpool.transform.localScale = new Vector3(1, 1, 1);
                    //leftpool.UpdateFromLocalPlayer(PlayerMaterial.MaskType.ComplexUI);
                    leftpool.cosmetics.colorBlindText.transform.localPosition = new Vector3(0.3f, -0.251f, -0.5f);
                    var lefttext = leftpool.NameText();
                    lefttext.gameObject.SetActive(true);
                    lefttext.text = CachedPlayer.LocalPlayer.Data.PlayerName;
                    lefttext.transform.localPosition = new Vector3(0, 1, -0.5f);
                    lefttext.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
                    left = leftpool;

                    //rightplayer = PlayerControl.LocalPlayer;
                    PoolablePlayer rightpool = GameObject.Instantiate(minigame.vitals[0].PlayerIcon, minigame.transform);
                    rightpool.UpdateFromPlayerOutfit(PlayerControl.LocalPlayer.CurrentOutfit, PlayerMaterial.MaskType.ComplexUI, false, true);
                    rightpool.transform.localPosition = new Vector3(2f, 0.5f, 0f);
                    rightpool.transform.localScale = new Vector3(-1, 1, 1);
                    //rightpool.UpdateFromLocalPlayer(PlayerMaterial.MaskType.ComplexUI);
                    rightpool.cosmetics.colorBlindText.transform.localPosition = new Vector3(-0.3f, -0.251f, -0.5f);
                    rightpool.cosmetics.colorBlindText.transform.localScale = new Vector3(-3, 3, 3);
                    var righttext = rightpool.NameText();
                    righttext.gameObject.SetActive(true);
                    righttext.text = CachedPlayer.LocalPlayer.Data.PlayerName;
                    righttext.transform.localPosition = new Vector3(0, 1, -0.5f);
                    righttext.transform.localScale = new Vector3(-1.5f, 1.5f, 1.5f);
                    rightpool.name = "right";
                    right = rightpool;

                    var timetext = GameObject.Instantiate(rightpool.NameText(), minigame.transform);
                    timetext.gameObject.SetActive(true);
                    timetext.GetComponent<TextMeshPro>().characterWidthAdjustment = 10f;
                    timetext.text = "";
                    timetext.transform.localPosition = new Vector3(0, 1, -0.5f);
                    timetext.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);

                    var startbutton = GameObject.Instantiate(minigame.transform.FindChild("CloseButton"), minigame.transform);
                    startbutton.GetComponent<SpriteRenderer>().sprite = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.SecretRoom_Aislehjhrtjh.png", 115f);
                    var button = startbutton.GetComponent<PassiveButton>();
                    startbutton.transform.localScale = new Vector3(1, 20, 1);
                    button.OnClick = new();
                    bool Is = false;
                    button.OnClick.AddListener((UnityEngine.Events.UnityAction)(() =>
                    {
                        if (leftplayer == null && rightplayer == null) return;
                        if (!Is)
                        {
                            MessageWriter writer = RPCHelper.StartRPC(CustomRPC.SetSecretRoomTeleportStatus);
                            writer.Write((byte)Status.Wait);
                            RPCHelper.EndRPC(writer);
                            Is = true;
                            var obj = GameObject.FindObjectOfType<VitalsMinigame>();
                            obj.BatteryText.text = ModTranslation.GetString("Processing"); // 処理中...
                            new LateTask(() =>
                            {
                                if (obj)
                                {
                                    obj.BatteryText.text = ModTranslation.GetString("SuccessfulExperiment"); // 実験成功
                                    MessageWriter writer = RPCHelper.StartRPC(CustomRPC.SetSecretRoomTeleportStatus);
                                    writer.Write((byte)Status.Teleport);
                                    RPCHelper.EndRPC(writer);
                                    new LateTask(() =>
                                    {
                                        GameObject.FindObjectOfType<VitalsMinigame>().Close();
                                        leftplayer = null;
                                        rightplayer = null;
                                    }, 0.1f, "VitalText Close");
                                }
                            }, 1f, "実験成功");
                        }
                    }));
                    foreach (VitalsPanel panel in minigame.vitals) GameObject.Destroy(panel.gameObject);
                }
                return false;
            }
            return true;
        }
        static IEnumerator Escape(Console __instance, Coroutine coro)
        {
            while (true)
            {
                if (RoleClass.IsMeeting || (AmongUsClient.Instance.GameState != AmongUsClient.GameStates.Started && AmongUsClient.Instance.NetworkMode != NetworkModes.FreePlay))
                {
                    yield break;
                }
                if ((leftplayer != null && leftplayer.PlayerId == CachedPlayer.LocalPlayer.PlayerId) ||
                    (rightplayer != null && rightplayer.PlayerId == CachedPlayer.LocalPlayer.PlayerId))
                {
                    while (IsWait)
                    {
                        if (RoleClass.IsMeeting || (AmongUsClient.Instance.GameState != AmongUsClient.GameStates.Started && AmongUsClient.Instance.NetworkMode != NetworkModes.FreePlay))
                        {
                            LowerInfoText.text = "";
                            yield break;
                        }
                        yield return null;
                    }
                    if (Input.GetKey(KeyCode.Escape))
                    {
                        LowerInfoText.text = "";
                        if (IsWait) yield break;
                        MessageWriter writer = RPCHelper.StartRPC(CustomRPC.SetSecretRoomTeleportStatus);
                        writer.Write((byte)Status.Break);
                        byte id = 0;
                        if (rightplayer != null && rightplayer.PlayerId == CachedPlayer.LocalPlayer.PlayerId)
                        {
                            id = 1;
                        }
                        writer.Write(id);
                        RPCHelper.EndRPC(writer);
                        SetSecretRoomTeleportStatus(Status.Break, id, 0);
                        if (coro != null)
                        {
                            __instance.StopCoroutine(coro);
                        }
                        PlayerControl.LocalPlayer.moveable = true;
                        Camera.main.GetComponent<FollowerCamera>().Locked = false;
                        yield break;
                    }
                }
                else
                {
                    LowerInfoText.text = "";
                    Camera.main.GetComponent<FollowerCamera>().Locked = false;
                    yield break;
                }
                yield return null;
            }
        }

        static IEnumerator Move2(Console __instance)
        {
            PlayerPhysics myPhysics = PlayerControl.LocalPlayer.MyPhysics;
            Vector2 worldPos = __instance.name.Contains("2") ? new Vector2(0.14f, -5.025f) : __instance.transform.position;
            worldPos = __instance.transform.position;
            Camera.main.GetComponent<FollowerCamera>().Locked = false;
            yield return myPhysics.WalkPlayerTo(worldPos, 0.001f);
            yield return new WaitForSeconds(0.1f);
            Camera.main.GetComponent<FollowerCamera>().Locked = true;
        }
    }
    [HarmonyPatch(typeof(Console), nameof(Console.CanUse))]
    public static class ConsoleCanUsePatch
    {
        public static bool Prefix(ref float __result, Console __instance, [HarmonyArgument(0)] GameData.PlayerInfo pc, [HarmonyArgument(1)] out bool canUse, [HarmonyArgument(2)] out bool couldUse)
        {
            canUse = false;
            couldUse = true;
            __result = byte.MaxValue;

            PlayerTask task = null;
            if (PlayerControl.LocalPlayer.IsRole(RoleId.Tasker) && (task = __instance.FindTask(pc.Object)) != null)
                foreach (Console console in task.FindConsoles())
                    console.AllowImpostor = true;


            if (__instance.name is not "secretroom_teleport-on2" and not "secretroom_teleport-on" and not "secretroom_teleport-console") return true;

            if (__instance.name == "secretroom_teleport-console")
            {
                if (UsePlayer != null) return false;
            }
            else
            {
                if (IsWait) return false;
                if (__instance.name.Contains("2"))
                {
                    if (leftplayer != null) return false;
                }
                else
                {
                    if (rightplayer != null) return false;
                }
            }

            float num = float.MaxValue;
            PlayerControl @object = pc.Object;
            Vector2 truePosition = @object.GetTruePosition();
            Vector3 position = __instance.transform.position;
            couldUse = !pc.IsDead || (@object.CanMove && pc.Role.CanUse(__instance.TryCast<IUsable>()) && (!__instance.onlyFromBelow || truePosition.y < position.y));
            canUse = couldUse;
            if (canUse)
            {
                num = Vector2.Distance(truePosition, __instance.transform.position);
                canUse &= num <= __instance.UsableDistance;
                if (__instance.checkWalls)
                {
                    canUse &= !PhysicsHelpers.AnythingBetween(truePosition, position, Constants.ShadowMask, useTriggers: false);
                }
            }
            __result = num;
            return false;
        }
    }
    static void ViewMinigame()
    {
        var moto = PlayerControl.LocalPlayer.Data.Role.Role;
        FastDestroyableSingleton<RoleManager>.Instance.SetRole(PlayerControl.LocalPlayer, RoleTypes.Scientist);
        CachedPlayer.LocalPlayer.Data.Role.TryCast<ScientistRole>().UseAbility();
        FastDestroyableSingleton<RoleManager>.Instance.SetRole(PlayerControl.LocalPlayer, moto);
    }
    static Console ActivateConsole(GameObject obj, float Distance = 1.7f)
    {
        if (obj == null) return null;
        obj.layer = LayerMask.NameToLayer("ShortObjects");
        Console console = obj.GetComponent<Console>();
        PassiveButton button = obj.GetComponent<PassiveButton>();
        CircleCollider2D collider = obj.GetComponent<CircleCollider2D>();
        if (!console)
        {
            console = obj.AddComponent<Console>();
            console.checkWalls = true;
            console.usableDistance = 0.7f;
            console.TaskTypes = new TaskTypes[0];
            console.ValidTasks = new Il2CppReferenceArray<TaskSet>(0);
            var list = ShipStatus.Instance.AllConsoles.ToList();
            list.Add(console);
            ShipStatus.Instance.AllConsoles = new Il2CppReferenceArray<Console>(list.ToArray());
        }
        if (console.Image == null)
        {
            console.Image = obj.GetComponent<SpriteRenderer>();
            console.Image.material = new Material(ShipStatus.Instance.AllConsoles[0].Image.material);
        }
        if (!button)
        {
            button = obj.AddComponent<PassiveButton>();
            button.OnMouseOut = new UnityEngine.Events.UnityEvent();
            button.OnMouseOver = new UnityEngine.Events.UnityEvent();
            button._CachedZ_k__BackingField = 0.1f;
            button.CachedZ = 0.1f;
        }
        if (!collider)
        {
            collider = obj.AddComponent<CircleCollider2D>();
            collider.radius = 0.4f;
            collider.isTrigger = true;
        }
        return console;
    }
}