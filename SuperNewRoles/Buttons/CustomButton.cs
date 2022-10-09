using System;
using System.Collections.Generic;
using HarmonyLib;

using UnityEngine;
using UnityEngine.UI;

namespace SuperNewRoles.Buttons
{
    public class CustomButton
    {
        public static List<CustomButton> buttons = new();
        public static List<CustomButton> currentButtons
        {
            get
            {
                RoleId Role = PlayerControl.LocalPlayer.GetRole();
                bool IsAlive = CachedPlayer.LocalPlayer.IsAlive();
                return buttons.FindAll(x => x.HasButton(IsAlive, Role));
            }
        }
        public ActionButton actionButton;
        public Vector3 PositionOffset;
        public Vector3 LocalScale = Vector3.one;
        public float MaxTimer = float.MaxValue;
        public float Timer = 0f;
        public bool effectCancellable = false;
        private readonly Action OnClick;
        private readonly Action OnMeetingEnds;
        private readonly Func<bool, RoleId, bool> HasButton;
        private readonly Func<bool> CouldUse;
        public readonly Action OnEffectEnds;
        public bool HasEffect;
        public bool isEffectActive = false;
        public bool showButtonText = true;
        public string buttonText = null;
        public float EffectDuration;
        public Sprite Sprite;
        public Color? color;
        private readonly HudManager hudManager;
        private readonly bool mirror;
        private readonly KeyCode? hotkey;
        private readonly int joystickkey;
        private readonly Func<bool> StopCountCool;
        public CustomButton(Action OnClick, Func<bool, RoleId, bool> HasButton, Func<bool> CouldUse, Action OnMeetingEnds, Sprite Sprite, Vector3 PositionOffset, HudManager hudManager, ActionButton textTemplate, KeyCode? hotkey, int joystickkey, Func<bool> StopCountCool, bool HasEffect, float EffectDuration, Action OnEffectEnds, bool mirror = false, string buttonText = "", Color? color = null)
        {
            this.hudManager = hudManager;
            this.OnClick = OnClick;
            this.HasButton = HasButton;
            this.CouldUse = CouldUse;
            this.PositionOffset = PositionOffset;
            this.OnMeetingEnds = OnMeetingEnds;
            this.HasEffect = HasEffect;
            this.EffectDuration = EffectDuration;
            this.OnEffectEnds = OnEffectEnds;
            this.Sprite = Sprite;
            this.mirror = mirror;
            this.hotkey = hotkey;
            this.joystickkey = joystickkey;
            this.buttonText = buttonText;
            this.StopCountCool = StopCountCool;
            this.color = color;
            this.Timer = 16.2f;
            buttons.Add(this);
            this.actionButton = UnityEngine.Object.Instantiate(textTemplate, textTemplate.transform.parent);
            PassiveButton button = this.actionButton.GetComponent<PassiveButton>();
            button.OnClick = new Button.ButtonClickedEvent();
            button.Colliders = new Collider2D[] { button.GetComponent<BoxCollider2D>() };

            button.OnClick.AddListener((UnityEngine.Events.UnityAction)(() => this.OnClickEvent()));

            this.LocalScale = this.actionButton.transform.localScale;
            if (textTemplate)
            {
                UnityEngine.Object.Destroy(this.actionButton.buttonLabelText);
                this.actionButton.buttonLabelText = UnityEngine.Object.Instantiate(textTemplate.buttonLabelText, this.actionButton.transform);
            }
            this.SetActive(false);
        }
        public CustomButton(Action OnClick, Func<bool, RoleId, bool> HasButton, Func<bool> CouldUse, Action OnMeetingEnds, Sprite Sprite, Vector3 PositionOffset, HudManager hudManager, ActionButton textTemplate, KeyCode? hotkey, int joystickkey, Func<bool> StopCountCool, bool mirror = false, string buttonText = "", Color? color = null)
        : this(OnClick, HasButton, CouldUse, OnMeetingEnds, Sprite, PositionOffset, hudManager, textTemplate, hotkey, joystickkey, StopCountCool, false, 0f, () => { }, mirror, buttonText, color) { }

        void OnClickEvent()
        {
            if ((this.Timer < 0f && this.CouldUse() && CachedPlayer.LocalPlayer.PlayerControl.CanMove) || (this.HasEffect && this.isEffectActive && this.effectCancellable))
            {
                this.actionButton.graphic.color = new Color(1f, 1f, 1f, 0.3f);
                this.OnClick();

                if (this.isEffectActive)
                {
                    this.isEffectActive = false;
                    return;
                }
                if (this.HasEffect && !this.isEffectActive)
                {
                    this.Timer = this.EffectDuration;
                    this.actionButton.cooldownTimerText.color = new Color(0F, 0.8F, 0F);
                    this.isEffectActive = true;
                }
            }
        }

        public static void HudUpdate()
        {
            buttons.RemoveAll(item => item.actionButton == null);

            bool isAlive = PlayerControl.LocalPlayer.IsAlive();
            RoleId role = PlayerControl.LocalPlayer.GetRole();
            foreach (CustomButton btn in buttons)
            {
                try
                {
                    btn.Update(isAlive, role);
                }
                catch (Exception e)
                {
                    System.Console.WriteLine("ButtonError:" + e);
                }
            }
        }

        public static void MeetingEndedUpdate()
        {
            buttons.RemoveAll(item => item.actionButton == null);
            bool isAlive = PlayerControl.LocalPlayer.IsAlive();
            RoleId role = PlayerControl.LocalPlayer.GetRole();
            foreach (CustomButton btn in buttons)
            {
                try
                {
                    if (btn.HasButton(isAlive, role))
                    {
                        btn.OnMeetingEnds();
                        btn.Update(isAlive, role);
                    }
                }
                catch (Exception e)
                {
                    if (ConfigRoles.DebugMode.Value) System.Console.WriteLine("MeetingEnd_ButtonError:" + e);
                }
            }
        }

        public void SetActive(bool isActive)
        {
            if (isActive)
            {
                this.actionButton.gameObject.SetActive(true);
                this.actionButton.graphic.enabled = true;
            }
            else
            {
                this.actionButton.gameObject.SetActive(false);
                this.actionButton.graphic.enabled = false;
            }
        }

        private void Update(bool isAlive, RoleId role)
        {
            CachedPlayer localPlayer = CachedPlayer.LocalPlayer;
            bool moveable = localPlayer.PlayerControl.moveable;

            if (localPlayer.Data == null || MeetingHud.Instance || ExileController.Instance || !this.HasButton(isAlive, role))
            {
                this.SetActive(false);
                return;
            }
            this.SetActive(this.hudManager.UseButton.isActiveAndEnabled);

            this.actionButton.graphic.sprite = this.Sprite;
            if (this.showButtonText && this.buttonText != "")
            {
                this.actionButton.OverrideText(this.buttonText);
            }
            this.actionButton.buttonLabelText.enabled = this.showButtonText; // Only show the text if it's a kill button

            if (this.hudManager.UseButton != null)
            {
                Vector3 pos = this.hudManager.UseButton.transform.localPosition;
                if (this.mirror) pos = new Vector3(-pos.x, pos.y, pos.z);
                this.actionButton.transform.localPosition = pos + this.PositionOffset;
                if (PlayerControl.LocalPlayer.IsRole(RoleId.GM))
                {
                    this.actionButton.transform.localScale = new(0.7f, 0.7f, 0.7f);
                }
                else
                {
                    if (currentButtons.Count <= 1)
                    {
                        if (this.actionButton is KillButton)
                        {
                            this.actionButton.transform.localPosition = FastDestroyableSingleton<HudManager>.Instance.KillButton.transform.localPosition;
                            this.actionButton.transform.localScale = FastDestroyableSingleton<HudManager>.Instance.KillButton.transform.localScale;
                        }
                        else
                        {
                            this.actionButton.transform.localPosition = FastDestroyableSingleton<HudManager>.Instance.AbilityButton.transform.localPosition;
                            this.actionButton.transform.localScale = FastDestroyableSingleton<HudManager>.Instance.AbilityButton.transform.localScale;
                        }
                    }
                    else if (currentButtons.Count == 2)
                    {
                        if (currentButtons[0] == this)
                        {
                            if (this.actionButton is KillButton)
                            {
                                this.actionButton.transform.localPosition = FastDestroyableSingleton<HudManager>.Instance.KillButton.transform.localPosition;
                                this.actionButton.transform.localScale = FastDestroyableSingleton<HudManager>.Instance.KillButton.transform.localScale;

                            }
                            else
                            {
                                this.actionButton.transform.localPosition = FastDestroyableSingleton<HudManager>.Instance.AbilityButton.transform.localPosition;
                                this.actionButton.transform.localScale = FastDestroyableSingleton<HudManager>.Instance.AbilityButton.transform.localScale;
                            }
                        }
                        else if (currentButtons[1] == this)
                        {
                            if (currentButtons[0].actionButton is KillButton)
                            {
                                this.actionButton.transform.localPosition = FastDestroyableSingleton<HudManager>.Instance.AbilityButton.transform.localPosition;
                                this.actionButton.transform.localScale = FastDestroyableSingleton<HudManager>.Instance.AbilityButton.transform.localScale;
                            }
                            else
                            {
                                Vector3 poss = FastDestroyableSingleton<HudManager>.Instance.AbilityButton.transform.localPosition;
                                poss.x -= 1.5f;
                                poss.y -= 1.5f;
                                this.actionButton.transform.localPosition = poss;
                                this.actionButton.transform.localScale = FastDestroyableSingleton<HudManager>.Instance.AbilityButton.transform.localScale;
                            }
                        }
                    }
                }
            }
            if (this.CouldUse())
            {
                this.actionButton.graphic.color = this.actionButton.buttonLabelText.color = Palette.EnabledColor;
                this.actionButton.graphic.material.SetFloat("_Desat", 0f);
            }
            else
            {
                this.actionButton.graphic.color = this.actionButton.buttonLabelText.color = Palette.DisabledClear;
                this.actionButton.graphic.material.SetFloat("_Desat", 1f);
            }

            if (this.color != null)
            {
                this.actionButton.graphic.color = (Color)this.color;
            }

            if (this.Timer >= 0)
            {
                if ((this.HasEffect && this.isEffectActive) ||
                    (!localPlayer.PlayerControl.inVent && moveable && !this.StopCountCool()))
                    this.Timer -= Time.deltaTime;
            }

            if (this.Timer <= 0 && this.HasEffect && this.isEffectActive)
            {
                this.isEffectActive = false;
                this.actionButton.cooldownTimerText.color = Palette.EnabledColor;
                this.OnEffectEnds();
            }

            this.actionButton.SetCoolDown(this.Timer, (this.HasEffect && this.isEffectActive) ? this.EffectDuration : this.MaxTimer);
            // Trigger OnClickEvent if the hotkey is being pressed down
            if ((this.hotkey.HasValue && Input.GetButtonDown(this.hotkey.Value.ToString())) || ConsoleJoystick.player.GetButtonDown(this.joystickkey)) this.OnClickEvent();
        }
    }
}