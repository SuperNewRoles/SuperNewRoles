using System.Collections;
using System.Linq;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using HarmonyLib;
using SuperNewRoles.CustomOptions.Categories;
using SuperNewRoles.Events;
using SuperNewRoles.MapCustoms;
using SuperNewRoles.Modules;
using SuperNewRoles.Modules.Events.Bases;
using SuperNewRoles.Roles;
using SuperNewRoles.Roles.Ability.CustomButton;
using UnityEngine;

namespace SuperNewRoles.Patches;

public static class IntroCutscenePatch
{
    [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.CoBegin))]
    class CoBeginPatch
    {
        public static void Postfix(IntroCutscene __instance)
        {
            PoolablePrefabManager.OnIntroCutsceneDestroy(__instance);
            new LateTask(() =>
            {
                Initialize();
            }, 5f, "Initialize");
        }
    }
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

                foreach (var modifier in player.ModifierRoleBases)
                {
                    var randomIntroNum = Random.Range(1, modifier.IntroNum + 1);
                    __instance.RoleBlurbText.text += "\n" + ModTranslation.GetString($"{modifier.ModifierRole}Intro{randomIntroNum}");
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

    [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.BeginCrewmate))]
    class BeginCrewmatePatch
    {
        public static void Prefix(IntroCutscene __instance, ref Il2CppSystem.Collections.Generic.List<PlayerControl> teamToDisplay)
        {
            SetupIntroTeamIcons(__instance, ref teamToDisplay);
        }

        public static void Postfix(IntroCutscene __instance, ref Il2CppSystem.Collections.Generic.List<PlayerControl> teamToDisplay)
        {
            SetupIntroTeam(__instance, ref teamToDisplay);
        }
    }
    [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.BeginImpostor))]
    class BeginImpostorPatch
    {
        public static void Prefix(IntroCutscene __instance, ref Il2CppSystem.Collections.Generic.List<PlayerControl> yourTeam)
        {
            SetupIntroTeamIcons(__instance, ref yourTeam);
        }

        public static void Postfix(IntroCutscene __instance, ref Il2CppSystem.Collections.Generic.List<PlayerControl> yourTeam)
        {
            SetupIntroTeam(__instance, ref yourTeam);
        }
    }

    /// <summary>
    /// イントロ画面のチームアイコンをセットアップする
    /// </summary>
    /// <param name="__instance">イントロ画面のインスタンス</param>
    /// <param name="yourTeam">チームを表示するプレイヤーのリスト</param>
    public static void SetupIntroTeamIcons(IntroCutscene __instance, ref Il2CppSystem.Collections.Generic.List<PlayerControl> yourTeam)
    {
        if (Categories.ModeOption == ModeId.Default)
        {
            if (ExPlayerControl.LocalPlayer.IsCrewmate())
            {
                yourTeam = new();

                foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                {
                    yourTeam.Add(p);
                }
            }
            else if (ExPlayerControl.LocalPlayer.IsImpostor())
            {
                yourTeam = new();
                foreach (ExPlayerControl player in ExPlayerControl.ExPlayerControls)
                {
                    if (player.IsImpostor())
                    {
                        yourTeam.Add(player);
                    }
                }
            }
            else
            {
                yourTeam = new();
                yourTeam.Add(PlayerControl.LocalPlayer);
            }
        }
    }

    public static void SetupIntroTeam(IntroCutscene __instance, ref Il2CppSystem.Collections.Generic.List<PlayerControl> yourTeam)
    {
        Color32 color = __instance.TeamTitle.color;
        Color32 backcolor = __instance.BackgroundBar.material.color;
        string TeamTitle = __instance.TeamTitle.text;
        string ImpostorText = __instance.ImpostorText.text;
        if (ExPlayerControl.LocalPlayer.IsNeutral())
        {
            TeamTitle = ModTranslation.GetString("Neutral");
            ImpostorText = ModTranslation.GetString("NeutralSubIntro");
            color = new(127, 127, 127, byte.MaxValue);
        }
        else if (ExPlayerControl.LocalPlayer.IsMadRoles())
        {
            color = Palette.ImpostorRed;
            TeamTitle = ModTranslation.GetString("Madmate");
            ImpostorText = ModTranslation.GetString("MadRolesSubIntro");
        }
        else if (ExPlayerControl.LocalPlayer.IsFriendRoles())
        {
            color = Palette.CrewmateBlue;
            TeamTitle = ModTranslation.GetString("JackalFriends");
            ImpostorText = ModTranslation.GetString("FriendSubIntro");
        }
        __instance.TeamTitle.text = TeamTitle;
        __instance.ImpostorText.text = ImpostorText;
        if (ImpostorText != "")
            __instance.ImpostorText.gameObject.SetActive(true);
        __instance.BackgroundBar.material.color = color;
        __instance.TeamTitle.color = color;
    }
    // IntroCutscene.OnDestroyのかわり
    public static void Initialize()
    {
        if (AmongUsClient.Instance.NetworkMode == NetworkModes.FreePlay) return;
        foreach (var player in ExPlayerControl.ExPlayerControls)
        {
            NameText.UpdateNameInfo(player);
        }
        NameText.RegisterNameTextUpdateEvent();
        SaboAndVent.RegisterListener();
        FinalStatusListener.LoadListener();
        CustomDeathExtensions.Register();
        SetTargetPatch.Register();

        FungleAdditionalAdmin.AddAdmin();
        FungleAdditionalElectrical.CreateElectrical();
        MushroomMixup.Initialize();
        ZiplineUpdown.Initialize();
        ReportDistancePatch.Init();

        ReAssignTasks();

        NameText.UpdateAllNameInfo();

        if (GeneralSettingOptions.PetOnlyMe)
        {
            foreach (var player in ExPlayerControl.ExPlayerControls)
            {
                if (player.AmOwner) continue;
                if (player.IsDead() || player.Player == null || player.cosmetics == null) continue;
                player.Player.SetPet("");
            }
        }

        // 情報機器制限の設定を初期化
        if (MapSettingOptions.DeviceOptions)
        {
            DevicesPatch.ClearAndReload();
        }
        foreach (var ability in ExPlayerControl.LocalPlayer.PlayerAbilities)
        {
            if (ability is CustomButtonBase customButtonBase && customButtonBase.IsFirstCooldownTenSeconds)
            {
                customButtonBase.SetCoolTenSeconds();
            }
        }
    }
    private static void ReAssignTasks()
    {
        // ローカルプレイヤーのみがタスクを再割り当てする
        var localPlayer = ExPlayerControl.LocalPlayer;
        if (localPlayer != null && localPlayer.CustomTaskAbility != null && localPlayer.CustomTaskAbility.assignTaskData != null)
        {
            localPlayer.CustomTaskAbility.AssignTasks();
        }
    }
}