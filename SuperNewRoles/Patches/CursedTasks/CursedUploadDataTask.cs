using System;
using AmongUs.Data;
using HarmonyLib;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace SuperNewRoles.Patches.CursedTasks;

public class CursedUploadDataTask
{
    public static GameObject BlueScreen;

    [HarmonyPatch(typeof(UploadDataGame))]
    public static class UploadDataGamePatch
    {
        [HarmonyPatch(nameof(UploadDataGame.Begin)), HarmonyPostfix]
        public static void BeginPostfix(UploadDataGame __instance)
        {
            if (!Main.IsCursed) return;
            if (BlueScreen != null) Object.Destroy(BlueScreen);
            BlueScreen = new("BlueScreen");
            BlueScreen.SetActive(false);
            BlueScreen.transform.position = __instance.transform.position;
            BlueScreen.layer = 5;

            SpriteRenderer sprite = BlueScreen.AddComponent<SpriteRenderer>();
            sprite.sprite = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.Cursed.BlueScreen.png", 275f);
            BlueScreen.transform.SetParent(__instance.transform.FindChild("Background"));

            AudioSource sound = BlueScreen.AddComponent<AudioSource>();
            sound.clip = ModHelpers.loadAudioClipFromResources("SuperNewRoles.Resources.Cursed.BlueScreenSound.raw", "BlueScreenSound");
            sound.loop = false;
            sound.volume = DataManager.Settings.Audio.SfxVolume >= 0.2f ? DataManager.Settings.Audio.SfxVolume : 0.2f;
            sound.Stop();
        }

        [HarmonyPatch(nameof(UploadDataGame.Click)), HarmonyPostfix]
        public static void ClickPostfix(UploadDataGame __instance)
        {
            if (!Main.IsCursed) return;
            BlueScreen.transform.localPosition = __instance.transform.FindChild("Background/dateTransfer_glassTop").localPosition;
            AudioSource sound = BlueScreen.AddComponent<AudioSource>();
            sound.Stop();
            if (!BlueScreen || !__instance)
            {
                Logger.Info("ブルスクを出せませんでした", "CursedUploadDataTask");
                return;

            }
            bool active = Random.RandomRangeInt(1, 4) != 3;
            float time = Random.RandomRange(0.5f, 2f);
            Logger.Info($"ブルスク : {active}, time : {time}", "CursedUploadDataTask");
            new LateTask(() =>
            {
                if (active)
                {
                    Logger.Info("ブルスク出現!", "CursedUploadDataTask");
                    BlueScreen.SetActive(true);
                    sound.volume = DataManager.Settings.Audio.SfxVolume >= 0.2f ? DataManager.Settings.Audio.SfxVolume : 0.2f;
                    sound.Play();
                }
            }, time, "CursedUploadDataTask");
        }
    }

    [HarmonyPatch(typeof(AirshipUploadGame))]
    public static class AirshipUploadGamePatch
    {
        public static DateTime Timer;
        [HarmonyPatch(nameof(AirshipUploadGame.Start)), HarmonyPostfix]
        public static void StartPostfix(AirshipUploadGame __instance)
        {
            if (!Main.IsCursed) return;
            Timer = DateTime.Now;
            __instance.Poor.gameObject.GetComponent<BoxCollider2D>().size /= 2f;
            __instance.Good.gameObject.GetComponent<BoxCollider2D>().size /= 2f;
            __instance.Perfect.gameObject.GetComponent<BoxCollider2D>().size /= 2f;
        }
        [HarmonyPatch(nameof(AirshipUploadGame.Update)), HarmonyPostfix]
        public static void UpdatePostfix(AirshipUploadGame __instance)
        {
            if (!Main.IsCursed) return;
            if (__instance.amClosing != Minigame.CloseState.None) return;
            float num = Time.deltaTime * (__instance.Hotspot.IsTouching(__instance.Perfect) ? 2f :
                                          __instance.Hotspot.IsTouching(__instance.Good) ? 1f :
                                          __instance.Hotspot.IsTouching(__instance.Poor) ? 0.5f : 1f);
            __instance.timer -= num;
            if (__instance.timer <= 0f) __instance.timer = 0f;

            if ((float)(Timer + new TimeSpan(0, 0, 0, 10) - DateTime.Now).TotalSeconds <= 0f)
            {
                Timer = DateTime.Now;
                __instance.Hotspot.transform.localPosition = Random.insideUnitCircle.normalized * 2.5f;
            }
        }
    }
}