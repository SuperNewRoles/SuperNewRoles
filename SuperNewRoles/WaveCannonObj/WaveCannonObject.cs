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
        Default,
        Santa
    }
    public enum RpcType
    {
        Spawn,
        Shoot
    }
    public static readonly IReadOnlyDictionary<string, Func<WaveCannonObject, IWaveCannonAnimationHandler>> WCCreateAnimHandlers =
        new Dictionary<string, Func<WaveCannonObject, IWaveCannonAnimationHandler>>()
    {
        { WCAnimType.Default.ToString(), (waveCannon) => new WCDefaultAnimHandler(waveCannon) },
        { WCAnimType.Santa.ToString(), (waveCannon) => new WCSantaAnimHandler(waveCannon) }
    };
    public static PlayerData<WaveCannonObject> Objects = new();

    public Transform effectGameObjectsParent;
    public List<GameObject> effectGameObjects;

    public WCAnimType CurrentAnimType { get; private set; }
    public IWaveCannonAnimationHandler CurrentAnimationHandler { get; private set; }

    public PlayerControl Owner;
    public int Id;

    public List<SpriteRenderer> effectrenders { get; private set; }
    public byte OwnerPlayerId { get; private set; }
    public static Dictionary<byte, int> Ids;
    private static GameObject _waveCannonObjectPrefab;
    public static GameObject WaveCannonObjectPrefab
    {
        get
        {
            if (_waveCannonObjectPrefab is null)
            {
                var resourceAudioAssetBundleStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("SuperNewRoles.Resources.WaveCannon.WaveCannonEffects.bundle");
                var assetBundleBundle = AssetBundle.LoadFromMemory(resourceAudioAssetBundleStream.ReadFully());
                _waveCannonObjectPrefab = assetBundleBundle.LoadAsset<GameObject>("WaveCannonEffects.prefab").DontUnload();
            }
            return _waveCannonObjectPrefab;
        }
    }
    public Dictionary<PlayerControl, (RoleEffectAnimation, float, Vector3)> WiseManData;
    public bool IsShootNow;
    public bool IsFlipX;
    public int DestroyIndex { get; set; } = 0;
    static Vector3 OwnerPos;
    public List<PolygonCollider2D> colliders;
    public bool IsShootFirst;
    public List<byte> CannotMurderPlayers;
    public static List<(float, Vector2)> RotateSet = new() { (90, new(-3.05f, 25.5f)), (270, new(-4f, -26.5f)), (45, new(14.3f, 17.75f)), (45, new(14.3f, -17.75f)), (180, new(-30.7f, -0.8f)) };

    public override void Awake()
    {
        base.Awake();
        colliders = new();
        WiseManData = new();
        effectGameObjects = new();
        effectrenders = new();
        CannotMurderPlayers = new();
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
        CurrentAnimType = animType;
        Logger.Info("WaveCannon Animation:" + animType.ToString());
        CurrentAnimationHandler = CreateAnimHandler();
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
        CreateCollider(CreateEffect());
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
        //PlayerPosition.x -= 13;
        float posvalue = PlayerPosition.x - effectrenders[0].transform.parent.position.x;
        if (posvalue < 0)
            posvalue *= -1;
        effectrenders[0].transform.parent.localScale = new(posvalue * 0.0145f, 1, 1);
        SpriteRenderer effectrender = CreateEffect();
        GameObject effect = effectrender.transform.parent.gameObject;
        effect.transform.position = new(PlayerPosition.x, effect.transform.position.y, effect.transform.position.z + 0.1f);
        effect.transform.Rotate(new(0, 0, Angle));
        CreateCollider(effectrender);
        colliders.RemoveAt(0);
        CreateCollider(effectrenders[0]);
        return effect;
        //Position = new(effect.transform.position.x - PlayerPosition.x, effect.transform.position.y, effect.transform.position.z);
        //Position.x += RotateSet[index].Item2.x - 3;
        //Position.y += RotateSet[index].Item2.y;
        //Position.y -= PlayerPosition.y;
        //effect.transform.localPosition = Position;
        //effect.transform.Rotate(new(0,0, RotateSet[index].Item1));
        //Vector3 pos = effectGameObjects[0].transform.localScale;
        //pos.x = Position.x / 7.06997959f;
        //effectGameObjects[0].transform.localScale = pos;
        //pos = effectGameObjects[0].transform.localPosition;
        //pos.x -= 19f;
        //effectGameObjects[0].transform.localPosition = pos;
    }
    public SpriteRenderer CreateEffect()
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
        render.sprite = CurrentAnimationHandler.ColliderSprite;
        PolygonCollider2D collider = render.gameObject.AddComponent<PolygonCollider2D>();
        collider.isTrigger = true;
        render.sprite = oldSprite;
        colliders.Add(collider);
        return collider;
    }
    public void Shoot()
    {
        IsShootFirst = true;
        Options.SetEffectSound(ModHelpers.loadAudioClipFromResources("SuperNewRoles.Resources.WaveCannon.ShootSound.raw"), false);
        IsShootNow = true;

        CurrentAnimationHandler.OnShot();

        //
        foreach (var data in WiseMan.WiseManData.ToArray())
        {
            if (data.Value is null) continue;
            PlayerControl player = ModHelpers.PlayerById(data.Key);
            if (player is null) continue;
            if (!player.Collider.IsTouching(colliders[0])) continue;
            CreateRotationEffect(player.GetTruePosition(), data.Value.Value);
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
        foreach (var data in WiseManData.ToArray())
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
            //Owner.transform.position = OwnerPos;

            if (IsShootNow)
                foreach (PlayerControl player in CachedPlayer.AllPlayers)
                {
                    if (player.IsDead()) continue;
                    if (WiseManData.ContainsKey(player)) continue;
                    if (CannotMurderPlayers.Contains(player.PlayerId)) continue;
                    if (player.PlayerId == PlayerControl.LocalPlayer.PlayerId) continue;
                    //float posdata = player.GetTruePosition().y - transform.position.y;
                    //if (posdata is > 1 or < (-1)) continue;
                    //posdata = transform.position.x - (IsFlipX ? -2 : 2);
                    //if ((IsFlipX && player.transform.position.x > posdata) || (!IsFlipX && player.transform.position.x < posdata)) continue;
                    foreach (Collider2D col in colliders)
                    {
                        if (!player.Collider.IsTouching(col)) continue;
                        if (player.IsRole(RoleId.Shielder) && RoleClass.Shielder.IsShield.ContainsKey(player.PlayerId) && RoleClass.Shielder.IsShield[player.PlayerId])
                        {
                            MessageWriter msgwriter = RPCHelper.StartRPC(CustomRPC.ShielderProtect);
                            msgwriter.Write(CachedPlayer.LocalPlayer.PlayerId);
                            msgwriter.Write(player.PlayerId);
                            msgwriter.Write(0);
                            msgwriter.EndRPC();
                            RPCProcedure.ShielderProtect(CachedPlayer.LocalPlayer.PlayerId, player.PlayerId, 0);
                            CannotMurderPlayers.Add(player.PlayerId);
                            return;
                        }
                        MessageWriter writer = RPCHelper.StartRPC(CustomRPC.RPCMurderPlayer);
                        writer.Write(CachedPlayer.LocalPlayer.PlayerId);
                        writer.Write(player.PlayerId);
                        writer.Write((byte)0);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        float Timer = PlayerControl.LocalPlayer.killTimer;
                        RPCProcedure.RPCMurderPlayer(CachedPlayer.LocalPlayer.PlayerId, player.PlayerId, 0);
                        if (PlayerControl.LocalPlayer.IsImpostor())
                        {
                            PlayerControl.LocalPlayer.killTimer = Timer;
                            FastDestroyableSingleton<HudManager>.Instance.KillButton.cooldownTimerText.text = PlayerControl.LocalPlayer.killTimer <= 0f ? "" : PlayerControl.LocalPlayer.killTimer.ToString();
                        }
                    }
                }

    }
}