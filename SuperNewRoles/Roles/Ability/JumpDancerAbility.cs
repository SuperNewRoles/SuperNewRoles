using System;
using System.Collections.Generic;
using System.Linq;
using Hazel;
using SuperNewRoles.Events;
using SuperNewRoles.Modules;
using SuperNewRoles.Modules.Events.Bases;
using SuperNewRoles.Roles.Ability.CustomButton;
using UnityEngine;

namespace SuperNewRoles.Roles.Ability;

public class JumpDancerAbility : CustomButtonBase, IAbilityCount
{
    public Dictionary<byte, float> JumpingPlayerIds = new();
    private List<PlayerControl> _jumpingPlayers = new();
    public List<PlayerControl> JumpingPlayers
    {
        get
        {
            if (_jumpingPlayers.Count != JumpingPlayerIds.Count)
            {
                List<PlayerControl> newplayers = new();
                foreach (byte pid in JumpingPlayerIds.Keys)
                {
                    PlayerControl player = GameData.Instance.GetPlayerById(pid)?.Object;
                    if (player != null)
                        newplayers.Add(player);
                }
                _jumpingPlayers = newplayers;
            }
            return _jumpingPlayers;
        }
    }
    private readonly float cooldown;
    public override string buttonText => ModTranslation.GetString("JumpDancerButtonName");

    public override float DefaultTimer => cooldown;

    public override Sprite Sprite => AssetManager.GetAsset<Sprite>("JumpDancerButton.png");

    protected override KeyType keytype => KeyType.Ability1;

    public JumpDancerAbility(float cooldown) : base()
    {
        this.cooldown = cooldown;
    }

    public override bool CheckIsAvailable()
    {
        return !CheckCan(PlayerControl.LocalPlayer);
    }

    bool CheckCan(PlayerControl player)
    {
        return !player.CanMove || player.inMovingPlat || player.onLadder || JumpingPlayerIds.ContainsKey(player.PlayerId) || !(
                player.MyPhysics.Animations.IsPlayingRunAnimation() ||
                player.MyPhysics.Animations.IsPlayingGhostIdleAnimation() ||
                player.MyPhysics.Animations.IsPlayingGuardianAngelIdleAnimation() ||
                player.MyPhysics.Animations.Animator.GetCurrentAnimation() == player.MyPhysics.Animations.group.IdleAnim);
    }

    public override void OnClick()
    {
        if (!CheckIsAvailable()) return;

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

        // RPC送信ロジックはカスタムRPCの代わりに直接効果を適用する
        RpcSetJump(this, players.ToArray());
    }
    [CustomRPC]
    public static void RpcSetJump(JumpDancerAbility source, PlayerControl[] players)
    {
        foreach (PlayerControl player in players)
        {
            if (source.JumpingPlayerIds.ContainsKey(player.PlayerId) || player.inMovingPlat || player.onLadder)
                continue;
            if (player.PlayerId == PlayerControl.LocalPlayer.PlayerId)
            {
                SoundManager.Instance.PlaySound(AssetManager.GetAsset<AudioClip>(ModHelpers.GetRandomFormat("JumpDancerSe{0}.wav", 1, 2)), false, 1f);
            }
            source.JumpingPlayerIds.Add(player.PlayerId, 0f);
            player.moveable = false;
            player.MyPhysics.Animations.PlayIdleAnimation();
            player.MyPhysics.SetNormalizedVelocity(Vector2.zero);
            player.Collider.enabled = false;
        }
    }

    public void FixedUpdate()
    {
        if (JumpingPlayerIds.Count <= 0)
            return;
        foreach (var data in JumpingPlayerIds.ToArray())
        {
            ExPlayerControl player = ExPlayerControl.ById(data.Key);
            if (player == null) continue;

            if (data.Value > 0.9f || player.Player.inMovingPlat || player.Player.onLadder)
            {
                player.transform.localScale = new(0.7f, 0.7f, 1);
                player.Player.moveable = true;
                if (player.IsAlive()) player.Player.Collider.enabled = true;
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

    private EventListener fixedUpdateEventListener;

    public override void AttachToAlls()
    {
        fixedUpdateEventListener = FixedUpdateEvent.Instance.AddListener(FixedUpdate);
    }

    public override void DetachToAlls()
    {
        fixedUpdateEventListener?.RemoveListener();
    }
}