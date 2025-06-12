using System;
using UnityEngine;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability.CustomButton;
using System.Collections.Generic;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using System.Linq;
using HarmonyLib;
using SuperNewRoles.CustomObject;

namespace SuperNewRoles.Roles.Ability;

public class CustomVentAbility : CustomButtonBase, IButtonEffect
{
    public Func<bool> CanUseVent { get; }
    public Func<float?> VentCooldown { get; }
    public Func<float?> VentDuration { get; }

    public override Sprite Sprite => HudManager.Instance?.ImpostorVentButton?.graphic?.sprite;
    public override string buttonText => FastDestroyableSingleton<TranslationController>.Instance.GetString(StringNames.VentLabel);
    protected override KeyType keytype => KeyType.Vent;
    public override float DefaultTimer => VentCooldown?.Invoke() ?? 0;
    public override bool IsFirstCooldownTenSeconds => DefaultTimer > 0.1f;

    public bool isEffectActive { get; set; }

    public Action OnEffectEnds => () => { if (VentDuration?.Invoke() != null && Vent.currentVent != null) exitVent(); };

    public float EffectDuration => VentDuration?.Invoke() ?? 0f;

    public float EffectTimer { get; set; }

    public bool effectCancellable => true;

    //public bool IsEffectDurationInfinity => VentDuration?.Invoke() == null;

    public CustomVentAbility(Func<bool> canUseVent, Func<float?> ventCooldown = null, Func<float?> ventDuration = null)
    {
        CanUseVent = canUseVent;
        VentCooldown = ventCooldown;
        VentDuration = ventDuration;
    }

    private void exitVent()
    {
        if (Vent.currentVent != null)
            Vent.currentVent.SetButtons(false);
        PlayerControl.LocalPlayer.MyPhysics.RpcExitVent(Vent.currentVent.Id);
    }

    public override void OnClick()
    {
        if (PlayerControl.LocalPlayer.inVent)
        {
            if (Vent.currentVent == null && PlayerControl.LocalPlayer.Visible)
                return;
            float num = Vector2.Distance(PlayerControl.LocalPlayer.Collider.bounds.center, Vent.currentVent.transform.position);
            // ベントに入っている途中に出れないように
            if (num < 10000)
            {
                exitVent();
            }
            return;
        }
        CurrentVent = SetVentTarget();
        if (CurrentVent != null)
        {
            // ベントの使用処理を実装
            PlayerControl.LocalPlayer.MyPhysics?.RpcEnterVent(CurrentVent.Id);
            CurrentVent.SetButtons(true);
        }
    }
    private static void SetVentOutline(Vent vent, bool show, Color32 color)
    {
        var rend = vent.myRend;
        if (rend == null) return;
        rend.material.SetFloat("_Outline", show ? 1f : 0f);
        if (show)
            rend.material.SetColor("_OutlineColor", color);
    }
    private int _lastCheckedCount;
    private bool? _lastCheckedResult;
    private Vent CurrentVent;
    private Vent SetVentTarget(float? distance = null)
    {
        Vector3 center = PlayerControl.LocalPlayer.Collider.bounds.center;
        foreach (Vent vent in ShipStatus.Instance.AllVents)
        {
            Vector3 position = vent.transform.position;
            float num = Vector2.Distance(center, position);
            if (distance.HasValue)
            {
                if (num > distance.Value)
                    continue;
            }
            else if (num > vent.UsableDistance)
                continue;
            if (!PhysicsHelpers.AnythingBetween(PlayerControl.LocalPlayer.Collider, center, position, Constants.ShipOnlyMask, useTriggers: false))
            {
                return vent;
            }
        }
        return null;
    }
    public override bool CheckIsAvailable()
    {
        if (PlayerControl.LocalPlayer.inVent)
        {
            if (Vent.currentVent == null)
                return false;
            SetVentOutline(Vent.currentVent, true, CustomRoleManager.GetRoleById(((ExPlayerControl)Player).Role)?.RoleColor ?? Palette.ImpostorRed);
            float num = Vector2.Distance(PlayerControl.LocalPlayer.Collider.bounds.center, Vent.currentVent.transform.position);
            return num < 10000;
        }
        // プレイヤーが移動不可または死亡している場合は使用不可
        if (!PlayerControl.LocalPlayer.CanMove || ExPlayerControl.LocalPlayer.IsDead())
            return false;
        // キャッシュされた結果が有効な場合はそれを返す
        if (IsValidCachedResult())
        {
            _lastCheckedCount--;
            return _lastCheckedResult ?? false;
        }

        // キャッシュをリセット
        ResetCache();

        // 使用可能なベントを探す
        var currentVent = SetVentTarget();
        if (CurrentVent != null && CurrentVent != currentVent)
        {
            SetVentOutline(CurrentVent, false, Palette.White);
        }
        if (currentVent != null)
        {
            SetVentOutline(currentVent, true, CustomRoleManager.GetRoleById(((ExPlayerControl)Player).Role)?.RoleColor ?? Palette.ImpostorRed);
            CurrentVent = currentVent;
            _lastCheckedResult = true;
        }
        else
        {
            // バニラでは、通報範囲にあるベントはOutlineが表示されるので
            var outfitVent = SetVentTarget(PlayerControl.LocalPlayer.MaxReportDistance);
            if (outfitVent != null)
            {
                CurrentVent = outfitVent;
                SetVentOutline(outfitVent, true, CustomRoleManager.GetRoleById(((ExPlayerControl)Player).Role)?.RoleColor ?? Palette.ImpostorRed);
            }
            _lastCheckedResult = false;
        }
        return _lastCheckedResult ?? false;
    }

    private bool IsValidCachedResult()
    {
        return _lastCheckedCount <= 10 &&
               _lastCheckedCount > 0 &&
               _lastCheckedResult != null;
    }

    private void ResetCache()
    {
        _lastCheckedCount = 10;
    }
    public override bool CheckHasButton()
    {
        return ExPlayerControl.LocalPlayer.IsAlive() && CheckCanUseVent();
    }

    public override void AttachToLocalPlayer()
    {
        base.AttachToLocalPlayer();
    }

    public bool CheckCanUseVent()
    {
        return CanUseVent();
    }
}
[HarmonyPatch(typeof(Vent), nameof(Vent.CanUse))]
public class VentSetButtonsPatch
{
    public static bool Prefix(Vent __instance, NetworkedPlayerInfo pc, ref float __result, out bool canUse, out bool couldUse)
    {
        canUse = couldUse = false;
        __result = 0;
        if (AmongUsClient.Instance.NetworkMode == NetworkModes.FreePlay) return true;
        if (!ExPlayerControl.LocalPlayer.CanUseVent())
        {
            canUse = couldUse = false;
            __result = 0;
            return false;
        }

        if (WormHole.IsWormHole(__instance) && !((ExPlayerControl)pc.Object).IsImpostor())
        {
            __result = float.MaxValue;
            canUse = false;
            couldUse = false;
            return false;
        }

        if (pc.Object.inVent && Vent.currentVent != null)
        {
            if (__instance.Id == Vent.currentVent.Id)
            {
                canUse = couldUse = true;
                __result = 0f;
                return false;
            }
            else
            {
                canUse = couldUse = false;
                __result = float.MaxValue;
                return false;
            }
        }
        canUse = couldUse = true;
        return true;
    }
}