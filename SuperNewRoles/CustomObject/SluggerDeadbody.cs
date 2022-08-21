using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using SuperNewRoles.Roles;
using UnityEngine;

namespace SuperNewRoles.CustomObject
{
    public class SluggerDeadbody
    {
        [HarmonyPatch(typeof(PlayerControl),nameof(PlayerControl.FixedUpdate))]
        class FixedUpdatePatch
        {
            public static void Postfix(PlayerControl __instance)
            {
                if (__instance == PlayerControl.LocalPlayer)
                {
                    AllFixedUpdate();
                }
            }
        }
        public static List<SluggerDeadbody> DeadBodys = new();
        public Vector2 Force;
        public PlayerControl Source
        {
            get
            {
                if (_source == null)
                {
                    _source = SourceId.GetPlayerControl();
                }
                return _source;
            }
            set
            {
                _source = value;
                SourceId = _source.PlayerId;
            }
        }
        private PlayerControl _source;
        public byte SourceId;
        public PlayerControl Player
        {
            get
            {
                if (_player == null)
                {
                    _player = PlayerId.GetPlayerControl();
                }
                return _player;
            }
            set
            {
                _player = value;
                PlayerId = _player.PlayerId;
            }
        }
        private PlayerControl _player;
        public byte PlayerId;
        public GameObject gameObject;
        public Transform transform => gameObject.transform;
        public SpriteRenderer Renderer;
        public float UpdateTime;
        public Sprite[] Sprites;
        public int Index;
        public const float DefaultUpdateTime = 0.03f;
        public int SpriteType;
        public Sprite[] GetSprites()
        {
            //0～2はアニメーションあり(index:8)
            //3～4はシンプル
            var type = ModHelpers.GetRandomInt(4);
            type = 4;
            SpriteType = type;
            switch (type)
            {
                case 0:
                case 1:
                case 2:
                    List<Sprite> sprites = new();
                    for (int i = 1; i <= 8; i++)
                    {
                        sprites.Add(ModHelpers.LoadSpriteFromResources($"SuperNewRoles.Resources.harisen.deadbody_{type}_{i}.PNG", 115f));
                    }
                    return sprites.ToArray();
                case 3:
                case 4:
                    return new Sprite[]{ ModHelpers.LoadSpriteFromResources($"SuperNewRoles.Resources.harisen.deadbody_{type}.PNG",115f)};
            }
            return null;
        }
        public void Start(byte SourceId, byte TargetId, Vector2 force)
        {
            this.SourceId = SourceId;
            this.PlayerId = TargetId;
            Force = force;
            gameObject = new("SluggerDeadBody");
            Renderer = gameObject.AddComponent<SpriteRenderer>();
            var body = gameObject.AddComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            Vector3 kakeru = Source.transform.position - Player.transform.position;
            body.velocity = kakeru * -10f;
            Index = 0;
            Sprites = GetSprites();
            DeadBodys.Add(this);
            transform.position = Player.transform.position;
            transform.localScale = new(0.1f,0.1f,0);
            transform.Rotate((Source.transform.position - Player.transform.position));
            if (SpriteType == 3)
            {
                transform.Rotate((Source.transform.position - Player.transform.position) * -1f);
            }
            Renderer.sharedMaterial = DestroyableSingleton<HatManager>.Instance.PlayerMaterial;
            Renderer.maskInteraction = SpriteMaskInteraction.None;
            PlayerMaterial.SetColors(Player.Data.DefaultOutfit.ColorId, Renderer);
            PlayerMaterial.Properties Properties = new()
            {
                MaskLayer = 0,
                MaskType = PlayerMaterial.MaskType.None,
                ColorId = Player.Data.DefaultOutfit.ColorId
            };
            Renderer.material.SetInt(PlayerMaterial.MaskLayer, Properties.MaskLayer);
        }
        public static void AllFixedUpdate()
        {
            foreach (SluggerDeadbody deadbody in DeadBodys.ToArray())
            {
                deadbody.FixedUpdate();
            }
        }
        public void FixedUpdate()
        {
            //transform.position += new Vector3(Force.x,Force.y,0);
            if (gameObject == null)
            {
                DeadBodys.Remove(this);
                return;
            }
            UpdateTime -= Time.fixedDeltaTime;
            if (UpdateTime <= 0)
            {
                UpdateTime = 0.1f;
                Index++;
                if (Sprites.Length <= Index)
                {
                    Index = 0;
                }
                Renderer.sprite = Sprites[Index];
            }
            if (Vector2.Distance(CachedPlayer.LocalPlayer.transform.position, transform.position) > 30 || RoleClass.IsMeeting)
            {
                foreach (SluggerDeadbody deadbody in DeadBodys)
                {
                    if (deadbody.SourceId == SourceId)
                    {
                        GameObject.Destroy(deadbody.gameObject);
                    }
                }
                DeadBodys.RemoveAll(x => x.SourceId == SourceId);
                return;
            }
        }
    }
}
