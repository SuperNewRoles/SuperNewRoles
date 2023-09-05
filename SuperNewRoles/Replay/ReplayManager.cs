using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace SuperNewRoles.Replay
{
    public static class ReplayManager
    {
        public static string LastSavedName;
        public static bool IsReplayMode = false;
        public static bool IsRecording = false;
        public static ReplayData CurrentReplay
        {
            get
            {
                return replay;
            }
            set
            {
                replay = value;
            }
        }
        private static ReplayData replay;
        public static void ClearAndReloads()
        {
            Recorder.ClearAndReloads();
            ReplayLoader.ClearAndReloads();
        }
        public static void CreateReplayButton(MainMenuManager __instance, PassiveButton FreePlayButton)
        {
            PassiveButton ReplayButton = GameObject.Instantiate(FreePlayButton, FreePlayButton.transform.parent);
            ReplayButton.transform.localPosition = new(2f, -1.75f, 0);
            GameObject.Destroy(ReplayButton.buttonText.GetComponent<TextTranslatorTMP>());
            ReplayButton.buttonText.text = ModTranslation.GetString("ReplayName");
            ReplayButton.buttonText.alignment = TextAlignmentOptions.Center;
            ReplayButton.OnClick = new();
            ReplayButton.OnClick.AddListener((UnityAction)(() =>
            {
                Logger.Info("クリック");
                IsReplaySelector = true;
                __instance.playLocalButton.OnClick.Invoke();
            }));
            ReplayButton.name = "ReplayButton";
        }
        public static bool IsReplaySelector = false;
        public static void HudUpdate()
        {
            if (IsReplayMode)
                ReplayLoader.HudUpdate();
            else if (IsRecording)
                Recorder.HudUpdate();
        }
        public static void CoIntroDestory()
        {
            if (IsReplayMode)
                ReplayLoader.CoIntroDestory();
            else if (IsRecording)
                Recorder.CoIntroDestroy();
        }
    }
}