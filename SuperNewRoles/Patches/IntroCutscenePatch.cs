using System.Collections;
using System.Linq;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using HarmonyLib;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles;
using UnityEngine;

namespace SuperNewRoles.Patches;

[HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.ShowRole))]
class SetUpRoleTextPatch
{
    private static byte ToByteIntro(float f)
    {
        f = Mathf.Clamp01(f);
        return (byte)(f * 255);
    }
    static bool Prefix(IntroCutscene __instance, ref Il2CppSystem.Collections.IEnumerator __result)
    {
        __result = SetupRole(__instance).WrapToIl2Cpp();
        return false;
    }

    private static IEnumerator SetupRole(IntroCutscene __instance)
    {
        new LateTask(() =>
        {
            ExPlayerControl player = PlayerControl.LocalPlayer;
            RoleId myrole = player.Role;

            if (player.Role is RoleId.BestFalseCharge)
                myrole = RoleId.Crewmate;
            var rolebase = CustomRoleManager.GetRoleById(myrole);
            if (rolebase != null)
            {
                Color roleColor = rolebase.RoleColor;
                __instance.YouAreText.color = roleColor;           //あなたのロールは...を役職の色に変更
                __instance.RoleText.color = roleColor;             //役職名の色を変更
                __instance.RoleBlurbText.color = roleColor;        //イントロの簡易説明の色を変更

                __instance.RoleText.text = ModTranslation.GetString(rolebase.Role.ToString());               //役職名を変更

                var randomIntroNum = Random.Range(1, rolebase.IntroNum + 1); // 1からrolebase.IntroNumまでのランダムな数を取得
                __instance.RoleBlurbText.text = ModTranslation.GetString($"{rolebase.Role}Intro{randomIntroNum}");     //イントロの簡易説明をランダムに変更
            }

            if (myrole is RoleId.Crewmate or RoleId.Impostor)
            {
                __instance.RoleText.text = player.Data.Role.NiceName;
                __instance.RoleBlurbText.text = player.Data.Role.Blurb;
                __instance.YouAreText.color = player.Data.Role.TeamColor;   //あなたのロールは...を役職の色に変更
                __instance.RoleText.color = player.Data.Role.TeamColor;     //役職名の色を変更
                __instance.RoleBlurbText.color = player.Data.Role.TeamColor;//イントロの簡易説明の色を変更
            }

            //プレイヤーを作成&位置変更
            __instance.ourCrewmate = __instance.CreatePlayer(0, 1, PlayerControl.LocalPlayer.Data, false);
            __instance.ourCrewmate.gameObject.SetActive(false);
            __instance.ourCrewmate.transform.localPosition = new Vector3(0f, -1.05f, -18f);
            __instance.ourCrewmate.transform.localScale = new Vector3(1f, 1f, 1f);

            //サウンド再生
            var sound = PlayerControl.LocalPlayer.Data.Role.IntroSound;
            if (rolebase != null)
                sound = RoleManager.Instance.AllRoles.FirstOrDefault(x => x.Role == rolebase.IntroSoundType)?.IntroSound;
            SoundManager.Instance.PlaySound(sound, false, 1);

            //字幕やプレイヤーを再表示する(Prefixで消している)
            __instance.ourCrewmate.gameObject.SetActive(true);
            __instance.YouAreText.gameObject.SetActive(true);
            __instance.RoleText.gameObject.SetActive(true);
            __instance.RoleBlurbText.gameObject.SetActive(true);
        }, 0f, "Override Role Text");

        //メッセージ表示2.5秒後にすべて非表示にする
        yield return new WaitForSeconds(2.5f);
        __instance.ourCrewmate.gameObject.SetActive(false);     //プレイヤーを消す
        __instance.YouAreText.gameObject.SetActive(false);      //あなたのロールは...を消す
        __instance.RoleText.gameObject.SetActive(false);        //役職名を消す
        __instance.RoleBlurbText.gameObject.SetActive(false);   //役職のイントロ説明文を消す

        yield break;
    }
}