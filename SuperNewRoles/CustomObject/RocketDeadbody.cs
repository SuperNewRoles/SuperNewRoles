using System;
using UnityEngine;

namespace SuperNewRoles.CustomObject;
public class RocketDeadbody : CustomAnimation
{
    public RocketDeadbody(IntPtr intPtr) : base(intPtr)
    {
    }
    private PlayerControl Player;
    private Vector3 BasePos;
    private readonly static Vector3 movepos = new(0,0,0.1f);
    private bool IsFirework;
    public void Init(PlayerControl player)
    {
        CustomAnimationOptions customAnimationOptions = new(GetSprites("SuperNewRoles.Resources.Rocket.RocketPlayer", 2), 10, true);
        base.Init(customAnimationOptions);
        //カラーを変更する
        PlayerMaterial.SetColors(Player.Data.DefaultOutfit.ColorId, spriteRenderer);
        PlayerMaterial.Properties Properties = new()
        {
            MaskLayer = 0,
            MaskType = PlayerMaterial.MaskType.None,
            ColorId = Player.Data.DefaultOutfit.ColorId
        };
        spriteRenderer.material.SetInt(PlayerMaterial.MaskLayer, Properties.MaskLayer);
        transform.position = Player.transform.position;
        BasePos = transform.position;
        IsFirework = false;
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
                    Options.SetSprites(GetSprites("SuperNewRoles.Resources.Rocket.Fireworks.fireworks__", 20), IsLoop:false, frameRate: 30);
                    Options.SetOnEndAnimation((a,b) => GameObject.Destroy(a.gameObject));
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