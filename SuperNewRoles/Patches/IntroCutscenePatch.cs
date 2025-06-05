using System.Collections;
using System.Linq;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using HarmonyLib;
using SuperNewRoles.CustomCosmetics.CosmeticsPlayer;
using SuperNewRoles.CustomOptions.Categories;
using SuperNewRoles.Events;
using SuperNewRoles.MapCustoms;
using SuperNewRoles.Modules;
using SuperNewRoles.Modules.Events.Bases;
using SuperNewRoles.Roles;
using SuperNewRoles.Roles.Ability;
using SuperNewRoles.Roles.Ability.CustomButton;
using SuperNewRoles.Roles.Neutral;
using UnityEngine;

namespace SuperNewRoles.Patches;

public static class IntroCutscenePatch
{
    [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.CoBegin))]
    class CoBeginPatch
    {
        public static void Postfix(IntroCutscene __instance)
        {
            Logger.Info("AASAAAAAAAAAAAAAAAAAAAAjhygtrfegtyjut5refrgdbAaaa");
            // なんかバグるからとりあえず
            if (ModHelpers.IsHnS())
            {
                __instance.FrontMost.gameObject.SetActive(false);
                __instance.BackgroundBar.gameObject.SetActive(false);
                __instance.Foreground.gameObject.SetActive(false);
                __instance.transform.Find("BackgroundLayer")?.gameObject.SetActive(false);
            }
            PoolablePrefabManager.OnIntroCutsceneDestroy(__instance);
            new LateTask(() =>
            {
                Initialize();
            }, 5f, "Initialize");
        }
    }
    [HarmonyCoroutinePatch(typeof(IntroCutscene), nameof(IntroCutscene.ShowRole))]
    public static class SetUpRoleTextPatch
    {
        private static int last;
        private static byte ToByteIntro(float f)
        {
            f = Mathf.Clamp01(f);
            return (byte)(f * 255);
        }
        public static void Postfix(object __instance) // IEnumerator<object>などの具体的な型ではなくobject型で受け取る
        {
            IntroCutscene introCutscene = HarmonyCoroutinePatchProcessor.GetParentFromCoroutine<IntroCutscene>(__instance);
            if (introCutscene == null) return;

            if (introCutscene.GetInstanceID() == last)
                return;
            last = introCutscene.GetInstanceID();
            ExPlayerControl player = PlayerControl.LocalPlayer;
            RoleId myrole = player.Role;

            if (player.Role is RoleId.BestFalseCharge)
                myrole = RoleId.Crewmate;
            var rolebase = CustomRoleManager.GetRoleById(myrole);
            if (rolebase != null)
            {
                Color roleColor = rolebase.RoleColor;
                introCutscene.YouAreText.color = roleColor;           //あなたのロールは...を役職の色に変更
                introCutscene.RoleText.color = roleColor;             //役職名の色を変更
                introCutscene.RoleBlurbText.color = roleColor;        //イントロの簡易説明の色を変更

                introCutscene.RoleText.text = ModTranslation.GetString(rolebase.Role.ToString());               //役職名を変更

                var randomIntroNum = Random.Range(1, rolebase.IntroNum + 1); // 1からrolebase.IntroNumまでのランダムな数を取得
                introCutscene.RoleBlurbText.text = ModTranslation.GetString($"{rolebase.Role}Intro{randomIntroNum}");     //イントロの簡易説明をランダムに変更
            }

            if (myrole is RoleId.Crewmate or RoleId.Impostor)
            {
                introCutscene.RoleText.text = player.Data.Role.NiceName;
                introCutscene.RoleBlurbText.text = player.Data.Role.Blurb;
                introCutscene.YouAreText.color = player.Data.Role.TeamColor;   //あなたのロールは...を役職の色に変更
                introCutscene.RoleText.color = player.Data.Role.TeamColor;     //役職名の色を変更
                introCutscene.RoleBlurbText.color = player.Data.Role.TeamColor;//イントロの簡易説明の色を変更
            }

            foreach (var modifier in player.ModifierRoleBases)
            {
                var randomIntroNum = Random.Range(1, modifier.IntroNum + 1);
                introCutscene.RoleBlurbText.text += "\n" + ModHelpers.CsWithTranslation(modifier.RoleColor, $"{modifier.ModifierRole}Intro{randomIntroNum}");
            }

            //プレイヤーを作成&位置変更
            introCutscene.ourCrewmate = introCutscene.CreatePlayer(0, 1, PlayerControl.LocalPlayer.Data, false);
            introCutscene.ourCrewmate.gameObject.SetActive(false);
            introCutscene.ourCrewmate.transform.localPosition = new Vector3(0f, -1.05f, -18f);
            introCutscene.ourCrewmate.transform.localScale = new Vector3(1f, 1f, 1f);

            //サウンド再生
            var sound = PlayerControl.LocalPlayer.Data.Role.IntroSound;
            if (rolebase != null)
                sound = RoleManager.Instance.AllRoles.FirstOrDefault(x => x.Role == rolebase.IntroSoundType)?.IntroSound;
            SoundManager.Instance.PlaySound(sound, false, 1);

            //字幕やプレイヤーを再表示する(Prefixで消している)
            introCutscene.ourCrewmate.gameObject.SetActive(true);
            introCutscene.YouAreText.gameObject.SetActive(true);
            introCutscene.RoleText.gameObject.SetActive(true);
            introCutscene.RoleBlurbText.gameObject.SetActive(true);
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
            color = Jackal.Instance.RoleColor;
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
            GameObject moddedCosmetics = CustomCosmeticsLayers.ExistsOrInitialize(player.cosmetics)?.ModdedCosmetics;
            if (moddedCosmetics != null)
            {
                moddedCosmetics.SetActive(false);
                new LateTask(() => moddedCosmetics.SetActive(true), 0.1f);
            }
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
        if (!GameSettingOptions.ImmediateKillCooldown)
        {
            foreach (var ability in ExPlayerControl.LocalPlayer.PlayerAbilities)
            {
                if (ability is CustomButtonBase customButtonBase && customButtonBase.IsFirstCooldownTenSeconds)
                {
                    customButtonBase.SetCoolTenSeconds();
                }
            }
        }
    }
    private static void ReAssignTasks()
    {
        // ローカルプレイヤーのみがタスクを再割り当てする
        CustomTaskAbility customTaskAbility = null;
        foreach (var taskAbility in ExPlayerControl.LocalPlayer.GetAbilities<CustomTaskAbility>())
        {
            customTaskAbility = taskAbility;
            if (taskAbility.assignTaskData != null)
            {
                taskAbility.AssignTasks();
                return;
            }
        }
        customTaskAbility?.AssignTasks();
    }
}