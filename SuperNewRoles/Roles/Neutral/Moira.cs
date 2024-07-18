using System.Collections.Generic;
using System.Linq;
using System.Text;
using AmongUs.GameOptions;
using Hazel;
using SuperNewRoles.Helpers;
using SuperNewRoles.Mode;
using SuperNewRoles.Mode.SuperHostRoles;
using SuperNewRoles.Patches;
using SuperNewRoles.Roles.Role;
using SuperNewRoles.Roles.RoleBases;
using SuperNewRoles.Roles.RoleBases.Interfaces;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SuperNewRoles.Roles.Neutral;

public class Moira : RoleBase, INeutral, IMeetingHandler, IWrapUpHandler, INameHandler, IRpcHandler, IHijackingWinner, ISupportSHR, ISHRChatCommand, ISHRAntiBlackout
{
    public static new RoleInfo Roleinfo = new(
        typeof(Moira),
        (p) => new Moira(p),
        RoleId.Moira,
        "Moira",
        new(201, 127, 219, byte.MaxValue),
        new(RoleId.Moira, TeamTag.Neutral, RoleTag.Information),
        TeamRoleType.Neutral,
        TeamType.Neutral
    );
    public static new OptionInfo Optioninfo = new(RoleId.Moira, 303300, true, optionCreator: CreateOption, MaxPlayer: 1);
    public static new IntroInfo Introinfo = new(RoleId.Moira, 1, RoleTypes.Shapeshifter);
    
    public static CustomOption AbilityLimit;
    public static CustomOption ChangeVote;
    private static void CreateOption()
    {
        AbilityLimit = CustomOption.Create(Optioninfo.OptionId++, true, CustomOptionType.Neutral, "MoiraAbilityLimit", 5f, 1f, 10f, 1f, Optioninfo.RoleOption);
        ChangeVote = CustomOption.Create(Optioninfo.OptionId++, true, CustomOptionType.Neutral, "MoiraChangeVote", true, Optioninfo.RoleOption);
    }

    public int Limit;
    public int OldLimit;
    public bool IsLimitOver => Limit <= 0;
    public List<(byte, byte)> ChangeData;
    public (byte, byte) SwapVoteData;
    public bool AbilityUsedThisMeeting;
    public PlayerControl Selected;
    public Mode Status;
    public Moira(PlayerControl player) : base(player, Roleinfo, Optioninfo, Introinfo)
    {
        Limit = AbilityLimit.GetInt();
        OldLimit = Limit;
        ChangeData = new();
        SwapVoteData = (byte.MaxValue, byte.MaxValue);
        AbilityUsedThisMeeting = false;
        Selected = null;
        Status = Mode.Standby;
    }

    public RoleTypes RealRole => RoleTypes.Crewmate;

    public IHijackingWinner.Rank Priority => IHijackingWinner.Rank.Rank3;

    public WinCondition Condition => WinCondition.MoiraWin;

    public void OnClickButton(PlayerControl target)
    {
        if (Player.IsDead())
        {
            DestroyClickButton();
            return;
        }
        if (IsLimitOver) return;
        Transform transform = MeetingHud.Instance.playerStates.First(x => x.TargetPlayerId == target.PlayerId)?.transform.FindChild("MoiraButton");
        if (!transform) return;
        SpriteRenderer sprite = transform.GetComponent<SpriteRenderer>();
        if (sprite.color == Palette.EnabledColor)
        {
            if (Selected == null)
            {
                Selected = target;
                sprite.color = Palette.DisabledClear;
                return;
            }
            UseAbility(Selected, target);
            DestroyClickButton();
        }
        else
        {
            Selected = null;
            sprite.color = Palette.EnabledColor;
        }
    }

    public void UseAbility(PlayerControl target1, PlayerControl target2)
    {
        if (AbilityUsedThisMeeting) return;
        if (IsLimitOver) return;
        Limit--;
        MessageWriter writer = RpcWriter;
        writer.Write(Limit);
        writer.Write(target1.PlayerId);
        writer.Write(target2.PlayerId);
        SendRpc(writer);
        AbilityUsedThisMeeting = true;
    }

    public void DestroyClickButton()
    {
        foreach (PlayerVoteArea data in MeetingHud.Instance.playerStates)
        {
            if (data.transform.FindChild("MoiraButton") != null)
                Object.Destroy(data.transform.FindChild("MoiraButton").gameObject);
        }
    }

    public void SwapRole(byte target1Id, byte target2Id)
    {
        PlayerControl target1 = ModHelpers.PlayerById(target1Id);
        PlayerControl target2 = ModHelpers.PlayerById(target2Id);
        if (!target1 || !target2) return;

        RoleTypes player1_role_type = !target1.Data.RoleWhenAlive.HasValue ? target1.Data.Role.Role : target1.Data.RoleWhenAlive.Value;
        RoleTypes player2_role_type = !target2.Data.RoleWhenAlive.HasValue ? target2.Data.Role.Role : target2.Data.RoleWhenAlive.Value;


        switch (ModeHandler.GetMode())
        {
            case ModeId.Default:
                if (target1.IsAlive()) target1.RPCSetRoleUnchecked(player2_role_type);
                if ((bool)target1.Data.RoleWhenAlive?.HasValue) target1.Data.RoleWhenAlive.value = player2_role_type;

                if (target2.IsAlive()) target2.RPCSetRoleUnchecked(player1_role_type);
                if ((bool)target2.Data.RoleWhenAlive?.HasValue) target2.Data.RoleWhenAlive.value = player1_role_type;

                target1.SwapRoleRPC(target2);
                break;
            case ModeId.SuperHostRoles:
                CustomRpcSender sender = CustomRpcSender.Create("RoleChenge");

                if (target1.IsAlive()) SHRSwapTo(sender, target1, target2, player2_role_type);
                if ((bool)target1.Data.RoleWhenAlive?.HasValue) target1.Data.RoleWhenAlive.value = player2_role_type;
                
                if (target2.IsAlive()) SHRSwapTo(sender, target2, target1, player1_role_type);
                if ((bool)target2.Data.RoleWhenAlive?.HasValue) target2.Data.RoleWhenAlive.value = player1_role_type;

                target1.SwapRoleRPC(target2);

                ChangeName.SetRoleNames(sender: sender);
                sender.SendMessage();

                break;
        }
    }
    private void SHRSwapTo(CustomRpcSender sender, PlayerControl player1, PlayerControl player2, RoleTypes player2RoleTypes)
    {
        if (player2.GetRoleBase() is ISupportSHR shr && shr.IsDesync) sender.SetRoleDesync(player1, shr.DesyncRole);
        else
        {
            RoleId role = player2.GetRole();
            var data = RoleSelectHandler.GetDesyncRole(role);
            if (data.IsDesync) sender.SetRoleDesync(player1, data.RoleType);
            else
            {
                RoleTypes targetRoleTypes = player2RoleTypes;
                if (targetRoleTypes.IsImpostorRole())
                {
                    sender.RpcSetRole(player1, targetRoleTypes, true);
                    foreach(PlayerControl player in PlayerControl.AllPlayerControls)
                    {
                        if (player.PlayerId == player1.PlayerId)
                            continue;
                        if ((!player.IsMod() &&
                            player is ISupportSHR supportSHR && supportSHR.IsDesync) ||
                            RoleSelectHandler.GetDesyncRole(player.GetRole()).IsDesync)
                        {
                            sender.RpcSetRole(player, RoleTypes.Scientist, true, player.GetClientId());
                        }
                        if (!player.IsImpostor())
                            continue;
                        sender.RpcSetRole(player, player.Data.Role.Role, true, player1.GetClientId());
                    }
                }
                else sender.RpcSetRole(player1, targetRoleTypes, true);
            }
        }
    }

    public void StartMeeting()
    {
        Selected = null;
        Status = Mode.Standby;
        if (RoleClass.Assassin.TriggerPlayer != null) return;
        switch (ModeHandler.GetMode())
        {
            case ModeId.Default:
                if (!Player.AmOwner || IsLimitOver) break;
                foreach (PlayerVoteArea data in MeetingHud.Instance.playerStates)
                {
                    PlayerControl player = ModHelpers.PlayerById(data.TargetPlayerId);
                    if (player.IsAlive() && !player.AmOwner)
                    {
                        Transform target = Object.Instantiate(data.Buttons.transform.Find("CancelButton"), data.transform);
                        target.name = "MoiraButton";
                        target.localPosition = new Vector3(1f, 0.01f, -1f);
                        target.GetComponent<SpriteRenderer>().sprite = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.MoiraButton.png", 200f);
                        PassiveButton button = target.GetComponent<PassiveButton>();
                        button.OnClick.RemoveAllListeners();
                        button.OnClick.AddListener(() => OnClickButton(player));
                    }
                }
                break;
            case ModeId.SuperHostRoles:
                if (!AmongUsClient.Instance.AmHost) return;
                new LateTask(() =>
                {
                    if (!IsLimitOver) AddChatPatch.SendChat(Player, ModTranslation.GetString("BalancerForActivate"), ModTranslation.GetString("MoiraName"));
                    else AddChatPatch.SendChat(Player, ModTranslation.GetString("BalancerUsed"), ModTranslation.GetString("MoiraName"));
                }, 0.5f);
                break;
        }
    }

    public void CloseMeeting()
    {
        DestroyClickButton();
        if (Player.IsDead() || !AbilityUsedThisMeeting) return;
        AbilityUsedThisMeeting = false;

        if (ModeHandler.IsMode(ModeId.SuperHostRoles) && AmongUsClient.Instance.AmHost)
        {
            new LateTask(() =>
            {
                AddChatPatch.SendCommand(
                    null,
                    $"{ModTranslation.GetString("MoiraSwapText", SwapVoteData.Item1.GetPlayerControl().Data.PlayerName, SwapVoteData.Item2.GetPlayerControl().Data.PlayerName)}",
                    ModTranslation.GetString("GuesserBigNewsTitle")
                );
            }, 0.2f);
        }

        PlayerVoteArea swapped1 = null;
        PlayerVoteArea swapped2 = null;
        foreach (PlayerVoteArea playerVoteArea in MeetingHud.Instance.playerStates)
        {
            if (playerVoteArea.TargetPlayerId == SwapVoteData.Item1) swapped1 = playerVoteArea;
            if (playerVoteArea.TargetPlayerId == SwapVoteData.Item2) swapped2 = playerVoteArea;
        }
        if (swapped1 != null && swapped2 != null)
        {
            MeetingHud.Instance.StartCoroutine(Effects.Slide3D(swapped1.transform, swapped1.transform.localPosition, swapped2.transform.localPosition, 1.5f));
            MeetingHud.Instance.StartCoroutine(Effects.Slide3D(swapped2.transform, swapped2.transform.localPosition, swapped1.transform.localPosition, 1.5f));
        }
    }

    public bool CastVote(byte target_id)
    {
        if (!ModeHandler.IsMode(ModeId.SuperHostRoles)) return true;
        if (!AmongUsClient.Instance.AmHost) return true;
        if (RoleClass.Assassin.TriggerPlayer != null) return true;
        PlayerControl target = target_id.GetPlayerControl();
        if (BotManager.IsBot(target_id)) return false;
        switch (Status)
        {
            case Mode.Standby:
                if (target != Player) return true;
                if (IsLimitOver)
                {
                    AddChatPatch.SendCommand(Player, ModTranslation.GetString("MoiraErrorVoteLimitOverText"), ModTranslation.GetString("MoiraErrorVoteLimitOver"));
                    Status = Mode.AbilityEnd;
                    return false;
                }
                Status = Mode.Selection;
                AddChatPatch.SendChat(Player, $"{ModTranslation.GetString("MoiraSelectionText")}\n\n{ModTranslation.GetString("Balancer1st")}", ModTranslation.GetString("MoiraSelection"));
                return false;
            case Mode.Selection:
                if (target_id is 252 or 253)
                {
                    if (Selected)
                    {
                        Selected = null;
                        AddChatPatch.SendChat(Player, $"{ModTranslation.GetString("MoiraSelectionBackText")}\n\n{ModTranslation.GetString("Balancer1st")}", ModTranslation.GetString("MoiraSelectionBack"));
                    }
                    else
                    {
                        Status = Mode.Standby;
                        AddChatPatch.SendChat(Player, ModTranslation.GetString("MoiraSelectionCancelText"), ModTranslation.GetString("MoiraSelectionCancel"));
                    }
                }
                else if (target == Player)
                {
                    if (Selected)
                    {
                        Selected = null;
                        AddChatPatch.SendChat(Player, $"{ModTranslation.GetString("MoiraSelectionBackText")}\n\n{ModTranslation.GetString("Balancer1st")}", ModTranslation.GetString("MoiraSelectionBack"));
                    }
                    else
                    {
                        Status = Mode.Standby;
                        AddChatPatch.SendChat(Player, ModTranslation.GetString("BalancerSelectionSelfSelectText"), ModTranslation.GetString("MoiraSelectionCancel"));
                        return true;
                    }
                }
                else if (Selected)
                {
                    if (Selected == target)
                    {
                        Selected = null;
                        AddChatPatch.SendChat(Player, $"{ModTranslation.GetString("MoiraSelectionBackText")}\n\n{ModTranslation.GetString("Balancer1st")}", ModTranslation.GetString("MoiraSelectionBack"));
                        return false;
                    }
                    Status = Mode.AbilityEnd;
                    UseAbility(Selected, target);
                    AddChatPatch.SendChat(Player, $"{ModTranslation.GetString("MoiraSelectionEndText")}\n\n{ModTranslation.GetString("Balancer1st")} : {Selected?.Data.PlayerName}\n{ModTranslation.GetString("Balancer2nd")} : {target?.Data.PlayerName}", ModTranslation.GetString("MoiraSelectionEnd"));
                    Logger.Info($"Swap fixed. {SwapVoteData.Item1}, {SwapVoteData.Item2}", "Moira");
                }
                else
                {
                    Selected = target;
                    AddChatPatch.SendChat(Player, $"{ModTranslation.GetString("MoiraSelectionText")}\n\n{ModTranslation.GetString("Balancer1st")} : {target?.Data.PlayerName}\n\n{ModTranslation.GetString("Balancer2nd")}", ModTranslation.GetString("MoiraSelection"));
                }
                return false;
        }
        return true;
    }

    public void CalculateVotes(Dictionary<byte, int> dic)
    {
        if (!AbilityUsedThisMeeting || !ChangeVote.GetBool() || Player.IsDead()) return;
        PlayerVoteArea swapped1 = null;
        PlayerVoteArea swapped2 = null;
        foreach (PlayerVoteArea playerVoteArea in MeetingHud.Instance.playerStates)
        {
            if (playerVoteArea.TargetPlayerId == SwapVoteData.Item1) swapped1 = playerVoteArea;
            if (playerVoteArea.TargetPlayerId == SwapVoteData.Item2) swapped2 = playerVoteArea;
            if (swapped1 && swapped2) break;
        }
        if (swapped1 && swapped2)
        {
            if (!dic.ContainsKey(swapped1.TargetPlayerId)) dic[swapped1.TargetPlayerId] = 0;
            if (!dic.ContainsKey(swapped2.TargetPlayerId)) dic[swapped2.TargetPlayerId] = 0;
            (dic[swapped1.TargetPlayerId], dic[swapped2.TargetPlayerId]) = (dic[swapped2.TargetPlayerId], dic[swapped1.TargetPlayerId]);
        }
    }

    public void OnWrapUp()
    {
        OldLimit = Limit;
        if (!AmongUsClient.Instance.AmHost)
        {
            SwapVoteData = new(byte.MaxValue, byte.MaxValue);
            return;
        }
        if (ModeHandler.IsMode(ModeId.SuperHostRoles))
            return;
        SwapRole(SwapVoteData.Item1, SwapVoteData.Item2);
        SwapVoteData = new(byte.MaxValue, byte.MaxValue);
    }

    public void OnWrapUp(PlayerControl exiled)
    {
        if (!AmongUsClient.Instance.AmHost) return;
        if (exiled != Player) return;
        ChangeData.Reverse();
        new LateTask(() =>
        {
            foreach (var data in ChangeData)
                SwapRole(data.Item1, data.Item2);
        }, 0.2f);
    }

    public void OnHandleAllPlayer()
    {
        if (OldLimit > 0) return;
        SetNamesClass.SetPlayerNameText(Player, $"{Player.NameText().text} {"(⇔)".Color(Roleinfo.RoleColor)}");
    }

    public void RpcReader(MessageReader reader)
    {
        Limit = reader.ReadInt32();
        SwapVoteData = (reader.ReadByte(), reader.ReadByte());
        ChangeData.Add(SwapVoteData);
        AbilityUsedThisMeeting = true;
    }

    public bool CanWin(GameOverReason gameOverReason, WinCondition winCondition) => Player.IsAlive() && IsLimitOver;

    public void BuildName(StringBuilder Suffix, StringBuilder RoleNameText, PlayerData<string> ChangePlayers) => RoleNameText.Append($" ({Limit})");

    public bool BuildAllName(out string text)
    {
        if (OldLimit > 0)
        {
            text = "";
            return false;
        }
        text = $" {"(⇔)".Color(Roleinfo.RoleColor)}";
        return true;
    }

    public string CommandName => "swap";

    private string MoiraInfoTitle => "<size=160%>" + CustomOptionHolder.Cs(Roleinfo.RoleColor, Roleinfo.NameKey + "Name") + "</size>";
    private static string CommandUsage(string text) => $"{text}\n{ModTranslation.GetString("MoiraCommandUsage")}";

    public bool OnChatCommand(string[] args)
    {
        if (IsLimitOver)
        {
            AddChatPatch.SendCommand(Player, ModTranslation.GetString("MoiraErrorLimitOver"), MoiraInfoTitle);
            return true;
        }
        if (args.Length < 2)
        {
            AddChatPatch.SendCommand(Player, ModTranslation.GetString("MoiraCommandUsage"), MoiraInfoTitle);
            return true;
        }
        PlayerControl target1 = args[0].GetPlayerControl();
        PlayerControl target2 = args[1].GetPlayerControl();
        if (target1 == null || target1 == Player || target2 == null || target2 == Player)
        {
            AddChatPatch.SendCommand(Player, CommandUsage("GuesserErrorNoneTarget"), MoiraInfoTitle);
            return true;
        }
        if (target1 == target2)
        {
            AddChatPatch.SendCommand(Player, CommandUsage("MoiraErrorSamePerson"), MoiraInfoTitle);
            return true;
        }
        Status = Mode.AbilityEnd;
        UseAbility(target1, target2);
        AddChatPatch.SendChat(Player, $"{ModTranslation.GetString("MoiraSelectionEndText")}\n\n{ModTranslation.GetString("Balancer1st")} : {target1.Data.PlayerName}\n{ModTranslation.GetString("Balancer2nd")} : {target2.Data.PlayerName}", ModTranslation.GetString("MoiraSelectionEnd"));
        return true;
    }

    public void StartAntiBlackout()
    {
    }

    public void EndAntiBlackout()
    {
        SwapRole(SwapVoteData.Item1, SwapVoteData.Item2);
        SwapVoteData = new(byte.MaxValue, byte.MaxValue);
    }

    public enum Mode
    {
        Standby,
        Selection,
        AbilityEnd
    }
}