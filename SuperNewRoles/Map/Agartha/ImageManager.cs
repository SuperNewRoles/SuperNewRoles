using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SuperNewRoles.Map.Agartha
{
    public static class ImageManager
	{
        private static Sprite m_MiniMap;
		public static Sprite MiniMap
		{
			get
            {
                if (m_MiniMap != null) return m_MiniMap;
                m_MiniMap = ModHelpers.loadSpriteFromResources("SuperNewRoles.Resources.Agartha.MiniMap.png", 115f);
                return m_MiniMap;
            }
        }
        private static Sprite m_ExileBackImage;
        public static Sprite ExileBackImage
        {
            get
            {
                if (m_ExileBackImage != null) return m_ExileBackImage;
                m_ExileBackImage = ModHelpers.loadSpriteFromResources("SuperNewRoles.Resources.Agartha.ExileBack.png", 115f);
                return m_ExileBackImage;
            }
        }
        private static Sprite m_Admin_Table;
        public static Sprite Admin_Table
        {
            get
            {
                if (m_Admin_Table != null) return m_Admin_Table;
                m_Admin_Table = ModHelpers.loadSpriteFromResources("SuperNewRoles.Resources.Agartha.Admin_Table.png", 115f);
                return m_Admin_Table;
            }
        }
    }
}
