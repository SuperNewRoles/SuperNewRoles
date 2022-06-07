using HarmonyLib;
using SuperNewRoles.CustomRPC;
using SuperNewRoles.Intro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperNewRoles.Roles
{
    class HandleGhostRole
    {
        [HarmonyPatch(typeof(RoleManager),nameof(RoleManager.TryAssignRoleOnDeath))]
        class AssignRole
        {
            public static bool Prefix(RoleManager __instance, [HarmonyArgument(0)] PlayerControl player)
            {
                return !HandleAssign(player);
            }
        }
        public static bool HandleAssign(PlayerControl player)
        {
            if (player.isCrew())
            {
                return HandleCrewAssign(player);
            } else if (player.isNeutral())
            {
                return HandleNeutralAssign(player);
            } else
            {
                return HandleImpostorAssign(player);
            }
        }
        public static bool HandleCrewAssign(PlayerControl player)
        {
            List<IntroDate> GhostRoles = new List<IntroDate>();
            foreach (IntroDate intro in IntroDate.GhostRoleDatas)
            {
                if (intro.Team != TeamRoleType.Crewmate) continue;
                GhostRoles.Add(intro);
            }
            var assignrole = Assing(GhostRoles);
            if (assignrole == RoleId.DefaultRole) return false;
            player.setRoleRPC(assignrole);
            return true;
        }
        public static bool HandleNeutralAssign(PlayerControl player)
        {
            List<IntroDate> GhostRoles = new List<IntroDate>();
            foreach (IntroDate intro in IntroDate.GhostRoleDatas)
            {
                if (intro.Team != TeamRoleType.Neutral) continue;
                GhostRoles.Add(intro);
            }
            var assignrole = Assing(GhostRoles);
            if (assignrole == RoleId.DefaultRole) return false;
            player.setRoleRPC(assignrole);
            return false;
        }
        public static bool HandleImpostorAssign(PlayerControl player)
        {
            List<IntroDate> GhostRoles = new List<IntroDate>();
            foreach (IntroDate intro in IntroDate.GhostRoleDatas)
            {
                if (intro.Team != TeamRoleType.Impostor) continue;
                GhostRoles.Add(intro);
            }
            var assignrole = Assing(GhostRoles);
            if (assignrole == RoleId.DefaultRole) return false;
            player.setRoleRPC(assignrole);
            return false;
        }
        public static RoleId Assing(List<IntroDate> datas)
        {
            List<RoleId> Assigns = new List<RoleId>();
            List<RoleId> Assignnos = new List<RoleId>();
            foreach (IntroDate data in datas)
            {
                var count = AllRoleSetClass.GetPlayerCount(data.RoleId);
                var selection = IntroDate.GetOption(data.RoleId).getSelection();
                if (selection != 0 && count > 0 && count > PlayerControl.AllPlayerControls.ToArray().ToList().Count((PlayerControl pc)=> false))
                {
                    if (selection == 10)
                    {
                        Assigns.Add(data.RoleId);
                    } else if (Assigns.Count < 0){
                        for (int i = 0; i < selection;i++) {
                            Assignnos.Add(data.RoleId);
                        }
                    }
                }
            }
            if (Assigns.Count > 0)
            {
                return ModHelpers.GetRandom(Assigns);
            } else if (Assignnos.Count > 0)
            {
                return ModHelpers.GetRandom(Assignnos);
            }
            return RoleId.DefaultRole;
        }
    }
}
