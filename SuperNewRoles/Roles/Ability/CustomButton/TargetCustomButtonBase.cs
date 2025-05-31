using System;
using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using SuperNewRoles.Modules;
using UnityEngine;

namespace SuperNewRoles.Roles.Ability.CustomButton;

public abstract class TargetCustomButtonBase : CustomButtonBase
{
    public ExPlayerControl Target { get; protected set; }
    public virtual bool ShowOutline { get; protected set; } = true;
    public abstract Color32 OutlineColor { get; }
    private PlayerControl _lastShowTarget;
    public abstract bool OnlyCrewmates { get; }
    public virtual bool TargetPlayersInVents { get; } = false;
    public virtual IEnumerable<PlayerControl> UntargetablePlayers { get; } = null;
    public virtual PlayerControl TargetingPlayer => PlayerControl.LocalPlayer;
    public virtual Func<ExPlayerControl, bool>? IsTargetable { get; } = null;
    public virtual Func<bool> IsDeadPlayerOnly { get; } = null;
    public bool TargetIsExist => Target != null;
    public virtual bool IgnoreWalls => false;
    public override void OnUpdate()
    {
        base.OnUpdate();
        Target = SetTarget(onlyCrewmates: OnlyCrewmates, targetPlayersInVents: TargetPlayersInVents, untargetablePlayers: UntargetablePlayers, targetingPlayer: TargetingPlayer, isTargetable: IsTargetable, isDeadPlayerOnly: IsDeadPlayerOnly, ignoreWalls: IgnoreWalls);
        if (ShowOutline && _lastShowTarget != Target)
        {
            if (_lastShowTarget != null)
                SetOutline(_lastShowTarget, false, OutlineColor);
            if (Target != null)
                SetOutline(Target, true, OutlineColor);
            _lastShowTarget = Target;
        }
    }
    private static void SetOutline(PlayerControl player, bool show, Color32 color)
    {
        var rend = player.cosmetics.currentBodySprite.BodySprite;
        if (player == null || rend == null) return;
        rend.material.SetFloat("_Outline", show ? 1f : 0f);
        if (show)
            rend.material.SetColor("_OutlineColor", color);
    }
    public static PlayerControl SetTarget(bool onlyCrewmates = false, bool targetPlayersInVents = false, IEnumerable<PlayerControl> untargetablePlayers = null, PlayerControl targetingPlayer = null, Func<ExPlayerControl, bool> isTargetable = null, Func<bool> isDeadPlayerOnly = null, bool ignoreWalls = false)
    {
        PlayerControl result = null;
        float num = GameManager.Instance.LogicOptions.GetKillDistance();
        if (!ShipStatus.Instance) return result;
        if (targetingPlayer == null) targetingPlayer = PlayerControl.LocalPlayer;
        if (targetingPlayer.inVent) return result;

        bool IsDeadPlayerOnly = isDeadPlayerOnly != null && isDeadPlayerOnly();

        Vector2 truePosition = targetingPlayer.GetTruePosition();
        Il2CppSystem.Collections.Generic.List<NetworkedPlayerInfo> allPlayers = GameData.Instance.AllPlayers;
        for (int i = 0; i < allPlayers.Count; i++)
        {
            NetworkedPlayerInfo playerInfo = allPlayers[i];
            if (playerInfo.Disconnected ||
                playerInfo.PlayerId == targetingPlayer.PlayerId ||
                (playerInfo.IsDead && !IsDeadPlayerOnly) ||
                (!playerInfo.IsDead && IsDeadPlayerOnly) ||
                (onlyCrewmates && playerInfo.Role.IsImpostor)
               )
                continue;
            if (isTargetable != null && !isTargetable(playerInfo))
                continue;
            PlayerControl @object = playerInfo.Object;
            if (untargetablePlayers != null &&
                untargetablePlayers.Any(x => x == @object))
            {
                // if that player is not targetable: skip check
                continue;
            }
            if (@object == null ||
                (@object.inVent && !targetPlayersInVents))
                continue;
            Vector2 vector = @object.GetTruePosition() - truePosition;
            float magnitude = vector.magnitude;
            if (magnitude > num ||
                (!IsDeadPlayerOnly && !ignoreWalls && PhysicsHelpers.AnyNonTriggersBetween(truePosition, vector.normalized, magnitude, Constants.ShipAndObjectsMask))
                )
                continue;
            result = @object;
            num = magnitude;
        }
        return result;
    }
}
