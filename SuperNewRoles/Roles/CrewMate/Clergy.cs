using System;
using System.Collections.Generic;
using UnityEngine;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Roles.Ability;
using SuperNewRoles.Roles.Ability.CustomButton;
using SuperNewRoles.Modules;
using SuperNewRoles.Events;
using SuperNewRoles.Modules.Events.Bases;
using AmongUs.GameOptions;

namespace SuperNewRoles.Roles.Crewmate;

class Clergy : RoleBase<Clergy>
{
    public override RoleId Role { get; } = RoleId.Clergy;
    public override Color32 RoleColor { get; } = new(255, 244, 196, 255);
    public override List<Func<AbilityBase>> Abilities { get; } = [
        () => new ClergyAbility(new ClergyAbilityData(
            ClergyButtonCooldown,
            ClergyEffectDuration,
            ClergyVisionMultiplier,
            ClergyActiveForNeutralKiller
        ))
    ];

    public override QuoteMod QuoteMod { get; } = QuoteMod.SuperNewRoles;
    public override RoleTypes IntroSoundType { get; } = RoleTypes.Crewmate;
    public override short IntroNum { get; } = 1;

    public override AssignedTeamType AssignedTeam { get; } = AssignedTeamType.Crewmate;
    public override WinnerTeamType WinnerTeam { get; } = WinnerTeamType.Crewmate;
    public override TeamTag TeamTag { get; } = TeamTag.Crewmate;
    public override RoleTag[] RoleTags { get; } = [RoleTag.Support];
    public override RoleOptionMenuType OptionTeam { get; } = RoleOptionMenuType.Crewmate;

    [CustomOptionFloat("ClergyButtonCooldown", 5f, 120f, 5f, 30f, translationName: "CoolTime")]
    public static float ClergyButtonCooldown;

    [CustomOptionFloat("ClergyEffectDuration", 5f, 60f, 5f, 15f)]
    public static float ClergyEffectDuration;

    [CustomOptionFloat("ClergyVisionMultiplier", 0.1f, 1f, 0.1f, 0.3f)]
    public static float ClergyVisionMultiplier;
    [CustomOptionBool("ClergyActiveForNeutralKiller", false)]
    public static bool ClergyActiveForNeutralKiller;
}

public record ClergyAbilityData(float Cooldown, float Duration, float VisionMultiplier, bool ActiveForNeutralKiller);

public class ClergyAbility : CustomButtonBase, IButtonEffect
{
    public ClergyAbilityData Data { get; }

    public override float DefaultTimer => Data.Cooldown;
    public override string buttonText => ModTranslation.GetString("ClergyButtonText");
    public override Sprite Sprite => AssetManager.GetAsset<Sprite>("ClergyButton.png") ?? HudManager.Instance.UseButton.graphic.sprite;
    protected override KeyType keytype => KeyType.Ability1;

    public bool isEffectActive { get; set; }
    public Action OnEffectEnds => () => RpcSetClergyEffect(false);
    public float EffectDuration => Data.Duration;
    public float EffectTimer { get; set; }

    private EventListener<ShipStatusLightEventData> _shipStatusLightListener;
    private EventListener<MeetingStartEventData> _meetingStartListener;
    private EventListener _fixedUpdateListener;
    private bool active;
    private float transitionTimer;
    private float effectTimer;
    private const float TRANSITION_DURATION = 0.5f; // 2秒かけて遷移

    public ClergyAbility(ClergyAbilityData data)
    {
        Data = data;
    }

    public override void AttachToAlls()
    {
        base.AttachToAlls();
        _shipStatusLightListener = ShipStatusLightEvent.Instance.AddListener(OnShipStatusLight);
        _meetingStartListener = MeetingStartEvent.Instance.AddListener(OnMeetingStart);
        _fixedUpdateListener = FixedUpdateEvent.Instance.AddListener(OnFixedUpdate);
    }

    public override void DetachToAlls()
    {
        base.DetachToAlls();
        _shipStatusLightListener?.RemoveListener();
        _meetingStartListener?.RemoveListener();
        _fixedUpdateListener?.RemoveListener();
    }

    public override bool CheckIsAvailable()
    {
        return !active && transitionTimer <= 0;
    }

    public override void OnClick()
    {
        RpcSetClergyEffect(true);
    }

    private void OnFixedUpdate()
    {
        if (active)
        {
            // 効果時間の管理
            effectTimer -= Time.fixedDeltaTime;
            if (effectTimer <= 0)
            {
                active = false;
            }

            // 効果開始時の遷移
            if (transitionTimer < TRANSITION_DURATION)
            {
                transitionTimer += Time.fixedDeltaTime;
                transitionTimer = Mathf.Min(transitionTimer, TRANSITION_DURATION);
            }
        }
        else
        {
            // 効果終了時の遷移
            if (transitionTimer > 0)
            {
                transitionTimer -= Time.fixedDeltaTime;
                transitionTimer = Mathf.Max(transitionTimer, 0);
            }
        }
    }

    private void OnShipStatusLight(ShipStatusLightEventData data)
    {
        if (transitionTimer <= 0) return;
        if (data.player == null || data.player.IsDead) return;

        ExPlayerControl player = (ExPlayerControl)data.player;

        // クルーメイト以外の視界を下げる
        if (CheckPlayer(player))
        {
            // 遷移の進行度を計算（0から1）
            float transitionProgress = transitionTimer / TRANSITION_DURATION;

            // スムーズな遷移のためのイージング関数（ease-in-out）
            float easedProgress = transitionProgress * transitionProgress * (3f - 2f * transitionProgress);

            // 通常の視界から設定倍率まで徐々に変化
            float visionMultiplier = Mathf.Lerp(1f, Data.VisionMultiplier, easedProgress);
            data.lightRadius *= visionMultiplier;
        }
    }

    private bool CheckPlayer(ExPlayerControl player)
    {
        return player.IsImpostor() || (Data.ActiveForNeutralKiller && player.IsNeutral() && player.IsKiller());
    }

    private void OnMeetingStart(MeetingStartEventData data)
    {
        active = false;
        // 会議開始時は即座に効果を終了
        transitionTimer = 0;
    }

    [CustomRPC]
    public void RpcSetClergyEffect(bool isActive)
    {
        active = isActive;
        if (active)
        {
            // 効果開始時はタイマーをリセット
            transitionTimer = 0;
            effectTimer = EffectDuration;
            if (CheckPlayer(ExPlayerControl.LocalPlayer))
                new CustomMessage(ModTranslation.GetString("ClergyEffectMessage"), EffectDuration);
        }
    }
}