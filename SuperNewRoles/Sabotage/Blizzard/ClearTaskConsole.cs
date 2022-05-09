using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SuperNewRoles.Sabotage.Blizzard.DisableTask
{
    public static class VisibleONDOTask
    {
        public static void VisibleTask() //タスク画面を開く
        {

        }

        public static void DisableTask() //タスク画面を閉じる
        {

        }

        public static void ONDOCollider() //黄色枠線表示
        {

        }

        public static int GetRandomONDO() //ランダムに温度を取得
        {
            System.Random ONDO = new System.Random();
            return ONDO.Next(-25, 15);
        }

        public static void ChangeONDO(string id, int i) //温度変更(ID, 増分)
        {

        }
    }
}
