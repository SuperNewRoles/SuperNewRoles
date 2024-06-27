using HarmonyLib;

namespace SuperNewRoles.Roles.Impostor;

public static class Cracker
{
    //ここにコードを書きこんでください
    [HarmonyPatch(typeof(VitalsMinigame), nameof(VitalsMinigame.Update))]
    class VitalsMinigameUpdatePatch
    {
        static void Postfix(VitalsMinigame __instance)
        {
            for (int k = 0; k < __instance.vitals.Length; k++)
            {
                VitalsPanel vitalsPanel = __instance.vitals[k];
                NetworkedPlayerInfo player = GameData.Instance.AllPlayers[k];
                if (!CustomOptionHolder.CrackerIsVitalsView.GetBool() && RoleClass.Cracker.CrackedPlayers.Contains(player.PlayerId) && (player.PlayerId != CachedPlayer.LocalPlayer.PlayerId || !CustomOptionHolder.CrackerIsSelfNone.GetBool()))
                    if (!vitalsPanel.IsDead)
                        vitalsPanel.SetDead();
            }
        }
    }

    public static void WrapUp()
    {
        RoleClass.Cracker.TurnCount = RoleClass.Cracker.DefaultCount;
        RoleClass.Cracker.CrackedPlayers = new();
        RoleClass.Cracker.currentCrackedPlayers = new();
    }
}