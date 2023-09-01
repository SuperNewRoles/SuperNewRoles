using System.Collections;
using AmongUs.GameOptions;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using HarmonyLib;
using SuperNewRoles.Buttons;
using SuperNewRoles.Mode;
using SuperNewRoles.Roles;
using SuperNewRoles.Roles.Neutral;
using UnityEngine;

namespace SuperNewRoles.Patches;

[HarmonyPatch(typeof(Constants), nameof(Constants.ShouldHorseAround))]
public static class ShouldAlwaysHorseAround
{
    public static bool isHorseMode;
    public static void Postfix(ref bool __result)
    {
        if (__result) return;

        if (isHorseMode != HorseModeOption.enableHorseMode && LobbyBehaviour.Instance != null) __result = isHorseMode;
        else
        {
            __result = HorseModeOption.enableHorseMode;
            isHorseMode = HorseModeOption.enableHorseMode;
        }
        return;
    }
}
[HarmonyPatch]
public class IntroPatch
{
    [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.CoBegin))]
    class IntroCutsceneCoBeginPatch
    {
        static void Postfix()
        {
            Logger.Info("=================Player Info=================", "Intro Begin");
            Logger.Info("=================Player Data=================", "Player Info");
            {
                Logger.Info($"プレイヤー数：{CachedPlayer.AllPlayers.Count}人", "All Player Count");
                foreach (PlayerControl p in CachedPlayer.AllPlayers)
                { Logger.Info($"{(p.AmOwner ? "[H]" : "[ ]")}{(p.IsMod() ? "[M]" : "[ ]")}{p.name}(cid:{p.GetClientId()})(pid:{p.PlayerId})({p.GetClient()?.PlatformData?.Platform}){(p.IsBot() ? "(BOT)" : "")}", "Player info"); }
            }
            Logger.Info("=================Role Data=================", "Player Info");
            foreach (PlayerControl p in CachedPlayer.AllPlayers)
            {
                Logger.Info($"{p.name}=>{p.GetRole()}({p.GetRoleType()}){(p.IsLovers() ? "[♥]" : "")}{(p.IsQuarreled() ? "[○]" : "")}", "Role Data");
            }
            Logger.Info("=================Other Data=================", "Intro Begin");
            {
                Logger.Info($"MapId:{GameManager.Instance.LogicOptions.currentGameOptions.MapId} MapNames:{(MapNames)GameManager.Instance.LogicOptions.currentGameOptions.MapId}", "Other Data");
                Logger.Info($"Mode:{ModeHandler.GetMode()}", "Other Data");
                foreach (IntroData data in IntroData.Intros.Values) { data._titleDesc = IntroData.GetTitle(data.NameKey, data.TitleNum); }
            }
            Logger.Info("=================Activate Roles Data=================", " Other Data");
            {
                Logger.Info($"インポスター役職 : 最大 {CustomOptionHolder.impostorRolesCountMax.GetSelection()}役職", "ImpostorRole");
                Logger.Info($"クルーメイト役職 : 最大 {CustomOptionHolder.crewmateRolesCountMax.GetSelection()}役職", "CremateRole");
                Logger.Info($"第三陣営役職 : 最大 {CustomOptionHolder.neutralRolesCountMax.GetSelection()}役職", "NeutralRole");
                CustomOverlays.GetActivateRoles(true); // 現在の役職設定を取得し、辞書に保存するついでにlogに記載する
            }
        }
    }
    [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.OnDestroy))]
    class IntroCutsceneOnDestroyPatch
    {
        public static PoolablePlayer playerPrefab;
        public static void Prefix(IntroCutscene __instance)
        {
            if (ModeHandler.IsMode(ModeId.HideAndSeek))
            {
                new LateTask(() => RoleClass.IsFirstMeetingEnd = true, 6);
            }
            // プレイヤーのアイコンを生成
            if (PlayerControl.LocalPlayer != null && FastDestroyableSingleton<HudManager>.Instance != null)
            {
                Vector3 bottomLeft = new(-FastDestroyableSingleton<HudManager>.Instance.UseButton.transform.localPosition.x, FastDestroyableSingleton<HudManager>.Instance.UseButton.transform.localPosition.y, FastDestroyableSingleton<HudManager>.Instance.UseButton.transform.localPosition.z);

                int index = -1;
                foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                {
                    GameData.PlayerInfo data = p.Data;
                    Logger.Info($"生成:{p.Data.PlayerName}");
                    PoolablePlayer player = Object.Instantiate(__instance.PlayerPrefab, FastDestroyableSingleton<HudManager>.Instance.transform);
                    playerPrefab = __instance.PlayerPrefab;
                    p.SetPlayerMaterialColors(player.cosmetics.currentBodySprite.BodySprite);
                    player.SetSkin(data.DefaultOutfit.SkinId, data.DefaultOutfit.ColorId);
                    player.cosmetics.SetHat(data.DefaultOutfit.HatId, data.DefaultOutfit.ColorId);
                    // PlayerControl.SetPetImage(data.DefaultOutfit.PetId, data.DefaultOutfit.ColorId, player.PetSlot);
                    player.cosmetics.nameText.text = data.PlayerName;
                    player.SetFlipX(true);
                    MapOption.MapOption.playerIcons[p.PlayerId] = player;
                    if (CachedPlayer.LocalPlayer.PlayerControl.IsRole(RoleId.Hitman))
                    {
                        player.transform.localPosition = bottomLeft + new Vector3(-0.25f, 0f, 0);
                        player.transform.localScale = Vector3.one * 0.4f;
                        player.gameObject.SetActive(false);
                    }
                    else if (PlayerControl.LocalPlayer.IsRole(RoleId.GM))
                    {
                        player.gameObject.SetActive(false);
                        if (p.PlayerId == CachedPlayer.LocalPlayer.PlayerId) continue;
                        index++;
                        player.transform.localPosition = new(-4.75f + (index * 0.425f), -2.4f, 0);
                        player.transform.localScale = new(0.25f, 0.25f, 0.25f);
                        player.cosmetics.nameText.transform.localPosition -= new Vector3(0, 0.3f, 0);
                        PassiveButton button = player.gameObject.AddComponent<PassiveButton>();
                        button.OnMouseOut = new();
                        button.OnMouseOver = new();
                        button.OnClick = new();
                        static void Create(PassiveButton button, PlayerControl target)
                        {
                            button.OnClick.AddListener((UnityEngine.Events.UnityAction)(() =>
                            {
                                Roles.Neutral.GM.target = target;
                                FastDestroyableSingleton<RoleManager>.Instance.SetRole(PlayerControl.LocalPlayer, RoleTypes.Shapeshifter);
                                foreach (CachedPlayer p in CachedPlayer.AllPlayers)
                                {
                                    p.Data.Role.NameColor = Color.white;
                                }
                                CachedPlayer.LocalPlayer.Data.IsDead = false;
                                CachedPlayer.LocalPlayer.Data.Role.TryCast<ShapeshifterRole>().UseAbility();
                                CachedPlayer.LocalPlayer.Data.IsDead = true;
                                FastDestroyableSingleton<RoleManager>.Instance.SetRole(PlayerControl.LocalPlayer, RoleTypes.CrewmateGhost);
                                foreach (CachedPlayer p in CachedPlayer.AllPlayers)
                                {
                                    if (p.PlayerControl.IsImpostor())
                                    {
                                        p.Data.Role.NameColor = RoleClass.ImpostorRed;
                                    }
                                }
                            }));
                        }
                        Create(button, p);
                        button.Colliders = new Collider2D[] { player.gameObject.AddComponent<PolygonCollider2D>() };
                        button._CachedZ_k__BackingField = 0.1f;
                        button.CachedZ = 0.1f;
                        player.gameObject.SetActive(true);
                    }
                    else
                    {
                        player.gameObject.SetActive(false);
                    }
                    Logger.Info($"生成完了:{p.Data.PlayerName}");
                }
            }

            if (CachedPlayer.LocalPlayer.PlayerControl.IsRole(RoleId.Hitman))
            {
                RoleClass.Hitman.UpdateTime = CustomOptionHolder.HitmanChangeTargetTime.GetFloat();
                Roles.Neutral.Hitman.SetTarget();
                Roles.Neutral.Hitman.DestroyIntroHandle(__instance);
                if (FastDestroyableSingleton<HudManager>.Instance != null)
                {
                    Vector3 bottomLeft = new Vector3(-FastDestroyableSingleton<HudManager>.Instance.UseButton.transform.localPosition.x, FastDestroyableSingleton<HudManager>.Instance.UseButton.transform.localPosition.y, FastDestroyableSingleton<HudManager>.Instance.UseButton.transform.localPosition.z) + new Vector3(-0.25f, 1f, 0);
                    RoleClass.Hitman.cooldownText = Object.Instantiate(FastDestroyableSingleton<HudManager>.Instance.KillButton.cooldownTimerText, FastDestroyableSingleton<HudManager>.Instance.transform);
                    RoleClass.Hitman.cooldownText.alignment = TMPro.TextAlignmentOptions.Center;
                    RoleClass.Hitman.cooldownText.transform.localPosition = bottomLeft + new Vector3(0f, -1f, -1f);
                    RoleClass.Hitman.cooldownText.gameObject.SetActive(true);
                }
            }
        }
    }

    public static void SetupIntroTeamIcons(IntroCutscene __instance, ref Il2CppSystem.Collections.Generic.List<PlayerControl> yourTeam)
    {
        if (ModeHandler.IsMode(ModeId.Default))
        {
            var newTeam2 = new Il2CppSystem.Collections.Generic.List<PlayerControl>();
            newTeam2.Add(PlayerControl.LocalPlayer);
            yourTeam = newTeam2;
            if (PlayerControl.LocalPlayer.IsCrew())
            {
                var newTeam = new Il2CppSystem.Collections.Generic.List<PlayerControl>();
                newTeam.Add(PlayerControl.LocalPlayer);
                foreach (PlayerControl p in CachedPlayer.AllPlayers)
                {
                    if (p.PlayerId != CachedPlayer.LocalPlayer.PlayerId)
                    {
                        newTeam.Add(p);
                    }
                }
                yourTeam = newTeam;
            }
            else
            {
                switch (PlayerControl.LocalPlayer.GetRole())
                {
                    case RoleId.Madmate:
                    case RoleId.MadMayor:
                    case RoleId.MadJester:
                    case RoleId.MadSeer:
                    case RoleId.Worshiper:
                    case RoleId.BlackCat:
                        if (Madmate.CheckImpostor(PlayerControl.LocalPlayer)) break;
                        ImpostorIntroTeam:
                        Il2CppSystem.Collections.Generic.List<PlayerControl> ImpostorTeams = new();
                        ImpostorTeams.Add(PlayerControl.LocalPlayer);
                        foreach (PlayerControl player in CachedPlayer.AllPlayers)
                        {
                            if ((player.IsImpostor() || player.IsRole(RoleId.Spy, RoleId.Egoist)) && player.PlayerId != CachedPlayer.LocalPlayer.PlayerId)
                            {
                                ImpostorTeams.Add(player);
                            }
                            if (player.IsRole(RoleId.Egoist))
                            {
                                player.Data.Role.NameColor = Color.red;
                            }
                        }
                        yourTeam = ImpostorTeams;
                        break;
                    case RoleId.SeerFriends:
                    case RoleId.MayorFriends:
                    case RoleId.JackalFriends:
                        if (JackalFriends.CheckJackal(PlayerControl.LocalPlayer)) break;
                        JackalIntroTeam:
                        Il2CppSystem.Collections.Generic.List<PlayerControl> JackalTeams = new();
                        JackalTeams.Add(PlayerControl.LocalPlayer);
                        foreach (PlayerControl player in CachedPlayer.AllPlayers)
                        {
                            if (player.IsJackalTeamJackal() && player.PlayerId != CachedPlayer.LocalPlayer.PlayerId)
                            {
                                JackalTeams.Add(player);
                            }
                        }
                        yourTeam = JackalTeams;
                        break;
                    case RoleId.Fox:
                        Il2CppSystem.Collections.Generic.List<PlayerControl> FoxTeams = new();
                        int FoxNum = 0;
                        foreach (PlayerControl player in CachedPlayer.AllPlayers)
                        {
                            if (player.IsRole(RoleId.Fox) || (player.IsRole(RoleId.FireFox) && FireFox.FireFoxIsCheckFox.GetBool()))
                            {
                                FoxNum++;
                                FoxTeams.Add(player);
                            }
                        }
                        yourTeam = FoxTeams;
                        break;
                    case RoleId.FireFox:
                        Il2CppSystem.Collections.Generic.List<PlayerControl> FireFoxTeams = new();
                        int FireFoxNum = 0;
                        foreach (PlayerControl player in CachedPlayer.AllPlayers)
                        {
                            if (player.IsRole(RoleId.FireFox) || (player.IsRole(RoleId.Fox) && FireFox.FireFoxIsCheckFox.GetBool()))
                            {
                                FireFoxNum++;
                                FireFoxTeams.Add(player);
                            }
                        }
                        yourTeam = FireFoxTeams;
                        break;
                    case RoleId.TheFirstLittlePig:
                    case RoleId.TheSecondLittlePig:
                    case RoleId.TheThirdLittlePig:
                        Il2CppSystem.Collections.Generic.List<PlayerControl> TheThreeLittlePigsTeams = new();
                        int TheThreeLittlePigsNum = 0;
                        foreach (var players in TheThreeLittlePigs.TheThreeLittlePigsPlayer)
                        {
                            if (players.TrueForAll(x => x.PlayerId != PlayerControl.LocalPlayer.PlayerId)) continue;
                            foreach (PlayerControl player in players)
                            {
                                TheThreeLittlePigsNum++;
                                TheThreeLittlePigsTeams.Add(player);
                            }
                            break;
                        }
                        yourTeam = TheThreeLittlePigsTeams;
                        break;
                    default:
                        if (PlayerControl.LocalPlayer.IsImpostor())
                        {
                            goto ImpostorIntroTeam;
                        }
                        else if (PlayerControl.LocalPlayer.IsJackalTeamJackal())
                        {
                            goto JackalIntroTeam;
                        }
                        break;
                }
            }
        }
        else
        {
            var temp = ModeHandler.TeamHandler(__instance);
            if (temp != new Il2CppSystem.Collections.Generic.List<PlayerControl>())
            {
                yourTeam = temp;
            }
            else
            {
                temp = new();
                temp.Add(PlayerControl.LocalPlayer);
                yourTeam = temp;
            }
        }
        if (PlayerControl.LocalPlayer.IsImpostor())
        {
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            {
                if (player.IsImpostorAddedFake())
                    player.Data.Role.NameColor = Color.red;
            }
        }
    }

    public static void SetupIntroTeam(IntroCutscene __instance, ref Il2CppSystem.Collections.Generic.List<PlayerControl> yourTeam)
    {
        IntroHandler.Handler();

        Color32 color = __instance.TeamTitle.color;
        Color32 backcolor = __instance.BackgroundBar.material.color;
        string TeamTitle = __instance.TeamTitle.text;
        string ImpostorText = __instance.ImpostorText.text;
        if (ModeHandler.IsMode(ModeId.Default, ModeId.SuperHostRoles))
        {
            if (PlayerControl.LocalPlayer.IsNeutral() && !PlayerControl.LocalPlayer.IsRole(RoleId.GM))
            {
                IntroData Intro = IntroData.GetIntroData(PlayerControl.LocalPlayer.GetRole(), PlayerControl.LocalPlayer);
                TeamTitle = ModTranslation.GetString("Neutral");
                ImpostorText = ModTranslation.GetString("NeutralSubIntro");
                color = new(127, 127, 127, byte.MaxValue);
            }
            else if (PlayerControl.LocalPlayer.IsMadRoles())
            {
                color = RoleClass.ImpostorRed;
                TeamTitle = ModTranslation.GetString("MadmateName");
                ImpostorText = ModTranslation.GetString("MadRolesSubIntro");
            }
            else if (PlayerControl.LocalPlayer.IsFriendRoles())
            {
                color = RoleClass.JackalBlue;
                TeamTitle = ModTranslation.GetString("JackalFriendsName");
                ImpostorText = ModTranslation.GetString("FriendRolesSubIntro");
            }
            else
            {
                switch (PlayerControl.LocalPlayer.GetRole())
                {
                    case RoleId.SatsumaAndImo:
                    case RoleId.GM:
                        IntroData Intro = IntroData.GetIntroData(PlayerControl.LocalPlayer.GetRole(), PlayerControl.LocalPlayer);
                        color = Intro.color;
                        TeamTitle = ModTranslation.GetString(Intro.NameKey + "Name");
                        ImpostorText = "";
                        break;
                }
                foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                {
                    if (player.IsRole(RoleId.Egoist))
                    {
                        player.Data.Role.NameColor = Color.white;
                    }
                }
            }
        }
        else
        {
            (TeamTitle, ImpostorText, color) = ModeHandler.IntroHandler(__instance);
            if (TeamTitle == "NONE" && ImpostorText == "NONE") return;
        }
        if (OldModeButtons.IsOldMode)
        {
            if (ModeHandler.IsMode(ModeId.Default))
            {
                var myrole = PlayerControl.LocalPlayer.GetRole();
                if (myrole is not (RoleId.DefaultRole or RoleId.Bestfalsecharge))
                {
                    var data = IntroData.GetIntroData(myrole, PlayerControl.LocalPlayer);
                    color = data.color;
                    TeamTitle = ModTranslation.GetString(data.NameKey + "Name");
                    ImpostorText = data.TitleDesc;
                }
                if (PlayerControl.LocalPlayer.IsLovers())
                {
                    ImpostorText += "\n" + ModHelpers.Cs(RoleClass.Lovers.color, string.Format(ModTranslation.GetString("LoversIntro"), PlayerControl.LocalPlayer.GetOneSideLovers()?.Data?.PlayerName ?? ""));
                }
                if (PlayerControl.LocalPlayer.IsQuarreled())
                {
                    ImpostorText += "\n" + ModHelpers.Cs(RoleClass.Quarreled.color, string.Format(ModTranslation.GetString("QuarreledIntro"), PlayerControl.LocalPlayer.GetOneSideQuarreled()?.Data?.PlayerName ?? ""));
                }
            }
            __instance.ImpostorText.gameObject.SetActive(true);
            if (ImpostorText.Length >= 10)
            {
                __instance.ImpostorText.transform.localPosition += new Vector3(0, 0.5f, 0);
                __instance.ImpostorText.transform.localScale *= 1.5f;
            }
        }
        __instance.TeamTitle.text = TeamTitle;
        __instance.ImpostorText.text = ImpostorText;
        if (ImpostorText != "")
            __instance.ImpostorText.gameObject.SetActive(true);
        __instance.BackgroundBar.material.color = color;
        __instance.TeamTitle.color = color;

        //SetUpRoleTextPatch.Postfix(__instance);
    }

    [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.ShowTeam))]
    class ShowTeam
    {
        public static void Postfix()
        {
            if (PlayerControl.LocalPlayer.IsRole(RoleId.SeeThroughPerson)) Roles.Crewmate.SeeThroughPerson.AwakePatch();
        }
    }
    [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.OnDestroy))]
    class IntroCutsceneDestroyPatch
    {
        public static void Prefix()
        {
            float SetTime = 0;
            bool Flag = true;
            switch (PlayerControl.LocalPlayer.GetRole())
            {
                case RoleId.DarkKiller:
                    SetTime = RoleClass.DarkKiller.KillCoolTime;
                    break;
                case RoleId.Minimalist:
                    SetTime = RoleClass.Minimalist.KillCoolTime;
                    break;
                case RoleId.Samurai:
                    SetTime = RoleClass.Samurai.KillCoolTime;
                    break;
                case RoleId.HomeSecurityGuard:
                    foreach (PlayerTask task in PlayerControl.LocalPlayer.myTasks)
                    {
                        task.Complete();
                    }
                    Flag = false;
                    break;
                default:
                    Flag = false;
                    break;
            }
            if (Flag)
            {
                PlayerControl.LocalPlayer.SetKillTimerUnchecked(SetTime);
            }
            PlayerControlHelper.RefreshRoleDescription(PlayerControl.LocalPlayer);
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
        static IEnumerator settask()
        {
            while (true)
            {
                if (PlayerControl.LocalPlayer == null) yield break;
                if (PlayerControl.LocalPlayer.myTasks.Count == (GameManager.Instance.LogicOptions.currentGameOptions.GetInt(Int32OptionNames.NumCommonTasks) + GameManager.Instance.LogicOptions.currentGameOptions.GetInt(Int32OptionNames.NumShortTasks) + GameManager.Instance.LogicOptions.currentGameOptions.GetInt(Int32OptionNames.NumLongTasks))) yield break;

                yield return null;
            }
        }
        static bool Prefix(IntroCutscene __instance, ref Il2CppSystem.Collections.IEnumerator __result)
        {
            __result = SetupRole(__instance).WrapToIl2Cpp();
            return false;
        }

        private static IEnumerator SetupRole(IntroCutscene __instance)
        {
            CustomButton.MeetingEndedUpdate();
            if (OldModeButtons.IsOldMode) yield break;
            new LateTask(() =>
            {
                var player = PlayerControl.LocalPlayer;
                var myrole = PlayerControl.LocalPlayer.GetRole();
                var data = IntroData.GetIntroData(myrole);

                __instance.YouAreText.color = data.color;           //あなたのロールは...を役職の色に変更
                __instance.RoleText.color = data.color;             //役職名の色を変更
                __instance.RoleBlurbText.color = data.color;        //イントロの簡易説明の色を変更

                if (myrole is RoleId.Bestfalsecharge)
                {
                    data = IntroData.CrewmateIntro;
                    __instance.YouAreText.color = Palette.CrewmateBlue;     //あなたのロールは...を役職の色に変更
                    __instance.RoleText.color = Palette.CrewmateBlue;       //役職名の色を変更
                    __instance.RoleBlurbText.color = Palette.CrewmateBlue;  //イントロの簡易説明の色を変更
                }

                __instance.RoleText.text = data.Name;               //役職名を変更
                __instance.RoleBlurbText.text = data.TitleDesc;     //イントロの簡易説明を変更

                if (myrole is RoleId.DefaultRole)
                {
                    __instance.RoleText.text = player.Data.Role.NiceName;
                    __instance.RoleBlurbText.text = player.Data.Role.Blurb;
                    __instance.YouAreText.color = player.Data.Role.TeamColor;   //あなたのロールは...を役職の色に変更
                    __instance.RoleText.color = player.Data.Role.TeamColor;     //役職名の色を変更
                    __instance.RoleBlurbText.color = player.Data.Role.TeamColor;//イントロの簡易説明の色を変更
                }

                //重複を持っていたらメッセージ追記
                if (PlayerControl.LocalPlayer.IsLovers()) __instance.RoleBlurbText.text += "\n" + ModHelpers.Cs(RoleClass.Lovers.color, string.Format(ModTranslation.GetString("LoversIntro"), PlayerControl.LocalPlayer.GetOneSideLovers()?.Data?.PlayerName ?? ""));
                if (PlayerControl.LocalPlayer.IsQuarreled()) __instance.RoleBlurbText.text += "\n" + ModHelpers.Cs(RoleClass.Quarreled.color, string.Format(ModTranslation.GetString("QuarreledIntro"), PlayerControl.LocalPlayer.GetOneSideQuarreled()?.Data?.PlayerName ?? ""));

                ModeHandler.YouAreIntroHandler(__instance);

                //プレイヤーを作成&位置変更
                __instance.ourCrewmate = __instance.CreatePlayer(0, 1, PlayerControl.LocalPlayer.Data, false);
                __instance.ourCrewmate.gameObject.SetActive(false);
                __instance.ourCrewmate.transform.localPosition = new Vector3(0f, -1.05f, -18f);
                __instance.ourCrewmate.transform.localScale = new Vector3(1f, 1f, 1f);

                //サウンド再生
                IntroData.PlayIntroSound(myrole);

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
}