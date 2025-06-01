using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using HarmonyLib;
using SuperNewRoles.CustomOptions.Categories;
using TMPro;
using UnityEngine;

namespace SuperNewRoles.Modules;

public static class SyncSpawn
{
    public static HashSet<ExPlayerControl> SpawnedPlayers = new();
    public static SpawnInMinigame.SpawnLocation SpawnLocation;
    public static bool spawnSuccess = false;

    [HarmonyPatch(typeof(SpawnInMinigame), nameof(SpawnInMinigame.SpawnAt))]
    public static class SpawnInMinigameSpawnAtPatch
    {
        public static bool Prefix(SpawnInMinigame __instance, SpawnInMinigame.SpawnLocation spawnPoint)
        {
            if (!GameSettingOptions.SyncSpawn) return true;
            if (__instance.amClosing != Minigame.CloseState.None) return false;
            __instance.gotButton = true;
            SpawnLocation = spawnPoint;
            RpcSpawnSelected(ExPlayerControl.LocalPlayer);
            __instance.StopAllCoroutines();
            __instance.StartCoroutine(WaitForSpawn(__instance).WrapToIl2Cpp());
            return false;
        }
    }

    [HarmonyPatch(typeof(SpawnInMinigame), nameof(SpawnInMinigame.Close), [])]
    public static class SpawnInMinigameClosePatch
    {
        public static bool Prefix(SpawnInMinigame __instance)
        {
            if (spawnSuccess) return true;
            if (!GameSettingOptions.SyncSpawn) return true;
            if (!__instance.gotButton)
                __instance.LocationButtons[__instance.LocationButtons.GetRandomIndex()].ReceiveClickUp();
            return false;
        }
    }

    public static IEnumerator WaitForSpawn(SpawnInMinigame __instance)
    {
        Logger.Info("WaitForSpawn");
        const float EaseLevel = 0.55f; // 0~1の範囲で指定（1に近いほど最初が速く、後が遅くなる）

        float startTime = Time.time;
        GameObject selectedButton = __instance.LocationButtons.FirstOrDefault(x => x.GetComponent<ButtonAnimRolloverHandler>().StaticOutImage == SpawnLocation.Image).gameObject;
        List<GameObject> notSelectedButtons = __instance.LocationButtons
            .Where(x => x.GetComponent<ButtonAnimRolloverHandler>().StaticOutImage != SpawnLocation.Image)
            .Select(x => x.gameObject).ToList();
        yield return null;

        Vector3 initialPos = selectedButton.transform.localPosition;
        Vector3 targetPos = new(0f, -0.25f, 0f);
        float fadeDuration = 0.2f; // フェードアウトにかかる時間
        float moveDelay = 0.1f;    // 移動開始前の遅延
        float moveDuration = 0.4f; // 選択ボタンの移動にかかる時間
        bool fadeFinished = false;
        bool moveFinished = false;
        bool animationFinished = false;

        // notSelectedButtonsの初期状態（アルファ1）を設定
        foreach (var btn in notSelectedButtons)
        {
            SpriteRenderer sr = btn.GetComponent<SpriteRenderer>();
            TextMeshPro text = btn.GetComponentInChildren<TextMeshPro>();
            sr.color = new Color(1, 1, 1, 1);
            text.color = new Color(1, 1, 1, 1);
        }

        while (true)
        {
            Logger.Info("WaitForSpawn2");
            float elapsed = Time.time - startTime;

            // フェードアウト処理（即開始、0.2秒かけてフェードアウト）
            if (!fadeFinished)
            {
                if (elapsed < fadeDuration)
                {
                    float tFade = Mathf.Clamp01(elapsed / fadeDuration);
                    foreach (var btn in notSelectedButtons)
                    {
                        SpriteRenderer sr = btn.GetComponent<SpriteRenderer>();
                        TextMeshPro text = btn.GetComponentInChildren<TextMeshPro>();
                        sr.color = new Color(1, 1, 1, Mathf.Lerp(1f, 0f, tFade));
                        text.color = new Color(1, 1, 1, Mathf.Lerp(1f, 0f, tFade));
                    }
                }
                else
                {
                    foreach (var btn in notSelectedButtons)
                    {
                        SpriteRenderer sr = btn.GetComponent<SpriteRenderer>();
                        TextMeshPro text = btn.GetComponentInChildren<TextMeshPro>();
                        sr.color = new Color(0, 0, 0, 0);
                        text.color = new Color(0, 0, 0, 0);
                        text.enabled = false;
                    }
                    fadeFinished = true;
                }
            }

            // 選択ボタンの移動処理
            // イージング関数を調整可能に変更
            if (!moveFinished)
            {
                if (elapsed < moveDelay)
                {
                    selectedButton.transform.localPosition = initialPos;
                }
                else if (elapsed < moveDelay + moveDuration)
                {
                    float normalizedTime = Mathf.Clamp01((elapsed - moveDelay) / moveDuration);

                    // EaseLevelに基づいてイージング関数を調整
                    // EaseLevel=0の場合は線形補間、1に近づくほど初速が速く後半が遅くなる
                    float power = 1f + 3f * EaseLevel; // 1.0～4.0の範囲で調整
                    float easeValue = 1f - Mathf.Pow(1f - normalizedTime, power);

                    selectedButton.transform.localPosition = Vector3.Lerp(initialPos, targetPos, easeValue);
                }
                else
                {
                    selectedButton.transform.localPosition = targetPos;
                    moveFinished = true;
                }
            }

            Logger.Info("WaitForSpawn5");

            // アニメーション完了後、全ボタンのOnMouseOutを呼び出し、無効化する
            if (fadeFinished && moveFinished && !animationFinished)
            {
                foreach (var button in __instance.LocationButtons)
                {
                    button.OnMouseOut?.Invoke();
                    button.enabled = false;
                }
                animationFinished = true;
            }

            Logger.Info("WaitForSpawn6");

            if (__instance == null) yield break;
            Logger.Info("WaitForSpawn3");
            int aliveCount = ExPlayerControl.ExPlayerControls.Count(x => x.IsAlive());
            if (SpawnedPlayers.Count >= aliveCount && __instance != null && AmongUsClient.Instance.AmHost)
            {
                Logger.Info("WaitForSpawn4");
                new LateTask(() => RpcAllSelectedFromHost(), 0.5f);
                yield break;
            }
            __instance.Text.text = ModTranslation.GetString("WaitingSpawnText",
                aliveCount - SpawnedPlayers.Count,
                aliveCount);
            yield return null;
        }
    }
    [CustomRPC]
    public static void RpcSpawnSelected(ExPlayerControl source)
    {
        Logger.Info("RpcSpawnSelected");
        if (source.IsDead()) return;
        SpawnedPlayers.Add(source);
        Logger.Info($"RpcSpawnSelected: {source.Player.name} が選択されました。");
        Logger.Info($"RpcSpawnSelected: 選択中のプレイヤー数: {SpawnedPlayers.Count}");
        foreach (var player in SpawnedPlayers)
        {
            Logger.Info($"RpcSpawnSelected: {player.Player.name} が選択中");
        }
        Logger.Info($"---------------------------------------");
    }
    [CustomRPC]
    public static void RpcAllSelectedFromHost()
    {
        Logger.Info($"[SyncSpawn] RpcAllSelectedFromHost executed on client {PlayerControl.LocalPlayer.name}({PlayerControl.LocalPlayer.PlayerId}). SpawnLocation: {SpawnLocation?.Name.ToString() ?? "null"}. Current SpawnedPlayers.Count on this client: {SpawnedPlayers.Count}. Players: {string.Join(", ", SpawnedPlayers.Select(p => p.Player.name))}");
        spawnSuccess = true;
        if (!GameSettingOptions.SyncSpawn)
        {
            Logger.Info("[SyncSpawn] SyncSpawn option is disabled, RpcAllSelectedFromHost doing nothing further.");
            return;
        }

        SpawnInMinigame spawnInMinigame = GameObject.FindObjectOfType<SpawnInMinigame>();
        if (spawnInMinigame == null)
        {
            Logger.Error("[SyncSpawn] SpawnInMinigame instance not found in RpcAllSelectedFromHost. Aborting.");
            return;
        }

        PlayerControl.LocalPlayer.SetKinematic(b: true);
        PlayerControl.LocalPlayer.NetTransform.SetPaused(isPaused: true);
        PlayerControl.LocalPlayer.NetTransform.RpcSnapTo(SpawnLocation.Location);

        Logger.Info("RpcAllSelectedFromHost4");
        DestroyableSingleton<HudManager>.Instance.PlayerCam.SnapToTarget();

        spawnInMinigame.StopAllCoroutines();
        spawnInMinigame.StartCoroutine(spawnInMinigame.CoSpawnAt(PlayerControl.LocalPlayer, SpawnLocation));

        Logger.Info($"[SyncSpawn] RpcAllSelectedFromHost finished for {PlayerControl.LocalPlayer.name}({PlayerControl.LocalPlayer.PlayerId})");
    }
    public static void ClearAndReloads()
    {
        SpawnedPlayers.Clear();
        spawnSuccess = false;
    }
    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.CoStartGame))]
    public static class AmongUsClientCoStartGamePatch
    {
        public static void Postfix()
        {
            ClearAndReloads();
        }
    }
    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Start))]
    public static class MeetingHudStartPatch
    {
        public static void Postfix()
        {
            ClearAndReloads();
        }
    }
}