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
            CafeCol.points = new Vector2[] { new Vector2(1f, 4.9f),new Vector2(3.45f, 4.9f),new Vector2(3.45f, -5.15f), new Vector2(-3.45f, -5.15f), new Vector2(-3.45f, -1.8f) };
            EdgeCollider2D CafeCol2 = Cafe.gameObject.AddComponent<EdgeCollider2D>();
            CafeCol2.points = new Vector2[] { new Vector2(-3.42f, -0.4f), new Vector2(-3.2f, 3.3f), new Vector2(-1.65f, 5f), new Vector2(-0.3f, 5f) };
            //EdgeCollider2D ElecCol = Cafe.gameObject.AddComponent<EdgeCollider2D>();
            //ElecCol.points = new Vector2[] { new Vector2(9f, 4.9f), new Vector2(11.45f, 4.9f), new Vector2(11.45f, -5.15f), new Vector2(6.45f, -5.15f), new Vector2(6.45f, -1.8f) };
            
            //EdgeCollider2D ElecCol2 = Cafe.gameObject.AddComponent<EdgeCollider2D>();
            //CafeCol2.points = new Vector2[] { new Vector2(-3.42f, -0.4f), new Vector2(-3.2f, 3.3f), new Vector2(-1.65f, 5f), new Vector2(-0.3f, 5f) };
        }
    }
}
