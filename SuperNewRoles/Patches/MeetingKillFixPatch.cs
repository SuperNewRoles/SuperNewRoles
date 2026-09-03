using System.Collections.Generic;
using HarmonyLib;
using SuperNewRoles.Modules;
using UnityEngine;

namespace SuperNewRoles.Patches;

[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Start))]
public static class MeetingHud_Start_DeadBodyCache
{
    public static void Postfix()
    {
        MeetingHud_Update.ResetDeadBodyCache();
    }
}

[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Update))]
public static class MeetingHud_Update
{
    private static int counter = 0;
    private static int _cachedDeadPlayerCount = -1;
    private static int _lastFoundBodyCount = -1;
    private static readonly HashSet<byte> DeadPlayers = new();

    internal static void ResetDeadBodyCache()
    {
        counter = 0;
        _cachedDeadPlayerCount = -1;
        _lastFoundBodyCount = -1;
        DeadPlayers.Clear();
    }

    private static int CountDeadPlayers()
    {
        int count = 0;
        if (PlayerControl.AllPlayerControls == null)
            return 0;
        foreach (PlayerControl player in PlayerControl.AllPlayerControls)
        {
            if (player?.Data != null && player.Data.IsDead)
                count++;
        }
        return count;
    }

    public static void Postfix(MeetingHud __instance)
    {
        counter++;
        if (counter < 10 && counter > 0) return;
        counter = 0;

        int deadPlayerCount = CountDeadPlayers();
        if (deadPlayerCount == _cachedDeadPlayerCount && _lastFoundBodyCount == 0)
            return;

        DeadPlayers.Clear();
        DeadBody[] bodies = GameObject.FindObjectsOfType<DeadBody>();
        _lastFoundBodyCount = bodies.Length;
        // 会議開始直前のキルでは、死体が会議生成処理で消費されることがある。
        // 死体だけを信頼すると、死亡済みのプレイヤーが投票枠では生存扱いのまま残る。
        if (PlayerControl.AllPlayerControls != null)
        {
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            {
                if (player?.Data != null && player.Data.IsDead)
                    DeadPlayers.Add(player.PlayerId);
            }
        }
        // 死体がまだ生成されていない会議キルでは、死亡数だけ先に増える。
        // 空スキャンで cached を更新すると以降スキップされ、死体も投票枠も直らない。
        if (bodies.Length != 0 || deadPlayerCount == 0)
            _cachedDeadPlayerCount = deadPlayerCount;
        foreach (DeadBody deadBody in bodies)
        {
            DeadPlayers.Add(deadBody.ParentId);
            GameObject.Destroy(deadBody.gameObject);
        }
        if (DeadPlayers.Count == 0) return;
        bool dead = false;
        foreach (PlayerVoteArea pva in MeetingHud.Instance.playerStates)
        {
            if (pva.AmDead) continue;
            if (DeadPlayers.Contains(pva.PlayerId))
            {
                pva.SetDead(true);
                pva.Overlay.gameObject.SetActive(true);
                dead = true;
            }
        }
        if (!dead) return;
        if (Constants.ShouldPlaySfx()) SoundManager.Instance.PlaySound(ExPlayerControl.LocalPlayer.Player.KillSfx, false, 0.8f);
        foreach (PlayerVoteArea pva in MeetingHud.Instance.playerStates)
            pva.UnsetVote();
        MeetingHud.Instance.ClearVote(PlayerControl.LocalPlayer.PlayerId, true);
        if (AmongUsClient.Instance.AmHost)
            MeetingHud.Instance.CheckForEndVoting();
    }
}
