using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using Hazel;
using SuperNewRoles.Helpers;
using SuperNewRoles.Mode.SuperHostRoles;
using Unity.Services.Authentication.Internal;
using UnityEngine;

namespace SuperNewRoles.Roles.Neutral;

public static class Moira
{
    private const int OptionId = 1207;
    public static CustomRoleOption MoiraOption;
    public static CustomOption MoiraPlayerCount;
    public static CustomOption MoiraWinLimit;
    public static CustomOption MoiraChangeVote;
    public static void SetupCustomOptions()
    {
        MoiraOption = CustomOption.SetupCustomRoleOption(OptionId, false, RoleId.Moira);
        MoiraPlayerCount = CustomOption.Create(OptionId + 1, false, CustomOptionType.Neutral, "SettingPlayerCountName", CustomOptionHolder.CrewPlayers[0], CustomOptionHolder.CrewPlayers[1], CustomOptionHolder.CrewPlayers[2], CustomOptionHolder.CrewPlayers[3], MoiraOption);
        MoiraWinLimit = CustomOption.Create(OptionId + 2, false, CustomOptionType.Neutral, "MoiraWinLimit", 5f, 1f, 10f, 1f, MoiraOption);
        MoiraChangeVote = CustomOption.Create(OptionId + 3, false, CustomOptionType.Neutral, "MoiraChangeVote", true, MoiraOption);
    }
    
    public static List<PlayerControl> MoiraPlayer;
    public static Color32 color = RoleClass.ImpostorRed;
    public static byte WinLimit;
    public static List<byte> AbilityUsedPlayers;
    public static List<byte> AbilityUsedWrapUpSetPlayers;
    public static Dictionary<byte, List<(byte, byte)>> ChangeData;
    public static Dictionary<byte, (byte, byte)> SwapVoteData;
    public static bool AbilityUsedThisMeeting;
    public static void ClearAndReload()
    {
        MoiraPlayer = new();
        WinLimit = (byte)MoiraWinLimit.GetFloat();
        AbilityUsedPlayers = new();
        AbilityUsedWrapUpSetPlayers = new();
        ChangeData = new();
        SwapVoteData = new();
        AbilityUsedThisMeeting = false;
    }

    public static void MeetingUpdateButtons(MeetingHud __instance)
    {
        __instance.playerStates.ToList().ForEach(x => { if (x.transform.FindChild("MoiraButton") != null) Object.Destroy(x.transform.FindChild("MoiraButton").gameObject); });
    }

    public static void OnClickButton(PlayerControl target, MeetingHud __instance)
    {
        if (Selected is null)
        {
            Selected = target;
            __instance.playerStates.ToList().ForEach(x => { if (x.transform.FindChild("MoiraButton") != null && x.TargetPlayerId == target.PlayerId) x.transform.FindChild("MoiraButton").gameObject.SetActive(false); });
            return;
        }
        UseAbility(Selected, target);
        Selected = null;
        MeetingUpdateButtons(__instance);
    }

    static PlayerControl Selected;

    public static void StartMeeting(MeetingHud __instance)
    {
        Selected = null;
        if (WinLimit > 0)
        {
            for (int i = 0; i < __instance.playerStates.Length; i++)
            {
                PlayerVoteArea playerVoteArea = __instance.playerStates[i];
                var player = ModHelpers.PlayerById(__instance.playerStates[i].TargetPlayerId);
                if (player.IsAlive() && player.PlayerId != CachedPlayer.LocalPlayer.PlayerId)
                {
                    GameObject template = playerVoteArea.Buttons.transform.Find("CancelButton").gameObject;
                    GameObject targetBox = Object.Instantiate(template, playerVoteArea.transform);
                    targetBox.name = "MoiraButton";
                    targetBox.transform.localPosition = new Vector3(1f, 0.03f, -1f);
                    SpriteRenderer renderer = targetBox.GetComponent<SpriteRenderer>();
                    renderer.sprite = RoleClass.SoothSayer.GetButtonSprite();
                    PassiveButton button = targetBox.GetComponent<PassiveButton>();
                    button.OnClick.RemoveAllListeners();
                    int copiedIndex = i;
                    button.OnClick.AddListener((UnityEngine.Events.UnityAction)(() => OnClickButton(player, __instance)));
                }
            }
        }
    }

    public static void PopulateVotes(MeetingHud __instance)
    {
        foreach (var data in SwapVoteData)
        {
            PlayerVoteArea swapped1 = null;
            PlayerVoteArea swapped2 = null;
            foreach (PlayerVoteArea playerVoteArea in __instance.playerStates)
            {
                if (playerVoteArea.TargetPlayerId == data.Value.Item1) swapped1 = playerVoteArea;
                if (playerVoteArea.TargetPlayerId == data.Value.Item2) swapped2 = playerVoteArea;
            }
            PlayerControl source = ModHelpers.PlayerById(data.Key);
            bool doSwap = swapped1 != null && swapped2 != null && source.IsRole(RoleId.Moira) && source.IsAlive();
            if (doSwap)
            {
                __instance.StartCoroutine(Effects.Slide3D(swapped1.transform, swapped1.transform.localPosition, swapped2.transform.localPosition, 1.5f));
                __instance.StartCoroutine(Effects.Slide3D(swapped2.transform, swapped2.transform.localPosition, swapped1.transform.localPosition, 1.5f));
            }
        }
    }

    public static void UseAbility(PlayerControl player1, PlayerControl player2)
    {
        if (AbilityUsedThisMeeting) return;
        if (WinLimit <= 0) return;
        WinLimit--;
        RpcChangeRole(player1, player2, WinLimit <= 0);
        AbilityUsedThisMeeting = true;
    }

    public static void RpcChangeRole(PlayerControl player1, PlayerControl player2, bool IsUseEnd)
    {
        MessageWriter writer = RPCHelper.StartRPC(CustomRPC.MoiraChangeRole);
        writer.Write(PlayerControl.LocalPlayer.PlayerId);
        writer.Write(player1.PlayerId);
        writer.Write(player2.PlayerId);
        writer.Write(IsUseEnd);
        writer.EndRPC();
        RPCProcedure.MoiraChangeRole(PlayerControl.LocalPlayer.PlayerId, player1.PlayerId, player2.PlayerId, IsUseEnd);
    }
    public static void ChangeRole(byte source, byte player1Id, byte player2Id, bool IsUseEnd)
    {
        if (!ChangeData.ContainsKey(source)) ChangeData.Add(source, new());
        if (!SwapVoteData.ContainsKey(source) && !SwapVoteData.ContainsValue((player1Id, player2Id)) && !SwapVoteData.ContainsValue((player2Id, player1Id))) SwapVoteData.Add(source, (player1Id, player2Id));
        ChangeData[source].Add((player1Id, player2Id));
        if (IsUseEnd) AbilityUsedWrapUpSetPlayers.Add(source);
    }
    public static void SwapRole(byte player1Id, byte player2Id)
    {
        PlayerControl player1 = ModHelpers.PlayerById(player1Id);
        PlayerControl player2 = ModHelpers.PlayerById(player2Id);
        if (player1 is null || player2 is null) return;
        RoleId player1Role = player1.GetRole();
        RoleId player2Role = player2.GetRole();
        RoleTypes player1RoleType = player1.Data.Role.Role;
        RoleTypes player2RoleType = player2.Data.Role.Role;
        player1.SetRoleRPC(player2Role);
        player1.RPCSetRoleUnchecked(player2RoleType);
        player2.SetRoleRPC(player1Role);
        player2.RPCSetRoleUnchecked(player1RoleType);
    }
    public static void WrapUp(GameData.PlayerInfo exiled)
    {
        foreach (byte pid in AbilityUsedWrapUpSetPlayers)
        {
            AbilityUsedPlayers.Add(pid);
        }
        AbilityUsedWrapUpSetPlayers = new();
        SwapVoteData = new();
        if (!AmongUsClient.Instance.AmHost) return;
        foreach (var data in SwapVoteData)
        {
            SwapRole(data.Value.Item1, data.Value.Item2);
        }
        if (exiled is null) return;
        if (exiled.Object.IsRole(RoleId.Moira))
            if (ChangeData.ContainsKey(exiled.PlayerId))
                foreach (var data in ChangeData[exiled.PlayerId])
                    SwapRole(data.Item1, data.Item2);
    }
}
