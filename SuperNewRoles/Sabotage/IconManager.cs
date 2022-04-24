using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SuperNewRoles.Sabotage
{
    public static class IconManager
    {
        private static Sprite CognitiveDeficitbuttonSprite;
        public static Sprite CognitiveDeficitgetButtonSprite()
        {
            if (CognitiveDeficitbuttonSprite) return CognitiveDeficitbuttonSprite;
            CognitiveDeficitbuttonSprite = ModHelpers.loadSpriteFromResources("SuperNewRoles.Resources.CognitiveDeficitButton.png", 115f);
            return CognitiveDeficitbuttonSprite;
        }
        private static Sprite BlizzardbuttonSprite;
        public static Sprite BlizzardgetButtonSprite()
        {
            if (BlizzardbuttonSprite) return BlizzardbuttonSprite;
            BlizzardbuttonSprite = ModHelpers.loadSpriteFromResources("SuperNewRoles.Resources.BlizzardButton.png", 115f);
            return BlizzardbuttonSprite;
        }
    }
}
