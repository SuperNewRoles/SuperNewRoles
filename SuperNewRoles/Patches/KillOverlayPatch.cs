using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using UnityEngine;

namespace SuperNewRoles.Patches
{
    class KillOverlayA
    {
        [HarmonyPatch(typeof(KillOverlay), "ShowKillAnimation")]
        public static class KillOverlay_ShowKillAnimation_Patch
        {
            [HarmonyPrefix]
            public static void Prefix(KillOverlay __instance, [HarmonyArgument(0)] GameData.PlayerInfo killer, [HarmonyArgument(1)] GameData.PlayerInfo victim, ref OverlayKillAnimation[] __state)
            {
                if (killer.PlayerId == victim.PlayerId)
                {
                    __state = __instance.KillAnims;
                    int index = ModHelpers.GetRandomIndex(__state.ToList());
                    Logger.Info(__state.Length.ToString() + ":" + index.ToString());
                    //0を変えることで強制的にキルアニメーションが変わる
                    var anim = __state[3];
                    __instance.KillAnims = new OverlayKillAnimation[1] { anim };
                    Logger.Info(__instance.KillAnims.Length.ToString());
                }
            }

            [HarmonyPostfix]
            public static void Postfix(KillOverlay __instance, [HarmonyArgument(0)] GameData.PlayerInfo killer, [HarmonyArgument(1)] GameData.PlayerInfo victim, OverlayKillAnimation[] __state)
            {
                if (killer.PlayerId == victim.PlayerId)
                {
                    if (!Constants.ShouldHorseAround())
                    {
                        var anim = __instance.transform.FindChild("PunchShootKill(Clone)");
                        anim.transform.FindChild("Impostor").gameObject.SetActive(false);
                        //anim.transform.FindChild("killstabknife").gameObject.SetActive(false);
                        //anim.transform.FindChild("killstabknifehand").gameObject.SetActive(false);
                        anim.transform.FindChild("PetSlot").gameObject.SetActive(false);

                        anim.transform.FindChild("Victim").localPosition = new(-1.15f, 0.2f, 0);
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
                        __instance.KillAnims = __state;
                    }
                }
            }
        }

    }
}