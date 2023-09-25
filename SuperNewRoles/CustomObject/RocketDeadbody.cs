using System;
using UnityEngine;

namespace SuperNewRoles.CustomObject;
public class RocketDeadbody : CustomAnimation
{
    public RocketDeadbody(IntPtr intPtr) : base(intPtr)
    {
    }
    private Vector3 BasePos;
    private readonly static Vector3 movepos = new(0,0.01f,0);
    private bool IsFirework;
    public override void Awake()
    {
        base.Awake();
        Logger.Info("Awaked");
        spriteRenderer.sharedMaterial = FastDestroyableSingleton<HatManager>.Instance.PlayerMaterial;
        spriteRenderer.maskInteraction = SpriteMaskInteraction.None;
    }
    public void Init(PlayerControl Player)
    {
        Logger.Info("a");
        CustomAnimationOptions customAnimationOptions = new(GetSprites("SuperNewRoles.Resources.Rocket.RocketPlayer", 2), 10, true);
        Logger.Info("b");
        base.Init(customAnimationOptions);
        Logger.Info("c");
        Logger.Info(Player == null ? "null" : "nonull");
        Logger.Info(Player.Data == null ? "null" : "nonull");
        Logger.Info(Player.Data.DefaultOutfit == null ? "null" : "nonull");
        Logger.Info(Player.Data.DefaultOutfit.ColorId == null ? "null" : "nonull");
        Logger.Info(spriteRenderer == null ? "null" : "nonull");
        //カラーを変更する
        PlayerMaterial.SetColors(Player.Data.DefaultOutfit.ColorId, spriteRenderer);
        Logger.Info("d");
        PlayerMaterial.Properties Properties = new()
        {
            MaskLayer = 0,
            MaskType = PlayerMaterial.MaskType.None,
            ColorId = Player.Data.DefaultOutfit.ColorId
        };
        Logger.Info("e");
        spriteRenderer.material.SetInt(PlayerMaterial.MaskLayer, Properties.MaskLayer);
        Logger.Info("f");
        transform.position = Player.transform.position;
        Logger.Info("g");
        BasePos = transform.position;
        Logger.Info("h");
        IsFirework = false;
        Logger.Info("i");
    }
    public override void Update()
    {
        if (!IsFirework)
        {
            if (Playing)
            {
                transform.position += movepos;
                if ((transform.position - BasePos).y > 3.1f)
                {
                    IsFirework = true;
                    Options.SetSprites(GetSprites("SuperNewRoles.Resources.Rocket.Fireworks.fireworks_", 20,2), IsLoop:false, frameRate: 30);
                    Options.SetOnEndAnimation((a,b) => GameObject.Destroy(a.gameObject));
                    Options.SetPlayEndDestroy(true);
                    return;
                }
            }
            else if (IsRewinding)
            {
                transform.position -= movepos;
            }
        }
    }
}