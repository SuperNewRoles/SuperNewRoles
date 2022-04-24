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
            foreach (EdgeCollider2D col in Cafe.GetComponents<EdgeCollider2D>())
            {
                i++;
                if (i < 5)
                {
                    GameObject.Destroy(col);
                }
            }
            Cafe.gameObject.SetActive(true);
            EdgeCollider2D CafeCol = Cafe.gameObject.AddComponent<EdgeCollider2D>();
            CafeCol.points = new Vector2[] { new Vector2(1f, 4.95f),new Vector2(3.85f, 4.95f),new Vector2(3.85f, -4.6f), new Vector2(-3.55f, -4.6f), new Vector2(-3.55f, -1.8f) };
            EdgeCollider2D CafeCol2 = Cafe.gameObject.AddComponent<EdgeCollider2D>();
            CafeCol2.points = new Vector2[] { new Vector2(-3.42f, -0.4f), new Vector2(-3.2f, 3.3f), new Vector2(-1.65f, 5f), new Vector2(-0.3f, 5f) };

            EdgeCollider2D ElecCol = Cafe.gameObject.AddComponent<EdgeCollider2D>();
            ElecCol.points = new Vector2[] { new Vector2(10.5f, 5.1f),new Vector2(10.5f, 4.7f), new Vector2(11.6f, 4.7f), new Vector2(11.6f, -5f), 
                new Vector2(11.6f, -13.53f), new Vector2(9.45f, -13.53f) };
            EdgeCollider2D ElecCol2 = Cafe.gameObject.AddComponent<EdgeCollider2D>();
            ElecCol2.points = new Vector2[] { new Vector2(10.2f, -5f), new Vector2(5.05f, -5f), new Vector2(5.05f, 4.75f), new Vector2(8.725f, 4.75f), new Vector2(8.725f, 5.1f) };

            EdgeCollider2D LaboCol = Cafe.gameObject.AddComponent<EdgeCollider2D>();
            LaboCol.points = new Vector2[] { new Vector2(8.45f, -13.53f), new Vector2(6f, -13.53f), new Vector2(6f, -11.4f), new Vector2(4.8f, -11.4f), new Vector2(4.8f, -12.4f), new Vector2(1.55f, -12.4f), new Vector2(1.55f, -7.7f), new Vector2(4.45f, -7.7f), new Vector2(5.25f, -8.5f), new Vector2(5.25f, -9.4f), new Vector2(6.025f, -9.4f), new Vector2(6.025f, -5.75f), new Vector2(10.35f, -5.75f), new Vector2(10.35f, -5f) };
        }
    }
}
