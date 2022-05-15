using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SuperNewRoles.Sabotage
{
    public static class SetSprite
    {
        private static Sprite ONDOSprite;
        public static Sprite ONDOgetSprite(string id)
        {
            if (ONDOSprite) return ONDOSprite;
            ONDOSprite = ModHelpers.loadSpriteFromResources("SuperNewRoles.Resources.Blizzard.Temp." + id +".png", 115f);
            return ONDOSprite;
        }
    }
}
