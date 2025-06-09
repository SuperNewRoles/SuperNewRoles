using System;
using UnityEngine;
using SuperNewRoles.Roles.Ability.CustomButton;
using SuperNewRoles.Modules;
using SuperNewRoles.Events;
using SuperNewRoles.Events.PCEvents;
using SuperNewRoles.Roles.Crewmate;
using System.Linq;
using System.Collections;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using SuperNewRoles.Modules.Events.Bases;
using AmongUs.GameOptions;
using SuperNewRoles.MapDatabase;

namespace SuperNewRoles.Roles.Ability;

public class BuskerPseudocideAbility : CustomButtonBase, IButtonEffect
{
    public BuskerPseudocideAbility(float coolTime, float duration)
    {
        DefaultTimer = coolTime;
        EffectDuration = duration;
    }
    public override float DefaultTimer { get; }
    public override Sprite Sprite => AssetManager.GetAsset<Sprite>(isEffectActive ? "BuskerRebornButton.png" : "BuskerPseudocideButton.png");
    protected override KeyType keytype => KeyType.Ability1;

    // IButtonEffect implementation
    public bool isEffectActive { get; set; }
    public Action OnEffectEnds => () => { if (EffectTimer > 0) OnReborn(); else OnPseudocideEnd(); };
    public float EffectDuration { get; set; }
    public float EffectTimer { get; set; }

    public bool effectCancellable => true;

    private DeadBody CurrentDeadbody;

    private DisibleHauntAbility _disableHauntAbility;
    private HideRoleOnGhostAbility _hideRoleOnGhostAbility;

    public override bool CheckHasButton()
    {
        return Player == ExPlayerControl.LocalPlayer && (isEffectActive || ExPlayerControl.LocalPlayer.IsAlive());
    }

    public override bool CheckIsAvailable()
    {
        return !Player.Data.IsDead && !isEffectActive;
    }

    public bool IsEffectAvailable() =>
        (CurrentDeadbody != null && CurrentDeadbody.enabled && CurrentDeadbody.gameObject.activeInHierarchy && !CurrentDeadbody.Reported) &&
        (MapDatabase.MapDatabase.GetCurrentMapData()?.CheckMapArea(PlayerControl.LocalPlayer.transform.position) ?? true);

    public override void AttachToAlls()
    {
        base.AttachToAlls();
        _disableHauntAbility = new DisibleHauntAbility(() => isEffectActive);
        _hideRoleOnGhostAbility = new HideRoleOnGhostAbility((player) => isEffectActive);
        Player.AttachAbility(_disableHauntAbility, new AbilityParentAbility(this));
        Player.AttachAbility(_hideRoleOnGhostAbility, new AbilityParentAbility(this));
    }

    public override void OnClick()
    {
        if (isEffectActive)
        {
            // 復活処理
            OnReborn();
        }
        else
        {
            // 偽装死処理
            StartPseudocide();
        }
    }

    private void GenerateDeadbody()
    {
        DeadBody deadBody = GameObject.Instantiate(GameManager.Instance.DeadBodyPrefab);
        deadBody.enabled = true;
        deadBody.gameObject.SetActive(true);
        deadBody.ParentId = Player.PlayerId;
        foreach (SpriteRenderer b in deadBody.bodyRenderers)
        {
            Player.Player.SetPlayerMaterialColors(b);
        }
        Player.Player.SetPlayerMaterialColors(deadBody.bloodSplatter);
        Vector3 val = Player.Player.transform.position + Player.Player.KillAnimations.FirstOrDefault().BodyOffset;
        val.z = val.y / 1000f;
        deadBody.transform.position = val;
        CurrentDeadbody = deadBody;
    }
    private void CleanDeadbody()
    {
        if (CurrentDeadbody != null)
            GameObject.Destroy(CurrentDeadbody.gameObject);
        CurrentDeadbody = null;
    }

    [CustomRPC]
    private void StartPseudocide()
    {
        // プレイヤーを死亡状態にする（偽装）
        Player.CustomDeath(CustomDeathType.BuskerFakeDeath);
        GenerateDeadbody();
        RoleManager.Instance.SetRole(Player, RoleTypes.CrewmateGhost);
    }

    [CustomRPC]
    private void OnReborn()
    {
        // プレイヤーを復活させる
        Player.Player.Revive();
        RoleManager.Instance.SetRole(Player, RoleTypes.Crewmate);
        CleanDeadbody();
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
    private void OnPseudocideEnd()
    {
        // 時間切れで本当に死ぬ
        Player.CustomDeath(CustomDeathType.SuicideSecrets);
    }

    public override void OnMeetingEnds()
    {
        if (isEffectActive)
        {
            // 会議中に偽装死していた場合は本当に死ぬ
            OnPseudocideEnd();
        }
        base.OnMeetingEnds();
    }

    private EventListener fixedUpdateEvent;
    private EventListener<MeetingStartEventData> _meetingStartListener;

    public override void AttachToLocalPlayer()
    {
        base.AttachToLocalPlayer();

        // 会議開始イベントをリッスン
        _meetingStartListener = MeetingStartEvent.Instance.AddListener(OnMeetingStart);
        fixedUpdateEvent = FixedUpdateEvent.Instance.AddListener(OnFixedUpdate);
    }
    public override void DetachToLocalPlayer()
    {
        base.DetachToLocalPlayer();
        _meetingStartListener?.RemoveListener();
        fixedUpdateEvent?.RemoveListener();
    }

    private void OnFixedUpdate()
    {
        if (CurrentDeadbody == null && isEffectActive && EffectTimer > 0.1f)
            EffectTimer = 0.00001f;
    }

    private void OnMeetingStart(MeetingStartEventData data)
    {
        if (isEffectActive)
        {
            // 会議が始まったら本当に死ぬ
            OnPseudocideEnd();
            isEffectActive = false;
        }
    }

    public override string buttonText => isEffectActive ?
        ModTranslation.GetString("BuskerRebornButtonName") :
        ModTranslation.GetString("BuskerPseudocideButtonName");

    public override ShowTextType showTextType => isEffectActive ? ShowTextType.ShowWithCount : ShowTextType.Hidden;

    public override string showText => isEffectActive ?
        string.Format(ModTranslation.GetString("BuskerReallyDeadTimeText"), Mathf.CeilToInt(EffectTimer)) :
        string.Empty;
}