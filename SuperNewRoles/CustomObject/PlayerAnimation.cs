using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using Hazel;
using SuperNewRoles.Helpers;
using UnityEngine;

namespace SuperNewRoles.CustomObject
{
    public enum RpcAnimationType
    {
        Stop,
        SluggerCharge,
        SluggerMurder
    }
    public class PlayerAnimation
    {
        public static List<PlayerAnimation> PlayerAnimations = new();
        public PlayerControl Player;
        public PlayerPhysics Physics;
        public byte PlayerId;
        public GameObject gameObject;
        public Transform transform;
        public SpriteRenderer SpriteRender;
        public static PlayerAnimation GetPlayerAnimation(byte PlayerId)
        {
            return PlayerAnimations.Find(x => x.PlayerId == PlayerId);
        }
        public PlayerAnimation(PlayerControl Player)
        {
            if (PlayerAnimations.FindAll(x => x.Player != null).Count <= 0) PlayerAnimations = new();
            this.Player = Player;
            if (Player == null)
            {
                Logger.Error($"Playerがnullでした", "PlayerAnimation");
                return;
            }
            this.Physics = Player.MyPhysics;
            this.PlayerId = Player.PlayerId;
            this.gameObject = new("PlayerAnimation");
            this.transform = this.gameObject.transform;
            this.transform.SetParent(Player.transform.FindChild("Sprite"));
            this.SpriteRender = this.gameObject.AddComponent<SpriteRenderer>();
            PlayerAnimations.Add(this);
        }
        public void OnDestroy()
        {
            if (CachedPlayer.LocalPlayer == null || this.PlayerId == CachedPlayer.LocalPlayer.PlayerId)
            {
                PlayerAnimations = new();
                return;
            }
            PlayerAnimations.Remove(this);
        }
        public bool Playing = false;
        public bool IsLoop;
        public float Framerate;
        private float updatetime;
        private float updatedefaulttime;
        private int index;
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
            this.Sprites = sprites;
            this.IsLoop = isLoop;
            this.Playing = true;
            this.Framerate = framerate;
            this.updatedefaulttime = 1 / framerate;
            this.updatetime = this.updatedefaulttime;
            this.index = 0;
            this.OnAnimationEnd = onAnimationEnd;
            this.OnFixedUpdate = onFixedUpdate;
            this.SpriteRender.sprite = sprites[0];
        }
        public static void FixedAllUpdate()
        {
            foreach (PlayerAnimation Anim in PlayerAnimations.ToArray())
            {
                Anim.FixedUpdate();
            }
        }
        public void FixedUpdate()
        {
            if (this.SpriteRender == null)
            {
                this.OnDestroy();
                return;
            }
            if (!this.Playing)
            {
                this.SpriteRender.sprite = null;
                return;
            }
            this.updatetime -= Time.fixedDeltaTime;
            this.OnFixedUpdate?.Invoke();
            if (this.updatetime <= 0)
            {
                this.index++;
                if (this.Sprites.Length <= this.index)
                {
                    if (this.IsLoop)
                    {
                        this.index = 0;
                    }
                    else
                    {
                        this.Playing = false;
                        Logger.Info($"チェック:{this.OnAnimationEnd != null}");
                        this.OnAnimationEnd?.Invoke();
                        return;
                    }
                }
                this.SpriteRender.sprite = this.Sprites[this.index];
                this.updatetime = this.updatedefaulttime;
            }
        }
        public void RpcAnimation(RpcAnimationType AnimType)
        {
            MessageWriter writer = RPCHelper.StartRPC(CustomRPC.PlayPlayerAnimation);
            writer.Write(this.PlayerId);
            writer.Write((byte)AnimType);
            writer.EndRPC();
            this.HandleAnim(AnimType);
        }
        public void HandleAnim(RpcAnimationType AnimType)
        {
            switch (AnimType)
            {
                case RpcAnimationType.Stop:
                    this.Playing = false;
                    this.SpriteRender.sprite = null;
                    if (this.SoundManagerSource != null) this.SoundManagerSource.Stop();
                    break;
                case RpcAnimationType.SluggerCharge:
                    this.Playing = true;
                    SluggerChargeCreateAnimation();
                    void SluggerChargeCreateAnimation()
                    {
                        if (this.Player.IsDead() || !this.Player.IsRole(RoleId.Slugger) || !this.Playing)
                        {
                            this.Playing = false;
                            this.SpriteRender.sprite = null;
                            return;
                        }
                        this.Init(GetSprites("SuperNewRoles.Resources.harisen.tame_", 4), false, 12, new(() =>
                        {
                            SluggerChargeCreateAnimation();
                        }), new(() =>
                        {
                            this.transform.localScale = new(this.Physics.FlipX ? 1 : -1, 1, 1);
                            this.transform.localPosition = new(this.Physics.FlipX ? -0.75f : 0.75f, 0, -1);
                            if (this.SoundManagerSource == null)
                            {
                                if (Vector2.Distance(CachedPlayer.LocalPlayer.transform.position, this.transform.position) <= 5f)
                                {
                                    this.SoundManagerSource = SoundManager.Instance.PlaySound(ModHelpers.loadAudioClipFromResources("SuperNewRoles.Resources.harisen.Charge.raw"), false);
                                }
                            }
                            else
                            {
                                if (!this.SoundManagerSource.isPlaying)
                                {
                                    if (Vector2.Distance(CachedPlayer.LocalPlayer.transform.position, this.transform.position) <= 5f)
                                    {
                                        this.SoundManagerSource = SoundManager.Instance.PlaySound(ModHelpers.loadAudioClipFromResources("SuperNewRoles.Resources.harisen.Charge.raw"), false);
                                    }
                                }
                            }

                        }));
                    }
                    break;
                case RpcAnimationType.SluggerMurder:
                    this.Init(GetSprites("SuperNewRoles.Resources.harisen.harisen_", 8), false, 40);
                    this.OnAnimationEnd = new(() =>
                    {
                        this.Init(GetSprites("SuperNewRoles.Resources.harisen.harisen_", 1, start: 9), false, 8);
                        this.OnAnimationEnd = new(() =>
                        {
                            this.Init(GetSprites("SuperNewRoles.Resources.harisen.harisen_", 2, start: 10), false, 40);
                            this.OnFixedUpdate = new(() =>
                            {
                                this.transform.localScale = new(this.Physics.FlipX ? 1 : -1, 1, 1);
                                this.transform.localPosition = new(this.Physics.FlipX ? -0.75f : 0.75f, 0, -1);
                            });
                        });
                        this.OnFixedUpdate = new(() =>
                        {
                            this.transform.localScale = new(this.Physics.FlipX ? 1 : -1, 1, 1);
                            this.transform.localPosition = new(this.Physics.FlipX ? -0.75f : 0.75f, 0, -1);
                        });
                    });
                    this.OnFixedUpdate = new(() =>
                    {
                        this.transform.localScale = new(this.Physics.FlipX ? 1 : -1, 1, 1);
                        this.transform.localPosition = new(this.Physics.FlipX ? -0.75f : 0.75f, 0, -1);
                    });
                    if (Vector2.Distance(CachedPlayer.LocalPlayer.transform.position, this.transform.position) <= 5f)
                    {
                        SoundManager.Instance.PlaySound(ModHelpers.loadAudioClipFromResources("SuperNewRoles.Resources.harisen.Hit.raw"), false, 1.5f);
                    }
                    break;
            }
        }
    }
}