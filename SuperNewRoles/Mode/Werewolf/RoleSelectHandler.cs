using System.Collections.Generic;
using SuperNewRoles.CustomRPC;

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
            if (CustomOption.CustomOptions.MadMateOption.GetString().Replace("0%", "") != "")
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.MadMateOption.GetString().Replace("0%", ""));
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
            if (CustomOption.CustomOptions.SoothSayerOption.GetString().Replace("0%", "") != "")
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.SoothSayerOption.GetString().Replace("0%", ""));
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
            if (CustomOption.CustomOptions.SpiritMediumOption.GetString().Replace("0%", "") != "")
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.SpiritMediumOption.GetString().Replace("0%", ""));
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
            /*if (WerewolfOptions.WerewolfHunterOption.GetString().Replace("0%", "") != "")
            {
                SuperNewRolesPlugin.Logger.LogInfo("[WereWolf] ADDWOLF@ame");
                int OptionDate = int.Parse(WerewolfOptions.WerewolfHunterOption.GetString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.Hunter;
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
            }*/
            AllRoleSetClass.Impoonepar = Impoonepar;
            AllRoleSetClass.Imponotonepar = Imponotonepar;
            AllRoleSetClass.Neutonepar = Neutonepar;
            AllRoleSetClass.Neutnotonepar = Neutnotonepar;
            AllRoleSetClass.Crewonepar = Crewonepar;
            AllRoleSetClass.Crewnotonepar = Crewnotonepar;
        }
    }
}