using System;
using System.Collections.Generic;
using SuperNewRoles.Buttons;
using SuperNewRoles.Helpers;
using UnityEngine;
using HarmonyLib;
using SuperNewRoles.Mode;

namespace SuperNewRoles.Roles.Crewmate;

public class Squid
{
    // CustomOptionDate
    private const int OptionId = 1118;// 設定のId
    public static CustomRoleOption SquidOption;
    public static CustomOption SquidPlayerCount;
    public static CustomOption SquidCoolTime;
    public static CustomOption SquidDurationTime;
    public static CustomOption SquidBoostSpeed;
    public static CustomOption SquidBoostSpeedTime;
    public static CustomOption SquidNotKillTime;
    public static CustomOption SquidDownVision;
    public static CustomOption SquidObstructionTime;
    public static void SetupCustomOptions()
    {
        SquidOption = CustomOption.SetupCustomRoleOption(OptionId, false, RoleId.Squid);
        SquidPlayerCount = CustomOption.Create(OptionId + 1, false, CustomOptionType.Crewmate, "SettingPlayerCountName", CustomOptionHolder.CrewPlayers[0], CustomOptionHolder.CrewPlayers[1], CustomOptionHolder.CrewPlayers[2], CustomOptionHolder.CrewPlayers[3], SquidOption);
        SquidCoolTime = CustomOption.Create(OptionId + 2, false, CustomOptionType.Crewmate, "SquidCoolTimeSetting", 30f, 0f, 60f, 2.5f, SquidOption);
        SquidDurationTime = CustomOption.Create(OptionId + 3, false, CustomOptionType.Crewmate, "SquidDurationTimeSetting", 10f, 2.5f, 30f, 2.5f, SquidOption);
        SquidBoostSpeed = CustomOption.Create(OptionId + 4, false, CustomOptionType.Crewmate, "SquidBoostSpeedSetting", 1.25f, 0f, 5f, 0.25f, SquidOption);
        SquidBoostSpeedTime = CustomOption.Create(OptionId + 5, false, CustomOptionType.Crewmate, "SquidBoostSpeedTimeSetting", 2.5f, 0f, 10f, 0.5f, SquidOption);
        SquidNotKillTime = CustomOption.Create(OptionId + 6, false, CustomOptionType.Crewmate, "SquidNotKillTimeSetting", 2.5f, 0f, 10f, 0.5f, SquidOption);
        SquidDownVision = CustomOption.Create(OptionId + 10, false, CustomOptionType.Crewmate, "SquidDownVisionSetting", 0.5f, 0f, 5f, 0.25f, SquidOption);
        SquidObstructionTime = CustomOption.Create(OptionId + 11, false, CustomOptionType.Crewmate, "SquidObstructionTimeSetting", 5f, 2.5f, 30f, 2.5f, SquidOption);
    }

    public static List<PlayerControl> SquidPlayer;
    public static Color32 color = new(187, 255, 255, byte.MaxValue);
    public static float CoolTime;
    public static float DurationTime;
    public static float BoostSpeed;
    public static float BoostSpeedTime;
    public static float NotKillTime;
    public static float DownVision;
    public static float ObstructionTime;
    public static Sprite GetButtonSprite() => ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.SquidButton.png", 115f);
    public static DateTime ButtonTimer;
    public static int DefaultKillDistance;
    public static Dictionary<byte, bool> IsVigilance;
    public static bool IsSpeedBoost;
    public static float SpeedBoostTimer;
    public static bool IsKillGuard;
    public static bool IsDownVision;
    public static float ObstructionTimer;
    public static void ClearAndReload()
    {
        SquidPlayer = new();
        CoolTime = SquidCoolTime.GetFloat();
        DurationTime = SquidDurationTime.GetFloat();
        BoostSpeed = SquidBoostSpeed.GetFloat();
        BoostSpeedTime = SquidBoostSpeedTime.GetFloat();
        NotKillTime = SquidNotKillTime.GetFloat();
        DownVision = SquidDownVision.GetFloat();
        ObstructionTime = SquidObstructionTime.GetFloat();
        ButtonTimer = DateTime.Now;
        DefaultKillDistance = PlayerControl.GameOptions.KillDistance;
        IsVigilance = new();
        IsSpeedBoost = false;
        SpeedBoostTimer = BoostSpeedTime;
        IsKillGuard = false;
        IsDownVision = false;
        ObstructionTimer = ObstructionTime;
    }

    public static CustomButton SquidButton;
    public static void SetupCustomButton(HudManager player)
    {
        SquidButton = new(
            () =>
            {
                ButtonTimer = DateTime.Now;
                SetVigilance(true, PlayerControl.LocalPlayer);
            },
            (bool isAlive, RoleId role) => { return isAlive && role == RoleId.Squid; },
            () => { return PlayerControl.LocalPlayer.CanMove; },
            () =>
            {
                SquidButton.MaxTimer = CoolTime;
                SquidButton.Timer = CoolTime;
            },
            GetButtonSprite(),
            new Vector3(-1.8f, -0.06f, 0),
            player,
            player.AbilityButton,
            KeyCode.F,
            49,
            () => { return false; }
        )
        {
            buttonText = ModTranslation.GetString("SquidButtonName"),
            showButtonText = true
        };
    }
    public static void SquidButtonTimer()
    {
        if (IsVigilance.ContainsKey(PlayerControl.LocalPlayer.PlayerId) && IsVigilance[PlayerControl.LocalPlayer.PlayerId])
        {
            var timeSpanData = new TimeSpan(0, 0, 0, (int)DurationTime);
            SquidButton.MaxTimer = DurationTime;
            SquidButton.Timer = (float)(ButtonTimer + timeSpanData - DateTime.Now).TotalSeconds;
            SquidButton.actionButton.cooldownTimerText.color = Color.green;
            if (SquidButton.Timer <= 0f)
            {
                ResetCooldown();
            }
        }
        else
        {
            if (ButtonTimer == null)
            {
                ButtonTimer = DateTime.Now;
            }
            var timeSpanData = new TimeSpan(0, 0, 0, (int)CoolTime);
            SquidButton.Timer = (float)(ButtonTimer + timeSpanData - DateTime.Now).TotalSeconds;
            if (SquidButton.Timer <= 0f) SquidButton.Timer = 0f;
        }
    }
    public static void ResetCooldown(bool vigilanceReset = true)
    {
        SquidButton.actionButton.cooldownTimerText.color = Color.white;
        SquidButton.MaxTimer = CoolTime;
        SquidButton.Timer = CoolTime;
        ButtonTimer = DateTime.Now;
        if (vigilanceReset) SetVigilance(false, PlayerControl.LocalPlayer);
    }

    public static void SetKillTimer(float killCool)
    {
        foreach (var button in CustomButton.buttons)
        {
            if (button.actionButton.name == "KillButton(Clone)" && button.HasButton(PlayerControl.LocalPlayer.IsAlive(), PlayerControl.LocalPlayer.GetRole()))
            {
                if (button.Timer <= killCool || button.Timer >= (button.MaxTimer - 0.25f))
                {
                    button.Timer = killCool;
                }
            }
        }
        new LateTask(() =>
        {
            foreach (var button in CustomButton.buttons)
            {
                if (button.actionButton.name == "KillButton(Clone)" && button.HasButton(PlayerControl.LocalPlayer.IsAlive(), PlayerControl.LocalPlayer.GetRole()))
                {
                    if (button.Timer <= killCool || button.Timer >= (button.MaxTimer - 0.5f))
                    {
                        button.Timer = killCool;
                    }
                }
            }
        }, 0.25f, "Squid SetKillTime");
    }

    public static Sprite GetInkSprite(float size = 250f) => ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.SquidInk.png", size);
    public static void InkSet()
    {
        var random = new System.Random();
        int amount = DownVision switch
        {
            0.25f => random.Next(1, 5),
            0.5f => random.Next(6, 10),
            0.75f => random.Next(11, 15),
            _ => random.Next(16, 20)
        };
        List<Vector3> allDefaultPosition = new();
        for (int i = 0; i < amount; i++)
        {
            GameObject ink = new();
            Vector3 defaultPos = SetPosition($"Squid Ink{i + 1}");
            ink.name = $"Squid Ink{i + 1}";
            ink.transform.position = FastDestroyableSingleton<HudManager>.Instance.FullScreen.gameObject.transform.position + defaultPos;
            ink.layer = 4;
            var rend = ink.AddComponent<SpriteRenderer>();
            rend.sprite = GetInkSprite(random.Next(175, 275));
            rend.color = new(20 / 255f, 10 / 255f, 25 / 255f);
            Logger.Info($"[イカインク] defaultPos : (X : {defaultPos.x}, Y : {defaultPos.y}), inkSize : {rend.sprite.pixelsPerUnit}");
            FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(ObstructionTime, new Action<float>((p) =>
            {
                if (rend != null)
                {
                    var tmp = rend.color;
                    tmp.a = p >= 0.9f ? Mathf.Clamp01(1 - ((p - 0.9f) * 10)) : tmp.a;
                    rend.color = tmp;
                    rend.transform.position = FastDestroyableSingleton<HudManager>.Instance.FullScreen.gameObject.transform.position + defaultPos;
                }
                if ((p == 1f || RoleClass.IsMeeting) && rend != null && rend.gameObject != null) UnityEngine.Object.Destroy(rend.gameObject);
            })));
        }
        Vector3 SetPosition(string name)
        {
            int repetitionCount = 0;
        repetition:;
            Vector3 position = DownVision switch
            {
                0.25f => new(random.Next(-125, 125) / 100f, random.Next(-14, 10) / 10f, 0f),
                0.5f => new(random.Next(-225, 225) / 100f, random.Next(-25, 20) / 10f, 0f),
                0.75f => new(random.Next(-325, 325) / 100f, random.Next(-225, 265) / 100f, 0f),
                _ => new(random.Next(-425, 425) / 100f, random.Next(-265, 245) / 100f, 0f)
            };
            foreach (var pos in allDefaultPosition)
            {
                if (pos == null) break;
                if (Vector3.Distance(position, pos) <= DownVision switch
                {
                    0.25f => 0.5f,
                    0.5f => 1f,
                    0.75f => 1.5f,
                    _ => 2
                })
                {
                    repetitionCount++;
                    Logger.Info($"{name}の位置が{allDefaultPosition.IndexOf(pos)}に近い為再実行しました, 距離 : {Vector3.Distance(position, pos)}");
                    goto repetition;
                }
            }
            allDefaultPosition.Add(position);
            Logger.Info($"{name}の位置を{(repetitionCount >= 1 ? $"{repetitionCount}回目で" : "")}決定しました");
            return position;
        }
    }

    public static void SetVigilance(bool isVigilance, PlayerControl player)
    {
        var writer = RPCHelper.StartRPC(CustomRPC.SetVigilance);
        writer.Write(isVigilance);
        writer.Write(player.PlayerId);
        writer.EndRPC();
        RPCProcedure.SetVigilance(isVigilance, player.PlayerId);
    }
    public static void SetSpeedBoost(bool isSpeedBoost, PlayerControl player)
    {
        var writer = RPCHelper.StartRPC(CustomRPC.SetSpeedBoost, player);
        writer.Write(isSpeedBoost);
        writer.Write(player.PlayerId);
        writer.EndRPC();
    }

    public static void FixedUpdate()
    {
        if (IsSpeedBoost)
        {
            SpeedBoostTimer -= Time.fixedDeltaTime;
            if (SpeedBoostTimer <= 0)
            {
                SetSpeedBoost(false, PlayerControl.LocalPlayer);
                SpeedBoostTimer = BoostSpeedTime;
            }
        }
        if (IsDownVision)
        {
            ObstructionTimer -= Time.fixedDeltaTime;
            if (ObstructionTimer <= 0)
            {
                ObstructionTimer = ObstructionTime;
                IsDownVision = false;
                PlayerControl.GameOptions.KillDistance = DefaultKillDistance;
            }
        }
    }

    [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.FixedUpdate))]
    public static class PlayerPhysicsSpeedPatch
    {
        public static void Postfix(PlayerPhysics __instance)
        {
            if (AmongUsClient.Instance.GameState != AmongUsClient.GameStates.Started) return;
            if (ModeHandler.IsMode(ModeId.Default))
            {
                if (__instance.AmOwner && __instance.myPlayer.IsRole(RoleId.Squid) && IsSpeedBoost && __instance.myPlayer.CanMove && GameData.Instance)
                    __instance.body.velocity *= BoostSpeed;
            }
        }
    }
}