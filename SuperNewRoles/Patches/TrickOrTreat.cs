using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;

namespace SuperNewRoles.Patches;
public static class TrickOrTreat
{
    [HarmonyPatch(typeof(PlayerParticles), nameof(PlayerParticles.Start))]
    class PlayerParticlesStartPatch
    {
        public static void Prefix(PlayerParticles __instance)
        {
            //とりあえず僕の誕生日終わるまで出しとく
            if (DateTime.UtcNow >= new DateTime(2023, 11, 4, 15, 0, 0))
                return;
            int index = 1;
            foreach (PlayerParticleInfo info in __instance.Sprites)
            {
                info.image = ModHelpers.LoadSpriteFromResources($"SuperNewRoles.Resources.TrickOrTreat.Treat_0{index}.png", 115f);
                index++;
            }
        }
    }
}
