using System;
using UnityEngine;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability.CustomButton;

namespace SuperNewRoles.Roles.Ability;

public class GuesserAbility : CustomMeetingButtonBase
{
    private readonly int maxShots;
    private readonly int shotsPerMeeting;
    private readonly bool cannotShootCrewmate;
    private readonly bool cannotShootImpostor;
    private readonly bool cannotShootStar;

    public override bool HasButtonLocalPlayer => false;

    public override Sprite Sprite => AssetManager.GetAsset<Sprite>("TargetIcon.png");

    public GuesserAbility(int maxShots, int shotsPerMeeting, bool cannotShootCrewmate, bool cannotShootImpostor, bool cannotShootStar)
    {
        this.maxShots = maxShots;
        this.shotsPerMeeting = shotsPerMeeting;
        this.cannotShootCrewmate = cannotShootCrewmate;
        this.cannotShootImpostor = cannotShootImpostor;
        this.cannotShootStar = cannotShootStar;
    }

    public override bool CheckHasButton(ExPlayerControl player)
    {
        return player.IsAlive();
    }

    public override bool CheckIsAvailable(ExPlayerControl player)
    {
        return player.IsAlive();
    }

    public override void OnClick()
    {
        // TODO: 推測UI表示などの処理を実装
        Logger.Info("GuesserAbility OnClick");
    }

    public void TryGuess(PlayerControl target, RoleId guessedRole)
    {
        if (target == null) return;

    }
}