using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using UnityEngine;

namespace SuperNewRoles.CustomObject
{
    public class PlayerAnimation
    {
        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
        public static class PlayerControlFixedUpdatePatch
        {
            public static void Postfix(PlayerControl __instance)
            {
                if (PlayerAnimation.GetPlayerAnimation(__instance.PlayerId) == null) new PlayerAnimation(__instance);
                if (__instance == PlayerControl.LocalPlayer)
                {
                    PlayerAnimation.FixedAllUpdate();
                }
                if (Input.GetKeyDown(KeyCode.M) && __instance == PlayerControl.LocalPlayer)
                {
                    var anim = PlayerAnimation.GetPlayerAnimation(__instance.PlayerId);
                    anim.Init(GetSprites("SuperNewRoles.Resources.harisen.harisen_", 11), false, 15);
                    anim.transform.localPosition = new(0.75f, 0, -1);
                    anim.transform.localScale = new(-1, 1, 1);
                }
            }
        }
        public static List<PlayerAnimation> PlayerAnimations = new();
        public PlayerControl Player;
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
                Logger.Error($"Playerがnullでした","PlayerAnimation");
                return;
            }
            PlayerId = Player.PlayerId;
            gameObject = new("PlayerAnimation");
            transform = gameObject.transform;
            transform.SetParent(Player.transform.FindChild("Sprite"));
            SpriteRender = gameObject.AddComponent<SpriteRenderer>();
            PlayerAnimations.Add(this);
        }
        public bool Playing = false;
        public bool IsLoop;
        public float Framerate;
        private float updatetime;
        private float updatedefaulttime;
        private int index;
        public Sprite[] Sprites;
        /// <summary>
        /// 指定したパスの連番のファイルを取得できます。
        /// </summary>
        /// <param name="path">そこまでのパス</param>
        /// <param name="num">連番数です。1～6だと5です。</param>
        /// <returns></returns>
        public static Sprite[] GetSprites(string path, int num, float pixelsPerUnit = 115f)
        {
            List<Sprite> Sprites = new();
            for (int i = 1; i < num + 1; i++)
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
                Sprites.Add(ModHelpers.LoadSpriteFromResources(path + countdata + ".png", pixelsPerUnit));
            }
            return Sprites.ToArray();
        }
        public void Init(Sprite[] sprites, bool isLoop, float framerate)
        {
            Logger.Info("いにっと");
            Sprites = sprites;
            IsLoop = isLoop;
            Playing = true;
            Framerate = framerate;
            updatedefaulttime = 1 / framerate;
            updatetime = updatedefaulttime;
            index = 0;
        }
        public static void FixedAllUpdate()
        {
            foreach (PlayerAnimation Anim in PlayerAnimations)
            {
                Anim.FixedUpdate();
            }
        }
        public void FixedUpdate()
        {
            if (!Playing) return;
            updatetime -= Time.fixedDeltaTime;
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
                        SpriteRender.sprite = null;
                        return;
                    }
                }
                SpriteRender.sprite = Sprites[index];
                updatetime = updatedefaulttime;
            }
        }
    }
}
