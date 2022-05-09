using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SuperNewRoles.Sabotage
{
    public static class ImageManager
    {
        private static Sprite m_ONDO;
        public static Sprite ONDO
        {
            get
            {
                if (m_ONDO != null) return m_ONDO;
                m_ONDO = ModHelpers.loadSpriteFromResources("SuperNewRoles.Resources.Blizzard.ONDO.png", 115f);
                return m_ONDO;
            }
        }
        private static Dictionary<string, Sprite> Datas = new Dictionary<string, Sprite>();
        public static Sprite ONDOgetSprite(string id)
        {
            //if (Datas.ContainsKey(id)) return Datas[id];
            Datas[id] = ModHelpers.loadSpriteFromResources("SuperNewRoles.Resources.Blizzard." + id + ".png", 115f);
            return Datas[id];
        }
    }
}
