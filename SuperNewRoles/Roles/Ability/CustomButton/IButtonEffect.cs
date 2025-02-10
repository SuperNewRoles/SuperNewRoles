using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SuperNewRoles.Roles.Ability.CustomButton;

internal interface IButtonEffect
{
    public bool isEffectActive { get; set; }
    protected Action OnEffectEnds { get; }
    public bool effectCancellable { get; }
    public float EffectDuration { get; }
    public bool IsEffectDurationInfinity { get; }
    public float FillUpTime { get; }
    private static readonly Color color = new(0F, 0.8F, 0F);
    float EffectTimer { get; set; }

    public void OnClick(ActionButton actionButton)
    {
        if (this.isEffectActive)
        {
            this.isEffectActive = false;
        }
        else
        {
            this.EffectTimer = IsEffectDurationInfinity ? 0f : EffectDuration;
            actionButton.cooldownTimerText.color = color;
            this.isEffectActive = true;
        }
    }

    public void OnFixedUpdate(ActionButton actionButton)
    {
        if (EffectTimer >= 0)
        {
            if (isEffectActive) EffectTimer -= Time.deltaTime;
        }
        if (EffectTimer <= 0 && isEffectActive)
        {
            actionButton.cooldownTimerText.color = Palette.EnabledColor;
            if (!IsEffectDurationInfinity || !effectCancellable)
            {
                isEffectActive = false;
                OnEffectEnds();
            }
        }
        this.DoEffect(actionButton);

        if (isEffectActive) actionButton.SetCoolDown(EffectTimer, IsEffectDurationInfinity ? 0f : EffectDuration);
    }

    public virtual void DoEffect(ActionButton actionButton, float effectStartTime = 3f)
    {
        //以下はFillup。もし別のeffectにしたくなったらoverrideして自分でなんとかする。
        if (actionButton.isCoolingDown && EffectTimer < effectStartTime)
        {
            actionButton.graphic.transform.localPosition = actionButton.position + (Vector3)UnityEngine.Random.insideUnitCircle * 0.05f;
        }
        else
        {
            actionButton.graphic.transform.localPosition = actionButton.position;
        }
    }
}