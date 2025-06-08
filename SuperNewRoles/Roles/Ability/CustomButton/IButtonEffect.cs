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
    public abstract Action OnEffectEnds { get; }
    public abstract float EffectDuration { get; }
    public virtual bool effectCancellable => false;
    public virtual bool IsEffectDurationInfinity => false;
    public virtual float FillUpTime => 0f;
    public virtual bool doAdditionalEffect => true;

    public static readonly Color color = new(0F, 0.8F, 0F);
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
    public virtual void OnCancel(ActionButton actionButton)
    {
        if (isEffectActive)
        {
            isEffectActive = false;
            actionButton.cooldownTimerText.color = Palette.EnabledColor;
            OnEffectEnds();
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

    public virtual bool IsEffectAvailable() => true;

    public virtual void DoEffect(ActionButton actionButton, float effectStartTime = 3f)
    {
        //以下はFillup。もし別のeffectにしたくなったらoverrideして自分でなんとかする。
        if (isEffectActive && actionButton.isCoolingDown && EffectTimer < effectStartTime && doAdditionalEffect)
        {
            actionButton.graphic.transform.localPosition = actionButton.position + (Vector3)UnityEngine.Random.insideUnitCircle * 0.05f;
        }
        else
        {
            actionButton.graphic.transform.localPosition = actionButton.position;
        }
    }
}