using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using Hazel;
using SuperNewRoles.Helpers;
using UnityEngine;

namespace SuperNewRoles.Roles.Neutral;

public class Moira
{
    private const int OptionId = 303300;
    public static CustomRoleOption MoiraOption;
    public static CustomOption MoiraPlayerCount;
    public static CustomOption MoiraAbilityLimit;
    public static CustomOption MoiraChangeVote;
    public static void SetupCustomOptions()
    {
        MoiraOption = CustomOption.SetupCustomRoleOption(OptionId, false, RoleId.Moira);
        MoiraPlayerCount = CustomOption.Create(OptionId + 1, false, CustomOptionType.Neutral, "SettingPlayerCountName", CustomOptionHolder.AlonePlayers[0], CustomOptionHolder.AlonePlayers[1], CustomOptionHolder.AlonePlayers[2], CustomOptionHolder.AlonePlayers[3], MoiraOption);
        MoiraAbilityLimit = CustomOption.Create(OptionId + 2, false, CustomOptionType.Neutral, "MoiraAbilityLimit", 5f, 1f, 10f, 1f, MoiraOption);
        MoiraChangeVote = CustomOption.Create(OptionId + 3, false, CustomOptionType.Neutral, "MoiraChangeVote", true, MoiraOption);
    }

    public static List<PlayerControl> MoiraPlayer;
    public static Color32 color = new(201, 127, 219, byte.MaxValue);
    public static int AbilityLimit;
    public static List<(byte, byte)> ChangeData;
    public static (byte, byte) SwapVoteData;
    public static bool AbilityUsedUp;
    public static bool AbilityUsedThisMeeting;
    public static Dictionary<byte, RoleTypes> RoleTypeData;
    public static Sprite GetButtonSprite() => ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.MoiraButton.png", 200f);
    public static PlayerControl Player
    {
        get
        {
            if (MoiraPlayer.Count <= 0) return null;
            return MoiraPlayer[0];
        }
    }
    public static void ClearAndReload()
    {
        MoiraPlayer = new();
        AbilityLimit = MoiraAbilityLimit.GetInt();
        ChangeData = new();
        SwapVoteData = new(byte.MaxValue, byte.MaxValue);
        AbilityUsedUp = false;
        AbilityUsedThisMeeting = false;
        RoleTypeData = new();
    }

    public static PlayerControl Selected;
    public static void StartMeeting(MeetingHud __instance)
    {
        if (AbilityLimit > 0)
        {
            Selected = null;
            foreach (PlayerVoteArea data in __instance.playerStates)
            {
                PlayerControl player = ModHelpers.PlayerById(data.TargetPlayerId);
                if (player.IsAlive() && !player.AmOwner)
                {
                    Transform target = Object.Instantiate(data.Buttons.transform.Find("CancelButton"), data.transform);
                    target.name = "MoiraButton";
                    target.localPosition = new Vector3(1f, 0.01f, -1f);
                    target.GetComponent<SpriteRenderer>().sprite = GetButtonSprite();
                    PassiveButton button = target.GetComponent<PassiveButton>();
                    button.OnClick.RemoveAllListeners();
                    button.OnClick.AddListener((UnityEngine.Events.UnityAction)(() => OnClickButton(__instance, player)));
                }
            }
        }
    }

    public static void OnClickButton(MeetingHud __instance, PlayerControl target)
    {
        if (PlayerControl.LocalPlayer.IsDead())
        {
            DestroyClickButton(__instance);
            return;
        }
        if (AbilityLimit <= 0) return;
        Transform transform = __instance.playerStates.First(x => x.TargetPlayerId == target.PlayerId).transform.FindChild("MoiraButton");
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
            DestroyClickButton(__instance);
        }
        else
        {
            Selected = null;
            sprite.color = Palette.EnabledColor;
        }
    }

    public static void UseAbility(PlayerControl player1, PlayerControl player2)
    {
        if (AbilityUsedThisMeeting) return;
        if (AbilityLimit <= 0) return;
        AbilityLimit--;
        RpcChangeRole(player1, player2, AbilityLimit <= 0);
        AbilityUsedThisMeeting = true;
    }

    public static void DestroyClickButton(MeetingHud __instance)
    {
        foreach (PlayerVoteArea data in __instance.playerStates)
        {
            if (data.transform.FindChild("MoiraButton") != null)
                Object.Destroy(data.transform.FindChild("MoiraButton").gameObject);
        }
    }

    public static void SwapVoteArea(MeetingHud __instance)
    {
        DestroyClickButton(__instance);
        if (Player.IsDead()) return;
        PlayerVoteArea swapped1 = null;
        PlayerVoteArea swapped2 = null;
        foreach (PlayerVoteArea playerVoteArea in __instance.playerStates)
        {
            if (playerVoteArea.TargetPlayerId == SwapVoteData.Item1) swapped1 = playerVoteArea;
            if (playerVoteArea.TargetPlayerId == SwapVoteData.Item2) swapped2 = playerVoteArea;
        }
        if (swapped1 != null && swapped2 != null)
        {
            __instance.StartCoroutine(Effects.Slide3D(swapped1.transform, swapped1.transform.localPosition, swapped2.transform.localPosition, 1.5f));
            __instance.StartCoroutine(Effects.Slide3D(swapped2.transform, swapped2.transform.localPosition, swapped1.transform.localPosition, 1.5f));
        }
    }

    public static void RpcChangeRole(PlayerControl player1, PlayerControl player2, bool IsUseEnd)
    {
        MessageWriter writer = RPCHelper.StartRPC(CustomRPC.MoiraChangeRole);
        writer.Write(player1.PlayerId);
        writer.Write(player2.PlayerId);
        writer.Write(IsUseEnd);
        writer.EndRPC();
        RPCProcedure.MoiraChangeRole(player1.PlayerId, player2.PlayerId, IsUseEnd);
    }

    public static void SwapRole(byte player1Id, byte player2Id)
    {
        PlayerControl player1 = ModHelpers.PlayerById(player1Id);
        PlayerControl player2 = ModHelpers.PlayerById(player2Id);
        if (player1 is null || player2 is null) return;
        RoleId player1Role = player1.GetRole();
        RoleId player2Role = player2.GetRole();
        RoleTypes player1RoleType = !player1.Data.RoleWhenAlive.HasValue ? player1.Data.Role.Role : RoleTypeData.ContainsKey(player1Id) ? RoleTypeData[player1Id] : player1.Data.RoleWhenAlive.Value;
        RoleTypes player2RoleType = !player2.Data.RoleWhenAlive.HasValue ? player2.Data.Role.Role : RoleTypeData.ContainsKey(player2Id) ? RoleTypeData[player2Id] : player2.Data.RoleWhenAlive.Value;

        player1.SetRoleRPC(player2Role);
        if (player1.IsAlive()) player1.RPCSetRoleUnchecked(player2RoleType);
        RoleTypeData[player1Id] = player2RoleType;

        player2.SetRoleRPC(player1Role);
        if (player2.IsAlive()) player2.RPCSetRoleUnchecked(player1RoleType);
        RoleTypeData[player2Id] = player1RoleType;
    }

    public static void WrapUp(GameData.PlayerInfo exiled)
    {
        AbilityUsedThisMeeting = false;
        if (!AmongUsClient.Instance.AmHost)
        {
            SwapVoteData = new(byte.MaxValue, byte.MaxValue);
            return;
        }
        SwapRole(SwapVoteData.Item1, SwapVoteData.Item2);
        SwapVoteData = new(byte.MaxValue, byte.MaxValue);
        if (exiled is null) return;
        if (exiled.Object.IsRole(RoleId.Moira))
        {
            ChangeData.Reverse();
            foreach (var data in ChangeData)
                SwapRole(data.Item1, data.Item2);
        }
    }
}