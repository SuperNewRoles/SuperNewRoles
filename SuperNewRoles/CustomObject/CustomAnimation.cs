using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SuperNewRoles.Roles;
using UnityEngine;

namespace SuperNewRoles.CustomObject;
public struct CustomAnimationOptions
{
    public bool PlayEndDestory { get; private set; }
    public Sprite[] Sprites { get; private set; }
    public int frameRate { get; private set; }
    public bool IsLoop { get; private set; }
    public bool IsMeetingDestroy { get; private set; }
    public AudioClip EffectSound { get; private set; }
    public bool IsEffectSoundLoop { get; private set; }
    public float UpdateTime { get; private set; }
    public Action<CustomAnimation, CustomAnimationOptions> OnEndAnimation { get; private set; }
    public CustomAnimationOptions()
    {
        Sprites = new Sprite[] { };
    }
    public CustomAnimationOptions(Sprite[] sprites, int framerate, bool loop = false, bool playenddestroy = false, Action<CustomAnimation, CustomAnimationOptions> OnEndAnimation = null, bool IsMeetingDestroy = true, AudioClip EffectSound = null, bool IsEffectSoundLoop = false)
    {
        Sprites = sprites;
        frameRate = framerate;
        IsLoop = loop;
        PlayEndDestory = playenddestroy;
        this.OnEndAnimation = OnEndAnimation;
        this.IsMeetingDestroy = IsMeetingDestroy;
        this.EffectSound = EffectSound;
        UpdateTime = 1.0f / frameRate;
    }
    public void SetPlayEndDestroy(bool Is)
    {
        PlayEndDestory = Is;
    }
    public void SetSprites(Sprite[] sprites, bool? IsLoop = null, int? frameRate = null)
    {
        Sprites = sprites;
        if (IsLoop != null)
            this.IsLoop = IsLoop.Value;
        if (frameRate != null)
        {
            this.frameRate = frameRate.Value;
            UpdateTime = 1.0f / this.frameRate;
        }
    }
    public void SetEffectSound(AudioClip clip, bool IsLoop)
    {
        EffectSound = clip;
        IsEffectSoundLoop = IsLoop;
    }
    public void SetOnEndAnimation(Action<CustomAnimation, CustomAnimationOptions> newAnim)
    {
        this.OnEndAnimation = newAnim;
    }
}
public class CustomAnimation : MonoBehaviour
{
    public static List<CustomAnimation> CustomAnimations = new();
    public CustomAnimation(IntPtr intPtr) : base(intPtr)
    {
    }
    public SpriteRenderer spriteRenderer;
    public bool Playing;
    public CustomAnimationOptions Options;
    private float UpdateTimer;
    public int Index { get; private set; }
    public AudioSource audioSource;
    public bool IsRewinding { get; private set; }
    public static Sprite[] GetSprites(string path, int Count, int Digits = 4)
    {
        Sprite[] Sprites = new Sprite[Count];
        for (int i = 1; i < Count + 1; i++)
        {
            int zerodigts = Digits - ModHelpers.GetDigit(i);
            if (zerodigts < 0)
            {
                Logger.Info("Digitsミスってね？直して～～～");
                return Sprites.ToArray();
            }
            string countdata = ModHelpers.GetStringByCount('0', zerodigts) + i.ToString();
            Sprites[i - 1] = ModHelpers.LoadSpriteFromResources(path + "_" + countdata + ".png", 110f);
        }
        return Sprites.ToArray();
    }
    public virtual void Awake()
    {
        spriteRenderer = gameObject.GetOrAddComponent<SpriteRenderer>();
        Playing = false;
        CustomAnimations.Add(this);
    }
    public virtual void Init(CustomAnimationOptions option)
    {
        Options = option;
        spriteRenderer.sprite = option.Sprites[0];
        Index = 1;
        UpdateTimer = Options.UpdateTime;
        Play();
    }
    public virtual void Play(bool IsPlayMusic = true)
    {
        Playing = true;
        if (IsPlayMusic)
        {
            if (audioSource != null)
                audioSource.Stop();
            if (Options.EffectSound != null)
                audioSource = SoundManager.Instance.PlaySound(Options.EffectSound, Options.IsEffectSoundLoop, audioMixer: SoundManager.Instance.sfxMixer);
        }
        IsRewinding = false;
    }
    public virtual void Pause(bool IsStopMusic = true)
    {
        Playing = false;
        if (IsStopMusic)
            audioSource?.Pause();
        IsRewinding = false;
    }
    public virtual void Stop(bool IsStopMusic = true)
    {
        Pause(IsStopMusic: IsStopMusic);
        spriteRenderer.sprite = Options.Sprites[0];
        Index = 1;
        UpdateTimer = Options.UpdateTime;
    }
    public virtual void Update()
    {
        if (Options.IsMeetingDestroy && RoleClass.IsMeeting)
        {
            GameObject.Destroy(this.gameObject);
            return;
        }
        if (Playing && Options.Sprites.Length > 1)
        {
            if (IsRewinding)
            {
                UpdateTimer += Time.deltaTime;
                if (UpdateTimer >= Options.UpdateTime)
                {
                    spriteRenderer.sprite = Options.Sprites[Index - 1];
                    Index--;
                    if (Index <= 0)
                    {
                        if (Options.IsLoop)
                            Index = Options.Sprites.Length;
                        else
                        {
                            if (Options.PlayEndDestory)
                                Destroy(this.gameObject);
                            else
                                Playing = false;
                        }
                        if (Options.OnEndAnimation != null)
                            Options.OnEndAnimation(this, Options);
                    }
                    UpdateTimer = 0f;
                    OnRenderUpdate();
                }
            }
            else
            {
                UpdateTimer -= Time.deltaTime;
                if (UpdateTimer <= 0)
                {
                    spriteRenderer.sprite = Options.Sprites[Index];
                    Index++;
                    if (Options.Sprites.Length <= Index)
                    {
                        if (Options.IsLoop)
                            Index = 0;
                        else
                        {
                            if (Options.PlayEndDestory)
                                Destroy(this.gameObject);
                            else
                                Playing = false;
                        }
                        if (Options.OnEndAnimation != null)
                            Options.OnEndAnimation(this, Options);
                    }
                    UpdateTimer = Options.UpdateTime;
                    OnRenderUpdate();
                }
            }
        }
    }
    public virtual void OnDestroy()
    {
        CustomAnimations.Remove(this);
        if (audioSource != null)
            audioSource.Stop();
    }
    public virtual void OnRenderUpdate()
    {

    }
    public virtual void OnPlayRewind()
    {
        IsRewinding = true;
    }
}