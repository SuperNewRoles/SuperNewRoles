using System.Collections.Generic;
using SuperNewRoles.Buttons;
using SuperNewRoles.Helpers;
using TMPro;
using UnityEngine;

namespace SuperNewRoles.Roles.Impostor;

public static class Bat
{
    public static class CustomOptionData
    {
        private static int optionId = 205800;
        public static CustomRoleOption Option;
        public static CustomOption PlayerCount;
        public static CustomOption BatButtonCooldown;
        public static CustomOption BatButtonDuration;
        public static CustomOption BatIsCanUseCount;
        public static CustomOption BatCanUseCount;

        public static void SetupCustomOptions()
        {
            Option = CustomOption.SetupCustomRoleOption(optionId, false, RoleId.Bat); optionId++;
            PlayerCount = CustomOption.Create(optionId, false, CustomOptionType.Impostor, "SettingPlayerCountName", CustomOptionHolder.ImpostorPlayers[0], CustomOptionHolder.ImpostorPlayers[1], CustomOptionHolder.ImpostorPlayers[2], CustomOptionHolder.ImpostorPlayers[3], Option); optionId++;
            BatButtonCooldown = CustomOption.Create(optionId, false, CustomOptionType.Impostor, "NiceScientistCooldownSetting", 30f, 2.5f, 60f, 2.5f, Option); optionId++;
            BatButtonDuration = CustomOption.Create(optionId, false, CustomOptionType.Impostor, "BatButtonDurationSetting", 5f, 2.5f, 60f, 2.5f, Option); optionId++;
            BatIsCanUseCount = CustomOption.Create(optionId, false, CustomOptionType.Impostor, "BatIsCanUseCountSetting", true, Option); optionId++;
            BatCanUseCount = CustomOption.Create(optionId, false, CustomOptionType.Impostor, "BatCanUseCountSetting", 3, 1, 10, 1, BatIsCanUseCount); optionId++;
        }
    }
    public static void RpcDeviceStop()
    {
        RPCHelper.SendSingleRpc(CustomRPC.BatSetDeviceStop);
        BatSetDeviceStop();
    }
    public static void BatSetDeviceStop()
    {
        RoleData.DeviceStopTimer = RoleData.DeviceStopTime;
        RoleData.AliveData = new();
        foreach (PlayerControl player in CachedPlayer.AllPlayers.AsSpan())
        {
            RoleData.AliveData[player.PlayerId] = player.IsAlive();
        }
        RoleData.RoomAdminData = new();
        if (MapBehaviour.Instance == null)
            FastDestroyableSingleton<HudManager>.Instance.InitMap();
        MapCountOverlay __instance = MapBehaviour.Instance.countOverlay;
        foreach (CounterArea counterArea in __instance.CountAreas)
        {
            RoleData.RoomAdminData[(int)counterArea.RoomType] = 0;
            // ロミジュリと絵画の部屋をアドミンの対象から外す
            if (counterArea.RoomType > SystemTypes.Hallway)
            {
                PlainShipRoom plainShipRoom = MapUtilities.CachedShipStatus.FastRooms[counterArea.RoomType];

                if (plainShipRoom != null && plainShipRoom.roomArea)
                {
                    HashSet<int> hashSet = new();
                    int num = plainShipRoom.roomArea.OverlapCollider(__instance.filter, __instance.buffer);
                    int count = 0;

                    for (int j = 0; j < num; j++)
                    {
                        Collider2D collider2D = __instance.buffer[j];
                        if (collider2D.CompareTag("DeadBody") && __instance.includeDeadBodies)
                        {
                            count++;
                        }
                        else
                        {
                            PlayerControl component = collider2D.GetComponent<PlayerControl>();
                            if (!component) continue;
                            if (component.Data == null || component.Data.Disconnected || component.Data.IsDead) continue;
                            if (!__instance.showLivePlayerPosition && component.AmOwner) continue;
                            if (!hashSet.Add(component.PlayerId)) continue;

                            if (component.IsRole(RoleId.Vampire, RoleId.Dependents)) continue;
                            if (!CustomOptionHolder.CrackerIsAdminView.GetBool() && RoleClass.Cracker.CrackedPlayers.Contains(component.PlayerId) &&
                               (component.PlayerId != CachedPlayer.LocalPlayer.PlayerId || !CustomOptionHolder.CrackerIsSelfNone.GetBool()))
                                continue;
                            count++;
                        }
                    }
                    RoleData.RoomAdminData[(int)counterArea.RoomType] = count;

                }
                else Debug.LogWarning($"Couldn't find counter for:{counterArea.RoomType}");
            }
        }
    }
    internal static class RoleData
    {
        public static List<PlayerControl> Player;
        public static Color32 color = RoleClass.ImpostorRed;
        public static float BatButtonCooldown;
        public static float DeviceStopTimer;
        public static float DeviceStopTime;
        //デバイス停止中か
        public static bool IsDeviceStop => DeviceStopTimer > 0;
        //生存しているならtrue
        public static Dictionary<byte, bool> AliveData;
        public static Dictionary<int, int> RoomAdminData;
        public static int CanUseCount;
        public static void ClearAndReload()
        {
            Player = new();
            BatButtonCooldown = CustomOptionData.BatButtonCooldown.GetFloat();
            DeviceStopTimer = 0;
            DeviceStopTime = CustomOptionData.BatButtonDuration.GetFloat();
            AliveData = new();
            CanUseCount = CustomOptionData.BatIsCanUseCount.GetBool() ? CustomOptionData.BatCanUseCount.GetInt() : -1;
        }
    }
    public static void FixedUpdate()
    {
        if (RoleData.DeviceStopTimer > 0)
        {
            RoleData.DeviceStopTimer -= Time.fixedDeltaTime;
            if (RoleData.DeviceStopTimer <= 0)
            {
                RoleData.AliveData.Clear();
                RoleData.RoomAdminData.Clear();
                RoleData.DeviceStopTimer = -1;
            }
        }
    }
    internal static class Button
    {
        private static CustomButton BatButton;
        private static TMP_Text BatCountText;
        private static Sprite GetButtonSprite() => ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.BatButton.png", 115f);

        internal static void SetupCustomButtons(HudManager hm)
        {
            BatButton = new(
                () =>
                {
                    //使用中じゃないなら
                    if (!BatButton.isEffectActive)
                    {
                        RpcDeviceStop();
                        if (RoleData.CanUseCount > 0)
                            RoleData.CanUseCount--;
                        ResetBatButtonText();
                    }
                },
                (bool isAlive, RoleId role) => { return isAlive && role == RoleId.Bat; },
                () => { return PlayerControl.LocalPlayer.CanMove && (RoleData.CanUseCount == -1 || RoleData.CanUseCount > 0); },
                () => { ResetBatButtonCool(); BatButton.isEffectActive = false; },
                GetButtonSprite(),
                new Vector3(-2f, 1, 0),
                hm,
                hm.AbilityButton,
                KeyCode.F,
                49,
                () => { return false; },
                true,
                5f,
                () =>
                {
                    ResetBatButtonCool();
                }
            )
            {
                buttonText = ModTranslation.GetString("BatButtonName"),
                showButtonText = true
            };
            BatCountText = GameObject.Instantiate(BatButton.actionButton.cooldownTimerText, BatButton.actionButton.cooldownTimerText.transform.parent);
            BatCountText.text = "";
            BatCountText.enableWordWrapping = false;
            BatCountText.transform.localScale = Vector3.one * 0.5f;
            BatCountText.transform.localPosition += new Vector3(-0.05f, 0.7f, 0);
        }

        private static void ResetBatButtonCool()
        {
            BatButton.MaxTimer = RoleData.BatButtonCooldown;
            BatButton.Timer = RoleData.BatButtonCooldown;
            BatButton.effectCancellable = false;
            BatButton.EffectDuration = RoleData.DeviceStopTime;
            ResetBatButtonText();
        }
        private static void ResetBatButtonText()
        {
            if (RoleData.CanUseCount >= 0)
                BatCountText.text = string.Format(ModTranslation.GetString("BatNumTextName"), RoleData.CanUseCount);
            else
                BatCountText.text = "";
        }
    }

    // ここにコードを書きこんでください
}