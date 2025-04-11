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
    public static SpawnInMinigame spawnInMinigame;
    public static bool spawnSuccess = false;

    [HarmonyPatch(typeof(SpawnInMinigame), nameof(SpawnInMinigame.SpawnAt))]
    public static class SpawnInMinigameSpawnAtPatch
    {
        public static bool Prefix(SpawnInMinigame __instance, SpawnInMinigame.SpawnLocation spawnPoint)
        {
            if (!GameSettingOptions.SyncSpawn) return true;
            if (__instance.amClosing != Minigame.CloseState.None) return false;
            {
                __instance.gotButton = true;
            }
            SpawnLocation = spawnPoint;
            spawnInMinigame = __instance;
            RpcSpawnSelected(ExPlayerControl.LocalPlayer);
            __instance.StopAllCoroutines();
            __instance.StartCoroutine(WaitForSpawn().WrapToIl2Cpp());
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

    public static IEnumerator WaitForSpawn()
    {
        float startTime = Time.time;
        GameObject selectedButton = spawnInMinigame.LocationButtons.FirstOrDefault(x => x.GetComponent<ButtonAnimRolloverHandler>().StaticOutImage == SpawnLocation.Image).gameObject;
        List<GameObject> notSelectedButtons = spawnInMinigame.LocationButtons
            .Where(x => x.GetComponent<ButtonAnimRolloverHandler>().StaticOutImage != SpawnLocation.Image)
            .Select(x => x.gameObject).ToList();
        yield return null;

        Vector3 initialPos = selectedButton.transform.localPosition;
        Vector3 targetPos = new Vector3(0f, -0.25f, 0f);
        float fadeDuration = 0.2f; // フェードアウトにかかる時間（遅延なし）
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
            float elapsed = Time.time - startTime;

            // フェードアウトの処理（即開始、0.2秒かけてフェードアウト）
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

            // 選択ボタンの移動処理（0.1秒の遅延後、0.4秒かけて滑らかに移動、動き出しは早く、終わりは遅くなるようにイージング適用）
            if (!moveFinished)
            {
                if (elapsed < moveDelay)
                {
                    selectedButton.transform.localPosition = initialPos;
                }
                else if (elapsed < moveDelay + moveDuration)
                {
                    float normalizedTime = Mathf.Clamp01((elapsed - moveDelay) / moveDuration);
                    float easeValue = Mathf.Sin(normalizedTime * (Mathf.PI * 0.5f)); // 動き出し早く、終盤で徐々に減速
                    selectedButton.transform.localPosition = Vector3.Lerp(initialPos, targetPos, easeValue);
                }
                else
                {
                    selectedButton.transform.localPosition = targetPos;
                    moveFinished = true;
                }
            }

            // 両方のアニメーションが完了したら、全てのボタンのOnMouseOutを呼び出し、ボタンを無効化する
            if (fadeFinished && moveFinished && !animationFinished)
            {
                foreach (var button in spawnInMinigame.LocationButtons)
                {
                    button.OnMouseOut?.Invoke();
                    button.enabled = false;
                }
                animationFinished = true;
            }

            if (spawnInMinigame == null) yield break;
            if (SpawnedPlayers.Count >= ExPlayerControl.ExPlayerControls.Count && spawnInMinigame != null && AmongUsClient.Instance.AmHost)
            {
                new LateTask(() => RpcAllSelectedFromHost(), 0f);
            }
            spawnInMinigame.Text.text = ModTranslation.GetString("WaitingSpawnText",
                ExPlayerControl.ExPlayerControls.Count - SpawnedPlayers.Count,
                ExPlayerControl.ExPlayerControls.Count);
            yield return null;
        }
    }
    [CustomRPC]
    public static void RpcSpawnSelected(ExPlayerControl source)
    {
        if (!GameSettingOptions.SyncSpawn) return;
        SpawnedPlayers.Add(source);
    }
    [CustomRPC]
    public static void RpcAllSelectedFromHost()
    {
        spawnSuccess = true;
        // デバッグログを追加
        if (!GameSettingOptions.SyncSpawn)
            return;

        SpawnInMinigame spawnInMinigame = GameObject.FindObjectOfType<SpawnInMinigame>();
        if (spawnInMinigame == null)
            return;

        PlayerControl.LocalPlayer.SetKinematic(b: true);
        PlayerControl.LocalPlayer.NetTransform.SetPaused(isPaused: true);
        PlayerControl.LocalPlayer.NetTransform.RpcSnapTo(SpawnLocation.Location);

        DestroyableSingleton<HudManager>.Instance.PlayerCam.SnapToTarget();

        spawnInMinigame.StopAllCoroutines();
        spawnInMinigame.StartCoroutine(spawnInMinigame.CoSpawnAt(PlayerControl.LocalPlayer, SpawnLocation));


        spawnInMinigame = null;
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