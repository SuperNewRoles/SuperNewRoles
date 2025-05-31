using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using Hazel;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability.CustomButton;
using SuperNewRoles.Roles.Crewmate;
using UnityEngine;

namespace SuperNewRoles.Roles.Ability;

public record SantaAbilityData(int InitialCount, float Cooldown, bool TryLoversToDeath, bool TryMadRolesToDeath, bool TryJackalFriendsToDeath, bool ForImpostor, List<(RoleId role, int percentage)> Roles);
public class SantaAbility : AbilityBase
{
    private CustomSidekickButtonAbility ButtonAbility;
    private SantaAbilityData _data;
    private List<RoleId> _tickets;
    private RoleId _selectedRole;
    private Sprite _buttonSprite;
    public int _count;
    public SantaAbility(SantaAbilityData data, string buttonSpritePath)
    {
        _data = data;
        _tickets = new();
        foreach (var role in data.Roles)
        {
            for (int i = 0; i < (role.percentage / 5); i++)
            {
                _tickets.Add(role.role);
            }
        }
        if (_tickets.Count == 0)
            data.Roles.ForEach(role => _tickets.Add(role.role));
        _buttonSprite = AssetManager.GetAsset<Sprite>(buttonSpritePath);
        _count = data.InitialCount;
    }

    public override void AttachToAlls()
    {
        ButtonAbility = new(new(
            canCreateSidekick: (_) => ButtonAbility.HasCount,
            sidekickCooldown: () => _data.Cooldown,
            sidekickRole: () => _selectedRole,
            sidekickRoleVanilla: () => CustomRoleManager.GetRoleById(_selectedRole)?.AssignedTeam == AssignedTeamType.Impostor ? (RoleTypes?)null : RoleTypes.Crewmate,
            sidekickSprite: _buttonSprite,
            sidekickText: ModTranslation.GetString("SantaButtonText"),
            sidekickCount: () => _count,
            isTargetable: (player) => player.IsAlive(),
            sidekickSuccess: (player) =>
            {
                if (player.IsMadRoles() && _data.TryMadRolesToDeath)
                    return false;
                else if (player.IsLovers() && _data.TryLoversToDeath)
                    return false;
                else if (player.IsFriendRoles() && _data.TryJackalFriendsToDeath)
                    return false;
                _selectedRole = SelectRole();
                return _data.ForImpostor ? player.IsImpostor() : player.IsCrewmateOrMadRoles();
            },
            onSidekickCreated: (player) =>
            {
                Logger.Info($"SelectedRole: {_selectedRole}");
                bool successed = false;
                if (player.IsMadRoles() && _data.TryMadRolesToDeath)
                    successed = false;
                else if (player.IsLovers() && _data.TryLoversToDeath)
                    successed = false;
                else if (player.IsFriendRoles() && _data.TryJackalFriendsToDeath)
                    successed = false;
                else
                    successed = _data.ForImpostor ? player.IsImpostor() : player.IsCrewmateOrMadRoles();
                RpcSantaAssigndRole(Player, player, successed);
            },
            showSidekickLimitText: () => true
        ));

        Player.AttachAbility(ButtonAbility, new AbilityParentAbility(this));
    }
    private RoleId SelectRole()
    {
        return _selectedRole = _tickets[Random.Range(0, _tickets.Count)];
    }
    [CustomRPC]
    public void RpcSantaAssigndRole(ExPlayerControl source, ExPlayerControl target, bool successed)
    {
        if (!successed)
        {
            // サンタの自爆処理
            source.CustomDeath(CustomDeathType.Suicide);
            return;
        }
        _count--;
    }
}