using SuperNewRoles.CustomRPC;
using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using Hazel;

namespace SuperNewRoles
{
    [HarmonyPatch(typeof(RoleManager), nameof(RoleManager.SelectRoles))]
    class RoleManagerSelectRolesPatch
    {
        public static void Postfix()
        {
            AllRoleSetClass.AllRoleSet();
        }
    }
    class AllRoleSetClass
    {
        public static List<RoleId> Impoonepar;
        public static List<RoleId> Imponotonepar;
        public static List<RoleId> Neutonepar;
        public static List<RoleId> Neutnotonepar;
        public static List<RoleId> Crewonepar;
        public static List<RoleId> Crewnotonepar;
        public static List<PlayerControl> CrewMatePlayers;
        public static List<PlayerControl> ImpostorPlayers;

        public static void AllRoleSet()
        {
            if (!AmongUsClient.Instance.AmHost) return;
            CrewOrImpostorSet();
            OneOrNotListSet();
            ImpostorRandomSelect();
        }
        public static void ImpostorRandomSelect()
        {
            SuperNewRolesPlugin.Logger.LogInfo(Impoonepar);
            foreach (RoleId id in Impoonepar)
            {
                SuperNewRolesPlugin.Logger.LogInfo(id);
            }
            bool IsNotEndRandomSelect= true;
            while (IsNotEndRandomSelect)
            {
                RoleId SelectRoleDate= ModHelpers.GetRandom(Impoonepar);
                int PlayerCount = (int)GetPlayerCount(SelectRoleDate);
                if (PlayerCount >= ImpostorPlayers.Count)
                {
                    foreach(PlayerControl Player in ImpostorPlayers)
                    {
                        Player.setRoleRPC(SelectRoleDate);
                    }
                } 
            }
        }
        public static float GetPlayerCount(RoleId RoleDate)
        {
            switch (RoleDate)
            {
                case (RoleId.EvilSpeedBooster):
                    return CustomOption.CustomOptions.EvilSpeedBoosterPlayerCount.getFloat();
                case (RoleId.EvilScientist):
                    return CustomOption.CustomOptions.EvilScientistPlayerCount.getFloat();
                case (RoleId.EvilLighter):
                    return CustomOption.CustomOptions.EvilLighterPlayerCount.getFloat();
               
            }
            return 1;
        }
        public static void CrewOrImpostorSet()
        {
            CrewMatePlayers = new List<PlayerControl>();
            ImpostorPlayers = new List<PlayerControl>();
            foreach(PlayerControl Player in PlayerControl.AllPlayerControls)
            {
                SuperNewRolesPlugin.Logger.LogInfo(Player.Data.PlayerName+":"+ Player.Data.Role.IsImpostor);
                if (Player.Data.Role.IsImpostor)
                {
                    ImpostorPlayers.Add(Player);
                    SuperNewRolesPlugin.Logger.LogInfo("ImpostorAdd");
                } else
                {
                    CrewMatePlayers.Add(Player);
                    SuperNewRolesPlugin.Logger.LogInfo("CrewAdd");
                }
                SuperNewRolesPlugin.Logger.LogInfo("AddEnd");
            }
        }
        public static void OneOrNotListSet()
        {
            Impoonepar = new List<RoleId>();
            Imponotonepar = new List<RoleId>();
            Neutonepar = new List<RoleId>();
            Neutnotonepar = new List<RoleId>();
            Crewonepar = new List<RoleId>();
            Crewnotonepar = new List<RoleId>();
            if (!(CustomOption.CustomOptions.SoothSayerOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.SoothSayerOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.SoothSayer;
                if (OptionDate == 10)
                {
                    Crewonepar.Add(ThisRoleId);
                } else
                {
                    for (int i = 1; i <= OptionDate; i++)
                    {
                        Crewnotonepar.Add(ThisRoleId);
                    }
                }
            } else if (!(CustomOption.CustomOptions.JesterOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.JesterOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.Jester;
                if (OptionDate == 10)
                {
                    Neutonepar.Add(ThisRoleId);
                }
                else
                {
                    for (int i = 1; i <= OptionDate; i++)
                    {
                        Neutnotonepar.Add(ThisRoleId);
                    }
                }
            }
            else if (!(CustomOption.CustomOptions.LighterOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.LighterOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.Lighter;
                if (OptionDate == 10)
                {
                    Crewonepar.Add(ThisRoleId);
                }
                else
                {
                    for (int i = 1; i <= OptionDate; i++)
                    {
                        Crewnotonepar.Add(ThisRoleId);
                    }
                }
            }
            else if (!(CustomOption.CustomOptions.EvilLighterOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.EvilLighterOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.EvilLighter;
                if (OptionDate == 10)
                {
                    Impoonepar.Add(ThisRoleId);
                }
                else
                {
                    for (int i = 1; i <= OptionDate; i++)
                    {
                        Imponotonepar.Add(ThisRoleId);
                    }
                }
            }
            else if (!(CustomOption.CustomOptions.EvilScientistOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.EvilScientistOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.EvilScientist;
                if (OptionDate == 10)
                {
                    Impoonepar.Add(ThisRoleId);
                }
                else
                {
                    for (int i = 1; i <= OptionDate; i++)
                    {
                        Imponotonepar.Add(ThisRoleId);
                    }
                }
            }
            else if (!(CustomOption.CustomOptions.SheriffOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.SheriffOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.Sheriff;
                if (OptionDate == 10)
                {
                    Crewonepar.Add(ThisRoleId);
                }
                else
                {
                    for (int i = 1; i <= OptionDate; i++)
                    {
                        Crewnotonepar.Add(ThisRoleId);
                    }
                }
            }
            else if (!(CustomOption.CustomOptions.MeetingSheriffOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.MeetingSheriffOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.MeetingSheriff;
                if (OptionDate == 10)
                {
                    Crewonepar.Add(ThisRoleId);
                }
                else
                {
                    for (int i = 1; i <= OptionDate; i++)
                    {
                        Crewnotonepar.Add(ThisRoleId);
                    }
                }
            }
            else if (!(CustomOption.CustomOptions.AllKillerOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.AllKillerOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.AllKiller;
                if (OptionDate == 10)
                {
                    Neutonepar.Add(ThisRoleId);
                }
                else
                {
                    for (int i = 1; i <= OptionDate; i++)
                    {
                        Neutnotonepar.Add(ThisRoleId);
                    }
                }
            }
            else if (!(CustomOption.CustomOptions.TeleporterOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.TeleporterOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.Teleporter;
                if (OptionDate == 10)
                {
                    Impoonepar.Add(ThisRoleId);
                }
                else
                {
                    for (int i = 1; i <= OptionDate; i++)
                    {
                        Imponotonepar.Add(ThisRoleId);
                    }
                }
            }
            else if (!(CustomOption.CustomOptions.SpiritMediumOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.SpiritMediumOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.SpiritMedium;
                if (OptionDate == 10)
                {
                    Crewonepar.Add(ThisRoleId);
                }
                else
                {
                    for (int i = 1; i <= OptionDate; i++)
                    {
                        Crewnotonepar.Add(ThisRoleId);
                    }
                }
            }
            else if (!(CustomOption.CustomOptions.SpeedBoosterOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.SpeedBoosterOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.SpeedBooster;
                if (OptionDate == 10)
                {
                    Crewonepar.Add(ThisRoleId);
                }
                else
                {
                    for (int i = 1; i <= OptionDate; i++)
                    {
                        Crewnotonepar.Add(ThisRoleId);
                    }
                }
            }
            else if (!(CustomOption.CustomOptions.EvilSpeedBoosterOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.EvilSpeedBoosterOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.EvilSpeedBooster;
                if (OptionDate == 10)
                {
                    Impoonepar.Add(ThisRoleId);
                }
                else
                {
                    for (int i = 1; i <= OptionDate; i++)
                    {
                        Imponotonepar.Add(ThisRoleId);
                    }
                }
            }
            else if (!(CustomOption.CustomOptions.TaskerOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.TaskerOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.Tasker;
                if (OptionDate == 10)
                {
                    Impoonepar.Add(ThisRoleId);
                }
                else
                {
                    for (int i = 1; i <= OptionDate; i++)
                    {
                        Imponotonepar.Add(ThisRoleId);
                    }
                }
            }
            else if (!(CustomOption.CustomOptions.DoorrOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.DoorrOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.Doorr;
                if (OptionDate == 10)
                {
                    Crewonepar.Add(ThisRoleId);
                }
                else
                {
                    for (int i = 1; i <= OptionDate; i++)
                    {
                        Crewnotonepar.Add(ThisRoleId);
                    }
                }
            }
            else if (!(CustomOption.CustomOptions.EvilDoorrOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.EvilDoorrOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.EvilDoorr;
                if (OptionDate == 10)
                {
                    Impoonepar.Add(ThisRoleId);
                }
                else
                {
                    for (int i = 1; i <= OptionDate; i++)
                    {
                        Imponotonepar.Add(ThisRoleId);
                    }
                }
            }
            else if (!(CustomOption.CustomOptions.SealdorOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.SealdorOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.Sealdor;
                if (OptionDate == 10)
                {
                    Crewonepar.Add(ThisRoleId);
                }
                else
                {
                    for (int i = 1; i <= OptionDate; i++)
                    {
                        Crewnotonepar.Add(ThisRoleId);
                    }
                }
            }
            else if (!(CustomOption.CustomOptions.SpeederOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.SpeederOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.Speeder;
                if (OptionDate == 10)
                {
                    Impoonepar.Add(ThisRoleId);
                }
                else
                {
                    for (int i = 1; i <= OptionDate; i++)
                    {
                        Imponotonepar.Add(ThisRoleId);
                    }
                }
            }
            else if (!(CustomOption.CustomOptions.FreezerOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.FreezerOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.Freezer;
                if (OptionDate == 10)
                {
                    Impoonepar.Add(ThisRoleId);
                }
                else
                {
                    for (int i = 1; i <= OptionDate; i++)
                    {
                        Imponotonepar.Add(ThisRoleId);
                    }
                }
            }
            else if (!(CustomOption.CustomOptions.GuesserOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.GuesserOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.Guesser;
                if (OptionDate == 10)
                {
                    Crewonepar.Add(ThisRoleId);
                }
                else
                {
                    for (int i = 1; i <= OptionDate; i++)
                    {
                        Crewnotonepar.Add(ThisRoleId);
                    }
                }
            }
            else if (!(CustomOption.CustomOptions.EvilGuesserOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.EvilGuesserOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.EvilGuesser;
                if (OptionDate == 10)
                {
                    Impoonepar.Add(ThisRoleId);
                }
                else
                {
                    for (int i = 1; i <= OptionDate; i++)
                    {
                        Imponotonepar.Add(ThisRoleId);
                    }
                }
            }
            else if (!(CustomOption.CustomOptions.VultureOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.VultureOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.Vulture;
                if (OptionDate == 10)
                {
                    Neutonepar.Add(ThisRoleId);
                }
                else
                {
                    for (int i = 1; i <= OptionDate; i++)
                    {
                        Neutnotonepar.Add(ThisRoleId);
                    }
                }
            }
            else if (!(CustomOption.CustomOptions.NiceScientistOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.NiceScientistOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.NiceScientist;
                if (OptionDate == 10)
                {
                    Crewonepar.Add(ThisRoleId);
                }
                else
                {
                    for (int i = 1; i <= OptionDate; i++)
                    {
                        Crewnotonepar.Add(ThisRoleId);
                    }
                }
            }
            else if (!(CustomOption.CustomOptions.ClergymanOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.ClergymanOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.Clergyman;
                if (OptionDate == 10)
                {
                    Crewonepar.Add(ThisRoleId);
                }
                else
                {
                    for (int i = 1; i <= OptionDate; i++)
                    {
                        Crewnotonepar.Add(ThisRoleId);
                    }
                }
            }
            else if (!(CustomOption.CustomOptions.MadMateOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.MadMateOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.MadMate;
                if (OptionDate == 10)
                {
                    Crewonepar.Add(ThisRoleId);
                }
                else
                {
                    for (int i = 1; i <= OptionDate; i++)
                    {
                        Crewnotonepar.Add(ThisRoleId);
                    }
                }
            }
        }
    }
}
