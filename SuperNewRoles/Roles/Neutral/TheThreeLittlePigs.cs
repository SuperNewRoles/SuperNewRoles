using System.Collections.Generic;
using SuperNewRoles.CustomOption;
using UnityEngine;

namespace SuperNewRoles.Roles.Neutral
{
    public static class TheThreeLittlePigs
    {
        //1番目の子豚
        public static class TheFirstLittlePig
        {
            //RoleClass
            public static List<PlayerControl> TheFirstLittlePigPlayer;
            public static int ClearTask;
            public static float DefaultKillCool;
            public static float KillTimer;
            public static bool TimerAdvances;
            public static bool Flash;
        }
        //2番目の子豚
        public static class TheSecondLittlePig
        {
            //RoleClass
            public static List<PlayerControl> TheSecondLittlePigPlayer;
            public static int ClearTask;
            public static int GuardCount;
        }
        //3番目の子豚
        public static class TheThirdLittlePig
        {
            //RoleClass
            public static List<PlayerControl> TheThirdLittlePigPlayer;
            public static int ClearTask;
            public static int CounterKillCount;
        }
        //RoleClass
        public static Color32 color = new(255, 99, 123, byte.MaxValue);
        public static void ClearAndReload()
        {
            //1番目の子豚
            TheFirstLittlePig.TheFirstLittlePigPlayer = new();
            TheFirstLittlePig.DefaultKillCool = PlayerControl.GameOptions.KillDistance;
            TheFirstLittlePig.KillTimer = TheFirstLittlePig.DefaultKillCool;
            TheFirstLittlePig.TimerAdvances = CustomOptions.TheFirstLittlePigAlwaysTimerAdvances.GetBool();
            TheFirstLittlePig.Flash = CustomOptions.TheFirstLittlePigFlush.GetBool();
            //2番目の子豚
            TheSecondLittlePig.TheSecondLittlePigPlayer = new();
            TheSecondLittlePig.GuardCount = CustomOptions.TheSecondLittlePigMaxGuardCount.GetInt();
            //3番目の子豚
            TheThirdLittlePig.TheThirdLittlePigPlayer = new();
            TheThirdLittlePig.CounterKillCount = CustomOptions.TheThirdLittlePigCounterKillCount.GetInt();

            int commonTask = CustomOptions.TheThreeLittlePigsCommonTask.GetInt();
            int shortTask = CustomOptions.TheThreeLittlePigsShortTask.GetInt();
            int longTask = CustomOptions.TheThreeLittlePigsLongTask.GetInt();
            if(commonTask + shortTask + longTask == 0)
            {
                commonTask = PlayerControl.GameOptions.NumCommonTasks;
                shortTask = PlayerControl.GameOptions.NumShortTasks;
                longTask = PlayerControl.GameOptions.NumLongTasks;
            }
            int AllTask = commonTask + shortTask + longTask;
            TheFirstLittlePig.ClearTask = (int)(AllTask * (int.Parse(CustomOptions.TheFirstLittlePigClearTask.GetString().Replace("%", "")) / 100f));
            TheSecondLittlePig.ClearTask = (int)(AllTask * (int.Parse(CustomOptions.TheSecondLittlePigClearTask.GetString().Replace("%", "")) / 100f));
            TheThirdLittlePig.ClearTask = (int)(AllTask * (int.Parse(CustomOptions.TheThirdLittlePigClearTask.GetString().Replace("%", "")) / 100f));
        }
    }
}
