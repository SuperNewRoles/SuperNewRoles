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
                if (__instance == PlayerControl.LocalPlayer)
                    AllFixedUpdate();
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
        private Action OnPlayEnd;
        public static Dictionary<byte, int> Ids;
        public bool IsShootNow;
        public bool IsFlipX;
        private int DestroyIndex = 0;
        static Vector3 OwnerPos;

        public WaveCannonObject(Vector3 pos, bool FlipX, PlayerControl _owner)
        {
            if (_owner.PlayerId == CachedPlayer.LocalPlayer.PlayerId)
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
            if (!Ids.ContainsKey(Owner.PlayerId))
            {
                Ids[Owner.PlayerId] = 0;
            }
            Id = Ids[Owner.PlayerId];
            Ids[Owner.PlayerId]++;
            IsShootNow = false;
        }
        public void Shoot()
        {
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
                        CachedPlayer.LocalPlayer.PlayerControl.moveable = true;
                        Camera.main.GetComponent<FollowerCamera>().Locked = false;
                        HudManagerStartPatch.WaveCannonButton.MaxTimer = CustomOptions.WaveCannonCoolTime.GetFloat();
                        HudManagerStartPatch.WaveCannonButton.Timer = HudManagerStartPatch.WaveCannonButton.MaxTimer;
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
            if (RoleClass.IsMeeting) { GameObject.Destroy(this.gameObject); return; }
            if (Owner != null && Owner.IsDead()) {
                GameObject.Destroy(this.gameObject);
                if (Owner.PlayerId == CachedPlayer.LocalPlayer.PlayerId)
                {
                    CachedPlayer.LocalPlayer.PlayerControl.moveable = true;
                    Camera.main.GetComponent<FollowerCamera>().Locked = false;
                }
                Objects.Remove(this);
                return;
            }
            if (Owner != null && Owner.PlayerId == CachedPlayer.LocalPlayer.PlayerId && !RoleClass.IsMeeting) CachedPlayer.LocalPlayer.transform.position = OwnerPos;
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
