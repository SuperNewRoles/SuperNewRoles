using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Hazel;
using SuperNewRoles.Buttons;
using SuperNewRoles.Helpers;
using SuperNewRoles.Roles;
using SuperNewRoles.Roles.Crewmate;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace SuperNewRoles.CustomObject;

public class WaveCannonObject
{
    public enum RpcType
    {
        Spawn,
        Shoot
    }
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
    class FixedUpdatePatch
    {
        public static void Postfix(PlayerControl __instance)
        {
            if (__instance.PlayerId == PlayerControl.LocalPlayer.PlayerId)
            {
                AllFixedUpdate();
            }
        }
    }
    public static List<WaveCannonObject> Objects = new();

    public GameObject gameObject;
    public Transform effectGameObjectsParent;
    public List<GameObject> effectGameObjects;
    public Transform transform => gameObject.transform;

    private static Sprite _colliderSprite;
    public static Sprite ColliderSprite {
        get
        {
            if (_colliderSprite is null)
            {
                _colliderSprite = ModHelpers.LoadSpriteFromResources($"SuperNewRoles.Resources.WaveCannon.Shoot_0004.png", 115f);
            }
            return _colliderSprite;
        }
    }

    public PlayerControl Owner;
    public int Id;
    private List<Sprite> sprites;
    private float UpdateTime;
    private float DefaultUpdateTime => 1f / freamrate;
    private int freamrate;
    private int index;
    private bool IsLoop;
    private bool Playing;
    private readonly SpriteRenderer render;
    private readonly List<SpriteRenderer> effectrenders;
    private readonly byte OwnerPlayerId;
    private Action OnPlayEnd;
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
    private int DestroyIndex = 0;
    static Vector3 OwnerPos;
    static AudioSource ChargeSound;
    public List<PolygonCollider2D> colliders;
    public static List<(float, Vector2)> RotateSet = new() { (90, new(-3.05f, 25.5f)), (270, new(-4f, -26.5f)), (45, new(14.3f, 17.75f)), (45, new(14.3f, -17.75f)), (180, new(-30.7f, -0.8f)) };

    public WaveCannonObject(Vector3 pos, bool FlipX, PlayerControl _owner)
    {
        OwnerPlayerId = _owner.PlayerId;
        if (OwnerPlayerId == CachedPlayer.LocalPlayer.PlayerId)
        {
            CachedPlayer.LocalPlayer.PlayerControl.moveable = false;

            Camera.main.GetComponent<FollowerCamera>().Locked = true;
        }
        colliders = new();
        WiseManData = new();
        effectGameObjects = new();
        effectrenders = new();
        OwnerPos = _owner.transform.position;
        IsFlipX = FlipX;
        Owner = _owner;
        Id = 0;
        gameObject = new("WaveCannonObject");
        render = gameObject.AddComponent<SpriteRenderer>();
        index = 0;
        sprites = new();
        for (int i = 1; i <= 5; i++)
        {
            sprites.Add(ModHelpers.LoadSpriteFromResources($"SuperNewRoles.Resources.WaveCannon.Charge_000{i}.png", 115f));
        }
        render.sprite = sprites[0];
        IsLoop = true;
        freamrate = 25;
        Playing = true;
        Objects.Add(this);
        pos.z -= 0.0003f;
        transform.position = pos + new Vector3(FlipX ? -4 : 4, 0, 0);
        transform.localScale = new(FlipX ? -1 : 1, 1, 1);
        effectGameObjectsParent = new GameObject("WaveCannonEffects").transform;
        effectGameObjectsParent.SetParent(transform);
        effectGameObjectsParent.localPosition = new(31.25f, -1.45f, 2.39f);
        effectGameObjectsParent.localScale = new(1, 1, 1);
        CreateCollider(CreateEffect());
        if (!Ids.ContainsKey(OwnerPlayerId))
        {
            Ids[OwnerPlayerId] = 0;
        }
        Id = Ids[OwnerPlayerId];
        Ids[OwnerPlayerId]++;
        IsShootNow = false;
        ChargeSound = SoundManager.Instance.PlaySound(ModHelpers.loadAudioClipFromResources("SuperNewRoles.Resources.WaveCannon.ChargeSound.raw"), true);
    }
    public GameObject CreateRotationEffect(Vector3 PlayerPosition, float Angle)
    {
        //PlayerPosition.x -= 13;
        float posvalue = PlayerPosition.x - effectrenders[0].transform.parent.position.x;
        if (posvalue < 0)
        {
            posvalue *= -1;
        }
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
        GameObject NewEffect = GameObject.Instantiate(WaveCannonObjectPrefab, effectGameObjectsParent);
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
        render.sprite = ColliderSprite;
        PolygonCollider2D collider = render.gameObject.AddComponent<PolygonCollider2D>();
        collider.isTrigger = true;
        render.sprite = oldSprite;
        colliders.Add(collider);
        return collider;
    }
    public void Shoot()
    {
        if (ChargeSound != null)
            ChargeSound.Stop();
        SoundManager.Instance.PlaySound(ModHelpers.loadAudioClipFromResources("SuperNewRoles.Resources.WaveCannon.ShootSound.raw"), false);
        IsShootNow = true;
        render.sprite = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.WaveCannon.Cannon.png", 115f);
        sprites = new();
        for (int i = 1; i <= 12; i++)
        {
            sprites.Add(ModHelpers.LoadSpriteFromResources($"SuperNewRoles.Resources.WaveCannon.Shoot_00{(i <= 9 ? "0" : "")}{i}.png", 115f));
        }
        foreach (var obj in effectrenders) obj.sprite = sprites[0];
        IsLoop = false;
        freamrate = 12;
        Playing = true;
        OnPlayEnd = () =>
        {
            IsLoop = true;
            freamrate = 15;
            Playing = true;
            sprites = new();
            for (int i = 6; i <= 12; i++)
            {
                sprites.Add(ModHelpers.LoadSpriteFromResources($"SuperNewRoles.Resources.WaveCannon.Shoot_00{(i <= 9 ? "0" : "")}{i}.png", 115f));
            }
            foreach (var obj in effectrenders) obj.sprite = sprites[0];
            OnPlayEnd = () =>
            {
                DestroyIndex++;
                if (DestroyIndex > 3)
                {
                    OnDestroy();
                    GameObject.Destroy(this.gameObject);
                    if (OwnerPlayerId == CachedPlayer.LocalPlayer.PlayerId)
                    {
                        if (PlayerControl.LocalPlayer.IsRole(RoleId.WaveCannon))
                        {
                            if (CustomOptionHolder.WaveCannonIsSyncKillCoolTime.GetBool())
                                PlayerControl.LocalPlayer.SetKillTimer(RoleHelpers.GetCoolTime(PlayerControl.LocalPlayer));
                        }
                        else
                        {
                            if (CustomOptionHolder.WaveCannonJackalIsSyncKillCoolTime.GetBool())
                                Roles.Neutral.WaveCannonJackal.ResetCooldowns();
                        }
                        CachedPlayer.LocalPlayer.PlayerControl.moveable = true;
                        Camera.main.GetComponent<FollowerCamera>().Locked = false;
                        HudManagerStartPatch.WaveCannonButton.MaxTimer = PlayerControl.LocalPlayer.IsRole(RoleId.WaveCannon) ? CustomOptionHolder.WaveCannonCoolTime.GetFloat() : CustomOptionHolder.WaveCannonJackalCoolTime.GetFloat();
                        HudManagerStartPatch.WaveCannonButton.Timer = HudManagerStartPatch.WaveCannonButton.MaxTimer;
                        RoleClass.WaveCannon.CannotMurderPlayers = new();
                    }
                }
            };
        };
        foreach (var data in WiseMan.WiseManData.ToArray())
        {
            if (data.Value is null) continue;
            PlayerControl player = ModHelpers.PlayerById(data.Key);
            if (player is null) continue;
            if (!player.Collider.IsTouching(colliders[0])) continue;
            CreateRotationEffect(player.GetTruePosition(), data.Value.Value);
            WiseMan.WiseManData[player.PlayerId] = null;
            WiseMan.WiseManPosData[player] = null;
            RoleEffectAnimation anim = UnityEngine.Object.Instantiate(DestroyableSingleton<RoleManager>.Instance.protectAnim, player.gameObject.transform);
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
    
    public static void AllFixedUpdate()
    {
        if (Objects.Count <= 0) return;
        foreach (WaveCannonObject obj in Objects.ToArray())
        {
            obj.FixedUpdate();
        }
    }
    public void OnDestroy()
    {
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
            {
                if (anim is not null)
                {
                    GameObject.Destroy(anim.gameObject);
                }
            }
        }
    }
    public void FixedUpdate()
    {
        if (render == null) { Objects.Remove(this); return; }
        if (RoleClass.IsMeeting)
        {
            if (ChargeSound != null)
                ChargeSound.Stop();
            OnDestroy();
            GameObject.Destroy(effectGameObjectsParent.gameObject);
            GameObject.Destroy(gameObject);
            return;
        }
        if (Owner != null && (Owner.IsDead() || !(Owner.GetRole() is RoleId.WaveCannon or RoleId.WaveCannonJackal)))
        {
            OnDestroy();
            GameObject.Destroy(this.gameObject);
            if (OwnerPlayerId == CachedPlayer.LocalPlayer.PlayerId)
            {
                CachedPlayer.LocalPlayer.PlayerControl.moveable = true;
                Camera.main.GetComponent<FollowerCamera>().Locked = false;
            }
            if (ChargeSound != null)
                ChargeSound.Stop();
            return;
        }
        foreach (var data in WiseManData.ToArray())
        {
            if (data.Key is null) continue;
            if (data.Value.Item1 is null) continue;
            data.Key.moveable = false;
            data.Key.transform.position = data.Value.Item3;
            if (data.Key.PlayerId == PlayerControl.LocalPlayer.PlayerId)
            {
                Camera.main.GetComponent<FollowerCamera>().Locked = true;
            }
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
                    if (RoleClass.WaveCannon.CannotMurderPlayers.Contains(player.PlayerId)) continue;
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
                            RoleClass.WaveCannon.CannotMurderPlayers.Add(player.PlayerId);
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
        if (Playing)
        {
            UpdateTime -= Time.fixedDeltaTime;
            if (UpdateTime <= 0)
            {
                index++;
                if (index >= sprites.Count)
                {
                    index = 0;
                    if (OnPlayEnd != null && IsLoop) OnPlayEnd();
                    if (!IsLoop)
                    {
                        Playing = false;
                        OnPlayEnd?.Invoke();
                        return;
                    }
                }
                UpdateTime = DefaultUpdateTime;
                if (IsShootNow) foreach (var obj in effectrenders) obj.sprite = sprites[index];
                else render.sprite = sprites[index];
            }
        }
    }
}