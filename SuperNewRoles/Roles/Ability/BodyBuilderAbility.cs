using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using SuperNewRoles.Events;
using SuperNewRoles.Events.PCEvents;
using SuperNewRoles.Modules;
using SuperNewRoles.Modules.Events.Bases;
using SuperNewRoles.Roles.Ability.CustomButton;
using SuperNewRoles.CustomOptions;
using UnityEngine;
using Action = System.Action;
using IEnumerator = Il2CppSystem.Collections.IEnumerator;
using Object = UnityEngine.Object;
using SuperNewRoles.Modules.Events;

namespace SuperNewRoles.Roles.Ability;

public class BodyBuilderAbility : CustomButtonBase
{
    // CustomButtonBase
    public override Sprite Sprite => AssetManager.GetAsset<Sprite>("BodyBuilderButton.png");
    public override string buttonText => ModTranslation.GetString("BodyBuilderButtonText");
    protected override KeyType keytype => KeyType.Ability1;
    public override float DefaultTimer => 0f;

    private GameObject posingObject;
    private Vector3 lastPosition = Vector3.zero;

    // 初期が0なので
    public override bool IsFirstCooldownTenSeconds => false;

    // ポーズID範囲
    private static readonly IntRange PosingIdRange = new(1, 5);

    // アセットバンドル
    private EventListener<MurderEventData> _murderEvent;
    private EventListener<MeetingStartEventData> _meetingStartEvent;
    private EventListener<ExileEventData> _exileEvent;
    private EventListener<PlayerPhysicsFixedUpdateEventData> _onPlayerPhysicsFixedUpdateEvent;
    public override void AttachToLocalPlayer()
    {
        base.AttachToLocalPlayer();
        _murderEvent = MurderEvent.Instance.AddListener(x =>
        {
            if (x.target == Player)
                CancelPosing();
        });
        _meetingStartEvent = MeetingStartEvent.Instance.AddListener(x => CancelPosing());
        _exileEvent = ExileEvent.Instance.AddListener(x => CancelPosing());
        _onPlayerPhysicsFixedUpdateEvent = PlayerPhysicsFixedUpdateEvent.Instance.AddListener(x => UpdatePhysics(x));
    }

    public override void DetachToLocalPlayer()
    {
        base.DetachToLocalPlayer();
        _murderEvent?.RemoveListener();
        _meetingStartEvent?.RemoveListener();
        _exileEvent?.RemoveListener();
        _onPlayerPhysicsFixedUpdateEvent?.RemoveListener();
    }

    public override void DetachToAlls()
    {
        base.DetachToAlls();
        if (posingObject != null)
        {
            CancelPosing();
        }
    }

    public override void OnClick()
    {
        // ポージング開始
        Logger.Info("BodyBuilder ポージング開始");
        byte posingId = (byte)PosingIdRange.Next();
        RpcStartPosing(posingId);
    }

    public override bool CheckIsAvailable()
    {
        return true;
    }

    public override bool CheckHasButton()
    {
        // 全タスク完了時のみ使用可能
        return Player == ExPlayerControl.LocalPlayer && Player.IsTaskComplete();
    }

    private void UpdatePhysics(PlayerPhysicsFixedUpdateEventData data)
    {
        if (data.Instance.myPlayer.AmOwner && posingObject != null && data.Instance.myPlayer.MyPhysics.body.velocity.magnitude > 0.01f)
            RpcCancelPosing();
    }

    [CustomRPC]
    public void RpcStartPosing(byte posingId)
    {
        lastPosition = Player.transform.position;
        StartPosing(posingId);
    }

    [CustomRPC]
    public void RpcCancelPosing()
    {
        CancelPosing();
    }

    private void StartPosing(byte posingId)
    {
        // 使用者が霊界かつ自身が霊界でないならreturn
        if (Player.IsDead() && ExPlayerControl.LocalPlayer.IsAlive())
            return;

        CancelPosing();
        Player.NetTransform.Halt();

        // 音を再生
        var distance = Vector2.Distance(PlayerControl.LocalPlayer.transform.position, Player.transform.position);
        var volume = 1 / distance <= 0.25f ? 0f : 1 / distance;
        // 最悪音ならなくてもいいので遅延
        MapLoader.LoadMap(MapNames.Fungle, (ship) =>
        {
            var liftWeightsTask = ship.ShortTasks.FirstOrDefault(x => x.TaskType == TaskTypes.LiftWeights);
            if (liftWeightsTask != null)
            {
                var liftWeightsMinigame = liftWeightsTask.MinigamePrefab.TryCast<LiftWeightsMinigame>();
                if (liftWeightsMinigame != null)
                {
                    ModHelpers.PlaySound(Player.transform, liftWeightsMinigame.completeAllRepsSound, false, volume, null);
                }
            }
        });

        // ポーズプレハブを生成
        var prefab = GetPrefab(posingId);
        var pose = Object.Instantiate(prefab, Player.NetTransform.transform);
        foreach (SpriteRenderer renderer in Player.Player.gameObject.GetComponentsInChildren<SpriteRenderer>())
        {
            renderer.color = new(1f, 1f, 1f, 0f);
        }

        var pos = pose.gameObject.transform.position;
        pos.z -= 0.5f;
        pos.y += Player.cosmetics.currentBodySprite.BodySprite.transform.parent.localScale.x == 1 ? 0f : 0.8f;
        pose.gameObject.transform.position = pos;
        pose.gameObject.transform.localScale = Player.cosmetics.currentBodySprite.BodySprite.transform.parent.localScale;

        var spriteRenderer = pose.GetComponent<SpriteRenderer>();
        spriteRenderer.sharedMaterial = FastDestroyableSingleton<HatManager>.Instance.PlayerMaterial;
        spriteRenderer.maskInteraction = SpriteMaskInteraction.None;
        PlayerMaterial.SetMaskLayerBasedOnLocalPlayer(spriteRenderer, false);
        PlayerMaterial.SetColors(Player.Data.DefaultOutfit.ColorId, spriteRenderer);
        spriteRenderer.color = new(1f, 1f, 1f, Player.IsDead() ? 0.5f : 1f);

        posingObject = pose;
    }

    private void CancelPosing()
    {
        if (Player != null)
        {
            foreach (SpriteRenderer renderer in Player.Player.gameObject.GetComponentsInChildren<SpriteRenderer>())
            {
                renderer.color = new(1f, 1f, 1f, 1f);
            }
        }

        if (posingObject != null)
        {
            Object.Destroy(posingObject);
            posingObject = null;
        }
    }

    private static GameObject GetPrefab(byte id)
    {
        Logger.Info($"Loading BodyBuilderAnim0{id}");
        return AssetManager.GetAsset<GameObject>($"BodyBuilderAnim0{id}").DontUnload();
    }
}