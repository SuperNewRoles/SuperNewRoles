using Hazel;
using SuperNewRoles.CustomRPC;
using SuperNewRoles.Helpers;
using SuperNewRoles.Mode;
using System;
using System.Collections.Generic;
using System.Text;
using SuperNewRoles.EndGame;
using HarmonyLib;
using SuperNewRoles.Buttons;
using UnityEngine;

namespace SuperNewRoles.Roles
{
    public static class Conjurer
    {
        public static void FirstAddAdd()
        {
            RoleClass.Conjurer.FirstAdd = true;
        }
        //FirstAddをtrueに

        public static void SecondAddAdd()
        {
            RoleClass.Conjurer.SecondAdd = true;
        }
        //SecondAddをtrueに

        public static void ThirdAddAdd()
        {
            RoleClass.Conjurer.ThirdAdd = true;
        }
        //ThirdAddをtrueに

        public static void AllClear()
        {
            RoleClass.Conjurer.FirstAdd = false;
            RoleClass.Conjurer.SecondAdd = false;
            RoleClass.Conjurer.ThirdAdd = false;
        }
        //全部falseに

        public static bool IsFirstAdded()
        {
            if (RoleClass.Conjurer.FirstAdd)
            {
                return true;
            }
            return false;
        }
        //一回追加されたかを判定する

        public static bool IsSecondAdded()
        {
            if (RoleClass.Conjurer.SecondAdd)
            {
                return true;
            }
            return false;
        }
        //二回追加されたかを判定する

        public static bool IsThirdAdded()
        {
            if (RoleClass.Conjurer.ThirdAdd)
            {
                return true;
            }
            return false;
        }
        //三回追加されたかを判定する
    }
}
