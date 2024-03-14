using System;
using System.Collections.Generic;
using Hazel;
using SuperNewRoles.Helpers;
using SuperNewRoles.Replay.ReplayActions;
using UnityEngine;

namespace SuperNewRoles.CustomObject;

public enum RpcAnimationType
{
    Stop,
    SluggerCharge,
    SluggerMurder,
    PushHand
}
public class PlayerAnimation
{
    public static Dictionary<byte, PlayerAnimation> PlayerAnimations = new();
    public PlayerControl Player;
    public PlayerPhysics Physics;
    public byte PlayerId;
    public GameObject gameObject;
    public Transform transform;
    public SpriteRenderer SpriteRender;
    public static PlayerAnimation GetPlayerAnimation(byte PlayerId)
    {
        return PlayerAnimations.TryGetValue(PlayerId, out PlayerAnimation anim) ? anim : null;
    }
    public static bool IsCreatedAnim(byte PlayerId)
    {
        return PlayerAnimations.ContainsKey(PlayerId);
    }
    public PlayerAnimation(PlayerControl Player)
    {
        if (PlayerAnimations.Values.FirstOrDefault(anim => anim.Player != null) == null) PlayerAnimations = new();
        this.Player = Player;
        if (Player == null)
        {
            Logger.Error($"Playerがnullでした", "PlayerAnimation");
            return;
        }
        Physics = Player.MyPhysics;
        PlayerId = Player.PlayerId;
        gameObject = new("PlayerAnimation");
        transform = gameObject.transform;
        transform.SetParent(Player.cosmetics.normalBodySprite.BodySprite.transform.parent);
        transform.localScale = Vector3.one * 0.5f;
        SpriteRender = gameObject.AddComponent<SpriteRenderer>();
        _defaultMaterial = SpriteRender.sharedMaterial;
        PlayerAnimations.Add(PlayerId, this);
        IsRewinding = false;
        IsPausing = false;
    }
    public void OnDestroy()
    {
        if (CachedPlayer.LocalPlayer == null || PlayerId == CachedPlayer.LocalPlayer.PlayerId)
        {
            PlayerAnimations = new();
            return;
        }
        PlayerAnimations.Remove(PlayerId);
    }
    public bool Playing = false;
    public bool IsLoop;
    public float Framerate;
    private float updatetime;
    private float updatedefaulttime;
    private int index;
    private bool IsRewinding;
    private bool IsPausing;
    private static Material _defaultMaterial;
    public Sprite[] Sprites;
    public Action OnAnimationEnd;
    public Action OnFixedUpdate;
    public AudioSource SoundManagerSource;
    /// <summary>
    /// 指定したパスの連番のファイルを取得できます。
    /// </summary>
    /// <param name="path">そこまでのパス</param>
    /// <param name="num">連番数です。1～6だと5です。</param>
    /// <returns></returns>
    public static Sprite[] GetSprites(string path, int num, float pixelsPerUnit = 115f, int start = 1)
    {
        List<Sprite> Sprites = new();
        for (int i = start; i < num + start; i++)
        {
            string countdata = "000" + i.ToString();
            if (i >= 10)
            {
                if (i >= 100)
                {
                    countdata = "0" + i.ToString();
                }
                else
                {
                    countdata = "00" + i.ToString();
                }
            }
            Logger.Info(path + countdata + ".png");
            Sprites.Add(ModHelpers.LoadSpriteFromResources(path + countdata + ".png", pixelsPerUnit));
        }
        return Sprites.ToArray();
    }
    public void Init(Sprite[] sprites, bool isLoop, float framerate, Action onAnimationEnd = null, Action onFixedUpdate = null)
    {
        Logger.Info("いにっと");
        Sprites = sprites;
        IsLoop = isLoop;
        Playing = true;
        Framerate = framerate;
        updatedefaulttime = 1 / framerate;
        updatetime = updatedefaulttime;
        index = 0;
        OnAnimationEnd = onAnimationEnd;
        OnFixedUpdate = onFixedUpdate;
        SpriteRender.sprite = sprites[0];
    }
    public virtual void OnPlayRewind()
    {
        IsPausing = false;
        IsRewinding = true;
        if (SoundManagerSource != null)
            SoundManagerSource.pitch = -1f;
    }
    public virtual void Play()
    {
        IsPausing = false;
        IsRewinding = false;
        if (SoundManagerSource != null)
            SoundManagerSource.pitch = 1f;
    }
    public virtual void Pause()
    {
        IsPausing = true;
        IsRewinding = false;
        if (SoundManagerSource != null)
            SoundManagerSource.pitch = 0f;
    }
    public static void FixedAllUpdate()
    {
        foreach (PlayerAnimation Anim in PlayerAnimations.Values)
        {
            Anim.FixedUpdate();
        }
    }
    public void FixedUpdate()
    {
        if (SpriteRender == null)
        {
            OnDestroy();
            return;
        }
        if (!Playing)
        {
            SpriteRender.sprite = null;
            return;
        }
        if (IsPausing)
        {
            return;
        }
        if (IsRewinding)
        {
            updatetime += Time.fixedDeltaTime;
            if (OnFixedUpdate != null) OnFixedUpdate();
            if (updatetime >= updatedefaulttime)
            {
                index--;
                if (index < 0)
                {
                    if (IsLoop)
                    {
                        index = Sprites.Length - 1;
                    }
                    else
                    {
                        Playing = false;
                        Logger.Info($"チェック:{OnAnimationEnd != null}");
                        if (OnAnimationEnd != null) OnAnimationEnd();
                        return;
                    }
                }
                SpriteRender.sprite = Sprites[index];
                updatetime = 0;
            }
        }
        else
        {
            updatetime -= Time.fixedDeltaTime;
            if (OnFixedUpdate != null) OnFixedUpdate();
            if (updatetime <= 0)
            {
                index++;
                if (Sprites.Length <= index)
                {
                    if (IsLoop)
                    {
                        index = 0;
                    }
                    else
                    {
                        Playing = false;
                        Logger.Info($"チェック:{OnAnimationEnd != null}");
                        if (OnAnimationEnd != null) OnAnimationEnd();
                        return;
                    }
                }
                SpriteRender.sprite = Sprites[index];
                updatetime = updatedefaulttime;
            }
        }
    }
    public void RpcAnimation(RpcAnimationType AnimType)
    {
        MessageWriter writer = RPCHelper.StartRPC(CustomRPC.PlayPlayerAnimation);
        writer.Write(PlayerId);
        writer.Write((byte)AnimType);
        writer.EndRPC();
        HandleAnim(AnimType);
    }
    private void SetColorMaterialActive(bool active)
    {
        if (active)
        {
            SpriteRender.sharedMaterial = FastDestroyableSingleton<HatManager>.Instance.PlayerMaterial;
            SpriteRender.maskInteraction = SpriteMaskInteraction.None;
            PlayerMaterial.SetMaskLayerBasedOnLocalPlayer(SpriteRender, false);

            PlayerMaterial.SetColors(Player.Data.DefaultOutfit.ColorId, SpriteRender);
        }
        else
        {
            SpriteRender.sharedMaterial = _defaultMaterial;
            SpriteRender.maskInteraction = SpriteMaskInteraction.None;
        }
    }
    public void HandleAnim(RpcAnimationType AnimType)
    {
        ReplayActionPlayerAnimation.Create(PlayerId, (byte)AnimType);
        switch (AnimType)
        {
            case RpcAnimationType.Stop:
                Playing = false;
                SpriteRender.sprite = null;
                if (SoundManagerSource != null) SoundManagerSource.Stop();
                break;
            case RpcAnimationType.SluggerCharge:
                Playing = true;
                SluggerChargeCreateAnimation();
                void SluggerChargeCreateAnimation()
                {
                    if (Player.IsDead() || !Player.IsRole(RoleId.Slugger) || !Playing)
                    {
                        Playing = false;
                        SpriteRender.sprite = null;
                        return;
                    }
                    Init(GetSprites("SuperNewRoles.Resources.harisen.tame_", 4), false, 12, new(() =>
                    {
                        Playing = true;
                        SluggerChargeCreateAnimation();
                    }), new(() =>
                    {
                        transform.localScale = new(Physics.FlipX ? 0.5f : -0.5f, 0.5f, 0.5f);
                        transform.localPosition = new(Physics.FlipX ? -0.45f : 0.45f, 0, -1);
                        if (SoundManagerSource == null)
                        {
                            if (Vector2.Distance(CachedPlayer.LocalPlayer.transform.position, transform.position) <= 5f)
                            {
                                SoundManagerSource = SoundManager.Instance.PlaySound(ModHelpers.loadAudioClipFromResources("SuperNewRoles.Resources.harisen.Charge.raw"), false);
                            }
                        }
                        else
                        {
                            if (!SoundManagerSource.isPlaying)
                            {
                                if (Vector2.Distance(CachedPlayer.LocalPlayer.transform.position, transform.position) <= 5f)
                                {
                                    SoundManagerSource = SoundManager.Instance.PlaySound(ModHelpers.loadAudioClipFromResources("SuperNewRoles.Resources.harisen.Charge.raw"), false);
                                }
                            }
                        }

                    }));
                }
                break;
            case RpcAnimationType.SluggerMurder:
                Init(GetSprites("SuperNewRoles.Resources.harisen.harisen_", 8), false, 40);
                OnAnimationEnd = new(() =>
                {
                    Init(GetSprites("SuperNewRoles.Resources.harisen.harisen_", 1, start: 9), false, 8);
                    OnAnimationEnd = new(() =>
                    {
                        Init(GetSprites("SuperNewRoles.Resources.harisen.harisen_", 2, start: 10), false, 40);
                        OnFixedUpdate = new(() =>
                        {
                            transform.localScale = new(Physics.FlipX ? 0.5f : -0.5f, 0.5f, 0.5f);
                            transform.localPosition = new(Physics.FlipX ? -0.45f : 0.45f, 0, -1);
                        });
                    });
                    OnFixedUpdate = new(() =>
                    {
                        transform.localScale = new(Physics.FlipX ? 0.5f : -0.5f, 0.5f, 0.5f);
                        transform.localPosition = new(Physics.FlipX ? -0.45f : 0.45f, 0, -1);
                    });
                });
                OnFixedUpdate = new(() =>
                {
                    transform.localScale = new(Physics.FlipX ? 0.5f : -0.5f, 0.5f, 0.5f);
                    transform.localPosition = new(Physics.FlipX ? -0.45f : 0.45f, 0, -1);
                });
                if (Vector2.Distance(CachedPlayer.LocalPlayer.transform.position, transform.position) <= 5f)
                {
                    SoundManager.Instance.PlaySound(ModHelpers.loadAudioClipFromResources("SuperNewRoles.Resources.harisen.Hit.raw"), false, 1.5f);
                }
                break;
            case RpcAnimationType.PushHand:
                SetColorMaterialActive(true);
                Init(GetSprites("SuperNewRoles.Resources.Pusher.pushanim_", 9, start: 6), false, 20);
                OnAnimationEnd = new(() =>
                {
                    new LateTask(() => SetColorMaterialActive(false), 0.035f);
                });
                OnFixedUpdate = new(() =>
                {
                    transform.localScale = new(Physics.FlipX ? 0.6f : -0.6f, 0.6f, 0.6f);
                    transform.localPosition = new(Physics.FlipX ? -0.4f : 0.4f, 0.05f, -1);
                });
                break;
        }
    }
}