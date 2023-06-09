using System.Collections.Generic;
using UnityEngine;

namespace SuperNewRoles.Roles.Crewmate;

public static class Pteranodon
{
    private const int OptionId = 1271;
    public static CustomRoleOption PteranodonOption;
    public static CustomOption PteranodonPlayerCount;
    public static void SetupCustomOptions()
    {
        PteranodonOption = CustomOption.SetupCustomRoleOption(OptionId, false, RoleId.Pteranodon);
        PteranodonPlayerCount = CustomOption.Create(OptionId + 1, false, CustomOptionType.Crewmate, "SettingPlayerCountName", CustomOptionHolder.CrewPlayers[0], CustomOptionHolder.CrewPlayers[1], CustomOptionHolder.CrewPlayers[2], CustomOptionHolder.CrewPlayers[3], PteranodonOption);
    }
    
    public static List<PlayerControl> PteranodonPlayer;
    public static Color32 color = new Color32(0, 255, 255, byte.MaxValue);
    public static bool IsPteranodonNow;
    public static Vector2 StartPosition;
    public static Vector2 TargetPosition;
    public static float Timer;
    public const float StartTime = 2f;
    public static FollowerCamera FCamera {
        get
        {
            if (_camera == null)
                _camera = Camera.main.GetComponent<FollowerCamera>();
            return _camera;
        }
    }
    public static FollowerCamera _camera;
    public static void ClearAndReload()
    {
        PteranodonPlayer = new();
        IsPteranodonNow = false;
        StartPosition = new();
        TargetPosition = new();
        Timer = 0;
    }
    public static void FixedUpdate()
    {
        if (IsPteranodonNow)
        {
            Timer -= Time.fixedDeltaTime;
            Vector3 pos = PlayerControl.LocalPlayer.transform.position;
            float tarpos = (TargetPosition.x - StartPosition.x);
            Logger.Info(tarpos.ToString());
            if (Timer > (StartTime / 2f))
            {
                pos.y += ((Timer - (StartTime / 2)) * 4) * Time.fixedDeltaTime;
            }
            else
            {
                pos.y -= (((StartTime / 2) - Timer) * 4f) * Time.fixedDeltaTime;
            }
            pos.x += tarpos * Time.fixedDeltaTime * 0.5f;
            if (Timer <= 0)
            {
                IsPteranodonNow = false;
                PlayerControl.LocalPlayer.Collider.enabled = true;
                PlayerControl.LocalPlayer.moveable = true;
            }
            PlayerControl.LocalPlayer.transform.position = pos;
            FCamera.Update();
        }
    }
    public static void WrapUp()
    {
        IsPteranodonNow = false;
        PlayerControl.LocalPlayer.Collider.enabled = false;
    }
    // ここにコードを書きこんでください
}