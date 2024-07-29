using System;
using System.Collections.Generic;
using HarmonyLib;
using SuperNewRoles.CustomObject;
using SuperNewRoles.Roles;
using SuperNewRoles.Roles.RoleBases;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SuperNewRoles.Patches.CursedTasks;

public class CursedMedScanTask
{
    public static bool IsRetrograde;
    public static bool IsRetrogradeStop;
    public static bool Isﾖｯｷﾝｸﾞ;
    public static List<BloodStain> Bloods;
    public static DateTime Timer;

    [HarmonyPatch(typeof(MedScanMinigame))]
    public static class MedScanMinigamePatch
    {
        [HarmonyPatch(nameof(MedScanMinigame.Begin)), HarmonyPrefix]
        public static void BeginPrefix()
        {
            if (!Main.IsCursed) return;
            IsRetrograde = false;
            IsRetrogradeStop = false;
            Isﾖｯｷﾝｸﾞ = BoolRange.Next(0.2f);
            Bloods = new();
            Timer = DateTime.Now;
        }

        [HarmonyPatch(nameof(MedScanMinigame.FixedUpdate)), HarmonyPrefix]
        public static void FixedUpdatePrefix(MedScanMinigame __instance)
        {
            if (!Main.IsCursed) return;
            __instance.ScanDuration = 60f;
            string color = FastDestroyableSingleton<TranslationController>.Instance.GetString(Palette.ColorNames[PlayerControl.LocalPlayer.Data.DefaultOutfit.ColorId]);
            string playerIdText = $"0b{Convert.ToInt32(Convert.ToString(PlayerControl.LocalPlayer.PlayerId, 2)):000000000}";
            string text = string.Format(ModTranslation.GetString("CursedMedScanTaskText"), color.Replace("(MOD)", ""), playerIdText, PlayerControl.LocalPlayer.Data.DefaultOutfit.PlayerName, CustomRoles.GetRoleTeam(PlayerControl.LocalPlayer), PlayerControl.LocalPlayer.IsHauntedWolf() ? IntroData.CrewmateIntro.Name : CustomRoles.GetRoleName(PlayerControl.LocalPlayer), color, MedScanMinigame.BloodTypes[(int)PlayerControl.LocalPlayer.BodyType]);
            string errortext = "";
            int dealing = Math.DivRem(text[(int)Math.Ceiling(text.Length / (__instance.ScanDuration - 10) * (__instance.ScanDuration / 2) + 1)..].Length, (Isﾖｯｷﾝｸﾞ ? " ﾖｯｷﾝｸﾞﾍﾞｯﾋﾟﾝｲｰｴｯｸｽｵｲｼｲ" : " Error").Length, out int over);
            for (int i = 0; i < dealing; i++)
                errortext += Isﾖｯｷﾝｸﾞ ? " ﾖｯｷﾝｸﾞﾍﾞｯﾋﾟﾝｲｰｴｯｸｽｵｲｼｲ" : " Error";
            for (int i = 0; i < over; i++)
                errortext += " ";
            __instance.completeString = IsRetrogradeStop ? text : text[..(int)Math.Ceiling(text.Length / (__instance.ScanDuration - 10) * (__instance.ScanDuration / 2))] + errortext;
            if (!IsRetrograde && __instance.ScanTimer >= 45) IsRetrograde = true;
            if (IsRetrograde && !IsRetrogradeStop)
            {
                __instance.ScanTimer -= Time.fixedDeltaTime * 2;
                __instance.charStats.text = __instance.completeString[..(int)Math.Ceiling(__instance.completeString.Length / (__instance.ScanDuration - 10) * __instance.ScanTimer)];
                if (__instance.ScanTimer <= 30) IsRetrogradeStop = true;
            }
            if (__instance.ScanTimer >= 30 && !IsRetrogradeStop)
            {
                __instance.charStats.color = BoolRange.Next(0.5f) ? Color.red : Color.white;
                TimeSpan span = new(0, 0, 0, 5);
                if ((Timer + span - DateTime.Now).TotalSeconds <= 0f)
                {
                    Timer = DateTime.Now;
                    Vector2 screen = FastDestroyableSingleton<HudManager>.Instance.FullScreen.gameObject.transform.position;
                    BloodStain blood = new(new(RoleClass.rnd.Next(-325, 325) / 100f + screen.x, RoleClass.rnd.Next(-225, 265) / 100f + screen.y, Camera.main.transform.position.z - 5f));
                    blood.BloodStainObject.layer = 5;
                    blood.BloodStainObject.transform.localScale *= 15f;
                    Bloods.Add(blood);
                }
            }
            else
            {
                __instance.charStats.color = Color.white;
                foreach (var blood in Bloods)
                    Object.Destroy(blood.BloodStainObject);
                Bloods.Clear();
            }
        }

        [HarmonyPatch(nameof(MedScanMinigame.Close)), HarmonyPrefix]
        public static void ClosePrefix()
        {
            if (!Main.IsCursed) return;
            foreach (var blood in Bloods)
                Object.Destroy(blood.BloodStainObject);
            Bloods.Clear();
        }
    }
}