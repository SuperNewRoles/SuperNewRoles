using System;
using UnityEngine;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability.CustomButton;
using SuperNewRoles.WaveCannonObj;
using SuperNewRoles.Events.PCEvents;
using SuperNewRoles.Modules.Events.Bases;
using SuperNewRoles.SuperTrophies;
using System.Linq;

namespace SuperNewRoles.Roles.Ability;

public class WaveCannonAbility : CustomButtonBase, IButtonEffect
{
    private float coolDown;
    // IButtonEffect
    private float effectDuration;
    public float bulletDuration;
    public float EffectDuration => bullet != null ? bulletDuration : effectDuration;
    public Action OnEffectEnds => () => { RpcShootCannon(); };
    public bool isEffectActive { get; set; }
    public float EffectTimer { get; set; }
    public override Sprite Sprite => bullet != null ? AssetManager.GetAsset<Sprite>("WaveCannonLoadedBulletButton.png") : AssetManager.GetAsset<Sprite>("WaveCannonButton.png");
    public override string buttonText => ModTranslation.GetString("WaveCannonButtonText");
    protected override KeyType keytype => KeyType.Ability1;
    public override float DefaultTimer => coolDown;
    public WaveCannonObjectBase WaveCannonObject { get; private set; }
    public WaveCannonType Type { get { return bullet != null ? WaveCannonType.Bullet : _type; } }
    public ExPlayerControl bullet;
    private WaveCannonType _type;
    public bool isResetKillCooldown { get; }
    private EventListener<MurderEventData> _onMurderEvent;
    public WaveCannonAbility(float coolDown, float effectDuration, WaveCannonType type, bool isResetKillCooldown = false)
    {
        this.coolDown = coolDown;
        this.effectDuration = effectDuration;
        this._type = type;
        this.isResetKillCooldown = isResetKillCooldown;
    }
    public override void OnClick()
    {
        WaveCannonObjectBase.RpcSpawnFromType(PlayerControl.LocalPlayer, Type, this.AbilityId, PlayerControl.LocalPlayer.MyPhysics.FlipX, PlayerControl.LocalPlayer.transform.position);
        ResetTimer();
    }
    [CustomRPC]
    public static void RpcBulletSuicide(ExPlayerControl source)
    {
        source.CustomDeath(CustomDeathType.SuicideSecrets);
    }
    public override void AttachToLocalPlayer()
    {
        base.AttachToLocalPlayer();
        _onMurderEvent = MurderEvent.Instance.AddListener(x =>
        {
            if (isResetKillCooldown && x.killer == PlayerControl.LocalPlayer)
                ExPlayerControl.LocalPlayer.ResetKillCooldown();
        });
    }
    public override void DetachToLocalPlayer()
    {
        base.DetachToLocalPlayer();
        if (_onMurderEvent != null)
            MurderEvent.Instance.RemoveListener(_onMurderEvent);
    }

    public override bool CheckIsAvailable()
    {
        return PlayerControl.LocalPlayer.CanMove;
    }
    public override void Detach()
    {
        base.Detach();
        new LateTask(() => WaveCannonObject?.Detach(), 0f);
    }
    public void SpawnedWaveCannonObject(WaveCannonObjectBase waveCannonObject)
    {
        WaveCannonObject = waveCannonObject;
    }
    [CustomRPC]
    public void RpcShootCannon()
    {
        WaveCannonObject?.OnShoot();
        if (bullet != null)
            bullet.CustomDeath(CustomDeathType.SuicideSecrets);
        bullet = null;
    }
}

public enum WaveCannonTypeForOption
{
    Tank = WaveCannonType.Tank,
}
public enum WaveCannonType
{
    Tank,
    Cannon,
    Santa,
    Bullet,
}
public class WaveCannonFiveShotTrophy : SuperTrophyAbility<WaveCannonFiveShotTrophy>
{
    public override TrophiesEnum TrophyId => TrophiesEnum.WaveCannonFiveShot;
    public override TrophyRank TrophyRank => TrophyRank.Bronze;

    private WaveCannonAbility _waveCannonAbility;
    private EventListener<MurderEventData> _onMurderEvent;

    private int _lastInstanceId;
    private int _killedCounter;
    public const int NeededKilledCount = 5;
    public override Type[] TargetAbilities => new Type[] { typeof(WaveCannonAbility) };

    public override void OnRegister()
    {
        // WaveCannonAbility の取得とカウンターの初期化
        _waveCannonAbility = ExPlayerControl.LocalPlayer.PlayerAbilities
            .FirstOrDefault(x => x is WaveCannonAbility) as WaveCannonAbility;
        _killedCounter = 0;

        // 殺害イベントのリスナーを登録
        _onMurderEvent = MurderEvent.Instance.AddListener(HandleMurderEvent);
    }

    private void HandleMurderEvent(MurderEventData data)
    {
        // ローカルプレイヤーによるアクション以外は無視する
        if (data.killer != PlayerControl.LocalPlayer)
        {
            return;
        }

        // WaveCannonObject と内部の WaveCannonObject が有効か確認
        var cannonObj = _waveCannonAbility?.WaveCannonObject?.WaveCannonObject;
        if (cannonObj == null)
        {
            return;
        }

        int currentInstanceId = cannonObj.GetInstanceID();
        if (currentInstanceId != _lastInstanceId)
        {
            _killedCounter = 0;
            _lastInstanceId = currentInstanceId;
        }

        _killedCounter++;
        if (_killedCounter >= NeededKilledCount)
        {
            Complete();
        }
    }

    public override void OnDetached()
    {
        if (_onMurderEvent != null)
        {
            MurderEvent.Instance.RemoveListener(_onMurderEvent);
            _onMurderEvent = null;
        }
    }
}
public class WaveCannonOneThousandShotTrophy : SuperTrophyAbility<WaveCannonOneThousandShotTrophy>
{
    public override TrophiesEnum TrophyId => TrophiesEnum.WaveCannonOneThousandShot;
    public override TrophyRank TrophyRank => TrophyRank.Bronze;

    public override Type[] TargetAbilities => [typeof(WaveCannonAbility)];

    private WaveCannonAbility _waveCannonAbility;
    private EventListener<MurderEventData> _onMurderEvent;

    public override void OnRegister()
    {
        _waveCannonAbility = ExPlayerControl.LocalPlayer.PlayerAbilities
            .FirstOrDefault(x => x is WaveCannonAbility) as WaveCannonAbility;
        _onMurderEvent = MurderEvent.Instance.AddListener(HandleMurderEvent);
    }
    private void HandleMurderEvent(MurderEventData data)
    {
        if (data.killer != PlayerControl.LocalPlayer)
        {
            return;
        }

        var cannonObj = _waveCannonAbility?.WaveCannonObject?.WaveCannonObject;
        if (cannonObj == null)
        {
            return;
        }

        TrophyData++;
        if (TrophyData >= 1000)
        {
            Complete();
        }
    }
    public override void OnDetached()
    {
        if (_onMurderEvent != null)
        {
            MurderEvent.Instance.RemoveListener(_onMurderEvent);
            _onMurderEvent = null;
        }
    }
}
