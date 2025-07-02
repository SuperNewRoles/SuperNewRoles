using System;
using SuperNewRoles.Events;
using SuperNewRoles.Events.PCEvents;
using SuperNewRoles.Modules;
using SuperNewRoles.Modules.Events.Bases;
using TMPro;
using UnityEngine;

namespace SuperNewRoles.Roles.Ability;

public class SuicideTimerAbility : AbilityBase
{
    public Func<float> SuicideTimeGetter { get; }
    public Func<bool> ResetOnMeetingGetter { get; }
    private EventListener<MurderEventData> MurderEventListener;
    private EventListener<WrapUpEventData> WrapUpEventListener;
    private EventListener UpdateEventListener;
    private float timer;
    public float CurrentTimer => timer;
    private bool firstKilled = false;
    private TextMeshPro timerText;

    public SuicideTimerAbility(Func<float> suicideTimeGetter, Func<bool> resetOnMeetingGetter)
    {
        SuicideTimeGetter = suicideTimeGetter;
        ResetOnMeetingGetter = resetOnMeetingGetter;
    }

    public override void AttachToLocalPlayer()
    {
        MurderEventListener = MurderEvent.Instance.AddListener(OnMurder);
        WrapUpEventListener = WrapUpEvent.Instance.AddListener(OnWrapUp);
        UpdateEventListener = FixedUpdateEvent.Instance.AddListener(OnUpdate);
        // 初期状態でタイマーをセット
        ResetSuicideTimer();

        timerText = GameObject.Instantiate(FastDestroyableSingleton<HudManager>.Instance.KillButton.cooldownTimerText, FastDestroyableSingleton<HudManager>.Instance.KillButton.cooldownTimerText.transform.parent);
        timerText.text = "";
        timerText.enableWordWrapping = false;
        timerText.transform.localScale = Vector3.one * 0.5f;
        timerText.transform.localPosition += new Vector3(-0.05f, 0.7f, 0);
    }

    public override void DetachToLocalPlayer()
    {
        base.DetachToLocalPlayer();
        MurderEvent.Instance.RemoveListener(MurderEventListener);
        WrapUpEvent.Instance.RemoveListener(WrapUpEventListener);
        FixedUpdateEvent.Instance.RemoveListener(UpdateEventListener);
    }

    private void OnMurder(MurderEventData data)
    {
        if (Player == null) return;
        if (data.killer == null) return;
        if (Player.PlayerId == data.killer.PlayerId)
        {
            // キルが発生した時にタイマーをリセット
            ResetSuicideTimer();
            firstKilled = true;
        }
    }

    private void OnWrapUp(WrapUpEventData data)
    {
        if (Player == null) return;

        // 会議後にリセットする設定の場合
        if (ResetOnMeetingGetter())
        {
            new LateTask(() =>
            {
                ResetSuicideTimer();
            }, 0.5f, "SerialKiller ResetTimer After Meeting");
        }
    }

    private void ResetSuicideTimer()
    {
        // 新しいタイマーをセット
        float suicideTime = SuicideTimeGetter();
        timer = suicideTime;
    }

    private void OnUpdate()
    {
        if (MeetingHud.Instance != null || ExileController.Instance != null || !firstKilled)
            return;
        if (ExPlayerControl.LocalPlayer.IsDead())
        {
            if (timerText != null)
                GameObject.Destroy(timerText.gameObject);
            return;
        }
        timer -= Time.fixedDeltaTime;
        if (timer <= 0)
        {
            ExPlayerControl.LocalPlayer.RpcCustomDeath(CustomDeathType.Suicide);
        }
        if (timerText == null) return;
        timerText.text = string.Format(ModTranslation.GetString("SerialKillerSuicideText"), ((int)timer) + 1);
    }
}