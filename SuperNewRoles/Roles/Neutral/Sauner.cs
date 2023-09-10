using System.Collections.Generic;
using UnityEngine;

namespace SuperNewRoles.Roles.Neutral;

public static class Sauner
{
    public enum SaunerState
    {
        None,
        //暗室
        Darkroom,
        //シャワー
        Shower,
        //展望デッキ
        ObservationDeck
    }
    public static class CustomOptionData
    {
        private static int optionId = 303400;
        public static CustomRoleOption Option;
        public static CustomOption PlayerCount;

        public static void SetupCustomOptions()
        {
            Option = CustomOption.SetupCustomRoleOption(optionId, false, RoleId.Sauner); optionId++;
            PlayerCount = CustomOption.Create(optionId, false, CustomOptionType.Neutral, "SettingPlayerCountName", CustomOptionHolder.CrewPlayers[0], CustomOptionHolder.CrewPlayers[1], CustomOptionHolder.CrewPlayers[2], CustomOptionHolder.CrewPlayers[3], Option); optionId++;
        }
    }

    internal static class RoleData
    {
        public static List<PlayerControl> Player;
        public static SaunerState CurrentState;
        public static Color32 color = new(219, 152, 101, byte.MaxValue);

        public static void ClearAndReload()
        {
            Player = new();
            CurrentState = new();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="Up">左上の座標</param>
    /// <param name="Down">右下の座標</param>
    /// <param name="pos">今の座標</param>
    /// <returns></returns>
    static bool CheckPos(Vector2 Up, Vector2 Down, Vector2 pos)
    {
        return Up.x >= pos.x && pos.x >= Down.x &&
               Up.y >= pos.y && pos.y >= Down.y;
    }
    public static bool CheckRoom(SaunerState room, Vector2 nowpos)
    {
        switch (room)
        {
            case SaunerState.Darkroom:
                return CheckPos(new(11.125f, 3.2f), new(13.8f, 0.9f), nowpos);
            case SaunerState.Shower:
                return CheckPos(new(20.3f, 3.5f), new(24.9f, 1.4f), nowpos);
            case SaunerState.ObservationDeck:
                       //セキュ下展望
                return CheckPos(new(5.1f, -13.95f), new(11.1f, -17f), nowpos) ||
                       //展望デッキ下
                       CheckPos(new(-14.7f, -13.8f), new(-12.5f, -17f), nowpos) ||
                       //コックピット
                       CheckPos(new(-25.2f, 1.7f), new(-16.3f, -3.9f), nowpos);
            default:
                return false;
        }
    }

    public static void FixedUpdate()
    {
        bool IsRoom = CheckRoom(RoleData.CurrentState, PlayerControl.LocalPlayer.transform.position);
        Logger.Info(RoleData.CurrentState+":"+IsRoom);
    }

    // ここにコードを書きこんでください
}