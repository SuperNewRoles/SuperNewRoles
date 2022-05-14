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
        public static void VisibleTask() //タスク画面を開く
        {
            IsOpenBlizzardTemp = true;
            ONDO = TaskConsole.GetRandomONDO();
            SoundManager.Instance.PlaySound(ModHelpers.loadAudioClipFromResources("SuperNewRoles.Resources.Blizzard.Sounds.Panel_GenericAppear.wav"), false, 1f);
        }
        public static void DisableTask() //タスク画面を閉じる
        {
            IsOpenBlizzardTemp = false;
            SoundManager.Instance.PlaySound(ModHelpers.loadAudioClipFromResources("SuperNewRoles.Resources.Blizzard.Sounds.Panel_GenericAppear.wav"), false, 1f);
        }
    }
}
