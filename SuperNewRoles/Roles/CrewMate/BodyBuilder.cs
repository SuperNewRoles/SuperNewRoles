
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Agartha;
using AmongUs.GameOptions;
using HarmonyLib;
using Hazel;
using SuperNewRoles.CustomCosmetics.CustomCosmeticsData;
using SuperNewRoles.Patches;
using SuperNewRoles.Roles.Attribute;
using SuperNewRoles.Roles.Role;
using SuperNewRoles.Roles.RoleBases;
using SuperNewRoles.Roles.RoleBases.Interfaces;
using UnityEngine;

namespace SuperNewRoles.Roles.Crewmate.BodyBuilder;

// 提案者：Cade Mofu さん。ありがとうございます！
[HarmonyPatch]
public class BodyBuilder : InvisibleRoleBase, ICrewmate, ICustomButton, IDeathHandler, IRpcHandler, IHandleChangeRole
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
        writer.Write(UnityEngine.Random.Range(1, 5));
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
            if (!ChangeAllTaskLiftWeights.GetBool() || !PlayerControl.LocalPlayer.IsRole(RoleId.BodyBuilder))
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
            if (!ChangeAllTaskLiftWeights.GetBool() || !PlayerControl.LocalPlayer.IsRole(RoleId.BodyBuilder))
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
        if (ChangeAllTaskLiftWeights.GetBool())
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

        cancelPosing();
        Player.NetTransform.Halt();

        var distance = Vector2.Distance(CachedPlayer.LocalPlayer.transform.position, Player.NetTransform.transform.position);
        var volume = 1 / distance <= 0.25f ? 0f : 1 / distance;
        ShipStatus ship = GameManager.Instance.LogicOptions.MapId == (int)MapNames.Fungle ? ShipStatus.Instance : MapLoader.Fungle;
        AudioClip clip = ship.ShortTasks.FirstOrDefault(x => x.TaskType == TaskTypes.LiftWeights).MinigamePrefab.TryCast<LiftWeightsMinigame>().completeAllRepsSound;
        ModHelpers.PlaySound(Player.NetTransform.transform, clip, false, volume);

        var prefab = getPrefab(id);
        var pose = UnityEngine.Object.Instantiate(prefab, Player.NetTransform.transform);
        SetInvisibleRPC(Player.PlayerId, (byte)RpcType.Start, Player.PlayerId);

        var pos = pose.gameObject.transform.position;
        pos.z -= 0.5f;
        pose.gameObject.transform.position = pos;

        var spriteRenderer = pose.GetComponent<SpriteRenderer>();
        spriteRenderer.sharedMaterial = FastDestroyableSingleton<HatManager>.Instance.PlayerMaterial;
        spriteRenderer.maskInteraction = SpriteMaskInteraction.None;
        PlayerMaterial.SetMaskLayerBasedOnLocalPlayer(spriteRenderer, false);
        PlayerMaterial.SetColors(Player.Data.DefaultOutfit.ColorId, spriteRenderer);
        spriteRenderer.color = new(1f, 1f, 1f, 1f / (Convert.ToInt32(Player.IsDead()) * 2));

        myObject = pose;
    }
    private void cancelPosing()
    {
        SetInvisibleRPC(Player.PlayerId, (byte)RpcType.End, Player.PlayerId);

        if (myObject != null)
            GameObject.Destroy(myObject);
    }
    [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.SetNormalizedVelocity)), HarmonyPostfix]
    static void onMovePlayer(PlayerPhysics __instance, [HarmonyArgument(0)] Vector2 direction)
    {
        PlayerControl player = __instance.myPlayer;
        if (!player.IsRole(RoleId.BodyBuilder) || direction == Vector2.zero)
            return;

        player.GetRoleBase<BodyBuilder>().useAbility(false);
    }
    public void OnChangeRole()
        => useAbility(false);
    public void OnAmDeath(DeathInfo deathInfo)
        => useAbility(false);
}