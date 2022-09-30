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
        public enum RpcType{
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
        public Transform transform => gameObject.transform;

        public PlayerControl Owner;
        public int Id;
        private List<Sprite> sprites;
        private float UpdateTime;
        private float DefaultUpdateTime => 1f / freamrate;
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
            OwnerPlayerId = _owner.PlayerId;
            if (OwnerPlayerId == CachedPlayer.LocalPlayer.PlayerId)
            {
                CachedPlayer.LocalPlayer.PlayerControl.moveable = false;

                Camera.main.GetComponent<FollowerCamera>().Locked = true;
            }
            OwnerPos = _owner.transform.position;
            IsFlipX = FlipX;
            Owner = _owner;
            Id = 0;
            gameObject = new("WaveCannonObject");
            effectGameObject = new("WaveCannonEffect");
            effectGameObject.transform.SetParent(transform);
            effectGameObject.transform.localPosition = new(22.45f, 0, 1);
            effectGameObject.transform.localScale = new(7 * 1.4f, 1.5f, 1);
            render = gameObject.AddComponent<SpriteRenderer>();
            effectrender = effectGameObject.AddComponent<SpriteRenderer>();
            index = 0;
            sprites = new();
            for (int i = 1; i <= 5; i++)
            {
                sprites.Add(ModHelpers.LoadSpriteFromResources($"SuperNewRoles.Resources.WaveCannon.Charge_000{i}.png", 115f));
            }
            render.sprite = sprites[0];
            IsLoop = true;
            freamrate = 25;
            Playing = true;
            Objects.Add(this);
            pos.z -= 0.0003f;
            transform.position = pos + new Vector3(FlipX ? -4 : 4, 0, 0);
            transform.localScale = new(FlipX ? -1 : 1, 1, 1);
            if (!Ids.ContainsKey(OwnerPlayerId))
            {
                Ids[OwnerPlayerId] = 0;
            }
            Id = Ids[OwnerPlayerId];
            Ids[OwnerPlayerId]++;
            IsShootNow = false;
            ChargeSound = SoundManager.Instance.PlaySound(ModHelpers.loadAudioClipFromResources("SuperNewRoles.Resources.WaveCannon.ChargeSound.raw"), true);
        }
        public void Shoot()
        {
            if (ChargeSound != null)
                ChargeSound.Stop();
            SoundManager.Instance.PlaySound(ModHelpers.loadAudioClipFromResources("SuperNewRoles.Resources.WaveCannon.ShootSound.raw"), false);
            IsShootNow = true;
            render.sprite = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.WaveCannon.Cannon.png", 115f);
            sprites = new();
            for (int i = 1; i <= 12; i++)
            {
                sprites.Add(ModHelpers.LoadSpriteFromResources($"SuperNewRoles.Resources.WaveCannon.Shoot_00{(i <= 9 ? "0" : "")}{i}.png", 115f));
            }
            effectrender.sprite = sprites[0];
            IsLoop = false;
            freamrate = 12;
            Playing = true;
            OnPlayEnd = () => {
                IsLoop = true;
                freamrate = 15;
                Playing = true;
                sprites = new();
                for (int i = 6; i <= 12; i++)
                {
                    sprites.Add(ModHelpers.LoadSpriteFromResources($"SuperNewRoles.Resources.WaveCannon.Shoot_00{(i <= 9 ? "0" : "")}{i}.png", 115f));
                }
                effectrender.sprite = sprites[0];
                OnPlayEnd = () =>
                {
                    DestroyIndex++;
                    if (DestroyIndex > 3)
                    {
                        GameObject.Destroy(this.gameObject);
                        if (OwnerPlayerId == CachedPlayer.LocalPlayer.PlayerId)
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
            if (render == null) { Objects.Remove(this); return; }
            if (RoleClass.IsMeeting)
            {
                if (ChargeSound != null)
                    ChargeSound.Stop();
                GameObject.Destroy(effectGameObject);
                GameObject.Destroy(gameObject);
                return;
            }
            if (Owner != null && (Owner.IsDead() || !(Owner.GetRole() is RoleId.WaveCannon or RoleId.WaveCannonJackal))) {
                GameObject.Destroy(this.gameObject);
                if (OwnerPlayerId == CachedPlayer.LocalPlayer.PlayerId)
                {
                    CachedPlayer.LocalPlayer.PlayerControl.moveable = true;
                    Camera.main.GetComponent<FollowerCamera>().Locked = false;
                }
                if (ChargeSound != null)
                    ChargeSound.Stop();
                return;
            }
            Logger.Info($"{OwnerPlayerId} : {Owner != null} : {OwnerPlayerId == CachedPlayer.LocalPlayer.PlayerId} : {CachedPlayer.LocalPlayer.PlayerId} : {PlayerControl.LocalPlayer.PlayerId} : {!RoleClass.IsMeeting} : {OwnerPos}","WaveCannonUpdate");
            if (Owner != null && OwnerPlayerId == PlayerControl.LocalPlayer.PlayerId && !RoleClass.IsMeeting) {
                //Owner.transform.position = OwnerPos;

                if (IsShootNow)
                {
                    foreach (PlayerControl player in CachedPlayer.AllPlayers)
                    {
                        if (player.IsDead()) continue;
                        if (RoleClass.WaveCannon.CannotMurderPlayers.Contains(player.PlayerId)) continue;
                        if (player.PlayerId == CachedPlayer.LocalPlayer.PlayerId) continue;
                        float posdata = player.GetTruePosition().y - transform.position.y;
                        if (posdata > 1 || posdata < -1) continue;
                        posdata = transform.position.x - (IsFlipX ? -2 : 2);
                        if ((IsFlipX && player.transform.position.x > posdata) || (!IsFlipX && player.transform.position.x < posdata)) continue;
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
            if (Playing)
            {
                UpdateTime -= Time.fixedDeltaTime;
                if (UpdateTime <= 0)
                {
                    index++;
                    if (index >= sprites.Count)
                    {
                        index = 0;
                        if (OnPlayEnd != null && IsLoop) OnPlayEnd();
                        if (!IsLoop)
                        {
                            Playing = false;
                            if (OnPlayEnd != null) OnPlayEnd();
                            return;
                        }
                    }
                    UpdateTime = DefaultUpdateTime;
                    if (IsShootNow) effectrender.sprite = sprites[index];
                    else render.sprite = sprites[index];
                }
            }
        }
    }
}
