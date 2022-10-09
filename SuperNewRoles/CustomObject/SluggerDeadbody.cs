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
        public static List<SluggerDeadbody> DeadBodys = new();
        public Vector2 Force;
        public PlayerControl Source
        {
            get
            {
                if (this._source == null)
                {
                    this._source = this.SourceId.GetPlayerControl();
                }
                return this._source;
            }
            set
            {
                this._source = value;
                this.SourceId = this._source.PlayerId;
            }
        }
        private PlayerControl _source;
        public byte SourceId;
        public PlayerControl Player
        {
            get
            {
                if (this._player == null)
                {
                    this._player = this.PlayerId.GetPlayerControl();
                }
                return this._player;
            }
            set
            {
                this._player = value;
                this.PlayerId = this._player.PlayerId;
            }
        }
        private PlayerControl _player;
        public byte PlayerId;
        public GameObject gameObject;
        public Transform transform => this.gameObject.transform;
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
            int type = ModHelpers.GetRandomInt(4);
            type = 4;
            this.SpriteType = type;
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
                    return new Sprite[] { ModHelpers.LoadSpriteFromResources($"SuperNewRoles.Resources.harisen.deadbody_{type}.PNG", 115f) };
            }
            return null;
        }
        public void Start(byte SourceId, byte TargetId, Vector2 force)
        {
            this.SourceId = SourceId;
            this.PlayerId = TargetId;
            this.Force = force;
            this.gameObject = new("SluggerDeadBody");
            this.Renderer = this.gameObject.AddComponent<SpriteRenderer>();
            Rigidbody2D body = this.gameObject.AddComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            Vector3 kakeru = this.Source.transform.position - this.Player.transform.position;
            body.velocity = kakeru * -10f;
            this.Index = 0;
            this.Sprites = this.GetSprites();
            DeadBodys.Add(this);
            this.transform.position = this.Player.transform.position;
            this.transform.localScale = new(0.1f, 0.1f, 0);
            this.transform.Rotate(this.Source.transform.position - this.Player.transform.position);
            if (this.SpriteType == 3)
            {
                this.transform.Rotate((this.Source.transform.position - this.Player.transform.position) * -1f);
            }
            this.Renderer.sharedMaterial = DestroyableSingleton<HatManager>.Instance.PlayerMaterial;
            this.Renderer.maskInteraction = SpriteMaskInteraction.None;
            PlayerMaterial.SetColors(this.Player.Data.DefaultOutfit.ColorId, this.Renderer);
            PlayerMaterial.Properties Properties = new()
            {
                MaskLayer = 0,
                MaskType = PlayerMaterial.MaskType.None,
                ColorId = this.Player.Data.DefaultOutfit.ColorId
            };
            this.Renderer.material.SetInt(PlayerMaterial.MaskLayer, Properties.MaskLayer);
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
            if (this.gameObject == null)
            {
                DeadBodys.Remove(this);
                return;
            }
            this.UpdateTime -= Time.fixedDeltaTime;
            if (this.UpdateTime <= 0)
            {
                this.UpdateTime = 0.1f;
                this.Index++;
                if (this.Sprites.Length <= this.Index)
                {
                    this.Index = 0;
                }
                this.Renderer.sprite = this.Sprites[this.Index];
            }
            if (Vector2.Distance(CachedPlayer.LocalPlayer.transform.position, this.transform.position) > 30 || RoleClass.IsMeeting)
            {
                foreach (SluggerDeadbody deadbody in DeadBodys)
                {
                    if (deadbody.SourceId == this.SourceId)
                    {
                        GameObject.Destroy(deadbody.gameObject);
                    }
                }
                DeadBodys.RemoveAll(x => x.SourceId == this.SourceId);
                return;
            }
        }
    }
}