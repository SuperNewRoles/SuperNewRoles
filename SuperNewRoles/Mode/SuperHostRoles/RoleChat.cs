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
            IntroData data = IntroData.CrewmateIntro;
            if (rolename == ModTranslation.GetString("LoversName")) { }
            else
            {
                data = GetNameIntroDate(rolename);
                IntroDesc = data.TitleDesc;
                Desc = data.Description;
            }
            if (data == IntroData.CrewmateIntro) return "";

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
        public static IntroData GetNameIntroDate(string role)
        {
            Dictionary<string, IntroData> NameData = new()
            {
                { ModTranslation.GetString("JesterName"), IntroData.JesterIntro },
                { ModTranslation.GetString("SheriffName"), IntroData.SheriffIntro },
                { ModTranslation.GetString("MadMateName"), IntroData.MadMateIntro },
                { ModTranslation.GetString("BaitName"), IntroData.BaitIntro },
                { ModTranslation.GetString("HomeSecurityGuardName"), IntroData.HomeSecurityGuardIntro },
                { ModTranslation.GetString("StuntmanName"), IntroData.StuntManIntro },
                { ModTranslation.GetString("HomeSecurityGuardName"), IntroData.HomeSecurityGuardIntro },
                { ModTranslation.GetString("StuntmanName"), IntroData.StuntManIntro },
                { ModTranslation.GetString("EvilGamblerdName"), IntroData.EvilGamblerIntro },
                { ModTranslation.GetString("GodName"), IntroData.GodIntro },
                { ModTranslation.GetString("MinimalistName"), IntroData.MinimalistIntro },
                { ModTranslation.GetString("EgoistName"), IntroData.EgoistIntro },
                { ModTranslation.GetString("MayorName"), IntroData.MayorIntro },
                { ModTranslation.GetString("trueloverName"), IntroData.trueloverIntro },
                { ModTranslation.GetString("TechnicianName"), IntroData.TechnicianIntro },
                { ModTranslation.GetString("MadStuntmanName"), IntroData.MadStuntManIntro },
                { ModTranslation.GetString("SamuraiName"), IntroData.SamuraiIntro },
                { ModTranslation.GetString("BlackCatName"), IntroData.BlackCatIntro },
            };
            foreach (var data in NameData)
            {
                if (data.Key == role)
                {
                    return data.Value;
                }
            }
            return IntroData.CrewmateIntro;
        }
    }
}