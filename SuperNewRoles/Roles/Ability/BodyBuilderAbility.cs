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
using SuperNewRoles.Roles.Crewmate;

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
    private EventListener<ExileControllerEventData> _exileControllerEvent;
    private EventListener<MeetingCalledAnimationInitializeEventData> _meetingCalledAnimationInitializeEvent;
    private EventListener<PlayerPhysicsFixedUpdateEventData> _onPlayerPhysicsFixedUpdateEvent;
    public override void AttachToLocalPlayer()
    {
        base.AttachToLocalPlayer();
        _onPlayerPhysicsFixedUpdateEvent = PlayerPhysicsFixedUpdateEvent.Instance.AddListener(x => UpdatePhysics(x));
    }

    public override void AttachToAlls()
    {
        base.AttachToAlls();
        _murderEvent = MurderEvent.Instance.AddListener(x =>
        {
            if (x.target == Player)
                CancelPosing();
        });
        _meetingStartEvent = MeetingStartEvent.Instance.AddListener(x =>
        {
            CancelPosing();
            BodyBuilderMuscleDisplay.RefreshNamePlate(Player.VoteArea, Player.Data);
        });
        _exileEvent = ExileEvent.Instance.AddListener(x => CancelPosing());
        _exileControllerEvent = ExileControllerEvent.Instance.AddListener(x =>
        {
            if (x.instance.initData.networkedPlayer?.PlayerId != Player.PlayerId)
                return;
            BodyBuilderMuscleDisplay.Refresh(x.instance.Player, Player.Data, BodyBuilderMuscleDisplayContext.Exile);
        });
        _meetingCalledAnimationInitializeEvent = MeetingCalledAnimationInitializeEvent.Instance.AddListener(x =>
        {
            if (x.outfit.PlayerName != Player.Data.DefaultOutfit.PlayerName)
                return;
            BodyBuilderMuscleDisplay.Refresh(x.animation.playerParts, Player.Data, BodyBuilderMuscleDisplayContext.MeetingCall);
            BodyBuilderMuscleDisplay.Refresh(x.animation.classicPlayerParts, Player.Data, BodyBuilderMuscleDisplayContext.MeetingCall);
            BodyBuilderMuscleDisplay.EnsureMeetingButtonsVisible(x.animation);
            new LateTask(
                () => BodyBuilderMuscleDisplay.EnsureMeetingButtonsVisible(x.animation),
                0f,
                "BodyBuilder meeting button restore",
                false);
            new LateTask(
                () => BodyBuilderMuscleDisplay.EnsureMeetingButtonsVisible(x.animation),
                0.1f,
                "BodyBuilder meeting button restore retry",
                false);
        });
    }

    public override void DetachToLocalPlayer()
    {
        base.DetachToLocalPlayer();
        _onPlayerPhysicsFixedUpdateEvent?.RemoveListener();
    }

    public override void DetachToAlls()
    {
        base.DetachToAlls();
        if (posingObject != null)
        {
            CancelPosing();
        }
        _murderEvent?.RemoveListener();
        _meetingStartEvent?.RemoveListener();
        _exileEvent?.RemoveListener();
        _exileControllerEvent?.RemoveListener();
        _meetingCalledAnimationInitializeEvent?.RemoveListener();
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
        return ExPlayerControl.LocalPlayer.IsDead() || PlayerControl.LocalPlayer.CanMove;
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

    internal static GameObject GetPrefab(byte id)
    {
        Logger.Info($"Loading BodyBuilderAnim0{id}");
        return AssetManager.GetAsset<GameObject>($"BodyBuilderAnim0{id}").DontUnload();
    }

    internal static byte NextPosingId() => (byte)PosingIdRange.Next();
}

internal static class BodyBuilderMuscleDisplay
{
    private const string MuscleObjectName = "BodyBuilderMoreMuscularPose";
    private const string NamePlateMaskObjectName = "BodyBuilderMoreMuscularMask";
    private const string PoseIdMarkerPrefix = "BodyBuilderPoseId_";
    private const float NamePlateScale = 1.5f;
    private const float NamePlateMaskLeftInset = 0.08f;
    private const float ChatScale = 1.6f;
    private const float MeetingCallScale = 2f;
    private const float ExileScale = 1.5f;

    internal static Dictionary<byte, Vector3> PoseLocalPositionOffsets { get; } = new()
    {
        [1] = Vector3.zero,
        [2] = Vector3.zero,
        [3] = new(-0.1f, 0.3f, 0),
        [4] = new(0.3f, 0.4f, 0f),
        [5] = new(0, -0.2f, 9f)
    };

    internal static Dictionary<byte, Vector3> ChatPoseLocalPositions { get; } = new()
    {
        [1] = new(0.1f, -0.3f, -0.5f),
        [2] = new(0.1f, -0.4f, -0.5f),
        [3] = new(-0.15f, -0.1f, -0.5f),
        [4] = new(0.3f, 0.4f, -0.5f),
        [5] = new(0f, -0.2f, 8.5f)
    };

    internal static Dictionary<byte, float> ChatPoseLocalScales { get; } = new()
    {
        [1] = ChatScale,
        [2] = ChatScale,
        [3] = ChatScale,
        [4] = ChatScale,
        [5] = ChatScale
    };

    internal static Dictionary<byte, Vector3> NamePlatePoseLocalPositions { get; } = new()
    {
        [1] = new(0f, -0.3f, -0.5f),
        [2] = new(0f, -0.3f, -0.5f),
        [3] = new(-0.1f, -0.1f, -0.5f),
        [4] = new(0.3f, 0.4f, -0.5f),
        [5] = new(0f, -0.5f, 8.5f)
    };

    internal static Dictionary<byte, float> NamePlatePoseLocalScales { get; } = new()
    {
        [1] = NamePlateScale,
        [2] = NamePlateScale,
        [3] = NamePlateScale,
        [4] = NamePlateScale,
        [5] = NamePlateScale
    };

    // 追放画面はマップごとに全ポーズ共通の位置・サイズを調整できる。
    internal static Dictionary<MapNames, (Vector3 LocalPosition, float LocalScale)> ExileTransformsByMap { get; } = new()
    {
        [MapNames.Skeld] = (new(0f, 0f, -0.5f), ExileScale),
        [MapNames.MiraHQ] = (new(0f, 0f, -0.5f), ExileScale),
        [MapNames.Polus] = (new(0f, 0f, -0.5f), ExileScale),
        [MapNames.Dleks] = (new(0f, 0f, -0.5f), ExileScale),
        [MapNames.Airship] = (new(0f, 0f, -0.5f), ExileScale),
        [MapNames.Fungle] = (new(0.2f, 0.4f, -0.5f), 1.35f)
    };

    public static void Refresh(
        PoolablePlayer target,
        NetworkedPlayerInfo playerInfo,
        BodyBuilderMuscleDisplayContext context)
    {
        if (target == null)
            return;

        Transform existing = target.transform.Find(MuscleObjectName);
        if (!ShouldDisplay(playerInfo))
        {
            Remove(target, existing);
            return;
        }

        SpriteRenderer referenceRenderer = target.cosmetics?.currentBodySprite?.BodySprite;
        if (referenceRenderer == null)
            return;

        if (existing != null)
        {
            HidePoolablePlayer(target, existing, context);
            ConfigurePose(existing.gameObject, referenceRenderer, playerInfo, context, GetPoseId(existing));
            return;
        }

        byte posingId = BodyBuilderAbility.NextPosingId();
        GameObject prefab = BodyBuilderAbility.GetPrefab(posingId);
        if (prefab == null)
            return;

        HidePoolablePlayer(target, null, context);
        GameObject pose = Object.Instantiate(prefab, target.transform);
        pose.name = MuscleObjectName;
        var poseIdMarker = new GameObject($"{PoseIdMarkerPrefix}{posingId}");
        poseIdMarker.transform.SetParent(pose.transform, false);
        ConfigurePose(pose, referenceRenderer, playerInfo, context, posingId);
    }

    public static void RefreshNamePlate(PlayerVoteArea voteArea, NetworkedPlayerInfo playerInfo)
    {
        if (voteArea == null)
            return;

        Refresh(voteArea.PlayerIcon, playerInfo, BodyBuilderMuscleDisplayContext.NamePlate);
        ConfigureNamePlateMask(voteArea);
    }

    private static void ConfigureNamePlateMask(PlayerVoteArea voteArea)
    {
        Transform existingMask = voteArea.transform.Find(NamePlateMaskObjectName);
        Transform pose = voteArea.PlayerIcon?.transform.Find(MuscleObjectName);
        SpriteRenderer maskArea = voteArea.MaskArea;
        SpriteRenderer referenceRenderer = voteArea.PlayerIcon?.cosmetics?.currentBodySprite?.BodySprite;
        if (pose == null || maskArea == null || maskArea.sprite == null || referenceRenderer == null)
        {
            if (existingMask != null)
            {
                existingMask.gameObject.SetActive(false);
                Object.Destroy(existingMask.gameObject);
            }
            return;
        }

        GameObject maskObject;
        SpriteMask spriteMask;
        if (existingMask == null)
        {
            maskObject = new GameObject(NamePlateMaskObjectName);
            maskObject.transform.SetParent(voteArea.transform, false);
            spriteMask = maskObject.AddComponent<SpriteMask>();
        }
        else
        {
            maskObject = existingMask.gameObject;
            spriteMask = maskObject.GetComponent<SpriteMask>() ?? maskObject.AddComponent<SpriteMask>();
        }

        maskObject.layer = maskArea.gameObject.layer;
        maskObject.transform.localRotation = maskArea.transform.localRotation;
        Vector3 maskPosition = maskArea.transform.localPosition;
        maskPosition.x += NamePlateMaskLeftInset * 0.5f;
        maskObject.transform.localPosition = maskPosition;
        Vector3 maskScale = maskArea.transform.localScale;
        maskScale.x = Mathf.Max(0.01f, maskScale.x - NamePlateMaskLeftInset);
        maskObject.transform.localScale = maskScale;

        spriteMask.sprite = maskArea.sprite;
        spriteMask.alphaCutoff = 0.001f;
        spriteMask.isCustomRangeActive = true;
        spriteMask.frontSortingLayerID = referenceRenderer.sortingLayerID;
        spriteMask.backSortingLayerID = referenceRenderer.sortingLayerID;
        spriteMask.frontSortingOrder = referenceRenderer.sortingOrder;
        spriteMask.backSortingOrder = referenceRenderer.sortingOrder;

        foreach (SpriteRenderer renderer in pose.GetComponentsInChildren<SpriteRenderer>(true))
            renderer.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
    }

    private static void ConfigurePose(
        GameObject pose,
        SpriteRenderer referenceRenderer,
        NetworkedPlayerInfo playerInfo,
        BodyBuilderMuscleDisplayContext context,
        byte posingId)
    {
        float localY = context switch
        {
            BodyBuilderMuscleDisplayContext.NamePlate => -0.3f,
            BodyBuilderMuscleDisplayContext.MeetingCall => -0.1f,
            _ => 0f
        };
        Vector3 poseOffset = PoseLocalPositionOffsets.TryGetValue(posingId, out Vector3 offset) ? offset : Vector3.zero;
        Vector3 defaultLocalPosition = new Vector3(0f, localY, -0.5f) + poseOffset;
        Vector3 localPosition = context switch
        {
            BodyBuilderMuscleDisplayContext.Chat when ChatPoseLocalPositions.TryGetValue(posingId, out Vector3 position) => position,
            BodyBuilderMuscleDisplayContext.NamePlate when NamePlatePoseLocalPositions.TryGetValue(posingId, out Vector3 position) => position,
            _ => defaultLocalPosition
        };
        pose.transform.localRotation = Quaternion.identity;
        float localScale = context switch
        {
            BodyBuilderMuscleDisplayContext.NamePlate when NamePlatePoseLocalScales.TryGetValue(posingId, out float scale) => scale,
            BodyBuilderMuscleDisplayContext.Chat when ChatPoseLocalScales.TryGetValue(posingId, out float scale) => scale,
            BodyBuilderMuscleDisplayContext.NamePlate => NamePlateScale,
            BodyBuilderMuscleDisplayContext.Chat => ChatScale,
            BodyBuilderMuscleDisplayContext.MeetingCall => MeetingCallScale,
            BodyBuilderMuscleDisplayContext.Exile => ExileScale,
            _ => 1f
        };
        if (context == BodyBuilderMuscleDisplayContext.Exile
            && TryGetExileTransform(out Vector3 exilePosition, out float exileScale))
        {
            localPosition = exilePosition;
            localScale = exileScale;
        }

        pose.transform.localPosition = localPosition;
        pose.transform.localScale = Vector3.one * localScale;

        int displayLayer = referenceRenderer.gameObject.layer;
        foreach (Transform child in pose.GetComponentsInChildren<Transform>(true))
            child.gameObject.layer = displayLayer;

        foreach (SpriteRenderer renderer in pose.GetComponentsInChildren<SpriteRenderer>(true))
        {
            renderer.gameObject.layer = displayLayer;
            renderer.sortingLayerID = referenceRenderer.sortingLayerID;
            renderer.sortingOrder = referenceRenderer.sortingOrder;
            renderer.maskInteraction = referenceRenderer.maskInteraction;
            renderer.sharedMaterial = referenceRenderer.sharedMaterial;
            PlayerMaterial.SetColors(playerInfo.DefaultOutfit.ColorId, renderer);
            renderer.color = new(1f, 1f, 1f, playerInfo.IsDead && context != BodyBuilderMuscleDisplayContext.Exile ? 0.5f : 1f);
        }
    }

    private static byte GetPoseId(Transform pose)
    {
        foreach (byte posingId in PoseLocalPositionOffsets.Keys)
        {
            if (pose.Find($"{PoseIdMarkerPrefix}{posingId}") != null)
                return posingId;
        }
        return 0;
    }

    private static bool TryGetExileTransform(out Vector3 localPosition, out float localScale)
    {
        localPosition = Vector3.zero;
        localScale = ExileScale;

        var gameOptions = GameOptionsManager.Instance?.CurrentGameOptions;
        if (gameOptions == null)
            return false;

        MapNames map = (MapNames)gameOptions.MapId;
        if (!ExileTransformsByMap.TryGetValue(map, out (Vector3 LocalPosition, float LocalScale) transform))
            return false;

        localPosition = transform.LocalPosition;
        localScale = transform.LocalScale;
        return true;
    }

    private static bool ShouldDisplay(NetworkedPlayerInfo playerInfo)
    {
        if (!BodyBuilder.BodyBuilderMoreMuscular || playerInfo == null)
            return false;

        ExPlayerControl player = ExPlayerControl.ById(playerInfo.PlayerId);
        return player != null && player.Role == RoleId.BodyBuilder && player.IsTaskComplete();
    }

    private static void HidePoolablePlayer(PoolablePlayer target, Transform pose, BodyBuilderMuscleDisplayContext context)
    {
        foreach (SpriteRenderer renderer in target.GetComponentsInChildren<SpriteRenderer>(true))
        {
            if (renderer == null || pose != null && renderer.transform.IsChildOf(pose))
                continue;
            if (context == BodyBuilderMuscleDisplayContext.MeetingCall && IsMeetingButtonRenderer(renderer, target.transform))
            {
                ReleaseForcedRendering(renderer);
                continue;
            }
            renderer.forceRenderingOff = true;
        }
    }

    public static void EnsureMeetingButtonsVisible(MeetingCalledAnimation animation)
    {
        if (animation == null)
            return;

        RestoreButtons(animation.emergencyParent);
        RestoreButtons(animation.emergencyClassicParent);

        static void RestoreButtons(GameObject root)
        {
            if (root == null)
                return;
            foreach (SpriteRenderer renderer in root.GetComponentsInChildren<SpriteRenderer>(true))
            {
                if (renderer != null && IsMeetingButtonRenderer(renderer, root.transform))
                    ReleaseForcedRendering(renderer);
            }
        }
    }

    public static void FadeOutExilePose(PoolablePlayer target)
    {
        Transform pose = target?.transform.Find(MuscleObjectName);
        if (pose == null)
            return;

        SpriteRenderer[] renderers = pose.GetComponentsInChildren<SpriteRenderer>(true);
        Color[] initialColors = renderers.Select(renderer => renderer.color).ToArray();
        target.StartCoroutine(Effects.Lerp(0.6f, new Action<float>(progress =>
        {
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] == null)
                    continue;

                Color color = initialColors[i];
                color.a *= 1f - progress;
                renderers[i].color = color;
            }

            if (progress >= 1f && pose != null)
                pose.gameObject.SetActive(false);
        })));
    }

    private static bool IsMeetingButtonRenderer(SpriteRenderer renderer, Transform root)
    {
        for (Transform current = renderer.transform; current != null; current = current.parent)
        {
            if (current.name.Contains("Button", StringComparison.OrdinalIgnoreCase))
                return true;
            if (current == root)
                break;
        }
        return false;
    }

    private static void Remove(PoolablePlayer target, Transform pose)
    {
        if (pose == null)
            return;

        pose.gameObject.SetActive(false);
        Object.Destroy(pose.gameObject);
        foreach (SpriteRenderer renderer in target.GetComponentsInChildren<SpriteRenderer>(true))
        {
            if (renderer == null || renderer.transform.IsChildOf(pose))
                continue;
            ReleaseForcedRendering(renderer);
        }
    }

    private static void ReleaseForcedRendering(SpriteRenderer renderer)
    {
        renderer.forceRenderingOff = false;
        if (!renderer.enabled)
            return;
        renderer.enabled = false;
        renderer.enabled = true;
    }
}

internal enum BodyBuilderMuscleDisplayContext
{
    NamePlate,
    Chat,
    MeetingCall,
    Exile
}

[HarmonyPatch(typeof(PlayerVoteArea), nameof(PlayerVoteArea.SetCosmetics))]
public static class BodyBuilderPlayerVoteAreaSetCosmeticsPatch
{
    [HarmonyPriority(Priority.Last)]
    public static void Postfix(PlayerVoteArea __instance, NetworkedPlayerInfo playerInfo)
        => BodyBuilderMuscleDisplay.RefreshNamePlate(__instance, playerInfo);
}

[HarmonyPatch(typeof(ChatBubble), nameof(ChatBubble.SetCosmetics))]
public static class BodyBuilderChatBubbleSetCosmeticsPatch
{
    [HarmonyPriority(Priority.Last)]
    public static void Postfix(ChatBubble __instance, NetworkedPlayerInfo playerInfo)
        => BodyBuilderMuscleDisplay.Refresh(__instance.Player, playerInfo, BodyBuilderMuscleDisplayContext.Chat);
}

[HarmonyPatch(typeof(ExileController), nameof(ExileController.Begin))]
public static class BodyBuilderExileControllerBeginPatch
{
    [HarmonyPriority(Priority.Last)]
    public static void Postfix(ExileController __instance)
        => BodyBuilderMuscleDisplay.Refresh(__instance.Player, __instance.initData.networkedPlayer, BodyBuilderMuscleDisplayContext.Exile);
}

[HarmonyCoroutinePatch(typeof(FungleExileController), "FadeBlackRaftAndPlayer")]
public static class BodyBuilderFungleExileFirePatch
{
    public static void Postfix(object __instance, bool __result)
    {
        // コルーチン終了時には、黒フェードが完了して炎が有効化されている。
        if (__result)
            return;

        FungleExileController controller = HarmonyCoroutinePatchProcessor.GetParentFromCoroutine<FungleExileController>(__instance);
        BodyBuilderMuscleDisplay.FadeOutExilePose(controller?.Player);
    }
}
