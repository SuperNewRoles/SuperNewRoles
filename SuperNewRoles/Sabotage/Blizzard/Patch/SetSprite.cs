using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SuperNewRoles.Sabotage
{
    public static class SetSprite
    {
        private static Sprite ONDOSprite;
        public static Sprite ONDOgetSprite()
        {
            if (ONDOSprite) return ONDOSprite;
            ONDOSprite = ModHelpers.loadSpriteFromResources("SuperNewRoles.Resources.Blizzard.ONDO.png", 115f);
            return ONDOSprite;
        }
    }
}
