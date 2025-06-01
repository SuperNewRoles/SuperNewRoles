using System;
using UnityEngine;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability.CustomButton;
using UnityEngine.Events;
using System.Linq;
using System.Collections.Generic;

namespace SuperNewRoles.Roles.Ability;

public class GuesserAbility : CustomMeetingButtonBase, IAbilityCount
{
    private readonly int shotsPerMeeting;
    private readonly bool cannotShootCrewmate;
    private readonly bool cannotShootCelebrity;
    private readonly bool CelebrityLimitedTurns;
    private readonly int CelebrityLimitedTurnsCount;
    private readonly int maxShots;
    private readonly bool madmateSuicide;
    // ※ 画面上に既にUIが存在しているか確認するためのフィールド
    private GameObject guesserUI;
    private bool HideButtons = false;
    public override bool HasButtonLocalPlayer => false;
    private int ShotThisMeeting;
    private int MeetingCount = -1;
    private TMPro.TextMeshPro limitText;
    public override Sprite Sprite => AssetManager.GetAsset<Sprite>("TargetIcon.png");

    public GuesserAbility(int maxShots, int shotsPerMeeting, bool cannotShootCrewmate, bool cannotShootCelebrity, bool celebrityLimitedTurns = false, int celebrityLimitedTurnsCount = 3, bool madmateSuicide = false)
    {
        this.maxShots = maxShots;
        this.shotsPerMeeting = shotsPerMeeting;
        this.cannotShootCrewmate = cannotShootCrewmate;
        this.cannotShootCelebrity = cannotShootCelebrity;
        this.CelebrityLimitedTurns = celebrityLimitedTurns;
        this.CelebrityLimitedTurnsCount = celebrityLimitedTurnsCount;
        this.madmateSuicide = madmateSuicide;
        Count = maxShots;
    }

    public override bool CheckHasButton(ExPlayerControl player)
    {
        // スターを撃てない設定がONで、撃てないターンを制限する設定がONの場合、
        // 指定されたターン数まではスターを撃てないようにする
        if (cannotShootCelebrity && CelebrityLimitedTurns && MeetingCount < CelebrityLimitedTurnsCount && player.Role == RoleId.Celebrity)
        {
            return false;
        }
        return ExPlayerControl.LocalPlayer.IsAlive() && !HideButtons && HasCount && player.IsAlive() && ShotThisMeeting < shotsPerMeeting;
    }

    public override bool CheckIsAvailable(ExPlayerControl player)
    {
        return player.IsAlive();
    }

    public override void OnMeetingStart()
    {
        HideButtons = false;
        if (guesserUI != null)
            GameObject.Destroy(guesserUI);
        guesserUI = null;
        ShotThisMeeting = 0;
        MeetingCount++;

        // 残り使用回数を表示するテキストを作成
        if (Player.IsDead()) return;
        if (limitText != null)
            GameObject.Destroy(limitText.gameObject);
        limitText = GameObject.Instantiate(FastDestroyableSingleton<HudManager>.Instance.KillButton.cooldownTimerText, MeetingHud.Instance.transform);
        limitText.text = ModTranslation.GetString("GuesserLimitText", Count, Math.Min(maxShots, shotsPerMeeting) - ShotThisMeeting);
        limitText.enableWordWrapping = false;
        limitText.transform.localScale = Vector3.one * 0.5f;
        limitText.transform.localPosition = new Vector3(-3.58f, 2.27f, -10);
        limitText.gameObject.SetActive(true);
        limitText.alignment = TMPro.TextAlignmentOptions.Left;
    }
    public override void OnMeetingUpdate()
    {
        if (ExPlayerControl.LocalPlayer.IsDead())
        {
            HideButtons = false;
            if (guesserUI != null)
                GameObject.Destroy(guesserUI);
            guesserUI = null;

            // プレイヤーが死亡した場合、テキストも削除
            if (limitText != null)
                GameObject.Destroy(limitText.gameObject);
            limitText = null;
        }
        base.OnMeetingUpdate();
    }
    private static void SetActiveAllPlayerVoteArea(bool active)
    {
        foreach (var playerVoteArea in MeetingHud.Instance.playerStates)
        {
            if (playerVoteArea.gameObject.activeSelf != active)
                playerVoteArea.gameObject.SetActive(active);
        }
    }
    public override void OnClick(ExPlayerControl exPlayer, GameObject button)
    {
        // ミーティングHUDの取得と状態チェック（仮想例）
        MeetingHud meetingHud = MeetingHud.Instance;
        if (meetingHud == null) return;
        if (guesserUI != null || !(meetingHud.state == MeetingHud.VoteStates.Voted ||
                                    meetingHud.state == MeetingHud.VoteStates.NotVoted ||
                                    meetingHud.state == MeetingHud.VoteStates.Discussion))
            return;

        if (exPlayer.IsDead()) return;

        // スターを撃てない設定がONで、撃てないターンを制限する設定がONの場合、
        // 指定されたターン数まではスターを撃てないようにする
        if (cannotShootCelebrity && CelebrityLimitedTurns && MeetingCount < CelebrityLimitedTurnsCount && exPlayer.Role == RoleId.Celebrity)
        {
            return;
        }

        // UI生成に必要なローカル変数群
        int Page = 1;
        int currentTeamType = (int)AssignedTeamType.Crewmate;
        const int MaxOneScreenRole = 10; // 1画面に表示する役割ボタンの最大数（仮定）

        var roleButtons = new System.Collections.Generic.Dictionary<AssignedTeamType, System.Collections.Generic.List<Transform>>();
        var roleSelectButtons = new System.Collections.Generic.Dictionary<AssignedTeamType, SpriteRenderer>();
        var pageButtons = new System.Collections.Generic.List<SpriteRenderer>();
        System.Collections.Generic.List<Transform> buttons = new();
        Transform selectedButton = null;
        System.Collections.Generic.List<int> teamButtonCount = new() { 0, 0, 0 };

        // ミーティングHUD上のプレイヤー状態UIを非表示にする
        HideButtons = true;
        SetActiveAllPlayerVoteArea(false);

        // UIコンテナの生成（PhoneUIプレハブを元に）
        Transform prototype = meetingHud.transform.Find("MeetingContents/PhoneUI");
        Transform container = UnityEngine.Object.Instantiate(prototype, meetingHud.transform);
        container.localPosition = new Vector3(0, 0, -200f);
        guesserUI = container.gameObject;

        // テンプレートの取得
        var buttonTemplate = meetingHud.playerStates[0].transform.Find("votePlayerBase");
        var maskTemplate = meetingHud.playerStates[0].transform.Find("MaskArea");
        var smallButtonTemplate = meetingHud.playerStates[0].Buttons.transform.Find("CancelButton");
        var textTemplate = meetingHud.playerStates[0].NameText;

        // --- Exitボタンの生成 ---
        Transform exitButtonParent = new GameObject().transform;
        exitButtonParent.SetParent(container);
        Transform exitButton = UnityEngine.Object.Instantiate(buttonTemplate, exitButtonParent);
        exitButton.Find("ControllerHighlight").gameObject.SetActive(false);
        Transform exitButtonMask = UnityEngine.Object.Instantiate(maskTemplate, exitButtonParent);
        exitButton.gameObject.GetComponent<SpriteRenderer>().sprite = smallButtonTemplate.GetComponent<SpriteRenderer>().sprite;
        exitButtonParent.localPosition = new Vector3(2.725f, 2.1f, -200f);
        exitButtonParent.localScale = new Vector3(0.25f, 0.9f, 1f);
        exitButtonParent.SetAsFirstSibling();
        var exitPassive = exitButton.GetComponent<PassiveButton>();
        exitPassive.OnClick = new();
        exitPassive.ClickSound = null;
        exitPassive.OnClick.AddListener((UnityAction)(() =>
        {
            HideButtons = false;
            SetActiveAllPlayerVoteArea(true);
            UnityEngine.Object.Destroy(container.gameObject);
        }));
        var ExitButton = exitPassive;

        // --- チーム選択ボタンの生成 ---
        for (int index = 0; index < 3; index++)
        {
            Transform teamButtonParent = new GameObject().transform;
            teamButtonParent.SetParent(container);
            Transform teamButton = UnityEngine.Object.Instantiate(buttonTemplate, teamButtonParent);
            teamButton.Find("ControllerHighlight").gameObject.SetActive(false);
            Transform teamButtonMask = UnityEngine.Object.Instantiate(maskTemplate, teamButtonParent);
            var teamLabel = UnityEngine.Object.Instantiate(textTemplate, teamButton);
            roleSelectButtons.Add((AssignedTeamType)index, teamButton.GetComponent<SpriteRenderer>());
            teamButtonParent.localPosition = new Vector3(-2.75f + (index * 1.75f), 2.225f, -200);
            teamButtonParent.localScale = new Vector3(0.55f, 0.55f, 1f);
            // チームに応じたラベル色の設定
            if ((AssignedTeamType)index == AssignedTeamType.Crewmate)
                teamLabel.color = Color.white;
            else if ((AssignedTeamType)index == AssignedTeamType.Impostor)
                teamLabel.color = Palette.ImpostorRed;
            else
                teamLabel.color = new Color32(127, 127, 127, 255);
            Logger.Info(teamLabel.color.ToString(), ((AssignedTeamType)index).ToString());
            string key = ((AssignedTeamType)index).ToString();
            teamLabel.text = ModTranslation.GetString(key);
            teamLabel.alignment = TMPro.TextAlignmentOptions.Center;
            teamLabel.transform.localPosition = new Vector3(0, 0, teamLabel.transform.localPosition.z);
            teamLabel.transform.localScale *= 1.6f;
            teamLabel.autoSizeTextContainer = true;

            // チームボタンタップ時の処理
            void CreateTeamButton(Transform tb, AssignedTeamType type)
            {
                var passiveButton = tb.GetComponent<PassiveButton>();
                passiveButton.OnClick = new();
                passiveButton.ClickSound = null;
                passiveButton.OnClick.AddListener((UnityAction)(() =>
                {
                    guesserSelectRole(type);
                    ReloadPage();
                }));
            }
            if (ExPlayerControl.LocalPlayer.IsAlive())
                CreateTeamButton(teamButton, (AssignedTeamType)index);
        }

        // --- ローカル関数: guesserSelectRole ---
        void guesserSelectRole(AssignedTeamType team, bool resetSelection = true)
        {
            currentTeamType = (int)team;
            if (resetSelection) Page = 1;
            foreach (var RoleButton in roleButtons)
            {
                int index = 0;
                foreach (var RoleBtn in RoleButton.Value)
                {
                    if (RoleBtn == null) continue;
                    index++;
                    if (index <= (Page - 1) * 40) { RoleBtn.gameObject.SetActive(false); continue; }
                    if ((Page * 40) < index) { RoleBtn.gameObject.SetActive(false); continue; }
                    RoleBtn.gameObject.SetActive(RoleButton.Key == team);
                }
            }
            foreach (var RoleButton in roleSelectButtons)
            {
                if (RoleButton.Value == null) continue;
                RoleButton.Value.color = new(0, 0, 0, RoleButton.Key == team ? 1 : 0.25f);
            }
        }

        // --- ローカル関数: ReloadPage ---
        void ReloadPage()
        {
            pageButtons[0].color = new Color(1, 1, 1, 1f);
            pageButtons[1].color = new Color(1, 1, 1, 1f);
            int totalPages = roleButtons.ContainsKey((AssignedTeamType)currentTeamType) ?
                (roleButtons[(AssignedTeamType)currentTeamType].Count / MaxOneScreenRole +
                 (roleButtons[(AssignedTeamType)currentTeamType].Count % MaxOneScreenRole != 0 ? 1 : 0)) : 0;
            if (totalPages < Page)
            {
                Page -= 1;
                pageButtons[1].color = new Color(1, 1, 1, 0.1f);
            }
            else if (totalPages < Page + 1)
            {
                pageButtons[1].color = new Color(1, 1, 1, 0.1f);
            }
            if (Page <= 1)
            {
                Page = 1;
                pageButtons[0].color = new Color(1, 1, 1, 0.1f);
            }
            guesserSelectRole((AssignedTeamType)currentTeamType, false);
            Logger.Info("Page:" + Page);
        }

        // --- ローカル関数: CreatePage ---
        void CreatePage(bool isNext, MeetingHud hud, Transform cont)
        {
            var btnTemplate = hud.playerStates[0].transform.Find("votePlayerBase");
            var maskTemplateLocal = hud.playerStates[0].transform.Find("MaskArea");
            var smallBtnTemplate = hud.playerStates[0].Buttons.transform.Find("CancelButton");
            var textTemplateLocal = hud.playerStates[0].NameText;
            Transform pageButtonParent = new GameObject().transform;
            pageButtonParent.SetParent(cont);
            Transform pageButton = UnityEngine.Object.Instantiate(btnTemplate, pageButtonParent);
            pageButton.Find("ControllerHighlight").gameObject.SetActive(false);
            Transform pageButtonMask = UnityEngine.Object.Instantiate(maskTemplateLocal, pageButtonParent);
            var pageLabel = UnityEngine.Object.Instantiate(textTemplateLocal, pageButton);
            pageButtonParent.localPosition = isNext ? new Vector3(3.535f, -2.2f, -200) : new Vector3(-3.475f, -2.2f, -200);
            pageButtonParent.localScale = new Vector3(0.55f, 0.55f, 1f);
            pageLabel.color = Color.white;
            pageLabel.text = ModTranslation.GetString(isNext ? "NextPage" : "PreviousPage");
            pageLabel.alignment = TMPro.TextAlignmentOptions.Center;
            pageLabel.transform.localPosition = new Vector3(0, 0, pageLabel.transform.localPosition.z);
            pageLabel.transform.localScale *= 1.6f;
            pageLabel.autoSizeTextContainer = true;
            if (!isNext && Page <= 1)
                pageButton.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0.1f);
            var pagePassive = pageButton.GetComponent<PassiveButton>();
            pagePassive.OnClick = new();
            pagePassive.ClickSound = null;
            pagePassive.OnClick.AddListener((UnityAction)(() =>
            {
                int totalPages = roleButtons.ContainsKey((AssignedTeamType)currentTeamType) ?
                    (roleButtons[(AssignedTeamType)currentTeamType].Count / MaxOneScreenRole +
                     (roleButtons[(AssignedTeamType)currentTeamType].Count % MaxOneScreenRole != 0 ? 1 : 0)) : 0;
                // ページを移動できるか判定
                if (isNext && Page >= totalPages) return;
                else if (!isNext && Page <= 1) return;
                Logger.Info("クリック");
                if (isNext)
                    Page += 1;
                else
                    Page -= 1;
                ReloadPage();
            }));
            pageButtons.Add(pageButton.GetComponent<SpriteRenderer>());
        }

        if (ExPlayerControl.LocalPlayer.IsAlive())
        {
            CreatePage(false, meetingHud, container);
            CreatePage(true, meetingHud, container);
        }

        int ind = 0;

        // --- ローカル関数: CreateRole ---
        void CreateRole(IRoleBase rolebase)
        {
            if (rolebase == null)
                throw new Exception("rolebaseがnullです");
            AssignedTeamType team = rolebase.AssignedTeam;
            if (teamButtonCount[(int)team] >= 40)
                teamButtonCount[(int)team] = 0;
            Transform buttonParent = new GameObject().transform;
            buttonParent.SetParent(container);
            Transform button = UnityEngine.Object.Instantiate(buttonTemplate, buttonParent);
            button.Find("ControllerHighlight").gameObject.SetActive(false);
            Transform buttonMask = UnityEngine.Object.Instantiate(maskTemplate, buttonParent);
            var label = UnityEngine.Object.Instantiate(textTemplate, button);
            if (!roleButtons.ContainsKey(team))
            {
                roleButtons.Add(team, new System.Collections.Generic.List<Transform>());
            }
            roleButtons[team].Add(button);
            buttons.Add(button);
            int row = teamButtonCount[(int)team] / 5;
            int col = teamButtonCount[(int)team] % 5;
            buttonParent.localPosition = new Vector3(-3.47f + 1.75f * col, 1.5f - 0.45f * row, -200f);
            buttonParent.localScale = new Vector3(0.55f, 0.55f, 1f);
            label.text = ModHelpers.Cs(rolebase.RoleColor, ModTranslation.GetString(rolebase.Role.ToString()));
            label.alignment = TMPro.TextAlignmentOptions.Center;
            label.transform.localPosition = new Vector3(0, 0, label.transform.localPosition.z);
            label.transform.localScale *= 1.6f;
            label.autoSizeTextContainer = true;
            PassiveButton passiveButton = button.GetComponent<PassiveButton>();
            passiveButton.OnClick = new();
            passiveButton.ClickSound = null;
            if (ExPlayerControl.LocalPlayer.IsAlive())
            {
                passiveButton.OnClick.AddListener((UnityAction)(() =>
                {
                    Logger.Info("ボタンクリック!!!!!!!!!!!!!!!");
                    if (selectedButton != button)
                    {
                        selectedButton = button;
                        buttons.ForEach(x => x.GetComponent<SpriteRenderer>().color = (x == selectedButton) ? Color.red : Color.white);
                    }
                    else
                    {
                        if (!(meetingHud.state == MeetingHud.VoteStates.Voted ||
                              meetingHud.state == MeetingHud.VoteStates.NotVoted ||
                              meetingHud.state == MeetingHud.VoteStates.Discussion) || exPlayer == null)
                            return;
                        this.UseAbilityCount();
                        this.ShotThisMeeting++;

                        // 残り使用回数を更新
                        if (limitText != null)
                            limitText.text = ModTranslation.GetString("GuesserLimitText", Count, Math.Min(maxShots, shotsPerMeeting) - ShotThisMeeting);

                        var targetRole = exPlayer.Role;
                        ExPlayerControl dyingTarget = (targetRole == rolebase.Role) ? exPlayer : PlayerControl.LocalPlayer;
                        if (madmateSuicide && (ExPlayerControl.LocalPlayer.IsMadRoles() || ExPlayerControl.LocalPlayer.IsFriendRoles()))
                        {
                            dyingTarget = ExPlayerControl.LocalPlayer;
                        }

                        RpcShotGuesser(PlayerControl.LocalPlayer, dyingTarget, madmateSuicide && (dyingTarget.IsMadRoles() || dyingTarget.IsFriendRoles()), PlayerControl.LocalPlayer == exPlayer);
                        HideButtons = false;
                        SetActiveAllPlayerVoteArea(true);
                        UnityEngine.Object.Destroy(container.gameObject);

                    }
                }));
            }
            teamButtonCount[(int)team]++;
            ind++;
        }

        List<RoleId> GeneratedButtons = new();
        // --- 役割ボタンの生成（IntroData／RoleInfo から） ---
        foreach (IRoleBase roleBase in CustomRoleManager.AllRoles)
        {
            if (!IsValidRole(roleBase)) continue;

            CreateRoleAndRelated(roleBase, ref GeneratedButtons);
        }

        foreach (IGhostRoleBase roleBase in CustomRoleManager.AllGhostRoles)
        {
            if (!IsValidGhostRole(roleBase)) continue;

            RelatedGhostRole(roleBase, ref GeneratedButtons);
        }

        foreach (IModifierBase roleBase in CustomRoleManager.AllModifiers)
        {
            if (!IsValidModifierRole(roleBase)) continue;

            RelatedModifierRole(roleBase, ref GeneratedButtons);
        }

        void CreateRoleAndRelated(IRoleBase role, ref List<RoleId> generated)
        {
            if (generated.Contains(role.Role)) return;

            CreateRole(role);
            generated.Add(role.Role);

            if (role.RelatedRoleIds == null) return;

            foreach (var relatedId in role.RelatedRoleIds)
            {
                if (generated.Contains(relatedId)) continue;

                var relatedRole = CustomRoleManager.GetRoleById(relatedId);
                if (relatedRole == null) continue;

                CreateRoleAndRelated(relatedRole, ref generated);
            }
        }

        void RelatedGhostRole(IGhostRoleBase role, ref List<RoleId> generated)
        {
            if (role.RelatedRoleIds == null) return;

            foreach (var relatedId in role.RelatedRoleIds)
            {
                if (generated.Contains(relatedId)) continue;

                var relatedRole = CustomRoleManager.GetRoleById(relatedId);
                if (relatedRole == null) continue;

                CreateRoleAndRelated(relatedRole, ref generated);
            }
        }

        void RelatedModifierRole(IModifierBase role, ref List<RoleId> generated)
        {
            if (role.RelatedRoleIds == null) return;

            foreach (var relatedId in role.RelatedRoleIds)
            {
                if (generated.Contains(relatedId)) continue;

                var relatedRole = CustomRoleManager.GetRoleById(relatedId);
                if (relatedRole == null) continue;

                CreateRoleAndRelated(relatedRole, ref generated);
            }
        }

        bool IsValidGhostRole(IGhostRoleBase role)
        {
            if (role == null) return false;
            if (RoleOptionManager.TryGetGhostRoleOption(role.Role, out var option)) return false;
            if (option.NumberOfCrews == 0 || option.Percentage == 0) return false;
            return true;
        }

        bool IsValidModifierRole(IModifierBase role)
        {
            if (role == null) return false;
            if (!RoleOptionManager.TryGetModifierRoleOption(role.ModifierRole, out var option)) return false;
            if (!role.UseTeamSpecificAssignment && (option.NumberOfCrews == 0 || option.Percentage == 0)) return false;
            if (role.UseTeamSpecificAssignment && (
                (role.CrewmateChance == 0 || option.NumberOfCrews == 0) &&
                (role.ImpostorChance == 0 || option.MaxImpostors == 0) &&
                (role.NeutralChance == 0 || option.MaxNeutrals == 0))
                ) return false;
            return true;
        }

        bool IsValidRole(IRoleBase role)
        {
            if (role == null)
            {
                Logger.Info("continueになりました:" + role.Role, "Guesser");
                return false;
            }
            if (role.Role == RoleId.Crewmate && !cannotShootCrewmate)
                return true;
            else if (role.Role == RoleId.Impostor)
                return true;
            else if (role.Role == RoleId.Celebrity && cannotShootCelebrity)
            {
                // スターを撃てない設定がONで、撃てないターンを制限する設定がONの場合、
                // 指定されたターン数以降はスターを撃てるようにする
                if (!CelebrityLimitedTurns || MeetingCount < CelebrityLimitedTurnsCount)
                    return false;
            }

            if (!RoleOptionManager.TryGetRoleOption(role.Role, out var option) || option.NumberOfCrews == 0 || option.Percentage == 0)
            {
                Logger.Info("continueになりました:" + role.Role, "Guesser");
                return false;
            }

            return true;
        }
        container.localScale *= 0.75f;
        guesserSelectRole(AssignedTeamType.Crewmate);
        ReloadPage();
    }
    [CustomRPC]
    public static void RpcShotGuesser(PlayerControl killer, ExPlayerControl dyingTarget, bool isSuicide, bool isMisFire)
    {
        if (killer == null || dyingTarget == null)
            return;
        if (dyingTarget == null) return;
        dyingTarget.Player.Exiled();

        if (isMisFire || isSuicide) dyingTarget.FinalStatus = FinalStatus.GuesserMisFire;
        else dyingTarget.FinalStatus = FinalStatus.GuesserKill;
        if (Constants.ShouldPlaySfx()) SoundManager.Instance.PlaySound(dyingTarget.Player.KillSfx, false, 0.8f);

        // GuesserShotEventを発行
        Events.GuesserShotEvent.Invoke(killer, dyingTarget.Player, isMisFire || isSuicide);

        if (MeetingHud.Instance)
        {
            foreach (PlayerVoteArea pva in MeetingHud.Instance.playerStates)
            {
                if (pva.TargetPlayerId == dyingTarget.PlayerId)
                {
                    pva.SetDead(pva.DidReport, true);
                    pva.Overlay.gameObject.SetActive(true);
                }
                pva.UnsetVote();
            }
            MeetingHud.Instance.ClearVote();
            if (AmongUsClient.Instance.AmHost)
                MeetingHud.Instance.CheckForEndVoting();
        }
        if (FastDestroyableSingleton<HudManager>.Instance != null && killer != null)
        {
            if (PlayerControl.LocalPlayer == dyingTarget)
            {
                FastDestroyableSingleton<HudManager>.Instance.KillOverlay.ShowKillAnimation(killer.Data, dyingTarget.Data);
            }
        }
    }
}