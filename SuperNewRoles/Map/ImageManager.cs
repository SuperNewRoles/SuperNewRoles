using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SuperNewRoles.Map
{
    public static class ImageManager
    {
        private static Sprite AgarthaAdminButton;
        public static Sprite GetAgarthaAdminButton()
        {
            if (AgarthaAdminButton != null) return AgarthaAdminButton;
            AgarthaAdminButton = ModHelpers.loadSpriteFromResources("SuperNewRoles.Resources.Agartha.AdminButton.png", 115f);
            return AgarthaAdminButton;
        }
    }
}