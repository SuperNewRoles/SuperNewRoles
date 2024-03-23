using System;
using System.Collections.Generic;
using UnityEngine;

namespace SuperNewRoles.CustomObject;

public class SluggerDeadbody : CustomAnimation
{
    public SluggerDeadbody(IntPtr intPtr) : base(intPtr)
    {
    }
    public static List<SluggerDeadbody> DeadBodys = new();
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
    public float UpdateTime;
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
                return new Sprite[] { ModHelpers.LoadSpriteFromResources($"SuperNewRoles.Resources.harisen.deadbody_{type}.PNG", 115f) };
        }
        return null;
    }
    public Rigidbody2D body;
    public Vector2 Velocity;
    public override void Awake()
    {
        base.Awake();
        body = gameObject.GetOrAddComponent<Rigidbody2D>();
        body.gravityScale = 0;
        spriteRenderer.sharedMaterial = FastDestroyableSingleton<HatManager>.Instance.PlayerMaterial;
        spriteRenderer.maskInteraction = SpriteMaskInteraction.None;
        PlayerMaterial.SetMaskLayerBasedOnLocalPlayer(spriteRenderer, false);
        DeadBodys.Add(this);
    }
    public void Init(byte SourceId, byte TargetId)
    {
        CustomAnimationOptions customAnimationOptions = new(GetSprites(), 10, true);
        base.Init(customAnimationOptions);
        this.SourceId = SourceId;
        this.PlayerId = TargetId;
        Velocity = Source.transform.position - Player.transform.position;
        Velocity *= -10f;
        body.velocity = Velocity;
        transform.position = Player.transform.position;
        transform.localScale = new(0.1f, 0.1f, 0);
        transform.Rotate(Source.transform.position - Player.transform.position);
        if (SpriteType == 3)
        {
            transform.Rotate((Source.transform.position - Player.transform.position) * -1f);
        }
        PlayerMaterial.SetColors(Player.Data.DefaultOutfit.ColorId, spriteRenderer);
    }
    public override void Play(bool IsPlayMusic = true)
    {
        base.Play(IsPlayMusic);
        body.velocity = Velocity;
    }
    public override void Pause(bool IsStopMusic = true)
    {
        base.Pause(IsStopMusic);
        body.velocity = new();
    }
    public override void OnPlayRewind()
    {
        base.OnPlayRewind();
        body.velocity = -Velocity;
    }
    public override void OnDestroy()
    {
        base.OnDestroy();
        DeadBodys.Remove(this);
    }
    public override void Update()
    {
        if (Vector2.Distance(CachedPlayer.LocalPlayer.transform.position, transform.position) > 30)
            Destroy(this.gameObject);
    }
}