using System;
using System.Collections.Generic;
using System.Text;
using InnerNet;
using SuperNewRoles.Intro;
using SuperNewRoles.Roles;

namespace SuperNewRoles.Mode.SuperHostRoles
{
    class RoleChat
    {
        public static bool SendChat(ChatController __instance)
        {
            string text = __instance.TextArea.text;
            string[] args = text.Split(' ');
            bool handled = false;
            if (AmongUsClient.Instance.GameState != InnerNetClient.GameStates.Started)
            {
                if (AmongUsClient.Instance.AmHost)
                {
                    switch (args[0])
                    {
                        case "/help":
                            if (args.Length != 1)
                            {
                                switch (args[1])
                                {
                                    case "role":
                                        if (args.Length != 2)
                                        {
                                            PlayerControl.LocalPlayer.RpcSendChat(GetRoleDes(args[3]));
                                        }
                                        break;
                                }
                            }
                            handled = true;
                            break;
                    }
                }
            }
            return handled;
        }
        public static string GetRoleDes(string rolename)
        {
            string IntroDesc;
            string Desc;
            IntroDate data = IntroDate.CrewmateIntro;
            if (rolename == ModTranslation.getString("LoversName")) { }
            else
            {
                data = GetNameIntroDate(rolename);
                IntroDesc = data.TitleDesc;
                Desc = data.Description;
            }
            if (data == IntroDate.CrewmateIntro) return "";

            string team = "重複";
            if (data.Team == TeamRoleType.Crewmate)
            {
                team = ModTranslation.getString("CrewMateName");
            }
            else if (data.Team == TeamRoleType.Impostor)
            {
                team = ModTranslation.getString("ImpostorName");
            }
            else if (data.Team == TeamRoleType.Neutral)
            {
                team = ModTranslation.getString("NeutralName");
            }
            string returndata = "";
            returndata = rolename + "\n";
            returndata += team + "陣営\n";
            returndata += data.Description;
            return "";
        }
        public static IntroDate GetNameIntroDate(string role)
        {
            Dictionary<string, IntroDate> NameData = new()
            {
                {ModTranslation.getString("JesterName"),IntroDate.JesterIntro },
                {ModTranslation.getString("SheriffName"),IntroDate.SheriffIntro },
                {ModTranslation.getString("MadMateName"),IntroDate.MadMateIntro },
                {ModTranslation.getString("BaitName"),IntroDate.BaitIntro },
                {ModTranslation.getString("HomeSecurityGuardName"),IntroDate.HomeSecurityGuardIntro },
                {ModTranslation.getString("StuntmanName"),IntroDate.StuntManIntro },
                {ModTranslation.getString("HomeSecurityGuardName"),IntroDate.HomeSecurityGuardIntro },
                {ModTranslation.getString("StuntmanName"),IntroDate.StuntManIntro },
                {ModTranslation.getString("EvilGamblerdName"),IntroDate.EvilGamblerIntro },
                {ModTranslation.getString("GodName"),IntroDate.GodIntro },
                {ModTranslation.getString("MinimalistName"),IntroDate.MinimalistIntro },
                {ModTranslation.getString("EgoistName"),IntroDate.EgoistIntro },
                {ModTranslation.getString("MayorName"),IntroDate.MayorIntro },
                {ModTranslation.getString("trueloverName"),IntroDate.trueloverIntro },
                {ModTranslation.getString("TechnicianName"),IntroDate.TechnicianIntro },
                {ModTranslation.getString("MadStuntmanName"),IntroDate.MadStuntManIntro },
                {ModTranslation.getString("SamuraiName"),IntroDate.SamuraiIntro },
            };
            foreach (var data in NameData)
            {
                if (data.Key == role)
                {
                    return data.Value;
                }
            }
            return IntroDate.CrewmateIntro;
        }
    }
}
