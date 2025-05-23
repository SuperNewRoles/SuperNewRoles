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

    private bool changeAllTaskLiftWeights;
    private TaskOptionData? taskOptionData;
    private GameObject posingObject;
    private Vector3 lastPosition = Vector3.zero;

    // ポーズID範囲
    private static readonly IntRange PosingIdRange = new(1, 5);

    // アセットバンドル
    private EventListener<MurderEventData> _murderEvent;
    private EventListener<MeetingStartEventData> _meetingStartEvent;
    private EventListener<ExileEventData> _exileEvent;
    private EventListener<PlayerPhysicsFixedUpdateEventData> _onPlayerPhysicsFixedUpdateEvent;

    public BodyBuilderAbility(bool changeAllTaskLiftWeights, TaskOptionData? taskOptionData)
    {
        this.changeAllTaskLiftWeights = changeAllTaskLiftWeights;
        this.taskOptionData = taskOptionData;
    }

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
        return Player == ExPlayerControl.LocalPlayer && Player.Player.AllTasksCompleted();
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
        Logger.Info("BodyBuilder ポージング開始");
        // 使用者が霊界かつ自身が霊界でないならreturn
        if (Player.IsDead() && !PlayerControl.LocalPlayer.Data.IsDead)
            return;
        Logger.Info("BodyBuilder ポージング開始2");

        CancelPosing();
        Player.NetTransform.Halt();
        Logger.Info("BodyBuilder ポージング開始3");

        // 音を再生
        var distance = Vector2.Distance(PlayerControl.LocalPlayer.transform.position, Player.transform.position);
        var volume = 1 / distance <= 0.25f ? 0f : 1 / distance;
        ShipStatus ship = MapLoader.Fungle;
        var liftWeightsTask = ship.ShortTasks.FirstOrDefault(x => x.TaskType == TaskTypes.LiftWeights);
        if (liftWeightsTask != null)
        {
            var liftWeightsMinigame = liftWeightsTask.MinigamePrefab.TryCast<LiftWeightsMinigame>();
            if (liftWeightsMinigame != null)
            {
                ModHelpers.PlaySound(Player.transform, liftWeightsMinigame.completeAllRepsSound, false, volume, null);
            }
        }
        Logger.Info("BodyBuilder ポージング開始4");

        // ポーズプレハブを生成
        var prefab = GetPrefab(posingId);
        var pose = Object.Instantiate(prefab, Player.NetTransform.transform);
        foreach (SpriteRenderer renderer in Player.Player.gameObject.GetComponentsInChildren<SpriteRenderer>())
        {
            renderer.color = new(1f, 1f, 1f, 0f);
        }
        Logger.Info("BodyBuilder ポージング開始5");

        var pos = pose.gameObject.transform.position;
        pos.z -= 0.5f;
        pos.y += 0.6f;
        pose.gameObject.transform.position = pos;
        pose.gameObject.transform.localScale = Player.cosmetics.currentBodySprite.BodySprite.transform.localScale;

        var spriteRenderer = pose.GetComponent<SpriteRenderer>();
        spriteRenderer.sharedMaterial = FastDestroyableSingleton<HatManager>.Instance.PlayerMaterial;
        spriteRenderer.maskInteraction = SpriteMaskInteraction.None;
        PlayerMaterial.SetMaskLayerBasedOnLocalPlayer(spriteRenderer, false);
        PlayerMaterial.SetColors(Player.Data.DefaultOutfit.ColorId, spriteRenderer);
        spriteRenderer.color = new(1f, 1f, 1f, Player.IsDead() ? 0.5f : 1f);

        posingObject = pose;
        Logger.Info("BodyBuilder ポージング開始6");
    }

    private void CancelPosing()
    {
        Logger.Info("BodyBuilder Remove");
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

// ウェイトリフティングタスクのパッチ
[HarmonyPatch]
public static class BodyBuilderTaskPatches
{
    [HarmonyPatch(typeof(Console), nameof(Console.Use))]
    public static class ConsolePatch
    {
        private static Minigame preMinigame;

        static void Prefix(Console __instance)
        {
            if (!ExPlayerControl.LocalPlayer.HasAbility<BodyBuilderAbility>()) return;

            // Fungle以外でも全タスクをウェイトリフティングに変更する設定がOFFの場合は処理しない
            if (!SuperNewRoles.Roles.Crewmate.BodyBuilder.ChangeAllTaskLiftWeights &&
                GameManager.Instance.LogicOptions.currentGameOptions.MapId != (byte)MapNames.Fungle)
                return;

            __instance.CanUse(PlayerControl.LocalPlayer.Data, out bool canUse, out bool _);
            if (!canUse) return;

            PlayerTask task = __instance.FindTask(PlayerControl.LocalPlayer);
            if (task.TaskType is TaskTypes.FixLights or TaskTypes.RestoreOxy or TaskTypes.ResetReactor or
                TaskTypes.ResetSeismic or TaskTypes.FixComms or TaskTypes.StopCharles or TaskTypes.MushroomMixupSabotage)
                return;

            preMinigame = task.MinigamePrefab;
            ShipStatus ship = GameManager.Instance.LogicOptions.MapId == (int)MapNames.Fungle ? ShipStatus.Instance : MapLoader.Fungle;
            var liftWeightsTask = ship.ShortTasks.FirstOrDefault(x => x.TaskType == TaskTypes.LiftWeights);
            if (liftWeightsTask != null)
            {
                task.MinigamePrefab = liftWeightsTask.MinigamePrefab;
            }
        }

        static void Postfix(Console __instance)
        {
            var bodyBuilderPlayer = ExPlayerControl.ExPlayerControls.FirstOrDefault(x => x.Role == RoleId.BodyBuilder && x.Player == PlayerControl.LocalPlayer);
            if (bodyBuilderPlayer == null) return;

            if (!SuperNewRoles.Roles.Crewmate.BodyBuilder.ChangeAllTaskLiftWeights &&
                GameManager.Instance.LogicOptions.currentGameOptions.MapId != (byte)MapNames.Fungle)
                return;

            __instance.CanUse(PlayerControl.LocalPlayer.Data, out bool canUse, out bool _);
            if (!canUse) return;

            PlayerTask task = __instance.FindTask(PlayerControl.LocalPlayer);
            if (preMinigame != null)
            {
                task.MinigamePrefab = preMinigame;
                preMinigame = null;
            }
        }
    }

    [HarmonyPatch(typeof(LiftWeightsMinigame))]
    public static class LiftWeightsMinigamePatch
    {
        [HarmonyPatch(nameof(LiftWeightsMinigame.Begin)), HarmonyPrefix]
        public static void BeginPrefix(PlayerTask task, ref int __state)
        {
            if (task is NormalPlayerTask normal) __state = normal.MaxStep;
            else __state = 1;
        }

        [HarmonyPatch(nameof(LiftWeightsMinigame.Begin)), HarmonyPostfix]
        public static void BeginPostfix(LiftWeightsMinigame __instance, ref int __state)
        {
            if (!ExPlayerControl.LocalPlayer.HasAbility<BodyBuilderAbility>()) return;

            __instance.MyNormTask.MaxStep = __state;
            List<SpriteRenderer> sprites = new(__instance.counters);

            while (__instance.MyNormTask.MaxStep > sprites.Count)
            {
                SpriteRenderer sprite = Object.Instantiate(sprites[0], sprites[0].transform.parent);
                sprite.name = $"Counter {sprites.Count + 1}";
                sprites.Add(sprite);
            }

            while (__instance.MyNormTask.MaxStep < sprites.Count)
            {
                SpriteRenderer sprite = sprites.Last();
                sprites.RemoveAt(sprites.Count - 1);
                Object.Destroy(sprite.gameObject);
            }

            __instance.counters = sprites.ToArray();

            for (int i = 0; i < __instance.counters.Length; i++)
            {
                Vector3 pos = new(0.282f * (i / 5), 0.564f - 0.282f * (i % 5));
                __instance.counters[i].transform.localPosition = pos;
                __instance.counters[i].color = __instance.MyNormTask.TaskStep > i ? Color.green : new(0.5849f, 0.5849f, 0.5849f);
            }
            __instance.OnValidate();
        }

        [HarmonyPatch(nameof(LiftWeightsMinigame.EndLifting)), HarmonyPrefix]
        public static bool EndLiftingPrefix(LiftWeightsMinigame __instance)
        {
            if (!ExPlayerControl.LocalPlayer.HasAbility<BodyBuilderAbility>()) return true;

            if (__instance.state != LiftWeightsMinigame.State.Lifting) return false;

            if (__instance.validFillPercentRange.Contains(__instance.currentBarFillPercent))
            {
                __instance.counters[__instance.MyNormTask.taskStep].color = Color.green;
                if (Constants.ShouldPlaySfx()) SoundManager.Instance.PlaySound(__instance.completeRepSound, false, 1f, null);
                __instance.StartCoroutine(Effects.Bloop(0f, __instance.counters[__instance.MyNormTask.taskStep].transform, __instance.counters[__instance.MyNormTask.taskStep].transform.localScale.x, 0.5f));
                __instance.MyNormTask.NextStep();
                VibrationManager.Vibrate(0.7f, 0.7f, 0.2f, VibrationManager.VibrationFalloff.None, null, false, "");

                if (__instance.MyNormTask.IsComplete)
                {
                    if (Constants.ShouldPlaySfx())
                    {
                        __instance.StartCoroutine(Effects.Sequence(new IEnumerator[]
                        {
                            Effects.Wait(0.1f),
                            Effects.Action((Action)(() => SoundManager.Instance.PlaySound(__instance.completeAllRepsSound, false, 1f, null)))
                        }));
                    }
                    __instance.StartCoroutine(__instance.CoStartClose(0.75f));
                }
                else
                {
                    PlayerControl player = __instance.MyTask.Owner;
                    Console console = __instance.Console;
                    Vector2 truePosition = player.GetTruePosition();
                    Vector3 position = console.transform.position;
                    bool use = player.Data.Role.CanUse(console.TryCast<IUsable>()) && (!console.onlySameRoom || console.InRoom(truePosition)) &&
                              (!console.onlyFromBelow || truePosition.y < position.y) && console.FindTask(player);

                    if (use)
                    {
                        float distance = Vector2.Distance(truePosition, console.transform.position);
                        use &= distance <= console.UsableDistance;
                        if (console.checkWalls) use &= !PhysicsHelpers.AnythingBetween(truePosition, position, Constants.ShadowMask, false);
                    }

                    if (!use) __instance.StartCoroutine(__instance.CoStartClose(0.75f));
                }
            }
            else
            {
                __instance.fillBar.color = Color.red;
                if (Constants.ShouldPlaySfx()) SoundManager.Instance.PlaySound(__instance.failRepSound, false, 1f, null);
            }

            __instance.barfillAudioSource.Stop();
            __instance.barfillAudioSource.volume = 0f;
            __instance.state = LiftWeightsMinigame.State.Dropping;
            return false;
        }
    }

    [HarmonyPatch(typeof(NormalPlayerTask))]
    public static class NormalPlayerTaskPatch
    {
        [HarmonyPatch(nameof(NormalPlayerTask.Initialize)), HarmonyPostfix]
        public static void InitializePostfix(NormalPlayerTask __instance)
        {
            if (__instance.TaskType != TaskTypes.LiftWeights) return;
            __instance.MaxStep = 3;
        }
    }
}