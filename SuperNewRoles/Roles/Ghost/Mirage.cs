using System;
using System.Collections.Generic;
using System.Linq;
using SuperNewRoles.Events.PCEvents;
using SuperNewRoles.Modules;
using SuperNewRoles.Modules.Events.Bases;
using SuperNewRoles.Roles.Ability;
using SuperNewRoles.Roles.Ability.CustomButton;
using UnityEngine;

namespace SuperNewRoles.Roles.Ghost;

class Mirage : GhostRoleBase<Mirage>
{
    public override GhostRoleId Role { get; } = GhostRoleId.Mirage;
    public override Color32 RoleColor { get; } = Palette.ImpostorRed;

    public override List<Func<AbilityBase>> Abilities => [
        () => new MirageAbility(MirageLimitUse, MirageLimitCount),
        () => new DisibleHauntAbility(() => true),
    ];

    public override QuoteMod QuoteMod => QuoteMod.SuperNewRoles;

    public override AssignedTeamType AssignedTeam => AssignedTeamType.Impostor;

    public override WinnerTeamType WinnerTeam => WinnerTeamType.Impostor;

    public override TeamTag TeamTag => TeamTag.Impostor;

    public override RoleTag[] RoleTags => [RoleTag.GhostRole];

    public override short IntroNum => 1;

    [CustomOptionBool("MirageLimitUse", false)]
    public static bool MirageLimitUse;
    [CustomOptionInt("MirageLimitCount", 1, 10, 1, 2, parentFieldName: nameof(MirageLimitUse))]
    public static int MirageLimitCount;
}

public class MirageAbility : CustomButtonBase, IAbilityCount
{
    public bool LimitUse;
    public int LimitCount;
    public MirageAbility(bool limitUse, int limitCount)
    {
        LimitUse = limitUse;
        LimitCount = limitCount;
        Count = LimitCount;
    }
    public override float DefaultTimer => 0f;
    public override string buttonText => ModTranslation.GetString("MirageButtonText");
    public override Sprite Sprite => AssetManager.GetAsset<Sprite>("MirageButton.png");
    protected override KeyType keytype => KeyType.Ability1;
    private EventListener<DieEventData> dieEventListener;

    public override ShowTextType showTextType => LimitUse && HasCount ? ShowTextType.ShowWithCount : ShowTextType.Hidden;

    public override void AttachToLocalPlayer()
    {
        base.AttachToLocalPlayer();
        dieEventListener = DieEvent.Instance.AddListener(OnPlayerDead);
    }

    public override void DetachToLocalPlayer()
    {
        base.DetachToLocalPlayer();
        dieEventListener?.RemoveListener();
    }
    public override bool CheckIsAvailable()
    {
        return GameObject.FindObjectsOfType<DeadBody>().Length > 0;
    }

    public override bool CheckHasButton()
    {
        return HasCount;
    }

    public override void OnClick()
    {
        if (LimitUse)
            this.UseAbilityCount();
        HashSet<byte> deadPlayers = new();
        foreach (var deadBody in GameObject.FindObjectsOfType<DeadBody>())
            deadPlayers.Add(deadBody.ParentId);
        List<ExPlayerControl> players = new();
        foreach (var player in deadPlayers)
        {
            ExPlayerControl pl = ExPlayerControl.ById(player);
            if (pl != null)
                players.Add(pl);
        }
        if (players.Count == 0)
            return;
        RpcSpawnDeadbody(players.GetRandom(), ExPlayerControl.LocalPlayer.transform.position);
    }

    private void OnPlayerDead(DieEventData data)
    {
        if (data.player == ExPlayerControl.LocalPlayer)
            return;
        FlashHandler.ShowFlash(Color.cyan);
    }

    [CustomRPC]
    public static void RpcSpawnDeadbody(PlayerControl player, Vector3 position)
    {
        DeadBody deadBody = GameObject.Instantiate<DeadBody>(GameManager.Instance.DeadBodyPrefab);
        deadBody.ParentId = player.PlayerId;
        foreach (var bodyRenderer in deadBody.bodyRenderers)
        {
            player.SetPlayerMaterialColors((Renderer)(object)bodyRenderer);
        }
        player.SetPlayerMaterialColors((Renderer)(object)deadBody.bloodSplatter);
        Vector3 val = position + (PlayerControl.LocalPlayer.KillAnimations.FirstOrDefault()?.BodyOffset ?? Vector3.zero);
        val.z = val.y / 1000f;
        deadBody.transform.position = val;
        deadBody.enabled = true;
    }
}