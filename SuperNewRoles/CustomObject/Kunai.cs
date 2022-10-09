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
            this.kunai = new GameObject("Kunai")
            {
                layer = 5
            };
            this.image = this.kunai.AddComponent<SpriteRenderer>();
            this.image.sprite = GetSprite();
        }
    }
}