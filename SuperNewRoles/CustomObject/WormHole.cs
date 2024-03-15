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
    private static int MaxId;
    public int Id { get; private set; }
    public PlayerControl Owner { get; private set; }
    public float ActivateTimer { get; private set; }
    public TextMeshPro TimerText { get; private set; }
    public bool IsActivating { get; private set; }
    public Vent _vent { get; private set; }

    public const string ResourcePath = "WormHole_{0}.png";

    public WormHole(IntPtr intPtr) : base(intPtr)
    {
    }

    public WormHole Init(PlayerControl owner)
    {
        gameObject.transform.position = owner.GetTruePosition();
        gameObject.layer = 12; //ShortObjectにレイヤーを設定

        Id = MaxId++;
        Owner = owner;
        ActivateTimer = DimensionWalker.ActivateWormHoleTime.GetInt();
        TimerText = GameObject.Instantiate(FastDestroyableSingleton<HudManager>.Instance.ImpostorVentButton.buttonLabelText, gameObject.transform);
        //TimerText.gameObject.transform.localPosition = new();
        IsActivating = false;

        var tempVent = UnityEngine.Object.FindObjectOfType<Vent>();
        _vent = UnityEngine.Object.Instantiate<Vent>(tempVent, gameObject.transform);
        _vent.gameObject.transform.position = gameObject.transform.position;
        //_vent.myRend.sprite = ModHelpers.LoadSpriteFromResources(string.Format(ResourcePath, "0"), 125f);
        _vent.Id = MapUtilities.CachedShipStatus.AllVents.Select(x => x.Id).Max() + 1;
        _vent.Left = null;
        _vent.Right = null;
        _vent.Center = null;
        _vent.EnterVentAnim = null;
        _vent.ExitVentAnim = null;
        _vent.name = "WormHoleVent";
        _vent.GetComponent<PowerTools.SpriteAnim>()?.Stop();
        _vent.gameObject.SetActive(false);

        if (!(PlayerControl.LocalPlayer.GetRoleBase() is IImpostor || PlayerControl.LocalPlayer.IsImpostor()))
            TimerText.gameObject.SetActive(false);

        MapUtilities.AddVent(_vent);
        AllWormHoles.Add(this);
        return this;
    }

    public override void OnDestroy()
    {
        MapUtilities.RemoveVent(_vent);
        AllWormHoles.Remove(this);
    }

    public override void Update()
    {
        if (IsActivating)
            return;

        if (TimerText != null)
            TimerText.text = System.Math.Clamp(Mathf.CeilToInt(ActivateTimer), 0, DimensionWalker.ActivateWormHoleTime.GetInt()).ToString();

        if (ActivateTimer <= 0)
            Activate();
        else
            ActivateTimer -= Time.deltaTime;
    }

    public void Activate()
    {
        IsActivating = true;
        TimerText.gameObject.SetActive(false);
        gameObject.SetActive(true);
        _vent.gameObject.SetActive(true);

        ConnectVents();
    }

    private void ConnectVents()
    {
        //設置した人が同じ有効化済みワームホールをすべて検索 & リストに入れる
        List<WormHole> myHoles = AllWormHoles.Where(x => x.Owner == Owner && x.IsActivating).ToList();

        Logger.Info($"aaaaaaaaaaaaaaa{myHoles.Count()}");

        if (myHoles[0] != null) {
            myHoles[0]._vent.Left = myHoles[1]?._vent;
            myHoles[0]._vent.Right = myHoles[2]?._vent;
        }

        if (myHoles[1] != null) {
            myHoles[1]._vent.Left = myHoles[0]?._vent;
            myHoles[1]._vent.Right = myHoles[2]?._vent;
        }

        if (myHoles[2] != null) {
            myHoles[2]._vent.Left = myHoles[0]?._vent;
            myHoles[2]._vent.Right = myHoles[1]?._vent;
        }
    }

    public static GameObject GetWormHoleFromId(int ventId)
    {
        var vent = MapUtilities.CachedShipStatus.AllVents.FirstOrDefault(x => x.Id == ventId);
        return vent.gameObject.transform.parent.gameObject;
    }

    /*[HarmonyPatch(typeof(Vent), nameof(Vent.CanUse)), HarmonyPrefix]
    static bool canUse(Vent __instance, ref float __result, [HarmonyArgument(0)] GameData.PlayerInfo playerInfo, [HarmonyArgument(1)] out bool canUse, [HarmonyArgument(2)] out bool couldUse)
    {
        var player = playerInfo.PlayerId.GetPlayerControl();
        if (__instance.gameObject.name == "WormHoleVent" && !(player.IsImpostor() || player.GetRoleBase() is DimensionWalker)) {
            canUse = couldUse = false;
            __result = float.MaxValue;
            return false;
        }

        canUse = couldUse = true;
        return true;
    }*/
}