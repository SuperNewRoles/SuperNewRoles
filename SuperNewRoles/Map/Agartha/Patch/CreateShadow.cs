using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SuperNewRoles.Map.Agartha.Patch
{
    public static class CreateShadow
    {
        public static void Create(Transform Miraship)
        {
            Transform Cafe = Miraship.FindChild("Cafe").FindChild("Shadows");
            int i = 0;
            EdgeCollider2D CafeA = null;
            EdgeCollider2D CafeB = null;
            foreach (EdgeCollider2D col in Cafe.GetComponents<EdgeCollider2D>())
            {
                i++;
                if (i < 4)
                {
                    GameObject.Destroy(col);
                }
            }
            Cafe.gameObject.SetActive(true);
            EdgeCollider2D CafeCol = Cafe.GetComponent<EdgeCollider2D>();
            CafeCol.points = new Vector2[] { new Vector2(14.2914f, 21.2059f),new Vector2(16.6072f, 21.2059f),new Vector2(16.6135f, 9.1f),new Vector2(9.8247f, 9.1f), new Vector2(9.8247f, 15.1f) };
            EdgeCollider2D CafeCol2 = Cafe.gameObject.AddComponent<EdgeCollider2D>();
            CafeCol2.points = new Vector2[] { new Vector2(9.8053f, 16.0165f),new Vector2(9.8247f, 18.2745f),new Vector2(12.7073f, 20.5457f) };
        }
    }
}
