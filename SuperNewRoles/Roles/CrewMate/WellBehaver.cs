using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Hazel;
using SuperNewRoles.Buttons;
using SuperNewRoles.CustomObject;
using SuperNewRoles.Helpers;
using UnityEngine;

namespace SuperNewRoles.Roles.Crewmate;

public class WellBehaver
{
    private const int OptionId = 406400;
    public static CustomRoleOption WellBehaverOption;
    public static CustomOption WellBehaverPlayerCount;
    public static CustomOption WellBehaverLimitTrashCount;
    public static CustomOption WellBehaverFrequencyGarbageDumping;
    public static CustomOption WellBehaverAllPlayerCanSeeGarbage;
    public static void SetupCustomOptions()
    {
        WellBehaverOption = CustomOption.SetupCustomRoleOption(OptionId, false, RoleId.WellBehaver);
        WellBehaverPlayerCount = CustomOption.Create(OptionId + 1, false, CustomOptionType.Crewmate, "SettingPlayerCountName", CustomOptionHolder.CrewPlayers[0], CustomOptionHolder.CrewPlayers[1], CustomOptionHolder.CrewPlayers[2], CustomOptionHolder.CrewPlayers[3], WellBehaverOption);
        WellBehaverLimitTrashCount = CustomOption.Create(OptionId + 2, false, CustomOptionType.Crewmate, "WellBehaverLimitTrashCountSetting", 15f, 1f, 60f, 1f, WellBehaverOption);
        WellBehaverFrequencyGarbageDumping = CustomOption.Create(OptionId + 3, false, CustomOptionType.Crewmate, "WellBehaverFrequencyGarbageDumpingSetting", 5f, 1f, 30f, 1f, WellBehaverOption);
        WellBehaverAllPlayerCanSeeGarbage = CustomOption.Create(OptionId + 4, false, CustomOptionType.Crewmate, "WellBehaverCanAllPlayerSeeGarbageSetting", true, WellBehaverOption);
    }

    public static List<PlayerControl> WellBehaverPlayer;
    public static Color32 color = new(254, 196, 88, byte.MaxValue);
    public static List<PlayerControl> AlivePlayer;
    public static (int Now, int Next, int NexNex) AllowableLimitCorrection;
    public static Dictionary<byte, float> GarbageDumpingTimer;
    public static void ClearAndReload()
    {
        WellBehaverPlayer = new();
        AlivePlayer = new();
        AllowableLimitCorrection = (1, 1, 1);
        GarbageDumpingTimer = new();
        Garbage.ClearAndReload();
    }

    public static bool IsWaitSpawn(PlayerControl player) => ModHelpers.IsPositionDistance(player.transform.position, new Vector2(3, 6), 0.5f) ||
                                                            ModHelpers.IsPositionDistance(player.transform.position, new Vector2(-25, 40), 0.5f) ||
                                                            ModHelpers.IsPositionDistance(player.transform.position, new Vector2(-1.4f, 2.3f), 0.5f);

    public static CustomButton WellBehaverPickUpGarbageButton;
    public static void SetupCustomButtons(HudManager __instance)
    {
        WellBehaverPickUpGarbageButton = new(
            () =>
            {
                Garbage garbage = null;
                float min_distance = float.MaxValue;
                Vector2 truepos = PlayerControl.LocalPlayer.GetTruePosition();
                foreach (Garbage data in Garbage.AllGarbage)
                {
                    Vector2 pos = data.GarbageObject.transform.position;
                    if (PhysicsHelpers.AnythingBetween(truepos, pos, Constants.ShadowMask, false)) continue;
                    float distance = Vector2.Distance(truepos, pos);
                    if (distance <= Garbage.Distance && distance < min_distance)
                    {
                        min_distance = distance;
                        garbage = data;
                    }
                }
                if (garbage == null) return;
                Logger.Info($"{garbage.GarbageObject.name}が拾われた", "Garbage");
                MessageWriter writer = RPCHelper.StartRPC(CustomRPC.DestroyGarbage);
                writer.Write(garbage.GarbageObject.name);
                writer.EndRPC();
                garbage.Clear();
            },
            (bool isAlive, RoleId role) => { return isAlive && role == RoleId.WellBehaver; },
            () =>
            {
                if (!PlayerControl.LocalPlayer.CanMove) return false;
                return Garbage.AllGarbage.Any(x => Vector2.Distance(x.GarbageObject.transform.position, PlayerControl.LocalPlayer.GetTruePosition()) <= Garbage.Distance);
            },
            () =>
            {
                WellBehaverPickUpGarbageButton.MaxTimer = 0f;
                WellBehaverPickUpGarbageButton.Timer = WellBehaverPickUpGarbageButton.MaxTimer;
            },
            ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.WellBehaverButton.png", 115f),
            new Vector3(-2f, 0, 0),
            __instance,
            __instance.AbilityButton,
            KeyCode.F,
            49,
            () => { return false; }
        )
        {
            buttonText = ModTranslation.GetString("WellBehaverPickUpGarbageButtonName"),
            showButtonText = true,
        };
    }

    public static void FixedUpdate()
    {
        bool active = PlayerControl.LocalPlayer.IsRole(RoleId.WellBehaver) || WellBehaverAllPlayerCanSeeGarbage.GetBool() || PlayerControl.LocalPlayer.IsDead();
        if (Garbage.AllGarbageObject?.activeSelf != active) Garbage.AllGarbageObject?.SetActive(active);
        if (!AmongUsClient.Instance.AmHost) return;
        if (AlivePlayer.Count <= 0 || RoleClass.IsMeeting) return;
        if (Garbage.AllGarbage.Count >= WellBehaverLimitTrashCount.GetInt() * AllowableLimitCorrection.Now && WellBehaverPlayer.Any(x => x.IsAlive()))
        {
            foreach (PlayerControl player in WellBehaverPlayer)
            {
                if (player.IsDead()) continue;
                player.RpcMurderPlayer(player);
                player.RpcSetFinalStatus(FinalStatus.WorshiperSelfDeath);
            }
        }

        List<byte> keys = new();
        foreach (byte key in GarbageDumpingTimer.Keys)
        {
            PlayerControl player = ModHelpers.PlayerById(key);
            if (GameManager.Instance.LogicOptions.currentGameOptions.MapId == (byte)MapNames.Airship && IsWaitSpawn(player)) continue;
            if (player.inMovingPlat || player.onLadder) continue;
            keys.Add(key);
        }
        foreach (byte id in keys)
        {
            PlayerControl player = ModHelpers.PlayerById(id);
            if (player.IsDead()) continue;
            GarbageDumpingTimer[id] += Time.fixedDeltaTime;
            if (GarbageDumpingTimer[id] >= WellBehaverFrequencyGarbageDumping.GetFloat())
            {
                GarbageDumpingTimer[id] = 0;
                Vector2 truepos = player.GetTruePosition();
                MessageWriter writer = RPCHelper.StartRPC(CustomRPC.CreateGarbage);
                writer.Write(truepos.x);
                writer.Write(truepos.y);
                writer.EndRPC();
                new Garbage(truepos);
            }
        }
    }

    public static void WrapUp()
    {
        if (!AmongUsClient.Instance.AmHost) return;
        // 誰もいないなら処理しない
        if (WellBehaverPlayer.Count <= 0) return;
        GarbageDumpingTimer.Clear();
        foreach (PlayerControl player in AlivePlayer)
        {
            if (player.IsAlive()) continue;
            AllowableLimitCorrection = (AllowableLimitCorrection.Now, AllowableLimitCorrection.Next - 1, AllowableLimitCorrection.NexNex - 1);
        }
        AlivePlayer = WellBehaverPlayer.FindAll(x => x.IsAlive());
        int count = AlivePlayer.Count;

        // Capacityを指定してメモリにやさしく
        List<PlayerControl> players = new(PlayerControl.AllPlayerControls.Count / 2);
        foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            if (player.IsAlive() && (!player.IsRole(RoleId.WellBehaver) || count > 1))
                players.Add(player);

        for (int i = 0; i < count; i++)
        {
            PlayerControl player = players.GetRandom();
            // 必要ない場合には削除しない
            if (count > 1) players.RemoveAll(x => x.PlayerId == player.PlayerId);
            GarbageDumpingTimer[player.PlayerId] = 0f;
        }
    }

    [HarmonyPatch(typeof(IntroCutscene))]
    public static class IntroCutscenePatch
    {
        [HarmonyPatch(nameof(IntroCutscene.OnDestroy)), HarmonyPostfix]
        public static void OnDestroyPostfix()
        {
            if (!AmongUsClient.Instance.AmHost) return;
            AlivePlayer = WellBehaverPlayer.FindAll(x => x.IsAlive());
            AllowableLimitCorrection = (WellBehaverPlayer.Count, WellBehaverPlayer.Count, WellBehaverPlayer.Count);
            WrapUp();
        }
    }

    [HarmonyPatch(typeof(MeetingHud))]
    public static class MeetingHudPatch
    {
        [HarmonyPatch(nameof(MeetingHud.Awake)), HarmonyPostfix]
        public static void AwakePostfix()
        {
            if (!AmongUsClient.Instance.AmHost) return;
            AllowableLimitCorrection = (AllowableLimitCorrection.Next, AllowableLimitCorrection.NexNex, AllowableLimitCorrection.NexNex);
            foreach (PlayerControl player in AlivePlayer)
            {
                if (player.IsAlive()) continue;
                AllowableLimitCorrection = (AllowableLimitCorrection.Now, AllowableLimitCorrection.Next - 1, AllowableLimitCorrection.NexNex - 1);
            }
            AlivePlayer = WellBehaverPlayer.FindAll(x => x.IsAlive());
        }
    }
}
