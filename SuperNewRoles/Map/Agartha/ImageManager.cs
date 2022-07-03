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
        private static Sprite m_Room_Admin;
        public static Sprite Room_Admin
        {
            get
            {
                if (m_Room_Admin != null) return m_Room_Admin;
                m_Room_Admin = ModHelpers.loadSpriteFromResources("SuperNewRoles.Resources.Agartha.Room_Admin.png", 115f);
                return m_Room_Admin;
            }
        }
        private static Sprite m_Room_Meeting;
        public static Sprite Room_Meeting
        {
            get
            {
                if (m_Room_Meeting != null) return m_Room_Meeting;
                m_Room_Meeting = ModHelpers.loadSpriteFromResources("SuperNewRoles.Resources.Agartha.Room_Meeting.png", 115f);
                return m_Room_Meeting;
            }
        }
        private static Sprite m_Object_Table1;
        public static Sprite Object_Table1
        {
            get
            {
                if (m_Object_Table1 != null) return m_Object_Table1;
                m_Object_Table1 = ModHelpers.loadSpriteFromResources("SuperNewRoles.Resources.Agartha.Object_Table1.png", 115f);
                return m_Object_Table1;
            }
        }
        private static Sprite m_CustomExilePlayer;
        public static Sprite CustomExilePlayer
        {
            get
            {
                if (m_CustomExilePlayer != null) return m_CustomExilePlayer;
                m_CustomExilePlayer = ModHelpers.loadSpriteFromResources("SuperNewRoles.Resources.Agartha.CustomExilePlayer.png", 115f);
                return m_CustomExilePlayer;
            }
        }
        private static Sprite m_Room_Security;
        public static Sprite Room_Security
        {
            get
            {
                if (m_Room_Security != null) return m_Room_Security;
                m_Room_Security = ModHelpers.loadSpriteFromResources("SuperNewRoles.Resources.Agartha.Room_Security.png", 115f);
                return m_Room_Security;
            }
        }
    }
}
