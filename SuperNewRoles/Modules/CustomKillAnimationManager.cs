using System;
using System.Collections;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using HarmonyLib;
using PowerTools;

namespace SuperNewRoles.Modules;

public class CustomKillAnimationManager
{
    public static ICustomKillAnimation CurrentCustomKillAnimation { get; private set; }
    public static void SetCurrentCustomKillAnimation(ICustomKillAnimation customKillAnimation)
    {
        CurrentCustomKillAnimation = customKillAnimation;
    }
    public static void ClearCurrentCustomKillAnimation()
    {
        CurrentCustomKillAnimation = null;
    }
}
public interface ICustomKillAnimation
{
    public void Initialize(OverlayKillAnimation __instance, KillOverlayInitData initData);
    public bool FixedUpdate();
}
[HarmonyPatch(typeof(OverlayKillAnimation), nameof(OverlayKillAnimation.Initialize))]
public static class OverlayKillAnimationInitializePatch
{
    public static void Postfix(OverlayKillAnimation __instance)
    {
        if (CustomKillAnimationManager.CurrentCustomKillAnimation == null) return;
        __instance.killerParts.gameObject.SetActive(false);
        __instance.victimParts.gameObject.SetActive(false);
    }
}
[HarmonyPatch(typeof(OverlayKillAnimation), nameof(OverlayKillAnimation.CoShow))]
public static class OverlayKillAnimationCoShowPatch
{
    public static bool Prefix(OverlayKillAnimation __instance, KillOverlay parent, ref Il2CppSystem.Collections.IEnumerator __result)
    {
        if (CustomKillAnimationManager.CurrentCustomKillAnimation == null) return true;
        __result = ShowCustomKillAnimation(__instance, parent).WrapToIl2Cpp();
        return false;
    }
    private static IEnumerator ShowCustomKillAnimation(OverlayKillAnimation __instance, KillOverlay parent)
    {
        if (Constants.ShouldPlaySfx())
        {
            SoundManager.Instance.PlaySound(__instance.Stinger, loop: false).volume = __instance.StingerVolume;
        }
        for (int i = 0; i < __instance.transform.childCount; i++)
        {
            __instance.transform.GetChild(i).gameObject.SetActive(false);
        }
        parent.background.enabled = true;
        yield return Effects.Wait(1f / 12f);
        parent.background.enabled = false;
        parent.flameParent.SetActive(true);
        parent.flameParent.transform.localScale = new(1f, 0.3f, 1f);
        parent.flameParent.transform.localEulerAngles = new(0f, 0f, 25f);
        yield return Effects.Wait(1f / 12f);
        parent.flameParent.transform.localScale = new(1f, 0.5f, 1f);
        parent.flameParent.transform.localEulerAngles = new(0f, 0f, -15f);
        yield return Effects.Wait(1f / 12f);
        parent.flameParent.transform.localScale = new(1f, 1f, 1f);
        parent.flameParent.transform.localEulerAngles = new(0f, 0f, 0f);
        __instance.gameObject.SetActive(true);
        CustomKillAnimationManager.CurrentCustomKillAnimation.Initialize(__instance, __instance.initData);
        yield return null;
        while (CustomKillAnimationManager.CurrentCustomKillAnimation.FixedUpdate())
        {
            yield return null;
        }
        CustomKillAnimationManager.ClearCurrentCustomKillAnimation();
        __instance.gameObject.SetActive(false);
        yield return new WaitForLerp(1f / 6f, (Action<float>)((float t) =>
        {
            //IL_0021: Unknown result type (might be due to invalid IL or missing references)
            parent.flameParent.transform.localScale = new(1f, 1f - t, 1f);
        }));
        parent.flameParent.SetActive(false);
    }
}