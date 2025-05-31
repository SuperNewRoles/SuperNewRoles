using System;
using System.Collections.Generic;
using AmongUs.GameOptions;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Modules;
using SuperNewRoles.Events;
using SuperNewRoles.Events.PCEvents;
using SuperNewRoles.Modules.Events.Bases;
using UnityEngine;
using SuperNewRoles.Roles.Ability;
using System.Linq;

namespace SuperNewRoles.Roles.Impostor;

class NekoKabocha : RoleBase<NekoKabocha>
{
    public override RoleId Role { get; } = RoleId.NekoKabocha;
    public override Color32 RoleColor { get; } = Palette.ImpostorRed;
    public override List<Func<AbilityBase>> Abilities { get; } = [() => new NekoKabochaRevenge(
        new(NekoKabochaCanRevengeCrewmate,
            NekoKabochaCanRevengeNeutral,
            NekoKabochaCanRevengeImpostor,
            NekoKabochaCanRevengeExiled))
        ];

    public override QuoteMod QuoteMod { get; } = QuoteMod.TheOtherRolesGM;
    public override RoleTypes IntroSoundType { get; } = RoleTypes.Impostor;
    public override short IntroNum { get; } = 1;

    public override AssignedTeamType AssignedTeam { get; } = AssignedTeamType.Impostor;
    public override WinnerTeamType WinnerTeam { get; } = WinnerTeamType.Impostor;
    public override TeamTag TeamTag { get; } = TeamTag.Impostor;
    public override RoleTag[] RoleTags { get; } = [RoleTag.SpecialKiller];
    public override RoleOptionMenuType OptionTeam { get; } = RoleOptionMenuType.Impostor;

    [CustomOptionBool("NekoKabocha.CanRevengeCrewmate", true)]
    public static bool NekoKabochaCanRevengeCrewmate;

    [CustomOptionBool("NekoKabocha.CanRevengeNeutral", true)]
    public static bool NekoKabochaCanRevengeNeutral;

    [CustomOptionBool("NekoKabocha.CanRevengeImpostor", true)]
    public static bool NekoKabochaCanRevengeImpostor;

    [CustomOptionBool("NekoKabocha.CanRevengeExiled", true)]
    public static bool NekoKabochaCanRevengeExiled;
}

public record NekoKabochaData(bool CanRevengeCrewmate, bool CanRevengeNeutral, bool CanRevengeImpostor, bool CanRevengeExiled);

public class NekoKabochaRevenge : AbilityBase
{
    private EventListener<MurderEventData> _onMurderEvent;
    private EventListener<WrapUpEventData> _onWrapUpEvent;

    public NekoKabochaData Data { get; }

    public NekoKabochaRevenge(NekoKabochaData data)
    {
        Data = data;
    }

    public override void AttachToLocalPlayer()
    {
        // 殺害イベントリスナーの登録
        _onMurderEvent = MurderEvent.Instance.AddListener(OnMurder);

        // 会議終了/追放イベントリスナーの登録
        _onWrapUpEvent = WrapUpEvent.Instance.AddListener(OnWrapUp);
    }

    private void OnMurder(MurderEventData data)
    {
        if (data.target != null && data.target == ExPlayerControl.LocalPlayer && data.killer != null)
        {
            bool canRevenge = false;
            ExPlayerControl killer = data.killer;

            // 殺害したプレイヤーのタイプに基づいて復讐できるかどうかを判断
            if (killer.IsCrewmate() && Data.CanRevengeCrewmate)
                canRevenge = true;
            else if (killer.IsNeutral() && Data.CanRevengeNeutral)
                canRevenge = true;
            else if (killer.IsImpostor() && Data.CanRevengeImpostor)
                canRevenge = true;

            if (canRevenge)
                ExPlayerControl.LocalPlayer.RpcCustomDeath(killer, CustomDeathType.Kill);
        }
    }

    private void OnWrapUp(WrapUpEventData data)
    {
        if (Data.CanRevengeExiled &&
            data.exiled != null && data.exiled.Object != null &&
            data.exiled.PlayerId == ExPlayerControl.LocalPlayer.PlayerId)
        {
            List<ExPlayerControl> targets = ExPlayerControl.ExPlayerControls.Where(x => x.IsAlive() && !x.AmOwner).ToList();

            if (targets.Count > 0)
            {
                ExPlayerControl randomTarget = ModHelpers.GetRandom(targets);
                ExPlayerControl.LocalPlayer.RpcCustomDeath(randomTarget, CustomDeathType.Revange);
            }
        }
    }

    public override void DetachToLocalPlayer()
    {
        base.DetachToLocalPlayer();

        if (_onMurderEvent != null)
        {
            MurderEvent.Instance.RemoveListener(_onMurderEvent);
            _onMurderEvent = null;
        }

        if (_onWrapUpEvent != null)
        {
            WrapUpEvent.Instance.RemoveListener(_onWrapUpEvent);
            _onWrapUpEvent = null;
        }
    }
}