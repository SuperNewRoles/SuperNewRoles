using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using HarmonyLib;
using UnityEngine;

namespace SuperNewRoles.Patches;
public static class TrickOrTreat
{
    [HarmonyPatch(typeof(PlayerParticles), nameof(PlayerParticles.Start))]
    class PlayerParticlesStartPatch
    {
        public static bool Prefix(PlayerParticles __instance)
        {
            //とりあえず僕の誕生日終わるまで出しとく
            if (DateTime.UtcNow >= new DateTime(2023, 11, 4, 15, 0, 0))
                return true;
            int index = 1;
            foreach (PlayerParticleInfo info in __instance.Sprites)
            {
                info.image = ModHelpers.LoadSpriteFromResources($"SuperNewRoles.Resources.TrickOrTreat.Treat_0{index}.png", 115f);
                index++;
            }
            //以下バニラコード

            __instance.fill = new RandomFill<PlayerParticleInfo>();
            if (Constants.ShouldHorseAround())
            {
                __instance.fill.Set(__instance.HorseSprites.WrapToIl2Cpp().TryCast<Il2CppSystem.Collections.Generic.IEnumerable<PlayerParticleInfo>>());
                __instance.pool.Prefab = __instance.HorsePrefab;
            }
            else
            {
                __instance.fill.Set(__instance.Sprites.WrapToIl2Cpp().TryCast<Il2CppSystem.Collections.Generic.IEnumerable<PlayerParticleInfo>>());
            }
            foreach (PoolableBehavior beh in __instance.pool.inactiveChildren)
            {
                __instance.pool.inactiveChildren.Add(GameObject.Instantiate(beh, beh.transform.parent));
            }
            int num = 0;
            while (__instance.pool.NotInUse > 0)
            {
                PlayerParticle playerParticle = __instance.pool.Get<PlayerParticle>();
                __instance.PlacePlayer(playerParticle, initial: true);
            }
            return false;
        }
    }
}
