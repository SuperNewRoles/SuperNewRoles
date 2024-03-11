using System.Collections.Generic;
using Hazel;
using SuperNewRoles.Helpers;
using SuperNewRoles.Patches;
using SuperNewRoles.Roles.Crewmate;
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
        public static CustomOption DarkroomTime;
        public static CustomOption ShowerTime;
        public static CustomOption DeckTime;

        public static void SetupCustomOptions()
        {
            Option = CustomOption.SetupCustomRoleOption(optionId, false, RoleId.Sauner); optionId++;
            PlayerCount = CustomOption.Create(optionId, false, CustomOptionType.Neutral, "SettingPlayerCountName", CustomOptionHolder.CrewPlayers[0], CustomOptionHolder.CrewPlayers[1], CustomOptionHolder.CrewPlayers[2], CustomOptionHolder.CrewPlayers[3], Option); optionId++;
            DarkroomTime = CustomOption.Create(optionId, false, CustomOptionType.Neutral, "SaunerDarkroomTime", 180f, 20f, 600f, 20f, Option); optionId++;
            ShowerTime = CustomOption.Create(optionId, false, CustomOptionType.Neutral, "SaunerShowerTime", 20f, 20f, 600f, 20f, Option); optionId++;
            DeckTime = CustomOption.Create(optionId, false, CustomOptionType.Neutral, "SaunerDeckTime", 60f, 20f, 600f, 20f, Option); optionId++;
        }
    }

    internal static class RoleData
    {
        public static List<PlayerControl> Player;
        public static SaunerState CurrentState;
        public static bool LastSaunerFlash;
        public static float SaunaTimer;
        public static ImportantTextTask TaskText;
        public static AudioSource Audio;
        public static Color32 color = new(219, 152, 101, byte.MaxValue);

        public static void ClearAndReload()
        {
            Player = new();
            CurrentState = SaunerState.Darkroom;
            LastSaunerFlash = false;
            SaunaTimer = CustomOptionData.DarkroomTime.GetFloat();
            TaskText = null;
            Audio = null;
        }
    }

    /// <summary>
    /// 今の座標が指定した座標内に入っているかを判定する
    /// </summary>
    /// <param name="Up">左上の座標</param>
    /// <param name="Down">右下の座標</param>
    /// <param name="pos">今の座標</param>
    /// <returns></returns>
    static bool CheckPos(Vector2 Up, Vector2 Down, Vector2 pos)
    {
        return Up.x <= pos.x && pos.x <= Down.x &&
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
                //セキュ下展望と展望デッキ下
                return CheckPos(new(-14.7f, -13.95f), new(11.1f, -17f), nowpos);
            //コックピット(ミスで追加したのでコメントアウト)
            //|| CheckPos(new(-25.2f, 1.7f), new(-16.3f, -3.9f), nowpos);
            default:
                return false;
        }
    }
    public static List<Vector2> GetSaunaPos()
    {
        switch (RoleData.CurrentState)
        {
            case SaunerState.Darkroom:
                return new() { new(12.5f, 2.2f) };
            case SaunerState.Shower:
                return new() { new(22.5f, 2.55f) };
            case SaunerState.ObservationDeck:
                return new() { new(-13.7f, -15), new(8f, -14.6f) };
            default:
                Logger.Info("GetSaunerPosで予期しない位置が入力されました：" + RoleData.CurrentState.ToString());
                return new();
        }
    }
    public static Color GetFlashColor(SaunerState state)
    {
        switch (state)
        {
            case SaunerState.Darkroom:
                return new Color32(255, 123, 99, 103);
            case SaunerState.Shower:
                return new Color32(65, 161, 255, 103);
            case SaunerState.ObservationDeck:
                return new Color32(38, 200, 94, 103);
            default:
                Logger.Info("想定していないStateが渡されました:" + state.ToString());
                return new(255, 255, 255, 255);
        }
    }

    public static string GetSaunaText()
    {
        float OptionTime = -1;
        string Text;
        switch (RoleData.CurrentState)
        {
            case SaunerState.Darkroom:
                Text = "InSauna";
                OptionTime = CustomOptionData.DarkroomTime.GetFloat();
                break;
            case SaunerState.Shower:
                Text = "Shower";
                OptionTime = CustomOptionData.ShowerTime.GetFloat();
                break;
            case SaunerState.ObservationDeck:
                Text = "AirBath";
                OptionTime = CustomOptionData.DeckTime.GetFloat();
                break;
            default:
                return $"エラー({RoleData.CurrentState})({RoleData.SaunaTimer})(334秒)";
        }
        Text = ModTranslation.GetString("SaunerText" + Text);
        if (RoleData.SaunaTimer < OptionTime)
        {
            Text += string.Format(ModTranslation.GetString("SaunerTextRemaing"), (int)(RoleData.SaunaTimer + 1));
        }
        return Text;
    }
    public static void CheckAndFlash()
    {
        if (RoleData.LastSaunerFlash && !MeetingHud.Instance)
            SeerHandler.ShowFlash(GetFlashColor(RoleData.CurrentState), 6, CheckAndFlash);
    }
    public static AudioSource GetAudio(SaunerState state)
    {
        AudioClip clip = null;
        switch (state)
        {
            case SaunerState.Darkroom:
                clip = ContentManager.GetContent<AudioClip>("Sauner_SaunaBGM.wav");
                break;
            case SaunerState.Shower:
                clip = ContentManager.GetContent<AudioClip>("Sauner_ShowerBGM.wav");
                break;
            case SaunerState.ObservationDeck:
                clip = ContentManager.GetContent<AudioClip>("Sauner_BardBGM.wav");
                break;

        }
        if (clip != null)
            return SoundManager.Instance.PlaySound(clip, true, audioMixer: SoundManager.Instance.sfxMixer);
        return null;
    }
    public static void FixedUpdate()
    {
        if (!RoleClass.IsMeeting)
        {
            bool IsRoom = CheckRoom(RoleData.CurrentState, PlayerControl.LocalPlayer.transform.position);
            if (RoleData.LastSaunerFlash != IsRoom)
            {
                RoleData.LastSaunerFlash = IsRoom;
                if (IsRoom)
                {
                    CheckAndFlash();
                    if (RoleData.Audio == null)
                        RoleData.Audio = GetAudio(RoleData.CurrentState);
                }
                else
                {
                    SeerHandler.HideFlash();
                    if (RoleData.Audio != null)
                    {
                        RoleData.Audio.Stop();
                        RoleData.Audio = null;
                    }
                }
            }
        }
        else
        {
            RoleData.LastSaunerFlash = false;
            if (RoleData.Audio != null)
            {
                RoleData.Audio.Stop();
                RoleData.Audio = null;
            }
        }
        if (RoleData.LastSaunerFlash)
        {
            RoleData.SaunaTimer -= Time.fixedDeltaTime;
            if (RoleData.SaunaTimer <= 0)
            {
                switch (RoleData.CurrentState)
                {
                    case SaunerState.Darkroom:
                        RoleData.CurrentState = SaunerState.Shower;
                        RoleData.SaunaTimer = CustomOptionData.ShowerTime.GetFloat();
                        break;
                    case SaunerState.Shower:
                        RoleData.CurrentState = SaunerState.ObservationDeck;
                        RoleData.SaunaTimer = CustomOptionData.DeckTime.GetFloat();
                        break;
                    case SaunerState.ObservationDeck:
                        MessageWriter writer1 = RPCHelper.StartRPC(CustomRPC.ShareWinner);
                        writer1.Write(PlayerControl.LocalPlayer.PlayerId);
                        writer1.EndRPC();
                        RPCProcedure.ShareWinner(PlayerControl.LocalPlayer.PlayerId);
                        if (AmongUsClient.Instance.AmHost) CheckGameEndPatch.CustomEndGame((GameOverReason)CustomGameOverReason.SaunerWin, false);
                        else
                        {
                            MessageWriter writer2 = RPCHelper.StartRPC(CustomRPC.CustomEndGame);
                            writer2.Write((byte)CustomGameOverReason.SaunerWin);
                            writer2.Write(false);
                            writer2.EndRPC();
                        }
                        break;
                }
            }
        }
        if (RoleData.TaskText == null)
        {
            var task = new GameObject("RoleTask").AddComponent<ImportantTextTask>();
            task.transform.SetParent(PlayerControl.LocalPlayer.transform, false);
            PlayerControl.LocalPlayer.myTasks.Insert(1, task);
            task.HasLocation = true;
            RoleData.TaskText = task;
        }
        Color color = GetFlashColor(RoleData.CurrentState);
        color.a = 1f;
        RoleData.TaskText.Text = "<size=0%>Sauner</size>" + ModHelpers.Cs(color, $"{ModTranslation.GetString($"Sauner{RoleData.CurrentState}")}: {GetSaunaText()}");
        int index = 0;
        foreach (PlayerTask t in PlayerControl.LocalPlayer.myTasks)
        {
            NormalPlayerTask npt = t.TryCast<NormalPlayerTask>();
            if (npt != null)
            {
                if (npt.Length != NormalPlayerTask.TaskLength.Common)
                    PlayerControl.LocalPlayer.myTasks.RemoveAt(index);
                else
                    npt.HasLocation = false;
            }
            index++;
        }
    }

    // ここにコードを書きこんでください
}