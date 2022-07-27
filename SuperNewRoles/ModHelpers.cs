using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Hazel;
using SuperNewRoles.CustomOption;
using SuperNewRoles.CustomRPC;
using SuperNewRoles.Helpers;
using SuperNewRoles.Roles;
using TMPro;
using UnhollowerBaseLib;
using UnityEngine;

namespace SuperNewRoles
{
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
        public static void SetKillTimerUnchecked(this PlayerControl player, float time, float max = float.NegativeInfinity)
        {
            if (max == float.NegativeInfinity) max = time;

            player.killTimer = time;
            FastDestroyableSingleton<HudManager>.Instance.KillButton.SetCoolDown(time, max);
        }

        public static Sprite CreateSprite(string path, bool fromDisk = false)
        {
            Texture2D texture = fromDisk ? ModHelpers.loadTextureFromDisk(path) : ModHelpers.LoadTextureFromResources(path);
            if (texture == null)
                return null;
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.53f, 0.575f), texture.width * 0.375f);
            if (sprite == null)
                return null;
            texture.hideFlags |= HideFlags.HideAndDontSave | HideFlags.DontUnloadUnusedAsset;
            sprite.hideFlags |= HideFlags.HideAndDontSave | HideFlags.DontUnloadUnusedAsset;
            return sprite;
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
                foreach (CachedPlayer p in CachedPlayer.AllPlayers)
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
            SkinViewData nextSkin = DestroyableSingleton<HatManager>.Instance.GetSkinById(SkinId).viewData.viewData;
            AnimationClip clip = null;
            var spriteAnim = playerPhysics.GetSkin().animator;
            var anim = spriteAnim.m_animator;
            var skinLayer = playerPhysics.GetSkin();

            var currentPhysicsAnim = playerPhysics.Animator.GetCurrentAnimation();
            clip = currentPhysicsAnim == playerPhysics.CurrentAnimationGroup.RunAnim
                ? nextSkin.RunAnim
                : currentPhysicsAnim == playerPhysics.CurrentAnimationGroup.SpawnAnim
                ? nextSkin.SpawnAnim
                : currentPhysicsAnim == playerPhysics.CurrentAnimationGroup.EnterVentAnim
                ? nextSkin.EnterVentAnim
                : currentPhysicsAnim == playerPhysics.CurrentAnimationGroup.ExitVentAnim
                ? nextSkin.ExitVentAnim
                : currentPhysicsAnim == playerPhysics.CurrentAnimationGroup.IdleAnim ? nextSkin.IdleAnim : nextSkin.IdleAnim;

            float progress = playerPhysics.Animator.m_animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
            skinLayer.skin = nextSkin;

            spriteAnim.Play(clip, 1f);
            anim.Play("a", 0, progress % 1);
            anim.Update(0f);
        }
        public static Dictionary<byte, PlayerControl> AllPlayersById()
        {
            Dictionary<byte, PlayerControl> res = new();
            foreach (CachedPlayer player in CachedPlayer.AllPlayers)
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
            foreach (T item in items)
            {
                UnityEngine.Object.Destroy(item);
            }
        }
        public static MurderAttemptResult CheckMuderAttempt(PlayerControl killer, PlayerControl target, bool blockRewind = false)
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
                    if (!RoleClass.StuntMan.GuardCount.ContainsKey(target.PlayerId))
                    {
                        MessageWriter writer = RPCHelper.StartRPC(CustomRPC.CustomRPC.UncheckedProtect);
                        writer.Write(target.PlayerId);
                        writer.Write(target.PlayerId);
                        writer.Write(0);
                        writer.EndRPC();
                        RPCProcedure.UncheckedProtect(target.PlayerId, target.PlayerId, 0);
                        IsSend = true;
                    }
                    else
                    {
                        if (!(RoleClass.StuntMan.GuardCount[target.PlayerId] <= 0))
                        {
                            MessageWriter writer = RPCHelper.StartRPC(CustomRPC.CustomRPC.UncheckedProtect);
                            writer.Write(target.PlayerId);
                            writer.Write(target.PlayerId);
                            writer.Write(0);
                            writer.EndRPC();
                            RPCProcedure.UncheckedProtect(target.PlayerId, target.PlayerId, 0);
                            IsSend = true;
                        }
                    }
                    if (IsSend)
                    {
                        MessageWriter writer = RPCHelper.StartRPC(CustomRPC.CustomRPC.UseStuntmanCount);
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
                    if (!RoleClass.MadStuntMan.GuardCount.ContainsKey(target.PlayerId))
                    {
                        MessageWriter writer = RPCHelper.StartRPC(CustomRPC.CustomRPC.UncheckedProtect);
                        writer.Write(target.PlayerId);
                        writer.Write(target.PlayerId);
                        writer.Write(0);
                        writer.EndRPC();
                        RPCProcedure.UncheckedProtect(target.PlayerId, target.PlayerId, 0);
                        IsSend = true;
                    }
                    else
                    {
                        if (!(RoleClass.MadStuntMan.GuardCount[target.PlayerId] <= 0))
                        {
                            MessageWriter writer = RPCHelper.StartRPC(CustomRPC.CustomRPC.UncheckedProtect);
                            writer.Write(target.PlayerId);
                            writer.Write(target.PlayerId);
                            writer.Write(0);
                            writer.EndRPC();
                            RPCProcedure.UncheckedProtect(target.PlayerId, target.PlayerId, 0);
                            IsSend = true;
                        }
                    }

                    if (IsSend)
                    {
                        MessageWriter writer = RPCHelper.StartRPC(CustomRPC.CustomRPC.UseStuntmanCount);
                        writer.Write(target.PlayerId);
                        writer.EndRPC();
                        RPCProcedure.UseStuntmanCount(target.PlayerId);
                    }
                }
            }
            if (target.IsRole(RoleId.Shielder) && !killer.IsRole(RoleId.OverKiller) && RoleClass.Shielder.IsShield[target.PlayerId])
            {
                MessageWriter writer = RPCHelper.StartRPC(CustomRPC.CustomRPC.ShielderProtect);
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
                    if (!RoleClass.Fox.KillGuard.ContainsKey(target.PlayerId))
                    {
                        MessageWriter writer = RPCHelper.StartRPC(CustomRPC.CustomRPC.UncheckedProtect);
                        writer.Write(target.PlayerId);
                        writer.Write(target.PlayerId);
                        writer.Write(0);
                        writer.EndRPC();
                        RPCProcedure.UncheckedProtect(target.PlayerId, target.PlayerId, 0);
                        IsSend = true;
                    }
                    else
                    {
                        if (!(RoleClass.Fox.KillGuard[target.PlayerId] <= 0))
                        {
                            MessageWriter writer = RPCHelper.StartRPC(CustomRPC.CustomRPC.UncheckedProtect);
                            writer.Write(target.PlayerId);
                            writer.Write(target.PlayerId);
                            writer.Write(0);
                            writer.EndRPC();
                            RPCProcedure.UncheckedProtect(target.PlayerId, target.PlayerId, 0);
                            IsSend = true;
                        }
                    }
                    if (IsSend)
                    {
                        MessageWriter writer = RPCHelper.StartRPC(CustomRPC.CustomRPC.UseStuntmanCount);
                        writer.Write(target.PlayerId);
                        writer.EndRPC();
                        RPCProcedure.UseStuntmanCount(target.PlayerId);
                    }
                }
            }
            return MurderAttemptResult.PerformKill;
        }
        public static void GenerateAndAssignTasks(this PlayerControl player, int numCommon, int numShort, int numLong)
        {
            if (player == null) return;

            List<byte> taskTypeIds = GenerateTasks(numCommon, numShort, numLong);

            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.UncheckedSetTasks, SendOption.Reliable, -1);
            writer.Write(player.PlayerId);
            writer.WriteBytesAndSize(taskTypeIds.ToArray());
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            RPCProcedure.UncheckedSetTasks(player.PlayerId, taskTypeIds.ToArray());
        }
        public static List<byte> GenerateTasks(int numCommon, int numShort, int numLong)
        {
            if (numCommon + numShort + numLong <= 0)
            {
                numShort = 1;
            }

            var tasks = new Il2CppSystem.Collections.Generic.List<byte>();
            var hashSet = new Il2CppSystem.Collections.Generic.HashSet<TaskTypes>();

            var commonTasks = new Il2CppSystem.Collections.Generic.List<NormalPlayerTask>();
            foreach (var task in MapUtilities.CachedShipStatus.CommonTasks.OrderBy(x => RoleClass.rnd.Next())) commonTasks.Add(task);

            var shortTasks = new Il2CppSystem.Collections.Generic.List<NormalPlayerTask>();
            foreach (var task in MapUtilities.CachedShipStatus.NormalTasks.OrderBy(x => RoleClass.rnd.Next())) shortTasks.Add(task);

            var longTasks = new Il2CppSystem.Collections.Generic.List<NormalPlayerTask>();
            foreach (var task in MapUtilities.CachedShipStatus.LongTasks.OrderBy(x => RoleClass.rnd.Next())) longTasks.Add(task);

            int start = 0;
            MapUtilities.CachedShipStatus.AddTasksFromList(ref start, numCommon, tasks, hashSet, commonTasks);

            start = 0;
            MapUtilities.CachedShipStatus.AddTasksFromList(ref start, numShort, tasks, hashSet, shortTasks);

            start = 0;
            MapUtilities.CachedShipStatus.AddTasksFromList(ref start, numLong, tasks, hashSet, longTasks);

            return tasks.ToArray().ToList();
        }
        static float tien;
        public static MurderAttemptResult CheckMuderAttemptAndKill(PlayerControl killer, PlayerControl target, bool isMeetingStart = false, bool showAnimation = true)
        {
            // The local player checks for the validity of the kill and performs it afterwards (different to vanilla, where the host performs all the checks)
            // The kill attempt will be shared using a custom RPC, hence combining modded and unmodded versions is impossible

            tien = 0;

            MurderAttemptResult murder = CheckMuderAttempt(killer, target, isMeetingStart);
            if (murder == MurderAttemptResult.PerformKill)
            {
                if (tien <= 0)
                {
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.RPCMurderPlayer, SendOption.Reliable, -1);
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
                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.RPCMurderPlayer, SendOption.Reliable, -1);
                        writer.Write(killer.PlayerId);
                        writer.Write(target.PlayerId);
                        writer.Write(showAnimation ? byte.MaxValue : 0);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        RPCProcedure.RPCMurderPlayer(killer.PlayerId, target.PlayerId, showAnimation ? Byte.MaxValue : (byte)0);
                    }, tien);
                }
            }
            return murder;
        }
        public static void UncheckedMurderPlayer(PlayerControl killer, PlayerControl target, bool isMeetingStart = false, bool showAnimation = true)
        {
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.RPCMurderPlayer, SendOption.Reliable, -1);
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
            var client = AmongUsClient.Instance.allClients.GetFastEnumerator().ToArray().Where(cd => cd.Character.PlayerId == player.PlayerId).FirstOrDefault();
            return client;
        }
        public static int GetClientId(this PlayerControl player)
        {
            var client = player.GetClient();
            return client == null ? -1 : client.Id;
        }
        public static bool IsSucsessChance(int SucsessChance, int MaxChance = 10)
        {
            //成功確率が0%ならfalseを返す
            if (SucsessChance == 0) return false;
            //成功確率が最大と一緒かそれ以上ならtrueを返す
            if (SucsessChance >= MaxChance) return true;
            return UnityEngine.Random.Range(1, MaxChance) <= SucsessChance;
        }
        public static bool HidePlayerName(PlayerControl source, PlayerControl target)
        {
            if (source == null || target == null) return true;
            else if (source.IsDead() || source.IsRole(RoleId.God)) return false;
            else if (source.PlayerId == target.PlayerId) return false; // Player sees his own name
            else if (source.IsImpostor() && target.IsImpostor()) return false;
            else if (GameData.Instance && RoleClass.NiceScientist.IsScientistPlayers.ContainsKey(target.PlayerId) && RoleClass.NiceScientist.IsScientistPlayers[target.PlayerId]) return true;
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
        internal static string Cs(object unityEngine, string v)
        {
            throw new NotImplementedException();
        }

        public static Texture2D LoadTextureFromResources(string path)
        {
            try
            {
                Texture2D texture = new(2, 2, TextureFormat.ARGB32, true);
                Assembly assembly = Assembly.GetExecutingAssembly();
                Stream stream = assembly.GetManifestResourceStream(path);
                var byteTexture = new byte[stream.Length];
                var read = stream.Read(byteTexture, 0, (int)stream.Length);
                LoadImage(texture, byteTexture, false);
                return texture;
            }
            catch
            {
                System.Console.WriteLine("Error loading texture from resources: " + path);
            }
            return null;
        }

        public static string Cs(Color c, string s)
        {
            return string.Format("<color=#{0:X2}{1:X2}{2:X2}{3:X2}>{4}</color>", CustomOptions.ToByte(c.r), CustomOptions.ToByte(c.g), CustomOptions.ToByte(c.b), CustomOptions.ToByte(c.a), s);
        }
        public static T GetRandom<T>(List<T> list)
        {
            var indexdate = UnityEngine.Random.Range(0, list.Count);
            return list[indexdate];
        }
        public static PlayerControl GetRandompc(List<PlayerControl> list)
        {
            var indexdate = UnityEngine.Random.Range(0, list.Count);
            return list[indexdate];
        }
        public static int GetRandomIndex<T>(List<T> list)
        {
            var indexdate = UnityEngine.Random.Range(0, list.Count);
            return indexdate;
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
            return player.transform.FindChild("NameText_TMP").GetComponent<TextMeshPro>();
        }
        public static SpriteRenderer MyRend(this PlayerControl player)
        {
            bool Isnull = true;
            if (MyRendCache.ContainsKey(player.PlayerId))
            {
                Isnull = MyRendCache[player.PlayerId] == null;
            }
            if (Isnull)
            {
                MyRendCache[player.PlayerId] = player.transform.FindChild("Sprite").GetComponent<SpriteRenderer>();
            }
            return MyRendCache[player.PlayerId];
        }
        public static SpriteRenderer rend(this PlayerPhysics player)
        {
            byte PlayerId = player.myPlayer.PlayerId;
            bool Isnull = true;
            if (MyRendCache.ContainsKey(PlayerId))
            {
                Isnull = MyRendCache[PlayerId] == null;
            }
            if (Isnull)
            {
                MyRendCache[PlayerId] = player.transform.FindChild("Sprite").GetComponent<SpriteRenderer>();
            }
            return MyRendCache[PlayerId];
        }
        public static SkinLayer GetSkin(this PlayerControl player)
        {
            byte PlayerId = player.PlayerId;
            bool Isnull = true;
            if (SkinLayerCache.ContainsKey(PlayerId))
            {
                Isnull = SkinLayerCache[PlayerId] == null;
            }
            if (Isnull)
            {
                SkinLayerCache[PlayerId] = player.transform.FindChild("Skin").GetComponent<SkinLayer>();
            }
            return SkinLayerCache[PlayerId];
        }
        public static SkinLayer GetSkin(this PlayerPhysics player)
        {
            byte PlayerId = player.myPlayer.PlayerId;
            bool Isnull = true;
            if (SkinLayerCache.ContainsKey(PlayerId))
            {
                Isnull = SkinLayerCache[PlayerId] == null;
            }
            if (Isnull)
            {
                SkinLayerCache[PlayerId] = player.transform.FindChild("Skin").GetComponent<SkinLayer>();
            }
            return SkinLayerCache[PlayerId];
        }
        public static HatParent HatRenderer(this PlayerControl player)
        {
            byte PlayerId = player.PlayerId;
            bool Isnull = true;
            if (HatRendererCache.ContainsKey(PlayerId))
            {
                Isnull = HatRendererCache[PlayerId] == null;
            }
            if (Isnull)
            {
                HatRendererCache[PlayerId] = player.transform.FindChild("Sprite/Hat").GetComponent<HatParent>();
            }
            return HatRendererCache[PlayerId];
        }
        public static SpriteRenderer HatRend(this PlayerControl player)
        {
            byte PlayerId = player.PlayerId;
            bool Isnull = true;
            if (HatRendCache.ContainsKey(PlayerId))
            {
                Isnull = HatRendCache[PlayerId] == null;
            }
            if (Isnull)
            {
                HatRendCache[PlayerId] = player.transform.FindChild("Sprite/Hat").GetComponent<SpriteRenderer>();
            }
            return HatRendCache[PlayerId];
        }
        public static VisorLayer VisorSlot(this PlayerControl player)
        {
            byte PlayerId = player.PlayerId;
            bool Isnull = true;
            if (VisorSlotCache.ContainsKey(PlayerId))
            {
                Isnull = VisorSlotCache[PlayerId] == null;
            }
            if (Isnull)
            {
                VisorSlotCache[PlayerId] = player.transform.FindChild("Sprite/Visor").GetComponent<VisorLayer>();
            }
            return VisorSlotCache[PlayerId];
        }

        public static HatParent HatSlot(this PoolablePlayer player)
        {
            return player.transform.FindChild("HatSlot").GetComponent<HatParent>();
        }
        public static VisorLayer VisorSlot(this PoolablePlayer player)
        {
            return player.transform.FindChild("Visor").GetComponent<VisorLayer>();
        }

        public static Texture2D loadTextureFromDisk(string path)
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

        public static PlayerControl playerById(byte id)
        {
            foreach (CachedPlayer player in CachedPlayer.AllPlayers)
            {
                if (player.PlayerId == id)
                {
                    return player;
                }
            }
            return null;
        }

        public static bool IsCheckListPlayerControl(this List<PlayerControl> ListDate, PlayerControl CheckPlayer)
        {
            foreach (PlayerControl Player in ListDate)
            {
                if (Player.PlayerId == CheckPlayer.PlayerId)
                {
                    return true;
                }
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
}