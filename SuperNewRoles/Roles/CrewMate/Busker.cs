using System.Collections;
using Agartha.CustomAnimation;
using AmongUs.GameOptions;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using Hazel;
using SuperNewRoles.Roles.Role;
using SuperNewRoles.Roles.RoleBases;
using SuperNewRoles.Roles.RoleBases.Interfaces;
using UnityEngine;

namespace SuperNewRoles.Roles.Crewmate;

public class Busker : RoleBase, ICrewmate, IRpcHandler, ICustomButton, IWrapUpHandler, IFixedUpdaterMe, INameHandler
{
    public static new RoleInfo Roleinfo = new(
        typeof(Busker),
        (p) => new Busker(p),
        RoleId.Busker,
        "Busker",
        new Color32(255, 172, 117, 255),
        new(RoleId.Busker, TeamTag.Crewmate),
        TeamRoleType.Crewmate,
        TeamType.Crewmate,
        quoteMod: QuoteMod.NebulaOnTheShip
        );

    public static new OptionInfo Optioninfo =
        new(RoleId.Busker, 406900, false,
            CoolTimeOption: (15, 0, 60, 2.5f, false),
            optionCreator: CreateOption);
    public static new IntroInfo Introinfo =
        new(RoleId.Busker, introSound: RoleTypes.Scientist);

    public static CustomOption PseudocideDurationOption;

    public CustomButtonInfo PseudocideButtonInfo;
    public CustomButtonInfo RebornButtonInfo;
    public CustomButtonInfo[] CustomButtonInfos { get; }

    private bool AmPseudocide;
    private DeadBody CurrentDeadbody;

    public bool CanGhostSeeRole => !HasButtonReborn();

    public void FixedUpdateMeDefaultDead() => FixedUpdateMeDefaultAlive();
    public void FixedUpdateMeDefaultAlive()
    {
        bool enabled = Player.Collider.enabled;
        Player.Collider.enabled = true;
        Player.Collider.enabled = enabled;
    }

    private static void CreateOption()
    {
        PseudocideDurationOption = CustomOption.Create(Optioninfo.OptionId++, false, Optioninfo.RoleOption.type,
            "BuskerPseudocideDurationOption", 30f, 2.5f, 120f, 2.5f, Optioninfo.RoleOption);
    }

    public void RpcReader(MessageReader reader)
    {
        AmPseudocide = reader.ReadBoolean();
        if (AmPseudocide)
        {
            Player.Die(DeathReason.Kill, false);
            GenerateDeadbody();
            return;
        }
        Player.Revive();
        CleanDeadbody();
        // ベントから出るモーションを出す
        Player.MyPhysics.StartCoroutine(PlayExitVent(Player).WrapToIl2Cpp());
    }
    private static IEnumerator PlayExitVent(PlayerControl player)
    {
        PlayerAnimations anim = player.MyPhysics.Animations;
        yield return anim.CoPlayExitVentAnimation();
        if (anim.Animator.GetCurrentAnimation() != anim.group.ExitVentAnim)
            yield break;
        anim.PlayIdleAnimation();
    }
    private bool HasButtonReborn()
    {
        if (!AmPseudocide)
            return false;
        if (CurrentDeadbody != null &&
            CurrentDeadbody.enabled &&
            CurrentDeadbody.gameObject.active &&
            !DeadBodyManager.IsDeadbodyUsed(CurrentDeadbody))
            return true;
        return AmPseudocide = false;
    }
    private void GenerateDeadbody()
    {
        DeadBody deadBody = Object.Instantiate(GameManager.Instance.DeadBodyPrefab);
        deadBody.enabled = true;
        deadBody.gameObject.SetActive(true);
        deadBody.ParentId = Player.PlayerId;
        deadBody.bodyRenderers.ForEach(delegate (SpriteRenderer b)
        {
            Player.SetPlayerMaterialColors((Renderer)(object)b);
        });
        Player.SetPlayerMaterialColors((Renderer)(object)deadBody.bloodSplatter);
        Vector3 val = (Player).transform.position + Player.KillAnimations.FirstOrDefault().BodyOffset;
        val.z = val.y / 1000f;
        deadBody.transform.position = val;
        CurrentDeadbody = deadBody;
    }
    private void CleanDeadbody()
    {
        DeadBody[] array = Object.FindObjectsOfType<DeadBody>();
        for (int i = 0; i < array.Length; i++)
        {
            if (GameData.Instance.GetPlayerById(array[i].ParentId).PlayerId == Player.PlayerId)
            {
                Object.Destroy(array[i].gameObject);
            }
        }
        CurrentDeadbody = null;
    }

    public Busker(PlayerControl p) : base(p, Roleinfo, Optioninfo, Introinfo)
    {
        CurrentDeadbody = null;
        PseudocideButtonInfo = new(null, this, BuskerPseudocideOnClick,
            (isAlive) => isAlive && !AmPseudocide, CustomButtonCouldType.CanMove, null,
            ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.BuskerPseudocideButton.png", 115f),
            () => Optioninfo.CoolTime, new(1, 2, 0), "BuskerPseudocideButtonName",
            KeyCode.F, 49);
        RebornButtonInfo = new(null, this, BuskerRebornOnClick,
            (isAlive) => HasButtonReborn(), CustomButtonCouldType.Always, () => RebornButtonInfo.GetOrCreateButton().wantEffectCouldUse = RebornButtonInfo.GetOrCreateButton().effectCancellable = true,
            ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.BuskerRebornButton.png", 115f),
            () => 0f, new(1, 2, 0), "BuskerRebornButtonName",
            KeyCode.F, 49, DurationTime: PseudocideDurationOption.GetFloat,
            OnEffectEnds: () => AmPseudocide = false, hasSecondButtonInfo: true,
            CouldUse: RebornCouldUse);
        RebornButtonInfo.GetOrCreateButton().effectCancellable = true;
        RebornButtonInfo.SecondButtonInfoText.text = ModTranslation.GetString("BuskerReallyDeadTimeText");

        CustomButtonInfos = [PseudocideButtonInfo, RebornButtonInfo];
    }
    private bool RebornCouldUse()
    {
        bool enabled = Player.Collider.enabled;
        Player.Collider.enabled = true;
        Player.Collider.enabled = enabled;
        return MapDatabase.MapDatabase.GetCurrentMapData().CheckMapArea(Player.GetTruePosition());
    }
    private void BuskerPseudocideOnClick()
    {
        MessageWriter writer = RpcWriter;
        writer.Write(true);
        SendRpc(writer);
        new LateTask(() =>
        {
            if (!RebornButtonInfo.customButton.isEffectActive)
            {
                RebornButtonInfo.customButton.Timer = RebornButtonInfo.customButton.EffectDuration;
                RebornButtonInfo.customButton.actionButton.cooldownTimerText.color = new Color(0F, 0.8F, 0F);
                RebornButtonInfo.customButton.isEffectActive = true;
            }
        }, 0f);
    }
    private void BuskerRebornOnClick()
    {
        if (!RebornButtonInfo.customButton.isEffectActive)
            return;
        if (!MapDatabase.MapDatabase.GetCurrentMapData().CheckMapArea(Player.GetTruePosition()))
            return;
        MessageWriter writer = RpcWriter;
        writer.Write(false);
        SendRpc(writer);
    }
    public void OnWrapUp()
    {
        AmPseudocide = false;
    }
}