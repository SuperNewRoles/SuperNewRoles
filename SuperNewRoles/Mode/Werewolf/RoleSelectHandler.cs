using System.Collections.Generic;


namespace SuperNewRoles.Mode.Werewolf
{
    public static class RoleSelectHandler
    {
        public static void RoleSelect()
        {
            if (!AmongUsClient.Instance.AmHost) return;
            OneOrNotListSet();
            AllRoleSetClass.AllRoleSet();
            ChacheManager.ResetChache();
        }
        public static void OneOrNotListSet()
        {
            List<RoleId> Impoonepar = new();
            List<RoleId> Imponotonepar = new();
            List<RoleId> Neutonepar = new();
            List<RoleId> Neutnotonepar = new();
            List<RoleId> Crewonepar = new();
            List<RoleId> Crewnotonepar = new();
            if (CustomOptions.MadMateOption.GetString().Replace("0%", "") != "")
            {
                int OptionDate = int.Parse(CustomOptions.MadMateOption.GetString().Replace("0%", ""));
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
            if (CustomOptions.SoothSayerOption.GetString().Replace("0%", "") != "")
            {
                int OptionDate = int.Parse(CustomOptions.SoothSayerOption.GetString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.SoothSayer;
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
            if (CustomOptions.SpiritMediumOption.GetString().Replace("0%", "") != "")
            {
                int OptionDate = int.Parse(CustomOptions.SpiritMediumOption.GetString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.SpiritMedium;
                if (OptionDate == 10) Crewonepar.Add(ThisRoleId);
                else
                {
                    for (int i = 1; i <= OptionDate; i++)
                    {
                        Crewnotonepar.Add(ThisRoleId);
                    }
                }
            }
            AllRoleSetClass.Impoonepar = Impoonepar;
            AllRoleSetClass.Imponotonepar = Imponotonepar;
            AllRoleSetClass.Neutonepar = Neutonepar;
            AllRoleSetClass.Neutnotonepar = Neutnotonepar;
            AllRoleSetClass.Crewonepar = Crewonepar;
            AllRoleSetClass.Crewnotonepar = Crewnotonepar;
        }
    }
}