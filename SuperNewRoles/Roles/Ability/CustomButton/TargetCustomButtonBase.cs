using System;
using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Neutral;
using UnityEngine;

namespace SuperNewRoles.Roles.Ability.CustomButton;

public abstract class TargetCustomButtonBase : CustomButtonBase
{
    public ExPlayerControl Target { get; protected set; }
    public virtual bool ShowOutline { get; protected set; } = true;
    public abstract Color32 OutlineColor { get; }
    private PlayerControl _lastShowTarget;
    private static readonly Dictionary<PlayerControl, HashSet<TargetCustomButtonBase>> OutlineOwners = new();
    public abstract bool OnlyCrewmates { get; }
    public virtual bool TargetPlayersInVents { get; } = false;
    public virtual IEnumerable<PlayerControl> UntargetablePlayers { get; } = null;
    public virtual PlayerControl TargetingPlayer => PlayerControl.LocalPlayer;
    public virtual Func<ExPlayerControl, bool>? IsTargetable { get; } = null;
    public virtual Func<bool> IsDeadPlayerOnly { get; } = null;
    public bool TargetIsExist => Target != null;
    public virtual bool IgnoreWalls => false;
    protected virtual bool CanTargetFrankensteinBody => false;
    public override void OnUpdate()
    {
        // Refresh the target before CustomButtonBase handles keyboard input.
        // Otherwise a target that died/disconnected between HUD updates can consume the click.
        bool canUpdateTarget = PlayerControl.LocalPlayer?.Data != null &&
            MeetingHud.Instance == null &&
            ExileController.Instance == null &&
            CheckHasButton();
        if (!canUpdateTarget)
        {
            ClearTarget();
            base.OnUpdate();
            return;
        }

        bool hudActive = HudManager.Instance != null &&
            (HudManager.Instance.UseButton.isActiveAndEnabled || HudManager.Instance.PetButton.isActiveAndEnabled);
        if (!hudActive)
        {
            ClearTarget();
            base.OnUpdate();
            return;
        }

        Target = SetTarget(onlyCrewmates: OnlyCrewmates, targetPlayersInVents: TargetPlayersInVents, untargetablePlayers: UntargetablePlayers, targetingPlayer: TargetingPlayer, isTargetable: IsTargetable, isDeadPlayerOnly: IsDeadPlayerOnly, ignoreWalls: IgnoreWalls);
        UpdateTargetOutline();
        base.OnUpdate();
    }

    public override void DetachToLocalPlayer()
    {
        ClearTarget();
        base.DetachToLocalPlayer();
    }

    private void UpdateTargetOutline()
    {
        if (!ShowOutline)
        {
            ClearTargetOutline();
            return;
        }

        if (_lastShowTarget == Target) return;
        ClearTargetOutline();
        if (Target != null)
        {
            if (!OutlineOwners.TryGetValue(Target, out var owners))
            {
                owners = new HashSet<TargetCustomButtonBase>();
                OutlineOwners[Target] = owners;
            }
            owners.Add(this);
            SetOutline(Target, true, OutlineColor);
        }
        _lastShowTarget = Target;
    }

    private void ClearTarget()
    {
        ClearTargetOutline();
        Target = null;
    }

    private void ClearTargetOutline()
    {
        PlayerControl previousTarget = _lastShowTarget;
        _lastShowTarget = null;
        if (previousTarget == null) return;

        if (!OutlineOwners.TryGetValue(previousTarget, out var owners))
        {
            SetOutline(previousTarget, false, default);
            return;
        }

        owners.Remove(this);
        if (owners.Count == 0)
        {
            OutlineOwners.Remove(previousTarget);
            SetOutline(previousTarget, false, default);
            return;
        }

        TargetCustomButtonBase remainingOwner = owners.First();
        SetOutline(previousTarget, true, remainingOwner.OutlineColor);
    }

    internal static void ClearOutlineOwners()
    {
        foreach (PlayerControl target in OutlineOwners.Keys.ToArray())
            SetOutline(target, false, default);
        OutlineOwners.Clear();
    }

    internal bool ShouldCancelClickForTarget()
    {
        if (Target == null) return false;
        if (Target.Data == null || Target.Data.Disconnected) return true;
        if (Target.Data.IsDead && IsDeadPlayerOnly?.Invoke() != true) return true;
        if (ShouldCancelClickForTargetCore()) return true;
        if (CanTargetFrankensteinBody) return false;
        ExPlayerControl exTarget = Target;
        return exTarget != null && exTarget.TryGetAbility<FrankensteinAbility>(out var frankensteinAbility) && frankensteinAbility.IsMonster;
    }
    protected virtual bool ShouldCancelClickForTargetCore() => false;
    private static void SetOutline(PlayerControl player, bool show, Color32 color)
    {
        if (player == null || player.cosmetics == null || player.cosmetics.currentBodySprite == null) return;
        var rend = player.cosmetics.currentBodySprite.BodySprite;
        if (rend == null) return;
        rend.material.SetFloat("_Outline", show ? 1f : 0f);
        if (show)
            rend.material.SetColor("_OutlineColor", color);
    }
    public static PlayerControl SetTarget(bool onlyCrewmates = false, bool targetPlayersInVents = false, IEnumerable<PlayerControl> untargetablePlayers = null, PlayerControl targetingPlayer = null, Func<ExPlayerControl, bool> isTargetable = null, Func<bool> isDeadPlayerOnly = null, bool ignoreWalls = false)
    {
        if (!ShipStatus.Instance) return null;
        if (targetingPlayer == null) targetingPlayer = PlayerControl.LocalPlayer;
        if (targetingPlayer.inVent) return null;

        PlayerControl result = null;
        float num = GameManager.Instance.LogicOptions.GetKillDistance();
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
