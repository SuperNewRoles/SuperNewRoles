using System;
using System.Collections.Generic;
using Hazel;
using SuperNewRoles.Buttons;
using SuperNewRoles.Helpers;
using UnityEngine;
using static Il2CppSystem.Globalization.CultureInfo;

namespace SuperNewRoles.Roles.Crewmate;

public static class NiceMechanic
{
    private const int OptionId = 403300;
    public static CustomRoleOption NiceMechanicOption;
    public static CustomOption NiceMechanicPlayerCount;
    public static CustomOption NiceMechanicCoolTime;
    public static CustomOption NiceMechanicDurationTime;
    public static CustomOption NiceMechanicUseVent;
    public static void SetupCustomOptions()
    {
        NiceMechanicOption = CustomOption.SetupCustomRoleOption(OptionId, false, RoleId.NiceMechanic);
        NiceMechanicPlayerCount = CustomOption.Create(OptionId + 1, false, CustomOptionType.Crewmate, "SettingPlayerCountName", CustomOptionHolder.CrewPlayers[0], CustomOptionHolder.CrewPlayers[1], CustomOptionHolder.CrewPlayers[2], CustomOptionHolder.CrewPlayers[3], NiceMechanicOption);
        NiceMechanicCoolTime = CustomOption.Create(OptionId + 2, false, CustomOptionType.Crewmate, "NiceScientistCooldownSetting", 30f, 2.5f, 60f, 2.5f, NiceMechanicOption);
        NiceMechanicDurationTime = CustomOption.Create(OptionId + 3, false, CustomOptionType.Crewmate, "NiceScientistDurationSetting", 10f, 2.5f, 30f, 2.5f, NiceMechanicOption, format: "unitSeconds");
        NiceMechanicUseVent = CustomOption.Create(OptionId + 4, false, CustomOptionType.Crewmate, "JackalUseVentSetting", true, NiceMechanicOption);
    }

    public static List<PlayerControl> NiceMechanicPlayer;
    public static Color32 color = new(82, 108, 173, byte.MaxValue);
    public static Dictionary<byte, Vent> TargetVent;
    public static bool IsLocalUsingNow => TargetVent.ContainsKey(PlayerControl.LocalPlayer.PlayerId) && TargetVent[PlayerControl.LocalPlayer.PlayerId] is not null;
    public static Sprite GetButtonSprite() => ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.MechanicButton_Nice.png", 115f);
    public static void ClearAndReload()
    {
        NiceMechanicPlayer = new();
        TargetVent = new();
    }

    public static void FixedUpdate()
    {
        foreach (var data in TargetVent)
        {
            if (data.Value is null) continue;
            PlayerControl player = ModHelpers.PlayerById(data.Key);
            if (player is null) continue;
            Vector2 truepos = player.GetTruePosition();
            data.Value.transform.position = new(truepos.x, truepos.y, player.transform.position.z + 0.0025f);
            SetHideStatus(player, true);
            if (Vent.currentVent is not null && Vent.currentVent.Id == data.Value.Id)
            {
                PlayerControl.LocalPlayer.transform.position = Vent.currentVent.transform.position;
            }
        }
    }
    public static void RpcSetVentStatusMechanic(PlayerControl source, Vent targetvent, bool Is, Vector3? pos = null)
    {
        Vector3 position = pos ?? new();
        byte[] buff = new byte[sizeof(float) * 3];
        Buffer.BlockCopy(BitConverter.GetBytes(position.x), 0, buff, 0 * sizeof(float), sizeof(float));
        Buffer.BlockCopy(BitConverter.GetBytes(position.y), 0, buff, 1 * sizeof(float), sizeof(float));
        Buffer.BlockCopy(BitConverter.GetBytes(position.z), 0, buff, 2 * sizeof(float), sizeof(float));
        MessageWriter writer = RPCHelper.StartRPC(CustomRPC.SetVentStatusMechanic);
        writer.Write(source.PlayerId);
        writer.Write((byte)targetvent.Id);
        writer.Write(Is);
        writer.WriteBytesAndSize(buff);
        writer.EndRPC();
        RPCProcedure.SetVentStatusMechanic(source.PlayerId, (byte)targetvent.Id, Is, buff);
    }
    public static void SetVentStatusMechanic(PlayerControl source, Vent targetvent, bool Is, Vector3 pos)
    {
        if (Is)
        {
            TargetVent[source.PlayerId] = targetvent;
            Vector2 truepos = source.GetTruePosition();
            targetvent.transform.position = new(truepos.x, truepos.y, source.transform.position.z + 0.0025f);
            SetHideStatus(source, true);
            if (Vent.currentVent is not null && Vent.currentVent.Id == targetvent.Id)
            {
                targetvent.SetButtons(false);
            }
        }
        else
        {
            TargetVent[source.PlayerId] = null;
            Vector2 truepos = source.transform.position;
            targetvent.transform.position = pos;
            SetHideStatus(source, false);
            if (Vent.currentVent is not null && Vent.currentVent.Id == targetvent.Id)
            {
                targetvent.SetButtons(true);
            }
        }
    }
    public static void ChangeRole(PlayerControl Target)
    {
        if (PlayerControl.LocalPlayer.PlayerId == Target.PlayerId && IsLocalUsingNow)
        {
            Vector3 truepos = PlayerControl.LocalPlayer.GetTruePosition();
            RpcSetVentStatusMechanic(PlayerControl.LocalPlayer, HudManagerStartPatch.SetTargetVent(forceout: true), false, new(truepos.x, truepos.y, truepos.z + 0.0025f));
        }
    }
    public static void SetHideStatus(PlayerControl Target, bool ison)
    {
        var opacity = 0f;
        if (ison)
        {
            opacity = 0f;
            Target.MyRend().material.SetFloat("_Outline", 0f);
        }
        else
        {
            opacity = 1.5f;
        }
        Attribute.InvisibleRole.SetOpacity(Target, opacity, false);
    }
    public static void StartMeeting()
    {
        foreach (var data in TargetVent)
        {
            PlayerControl player = ModHelpers.PlayerById(data.Key);
            if (player is null) return;
            SetHideStatus(player, false);
        }
        TargetVent = new();
    }
    // ここにコードを書きこんでください
}