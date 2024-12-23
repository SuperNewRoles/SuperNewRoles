using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Hazel;
using SuperNewRoles;
using SuperNewRoles.Buttons;
using SuperNewRoles.CustomObject;
using SuperNewRoles.Helpers;
using SuperNewRoles.Roles;
using SuperNewRoles.Roles.Crewmate;
using SuperNewRoles.Roles.Impostor;
using SuperNewRoles.Roles.Neutral;
using SuperNewRoles.Roles.RoleBases;
using SuperNewRoles.WaveCannonObj.AnimationHandlers;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace SuperNewRoles.WaveCannonObj;

public class WaveCannonObject : CustomAnimation
{
    public WaveCannonObject(IntPtr intPtr) : base(intPtr)
    {
    }
    public enum WCAnimType
    {
        Tank, //戦車
        Cannon, //大砲
        Santa, //サンタ

        None,
        // 以下表示しない
        Bullet, //弾
    }
    public enum RpcType
    {
        Spawn,
        Shoot
    }

    // static系
    public static PlayerData<WaveCannonObject> Objects = new();
    public static readonly IReadOnlyDictionary<string, Func<WaveCannonObject, IWaveCannonAnimationHandler>> WCCreateAnimHandlers =
        new Dictionary<string, Func<WaveCannonObject, IWaveCannonAnimationHandler>>()
    {
        { WCAnimType.Tank.ToString(), (waveCannon) => new WCTankAnimHandler(waveCannon) },
        { WCAnimType.Bullet.ToString(), (waveCannon) => new WCTankAnimHandler(waveCannon) },
        { WCAnimType.Cannon.ToString(), (waveCannon) => new WCDefaultAnimHandler(waveCannon) },
        { WCAnimType.Santa.ToString(), (waveCannon) => new WCSantaAnimHandler(waveCannon) },
    };
    public static List<(float, Vector2)> RotateSet = new() { (90, new(-3.05f, 25.5f)), (270, new(-4f, -26.5f)), (45, new(14.3f, 17.75f)), (45, new(14.3f, -17.75f)), (180, new(-30.7f, -0.8f)) };
    public static Dictionary<byte, int> Ids;
    public static Dictionary<int, WaveCannonEffect> EffectPrefabs = new();
    private static GameObject _waveCannonObjectPrefab;
    public static GameObject WaveCannonObjectPrefab
    {
        get
        {
            if (_waveCannonObjectPrefab is null)
            {
                var resourceAudioAssetBundleStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("SuperNewRoles.Resources.WaveCannon.WaveCannonEffectsOld.bundle");
                var assetBundleBundle = AssetBundle.LoadFromMemory(resourceAudioAssetBundleStream.ReadFully());
                _waveCannonObjectPrefab = assetBundleBundle.LoadAsset<GameObject>("WaveCannonEffects.prefab").DontUnload();
            }
            return _waveCannonObjectPrefab;
        }
    }

    // Animation系
    public WCAnimType CurrentAnimType { get; private set; }
    public IWaveCannonAnimationHandler CurrentAnimationHandler { get; private set; }

    // Effect系
    public Transform effectGameObjectsParent;
    public List<WaveCannonEffect> WaveCannonEffects;
    public List<SpriteRenderer> effectrenders { get; private set; }
    public HashSet<Collider2D> WaveColliders;

    // 持ち主系
    public PlayerControl Owner;
    public byte OwnerPlayerId { get; private set; }

    // 波動砲詳細系
    public int Id;

    // 賢者系
    public Dictionary<PlayerControl, (RoleEffectAnimation, float, Vector3)> WiseManData;
    public List<byte> CannotMurderPlayers;

    public bool IsShootNow;
    public bool IsFlipX;
    public int DestroyIndex { get; set; } = 0;
    static Vector3 OwnerPos;
    public bool IsShootFirst;

    //Santa用の旧仕様
    public List<PolygonCollider2D> colliders;
    public List<GameObject> effectGameObjects;


    public override void Awake()
    {
        base.Awake();
        WiseManData = new();
        WaveColliders = new();
        WaveCannonEffects = new();
        effectrenders = new();
        CannotMurderPlayers = new();

        //以下旧仕様
        colliders = new();
        effectGameObjects = new();
    }
    private WaveCannonEffect GetPrefab()
    {
        if (!EffectPrefabs.TryGetValue((int)CurrentAnimType, out WaveCannonEffect prefab))
        {
            EffectPrefabs[(int)CurrentAnimType] = prefab = AssetManager.GetAsset<GameObject>($"WaveCannon{(CurrentAnimType == WCAnimType.Santa ? WCAnimType.Cannon : CurrentAnimType).ToString()}.prefab", AssetManager.AssetBundleType.Wavecannon).GetComponent<WaveCannonEffect>();
        }
        return prefab;
    }
    private IWaveCannonAnimationHandler CreateAnimHandler(WCAnimType? animType = null)
    {
        if (animType == null)
            animType = CurrentAnimType;
        if (WCCreateAnimHandlers.TryGetValue(animType.ToString(), out var handler))
            return handler(this);
        return null;
    }
    public WaveCannonObject Init(Vector3 pos, bool FlipX, PlayerControl _owner, WCAnimType animType)
    {
        if (animType == WCAnimType.Santa)
        {
            return OldInit(pos, FlipX, _owner, animType);
        }

        // 保存
        OwnerPos = _owner.transform.position;
        IsFlipX = FlipX;
        //使用者を設定
        Owner = _owner;
        OwnerPlayerId = _owner.PlayerId;
        // 使用者よりも前に描画
        pos.z -= 0.5f;
        // 波動砲の位置を調整
        transform.position = pos;
        transform.localScale = new(FlipX ? -1 : 1, 1, 1);

        CurrentAnimType = animType;

        //当たり判定の親を作成
        effectGameObjectsParent = new GameObject("WaveCannonEffects").transform;
        effectGameObjectsParent.SetParent(transform);
        effectGameObjectsParent.localPosition = new(0.75f, 0, 0);
        effectGameObjectsParent.localScale = Vector3.one;
        //当たり判定の部分を作成
        CreateEffect();

        Logger.Info("WaveCannon Animation:" + animType.ToString());
        CurrentAnimationHandler = CreateAnimHandler();
        if (CurrentAnimationHandler == null)
        {
            throw new Exception($"Failed to create animation handler for {animType}" +
                                 "How to fix: Make sure you have added the animation handler to the WCCreateAnimHandlers dictionary in WaveCannonObject.cs\n" +
                                 $"{animType}のHandlerの生成に失敗しました。\n" +
                                 "修正方法: WaveCannonObject.csのWCCreateAnimHandlersにHandlerを追加してください。");
        }

        // AnimationHandlerを元にCustomAnimationを初期化
        CustomAnimationOptions customAnimationOptions = CurrentAnimationHandler.Init();
        base.Init(customAnimationOptions);

        //移動ロック
        if (OwnerPlayerId == CachedPlayer.LocalPlayer.PlayerId)
        {
            CachedPlayer.LocalPlayer.PlayerControl.moveable = false;
            Camera.main.GetComponent<FollowerCamera>().Locked = true;
        }
        //Id
        if (!Ids.ContainsKey(OwnerPlayerId))
            Ids[OwnerPlayerId] = 0;
        Id = Ids[OwnerPlayerId];
        Ids[OwnerPlayerId]++;
        //シュート中かを無効に
        IsShootNow = false;
        Objects[OwnerPlayerId] = this;
        return this;
    }
    public WaveCannonObject OldInit(Vector3 pos, bool FlipX, PlayerControl _owner, WCAnimType animType)
    {
        CurrentAnimType = animType;
        Logger.Info("WaveCannon Animation:" + animType.ToString());
        CurrentAnimationHandler = CreateAnimHandler();
        WCSantaHandler.WiseManVector = new();
        WCSantaHandler.Angle = 0;
        WCSantaHandler.reflection = false;
        WCSantaHandler.Xdiff = 0;
        WCSantaHandler.IsFlipX = FlipX;
        if (CurrentAnimationHandler == null)
        {
            throw new Exception($"Failed to create animation handler for {animType}" +
                                 "How to fix: Make sure you have added the animation handler to the WCCreateAnimHandlers dictionary in WaveCannonObject.cs\n" +
                                 $"{animType}のHandlerの生成に失敗しました。\n" +
                                 "修正方法: WaveCannonObject.csのWCCreateAnimHandlersにHandlerを追加してください。");
        }
        CustomAnimationOptions customAnimationOptions = CurrentAnimationHandler.Init();
        base.Init(customAnimationOptions);
        //使用者を設定
        OwnerPlayerId = _owner.PlayerId;
        //移動ロック
        if (OwnerPlayerId == CachedPlayer.LocalPlayer.PlayerId)
        {
            CachedPlayer.LocalPlayer.PlayerControl.moveable = false;
            Camera.main.GetComponent<FollowerCamera>().Locked = true;
        }
        //保存
        OwnerPos = _owner.transform.position;
        IsFlipX = FlipX;
        Owner = _owner;
        //使用者よりも前に描画
        pos.z -= 0.0003f;
        //波動砲の位置を調整
        transform.position = pos + new Vector3(FlipX ? -4 : 4, 0, 0);
        transform.localScale = new(FlipX ? -1 : 1, 1, 1);
        //当たり判定の親を作成
        effectGameObjectsParent = new GameObject("WaveCannonEffects").transform;
        effectGameObjectsParent.SetParent(transform);
        effectGameObjectsParent.localPosition = new(31.25f, -1.45f, 2.39f);
        effectGameObjectsParent.localScale = new(1, 1, 1);
        //当たり判定の部分を作成
        CreateCollider(OldCreateEffect());
        //Id
        if (!Ids.ContainsKey(OwnerPlayerId))
            Ids[OwnerPlayerId] = 0;
        Id = Ids[OwnerPlayerId];
        Ids[OwnerPlayerId]++;
        //シュート中かを無効に
        IsShootNow = false;
        Objects[OwnerPlayerId] = this;
        return this;
    }

    public GameObject CreateRotationEffect(Vector3 PlayerPosition, float Angle)
    {
        if (CurrentAnimType == WCAnimType.Cannon)
        {
            //PlayerPosition.x -= 13;
            float posvalue = PlayerPosition.x - WaveCannonEffects.FirstOrDefault().roopanimator.transform.position.x;
            if (posvalue < 0)
                posvalue *= -1;
            WaveCannonEffects.FirstOrDefault().transform.localScale = new(posvalue * 0.0145f, 1, 1);
            WaveCannonEffect effect = CreateEffect();
            effect.transform.position = new(PlayerPosition.x - 5.5f, effect.transform.position.y, effect.transform.position.z + 0.1f);
            effect.transform.Rotate(new(0, 0, Angle));
            effect.roopanimator.transform.localPosition += new Vector3(4, 0);
            return effect.gameObject;
        }
        else
        {
            // 最後の描画のループを取得
            SpriteRenderer renderer = WaveCannonEffects[WaveCannonEffects.Count - 1].roopanimator.GetComponent<SpriteRenderer>();
            PolygonCollider2D polygonCollider2D = WaveCannonEffects[WaveCannonEffects.Count - 1].WaveColliders.FirstOrDefault().TryCast<PolygonCollider2D>();
            // 計算
            float PlayerPositionX = PlayerPosition.x;
            float MyLocalPositionX = transform.localPosition.x;
            if (IsFlipX)
                PlayerPositionX *= -1;
            if (IsFlipX)
                MyLocalPositionX *= -1;
            Vector3 newroopPosition = new(MyLocalPositionX + PlayerPositionX,
                renderer.transform.localPosition.y, renderer.transform.localPosition.z);

            // プレイヤーの位置と波動の距離を計算し、先頭の部分補正を入れる
            float distanceX = Vector2.Distance(new(PlayerPosition.x, 0f), new(renderer.transform.parent.position.x + transform.localScale.x * 2.45f, 0f));
            // なんやかんやで計算する。
            renderer.transform.localPosition = new(distanceX / 3f + 2.53f - 0.5f, 0);
            float sizeX = distanceX / (1.5f * transform.localScale.y) - 1f;
            renderer.size = new(sizeX,
                renderer.size.y);

            float maxPositionX = -1;
            List<int> maxPositions = [];
            int index = -1;
            foreach (Vector2 point in polygonCollider2D.points)
            {
                index++;
                if (point.x < maxPositionX)
                    continue;
                if (point.x != maxPositionX)
                {
                    maxPositions = [];
                    maxPositionX = point.x;
                }
                maxPositions.Add(index);
            }


            var newpoints = polygonCollider2D.points.ToList();
            // コライダーの判定を調整
            foreach (int posIndex in maxPositions)
            {
                newpoints[posIndex] = new(sizeX + 2.54724f, polygonCollider2D.points[posIndex].y);
            }

            polygonCollider2D.enabled = false;
            polygonCollider2D.points = newpoints.ToArray();
            polygonCollider2D.enabled = true;

            GameObject RotationEmptyParent = new("RotationEmptyParent");
            RotationEmptyParent.transform.SetParent(effectGameObjectsParent);

            WaveCannonEffect newEffect = CreateEffect();
            newEffect.transform.SetParent(RotationEmptyParent.transform, true);
            RotationEmptyParent.transform.localPosition = new Vector3(PlayerPositionX - MyLocalPositionX - 0.8f, 0f, newEffect.transform.localPosition.z); //newEffect.transform.localPosition.x - 1.5f, 0.5f);
            newEffect.transform.localPosition = Vector3.zero;

            RotationEmptyParent.transform.Rotate(new(0, 0, Angle));

            newEffect.SetChargeState(false);
            return newEffect.gameObject;
        }
    }
    public GameObject OldCreateRotationEffect(Vector3 PlayerPosition, float Angle)
    {
        return null;
        float posvalue = PlayerPosition.x - effectrenders[0].transform.parent.position.x;
        WCSantaHandler.reflection = true;
        WCSantaHandler.WiseManVector = PlayerPosition;
        WCSantaHandler.Angle = Angle;
        if (posvalue < 0) posvalue *= -1;
        WCSantaHandler.Xdiff = posvalue;
        effectrenders[0].transform.parent.localScale = new(posvalue * 0.0145f, 1, 1);
        SpriteRenderer effectrender = OldCreateEffect();
        GameObject effect = effectrender.transform.parent.gameObject;
        effect.transform.position = new(PlayerPosition.x, effect.transform.position.y, effect.transform.position.z + 0.1f);
        effect.transform.Rotate(new(0, 0, Angle));
        CreateCollider(effectrender);
        colliders.RemoveAt(0);
        CreateCollider(effectrenders[0]);
        return effect;
    }
    public WaveCannonEffect CreateEffect()
    {
        // Prefabを取得
        WaveCannonEffect Prefab = GetPrefab();

        // FIXME: どうにかする
        //if (Prefab == null)
        //    Prefab = WaveCannonObjectPrefab;

        // 生成して位置を調整
        WaveCannonEffect NewEffect = Instantiate(Prefab, effectGameObjectsParent);
        Vector3 pos = NewEffect.transform.localPosition;
        pos.z += 0.1f;
        NewEffect.transform.localPosition = pos;
        /*
        SpriteRenderer render = NewEffect.GetComponentInChildren<SpriteRenderer>();
        effectrenders.Add(render);*/
        NewEffect.SetChargeState(!IsShootNow);
        WaveCannonEffects.Add(NewEffect);
        foreach (Collider2D collider in NewEffect.WaveColliders)
            WaveColliders.Add(collider);
        return NewEffect;
    }
    public SpriteRenderer OldCreateEffect()
    {
        GameObject NewEffect = Instantiate(WaveCannonObjectPrefab, effectGameObjectsParent);
        Vector3 pos = NewEffect.transform.localPosition;
        pos.z += 0.1f;
        NewEffect.transform.localPosition = pos;
        SpriteRenderer render = NewEffect.GetComponentInChildren<SpriteRenderer>();
        effectrenders.Add(render);
        effectGameObjects.Add(render.gameObject);
        return render;
    }
    public PolygonCollider2D CreateCollider(SpriteRenderer render)
    {
        Sprite oldSprite = render.sprite;
        render.sprite = ((WCSantaAnimHandler)CurrentAnimationHandler).ColliderSprite;
        PolygonCollider2D collider = render.gameObject.AddComponent<PolygonCollider2D>();
        collider.isTrigger = true;
        render.sprite = oldSprite;
        colliders.Add(collider);
        return collider;
    }
    public void Shoot()
    {
        IsShootNow = IsShootNow = true;
        if (CurrentAnimType == WCAnimType.Bullet)
            Options.SetEffectSound(AssetManager.GetAsset<AudioClip>("BulletShootSound.ogg", AssetManager.AssetBundleType.Wavecannon), false, false);
        else
            Options.SetEffectSound(ModHelpers.loadAudioClipFromResources("SuperNewRoles.Resources.WaveCannon.ShootSound.raw"), false);

        foreach(WaveCannonEffect effect in WaveCannonEffects)
            effect.SetChargeState(false);

        CurrentAnimationHandler.OnShot();

        if (CurrentAnimType == WCAnimType.Santa)
        {
            OldShoot();
            return;
        }

        // 賢者の判定
        foreach (var data in WiseMan.WiseManData.ToArray())
        {
            if (data.Value == null) continue;
            PlayerControl player = ModHelpers.PlayerById(data.Key);
            if (player == null) continue;

            // 賢者が波動砲に触れているかを判定
            bool touching = false;
            foreach(Collider2D collider in WaveColliders)
            {
                if (!player.Collider.IsTouching(collider))
                    continue;
                touching = true;
            }
            if (!touching)
                continue;

            // 賢者ガード判定を削除
            WiseMan.WiseManData[player.PlayerId] = null;
            WiseMan.WiseManPosData[player] = null;
            if (player.PlayerId == PlayerControl.LocalPlayer.PlayerId)
            {
                HudManagerStartPatch.WiseManButton.isEffectActive = false;
                HudManagerStartPatch.WiseManButton.MaxTimer = WiseMan.WiseManCoolTime.GetFloat();
                HudManagerStartPatch.WiseManButton.Timer = HudManagerStartPatch.WiseManButton.MaxTimer;
                PlayerControl.LocalPlayer.moveable = true;
            }
            // 弾の場合、貫通させるためそのまま次へ
            if (CurrentAnimType == WCAnimType.Bullet)
                continue;
            // 方向を変えた波動を生成
            CreateRotationEffect(player.GetTruePosition(), data.Value.Value);
            // 賢者のバリアエフェクトを生成
            RoleEffectAnimation anim = Instantiate(DestroyableSingleton<RoleManager>.Instance.protectAnim, player.gameObject.transform);
            anim.Play(player, null, player.cosmetics.FlipX, RoleEffectAnimation.SoundType.Global);
            WiseManData[player] = (anim, 0.75f, player.transform.position);
            anim.Renderer.transform.localScale = new(1.1f, 1.6f, 1);
        }
    }
    public void OldShoot()
    {
        foreach (var data in WiseMan.WiseManData.ToArray())
        {
            if (data.Value is null) continue;
            PlayerControl player = ModHelpers.PlayerById(data.Key);
            if (player is null) continue;
            if (!player.Collider.IsTouching(colliders[0])) continue;
            OldCreateRotationEffect(player.GetTruePosition(), data.Value.Value);
            WiseMan.WiseManData[player.PlayerId] = null;
            WiseMan.WiseManPosData[player] = null;
            RoleEffectAnimation anim = Instantiate(DestroyableSingleton<RoleManager>.Instance.protectAnim, player.gameObject.transform);
            anim.Play(player, null, player.cosmetics.FlipX, RoleEffectAnimation.SoundType.Global);
            WiseManData[player] = (anim, 0.75f, player.transform.position);
            anim.Renderer.transform.localScale = new(1.1f, 1.6f, 1);
            if (player.PlayerId == PlayerControl.LocalPlayer.PlayerId)
            {
                HudManagerStartPatch.WiseManButton.isEffectActive = false;
                HudManagerStartPatch.WiseManButton.MaxTimer = WiseMan.WiseManCoolTime.GetFloat();
                HudManagerStartPatch.WiseManButton.Timer = HudManagerStartPatch.WiseManButton.MaxTimer;
                PlayerControl.LocalPlayer.moveable = true;
            }
        }
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        Objects.Remove(OwnerPlayerId);
        foreach (var data in WiseManData)
        {
            if (data.Key is null) continue;
            if (data.Value.Item1 is null) continue;
            if (data.Key.PlayerId == PlayerControl.LocalPlayer.PlayerId)
            {
                data.Key.moveable = true;
                Camera.main.GetComponent<FollowerCamera>().Locked = false;
            }
            foreach (var anim in data.Key.GetComponentsInChildren<RoleEffectAnimation>())
                if (anim is not null)
                    Destroy(anim.gameObject);
        }
        if (OwnerPlayerId == CachedPlayer.LocalPlayer.PlayerId)
        {
            CachedPlayer.LocalPlayer.PlayerControl.moveable = true;
            Camera.main.GetComponent<FollowerCamera>().Locked = false;
        }
    }
    public override void OnRenderUpdate()
    {
        if (IsShootNow)
            CurrentAnimationHandler.RendererUpdate();
    }
    public override void Update()
    {
        base.Update();
        if (Owner != null && (Owner.IsDead() || !(Owner.GetRole() is RoleId.WaveCannon or RoleId.WaveCannonJackal)))
        {
            Destroy(gameObject);
            return;
        }
        CurrentAnimationHandler.OnUpdate();
        foreach (var data in WiseManData.ToArray())
        {
            if (data.Key is null) continue;
            if (data.Value.Item1 is null) continue;
            data.Key.moveable = false;
            data.Key.transform.position = data.Value.Item3;
            if (data.Key.PlayerId == PlayerControl.LocalPlayer.PlayerId)
                Camera.main.GetComponent<FollowerCamera>().Locked = true;
            if (data.Value.Item2 > 0)
            {
                WiseManData[data.Key] = (data.Value.Item1, data.Value.Item2 - Time.fixedDeltaTime, data.Value.Item3);
                continue;
            }
            data.Value.Item1.Animator.Pause();
            data.Value.Item1.AudioSource.Pause();
        }
        if (Owner != null && OwnerPlayerId == PlayerControl.LocalPlayer.PlayerId && !RoleClass.IsMeeting)
        {
            //Owner.transform.position = OwnerPos;
            if (IsShootNow)
            {
                foreach (PlayerControl player in CachedPlayer.AllPlayers)
                {
                    if (player.IsDead()) continue;
                    if (WiseManData.ContainsKey(player)) continue;
                    if (CannotMurderPlayers.Contains(player.PlayerId)) continue;
                    if (player.PlayerId == PlayerControl.LocalPlayer.PlayerId) continue;

                    if (CurrentAnimType == WCAnimType.Santa)
                        foreach (Collider2D collider in colliders) if (internalCheckCollision(player, collider)) return;
                    else
                        foreach (Collider2D WaveCollider in WaveColliders) if (internalCheckCollision(player, WaveCollider)) return;
                }
            }
        }
        bool internalCheckCollision(PlayerControl player, Collider2D col)
        {
            if (!player.Collider.IsTouching(col)) return false;
            if (player.IsRole(RoleId.Shielder) && RoleClass.Shielder.IsShield.ContainsKey(player.PlayerId) && RoleClass.Shielder.IsShield[player.PlayerId])
            {
                MessageWriter msgwriter = RPCHelper.StartRPC(CustomRPC.ShielderProtect);
                msgwriter.Write(CachedPlayer.LocalPlayer.PlayerId);
                msgwriter.Write(player.PlayerId);
                msgwriter.Write(0);
                msgwriter.EndRPC();
                RPCProcedure.ShielderProtect(CachedPlayer.LocalPlayer.PlayerId, player.PlayerId, 0);
                CannotMurderPlayers.Add(player.PlayerId);
                return true;
            }
            MessageWriter writer = RPCHelper.StartRPC(CustomRPC.RPCMurderPlayer);
            writer.Write(CachedPlayer.LocalPlayer.PlayerId);
            writer.Write(player.PlayerId);
            writer.Write((byte)0);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            float Timer = PlayerControl.LocalPlayer.killTimer;
            RPCProcedure.RPCMurderPlayer(CachedPlayer.LocalPlayer.PlayerId, player.PlayerId, 0);
            player.RpcSetFinalStatus(FinalStatus.Evaporation);
            if (PlayerControl.LocalPlayer.IsImpostor())
            {
                PlayerControl.LocalPlayer.killTimer = Timer;
                FastDestroyableSingleton<HudManager>.Instance.KillButton.cooldownTimerText.text = PlayerControl.LocalPlayer.killTimer <= 0f ? "" : PlayerControl.LocalPlayer.killTimer.ToString();
            }
            return false;
        }
    }
}