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
            Physics = Player.MyPhysics;
            PlayerId = Player.PlayerId;
            gameObject = new("PlayerAnimation");
            transform = gameObject.transform;
            transform.SetParent(Player.transform.FindChild("Sprite"));
            SpriteRender = gameObject.AddComponent<SpriteRenderer>();
            PlayerAnimations.Add(this);
        }
        public void OnDestroy()
        {
            if (CachedPlayer.LocalPlayer == null || PlayerId == CachedPlayer.LocalPlayer.PlayerId)
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
        public static void FixedAllUpdate()
        {
            foreach (PlayerAnimation Anim in PlayerAnimations.ToArray())
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
                        if (OnAnimationEnd != null)
                        {
                            OnAnimationEnd();
                        }
                        return;
                    }
                }
                SpriteRender.sprite = Sprites[index];
                updatetime = updatedefaulttime;
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
        public void HandleAnim(RpcAnimationType AnimType)
        {
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
                            SluggerChargeCreateAnimation();
                        }), new(() =>
                        {
                            transform.localScale = new(Physics.FlipX ? 1 : -1, 1, 1);
                            transform.localPosition = new(Physics.FlipX ? -0.75f : 0.75f, 0, -1);
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
                                transform.localScale = new(Physics.FlipX ? 1 : -1, 1, 1);
                                transform.localPosition = new(Physics.FlipX ? -0.75f : 0.75f, 0, -1);
                            });
                        });
                        OnFixedUpdate = new(() =>
                        {
                            transform.localScale = new(Physics.FlipX ? 1 : -1, 1, 1);
                            transform.localPosition = new(Physics.FlipX ? -0.75f : 0.75f, 0, -1);
                        });
                    });
                    OnFixedUpdate = new(() =>
                    {
                        transform.localScale = new(Physics.FlipX ? 1 : -1, 1, 1);
                        transform.localPosition = new(Physics.FlipX ? -0.75f : 0.75f, 0, -1);
                    });
                    if (Vector2.Distance(CachedPlayer.LocalPlayer.transform.position, transform.position) <= 5f)
                    {
                        SoundManager.Instance.PlaySound(ModHelpers.loadAudioClipFromResources("SuperNewRoles.Resources.harisen.Hit.raw"), false, 1.5f);
                    }
                    break;
            }
        }
    }
}