using System.Collections.Generic;
using Hazel;
using SuperNewRoles.Helpers;
using UnityEngine;

namespace SuperNewRoles.Roles.Crewmate;

public static class WiseMan
{
    private const int OptionId = 400500;
    public static CustomRoleOption WiseManOption;
    public static CustomOption WiseManPlayerCount;
    public static CustomOption WiseManCoolTime;
    public static CustomOption WiseManDurationTime;
    public static void SetupCustomOptions()
    {
        WiseManOption = CustomOption.SetupCustomRoleOption(OptionId, false, RoleId.WiseMan);
        WiseManPlayerCount = CustomOption.Create(OptionId + 1, false, CustomOptionType.Crewmate, "SettingPlayerCountName", CustomOptionHolder.CrewPlayers[0], CustomOptionHolder.CrewPlayers[1], CustomOptionHolder.CrewPlayers[2], CustomOptionHolder.CrewPlayers[3], WiseManOption);
        WiseManCoolTime = CustomOption.Create(OptionId + 2, false, CustomOptionType.Crewmate, "NiceScientistCooldownSetting", 30f, 2.5f, 60f, 2.5f, WiseManOption, format: "unitSeconds");
        WiseManDurationTime = CustomOption.Create(OptionId + 3, false, CustomOptionType.Crewmate, "NiceScientistDurationSetting", 10f, 2.5f, 30f, 2.5f, WiseManOption, format: "unitSeconds");
    }
    public static float GetRandomAngle
    {
        get
        {
            return ModHelpers.GetRandom(new List<float>() { 135, 90, 270, 225 });
        }
    }
    public static List<PlayerControl> WiseManPlayer;
    public static Color32 color = new(85, 180, 236, byte.MaxValue);
    public static Dictionary<byte, float?> WiseManData;
    public static Dictionary<PlayerControl, Vector3?> WiseManPosData;
    public static void ClearAndReload()
    {
        WiseManPlayer = new();
        WiseManData = new();
        WiseManPosData = new();
    }

    public static Sprite GetButtonSprite() => ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.WiseManButton.png", 115f);

    public static void FixedUpdate()
    {
        foreach (var data in WiseManPosData)
        {
            if (data.Key is null) continue;
            if (!data.Value.HasValue) continue;
            data.Key.transform.position = data.Value.Value;
        }
    }

    public static void RpcSetWiseManStatus(float rotate, bool Is)
    {
        MessageWriter writer = RPCHelper.StartRPC(CustomRPC.SetWiseManStatus);
        writer.Write(PlayerControl.LocalPlayer.PlayerId);
        writer.Write(rotate);
        writer.Write(Is);
        writer.EndRPC();
        RPCProcedure.SetWiseManStatus(PlayerControl.LocalPlayer.PlayerId, rotate, Is);
    }

    public static void SetWiseManStatus(PlayerControl source, float rotate, bool Is)
    {
        WiseManData[source.PlayerId] = Is ? rotate : null;
        WiseManPosData[source] = Is ? source.transform.position : null;
    }
    public static void StartMeeting()
    {
        RpcSetWiseManStatus(0, false);
        Camera.main.GetComponent<FollowerCamera>().Locked = false;
    }
    public static void OnChangeRole()
    {
        RpcSetWiseManStatus(0, false);
        Camera.main.GetComponent<FollowerCamera>().Locked = false;
    }

    // ここにコードを書きこんでください
}