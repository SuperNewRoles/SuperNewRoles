using System;
using System.Collections;
using System.Linq;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using HarmonyLib;
using SuperNewRoles.CustomCosmetics;
using SuperNewRoles.CustomCosmetics.CosmeticsPlayer;
using SuperNewRoles.CustomCosmetics.UI;
using SuperNewRoles.CustomOptions.Categories;
using SuperNewRoles.Events;
using SuperNewRoles.Extensions;
using SuperNewRoles.MapCustoms;
using SuperNewRoles.Modules;
using SuperNewRoles.Modules.Events.Bases;
using SuperNewRoles.Roles;
using SuperNewRoles.Roles.Ability;
using SuperNewRoles.Roles.Ability.CustomButton;
using SuperNewRoles.Roles.Neutral;
using SuperNewRoles.Mode;
using UnityEngine;
using AmongUs.GameOptions;

namespace SuperNewRoles.Patches;

public static class IntroCutscenePatch
{
    [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.CoBegin))]
    class CoBeginPatch
    {
        public static void Postfix(IntroCutscene __instance)
        {
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
            try
            {
                IntroCutscene introCutscene = HarmonyCoroutinePatchProcessor.GetParentFromCoroutine<IntroCutscene>(__instance);
                if (introCutscene == null) return;

                if (introCutscene.GetInstanceID() == last)
                    return;
                last = introCutscene.GetInstanceID();

                // プレイヤーの存在確認
                if (PlayerControl.LocalPlayer == null)
                {
                    Logger.Warning("LocalPlayer is null in IntroCutscene");
                    return;
                }

                ExPlayerControl player = ExPlayerControl.LocalPlayer;
                if (player == null)
                {
                    Logger.Warning("ExPlayerControl is null in IntroCutscene");
                    return;
                }

                RoleId myrole = player.Role;

                // モードのカスタムイントロをチェック
                if (ModeManager.IsModeActive && ModeManager.CurrentMode.HasCustomIntro)
                {
                    SetupModeIntro(introCutscene, ModeManager.CurrentMode, player);
                    return;
                }

                var hideMyRoleAbility = player.GetAbility<HideMyRoleWhenAliveAbility>();
                if (hideMyRoleAbility != null) myrole = hideMyRoleAbility.FalseRoleId(player);

                var rolebase = CustomRoleManager.GetRoleById(myrole);
                if (rolebase != null)
                {
                    Color roleColor = rolebase.RoleColor;
                    introCutscene.YouAreText.color = roleColor;           //あなたのロールは...を役職の色に変更
                    introCutscene.RoleText.color = roleColor;             //役職名の色を変更
                    introCutscene.RoleBlurbText.color = roleColor;        //イントロの簡易説明の色を変更

                    introCutscene.RoleText.text = ModTranslation.GetString(rolebase.Role.ToString());               //役職名を変更

                    var randomIntroNum = UnityEngine.Random.Range(1, rolebase.IntroNum + 1); // 1からrolebase.IntroNumまでのランダムな数を取得
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
                    // 生きている時は役職を自覚できないモディファイアは処理をスキップ
                    if (hideMyRoleAbility != null && hideMyRoleAbility.IsCheckTargetModifierRoleHidden(player, modifier.ModifierRole)) continue;

                    var randomIntroNum = UnityEngine.Random.Range(1, modifier.IntroNum + 1);
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
            catch (Exception ex)
            {
                Logger.Error($"Error in IntroCutscene.SetUpRoleTextPatch: {ex.Message}\n{ex.StackTrace}");
                // エラーが発生してもイントロを続行できるようにする
            }
        }

        /// <summary>
        /// モードのカスタムイントロを設定する
        /// </summary>
        /// <param name="introCutscene">イントロ画面</param>
        /// <param name="mode">モードのインスタンス</param>
        /// <param name="player">プレイヤー</param>
        private static void SetupModeIntro(IntroCutscene introCutscene, IModeBase mode, PlayerControl player)
        {
            var introInfo = mode.GetIntroInfo(player);

            // 色とテキストを設定
            introCutscene.YouAreText.color = introInfo.RoleColor;
            introCutscene.RoleText.color = introInfo.RoleColor;
            introCutscene.RoleBlurbText.color = introInfo.RoleColor;

            introCutscene.RoleText.text = introInfo.RoleTitle;
            introCutscene.RoleBlurbText.text = introInfo.IntroMessage;

            //プレイヤーを作成&位置変更
            introCutscene.ourCrewmate = introCutscene.CreatePlayer(0, 1, player.Data, false);
            introCutscene.ourCrewmate.gameObject.SetActive(false);
            introCutscene.ourCrewmate.transform.localPosition = new Vector3(0f, -1.05f, -18f);
            introCutscene.ourCrewmate.transform.localScale = new Vector3(1f, 1f, 1f);

            // サウンドを設定
            var sound = player.Data.Role.IntroSound;
            if (introInfo.CustomIntroSound != null)
            {
                // カスタムサウンドがある場合はそれを使用
                sound = introInfo.CustomIntroSound;
            }
            else if (introInfo.IntroSoundType.HasValue)
            {
                // 指定された役職タイプのサウンドを使用
                var roleInfo = RoleManager.Instance.AllRoles.FirstOrDefault(x => x.Role == introInfo.IntroSoundType.Value);
                if (roleInfo != null && roleInfo.IntroSound != null)
                {
                    sound = roleInfo.IntroSound;
                }
            }
            SoundManager.Instance.PlaySound(sound, false, 1);

            //字幕やプレイヤーを再表示する
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
                yourTeam.Add(PlayerControl.LocalPlayer);
                foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                {
                    if (p == PlayerControl.LocalPlayer) continue;
                    yourTeam.Add(p);
                }
            }
            else if (ExPlayerControl.LocalPlayer.IsImpostor())
            {
                yourTeam = new();
                yourTeam.Add(PlayerControl.LocalPlayer);
                foreach (ExPlayerControl player in ExPlayerControl.ExPlayerControls)
                {
                    if (player.IsImpostor())
                    {
                        if (player == ExPlayerControl.LocalPlayer) continue;
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

        // モードのカスタムイントロの場合
        if (ModeManager.IsModeActive && ModeManager.CurrentMode.HasCustomIntro)
        {
            var introInfo = ModeManager.CurrentMode.GetIntroInfo(PlayerControl.LocalPlayer);
            TeamTitle = introInfo.RoleTitle;
            ImpostorText = introInfo.RoleSubTitle;
            color = introInfo.RoleColor;

            // チームメイトリストを更新
            yourTeam.Clear();
            foreach (var member in introInfo.TeamMembers)
            {
                yourTeam.Add(member);
            }
        }
        else if (ExPlayerControl.LocalPlayer.IsNeutral())
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
            ImpostorText = ModTranslation.GetString("FriendRolesSubIntro");
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
                moddedCosmetics.SetActive(true);
            }

            // ローカルプレイヤーのHat2/Visor2を確実に設定
            if (player == PlayerControl.LocalPlayer)
            {
                PlayerControlRpcExtensions.RpcCustomSetCosmetics(player.PlayerId, CostumeTabType.Hat2, CustomCosmeticsSaver.CurrentHat2Id, player.Data.DefaultOutfit.ColorId);
                PlayerControlRpcExtensions.RpcCustomSetCosmetics(player.PlayerId, CostumeTabType.Visor2, CustomCosmeticsSaver.CurrentVisor2Id, player.Data.DefaultOutfit.ColorId);
            }
        }
        NameText.RegisterNameTextUpdateEvent();
        SaboAndVent.RegisterListener();
        FinalStatusListener.LoadListener();
        CustomDeathExtensions.Register();
        SetTargetPatch.Register();

        // The Fungle マップ初期化処理を段階的に実行（競合状態を回避）
        new LateTask(() =>
        {
            try
            {
                FungleAdditionalAdmin.AddAdmin();
                FungleAdditionalElectrical.CreateElectrical();
                MushroomMixup.Initialize();
                ZiplineUpdown.Initialize();
            }
            catch (Exception ex)
            {
                Logger.Error($"Error during Fungle map initialization: {ex}");
            }
        }, 0.5f, "FungleMapInit");
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