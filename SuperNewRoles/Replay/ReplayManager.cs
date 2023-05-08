using System;
using System.Collections.Generic;
using System.Text;

namespace SuperNewRoles.Replay
{
    public static class ReplayManager
    {
        public static bool IsReplayMode = false;
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
        public static void HudUpdate()
        {
            if (IsReplayMode)
                ReplayLoader.HudUpdate();
            else
                Recorder.HudUpdate();
        }
        public static void CoIntroStart()
        {
            if (IsReplayMode)
                ReplayLoader.CoIntroStart();
            else
                Recorder.CoIntroStart();
        }
    }
}
