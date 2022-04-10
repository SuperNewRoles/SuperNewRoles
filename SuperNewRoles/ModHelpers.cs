using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Collections;
using UnhollowerBaseLib;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;
using HarmonyLib;
using Hazel;
using SuperNewRoles.CustomOption;
using SuperNewRoles.CustomRPC;
using SuperNewRoles.Roles;

namespace SuperNewRoles
{
    public static class ModHelpers
    {
        public enum MurderAttemptResult
        {
            PerformKill,
            SuppressKill,
            BlankKill
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
            DestroyableSingleton<HudManager>.Instance.KillButton.SetCoolDown(time, max);
        }
        public static byte? GetKey(this Dictionary<byte, byte> dec, byte Value)
        {
            foreach (var data in dec)
            {
                if(data.Value == Value)
                {
                    return data.Key;
                }
            }
            return null;
        }// parent直下の子オブジェクトをforeachループで取得する
        public static GameObject[] GetChildren(this GameObject ParentObject)
        {
            GameObject[] ChildObject = new GameObject[ParentObject.transform.childCount];

            for (int i = 0; i < ParentObject.transform.childCount; i++)
            {
                ChildObject[i] = ParentObject.transform.GetChild(i).gameObject;
            }
            return ChildObject;
        }
        public static void DeleteObject(this Transform[] trans,string notdelete)
        {
            foreach(Transform tran in trans)
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
                List<PlayerControl> ps = new List<PlayerControl>();
                foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                {
                    if (!p.Data.Disconnected) ps.Add(p);
                }
                return ps;
            }
        }
        public static void SetActiveAllObject(this GameObject[] trans, string notdelete,bool IsActive)
        {
            foreach (GameObject tran in trans)
            {
                if (tran.name != notdelete)
                {
                    tran.SetActive(IsActive);
                }
            }
        }
        public static void setSkinWithAnim(PlayerPhysics playerPhysics, string SkinId)
        {
            SkinViewData nextSkin = DestroyableSingleton<HatManager>.Instance.GetSkinById(SkinId).viewData.viewData;
            AnimationClip clip = null;
            var spriteAnim = playerPhysics.Skin.animator;
            var anim = spriteAnim.m_animator;
            var skinLayer = playerPhysics.Skin;

            var currentPhysicsAnim = playerPhysics.Animator.GetCurrentAnimation();
            if (currentPhysicsAnim == playerPhysics.CurrentAnimationGroup.RunAnim) clip = nextSkin.RunAnim;
            else if (currentPhysicsAnim == playerPhysics.CurrentAnimationGroup.SpawnAnim) clip = nextSkin.SpawnAnim;
            else if (currentPhysicsAnim == playerPhysics.CurrentAnimationGroup.EnterVentAnim) clip = nextSkin.EnterVentAnim;
            else if (currentPhysicsAnim == playerPhysics.CurrentAnimationGroup.ExitVentAnim) clip = nextSkin.ExitVentAnim;
            else if (currentPhysicsAnim == playerPhysics.CurrentAnimationGroup.IdleAnim) clip = nextSkin.IdleAnim;
            else clip = nextSkin.IdleAnim;

            float progress = playerPhysics.Animator.m_animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
            skinLayer.skin = nextSkin;

            spriteAnim.Play(clip, 1f);
            anim.Play("a", 0, progress % 1);
            anim.Update(0f);
        }
        public static Dictionary<byte, PlayerControl> allPlayersById()
        {
            Dictionary<byte, PlayerControl> res = new Dictionary<byte, PlayerControl>();
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                res.Add(player.PlayerId, player);
            return res;
        }

        public static void destroyList<T>(Il2CppSystem.Collections.Generic.List<T> items) where T : UnityEngine.Object
        {
            if (items == null) return;
            foreach (T item in items)
            {
                UnityEngine.Object.Destroy(item);
            }
        }
        public static void destroyList<T>(List<T> items) where T : UnityEngine.Object
        {
            if (items == null) return;
            foreach (T item in items)
            {
                UnityEngine.Object.Destroy(item);
            }
        }
        public static MurderAttemptResult checkMuderAttempt(PlayerControl killer, PlayerControl target, bool blockRewind = false)
        {
            // Modified vanilla checks
            if (AmongUsClient.Instance.IsGameOver) return MurderAttemptResult.SuppressKill;
            if (killer == null || killer.Data == null || killer.Data.IsDead || killer.Data.Disconnected) return MurderAttemptResult.SuppressKill; // Allow non Impostor kills compared to vanilla code
            if (target == null || target.Data == null || target.Data.IsDead || target.Data.Disconnected) return MurderAttemptResult.SuppressKill; // Allow killing players in vents compared to vanilla code
            return MurderAttemptResult.PerformKill;
        }
        public static void generateAndAssignTasks(this PlayerControl player, int numCommon, int numShort, int numLong)
        {
            if (player == null) return;

            List<byte> taskTypeIds = generateTasks(numCommon, numShort, numLong);

            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.UncheckedSetTasks, Hazel.SendOption.Reliable, -1);
            writer.Write(player.PlayerId);
            writer.WriteBytesAndSize(taskTypeIds.ToArray());
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            RPCProcedure.uncheckedSetTasks(player.PlayerId, taskTypeIds.ToArray());
        }
        public static List<byte> generateTasks(int numCommon, int numShort, int numLong)
        {
            if (numCommon + numShort + numLong <= 0)
            {
                numShort = 1;
            }

            var tasks = new Il2CppSystem.Collections.Generic.List<byte>();
            var hashSet = new Il2CppSystem.Collections.Generic.HashSet<TaskTypes>();

            var commonTasks = new Il2CppSystem.Collections.Generic.List<NormalPlayerTask>();
            foreach (var task in ShipStatus.Instance.CommonTasks.OrderBy(x => RoleClass.rnd.Next())) commonTasks.Add(task);

            var shortTasks = new Il2CppSystem.Collections.Generic.List<NormalPlayerTask>();
            foreach (var task in ShipStatus.Instance.NormalTasks.OrderBy(x => RoleClass.rnd.Next())) shortTasks.Add(task);

            var longTasks = new Il2CppSystem.Collections.Generic.List<NormalPlayerTask>();
            foreach (var task in ShipStatus.Instance.LongTasks.OrderBy(x => RoleClass.rnd.Next())) longTasks.Add(task);

            int start = 0;
            ShipStatus.Instance.AddTasksFromList(ref start, numCommon, tasks, hashSet, commonTasks);

            start = 0;
            ShipStatus.Instance.AddTasksFromList(ref start, numShort, tasks, hashSet, shortTasks);

            start = 0;
            ShipStatus.Instance.AddTasksFromList(ref start, numLong, tasks, hashSet, longTasks);

            return tasks.ToArray().ToList();
        }
        public static MurderAttemptResult checkMuderAttemptAndKill(PlayerControl killer, PlayerControl target, bool isMeetingStart = false, bool showAnimation = true)
        {
            // The local player checks for the validity of the kill and performs it afterwards (different to vanilla, where the host performs all the checks)
            // The kill attempt will be shared using a custom RPC, hence combining modded and unmodded versions is impossible

            MurderAttemptResult murder = checkMuderAttempt(killer, target, isMeetingStart);
            if (murder == MurderAttemptResult.PerformKill)
            {
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.RPCMurderPlayer, Hazel.SendOption.Reliable, -1);
                writer.Write(killer.PlayerId);
                writer.Write(target.PlayerId);
                writer.Write(showAnimation ? Byte.MaxValue : 0);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.RPCMurderPlayer(killer.PlayerId, target.PlayerId, showAnimation ? Byte.MaxValue : (byte)0);
            }
            return murder;
        }
        public static void SetPrivateRole(this PlayerControl player, RoleTypes role, PlayerControl seer = null)
        {
            if (player == null) return;
            if (seer == null) seer = player;
            var clientId = seer.getClientId();
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(player.NetId, (byte)RpcCalls.SetRole, Hazel.SendOption.Reliable, clientId);
            writer.Write((ushort)role);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }
        public static InnerNet.ClientData getClient(this PlayerControl player)
        {
            var client = AmongUsClient.Instance.allClients.ToArray().Where(cd => cd.Character.PlayerId == player.PlayerId).FirstOrDefault();
            return client;
        }
        public static int getClientId(this PlayerControl player)
        {
            var client = player.getClient();
            if (client == null) return -1;
            return client.Id;
        }
        public static bool hidePlayerName(PlayerControl source, PlayerControl target)
        {
            if (source == null || target == null) return true;
            else if (source.isDead() || source.isRole(RoleId.God)) return false;
            else if (source.PlayerId == target.PlayerId) return false; // Player sees his own name
            else if (source.isImpostor() && target.isImpostor()) return false;
            else if ((target.isRole(RoleId.NiceScientist) || target.isRole(RoleId.EvilScientist))  && GameData.Instance && RoleClass.NiceScientist.IsScientistPlayers[target.PlayerId]) return true;
            return true;
        }
        public static Sprite loadSpriteFromResources(string path, float pixelsPerUnit)
        {
            try
            {
                Texture2D texture = loadTextureFromResources(path);
                return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), pixelsPerUnit);
            }
            catch
            {
                System.Console.WriteLine("Error loading sprite from path: " + path);
            }
            return null;
        }

        internal static string cs(object unityEngine, string v)
        {
            throw new NotImplementedException();
        }

        public static Texture2D loadTextureFromResources(string path)
        {
            try
            {
                Texture2D texture = new Texture2D(2, 2, TextureFormat.ARGB32, true);
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

        public static string cs(Color c, string s)
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
        public static Texture2D loadTextureFromDisk(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    Texture2D texture = new Texture2D(2, 2, TextureFormat.ARGB32, true);
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
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            {
                if (player.PlayerId == id)
                {
                    return player;
                }
            }
            return null;
        }
        public static bool IsCheckListPlayerControl(this List<PlayerControl> ListDate,PlayerControl CheckPlayer)
        {
            foreach(PlayerControl Player in ListDate)
            {
                if (Player.PlayerId == CheckPlayer.PlayerId)
                {
                    return true;
                }
            }
            return false;
        }
    }
}