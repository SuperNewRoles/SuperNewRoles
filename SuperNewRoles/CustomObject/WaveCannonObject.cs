using System;
using System.Collections.Generic;
using HarmonyLib;
using Hazel;
using SuperNewRoles.Buttons;
using SuperNewRoles.Helpers;
using SuperNewRoles.Roles;
using UnityEngine;

namespace SuperNewRoles.CustomObject
{
    public class WaveCannonObject
    {
        public enum RpcType
        {
            Spawn,
            Shoot
        }
        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
        class FixedUpdatePatch
        {
            public static void Postfix(PlayerControl __instance)
            {
                if (__instance.PlayerId == PlayerControl.LocalPlayer.PlayerId)
                {
                    AllFixedUpdate();
                }
            }
        }
        public static List<WaveCannonObject> Objects = new();

        public GameObject gameObject;
        public GameObject effectGameObject;
        public Transform transform => this.gameObject.transform;

        public PlayerControl Owner;
        public int Id;
        private List<Sprite> sprites;
        private float UpdateTime;
        private float DefaultUpdateTime => 1f / this.freamrate;
        private int freamrate;
        private int index;
        private bool IsLoop;
        private bool Playing;
        private SpriteRenderer render;
        private SpriteRenderer effectrender;
        private byte OwnerPlayerId;
        private Action OnPlayEnd;
        public static Dictionary<byte, int> Ids;
        public bool IsShootNow;
        public bool IsFlipX;
        private int DestroyIndex = 0;
        static Vector3 OwnerPos;
        static AudioSource ChargeSound;

        public WaveCannonObject(Vector3 pos, bool FlipX, PlayerControl _owner)
        {
            this.OwnerPlayerId = _owner.PlayerId;
            if (this.OwnerPlayerId == CachedPlayer.LocalPlayer.PlayerId)
            {
                CachedPlayer.LocalPlayer.PlayerControl.moveable = false;

                Camera.main.GetComponent<FollowerCamera>().Locked = true;
            }
            OwnerPos = _owner.transform.position;
            this.IsFlipX = FlipX;
            this.Owner = _owner;
            this.Id = 0;
            this.gameObject = new("WaveCannonObject");
            this.effectGameObject = new("WaveCannonEffect");
            this.effectGameObject.transform.SetParent(this.transform);
            this.effectGameObject.transform.localPosition = new(22.45f, 0, 1);
            this.effectGameObject.transform.localScale = new(7 * 1.4f, 1.5f, 1);
            this.render = this.gameObject.AddComponent<SpriteRenderer>();
            this.effectrender = this.effectGameObject.AddComponent<SpriteRenderer>();
            this.index = 0;
            this.sprites = new();
            for (int i = 1; i <= 5; i++)
            {
                this.sprites.Add(ModHelpers.LoadSpriteFromResources($"SuperNewRoles.Resources.WaveCannon.Charge_000{i}.png", 115f));
            }
            this.render.sprite = this.sprites[0];
            this.IsLoop = true;
            this.freamrate = 25;
            this.Playing = true;
            Objects.Add(this);
            pos.z -= 0.0003f;
            this.transform.position = pos + new Vector3(FlipX ? -4 : 4, 0, 0);
            this.transform.localScale = new(FlipX ? -1 : 1, 1, 1);
            if (!Ids.ContainsKey(this.OwnerPlayerId))
            {
                Ids[this.OwnerPlayerId] = 0;
            }
            this.Id = Ids[this.OwnerPlayerId];
            Ids[this.OwnerPlayerId]++;
            this.IsShootNow = false;
            ChargeSound = SoundManager.Instance.PlaySound(ModHelpers.loadAudioClipFromResources("SuperNewRoles.Resources.WaveCannon.ChargeSound.raw"), true);
        }
        public void Shoot()
        {
            if (ChargeSound != null)
                ChargeSound.Stop();
            SoundManager.Instance.PlaySound(ModHelpers.loadAudioClipFromResources("SuperNewRoles.Resources.WaveCannon.ShootSound.raw"), false);
            this.IsShootNow = true;
            this.render.sprite = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.WaveCannon.Cannon.png", 115f);
            this.sprites = new();
            for (int i = 1; i <= 12; i++)
            {
                this.sprites.Add(ModHelpers.LoadSpriteFromResources($"SuperNewRoles.Resources.WaveCannon.Shoot_00{(i <= 9 ? "0" : "")}{i}.png", 115f));
            }
            this.effectrender.sprite = this.sprites[0];
            this.IsLoop = false;
            this.freamrate = 12;
            this.Playing = true;
            this.OnPlayEnd = () =>
            {
                this.IsLoop = true;
                this.freamrate = 15;
                this.Playing = true;
                this.sprites = new();
                for (int i = 6; i <= 12; i++)
                {
                    this.sprites.Add(ModHelpers.LoadSpriteFromResources($"SuperNewRoles.Resources.WaveCannon.Shoot_00{(i <= 9 ? "0" : "")}{i}.png", 115f));
                }
                this.effectrender.sprite = this.sprites[0];
                this.OnPlayEnd = () =>
                {
                    this.DestroyIndex++;
                    if (this.DestroyIndex > 3)
                    {
                        GameObject.Destroy(this.gameObject);
                        if (this.OwnerPlayerId == CachedPlayer.LocalPlayer.PlayerId)
                        {
                            if (PlayerControl.LocalPlayer.IsRole(RoleId.WaveCannon))
                            {
                                if (CustomOptions.WaveCannonIsSyncKillCoolTime.GetBool())
                                    PlayerControl.LocalPlayer.SetKillTimer(RoleHelpers.GetCoolTime(PlayerControl.LocalPlayer));
                            }
                            else
                            {
                                if (CustomOptions.WaveCannonJackalIsSyncKillCoolTime.GetBool())
                                    Roles.Neutral.WaveCannonJackal.ResetCoolDowns();
                            }
                            CachedPlayer.LocalPlayer.PlayerControl.moveable = true;
                            Camera.main.GetComponent<FollowerCamera>().Locked = false;
                            HudManagerStartPatch.WaveCannonButton.MaxTimer = PlayerControl.LocalPlayer.IsRole(RoleId.WaveCannon) ? CustomOptions.WaveCannonCoolTime.GetFloat() : CustomOptions.WaveCannonJackalCoolTime.GetFloat();
                            HudManagerStartPatch.WaveCannonButton.Timer = HudManagerStartPatch.WaveCannonButton.MaxTimer;
                            RoleClass.WaveCannon.CannotMurderPlayers = new();
                        }
                    }
                };
            };
        }
        public static void AllFixedUpdate()
        {
            if (Objects.Count <= 0) return;
            foreach (WaveCannonObject obj in Objects.ToArray())
            {
                obj.FixedUpdate();
            }
        }
        public void FixedUpdate()
        {
            if (this.render == null) { Objects.Remove(this); return; }
            if (RoleClass.IsMeeting)
            {
                if (ChargeSound != null)
                    ChargeSound.Stop();
                GameObject.Destroy(this.effectGameObject);
                GameObject.Destroy(this.gameObject);
                return;
            }
            if (this.Owner != null && (this.Owner.IsDead() || !(this.Owner.GetRole() is RoleId.WaveCannon or RoleId.WaveCannonJackal)))
            {
                GameObject.Destroy(this.gameObject);
                if (this.OwnerPlayerId == CachedPlayer.LocalPlayer.PlayerId)
                {
                    CachedPlayer.LocalPlayer.PlayerControl.moveable = true;
                    Camera.main.GetComponent<FollowerCamera>().Locked = false;
                }
                if (ChargeSound != null)
                    ChargeSound.Stop();
                return;
            }
            Logger.Info($"{this.OwnerPlayerId} : {this.Owner != null} : {this.OwnerPlayerId == CachedPlayer.LocalPlayer.PlayerId} : {CachedPlayer.LocalPlayer.PlayerId} : {PlayerControl.LocalPlayer.PlayerId} : {!RoleClass.IsMeeting} : {OwnerPos}", "WaveCannonUpdate");
            if (this.Owner != null && this.OwnerPlayerId == PlayerControl.LocalPlayer.PlayerId && !RoleClass.IsMeeting)
            {
                //Owner.transform.position = OwnerPos;

                if (this.IsShootNow)
                {
                    foreach (PlayerControl player in CachedPlayer.AllPlayers)
                    {
                        if (player.IsDead()) continue;
                        if (RoleClass.WaveCannon.CannotMurderPlayers.Contains(player.PlayerId)) continue;
                        if (player.PlayerId == CachedPlayer.LocalPlayer.PlayerId) continue;
                        float posdata = player.GetTruePosition().y - this.transform.position.y;
                        if (posdata is > 1 or < (-1)) continue;
                        posdata = this.transform.position.x - (this.IsFlipX ? -2 : 2);
                        if ((this.IsFlipX && player.transform.position.x > posdata) || (!this.IsFlipX && player.transform.position.x < posdata)) continue;
                        if (player.IsRole(RoleId.Shielder) && RoleClass.Shielder.IsShield.ContainsKey(player.PlayerId) && RoleClass.Shielder.IsShield[player.PlayerId])
                        {
                            MessageWriter msgwriter = RPCHelper.StartRPC(CustomRPC.ShielderProtect);
                            msgwriter.Write(CachedPlayer.LocalPlayer.PlayerId);
                            msgwriter.Write(player.PlayerId);
                            msgwriter.Write(0);
                            msgwriter.EndRPC();
                            RPCProcedure.ShielderProtect(CachedPlayer.LocalPlayer.PlayerId, player.PlayerId, 0);
                            RoleClass.WaveCannon.CannotMurderPlayers.Add(player.PlayerId);
                            return;
                        }
                        MessageWriter writer = RPCHelper.StartRPC(CustomRPC.RPCMurderPlayer);
                        writer.Write(CachedPlayer.LocalPlayer.PlayerId);
                        writer.Write(player.PlayerId);
                        writer.Write((byte)0);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        float Timer = PlayerControl.LocalPlayer.killTimer;
                        RPCProcedure.RPCMurderPlayer(CachedPlayer.LocalPlayer.PlayerId, player.PlayerId, 0);
                        if (PlayerControl.LocalPlayer.IsImpostor())
                        {
                            PlayerControl.LocalPlayer.killTimer = Timer;
                            FastDestroyableSingleton<HudManager>.Instance.KillButton.cooldownTimerText.text = PlayerControl.LocalPlayer.killTimer <= 0f ? "" : PlayerControl.LocalPlayer.killTimer.ToString();
                        }
                    }
                }
            }
            if (this.Playing)
            {
                this.UpdateTime -= Time.fixedDeltaTime;
                if (this.UpdateTime <= 0)
                {
                    this.index++;
                    if (this.index >= this.sprites.Count)
                    {
                        this.index = 0;
                        if (this.OnPlayEnd != null && this.IsLoop) this.OnPlayEnd();
                        if (!this.IsLoop)
                        {
                            this.Playing = false;
                            this.OnPlayEnd?.Invoke();
                            return;
                        }
                    }
                    this.UpdateTime = this.DefaultUpdateTime;
                    if (this.IsShootNow) this.effectrender.sprite = this.sprites[this.index];
                    else this.render.sprite = this.sprites[this.index];
                }
            }
        }
    }
}