using System.Collections.Generic;
using Hazel;
using SuperNewRoles.Helpers;
using UnityEngine;
using static Il2CppSystem.Globalization.CultureInfo;

namespace SuperNewRoles.Roles.Crewmate;

public static class NiceMechanic
{
    private const int OptionId = 1207;
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
    public static Color32 color = new Color32(82, 108, 173, byte.MaxValue);
    public static Dictionary<byte, Vent> TargetVent;
    public static void ClearAndReload()
    {
        NiceMechanicPlayer = new();
        TargetVent = new();
    }

    public static void FixedUpdate()
    {
        foreach (var data in TargetVent)
        {
            PlayerControl player = ModHelpers.PlayerById(data.Key);
            if (player is null) continue;
            Vector2 truepos = player.GetTruePosition();
            data.Value.transform.position = new(truepos.x, truepos.y, player.transform.position.z - 0.5f);
            if (Vent.currentVent is not null && Vent.currentVent.Id == data.Value.Id)
            {
                PlayerControl.LocalPlayer.transform.position = Vent.currentVent.transform.position;
            }
        }
    }
    public static void RpcSetVentStatusMechanic(PlayerControl source, Vent targetvent, bool Is)
    {
        MessageWriter writer = RPCHelper.StartRPC(CustomRPC.SetVentStatusMechanic);
        writer.Write(source.PlayerId);
        writer.Write((byte)targetvent.Id);
        writer.Write(Is);
        writer.EndRPC();
        RPCProcedure.SetVentStatusMechanic(source.PlayerId, (byte)targetvent.Id, Is);
    }
    public static void SetVentStatusMechanic(PlayerControl source, Vent targetvent, bool Is)
    {
        if (Is)
        {
            TargetVent[source.PlayerId] = targetvent;
            Vector2 truepos = source.GetTruePosition();
            targetvent.transform.position = new(truepos.x, truepos.y, source.transform.position.z - 0.5f);
            source.cosmetics.currentBodySprite.BodySprite.enabled = false;
            if (Vent.currentVent is not null && Vent.currentVent.Id == targetvent.Id)
            {
                targetvent.SetButtons(false);
            }
        }
        else
        {
            TargetVent[source.PlayerId] = null;
            Vector2 truepos = source.GetTruePosition();
            targetvent.transform.position = new(truepos.x, truepos.y, source.transform.position.z - 0.5f);
            source.cosmetics.currentBodySprite.BodySprite.enabled = true;
            if (Vent.currentVent is not null && Vent.currentVent.Id == targetvent.Id && )
            {
                targetvent.SetButtons(true);
            }
        }
    }
    //メモ:役職変更時に処理を入れる
    public static void WrapUp()
    {
        foreach (var data in TargetVent)
        {
            PlayerControl player = ModHelpers.PlayerById(data.Key);
            if (player is null) return;
            player.cosmetics.currentBodySprite.BodySprite.enabled = true;
        }
        TargetVent = new();
    }
    // ここにコードを書きこんでください
}