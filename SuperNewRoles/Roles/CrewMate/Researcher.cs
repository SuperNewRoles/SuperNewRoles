using HarmonyLib;

namespace SuperNewRoles.Roles
{
    class Researcher
    {
        public static bool IsTarget()
        {
            /**
            Vector3 position = CachedPlayer.LocalPlayer.transform.position;
            Vector3 SamplePos = RoleClass.Researcher.SamplePosition;
            int r = 1;
            if ((position.x + r >= SamplePos.x) && (SamplePos.x >= position.x - r))
            {
                if ((position.y + r >= SamplePos.y) && (SamplePos.y >= position.y - r))
                {
                    if ((position.z + r >= SamplePos.z) && (SamplePos.z >= position.z - r))
                    {
                        return true ;
                    }
                }
            }
            **/
            return false;
        }
        [HarmonyPatch(typeof(UseButton), nameof(UseButton.DoClick))]
        class UseButtonUsePatch
        {
            static void Postfix(UseButton __instance)
            {
                /**
                if (AmongUsClient.Instance.GameState != AmongUsClient.GameStates.Started) return;
                if (!RoleClass.Researcher.ResearcherPlayer.IsCheckListPlayerControl(PlayerControl.LocalPlayer)) return;

                Vector3 position = CachedPlayer.LocalPlayer.transform.position;
                Vector3 SamplePos = RoleClass.Researcher.SamplePosition;
                SuperNewRolesPlugin.Logger.LogInfo("pos:"+position);
                SuperNewRolesPlugin.Logger.LogInfo("Samplepos:"+SamplePos);
                if (IsTarget()) {
                    SuperNewRolesPlugin.Logger.LogInfo("TARGETOK");
                    if (RoleClass.Researcher.MySample == 0) {
                        SuperNewRolesPlugin.Logger.LogInfo("SAMPLED!!!");
                        //RoleClass.Researcher.MySample--;
                        SoundManager.Instance.PlaySound(Minigame.Instance.OpenSound, false);
                        //RoleClass.Researcher.OKSamplePlayers.Add(RoleClass.Researcher.GetSamplePlayers[0]);
                        //RoleClass.Researcher.GetSamplePlayers.RemoveAt(0);
                    }
                }
                **/
            }
        }
        public class ReseUseButtonSetTargetPatch
        {
            public static void Postfix()
            {
                if (IsTarget())
                {
                    FastDestroyableSingleton<HudManager>.Instance.UseButton.SetEnabled();
                }
            }
        }
    }
}