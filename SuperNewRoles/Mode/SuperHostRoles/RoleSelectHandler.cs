using SuperNewRoles.CustomRPC;
using SuperNewRoles.Patch;
using SuperNewRoles.Roles;
using System;
using System.Collections.Generic;
using System.Text;

namespace SuperNewRoles.Mode.SuperHostRoles
{
    public static class RoleSelectHandler
    {
        public static void RoleSelect()
        {
            OneOrNotListSet();
            AllRoleSetClass.AllRoleSet();
            SetCustomRoles();
        }
        public static void SetCustomRoles() { 
            if (RoleClass.Jester.IsUseVent)
            {
                foreach (PlayerControl p in RoleClass.Jester.JesterPlayer)
                {
                    if (!ShareGameVersion.GameStartManagerUpdatePatch.VersionPlayers.ContainsKey(p.getClientId()))
                    {
                        p.RpcSetRoleDesync(RoleTypes.Engineer);
                    }
                }

                PlayerControl.GameOptions.RoleOptions.EngineerCooldown = 0f;
                PlayerControl.GameOptions.RoleOptions.EngineerInVentMaxTime = 0f;
                PlayerControl.LocalPlayer.RpcSyncSettings(PlayerControl.GameOptions);
            }
            if (RoleClass.MadMate.IsUseVent)
            {
                foreach (PlayerControl p in RoleClass.MadMate.MadMatePlayer)
                {
                    if (!ShareGameVersion.GameStartManagerUpdatePatch.VersionPlayers.ContainsKey(p.getClientId()))
                    {
                        p.RpcSetRoleDesync(RoleTypes.Engineer);
                    }
                }

                PlayerControl.GameOptions.RoleOptions.EngineerCooldown = 0f;
                PlayerControl.GameOptions.RoleOptions.EngineerInVentMaxTime = 0f;
                PlayerControl.LocalPlayer.RpcSyncSettings(PlayerControl.GameOptions);
            }
        }
        public static void OneOrNotListSet()
        {
            var Impoonepar = new List<RoleId>();
            var Imponotonepar = new List<RoleId>();
            var Neutonepar = new List<RoleId>();
            var Neutnotonepar = new List<RoleId>();
            var Crewonepar = new List<RoleId>();
            var Crewnotonepar = new List<RoleId>();
            if (!(CustomOption.CustomOptions.OpportunistOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.OpportunistOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.Opportunist;
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
            if (!(CustomOption.CustomOptions.GodOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.GodOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.God;
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
            if (!(CustomOption.CustomOptions.JesterOption.getString().Replace("0%", "") == ""))
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
            if (!(CustomOption.CustomOptions.NiceNekomataOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.NiceNekomataOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.NiceNekomata;
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
            if (!(CustomOption.CustomOptions.EvilNekomataOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.EvilNekomataOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.EvilNekomata;
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
            if (!(CustomOption.CustomOptions.BestfalsechargeOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.BestfalsechargeOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.Bestfalsecharge;
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
            if (!(CustomOption.CustomOptions.BaitOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.BaitOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.Bait;
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
            if (!(CustomOption.CustomOptions.HomeSecurityGuardOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.HomeSecurityGuardOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.HomeSecurityGuard;
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
            if (!(CustomOption.CustomOptions.MadMateOption.getString().Replace("0%", "") == ""))
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
            AllRoleSetClass.Impoonepar = Impoonepar;
            AllRoleSetClass.Imponotonepar = Imponotonepar;
            AllRoleSetClass.Neutonepar = Neutonepar;
            AllRoleSetClass.Neutnotonepar = Neutnotonepar;
            AllRoleSetClass.Crewonepar = Crewonepar;
            AllRoleSetClass.Crewnotonepar = Crewnotonepar;
        }
    }
}
