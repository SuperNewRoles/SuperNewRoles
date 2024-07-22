using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AmongUs.GameOptions;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using HarmonyLib;
using Hazel;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using SuperNewRoles.Helpers;
using SuperNewRoles.Mode;
using SuperNewRoles.Mode.SuperHostRoles;
using SuperNewRoles.Roles;
using SuperNewRoles.Roles.Attribute;
using SuperNewRoles.Roles.Crewmate;
using SuperNewRoles.Roles.Neutral;
using SuperNewRoles.Roles.RoleBases;
using SuperNewRoles.Roles.RoleBases.Interfaces;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using static UnityEngine.GraphicsBuffer;

namespace SuperNewRoles;

public static class ModHelpers
{
    public enum MurderAttemptResult
    {
        PerformKill,
        SuppressKill,
        BlankKill,
        GuardianGuardKill
    }
    public static bool ShowButtons
    {
        get
        {
            return !(MapBehaviour.Instance && MapBehaviour.Instance.IsOpen) &&
                    !MeetingHud.Instance &&
                    !ExileController.Instance;
        }
    }
    public static TKey GetKeyByValue<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TValue value, TKey defaultvalue = default)
    {
        foreach (var pair in dictionary)
        {
            if (EqualityComparer<TValue>.Default.Equals(pair.Value, value))
            {
                return pair.Key;
            }
        }
        return defaultvalue; // 見つからなかった場合
    }
    public static int GetAlivePlayerCount()
    {
        int count = 0;
        foreach (PlayerControl p in CachedPlayer.AllPlayers.AsSpan())
            if (p.IsAlive()) count++;
        return count;
    }
    public static byte ParseToByte(this string txt)
    {
        return byte.Parse(txt.ToString());
    }
    public static Ladder LadderById(byte id)
    {
        return ShipStatus.Instance.Ladders.FirstOrDefault((Ladder f) => f.Id == id);
    }
    public static Vent SetTargetVent(List<Vent> untargetablePlayers = null, PlayerControl targetingPlayer = null, bool forceout = false)
    {
        Vent result = null;
        float num = GameOptionsData.KillDistances[Mathf.Clamp(GameManager.Instance.LogicOptions.currentGameOptions.GetInt(Int32OptionNames.KillDistance), 0, 2)];
        if (!MapUtilities.CachedShipStatus) return result;
        if (targetingPlayer == null) targetingPlayer = PlayerControl.LocalPlayer;
        if (targetingPlayer.Data.IsDead || targetingPlayer.inVent) return result;

        if (untargetablePlayers == null)
        {
            untargetablePlayers = new();
        }

        Vector2 truePosition = targetingPlayer.GetTruePosition();
        var allPlayers = ShipStatus.Instance.AllVents;
        for (int i = 0; i < allPlayers.Count; i++)
        {
            Vent ventInfo = allPlayers[i];
            if (untargetablePlayers.Any(x => x == ventInfo))
            {
                continue;
            }
            if (ventInfo && (NiceMechanic.TargetVent.All(x => x.Value is null || x.Value.Id != ventInfo.Id) || forceout))
            {
                Vector2 vector = new Vector2(ventInfo.transform.position.x, ventInfo.transform.position.y) - truePosition;
                float magnitude = vector.magnitude;
                if (magnitude <= num && !PhysicsHelpers.AnyNonTriggersBetween(truePosition, vector.normalized, magnitude, Constants.ShipAndObjectsMask))
                {
                    result = ventInfo;
                    num = magnitude;
                }
            }
        }
        return result;
    }
    public static AudioSource PlaySound(Transform parent, AudioClip clip, bool loop, float volume = 1f, AudioMixerGroup audioMixer = null)
    {
        if (audioMixer == null)
        {
            audioMixer = (loop ? SoundManager.Instance.MusicChannel : SoundManager.Instance.SfxChannel);
        }
        AudioSource value = parent.GetComponent<AudioSource>() ?? parent.gameObject.AddComponent<AudioSource>();
        value.outputAudioMixerGroup = audioMixer;
        value.playOnAwake = false;
        value.volume = volume;
        value.loop = loop;
        value.clip = clip;
        value.Play();
        return value;
    }
    public static void SetKillTimerUnchecked(this PlayerControl player, float time, float max = float.NegativeInfinity)
    {
        if (max == float.NegativeInfinity) max = time;

        player.killTimer = time;
        FastDestroyableSingleton<HudManager>.Instance.KillButton.SetCoolDown(time, max);
    }

    public static Sprite CreateSprite(string path, bool fromDisk = false)
    {
        Texture2D texture = fromDisk ? ModHelpers.LoadTextureFromDisk(path) : ModHelpers.LoadTextureFromResources(path);
        if (texture == null)
            return null;
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.53f, 0.575f), texture.width * 0.375f);
        if (sprite == null)
            return null;
        texture.hideFlags |= HideFlags.HideAndDontSave | HideFlags.DontUnloadUnusedAsset;
        sprite.hideFlags |= HideFlags.HideAndDontSave | HideFlags.DontUnloadUnusedAsset;
        return sprite;
    }
    public static void Shuffle<T>(this IList<T> self, int startAt = 0)
    {
        for (int i = startAt; i < self.Count - 1; i++)
        {
            T value = self[i];
            int index = UnityEngine.Random.Range(i, self.Count);
            self[i] = self[index];
            self[index] = value;
        }
    }

    // Token: 0x060002F4 RID: 756 RVA: 0x00013308 File Offset: 0x00011508
    public static void Shuffle<T>(this System.Random r, IList<T> self)
    {
        for (int i = 0; i < self.Count; i++)
        {
            T value = self[i];
            int index = r.Next(self.Count);
            self[i] = self[index];
            self[index] = value;
        }
    }
    public static byte? GetKey(this Dictionary<byte, byte> dec, byte Value)
    {
        foreach (var data in dec)
        {
            if (data.Value == Value)
            {
                return data.Key;
            }
        }
        return null;
    }

    public static GameObject[] GetChildren(this GameObject ParentObject)
    {
        GameObject[] ChildObject = new GameObject[ParentObject.transform.childCount];

        for (int i = 0; i < ParentObject.transform.childCount; i++)
        {
            ChildObject[i] = ParentObject.transform.GetChild(i).gameObject;
        }
        return ChildObject;
    }
    public static void DeleteObject(this Transform[] trans, string notdelete)
    {
        foreach (Transform tran in trans)
        {
            if (tran.name != notdelete)
            {
                GameObject.Destroy(tran);
            }
        }
    }
    public static void DeleteObject(this GameObject[] trans, string notdelete)
    {
        foreach (GameObject tran in trans)
        {
            if (tran.name != notdelete)
            {
                GameObject.Destroy(tran);
            }
        }
    }
    public static List<PlayerControl> AllNotDisconnectedPlayerControl
    {
        get
        {
            List<PlayerControl> ps = new();
            foreach (CachedPlayer p in CachedPlayer.AllPlayers.AsSpan())
            {
                if (!p.Data.Disconnected) ps.Add(p.PlayerControl);
            }
            return ps;
        }
    }
    public static void SetActiveAllObject(this GameObject[] trans, string notdelete, bool IsActive)
    {
        foreach (GameObject tran in trans)
        {
            if (tran.name != notdelete)
            {
                tran.SetActive(IsActive);
            }
        }
    }
    public static void SetSkinWithAnim(PlayerPhysics playerPhysics, string SkinId)
    {
        /*
        SkinViewData nextSkin = FastDestroyableSingleton<HatManager>.Instance.GetSkinById(SkinId).viewData.viewData;
        AnimationClip clip = null;
        var spriteAnim = playerPhysics.GetSkin().animator;
        var anim = spriteAnim.m_animator;
        var skinLayer = playerPhysics.GetSkin();

        var currentPhysicsAnim = playerPhysics.Animations.Animator.GetCurrentAnimation();
        clip = currentPhysicsAnim == playerPhysics.Animations.group.RunAnim
            ? nextSkin.RunAnim
            : currentPhysicsAnim == playerPhysics.Animations.group.SpawnAnim
            ? nextSkin.SpawnAnim
            : currentPhysicsAnim == playerPhysics.Animations.group.EnterVentAnim
            ? nextSkin.EnterVentAnim
            : currentPhysicsAnim == playerPhysics.Animations.group.ExitVentAnim
            ? nextSkin.ExitVentAnim
            : currentPhysicsAnim == playerPhysics.Animations.group.IdleAnim ? nextSkin.IdleAnim : nextSkin.IdleAnim;

        float progress = playerPhysics.Animations.Animator.m_animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
        skinLayer.skin = nextSkin;

        spriteAnim.Play(clip, 1f);
        anim.Play("a", 0, progress % 1);
        anim.Update(0f);
        */
    }
    public static Dictionary<byte, PlayerControl> AllPlayersById()
    {
        Dictionary<byte, PlayerControl> res = new();
        foreach (PlayerControl player in CachedPlayer.AllPlayers.AsSpan())
            res.Add(player.PlayerId, player);
        return res;
    }

    public static void DestroyList<T>(Il2CppSystem.Collections.Generic.List<T> items) where T : UnityEngine.Object
    {
        if (items == null) return;
        foreach (T item in items)
        {
            UnityEngine.Object.Destroy(item);
        }
    }
    public static void DestroyList<T>(List<T> items) where T : UnityEngine.Object
    {
        if (items == null) return;
        foreach (T item in items.AsSpan())
        {
            UnityEngine.Object.Destroy(item);
        }
    }
    public static MurderAttemptResult CheckMurderAttempt(PlayerControl killer, PlayerControl target, bool blockRewind = false)
    {
        // Modified vanilla checks
        if (AmongUsClient.Instance.IsGameOver) return MurderAttemptResult.SuppressKill;
        if (killer == null || killer.Data == null || killer.Data.IsDead || killer.Data.Disconnected) return MurderAttemptResult.SuppressKill; // Allow non Impostor kills compared to vanilla code
        if (target == null || target.Data == null || target.Data.IsDead || target.Data.Disconnected) return MurderAttemptResult.SuppressKill; // Allow killing players in vents compared to vanilla code
        if (target.IsRole(RoleId.StuntMan) && !killer.IsRole(RoleId.OverKiller) && (!RoleClass.StuntMan.GuardCount.ContainsKey(target.PlayerId) || RoleClass.StuntMan.GuardCount[target.PlayerId] >= 1))
        {
            if (EvilEraser.IsOKAndTryUse(EvilEraser.BlockTypes.StuntmanGuard, killer))
            {
                bool IsSend = false;
                if (!RoleClass.StuntMan.GuardCount.ContainsKey(target.PlayerId) ||
                RoleClass.StuntMan.GuardCount[target.PlayerId] > 0)
                {
                    MessageWriter writer = RPCHelper.StartRPC(CustomRPC.UncheckedProtect);
                    writer.Write(target.PlayerId);
                    writer.Write(target.PlayerId);
                    writer.Write(0);
                    writer.EndRPC();
                    RPCProcedure.UncheckedProtect(target.PlayerId, target.PlayerId, 0);
                    IsSend = true;
                }
                if (IsSend)
                {
                    MessageWriter writer = RPCHelper.StartRPC(CustomRPC.UseStuntmanCount);
                    writer.Write(target.PlayerId);
                    writer.EndRPC();
                    RPCProcedure.UseStuntmanCount(target.PlayerId);
                }
            }
        }
        if (target.IsRole(RoleId.MadStuntMan) && !killer.IsRole(RoleId.OverKiller) && (!RoleClass.MadStuntMan.GuardCount.ContainsKey(target.PlayerId) || RoleClass.MadStuntMan.GuardCount[target.PlayerId] >= 1))
        {
            if (EvilEraser.IsOKAndTryUse(EvilEraser.BlockTypes.MadStuntmanGuard, killer))
            {
                bool IsSend = false;
                if (!RoleClass.MadStuntMan.GuardCount.ContainsKey(target.PlayerId) ||
                RoleClass.MadStuntMan.GuardCount[target.PlayerId] > 0)
                {
                    MessageWriter writer = RPCHelper.StartRPC(CustomRPC.UncheckedProtect);
                    writer.Write(target.PlayerId);
                    writer.Write(target.PlayerId);
                    writer.Write(0);
                    writer.EndRPC();
                    RPCProcedure.UncheckedProtect(target.PlayerId, target.PlayerId, 0);
                    IsSend = true;
                }
                if (IsSend)
                {
                    MessageWriter writer = RPCHelper.StartRPC(CustomRPC.UseStuntmanCount);
                    writer.Write(target.PlayerId);
                    writer.EndRPC();
                    RPCProcedure.UseStuntmanCount(target.PlayerId);
                }
            }
        }
        if (target.IsRole(RoleId.Shielder) && !killer.IsRole(RoleId.OverKiller) && RoleClass.Shielder.IsShield[target.PlayerId])
        {
            MessageWriter writer = RPCHelper.StartRPC(CustomRPC.ShielderProtect);
            writer.Write(target.PlayerId);
            writer.Write(target.PlayerId);
            writer.Write(0);
            writer.EndRPC();
            RPCProcedure.ShielderProtect(target.PlayerId, target.PlayerId, 0);
        }
        if (target.IsRole(RoleId.Fox) && !killer.IsRole(RoleId.OverKiller) && (!RoleClass.Fox.KillGuard.ContainsKey(target.PlayerId) || RoleClass.Fox.KillGuard[target.PlayerId] >= 1))
        {
            if (EvilEraser.IsOKAndTryUse(EvilEraser.BlockTypes.FoxGuard, killer))
            {
                bool IsSend = false;
                if (!RoleClass.Fox.KillGuard.ContainsKey(target.PlayerId) ||
                RoleClass.Fox.KillGuard[target.PlayerId] > 0)
                {
                    MessageWriter writer = RPCHelper.StartRPC(CustomRPC.UncheckedProtect);
                    writer.Write(target.PlayerId);
                    writer.Write(target.PlayerId);
                    writer.Write(0);
                    writer.EndRPC();
                    RPCProcedure.UncheckedProtect(target.PlayerId, target.PlayerId, 0);
                    IsSend = true;
                }
                if (IsSend)
                {
                    MessageWriter writer = RPCHelper.StartRPC(CustomRPC.UseStuntmanCount);
                    writer.Write(target.PlayerId);
                    writer.EndRPC();
                    RPCProcedure.UseStuntmanCount(target.PlayerId);
                }
            }
        }
        if (target.IsRole(RoleId.Safecracker) && !killer.IsRole(RoleId.OverKiller) && Safecracker.CheckTask(target, Safecracker.CheckTasks.KillGuard) && (!Safecracker.KillGuardCount.ContainsKey(target.PlayerId) || Safecracker.KillGuardCount[target.PlayerId] >= 1))
        {
            if (EvilEraser.IsOKAndTryUse(EvilEraser.BlockTypes.SafecrackerGuard, killer))
            {
                bool IsSend = false;
                if (!Safecracker.KillGuardCount.ContainsKey(target.PlayerId) ||
                    Safecracker.KillGuardCount[target.PlayerId] > 0)
                {
                    MessageWriter writer = RPCHelper.StartRPC(CustomRPC.UncheckedProtect);
                    writer.Write(target.PlayerId);
                    writer.Write(target.PlayerId);
                    writer.Write(0);
                    writer.EndRPC();
                    RPCProcedure.UncheckedProtect(target.PlayerId, target.PlayerId, 0);
                    IsSend = true;
                }
                if (IsSend)
                {
                    MessageWriter writer = RPCHelper.StartRPC(CustomRPC.SafecrackerGuardCount);
                    writer.Write(target.PlayerId);
                    writer.Write(true);
                    writer.EndRPC();
                    RPCProcedure.SafecrackerGuardCount(target.PlayerId, true);
                }
            }
        }
        if (target.IsRole(RoleId.Squid) && !killer.IsRole(RoleId.OverKiller) && Squid.IsVigilance.ContainsKey(target.PlayerId) && Squid.IsVigilance[target.PlayerId])
        {
            MessageWriter writer = RPCHelper.StartRPC(CustomRPC.ShielderProtect);
            writer.Write(target.PlayerId);
            writer.Write(target.PlayerId);
            writer.Write(0);
            writer.EndRPC();
            RPCProcedure.ShielderProtect(target.PlayerId, target.PlayerId, 0);
            Squid.SetVigilance(target, false);
            Squid.SetSpeedBoost(target);
            RPCHelper.StartRPC(CustomRPC.ShowFlash, target).EndRPC();
            Squid.Abilitys.IsKillGuard = true;
            Squid.Abilitys.IsObstruction = true;
            Squid.Abilitys.ObstructionTimer = Squid.SquidObstructionTime.GetFloat();
            GameOptionsManager.Instance.CurrentGameOptions.SetInt(Int32OptionNames.KillDistance, 0);
            Squid.InkSet();
        }
        if (target.IsRole(RoleId.TheSecondLittlePig) && !killer.IsRole(RoleId.OverKiller) && TheThreeLittlePigs.TaskCheck(target) && (!TheThreeLittlePigs.TheSecondLittlePig.GuardCount.ContainsKey(target.PlayerId) || TheThreeLittlePigs.TheSecondLittlePig.GuardCount[target.PlayerId] > 0))
        {
            MessageWriter writer1 = RPCHelper.StartRPC(CustomRPC.ShielderProtect);
            writer1.Write(target.PlayerId);
            writer1.Write(target.PlayerId);
            writer1.Write(0);
            writer1.EndRPC();
            RPCProcedure.ShielderProtect(target.PlayerId, target.PlayerId, 0);
            MessageWriter writer2 = RPCHelper.StartRPC(CustomRPC.UseTheThreeLittlePigsCount);
            writer2.Write(target.PlayerId);
            writer2.EndRPC();
            RPCProcedure.UseTheThreeLittlePigsCount(target.PlayerId);
        }
        if (target.IsRole(RoleId.TheThirdLittlePig) && !killer.IsRole(RoleId.OverKiller) && TheThreeLittlePigs.TaskCheck(target) && (!TheThreeLittlePigs.TheThirdLittlePig.CounterCount.ContainsKey(target.PlayerId) || TheThreeLittlePigs.TheThirdLittlePig.CounterCount[target.PlayerId] > 0))
        {
            MessageWriter writer1 = RPCHelper.StartRPC(CustomRPC.ShielderProtect);
            writer1.Write(target.PlayerId);
            writer1.Write(target.PlayerId);
            writer1.Write(0);
            writer1.EndRPC();
            RPCProcedure.ShielderProtect(target.PlayerId, target.PlayerId, 0);
            MessageWriter writer2 = RPCHelper.StartRPC(CustomRPC.UseTheThreeLittlePigsCount);
            writer2.Write(target.PlayerId);
            writer2.EndRPC();
            RPCProcedure.UseTheThreeLittlePigsCount(target.PlayerId);
            MessageWriter writer3 = RPCHelper.StartRPC(CustomRPC.RPCMurderPlayer);
            writer3.Write(target.PlayerId);
            writer3.Write(killer.PlayerId);
            writer3.Write((byte)0);
            writer3.EndRPC();
            RPCProcedure.RPCMurderPlayer(target.PlayerId, killer.PlayerId, 0);
            FinalStatusClass.RpcSetFinalStatus(killer, FinalStatus.TheThirdLittlePigCounterKill);
        }
        return MurderAttemptResult.PerformKill;
    }
    public static void AllRun<T>(this List<T> list, Action<T> act)
    {
        foreach (T obj in list.AsSpan())
            act(obj);
    }
    public static void AllRun<T>(this T[] array, Action<T> act)
    {
        foreach (T obj in array)
            act(obj);
    }
    public static void GenerateAndAssignTasks(this PlayerControl player, int numCommon, int numShort, int numLong)
    {
        if (player == null) return;

        List<byte> taskTypeIds = player.GenerateTasks((numCommon, numShort, numLong));

        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.UncheckedSetTasks, SendOption.Reliable, -1);
        writer.Write(player.PlayerId);
        writer.WriteBytesAndSize(taskTypeIds.ToArray());
        AmongUsClient.Instance.FinishRpcImmediately(writer);
        RPCProcedure.UncheckedSetTasks(player.PlayerId, taskTypeIds.ToArray());
    }
    public static List<byte> GenerateTasks(this PlayerControl player, (int numCommon, int numShort, int numLong) task)
    {
        if (task.numCommon + task.numShort + task.numLong <= 0) task.numShort = 1;
        if (player.GetRoleBase() is ITaskHolder taskHolder)
        {
            if (taskHolder.HaveMyNumTask(out (int, int, int)? mynumtask)) task = mynumtask.Value;
            if (taskHolder.AssignTask(out List<byte> mytasks, task)) return mytasks;
        }
        if (player.IsRole(RoleId.HamburgerShop) && (ModeHandler.IsMode(ModeId.SuperHostRoles) || !CustomOptionHolder.HamburgerShopChangeTaskPrefab.GetBool()))
            return Roles.CrewMate.HamburgerShop.GenerateTasks(task.numCommon + task.numShort + task.numLong);
        else if (player.IsRole(RoleId.Safecracker) && !(Safecracker.SafecrackerChangeTaskPrefab.GetBool() || GameManager.Instance.LogicOptions.currentGameOptions.MapId != (int)MapNames.Airship))
            return Safecracker.GenerateTasks(task.numCommon + task.numShort + task.numLong);

        Il2CppSystem.Collections.Generic.HashSet<TaskTypes> types = new();
        Il2CppSystem.Collections.Generic.List<byte> list = new();

        int start = 0;
        MapUtilities.CachedShipStatus.AddTasksFromList(ref start, task.numCommon, list, types, MapUtilities.CachedShipStatus.CommonTasks.GetShuffle().ToIl2CppList());
        start = 0;
        MapUtilities.CachedShipStatus.AddTasksFromList(ref start, task.numShort, list, types, MapUtilities.CachedShipStatus.ShortTasks.GetShuffle().ToIl2CppList());
        start = 0;
        MapUtilities.CachedShipStatus.AddTasksFromList(ref start, task.numLong, list, types, MapUtilities.CachedShipStatus.LongTasks.GetShuffle().ToIl2CppList());
        return list.ToList();
    }
    static float tien;
    public static string GetStringByCount(char txt, int count)
    {
        StringBuilder builder = new();
        for (int i = 0; i < count; i++)
        {
            builder.Append(txt);
        }
        return builder.ToString();
    }
    public static int GetDigit(int num)
    {
        // Mathf.Log10(0)はNegativeInfinityを返すため、別途処理する。
        return (num == 0) ? 1 : ((int)Mathf.Log10(num) + 1);
    }
    public static void SetSemiTransparent(this PoolablePlayer player, bool value)
    {
        float alpha = value ? 0.25f : 1f;
        foreach (SpriteRenderer r in player.gameObject.GetComponentsInChildren<SpriteRenderer>())
            r.color = new Color(r.color.r, r.color.g, r.color.b, alpha);
        player.cosmetics.nameText.color = new Color(player.cosmetics.nameText.color.r, player.cosmetics.nameText.color.g, player.cosmetics.nameText.color.b, alpha);
    }
    public static void HideChildren(Transform transform)
    {
        if (transform == null)
            return;
        for (int i = 0; i < transform.childCount; i++)
            transform.GetChild(i).gameObject.SetActive(false);
    }

    public static Console ActivateConsole(Transform trf) => ActivateConsole(trf.gameObject);

    public static AutoTaskConsole ActivateAutoTaskConsole(Transform trf) => ActivateAutoTaskConsole(trf.gameObject);

    public static Console ActivateConsole(GameObject obj)
    {
        if (obj == null)
        {
            Logger.Error($"ActivateConsole Object was not found!", "");
            return null;
        }
        obj.layer = LayerMask.NameToLayer("ShortObjects");
        Console console = obj.GetComponent<Console>();
        PassiveButton button = obj.GetComponent<PassiveButton>();
        CircleCollider2D collider = obj.GetComponent<CircleCollider2D>();
        if (!console)
        {
            console = obj.AddComponent<Console>();
            console.checkWalls = true;
            console.usableDistance = 0.7f;
            console.TaskTypes = new TaskTypes[0];
            console.ValidTasks = new Il2CppReferenceArray<TaskSet>(0);
            var list = ShipStatus.Instance.AllConsoles.ToList();
            list.Add(console);
            ShipStatus.Instance.AllConsoles = list.ToArray();
        }
        if (console.Image == null)
        {
            console.Image = obj.GetComponent<SpriteRenderer>();
            console.Image.material = new Material(ShipStatus.Instance.AllConsoles[0].Image.material);
        }
        if (!button)
        {
            button = obj.AddComponent<PassiveButton>();
            button.OnMouseOut = new UnityEngine.Events.UnityEvent();
            button.OnMouseOver = new UnityEngine.Events.UnityEvent();
            button._CachedZ_k__BackingField = 0.1f;
            button.CachedZ = 0.1f;
        }
        if (!collider)
        {
            collider = obj.AddComponent<CircleCollider2D>();
            collider.radius = 0.4f;
            collider.isTrigger = true;
        }
        return console;
    }
    public static AutoTaskConsole ActivateAutoTaskConsole(GameObject obj)
    {
        if (obj == null)
        {
            Logger.Error($"ActivateConsole Object was not found!", "");
            return null;
        }
        obj.layer = LayerMask.NameToLayer("ShortObjects");
        AutoTaskConsole console = obj.GetComponent<AutoTaskConsole>();
        PassiveButton button = obj.GetComponent<PassiveButton>();
        CircleCollider2D collider = obj.GetComponent<CircleCollider2D>();
        if (!console)
        {
            console = obj.AddComponent<AutoTaskConsole>();
            console.checkWalls = true;
            console.usableDistance = 0.7f;
            console.TaskTypes = new TaskTypes[0];
            console.ValidTasks = new(0);
            var list = MapUtilities.CachedShipStatus.AllConsoles.ToList();
            list.Add(console);
            MapUtilities.CachedShipStatus.AllConsoles = list.ToArray();
        }
        if (console.Image == null)
        {
            console.Image = obj.GetComponent<SpriteRenderer>();
            console.Image.material = new Material(MapUtilities.CachedShipStatus.AllConsoles[0].Image.material);
        }
        if (!collider)
        {
            collider = obj.AddComponent<CircleCollider2D>();
            collider.radius = 0.4f;
            collider.isTrigger = true;
        }
        return console;
    }
    public static MurderAttemptResult CheckMurderAttemptAndKill(PlayerControl killer, PlayerControl target, bool isMeetingStart = false, bool showAnimation = true)
    {
        // The local player checks for the validity of the kill and performs it afterwards (different to vanilla, where the host performs all the checks)
        // The kill attempt will be shared using a custom RPC, hence combining modded and unmodded versions is impossible

        tien = 0;

        MurderAttemptResult murder = CheckMurderAttempt(killer, target, isMeetingStart);
        if (murder == MurderAttemptResult.PerformKill)
        {
            if (tien <= 0)
            {
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.RPCMurderPlayer, SendOption.Reliable, -1);
                writer.Write(killer.PlayerId);
                writer.Write(target.PlayerId);
                writer.Write(showAnimation ? byte.MaxValue : 0);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.RPCMurderPlayer(killer.PlayerId, target.PlayerId, showAnimation ? Byte.MaxValue : (byte)0);
            }
            else
            {
                new LateTask(() =>
                {
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.RPCMurderPlayer, SendOption.Reliable, -1);
                    writer.Write(killer.PlayerId);
                    writer.Write(target.PlayerId);
                    writer.Write(showAnimation ? byte.MaxValue : 0);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.RPCMurderPlayer(killer.PlayerId, target.PlayerId, showAnimation ? Byte.MaxValue : (byte)0);
                }, tien, "CheckMuderAttemptAndKill");
            }
        }
        return murder;
    }
    public static void UncheckedMurderPlayer(this PlayerControl killer, PlayerControl target, bool isMeetingStart = false, bool showAnimation = true)
    {
        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.RPCMurderPlayer, SendOption.Reliable, -1);
        writer.Write(killer.PlayerId);
        writer.Write(target.PlayerId);
        writer.Write(showAnimation ? byte.MaxValue : 0);
        AmongUsClient.Instance.FinishRpcImmediately(writer);
        RPCProcedure.RPCMurderPlayer(killer.PlayerId, target.PlayerId, showAnimation ? Byte.MaxValue : (byte)0);
    }
    public static void SetPrivateRole(this CachedPlayer player, RoleTypes role, CachedPlayer seer = null)
    {
        if (player == null) return;
        if (seer == null) seer = player;
        var clientId = seer.PlayerControl.GetClientId();
        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(player.NetId, (byte)RpcCalls.SetRole, SendOption.Reliable, clientId);
        writer.Write((ushort)role);
        AmongUsClient.Instance.FinishRpcImmediately(writer);
    }
    public static InnerNet.ClientData GetClient(this PlayerControl player)
    {
        if (AmongUsClient.Instance?.allClients == null)
            return null;
        var client = AmongUsClient.Instance.allClients.FirstOrDefault(cd => cd.Character != null && cd.Character.PlayerId == player.PlayerId);
        return client;
    }
    public static T FirstOrDefault<T>(this Il2CppSystem.Collections.Generic.List<T> list, Func<T, bool> func)
    {
        foreach (T obj in list)
            if (func(obj))
                return obj;
        return default;
    }
    public static T FirstOrDefault<T>(this Il2CppSystem.Collections.Generic.List<T> list)
    {
        if (list.Count > 0)
            return list[0];
        return default;
    }
    public static T FirstOrDefault<T>(this List<T> list, Func<T, bool> func)
    {
        foreach (T obj in list.AsSpan())
            if (func(obj))
                return obj;
        return default;
    }
    public static T FirstOrDefault<T>(this List<T> list)
    {
        if (list.Count > 0)
            return list[0];
        return default;
    }
    public static T FirstOrDefault<T>(this T[] list)
    {
        if (list.Length > 0)
            return list[0];
        return default;
    }
    public static T FirstOrDefault<T>(this IEnumerable<T> list, Func<T, bool> func)
    {
        foreach (T obj in list)
            if (func(obj))
                return obj;
        return default;
    }
    public static T FirstOrDefault<T>(this IEnumerable<T> list)
    {
        foreach (T obj in list)
            return obj;
        return default;
    }
    public static T FirstOrDefault<T>(this Il2CppArrayBase<T> list, Func<T, bool> func)
    {
        foreach (T obj in list)
            if (func(obj))
                return obj;
        return default;
    }
    public static KeyValuePair<TKey, TValue> FirstOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> list, Func<KeyValuePair<TKey, TValue>, bool> func)
    {
        foreach (KeyValuePair<TKey, TValue> obj in list)
            if (func(obj))
                return obj;
        return default;
    }
    public static void ForEach<T>(this Il2CppArrayBase<T> list, Action<T> func)
    {
        foreach (T obj in list)
            func(obj);
    }
    public static bool Any<TKey, TValue>(this Dictionary<TKey, TValue> dict, Func<KeyValuePair<TKey, TValue>, bool> func)
    {
        foreach (KeyValuePair<TKey, TValue> obj in dict)
            if (func(obj))
                return true;
        return false;
    }
    public static bool Any<T>(this List<T> list, Func<T, bool> func)
    {
        if (list == null)
            return false;
        foreach (T obj in list.AsSpan())
            if (func(obj))
                return true;
        return false;
    }
    public static Il2CppSystem.Collections.Generic.KeyValuePair<TKey, TValue> FirstOrDefault<TKey, TValue>(this Il2CppSystem.Collections.Generic.Dictionary<TKey, TValue> list, Func<Il2CppSystem.Collections.Generic.KeyValuePair<TKey, TValue>, bool> func)
    {
        foreach (Il2CppSystem.Collections.Generic.KeyValuePair<TKey, TValue> obj in list)
            if (func(obj))
                return obj;
        return default;
    }
    public static T FirstOrDefault<T>(this Il2CppArrayBase<T> list)
    {
        foreach (T obj in list)
            return obj;
        return default;
    }
    public static List<T> ToList<T>(this Il2CppSystem.Collections.Generic.List<T> list)
    {
        List<T> newList = [.. list];
        return newList;
    }
    public static Il2CppSystem.Collections.Generic.List<T> ToIl2CppList<T>(this List<T> list)
    {
        Il2CppSystem.Collections.Generic.List<T> newList = new(list.Count);
        foreach (T item in list.AsSpan())
        {
            newList.Add(item);
        }
        return newList;
    }
    public static Il2CppSystem.Collections.Generic.List<T> ToIl2CppList<T>(this T[] array)
    {
        Il2CppSystem.Collections.Generic.List<T> newList = new(array.Length);
        foreach (T item in array)
        {
            newList.Add(item);
        }
        return newList;
    }
    public static AudioClip loadAudioClipFromWavResources(string path, string clipName = "UNNAMED_TOR_AUDIO_CLIP")
    {
        Assembly assembly = Assembly.GetExecutingAssembly();
        Stream stream = assembly.GetManifestResourceStream(path);
        return WavLoader.ToAudioClip(stream, clipName);
    }
    public static Dictionary<string, AudioClip> CachedAudioClips = new();
    public static AudioClip loadAudioClipFromResources(string path, string clipName = "UNNAMED_TOR_AUDIO_CLIP")
    {
        // must be "raw (headerless) 2-channel signed 32 bit pcm (le)" (can e.g. use Audacity® to export)
        try
        {
            if (CachedAudioClips.TryGetValue(path, out var audio)) return audio;
            Assembly assembly = Assembly.GetExecutingAssembly();
            Stream stream = assembly.GetManifestResourceStream(path);
            var byteAudio = new byte[stream.Length];
            _ = stream.Read(byteAudio, 0, (int)stream.Length);
            float[] samples = new float[byteAudio.Length / 4]; // 4 bytes per sample
            int offset;
            for (int i = 0; i < samples.Length; i++)
            {
                offset = i * 4;
                samples[i] = (float)BitConverter.ToInt32(byteAudio, offset) / int.MaxValue;
            }
            int channels = 2;
            int sampleRate = 48000;
            AudioClip audioClip = AudioClip.Create(clipName, samples.Length, channels, sampleRate, false);
            audioClip.SetData(samples, 0);
            audioClip.hideFlags |= HideFlags.HideAndDontSave | HideFlags.DontSaveInEditor;
            return CachedAudioClips[path] = audioClip;
        }
        catch
        {
            System.Console.WriteLine("Error loading AudioClip from resources: " + path);
        }
        return null;

        /* Usage example:
        AudioClip exampleClip = Helpers.loadAudioClipFromResources("TheOtherRoles.Resources.exampleClip.raw");
        if (Constants.ShouldPlaySfx()) SoundManager.Instance.PlaySound(exampleClip, false, 0.8f);
        */
    }
    public static int GetClientId(this PlayerControl player)
    {
        var client = player.GetClient();
        return client == null ? -1 : client.Id;
    }
    public static bool IsSuccessChance(int SuccessChance, int MaxChance = 10)
    {
        //成功確率が0%ならfalseを返す
        if (SuccessChance == 0) return false;
        //成功確率が最大と一緒かそれ以上ならtrueを返す
        if (SuccessChance >= MaxChance) return true;
        return UnityEngine.Random.Range(0, MaxChance) <= SuccessChance;
    }
    /// <summary>
    /// ランダムを取得します。max = 10だと0～10まで取得できます
    /// </summary>
    /// <param name="max"></param>
    /// <param name="min"></param>
    /// <returns></returns>
    public static int GetRandomInt(int max, int min = 0)
    {
        return UnityEngine.Random.Range(min, max + 1);
    }
    public static bool HidePlayerName(PlayerControl source, PlayerControl target)
    {
        if (source == null || target == null) return true;
        else if (source.IsDead() || source.IsRole(RoleId.God)) return false;
        else if (source.PlayerId == target.PlayerId) return false; // Player sees his own name
        else if (source.IsImpostor() && target.IsImpostor()) return false;
        else if (GameData.Instance && RoleClass.Kunoichi.IsScientistPlayers.ContainsKey(target.PlayerId) && RoleClass.Kunoichi.IsScientistPlayers[target.PlayerId]) return true;
        else if (InvisibleRoleBase.IsExistsInvisiblePlayer[target.PlayerId]) return true;
        return false;
    }

    public static Dictionary<string, Sprite> CachedSprites = new();

    public static Sprite LoadSpriteFromResources(string path, float pixelsPerUnit)
    {
        try
        {
            if (CachedSprites.TryGetValue(path + pixelsPerUnit, out var sprite)) return sprite;
            Texture2D texture = LoadTextureFromResources(path);
            sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), pixelsPerUnit);
            sprite.hideFlags |= HideFlags.HideAndDontSave | HideFlags.DontSaveInEditor;
            return CachedSprites[path + pixelsPerUnit] = sprite;
        }
        catch
        {
            System.Console.WriteLine("Error loading sprite from path: " + path);
        }
        return null;
    }
    public static async Task<Sprite> LoadSpriteFromResourcesAsync(string path, float pixelsPerUnit)
    {
        try
        {
            if (CachedSprites.TryGetValue(path + pixelsPerUnit, out var sprite)) return sprite;
            Texture2D texture = await LoadTextureFromResourcesAsync(path);
            sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), pixelsPerUnit);
            sprite.hideFlags |= HideFlags.HideAndDontSave | HideFlags.DontSaveInEditor;
            return CachedSprites[path + pixelsPerUnit] = sprite;
        }
        catch
        {
            System.Console.WriteLine("Error loading sprite from path: " + path);
        }
        return null;
    }

    public static bool IsCustomServer()
    {
        if (FastDestroyableSingleton<ServerManager>.Instance == null) return false;
        StringNames n = FastDestroyableSingleton<ServerManager>.Instance.CurrentRegion.TranslateName;
        return n is not StringNames.ServerNA and not StringNames.ServerEU and not StringNames.ServerAS;
    }
    public static object TryCast(this Il2CppObjectBase self, Type type)
    {
        return AccessTools.Method(self.GetType(), nameof(Il2CppObjectBase.TryCast)).MakeGenericMethod(type).Invoke(self, Array.Empty<object>());
    }

    public static Dictionary<string, Texture2D> CachedTexture = new();

    public static Texture2D LoadTextureFromResources(string path)
    {
        try
        {
            if (CachedTexture.TryGetValue(path, out Texture2D texture)) return texture;
            texture = new(2, 2, TextureFormat.ARGB32, true);
            Assembly assembly = Assembly.GetExecutingAssembly();
            Stream stream = assembly.GetManifestResourceStream(path);
            var byteTexture = new byte[stream.Length];
            var read = stream.Read(byteTexture, 0, (int)stream.Length);
            LoadImage(texture, byteTexture, false);
            texture.hideFlags |= HideFlags.HideAndDontSave | HideFlags.DontSaveInEditor;
            return CachedTexture[path] = texture;
        }
        catch
        {
            System.Console.WriteLine("Error loading texture from resources: " + path);
        }
        return null;
    }



    public static async Task<Texture2D> LoadTextureFromResourcesAsync(string path)
    {
        try
        {
            if (CachedTexture.TryGetValue(path, out Texture2D texture)) return texture;
            texture = new(2, 2, TextureFormat.ARGB32, true);
            Stream stream = SuperNewRolesPlugin.assembly.GetManifestResourceStream(path);
            var byteTexture = new byte[stream.Length];
            var read = (await stream.ReadAsync(byteTexture, 0, (int)stream.Length));
            LoadImage(texture, byteTexture, false);
            texture.hideFlags |= HideFlags.HideAndDontSave | HideFlags.DontSaveInEditor;
            return CachedTexture[path] = texture;
        }
        catch
        {
            System.Console.WriteLine("Error loading texture from resources: " + path);
        }
        return null;
    }

    public static SystemTypes GetInRoom(Vector2 pos)
    {
        if (ShipStatus.Instance is null) return SystemTypes.Doors;
        PlainShipRoom room = ShipStatus.Instance.AllRooms.FirstOrDefault(x => x.roomArea.ClosestPoint(pos) == pos);
        if (room is null) return SystemTypes.Doors;
        return room.RoomId;
    }

    public static string Cs(Color c, string s)
    {
        return $"<color=#{ToByte(c.r):X2}{ToByte(c.g):X2}{ToByte(c.b):X2}{ToByte(c.a):X2}>{s}</color>";
    }
    public static byte ToByte(float f)
    {
        f = Mathf.Clamp01(f);
        return (byte)(f * 255);
    }
    public static T GetRandom<T>(this Il2CppSystem.Collections.Generic.List<T> list)
    {
        var indexData = UnityEngine.Random.Range(0, list.Count);
        return list[indexData];
    }
    public static T GetRandom<T>(this List<T> list)
    {
        var indexData = UnityEngine.Random.Range(0, list.Count);
        return list[indexData];
    }
    public static T GetRandom<T>(this T[] list)
    {
        var indexData = UnityEngine.Random.Range(0, list.Length);
        return list[indexData];
    }
    public static int GetRandomIndex<T>(List<T> list)
    {
        var indexData = UnityEngine.Random.Range(0, list.Count);
        return indexData;
    }

    public static Dictionary<byte, SpriteRenderer> MyRendCache = new();
    public static Dictionary<byte, SkinLayer> SkinLayerCache = new();
    public static Dictionary<byte, HatParent> HatRendererCache = new();
    public static Dictionary<byte, SpriteRenderer> HatRendCache = new();
    public static Dictionary<byte, VisorLayer> VisorSlotCache = new();
    public static TextMeshPro NameText(this PlayerControl player)
    {
        return player.cosmetics.nameText;
    }
    public static TextMeshPro NameText(this PoolablePlayer player)
    {
        return player.cosmetics.nameText;
    }
    public static SpriteRenderer MyRend(this PlayerControl player)
    {
        return player.cosmetics.currentBodySprite.BodySprite;
    }
    public static SpriteRenderer Rend(this PlayerPhysics player)
    {
        return player.myPlayer.cosmetics.currentBodySprite.BodySprite;
    }
    public static SkinLayer GetSkin(this PlayerControl player)
    {
        return player.cosmetics.skin;
    }
    public static SkinLayer GetSkin(this PlayerPhysics player)
    {
        return player.myPlayer.cosmetics.skin;
    }
    public static HatParent HatRenderer(this PlayerControl player)
    {
        return player.cosmetics.hat;
    }
    public static SpriteRenderer HatRend(this PlayerControl player)
    {
        return player.cosmetics.hat.Parent;
    }
    public static VisorLayer VisorSlot(this PlayerControl player)
    {
        return player.cosmetics.visor;
    }

    public static HatParent HatSlot(this PoolablePlayer player)
    {
        return player.cosmetics.hat;
    }
    public static VisorLayer VisorSlot(this PoolablePlayer player)
    {
        return player.cosmetics.visor;
    }

    public static Texture2D LoadTextureFromDisk(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                Texture2D texture = new(2, 2, TextureFormat.ARGB32, true);
                byte[] byteTexture = File.ReadAllBytes(path);
                LoadImage(texture, byteTexture, false);
                return texture;
            }
        }
        catch
        {
            System.Console.WriteLine("Error loading texture from disk: " + path);
        }
        return null;
    }
    internal delegate bool d_LoadImage(IntPtr tex, IntPtr data, bool markNonReadable);
    internal static d_LoadImage iCall_LoadImage;
    private static bool LoadImage(Texture2D tex, byte[] data, bool markNonReadable)
    {
        if (iCall_LoadImage == null)
            iCall_LoadImage = IL2CPP.ResolveICall<d_LoadImage>("UnityEngine.ImageConversion::LoadImage");
        var il2cppArray = (Il2CppStructArray<byte>)data;
        return iCall_LoadImage.Invoke(tex.Pointer, il2cppArray.Pointer, markNonReadable);
    }

    public static T GetOrAddComponent<T>(this GameObject obj) where T : Component
    {
        T component = obj.GetComponent<T>();
        return component != null ? component : obj.AddComponent<T>();
    }
    internal static Dictionary<byte, PlayerControl> IdControlDic = new(); // ClearAndReloadで初期化されます
    internal static Dictionary<string, PlayerControl> ColorControlDic = new(); // ClearAndReloadで初期化されます
    internal static Dictionary<int, Vent> VentIdControlDic = new(); // ClearAndReloadで初期化されます
    public static PlayerControl GetPlayerControl(this byte id) => PlayerById(id);
    public static PlayerControl PlayerById(byte id)
    {
        if (!IdControlDic.ContainsKey(id))
        { // idが辞書にない場合全プレイヤー分のループを回し、辞書に追加する
            foreach (PlayerControl pc in CachedPlayer.AllPlayers.AsSpan())
            {
                if (!IdControlDic.ContainsKey(pc.PlayerId)) // Key重複対策
                    IdControlDic.Add(pc.PlayerId, pc);
            }
        }
        if (IdControlDic.ContainsKey(id)) return IdControlDic[id];
        Logger.Error($"idと合致するPlayerIdが見つかりませんでした。nullを返却します。id:{id}", "ModHelpers");
        return null;
    }
    public static PlayerControl GetPlayerControl(this string id) => PlayerByColor(id);
    public static PlayerControl PlayerByColor(string color_name)
    {
        if (ColorControlDic.TryGetValue(color_name, out PlayerControl player)) return player;
        foreach (PlayerControl check in CachedPlayer.AllPlayers.AsSpan())
        {
            if (color_name == check.Data.GetPlayerColorString())
            {
                ColorControlDic[color_name] = check;
                return check;
            }
        }
        Logger.Error($"カラーと合致するPlayerが見つかりませんでした。nullを返却します。color:{color_name}", "ModHelpers");
        return null;
    }
    public static Vent VentById(byte id)
    {
        if (!VentIdControlDic.ContainsKey(id))
        { // idが辞書にない場合全プレイヤー分のループを回し、辞書に追加する
            foreach (Vent vn in ShipStatus.Instance.AllVents)
            {
                if (!VentIdControlDic.ContainsKey(vn.Id)) // Key重複対策
                    VentIdControlDic.Add(vn.Id, vn);
            }
        }
        if (VentIdControlDic.ContainsKey(id)) return VentIdControlDic[id];
        Logger.Error($"idと合致するVentIdが見つかりませんでした。nullを返却します。id:{id}", "ModHelpers");
        return null;
    }

    private static Dictionary<ushort, bool> IsImpostorRoleCached = new();
    public static bool IsImpostorRole(this RoleTypes roleTypes)
    {
        if (IsImpostorRoleCached.TryGetValue((ushort)roleTypes, out bool value))
            return value;
        RoleBehaviour role = FastDestroyableSingleton<RoleManager>.Instance.GetRole(roleTypes);
        if (role == null)
            throw new NotImplementedException($"Not found roletypes: {roleTypes}");
        return IsImpostorRoleCached[(ushort)roleTypes] = role.IsImpostor;
    }

    public static bool IsCheckListPlayerControl(this List<PlayerControl> listData, PlayerControl CheckPlayer)
    {
        foreach (PlayerControl Player in listData.AsSpan())
        {
            if (Player is null) continue;
            if (Player.PlayerId == CheckPlayer.PlayerId)
                return true;
        }
        return false;
    }
    public static bool IsPosition(Vector3 pos, Vector2 pos2)
    {
        return pos.x == pos2.x && pos.y == pos2.y;
    }
    public static bool IsPositionDistance(Vector2 pos, Vector2 pos2, float distance)
    {
        float dis = Vector2.Distance(pos, pos2);
        return dis <= distance;
    }
    /// <summary>keyCodesが押されているか</summary>
    public static bool GetManyKeyDown(KeyCode[] keyCodes) =>
        keyCodes.All(x => Input.GetKey(x)) && keyCodes.Any(x => Input.GetKeyDown(x));

    public static void AddRanges(this List<PlayerControl> list, List<PlayerControl>[] collections)
    {
        foreach (var c in collections)
            list.AddRange(c);
    }

    public static string GetRPCNameFromByte(PlayerControl __instance, byte callId) =>
        Enum.GetName(typeof(RpcCalls), callId) != null ? // RpcCallsに当てはまる
            Enum.GetName(typeof(RpcCalls), callId) :
        Enum.GetName(typeof(CustomRPC), callId) != null ? // CustomRPCに当てはまる
            Enum.GetName(typeof(CustomRPC), callId) :
        $"{nameof(RpcCalls)}及び、{nameof(CustomRPC)}にも当てはまらない無効な値です:{callId}:{__instance.Data.PlayerName}";
    public static bool IsDebugMode() => ConfigRoles.DebugMode.Value && CustomOptionHolder.IsDebugMode.GetBool();
    /// <summary>
    /// 文字列が半角かどうかを判定します
    /// </summary>
    /// <remarks>半角の判定を正規表現で行います。半角カタカナは「ｦ」～半濁点を半角とみなします</remarks>
    /// <param name="target">対象の文字列</param>
    /// <returns>文字列が半角の場合はtrue、それ以外はfalse</returns>
    public static bool IsOneByteOnlyString(string target) => new Regex("^[\u0020-\u007E\uFF66-\uFF9F]+$").IsMatch(target);
    public static string[] CustomRates(int start = 0)
    {
        start = start >= 10 ? 10 : start;
        start = start <= 0 ? 0 : start;
        List<string> rates = new(CustomOptionHolder.rates);
        for (int i = 0; i < start; i++)
        {
            string text = rates[0];
            rates.Remove(text);
            rates.Add(text);
        }
        return rates.ToArray();
    }
    public static bool IsMap(MapNames Map)
    {
        if (GameManager.Instance == null)
            return false;
        return GameManager.Instance.LogicOptions.currentGameOptions.MapId == (byte)Map;
    }
    public static Il2CppSystem.Collections.Generic.IEnumerable<T> IEnumerableToIl2Cpp<T>(this IEnumerable<T> values) => Il2CppSystem.Linq.Enumerable.Cast<T>(values.WrapToIl2Cpp());
    public static Il2CppSystem.Collections.Generic.List<T> ListToIl2Cpp<T>(this Il2CppReferenceArray<T> values) where T : Il2CppObjectBase => values.ToList().ToIl2CppList();
    public static void ResetKillCool(this PlayerControl player, float timer = float.NegativeInfinity)
    {
        IGameOptions optdata = SyncSetting.OptionDatas[player].DeepCopy();
        if (timer == float.NegativeInfinity) timer = SyncSetting.OptionDatas[player].GetFloat(FloatOptionNames.KillCooldown);
        if (player.AmOwner) player.SetKillTimerUnchecked(timer);
        else
        {
            IGameOptions killcool = optdata.DeepCopy();
            killcool.SetFloat(FloatOptionNames.KillCooldown, SyncSetting.KillCoolSet(timer * 2));
            killcool.RpcSyncOption(player.GetClientId(), SendOption.None);
            player.RpcShowGuardEffect(player);
            optdata.RpcSyncOption(player.GetClientId(), SendOption.None);
        }
    }
    public static bool IsBlackout()
    {
        if (MapUtilities.CachedShipStatus.Systems.TryGetValue(SystemTypes.Electrical, out ISystemType elecsystem))
        {
            var ma = elecsystem.CastFast<SwitchSystem>();
            if (ma != null && ma.IsActive)
                return true;
        }
        return RoleBaseManager.GetInterfaces<ISpecialBlackout>().Any(x => x.IsBlackout());
    }
    public static bool Il2CppIs<T1, T2>(this T1 before, out T2 after) where T1 : Il2CppObjectBase where T2 : Il2CppObjectBase
    {
        after = before.TryCast<T2>();
        return after != null;
    }
    public static T[] GetShuffle<T>(this IEnumerable<T> values)
    {
        T[] data = values.ToArray();
        data.Shuffle();
        return data;
    }
    public static int CountLine(this string str)
    {
        if (str == null) return 0;
        int n = 0;
        foreach (var c in str.AsSpan())
        {
            if (c == '\n') n++;
        }
        return n + 1;
    }
    public static void SetStaticfontSizes(this TextMeshPro tmp, float size)
    {
        tmp.fontSize = tmp.fontSizeMax = tmp.fontSizeMin = size;
    }
    public static void AddListener(this UnityEngine.Events.UnityEvent @event, Action action) => @event.AddListener(action);
    public static T Find<T>(this Il2CppSystem.Collections.Generic.List<T> data, Func<T, bool> match) => data.Find(match);
    public static bool Any<T>(this Il2CppSystem.Collections.Generic.List<T> data, Func<T, bool> match) => data.FindIndex(match) >= 0;
    public static ReadOnlySpan<T> AsSpan<T>(this List<T> list) => CollectionsMarshal.AsSpan(list);
    public static void SetActiveEx(this GameObject obj, bool value)
    {
        if (obj.active != value) obj.SetActive(value);
    }
}
public static class CreateFlag
{
    public static List<string> OneTimeList = new();
    public static List<string> FirstRunList = new();
    public static void Run(Action action, string type, bool firstrun = false)
    {
        if (OneTimeList.Contains(type) || (firstrun && !FirstRunList.Contains(type)))
        {
            if (!FirstRunList.Contains(type)) FirstRunList.Add(type);
            OneTimeList.Remove(type);
            action();
        }
    }
    public static void NewFlag(string type)
    {
        if (!OneTimeList.Contains(type)) OneTimeList.Add(type);
    }
}