using System.Collections.Generic;
using HarmonyLib;
using SuperNewRoles.Modules;
using UnityEngine;

namespace SuperNewRoles.Patches;

[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Update))]
public static class MeetingHud_Update
{
    private static int counter = 0;
    public static void Postfix(MeetingHud __instance)
    {
        counter++;
        if (counter < 10 && counter > 0) return;
        counter = 0;
        HashSet<byte> deadPlayers = new();
        foreach (DeadBody deadBody in GameObject.FindObjectsOfType<DeadBody>())
        {
            deadPlayers.Add(deadBody.ParentId);
            GameObject.Destroy(deadBody.gameObject);
        }
        if (deadPlayers.Count == 0) return;
        bool dead = false;
        foreach (PlayerVoteArea pva in MeetingHud.Instance.playerStates)
        {
            if (pva.AmDead) continue;
            if (deadPlayers.Contains(pva.TargetPlayerId))
            {
                pva.SetDead(pva.DidReport, true);
                pva.Overlay.gameObject.SetActive(true);
                dead = true;
            }
        }
        if (!dead) return;
        if (Constants.ShouldPlaySfx()) SoundManager.Instance.PlaySound(ExPlayerControl.LocalPlayer.Player.KillSfx, false, 0.8f);
        foreach (PlayerVoteArea pva in MeetingHud.Instance.playerStates)
            pva.UnsetVote();
        MeetingHud.Instance.ClearVote();
        if (AmongUsClient.Instance.AmHost)
            MeetingHud.Instance.CheckForEndVoting();
    }
}