
using SuperNewRoles.Buttons;

namespace SuperNewRoles.Roles
{
    class TimeMaster
    {
        public static void ResetTimeMasterButton()
        {
            HudManagerStartPatch.TimeMasterTimeMasterShieldButton.Timer = HudManagerStartPatch.TimeMasterTimeMasterShieldButton.MaxTimer;
            HudManagerStartPatch.TimeMasterTimeMasterShieldButton.isEffectActive = false;
            HudManagerStartPatch.TimeMasterTimeMasterShieldButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
        }

    }
}

