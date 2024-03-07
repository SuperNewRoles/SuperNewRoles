using System;
using System.Linq;
using HarmonyLib;
using UnityEngine;

namespace SuperNewRoles.Patches;

class KillOverlayA
{
    [HarmonyPatch(typeof(OverlayKillAnimation), nameof(OverlayKillAnimation.Initialize))]
    public static class OverlayKillAnimationInitializePatch
    {
        public static void Postfix(OverlayKillAnimation __instance, GameData.PlayerInfo kInfo, GameData.PlayerInfo vInfo)
        {
            if (kInfo.PlayerId == vInfo.PlayerId)
            {
                if (!AprilFoolsMode.ShouldHorseAround())
                {
                    var anim = __instance.transform;
                    anim.transform.FindChild("Impostor").gameObject.SetActive(false);
                    //anim.transform.FindChild("killstabknife").gameObject.SetActive(false);
                    //anim.transform.FindChild("killstabknifehand").gameObject.SetActive(false);
                    anim.transform.FindChild("PetSlot").gameObject.SetActive(false);

                    anim.transform.FindChild("vInfo").localPosition = new(-1.15f, 0.2f, 0);
                    bool IsFirstEnd = false;
                    Transform pet = null;
                    for (int i = 0; i < anim.childCount; i++)
                    {
                        var child = anim.GetChild(i);
                        if (child.name == "PetSlot")
                        {
                            if (IsFirstEnd)
                            {
                                pet = child;
                                break;
                            }
                            IsFirstEnd = true;
                        }
                    }
                    pet.localPosition = new(-0.05f, -0.37f, 0.1f);
                }
            }
        }
    }
    [HarmonyPatch(typeof(KillOverlay), "ShowKillAnimation", new Type[] { typeof(OverlayKillAnimation), typeof(GameData.PlayerInfo), typeof(GameData.PlayerInfo) })]
    public static class KillOverlayShowKillAnimationPatch
    {
        public static void Prefix(KillOverlay __instance, ref OverlayKillAnimation killAnimation, GameData.PlayerInfo killer, GameData.PlayerInfo victim)
        {
            Logger.Info("Cohkoejhgmm\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\ned!");
            if (killer.PlayerId == victim.PlayerId)
            {
                //int index = ModHelpers.GetRandomIndex(__instance.KillAnims.ToList());
                //0を変えることで強制的にキルアニメーションが変わる
                var anim = __instance.KillAnims[3];
                killAnimation = anim;
            }
        }
    }
}