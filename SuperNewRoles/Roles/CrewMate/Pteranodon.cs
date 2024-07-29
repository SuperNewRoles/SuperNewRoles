using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Hazel;
using SuperNewRoles.Helpers;
using UnityEngine;
using static Il2CppSystem.Uri;
using static UnityEngine.GraphicsBuffer;

namespace SuperNewRoles.Roles.Crewmate;

public static class Pteranodon
{
    private const int OptionId = 406000;
    public static CustomRoleOption PteranodonOption;
    public static CustomOption PteranodonPlayerCount;
    public static CustomOption PteranodonCoolTime;
    public static void SetupCustomOptions()
    {
        PteranodonOption = CustomOption.SetupCustomRoleOption(OptionId, false, RoleId.Pteranodon);
        PteranodonPlayerCount = CustomOption.Create(OptionId + 1, false, CustomOptionType.Crewmate, "SettingPlayerCountName", CustomOptionHolder.CrewPlayers[0], CustomOptionHolder.CrewPlayers[1], CustomOptionHolder.CrewPlayers[2], CustomOptionHolder.CrewPlayers[3], PteranodonOption);
        PteranodonCoolTime = CustomOption.Create(OptionId + 2, false, CustomOptionType.Crewmate, "NiceScientistCooldownSetting", 30f, 2.5f, 60f, 2.5f, PteranodonOption);
    }

    public static List<PlayerControl> PteranodonPlayer;
    public static Color32 color = new(17, 128, 45, byte.MaxValue);
    public static bool IsPteranodonNow;
    public static Vector2 StartPosition;
    public static Vector2 TargetPosition;
    public static Vector2 CurrentPosition;
    public static float Timer;
    public static Dictionary<byte, (float, float, Vector2)> UsingPlayers;
    public const float StartTime = 2f;
    public static FollowerCamera FCamera
    {
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
        UsingPlayers = new();
    }
    public static void FixedUpdateAll()
    {
        foreach (var data in UsingPlayers.ToArray())
        {
            PlayerControl target = ModHelpers.PlayerById(data.Key);
            if (target.IsDead())
            {
                UsingPlayers.Remove(data.Key);
                continue;
            }
            float NewTimer = data.Value.Item2 - Time.fixedDeltaTime;
            Vector3 pos = data.Value.Item3;
            target.MyPhysics.Animations.PlayIdleAnimation();
            float tarpos = data.Value.Item1;
            if ((NewTimer + 0.025f) > (StartTime / 2f))
            {
                pos.y += (((NewTimer + 0.025f) - (StartTime / 2)) * 4f) * Time.fixedDeltaTime;
            }
            else
            {
                pos.y -= (((StartTime / 2) - (NewTimer + 0.025f)) * 4f) * Time.fixedDeltaTime;
            }
            pos.x += tarpos * Time.fixedDeltaTime * 0.5f;
            if (NewTimer <= 0)
            {
                UsingPlayers.Remove(data.Key);
            }
            target.NetTransform.SnapTo(pos);
            UsingPlayers[data.Key] = (data.Value.Item1, NewTimer, pos);
        }
    }
    [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.HandleAnimation))]
    private static class PlayerPhysicsHandleAnimationPatch
    {
        private static void Postfix(PlayerPhysics __instance)
        {
            if ((__instance.myPlayer.PlayerId == PlayerControl.LocalPlayer.PlayerId && IsPteranodonNow) ||
                UsingPlayers.ContainsKey(__instance.myPlayer.PlayerId))
            {
                __instance.GetSkin().SetIdle(__instance.FlipX);
            }
        }
    }
    public static void FixedUpdate()
    {
        if (IsPteranodonNow)
        {
            Timer -= Time.fixedDeltaTime;
            Vector3 pos = CurrentPosition;
            PlayerControl.LocalPlayer.MyPhysics.Animations.PlayIdleAnimation();
            float tarpos = (TargetPosition.x - StartPosition.x);
            if ((Timer + 0.025f) > (StartTime / 2f))
            {
                pos.y += (((Timer + 0.025f) - (StartTime / 2)) * 4f) * Time.fixedDeltaTime;
            }
            else
            {
                pos.y -= (((StartTime / 2) - (Timer + 0.025f)) * 4f) * Time.fixedDeltaTime;
            }
            pos.x += tarpos * Time.fixedDeltaTime * 0.5f;
            if (Timer <= 0)
            {
                IsPteranodonNow = false;
                PlayerControl.LocalPlayer.Collider.enabled = true;
                PlayerControl.LocalPlayer.moveable = true;

                Vector3 position = PlayerControl.LocalPlayer.transform.position;
                byte[] buff = new byte[sizeof(float) * 3];
                Buffer.BlockCopy(BitConverter.GetBytes(position.x), 0, buff, 0 * sizeof(float), sizeof(float));
                Buffer.BlockCopy(BitConverter.GetBytes(position.y), 0, buff, 1 * sizeof(float), sizeof(float));
                Buffer.BlockCopy(BitConverter.GetBytes(position.z), 0, buff, 2 * sizeof(float), sizeof(float));

                MessageWriter writer = RPCHelper.StartRPC(CustomRPC.PteranodonSetStatus);
                writer.Write(PlayerControl.LocalPlayer.PlayerId);
                writer.Write(false);
                writer.Write(false);
                writer.Write(0f);
                writer.Write(buff.Length);
                writer.Write(buff);
                writer.EndRPC();
            }
            PlayerControl.LocalPlayer.NetTransform.SnapTo(pos);
            CurrentPosition = pos;
            FCamera.transform.position = Vector3.Lerp(FCamera.centerPosition, (Vector2)FCamera.Target.transform.position + FCamera.Offset, 5f * Time.deltaTime);
        }
    }
    public static void WrapUp()
    {
        IsPteranodonNow = false;
        PlayerControl.LocalPlayer.Collider.enabled = true;
    }
    public static void SetStatus(PlayerControl player, bool Status, bool IsRight, float tarpos, Vector3 pos)
    {
        Logger.Info($"SetStatus:{Status}");
        if (Status)
        {
            AirshipStatus status = ShipStatus.Instance.TryCast<AirshipStatus>();
            if (status == null)
                return;
            Vector3 TargetPosition;
            if (IsRight)
                TargetPosition = status.GapPlatform.transform.parent.TransformPoint(status.GapPlatform.LeftUsePosition);
            else
                TargetPosition = status.GapPlatform.transform.parent.TransformPoint(status.GapPlatform.RightUsePosition);
            UsingPlayers.Add(player.PlayerId, (tarpos, StartTime, pos));
            player.NetTransform.enabled = false;
            player.Collider.enabled = false;
            player.moveable = false;
        }
        else
        {
            UsingPlayers.Remove(player.PlayerId);
            player.NetTransform.enabled = true;
            player.Collider.enabled = true;
            player.transform.position = pos;
            player.moveable = true;
        }
    }
    // ここにコードを書きこんでください
}