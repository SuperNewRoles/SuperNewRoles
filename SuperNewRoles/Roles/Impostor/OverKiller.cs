using System;
using System.Collections.Generic;
using UnityEngine;
using AmongUs.GameOptions;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Roles.Ability;
using SuperNewRoles.Roles.Ability.CustomButton;
using SuperNewRoles.Modules;
using SuperNewRoles.Events.PCEvents;
using SuperNewRoles.Modules.Events.Bases;
using SuperNewRoles.Ability;
using System.Linq;

namespace SuperNewRoles.Roles.Impostor;

class OverKiller : RoleBase<OverKiller>
{
    public override RoleId Role { get; } = RoleId.OverKiller;
    public override Color32 RoleColor { get; } = Palette.ImpostorRed;
    public override List<Func<AbilityBase>> Abilities { get; } = [
        () => new OverKillerAbility()
    ];

    public override QuoteMod QuoteMod { get; } = QuoteMod.SuperNewRoles;
    public override RoleTypes IntroSoundType { get; } = RoleTypes.Impostor;
    public override short IntroNum { get; } = 1;

    public override AssignedTeamType AssignedTeam { get; } = AssignedTeamType.Impostor;
    public override WinnerTeamType WinnerTeam { get; } = WinnerTeamType.Impostor;
    public override TeamTag TeamTag { get; } = TeamTag.Impostor;
    public override RoleTag[] RoleTags { get; } = [RoleTag.SpecialKiller];
    public override RoleOptionMenuType OptionTeam { get; } = RoleOptionMenuType.Impostor;

    [CustomOptionFloat("OverKillerKillCooldown", 2.5f, 120f, 2.5f, 45f, translationName: "KillCoolTime")]
    public static float OverKillerKillCooldown;

    [CustomOptionInt("OverKillerKillCount", 1, 60, 1, 30)]
    public static int OverKillerKillCount;

    [CustomOptionBool("OverKillerScatterBodies", true)]
    public static bool OverKillerScatterBodies;

    [CustomOptionFloat("OverKillerScatterRange", 0f, 5f, 0.1f, 1.5f)]
    public static float OverKillerScatterRange;
}

public class OverKillerAbility : AbilityBase
{
    private EventListener<MurderEventData> _onMurderEvent;

    public override void AttachToAlls()
    {
        base.AttachToAlls();
        Player.AttachAbility(new CustomKillButtonAbility(
            canKill: () => true,
            killCooldown: () => OverKiller.OverKillerKillCooldown,
            onlyCrewmates: () => true
        ),
        new AbilityParentAbility(this));
    }

    public override void AttachToLocalPlayer()
    {
        base.AttachToLocalPlayer();
        _onMurderEvent = MurderEvent.Instance.AddListener(HandleMurderEvent);
    }

    public override void DetachToLocalPlayer()
    {
        base.DetachToLocalPlayer();
        _onMurderEvent?.RemoveListener();
    }

    private void HandleMurderEvent(MurderEventData data)
    {
        if (data.killer?.PlayerId != Player.PlayerId) return;
        if (data.resultFlags.HasFlag(MurderResultFlags.FailedError) || data.resultFlags.HasFlag(MurderResultFlags.FailedProtected)) return;

        // 複数の死体を生成する
        CreateMultipleBodies(data.target, OverKiller.OverKillerKillCount);
    }

    private void CreateMultipleBodies(ExPlayerControl target, int bodyCount)
    {
        if (target == null) return;

        List<Vector3> positionsToScatter = new();
        var originalPosition = target.transform.position;
        var bodyOffset = target.Player.KillAnimations[0].BodyOffset;

        for (int i = 0; i < bodyCount; i++)
        {
            Vector3 position = originalPosition + bodyOffset;
            if (OverKiller.OverKillerScatterBodies)
            {
                position += new Vector3(
                    ModHelpers.GetRandomFloat(-OverKiller.OverKillerScatterRange, OverKiller.OverKillerScatterRange),
                    ModHelpers.GetRandomFloat(-OverKiller.OverKillerScatterRange, OverKiller.OverKillerScatterRange),
                    0f
                );
            }
            position.z = position.y / 1000f;
            positionsToScatter.Add(position);
        }

        // Send RPC in chunks of 25
        int chunkSize = 25;
        for (int i = 0; i < positionsToScatter.Count; i += chunkSize)
        {
            Vector3[] chunk = positionsToScatter.Skip(i).Take(chunkSize).ToArray();
            RpcCreateMultipleBodies(target.PlayerId, chunk);
        }
    }

    [CustomRPC]
    public void RpcCreateMultipleBodies(byte targetPlayerId, Vector3[] scatteredPositions)
    {
        ExPlayerControl target = ExPlayerControl.ById(targetPlayerId);
        if (target == null) return;

        var deadBodyPrefab = GameManager.Instance.GetDeadBody(Player.Data.Role);
        if (deadBodyPrefab == null) return;

        foreach (var position in scatteredPositions)
        {
            var deadBody = UnityEngine.Object.Instantiate(deadBodyPrefab);
            deadBody.enabled = false;
            deadBody.ParentId = target.PlayerId;

            deadBody.transform.position = position;

            // 体の色を設定
            foreach (var renderer in deadBody.bodyRenderers)
            {
                PlayerMaterial.SetColors(target.Player.CurrentOutfit.ColorId, renderer);
            }
            PlayerMaterial.SetColors(target.Player.CurrentOutfit.ColorId, deadBody.bloodSplatter);

            deadBody.enabled = true;
        }
    }
}