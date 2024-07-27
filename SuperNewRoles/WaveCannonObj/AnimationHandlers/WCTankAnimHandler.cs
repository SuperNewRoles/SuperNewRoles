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
public class WCTankAnimHandler : IWaveCannonAnimationHandler
{
    public WaveCannonObject CannonObject { get; }
    public PoolablePlayer CannonPoolable { get; }

    public static Sprite TankSprite
    {
        get
        {
            if (_tankSprite == null)
                _tankSprite = AssetManager.GetAsset<Sprite>("Tank.png", AssetManager.AssetBundleType.Wavecannon);
            return _tankSprite;
        }
    }
    private static Sprite _tankSprite;

    public WCTankAnimHandler(WaveCannonObject waveCannonObject)
    {
        CannonObject = waveCannonObject;
        CannonObject.spriteRenderer.sharedMaterial = FastDestroyableSingleton<HatManager>.Instance.PlayerMaterial;
        CannonObject.spriteRenderer.maskInteraction = SpriteMaskInteraction.None;
        PlayerMaterial.SetMaskLayerBasedOnLocalPlayer(CannonObject.spriteRenderer, false);
        PlayerMaterial.SetColors(CannonObject.Owner.CurrentOutfit.ColorId, CannonObject.spriteRenderer);
        CannonPoolable = GameObject.Instantiate(MapOption.MapOption.playerIcons[waveCannonObject.OwnerPlayerId], waveCannonObject.transform);
        SetChildLayer(CannonPoolable.transform, 0);
        CannonPoolable.transform.localPosition = new(0, 0.4f, 0.05f);
        CannonPoolable.transform.localScale = new(-0.35f, 0.35f, 0.35f);
        CannonPoolable.cosmetics.nameText.gameObject.SetActive(false);
        CannonPoolable.gameObject.SetActive(true);
        // CannonPoolable.UpdateFromPlayerOutfit(waveCannonObject.Owner.CurrentOutfit, PlayerMaterial.MaskType.None, false, false);
    }
    private static void SetChildLayer(Transform parent, int layer)
    {
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            child.gameObject.layer = layer;
            SetChildLayer(child, layer);
        }
    }
    public CustomAnimationOptions Init()
    {
        AudioClip ChargeSound;
        if (CannonObject.CurrentAnimType == WaveCannonObject.WCAnimType.Tank)
            ChargeSound = ModHelpers.loadAudioClipFromResources("SuperNewRoles.Resources.WaveCannon.ChargeSound.raw");
        else
            ChargeSound = AssetManager.GetAsset<AudioClip>("BulletChargeSound.ogg", AssetManager.AssetBundleType.Wavecannon);
        return new CustomAnimationOptions([TankSprite], 25, true, EffectSound: ChargeSound, IsEffectSoundLoop: true);
    }

    public void OnShot()
    {
        Sprite[] sprites = new Sprite[12];
        for (int i = 0; i < 12; i++)
            sprites[i] = TankSprite;
        CannonObject.Options.SetSprites(sprites,
            IsLoop: true, frameRate: 12);
        CannonObject.Stop();
        CannonObject.Play();
        CannonObject.Options.SetOnEndAnimation((anim, option) =>
        {
            CannonObject.IsShootFirst = false;
            for (int i = 0; i < 7; i++)
                sprites[i] = TankSprite;

            option.SetSprites(sprites, IsLoop: true, frameRate: 15);
            CannonObject.Stop(IsStopMusic: false);
            CannonObject.Play(IsPlayMusic: false);
            foreach (var obj in CannonObject.effectrenders.AsSpan()) obj.sprite = sprites[0];
            CannonObject.Options.SetOnEndAnimation((anim, option) =>
            {
                CannonObject.DestroyIndex++;
                if (CannonObject.DestroyIndex <= 3)
                    return;
                CannonObject?.Owner?.GetRoleBase<WaveCannonJackal>()?.SetDidntLoadBullet();
                if (CannonObject.OwnerPlayerId == CachedPlayer.LocalPlayer.PlayerId)
                {
                    CannonObject.Owner.GetRoleBase<WaveCannon>()?
                    .CustomButtonInfos?
                    .FirstOrDefault()?
                    .ResetCoolTime();
                    if (CannonObject.Owner.GetRoleBase<WaveCannon>() != null && WaveCannon.IsSyncKillCoolTime.GetBool())
                        PlayerControl.LocalPlayer.SetKillTimer(RoleHelpers.GetCoolTime(PlayerControl.LocalPlayer, null));
                    WaveCannonJackal.ResetCooldowns(false, true);
                }
                GameObject.Destroy(CannonObject.gameObject);
            });
        });
    }
    public void RendererUpdate()
    {
    }
}