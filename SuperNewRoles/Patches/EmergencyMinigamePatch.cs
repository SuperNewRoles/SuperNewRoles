using AmongUs.GameOptions;
using HarmonyLib;
using Il2CppSystem.Linq;
using SuperNewRoles.Mode;
using SuperNewRoles.Mode.PlusMode;

namespace SuperNewRoles.Patches;

class EmergencyMinigamePatch
{
    public static void ChangeFirstEmergencyCooldown()
    {
        GameOptionsManager.Instance.CurrentGameOptions.SetInt(Int32OptionNames.EmergencyCooldown, LobbyLimit);
    }
}