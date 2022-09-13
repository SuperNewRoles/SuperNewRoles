using System.Collections.Generic;
using SuperNewRoles.Patch;
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

            //キルクールが終わった時に画面を光らせる奴
            public static bool Processed;
            public static void FixedUpdate()
            {
                if (PlayerControl.LocalPlayer.IsRole(RoleId.TheFirstLittlePig))
                {
                    if (!RoleClass.IsMeeting)
                    {
                        if (TimerAdvances || PlayerControl.LocalPlayer.CanMove) KillTimer -= Time.fixedDeltaTime;
                        if (KillTimer <= 0 && !Processed && TaskCheck(PlayerControl.LocalPlayer))
                        {
                            Processed = true;
                            Seer.ShowFlash(new Color(241f / 255f, 91f / 245f, 71f / 255f), 5f);
                            /*
                            デフォルト // new Color(42f / 255f, 187f / 245f, 245f / 255f)
                            その1 // new Color(225f / 255f, 16f / 245f, 16f / 255f)
                            その2 // new Color(241f / 255f, 91f / 245f, 71f / 255f)
                            その3 // new Color(240f / 255f, 86f / 245f, 110f / 255f)
                            */
                        }
                    }
                    else
                    {
                        KillTimer = DefaultKillCool;
                        Processed = false;
                    }
                }
            }
        }
        //2番目の子豚
        public static class TheSecondLittlePig
        {
            //RoleClass
            public static List<PlayerControl> TheSecondLittlePigPlayer;
            public static int ClearTask;
            public static int GuardCount;

            //キルガード
            public static void KillGuard(PlayerControl target)
            {
                if (TaskCheck(target) && GuardCount > 0)
                {
                    target.RpcProtectPlayer(target, 0);
                    GuardCount--;
                }
            }
        }
        //3番目の子豚
        public static class TheThirdLittlePig
        {
            //RoleClass
            public static List<PlayerControl> TheThirdLittlePigPlayer;
            public static int ClearTask;
            public static int CounterKillCount;

            public static void CounterKill(PlayerControl __instance, PlayerControl target)
            {
                if (TaskCheck(target) && CounterKillCount > 0)
                {
                    target.RpcProtectPlayer(target, 0);
                    target.RpcMurderPlayer(__instance);
                    CounterKillCount--;
                }
            }
        }
        //RoleClass
        public static Color32 color = new(255, 99, 123, byte.MaxValue);
        public static List<PlayerControl> TheThreeLittlePigsPlayer;
        public static bool AddWin;
        public static void ClearAndReload()
        {
            //全員に関係
            TheThreeLittlePigsPlayer = new();
            TheThreeLittlePigsPlayer.AddRange(TheFirstLittlePig.TheFirstLittlePigPlayer);
            TheThreeLittlePigsPlayer.AddRange(TheSecondLittlePig.TheSecondLittlePigPlayer);
            TheThreeLittlePigsPlayer.AddRange(TheThirdLittlePig.TheThirdLittlePigPlayer);
            AddWin = CustomOptions.TheThreeLittlePigsIsAddWin.GetBool();
            //1番目の子豚
            TheFirstLittlePig.TheFirstLittlePigPlayer = new();
            TheFirstLittlePig.DefaultKillCool = PlayerControl.GameOptions.KillDistance;
            TheFirstLittlePig.KillTimer = TheFirstLittlePig.DefaultKillCool;
            TheFirstLittlePig.TimerAdvances = CustomOptions.TheFirstLittlePigAlwaysTimerAdvances.GetBool();
            TheFirstLittlePig.Flash = CustomOptions.TheFirstLittlePigFlush.GetBool();
            TheFirstLittlePig.Processed = false;
            //2番目の子豚
            TheSecondLittlePig.TheSecondLittlePigPlayer = new();
            TheSecondLittlePig.GuardCount = CustomOptions.TheSecondLittlePigMaxGuardCount.GetInt();
            //3番目の子豚
            TheThirdLittlePig.TheThirdLittlePigPlayer = new();
            TheThirdLittlePig.CounterKillCount = CustomOptions.TheThirdLittlePigCounterKillCount.GetInt();

            int commonTask = CustomOptions.TheThreeLittlePigsCommonTask.GetInt();
            int shortTask = CustomOptions.TheThreeLittlePigsShortTask.GetInt();
            int longTask = CustomOptions.TheThreeLittlePigsLongTask.GetInt();
            if (commonTask + shortTask + longTask == 0)
            {
                commonTask = PlayerControl.GameOptions.NumCommonTasks;
                shortTask = PlayerControl.GameOptions.NumShortTasks;
                longTask = PlayerControl.GameOptions.NumLongTasks;
            }
            int AllTask = commonTask + shortTask + longTask;
            TheFirstLittlePig.ClearTask = (int)(AllTask * (int.Parse(CustomOptions.TheFirstLittlePigClearTask.GetString().Replace("%", "")) / 100f));
            TheSecondLittlePig.ClearTask = (int)(AllTask * (int.Parse(CustomOptions.TheSecondLittlePigClearTask.GetString().Replace("%", "")) / 100f));
            TheThirdLittlePig.ClearTask = (int)(AllTask * (int.Parse(CustomOptions.TheThirdLittlePigClearTask.GetString().Replace("%", "")) / 100f));
            CheckedTask = new();
        }

        //タスクを完了しているかの判定
        public static List<byte> CheckedTask;
        public static bool TaskCheck(PlayerControl player)
        {
            if (CheckedTask.Contains(player.PlayerId)) return true;
            RoleId Role = player.GetRole();
            int clearTask = 0;
            switch (Role)
            {
                case RoleId.TheFirstLittlePig:
                    clearTask = TheFirstLittlePig.ClearTask;
                    break;
                case RoleId.TheSecondLittlePig:
                    clearTask = TheSecondLittlePig.ClearTask;
                    break;
                case RoleId.TheThirdLittlePig:
                    clearTask = TheThirdLittlePig.ClearTask;
                    break;
                default:
                    return false;
            }
            var taskdata = TaskCount.TaskDate(player.Data).Item1;
            if (clearTask <= taskdata)
            {
                CheckedTask.Add(player.PlayerId);
                return true;
            }
            return false;
        }
        public static bool WinCheck()
        {
            foreach(PlayerControl player in TheThreeLittlePigsPlayer)
            {
                if (!TaskCheck(player))
                {
                    return false;
                }
            }
            foreach(PlayerControl player in TheThreeLittlePigsPlayer)
            {
                if (player.IsAlive())
                {
                    return true;
                }
            }
            return false;
        }
    }
}