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
public class WCSantaAnimHandler : IWaveCannonAnimationHandler
{
    public WaveCannonObject CannonObject { get; }

    public List<WCSantaHandler> Santas;
    private float SantaSpawnTimer;
    public static readonly float SantaSpawnTimeInterval = 0.3f;

    public static Sprite[] CachedSpritesCharge = null;
    public static Sprite[] ChargeSprites
    {
        get
        {
            if (CachedSpritesCharge == null)
            {
                CachedSpritesCharge = new Sprite[5];
                for (int i = 1; i <= 5; i++)
                    CachedSpritesCharge[i - 1] = ModHelpers.LoadSpriteFromResources($"SuperNewRoles.Resources.WaveCannon.Charge_000{i}.png", 115f);
            }
            return CachedSpritesCharge;
        }
    }

    public Sprite ColliderSprite => WCDefaultAnimHandler.ColliderSpriteStatic;

    public WCSantaAnimHandler(WaveCannonObject waveCannonObject)
    {
        CannonObject = waveCannonObject;
        Santas = new();
        SantaSpawnTimer = -1;
    }
    public CustomAnimationOptions Init()
    {
        return new CustomAnimationOptions(ChargeSprites, 25, true, EffectSound: ModHelpers.loadAudioClipFromResources("SuperNewRoles.Resources.WaveCannon.ChargeSound.raw"), IsEffectSoundLoop: true);
    }

    public void OnShot()
    {
        SpawnSanta();
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
                            if (WaveCannonJackal.IsSyncKillCoolTime.GetBool())
                                WaveCannonJackal.ResetCooldowns(false, true);
                        }
                        CannonObject.Owner.GetRoleBase<WaveCannon>()?
                        .CustomButtonInfos?
                        .FirstOrDefault()?
                        .ResetCoolTime();
                    }
                    Santas.ForEach(santa => { if (santa != null) santa.transform.SetParent(null, true); });
                    GameObject.Destroy(CannonObject.gameObject);
                }
            });
        });
    }
    private void SpawnSanta()
    {
        WCSantaHandler SantaHandler = new GameObject("Santa").AddComponent<WCSantaHandler>();
        SantaHandler.transform.parent = CannonObject.transform;
        SantaHandler.transform.localPosition = new(-2.4f, 0.275f, 0.1f);
        SantaHandler.transform.localScale = new(-0.1f, 0.1f, 0.1f);
        SantaHandler.moveX = 2.4f;
        Santas.Add(SantaHandler);
        //タイマーをリセット
        SantaSpawnTimer = SantaSpawnTimeInterval;
    }
    public void RendererUpdate()
    {

    }
    public void OnUpdate()
    {
        if (SantaSpawnTimer == -1)
            return;
        Santas.RemoveAll(x => x == null);
        SantaSpawnTimer -= Time.deltaTime;
        if (SantaSpawnTimer <= 0)
            SpawnSanta();
    }
}