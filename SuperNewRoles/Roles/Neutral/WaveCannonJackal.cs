using SuperNewRoles.Buttons;

namespace SuperNewRoles.Roles.Neutral;

class WaveCannonJackal
{
    public static void ResetCooldowns()
    {
        HudManagerStartPatch.JackalKillButton.MaxTimer = CustomOptionHolder.WaveCannonJackalKillCooldown.GetFloat();
        HudManagerStartPatch.JackalKillButton.Timer = HudManagerStartPatch.JackalKillButton.MaxTimer;
    }
}