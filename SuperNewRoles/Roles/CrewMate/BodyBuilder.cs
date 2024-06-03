using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Agartha;
using AmongUs.GameOptions;
using HarmonyLib;
using Hazel;
using SuperNewRoles.Roles.Role;
using SuperNewRoles.Roles.RoleBases;
using SuperNewRoles.Roles.RoleBases.Interfaces;
using UnityEngine;
using Action = System.Action;
using IEnumerator = Il2CppSystem.Collections.IEnumerator;
using Object = UnityEngine.Object;

namespace SuperNewRoles.Roles.Crewmate.BodyBuilder;

// 提案者：Cade Mofu さん。ありがとうございます！
[HarmonyPatch]
public class BodyBuilder : RoleBase, ICrewmate, ICustomButton, IDeathHandler, IHandleChangeRole, IMeetingHandler, IRpcHandler
{
    public static new RoleInfo Roleinfo = new(
        typeof(BodyBuilder),
        (p) => new BodyBuilder(p),
        RoleId.BodyBuilder,
        "BodyBuilder",
        new(214, 143, 94, byte.MaxValue),
        new(RoleId.BodyBuilder, TeamTag.Crewmate),
        TeamRoleType.Crewmate,
        TeamType.Crewmate
        );
    public static new OptionInfo Optioninfo =
        new(RoleId.BodyBuilder, 452900, false,
            optionCreator: CreateOption);
    public static new IntroInfo Introinfo =
        new(RoleId.BodyBuilder, introSound: RoleTypes.Crewmate);

    public static readonly IntRange PosingIdRange = new(1, 5);

    public static CustomOption ChangeAllTaskLiftWeights;
    public static CustomOption CustomTaskNumAvailable;
    public static CustomOption CustomCommonTaskNum;
    public static CustomOption CustomShortTaskNum;
    public static CustomOption CustomLongTaskNum;
    private static void CreateOption()
    {
        ChangeAllTaskLiftWeights = CustomOption.Create(Optioninfo.OptionId++, false, CustomOptionType.Crewmate, "BodyBuilderChangeAllTaskLiftWeights", false, Optioninfo.RoleOption);
        CustomTaskNumAvailable = CustomOption.Create(Optioninfo.OptionId++, false, CustomOptionType.Crewmate, "IsSettingNumberOfUniqueTasks", true, Optioninfo.RoleOption);
        var taskOption = Patches.SelectTask.TaskSetting(Optioninfo.OptionId++, Optioninfo.OptionId++, Optioninfo.OptionId++, CustomTaskNumAvailable, CustomOptionType.Crewmate);
        CustomCommonTaskNum = taskOption.Item1;
        CustomShortTaskNum = taskOption.Item2;
        CustomLongTaskNum = taskOption.Item3;
    }

    public CustomButtonInfo[] CustomButtonInfos { get; }
    private CustomButtonInfo bodyBuilderButton { get; }
    public BodyBuilder(PlayerControl p) : base(p, Roleinfo, Optioninfo, Introinfo)
    {
        bodyBuilderButton = new(null, this, () => useAbility(),
            (isAlive) => Player.AllTasksCompleted(),
            CustomButtonCouldType.CanMove | CustomButtonCouldType.NotMoving,
            null, ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.BodyBuilder.BodyBuilderButton.png", 115f),
            null, new(-2f, 1, 0),
            "BodyBuilderButtonName", KeyCode.F
            );
        CustomButtonInfos = new CustomButtonInfo[1] { bodyBuilderButton };
    }

    private void useAbility(bool active = true)
    {
        RpcTypes type = active ? RpcTypes.Posing : RpcTypes.CancelPosing;
        MessageWriter writer = RpcWriter;
        writer.Write((byte)type);
        writer.Write(PosingIdRange.Next());
        SendRpc(writer);
    }

    private enum RpcTypes
    {
        Posing,
        CancelPosing,
        EndGame,
    }

    public void RpcReader(MessageReader reader)
    {
        RpcTypes type = (RpcTypes)reader.ReadByte();
        byte id = reader.ReadByte();

        switch (type)
        {
            case RpcTypes.Posing:
                playPosing(id);
                break;
            case RpcTypes.CancelPosing:
                cancelPosing();
                break;
        }
    }

    // タスク関係
    [HarmonyPatch(typeof(Console), nameof(Console.Use))]
    class ConsolePatch
    {
        private static Minigame preMinigame;
        static void Prefix(Console __instance)
        {
            if ((!ChangeAllTaskLiftWeights.GetBool() && GameManager.Instance.LogicOptions.currentGameOptions.MapId == (byte)MapNames.Fungle) || !PlayerControl.LocalPlayer.IsRole(RoleId.BodyBuilder))
                return;

            __instance.CanUse(PlayerControl.LocalPlayer.Data, out bool canUse, out bool _);

            if (!canUse)
                return;

            PlayerTask task = __instance.FindTask(CachedPlayer.LocalPlayer);
            if (task.TaskType is TaskTypes.FixLights or TaskTypes.RestoreOxy or TaskTypes.ResetReactor or TaskTypes.ResetSeismic or TaskTypes.FixComms or TaskTypes.StopCharles or TaskTypes.MushroomMixupSabotage)
                return;
            preMinigame = task.MinigamePrefab;
            ShipStatus ship = GameManager.Instance.LogicOptions.MapId == (int)MapNames.Fungle ? ShipStatus.Instance : MapLoader.Fungle;
            task.MinigamePrefab = ship.ShortTasks.FirstOrDefault(x => x.TaskType == TaskTypes.LiftWeights).MinigamePrefab;
        }

        static void Postfix(Console __instance)
        {
            if ((!ChangeAllTaskLiftWeights.GetBool() && GameManager.Instance.LogicOptions.currentGameOptions.MapId == (byte)MapNames.Fungle) || !PlayerControl.LocalPlayer.IsRole(RoleId.BodyBuilder))
                return;

            __instance.CanUse(PlayerControl.LocalPlayer.Data, out bool canUse, out bool _);

            if (!canUse)
                return;

            PlayerTask task = __instance.FindTask(CachedPlayer.LocalPlayer);
            task.MinigamePrefab = preMinigame;
            preMinigame = null;
        }
    }
    public bool HaveMyNumTask(out (int numCommon, int numShort, int numLong)? TaskData)
    {
        if (!CustomTaskNumAvailable.GetBool())
        {
            TaskData = null;
            return false;
        }
        TaskData = new(CustomCommonTaskNum.GetInt(), CustomShortTaskNum.GetInt(), CustomLongTaskNum.GetInt());
        return true;
    }
    public bool AssignTask(out List<byte> tasks, (int numCommon, int numShort, int numLong) TaskData)
    {
        if (ChangeAllTaskLiftWeights.GetBool() || GameManager.Instance.LogicOptions.currentGameOptions.MapId != (byte)MapNames.Fungle)
        {
            tasks = null;
            return false;
        }

        tasks = new();
        var count = TaskData.numCommon + TaskData.numShort + TaskData.numLong;
        var task = MapUtilities.CachedShipStatus.ShortTasks.FirstOrDefault(x => x.TaskType == TaskTypes.LiftWeights);

        for (int i = 0; i < count; i++)
            tasks.Add((byte)task.Index);

        return true;
    }



    // ポージング関係
    public static Dictionary<byte, GameObject> Prefabs { get; private set; } = new();
    private GameObject myObject;
    private static Stream resourceAudioAssetBundleStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("SuperNewRoles.Resources.BodyBuilder.BodyBuilderPoses.bundle");
    private static AssetBundle assetBundleBundle = AssetBundle.LoadFromMemory(resourceAudioAssetBundleStream.ReadFully());
    public static GameObject getPrefab(byte id)
    {
        if (!Prefabs.TryGetValue(id, out GameObject prefab))
            Prefabs[id] = prefab = assetBundleBundle.LoadAsset<GameObject>($"BodyBuilderAnim0{id}.prefab").DontUnload();

        return prefab;
    }
    private void playPosing(byte id)
    {
        //使用者が霊界かつ自身が霊界でないならreturn
        if (Player.IsDead() && PlayerControl.LocalPlayer.IsAlive())
            return;

        cancelPosing(myObject);
        Player.NetTransform.Halt();

        var distance = Vector2.Distance(CachedPlayer.LocalPlayer.transform.position, Player.NetTransform.transform.position);
        var volume = 1 / distance <= 0.25f ? 0f : 1 / distance;
        ShipStatus ship = GameManager.Instance.LogicOptions.MapId == (int)MapNames.Fungle ? ShipStatus.Instance : MapLoader.Fungle;
        AudioClip clip = ship.ShortTasks.FirstOrDefault(x => x.TaskType == TaskTypes.LiftWeights).MinigamePrefab.TryCast<LiftWeightsMinigame>().completeAllRepsSound;
        ModHelpers.PlaySound(Player.NetTransform.transform, clip, false, volume);

        var prefab = getPrefab(id);
        var pose = Object.Instantiate(prefab, Player.NetTransform.transform);
        Player.gameObject.GetComponentsInChildren<SpriteRenderer>().ForEach(x => x.color = new(1f, 1f, 1f, 0f));

        var pos = pose.gameObject.transform.position;
        pos.z -= 0.5f;
        pose.gameObject.transform.position = pos;

        var spriteRenderer = pose.GetComponent<SpriteRenderer>();
        spriteRenderer.sharedMaterial = FastDestroyableSingleton<HatManager>.Instance.PlayerMaterial;
        spriteRenderer.maskInteraction = SpriteMaskInteraction.None;
        PlayerMaterial.SetMaskLayerBasedOnLocalPlayer(spriteRenderer, false);
        PlayerMaterial.SetColors(Player.Data.DefaultOutfit.ColorId, spriteRenderer);
        spriteRenderer.color = new(1f, 1f, 1f, Player.IsDead() ? 0.5f : 1f);

        myObject = pose;
    }
    private void cancelPosing(bool wasPosing = false)
    {
        if (Player != null)
            Player.gameObject.GetComponentsInChildren<SpriteRenderer>().ForEach(x => x.color = new(1f, 1f, 1f, wasPosing ? 0f : 1f));

        if (myObject != null)
            Object.Destroy(myObject);
        myObject = null;
    }
    [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.SetNormalizedVelocity)), HarmonyPostfix]
    static void onMovePlayer(PlayerPhysics __instance, [HarmonyArgument(0)] Vector2 direction)
    {
        PlayerControl player = __instance.myPlayer;
        if (!player.IsRole(RoleId.BodyBuilder) || direction == Vector2.zero || player.GetRoleBase<BodyBuilder>().myObject == null)
            return;

        player.GetRoleBase<BodyBuilder>().useAbility(false);
    }
    public void OnChangeRole()
        => useAbility(false);
    public void OnAmDeath(DeathInfo deathInfo)
        => useAbility(false);
    public void StartMeeting()
        => useAbility(false);
    public void CloseMeeting() { }

    [HarmonyPatch(typeof(LiftWeightsMinigame))]
    public static class LiftWeightsMinigamePatch
    {
        [HarmonyPatch(nameof(LiftWeightsMinigame.Begin)), HarmonyPrefix]
        public static void BeginPrefix(PlayerTask task, ref int __state)
        {
            if (task.Il2CppIs(out NormalPlayerTask normal)) __state = normal.MaxStep;
            else __state = 1;
        }

        [HarmonyPatch(nameof(LiftWeightsMinigame.Begin)), HarmonyPostfix]
        public static void BeginPostfix(LiftWeightsMinigame __instance, ref int __state)
        {
            if (!PlayerControl.LocalPlayer.IsRole(RoleId.BodyBuilder)) return;
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
            return;
        }

        [HarmonyPatch(nameof(LiftWeightsMinigame.EndLifting)), HarmonyPrefix]
        public static bool EndLiftingPrefix(LiftWeightsMinigame __instance)
        {
            if (!PlayerControl.LocalPlayer.IsRole(RoleId.BodyBuilder)) return true;
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
                    bool use = false;
                    if (console.Il2CppIs(out IUsable usable))
                    {
                        use = player.Data.Role.CanUse(usable) && (!console.onlySameRoom || console.InRoom(truePosition)) && (!console.onlyFromBelow || truePosition.y < position.y) && console.FindTask(player);
                        float num = float.MaxValue;
                        if (use)
                        {
                            num = Vector2.Distance(truePosition, console.transform.position);
                            use &= num <= console.UsableDistance;
                            if (console.checkWalls) use &= !PhysicsHelpers.AnythingBetween(truePosition, position, Constants.ShadowMask, false);
                        }
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