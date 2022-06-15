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
    }
}