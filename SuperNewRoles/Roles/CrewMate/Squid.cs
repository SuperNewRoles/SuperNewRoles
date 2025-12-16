using System;
using System.Collections.Generic;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Events;
using SuperNewRoles.Events.PCEvents;
using SuperNewRoles.Modules;
using SuperNewRoles.Modules.Events;
using SuperNewRoles.Modules.Events.Bases;
using SuperNewRoles.Roles.Ability;
using SuperNewRoles.Roles.Ability.CustomButton;
using UnityEngine;

namespace SuperNewRoles.Roles.Crewmate;

class Squid : RoleBase<Squid>
{
    public override RoleId Role { get; } = RoleId.Squid;
    public override Color32 RoleColor { get; } = new(187, 255, 255, 255);

    public override List<Func<AbilityBase>> Abilities { get; } = [
        () => new SquidVigilanceAbility(new SquidVigilanceData(
            SquidCoolTime,
            SquidDurationTime,
            SquidBoostSpeed,
            SquidBoostSpeedTime,
            SquidNotKillTime,
            SquidDownVision,
            SquidObstructionTime
        ))
    ];

    public override QuoteMod QuoteMod { get; } = QuoteMod.SuperNewRoles;
    public override short IntroNum { get; } = 2;

    public override AssignedTeamType AssignedTeam { get; } = AssignedTeamType.Crewmate;
    public override WinnerTeamType WinnerTeam { get; } = WinnerTeamType.Crewmate;
    public override TeamTag TeamTag { get; } = TeamTag.Crewmate;
    public override RoleTag[] RoleTags { get; } = [RoleTag.Support];
    public override RoleOptionMenuType OptionTeam { get; } = RoleOptionMenuType.Crewmate;

    [CustomOptionFloat(nameof(SquidCoolTime), 0f, 60f, 2.5f, 30f, translationName: "SquidCoolTimeSetting", suffix: "Seconds")]
    public static float SquidCoolTime;

    [CustomOptionFloat(nameof(SquidDurationTime), 2.5f, 30f, 2.5f, 10f, translationName: "SquidDurationTimeSetting", suffix: "Seconds")]
    public static float SquidDurationTime;

    [CustomOptionFloat(nameof(SquidBoostSpeed), 0f, 5f, 0.25f, 1.25f, translationName: "SquidBoostSpeedSetting")]
    public static float SquidBoostSpeed;

    [CustomOptionFloat(nameof(SquidBoostSpeedTime), 0f, 10f, 0.5f, 2.5f, translationName: "SquidBoostSpeedTimeSetting", suffix: "Seconds")]
    public static float SquidBoostSpeedTime;

    [CustomOptionFloat(nameof(SquidNotKillTime), 0f, 10f, 0.5f, 2.5f, translationName: "SquidNotKillTimeSetting", suffix: "Seconds")]
    public static float SquidNotKillTime;

    [CustomOptionFloat(nameof(SquidDownVision), 0f, 5f, 0.25f, 0.5f, translationName: "SquidDownVisionSetting")]
    public static float SquidDownVision;

    [CustomOptionFloat(nameof(SquidObstructionTime), 0f, 30f, 2.5f, 5f, translationName: "SquidObstructionTimeSetting", suffix: "Seconds")]
    public static float SquidObstructionTime;
}

public record SquidVigilanceData(
    float Cooldown,
    float Duration,
    float BoostSpeedMultiplier,
    float BoostSpeedDuration,
    float NoKillDuration,
    float DownVisionMultiplier,
    float ObstructionDuration
);

public sealed class SquidVigilanceAbility : CustomButtonBase, IButtonEffect
{
    public SquidVigilanceData Data { get; }

    public override float DefaultTimer => Data.Cooldown;
    public override string buttonText => ModTranslation.GetString("SquidButtonName");
    public override Sprite Sprite => AssetManager.GetAsset<Sprite>("SquidButton.png") ?? HudManager.Instance.UseButton.graphic.sprite;
    protected override KeyType keytype => KeyType.Ability1;

    public bool isEffectActive { get; set; }
    public Action OnEffectEnds => () => RpcSetVigilance(this, false);
    public float EffectDuration => Data.Duration;
    public float EffectTimer { get; set; }

    private EventListener<PlayerPhysicsFixedUpdateEventData> _physicsUpdateListener;
    private EventListener<TryKillEventData> _tryKillListener;

    private bool _boostActive;
    private float _boostRemaining;

    public SquidVigilanceAbility(SquidVigilanceData data)
    {
        Data = data;
    }

    public override void AttachToAlls()
    {
        base.AttachToAlls();
        SquidSharedState.EnsureInitialized();
        _tryKillListener = TryKillEvent.Instance.AddListener(OnTryKill);
    }

    public override void AttachToLocalPlayer()
    {
        base.AttachToLocalPlayer();
        _physicsUpdateListener = PlayerPhysicsFixedUpdateEvent.Instance.AddListener(OnPhysicsFixedUpdate);
    }

    public override void DetachToAlls()
    {
        base.DetachToAlls();
        _tryKillListener?.RemoveListener();
    }

    public override void DetachToLocalPlayer()
    {
        base.DetachToLocalPlayer();
        _physicsUpdateListener?.RemoveListener();
    }

    public override bool CheckIsAvailable()
    {
        return Player.Player != null && Player.Player.CanMove && MeetingHud.Instance == null;
    }

    public override void OnClick()
    {
        RpcSetVigilance(this, true);
    }

    private void OnPhysicsFixedUpdate(PlayerPhysicsFixedUpdateEventData data)
    {
        if (!data.Instance.AmOwner) return;
        if (MeetingHud.Instance != null)
        {
            _boostActive = false;
            _boostRemaining = 0f;
            return;
        }

        if (!_boostActive) return;

        _boostRemaining -= Time.fixedDeltaTime;
        if (_boostRemaining <= 0f)
        {
            _boostActive = false;
            _boostRemaining = 0f;
            return;
        }

        var vel = data.Instance.body.velocity;
        data.Instance.body.velocity = vel * Mathf.Max(0f, Data.BoostSpeedMultiplier);
    }

    private void OnTryKill(TryKillEventData data)
    {
        if (!data.RefSuccess) return;

        if (SquidSharedState.IsNoKillActive(data.Killer))
        {
            data.RefSuccess = false;
            if (data.Killer.AmOwner || ExPlayerControl.LocalPlayer.IsDead())
                data.RefTarget.Player.ShowFailedMurder();
            return;
        }

        if (data.RefTarget != Player) return;
        if (!isEffectActive) return;

        // OverKillerは貫通（旧実装互換）
        if (data.Killer.Role == RoleId.OverKiller) return;

        data.RefSuccess = false;

        // 通常のキル失敗演出（キラー視点 + 霊視点）
        if (data.Killer.AmOwner || ExPlayerControl.LocalPlayer.IsDead())
            data.RefTarget.Player.ShowFailedMurder();

        // 警戒解除（ローカル状態更新 + 同期）
        isEffectActive = false;
        EffectTimer = EffectDuration;
        if (actionButton != null)
            actionButton.cooldownTimerText.color = Palette.EnabledColor;

        // 自分にフラッシュ + 加速
        if (Player.AmOwner)
        {
            FlashHandler.ShowFlash(Color.white, 0.5f);
        }
        _boostActive = true;
        _boostRemaining = Data.BoostSpeedDuration;

        // キラーに妨害（キル不可 + 視界低下 + インク）
        SquidSharedState.ApplyKillerDebuff(data.Killer.PlayerId, Data.NoKillDuration, Data.ObstructionDuration);

        if (data.Killer.AmOwner)
        {
            data.Killer.SetKillTimerUnchecked(Data.NoKillDuration, Data.NoKillDuration);
            SquidInkOverlay.Show(Data.DownVisionMultiplier, Data.ObstructionDuration);
        }
    }

    [CustomRPC]
    private static void RpcSetVigilance(SquidVigilanceAbility ability, bool active)
    {
        ability.isEffectActive = active;
        if (!active)
        {
            ability._boostActive = false;
            ability._boostRemaining = 0f;
            if (ability.actionButton != null)
                ability.actionButton.cooldownTimerText.color = Palette.EnabledColor;
        }
    }
}

internal static class SquidSharedState
{
    private static readonly Dictionary<byte, float> NoKillRemaining = new();
    private static readonly Dictionary<byte, float> ObstructionRemaining = new();

    private static bool _initialized;
    private static EventListener _fixedUpdate;
    private static EventListener<MeetingStartEventData> _meetingStart;
    private static EventListener<ShipStatusLightEventData> _shipStatusLight;

    public static void EnsureInitialized()
    {
        if (_initialized) return;
        _initialized = true;

        _fixedUpdate = FixedUpdateEvent.Instance.AddListener(OnFixedUpdate);
        _meetingStart = MeetingStartEvent.Instance.AddListener(_ => ClearRoundEffects());
        _shipStatusLight = ShipStatusLightEvent.Instance.AddListener(OnShipStatusLight);
    }

    [HarmonyLib.HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.CoStartGame))]
    private static class CoStartGamePatch
    {
        public static void Postfix()
        {
            ClearAll();
        }
    }

    private static void ClearAll()
    {
        NoKillRemaining.Clear();
        ObstructionRemaining.Clear();
        SquidInkOverlay.Clear();
    }

    private static void ClearRoundEffects()
    {
        NoKillRemaining.Clear();
        ObstructionRemaining.Clear();
        SquidInkOverlay.Clear();
    }

    public static void ApplyKillerDebuff(byte killerId, float noKillDuration, float obstructionDuration)
    {
        NoKillRemaining[killerId] = Mathf.Max(NoKillRemaining.TryGetValue(killerId, out var curNoKill) ? curNoKill : 0f, noKillDuration);
        ObstructionRemaining[killerId] = Mathf.Max(ObstructionRemaining.TryGetValue(killerId, out var curObs) ? curObs : 0f, obstructionDuration);
    }

    public static bool IsNoKillActive(ExPlayerControl killer)
    {
        return killer != null && NoKillRemaining.TryGetValue(killer.PlayerId, out var t) && t > 0f;
    }

    private static void OnFixedUpdate()
    {
        if (NoKillRemaining.Count != 0)
        {
            foreach (var key in new List<byte>(NoKillRemaining.Keys))
            {
                NoKillRemaining[key] -= Time.fixedDeltaTime;
                if (NoKillRemaining[key] <= 0f) NoKillRemaining.Remove(key);
            }
        }

        if (ObstructionRemaining.Count != 0)
        {
            foreach (var key in new List<byte>(ObstructionRemaining.Keys))
            {
                ObstructionRemaining[key] -= Time.fixedDeltaTime;
                if (ObstructionRemaining[key] <= 0f) ObstructionRemaining.Remove(key);
            }
        }
    }

    private static void OnShipStatusLight(ShipStatusLightEventData data)
    {
        if (data.player == null || data.player.IsDead) return;
        if (PlayerControl.LocalPlayer == null) return;
        if (data.player.PlayerId != PlayerControl.LocalPlayer.PlayerId) return;
        if (!ObstructionRemaining.TryGetValue(data.player.PlayerId, out var remaining) || remaining <= 0f) return;

        if (ShipStatus.Instance != null)
        {
            data.lightRadius = Mathf.Max(0f, ShipStatus.Instance.MaxLightRadius * Squid.SquidDownVision);
        }
        else
        {
            data.lightRadius = Mathf.Max(0f, data.lightRadius * Squid.SquidDownVision);
        }
    }
}

internal static class SquidInkOverlay
{
    private static readonly List<GameObject> InkObjects = new();
    private static Sprite _inkSprite;
    private static Texture2D _inkTexture;

    public static void Show(float downVisionSetting, float duration)
    {
        if (duration <= 0f) return;
        if (HudManager.Instance == null) return;
        if (AmongUsClient.Instance == null || AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started) return;

        Clear();

        var random = new System.Random();
        int amount = downVisionSetting switch
        {
            0.25f => random.Next(1, 5),
            0.5f => random.Next(6, 10),
            0.75f => random.Next(11, 15),
            _ => random.Next(16, 20)
        };

        var basePos = HudManager.Instance.FullScreen != null
            ? HudManager.Instance.FullScreen.transform.position
            : HudManager.Instance.transform.position;

        List<Vector3> usedPositions = new();
        for (int i = 0; i < amount; i++)
        {
            var ink = new GameObject($"SquidInk{i + 1}");
            ink.layer = HudManager.Instance.gameObject.layer;
            var rend = ink.AddComponent<SpriteRenderer>();
            rend.sprite = GetInkSprite();
            rend.color = new Color(20 / 255f, 10 / 255f, 25 / 255f, 1f);
            Vector3 offset = ChoosePosition(random, downVisionSetting, usedPositions);
            ink.transform.position = basePos + offset;
            ink.transform.Rotate(0f, 0f, random.Next(0, 360));
            float scale = random.Next(75, 125) / 100f;
            ink.transform.localScale = new Vector3(scale, scale, 1f);
            InkObjects.Add(ink);

            HudManager.Instance.StartCoroutine(Effects.Lerp(duration, new Action<float>(p =>
            {
                if (rend == null || ink == null) return;
                if (MeetingHud.Instance != null)
                {
                    UnityEngine.Object.Destroy(ink);
                    return;
                }

                var currentBasePos = HudManager.Instance.FullScreen != null
                    ? HudManager.Instance.FullScreen.transform.position
                    : HudManager.Instance.transform.position;
                ink.transform.position = currentBasePos + offset;

                if (p >= 0.9f)
                {
                    float t = Mathf.Clamp01(1f - ((p - 0.9f) * 10f));
                    var c = rend.color;
                    c.a = t;
                    rend.color = c;
                }

                if (p >= 1f && ink != null)
                {
                    UnityEngine.Object.Destroy(ink);
                }
            })));
        }
    }

    public static void Clear()
    {
        for (int i = InkObjects.Count - 1; i >= 0; i--)
        {
            if (InkObjects[i] != null)
                UnityEngine.Object.Destroy(InkObjects[i]);
        }
        InkObjects.Clear();
    }

    private static Sprite GetInkSprite()
    {
        if (_inkSprite != null) return _inkSprite;
        _inkSprite = AssetManager.GetAsset<Sprite>("SquidInk.png");
        if (_inkSprite != null) return _inkSprite;

        const int size = 96;
        _inkTexture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        _inkTexture.wrapMode = TextureWrapMode.Clamp;
        _inkTexture.filterMode = FilterMode.Bilinear;

        var center = new Vector2((size - 1) / 2f, (size - 1) / 2f);
        float radius = (size - 4) / 2f;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center);
                float a = Mathf.Clamp01(1f - (dist / radius));
                a = a * a;
                _inkTexture.SetPixel(x, y, new Color(1f, 1f, 1f, a));
            }
        }
        _inkTexture.Apply(false, false);
        _inkSprite = Sprite.Create(_inkTexture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        return _inkSprite;
    }

    private static Vector3 ChoosePosition(System.Random random, float downVision, List<Vector3> used)
    {
        int repetition = 0;
    retry:
        Vector3 pos = downVision switch
        {
            0.25f => new Vector3(random.Next(-125, 125) / 100f, random.Next(-14, 10) / 10f, 0f),
            0.5f => new Vector3(random.Next(-225, 225) / 100f, random.Next(-25, 20) / 10f, 0f),
            0.75f => new Vector3(random.Next(-325, 325) / 100f, random.Next(-225, 265) / 100f, 0f),
            _ => new Vector3(random.Next(-425, 425) / 100f, random.Next(-265, 245) / 100f, 0f)
        };

        float minDist = downVision switch
        {
            0.25f => 0.5f,
            0.5f => 1f,
            0.75f => 1.5f,
            _ => 2f
        };

        foreach (var u in used)
        {
            if (Vector3.Distance(pos, u) <= minDist)
            {
                repetition++;
                if (repetition < 20)
                    goto retry;
                break;
            }
        }
        used.Add(pos);
        return pos;
    }
}
