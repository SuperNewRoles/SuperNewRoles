using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using HarmonyLib;
using SuperNewRoles.Modules;
using UnityEngine;

namespace SuperNewRoles.Patches;
public static class TrickOrTreat
{
    [HarmonyPatch(typeof(PlayerParticles), nameof(PlayerParticles.Start))]
    class PlayerParticlesStartPatch
    {
        // 1年以上起動している場合は正常に動作しないけど流石にそんな事するやつはいない
        private static readonly DateTime startDate = new(DateTime.UtcNow.Year, 10, 24, 9, 0, 0);
        private static readonly DateTime endDate = new(DateTime.UtcNow.Year, 11, 4, 9, 0, 0);

        private static bool forceEnabled = false;

        public static bool Prefix(PlayerParticles __instance)
        {
            // 毎年10月24日9時～11月4日9時まで有効(UTC)
            DateTime now = DateTime.UtcNow;

            // 期間外の場合は通常の処理に戻す
            if (!forceEnabled && (now < startDate || now >= endDate))
                return true;

            // 馬ングアスの場合は馬を見せる
            if (AprilFoolsMode.ShouldHorseAround())
                return true;

            int index = 1;
            foreach (PlayerParticleInfo info in __instance.Sprites)
            {
                info.image = AssetManager.GetAsset<Sprite>($"TrickOrTreat_0{index}.png");
                index++;
            }
            //以下バニラコード

            __instance.fill = new RandomFill<PlayerParticleInfo>();

            // 元のコードには馬ングアスのコードがあったけどここでは不要なので削除済み
            __instance.fill.Set(__instance.Sprites.IEnumerableToIl2Cpp());

            // バニラよりもいっぱい出す
            int AdditionalCount = ModHelpers.GetRandomInt(100, min: 40);
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