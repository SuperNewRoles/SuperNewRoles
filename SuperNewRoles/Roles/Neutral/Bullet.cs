using AmongUs.GameOptions;
using Hazel;
using SuperNewRoles.Roles.Role;
using SuperNewRoles.Roles.RoleBases;
using SuperNewRoles.Roles.RoleBases.Interfaces;
using UnityEngine;

namespace SuperNewRoles.Roles.Neutral;

public class Bullet : RoleBase, ISidekick, INeutral, IVentAvailable, ISaboAvailable, IImpostorVision, ICustomButton, IRpcHandler, IFixedUpdaterAll, IMeetingHandler, INameHandler, IHandleChangeRole
{
    public static new RoleInfo Roleinfo = new(
        typeof(Bullet),
        (p) => new Bullet(p),
        RoleId.Bullet,
        "Bullet",
        RoleClass.JackalBlue,
        new(RoleId.Bullet, TeamTag.Jackal),
        TeamRoleType.Neutral,
        TeamType.Neutral
        );

    public bool CanUseSabo => WaveCannonJackal.Optioninfo.CanUseSabo;
    public bool CanUseVent => WaveCannonJackal.Optioninfo.CanUseVent;
    public bool IsImpostorVision => WaveCannonJackal.Optioninfo.IsImpostorVision;

    private bool willHasWaveCannon => WaveCannonJackal.CreatedSidekickHasWaveCannon.GetBool();
    public RoleId TargetRole => willHasWaveCannon ? RoleId.WaveCannonJackal : RoleId.Jackal;
    public WaveCannonJackal SidekickedParent;

    public CustomButtonInfo[] CustomButtonInfos { get; }

    public CustomButtonInfo LoadBulletButtonInfo;

    public static new IntroInfo Introinfo =
        new(RoleId.Bullet, introSound: RoleTypes.Shapeshifter);
    public Bullet(PlayerControl p) : base(p, Roleinfo, null, Introinfo)
    {
        LoadBulletButtonInfo = new(null, this, LoadBulletOnClick,
            (isAlive) => isAlive && SidekickedParent?.IsLoadedBullet == false, CustomButtonCouldType.CanMove, null,
            ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.BulletLoadBulletButton.png", 115f),
            WaveCannonJackal.BulletLoadBulletCooltime.GetFloat, new(1, 2), "BulletLoadBulletButtonName",
            KeyCode.F, 49, CouldUse: IsNearParent);
        CustomButtonInfos = [LoadBulletButtonInfo];
    }
    public void OnHandleName()
    {
        if (SidekickedParent == null)
            return;
        SetNamesClass.SetPlayerNameText(SidekickedParent.Player, SidekickedParent.Player.NameText().text + ModHelpers.Cs(WaveCannonJackal.Roleinfo.RoleColor, "☆"));
    }
    private bool IsNearParent()
    {
        if (Player == null)
            return false;
        if (SidekickedParent?.Player == null)
            return false;
        float num = GameManager.Instance.LogicOptions.GetKillDistance();
        if (!MapUtilities.CachedShipStatus) return false;
        if (Player.inVent) return false;

        Vector2 truePosition = Player.GetTruePosition();
        PlayerControl @object = SidekickedParent.Player;
        if (@object.IsDead() || @object.inVent)
            return false;
        Vector2 vector = @object.GetTruePosition() - truePosition;
        float magnitude = vector.magnitude;
        if (magnitude > num ||
            PhysicsHelpers.AnyNonTriggersBetween(truePosition, vector.normalized, magnitude, Constants.ShipAndObjectsMask)
            )
            return false;
        return true;
    }

    public void SetParent(PlayerControl player)
    {
        SidekickedParent = player?.GetRoleBase<WaveCannonJackal>();
    }

    private void LoadBulletOnClick()
    {
        SendRpc(RpcWriter);
    }

    public void RpcReader(MessageReader reader)
    {
        SidekickedParent?.LoadedBullet();
    }

    public void FixedUpdateAllDefault()
    {
        if (RoleClass.IsMeeting)
            return;
        if (SidekickedParent == null ||
            SidekickedParent.Player.IsDead())
            return;
        if (SidekickedParent?.IsLoadedBullet != true)
            return;
        if (Player == null || Player.transform == null)
            return;
        Player.MyPhysics.body.velocity = new();
        Player.NetTransform.SnapTo(SidekickedParent.Player.transform.position);
        Player.transform.position = SidekickedParent.Player.transform.position;
    }

    public void StartMeeting()
    {
        if (SidekickedParent == null)
            return;
        SidekickedParent.SetDidntLoadBullet();
    }

    public void CloseMeeting()
    {
    }

    public void OnChangeRole()
    {
        if (SidekickedParent == null)
            return;
        SidekickedParent.SetDidntLoadBullet();
    }
}