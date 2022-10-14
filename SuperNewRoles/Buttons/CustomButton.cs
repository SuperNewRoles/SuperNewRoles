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
            Timer = 16.2f;
            buttons.Add(this);
            actionButton = UnityEngine.Object.Instantiate(textTemplate, textTemplate.transform.parent);
            PassiveButton button = actionButton.GetComponent<PassiveButton>();
            button.OnClick = new Button.ButtonClickedEvent();
            button.Colliders = new Collider2D[] { button.GetComponent<BoxCollider2D>() };

            button.OnClick.AddListener((UnityEngine.Events.UnityAction)(() => OnClickEvent()));

            LocalScale = actionButton.transform.localScale;
            if (textTemplate)
            {
                UnityEngine.Object.Destroy(actionButton.buttonLabelText);
                actionButton.buttonLabelText = UnityEngine.Object.Instantiate(textTemplate.buttonLabelText, actionButton.transform);
            }
            SetActive(false);
        }
        public CustomButton(Action OnClick, Func<bool, RoleId, bool> HasButton, Func<bool> CouldUse, Action OnMeetingEnds, Sprite Sprite, Vector3 PositionOffset, HudManager hudManager, ActionButton textTemplate, KeyCode? hotkey, int joystickkey, Func<bool> StopCountCool, bool mirror = false, string buttonText = "", Color? color = null)
        : this(OnClick, HasButton, CouldUse, OnMeetingEnds, Sprite, PositionOffset, hudManager, textTemplate, hotkey, joystickkey, StopCountCool, false, 0f, () => { }, mirror, buttonText, color) { }

        void OnClickEvent()
        {
            if ((this.Timer <= 0f && CouldUse()) || (this.HasEffect && this.isEffectActive && this.effectCancellable))
            {
                actionButton.graphic.color = new Color(1f, 1f, 1f, 0.3f);
                this.OnClick();

                if (this.isEffectActive)
                {
                    this.isEffectActive = false;
                    return;
                }
                if (this.HasEffect && !this.isEffectActive)
                {
                    this.Timer = this.EffectDuration;
                    actionButton.cooldownTimerText.color = new Color(0F, 0.8F, 0F);
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
                actionButton.gameObject.SetActive(true);
                actionButton.graphic.enabled = true;
            }
            else
            {
                actionButton.gameObject.SetActive(false);
                actionButton.graphic.enabled = false;
            }
        }

        private void Update(bool isAlive, RoleId role)
        {
            var localPlayer = CachedPlayer.LocalPlayer;
            var moveable = localPlayer.PlayerControl.moveable;

            if (localPlayer.Data == null || MeetingHud.Instance || ExileController.Instance || !HasButton(isAlive, role))
            {
                SetActive(false);
                return;
            }
            SetActive(hudManager.UseButton.isActiveAndEnabled);

            actionButton.graphic.sprite = Sprite;
            if (showButtonText && buttonText != "")
            {
                actionButton.OverrideText(buttonText);
            }
            actionButton.buttonLabelText.enabled = showButtonText; // Only show the text if it's a kill button

            if (hudManager.UseButton != null)
            {
                Vector3 pos = hudManager.UseButton.transform.localPosition;
                if (mirror) pos = new Vector3(-pos.x, pos.y, pos.z);
                actionButton.transform.localPosition = pos + PositionOffset;
                if (PlayerControl.LocalPlayer.IsRole(RoleId.GM))
                {
                    actionButton.transform.localScale = new(0.7f, 0.7f, 0.7f);
                }
                else
                {
                    if (OldModeButtons.IsOldMode)
                    {
                        if (currentButtons.Count <= 1)
                        {
                            if (actionButton is KillButton)
                            {
                                actionButton.transform.localPosition = FastDestroyableSingleton<HudManager>.Instance.KillButton.transform.localPosition;
                                actionButton.transform.localScale = FastDestroyableSingleton<HudManager>.Instance.KillButton.transform.localScale;
                            }
                            else
                            {
                                actionButton.transform.localPosition = FastDestroyableSingleton<HudManager>.Instance.AbilityButton.transform.localPosition;
                                actionButton.transform.localScale = FastDestroyableSingleton<HudManager>.Instance.AbilityButton.transform.localScale;
                            }
                        }
                        else if (currentButtons.Count == 2)
                        {
                            if (currentButtons[0] == this)
                            {
                                if (actionButton is KillButton)
                                {
                                    actionButton.transform.localPosition = FastDestroyableSingleton<HudManager>.Instance.KillButton.transform.localPosition;
                                    actionButton.transform.localScale = FastDestroyableSingleton<HudManager>.Instance.KillButton.transform.localScale;

                                }
                                else
                                {
                                    actionButton.transform.localPosition = FastDestroyableSingleton<HudManager>.Instance.AbilityButton.transform.localPosition;
                                    actionButton.transform.localScale = FastDestroyableSingleton<HudManager>.Instance.AbilityButton.transform.localScale;
                                }
                            }
                            else if (currentButtons[1] == this)
                            {
                                if (currentButtons[0].actionButton is KillButton)
                                {
                                    actionButton.transform.localPosition = FastDestroyableSingleton<HudManager>.Instance.AbilityButton.transform.localPosition;
                                    actionButton.transform.localScale = FastDestroyableSingleton<HudManager>.Instance.AbilityButton.transform.localScale;
                                }
                                else
                                {
                                    Vector3 poss = FastDestroyableSingleton<HudManager>.Instance.AbilityButton.transform.localPosition;
                                    poss.x -= 1.5f;
                                    poss.y -= 1.5f;
                                    actionButton.transform.localPosition = poss;
                                    actionButton.transform.localScale = FastDestroyableSingleton<HudManager>.Instance.AbilityButton.transform.localScale;
                                }
                            }
                        }
                    }
                }
            }
            if (CouldUse())
            {
                actionButton.graphic.color = actionButton.buttonLabelText.color = Palette.EnabledColor;
                actionButton.graphic.material.SetFloat("_Desat", 0f);
            }
            else
            {
                actionButton.graphic.color = actionButton.buttonLabelText.color = Palette.DisabledClear;
                actionButton.graphic.material.SetFloat("_Desat", 1f);
            }

            if (color != null)
            {
                actionButton.graphic.color = (Color)color;
            }

            if (Timer >= 0)
            {
                if ((HasEffect && isEffectActive) ||
                    (!localPlayer.PlayerControl.inVent && moveable && !StopCountCool()))
                    Timer -= Time.deltaTime;
            }

            if (Timer <= 0 && HasEffect && isEffectActive)
            {
                isEffectActive = false;
                actionButton.cooldownTimerText.color = Palette.EnabledColor;
                OnEffectEnds();
            }

            actionButton.SetCoolDown(Timer, (HasEffect && isEffectActive) ? EffectDuration : MaxTimer);
            // Trigger OnClickEvent if the hotkey is being pressed down
            if ((hotkey.HasValue && Input.GetButtonDown(hotkey.Value.ToString())) || ConsoleJoystick.player.GetButtonDown(joystickkey)) OnClickEvent();
        }
    }
}