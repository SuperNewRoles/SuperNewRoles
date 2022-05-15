using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using UnityEngine;

namespace SuperNewRoles.Sabotage.Blizzard.Task
{
    public class OpenTask
    {
        public static bool IsOpenBlizzardTemp;
        public static int ONDO;
        [HarmonyPatch(typeof(Console), nameof(Console.Use))]
        public static void Postfix() //タスク画面を開く
        {
            IsOpenBlizzardTemp = true;
            CreateTask();
            ONDO = TaskConsole.GetRandomONDO();
        }
        public static void DisableTask() //タスク画面を閉じる
        {
            IsOpenBlizzardTemp = false;
            SoundManager.Instance.PlaySound(ModHelpers.loadAudioClipFromResources("SuperNewRoles.Resources.Blizzard.Sounds.Panel_GenericAppear.wav"), false, 1f);
        }

        public static Sprite BackGroundSprite;
        public static void CreateTask()
        {
            SoundManager.Instance.PlaySound(ModHelpers.loadAudioClipFromResources("SuperNewRoles.Resources.Blizzard.Sounds.Panel_GenericAppear.wav"), false, 1f);
            //BackGroundSprite.Sprite image = Resources.Load<Sprite>();

        }
    }
}
