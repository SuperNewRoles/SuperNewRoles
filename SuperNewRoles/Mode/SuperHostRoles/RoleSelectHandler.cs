
using SuperNewRoles.CustomRPC;
using SuperNewRoles.Patch;
using SuperNewRoles.Roles;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SuperNewRoles.Mode.SuperHostRoles
{
    public static class RoleSelectHandler
    {
        public static void RoleSelect()
        {
            if (!AmongUsClient.Instance.AmHost) return;
            OneOrNotListSet();
            CrewOrImpostorSet();
            AllRoleSetClass.AllRoleSet();
            SetCustomRoles();
            SyncSetting.CustomSyncSettings();
            ChacheManager.ResetChache();
            FixedUpdate.SetNames();
            main.SendAllRoleChat();
            new LateTask(() => {
                if (AmongUsClient.Instance.GameState == AmongUsClient.GameStates.Started)
                {
                    foreach (var pc in PlayerControl.AllPlayerControls)
                    {
                        pc.RpcSetRole(RoleTypes.Shapeshifter);
                    }
                }
            }, 3f, "SetImpostor");
        }
        public static void SetCustomRoles() {
            foreach (PlayerControl SheriffPlayer in RoleClass.Sheriff.SheriffPlayer)
            {
                if (!SheriffPlayer.IsMod())
                {
                    SheriffPlayer.RpcSetRoleDesync(RoleTypes.Impostor);
                    foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                    {
                        if (p.PlayerId != SheriffPlayer.PlayerId)
                        {
                            p.RpcSetRoleDesync(RoleTypes.Scientist, SheriffPlayer);
                        }
                    }
                } else
                {
                    SheriffPlayer.RpcSetRole(RoleTypes.Crewmate);
                }
            }
            if (RoleClass.Jester.IsUseVent)
            {
                foreach (PlayerControl p in RoleClass.Jester.JesterPlayer)
                {
                    if (!ShareGameVersion.GameStartManagerUpdatePatch.VersionPlayers.ContainsKey(p.getClientId()))
                    {
                        p.RpcSetRoleDesync(RoleTypes.Engineer);
                    }
                }
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
            }
            foreach (PlayerControl p in RoleClass.Egoist.EgoistPlayer)
            {
                p.RpcSetRoleDesync(RoleTypes.Impostor);
                foreach (PlayerControl p2 in PlayerControl.AllPlayerControls)
                {
                    p2.RpcSetRoleDesync(RoleTypes.Scientist, p);
                    p.RpcSetRoleDesync(RoleTypes.Impostor, p2);
                }
            }
        }
        public static void CrewOrImpostorSet()
        {
            AllRoleSetClass.CrewMatePlayers = new List<PlayerControl>();
            AllRoleSetClass.ImpostorPlayers = new List<PlayerControl>();
            foreach (PlayerControl Player in PlayerControl.AllPlayerControls)
            {
                if (AllRoleSetClass.impostors.IsCheckListPlayerControl(Player))
                {
                    AllRoleSetClass.ImpostorPlayers.Add(Player);
                }
                else
                {
                    AllRoleSetClass.CrewMatePlayers.Add(Player);
                }
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
            if (!(CustomOption.CustomOptions.StuntManOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.StuntManOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.StuntMan;
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
            if (!(CustomOption.CustomOptions.EgoistOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.EgoistOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.Egoist;
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
            if (!(CustomOption.CustomOptions.SheriffOption.getString().Replace("0%", "") == ""))
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
            
            if (!(CustomOption.CustomOptions.MinimalistOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.MinimalistOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.Minimalist;
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
            
            AllRoleSetClass.Impoonepar = Impoonepar;
            AllRoleSetClass.Imponotonepar = Imponotonepar;
            AllRoleSetClass.Neutonepar = Neutonepar;
            AllRoleSetClass.Neutnotonepar = Neutnotonepar;
            AllRoleSetClass.Crewonepar = Crewonepar;
            AllRoleSetClass.Crewnotonepar = Crewnotonepar;
        }
    }
}
