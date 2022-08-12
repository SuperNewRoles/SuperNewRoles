using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using UnhollowerRuntimeLib;
using UnityEngine;

namespace SuperNewRoles.Emote
{
    internal class Patch
    {
        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Start))]
        class PlayerPhysicsCoSpawnPlayerPatch {
            public static void Postfix(PlayerControl __instance)
            {
                var EmoteObj = new GameObject("EmoteObject");
                EmoteObj.transform.SetParent(__instance.cosmetics.currentBodySprite.BodySprite.transform);
                EmoteObj.AddComponent<SpriteRenderer>();
                new EmoteObject(EmoteObj);
            }
        }
        [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
        class HudManagerUpdatePatch
        {
            public static void Postfix(HudManager __instance)
            {
                foreach (EmoteObject obj in EmoteObject.EmoteObjects)
                {
                    obj.Update();
                }
            }
        }
        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.OnDestroy))]
        class PlayerControlOnDestroyPatch
        {
            public static void Prefix(PlayerControl __instance)
            {
                var obj = EmoteObject.EmoteObjects.Find(x => x.PlayerId == __instance.PlayerId);
                if (obj == null) return;
                obj.OnDestroy();
            }
        }
    }
}