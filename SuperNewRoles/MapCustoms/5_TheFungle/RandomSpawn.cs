using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using HarmonyLib;
using SuperNewRoles.Helpers;
using SuperNewRoles.Mode;
using UnityEngine;

namespace SuperNewRoles.MapCustoms;
public static class FungleRandomSpawn
{
    public static readonly Vector2[] SpawnPositions = new Vector2[]
    {
        new(-9.81f, 0.6f), //キャンプファイアー下
        new(-8f, 10.5f), //ドロップシップ
        new(-16.16f, 7.25f), //カフェ
        new(-15.5f, -7.5f), //キッチン
        new(9.25f, -12f), //温室
        new(14.75f, 0f), //中腹
        new(21.65f, 13.75f) //通信
    };
    public static IEnumerator Spawn(PlayerControl player)
    {
        yield return Effects.Wait(3f);
        player.RpcSnapTo(ModHelpers.GetRandom(SpawnPositions));
        yield break;
    }

    [HarmonyPatch(typeof(ShipStatus))]
    public static class ShipStatusPatch
    {
        [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.SpawnPlayer)), HarmonyPostfix]
        public static void SpawnPlayerPostfix()
        {
            if (!MapCustomHandler.IsMapCustom(MapCustomHandler.MapCustomId.TheFungle, false) ||
                !MapCustomClearAndReload.FungleRandomSpawn)
                return;
            HudManager.Instance.StartCoroutine(Spawn(PlayerControl.LocalPlayer).WrapToIl2Cpp());
            if (!ModeHandler.IsMode(ModeId.SuperHostRoles) ||
                !AmongUsClient.Instance.AmHost)
                return;
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            {
                if (player.IsMod()) continue;
                HudManager.Instance.StartCoroutine(Spawn(player).WrapToIl2Cpp());
            }
        }
    }

    [HarmonyPatch(typeof(MeetingHud))]
    public static class MeetingHudClosePatch
    {
        [HarmonyPatch(nameof(MeetingHud.Close)), HarmonyPostfix]
        public static void ClosePostfix()
        {
            if (!MapCustomHandler.IsMapCustom(MapCustomHandler.MapCustomId.TheFungle, false) ||
                !MapCustomClearAndReload.FungleRandomSpawn)
                return;
            HudManager.Instance.StartCoroutine(Spawn(PlayerControl.LocalPlayer).WrapToIl2Cpp());
            if (!ModeHandler.IsMode(ModeId.SuperHostRoles) ||
                !AmongUsClient.Instance.AmHost)
                return;
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            {
                if (player.IsMod()) continue;
                HudManager.Instance.StartCoroutine(Spawn(player).WrapToIl2Cpp());
            }
        }
    }
}