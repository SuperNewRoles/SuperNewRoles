using System.Collections.Generic;
using InnerNet;
using SuperNewRoles.Intro;

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
            if (rolename == ModTranslation.GetString("LoversName")) { }
            else
            {
                data = GetNameIntroDate(rolename);
                IntroDesc = data.TitleDesc;
                Desc = data.Description;
            }
            if (data == IntroDate.CrewmateIntro) return "";

            string team = "重複";
            if (data.Team == TeamRoleType.Crewmate) team = ModTranslation.GetString("CrewMateName");
            else if (data.Team == TeamRoleType.Impostor) team = ModTranslation.GetString("ImpostorName");
            else if (data.Team == TeamRoleType.Neutral)
            {
                team = ModTranslation.GetString("NeutralName");
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
                { ModTranslation.GetString("JesterName"), IntroDate.JesterIntro },
                { ModTranslation.GetString("SheriffName"), IntroDate.SheriffIntro },
                { ModTranslation.GetString("MadMateName"), IntroDate.MadMateIntro },
                { ModTranslation.GetString("BaitName"), IntroDate.BaitIntro },
                { ModTranslation.GetString("HomeSecurityGuardName"), IntroDate.HomeSecurityGuardIntro },
                { ModTranslation.GetString("StuntmanName"), IntroDate.StuntManIntro },
                { ModTranslation.GetString("HomeSecurityGuardName"), IntroDate.HomeSecurityGuardIntro },
                { ModTranslation.GetString("StuntmanName"), IntroDate.StuntManIntro },
                { ModTranslation.GetString("EvilGamblerdName"), IntroDate.EvilGamblerIntro },
                { ModTranslation.GetString("GodName"), IntroDate.GodIntro },
                { ModTranslation.GetString("MinimalistName"), IntroDate.MinimalistIntro },
                { ModTranslation.GetString("EgoistName"), IntroDate.EgoistIntro },
                { ModTranslation.GetString("MayorName"), IntroDate.MayorIntro },
                { ModTranslation.GetString("trueloverName"), IntroDate.trueloverIntro },
                { ModTranslation.GetString("TechnicianName"), IntroDate.TechnicianIntro },
                { ModTranslation.GetString("MadStuntmanName"), IntroDate.MadStuntManIntro },
                { ModTranslation.GetString("SamuraiName"), IntroDate.SamuraiIntro },
                { ModTranslation.GetString("BlackCatName"), IntroDate.BlackCatIntro },
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