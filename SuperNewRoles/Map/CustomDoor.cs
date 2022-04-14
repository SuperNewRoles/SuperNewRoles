using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SuperNewRoles.Map
{
    public class CustomDoor : MonoBehaviour
    {
        public SpriteRenderer render;
        public bool IsOpen;
        public void SetDoorWay(bool Open)
        {
            IsOpen = Open;
            if (IsOpen)
            {
                render.sprite = Agartha.ImageManager.Object_Door_Open;
            } else
            {
                render.sprite = Agartha.ImageManager.Object_Door_Open;
            }
        }
        void Start()
        {
            render = new SpriteRenderer();
            render.sprite = Agartha.ImageManager.Object_Door_Open; 
        }
    }
}
