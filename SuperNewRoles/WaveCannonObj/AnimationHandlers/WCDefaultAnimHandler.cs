using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SuperNewRoles.CustomObject;
using SuperNewRoles.Roles.Impostor;
using SuperNewRoles.Roles.Neutral;
using SuperNewRoles.Roles.RoleBases;
using UnityEngine;

namespace SuperNewRoles.WaveCannonObj.AnimationHandlers;
public class WCDefaultAnimHandler : IWaveCannonAnimationHandler
{
    public WaveCannonObject CannonObject { get; }
    public static Sprite[] ChargeSprites
    {
        get
        {
            if (CachedSpritesCharge == null)
            {
                CachedSpritesCharge = new Sprite[5];
                for (int i = 1; i <= 5; i++)
                    CachedSpritesCharge[i - 1] = null;// ModHelpers.LoadSpriteFromResources($"SuperNewRoles.Resources.WaveCannon.Charge_000{i}.png", 115f);
            }
            return CachedSpritesCharge;
        }
    }

    public Sprite ColliderSprite => ColliderSpriteStatic;
    public static Sprite ColliderSpriteStatic
    {
        get
        {
            if (_colliderSprite is null)
                _colliderSprite = ModHelpers.LoadSpriteFromResources($"SuperNewRoles.Resources.WaveCannon.Shoot_0004.png", 115f);
            return _colliderSprite;
        }
    }
    public static Sprite[] ShootSprites
    {
        get
        {
            if (CachedSpritesShoot == null)
            {
                CachedSpritesShoot = new Sprite[12];
                for (int i = 1; i <= 12; i++)
                    CachedSpritesShoot[i - 1] = ModHelpers.LoadSpriteFromResources($"SuperNewRoles.Resources.WaveCannon.Shoot_00{(i <= 9 ? "0" : "")}{i}.png", 115f);
            }
            return CachedSpritesShoot;
        }
    }
    public static Sprite[] CachedSpritesShoot = null;
    public static Sprite[] ShootSpritesNowing
    {
        get
        {
            if (CachedSpritesShootNowing == null)
            {
                CachedSpritesShootNowing = new Sprite[7];
                for (int i = 6; i <= 12; i++)
                    CachedSpritesShootNowing[i - 6] = ModHelpers.LoadSpriteFromResources($"SuperNewRoles.Resources.WaveCannon.Shoot_00{(i <= 9 ? "0" : "")}{i}.png", 115f);
            }
            return CachedSpritesShootNowing;
        }
    }
    public static Sprite[] CachedSpritesShootNowing = null;
    private static Sprite _colliderSprite;

    public static Sprite[] CachedSpritesCharge = null;
    public WCDefaultAnimHandler(WaveCannonObject waveCannonObject)
    {
        CannonObject = waveCannonObject;
        WaveCannonEffect WCEffect = CannonObject.WaveCannonEffects.FirstOrDefault();
        WCEffect.transform.localPosition = new(3.3f, 0, 0.1f);
    }
    public CustomAnimationOptions Init()
    {
        return new CustomAnimationOptions(ChargeSprites, 25, true, EffectSound: ModHelpers.loadAudioClipFromResources("SuperNewRoles.Resources.WaveCannon.ChargeSound.raw"), IsEffectSoundLoop: true);
    }

    public void OnShot()
    {
        WaveCannonEffect WCEffect = CannonObject.WaveCannonEffects.FirstOrDefault();
        WCEffect.transform.localPosition = new(0f, 0, 0.1f);
        CannonObject.transform.localPosition += new Vector3(CannonObject.IsFlipX ? -4.05f : 4.05f,0);
        foreach (var obj in CannonObject.effectrenders.AsSpan()) obj.sprite = ShootSprites[0];
        Sprite[] sprites = new Sprite[12];
        for (int i = 0; i < 12; i++)
            sprites[i] = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.WaveCannon.Cannon.png", 115f);
        CannonObject.Options.SetSprites(sprites,
            IsLoop: true, frameRate: 12);
        CannonObject.Stop();
        CannonObject.Play();
        CannonObject.Options.SetOnEndAnimation((anim, option) =>
        {
            CannonObject.IsShootFirst = false;
            for (int i = 0; i < 7; i++)
                sprites[i] = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.WaveCannon.Cannon.png", 115f);

            option.SetSprites(sprites, IsLoop: true, frameRate: 15);
            CannonObject.Stop(IsStopMusic: false);
            CannonObject.Play(IsPlayMusic: false);
            foreach (var obj in CannonObject.effectrenders.AsSpan()) obj.sprite = sprites[0];
            CannonObject.Options.SetOnEndAnimation((anim, option) =>
            {
                CannonObject.DestroyIndex++;
                if (CannonObject.DestroyIndex > 3)
                {
                    CannonObject?.Owner?.GetRoleBase<WaveCannonJackal>()?.SetDidntLoadBullet();
                    if (CannonObject.OwnerPlayerId == CachedPlayer.LocalPlayer.PlayerId)
                    {
                        if (PlayerControl.LocalPlayer.IsRole(RoleId.WaveCannon))
                        {
                            if (WaveCannon.IsSyncKillCoolTime.GetBool())
                                PlayerControl.LocalPlayer.SetKillTimer(RoleHelpers.GetCoolTime(PlayerControl.LocalPlayer, null));
                        }
                        else
                        {
                            WaveCannonJackal.ResetCooldowns(false, true);
                        }
                        CannonObject.Owner.GetRoleBase<WaveCannon>()?
                        .CustomButtonInfos?
                        .FirstOrDefault()?
                        .ResetCoolTime();
                    }
                    GameObject.Destroy(CannonObject.gameObject);
                }
            });
        });
    }
    public void RendererUpdate()
    {
        foreach (var obj in CannonObject.effectrenders.AsSpan())
        {
            if (CannonObject.IsShootFirst)
            {
                if (ShootSprites.Length > CannonObject.Index)
                    obj.sprite = ShootSprites[CannonObject.Index];
            }
            else
            {
                if (ShootSpritesNowing.Length > CannonObject.Index)
                    obj.sprite = ShootSpritesNowing[CannonObject.Index];
            }
        }
    }
}