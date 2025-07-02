using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using SuperNewRoles.CustomObject;
using SuperNewRoles.Modules;
using TMPro;
using UnityEngine;

namespace SuperNewRoles.CustomObject;

[HarmonyPatch]
public class WormHole : MonoBehaviour
{
    public static List<WormHole> AllWormHoles = new();
    public int Id { get; private set; }
    public PlayerControl Owner { get; private set; }
    public float ActivateTimer { get; private set; }
    public TextMeshPro TimerText { get; private set; }
    public bool IsActivating { get; private set; }
    public Vent _vent { get; private set; }
    public bool PlayAnimation { get; private set; }

    private SpriteRenderer spriteRenderer;

    // アニメーション関連
    private Sprite[] idleSprites;
    private Sprite[] useSprites;
    private bool isAnimating = false;
    private float animationTimer = 0f;
    private int currentFrame = 0;
    private float frameRate = 30f;
    private bool isUseAnimation = false;

    public WormHole(IntPtr intPtr) : base(intPtr)
    {
    }

    public WormHole Init(PlayerControl owner, float activateTime, bool playAnimation)
    {
        Vector3 pos = owner.GetTruePosition();
        gameObject.transform.position = new(pos.x, pos.y, pos.z + 0.1f);
        gameObject.transform.localScale = new(1f, 1f);
        gameObject.layer = 12; // ShortObjectにレイヤーを設定

        Owner = owner;
        ActivateTimer = activateTime;
        PlayAnimation = playAnimation;
        IsActivating = false;

        // SpriteRendererを追加
        spriteRenderer = gameObject.AddComponent<SpriteRenderer>();

        // アニメーション用スプライトを読み込み
        LoadAnimationSprites();

        spriteRenderer.sprite = idleSprites != null && idleSprites.Length > 0 ? idleSprites[0] : null;
        spriteRenderer.color = Palette.DisabledClear;

        // タイマーテキストを作成
        TimerText = GameObject.Instantiate(FastDestroyableSingleton<HudManager>.Instance.ImpostorVentButton.buttonLabelText, gameObject.transform);
        TimerText.color = Palette.DisabledClear;
        TimerText.transform.position = gameObject.transform.position;

        // ベントを作成
        var tempVent = UnityEngine.Object.FindObjectOfType<Vent>();
        _vent = UnityEngine.Object.Instantiate<Vent>(tempVent, gameObject.transform);
        _vent.gameObject.transform.position = gameObject.transform.position;
        _vent.Id = ShipStatus.Instance.AllVents.Select(x => x.Id).Max() + 1;
        _vent.Left = null;
        _vent.Right = null;
        _vent.Center = null;
        _vent.EnterVentAnim = null;
        _vent.ExitVentAnim = null;
        _vent.name = "WormHoleVent";
        _vent.GetComponent<PowerTools.SpriteAnim>()?.Stop();
        _vent.myRend.enabled = false;
        _vent.gameObject.SetActive(false);

        Id = _vent.Id;

        // インポスター以外には見えないようにする
        if (!ExPlayerControl.LocalPlayer.IsImpostor())
        {
            TimerText.gameObject.SetActive(false);
            spriteRenderer.enabled = false;
        }

        AddVent(_vent);
        AllWormHoles.Add(this);
        return this;
    }

    private void OnDestroy()
    {
        RemoveVent(_vent);
        AllWormHoles.Remove(this);
    }

    private static void AddVent(Vent vent)
    {
        var allVents = ShipStatus.Instance.AllVents.ToList();
        allVents.Add(vent);
        ShipStatus.Instance.AllVents = allVents.ToArray();
    }

    private static void RemoveVent(Vent vent)
    {
        var allVents = ShipStatus.Instance.AllVents.ToList();
        allVents.Remove(vent);
        ShipStatus.Instance.AllVents = allVents.ToArray();
    }

    private void LoadAnimationSprites()
    {
        idleSprites = CustomPlayerAnimationSimple.GetSprites("DimensionWalkerIdle_{0}.png", 1, 60, zeroPadding: 1);
        useSprites = CustomPlayerAnimationSimple.GetSprites("DimensionWalkerOpen_{0}.png", 1, 14, zeroPadding: 1);
    }

    private void Update()
    {
        if (!gameObject.activeInHierarchy)
            return;

        // アニメーション更新
        UpdateAnimation();

        if (IsActivating)
            return;

        if (TimerText != null)
            TimerText.text = Mathf.Max(0, Mathf.CeilToInt(ActivateTimer)).ToString();

        if (ActivateTimer <= 0)
            Activate();
        else
            ActivateTimer -= Time.deltaTime;
    }

    private void UpdateAnimation()
    {
        if (!isAnimating || spriteRenderer == null)
            return;

        animationTimer += Time.deltaTime;
        float frameTime = 1f / frameRate;

        if (animationTimer >= frameTime)
        {
            animationTimer = 0f;
            currentFrame++;

            Sprite[] currentSprites = isUseAnimation ? useSprites : idleSprites;

            if (currentSprites != null && currentSprites.Length > 0)
            {
                if (currentFrame >= currentSprites.Length)
                {
                    if (isUseAnimation)
                    {
                        // 使用アニメーション終了後はアイドルに戻る
                        StartIdleAnimation();
                    }
                    else
                    {
                        // アイドルアニメーションはループ
                        currentFrame = 0;
                    }
                }

                if (currentFrame < currentSprites.Length)
                {
                    spriteRenderer.sprite = currentSprites[currentFrame];
                }
            }
        }
    }

    private void StartIdleAnimation()
    {
        isAnimating = true;
        isUseAnimation = false;
        currentFrame = 0;
        animationTimer = 0f;
    }

    private void StartUseAnimation()
    {
        if (!PlayAnimation)
            return;

        isAnimating = true;
        isUseAnimation = true;
        currentFrame = 0;
        animationTimer = 0f;
    }

    private void Activate()
    {
        IsActivating = true;

        TimerText.color = Palette.EnabledColor;
        spriteRenderer.color = Palette.EnabledColor;

        _vent.gameObject.SetActive(true);
        TimerText.gameObject.SetActive(false);
        spriteRenderer.enabled = true;

        // アイドルアニメーションを開始
        StartIdleAnimation();

        ConnectVents();
    }

    public void InActivate()
    {
        IsActivating = false;
        gameObject.SetActive(false);
        AllWormHoles.Remove(this);

        ConnectVents();
    }

    private void ConnectVents()
    {
        // 設置した人が同じかつ、有効化済みのワームホールをすべて検索 & リストに
        List<WormHole> myHoles = AllWormHoles.Where(x => x.Owner == Owner && x.IsActivating).ToList();

        if (myHoles == null || myHoles.Count == 0)
            return;

        foreach (var h in myHoles)
        {
            h._vent.Left = null;
            h._vent.Right = null;
        }

        for (var i = 0; i < myHoles.Count - 1; i++)
        {
            var left = myHoles[i];
            var right = myHoles[i + 1];
            left._vent.Right = right._vent;
            right._vent.Left = left._vent;
        }

        if (myHoles.Count > 1)
        {
            myHoles.First()._vent.Left = myHoles.Last()._vent;
            myHoles.Last()._vent.Right = myHoles.First()._vent;
        }
    }

    public static WormHole GetWormHoleById(int ventId)
        => AllWormHoles.FirstOrDefault(x => x.Id == ventId);

    public static bool IsWormHole(Vent vent)
        => vent.gameObject.name == "WormHoleVent";

    [HarmonyPatch(typeof(VentButton), nameof(VentButton.DoClick)), HarmonyPostfix]
    static void useButtonTargetReset()
        => HudManager.Instance.UseButton.currentTarget = null;

    [HarmonyPatch(typeof(Vent), nameof(Vent.EnterVent)), HarmonyPostfix]
    static void enterVent(Vent __instance, [HarmonyArgument(0)] PlayerControl pc)
    {
        if (!IsWormHole(__instance))
            return;
        GetWormHoleById(__instance.Id)?.playUseAnimation(pc);
    }

    [HarmonyPatch(typeof(Vent), nameof(Vent.ExitVent)), HarmonyPostfix]
    static void exitVent(Vent __instance, [HarmonyArgument(0)] PlayerControl pc)
    {
        if (!IsWormHole(__instance))
            return;
        GetWormHoleById(__instance.Id)?.playUseAnimation(pc);
    }

    private void playUseAnimation(PlayerControl user)
    {
        if (!PlayAnimation && !user.AmOwner)
            return;

        // 使用アニメーションを再生
        StartUseAnimation();
    }
}