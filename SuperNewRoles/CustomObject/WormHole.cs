using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Il2CppSystem.Linq.Expressions.Interpreter;
using Il2CppSystem.Runtime.Remoting.Messaging;
using SuperNewRoles.Roles.Impostor.DimensionWalker;
using SuperNewRoles.Roles.RoleBases;
using SuperNewRoles.Roles.RoleBases.Interfaces;
using TMPro;
using UnityEngine;
using UnityEngine.ResourceManagement.Util;

namespace SuperNewRoles.CustomObject;

/**
ディメンションウォーカー用のワームホール
**/
[HarmonyPatch]
class WormHole : CustomAnimation
{
    public static List<WormHole> AllWormHoles = new();
    public int Id { get; private set; }
    public PlayerControl Owner { get; private set; }
    public float ActivateTimer { get; private set; }
    public TextMeshPro TimerText { get; private set; }
    public bool IsActivating { get; private set; }
    public Vent _vent { get; private set; }

    private const string ResourcePath_Use = "SuperNewRoles.Resources.DimensionWalker.Animation.Use.DimensionWalkerOpen";
    private const string ResourcePath_Idle = "SuperNewRoles.Resources.DimensionWalker.Animation.Idle.DimensionWalkerIdle";
    private static CustomAnimationOptions animOption_Idle = new(GetSprites(ResourcePath_Idle, 60, 2), 30, true, IsMeetingDestroy: false);

    public WormHole(IntPtr intPtr) : base(intPtr)
    {
    }

    public WormHole Init(PlayerControl owner)
    {
        Vector3 pos = owner.GetTruePosition();
        //gameObject.transform.SetParent(ShipStatus.Instance.gameObject.transform);
        gameObject.transform.position = new(pos.x, pos.y, pos.z + 0.1f);
        gameObject.transform.localScale = new(1f, 1f);
        gameObject.layer = 12; //ShortObjectにレイヤーを設定

        Owner = owner;
        ActivateTimer = DimensionWalker.ActivateWormHoleTime.GetInt();
        TimerText = GameObject.Instantiate(FastDestroyableSingleton<HudManager>.Instance.ImpostorVentButton.buttonLabelText, gameObject.transform);
        IsActivating = false;

        var tempVent = UnityEngine.Object.FindObjectOfType<Vent>();
        _vent = UnityEngine.Object.Instantiate<Vent>(tempVent, gameObject.transform);
        //_vent.transform.localScale = Vector3.Scale(ShipStatus.Instance.transform.lossyScale, tempVent.transform.lossyScale);
        _vent.gameObject.transform.position = gameObject.transform.position;
        _vent.Id = MapUtilities.CachedShipStatus.AllVents.Select(x => x.Id).Max() + 1;
        _vent.Left = null;
        _vent.Right = null;
        _vent.Center = null;
        _vent.EnterVentAnim = null;
        _vent.ExitVentAnim = null;
        _vent.name = "WormHoleVent";
        _vent.GetComponent<PowerTools.SpriteAnim>()?.Stop();
        _vent.myRend.enabled = false;
        _vent.gameObject.SetActive(false);
        TimerText.color = Palette.DisabledClear;
        TimerText.transform.position = gameObject.transform.position;
        spriteRenderer.color = Palette.DisabledClear;
        Id = _vent.Id;

        if (!PlayerControl.LocalPlayer.IsImpostor()) {
            TimerText.gameObject.SetActive(false);
            spriteRenderer.enabled = false;
        }

        MapUtilities.AddVent(_vent);
        AllWormHoles.Add(this);
        return this;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        MapUtilities.RemoveVent(_vent);
        AllWormHoles.Remove(this);
    }

    public override void Update()
    {
        if (!gameObject.active)
            return;

        base.Update();

        if (IsActivating)
            return;

        if (TimerText != null)
            TimerText.text = System.Math.Clamp(Mathf.CeilToInt(ActivateTimer), 0, DimensionWalker.ActivateWormHoleTime.GetInt()).ToString();

        if (ActivateTimer <= 0)
            Activate();
        else
            ActivateTimer -= Time.deltaTime;
    }

    private void Activate()
    {
        IsActivating = true;

        TimerText.color = Palette.EnabledColor;
        spriteRenderer.color = Palette.EnabledColor;

        _vent.gameObject.SetActive(true);
        TimerText.gameObject.SetActive(false);
        spriteRenderer.enabled = true;

        //base.Init(new(GetSprites(ResourcePath_Use, 15, 2), 30, true));
        Init(animOption_Idle);

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
        //設置した人が同じかつ、有効化済みのワームホールをすべて検索 & リストに
        List<WormHole> myHoles = AllWormHoles.Where(x => x.Owner == Owner && x.IsActivating).ToList();

        if (myHoles is null)
            return;

        for (var i = 0; i < myHoles.Count - 1; i++) {
            var left = myHoles[i];
            var right = myHoles[i + 1];
            left._vent.Right = right._vent;
            right._vent.Left = left._vent;
        }

        myHoles.First()._vent.Left = myHoles.Last()._vent;
        myHoles.Last()._vent.Right = myHoles.First()._vent;
    }

    public static WormHole GetWormHoleById(int ventId)
        => AllWormHoles.FirstOrDefault(x => x.Id == ventId);

    public static bool IsWormHole(Vent vent)
        => vent.gameObject.name == "WormHoleVent";

    // Useボタンのターゲットがあるときにベントに入るとそのままUseボタンが押せてしまう問題を強引に修正
    [HarmonyPatch(typeof(VentButton), nameof(VentButton.DoClick)), HarmonyPostfix]
    static void useButtonTargetReset()
        => HudManager.Instance.UseButton.currentTarget = null;

    [HarmonyPatch(typeof(Vent), nameof(Vent.EnterVent)), HarmonyPostfix]
    static void enterVent(Vent __instance, [HarmonyArgument(0)] PlayerControl pc) {
        if (!IsWormHole(__instance))
            return;
        GetWormHoleById(__instance.Id).playUseAnimation(pc);
    }

    [HarmonyPatch(typeof(Vent), nameof(Vent.ExitVent)), HarmonyPostfix]
    static void exitVent(Vent __instance, [HarmonyArgument(0)] PlayerControl pc) {
        if (!IsWormHole(__instance))
            return;
        GetWormHoleById(__instance.Id).playUseAnimation(pc);
    }

    private void playUseAnimation(PlayerControl user)
    {
        if (!DimensionWalker.DoPlayWormHoleAnimation.GetBool() && !user.AmOwner)
            return;
        Init(new CustomAnimationOptions(GetSprites(ResourcePath_Use, 15, 2), 30, false, OnEndAnimation:(anim, option) => base.Init(animOption_Idle), IsMeetingDestroy: false));
    }
}