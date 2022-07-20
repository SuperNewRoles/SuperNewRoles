using UnityEngine;

namespace SuperNewRoles.CustomObject
{
    public class Kunai
    {
        public SpriteRenderer image;
        public GameObject kunai;

        private static Sprite sprite;
        public static Sprite GetSprite()
        {
            if (sprite) return sprite;
            sprite = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.KunoichiKunai.png", 200f);
            return sprite;
        }

        public Kunai()
        {
            kunai = new GameObject("Kunai")
            {
                layer = 5
            };
            image = kunai.AddComponent<SpriteRenderer>();
            image.sprite = GetSprite();
        }
    }
}