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
            if (AprilFoolsMode.ShouldHorseAround())
                return true;
            int index = 1;
            foreach (PlayerParticleInfo info in __instance.Sprites)
            {
                info.image = ModHelpers.LoadSpriteFromResources($"SuperNewRoles.Resources.TrickOrTreat.Treat_0{index}.png", 115f);
                index++;
            }
            //以下バニラコード

            __instance.fill = new RandomFill<PlayerParticleInfo>();
            if (AprilFoolsMode.ShouldHorseAround())
            {
                __instance.fill.Set(__instance.HorseSprites.IEnumerableToIl2Cpp());
                __instance.pool.Prefab = __instance.HorsePrefab;
            }
            else
            {
                __instance.fill.Set(__instance.Sprites.IEnumerableToIl2Cpp());
            }
            int AdditionalCount = ModHelpers.GetRandomInt(15, min: 5);
            Logger.Info($"{AdditionalCount}お菓子増量キャンペーン中！");
            for (int i = 0; i < AdditionalCount; i++)
            {
                __instance.pool.CreateOneInactive(__instance.pool.Prefab);
            }
            int num = 0;
            Shader shader = Shader.Find("Sprites/Default");
            while (__instance.pool.NotInUse > 0)
            {
                PlayerParticle playerParticle = __instance.pool.Get<PlayerParticle>();
                playerParticle.myRend.material.shader = shader;
                playerParticle.myRend.sharedMaterial.shader = shader;
                __instance.PlacePlayer(playerParticle, initial: true);
            }
            return false;
        }
    }
}