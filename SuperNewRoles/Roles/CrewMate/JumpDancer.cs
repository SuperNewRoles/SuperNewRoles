using System.Collections.Generic;
using System.Linq;
using Hazel;
using SuperNewRoles.Buttons;
using SuperNewRoles.Helpers;
using UnityEngine;

namespace SuperNewRoles.Roles.Crewmate;

public static class JumpDancer
{
    private const int OptionId = 406300;
    public static CustomRoleOption JumpDancerOption;
    public static CustomOption JumpDancerPlayerCount;
    public static CustomOption JumpDancerCoolTime;
    public static void SetupCustomOptions()
    {
        JumpDancerOption = CustomOption.SetupCustomRoleOption(OptionId, false, RoleId.JumpDancer);
        JumpDancerPlayerCount = CustomOption.Create(OptionId + 1, false, CustomOptionType.Crewmate, "SettingPlayerCountName", CustomOptionHolder.CrewPlayers[0], CustomOptionHolder.CrewPlayers[1], CustomOptionHolder.CrewPlayers[2], CustomOptionHolder.CrewPlayers[3], JumpDancerOption);
        JumpDancerCoolTime = CustomOption.Create(OptionId + 2, false, CustomOptionType.Crewmate, "NiceScientistCooldownSetting", 0f, 0f, 60f, 2.5f, JumpDancerOption);
    }

    public static List<PlayerControl> JumpDancerPlayer;
    public static Color32 color = new(175, 225, 214, byte.MaxValue);
    public static CustomButton JumpDancerButton;
    public static Dictionary<byte, float> JumpingPlayerIds;
    public static List<PlayerControl> JumpingPlayers
    {
        get
        {
            if (_jumpingPlayers.Count != JumpingPlayerIds.Count)
            {
                List<PlayerControl> newplayers = new();
                foreach (byte pid in JumpingPlayerIds.Keys)
                {
                    newplayers.Add(ModHelpers.PlayerById(pid));
                }
                _jumpingPlayers = newplayers;
            }
            return _jumpingPlayers;
        }
    }
    private static List<PlayerControl> _jumpingPlayers;

    public static void ClearAndReload()
    {
        JumpDancerPlayer = new();
        JumpingPlayerIds = new();
        _jumpingPlayers = new();
    }
    //1秒
    public static void FixedUpdate()
    {
        if (JumpingPlayerIds.Count <= 0)
            return;
        foreach (var data in JumpingPlayerIds.ToArray())
        {
            PlayerControl player = ModHelpers.PlayerById(data.Key);
            if (data.Value > 0.9f || player.inMovingPlat || player.onLadder)
            {
                player.transform.localScale = new(0.7f, 0.7f, 1);
                player.moveable = true;
                if (player.IsAlive()) player.Collider.enabled = true;
                JumpingPlayerIds.Remove(data.Key);
                continue;
            }
            if (data.Value <= 0.1f)
            {
                player.transform.localScale -= new Vector3(0, Time.fixedDeltaTime, 0);
            }
            else if (data.Value <= 0.3f)
            {
                player.transform.localScale += new Vector3(0, Time.fixedDeltaTime, 0);
                player.transform.position += new Vector3(0, Time.fixedDeltaTime * 2, 0);
            }
            else if (data.Value <= 0.5f)
            {
                player.transform.localScale -= new Vector3(0, Time.fixedDeltaTime * 0.5f, 0);
                player.transform.position -= new Vector3(0, Time.fixedDeltaTime * 2, 0);
            }
            else if (data.Value <= 0.7f)
            {
                player.transform.localScale += new Vector3(0, Time.fixedDeltaTime * 0.5f, 0);
                player.transform.position += new Vector3(0, Time.fixedDeltaTime, 0);
            }
            else if (data.Value <= 0.9f)
            {
                player.transform.localScale -= new Vector3(0, Time.fixedDeltaTime * 0.5f, 0);
                player.transform.position -= new Vector3(0, Time.fixedDeltaTime, 0);
            }
            JumpingPlayerIds[data.Key] += Time.fixedDeltaTime;
        }
    }
    public static void SetJump(PlayerControl source, List<PlayerControl> players)
    {
        foreach (PlayerControl player in players)
        {
            if (JumpingPlayerIds.ContainsKey(player.PlayerId) || player.inMovingPlat || player.onLadder)
                continue;
            if (player.PlayerId == PlayerControl.LocalPlayer.PlayerId)
            {
                SoundManager.Instance.PlaySound(ModHelpers.loadAudioClipFromResources("SuperNewRoles.Resources.JumpDancerSe" + (ModHelpers.IsSuccessChance(5) ? "1" : "2") + ".raw"), false, audioMixer: SoundManager.Instance.SfxChannel);
            }
            JumpingPlayerIds.Add(player.PlayerId, 0f);
            player.moveable = false;
            player.MyPhysics.Animations.PlayIdleAnimation();
            player.Collider.enabled = false;
        }
    }
    public static void RpcJump(PlayerControl source, List<PlayerControl> players)
    {
        MessageWriter writer = RPCHelper.StartRPC(CustomRPC.JumpDancerJump);
        writer.Write(source.PlayerId);
        writer.Write(players.Count);
        foreach (PlayerControl player in players)
        {
            writer.Write(player.PlayerId);
        }
        writer.EndRPC();
        SetJump(source, players);
    }
    static bool CheckCan(PlayerControl player)
    {
        return !player.CanMove || player.inMovingPlat || player.onLadder || JumpingPlayerIds.ContainsKey(player.PlayerId) || !(
                    player.MyPhysics.Animations.IsPlayingRunAnimation() ||
                    player.MyPhysics.Animations.IsPlayingGhostIdleAnimation() ||
                    player.MyPhysics.Animations.IsPlayingGuardianAngelIdleAnimation() ||
                    player.MyPhysics.Animations.Animator.GetCurrentAnimation() == player.MyPhysics.Animations.group.IdleAnim);
    }
    public static void SetUpCustomButtons(HudManager __instance)
    {
        JumpDancerButton = new(
            () =>
            {
                List<PlayerControl> players = new();
                Vector2 localpos = PlayerControl.LocalPlayer.GetTruePosition();
                float LightRadius = ShipStatus.Instance.CalculateLightRadius(PlayerControl.LocalPlayer.Data);
                foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                {
                    if (CheckCan(player)) continue;
                    if (PlayerControl.LocalPlayer.PlayerId == player.PlayerId)
                    {
                        players.Add(player);
                    }
                    else if (Vector2.Distance(player.GetTruePosition(), localpos) <= LightRadius)
                    {
                        players.Add(player);
                    }
                }
                RpcJump(PlayerControl.LocalPlayer, players);
                JumpDancerButton.MaxTimer = JumpDancerCoolTime.GetFloat();
                JumpDancerButton.Timer = JumpDancerButton.MaxTimer;
            },
            (bool isAlive, RoleId role) => { return isAlive && role == RoleId.JumpDancer; },
            () =>
            {
                return !CheckCan(PlayerControl.LocalPlayer);
            },
            () =>
            {
                JumpDancerButton.MaxTimer = JumpDancerCoolTime.GetFloat();
                JumpDancerButton.Timer = JumpDancerButton.MaxTimer;
            },
            ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.JumpDancerButton.png", 115f),
            new(0, 1, 0),
            __instance,
            __instance.AbilityButton,
            KeyCode.F,
            49,
            () => { return false; }
        )
        {
            buttonText = ModTranslation.GetString("JumpDancerButtonName"),
            showButtonText = true
        };
    }
}