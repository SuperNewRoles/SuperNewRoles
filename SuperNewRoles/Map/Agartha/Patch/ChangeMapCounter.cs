using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SuperNewRoles.Map.Agartha.Patch
{
    public static class ChangeMapCounter
    {
        public static void Change(Transform Miraship)
        {
            Transform cafe = Miraship.FindChild("Cafe");
            cafe.position = new Vector3(13.2f, 16, 0f);
            //cafe.GetComponent<BoxCollider2D>().size = new Vector2(7f,3f);
            //GameObject.Destroy(cafe.GetComponent<EdgeCollider2D>());
            //cafe.GetComponent<BoxCollider2D>().enabled = false;
        }
    }
}
