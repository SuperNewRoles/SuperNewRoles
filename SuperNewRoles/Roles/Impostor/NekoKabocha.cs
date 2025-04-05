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
    public override List<Func<AbilityBase>> Abilities { get; } = [() => new NekoKabochaRevenge()];

    public override QuoteMod QuoteMod { get; } = QuoteMod.SuperNewRoles;
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

public class NekoKabochaRevenge : AbilityBase
{
    private EventListener<MurderEventData> _onMurderEvent;
    private EventListener<WrapUpEventData> _onWrapUpEvent;


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
            if (killer.IsCrewmate() && NekoKabocha.NekoKabochaCanRevengeCrewmate)
                canRevenge = true;
            else if (killer.IsNeutral() && NekoKabocha.NekoKabochaCanRevengeNeutral)
                canRevenge = true;
            else if (killer.IsImpostor() && NekoKabocha.NekoKabochaCanRevengeImpostor)
                canRevenge = true;

            if (canRevenge)
                ExPlayerControl.LocalPlayer.RpcCustomDeath(killer, CustomDeathType.Kill);
        }
    }

    private void OnWrapUp(WrapUpEventData data)
    {
        if (NekoKabocha.NekoKabochaCanRevengeExiled &&
            data.exiled != null && data.exiled.Object != null &&
            data.exiled == ExPlayerControl.LocalPlayer)
        {
            List<ExPlayerControl> targets = ExPlayerControl.ExPlayerControls.Where(x => x.IsAlive() && !x.AmOwner).ToList();

            if (targets.Count > 0)
            {
                ExPlayerControl randomTarget = ModHelpers.GetRandom(targets);
                randomTarget.RpcCustomDeath(CustomDeathType.Revange);
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