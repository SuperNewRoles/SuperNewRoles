using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AmongUs.GameOptions;
using HarmonyLib;
using Hazel;
using Il2CppInterop.Generator.Passes;
using SuperNewRoles.Helpers;
using SuperNewRoles.Mode;
using SuperNewRoles.Patches;
using SuperNewRoles.Roles.Crewmate;
using SuperNewRoles.Roles.CrewMate;
using SuperNewRoles.Roles.Impostor;
using SuperNewRoles.Roles.Neutral;
using SuperNewRoles.Roles.Role;
using SuperNewRoles.Roles.RoleBases;
using SuperNewRoles.Roles.RoleBases.Interfaces;
using UnityEngine;

namespace SuperNewRoles.Roles.Attribute;

public class GuesserBase : RoleBase, ISupportSHR, ISHRChatCommand, IMeetingHandler
{
    public int Count { get; private set; }
    public bool CanShotOneMeeting { get; }
    public bool CannotShotCrew { get; }
    public bool CannotShotCelebrity { get; }
    public int CanShotCelebrityRemainingTurn { get; private set; }
    private bool ShotOnThisMeeting { get; set; }

    public virtual RoleTypes RealRole => throw new NotImplementedException();

    public string CommandName => "bt";

    public GuesserBase(int ShotMaxCount, bool CanShotOneMeeting, bool CannotShotCrew, bool CannotShotCelebrity, bool BecomeShotCelebrity, int BecomeShotCelebrityTurn, PlayerControl p, RoleInfo Roleinfo, OptionInfo Optioninfo, IntroInfo Introinfo) : base(p, Roleinfo, Optioninfo, Introinfo)
    {
        Count = ShotMaxCount;
        this.CanShotOneMeeting = CanShotOneMeeting;
        this.CannotShotCrew = CannotShotCrew;
        this.CannotShotCelebrity = CannotShotCelebrity;
        CanShotCelebrityRemainingTurn = BecomeShotCelebrity && CannotShotCelebrity ? BecomeShotCelebrityTurn : 0;
    }
    public void BuildName(StringBuilder Suffix, StringBuilder RoleNameText, PlayerData<string> ChangePlayers)
    {
        RoleNameText.Append($" ({Count.ToString()})");
    }
    public void UseCount()
    {
        Count--;
        ShotOnThisMeeting = true;
    }
    public void StartMeeting()
    {
        CanShotCelebrityRemainingTurn--;
        ShotOnThisMeeting = false;
        if (!ModeHandler.IsMode(ModeId.SuperHostRoles))
            return;
        if (Player.PlayerId != PlayerControl.LocalPlayer.PlayerId &&
            (!AmongUsClient.Instance.AmHost || Player.IsMod()))
            return;
        new LateTask(() => AddChatPatch.SendCommand(Player, ModTranslation.GetString($"GuesserOnStartMeetingInfo{(Count > 0 ? "Can" : "Cannot")}Shot", Count) + "\n" + ModTranslation.GetString("GuesserCommandUsage"), GuesserInfoTitle), 1.75f);
    }

    public bool OnChatCommand(string[] args)
    {
        if (Player == null)
            return true;
        if (args.Length < 2){
            AddChatPatch.SendCommand(Player,
                ModTranslation.GetString("GuesserCommandUsage"),
                GuesserInfoTitle
            );
            return true;
        }
        if (Count is not (-1) and <= 0)
        {
            AddChatPatch.SendCommand(Player,
                GetErrorText("NoCount"),
                GuesserInfoTitle
            );
            return true;
        }
        if (!CanShotOneMeeting && ShotOnThisMeeting)
        {
            AddChatPatch.SendCommand(Player,
                GetErrorText("ShotOnThisMeeting"),
                GuesserInfoTitle
            );
            return true;
        }
        PlayerControl target = ModHelpers.PlayerByColor(args[0]);
        if (target == null || target == Player)
        {
            AddChatPatch.SendCommand(Player,
                GetErrorText("NoneTarget"),
                GuesserInfoTitle
            );
            return true;
        }
        string RoleName = args[1];
        bool isCrewmate = false;
        RoleId? role = RoleId.DefaultRole;
        if (RoleName.ToLower() is "impostor" or "impo" or "インポスター" or "インポ")
            isCrewmate = false;
        else if (RoleName.ToLower() is "crewmate" or "crew" or "クルーメイト" or "クルー")
            isCrewmate = true;
        else
        {
            role = RoleinformationText.GetRoleIdByName(RoleName);
            if (role == null)
            {
                AddChatPatch.SendCommand(Player,
                    GetErrorText("NoneRole"),
                    GuesserInfoTitle
                );
                return true;
            }
            isCrewmate = CustomRoles.GetRoleTeam(role.Value) is TeamRoleType.Crewmate;
        }
        if (isCrewmate && CannotShotCrew)
        {
            AddChatPatch.SendCommand(Player,
                GetErrorText("CannotShotCrew"),
                GuesserInfoTitle
            );
            return true;
        }
        else if (CannotShotCelebrity)
        {
            AddChatPatch.SendCommand(Player,
                GetErrorText("CannotShotCelebrity"),
                GuesserInfoTitle
            );
            return true;
        }
        else if (target.IsDead())
        {
            AddChatPatch.SendCommand(Player,
                GetErrorText("TargetIsDead"),
                GuesserInfoTitle
            );
            return true;
        }
        bool isSuccess = false;
        if (target.GetRole() == role)
        {
            if (role == RoleId.DefaultRole)
            {
                if ((target.IsCrew() && isCrewmate) ||
                    (target.IsImpostor() && !isCrewmate))
                {
                    isSuccess = true;
                }
            }
            else
                isSuccess = true;
        }
        Logger.Info($"Guesser: {Player.Data.PlayerName} guessed {target.Data.PlayerName} as {role} and {target.GetRole()} and {(isSuccess ? "success" : "failed")}");
        PlayerControl targetPlayer = PlayerControl.LocalPlayer;
        if (isSuccess)
            targetPlayer = target;
        else
            targetPlayer = Player;
        UseCount();
        targetPlayer.RpcInnerExiled();
            AddChatPatch.SendCommand(null,
               ModTranslation.GetString("GuesserPlayerWasDead", targetPlayer.Data.DefaultOutfit.PlayerName) + "\n",
               ModTranslation.GetString("GuesserBigNewsTitle")
            );
        Mode.SuperHostRoles.Helpers.ShowReactorFlash(0.75f);

        // Shoot player and send chat info if activated
        MessageWriter writer = RPCHelper.StartRPC(CustomRPC.GuesserShoot);
        writer.Write(Player.PlayerId);
        writer.Write(targetPlayer.PlayerId);
        writer.Write(targetPlayer.PlayerId);
        writer.Write((byte)role);
        AmongUsClient.Instance.FinishRpcImmediately(writer);
        RPCProcedure.GuesserShoot(Player.PlayerId, targetPlayer.PlayerId, targetPlayer.PlayerId, (byte)role);
        return true;
    }
    private string GuesserInfoTitle => "<size=160%>"+CustomOptionHolder.Cs(Roleinfo.RoleColor, Roleinfo.NameKey + "Name")+"</size>";
    private static string GetErrorText(string ErrorId)
    {
        return ModTranslation.GetString("GuesserError" + ErrorId) + "\n" +
            ModTranslation.GetString("GuesserCommandUsage");
    }

    public void CloseMeeting()
    {
    }
}
class Guesser
{
    public const int MaxOneScreenRole = 40;
    public static int Page;
    public static PassiveButton ExitButton;
    public static GameObject guesserUI;
    private static Dictionary<TeamRoleType, List<Transform>> RoleButtons;
    private static Dictionary<TeamRoleType, SpriteRenderer> RoleSelectButtons;
    private static List<SpriteRenderer> PageButtons;
    public static TeamRoleType currentTeamType;
    static void guesserSelectRole(TeamRoleType Role, bool SetPage = true)
    {
        currentTeamType = Role;
        if (SetPage) Page = 1;
        foreach (var RoleButton in RoleButtons)
        {
            int index = 0;
            foreach (var RoleBtn in RoleButton.Value.AsSpan())
            {
                if (RoleBtn == null) continue;
                index++;
                if (index <= (Page - 1) * 40) { RoleBtn.gameObject.SetActive(false); continue; }
                if ((Page * 40) < index) { RoleBtn.gameObject.SetActive(false); continue; }
                RoleBtn.gameObject.SetActive(RoleButton.Key == Role);
            }
        }
        foreach (var RoleButton in RoleSelectButtons)
        {
            if (RoleButton.Value == null) continue;
            RoleButton.Value.color = new(0, 0, 0, RoleButton.Key == Role ? 1 : 0.25f);
        }
    }
    static void guesserOnClick(int buttonTarget, MeetingHud __instance)
    {
        GuesserBase guesserBaseMe = PlayerControl.LocalPlayer.GetRoleBase<GuesserBase>();
        if (guesserUI != null || !(__instance.state is MeetingHud.VoteStates.Voted or MeetingHud.VoteStates.NotVoted or MeetingHud.VoteStates.Discussion)) return;
        if (__instance.playerStates[buttonTarget].AmDead) return;
        Page = 1;
        RoleButtons = new();
        RoleSelectButtons = new();
        PageButtons = new();
        __instance.playerStates.ForEach(x => x.gameObject.SetActive(false));

        Transform container = UnityEngine.Object.Instantiate(__instance.transform.FindChild("MeetingContents/PhoneUI"), __instance.transform);
        container.transform.localPosition = new Vector3(0, 0, -200f);
        guesserUI = container.gameObject;

        List<int> i = new() { 0, 0, 0 };
        var buttonTemplate = __instance.playerStates[0].transform.FindChild("votePlayerBase");
        var maskTemplate = __instance.playerStates[0].transform.FindChild("MaskArea");
        var smallButtonTemplate = __instance.playerStates[0].Buttons.transform.Find("CancelButton");
        var textTemplate = __instance.playerStates[0].NameText;

        Transform exitButtonParent = new GameObject().transform;
        exitButtonParent.SetParent(container);
        Transform exitButton = UnityEngine.Object.Instantiate(buttonTemplate, exitButtonParent);
        exitButton.FindChild("ControllerHighlight").gameObject.SetActive(false);
        Transform exitButtonMask = UnityEngine.Object.Instantiate(maskTemplate, exitButtonParent);
        exitButton.gameObject.GetComponent<SpriteRenderer>().sprite = smallButtonTemplate.GetComponent<SpriteRenderer>().sprite;
        exitButtonParent.transform.localPosition = new Vector3(2.725f, 2.1f, -200f);
        exitButtonParent.transform.localScale = new Vector3(0.25f, 0.9f, 1f);
        exitButtonParent.transform.SetAsFirstSibling();
        exitButton.GetComponent<PassiveButton>().OnClick.RemoveAllListeners();
        exitButton.GetComponent<PassiveButton>().OnClick.AddListener((System.Action)(() =>
        {
            __instance.playerStates.ForEach(x => x.gameObject.SetActive(true));
            UnityEngine.Object.Destroy(container.gameObject);
        }));
        ExitButton = exitButton.GetComponent<PassiveButton>();

        List<Transform> buttons = new();
        Transform selectedButton = null;

        for (int index = 0; index < 3; index++)
        {
            Transform TeambuttonParent = new GameObject().transform;
            TeambuttonParent.SetParent(container);
            Transform Teambutton = UnityEngine.Object.Instantiate(buttonTemplate, TeambuttonParent);
            Teambutton.FindChild("ControllerHighlight").gameObject.SetActive(false);
            Transform TeambuttonMask = UnityEngine.Object.Instantiate(maskTemplate, TeambuttonParent);
            TMPro.TextMeshPro Teamlabel = UnityEngine.Object.Instantiate(textTemplate, Teambutton);
            // Teambutton.GetComponent<SpriteRenderer>().sprite = FastDestroyableSingleton<HatManager>.Instance.GetNamePlateById("nameplate_NoPlate")?.viewData?.viewData?.Image;
            RoleSelectButtons.Add((TeamRoleType)index, Teambutton.GetComponent<SpriteRenderer>());
            TeambuttonParent.localPosition = new(-2.75f + (index * 1.75f), 2.225f, -200);
            TeambuttonParent.localScale = new(0.55f, 0.55f, 1f);
            Teamlabel.color = (TeamRoleType)index is TeamRoleType.Crewmate ? RoleClass.CrewmateWhite : ((TeamRoleType)index is TeamRoleType.Impostor ? RoleClass.ImpostorRed : new Color32(127, 127, 127, byte.MaxValue));
            Logger.Info(Teamlabel.color.ToString(), ((TeamRoleType)index).ToString());
            Teamlabel.text = ModTranslation.GetString(((TeamRoleType)index is TeamRoleType.Crewmate ? "Crewmate" : ((TeamRoleType)index).ToString()) + "Name");
            Teamlabel.alignment = TMPro.TextAlignmentOptions.Center;
            Teamlabel.transform.localPosition = new Vector3(0, 0, Teamlabel.transform.localPosition.z);
            Teamlabel.transform.localScale *= 1.6f;
            Teamlabel.autoSizeTextContainer = true;
            static void CreateTeamButton(Transform Teambutton, TeamRoleType type)
            {
                Teambutton.GetComponent<PassiveButton>().OnClick.AddListener((UnityEngine.Events.UnityAction)(() =>
                {
                    guesserSelectRole(type);
                    ReloadPage();
                }));
            }
            if (PlayerControl.LocalPlayer.IsAlive()) CreateTeamButton(Teambutton, (TeamRoleType)index);
        }
        static void ReloadPage()
        {
            PageButtons[0].color = new(1, 1, 1, 1f);
            PageButtons[1].color = new(1, 1, 1, 1f);
            if ((RoleButtons[currentTeamType].Count / MaxOneScreenRole + (RoleButtons[currentTeamType].Count % MaxOneScreenRole != 0 ? 1 : 0)) < Page)
            {
                Page -= 1;
                PageButtons[1].color = new(1, 1, 1, 0.1f);
            }
            else if ((RoleButtons[currentTeamType].Count / MaxOneScreenRole + (RoleButtons[currentTeamType].Count % MaxOneScreenRole != 0 ? 1 : 0)) < Page + 1)
            {
                PageButtons[1].color = new(1, 1, 1, 0.1f);
            }
            if (Page <= 1)
            {
                Page = 1;
                PageButtons[0].color = new(1, 1, 1, 0.1f);
            }
            guesserSelectRole(currentTeamType, false);
            Logger.Info("Page:" + Page);
        }
        static void CreatePage(bool IsNext, MeetingHud __instance, Transform container)
        {
            var buttonTemplate = __instance.playerStates[0].transform.FindChild("votePlayerBase");
            var maskTemplate = __instance.playerStates[0].transform.FindChild("MaskArea");
            var smallButtonTemplate = __instance.playerStates[0].Buttons.transform.Find("CancelButton");
            var textTemplate = __instance.playerStates[0].NameText;
            Transform PagebuttonParent = new GameObject().transform;
            PagebuttonParent.SetParent(container);
            Transform Pagebutton = UnityEngine.Object.Instantiate(buttonTemplate, PagebuttonParent);
            Pagebutton.FindChild("ControllerHighlight").gameObject.SetActive(false);
            Transform PagebuttonMask = UnityEngine.Object.Instantiate(maskTemplate, PagebuttonParent);
            TMPro.TextMeshPro Pagelabel = UnityEngine.Object.Instantiate(textTemplate, Pagebutton);
            // Pagebutton.GetComponent<SpriteRenderer>().sprite = FastDestroyableSingleton<HatManager>.Instance.GetNamePlateById("nameplate_NoPlate")?.viewData?.viewData?.Image;
            PagebuttonParent.localPosition = IsNext ? new(3.535f, -2.2f, -200) : new(-3.475f, -2.2f, -200);
            PagebuttonParent.localScale = new(0.55f, 0.55f, 1f);
            Pagelabel.color = Color.white;
            Pagelabel.text = ModTranslation.GetString(IsNext ? "NextPage" : "PreviousPage");
            Pagelabel.alignment = TMPro.TextAlignmentOptions.Center;
            Pagelabel.transform.localPosition = new Vector3(0, 0, Pagelabel.transform.localPosition.z);
            Pagelabel.transform.localScale *= 1.6f;
            Pagelabel.autoSizeTextContainer = true;
            if (!IsNext && Page <= 1) Pagebutton.GetComponent<SpriteRenderer>().color = new(1, 1, 1, 0.1f);
            Pagebutton.GetComponent<PassiveButton>().OnClick.AddListener((UnityEngine.Events.UnityAction)(() =>
            {
                Logger.Info("クリック");
                if (IsNext) Page += 1;
                else Page -= 1;
                ReloadPage();
            }));
            PageButtons.Add(Pagebutton.GetComponent<SpriteRenderer>());
        }
        if (PlayerControl.LocalPlayer.IsAlive())
        {
            CreatePage(false, __instance, container);
            CreatePage(true, __instance, container);
        }

        int ind = 0;
        bool cannotCrewShot = guesserBaseMe.CannotShotCrew;
        bool cannotShotCelebrity = guesserBaseMe.CannotShotCelebrity;
        foreach (IntroData roleInfo in IntroData.Intros.Values)
        {
            if (roleInfo == null ||
                roleInfo.RoleId == RoleId.Hunter ||
                roleInfo.RoleId == RoleId.DefaultRole ||
                (IntroData.GetOption(roleInfo.RoleId)?.GetSelection() is null or 0) ||
                roleInfo.RoleId == RoleId.Celebrity)
            {
                Logger.Info("continueになりました:" + roleInfo.RoleId, "Guesser");
                continue; // Not guessable roles
            }
            CreateRole(roleInfo);
        }
        foreach (RoleInfo roleInfo in RoleInfoManager.RoleInfos.Values)
        {
            if (roleInfo == null ||
                roleInfo.Role == RoleId.Hunter ||
                roleInfo.Role == RoleId.DefaultRole ||
                (IntroData.GetOption(roleInfo.Role)?.GetSelection() is null or 0) ||
                roleInfo.Role == RoleId.Celebrity)
            {
                Logger.Info("continueになりました:" + roleInfo.Role, "Guesser");
                continue; // Not guessable roles
            }
            CreateRole(roleInfo: roleInfo);
        }
        CreateRole(IntroData.ImpostorIntro);
        if (!cannotCrewShot)
            CreateRole(IntroData.CrewmateIntro);
        if (guesserBaseMe.CanShotCelebrityRemainingTurn < 0 && !guesserBaseMe.CannotShotCelebrity && (IntroData.GetOption(IntroData.CelebrityIntro.RoleId)?.GetSelection() is not null and not 0))
            CreateRole(IntroData.CelebrityIntro);
        if (Jackal.Optioninfo.RoleOption.GetSelection() is not 0 && Jackal.JackalCreateSidekick.GetBool()) CreateRole(IntroData.SidekickIntro);
        if (CustomOptionHolder.JackalSeerOption.GetSelection() is not 0 && CustomOptionHolder.JackalSeerCreateSidekick.GetBool()) CreateRole(IntroData.SidekickSeerIntro);
        if (WaveCannonJackal.Optioninfo.RoleOption.GetBool() && WaveCannonJackal.CanCreateSidekick.GetBool())
        {
            CreateRole(roleInfo: SidekickWaveCannon.Roleinfo);
            CreateRole(roleInfo: Bullet.Roleinfo);
            CreateRole(IntroData.JackalFriendsIntro);
            CreateRole(IntroData.JackalIntro);
        }
        if (PavlovsOwner.Optioninfo.RoleOption.GetSelection() is not 0) CreateRole(roleInfo: PavlovsDogs.Roleinfo);
        if (CustomOptionHolder.RevolutionistAndDictatorOption.GetSelection() is not 0) { CreateRole(IntroData.DictatorIntro); CreateRole(IntroData.RevolutionistIntro); }
        if (CustomOptionHolder.AssassinAndMarlinOption.GetSelection() is not 0) { CreateRole(IntroData.AssassinIntro); CreateRole(IntroData.MarlinIntro); }
        if (Chief.Optioninfo.RoleOption.GetSelection() is not 0) { CreateRole(IntroData.SheriffIntro); }
        if (CustomOptionHolder.MadMakerOption.GetSelection() is not 0 || CustomOptionHolder.FastMakerOption.GetSelection() is not 0 ||
            (CustomOptionHolder.LevelingerOption.GetSelection() is not 0 && Levelinger.LevelingerCanUse("SidekickName")) ||
            (EvilSeer.Optioninfo.RoleOption.GetSelection() is not 0 && EvilSeer.CreateMode == 4) ||
            EvilHacker.Optioninfo.RoleOption.GetSelection() is not 0 && EvilHacker.MadmateSetting.GetBool())
        { CreateRole(IntroData.MadmateIntro); }
        if (CustomOptionHolder.SideKillerOption.GetSelection() is not 0) { CreateRole(IntroData.MadKillerIntro); }
        if (CustomOptionHolder.VampireOption.GetSelection() is not 0) { CreateRole(IntroData.DependentsIntro); }
        if (OrientalShaman.OrientalShamanOption.GetSelection() is not 0) { CreateRole(IntroData.ShermansServantIntro); }
        if (Santa.Optioninfo.RoleOption.GetSelection() is not 0)
        {
            var presentRoleData = Santa.PresentRoleData();

            foreach (var roleInfo in presentRoleData.RoleInfo.AsSpan()) { CreateRole(roleInfo: roleInfo); }
            foreach (var introData in presentRoleData.IntroData.AsSpan()) { CreateRole(introData); }
        }
        if (Impostor.MadRole.BlackSanta.Optioninfo.RoleOption.GetSelection() is not 0)
        {
            var presentRoleData = Impostor.MadRole.BlackSanta.PresentRoleData();

            foreach (var roleInfo in presentRoleData.RoleInfo.AsSpan()) { CreateRole(roleInfo: roleInfo); }
            foreach (var introData in presentRoleData.IntroData.AsSpan()) { CreateRole(introData); }
        }
        void CreateRole(IntroData introInfo = null, RoleInfo roleInfo = null)
        {
            if (introInfo == null && roleInfo == null)
                throw new Exception("introInfoとroleInfoがnullです");
            TeamRoleType team = introInfo?.Team ?? roleInfo?.Team ?? TeamRoleType.Crewmate;
            Color color = introInfo?.color ?? roleInfo?.RoleColor ?? Color.white;
            string NameKey = introInfo?.NameKey ?? roleInfo?.NameKey ?? "Crewmate";
            RoleId role = introInfo?.RoleId ?? roleInfo?.Role ?? RoleId.DefaultRole;
            if (40 <= i[(int)team])
                i[(int)team] = 0;
            Transform buttonParent = new GameObject().transform;
            buttonParent.SetParent(container);
            Transform button = UnityEngine.Object.Instantiate(buttonTemplate, buttonParent);
            button.FindChild("ControllerHighlight").gameObject.SetActive(false);
            Transform buttonMask = UnityEngine.Object.Instantiate(maskTemplate, buttonParent);
            TMPro.TextMeshPro label = UnityEngine.Object.Instantiate(textTemplate, button);
            // button.GetComponent<SpriteRenderer>().sprite = FastDestroyableSingleton<HatManager>.Instance.GetNamePlateById("nameplate_NoPlate")?.viewData?.viewData?.Image;
            if (!RoleButtons.ContainsKey(team))
            {
                RoleButtons.Add(team, new());
            }
            RoleButtons[team].Add(button);
            buttons.Add(button);
            int row = i[(int)team] / 5;
            int col = i[(int)team] % 5;
            buttonParent.localPosition = new Vector3(-3.47f + 1.75f * col, 1.5f - 0.45f * row, -200f);
            buttonParent.localScale = new Vector3(0.55f, 0.55f, 1f);
            label.text = CustomOptionHolder.Cs(color, NameKey + "Name");
            label.alignment = TMPro.TextAlignmentOptions.Center;
            label.transform.localPosition = new Vector3(0, 0, label.transform.localPosition.z);
            label.transform.localScale *= 1.6f;
            label.autoSizeTextContainer = true;
            int copiedIndex = i[(int)team];

            button.GetComponent<PassiveButton>().OnClick.RemoveAllListeners();
            if (PlayerControl.LocalPlayer.IsAlive()) button.GetComponent<PassiveButton>().OnClick.AddListener((System.Action)(() =>
            {
                if (selectedButton != button)
                {
                    selectedButton = button;
                    foreach (var x in buttons.AsSpan())
                    {
                        x.GetComponent<SpriteRenderer>().color = x == selectedButton ? Color.red : Color.white;
                    }
                }
                else
                {
                    PlayerControl focusedTarget = ModHelpers.PlayerById(__instance.playerStates[buttonTarget].TargetPlayerId);
                    if (!(__instance.state == MeetingHud.VoteStates.Voted || __instance.state == MeetingHud.VoteStates.NotVoted) || focusedTarget == null) return;
                    if (guesserBaseMe.Count is not (-1) and <= 0) return;

                    var Role = focusedTarget.GetRole();

                    PlayerControl dyingTarget;
                    if (Role == role)
                    {
                        dyingTarget = focusedTarget;
                    }
                    else
                    {
                        dyingTarget = PlayerControl.LocalPlayer;
                    }


                    // Reset the GUI
                    __instance.playerStates.ForEach(x => x.gameObject.SetActive(true));
                    UnityEngine.Object.Destroy(container.gameObject);

                    guesserBaseMe.UseCount();
                    if ((guesserBaseMe.Count > 0) && dyingTarget != PlayerControl.LocalPlayer && guesserBaseMe.CanShotOneMeeting)
                    {
                        __instance.playerStates.ForEach(x => { if (x.TargetPlayerId == dyingTarget.PlayerId && x.transform.FindChild("ShootButton") != null) UnityEngine.Object.Destroy(x.transform.FindChild("ShootButton").gameObject); });
                    }
                    else
                    {
                        __instance.playerStates.ForEach(x => { if (x.transform.FindChild("ShootButton") != null) UnityEngine.Object.Destroy(x.transform.FindChild("ShootButton").gameObject); });
                    }
                    // Shoot player and send chat info if activated
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.GuesserShoot, SendOption.Reliable, -1);
                    writer.Write(CachedPlayer.LocalPlayer.PlayerId);
                    writer.Write(dyingTarget.PlayerId);
                    writer.Write(focusedTarget.PlayerId);
                    writer.Write((byte)role);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.GuesserShoot(PlayerControl.LocalPlayer.PlayerId, dyingTarget.PlayerId, focusedTarget.PlayerId, (byte)role);
                }
            }));
            i[(int)team]++;
            ind++;
        }
        container.transform.localScale *= 0.75f;
        guesserSelectRole(TeamRoleType.Crewmate);
        ReloadPage();
    }

    [HarmonyPatch(typeof(PlayerVoteArea), nameof(PlayerVoteArea.Select))]
    class PlayerVoteAreaSelectPatch
    {
        static bool Prefix(MeetingHud __instance)
        {
            return !(PlayerControl.LocalPlayer != null && PlayerControl.LocalPlayer.IsRole(RoleId.NiceGuesser, RoleId.EvilGuesser) && guesserUI != null);
        }
    }
    public class StartMeetingPatch
    {
        public static void Postfix(MeetingHud __instance)
        {
            if (ModeHandler.IsMode(ModeId.Default) && PlayerControl.LocalPlayer.GetRoleBase<GuesserBase>().Count is > 0 or (-1))
            {
                createGuesserButton(__instance);
            }
        }
    }
    public static void createGuesserButton(MeetingHud __instance)
    {
        for (int i = 0; i < __instance.playerStates.Length; i++)
        {
            PlayerVoteArea playerVoteArea = __instance.playerStates[i];
            if (playerVoteArea.AmDead || playerVoteArea.TargetPlayerId == PlayerControl.LocalPlayer.PlayerId) continue;

            GameObject template = playerVoteArea.Buttons.transform.Find("CancelButton").gameObject;
            GameObject targetBox = UnityEngine.Object.Instantiate(template, playerVoteArea.transform);
            targetBox.name = "ShootButton";
            targetBox.transform.localPosition = new Vector3(-0.95f, 0.03f, -1.3f);
            SpriteRenderer renderer = targetBox.GetComponent<SpriteRenderer>();
            renderer.sprite = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.TargetIcon.png", 115f);
            PassiveButton button = targetBox.GetComponent<PassiveButton>();
            button.OnClick.RemoveAllListeners();
            int copiedIndex = i;
            button.OnClick.AddListener((Action)(() => guesserOnClick(copiedIndex, __instance)));
        }
    }
}