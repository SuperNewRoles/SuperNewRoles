using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using UnityEngine;

namespace SuperNewRoles.Roles.Ability.CustomButton;

public abstract class TargetCustomButtonBase : CustomButtonBase
{
    public PlayerControl Target { get; protected set; }
    public abstract bool OnlyCrewmates { get; }
    public virtual bool TargetPlayersInVents { get; } = false;
    public virtual IEnumerable<PlayerControl> UntargetablePlayers { get; } = null;
    public virtual PlayerControl TargetingPlayer => PlayerControl.LocalPlayer;
    public bool TargetIsExist => Target != null;
    public override void OnFixedUpdate()
    {
        base.OnFixedUpdate();
        Target = SetTarget(onlyCrewmates: OnlyCrewmates, targetPlayersInVents: TargetPlayersInVents, untargetablePlayers: UntargetablePlayers, targetingPlayer: PlayerControl.LocalPlayer);
    }
    public static PlayerControl SetTarget(bool onlyCrewmates = false, bool targetPlayersInVents = false, IEnumerable<PlayerControl> untargetablePlayers = null, PlayerControl targetingPlayer = null)
    {
        PlayerControl result = null;
        float num = GameOptionsData.KillDistances[Mathf.Clamp(GameManager.Instance.LogicOptions.currentGameOptions.GetInt(Int32OptionNames.KillDistance), 0, 2)];
        if (!ShipStatus.Instance) return result;
        if (targetingPlayer == null) targetingPlayer = PlayerControl.LocalPlayer;
        if (targetingPlayer.Data.IsDead || targetingPlayer.inVent) return result;

        Vector2 truePosition = targetingPlayer.GetTruePosition();
        Il2CppSystem.Collections.Generic.List<NetworkedPlayerInfo> allPlayers = GameData.Instance.AllPlayers;
        for (int i = 0; i < allPlayers.Count; i++)
        {
            NetworkedPlayerInfo playerInfo = allPlayers[i];
            if (playerInfo.Disconnected ||
                playerInfo.PlayerId == targetingPlayer.PlayerId ||
                playerInfo.IsDead ||
                (onlyCrewmates && playerInfo.Role.IsImpostor)
               )
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
                PhysicsHelpers.AnyNonTriggersBetween(truePosition, vector.normalized, magnitude, Constants.ShipAndObjectsMask)
                )
                continue;
            result = @object;
            num = magnitude;
        }
        return result;
    }
}
