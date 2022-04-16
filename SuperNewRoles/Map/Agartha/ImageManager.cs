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
        private static Sprite m_Object_Door_Open;
        public static Sprite Object_Door_Open
        {
            get
            {
                if (m_Object_Door_Open != null) return m_Object_Door_Open;
                m_Object_Door_Open = ModHelpers.loadSpriteFromResources("SuperNewRoles.Resources.Agartha.Object_Table1.png", 115f);
                return m_Object_Door_Open;
            }
        }
        private static Sprite m_FreePlayButton;
        public static Sprite FreePlayButton
        {
            get
            {
                if (m_FreePlayButton != null) return m_FreePlayButton;
                m_FreePlayButton = ModHelpers.loadSpriteFromResources("SuperNewRoles.Resources.Agartha.FreePlayButton.png", 115f);
                return m_FreePlayButton;
            }
        }
        private static Sprite m_Room_WareHouse;
        public static Sprite Room_WareHouse
        {
            get
            {
                if (m_Room_WareHouse != null) return m_Room_WareHouse;
                m_Room_WareHouse = ModHelpers.loadSpriteFromResources("SuperNewRoles.Resources.Agartha.Room_WareHouse.png", 115f);
                return m_Room_WareHouse;
            }
        }
        private static Sprite m_Room_WorkRoom;
        public static Sprite Room_WorkRoom
        {
            get
            {
                if (m_Room_WorkRoom != null) return m_Room_WorkRoom;
                m_Room_WorkRoom = ModHelpers.loadSpriteFromResources("SuperNewRoles.Resources.Agartha.Room_WorkRoom.png", 115f);
                return m_Room_WorkRoom;
            }
        }
        private static Sprite m_Task_FixWiring1;
        public static Sprite Task_FixWiring1
        {
            get
            {
                if (m_Task_FixWiring1 != null) return m_Task_FixWiring1;
                m_Task_FixWiring1 = ModHelpers.loadSpriteFromResources("SuperNewRoles.Resources.Agartha.Task_FixWiring1.png", 115f);
                return m_Task_FixWiring1;
            }
        }
        private static Sprite m_Task_FixWiring_BackGround;
        public static Sprite Task_FixWiring_BackGround
        {
            get
            {
                if (m_Task_FixWiring_BackGround != null) return m_Task_FixWiring_BackGround;
                m_Task_FixWiring_BackGround = ModHelpers.loadSpriteFromResources("SuperNewRoles.Resources.Agartha.Task_FixWiring_BackGround.png", 115f);
                return m_Task_FixWiring_BackGround;
            }
        }
        private static Sprite m_Room_Up;
        public static Sprite Room_Up
        {
            get
            {
                if (m_Room_Up != null) return m_Room_Up;
                m_Room_Up = ModHelpers.loadSpriteFromResources("SuperNewRoles.Resources.Agartha.Room_Up.png", 115f);
                return m_Room_Up;
            }
        }
        private static Sprite m_Room_Down;
        public static Sprite Room_Down
        {
            get
            {
                if (m_Room_Down != null) return m_Room_Down;
                m_Room_Down = ModHelpers.loadSpriteFromResources("SuperNewRoles.Resources.Agartha.Room_Down.png", 115f);
                return m_Room_Down;
            }
        }
        private static Dictionary<string, Sprite> Datas = new Dictionary<string,Sprite>();
        public static Sprite AgarthagetSprite(string id)
        {
            //if (Datas.ContainsKey(id)) return Datas[id];
            Datas[id] = ModHelpers.loadSpriteFromResources("SuperNewRoles.Resources.Agartha." + id + ".png", 115f);
            return Datas[id];
        }
    }
}
