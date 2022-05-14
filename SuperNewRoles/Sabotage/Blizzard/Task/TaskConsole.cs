using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SuperNewRoles.Sabotage.Blizzard.Task
{

    public class TaskConsole
    {
        public static List<Console> AddConsoles;
        public static void ONDOColliderON() //黄色枠線表示
        {
            AddConsoles = new List<Console>();
            GameObject Object_ONDO0 = GameObject.Find("ONDO");
            AddConsoles.Add(Object_ONDO0.GetComponent<Console>());
            Object_ONDO0.gameObject.AddComponent<Console>().enabled = true;
            Object_ONDO0.gameObject.AddComponent<BoxCollider2D>().enabled = true;
        }
        public static void ONDOColliderOFF()
        {
            GameObject Object_ONDO0 = GameObject.Find("ONDO");
            Object_ONDO0.gameObject.AddComponent<BoxCollider2D>().enabled = false;
        }

        public static int GetRandomONDO() //ランダムに温度を取得
        {
            System.Random ONDO = new System.Random();
            return ONDO.Next(-25, 15);
        }
    }
}
